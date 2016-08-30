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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SamplePanelControl : UserControl, IMailPanel
    {
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(SamplePanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SamplePanelControl ctrl = (SamplePanelControl)obj;
        }

        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfoMail), typeof(SamplePanelControl), new FrameworkPropertyMetadata(ContactPropertyChanged));
        public ContactInfoMail Contact
        {
            get { return (ContactInfoMail)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SamplePanelControl ctrl = (SamplePanelControl)obj;

            if (args.NewValue != null && args.NewValue != args.OldValue)
            {

                if (args.NewValue != null && args.NewValue != args.OldValue)
                {
                    ctrl.MailMessage = ((ContactInfoMail)args.NewValue).Message;
                }


                if (string.IsNullOrEmpty(ctrl.Contact.Script))
                {
                    ctrl.ContentSplitterHor.MaximizeBottom();
                }

                string[] urlsplit = ctrl.ClientLink.BaseUri.AbsoluteUri.Split('/');
                string url = urlsplit[0] + "//" + urlsplit[2] + ctrl.Contact.ContentLink;
                HttpWebRequest wr;

                wr = WebRequest.Create(url + "displaytext") as HttpWebRequest;
                wr.Method = "GET";

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)wr.GetResponse())
                    {
                        using (Stream objStream = response.GetResponseStream())
                        {
                            ctrl.txtTest.Text = new StreamReader(objStream).ReadToEnd();
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SamplePanelControl), new FrameworkPropertyMetadata(false, IsReadOnlyPropertyChanged));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static void IsReadOnlyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty MailMessageProperty = DependencyProperty.Register("MailMessage", typeof(MailMessage), typeof(SamplePanelControl), new FrameworkPropertyMetadata(MailMessagePropertyChanged));
        public MailMessage MailMessage
        {
            get { return (MailMessage)GetValue(MailMessageProperty); }
            set { SetValue(MailMessageProperty, value); }
        }
        public static void MailMessagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty AttachmentsProperty = DependencyProperty.Register("Attachments", typeof(NixxisAttachmentCollection), typeof(SamplePanelControl));
        public NixxisAttachmentCollection Attachments
        {
            get { return (NixxisAttachmentCollection)GetValue(AttachmentsProperty); }
            set { SetValue(AttachmentsProperty, value); }
        }

        public static readonly DependencyProperty CanAddLocalAttachmentsProperty = DependencyProperty.Register("CanAddLocalAttachments", typeof(bool), typeof(SamplePanelControl), new PropertyMetadata(false));
        public bool CanAddLocalAttachments
        {
            get { return (bool)GetValue(CanAddLocalAttachmentsProperty); }
            set { SetValue(CanAddLocalAttachmentsProperty, value); }
        }



        public SamplePanelControl()
        {
            InitializeComponent();
            predefCrtl.SelectedItem = null;
            predefCrtl.TextSelected += new PredefinedTextSelectHandeler(predefCrtl_TextSelected);
        }

        public SamplePanelControl(HttpLink link, ContactInfoMail info): this()
        {
            ClientLink = link;
            Contact = info;
        }


        private void predefCrtl_TextSelected(object sender, PredefinedTextItem text)
        {
            txtTest.Text = text.FullText;
        }

        public List<string> GetAttachmentIds()
        {
            return new List<string>();
        }

        public string GetHtmlText()
        {
            return string.Empty;
        }

    }
}
