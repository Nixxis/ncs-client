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
using System.IO;
using System.ComponentModel;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgAddGlobalPrompt : Window, INotifyPropertyChanged
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddGlobalPrompt");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public DlgAddGlobalPrompt()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public PromptLink Created { get; internal set; }

        public Prompt Prompt { get; set; }

        private bool m_PromptUnder = false;

        public bool PromptUnder
        {
            get
            {
                return m_PromptUnder;
            }
            set
            {
                m_PromptUnder = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PromptUnder"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Prompt"));
                }
            }
        }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (RadioAddPromptUnder.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Path: {0}\n"), txtFilePath.Text);
                    sb.AppendFormat(Translate("Description: {0}\n"), txtPromptDescription.Text);
                    sb.AppendFormat(Translate("Language: {0}\n"), cboPromptLan.Text);
                    sb.AppendFormat(Translate("Related to {0}\n"), Prompt.Description);
                }
                else
                {
                    sb.AppendFormat(Translate("Path: {0}\n"), txtFilePath.Text);
                    sb.AppendFormat(Translate("Description: {0}\n"), txtPromptDescription.Text);
                    sb.AppendFormat(Translate("Language: {0}\n"), cboPromptLan.Text);
                }
                tb.Text = sb.ToString();
            }

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
                txtPromptDescription.Text = dlg.SafeFileName;
                
                
                WaveInfo wi = new WaveInfo(txtFilePath.Text);

                (Helpers.FindChild<TextBlock>(lblFileInfo, (a) => (true)) as TextBlock).Text = wi.ToString();


                if (!wi.IsSupported)
                    lblFileInfo.Foreground = Brushes.Red;
                else
                    lblFileInfo.Foreground = lbl.Foreground;

                lblFileInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            Prompt pr = core.Create<Prompt>();
            pr.Description = txtPromptDescription.Text;
            pr.Language.TargetId = (string)cboPromptLan.SelectedValue;
            pr.ComputePath("global", System.IO.Path.GetExtension(txtFilePath.Text));


            pr.LocalPath = txtFilePath.Text;


            if (RadioAddPromptUnder.IsChecked.GetValueOrDefault())
            {
                pr.RelatedTo.TargetId = Prompt.Id;
                Prompt.RelatedPrompts.Add(pr);
            }
            else
            {
                Created = (PromptLink)core.GlobalPrompts.Prompts.AddLinkItem(pr);
            }

            core.Prompts.Add(pr);

            
            DialogResult = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
