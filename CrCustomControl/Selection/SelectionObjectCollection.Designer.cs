namespace Nixxis.Windows.Controls
{
    partial class SelectionObjectCollection
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
            this.txtNewItem = new System.Windows.Forms.TextBox();
            this.btnAddItem = new System.Windows.Forms.Button();
            this.btnDeleteItem = new System.Windows.Forms.Button();
            this.lblText = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lstItems = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDownItem = new System.Windows.Forms.Button();
            this.btnUpItem = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtNewItem
            // 
            this.txtNewItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNewItem.Location = new System.Drawing.Point(103, 3);
            this.txtNewItem.Multiline = true;
            this.txtNewItem.Name = "txtNewItem";
            this.txtNewItem.Size = new System.Drawing.Size(120, 20);
            this.txtNewItem.TabIndex = 17;
            // 
            // btnAddItem
            // 
            this.btnAddItem.Location = new System.Drawing.Point(229, 3);
            this.btnAddItem.Name = "btnAddItem";
            this.btnAddItem.Size = new System.Drawing.Size(22, 20);
            this.btnAddItem.TabIndex = 16;
            this.btnAddItem.Text = "+";
            this.btnAddItem.UseVisualStyleBackColor = true;
            this.btnAddItem.Click += new System.EventHandler(this.btnAddItem_Click);
            // 
            // btnDeleteItem
            // 
            this.btnDeleteItem.Location = new System.Drawing.Point(229, 85);
            this.btnDeleteItem.Name = "btnDeleteItem";
            this.btnDeleteItem.Size = new System.Drawing.Size(22, 20);
            this.btnDeleteItem.TabIndex = 15;
            this.btnDeleteItem.Text = "-";
            this.btnDeleteItem.UseVisualStyleBackColor = true;
            this.btnDeleteItem.Click += new System.EventHandler(this.btnDeleteItem_Click);
            // 
            // lblText
            // 
            this.lblText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblText.Location = new System.Drawing.Point(0, 0);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(100, 103);
            this.lblText.TabIndex = 13;
            this.lblText.Text = "Server(s)";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.Controls.Add(this.lstItems, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnAddItem, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnDeleteItem, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtNewItem, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnDownItem, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnUpItem, 2, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(254, 108);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // lstItems
            // 
            this.lstItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstItems.FormattingEnabled = true;
            this.lstItems.IntegralHeight = false;
            this.lstItems.Location = new System.Drawing.Point(103, 29);
            this.lstItems.Name = "lstItems";
            this.tableLayoutPanel1.SetRowSpan(this.lstItems, 4);
            this.lstItems.Size = new System.Drawing.Size(120, 76);
            this.lstItems.TabIndex = 19;
            this.lstItems.SelectedIndexChanged += new System.EventHandler(this.lstItems_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblText);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 5);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.panel1.Name = "panel1";
            this.tableLayoutPanel1.SetRowSpan(this.panel1, 5);
            this.panel1.Size = new System.Drawing.Size(100, 103);
            this.panel1.TabIndex = 18;
            // 
            // btnDownItem
            // 
            this.btnDownItem.Location = new System.Drawing.Point(229, 59);
            this.btnDownItem.Name = "btnDownItem";
            this.btnDownItem.Size = new System.Drawing.Size(22, 20);
            this.btnDownItem.TabIndex = 20;
            this.btnDownItem.Text = "˅";
            this.btnDownItem.UseVisualStyleBackColor = true;
            this.btnDownItem.Visible = false;
            // 
            // btnUpItem
            // 
            this.btnUpItem.Location = new System.Drawing.Point(229, 33);
            this.btnUpItem.Name = "btnUpItem";
            this.btnUpItem.Size = new System.Drawing.Size(22, 20);
            this.btnUpItem.TabIndex = 21;
            this.btnUpItem.Text = "˄";
            this.btnUpItem.UseVisualStyleBackColor = true;
            this.btnUpItem.Visible = false;
            // 
            // SelectionObjectCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SelectionObjectCollection";
            this.Size = new System.Drawing.Size(254, 108);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtNewItem;
        private System.Windows.Forms.Button btnAddItem;
        private System.Windows.Forms.Button btnDeleteItem;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.Button btnDownItem;
        private System.Windows.Forms.Button btnUpItem;
    }
}
