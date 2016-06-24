using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace SyncFolder
{
    public class SyncNet
    {
        public SyncClient sync_client;      // Handles the tcp and netstream end 
                                            // and raw receiving and sending 
                                            // under the underlying network protocol.
                                            // Keeps track ofcurrent connections
                                            //  (1 at a time)
        
        public SyncCon sync_con;            // Handles incoming connection or 
                                            // connects on command. Keeps track of
                                            // current connections (1 at a time)

        public SyncFolder sync_folder;

        public SyncNet(int port, string root_path, bool test = false)
        {
            sync_folder = new SyncFolder(this,root_path);

            sync_con = new SyncCon(port,this,test);
            sync_con.TcpClientConnected += new tcp_client_connected(tcp_client_connected);
        }

        // New Connection astablished from remote device
        void tcp_client_connected(SyncClient client)
        {
            if (client == null) return;

            this.sync_client = client;
        }
        
        // Disconnects all connections and attempts new one
        public bool Connect(IPAddress ip)
        {
            if (sync_con.connected == true || 
                (sync_client != null && sync_client.connected))
                    Disconnect();

            TcpClient client = sync_con.Connect(ip);

            if (client == null)
                return false;

            sync_client = new SyncClient(client,this);
            return true;
        }

        // Disconnects sync_con, sync_client if connected
        public void Disconnect()
        { 
            sync_con.Disconnect();

            if (sync_client != null)
                sync_client.Disconnect();

            Console.WriteLine("Disconnected");
        }

        public void Reset(string root_path)
        {
            if (sync_client != null)
                sync_client.Close();
            if (sync_con.connected)
                sync_con.Disconnect();

            sync_folder.Close();

            sync_client = null;
            sync_folder = null;

            this.sync_folder = new SyncFolder(this, root_path);
        }
        

        #region Receive / Send

        // Receives data from head
        // null => Disconnect, TimeOut, FalseHash
        public byte[] Receive(SyncRequestHead head)
        {
            byte[] data = null;
            byte[] rec;
            int pack_size = sync_client.pack_size;

            using (MemoryStream ms = new MemoryStream())
            {
                while (ms.Position + pack_size <= head.size)
                {
                    rec = sync_client.receive_pack(pack_size);
                    if (rec == null) return null;                           // Disconnect or TimeOut

                    ms.Write(rec,0,rec.Length);
                }

                pack_size = (int)(head.size - ms.Position);

                if (pack_size > 0)
                {
                    rec = sync_client.receive_pack(pack_size);
                    if (rec == null) return null;                           // Disconnect or TimeOut

                    ms.Write(rec, 0, rec.Length);
                }

                data = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }

            if (SyncHash.Comp_Hash(head.hash, SyncHash.Get_SHA1_Hash(data)) == false)
                return null;    // Hash fail

            return data;
        }

        public bool Receive_File(SyncRequestHead head, SyncFetchContent fetch_content)
        {
            byte[] rec;
            int pack_size = sync_client.pack_size;
            string path = sync_folder.new_path + fetch_content.file_path;
            string dir_path = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir_path))
                Directory.CreateDirectory(dir_path);

            using (FileStream stream = new FileStream(path,FileMode.OpenOrCreate,FileAccess.ReadWrite))
            {
                if (stream.Length != 0)
                    stream.SetLength(0);

                while (stream.Position + pack_size <= head.size)
                {
                    rec = sync_client.receive_pack(pack_size);
                    if (rec == null) return false;                           // Disconnect or TimeOut

                    stream.Write(rec, 0, rec.Length);
                }

                pack_size = (int)(head.size - stream.Position);

                if (pack_size > 0)
                {
                    rec = sync_client.receive_pack(pack_size);
                    if (rec == null) return false;                           // Disconnect or TimeOut

                    stream.Write(rec, 0, rec.Length);
                }

                stream.Position = 0;

                if (SyncHash.Comp_Hash(head.hash, SyncHash.Get_SHA1_Hash(stream)) == false)
                    return false;    // Hash fail

                stream.Close();
                stream.Dispose();
            }


            return true;
        }

        // Send data from array
        // null => Disconnect, TimeOut, FalseHash
        public bool Send(byte[] data, RequestType type)
        {
            SyncRequestHead head = new SyncRequestHead(SyncHash.Get_SHA1_Hash(data), data.Length, type);
            byte[] send;
            int pack_size = sync_client.pack_size;

            // Send Head first
            send = head.Get_Bytes();
            if (send == null) return false;
            sync_client.send_pack(send);

            // Send Data
            using (MemoryStream ms = new MemoryStream(data))
            {
                send = new byte[pack_size];
                while (ms.Position + pack_size <= ms.Length)
                {
                    ms.Read(send, 0, pack_size);
                    if (!sync_client.send_pack(send))
                        return false;                                       // Disconnect or TimeOut
                }

                pack_size = (int)(ms.Length - ms.Position);

                if (pack_size > 0)
                {
                    send = new byte[pack_size];
                    ms.Read(send, 0, pack_size);
                    if (!sync_client.send_pack(send))
                        return false;
                }

                ms.Close();
                ms.Dispose();
            }

            return true;
        }

        public bool Send_File(FileStream stream)
        {
            SyncRequestHead head = new SyncRequestHead(SyncHash.Get_SHA1_Hash(stream), stream.Length, RequestType.File);
            byte[] send;
            int pack_size = sync_client.pack_size;

            if (head.size < 0) 
                return false;

            // Send Head first
            send = head.Get_Bytes();
            if (send == null) return false;
            if (!sync_client.send_pack(send))
                return false;

            // Send Data
            stream.Position = 0;

            send = new byte[pack_size];
            while (stream.Position + pack_size <= stream.Length)
            {
                stream.Read(send, 0, pack_size);
                if (!sync_client.send_pack(send))
                    return false;                                       // Disconnect or TimeOut
            }

            pack_size = (int)(stream.Length - stream.Position);

            if (pack_size > 0)
            {
                send = new byte[pack_size];
                stream.Read(send, 0, pack_size);
                if (!sync_client.send_pack(send))
                    return false;
            }

            return true;
        }

        #endregion

        #region Update

        // Receive Update
        public void update_loop(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sync_client.busy) return;
            sync_client.busy = true;

            if (!sync_client.connected)
            {
                Disconnect();
                return;
            }

            // Receive check
            if (sync_client.tcp_client.Available > 0)
            {
                SyncRequestHead head = sync_client.Receive_Request_Head();

                // Disconnect, TimeOut, LeftOverData
                if (head == null)
                {
                    Disconnect();
                    return;
                }

                determine_receive_head(head);
            }

            sync_client.busy = false;
        }

        private void determine_receive_head(SyncRequestHead head)
        {
            if (head.request_type == RequestType.FetchRequest)
            {
                if (!Sync_Fetch_Content(head))
                    Disconnect();
            }
            else if (head.request_type == RequestType.FileRequest)
            {
                if (!Sync_File(head))
                    Disconnect();
            }
        }

        #endregion

        // Sync Folder Functions
        #region SyncFolder Functions
        // Receive
        #region Receive

        private bool Sync_Fetch_Content(SyncRequestHead head)
        {
            byte[] data = Receive(head);
            if (data == null) return false;

            string sub_dir = Encoding.UTF8.GetString(data);

            if (sub_dir == "") sub_dir = "/";

            List<SyncFetchContent> fetch_content = sync_folder.get_sync_fetch_content(sub_dir);
            using (MemoryStream ms = new MemoryStream())
            { 
                byte[] buffer;
                for (int i = 0; i < fetch_content.Count; i++)
                {
                    buffer = fetch_content[i].Get_Bytes();
                    ms.Write(buffer,0,buffer.Length);
                }

                if (!Send(ms.ToArray(), RequestType.FetchContent))
                    return false;

                ms.Close();
                ms.Dispose();
            }

            return true;
        }
        private bool Sync_File(SyncRequestHead head)
        {
            byte[] data = Receive(head);
            if (data == null) 
                return false;

            string file_path = Encoding.UTF8.GetString(data).Split('\n')[0];            /////////////////////////////////

            if (file_path.Length == 0 || file_path.Substring(0, 1) != "/")
                file_path = "/" + file_path;


            // File !EXISTS
            if (!File.Exists(sync_folder.root_path + file_path))
                return Sync_File_Error(file_path,"File not found");

            using (FileStream stream = new FileStream(sync_folder.root_path + file_path, FileMode.Open, FileAccess.Read))
            {
                if (!Send_File(stream))
                    return false;
                stream.Close();
                stream.Dispose();
            }

            return true;
        }
        
        #endregion

        // Send
        // sub_dir = "/"=""=root "/sub/dir"=root:sub/dir
        #region Send

        public List<SyncFetchContent> Sync_Fetch_Request(string sub_dir)
        {
            // Set BUSY
            if (!set_busy()) return null;

            if (sub_dir == "") sub_dir = "/";

            // Send FetchRequest
            if (!Send(Encoding.UTF8.GetBytes(sub_dir),RequestType.FetchRequest))
                return null;

            // Wait for FetchContent
            SyncRequestHead r_head = sync_client.Receive_Request_Head_Wait(0);//10000);
            if (r_head == null || r_head.request_type != RequestType.FetchContent) return null;

            // Receive FetchContent
            byte[] fetch_data = Receive(r_head);
            if (fetch_data == null) return null;

            // Converte binary data to SyncFetchContent's
            List<SyncFetchContent> fetch_contents = new List<SyncFetchContent>();

            using (MemoryStream ms = new MemoryStream(fetch_data))
            using (BinaryReader reader = new BinaryReader(ms))
            {

                while (ms.Position < ms.Length)
                {
                    byte[] hash = reader.ReadBytes(SyncHash.hash_length);
                    List<byte> file_path = new List<byte>();
                    byte read;

                    while ((read = reader.ReadByte()) != 10) 
                        file_path.Add(read);

                    SyncFetchContent sfc = new SyncFetchContent(hash,Encoding.UTF8.GetString(file_path.ToArray()));
                    fetch_contents.Add(sfc);
                    file_path.Clear();
                }

                reader.Close();
                reader.Dispose();
                ms.Close();
                ms.Dispose();
            }

            // Unset BUSY
            sync_client.busy = false;

            return fetch_contents;
        }

        public bool Sync_File_Request(List<SyncFetchContent> fetch_content)
        {

            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < fetch_content.Count; i++)
                {
                    buffer = Encoding.UTF8.GetBytes(fetch_content[i].file_path+"\n");
                    ms.Write(buffer,0,buffer.Length);
                }

                if (!Send(ms.ToArray(), RequestType.FileRequest))
                    return false;

                ms.Close();
                ms.Dispose();
            }

            return true;
        }

        private bool Sync_File_Error(string file_name, string err_msg)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(file_name + "\n");
                ms.Write(buffer,0,buffer.Length);

                buffer = Encoding.UTF8.GetBytes(err_msg);
                ms.Write(buffer,0,buffer.Length);

                if (!Send(ms.ToArray(), RequestType.FileError))
                    return false;

                ms.Close();
                ms.Dispose();
            }

            return true;
        }

        #endregion
        #endregion

        // True= busy set and connected
        // False = busy not set and disconnected in the process
        public bool set_busy()
        {
            while ((sync_client.busy || sync_client.tcp_client.Available > 0) && sync_client.connected)
                Thread.Sleep(20);
            if (sync_client.connected)
            {
                sync_client.busy = true;
                return true;
            }
            return false;
        }
    }
}
