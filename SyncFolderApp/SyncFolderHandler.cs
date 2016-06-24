using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SyncFolder;
using System.Net;
using System.Threading;

namespace SyncFolderApp
{
    public static class SyncFolderHandler
    {
        public static SyncNet sync_net;
        public static List<SyncFetchContent> sync_fetch;
        public static List<string> file_err;
        public static List<string> merge_err;

        public static bool busy = false;
        private static string sub_dir;

        public static void Start_SyncNet()
        {
            sync_net = new SyncNet(Settings.port, Settings.folder, false);
        }

        public static void Reset_Settings()
        {
            sync_net.Reset(Settings.folder);
        }

        public static bool Connect(string ip_s)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(ip_s, out ip))
                return false;

            return sync_net.Connect(ip);
        }

        // Fetch content and store in sync_fetch
        // False = err, try again
        public static bool Fetch(string _sub_dir)
        {
            if (!set_busy()) return (busy = false);          // Set busy
            if (!try_connection()) return (busy = false);     // Check the / Try to Connect / ion

            sub_dir = _sub_dir;
            sync_fetch = sync_net.Sync_Fetch_Request(sub_dir);
            if (sync_fetch == null) return (busy = false);

            busy = false;
            return true;
        }

        public static bool Sync_Files()
        {
            if (sync_fetch == null) return (busy=false);   // Unsuccessfull fetch
            if (!set_busy()) return (busy = false);          // Set busy
            if (!try_connection()) return (busy = false);     // Check the / Try to Connect / ion

            if (!sync_net.sync_folder.Get_Sync_Files(sub_dir, sync_fetch, ref file_err))return (busy=false);

            busy = false;
            return true;
        }

        public static bool Merge()
        {
            if (sync_fetch == null || file_err == null) return (busy = false);   // Unsuccessfull fetch || Unsuccessfull sync_file
            if (!set_busy()) return (busy = false);          // Set busy

            if (!sync_net.sync_folder.Sync_File_Merge(ref merge_err, true)) return (busy = false);

            busy = false;
            return true;
        }

        private static bool try_connection()
        {
            if (sync_net.sync_client == null || sync_net.sync_client.connected == false)
                if (!Connect(Settings.ip))
                    return false;
            return true;
        }

        public static bool set_busy()
        {   
            int c_ms = 0;
            while (busy) Thread.Sleep(50);
            busy = true;
            return true;
        }
    }
}
