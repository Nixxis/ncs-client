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
    public partial class DlgAddQueue : Window, INotifyPropertyChanged
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddQueue");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddQueue()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public AdminObject Created { get; internal set; }

        public string GroupKey { get; set; }

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
                if (RadioAddQueue.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                    sb.AppendFormat(Translate("Queue will be linked to {0} teams.\n"), listTeams.NixxisSelectedItems.Count);
                }
                else
                {
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
            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (RadioAddQueue.IsChecked.GetValueOrDefault())
            {
                Queue queue = core.Create<Queue>();
                queue.GroupKey = GroupKey;
                queue.Description = txtDescription.Text;
                queue.Teams.AddRange(listTeams.NixxisSelectedItems);
                core.Queues.Add(queue);
                Created = queue;
            }
            else
            {
                Prompt pr = core.Create<Prompt>();
                pr.GroupKey = GroupKey;
                pr.Description = txtPromptDescription.Text;
                pr.Language.TargetId = (string)cboPromptLan.SelectedValue;
                pr.ComputePath((WizControl.Context as AdminObject).Id, System.IO.Path.GetExtension(txtFilePath.Text));


                pr.LocalPath = txtFilePath.Text;


                if (RadioAddPromptUnder.IsChecked.GetValueOrDefault())
                {
                    pr.RelatedTo.TargetId = Prompt.Id;
                    Prompt.RelatedPrompts.Add(pr);
                }
                else
                {
                    if (WizControl.Context is Activity)
                        (WizControl.Context as Activity).Campaign.Target.Prompts.Add(pr);
                    else if (WizControl.Context is Queue)
                        (WizControl.Context as Queue).Prompts.Add(pr);
                }


                core.Prompts.Add(pr);

                Created = pr;
            }
            
            DialogResult = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
