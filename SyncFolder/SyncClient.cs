using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace SyncFolder
{
    public class SyncClient
    {
        public TcpClient tcp_client;
        public NetworkStream net_stream;
        public int time_out = 10000;
        public int retry_delay = 20;
        public int pack_size = 8192 * 256;

        public bool connected
        {
            get
            {
                try
                {
                    if (tcp_client != null && tcp_client.Client != null && tcp_client.Client.Connected)
                    {
                        // Detect if client disconnected
                        if (tcp_client.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcp_client.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool busy = false;
        public System.Timers.Timer update_timer;
        public int update_interval = 100;

        public SyncClient(TcpClient tcp_client, SyncNet sync_net)
        {
            this.tcp_client = tcp_client;
            this.net_stream = tcp_client.GetStream();

            time_out *= pack_size / (8192 * 64);

            update_timer = new System.Timers.Timer(update_interval);
            update_timer.Elapsed += new System.Timers.ElapsedEventHandler(sync_net.update_loop);
            update_timer.Start();
        }

        public void Disconnect()
        {
            update_timer.Stop();

            tcp_client.Close();
            net_stream.Close();
            net_stream.Dispose();
        }

        public void Close()
        {
            if (connected)
                Disconnect();
            net_stream.Dispose();
            update_timer.Stop();
        }

        // Send_Receive_Backend


        #region Raw

        // Receives raw bytes of a given length
        // Accounts for timeouts and disconnects
        private byte[] raw_receive(int length)
        {
            if (connected == false) return null;

            try
            {
                MemoryStream stream = new MemoryStream();
                byte[] r_buffer = new byte[length];

                int c_time = 0;
                while ((c_time += retry_delay) <= time_out)
                    if (tcp_client.Available >= 1)
                    {
                        int read = net_stream.Read(r_buffer, 0, length);
                        length -= read;
                        stream.Write(r_buffer, 0, read);
                        c_time = 0;

                        if (length == 0)
                        {
                            r_buffer = stream.ToArray();
                            stream.Close();
                            stream.Dispose();
                            return r_buffer;
                        }
                    }
                    else if (connected)
                        Thread.Sleep(retry_delay);
                    else return null;
                // Timeout
                return null;
            }
            catch { return null; }
        }


        // Sends raw bytes of a given send_buffer
        // Accounts for disconnects
        private bool raw_send(byte[] s_buffer)
        {
            if (connected == false) return false;

            try
            {
                net_stream.Write(s_buffer, 0, s_buffer.Length);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Pack's
        // Computes a hash for the pack data, sends the hash (64bit)
        // then sends the pack (needs to be standatised length)
        // waits for 0/1 (1 byte) from the other client to see if hash matches
        // 1 == return; 0 == repeat from send_hash;
        public  bool send_pack(byte[] s_pack)
        {
            byte[] hash = SyncHash.Get_SHA1_Hash(s_pack);
            bool hash_match;

            do
            {
                clear_netstream();
                raw_send(hash);
                raw_send(s_pack);

                byte[] received = raw_receive(1);
                if (received == null)
                {
                    Console.WriteLine("send fail");
                    return false;     // time out or disconnect, cancel everything
                }

                hash_match = received[0] == 1;
                if (hash_match == false)
                {
                    Console.WriteLine("Send: Pack Lost");
                }
            } while (hash_match == false);

            return true;
        }

        // Reads hash, reads pack, compares pack hash to received hash
        // if they match send 1 and return pack; else send 0 and return null;
        public byte[] receive_pack(int length)
        {
            byte[] hash;
            byte[] r_pack = null;
            bool hash_match;

            do
            {
                hash = raw_receive(20);
                r_pack = raw_receive(length);

                if (hash == null || r_pack == null)
                {
                    Console.WriteLine("receive fail");
                    return null;    // Time out or disconnect
                }
                hash_match = SyncHash.Comp_Hash(hash, SyncHash.Get_SHA1_Hash(r_pack));

                clear_netstream();

                if (hash_match) raw_send(new byte[] { 1 });
                else
                {
                    Console.WriteLine("Receive: Pack Lost");
                    raw_send(new byte[] { 0 });
                }

            } while (hash_match == false);

            return r_pack;
        }

        private void clear_netstream()
        {
            // Clean up netstream
            if (tcp_client.Available > 0)
            {
                byte[] clr = new byte[tcp_client.Available];
                net_stream.Read(clr, 0, clr.Length);
            }
        }
        #endregion

        #region Receive RequestHead
        public SyncRequestHead Receive_Request_Head_Wait(int time_out = 0)
        {
            int c_ms = 0;
            while ((time_out == 0 || c_ms < time_out) && tcp_client.Available <= 0)
            {
                Thread.Sleep(20);
                c_ms += 20;
            }

            if (tcp_client.Available <= 0) return null;

            return Receive_Request_Head();
        }

        public SyncRequestHead Receive_Request_Head()
        {
            byte[] data = receive_pack(SyncRequestHead.length);

            if (data == null) return null;
            return new SyncRequestHead(data);
        }
        #endregion

    }
}
