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
    public partial class DlgAddPreprocessor : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddPreprocessor");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public DlgAddPreprocessor()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public Preprocessor Created { get; internal set; }

        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;


            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (radioChat.IsChecked.GetValueOrDefault())
                    sb.AppendLine(Translate("Handled media is chat"));
                else if (radioEmail.IsChecked.GetValueOrDefault())
                    sb.AppendLine(Translate("Handled media is email"));
                else if (radioVoice.IsChecked.GetValueOrDefault())
                    sb.AppendLine(Translate("Handled media is voice"));
                else if (radioHold.IsChecked.GetValueOrDefault())
                    sb.AppendLine(Translate("Processor is a music player"));
                sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                
                sb.AppendFormat(Translate("Resource: {0}\n"), txtResource.Text);

                sb.AppendFormat(Translate("Editor URL: {0}\n"), txtEditorUrl.Text);

                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            Preprocessor preproc = core.Create<Preprocessor>();

            if (radioChat.IsChecked.GetValueOrDefault())
                preproc.MediaType = MediaType.Chat;
            else if (radioEmail.IsChecked.GetValueOrDefault())
                preproc.MediaType = MediaType.Mail;
            else if (radioVoice.IsChecked.GetValueOrDefault())
                preproc.MediaType = MediaType.Voice;
            else if (radioHold.IsChecked.GetValueOrDefault())
                preproc.MediaType = MediaType.Custom1;

            preproc.GroupKey = GroupKey;
            preproc.Description = txtDescription.Text;
            preproc.Resource = txtResource.Text;
            preproc.EditorUrl = txtEditorUrl.Text;
            core.Preprocessors.Add(preproc);
            Created = preproc;
            DialogResult = true;

        }
    }
}
