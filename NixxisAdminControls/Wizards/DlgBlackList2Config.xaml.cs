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
    public partial class DlgBlackList2Config : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgBlackList2Config");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgBlackList2Config()
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
                if (chkDirectMarketing.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Direct marketing\n"));
                }
                else
                {
                    sb.AppendFormat(Translate("Not direct marketing\n"));
                }
                sb.AppendFormat(Translate("Reference: {0}\n"), txtReference.Text);
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
                chkDirectMarketing.IsChecked = true;

                if (nde.ChildNodes.Count > 0)
                {
                    chkDirectMarketing.IsChecked = System.Xml.XmlConvert.ToBoolean(nde.SelectSingleNode("DirectMarketing").InnerText);
                    txtReference.Text = nde.SelectSingleNode("Reference").InnerText;
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

            XmlElement elm = nde.OwnerDocument.CreateElement("DirectMarketing");
            elm.InnerText = System.Xml.XmlConvert.ToString(chkDirectMarketing.IsChecked.GetValueOrDefault());
            nde.AppendChild(elm);

            elm = nde.OwnerDocument.CreateElement("Reference");
            elm.InnerText = txtReference.Text;
            nde.AppendChild(elm);

        }

    }

    public class BlackList2Config : BaseMediaProviderConfigurator
    {
        public override bool? ShowConfigurationDialog(DependencyObject owner)
        {
            DlgBlackList2Config dlg = new DlgBlackList2Config();

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
                {
                    if(System.Xml.XmlConvert.ToBoolean(ConfigNode.SelectSingleNode("DirectMarketing").InnerText))
                        return string.Format("Belgian black list ({0})", "direct marketing");
                    else
                        return string.Format("Belgian black list ({0})", "not direct marketing");
                }

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
