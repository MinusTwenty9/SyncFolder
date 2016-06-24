using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SyncFolder
{
    public class SyncFileInfo
    {
        public string path;
        public string sub_dir;
        public long size;
        public byte[] hash;

        public SyncFileInfo(string path, string root_path)
        {
            this.path = path;
            comp_sub_dir(path,root_path);

            using (FileStream stream = File.OpenRead(path))
            {
                this.size = stream.Length;
                this.hash = SyncHash.Get_SHA1_Hash(stream);
                stream.Close();
                stream.Dispose();
            }
        }

        private void comp_sub_dir(string path, string root_path)
        {
            path = path.Replace("\\", "/");
            root_path = root_path.Replace("\\", "/");

            sub_dir = path.Substring(root_path.Length);
        }
    }
}
