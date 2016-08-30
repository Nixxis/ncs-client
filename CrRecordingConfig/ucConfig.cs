using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ContactRoute.Config;

namespace ContactRoute.Recording.Config
{
    public partial class ucConfig : baseUcConfig
    {
        public ucConfig()
        {
            InitializeComponent();
            sscTransferCcServer.IsAddVisible = true;
            sscTransferCcServer.IsDeleteVisible = true;
            sscTransferOfficeServer.IsAddVisible = true;
            sscTransferOfficeServer.IsDeleteVisible = true;
            sscTransferCcExt.IsAddVisible = true;
            sscTransferCcExt.IsDeleteVisible = true;
            sscTransferOfficeExt.IsAddVisible = true;
            sscTransferOfficeExt.IsDeleteVisible = true;
            sscTransferChatExt.IsAddVisible = true;
            sscTransferChatExt.IsDeleteVisible = true;
            sscTransferVoiceExt.IsAddVisible = true;
            sscTransferVoiceExt.IsDeleteVisible = true;
            sscTransferOfficeNewExt.IsAddVisible = true;
            sscTransferOfficeNewExt.IsDeleteVisible = true;
        }

        #region Members
        public override void SetProfileData(object data)
        {
            ProfileData profile = (ProfileData)data;
            if (m_ConfigMode != ConfigMode.User)
            {
                //profile
                //
                //General
                //
                chkParameterSearchType.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.SearchType];
                chkParameterDateTime.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.DateTime];
                chkParameterContact.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.Contact];
                chkParameterCampaign.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.Campaign];
                chkParameterInbound.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.Inbound];
                chkParameterOutbound.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.Outbound];
                chkParameterManuelCalls.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.ManuelCall];
                chkParameterDirectCalls.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.DirectCall];
                chkParameterChat.Checked = profile.SearchOptionsArray[ProfileData.SearchOptionsList.ChatContacts];
                //
                //Score
                //
                socScore.SetItems(profile.ScoreList);
                //
                //Service
                //
                SetAdminService();
                SetDataService();
                //
                //Database
                //
                SetServiceTypeToBox(this.cboDatabaseAdminType, profile.AdminConnectionType);
                SetServiceTypeToBox(this.cboDatabaseReportingType, profile.ReportConnectionType);
                //
                //Transfer
                //
                SetTransferType(this.cboTransferCcType, profile.TransferCcType);
                this.sscTransferCcServer.SetItems(profile.TransferCcIp);
                this.sscTransferCcExt.SetItems(profile.TransferCcExtension);
                SetTransferType(this.cboTransferOfficeTypeOld, profile.TransferOfficeType);
                this.sscTransferOfficeServer.SetItems(profile.TransferOfficeIp);
                this.sscTransferOfficeExt.SetItems(profile.TransferOfficeExtension);
                //
                //Transfer V2
                //
                SetTransferGrid(gridTransferVoice, profile.VoiceServerList);
                SetTransferGrid(gridTransferChat, profile.ChatServerList);
                SetTransferGrid(gridTransferOffice, profile.OfficeServerList);
            }
        }

        public override object GetProfileData(object data)
        {
            ProfileData profileData = new ProfileData();
            if (data != null)
            {
                profileData = (ProfileData)data;
            }
            if (m_ConfigMode != ConfigMode.User)
            {
                //
                //General
                //
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.SearchType] = this.chkParameterSearchType.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.DateTime] = this.chkParameterDateTime.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.Contact] = this.chkParameterContact.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.Campaign] = this.chkParameterCampaign.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.Inbound] = this.chkParameterInbound.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.Outbound] = this.chkParameterOutbound.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.ManuelCall] = this.chkParameterManuelCalls.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.DirectCall] = this.chkParameterDirectCalls.Checked;
                profileData.SearchOptionsArray[ProfileData.SearchOptionsList.ChatContacts] = this.chkParameterChat.Checked;
                //
                //Score
                //
                profileData.ScoreList = socScore.ToStringArray();
                //
                //Service
                //
                profileData.AdminConnectionType = (ProfileData.ServiceTypes)((DisplayEnumItem)this.cboDatabaseAdminType.SelectedItem).Id;
                profileData.ReportConnectionType = (ProfileData.ServiceTypes)((DisplayEnumItem)this.cboDatabaseReportingType.SelectedItem).Id;

                //
                //Transfer V2
                //
                profileData.VoiceServerList = GetTransferGrid(gridTransferVoice);
                profileData.ChatServerList = GetTransferGrid(gridTransferChat);
                profileData.OfficeServerList = GetTransferGrid(gridTransferOffice);
            }
            return profileData;
        }

        public override void SetConfigMode()
        {
            if (m_ConfigMode == ConfigMode.User)
            {
                if (tabControl1.TabPages.Contains(tabGeneral)) tabControl1.TabPages.Remove(tabGeneral);
                if (tabControl1.TabPages.Contains(tabService)) tabControl1.TabPages.Remove(tabService);
                if (tabControl1.TabPages.Contains(tabDatabase)) tabControl1.TabPages.Remove(tabDatabase);
                if (tabControl1.TabPages.Contains(tabTransfer)) tabControl1.TabPages.Remove(tabTransfer);
                if (tabControl1.TabPages.Contains(tabScore)) tabControl1.TabPages.Remove(tabScore);
                if (!tabControl1.TabPages.Contains(tabUserOptions)) tabControl1.TabPages.Add(tabUserOptions);
            }
            else
            {
                if (!tabControl1.TabPages.Contains(tabGeneral)) tabControl1.TabPages.Add(tabGeneral);
                if (!tabControl1.TabPages.Contains(tabService)) tabControl1.TabPages.Add(tabService);
                if (!tabControl1.TabPages.Contains(tabDatabase)) tabControl1.TabPages.Add(tabDatabase);
                if (!tabControl1.TabPages.Contains(tabTransfer)) tabControl1.TabPages.Add(tabTransfer);
                if (!tabControl1.TabPages.Contains(tabScore)) tabControl1.TabPages.Add(tabScore);
                if (!tabControl1.TabPages.Contains(tabUserOptions)) tabControl1.TabPages.Add(tabUserOptions);
            }
        }
        #endregion

        #region ObjTools
        //
        //Transfers array function
        //
        private void SetTransferGrid(DataGridView datagridview, TransferProfileData[] data)
        {
            datagridview.Rows.Clear();

            if (data == null) return;

            for (int i = 0; i < data.Length; i++)
            {
                int rowIndex = AddItemToDataGrid(datagridview, data[i]);
            }
        }
        private int AddItemToDataGrid(DataGridView datagridview, TransferProfileData data)
        {
            int rowIndex = datagridview.Rows.Add();

            DisplayItemInDataGrid(datagridview, rowIndex, data);

            return rowIndex;
        }
        private void DisplayItemInDataGrid(DataGridView datagridview, int rowIndex, TransferProfileData data)
        {
            datagridview.Rows[rowIndex].Tag = data;

            switch (data.MediaType)
            {
                case TransferProfileData.Medias.Voice:
                    datagridview["gtv_Server", rowIndex].Value = data.Host.ToString();
                    datagridview["gtv_Type", rowIndex].Value = data.Type.ToString();
                    datagridview["gtv_UserName", rowIndex].Value = data.User.ToString();
                    datagridview["gtv_Root", rowIndex].Value = data.Root.ToString();
                    break;
                case TransferProfileData.Medias.Chat:
                    datagridview["gtc_Server", rowIndex].Value = data.Host.ToString();
                    datagridview["gtc_Type", rowIndex].Value = data.Type.ToString();
                    datagridview["gtc_UserName", rowIndex].Value = data.User.ToString();
                    datagridview["gtc_Root", rowIndex].Value = data.Root.ToString();
                    break;
                case TransferProfileData.Medias.Office:
                    datagridview["gto_Server", rowIndex].Value = data.Host.ToString();
                    datagridview["gto_Type", rowIndex].Value = data.Type.ToString();
                    datagridview["gto_UserName", rowIndex].Value = data.User.ToString();
                    datagridview["gto_Root", rowIndex].Value = data.Root.ToString();
                    break;
                default:
                    break;
            }
        }
        private TransferProfileData[] GetTransferGrid(DataGridView datagridview)
        {
            if (datagridview.Rows.Count <= 0) return new TransferProfileData[0];

            TransferProfileData[] data = new TransferProfileData[datagridview.Rows.Count];

            for (int i = 0; i < datagridview.Rows.Count; i++)
            {
                data[i] = (TransferProfileData)datagridview.Rows[i].Tag;
            }

            return data;
        }
        //
        //Transfers object function
        //
        private void DisplayTransferObjectInControl(TransferProfileData data, Control control)
        {
            ContactRoute.Config.Common.GuiHelp.SetDataToControl(data, control, "$", false);
            if (control.Name == "tlpTransferVoice")
                this.sscTransferVoiceExt.SetItems(data.Extension);
            else if (control.Name == "tlpTransferChat")
                this.sscTransferChatExt.SetItems(data.Extension);
            else if (control.Name == "tlpTransferOffice")
                this.sscTransferOfficeNewExt.SetItems(data.Extension);
        }
        private int AddNewTransferItem(DataGridView datagridview, TransferProfileData data)
        {
            int rowIndex = AddItemToDataGrid(datagridview, data);

            if (rowIndex >= 0)
            {
                datagridview.ClearSelection();
                datagridview.Rows[rowIndex].Selected = true;
                datagridview.FirstDisplayedScrollingRowIndex = rowIndex;
            }

            return rowIndex;
        }
        private void UpdateTransferItemInDataGrid(DataGridView datagridview, Control control, int rowIndex)
        {
            TransferProfileData data = new TransferProfileData();

            data = (TransferProfileData)ContactRoute.Config.Common.GuiHelp.GetDataFromControl(data, control, "$", true);
            if (control.Name == "tlpTransferVoice")
            {
                data.Extension = new string[this.sscTransferVoiceExt.Items.Count];
                this.sscTransferVoiceExt.Items.CopyTo(data.Extension, 0);
            }
            else if (control.Name == "tlpTransferChat")
            {
                data.Extension = new string[this.sscTransferChatExt.Items.Count];
                this.sscTransferChatExt.Items.CopyTo(data.Extension, 0);
            }
            else if (control.Name == "tlpTransferOffice")
            {
                data.Extension = new string[this.sscTransferOfficeExt.Items.Count];
                this.sscTransferOfficeExt.Items.CopyTo(data.Extension, 0);
            }
            DisplayItemInDataGrid(datagridview, rowIndex, data);
        }
        private void DeleteTransferItemInDataGrid(DataGridView datagridview, int rowIndex)
        {
            if(rowIndex >= datagridview.Rows.Count) return;

            datagridview.Tag = null;
            datagridview.Rows.RemoveAt(rowIndex);

            if (datagridview.Rows.Count > 0)
            {
                datagridview.ClearSelection();
                datagridview.Rows[0].Selected = true;
                datagridview.FirstDisplayedScrollingRowIndex = 0;
            }
        }
        //
        //Others
        //
        private void SetTransferType(ComboBox cbo, ProfileData.TransferTypes type)
        {
            cbo.Items.Clear();
            foreach (ProfileData.TransferTypes item in Enum.GetValues(typeof(ProfileData.TransferTypes)))
            {
                cbo.Items.Add(item);
                if (item == type)
                {
                    cbo.SelectedItem = item;
                }
            }
        }
        private void SetServiceType(ComboBox cbo, ProfileData.ServiceTypes type)
        {
            cbo.Items.Clear();
            foreach (ProfileData.ServiceTypes item in Enum.GetValues(typeof(ProfileData.ServiceTypes)))
            {
                cbo.Items.Add(item);
                if (item == type) cbo.SelectedItem = item;
            }
        }
        private void SetServiceTypeToBox(ComboBox cbo, ProfileData.ServiceTypes type)
        {
            if (cbo.DataSource == null) return;

            foreach (DisplayEnumItem item in (List<DisplayEnumItem>)cbo.DataSource)
            {
                if ((ProfileData.ServiceTypes)item.Id == type)
                {
                    cbo.SelectedItem = item;
                    break;
                }
            }
        }
        public void SetRootBox(ComboBox cbo, TextBox txt)
        {
            if (cbo.SelectedIndex == 0)
                txt.Enabled = true;
            else if (cbo.SelectedIndex == 1)
                txt.Enabled = false;
            else
                txt.Enabled = true;
        }
        public void SetRootBox(CheckBox chk, TextBox txt)
        {
            if (chk.Checked)
                txt.Enabled = true;
            else
                txt.Enabled = false;
        }
        public void SetAdminService()
        {
            int currentSelectedIndex = cboDatabaseAdminType.SelectedIndex;

            List<DisplayEnumItem> list = new List<DisplayEnumItem>();
            
            if (chkServiceAdminAvailable.Checked)
                list.Add(new DisplayEnumItem(ProfileData.ServiceTypes.AppServer.GetHashCode(), ProfileData.ServiceTypes.AppServer.ToString()));
            else
                list.Add(new DisplayEnumItem(ProfileData.ServiceTypes.AppServer.GetHashCode(), ProfileData.ServiceTypes.AppServer.ToString() + " (no service)"));

            list.Add(new DisplayEnumItem(ProfileData.ServiceTypes.Direct.GetHashCode(), ProfileData.ServiceTypes.Direct.ToString()));

            cboDatabaseAdminType.DataSource = null;
            cboDatabaseAdminType.DisplayMember = "Description";
            cboDatabaseAdminType.ValueMember = "Id";
            cboDatabaseAdminType.DataSource = list;
            cboDatabaseAdminType.SelectedIndex = currentSelectedIndex;

        }
        public void SetDataService()
        {
            int currentSelectedIndex = cboDatabaseReportingType.SelectedIndex;

            List<DisplayEnumItem> list = new List<DisplayEnumItem>();

            if (chkServiceDataAvailable.Checked)
                list.Add(new DisplayEnumItem(ProfileData.ServiceTypes.AppServer.GetHashCode(), ProfileData.ServiceTypes.AppServer.ToString()));
            else
                list.Add(new DisplayEnumItem(ProfileData.ServiceTypes.AppServer.GetHashCode(), ProfileData.ServiceTypes.AppServer.ToString() + " (no service)"));

            list.Add(new DisplayEnumItem(ProfileData.ServiceTypes.Direct.GetHashCode(), ProfileData.ServiceTypes.Direct.ToString()));

            cboDatabaseReportingType.DataSource = null;
            cboDatabaseReportingType.DisplayMember = "Description";
            cboDatabaseReportingType.ValueMember = "Id";
            cboDatabaseReportingType.DataSource = list;
            cboDatabaseReportingType.SelectedIndex = currentSelectedIndex;
        }
        #endregion

        #region Info/Help
        private void InfoTransferRoot_Click(object sender, EventArgs e)
        {
            MessageBox.Show("http:// or ftp:// will be automatically added depending on the transfer type."
                + "\nVariables to use:"
                + "\n    {0} = Server name/ip"
                + "\n    {1} = Recording id (is default used for the file name)"
                + "\n    {2} = Extension"
                + "\n"
                + "\nExample:"
                + "\n    ftp://{0}/{1}.{2}"
                + "\n    {0}/{1}.{2}"
                + "\n    {0}/recording/{1}.{2}");

        }
        private void InfoFileFormatVoice_Click(object sender, EventArgs e)
        {
            MessageBox.Show("List of variables that can be used in the filename:"
                + "\n    {0}  = DateTime"
                + "\n    {1}  = Originator"
                + "\n    {2}  = Destination"
                + "\n    {3}  = Agent first name"
                + "\n    {4}  = Agent last name"
                + "\n    {5}  = Agent account"
                + "\n    {6}  = Agent phone extension "
                + "\n    {7}  = Campaign"
                + "\n    {8}  = Activity"
                + "\n    {9}  = Contact Media Type"
                + "\n    {10} = Contact end state"
                + "\n    {11} = Contact qualification"
                + "\n    {12} = Contact qualification value positive"
                + "\n    {13} = Contact qualification value argued"
                + "\n    {14} = Contact communication duration"
                + "\n    {15} = Contact Setup duration"
                + "\n    {16} = Contact total duration"
                + "\n    {17} = Recording id (guid)"
                + "\n    {18} = Contact internal id/contact list id (guid)"
                + "\n"
                + "\nExtension will be added automatically, depending on the source extension."
                + "\nOffice contacts can use all variables."
                );
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void cboServiceAdminType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRootBox((ComboBox)sender,txtServiceAdminRoot);
            if (((ComboBox)sender).SelectedIndex == 0)
                txtConnectionAdministrator.Enabled = false;
            else
                txtConnectionAdministrator.Enabled = true;

        }

        private void cboServiceDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRootBox((ComboBox)sender, txtServiceDataRoot);
        }

        private void cboServiceUserType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRootBox((ComboBox)sender, txtServiceUserRoot);
        }

        private void cboServiceRelayType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRootBox((ComboBox)sender, txtServiceRelayRoot);
        }

        private void btnAddOfficeServer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void chkServiceAdminAvailable_CheckedChanged(object sender, EventArgs e)
        {
            SetRootBox((CheckBox)sender, txtServiceAdminRoot);
            SetAdminService();
        }

        private void chkServiceDataAvailable_CheckedChanged(object sender, EventArgs e)
        {
            SetRootBox((CheckBox)sender, txtServiceDataRoot);
            SetDataService();
        }

        private void chkServiceUserAvailable_CheckedChanged(object sender, EventArgs e)
        {
            SetRootBox((CheckBox)sender, txtServiceUserRoot);
        }

        private void chkServiceRelayAvailable_CheckedChanged(object sender, EventArgs e)
        {
            SetRootBox((CheckBox)sender, txtServiceRelayRoot);
        }

        private void cboDatabaseAdminType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDatabaseAdminType.SelectedItem == null) return;

            if ((ProfileData.ServiceTypes)((DisplayEnumItem)cboDatabaseAdminType.SelectedItem).Id == ProfileData.ServiceTypes.AppServer)
            {
                txtConnectionAdministrator.Enabled = false;
                txtTimeoutAdministrator.Enabled = false;
            }
            else
            {
                txtConnectionAdministrator.Enabled = true;
                txtTimeoutAdministrator.Enabled = false;
            }
        }

        private void gridTransfer_SelectionChanged(object sender, EventArgs e)
        {
            if (((DataGridView)sender).Tag != null)
            {
                UpdateTransferItemInDataGrid((DataGridView)sender, ((DataGridView)sender).Parent.Parent, (int)((DataGridView)sender).Tag);
            }

            if (((DataGridView)sender).SelectedRows.Count > 0)
            {
                if (((DataGridView)sender).SelectedRows[0].Tag != null)
                {
                    if (((DataGridView)sender).SelectedRows[0].Tag.GetType() == typeof(TransferProfileData))
                    {
                        DisplayTransferObjectInControl((TransferProfileData)((DataGridView)sender).SelectedRows[0].Tag, ((DataGridView)sender).Parent.Parent);
                        ((DataGridView)sender).Tag = ((DataGridView)sender).SelectedRows[0].Index;
                    }
                }
            }
        }

        #region Voice
        private void btnTransferVoiceAdd_Click(object sender, EventArgs e)
        {
            AddNewTransferItem(gridTransferVoice, new TransferProfileData(TransferProfileData.Medias.Voice));
        }
        private void btnTransferVoiceSave_Click(object sender, EventArgs e)
        {
            if (gridTransferVoice.SelectedRows.Count > 0)
                UpdateTransferItemInDataGrid(gridTransferVoice, tlpTransferVoice, gridTransferVoice.SelectedRows[0].Index);
        }
        private void btnTransferVoiceDelete_Click(object sender, EventArgs e)
        {
            if(gridTransferVoice.SelectedRows.Count > 0)
                DeleteTransferItemInDataGrid(gridTransferVoice, gridTransferVoice.SelectedRows[0].Index);
        }
        private void cboTransferVoiceType_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferVoiceType.SelectedItem == TransferProfileData.Types.AppServer)
                {
                    lblTransferVoiceHost.Text = "Relay name";
                    lblTransferVoicePassword.Visible = false;
                    txtTransferVoicePassword.Visible = false;
                    lblTransferVoiceUser.Visible = false;
                    txtTransferVoiceUser.Visible = false;
                    chkTransferVoiceActiveFtp.Visible = false;
                }
                else if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferVoiceType.SelectedItem == TransferProfileData.Types.Http)
                {
                    lblTransferVoiceHost.Text = "Host";
                    lblTransferVoicePassword.Visible = true;
                    txtTransferVoicePassword.Visible = true;
                    lblTransferVoiceUser.Visible = true;
                    txtTransferVoiceUser.Visible = true;
                    chkTransferVoiceActiveFtp.Visible = false;
                }
                else if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferVoiceType.SelectedItem == TransferProfileData.Types.StrictHttp)
                {
                    lblTransferVoiceHost.Text = "Host";
                    lblTransferVoicePassword.Visible = true;
                    txtTransferVoicePassword.Visible = true;
                    lblTransferVoiceUser.Visible = true;
                    txtTransferVoiceUser.Visible = true;
                    chkTransferVoiceActiveFtp.Visible = false;
                }
                else
                {
                    lblTransferVoiceHost.Text = "Host";
                    lblTransferVoicePassword.Visible = true;
                    txtTransferVoicePassword.Visible = true;
                    lblTransferVoiceUser.Visible = true;
                    txtTransferVoiceUser.Visible = true;
                    chkTransferVoiceActiveFtp.Visible = true;
                }
            }
            catch
            {
                
                
            }
        }
        #endregion

        #region Chat
        private void btnTransferChatAdd_Click(object sender, EventArgs e)
        {
            AddNewTransferItem(gridTransferChat, new TransferProfileData(TransferProfileData.Medias.Chat));
        }
        private void btnTransferChatSave_Click(object sender, EventArgs e)
        {
            if (gridTransferChat.SelectedRows.Count > 0)
                UpdateTransferItemInDataGrid(gridTransferChat, tlpTransferChat, gridTransferChat.SelectedRows[0].Index);
        }
        private void btnTransferChatDelete_Click(object sender, EventArgs e)
        {
            if (gridTransferChat.SelectedRows.Count > 0)
                DeleteTransferItemInDataGrid(gridTransferChat, gridTransferChat.SelectedRows[0].Index);
        }
        private void cboTransferChatType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferChatType.SelectedItem == TransferProfileData.Types.AppServer)
                {
                    lblTransferChatHost.Text = "Relay name";
                    lblTransferChatPassword.Visible = false;
                    txtTransferChatPassword.Visible = false;
                    lblTransferChatUser.Visible = false;
                    txtTransferChatUser.Visible = false;
                    chkTransferChatActiveFtp.Visible = false;
                }
                else if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferChatType.SelectedItem == TransferProfileData.Types.Http)
                {
                    lblTransferChatHost.Text = "Host";
                    lblTransferChatPassword.Visible = false;
                    txtTransferChatPassword.Visible = false;
                    lblTransferChatUser.Visible = false;
                    txtTransferChatUser.Visible = false;
                    chkTransferChatActiveFtp.Visible = false;
                }
                else if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferChatType.SelectedItem == TransferProfileData.Types.StrictHttp)
                {
                    lblTransferChatHost.Text = "Host";
                    lblTransferChatPassword.Visible = false;
                    txtTransferChatPassword.Visible = false;
                    lblTransferChatUser.Visible = false;
                    txtTransferChatUser.Visible = false;
                    chkTransferChatActiveFtp.Visible = false;
                }
                else
                {
                    lblTransferChatHost.Text = "Host";
                    lblTransferChatPassword.Visible = true;
                    txtTransferChatPassword.Visible = true;
                    lblTransferChatUser.Visible = true;
                    txtTransferChatUser.Visible = true;
                    chkTransferChatActiveFtp.Visible = true;
                }
            }
            catch
            {


            }
        }
        #endregion

        #region Office
        private void btnTransferOfficeAdd_Click(object sender, EventArgs e)
        {
            AddNewTransferItem(gridTransferOffice, new TransferProfileData(TransferProfileData.Medias.Office));
        }
        private void btnTransferOfficeSave_Click(object sender, EventArgs e)
        {
            if (gridTransferOffice.SelectedRows.Count > 0)
                UpdateTransferItemInDataGrid(gridTransferOffice, tlpTransferOffice, gridTransferOffice.SelectedRows[0].Index);
        }
        private void btnTransferOfficeDelete_Click(object sender, EventArgs e)
        {
            if (gridTransferOffice.SelectedRows.Count > 0)
                DeleteTransferItemInDataGrid(gridTransferOffice, gridTransferOffice.SelectedRows[0].Index);
        }
        private void cboTransferOfficeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferOfficeType.SelectedItem == TransferProfileData.Types.AppServer)
                {
                    lblTransferOfficeHost.Text = "Relay name";
                    lblTransferOfficePassword.Visible = false;
                    txtTransferOfficePassword.Visible = false;
                    lblTransferOfficeUser.Visible = false;
                    txtTransferOfficeUser.Visible = false;
                    chkTransferOfficeActiveFtp.Visible = false;
                }
                else if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferOfficeType.SelectedItem == TransferProfileData.Types.Http)
                {
                    lblTransferOfficeHost.Text = "Host";
                    lblTransferOfficePassword.Visible = false;
                    txtTransferOfficePassword.Visible = false;
                    lblTransferOfficeUser.Visible = false;
                    txtTransferOfficeUser.Visible = false;
                    chkTransferOfficeActiveFtp.Visible = false;
                }
                else if ((ContactRoute.Recording.Config.TransferProfileData.Types)cboTransferOfficeType.SelectedItem == TransferProfileData.Types.StrictHttp)
                {
                    lblTransferOfficeHost.Text = "Host";
                    lblTransferOfficePassword.Visible = false;
                    txtTransferOfficePassword.Visible = false;
                    lblTransferOfficeUser.Visible = false;
                    txtTransferOfficeUser.Visible = false;
                    chkTransferOfficeActiveFtp.Visible = false;
                }
                else
                {
                    lblTransferOfficeHost.Text = "Host";
                    lblTransferOfficePassword.Visible = true;
                    txtTransferOfficePassword.Visible = true;
                    lblTransferOfficeUser.Visible = true;
                    txtTransferOfficeUser.Visible = true;
                    chkTransferOfficeActiveFtp.Visible = true;
                }
            }
            catch
            {


            }
        }
        #endregion

        private void chkGeneralDeleteMainConfig_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGeneralDeleteMainConfig.Checked != true) chkGeneralDeleteMainConfig.Checked = true;
        }

        



    }
}
