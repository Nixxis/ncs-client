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
using System.IO;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgAddPrompt : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddPrompt");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddPrompt()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            
            InitializeComponent();
        }

        public Prompt Created { get; internal set; }

        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Path: {0}\n"), txtFilePath.Text);
                sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                sb.AppendFormat(Translate("Language: {0}\n"), cboPromptLan.Text);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            Prompt pr = core.Create<Prompt>();
            pr.GroupKey = GroupKey;
            pr.Description = txtDescription.Text;

            pr.ComputePath((WizControl.Context as AdminObject).Id, System.IO.Path.GetExtension(txtFilePath.Text));

            pr.Language.TargetId = (string)cboPromptLan.SelectedValue;

            pr.LocalPath = txtFilePath.Text;
            
            if (WizControl.Context is Activity)
            {
                (WizControl.Context as Activity).Campaign.Target.Prompts.Add(pr);
                (WizControl.Context as Activity).FirePropertyChanged("AllPrompts");
            }
            else if (WizControl.Context is Queue)
            {
                (WizControl.Context as Queue).Prompts.Add(pr);
                (WizControl.Context as Queue).FirePropertyChanged("AllPrompts");
            }

            core.Prompts.Add(pr);
            Created = pr;
            DialogResult = true;
        }

        private void BrowseForFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".wav"; // Default file extension
            dlg.Filter = Translate("Wave sound files|*.wav|Alaw sound files|*.alaw|All files|*.*"); // Filter files by extension
            dlg.Multiselect = false;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                txtFilePath.Text = dlg.FileName;
                txtDescription.Text = dlg.SafeFileName;

                WaveInfo wi = new WaveInfo(txtFilePath.Text);
                (Helpers.FindChild<TextBlock>( lblFileInfo, (a)=>(true)) as TextBlock).Text = wi.ToString();

                if (!wi.IsSupported)
                    lblFileInfo.Foreground = Brushes.Red;
                else
                    lblFileInfo.Foreground = lbl.Foreground;

                lblFileInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }



    }
}
