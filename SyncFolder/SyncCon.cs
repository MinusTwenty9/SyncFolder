using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SyncFolder
{
    public class SyncCon
    {
        private TcpListener tcp_listener;
        private SyncNet sync_net;

        public bool connected = false;
        private bool connecting = false;
        public int port;
        public IPAddress ip;

        public event tcp_client_connected TcpClientConnected;

        public bool client_connected
        {
            get 
            {
                if (sync_net.sync_client != null && sync_net.sync_client.connected)
                    return true;
                else 
                    return false;
            }
        }

        public SyncCon(int port,SyncNet sync_net, bool test = false)
        {
            this.sync_net = sync_net;
            this.port = port;

            if (test == false)
                listener_start();
        }

        private void listener_start()
        {
            tcp_listener = new TcpListener(port);
            tcp_listener.Start();
            listener_start_accept();
        }

        private void listener_start_accept()
        {
            tcp_listener.BeginAcceptTcpClient(accept_tcp_client,tcp_listener);
        }

        private void accept_tcp_client(IAsyncResult res)
        {
            TcpClient client = tcp_listener.EndAcceptTcpClient(res);
            listener_start_accept();

            new_client(client);
        }

        private void new_client(TcpClient client)
        {
            // Check if you realy are still connected or if this 
            // is a attempt to reconnect from the other client's side
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            connected = client_connected;

            // Send client on to processing
            if (connecting == false && connected == false && TcpClientConnected != null)
            {
                connecting = true;
                connected = true;
                client.GetStream().WriteByte(1);
                TcpClientConnected.Invoke(new SyncClient(client, sync_net));
                Console.WriteLine("Connected");
                connecting = false;
            }
            // No one would take client, so tell him that
            // his connection didn't come through
            else
            {
                Console.WriteLine("Connection refused");
                client.GetStream().WriteByte(0);
                client.Close();
            }
        }

        public TcpClient Connect(IPAddress ip)
        {
            this.ip = ip;
            TcpClient client = null;
            for (int i = 0; i < 3; i++)
                try
                {
                    client = new TcpClient();
                    client.Connect(ip,port);

                    // Check if realy connected
                    int c_ms = 0;
                    while (client.Available <= 0 && c_ms < 1000)
                    {
                        Thread.Sleep(20);
                        c_ms += 20;
                    }
                    if(client.Available > 0)
                        if (client.GetStream().ReadByte() == 1)
                        {
                            connected = true;
                            break;
                        }
                    client.Close();
                    client = null;      // I
                }
                catch { client = null; }
            
            return client;
        }

        public void Disconnect()
        {
            connected = false;
        }

        public void Close()
        {
            if (connected || client_connected)
                Disconnect();

            tcp_listener.Stop();
            tcp_listener.Server.Close();
            tcp_listener.Server.Dispose();
        }
    }
    public delegate void tcp_client_connected(SyncClient client);
}
