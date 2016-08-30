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
    public partial class DlgAddLanguage : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddLanguage");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public DlgAddLanguage()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public Language Created { get; internal set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                sb.AppendFormat(Translate("Language will be known by {0} agents.\n"), listAgents.NixxisSelectedItems.Count);
                sb.AppendFormat(Translate("Language will be requested by {0} activity contexts.\n"), listActivities.NixxisSelectedItems.Count);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            

            Language.SimpleLanguage sl = (Language.SimpleLanguage)txtDescription.SelectedItem;

            if(core.Hidden.Languages.Contains(sl.Isocode))
            {
                AdminObject ao = core.Hidden.Languages[sl.Isocode];
                core.Delete(ao);
                core.RemoveDeletedObject(sl.Isocode);
            }

            Language lng = core.Create<Language>(sl.Isocode);

            lng.Description = sl.Description;
            lng.GroupKey = sl.GroupKey;

            lng.Agents.AddRange(listAgents.NixxisSelectedItems);
            lng.Activities.AddRange(listActivities.NixxisSelectedItems);

            try
            {
                core.Languages.Add(lng);
            }
            catch
            {
            }


            Created = lng;
            DialogResult = true;

        }
    }
}
