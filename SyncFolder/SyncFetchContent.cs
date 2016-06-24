using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncFolder
{
    public class SyncFetchContent
    {
        public byte[] hash;
        public string file_path;

        public SyncFetchContent(byte[] hash, string file_path)
        {
            this.hash = hash;
            this.file_path = file_path;
        }

        public SyncFetchContent(SyncFileInfo file_info)
        {
            this.hash = file_info.hash;
            this.file_path = file_info.sub_dir;
        }

        public byte[] Get_Bytes()
        { 
            byte[] file_path_b = Encoding.UTF8.GetBytes(file_path+'\n'.ToString());
            byte[] back = new byte[SyncHash.hash_length + file_path_b.Length ];
            Array.Copy(hash,0,back,0,SyncHash.hash_length);
            Array.Copy(file_path_b, 0, back, SyncHash.hash_length, file_path_b.Length);
            return back;
        }
    }
}
