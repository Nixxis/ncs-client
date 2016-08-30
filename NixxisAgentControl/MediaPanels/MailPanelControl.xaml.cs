using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for MailPanelControl.xaml
    /// </summary>
    public partial class MailPanelControl : UserControl, IMailPanel
    {
        #region Class data
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(MailPanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailPanelControl ctrl = (MailPanelControl)obj;
        }

        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfoMail), typeof(MailPanelControl), new FrameworkPropertyMetadata(ContactPropertyChanged));
        public ContactInfoMail Contact
        {
            get { return (ContactInfoMail)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailPanelControl ctrl = (MailPanelControl)obj;

            if (args.NewValue != null && args.NewValue != args.OldValue)
            {
                ctrl.MailMessage = ((ContactInfoMail)args.NewValue).Message;
            }
        }
        
        public static readonly DependencyProperty MailMessageProperty = DependencyProperty.Register("MailMessage", typeof(MailMessage), typeof(MailPanelControl), new FrameworkPropertyMetadata(MailMessagePropertyChanged));
        public MailMessage MailMessage
        {
            get { return (MailMessage)GetValue(MailMessageProperty); }
            set { SetValue(MailMessageProperty, value); }
        }
        public static void MailMessagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailPanelControl ctrl = (MailPanelControl)obj;
            ctrl.SetAttachments();
        }

        public static readonly DependencyProperty AttachmentsProperty = DependencyProperty.Register("Attachments", typeof(NixxisAttachmentCollection), typeof(MailPanelControl));
        public NixxisAttachmentCollection Attachments
        {
            get { return (NixxisAttachmentCollection)GetValue(AttachmentsProperty); }
            set { SetValue(AttachmentsProperty, value); }
        }

        public static readonly DependencyProperty CanEditReplyFromProperty = DependencyProperty.Register("CanEditReplyFrom", typeof(bool), typeof(MailPanelControl), new PropertyMetadata(true));
        public bool CanEditReplyFrom
        {
            get { return (bool)GetValue(CanEditReplyFromProperty); }
            set { SetValue(CanEditReplyFromProperty, value); }
        }

        public static readonly DependencyProperty CanEditReplyToProperty = DependencyProperty.Register("CanEditReplyTo", typeof(bool), typeof(MailPanelControl), new PropertyMetadata(true));
        public bool CanEditReplyTo
        {
            get { return (bool)GetValue(CanEditReplyToProperty); }
            set { SetValue(CanEditReplyToProperty, value); }
        }

        public static readonly DependencyProperty CanEditReplySubjectProperty = DependencyProperty.Register("CanEditReplySubject", typeof(bool), typeof(MailPanelControl), new PropertyMetadata(true));
        public bool CanEditReplySubject
        {
            get { return (bool)GetValue(CanEditReplySubjectProperty); }
            set { SetValue(CanEditReplySubjectProperty, value); }
        }

        public static readonly DependencyProperty CanAddLocalAttachmentsProperty = DependencyProperty.Register("CanAddLocalAttachments", typeof(bool), typeof(MailPanelControl), new PropertyMetadata(false));
        public bool CanAddLocalAttachments
        {
            get { return (bool)GetValue(CanAddLocalAttachmentsProperty); }
            set { SetValue(CanAddLocalAttachmentsProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(MailPanelControl), new FrameworkPropertyMetadata(false, IsReadOnlyPropertyChanged));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static void IsReadOnlyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailPanelControl ctrl = (MailPanelControl)obj;
            ctrl.MailEditableUpdate();
            //ctrl.MailMessage = ((ContactInfoMail)args.NewValue).Message;

        }
        #endregion

        #region Constructors
        public MailPanelControl()
        {
            InitializeComponent();
            InitControls();
            InitSettings();
        }

        public MailPanelControl(HttpLink link, ContactInfoMail info)
        {
            ClientLink = link;
            Contact = info;

            InitializeComponent();

            if (string.IsNullOrEmpty(Contact.Script))
            {
                ContentSplitterHor.MaximizeBottom();
            }

            InitControls();
            InitSettings();
        }
        private void InitControls()
        {
            predefCrtl.SelectedItem = null;
            predefCrtl.TextSelected += new PredefinedTextSelectHandeler(predefCrtl_TextSelected);
        }
        private void InitSettings()
        {
            if (ClientLink.ClientSettings["FreeResponseHeaders"] == null)
            {
                this.CanEditReplyFrom = false;
                this.CanEditReplyTo = false;
                this.CanEditReplySubject = false;
            }
            if (ClientLink.ClientSettings["LocalAttachments"] == null)
                this.CanAddLocalAttachments = false;
            else
                this.CanAddLocalAttachments = true;
        }
        #endregion

        #region Members Override
        public override void BeginInit()
        {
            base.BeginInit();
            this.Attachments = new NixxisAttachmentCollection();
            this.SetAttachments();
        }
        #endregion

        #region Members
        public void SetAttachments()
        {
            if (MailMessage == null || this.Attachments == null)
                return;

            foreach (AttachmentItem item in MailMessage.AttachCollection)
            {
                this.Attachments.Add(new NixxisAttachmentItem(item));
            }
        }

        public List<string> GetAttachmentIds()
        {
            List<string> attIds = new List<string>();

            foreach (NixxisAttachmentItem item in attchCrtl.ItemSource)
            {
                if (item.IsSelected)
                {
                    if (item.Attachment.LocationIsLocal)
                    {
                        ClientLink.MailAddAttachment(ClientLink.Contacts.ActiveContact, item.GetId(), item.Attachment.DescriptionAttachment, item.Attachment.Location);
                    }

                    attIds.Add(item.GetId());
                }
            }

            return attIds;
        }

        public string GetHtmlText()
        {
            return htmlEditor.GetHtmlText();
        }

        private void MailEditableUpdate()
        {
            htmlEditor.IsEditable = !IsReadOnly;
            if (IsReadOnly)
            {
                CanEditReplyFrom = false;
                CanEditReplySubject = false;
                CanEditReplyTo = false;
            }
        }

        private void predefCrtl_TextSelected(object sender, PredefinedTextItem text)
        {
            if(text != null)
                MailInsertText(text.FullText);
        }

        private void ImportAttachment(ReceivedAttachmentItem attItem)
        {
            if (string.IsNullOrEmpty(attItem.PhysicalFile))
            {
                string[] urlsplit = ClientLink.BaseUri.AbsoluteUri.Split('/');

                string[] tab = Contact.ContentLink.Split('/');
                string UrlMailData = urlsplit[0] + "//" + urlsplit[2] + "/" + tab[1] + "?action=attachmentdata&id=" + attItem.Id;

                HttpWebRequest wr = WebRequest.Create(UrlMailData) as HttpWebRequest;
                wr.Method = "GET";

                try
                {
                    HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                    using (Stream ResponseStream = response.GetResponseStream())
                    {
                        string TargetPath = System.IO.Path.GetTempPath();
                        string TargetFileName = null;
                        int CopyCount = 0;

                        while (TargetFileName == null)
                        {
                            TargetFileName = System.IO.Path.Combine(TargetPath, (CopyCount == 0) ? attItem.FileName : string.Format("{0} ({1}){2}", System.IO.Path.GetFileNameWithoutExtension(attItem.FileName), ++CopyCount, System.IO.Path.GetExtension(attItem.FileName)));

                            if (File.Exists(TargetFileName))
                            {
                                TargetFileName = null;
                                CopyCount++;
                            }
                        }

                        using (Stream TargetStream = File.Open(TargetFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                        {
                            byte[] Buffer = new byte[4096];
                            int Read;

                            while ((Read = ResponseStream.Read(Buffer, 0, Buffer.Length)) > 0)
                                TargetStream.Write(Buffer, 0, Read);

                            attItem.PhysicalFile = TargetFileName;
                        }
                    }
                }
                catch { }
            }

        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstOriginalAttachments.SelectedItem == null)
                return;

            ReceivedAttachmentItem att = lstOriginalAttachments.SelectedItem as ReceivedAttachmentItem;

            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                ImportAttachment(att);
                proc.StartInfo.FileName = att.PhysicalFile;
                proc.Start();
                proc.Close();
            }
            catch { }
        }

        public void MailInsertText(string text)
        {
            if (!string.IsNullOrEmpty(text) && !IsReadOnly)
                htmlEditor.InsertHtmlText(text);
        }
        #endregion

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            wbOriginalMsg.Navigate(HttpLink.UrlInsertSessionId(this.MailMessage.OriginalMessageUrl, ClientLink.SessionId));
        }

        private void wbOriginalMsg_Navigated(object sender, NavigationEventArgs e)
        {
        }

        private void Browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
        }

        private void MySelf_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
