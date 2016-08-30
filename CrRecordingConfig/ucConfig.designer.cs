using System.Windows.Forms;
using System.Collections.Specialized;
using System.Drawing;
namespace ContactRoute.Recording.Config
{
    partial class ucConfig
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucConfig));
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkGeneralPreLoadFileNames = new System.Windows.Forms.CheckBox();
            this.txtGeneralDescription = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chkGeneralIsOfficeVisible = new System.Windows.Forms.CheckBox();
            this.chkGeneralIsCallCenterVisible = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkParameterCampaign = new System.Windows.Forms.CheckBox();
            this.chkParameterContact = new System.Windows.Forms.CheckBox();
            this.chkParameterDateTime = new System.Windows.Forms.CheckBox();
            this.chkParameterSearchType = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkParameterChat = new System.Windows.Forms.CheckBox();
            this.chkParameterDirectCalls = new System.Windows.Forms.CheckBox();
            this.chkParameterManuelCalls = new System.Windows.Forms.CheckBox();
            this.chkParameterOutbound = new System.Windows.Forms.CheckBox();
            this.chkParameterInbound = new System.Windows.Forms.CheckBox();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.chkGeneralScoreAvailable = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.chkGeneralContactListId = new System.Windows.Forms.CheckBox();
            this.chkGeneralIsOptionVisible = new System.Windows.Forms.CheckBox();
            this.chkDebugInfo = new System.Windows.Forms.CheckBox();
            this.chkGeneralRecordingId = new System.Windows.Forms.CheckBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.chkGeneralDeleteMainConfig = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.tabService = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtServiceRelayRoot = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtServiceUserRoot = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtServiceDataRoot = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtServiceAdminRoot = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.chkServiceAdminAvailable = new System.Windows.Forms.CheckBox();
            this.chkServiceDataAvailable = new System.Windows.Forms.CheckBox();
            this.chkServiceUserAvailable = new System.Windows.Forms.CheckBox();
            this.chkServiceRelayAvailable = new System.Windows.Forms.CheckBox();
            this.tabDatabase = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnConnectionAdmin = new System.Windows.Forms.Button();
            this.txtConnectionAdministrator = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.txtConnectionReporting = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.btnConnectionReporting = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.cboDatabaseReportingType = new System.Windows.Forms.ComboBox();
            this.cboDatabaseAdminType = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtTimeoutAdministrator = new System.Windows.Forms.TextBox();
            this.txtTimeoutReporting = new System.Windows.Forms.TextBox();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.textBox24 = new System.Windows.Forms.TextBox();
            this.label55 = new System.Windows.Forms.Label();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.tabTransfer = new System.Windows.Forms.TabPage();
            this.btnAddOfficeServer = new System.Windows.Forms.TabControl();
            this.tabContactCenterVoice = new System.Windows.Forms.TabPage();
            this.tlpTransferVoice = new System.Windows.Forms.TableLayoutPanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.gridTransferVoice = new System.Windows.Forms.DataGridView();
            this.gtv_Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gtv_Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gtv_UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gtv_Root = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnTransferVoiceSave = new System.Windows.Forms.Button();
            this.cboTransferVoiceType = new System.Windows.Forms.ComboBox();
            this.label26 = new System.Windows.Forms.Label();
            this.lblTransferVoiceHost = new System.Windows.Forms.Label();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.chkTransferVoiceActiveFtp = new System.Windows.Forms.CheckBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.txtTransferVoicePassword = new System.Windows.Forms.TextBox();
            this.txtTransferVoiceUser = new System.Windows.Forms.TextBox();
            this.lblTransferVoicePassword = new System.Windows.Forms.Label();
            this.lblTransferVoiceUser = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.sscTransferVoiceExt = new Nixxis.Windows.Controls.SelectionStringCollectionV1();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnTransferVoiceDelete = new System.Windows.Forms.Button();
            this.btnTransferVoiceAdd = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label32 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.tabContactCenterChat = new System.Windows.Forms.TabPage();
            this.tlpTransferChat = new System.Windows.Forms.TableLayoutPanel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.gridTransferChat = new System.Windows.Forms.DataGridView();
            this.gtc_Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gtc_Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gtc_UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gtc_Root = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnTransferChatSave = new System.Windows.Forms.Button();
            this.cboTransferChatType = new System.Windows.Forms.ComboBox();
            this.label33 = new System.Windows.Forms.Label();
            this.lblTransferChatHost = new System.Windows.Forms.Label();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.chkTransferChatActiveFtp = new System.Windows.Forms.CheckBox();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.txtTransferChatPassword = new System.Windows.Forms.TextBox();
            this.txtTransferChatUser = new System.Windows.Forms.TextBox();
            this.lblTransferChatPassword = new System.Windows.Forms.Label();
            this.lblTransferChatUser = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.textBox14 = new System.Windows.Forms.TextBox();
            this.panel8 = new System.Windows.Forms.Panel();
            this.btnTransferChatDelete = new System.Windows.Forms.Button();
            this.btnTransferChatAdd = new System.Windows.Forms.Button();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.label39 = new System.Windows.Forms.Label();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.sscTransferChatExt = new Nixxis.Windows.Controls.SelectionObjectCollection();
            this.tabContactCenterOffice = new System.Windows.Forms.TabPage();
            this.tlpTransferOffice = new System.Windows.Forms.TableLayoutPanel();
            this.panel9 = new System.Windows.Forms.Panel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.gridTransferOffice = new System.Windows.Forms.DataGridView();
            this.gto_Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gto_Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gto_UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gto_Root = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnTransferOfficeSave = new System.Windows.Forms.Button();
            this.cboTransferOfficeType = new System.Windows.Forms.ComboBox();
            this.label40 = new System.Windows.Forms.Label();
            this.lblTransferOfficeHost = new System.Windows.Forms.Label();
            this.textBox15 = new System.Windows.Forms.TextBox();
            this.chkTransferOfficeActiveFtp = new System.Windows.Forms.CheckBox();
            this.textBox16 = new System.Windows.Forms.TextBox();
            this.label42 = new System.Windows.Forms.Label();
            this.txtTransferOfficePassword = new System.Windows.Forms.TextBox();
            this.txtTransferOfficeUser = new System.Windows.Forms.TextBox();
            this.lblTransferOfficePassword = new System.Windows.Forms.Label();
            this.lblTransferOfficeUser = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.textBox19 = new System.Windows.Forms.TextBox();
            this.sscTransferOfficeNewExt = new Nixxis.Windows.Controls.SelectionStringCollectionV1();
            this.panel11 = new System.Windows.Forms.Panel();
            this.btnTransferOfficeDelete = new System.Windows.Forms.Button();
            this.btnTransferOfficeAdd = new System.Windows.Forms.Button();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.label46 = new System.Windows.Forms.Label();
            this.comboBox6 = new System.Windows.Forms.ComboBox();
            this.tabContactCenter = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.sscTransferCcExt = new Nixxis.Windows.Controls.SelectionStringCollectionV1();
            this.sscTransferCcServer = new Nixxis.Windows.Controls.SelectionStringCollectionV1();
            this.label12 = new System.Windows.Forms.Label();
            this.cboTransferCcType = new System.Windows.Forms.ComboBox();
            this.chkTransferCcUseActiveFtp = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtTransferCcUser = new System.Windows.Forms.TextBox();
            this.txtTransferCcRoot = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTransferCcPass = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label18 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabOffice = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.sscTransferOfficeExt = new Nixxis.Windows.Controls.SelectionStringCollectionV1();
            this.sscTransferOfficeServer = new Nixxis.Windows.Controls.SelectionStringCollectionV1();
            this.label8 = new System.Windows.Forms.Label();
            this.cboTransferOfficeTypeOld = new System.Windows.Forms.ComboBox();
            this.chkTransferOfficeUseActiveFtpOld = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.txtTransferOfficeUserOld = new System.Windows.Forms.TextBox();
            this.txtTransferOfficeRoot = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtTransferOfficePassOld = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label22 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tabScore = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label27 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.socScore = new Nixxis.Windows.Controls.SelectionObjectCollection();
            this.tabUserOptions = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.label53 = new System.Windows.Forms.Label();
            this.textBox22 = new System.Windows.Forms.TextBox();
            this.label54 = new System.Windows.Forms.Label();
            this.textBox23 = new System.Windows.Forms.TextBox();
            this.label50 = new System.Windows.Forms.Label();
            this.textBox20 = new System.Windows.Forms.TextBox();
            this.label51 = new System.Windows.Forms.Label();
            this.textBox21 = new System.Windows.Forms.TextBox();
            this.label52 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.btnFileFormatHelp = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.label29 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.tabService.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabDatabase.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.tabTransfer.SuspendLayout();
            this.btnAddOfficeServer.SuspendLayout();
            this.tabContactCenterVoice.SuspendLayout();
            this.tlpTransferVoice.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTransferVoice)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.tabContactCenterChat.SuspendLayout();
            this.tlpTransferChat.SuspendLayout();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTransferChat)).BeginInit();
            this.panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.tabContactCenterOffice.SuspendLayout();
            this.tlpTransferOffice.SuspendLayout();
            this.panel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTransferOffice)).BeginInit();
            this.panel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.tabContactCenter.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabOffice.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.tabScore.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.groupBox13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBox14.SuspendLayout();
            this.tabUserOptions.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgList
            // 
            this.imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList.ImageStream")));
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            this.imgList.Images.SetKeyName(0, "FTP32.png");
            this.imgList.Images.SetKeyName(1, "DB32.png");
            this.imgList.Images.SetKeyName(2, "generalproperties.png");
            this.imgList.Images.SetKeyName(3, "login.png");
            this.imgList.Images.SetKeyName(4, "UserOptions32.png");
            this.imgList.Images.SetKeyName(5, "Score-32.png");
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabService);
            this.tabControl1.Controls.Add(this.tabDatabase);
            this.tabControl1.Controls.Add(this.tabTransfer);
            this.tabControl1.Controls.Add(this.tabScore);
            this.tabControl1.Controls.Add(this.tabUserOptions);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ImageList = this.imgList;
            this.tabControl1.ItemSize = new System.Drawing.Size(58, 58);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(783, 485);
            this.tabControl1.TabIndex = 2;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.tabControl2);
            this.tabGeneral.ImageIndex = 2;
            this.tabGeneral.Location = new System.Drawing.Point(4, 62);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(775, 419);
            this.tabGeneral.TabIndex = 2;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(3, 3);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(769, 413);
            this.tabControl2.TabIndex = 30;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(761, 387);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox5, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.groupBox15, 1, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(755, 381);
            this.tableLayoutPanel2.TabIndex = 29;
            // 
            // groupBox5
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.groupBox5, 2);
            this.groupBox5.Controls.Add(this.chkGeneralPreLoadFileNames);
            this.groupBox5.Controls.Add(this.txtGeneralDescription);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(3, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(749, 69);
            this.groupBox5.TabIndex = 27;
            this.groupBox5.TabStop = false;
            // 
            // chkGeneralPreLoadFileNames
            // 
            this.chkGeneralPreLoadFileNames.AutoSize = true;
            this.chkGeneralPreLoadFileNames.Enabled = false;
            this.chkGeneralPreLoadFileNames.Location = new System.Drawing.Point(10, 45);
            this.chkGeneralPreLoadFileNames.Name = "chkGeneralPreLoadFileNames";
            this.chkGeneralPreLoadFileNames.Size = new System.Drawing.Size(164, 17);
            this.chkGeneralPreLoadFileNames.TabIndex = 25;
            this.chkGeneralPreLoadFileNames.Tag = "#PreLoadFileNames";
            this.chkGeneralPreLoadFileNames.Text = "Preload filenames form server";
            this.chkGeneralPreLoadFileNames.UseVisualStyleBackColor = true;
            // 
            // txtGeneralDescription
            // 
            this.txtGeneralDescription.Location = new System.Drawing.Point(77, 19);
            this.txtGeneralDescription.Name = "txtGeneralDescription";
            this.txtGeneralDescription.Size = new System.Drawing.Size(284, 20);
            this.txtGeneralDescription.TabIndex = 3;
            this.txtGeneralDescription.Tag = "#Description";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Description";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chkGeneralIsOfficeVisible);
            this.groupBox4.Controls.Add(this.chkGeneralIsCallCenterVisible);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 78);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(371, 94);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Search";
            // 
            // chkGeneralIsOfficeVisible
            // 
            this.chkGeneralIsOfficeVisible.AutoSize = true;
            this.chkGeneralIsOfficeVisible.Location = new System.Drawing.Point(10, 48);
            this.chkGeneralIsOfficeVisible.Name = "chkGeneralIsOfficeVisible";
            this.chkGeneralIsOfficeVisible.Size = new System.Drawing.Size(136, 17);
            this.chkGeneralIsOfficeVisible.TabIndex = 2;
            this.chkGeneralIsOfficeVisible.Tag = "#IsOfficeVisible";
            this.chkGeneralIsOfficeVisible.Text = "Office Search available";
            this.chkGeneralIsOfficeVisible.UseVisualStyleBackColor = true;
            // 
            // chkGeneralIsCallCenterVisible
            // 
            this.chkGeneralIsCallCenterVisible.AutoSize = true;
            this.chkGeneralIsCallCenterVisible.Location = new System.Drawing.Point(10, 23);
            this.chkGeneralIsCallCenterVisible.Name = "chkGeneralIsCallCenterVisible";
            this.chkGeneralIsCallCenterVisible.Size = new System.Drawing.Size(114, 17);
            this.chkGeneralIsCallCenterVisible.TabIndex = 3;
            this.chkGeneralIsCallCenterVisible.Tag = "#IsCallCenterVisible";
            this.chkGeneralIsCallCenterVisible.Text = "Call center search ";
            this.chkGeneralIsCallCenterVisible.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkParameterCampaign);
            this.groupBox1.Controls.Add(this.chkParameterContact);
            this.groupBox1.Controls.Add(this.chkParameterDateTime);
            this.groupBox1.Controls.Add(this.chkParameterSearchType);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(380, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(372, 94);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search option";
            // 
            // chkParameterCampaign
            // 
            this.chkParameterCampaign.AutoSize = true;
            this.chkParameterCampaign.Location = new System.Drawing.Point(117, 45);
            this.chkParameterCampaign.Name = "chkParameterCampaign";
            this.chkParameterCampaign.Size = new System.Drawing.Size(73, 17);
            this.chkParameterCampaign.TabIndex = 3;
            this.chkParameterCampaign.Text = "Campaign";
            this.chkParameterCampaign.UseVisualStyleBackColor = true;
            // 
            // chkParameterContact
            // 
            this.chkParameterContact.AutoSize = true;
            this.chkParameterContact.Location = new System.Drawing.Point(10, 45);
            this.chkParameterContact.Name = "chkParameterContact";
            this.chkParameterContact.Size = new System.Drawing.Size(63, 17);
            this.chkParameterContact.TabIndex = 2;
            this.chkParameterContact.Text = "Contact";
            this.chkParameterContact.UseVisualStyleBackColor = true;
            // 
            // chkParameterDateTime
            // 
            this.chkParameterDateTime.AutoSize = true;
            this.chkParameterDateTime.Location = new System.Drawing.Point(117, 22);
            this.chkParameterDateTime.Name = "chkParameterDateTime";
            this.chkParameterDateTime.Size = new System.Drawing.Size(77, 17);
            this.chkParameterDateTime.TabIndex = 1;
            this.chkParameterDateTime.Text = "Date/Time";
            this.chkParameterDateTime.UseVisualStyleBackColor = true;
            // 
            // chkParameterSearchType
            // 
            this.chkParameterSearchType.AutoSize = true;
            this.chkParameterSearchType.Location = new System.Drawing.Point(10, 22);
            this.chkParameterSearchType.Name = "chkParameterSearchType";
            this.chkParameterSearchType.Size = new System.Drawing.Size(84, 17);
            this.chkParameterSearchType.TabIndex = 0;
            this.chkParameterSearchType.Text = "SearchType";
            this.chkParameterSearchType.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkParameterChat);
            this.groupBox2.Controls.Add(this.chkParameterDirectCalls);
            this.groupBox2.Controls.Add(this.chkParameterManuelCalls);
            this.groupBox2.Controls.Add(this.chkParameterOutbound);
            this.groupBox2.Controls.Add(this.chkParameterInbound);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 178);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(371, 114);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Media types";
            // 
            // chkParameterChat
            // 
            this.chkParameterChat.AutoSize = true;
            this.chkParameterChat.Location = new System.Drawing.Point(116, 22);
            this.chkParameterChat.Name = "chkParameterChat";
            this.chkParameterChat.Size = new System.Drawing.Size(92, 17);
            this.chkParameterChat.TabIndex = 4;
            this.chkParameterChat.Text = "Chat contacts";
            this.chkParameterChat.UseVisualStyleBackColor = true;
            // 
            // chkParameterDirectCalls
            // 
            this.chkParameterDirectCalls.AutoSize = true;
            this.chkParameterDirectCalls.Location = new System.Drawing.Point(14, 91);
            this.chkParameterDirectCalls.Name = "chkParameterDirectCalls";
            this.chkParameterDirectCalls.Size = new System.Drawing.Size(78, 17);
            this.chkParameterDirectCalls.TabIndex = 3;
            this.chkParameterDirectCalls.Text = "Direct calls";
            this.chkParameterDirectCalls.UseVisualStyleBackColor = true;
            // 
            // chkParameterManuelCalls
            // 
            this.chkParameterManuelCalls.AutoSize = true;
            this.chkParameterManuelCalls.Location = new System.Drawing.Point(14, 68);
            this.chkParameterManuelCalls.Name = "chkParameterManuelCalls";
            this.chkParameterManuelCalls.Size = new System.Drawing.Size(85, 17);
            this.chkParameterManuelCalls.TabIndex = 2;
            this.chkParameterManuelCalls.Text = "Manuel calls";
            this.chkParameterManuelCalls.UseVisualStyleBackColor = true;
            // 
            // chkParameterOutbound
            // 
            this.chkParameterOutbound.AutoSize = true;
            this.chkParameterOutbound.Location = new System.Drawing.Point(14, 45);
            this.chkParameterOutbound.Name = "chkParameterOutbound";
            this.chkParameterOutbound.Size = new System.Drawing.Size(73, 17);
            this.chkParameterOutbound.TabIndex = 1;
            this.chkParameterOutbound.Text = "Outbound";
            this.chkParameterOutbound.UseVisualStyleBackColor = true;
            // 
            // chkParameterInbound
            // 
            this.chkParameterInbound.AutoSize = true;
            this.chkParameterInbound.Location = new System.Drawing.Point(14, 22);
            this.chkParameterInbound.Name = "chkParameterInbound";
            this.chkParameterInbound.Size = new System.Drawing.Size(65, 17);
            this.chkParameterInbound.TabIndex = 0;
            this.chkParameterInbound.Text = "Inbound";
            this.chkParameterInbound.UseVisualStyleBackColor = true;
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.checkBox3);
            this.groupBox15.Controls.Add(this.chkGeneralScoreAvailable);
            this.groupBox15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox15.Location = new System.Drawing.Point(380, 178);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(372, 114);
            this.groupBox15.TabIndex = 28;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "Contact Info option";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(10, 45);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(160, 17);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Tag = "#KeepRecordingAvailable";
            this.checkBox3.Text = "Keep recoding flag available";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // chkGeneralScoreAvailable
            // 
            this.chkGeneralScoreAvailable.AutoSize = true;
            this.chkGeneralScoreAvailable.Location = new System.Drawing.Point(10, 22);
            this.chkGeneralScoreAvailable.Name = "chkGeneralScoreAvailable";
            this.chkGeneralScoreAvailable.Size = new System.Drawing.Size(99, 17);
            this.chkGeneralScoreAvailable.TabIndex = 1;
            this.chkGeneralScoreAvailable.Tag = "#ScoreAvailable";
            this.chkGeneralScoreAvailable.Text = "Score available";
            this.chkGeneralScoreAvailable.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel6);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(761, 387);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.groupBox9, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.groupBox7, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(755, 381);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.chkGeneralContactListId);
            this.groupBox9.Controls.Add(this.chkGeneralIsOptionVisible);
            this.groupBox9.Controls.Add(this.chkDebugInfo);
            this.groupBox9.Controls.Add(this.chkGeneralRecordingId);
            this.groupBox9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox9.Location = new System.Drawing.Point(3, 103);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(749, 94);
            this.groupBox9.TabIndex = 1;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Info";
            // 
            // chkGeneralContactListId
            // 
            this.chkGeneralContactListId.AutoSize = true;
            this.chkGeneralContactListId.Location = new System.Drawing.Point(170, 19);
            this.chkGeneralContactListId.Name = "chkGeneralContactListId";
            this.chkGeneralContactListId.Size = new System.Drawing.Size(130, 17);
            this.chkGeneralContactListId.TabIndex = 24;
            this.chkGeneralContactListId.Tag = "#ContactListIdVisible";
            this.chkGeneralContactListId.Text = "ContactListId is visible";
            this.chkGeneralContactListId.UseVisualStyleBackColor = true;
            // 
            // chkGeneralIsOptionVisible
            // 
            this.chkGeneralIsOptionVisible.AutoSize = true;
            this.chkGeneralIsOptionVisible.Location = new System.Drawing.Point(14, 69);
            this.chkGeneralIsOptionVisible.Name = "chkGeneralIsOptionVisible";
            this.chkGeneralIsOptionVisible.Size = new System.Drawing.Size(131, 17);
            this.chkGeneralIsOptionVisible.TabIndex = 21;
            this.chkGeneralIsOptionVisible.Tag = "#IsOptionVisible";
            this.chkGeneralIsOptionVisible.Text = "Option menu available";
            this.chkGeneralIsOptionVisible.UseVisualStyleBackColor = true;
            this.chkGeneralIsOptionVisible.Visible = false;
            // 
            // chkDebugInfo
            // 
            this.chkDebugInfo.AutoSize = true;
            this.chkDebugInfo.Location = new System.Drawing.Point(14, 44);
            this.chkDebugInfo.Name = "chkDebugInfo";
            this.chkDebugInfo.Size = new System.Drawing.Size(58, 17);
            this.chkDebugInfo.TabIndex = 22;
            this.chkDebugInfo.Tag = "#DebugAllMsg";
            this.chkDebugInfo.Text = "Debug";
            this.chkDebugInfo.UseVisualStyleBackColor = true;
            // 
            // chkGeneralRecordingId
            // 
            this.chkGeneralRecordingId.AutoSize = true;
            this.chkGeneralRecordingId.Location = new System.Drawing.Point(14, 19);
            this.chkGeneralRecordingId.Name = "chkGeneralRecordingId";
            this.chkGeneralRecordingId.Size = new System.Drawing.Size(126, 17);
            this.chkGeneralRecordingId.TabIndex = 23;
            this.chkGeneralRecordingId.Tag = "#RecordingIdVisible";
            this.chkGeneralRecordingId.Text = "RecordingId is visible";
            this.chkGeneralRecordingId.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.checkBox10);
            this.groupBox7.Controls.Add(this.chkGeneralDeleteMainConfig);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox7.Location = new System.Drawing.Point(3, 3);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(749, 94);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Config file";
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.Location = new System.Drawing.Point(14, 42);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(82, 17);
            this.checkBox10.TabIndex = 26;
            this.checkBox10.Tag = "#UseStreamForHttp";
            this.checkBox10.Text = "Stream Http";
            this.checkBox10.UseVisualStyleBackColor = true;
            // 
            // chkGeneralDeleteMainConfig
            // 
            this.chkGeneralDeleteMainConfig.AutoSize = true;
            this.chkGeneralDeleteMainConfig.Checked = true;
            this.chkGeneralDeleteMainConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGeneralDeleteMainConfig.Enabled = false;
            this.chkGeneralDeleteMainConfig.Location = new System.Drawing.Point(14, 19);
            this.chkGeneralDeleteMainConfig.Name = "chkGeneralDeleteMainConfig";
            this.chkGeneralDeleteMainConfig.Size = new System.Drawing.Size(223, 17);
            this.chkGeneralDeleteMainConfig.TabIndex = 25;
            this.chkGeneralDeleteMainConfig.Tag = "#DeleteMainConfig";
            this.chkGeneralDeleteMainConfig.Text = "Delete only the main profile (Profile.config)";
            this.chkGeneralDeleteMainConfig.UseVisualStyleBackColor = true;
            this.chkGeneralDeleteMainConfig.CheckedChanged += new System.EventHandler(this.chkGeneralDeleteMainConfig_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.tableLayoutPanel8);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(761, 387);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Extra";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.groupBox10, 0, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 2;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(755, 381);
            this.tableLayoutPanel8.TabIndex = 31;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.label25);
            this.groupBox10.Controls.Add(this.label24);
            this.groupBox10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox10.Location = new System.Drawing.Point(3, 3);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(749, 44);
            this.groupBox10.TabIndex = 30;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Appserver";
            // 
            // label25
            // 
            this.label25.BackColor = System.Drawing.SystemColors.Info;
            this.label25.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label25.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label25.Location = new System.Drawing.Point(114, 16);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(228, 15);
            this.label25.TabIndex = 29;
            this.label25.Tag = "#UserXmlKeyFormat";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(7, 16);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(108, 13);
            this.label24.TabIndex = 28;
            this.label24.Text = "User Xml Key Format:";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabService
            // 
            this.tabService.Controls.Add(this.groupBox3);
            this.tabService.ImageKey = "login.png";
            this.tabService.Location = new System.Drawing.Point(4, 62);
            this.tabService.Name = "tabService";
            this.tabService.Padding = new System.Windows.Forms.Padding(3);
            this.tabService.Size = new System.Drawing.Size(775, 419);
            this.tabService.TabIndex = 3;
            this.tabService.Text = "Service";
            this.tabService.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(769, 413);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.txtServiceRelayRoot, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.label11, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtServiceUserRoot, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtServiceDataRoot, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtServiceAdminRoot, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label14, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label16, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkServiceAdminAvailable, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkServiceDataAvailable, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkServiceUserAvailable, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.chkServiceRelayAvailable, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(763, 394);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // txtServiceRelayRoot
            // 
            this.txtServiceRelayRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtServiceRelayRoot.Location = new System.Drawing.Point(123, 104);
            this.txtServiceRelayRoot.Name = "txtServiceRelayRoot";
            this.txtServiceRelayRoot.Size = new System.Drawing.Size(637, 20);
            this.txtServiceRelayRoot.TabIndex = 24;
            this.txtServiceRelayRoot.Tag = "#ServiceRelayRoot";
            // 
            // label11
            // 
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Location = new System.Drawing.Point(3, 101);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(94, 26);
            this.label11.TabIndex = 22;
            this.label11.Text = "Relay service";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtServiceUserRoot
            // 
            this.txtServiceUserRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtServiceUserRoot.Enabled = false;
            this.txtServiceUserRoot.Location = new System.Drawing.Point(123, 78);
            this.txtServiceUserRoot.Name = "txtServiceUserRoot";
            this.txtServiceUserRoot.Size = new System.Drawing.Size(637, 20);
            this.txtServiceUserRoot.TabIndex = 21;
            this.txtServiceUserRoot.Tag = "#UserXmlUrl";
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Enabled = false;
            this.label7.Location = new System.Drawing.Point(3, 75);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 26);
            this.label7.TabIndex = 19;
            this.label7.Text = "User service";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtServiceDataRoot
            // 
            this.txtServiceDataRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtServiceDataRoot.Location = new System.Drawing.Point(123, 52);
            this.txtServiceDataRoot.Name = "txtServiceDataRoot";
            this.txtServiceDataRoot.Size = new System.Drawing.Size(637, 20);
            this.txtServiceDataRoot.TabIndex = 18;
            this.txtServiceDataRoot.Tag = "#ServiceDataRoot";
            this.txtServiceDataRoot.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 26);
            this.label5.TabIndex = 16;
            this.label5.Text = "Data service";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtServiceAdminRoot
            // 
            this.txtServiceAdminRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtServiceAdminRoot.Location = new System.Drawing.Point(123, 26);
            this.txtServiceAdminRoot.Name = "txtServiceAdminRoot";
            this.txtServiceAdminRoot.Size = new System.Drawing.Size(637, 20);
            this.txtServiceAdminRoot.TabIndex = 15;
            this.txtServiceAdminRoot.Tag = "#ServiceAdminRoot";
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 26);
            this.label3.TabIndex = 0;
            this.label3.Text = "Admin service";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label14
            // 
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Blue;
            this.label14.Location = new System.Drawing.Point(0, 0);
            this.label14.Margin = new System.Windows.Forms.Padding(0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(100, 23);
            this.label14.TabIndex = 25;
            this.label14.Text = "Name";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label16
            // 
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Blue;
            this.label16.Location = new System.Drawing.Point(120, 0);
            this.label16.Margin = new System.Windows.Forms.Padding(0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(643, 23);
            this.label16.TabIndex = 27;
            this.label16.Text = "Connection root";
            this.label16.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // chkServiceAdminAvailable
            // 
            this.chkServiceAdminAvailable.AutoSize = true;
            this.chkServiceAdminAvailable.Checked = true;
            this.chkServiceAdminAvailable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServiceAdminAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkServiceAdminAvailable.Location = new System.Drawing.Point(103, 29);
            this.chkServiceAdminAvailable.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.chkServiceAdminAvailable.Name = "chkServiceAdminAvailable";
            this.chkServiceAdminAvailable.Size = new System.Drawing.Size(14, 17);
            this.chkServiceAdminAvailable.TabIndex = 28;
            this.chkServiceAdminAvailable.Tag = "#ServiceAdminAvailable";
            this.chkServiceAdminAvailable.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkServiceAdminAvailable.UseVisualStyleBackColor = true;
            this.chkServiceAdminAvailable.CheckedChanged += new System.EventHandler(this.chkServiceAdminAvailable_CheckedChanged);
            // 
            // chkServiceDataAvailable
            // 
            this.chkServiceDataAvailable.AutoSize = true;
            this.chkServiceDataAvailable.Checked = true;
            this.chkServiceDataAvailable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServiceDataAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkServiceDataAvailable.Location = new System.Drawing.Point(103, 55);
            this.chkServiceDataAvailable.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.chkServiceDataAvailable.Name = "chkServiceDataAvailable";
            this.chkServiceDataAvailable.Size = new System.Drawing.Size(14, 17);
            this.chkServiceDataAvailable.TabIndex = 29;
            this.chkServiceDataAvailable.Tag = "#ServiceDataAvailable";
            this.chkServiceDataAvailable.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkServiceDataAvailable.UseVisualStyleBackColor = true;
            this.chkServiceDataAvailable.CheckedChanged += new System.EventHandler(this.chkServiceDataAvailable_CheckedChanged);
            // 
            // chkServiceUserAvailable
            // 
            this.chkServiceUserAvailable.AutoSize = true;
            this.chkServiceUserAvailable.Checked = true;
            this.chkServiceUserAvailable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServiceUserAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkServiceUserAvailable.Enabled = false;
            this.chkServiceUserAvailable.Location = new System.Drawing.Point(103, 81);
            this.chkServiceUserAvailable.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.chkServiceUserAvailable.Name = "chkServiceUserAvailable";
            this.chkServiceUserAvailable.Size = new System.Drawing.Size(14, 17);
            this.chkServiceUserAvailable.TabIndex = 30;
            this.chkServiceUserAvailable.Tag = "#ServiceUserAvailable";
            this.chkServiceUserAvailable.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkServiceUserAvailable.UseVisualStyleBackColor = true;
            this.chkServiceUserAvailable.CheckedChanged += new System.EventHandler(this.chkServiceUserAvailable_CheckedChanged);
            // 
            // chkServiceRelayAvailable
            // 
            this.chkServiceRelayAvailable.AutoSize = true;
            this.chkServiceRelayAvailable.Checked = true;
            this.chkServiceRelayAvailable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServiceRelayAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkServiceRelayAvailable.Location = new System.Drawing.Point(103, 107);
            this.chkServiceRelayAvailable.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.chkServiceRelayAvailable.Name = "chkServiceRelayAvailable";
            this.chkServiceRelayAvailable.Size = new System.Drawing.Size(14, 17);
            this.chkServiceRelayAvailable.TabIndex = 31;
            this.chkServiceRelayAvailable.Tag = "#ServiceRelayAvailable";
            this.chkServiceRelayAvailable.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkServiceRelayAvailable.UseVisualStyleBackColor = true;
            this.chkServiceRelayAvailable.CheckedChanged += new System.EventHandler(this.chkServiceRelayAvailable_CheckedChanged);
            // 
            // tabDatabase
            // 
            this.tabDatabase.Controls.Add(this.groupBox8);
            this.tabDatabase.ImageIndex = 1;
            this.tabDatabase.Location = new System.Drawing.Point(4, 62);
            this.tabDatabase.Name = "tabDatabase";
            this.tabDatabase.Padding = new System.Windows.Forms.Padding(3);
            this.tabDatabase.Size = new System.Drawing.Size(775, 419);
            this.tabDatabase.TabIndex = 1;
            this.tabDatabase.Text = "Database";
            this.tabDatabase.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.tableLayoutPanel3);
            this.groupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox8.Location = new System.Drawing.Point(3, 3);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(769, 413);
            this.groupBox8.TabIndex = 15;
            this.groupBox8.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.Controls.Add(this.btnConnectionAdmin, 4, 2);
            this.tableLayoutPanel3.Controls.Add(this.txtConnectionAdministrator, 2, 2);
            this.tableLayoutPanel3.Controls.Add(this.label19, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.txtConnectionReporting, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.label20, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label21, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label23, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnConnectionReporting, 4, 1);
            this.tableLayoutPanel3.Controls.Add(this.label17, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.cboDatabaseReportingType, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.cboDatabaseAdminType, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.label15, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.txtTimeoutAdministrator, 3, 2);
            this.tableLayoutPanel3.Controls.Add(this.txtTimeoutReporting, 3, 1);
            this.tableLayoutPanel3.Controls.Add(this.groupBox12, 0, 3);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 5;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(763, 394);
            this.tableLayoutPanel3.TabIndex = 19;
            // 
            // btnConnectionAdmin
            // 
            this.btnConnectionAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConnectionAdmin.Location = new System.Drawing.Point(736, 52);
            this.btnConnectionAdmin.Name = "btnConnectionAdmin";
            this.btnConnectionAdmin.Size = new System.Drawing.Size(24, 20);
            this.btnConnectionAdmin.TabIndex = 30;
            this.btnConnectionAdmin.Text = "...";
            this.btnConnectionAdmin.UseVisualStyleBackColor = true;
            this.btnConnectionAdmin.Visible = false;
            // 
            // txtConnectionAdministrator
            // 
            this.txtConnectionAdministrator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConnectionAdministrator.Location = new System.Drawing.Point(253, 52);
            this.txtConnectionAdministrator.Name = "txtConnectionAdministrator";
            this.txtConnectionAdministrator.Size = new System.Drawing.Size(397, 20);
            this.txtConnectionAdministrator.TabIndex = 18;
            this.txtConnectionAdministrator.Tag = "#AdminConnectionString";
            // 
            // label19
            // 
            this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label19.Location = new System.Drawing.Point(3, 49);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(94, 26);
            this.label19.TabIndex = 16;
            this.label19.Text = "Adminstrator";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtConnectionReporting
            // 
            this.txtConnectionReporting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConnectionReporting.Location = new System.Drawing.Point(253, 26);
            this.txtConnectionReporting.Name = "txtConnectionReporting";
            this.txtConnectionReporting.Size = new System.Drawing.Size(397, 20);
            this.txtConnectionReporting.TabIndex = 15;
            this.txtConnectionReporting.Tag = "#ReportConnectionString";
            // 
            // label20
            // 
            this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label20.Location = new System.Drawing.Point(3, 23);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(94, 26);
            this.label20.TabIndex = 0;
            this.label20.Text = "Reporting";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label21
            // 
            this.label21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label21.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.ForeColor = System.Drawing.Color.Blue;
            this.label21.Location = new System.Drawing.Point(0, 0);
            this.label21.Margin = new System.Windows.Forms.Padding(0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(100, 23);
            this.label21.TabIndex = 25;
            this.label21.Text = "Description";
            this.label21.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label23
            // 
            this.label23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label23.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.ForeColor = System.Drawing.Color.Blue;
            this.label23.Location = new System.Drawing.Point(250, 0);
            this.label23.Margin = new System.Windows.Forms.Padding(0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(403, 23);
            this.label23.TabIndex = 27;
            this.label23.Text = "Connection";
            this.label23.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnConnectionReporting
            // 
            this.btnConnectionReporting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConnectionReporting.Location = new System.Drawing.Point(736, 26);
            this.btnConnectionReporting.Name = "btnConnectionReporting";
            this.btnConnectionReporting.Size = new System.Drawing.Size(24, 20);
            this.btnConnectionReporting.TabIndex = 28;
            this.btnConnectionReporting.Text = "...";
            this.btnConnectionReporting.UseVisualStyleBackColor = true;
            this.btnConnectionReporting.Visible = false;
            // 
            // label17
            // 
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Blue;
            this.label17.Location = new System.Drawing.Point(100, 0);
            this.label17.Margin = new System.Windows.Forms.Padding(0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(150, 23);
            this.label17.TabIndex = 31;
            this.label17.Text = "Type";
            this.label17.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cboDatabaseReportingType
            // 
            this.cboDatabaseReportingType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboDatabaseReportingType.FormattingEnabled = true;
            this.cboDatabaseReportingType.Location = new System.Drawing.Point(103, 26);
            this.cboDatabaseReportingType.Name = "cboDatabaseReportingType";
            this.cboDatabaseReportingType.Size = new System.Drawing.Size(144, 21);
            this.cboDatabaseReportingType.TabIndex = 32;
            // 
            // cboDatabaseAdminType
            // 
            this.cboDatabaseAdminType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboDatabaseAdminType.FormattingEnabled = true;
            this.cboDatabaseAdminType.Location = new System.Drawing.Point(103, 52);
            this.cboDatabaseAdminType.Name = "cboDatabaseAdminType";
            this.cboDatabaseAdminType.Size = new System.Drawing.Size(144, 21);
            this.cboDatabaseAdminType.TabIndex = 33;
            this.cboDatabaseAdminType.SelectedIndexChanged += new System.EventHandler(this.cboDatabaseAdminType_SelectedIndexChanged);
            // 
            // label15
            // 
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Blue;
            this.label15.Location = new System.Drawing.Point(653, 0);
            this.label15.Margin = new System.Windows.Forms.Padding(0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(80, 23);
            this.label15.TabIndex = 34;
            this.label15.Text = "Timeout";
            this.label15.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtTimeoutAdministrator
            // 
            this.txtTimeoutAdministrator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTimeoutAdministrator.Location = new System.Drawing.Point(656, 52);
            this.txtTimeoutAdministrator.Name = "txtTimeoutAdministrator";
            this.txtTimeoutAdministrator.Size = new System.Drawing.Size(74, 20);
            this.txtTimeoutAdministrator.TabIndex = 35;
            this.txtTimeoutAdministrator.Tag = "#AdminConnectionTimeout";
            // 
            // txtTimeoutReporting
            // 
            this.txtTimeoutReporting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTimeoutReporting.Location = new System.Drawing.Point(656, 26);
            this.txtTimeoutReporting.Name = "txtTimeoutReporting";
            this.txtTimeoutReporting.Size = new System.Drawing.Size(74, 20);
            this.txtTimeoutReporting.TabIndex = 36;
            this.txtTimeoutReporting.Tag = "#ReportConnectionTimeout";
            // 
            // groupBox12
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.groupBox12, 5);
            this.groupBox12.Controls.Add(this.textBox24);
            this.groupBox12.Controls.Add(this.label55);
            this.groupBox12.Controls.Add(this.checkBox9);
            this.groupBox12.Controls.Add(this.checkBox8);
            this.groupBox12.Controls.Add(this.checkBox7);
            this.groupBox12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox12.ForeColor = System.Drawing.Color.Blue;
            this.groupBox12.Location = new System.Drawing.Point(3, 78);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(757, 114);
            this.groupBox12.TabIndex = 37;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Reporting appserver options";
            // 
            // textBox24
            // 
            this.textBox24.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.textBox24.Location = new System.Drawing.Point(81, 91);
            this.textBox24.Name = "textBox24";
            this.textBox24.Size = new System.Drawing.Size(383, 20);
            this.textBox24.TabIndex = 4;
            this.textBox24.Tag = "#ReportingConnectionAppUrlExt";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label55.Location = new System.Drawing.Point(6, 94);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(71, 13);
            this.label55.TabIndex = 3;
            this.label55.Text = "Url extension:";
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.checkBox9.Location = new System.Drawing.Point(6, 65);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(72, 17);
            this.checkBox9.TabIndex = 2;
            this.checkBox9.Tag = "#ReportingConnectionAppUseSkip";
            this.checkBox9.Text = "Use Skip ";
            this.checkBox9.UseVisualStyleBackColor = true;
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.checkBox8.Location = new System.Drawing.Point(6, 42);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(114, 17);
            this.checkBox8.TabIndex = 1;
            this.checkBox8.Tag = "#ReportingConnectionAppUseNoNull";
            this.checkBox8.Text = "Use NoNull values";
            this.checkBox8.UseVisualStyleBackColor = true;
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.checkBox7.Location = new System.Drawing.Point(6, 19);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(128, 17);
            this.checkBox7.TabIndex = 0;
            this.checkBox7.Tag = "#ReportingConnectionAppUseV1";
            this.checkBox7.Text = "Use V1 transfer mode";
            this.checkBox7.UseVisualStyleBackColor = true;
            // 
            // tabTransfer
            // 
            this.tabTransfer.Controls.Add(this.btnAddOfficeServer);
            this.tabTransfer.ImageIndex = 0;
            this.tabTransfer.Location = new System.Drawing.Point(4, 62);
            this.tabTransfer.Name = "tabTransfer";
            this.tabTransfer.Padding = new System.Windows.Forms.Padding(3);
            this.tabTransfer.Size = new System.Drawing.Size(775, 419);
            this.tabTransfer.TabIndex = 0;
            this.tabTransfer.Text = "Transfer";
            this.tabTransfer.UseVisualStyleBackColor = true;
            // 
            // btnAddOfficeServer
            // 
            this.btnAddOfficeServer.Controls.Add(this.tabContactCenterVoice);
            this.btnAddOfficeServer.Controls.Add(this.tabContactCenterChat);
            this.btnAddOfficeServer.Controls.Add(this.tabContactCenterOffice);
            this.btnAddOfficeServer.Controls.Add(this.tabContactCenter);
            this.btnAddOfficeServer.Controls.Add(this.tabOffice);
            this.btnAddOfficeServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddOfficeServer.Location = new System.Drawing.Point(3, 3);
            this.btnAddOfficeServer.Name = "btnAddOfficeServer";
            this.btnAddOfficeServer.SelectedIndex = 0;
            this.btnAddOfficeServer.Size = new System.Drawing.Size(769, 413);
            this.btnAddOfficeServer.TabIndex = 15;
            this.btnAddOfficeServer.SelectedIndexChanged += new System.EventHandler(this.btnAddOfficeServer_SelectedIndexChanged);
            // 
            // tabContactCenterVoice
            // 
            this.tabContactCenterVoice.Controls.Add(this.tlpTransferVoice);
            this.tabContactCenterVoice.Location = new System.Drawing.Point(4, 22);
            this.tabContactCenterVoice.Name = "tabContactCenterVoice";
            this.tabContactCenterVoice.Padding = new System.Windows.Forms.Padding(3);
            this.tabContactCenterVoice.Size = new System.Drawing.Size(761, 387);
            this.tabContactCenterVoice.TabIndex = 3;
            this.tabContactCenterVoice.Text = "Voice";
            this.tabContactCenterVoice.UseVisualStyleBackColor = true;
            // 
            // tlpTransferVoice
            // 
            this.tlpTransferVoice.ColumnCount = 5;
            this.tlpTransferVoice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpTransferVoice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTransferVoice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tlpTransferVoice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpTransferVoice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTransferVoice.Controls.Add(this.panel5, 0, 2);
            this.tlpTransferVoice.Controls.Add(this.panel4, 0, 0);
            this.tlpTransferVoice.Controls.Add(this.btnTransferVoiceSave, 0, 9);
            this.tlpTransferVoice.Controls.Add(this.cboTransferVoiceType, 4, 3);
            this.tlpTransferVoice.Controls.Add(this.label26, 3, 3);
            this.tlpTransferVoice.Controls.Add(this.lblTransferVoiceHost, 0, 3);
            this.tlpTransferVoice.Controls.Add(this.textBox9, 1, 3);
            this.tlpTransferVoice.Controls.Add(this.chkTransferVoiceActiveFtp, 1, 8);
            this.tlpTransferVoice.Controls.Add(this.textBox8, 1, 7);
            this.tlpTransferVoice.Controls.Add(this.label30, 0, 7);
            this.tlpTransferVoice.Controls.Add(this.txtTransferVoicePassword, 1, 6);
            this.tlpTransferVoice.Controls.Add(this.txtTransferVoiceUser, 1, 5);
            this.tlpTransferVoice.Controls.Add(this.lblTransferVoicePassword, 0, 6);
            this.tlpTransferVoice.Controls.Add(this.lblTransferVoiceUser, 0, 5);
            this.tlpTransferVoice.Controls.Add(this.label28, 0, 4);
            this.tlpTransferVoice.Controls.Add(this.textBox6, 1, 4);
            this.tlpTransferVoice.Controls.Add(this.sscTransferVoiceExt, 3, 5);
            this.tlpTransferVoice.Controls.Add(this.panel3, 0, 1);
            this.tlpTransferVoice.Controls.Add(this.pictureBox3, 2, 4);
            this.tlpTransferVoice.Controls.Add(this.label32, 3, 4);
            this.tlpTransferVoice.Controls.Add(this.comboBox2, 4, 4);
            this.tlpTransferVoice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTransferVoice.Location = new System.Drawing.Point(3, 3);
            this.tlpTransferVoice.Name = "tlpTransferVoice";
            this.tlpTransferVoice.RowCount = 10;
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpTransferVoice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTransferVoice.Size = new System.Drawing.Size(755, 381);
            this.tlpTransferVoice.TabIndex = 18;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.LightGray;
            this.tlpTransferVoice.SetColumnSpan(this.panel5, 5);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(3, 185);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(749, 7);
            this.panel5.TabIndex = 25;
            // 
            // panel4
            // 
            this.tlpTransferVoice.SetColumnSpan(this.panel4, 5);
            this.panel4.Controls.Add(this.gridTransferVoice);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(755, 156);
            this.panel4.TabIndex = 24;
            // 
            // gridTransferVoice
            // 
            this.gridTransferVoice.AllowUserToAddRows = false;
            this.gridTransferVoice.AllowUserToDeleteRows = false;
            this.gridTransferVoice.AllowUserToOrderColumns = true;
            this.gridTransferVoice.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTransferVoice.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gtv_Server,
            this.gtv_Type,
            this.gtv_UserName,
            this.gtv_Root});
            this.gridTransferVoice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransferVoice.Location = new System.Drawing.Point(0, 0);
            this.gridTransferVoice.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.gridTransferVoice.MultiSelect = false;
            this.gridTransferVoice.Name = "gridTransferVoice";
            this.gridTransferVoice.ReadOnly = true;
            this.gridTransferVoice.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTransferVoice.Size = new System.Drawing.Size(755, 156);
            this.gridTransferVoice.TabIndex = 23;
            this.gridTransferVoice.SelectionChanged += new System.EventHandler(this.gridTransfer_SelectionChanged);
            // 
            // gtv_Server
            // 
            this.gtv_Server.HeaderText = "Server";
            this.gtv_Server.Name = "gtv_Server";
            this.gtv_Server.ReadOnly = true;
            // 
            // gtv_Type
            // 
            this.gtv_Type.HeaderText = "Type";
            this.gtv_Type.Name = "gtv_Type";
            this.gtv_Type.ReadOnly = true;
            this.gtv_Type.Width = 60;
            // 
            // gtv_UserName
            // 
            this.gtv_UserName.HeaderText = "User";
            this.gtv_UserName.Name = "gtv_UserName";
            this.gtv_UserName.ReadOnly = true;
            // 
            // gtv_Root
            // 
            this.gtv_Root.HeaderText = "Root";
            this.gtv_Root.Name = "gtv_Root";
            this.gtv_Root.ReadOnly = true;
            this.gtv_Root.Width = 300;
            // 
            // btnTransferVoiceSave
            // 
            this.btnTransferVoiceSave.Location = new System.Drawing.Point(3, 354);
            this.btnTransferVoiceSave.Name = "btnTransferVoiceSave";
            this.btnTransferVoiceSave.Size = new System.Drawing.Size(75, 23);
            this.btnTransferVoiceSave.TabIndex = 29;
            this.btnTransferVoiceSave.Text = "Save";
            this.btnTransferVoiceSave.UseVisualStyleBackColor = true;
            this.btnTransferVoiceSave.Click += new System.EventHandler(this.btnTransferVoiceSave_Click);
            // 
            // cboTransferVoiceType
            // 
            this.cboTransferVoiceType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTransferVoiceType.FormattingEnabled = true;
            this.cboTransferVoiceType.Location = new System.Drawing.Point(490, 198);
            this.cboTransferVoiceType.Name = "cboTransferVoiceType";
            this.cboTransferVoiceType.Size = new System.Drawing.Size(262, 21);
            this.cboTransferVoiceType.TabIndex = 22;
            this.cboTransferVoiceType.Tag = "$Type";
            this.cboTransferVoiceType.SelectedIndexChanged += new System.EventHandler(this.cboTransferVoiceType_SelectedIndexChanged);
            // 
            // label26
            // 
            this.label26.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label26.Location = new System.Drawing.Point(390, 195);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(94, 26);
            this.label26.TabIndex = 17;
            this.label26.Text = "Type";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTransferVoiceHost
            // 
            this.lblTransferVoiceHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTransferVoiceHost.Location = new System.Drawing.Point(3, 195);
            this.lblTransferVoiceHost.Name = "lblTransferVoiceHost";
            this.lblTransferVoiceHost.Size = new System.Drawing.Size(94, 26);
            this.lblTransferVoiceHost.TabIndex = 26;
            this.lblTransferVoiceHost.Text = "Host";
            this.lblTransferVoiceHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox9
            // 
            this.textBox9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox9.Location = new System.Drawing.Point(103, 198);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(262, 20);
            this.textBox9.TabIndex = 21;
            this.textBox9.Tag = "$Host";
            // 
            // chkTransferVoiceActiveFtp
            // 
            this.chkTransferVoiceActiveFtp.AutoSize = true;
            this.chkTransferVoiceActiveFtp.Location = new System.Drawing.Point(103, 328);
            this.chkTransferVoiceActiveFtp.Name = "chkTransferVoiceActiveFtp";
            this.chkTransferVoiceActiveFtp.Size = new System.Drawing.Size(92, 17);
            this.chkTransferVoiceActiveFtp.TabIndex = 15;
            this.chkTransferVoiceActiveFtp.Tag = "$UseActiveFtp";
            this.chkTransferVoiceActiveFtp.Text = "Use active ftp";
            this.chkTransferVoiceActiveFtp.UseVisualStyleBackColor = true;
            // 
            // textBox8
            // 
            this.textBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox8.Location = new System.Drawing.Point(103, 302);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(262, 20);
            this.textBox8.TabIndex = 26;
            this.textBox8.Tag = "$RequestTimeOut";
            // 
            // label30
            // 
            this.label30.Location = new System.Drawing.Point(3, 299);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(94, 26);
            this.label30.TabIndex = 21;
            this.label30.Text = "Request timeout";
            this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTransferVoicePassword
            // 
            this.txtTransferVoicePassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferVoicePassword.Location = new System.Drawing.Point(103, 276);
            this.txtTransferVoicePassword.Name = "txtTransferVoicePassword";
            this.txtTransferVoicePassword.Size = new System.Drawing.Size(262, 20);
            this.txtTransferVoicePassword.TabIndex = 25;
            this.txtTransferVoicePassword.Tag = "$Password";
            // 
            // txtTransferVoiceUser
            // 
            this.txtTransferVoiceUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferVoiceUser.Location = new System.Drawing.Point(103, 250);
            this.txtTransferVoiceUser.Name = "txtTransferVoiceUser";
            this.txtTransferVoiceUser.Size = new System.Drawing.Size(262, 20);
            this.txtTransferVoiceUser.TabIndex = 24;
            this.txtTransferVoiceUser.Tag = "$User";
            // 
            // lblTransferVoicePassword
            // 
            this.lblTransferVoicePassword.Location = new System.Drawing.Point(3, 273);
            this.lblTransferVoicePassword.Name = "lblTransferVoicePassword";
            this.lblTransferVoicePassword.Size = new System.Drawing.Size(94, 26);
            this.lblTransferVoicePassword.TabIndex = 6;
            this.lblTransferVoicePassword.Text = "Password";
            this.lblTransferVoicePassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTransferVoiceUser
            // 
            this.lblTransferVoiceUser.Location = new System.Drawing.Point(3, 247);
            this.lblTransferVoiceUser.Name = "lblTransferVoiceUser";
            this.lblTransferVoiceUser.Size = new System.Drawing.Size(94, 26);
            this.lblTransferVoiceUser.TabIndex = 0;
            this.lblTransferVoiceUser.Text = "User";
            this.lblTransferVoiceUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label28
            // 
            this.label28.Location = new System.Drawing.Point(3, 221);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(94, 26);
            this.label28.TabIndex = 2;
            this.label28.Text = "Root";
            this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox6
            // 
            this.textBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox6.Location = new System.Drawing.Point(103, 224);
            this.textBox6.Multiline = true;
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(262, 20);
            this.textBox6.TabIndex = 23;
            this.textBox6.Tag = "$Root";
            // 
            // sscTransferVoiceExt
            // 
            this.sscTransferVoiceExt.BackColor = System.Drawing.Color.Transparent;
            this.tlpTransferVoice.SetColumnSpan(this.sscTransferVoiceExt, 2);
            this.sscTransferVoiceExt.DisplayValue = "";
            this.sscTransferVoiceExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferVoiceExt.HasMetaData = false;
            this.sscTransferVoiceExt.IsAddVisible = false;
            this.sscTransferVoiceExt.IsDeleteVisible = false;
            this.sscTransferVoiceExt.Items = ((System.Collections.Specialized.StringCollection)(resources.GetObject("sscTransferVoiceExt.Items")));
            this.sscTransferVoiceExt.Location = new System.Drawing.Point(387, 247);
            this.sscTransferVoiceExt.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferVoiceExt.Name = "sscTransferVoiceExt";
            this.sscTransferVoiceExt.NewItem = "";
            this.sscTransferVoiceExt.ReturnValue = "";
            this.tlpTransferVoice.SetRowSpan(this.sscTransferVoiceExt, 5);
            this.sscTransferVoiceExt.Size = new System.Drawing.Size(368, 134);
            this.sscTransferVoiceExt.TabIndex = 16;
            this.sscTransferVoiceExt.Tag = "";
            this.sscTransferVoiceExt.TextLabel = "Extension";
            this.sscTransferVoiceExt.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferVoiceExt.TextLabelWidth = 100F;
            // 
            // panel3
            // 
            this.tlpTransferVoice.SetColumnSpan(this.panel3, 5);
            this.panel3.Controls.Add(this.btnTransferVoiceDelete);
            this.panel3.Controls.Add(this.btnTransferVoiceAdd);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 156);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(755, 26);
            this.panel3.TabIndex = 28;
            // 
            // btnTransferVoiceDelete
            // 
            this.btnTransferVoiceDelete.Location = new System.Drawing.Point(84, 2);
            this.btnTransferVoiceDelete.Name = "btnTransferVoiceDelete";
            this.btnTransferVoiceDelete.Size = new System.Drawing.Size(75, 23);
            this.btnTransferVoiceDelete.TabIndex = 1;
            this.btnTransferVoiceDelete.Text = "Delete";
            this.btnTransferVoiceDelete.UseVisualStyleBackColor = true;
            this.btnTransferVoiceDelete.Click += new System.EventHandler(this.btnTransferVoiceDelete_Click);
            // 
            // btnTransferVoiceAdd
            // 
            this.btnTransferVoiceAdd.Location = new System.Drawing.Point(3, 2);
            this.btnTransferVoiceAdd.Name = "btnTransferVoiceAdd";
            this.btnTransferVoiceAdd.Size = new System.Drawing.Size(75, 23);
            this.btnTransferVoiceAdd.TabIndex = 0;
            this.btnTransferVoiceAdd.Text = "Add";
            this.btnTransferVoiceAdd.UseVisualStyleBackColor = true;
            this.btnTransferVoiceAdd.Click += new System.EventHandler(this.btnTransferVoiceAdd_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::CrRecordingConfig.Properties.Resources.help;
            this.pictureBox3.Location = new System.Drawing.Point(371, 224);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(16, 16);
            this.pictureBox3.TabIndex = 13;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.InfoTransferRoot_Click);
            // 
            // label32
            // 
            this.label32.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label32.Location = new System.Drawing.Point(390, 221);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(94, 26);
            this.label32.TabIndex = 30;
            this.label32.Text = "MediaType";
            this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox2
            // 
            this.comboBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox2.Enabled = false;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(490, 224);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(262, 21);
            this.comboBox2.TabIndex = 31;
            this.comboBox2.Tag = "$MediaType";
            // 
            // tabContactCenterChat
            // 
            this.tabContactCenterChat.Controls.Add(this.tlpTransferChat);
            this.tabContactCenterChat.Location = new System.Drawing.Point(4, 22);
            this.tabContactCenterChat.Name = "tabContactCenterChat";
            this.tabContactCenterChat.Padding = new System.Windows.Forms.Padding(3);
            this.tabContactCenterChat.Size = new System.Drawing.Size(761, 387);
            this.tabContactCenterChat.TabIndex = 2;
            this.tabContactCenterChat.Text = "Chat";
            this.tabContactCenterChat.UseVisualStyleBackColor = true;
            // 
            // tlpTransferChat
            // 
            this.tlpTransferChat.ColumnCount = 5;
            this.tlpTransferChat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpTransferChat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTransferChat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tlpTransferChat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpTransferChat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTransferChat.Controls.Add(this.panel6, 0, 2);
            this.tlpTransferChat.Controls.Add(this.panel7, 0, 0);
            this.tlpTransferChat.Controls.Add(this.btnTransferChatSave, 0, 9);
            this.tlpTransferChat.Controls.Add(this.cboTransferChatType, 4, 3);
            this.tlpTransferChat.Controls.Add(this.label33, 3, 3);
            this.tlpTransferChat.Controls.Add(this.lblTransferChatHost, 0, 3);
            this.tlpTransferChat.Controls.Add(this.textBox10, 1, 3);
            this.tlpTransferChat.Controls.Add(this.chkTransferChatActiveFtp, 1, 8);
            this.tlpTransferChat.Controls.Add(this.textBox11, 1, 7);
            this.tlpTransferChat.Controls.Add(this.label35, 0, 7);
            this.tlpTransferChat.Controls.Add(this.txtTransferChatPassword, 1, 6);
            this.tlpTransferChat.Controls.Add(this.txtTransferChatUser, 1, 5);
            this.tlpTransferChat.Controls.Add(this.lblTransferChatPassword, 0, 6);
            this.tlpTransferChat.Controls.Add(this.lblTransferChatUser, 0, 5);
            this.tlpTransferChat.Controls.Add(this.label38, 0, 4);
            this.tlpTransferChat.Controls.Add(this.textBox14, 1, 4);
            this.tlpTransferChat.Controls.Add(this.panel8, 0, 1);
            this.tlpTransferChat.Controls.Add(this.pictureBox4, 2, 4);
            this.tlpTransferChat.Controls.Add(this.label39, 3, 4);
            this.tlpTransferChat.Controls.Add(this.comboBox4, 4, 4);
            this.tlpTransferChat.Controls.Add(this.sscTransferChatExt, 3, 5);
            this.tlpTransferChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTransferChat.Location = new System.Drawing.Point(3, 3);
            this.tlpTransferChat.Name = "tlpTransferChat";
            this.tlpTransferChat.RowCount = 10;
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTransferChat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTransferChat.Size = new System.Drawing.Size(755, 381);
            this.tlpTransferChat.TabIndex = 19;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.LightGray;
            this.tlpTransferChat.SetColumnSpan(this.panel6, 5);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(3, 185);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(749, 7);
            this.panel6.TabIndex = 25;
            // 
            // panel7
            // 
            this.tlpTransferChat.SetColumnSpan(this.panel7, 5);
            this.panel7.Controls.Add(this.gridTransferChat);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(0, 0);
            this.panel7.Margin = new System.Windows.Forms.Padding(0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(755, 156);
            this.panel7.TabIndex = 24;
            // 
            // gridTransferChat
            // 
            this.gridTransferChat.AllowUserToAddRows = false;
            this.gridTransferChat.AllowUserToDeleteRows = false;
            this.gridTransferChat.AllowUserToOrderColumns = true;
            this.gridTransferChat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTransferChat.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gtc_Server,
            this.gtc_Type,
            this.gtc_UserName,
            this.gtc_Root});
            this.gridTransferChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransferChat.Location = new System.Drawing.Point(0, 0);
            this.gridTransferChat.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.gridTransferChat.MultiSelect = false;
            this.gridTransferChat.Name = "gridTransferChat";
            this.gridTransferChat.ReadOnly = true;
            this.gridTransferChat.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTransferChat.Size = new System.Drawing.Size(755, 156);
            this.gridTransferChat.TabIndex = 23;
            this.gridTransferChat.SelectionChanged += new System.EventHandler(this.gridTransfer_SelectionChanged);
            // 
            // gtc_Server
            // 
            this.gtc_Server.HeaderText = "Server";
            this.gtc_Server.Name = "gtc_Server";
            this.gtc_Server.ReadOnly = true;
            // 
            // gtc_Type
            // 
            this.gtc_Type.HeaderText = "Type";
            this.gtc_Type.Name = "gtc_Type";
            this.gtc_Type.ReadOnly = true;
            this.gtc_Type.Width = 60;
            // 
            // gtc_UserName
            // 
            this.gtc_UserName.HeaderText = "User";
            this.gtc_UserName.Name = "gtc_UserName";
            this.gtc_UserName.ReadOnly = true;
            // 
            // gtc_Root
            // 
            this.gtc_Root.HeaderText = "Root";
            this.gtc_Root.Name = "gtc_Root";
            this.gtc_Root.ReadOnly = true;
            this.gtc_Root.Width = 300;
            // 
            // btnTransferChatSave
            // 
            this.btnTransferChatSave.Location = new System.Drawing.Point(3, 354);
            this.btnTransferChatSave.Name = "btnTransferChatSave";
            this.btnTransferChatSave.Size = new System.Drawing.Size(75, 23);
            this.btnTransferChatSave.TabIndex = 29;
            this.btnTransferChatSave.Text = "Save";
            this.btnTransferChatSave.UseVisualStyleBackColor = true;
            this.btnTransferChatSave.Click += new System.EventHandler(this.btnTransferChatSave_Click);
            // 
            // cboTransferChatType
            // 
            this.cboTransferChatType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTransferChatType.FormattingEnabled = true;
            this.cboTransferChatType.Location = new System.Drawing.Point(490, 198);
            this.cboTransferChatType.Name = "cboTransferChatType";
            this.cboTransferChatType.Size = new System.Drawing.Size(262, 21);
            this.cboTransferChatType.TabIndex = 18;
            this.cboTransferChatType.Tag = "$Type";
            this.cboTransferChatType.SelectedIndexChanged += new System.EventHandler(this.cboTransferChatType_SelectedIndexChanged);
            // 
            // label33
            // 
            this.label33.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label33.Location = new System.Drawing.Point(390, 195);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(94, 26);
            this.label33.TabIndex = 17;
            this.label33.Text = "Type";
            this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTransferChatHost
            // 
            this.lblTransferChatHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTransferChatHost.Location = new System.Drawing.Point(3, 195);
            this.lblTransferChatHost.Name = "lblTransferChatHost";
            this.lblTransferChatHost.Size = new System.Drawing.Size(94, 26);
            this.lblTransferChatHost.TabIndex = 26;
            this.lblTransferChatHost.Text = "Host";
            this.lblTransferChatHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox10
            // 
            this.textBox10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox10.Location = new System.Drawing.Point(103, 198);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(262, 20);
            this.textBox10.TabIndex = 27;
            this.textBox10.Tag = "$Host";
            // 
            // chkTransferChatActiveFtp
            // 
            this.chkTransferChatActiveFtp.AutoSize = true;
            this.chkTransferChatActiveFtp.Location = new System.Drawing.Point(103, 328);
            this.chkTransferChatActiveFtp.Name = "chkTransferChatActiveFtp";
            this.chkTransferChatActiveFtp.Size = new System.Drawing.Size(92, 17);
            this.chkTransferChatActiveFtp.TabIndex = 15;
            this.chkTransferChatActiveFtp.Tag = "$UseActiveFtp";
            this.chkTransferChatActiveFtp.Text = "Use active ftp";
            this.chkTransferChatActiveFtp.UseVisualStyleBackColor = true;
            // 
            // textBox11
            // 
            this.textBox11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox11.Location = new System.Drawing.Point(103, 302);
            this.textBox11.Multiline = true;
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(262, 20);
            this.textBox11.TabIndex = 22;
            this.textBox11.Tag = "$RequestTimeOut";
            // 
            // label35
            // 
            this.label35.Location = new System.Drawing.Point(3, 299);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(94, 26);
            this.label35.TabIndex = 21;
            this.label35.Text = "Request timeout";
            this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTransferChatPassword
            // 
            this.txtTransferChatPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferChatPassword.Location = new System.Drawing.Point(103, 276);
            this.txtTransferChatPassword.Name = "txtTransferChatPassword";
            this.txtTransferChatPassword.Size = new System.Drawing.Size(262, 20);
            this.txtTransferChatPassword.TabIndex = 7;
            this.txtTransferChatPassword.Tag = "$Password";
            // 
            // txtTransferChatUser
            // 
            this.txtTransferChatUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferChatUser.Location = new System.Drawing.Point(103, 250);
            this.txtTransferChatUser.Name = "txtTransferChatUser";
            this.txtTransferChatUser.Size = new System.Drawing.Size(262, 20);
            this.txtTransferChatUser.TabIndex = 1;
            this.txtTransferChatUser.Tag = "$User";
            // 
            // lblTransferChatPassword
            // 
            this.lblTransferChatPassword.Location = new System.Drawing.Point(3, 273);
            this.lblTransferChatPassword.Name = "lblTransferChatPassword";
            this.lblTransferChatPassword.Size = new System.Drawing.Size(94, 26);
            this.lblTransferChatPassword.TabIndex = 6;
            this.lblTransferChatPassword.Text = "Password";
            this.lblTransferChatPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTransferChatUser
            // 
            this.lblTransferChatUser.Location = new System.Drawing.Point(3, 247);
            this.lblTransferChatUser.Name = "lblTransferChatUser";
            this.lblTransferChatUser.Size = new System.Drawing.Size(94, 26);
            this.lblTransferChatUser.TabIndex = 0;
            this.lblTransferChatUser.Text = "User";
            this.lblTransferChatUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label38
            // 
            this.label38.Location = new System.Drawing.Point(3, 221);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(94, 26);
            this.label38.TabIndex = 2;
            this.label38.Text = "Root";
            this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox14
            // 
            this.textBox14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox14.Location = new System.Drawing.Point(103, 224);
            this.textBox14.Multiline = true;
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new System.Drawing.Size(262, 20);
            this.textBox14.TabIndex = 3;
            this.textBox14.Tag = "$Root";
            // 
            // panel8
            // 
            this.tlpTransferChat.SetColumnSpan(this.panel8, 5);
            this.panel8.Controls.Add(this.btnTransferChatDelete);
            this.panel8.Controls.Add(this.btnTransferChatAdd);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(0, 156);
            this.panel8.Margin = new System.Windows.Forms.Padding(0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(755, 26);
            this.panel8.TabIndex = 28;
            // 
            // btnTransferChatDelete
            // 
            this.btnTransferChatDelete.Location = new System.Drawing.Point(84, 2);
            this.btnTransferChatDelete.Name = "btnTransferChatDelete";
            this.btnTransferChatDelete.Size = new System.Drawing.Size(75, 23);
            this.btnTransferChatDelete.TabIndex = 1;
            this.btnTransferChatDelete.Text = "Delete";
            this.btnTransferChatDelete.UseVisualStyleBackColor = true;
            this.btnTransferChatDelete.Click += new System.EventHandler(this.btnTransferChatDelete_Click);
            // 
            // btnTransferChatAdd
            // 
            this.btnTransferChatAdd.Location = new System.Drawing.Point(3, 2);
            this.btnTransferChatAdd.Name = "btnTransferChatAdd";
            this.btnTransferChatAdd.Size = new System.Drawing.Size(75, 23);
            this.btnTransferChatAdd.TabIndex = 0;
            this.btnTransferChatAdd.Text = "Add";
            this.btnTransferChatAdd.UseVisualStyleBackColor = true;
            this.btnTransferChatAdd.Click += new System.EventHandler(this.btnTransferChatAdd_Click);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::CrRecordingConfig.Properties.Resources.help;
            this.pictureBox4.Location = new System.Drawing.Point(371, 224);
            this.pictureBox4.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(16, 16);
            this.pictureBox4.TabIndex = 13;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.InfoTransferRoot_Click);
            // 
            // label39
            // 
            this.label39.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label39.Location = new System.Drawing.Point(390, 221);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(94, 26);
            this.label39.TabIndex = 30;
            this.label39.Text = "MediaType";
            this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox4
            // 
            this.comboBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox4.Enabled = false;
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Location = new System.Drawing.Point(490, 224);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(262, 21);
            this.comboBox4.TabIndex = 31;
            this.comboBox4.Tag = "$MediaType";
            // 
            // sscTransferChatExt
            // 
            this.sscTransferChatExt.BackColor = System.Drawing.Color.Transparent;
            this.tlpTransferChat.SetColumnSpan(this.sscTransferChatExt, 2);
            this.sscTransferChatExt.DisplayValue = "";
            this.sscTransferChatExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferChatExt.IsAddVisible = false;
            this.sscTransferChatExt.IsDeleteVisible = false;
            this.sscTransferChatExt.ItemType = null;
            this.sscTransferChatExt.Location = new System.Drawing.Point(387, 247);
            this.sscTransferChatExt.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferChatExt.MaxNumberOfItems = -1;
            this.sscTransferChatExt.Name = "sscTransferChatExt";
            this.sscTransferChatExt.NewItem = "";
            this.tlpTransferChat.SetRowSpan(this.sscTransferChatExt, 5);
            this.sscTransferChatExt.Size = new System.Drawing.Size(368, 134);
            this.sscTransferChatExt.TabIndex = 32;
            this.sscTransferChatExt.TextLabel = "Extension";
            this.sscTransferChatExt.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferChatExt.TextLabelWidth = 100F;
            // 
            // tabContactCenterOffice
            // 
            this.tabContactCenterOffice.Controls.Add(this.tlpTransferOffice);
            this.tabContactCenterOffice.Location = new System.Drawing.Point(4, 22);
            this.tabContactCenterOffice.Name = "tabContactCenterOffice";
            this.tabContactCenterOffice.Padding = new System.Windows.Forms.Padding(3);
            this.tabContactCenterOffice.Size = new System.Drawing.Size(761, 387);
            this.tabContactCenterOffice.TabIndex = 4;
            this.tabContactCenterOffice.Text = "Office";
            this.tabContactCenterOffice.UseVisualStyleBackColor = true;
            // 
            // tlpTransferOffice
            // 
            this.tlpTransferOffice.ColumnCount = 5;
            this.tlpTransferOffice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpTransferOffice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTransferOffice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tlpTransferOffice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpTransferOffice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTransferOffice.Controls.Add(this.panel9, 0, 2);
            this.tlpTransferOffice.Controls.Add(this.panel10, 0, 0);
            this.tlpTransferOffice.Controls.Add(this.btnTransferOfficeSave, 0, 9);
            this.tlpTransferOffice.Controls.Add(this.cboTransferOfficeType, 4, 3);
            this.tlpTransferOffice.Controls.Add(this.label40, 3, 3);
            this.tlpTransferOffice.Controls.Add(this.lblTransferOfficeHost, 0, 3);
            this.tlpTransferOffice.Controls.Add(this.textBox15, 1, 3);
            this.tlpTransferOffice.Controls.Add(this.chkTransferOfficeActiveFtp, 1, 8);
            this.tlpTransferOffice.Controls.Add(this.textBox16, 1, 7);
            this.tlpTransferOffice.Controls.Add(this.label42, 0, 7);
            this.tlpTransferOffice.Controls.Add(this.txtTransferOfficePassword, 1, 6);
            this.tlpTransferOffice.Controls.Add(this.txtTransferOfficeUser, 1, 5);
            this.tlpTransferOffice.Controls.Add(this.lblTransferOfficePassword, 0, 6);
            this.tlpTransferOffice.Controls.Add(this.lblTransferOfficeUser, 0, 5);
            this.tlpTransferOffice.Controls.Add(this.label45, 0, 4);
            this.tlpTransferOffice.Controls.Add(this.textBox19, 1, 4);
            this.tlpTransferOffice.Controls.Add(this.sscTransferOfficeNewExt, 3, 5);
            this.tlpTransferOffice.Controls.Add(this.panel11, 0, 1);
            this.tlpTransferOffice.Controls.Add(this.pictureBox5, 2, 4);
            this.tlpTransferOffice.Controls.Add(this.label46, 3, 4);
            this.tlpTransferOffice.Controls.Add(this.comboBox6, 4, 4);
            this.tlpTransferOffice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTransferOffice.Location = new System.Drawing.Point(3, 3);
            this.tlpTransferOffice.Name = "tlpTransferOffice";
            this.tlpTransferOffice.RowCount = 10;
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpTransferOffice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTransferOffice.Size = new System.Drawing.Size(755, 381);
            this.tlpTransferOffice.TabIndex = 19;
            // 
            // panel9
            // 
            this.panel9.BackColor = System.Drawing.Color.LightGray;
            this.tlpTransferOffice.SetColumnSpan(this.panel9, 5);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel9.Location = new System.Drawing.Point(3, 185);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(749, 7);
            this.panel9.TabIndex = 25;
            // 
            // panel10
            // 
            this.tlpTransferOffice.SetColumnSpan(this.panel10, 5);
            this.panel10.Controls.Add(this.gridTransferOffice);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel10.Location = new System.Drawing.Point(0, 0);
            this.panel10.Margin = new System.Windows.Forms.Padding(0);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(755, 156);
            this.panel10.TabIndex = 24;
            // 
            // gridTransferOffice
            // 
            this.gridTransferOffice.AllowUserToAddRows = false;
            this.gridTransferOffice.AllowUserToDeleteRows = false;
            this.gridTransferOffice.AllowUserToOrderColumns = true;
            this.gridTransferOffice.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTransferOffice.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gto_Server,
            this.gto_Type,
            this.gto_UserName,
            this.gto_Root});
            this.gridTransferOffice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTransferOffice.Location = new System.Drawing.Point(0, 0);
            this.gridTransferOffice.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.gridTransferOffice.MultiSelect = false;
            this.gridTransferOffice.Name = "gridTransferOffice";
            this.gridTransferOffice.ReadOnly = true;
            this.gridTransferOffice.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridTransferOffice.Size = new System.Drawing.Size(755, 156);
            this.gridTransferOffice.TabIndex = 23;
            this.gridTransferOffice.SelectionChanged += new System.EventHandler(this.gridTransfer_SelectionChanged);
            // 
            // gto_Server
            // 
            this.gto_Server.HeaderText = "Server";
            this.gto_Server.Name = "gto_Server";
            this.gto_Server.ReadOnly = true;
            // 
            // gto_Type
            // 
            this.gto_Type.HeaderText = "Type";
            this.gto_Type.Name = "gto_Type";
            this.gto_Type.ReadOnly = true;
            this.gto_Type.Width = 60;
            // 
            // gto_UserName
            // 
            this.gto_UserName.HeaderText = "User";
            this.gto_UserName.Name = "gto_UserName";
            this.gto_UserName.ReadOnly = true;
            // 
            // gto_Root
            // 
            this.gto_Root.HeaderText = "Root";
            this.gto_Root.Name = "gto_Root";
            this.gto_Root.ReadOnly = true;
            this.gto_Root.Width = 300;
            // 
            // btnTransferOfficeSave
            // 
            this.btnTransferOfficeSave.Location = new System.Drawing.Point(3, 354);
            this.btnTransferOfficeSave.Name = "btnTransferOfficeSave";
            this.btnTransferOfficeSave.Size = new System.Drawing.Size(75, 23);
            this.btnTransferOfficeSave.TabIndex = 29;
            this.btnTransferOfficeSave.Text = "Save";
            this.btnTransferOfficeSave.UseVisualStyleBackColor = true;
            this.btnTransferOfficeSave.Click += new System.EventHandler(this.btnTransferOfficeSave_Click);
            // 
            // cboTransferOfficeType
            // 
            this.cboTransferOfficeType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTransferOfficeType.FormattingEnabled = true;
            this.cboTransferOfficeType.Location = new System.Drawing.Point(490, 198);
            this.cboTransferOfficeType.Name = "cboTransferOfficeType";
            this.cboTransferOfficeType.Size = new System.Drawing.Size(262, 21);
            this.cboTransferOfficeType.TabIndex = 18;
            this.cboTransferOfficeType.Tag = "$Type";
            this.cboTransferOfficeType.SelectedIndexChanged += new System.EventHandler(this.cboTransferOfficeType_SelectedIndexChanged);
            // 
            // label40
            // 
            this.label40.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label40.Location = new System.Drawing.Point(390, 195);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(94, 26);
            this.label40.TabIndex = 17;
            this.label40.Text = "Type";
            this.label40.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTransferOfficeHost
            // 
            this.lblTransferOfficeHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTransferOfficeHost.Location = new System.Drawing.Point(3, 195);
            this.lblTransferOfficeHost.Name = "lblTransferOfficeHost";
            this.lblTransferOfficeHost.Size = new System.Drawing.Size(94, 26);
            this.lblTransferOfficeHost.TabIndex = 26;
            this.lblTransferOfficeHost.Text = "Host";
            this.lblTransferOfficeHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox15
            // 
            this.textBox15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox15.Location = new System.Drawing.Point(103, 198);
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new System.Drawing.Size(262, 20);
            this.textBox15.TabIndex = 27;
            this.textBox15.Tag = "$Host";
            // 
            // chkTransferOfficeActiveFtp
            // 
            this.chkTransferOfficeActiveFtp.AutoSize = true;
            this.chkTransferOfficeActiveFtp.Location = new System.Drawing.Point(103, 328);
            this.chkTransferOfficeActiveFtp.Name = "chkTransferOfficeActiveFtp";
            this.chkTransferOfficeActiveFtp.Size = new System.Drawing.Size(92, 17);
            this.chkTransferOfficeActiveFtp.TabIndex = 15;
            this.chkTransferOfficeActiveFtp.Tag = "$UseActiveFtp";
            this.chkTransferOfficeActiveFtp.Text = "Use active ftp";
            this.chkTransferOfficeActiveFtp.UseVisualStyleBackColor = true;
            // 
            // textBox16
            // 
            this.textBox16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox16.Location = new System.Drawing.Point(103, 302);
            this.textBox16.Multiline = true;
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new System.Drawing.Size(262, 20);
            this.textBox16.TabIndex = 22;
            this.textBox16.Tag = "$RequestTimeOut";
            // 
            // label42
            // 
            this.label42.Location = new System.Drawing.Point(3, 299);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(94, 26);
            this.label42.TabIndex = 21;
            this.label42.Text = "Request timeout";
            this.label42.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTransferOfficePassword
            // 
            this.txtTransferOfficePassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferOfficePassword.Location = new System.Drawing.Point(103, 276);
            this.txtTransferOfficePassword.Name = "txtTransferOfficePassword";
            this.txtTransferOfficePassword.Size = new System.Drawing.Size(262, 20);
            this.txtTransferOfficePassword.TabIndex = 7;
            this.txtTransferOfficePassword.Tag = "$Password";
            // 
            // txtTransferOfficeUser
            // 
            this.txtTransferOfficeUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferOfficeUser.Location = new System.Drawing.Point(103, 250);
            this.txtTransferOfficeUser.Name = "txtTransferOfficeUser";
            this.txtTransferOfficeUser.Size = new System.Drawing.Size(262, 20);
            this.txtTransferOfficeUser.TabIndex = 1;
            this.txtTransferOfficeUser.Tag = "$User";
            // 
            // lblTransferOfficePassword
            // 
            this.lblTransferOfficePassword.Location = new System.Drawing.Point(3, 273);
            this.lblTransferOfficePassword.Name = "lblTransferOfficePassword";
            this.lblTransferOfficePassword.Size = new System.Drawing.Size(94, 26);
            this.lblTransferOfficePassword.TabIndex = 6;
            this.lblTransferOfficePassword.Text = "Password";
            this.lblTransferOfficePassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTransferOfficeUser
            // 
            this.lblTransferOfficeUser.Location = new System.Drawing.Point(3, 247);
            this.lblTransferOfficeUser.Name = "lblTransferOfficeUser";
            this.lblTransferOfficeUser.Size = new System.Drawing.Size(94, 26);
            this.lblTransferOfficeUser.TabIndex = 0;
            this.lblTransferOfficeUser.Text = "User";
            this.lblTransferOfficeUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label45
            // 
            this.label45.Location = new System.Drawing.Point(3, 221);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(94, 26);
            this.label45.TabIndex = 2;
            this.label45.Text = "Root";
            this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox19
            // 
            this.textBox19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox19.Location = new System.Drawing.Point(103, 224);
            this.textBox19.Multiline = true;
            this.textBox19.Name = "textBox19";
            this.textBox19.Size = new System.Drawing.Size(262, 20);
            this.textBox19.TabIndex = 3;
            this.textBox19.Tag = "$Root";
            // 
            // sscTransferOfficeNewExt
            // 
            this.sscTransferOfficeNewExt.BackColor = System.Drawing.Color.Transparent;
            this.tlpTransferOffice.SetColumnSpan(this.sscTransferOfficeNewExt, 2);
            this.sscTransferOfficeNewExt.DisplayValue = "";
            this.sscTransferOfficeNewExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferOfficeNewExt.HasMetaData = false;
            this.sscTransferOfficeNewExt.IsAddVisible = false;
            this.sscTransferOfficeNewExt.IsDeleteVisible = false;
            this.sscTransferOfficeNewExt.Items = ((System.Collections.Specialized.StringCollection)(resources.GetObject("sscTransferOfficeNewExt.Items")));
            this.sscTransferOfficeNewExt.Location = new System.Drawing.Point(387, 247);
            this.sscTransferOfficeNewExt.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferOfficeNewExt.Name = "sscTransferOfficeNewExt";
            this.sscTransferOfficeNewExt.NewItem = "";
            this.sscTransferOfficeNewExt.ReturnValue = "";
            this.tlpTransferOffice.SetRowSpan(this.sscTransferOfficeNewExt, 5);
            this.sscTransferOfficeNewExt.Size = new System.Drawing.Size(368, 134);
            this.sscTransferOfficeNewExt.TabIndex = 16;
            this.sscTransferOfficeNewExt.Tag = "";
            this.sscTransferOfficeNewExt.TextLabel = "Extension";
            this.sscTransferOfficeNewExt.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferOfficeNewExt.TextLabelWidth = 100F;
            // 
            // panel11
            // 
            this.tlpTransferOffice.SetColumnSpan(this.panel11, 5);
            this.panel11.Controls.Add(this.btnTransferOfficeDelete);
            this.panel11.Controls.Add(this.btnTransferOfficeAdd);
            this.panel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel11.Location = new System.Drawing.Point(0, 156);
            this.panel11.Margin = new System.Windows.Forms.Padding(0);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(755, 26);
            this.panel11.TabIndex = 28;
            // 
            // btnTransferOfficeDelete
            // 
            this.btnTransferOfficeDelete.Location = new System.Drawing.Point(84, 2);
            this.btnTransferOfficeDelete.Name = "btnTransferOfficeDelete";
            this.btnTransferOfficeDelete.Size = new System.Drawing.Size(75, 23);
            this.btnTransferOfficeDelete.TabIndex = 1;
            this.btnTransferOfficeDelete.Text = "Delete";
            this.btnTransferOfficeDelete.UseVisualStyleBackColor = true;
            this.btnTransferOfficeDelete.Click += new System.EventHandler(this.btnTransferOfficeDelete_Click);
            // 
            // btnTransferOfficeAdd
            // 
            this.btnTransferOfficeAdd.Location = new System.Drawing.Point(3, 2);
            this.btnTransferOfficeAdd.Name = "btnTransferOfficeAdd";
            this.btnTransferOfficeAdd.Size = new System.Drawing.Size(75, 23);
            this.btnTransferOfficeAdd.TabIndex = 0;
            this.btnTransferOfficeAdd.Text = "Add";
            this.btnTransferOfficeAdd.UseVisualStyleBackColor = true;
            this.btnTransferOfficeAdd.Click += new System.EventHandler(this.btnTransferOfficeAdd_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::CrRecordingConfig.Properties.Resources.help;
            this.pictureBox5.Location = new System.Drawing.Point(371, 224);
            this.pictureBox5.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(16, 16);
            this.pictureBox5.TabIndex = 13;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Click += new System.EventHandler(this.InfoTransferRoot_Click);
            // 
            // label46
            // 
            this.label46.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label46.Location = new System.Drawing.Point(390, 221);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(94, 26);
            this.label46.TabIndex = 30;
            this.label46.Text = "MediaType";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox6
            // 
            this.comboBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox6.Enabled = false;
            this.comboBox6.FormattingEnabled = true;
            this.comboBox6.Location = new System.Drawing.Point(490, 224);
            this.comboBox6.Name = "comboBox6";
            this.comboBox6.Size = new System.Drawing.Size(262, 21);
            this.comboBox6.TabIndex = 31;
            this.comboBox6.Tag = "$MediaType";
            // 
            // tabContactCenter
            // 
            this.tabContactCenter.Controls.Add(this.tableLayoutPanel4);
            this.tabContactCenter.Location = new System.Drawing.Point(4, 22);
            this.tabContactCenter.Name = "tabContactCenter";
            this.tabContactCenter.Padding = new System.Windows.Forms.Padding(3);
            this.tabContactCenter.Size = new System.Drawing.Size(761, 387);
            this.tabContactCenter.TabIndex = 0;
            this.tabContactCenter.Text = "Contact center (compatibility)";
            this.tabContactCenter.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.sscTransferCcExt, 2, 5);
            this.tableLayoutPanel4.Controls.Add(this.sscTransferCcServer, 0, 5);
            this.tableLayoutPanel4.Controls.Add(this.label12, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.cboTransferCcType, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.chkTransferCcUseActiveFtp, 3, 3);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.pictureBox1, 2, 3);
            this.tableLayoutPanel4.Controls.Add(this.txtTransferCcUser, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.txtTransferCcRoot, 1, 3);
            this.tableLayoutPanel4.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.label4, 2, 2);
            this.tableLayoutPanel4.Controls.Add(this.txtTransferCcPass, 3, 2);
            this.tableLayoutPanel4.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label18, 0, 4);
            this.tableLayoutPanel4.Controls.Add(this.textBox1, 1, 4);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 7;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(755, 381);
            this.tableLayoutPanel4.TabIndex = 16;
            // 
            // sscTransferCcExt
            // 
            this.sscTransferCcExt.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel4.SetColumnSpan(this.sscTransferCcExt, 2);
            this.sscTransferCcExt.DisplayValue = "";
            this.sscTransferCcExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferCcExt.Enabled = false;
            this.sscTransferCcExt.HasMetaData = false;
            this.sscTransferCcExt.IsAddVisible = false;
            this.sscTransferCcExt.IsDeleteVisible = false;
            this.sscTransferCcExt.Items = ((System.Collections.Specialized.StringCollection)(resources.GetObject("sscTransferCcExt.Items")));
            this.sscTransferCcExt.Location = new System.Drawing.Point(377, 117);
            this.sscTransferCcExt.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferCcExt.Name = "sscTransferCcExt";
            this.sscTransferCcExt.NewItem = "";
            this.sscTransferCcExt.ReturnValue = "";
            this.sscTransferCcExt.Size = new System.Drawing.Size(378, 160);
            this.sscTransferCcExt.TabIndex = 16;
            this.sscTransferCcExt.TextLabel = "Extension";
            this.sscTransferCcExt.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferCcExt.TextLabelWidth = 100F;
            // 
            // sscTransferCcServer
            // 
            this.sscTransferCcServer.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel4.SetColumnSpan(this.sscTransferCcServer, 2);
            this.sscTransferCcServer.DisplayValue = "";
            this.sscTransferCcServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferCcServer.Enabled = false;
            this.sscTransferCcServer.HasMetaData = false;
            this.sscTransferCcServer.IsAddVisible = false;
            this.sscTransferCcServer.IsDeleteVisible = false;
            this.sscTransferCcServer.Items = ((System.Collections.Specialized.StringCollection)(resources.GetObject("sscTransferCcServer.Items")));
            this.sscTransferCcServer.Location = new System.Drawing.Point(0, 117);
            this.sscTransferCcServer.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferCcServer.Name = "sscTransferCcServer";
            this.sscTransferCcServer.NewItem = "";
            this.sscTransferCcServer.ReturnValue = "";
            this.sscTransferCcServer.Size = new System.Drawing.Size(377, 160);
            this.sscTransferCcServer.TabIndex = 14;
            this.sscTransferCcServer.TextLabel = "Server(s)";
            this.sscTransferCcServer.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferCcServer.TextLabelWidth = 100F;
            // 
            // label12
            // 
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(3, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(94, 26);
            this.label12.TabIndex = 17;
            this.label12.Text = "Type";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboTransferCcType
            // 
            this.cboTransferCcType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTransferCcType.Enabled = false;
            this.cboTransferCcType.FormattingEnabled = true;
            this.cboTransferCcType.Location = new System.Drawing.Point(103, 3);
            this.cboTransferCcType.Name = "cboTransferCcType";
            this.cboTransferCcType.Size = new System.Drawing.Size(271, 21);
            this.cboTransferCcType.TabIndex = 18;
            // 
            // chkTransferCcUseActiveFtp
            // 
            this.chkTransferCcUseActiveFtp.AutoSize = true;
            this.chkTransferCcUseActiveFtp.Enabled = false;
            this.chkTransferCcUseActiveFtp.Location = new System.Drawing.Point(480, 68);
            this.chkTransferCcUseActiveFtp.Name = "chkTransferCcUseActiveFtp";
            this.chkTransferCcUseActiveFtp.Size = new System.Drawing.Size(92, 17);
            this.chkTransferCcUseActiveFtp.TabIndex = 15;
            this.chkTransferCcUseActiveFtp.Tag = "#TransferCcUseActiveFtp";
            this.chkTransferCcUseActiveFtp.Text = "Use active ftp";
            this.chkTransferCcUseActiveFtp.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "User";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Enabled = false;
            this.pictureBox1.Image = global::CrRecordingConfig.Properties.Resources.help;
            this.pictureBox1.Location = new System.Drawing.Point(380, 68);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.TabIndex = 13;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.InfoTransferRoot_Click);
            // 
            // txtTransferCcUser
            // 
            this.txtTransferCcUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferCcUser.Enabled = false;
            this.txtTransferCcUser.Location = new System.Drawing.Point(103, 42);
            this.txtTransferCcUser.Name = "txtTransferCcUser";
            this.txtTransferCcUser.Size = new System.Drawing.Size(271, 20);
            this.txtTransferCcUser.TabIndex = 1;
            this.txtTransferCcUser.Tag = "#TransferCcUser";
            // 
            // txtTransferCcRoot
            // 
            this.txtTransferCcRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferCcRoot.Enabled = false;
            this.txtTransferCcRoot.Location = new System.Drawing.Point(103, 68);
            this.txtTransferCcRoot.Multiline = true;
            this.txtTransferCcRoot.Name = "txtTransferCcRoot";
            this.txtTransferCcRoot.Size = new System.Drawing.Size(271, 20);
            this.txtTransferCcRoot.TabIndex = 3;
            this.txtTransferCcRoot.Tag = "#TransferCcRoot";
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 26);
            this.label2.TabIndex = 2;
            this.label2.Text = "Root";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(380, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 26);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTransferCcPass
            // 
            this.txtTransferCcPass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferCcPass.Enabled = false;
            this.txtTransferCcPass.Location = new System.Drawing.Point(480, 42);
            this.txtTransferCcPass.Name = "txtTransferCcPass";
            this.txtTransferCcPass.Size = new System.Drawing.Size(272, 20);
            this.txtTransferCcPass.TabIndex = 7;
            this.txtTransferCcPass.Tag = "#TransferCcPass";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightGray;
            this.tableLayoutPanel4.SetColumnSpan(this.panel1, 4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 29);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(749, 7);
            this.panel1.TabIndex = 19;
            // 
            // label18
            // 
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(3, 91);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(94, 26);
            this.label18.TabIndex = 20;
            this.label18.Text = "Request timeout";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(103, 94);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(271, 20);
            this.textBox1.TabIndex = 21;
            this.textBox1.Tag = "#TransferCcRequestTimeOut";
            // 
            // tabOffice
            // 
            this.tabOffice.Controls.Add(this.tableLayoutPanel5);
            this.tabOffice.Location = new System.Drawing.Point(4, 22);
            this.tabOffice.Name = "tabOffice";
            this.tabOffice.Padding = new System.Windows.Forms.Padding(3);
            this.tabOffice.Size = new System.Drawing.Size(761, 387);
            this.tabOffice.TabIndex = 1;
            this.tabOffice.Text = "Office (compatibility)";
            this.tabOffice.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 4;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.sscTransferOfficeExt, 2, 5);
            this.tableLayoutPanel5.Controls.Add(this.sscTransferOfficeServer, 0, 5);
            this.tableLayoutPanel5.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.cboTransferOfficeTypeOld, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.chkTransferOfficeUseActiveFtpOld, 3, 3);
            this.tableLayoutPanel5.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.pictureBox2, 2, 3);
            this.tableLayoutPanel5.Controls.Add(this.txtTransferOfficeUserOld, 1, 2);
            this.tableLayoutPanel5.Controls.Add(this.txtTransferOfficeRoot, 1, 3);
            this.tableLayoutPanel5.Controls.Add(this.label10, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.label13, 2, 2);
            this.tableLayoutPanel5.Controls.Add(this.txtTransferOfficePassOld, 3, 2);
            this.tableLayoutPanel5.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.label22, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.textBox2, 1, 4);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 7;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(755, 381);
            this.tableLayoutPanel5.TabIndex = 17;
            // 
            // sscTransferOfficeExt
            // 
            this.sscTransferOfficeExt.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel5.SetColumnSpan(this.sscTransferOfficeExt, 2);
            this.sscTransferOfficeExt.DisplayValue = "";
            this.sscTransferOfficeExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferOfficeExt.Enabled = false;
            this.sscTransferOfficeExt.HasMetaData = false;
            this.sscTransferOfficeExt.IsAddVisible = false;
            this.sscTransferOfficeExt.IsDeleteVisible = false;
            this.sscTransferOfficeExt.Items = ((System.Collections.Specialized.StringCollection)(resources.GetObject("sscTransferOfficeExt.Items")));
            this.sscTransferOfficeExt.Location = new System.Drawing.Point(377, 117);
            this.sscTransferOfficeExt.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferOfficeExt.Name = "sscTransferOfficeExt";
            this.sscTransferOfficeExt.NewItem = "";
            this.sscTransferOfficeExt.ReturnValue = "";
            this.sscTransferOfficeExt.Size = new System.Drawing.Size(378, 160);
            this.sscTransferOfficeExt.TabIndex = 16;
            this.sscTransferOfficeExt.TextLabel = "Extension";
            this.sscTransferOfficeExt.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferOfficeExt.TextLabelWidth = 100F;
            // 
            // sscTransferOfficeServer
            // 
            this.sscTransferOfficeServer.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel5.SetColumnSpan(this.sscTransferOfficeServer, 2);
            this.sscTransferOfficeServer.DisplayValue = "";
            this.sscTransferOfficeServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sscTransferOfficeServer.Enabled = false;
            this.sscTransferOfficeServer.HasMetaData = false;
            this.sscTransferOfficeServer.IsAddVisible = false;
            this.sscTransferOfficeServer.IsDeleteVisible = false;
            this.sscTransferOfficeServer.Items = ((System.Collections.Specialized.StringCollection)(resources.GetObject("sscTransferOfficeServer.Items")));
            this.sscTransferOfficeServer.Location = new System.Drawing.Point(0, 117);
            this.sscTransferOfficeServer.Margin = new System.Windows.Forms.Padding(0);
            this.sscTransferOfficeServer.Name = "sscTransferOfficeServer";
            this.sscTransferOfficeServer.NewItem = "";
            this.sscTransferOfficeServer.ReturnValue = "";
            this.sscTransferOfficeServer.Size = new System.Drawing.Size(377, 160);
            this.sscTransferOfficeServer.TabIndex = 14;
            this.sscTransferOfficeServer.TextLabel = "Server(s)";
            this.sscTransferOfficeServer.TextLabelAlign = System.Drawing.ContentAlignment.TopRight;
            this.sscTransferOfficeServer.TextLabelWidth = 100F;
            // 
            // label8
            // 
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 26);
            this.label8.TabIndex = 17;
            this.label8.Text = "Type";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboTransferOfficeTypeOld
            // 
            this.cboTransferOfficeTypeOld.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboTransferOfficeTypeOld.Enabled = false;
            this.cboTransferOfficeTypeOld.FormattingEnabled = true;
            this.cboTransferOfficeTypeOld.Location = new System.Drawing.Point(103, 3);
            this.cboTransferOfficeTypeOld.Name = "cboTransferOfficeTypeOld";
            this.cboTransferOfficeTypeOld.Size = new System.Drawing.Size(271, 21);
            this.cboTransferOfficeTypeOld.TabIndex = 18;
            // 
            // chkTransferOfficeUseActiveFtpOld
            // 
            this.chkTransferOfficeUseActiveFtpOld.AutoSize = true;
            this.chkTransferOfficeUseActiveFtpOld.Enabled = false;
            this.chkTransferOfficeUseActiveFtpOld.Location = new System.Drawing.Point(480, 68);
            this.chkTransferOfficeUseActiveFtpOld.Name = "chkTransferOfficeUseActiveFtpOld";
            this.chkTransferOfficeUseActiveFtpOld.Size = new System.Drawing.Size(92, 17);
            this.chkTransferOfficeUseActiveFtpOld.TabIndex = 15;
            this.chkTransferOfficeUseActiveFtpOld.Tag = "#TransferOfficeUseActiveFtp";
            this.chkTransferOfficeUseActiveFtpOld.Text = "Use active ftp";
            this.chkTransferOfficeUseActiveFtpOld.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 26);
            this.label9.TabIndex = 0;
            this.label9.Text = "User";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Enabled = false;
            this.pictureBox2.Image = global::CrRecordingConfig.Properties.Resources.help;
            this.pictureBox2.Location = new System.Drawing.Point(380, 68);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(16, 16);
            this.pictureBox2.TabIndex = 13;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.InfoTransferRoot_Click);
            // 
            // txtTransferOfficeUserOld
            // 
            this.txtTransferOfficeUserOld.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferOfficeUserOld.Enabled = false;
            this.txtTransferOfficeUserOld.Location = new System.Drawing.Point(103, 42);
            this.txtTransferOfficeUserOld.Name = "txtTransferOfficeUserOld";
            this.txtTransferOfficeUserOld.Size = new System.Drawing.Size(271, 20);
            this.txtTransferOfficeUserOld.TabIndex = 1;
            this.txtTransferOfficeUserOld.Tag = "#TransferOfficeUser";
            // 
            // txtTransferOfficeRoot
            // 
            this.txtTransferOfficeRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferOfficeRoot.Enabled = false;
            this.txtTransferOfficeRoot.Location = new System.Drawing.Point(103, 68);
            this.txtTransferOfficeRoot.Multiline = true;
            this.txtTransferOfficeRoot.Name = "txtTransferOfficeRoot";
            this.txtTransferOfficeRoot.Size = new System.Drawing.Size(271, 20);
            this.txtTransferOfficeRoot.TabIndex = 3;
            this.txtTransferOfficeRoot.Tag = "#TransferOfficeRoot";
            // 
            // label10
            // 
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 65);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(94, 26);
            this.label10.TabIndex = 2;
            this.label10.Text = "Root";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(380, 39);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(94, 26);
            this.label13.TabIndex = 6;
            this.label13.Text = "Password";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTransferOfficePassOld
            // 
            this.txtTransferOfficePassOld.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTransferOfficePassOld.Enabled = false;
            this.txtTransferOfficePassOld.Location = new System.Drawing.Point(480, 42);
            this.txtTransferOfficePassOld.Name = "txtTransferOfficePassOld";
            this.txtTransferOfficePassOld.Size = new System.Drawing.Size(272, 20);
            this.txtTransferOfficePassOld.TabIndex = 7;
            this.txtTransferOfficePassOld.Tag = "#TransferOfficePass";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightGray;
            this.tableLayoutPanel5.SetColumnSpan(this.panel2, 4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 29);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(749, 7);
            this.panel2.TabIndex = 19;
            // 
            // label22
            // 
            this.label22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label22.Location = new System.Drawing.Point(3, 91);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(94, 26);
            this.label22.TabIndex = 21;
            this.label22.Text = "Request timeout";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox2
            // 
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(103, 94);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(271, 20);
            this.textBox2.TabIndex = 22;
            this.textBox2.Tag = "#TransferOfficeRequestTimeOut";
            // 
            // tabScore
            // 
            this.tabScore.Controls.Add(this.tableLayoutPanel9);
            this.tabScore.ImageIndex = 5;
            this.tabScore.Location = new System.Drawing.Point(4, 62);
            this.tabScore.Name = "tabScore";
            this.tabScore.Padding = new System.Windows.Forms.Padding(3);
            this.tabScore.Size = new System.Drawing.Size(775, 419);
            this.tabScore.TabIndex = 5;
            this.tabScore.Text = "Score/Comment";
            this.tabScore.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 1;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.groupBox13, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.groupBox14, 0, 1);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(769, 413);
            this.tableLayoutPanel9.TabIndex = 0;
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.numericUpDown1);
            this.groupBox13.Controls.Add(this.label27);
            this.groupBox13.Controls.Add(this.checkBox2);
            this.groupBox13.Controls.Add(this.checkBox1);
            this.groupBox13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox13.Location = new System.Drawing.Point(3, 3);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(763, 69);
            this.groupBox13.TabIndex = 0;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "General";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(136, 17);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(58, 20);
            this.numericUpDown1.TabIndex = 6;
            this.numericUpDown1.Tag = "#ScoreLevel";
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(16, 19);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(114, 13);
            this.label27.TabIndex = 4;
            this.label27.Text = "Number of stars (1 - 9):";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(19, 46);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(79, 17);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Tag = "#MultiScore";
            this.checkBox2.Text = "Multi Score";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Visible = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(223, 46);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(188, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Tag = "#UseCommentVersion0";
            this.checkBox1.Text = "Use Comment and score version 0";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.socScore);
            this.groupBox14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox14.Location = new System.Drawing.Point(3, 78);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(763, 332);
            this.groupBox14.TabIndex = 1;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "List";
            // 
            // socScore
            // 
            this.socScore.BackColor = System.Drawing.Color.Transparent;
            this.socScore.DisplayValue = "";
            this.socScore.Dock = System.Windows.Forms.DockStyle.Fill;
            this.socScore.IsAddVisible = false;
            this.socScore.IsDeleteVisible = false;
            this.socScore.ItemType = null;
            this.socScore.Location = new System.Drawing.Point(3, 16);
            this.socScore.MaxNumberOfItems = 20;
            this.socScore.Name = "socScore";
            this.socScore.NewItem = "";
            this.socScore.Size = new System.Drawing.Size(757, 313);
            this.socScore.TabIndex = 0;
            this.socScore.TextLabel = "Score items";
            this.socScore.TextLabelAlign = System.Drawing.ContentAlignment.TopLeft;
            this.socScore.TextLabelWidth = 100F;
            // 
            // tabUserOptions
            // 
            this.tabUserOptions.Controls.Add(this.tableLayoutPanel7);
            this.tabUserOptions.ImageIndex = 4;
            this.tabUserOptions.Location = new System.Drawing.Point(4, 62);
            this.tabUserOptions.Name = "tabUserOptions";
            this.tabUserOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabUserOptions.Size = new System.Drawing.Size(775, 419);
            this.tabUserOptions.TabIndex = 4;
            this.tabUserOptions.Text = "User options";
            this.tabUserOptions.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.groupBox6, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.groupBox11, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.groupBox16, 0, 2);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 4;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(769, 413);
            this.tableLayoutPanel7.TabIndex = 4;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.checkBox6);
            this.groupBox6.Controls.Add(this.checkBox5);
            this.groupBox6.Controls.Add(this.checkBox4);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(0, 150);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(769, 100);
            this.groupBox6.TabIndex = 0;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Search options";
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(7, 68);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(145, 17);
            this.checkBox6.TabIndex = 2;
            this.checkBox6.Tag = "#IncludeInActiveActivities";
            this.checkBox6.Text = "Include inactive activities";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(7, 44);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(155, 17);
            this.checkBox5.TabIndex = 1;
            this.checkBox5.Tag = "#IncludeInActiveCampaigns";
            this.checkBox5.Text = "Include inactive campaigns";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(7, 20);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(136, 17);
            this.checkBox4.TabIndex = 0;
            this.checkBox4.Tag = "#IncludeInActiveAgents";
            this.checkBox4.Text = "Include inactive agents";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.label53);
            this.groupBox11.Controls.Add(this.textBox22);
            this.groupBox11.Controls.Add(this.label54);
            this.groupBox11.Controls.Add(this.textBox23);
            this.groupBox11.Controls.Add(this.label50);
            this.groupBox11.Controls.Add(this.textBox20);
            this.groupBox11.Controls.Add(this.label51);
            this.groupBox11.Controls.Add(this.textBox21);
            this.groupBox11.Controls.Add(this.label52);
            this.groupBox11.Controls.Add(this.label49);
            this.groupBox11.Controls.Add(this.textBox4);
            this.groupBox11.Controls.Add(this.label48);
            this.groupBox11.Controls.Add(this.btnFileFormatHelp);
            this.groupBox11.Controls.Add(this.textBox3);
            this.groupBox11.Controls.Add(this.label47);
            this.groupBox11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox11.Location = new System.Drawing.Point(0, 0);
            this.groupBox11.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(769, 150);
            this.groupBox11.TabIndex = 1;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Save options";
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Enabled = false;
            this.label53.Location = new System.Drawing.Point(346, 89);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(35, 13);
            this.label53.TabIndex = 18;
            this.label53.Text = "Office";
            // 
            // textBox22
            // 
            this.textBox22.Enabled = false;
            this.textBox22.Location = new System.Drawing.Point(382, 86);
            this.textBox22.Name = "textBox22";
            this.textBox22.Size = new System.Drawing.Size(234, 20);
            this.textBox22.TabIndex = 17;
            this.textBox22.Tag = "#FileMultiSaveFormatOffice";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(36, 89);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(35, 13);
            this.label54.TabIndex = 16;
            this.label54.Text = "Office";
            // 
            // textBox23
            // 
            this.textBox23.Location = new System.Drawing.Point(72, 86);
            this.textBox23.Name = "textBox23";
            this.textBox23.Size = new System.Drawing.Size(234, 20);
            this.textBox23.TabIndex = 15;
            this.textBox23.Tag = "#FileSaveFormatOffice";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Enabled = false;
            this.label50.Location = new System.Drawing.Point(346, 63);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(29, 13);
            this.label50.TabIndex = 14;
            this.label50.Text = "Chat";
            // 
            // textBox20
            // 
            this.textBox20.Enabled = false;
            this.textBox20.Location = new System.Drawing.Point(382, 60);
            this.textBox20.Name = "textBox20";
            this.textBox20.Size = new System.Drawing.Size(234, 20);
            this.textBox20.TabIndex = 13;
            this.textBox20.Tag = "#FileMultiSaveFormatChat";
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(342, 37);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(34, 13);
            this.label51.TabIndex = 12;
            this.label51.Text = "Voice";
            // 
            // textBox21
            // 
            this.textBox21.Location = new System.Drawing.Point(382, 34);
            this.textBox21.Name = "textBox21";
            this.textBox21.Size = new System.Drawing.Size(234, 20);
            this.textBox21.TabIndex = 11;
            this.textBox21.Tag = "#FileMultiSaveFormatVoice";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label52.Location = new System.Drawing.Point(316, 16);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(105, 13);
            this.label52.TabIndex = 10;
            this.label52.Text = "File format multi save";
            this.label52.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(36, 63);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(29, 13);
            this.label49.TabIndex = 9;
            this.label49.Text = "Chat";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(72, 60);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(234, 20);
            this.textBox4.TabIndex = 8;
            this.textBox4.Tag = "#FileSaveFormatChat";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(32, 37);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(34, 13);
            this.label48.TabIndex = 7;
            this.label48.Text = "Voice";
            // 
            // btnFileFormatHelp
            // 
            this.btnFileFormatHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnFileFormatHelp.Location = new System.Drawing.Point(3, 112);
            this.btnFileFormatHelp.Name = "btnFileFormatHelp";
            this.btnFileFormatHelp.Size = new System.Drawing.Size(78, 23);
            this.btnFileFormatHelp.TabIndex = 6;
            this.btnFileFormatHelp.Text = "Format info";
            this.btnFileFormatHelp.UseVisualStyleBackColor = true;
            this.btnFileFormatHelp.Click += new System.EventHandler(this.InfoFileFormatVoice_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(72, 34);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(234, 20);
            this.textBox3.TabIndex = 5;
            this.textBox3.Tag = "#FileSaveFormatVoice";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.Location = new System.Drawing.Point(6, 16);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(55, 13);
            this.label47.TabIndex = 4;
            this.label47.Text = "File format";
            this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox16
            // 
            this.groupBox16.Controls.Add(this.label29);
            this.groupBox16.Controls.Add(this.textBox5);
            this.groupBox16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox16.Location = new System.Drawing.Point(0, 250);
            this.groupBox16.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Size = new System.Drawing.Size(769, 50);
            this.groupBox16.TabIndex = 2;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "General info";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(32, 19);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(97, 13);
            this.label29.TabIndex = 9;
            this.label29.Text = "Last save Location";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(135, 16);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(481, 20);
            this.textBox5.TabIndex = 8;
            this.textBox5.Tag = "#FileLastSaveLocation";
            // 
            // ucConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "ucConfig";
            this.Size = new System.Drawing.Size(783, 485);
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.tabService.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabDatabase.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.tabTransfer.ResumeLayout(false);
            this.btnAddOfficeServer.ResumeLayout(false);
            this.tabContactCenterVoice.ResumeLayout(false);
            this.tlpTransferVoice.ResumeLayout(false);
            this.tlpTransferVoice.PerformLayout();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTransferVoice)).EndInit();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.tabContactCenterChat.ResumeLayout(false);
            this.tlpTransferChat.ResumeLayout(false);
            this.tlpTransferChat.PerformLayout();
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTransferChat)).EndInit();
            this.panel8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.tabContactCenterOffice.ResumeLayout(false);
            this.tlpTransferOffice.ResumeLayout(false);
            this.tlpTransferOffice.PerformLayout();
            this.panel10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTransferOffice)).EndInit();
            this.panel11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.tabContactCenter.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabOffice.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.tabScore.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBox14.ResumeLayout(false);
            this.tabUserOptions.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox16.ResumeLayout(false);
            this.groupBox16.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imgList;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkParameterDirectCalls;
        private System.Windows.Forms.CheckBox chkParameterManuelCalls;
        private System.Windows.Forms.CheckBox chkParameterOutbound;
        private System.Windows.Forms.CheckBox chkParameterInbound;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkParameterCampaign;
        private System.Windows.Forms.CheckBox chkParameterContact;
        private System.Windows.Forms.CheckBox chkParameterDateTime;
        private System.Windows.Forms.CheckBox chkParameterSearchType;
        private System.Windows.Forms.TextBox txtGeneralDescription;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabService;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtServiceAdminRoot;
        private System.Windows.Forms.TabPage tabTransfer;
        private System.Windows.Forms.TabPage tabDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtServiceUserRoot;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtServiceDataRoot;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtServiceRelayRoot;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkGeneralScoreAvailable;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkGeneralIsOfficeVisible;
        private System.Windows.Forms.CheckBox chkGeneralIsCallCenterVisible;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btnConnectionAdmin;
        private System.Windows.Forms.TextBox txtConnectionAdministrator;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtConnectionReporting;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Button btnConnectionReporting;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cboTransferCcType;
        private System.Windows.Forms.TabControl btnAddOfficeServer;
        private System.Windows.Forms.TabPage tabContactCenter;
        private Nixxis.Windows.Controls.SelectionStringCollectionV1 sscTransferCcServer;
        private Nixxis.Windows.Controls.SelectionStringCollectionV1 sscTransferCcExt;
        private System.Windows.Forms.CheckBox chkTransferCcUseActiveFtp;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtTransferCcUser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTransferCcRoot;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTransferCcPass;
        private System.Windows.Forms.TabPage tabOffice;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Nixxis.Windows.Controls.SelectionStringCollectionV1 sscTransferOfficeExt;
        private Nixxis.Windows.Controls.SelectionStringCollectionV1 sscTransferOfficeServer;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cboTransferOfficeTypeOld;
        private System.Windows.Forms.CheckBox chkTransferOfficeUseActiveFtpOld;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.TextBox txtTransferOfficeUserOld;
        private System.Windows.Forms.TextBox txtTransferOfficeRoot;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtTransferOfficePassOld;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.CheckBox chkServiceAdminAvailable;
        private System.Windows.Forms.CheckBox chkServiceDataAvailable;
        private System.Windows.Forms.CheckBox chkServiceUserAvailable;
        private System.Windows.Forms.CheckBox chkServiceRelayAvailable;
        private System.Windows.Forms.ComboBox cboDatabaseReportingType;
        private System.Windows.Forms.ComboBox cboDatabaseAdminType;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtTimeoutAdministrator;
        private System.Windows.Forms.TextBox txtTimeoutReporting;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox chkGeneralPreLoadFileNames;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.CheckBox chkGeneralIsOptionVisible;
        private System.Windows.Forms.CheckBox chkDebugInfo;
        private System.Windows.Forms.CheckBox chkGeneralRecordingId;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.CheckBox chkGeneralDeleteMainConfig;
        private CheckBox chkGeneralContactListId;
        private Label label18;
        private TextBox textBox1;
        private Label label22;
        private TextBox textBox2;
        private CheckBox chkParameterChat;
        private TabPage tabContactCenterChat;
        private TabPage tabContactCenterVoice;
        private TabPage tabUserOptions;
        private TableLayoutPanel tableLayoutPanel7;
        private TableLayoutPanel tlpTransferVoice;
        private Nixxis.Windows.Controls.SelectionStringCollectionV1 sscTransferVoiceExt;
        private Label label26;
        private ComboBox cboTransferVoiceType;
        private CheckBox chkTransferVoiceActiveFtp;
        private Label lblTransferVoiceUser;
        private PictureBox pictureBox3;
        private TextBox txtTransferVoiceUser;
        private TextBox textBox6;
        private Label label28;
        private Label lblTransferVoicePassword;
        private TextBox txtTransferVoicePassword;
        private Label label30;
        private TextBox textBox8;
        private DataGridView gridTransferVoice;
        private Panel panel4;
        private Panel panel5;
        private Label lblTransferVoiceHost;
        private TextBox textBox9;
        private Panel panel3;
        private Button btnTransferVoiceDelete;
        private Button btnTransferVoiceAdd;
        private Button btnTransferVoiceSave;
        private DataGridViewTextBoxColumn gtv_Server;
        private DataGridViewTextBoxColumn gtv_Type;
        private DataGridViewTextBoxColumn gtv_UserName;
        private DataGridViewTextBoxColumn gtv_Root;
        private Label label32;
        private ComboBox comboBox2;
        private TabPage tabContactCenterOffice;
        private TableLayoutPanel tlpTransferChat;
        private Panel panel6;
        private Panel panel7;
        private DataGridView gridTransferChat;
        private Button btnTransferChatSave;
        private ComboBox cboTransferChatType;
        private Label label33;
        private Label lblTransferChatHost;
        private TextBox textBox10;
        private CheckBox chkTransferChatActiveFtp;
        private TextBox textBox11;
        private Label label35;
        private TextBox txtTransferChatPassword;
        private TextBox txtTransferChatUser;
        private Label lblTransferChatPassword;
        private Label lblTransferChatUser;
        private Label label38;
        private TextBox textBox14;
        private Panel panel8;
        private Button btnTransferChatDelete;
        private Button btnTransferChatAdd;
        private PictureBox pictureBox4;
        private Label label39;
        private ComboBox comboBox4;
        private TableLayoutPanel tlpTransferOffice;
        private Panel panel9;
        private Panel panel10;
        private DataGridView gridTransferOffice;
        private Button btnTransferOfficeSave;
        private ComboBox cboTransferOfficeType;
        private Label label40;
        private Label lblTransferOfficeHost;
        private TextBox textBox15;
        private CheckBox chkTransferOfficeActiveFtp;
        private TextBox textBox16;
        private Label label42;
        private TextBox txtTransferOfficePassword;
        private TextBox txtTransferOfficeUser;
        private Label lblTransferOfficePassword;
        private Label lblTransferOfficeUser;
        private Label label45;
        private TextBox textBox19;
        private Nixxis.Windows.Controls.SelectionStringCollectionV1 sscTransferOfficeNewExt;
        private Panel panel11;
        private Button btnTransferOfficeDelete;
        private Button btnTransferOfficeAdd;
        private PictureBox pictureBox5;
        private Label label46;
        private ComboBox comboBox6;
        private DataGridViewTextBoxColumn gtc_Server;
        private DataGridViewTextBoxColumn gtc_Type;
        private DataGridViewTextBoxColumn gtc_UserName;
        private DataGridViewTextBoxColumn gtc_Root;
        private DataGridViewTextBoxColumn gto_Server;
        private DataGridViewTextBoxColumn gto_Type;
        private DataGridViewTextBoxColumn gto_UserName;
        private DataGridViewTextBoxColumn gto_Root;
        private GroupBox groupBox6;
        private CheckBox checkBox6;
        private CheckBox checkBox5;
        private CheckBox checkBox4;
        private TabPage tabPage3;
        private TableLayoutPanel tableLayoutPanel8;
        private GroupBox groupBox10;
        private Label label25;
        private Label label24;
        private GroupBox groupBox11;
        private TextBox textBox3;
        private Label label47;
        private Button btnFileFormatHelp;
        private Label label49;
        private TextBox textBox4;
        private Label label48;
        private Label label50;
        private TextBox textBox20;
        private Label label51;
        private TextBox textBox21;
        private Label label52;
        private Label label53;
        private TextBox textBox22;
        private Label label54;
        private TextBox textBox23;
        private GroupBox groupBox12;
        private TextBox textBox24;
        private Label label55;
        private CheckBox checkBox9;
        private CheckBox checkBox8;
        private CheckBox checkBox7;
        private Nixxis.Windows.Controls.SelectionObjectCollection sscTransferChatExt;
        private TabPage tabScore;
        private TableLayoutPanel tableLayoutPanel9;
        private GroupBox groupBox13;
        private GroupBox groupBox14;
        private CheckBox checkBox1;
        private Label label27;
        private CheckBox checkBox2;
        private NumericUpDown numericUpDown1;
        private Nixxis.Windows.Controls.SelectionObjectCollection socScore;
        private GroupBox groupBox15;
        private CheckBox checkBox3;
        private CheckBox checkBox10;
        private GroupBox groupBox16;
        private Label label29;
        private TextBox textBox5;

    }
}
