using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SyncFolder
{
    public class SyncFolder
    {
        #region Variables, Init

        public SyncNet sync_net;
        public string root_path;
        public string new_path;
        public string last_path;

        public List<SyncFetchContent> downloaded = new List<SyncFetchContent>();
        public List<SyncFetchContent> deleted = new List<SyncFetchContent>();

        // Stats
        public int files_to_x = 0;
        public int files_x = 0;
        
        public SyncFolder(SyncNet sync_net, string root_path)
        {
            this.sync_net = sync_net;
            
            // Prepear root_path
            root_path = Path.GetFullPath(root_path);
            root_path = root_path.Replace("\\","/");
            if (root_path.Substring(root_path.Length - 1, 1) == "/")
                root_path = root_path.Substring(0,root_path.Length-1);

            this.root_path = root_path;
            this.new_path = root_path + "/.sync/new";
            this.last_path = root_path + "/.sync/last";

            if (!Directory.Exists(root_path))
                Directory.CreateDirectory(root_path);
        }

        #endregion

        #region Sync_Fetch

        public List<SyncFetchContent> get_sync_fetch_content(string sub_dir)
        {
            // Prepear sub_dir
            if (sub_dir.Length == 0 || sub_dir.Substring(0, 1) != "/")
                sub_dir = "/" + sub_dir;

            string path = root_path + sub_dir;
            List<SyncFileInfo> files = Get_Files(path);
            List<SyncFetchContent> fetch_content = new List<SyncFetchContent>();

            for (int i = 0; i < files.Count; i++)
                fetch_content.Add(new SyncFetchContent(files[i]));

            files.Clear();
            
            return fetch_content;
        }

        #endregion

        #region Sync_File

        public bool Get_Sync_Files(string sub_dir, List<SyncFetchContent> fetch_content, ref List<string> err_files)
        {
            if (fetch_content == null)
                return false;

            // Prepear sub_dir
            if (sub_dir.Length == 0 || sub_dir.Substring(0, 1) != "/")
                sub_dir = "/" + sub_dir;

            string n_path = new_path + sub_dir;
            string path = root_path + sub_dir;

            // // Clear new_path && last_path
            if (!clear_dir(new_path) || !clear_dir(last_path ))
                return false;

            // Hide .sync folder
            DirectoryInfo di = new DirectoryInfo(root_path + "/.sync");
            if (di.Attributes != (FileAttributes.Directory | FileAttributes.Hidden))
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            

            // Calc changes
            List<SyncFileInfo> file_info = Get_Files(path);
            List<SyncFetchContent> del_files = null;
            List<SyncFetchContent> download_files = null;
            List<SyncFetchContent> new_changed_files;
            err_files = new List<string>();

            int new_changed_files_index = 0;

            get_sync_file_info(fetch_content, file_info, ref del_files, ref download_files);

            new_changed_files = download_files.ToArray().ToList();      /////////////////

            // Download Files

            // Set BUSY
            if (!sync_net.set_busy()) return false;

            // Stats
            files_to_x = download_files.Count;

            while (download_files.Count > 0)
            {
                if (!sync_net.Sync_File_Request(new List<SyncFetchContent>() { download_files[0] }))
                    if (!reconnect())
                        return false;

                SyncRequestHead head = sync_net.sync_client.Receive_Request_Head_Wait(10000);
                if (head == null)
                    return false;

                // File Error
                #region File Error
                if (head.request_type == RequestType.FileError)
                {
                    byte[] err_data = sync_net.Receive(head);
                    if (err_data == null)
                        return false;
                    string err_file_name = Encoding.UTF8.GetString(err_data).Split('\n')[0];
                    err_files.Add(err_file_name);
                    download_files.RemoveAt(0);
                    new_changed_files.RemoveAt(new_changed_files_index);
                    continue;
                }
                #endregion

                if (sync_net.Receive_File(head,download_files[0]))
                {
                    download_files.RemoveAt(0);
                    new_changed_files_index++;

                    // Stats
                    files_x = files_to_x - download_files.Count;
                }
                else if (sync_net.sync_client.connected == false) return false;     // Disconnected
            }

            this.downloaded = new_changed_files;
            this.deleted = del_files;

            sync_net.sync_client.busy = false;

            return true;
        }

        // Call after everything
        public bool Sync_File_Merge(ref List<string> err, bool del_empty_dir)
        {
            err = new List<string>();
            for (int i = 0; i < downloaded.Count; i++)
            {
                try
                {
                    dir_exists(root_path + downloaded[i].file_path);
                    if (File.Exists(root_path + downloaded[i].file_path))
                    {
                        dir_exists(last_path + downloaded[i].file_path);
                        File.Move(root_path + downloaded[i].file_path, last_path + downloaded[i].file_path);
                        if (!file_timout_exists(root_path + downloaded[i].file_path))
                            continue;

                        File.Move(new_path + downloaded[i].file_path, root_path + downloaded[i].file_path);
                        //File.Replace(new_path + downloaded[i].file_path, root_path + downloaded[i].file_path, last_path[i] + downloaded[i].file_path, false);
                    }
                    else
                    {
                        File.Move(new_path + downloaded[i].file_path, root_path + downloaded[i].file_path);
                        //File.Copy(new_path + downloaded[i].file_path, root_path + downloaded[i].file_path,true);
                    }
                }
                catch { err.Add(downloaded[i].file_path); }
            }

            for (int i = 0; i < deleted.Count; i++)
            {
                try 
                {
                    if (File.Exists(root_path + deleted[i].file_path))
                    {
                        dir_exists(last_path + deleted[i].file_path);
                        dir_exists(root_path + deleted[i].file_path);
                        File.Move(root_path + deleted[i].file_path, last_path + deleted[i].file_path);

                        if (del_empty_dir == true)
                        {
                            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(root_path + deleted[i].file_path));
                            if (di.GetFiles().Length == 0 && di.GetDirectories().Length == 0)
                                di.Delete();
                        }
                    }
                }
                catch { err.Add(deleted[i].file_path); }
            }

                return true;
        }

        // Go through all files in fetch
        // Del if not in fetch
        private void get_sync_file_info(List<SyncFetchContent> fetch_content, List<SyncFileInfo> file_info,
            ref List<SyncFetchContent> del, ref List<SyncFetchContent> download)
        {
            del = new List<SyncFetchContent>();
            download = new List<SyncFetchContent>();

            // Del && Changed
            for (int i = 0; i < file_info.Count; i++)
            {
                bool found = false;
                for (int y = 0; y < fetch_content.Count; y++)
                    if (fetch_content[y].file_path == file_info[i].sub_dir)
                    {
                        if (!SyncHash.Comp_Hash(fetch_content[y].hash, file_info[i].hash))
                            download.Add(new SyncFetchContent(file_info[i]));                   // File Changed

                        found = true;
                        break;
                    }
                if (found == false)
                    del.Add(new SyncFetchContent(file_info[i]));                                // File Deleted
            }

            // New
            for (int i = 0; i < fetch_content.Count; i++)
            {
                bool found = false;
                for (int y = 0; y < file_info.Count; y++)
                    if (fetch_content[i].file_path == file_info[y].sub_dir)
                    {
                        found = true;
                        break;
                    }

                if (found == false)
                    download.Add(fetch_content[i]);                                             // New File

            }
        }

        private bool clear_dir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }

            foreach (DirectoryInfo di in new DirectoryInfo(path).GetDirectories())
            {
                try
                {
                    di.Delete(true);
                }
                catch { return false; }
            }

            foreach (FileInfo fi in new DirectoryInfo(path).GetFiles())
            {
                try
                {
                    fi.Delete();
                }
                catch { return false; }
            }

            return true;
        }

        private void dir_exists(string file_path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(file_path)))
                Directory.CreateDirectory(Path.GetDirectoryName(file_path));
        }

        private bool file_timout_exists(string path)
        {
            int c_ms = 0;
            while (File.Exists(path) && (c_ms += 20) <= 500)
                Thread.Sleep(20);

            return c_ms <= 500;
        }

        #endregion

        #region Helper functions

        public List<SyncFileInfo> Get_Files(string path)
        {
            List<SyncFileInfo> files = new List<SyncFileInfo>();
            
            if (Directory.Exists(path) && path.ToLower() != "/.sync")
                get_files(new DirectoryInfo(path),ref files,path);
            
            return files;
        }

        private void get_files(DirectoryInfo di, ref List<SyncFileInfo> files,string root_path)
        {
            foreach (FileInfo fi in di.GetFiles())
                files.Add(new SyncFileInfo(fi.FullName,this.root_path));

            foreach (DirectoryInfo c_di in di.GetDirectories())
                if (c_di.Name.ToLower() != ".sync")
                    get_files(c_di, ref files, root_path);
        }

        #endregion

        public void Close()
        {
            root_path = "";
            new_path = "";
            last_path = "";

            if (downloaded != null) downloaded.Clear();
            if (deleted != null) deleted.Clear();
        }

        private bool reconnect()
        {
            return sync_net.Connect(sync_net.sync_con.ip);
        }
    }
}
