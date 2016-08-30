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
    public partial class DlgAddLocation : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddLocation");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public DlgAddLocation()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public AdminObject Created { get; internal set; }

        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (RadioAddLoc.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtLocDescription.Text);
                }
                else
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                    switch ((NumberingCallType)CboCallType.SelectedValue)
                    {
                        case Admin.NumberingCallType.InboundGeneral:
                            sb.AppendFormat(Translate("Condition: Inbound call received from {0}{1} to destination {2}{3}{4}"), toggleSourceIsRegexp.IsChecked.GetValueOrDefault() ? "regexp " : string.Empty, txtSource.Text, toggleDestinationIsRegexp.IsChecked.GetValueOrDefault() ? "regexp " : string.Empty, txtDestination.Text, !string.IsNullOrEmpty(cboCarrier.SelectedValue as string) ? Translate( " using carrier ") + (cboCarrier.SelectedItem as Carrier).DisplayText: string.Empty);
                            break;

                        case Admin.NumberingCallType.OutboundActivity:
                            sb.AppendFormat(Translate("Condition: Outbound activity call from {0}{1} to destination {2}{3}{4}"), toggleSourceIsRegexp.IsChecked.GetValueOrDefault() ? "regexp " : string.Empty, txtSource.Text, toggleDestinationIsRegexp.IsChecked.GetValueOrDefault() ? "regexp " : string.Empty, txtDestination.Text, !string.IsNullOrEmpty(cboCarrier.SelectedValue as string) ? Translate(" using carrier ") + (cboCarrier.SelectedItem as Carrier).DisplayText : string.Empty);
                            break;

                        case Admin.NumberingCallType.OutboundGeneral:
                            sb.AppendFormat(Translate("Condition: Manual call from {0}{1} to destination {2}{3}{4}"), toggleSourceIsRegexp.IsChecked.GetValueOrDefault() ? "regexp " : string.Empty, txtSource.Text, toggleDestinationIsRegexp.IsChecked.GetValueOrDefault() ? "regexp " : string.Empty, txtDestination.Text, !string.IsNullOrEmpty(cboCarrier.SelectedValue as string) ? Translate(" using carrier ") + (cboCarrier.SelectedItem as Carrier).DisplayText : string.Empty);
                            break;
                    }

                    if (chkAllowed.IsChecked.GetValueOrDefault())
                    {
                        List<string> lst = new List<string>();

                        if (!string.IsNullOrEmpty(txtDestinationReplace.Text))
                            lst.Add(string.Format(Translate("Replace destination with {0}"), txtDestinationReplace.Text));

                        if (!string.IsNullOrEmpty(txtSourceReplace.Text))
                            lst.Add(string.Format(Translate("Replace source with {0}"), txtSourceReplace.Text));

                        if (!string.IsNullOrEmpty(cboSelectedCarrier.SelectedValue as string))
                            lst.Add(string.Format(Translate("Select carrier {0}"), (cboSelectedCarrier.SelectedItem as Carrier).DisplayText));

                        if (lst.Count == 0)
                            sb.AppendLine(Translate("Action: Allow routing of the call"));
                        else
                            sb.AppendLine(string.Concat(Translate("Action:"), string.Join(", ", lst.ToArray())));
                    }
                    else
                    {
                        sb.AppendLine(Translate("Action: Do not allow the call"));
                    }

                }
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;
            if (RadioAddLoc.IsChecked.GetValueOrDefault())
            {

                Location loc = core.Create<Location>();
                loc.Description = txtLocDescription.Text;
                core.Locations.Add(loc);

                foreach (Location l in core.Locations.Where((lo)=>(!lo.IsDummy)))
                {
                    if (l != loc)
                    {
                        LocationCost locost = core.Create<LocationCost>();
                        locost.FromLocation.TargetId = loc.Id;
                        locost.ToLocation.TargetId = l.Id;
                        locost.Cost = 0;
                        core.LocationCosts.Add(locost);

                        locost = core.Create<LocationCost>();
                        locost.ToLocation.TargetId = loc.Id;
                        locost.FromLocation.TargetId = l.Id;
                        locost.Cost = 0;
                        core.LocationCosts.Add(locost);
                    }
                }
                Created = loc;
                DialogResult = true;
            }
            else
            {
                NumberingRule nr = core.Create<NumberingRule>();
                nr.Description = txtDescription.Text;
                Location loc = WizControl.Context as Location;
                if (loc.NumberingRules.Count > 0)
                    nr.Sequence = loc.NumberingRules.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;

                nr.Allowed = chkAllowed.IsChecked.GetValueOrDefault();
                nr.Carrier.TargetId = cboCarrier.SelectedValue as string;
                nr.CarrierSelection.TargetId = cboSelectedCarrier.SelectedValue as string;
                nr.Destination = txtDestination.Text;
                nr.DestinationIsRegexp = toggleDestinationIsRegexp.IsChecked.GetValueOrDefault();
                nr.DestinationReplace = txtDestinationReplace.Text;
                nr.Source = txtSource.Text;
                nr.SourceIsRegexp = toggleSourceIsRegexp.IsChecked.GetValueOrDefault();
                nr.SourceReplace = txtSourceReplace.Text;
                nr.NumberingCallType = (NumberingCallType)CboCallType.SelectedValue;

                loc.NumberingRules.Add(nr);
                Created = nr;
                DialogResult = true;
            }
        }
    }
}
