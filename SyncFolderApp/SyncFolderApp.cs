using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace SyncFolderApp
{
    public partial class form_main : Form
    {
        System.Windows.Forms.Timer timer;

        public form_main()
        {
            InitializeComponent();

            Settings.Load_Settings();
            SyncFolderHandler.Start_SyncNet();
            set_notification_bar();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 250;
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            float t = SyncFolderHandler.sync_net.sync_folder.files_to_x;
            float c = SyncFolderHandler.sync_net.sync_folder.files_x;
            float p = (c / t) * 100;

            label_stats.Text = c + "/" + t;
            label_percent.Text = (t == 0 ? "100" : ((int)p).ToString()) + "%";
        }

        
        #region Notification

        NotifyIcon notify_icon;
        
        private void set_notification_bar()
        {
            Icon ico = Properties.Resources.SyncFolder;

            notify_icon = new NotifyIcon();
            notify_icon.Visible = true;
            notify_icon.Icon = ico;
            notify_icon.DoubleClick += new EventHandler(notify_icon_DoubleClick);
        }

        void notify_icon_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.BringToFront();
        }

        #endregion

        #region Events

        private void button_edit_settings_Click(object sender, EventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + "/settings.txt");
        }

        private void button_change_folder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Settings.folder;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    disable_buttons();
                    Settings.folder = fbd.SelectedPath;
                    Settings.Save_Settings();
                    SyncFolderHandler.Reset_Settings();

                    Enable_Control(button_fetch, true);
                    Enable_Control(button_auto, true);
                    Enable_Control(button_change_folder, true);
                    MessageBox.Show("New Dir: " + fbd.SelectedPath);
                }
                else MessageBox.Show("Dir change failed");
            }
        }

        private void button_hide_form_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            //this.Opacity = 0.0;
            this.Visible = false;
        }

        #endregion
                
        #region SyncFolder

        private void button_fetch_Click(object sender, EventArgs e)
        {
            DispatchAsync(_start_fetch);
        }

        private void button_get_files_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
            DispatchAsync(_start_get_files);
        }

        private void button_merge_Click(object sender, EventArgs e)
        {
            DispatchAsync(_start_merge);
        }

        private void button_auto_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
            DispatchAsync(_start_auto_sync);
        }

        private bool start_fetch()
        {
            disable_buttons();
            WriteLine("Fetching...");

            if (!SyncFolderHandler.Fetch(text_box_sub_dir.Text))
            {
                WriteLine("Fetch Failed");
                Enable_Control(button_fetch, true);
                Enable_Control(button_auto, true);
                return false;
            }

            if (check_box_show_details.Checked)
            {
                WriteLine("// Fetched files:");
                for (int i = 0; i < SyncFolderHandler.sync_fetch.Count; i++)
                    WriteLine(SyncFolderHandler.sync_fetch[i].file_path);
            }

            WriteLine("Fetch Successfull. ( " + SyncFolderHandler.sync_fetch.Count + " files fetched )");

            Enable_Control(button_fetch, true);
            Enable_Control(button_get_files, true);
            Enable_Control(button_auto, true);
            return true;
        }

        private bool start_get_files()
        {
            disable_buttons();
            WriteLine("Getting Files...");

            if (!SyncFolderHandler.Sync_Files())
            {
                WriteLine("Get Files Failed");
                Enable_Control(button_fetch, true);
                Enable_Control(button_auto, true);
                return false;
            }


            if (check_box_show_details.Checked)
            {
                WriteLine("// Downloaded Files:");
                for (int i = 0; i < SyncFolderHandler.sync_net.sync_folder.downloaded.Count; i++)
                    WriteLine("Get: " + SyncFolderHandler.sync_net.sync_folder.downloaded[i].file_path);
                WriteLine("// Files to Delete:");
                for (int i = 0; i < SyncFolderHandler.sync_net.sync_folder.deleted.Count; i++)
                    WriteLine("Del: " + SyncFolderHandler.sync_net.sync_folder.deleted[i].file_path);
            }

            for (int i = 0; i < SyncFolderHandler.file_err.Count; i++)
                WriteLine("File err: " + SyncFolderHandler.file_err[i]);

            WriteLine("Getting Files Successfull. ( " + SyncFolderHandler.sync_net.sync_folder.downloaded.Count + " downloaded; "
                 + SyncFolderHandler.sync_net.sync_folder.deleted.Count + " deleted )");

            Enable_Control(button_fetch, true);
            Enable_Control(button_get_files, true);
            Enable_Control(button_merge, true);
            Enable_Control(button_auto, true);
            return true;
        }

        private bool start_merge()
        {
            disable_buttons();
            WriteLine("Merging...");

            if (!SyncFolderHandler.Merge())
            {
                WriteLine("Merge Failed");
                Enable_Control(button_fetch, true);
                Enable_Control(button_auto, true);
                return false;
            }

            for (int i = 0; i < SyncFolderHandler.merge_err.Count; i++)
                WriteLine("Merge err: " + SyncFolderHandler.file_err[i]);

            WriteLine("Merge Successfull");

            Enable_Control(button_fetch, true);
            Enable_Control(button_auto, true);
            return true;
        }

        private bool start_auto_sync()
        {
            WriteLine("Auto Syncing...");
            WriteLine("Root: " + SyncFolderHandler.sync_net.sync_folder.root_path + text_box_sub_dir.Text);

            // Fetch
            if (!start_fetch())
            {
                WriteLine("Auto Sync failed at Fetch");
                return false;
            }

            // Get FIles
            if (!start_get_files())
            {
                WriteLine("Auto Sync failed at Getting Files");
                return false;
            }
            else
            {
                if (SyncFolderHandler.file_err.Count > 0)
                    if (MessageBox.Show(SyncFolderHandler.file_err.Count + " File Errors Found, Merge anyways?", "Merge anyways?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        WriteLine("Auto Sync stopped at Getting files because of file errors");
                        return false;
                    }
            }

            // Merge
            if (!start_merge())
            {
                WriteLine("Auto Sync failed at Merging");
                return false;
            }
            else
            {
                if (SyncFolderHandler.merge_err.Count > 0)
                    MessageBox.Show("Merging encounterd " + SyncFolderHandler.merge_err.Count + " errors");
            }

            WriteLine("Auto Sync finished successfull");
            return true;
        }


        #region start void returns

        private void _start_fetch()
        {
            Clear();
            start_fetch();
            Enable_Control(button_change_folder, true);
        }

        private void _start_get_files()
        {
            start_get_files();

            this.label_percent.Invoke(new MethodInvoker(delegate()
                {
                    timer.Enabled = false;
                    timer_Tick(null,null);
                }));
            Enable_Control(button_change_folder, true);
        }

        private void _start_merge()
        {
            start_merge();
            Enable_Control(button_change_folder, true);
        }

        private void _start_auto_sync()
        {
            Clear();
            disable_buttons();

            start_auto_sync();

            this.label_percent.Invoke(new MethodInvoker(delegate()
            {
                timer.Enabled = false;
                timer_Tick(null, null);
            }));
            Enable_Control(button_fetch, true);
            Enable_Control(button_auto, true);
            Enable_Control(button_change_folder, true);
        }

        #endregion


        #endregion

        #region Helper Functions

        public void DispatchAsync(Action action)
        {
            var task = new WaitCallback(o => action.Invoke());
            ThreadPool.QueueUserWorkItem(task);
        }

        #region En/Disable Buttons

        public void Enable_Control(Control c, bool enable)
        {
            if (c.InvokeRequired)
            {
                enable_control d = new enable_control(Enable_Control);
                this.Invoke(d, new object[] { c, enable });
            }
            else
                c.Enabled = enable;
        }
        private void disable_buttons(bool enable = false)
        {
            Enable_Control(button_fetch, enable);
            Enable_Control(button_get_files, enable);
            Enable_Control(button_merge, enable);
            Enable_Control(button_auto, enable);
            Enable_Control(button_change_folder, enable);
        }

        #endregion

        public void WriteLine(string text)
        {
            if (this.list_box_info.InvokeRequired)
            {
                write_line_callback d = new write_line_callback(WriteLine);
                this.Invoke(d,new object[]{ text });
            }
            else 
                list_box_info.Items.Add(text);
        }

        public void Clear()
        {
            if (this.list_box_info.InvokeRequired)
            {
                clear d = new clear(Clear);
                this.Invoke(d, new object[] { });
            }
            else
                list_box_info.Items.Clear();
        }

        delegate void write_line_callback(string text);
        delegate void clear();
        delegate void enable_control(Control c, bool enable);
        #endregion

        private void stats_timer_Tick(object sender, EventArgs e)
        {
        }
    }
}
