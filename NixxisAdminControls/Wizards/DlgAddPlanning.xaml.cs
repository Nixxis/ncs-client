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
    public partial class DlgAddPlanning : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddPlanning");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddPlanning()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;


            InitializeComponent();

            udDay.Value = DateTime.Now.Day;
            udMonth.Value = DateTime.Now.Month;
            udYear.Value = DateTime.Now.Year;
        }

        public object Created { get; internal set; }

        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (RadioAddPlanning.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                    tb.Text = sb.ToString();
                }
                else
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtSdDescription.Text);
                    
                    if (udDay.Value == 0)
                    {
                        sb.AppendFormat(Translate("Reapeat every day\n"));
                    }
                    else
                    {
                        sb.AppendFormat(Translate("Occurs on day {0}\n"), udDay.Value);
                    }

                    if (udMonth.Value == 0)
                    {
                        sb.AppendFormat(Translate("Reapeat every month\n"));
                    }
                    else
                    {
                        sb.AppendFormat(Translate("Occurs on month {0}\n"), udMonth.Value);
                    }

                    if (udYear.Value == 0)
                    {
                        sb.AppendFormat(Translate("Reapeat every year\n"));
                    }
                    else
                    {
                        sb.AppendFormat(Translate("Occurs on year {0}\n"), udYear.Value);
                    }

                    if (radioClosed.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Force a new closing schedule from {0:00}:{1:00} to {2:00}:{3:00}\n"), TpFrom.SelectedHour, TpFrom.SelectedMinute, TpTo.SelectedHour, TpTo.SelectedMinute);
                    }
                    else if (radioOpened.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Force a new opening schedule from {0:00}:{1:00} to {2:00}:{3:00}\n"), TpFrom.SelectedHour, TpFrom.SelectedMinute, TpTo.SelectedHour, TpTo.SelectedMinute);
                    }

                    tb.Text = sb.ToString();
                }
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (RadioAddPlanning.IsChecked.GetValueOrDefault())
            {
                Planning plan = core.Create<Planning>();
                plan.GroupKey = GroupKey;
                plan.Description = txtDescription.Text;
                core.Plannings.Add(plan);
                Created = plan;
                DialogResult = true;
            }
            else
            {
                SpecialDay sd = core.Create<SpecialDay>();
                sd.Description = txtSdDescription.Text;
                sd.Day = (int)udDay.Value;
                sd.Month = (int)udMonth.Value;
                sd.Year = (int)udYear.Value;
                sd.Closed = radioClosed.IsChecked.GetValueOrDefault();
                sd.From = TpFrom.SelectedHour * 60 + TpFrom.SelectedMinute;
                sd.To = TpTo.SelectedHour * 60 + TpTo.SelectedMinute;
                sd.PlanningId = (WizControl.Context as Planning).Id;
                (WizControl.Context as Planning).SpecialDays.Add(sd);
                Created = sd;
                DialogResult = true;
            }

        }
    }
}
