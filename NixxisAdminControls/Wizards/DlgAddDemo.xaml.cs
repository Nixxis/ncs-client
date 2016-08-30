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

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgAddDemo : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddDemo");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddDemo()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public Pause Created { get; internal set; }
        
        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                if (cboDestination.SelectedIndex > -1)
                    sb.AppendFormat(Translate("Inbound destination: {0}\n"), (cboDestination.SelectedItem as NumberingPlanEntry).DisplayText);
                else
                    sb.AppendFormat(Translate("Inbound destination: {0}\n"), cboDestination.Text);

                sb.AppendFormat(Translate("Outbound destination: {0}\n"), txtOutbound.Text);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = !((NumberingPlanEntry)e.Item).Activity.HasTarget;
        }
    }
}
