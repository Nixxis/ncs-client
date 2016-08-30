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
    public partial class DlgAddAmdSettings : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddAmdSettings");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddAmdSettings()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public string GroupKey { get; set; }


        public AmdSettings Created { get; internal set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            AmdSettings res = core.Create<AmdSettings>();
            res.GroupKey = GroupKey;
            res.Description = txtDescription.Text;
            core.AmdSettings.Add(res);
            Created = res;
            DialogResult = true;

        }
    }
}
