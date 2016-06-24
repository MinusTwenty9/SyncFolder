namespace SyncFolderApp
{
    partial class form_main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_main));
            this.button_change_folder = new System.Windows.Forms.Button();
            this.button_hide_form = new System.Windows.Forms.Button();
            this.list_box_info = new System.Windows.Forms.ListBox();
            this.button_edit_settings = new System.Windows.Forms.Button();
            this.text_box_sub_dir = new System.Windows.Forms.TextBox();
            this.button_fetch = new System.Windows.Forms.Button();
            this.button_get_files = new System.Windows.Forms.Button();
            this.button_merge = new System.Windows.Forms.Button();
            this.button_auto = new System.Windows.Forms.Button();
            this.check_box_show_details = new System.Windows.Forms.CheckBox();
            this.label_stats = new System.Windows.Forms.Label();
            this.label_percent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_change_folder
            // 
            this.button_change_folder.Location = new System.Drawing.Point(12, 12);
            this.button_change_folder.Name = "button_change_folder";
            this.button_change_folder.Size = new System.Drawing.Size(103, 23);
            this.button_change_folder.TabIndex = 0;
            this.button_change_folder.Text = "Change Folder";
            this.button_change_folder.UseVisualStyleBackColor = true;
            this.button_change_folder.Click += new System.EventHandler(this.button_change_folder_Click);
            // 
            // button_hide_form
            // 
            this.button_hide_form.Location = new System.Drawing.Point(12, 41);
            this.button_hide_form.Name = "button_hide_form";
            this.button_hide_form.Size = new System.Drawing.Size(103, 23);
            this.button_hide_form.TabIndex = 1;
            this.button_hide_form.Text = "Hide Form";
            this.button_hide_form.UseVisualStyleBackColor = true;
            this.button_hide_form.Click += new System.EventHandler(this.button_hide_form_Click);
            // 
            // list_box_info
            // 
            this.list_box_info.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.list_box_info.FormattingEnabled = true;
            this.list_box_info.Location = new System.Drawing.Point(121, 51);
            this.list_box_info.Name = "list_box_info";
            this.list_box_info.Size = new System.Drawing.Size(428, 225);
            this.list_box_info.TabIndex = 3;
            // 
            // button_edit_settings
            // 
            this.button_edit_settings.Location = new System.Drawing.Point(12, 70);
            this.button_edit_settings.Name = "button_edit_settings";
            this.button_edit_settings.Size = new System.Drawing.Size(103, 23);
            this.button_edit_settings.TabIndex = 4;
            this.button_edit_settings.Text = "Edit Settings";
            this.button_edit_settings.UseVisualStyleBackColor = true;
            this.button_edit_settings.Click += new System.EventHandler(this.button_edit_settings_Click);
            // 
            // text_box_sub_dir
            // 
            this.text_box_sub_dir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.text_box_sub_dir.Location = new System.Drawing.Point(121, 297);
            this.text_box_sub_dir.Name = "text_box_sub_dir";
            this.text_box_sub_dir.Size = new System.Drawing.Size(428, 20);
            this.text_box_sub_dir.TabIndex = 5;
            this.text_box_sub_dir.Text = "/";
            // 
            // button_fetch
            // 
            this.button_fetch.Location = new System.Drawing.Point(121, 12);
            this.button_fetch.Name = "button_fetch";
            this.button_fetch.Size = new System.Drawing.Size(103, 23);
            this.button_fetch.TabIndex = 6;
            this.button_fetch.Text = "Fetch";
            this.button_fetch.UseVisualStyleBackColor = true;
            this.button_fetch.Click += new System.EventHandler(this.button_fetch_Click);
            // 
            // button_get_files
            // 
            this.button_get_files.Enabled = false;
            this.button_get_files.Location = new System.Drawing.Point(230, 12);
            this.button_get_files.Name = "button_get_files";
            this.button_get_files.Size = new System.Drawing.Size(103, 23);
            this.button_get_files.TabIndex = 7;
            this.button_get_files.Text = "Get Files";
            this.button_get_files.UseVisualStyleBackColor = true;
            this.button_get_files.Click += new System.EventHandler(this.button_get_files_Click);
            // 
            // button_merge
            // 
            this.button_merge.Enabled = false;
            this.button_merge.Location = new System.Drawing.Point(339, 12);
            this.button_merge.Name = "button_merge";
            this.button_merge.Size = new System.Drawing.Size(103, 23);
            this.button_merge.TabIndex = 8;
            this.button_merge.Text = "Merge";
            this.button_merge.UseVisualStyleBackColor = true;
            this.button_merge.Click += new System.EventHandler(this.button_merge_Click);
            // 
            // button_auto
            // 
            this.button_auto.Location = new System.Drawing.Point(448, 12);
            this.button_auto.Name = "button_auto";
            this.button_auto.Size = new System.Drawing.Size(103, 23);
            this.button_auto.TabIndex = 9;
            this.button_auto.Text = "Auto Sync";
            this.button_auto.UseVisualStyleBackColor = true;
            this.button_auto.Click += new System.EventHandler(this.button_auto_Click);
            // 
            // check_box_show_details
            // 
            this.check_box_show_details.AutoSize = true;
            this.check_box_show_details.Location = new System.Drawing.Point(12, 108);
            this.check_box_show_details.Name = "check_box_show_details";
            this.check_box_show_details.Size = new System.Drawing.Size(88, 17);
            this.check_box_show_details.TabIndex = 10;
            this.check_box_show_details.Text = "Show Details";
            this.check_box_show_details.UseVisualStyleBackColor = true;
            // 
            // label_stats
            // 
            this.label_stats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_stats.AutoSize = true;
            this.label_stats.Location = new System.Drawing.Point(9, 278);
            this.label_stats.Name = "label_stats";
            this.label_stats.Size = new System.Drawing.Size(24, 13);
            this.label_stats.TabIndex = 11;
            this.label_stats.Text = "0/0";
            // 
            // label_percent
            // 
            this.label_percent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_percent.AutoSize = true;
            this.label_percent.Location = new System.Drawing.Point(9, 300);
            this.label_percent.Name = "label_percent";
            this.label_percent.Size = new System.Drawing.Size(21, 13);
            this.label_percent.TabIndex = 12;
            this.label_percent.Text = "0%";
            // 
            // form_main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 329);
            this.Controls.Add(this.label_percent);
            this.Controls.Add(this.label_stats);
            this.Controls.Add(this.check_box_show_details);
            this.Controls.Add(this.button_auto);
            this.Controls.Add(this.button_merge);
            this.Controls.Add(this.button_get_files);
            this.Controls.Add(this.button_fetch);
            this.Controls.Add(this.text_box_sub_dir);
            this.Controls.Add(this.button_edit_settings);
            this.Controls.Add(this.list_box_info);
            this.Controls.Add(this.button_hide_form);
            this.Controls.Add(this.button_change_folder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form_main";
            this.Text = "Sync Folder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_change_folder;
        private System.Windows.Forms.Button button_hide_form;
        private System.Windows.Forms.ListBox list_box_info;
        private System.Windows.Forms.Button button_edit_settings;
        private System.Windows.Forms.TextBox text_box_sub_dir;
        private System.Windows.Forms.Button button_fetch;
        private System.Windows.Forms.Button button_get_files;
        private System.Windows.Forms.Button button_merge;
        private System.Windows.Forms.Button button_auto;
        private System.Windows.Forms.CheckBox check_box_show_details;
        private System.Windows.Forms.Label label_stats;
        private System.Windows.Forms.Label label_percent;
    }
}

