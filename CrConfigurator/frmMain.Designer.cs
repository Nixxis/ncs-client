namespace CrConfigurator
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblVersionConfigFile = new System.Windows.Forms.Label();
            this.plServiceContainer = new System.Windows.Forms.Panel();
            this.tvServices = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblServiceTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbLocationl = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblVersionModel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 254F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblVersionModel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblVersionConfigFile, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.plServiceContainer, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tvServices, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1084, 524);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lblVersionConfigFile
            // 
            this.lblVersionConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVersionConfigFile.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblVersionConfigFile.Location = new System.Drawing.Point(3, 494);
            this.lblVersionConfigFile.Name = "lblVersionConfigFile";
            this.lblVersionConfigFile.Size = new System.Drawing.Size(248, 15);
            this.lblVersionConfigFile.TabIndex = 2;
            this.lblVersionConfigFile.Text = "Config item version";
            this.lblVersionConfigFile.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // plServiceContainer
            // 
            this.plServiceContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plServiceContainer.Location = new System.Drawing.Point(254, 65);
            this.plServiceContainer.Margin = new System.Windows.Forms.Padding(0);
            this.plServiceContainer.Name = "plServiceContainer";
            this.plServiceContainer.Size = new System.Drawing.Size(830, 429);
            this.plServiceContainer.TabIndex = 2;
            // 
            // tvServices
            // 
            this.tvServices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvServices.Location = new System.Drawing.Point(3, 33);
            this.tvServices.Name = "tvServices";
            this.tableLayoutPanel1.SetRowSpan(this.tvServices, 2);
            this.tvServices.Size = new System.Drawing.Size(248, 458);
            this.tvServices.TabIndex = 1;
            this.tvServices.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvServices_AfterSelect);
            // 
            // panel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.lblServiceTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1084, 30);
            this.panel1.TabIndex = 4;
            // 
            // lblServiceTitle
            // 
            this.lblServiceTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblServiceTitle.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServiceTitle.ForeColor = System.Drawing.Color.Blue;
            this.lblServiceTitle.Location = new System.Drawing.Point(0, 0);
            this.lblServiceTitle.Name = "lblServiceTitle";
            this.lblServiceTitle.Size = new System.Drawing.Size(1084, 30);
            this.lblServiceTitle.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lbLocationl);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(254, 30);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(830, 35);
            this.panel2.TabIndex = 5;
            // 
            // lbLocationl
            // 
            this.lbLocationl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbLocationl.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLocationl.Location = new System.Drawing.Point(0, 0);
            this.lbLocationl.Name = "lbLocationl";
            this.lbLocationl.Size = new System.Drawing.Size(830, 35);
            this.lbLocationl.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnSave);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(254, 494);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.tableLayoutPanel1.SetRowSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(830, 30);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(752, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(671, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblVersionModel
            // 
            this.lblVersionModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVersionModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersionModel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblVersionModel.Location = new System.Drawing.Point(3, 509);
            this.lblVersionModel.Name = "lblVersionModel";
            this.lblVersionModel.Size = new System.Drawing.Size(248, 15);
            this.lblVersionModel.TabIndex = 3;
            this.lblVersionModel.Text = "ConfigModel version";
            this.lblVersionModel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 524);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "Configurator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView tvServices;
        private System.Windows.Forms.Panel plServiceContainer;
        private System.Windows.Forms.Label lblServiceTitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbLocationl;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblVersionConfigFile;
        private System.Windows.Forms.Label lblVersionModel;
    }
}

