namespace Nixxis.Windows.Controls
{
    partial class FileView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.trvFile = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // trvFile
            // 
            this.trvFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvFile.Location = new System.Drawing.Point(0, 0);
            this.trvFile.Name = "trvFile";
            this.trvFile.Size = new System.Drawing.Size(150, 281);
            this.trvFile.TabIndex = 0;
            this.trvFile.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvFile_AfterSelect);
            // 
            // FileView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trvFile);
            this.Name = "FileView";
            this.Size = new System.Drawing.Size(150, 281);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView trvFile;
    }
}
