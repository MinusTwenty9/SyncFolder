using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SyncFolder
{
    public class SyncRequestHead
    {
        public static int length = 29;

        public byte[] hash;
        public long size;
        public byte type;
        public RequestType request_type;

        public SyncRequestHead(byte[] hash, long size, RequestType request_type)
        {
            this.hash = hash;
            this.size = size;
            this.request_type = request_type;
            this.type = (byte)request_type;
        }

        // hash[20] + size[8] + type[1] => 25 bytes
        public SyncRequestHead(byte[] head_data)
        {
            using (MemoryStream ms = new MemoryStream(head_data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                hash = reader.ReadBytes(20);
                size = reader.ReadInt64();
                type = reader.ReadByte();
                request_type = (RequestType)type;

                reader.Close();
                reader.Dispose();
                ms.Close();
                ms.Dispose();
            }
        }

        public byte[] Get_Bytes()
        {
            if (hash == null || type == null) return null;
            byte[] head = null;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(hash);
                writer.Write(size);
                writer.Write(type);

                head = ms.ToArray();

                writer.Close();
                writer.Dispose();
                ms.Close();
                ms.Dispose();
            }

            return head;
        }
    }

    public enum RequestType
    { 
        FetchRequest = 0,
        FetchContent = 1,
        FileRequest = 2,
        File = 3,
        FileError = 4
    }
}
