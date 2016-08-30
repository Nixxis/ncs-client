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
    public partial class DlgRemoveCarrier : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgRemoveCarrier");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgRemoveCarrier()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }


        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();

                if (RadioRemoveCarrier.IsChecked.GetValueOrDefault())
                {
                    if(WizControl.Context is Carrier)
                        sb.AppendFormat(Translate("Carrier {0} will be removed with all its numbering plan entries.\n"), (WizControl.Context as Carrier).DisplayText );
                    else if(WizControl.Context is NumberingPlanEntry)
                        sb.AppendFormat(Translate("Carrier {0} will be removed with all its numbering plan entries.\n"), (WizControl.Context as NumberingPlanEntry).Carrier.Target.DisplayText);
                }
                else if (RadioRemoveCarrierFromNumPlan.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Carrier {0} will be removed with all its numbering plan entries.\n"), (WizControl.Context as NumberingPlanEntry).Carrier.Target.DisplayText);
                }
                else if (RadioRemoveNumberingPlan.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Numbering plan entry {0} will be removed.\n"), (WizControl.Context as NumberingPlanEntry).ShortDisplayText);
                }
                else if (RadioRemoveNumberingPlanRange.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Numbering plan entries between {0} and {1} will be removed{2}.\n"), txtRangeFrom.Value, txtRangeTo.Value, chkOnlyDeleteUnaffected.IsChecked.GetValueOrDefault() ? Translate(" (only unaffected entries)"): Translate(" (even affected entries)"));
                }

                tb.Text = sb.ToString();
            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (RadioRemoveCarrier.IsChecked.GetValueOrDefault())
            {
                Carrier c = null;
                if (WizControl.Context is Carrier)
                    c = WizControl.Context as Carrier;
                else
                    c = (WizControl.Context as NumberingPlanEntry).Carrier.Target;
                
                if(c!=null)
                    c.Core.Delete(c);
            }
            else if (RadioRemoveCarrierFromNumPlan.IsChecked.GetValueOrDefault())
            {
                Carrier c = (WizControl.Context as NumberingPlanEntry).Carrier.Target;
                c.Core.Delete(c);
            }
            else if (RadioRemoveNumberingPlan.IsChecked.GetValueOrDefault())
            {
                NumberingPlanEntry c = WizControl.Context as NumberingPlanEntry;
                if (c.Activity.HasTarget)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Entry \"{0}\" is affected to activity \"{1}\".\nAre you sure you want to delete this entry (and let the activity unaffected)?"), c.ShortDisplayText, c.Activity.Target.DisplayText );
                    dlg.Owner = Application.Current.MainWindow;
                    if (!dlg.ShowDialog().GetValueOrDefault())
                    {
                        DialogResult = false;
                        return;
                    }
                }
                c.Core.Delete(c);                
            }
            else if (RadioRemoveNumberingPlanRange.IsChecked.GetValueOrDefault())
            {
                Carrier c = (WizControl.Context as NumberingPlanEntry).Carrier.Target;
                for (int i = 0; i < c.NumberingPlanEntries.Count; i++)
                {
                    int temp;
                    
                    if (!c.NumberingPlanEntries[i].EntryIsRegexp && 
                        Int32.TryParse(c.NumberingPlanEntries[i].Entry, out temp) &&
                        temp <= txtRangeTo.Value &&
                        temp >= txtRangeFrom.Value)
                    {
                        if( !chkOnlyDeleteUnaffected.IsChecked.GetValueOrDefault() || !c.NumberingPlanEntries[i].Activity.HasTarget)
                        {
                        c.Core.Delete(c.NumberingPlanEntries[i]);
                        i--;
                            }
                    }
                }
            }


            DialogResult = true;
        }


    }
}
