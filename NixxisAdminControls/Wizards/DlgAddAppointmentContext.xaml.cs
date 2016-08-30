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
    public partial class DlgAddAppointmentContext : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddAppointmentContext");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddAppointmentContext()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public string GroupKey { get; set; }

        public AdminObject Created { get; internal set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (RadioAddApp.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                }
                else if (RadioAddMember.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtMemberDescription.Text);
                }
                else
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtAreaDescription.Text);
                }
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;
            if (RadioAddApp.IsChecked.GetValueOrDefault())
            {
                AppointmentsContext appCtx = core.Create<AppointmentsContext>();
                appCtx.GroupKey = GroupKey;
                appCtx.Description = txtDescription.Text;
                core.AppointmentsContexts.Add(appCtx);
                Created = appCtx;
                DialogResult = true;

            }
            else if (RadioAddMember.IsChecked.GetValueOrDefault())
            {
                AppointmentsMember area = core.Create<AppointmentsMember>();
                area.Description = txtMemberDescription.Text;
                AppointmentsContext app = (AppointmentsContext)WizControl.Context;

                app.Members.Add(area);
                Created = area;
                DialogResult = true;
            }
            else
            {
                AppointmentsArea area = core.Create<AppointmentsArea>();
                area.Description = txtAreaDescription.Text;
                AppointmentsContext app = (AppointmentsContext)WizControl.Context;

                if (app.Areas.Count > 0)
                    area.Sequence = app.Areas.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;

                app.Areas.Add(area);
                Created = area;
                DialogResult = true;
            }

        }
    }
}
