using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;

namespace SyncFolderApp
{
    public static class Settings
    {
        public static string folder;
        public static int port;
        public static string ip;

        public static void Load_Settings()
        {
            folder = "./dir/";
            port = 37264;
            ip = "127.0.0.1";

            if (!File.Exists("./settings.txt"))
            {
                Save_Settings();

                return;
            }

            using (FileStream stream = new FileStream("./settings.txt", FileMode.Open, FileAccess.Read))
            using(StreamReader reader = new StreamReader(stream))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                    set_setting(line);

                reader.Close();
                reader.Dispose();
                stream.Close();
                stream.Dispose();
            }
        }

        public static void Save_Settings()
        {
            using (FileStream stream = new FileStream("./settings.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("folder=" + folder);
                    writer.WriteLine("port=" + port.ToString());
                    writer.WriteLine("ip=" + ip.ToString());
                    writer.Close();
                    writer.Dispose();
                }

                stream.Close();
                stream.Dispose();
            }
        }

        private static void set_setting(string line)
        {
            string[] setting = line.Split('=');
            string name = setting[0].ToLower();
            string detail = setting[1];

            if (name == "folder")
                folder = detail;
            else if (name == "port")
            {
                if (!int.TryParse(detail.Replace(" ", ""), out port))
                    port = 37264;
            }
            else if (name == "ip")
                ip = detail;

        }
    }
}
