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
using System.Windows.Shapes;
using Nixxis.Client.Controls;
using System.Xml;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for Pop3ProviderConfig.xaml
    /// </summary>
    public partial class Pop3ProviderConfigDlg : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("Pop3ProviderConfig");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public Pop3ProviderConfigDlg()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Server: {0}\n"), txtServer.Text);
                sb.AppendFormat(Translate("Port: {0}\n"), txtPort.Text);
                if (chkUsesSSL.IsChecked.GetValueOrDefault())
                    sb.AppendFormat(Translate("Uses SSL\n"));
                if (chkKeepOnServer.IsChecked.GetValueOrDefault())
                    sb.AppendFormat(Translate("Keep messages on server\n"));
                else
                    sb.AppendFormat(Translate("Delete messages from server\n"));
                sb.AppendFormat(Translate("User: {0}\n"), txtUser.Text);
                sb.AppendFormat(Translate("Password: {0}\n"), txtPassword.Text);
                sb.AppendFormat(Translate("Address: {0}\n"), txtAddress.Text);
                sb.AppendFormat(Translate("SMTP server: {0}\n"), txtSMTPServer.Text);
                sb.AppendFormat(Translate("SMTP port: {0}\n"), txtSMTPPort.Text);
                if (chkSMTPUsesSSL.IsChecked.GetValueOrDefault())
                    sb.AppendFormat(Translate("SMTP uses SSL\n"));
                sb.AppendFormat(Translate("SMTP login: {0}\n"), txtSMTPUser.Text);
                sb.AppendFormat(Translate("SMTP password: {0}\n"), txtSMTPPassword.Text);
                tb.Text = sb.ToString();

            }
        }


        public void Initialize(AdminCore core)
        {
        }

        public void LoadConfig(XmlNode nde)
        {
            try
            {
                if (nde.ChildNodes.Count > 0)
                {
                    txtServer.Text = nde.SelectSingleNode("Server").InnerText;
                    txtPort.Text = nde.SelectSingleNode("Port").InnerText;
                    chkUsesSSL.IsChecked = XmlConvert.ToBoolean(nde.SelectSingleNode("UseSSL").InnerText);
                    chkKeepOnServer.IsChecked = XmlConvert.ToBoolean(nde.SelectSingleNode("KeepMessages").InnerText);
                    txtUser.Text = nde.SelectSingleNode("User").InnerText;
                    txtPassword.Text = nde.SelectSingleNode("Password").InnerText;
                    txtAddress.Text = nde.SelectSingleNode("Address").InnerText;
                    txtSMTPServer.Text = nde.SelectSingleNode("SmtpServer").InnerText;
                    txtSMTPPort.Text = nde.SelectSingleNode("SmtpPort").InnerText;
                    chkSMTPUsesSSL.IsChecked = XmlConvert.ToBoolean(nde.SelectSingleNode("SmtpSSL").InnerText);
                    txtSMTPUser.Text = nde.SelectSingleNode("SmtpLogin").InnerText;
                    txtSMTPPassword.Text = nde.SelectSingleNode("SmtpPassword").InnerText;
                }
            }
            catch
            {
            }
        }

        public void SaveConfig(XmlNode nde)
        {
            while (nde.ChildNodes.Count > 0)
                nde.RemoveChild(nde.ChildNodes[0]);

            XmlElement elm = nde.OwnerDocument.CreateElement("Server");
            elm.InnerText = txtServer.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("Port");
            elm.InnerText = txtPort.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("UseSSL");
            elm.InnerText = XmlConvert.ToString(chkUsesSSL.IsChecked.GetValueOrDefault());
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("KeepMessages");
            elm.InnerText = XmlConvert.ToString(chkKeepOnServer.IsChecked.GetValueOrDefault());
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("User");
            elm.InnerText = txtUser.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("Password");
            elm.InnerText = txtPassword.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("Address");
            elm.InnerText = txtAddress.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("SmtpServer");
            elm.InnerText = txtSMTPServer.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("SmtpPort");
            elm.InnerText = txtSMTPPort.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("SmtpSSL");
            elm.InnerText = XmlConvert.ToString(chkSMTPUsesSSL.IsChecked.GetValueOrDefault());
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("SmtpLogin");
            elm.InnerText = txtSMTPUser.Text;
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("SmtpPassword");
            elm.InnerText = txtSMTPPassword.Text;
            nde.AppendChild(elm);
        }

        private void chkUsesSSL_Checked(object sender, RoutedEventArgs e)
        {
            if(chkUsesSSL.IsChecked.GetValueOrDefault())
            {
                if (txtPort.Text == "110")
                    txtPort.Text = "995";
            }
            else
            {
                if (txtPort.Text == "995")
                    txtPort.Text = "110";
            }

        }

        private void chkSMTPUsesSSL_Checked(object sender, RoutedEventArgs e)
        {
            if (chkSMTPUsesSSL.IsChecked.GetValueOrDefault())
            {
                if (txtSMTPPort.Text == "25")
                    txtSMTPPort.Text = "465";
            }
            else
            {
                if (txtSMTPPort.Text == "465")
                    txtSMTPPort.Text = "25";
            }


        }

    }

    public class Pop3ProviderConfig : BaseMediaProviderConfigurator
    {

        public override bool? ShowConfigurationDialog(DependencyObject owner)
        {
            Pop3ProviderConfigDlg dlg = new Pop3ProviderConfigDlg();
            dlg.Owner = owner as Window;

            dlg.LoadConfig(ConfigNode);

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                dlg.SaveConfig(ConfigNode);

                return true;
            }
            return false;
        }

        public override string ConfigDescription
        {
            get
            {
                if (Config == null)
                    return "Not configured";
                else
                    return string.Format("Pop3 {0}", ConfigNode.SelectSingleNode("User").InnerText);

            }
        }

        public override MediaProviderConfiguratorCapabilities Capabilities
        {
            get
            {
                return MediaProviderConfiguratorCapabilities.ShowModalDialog;
            }
        }

    }
}
