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
    public partial class DlgAddCarrier : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddCarrier");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddCarrier()
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
                if (RadioAddCarrier.IsChecked.GetValueOrDefault())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat(Translate("Carrier description: {0}\n"), txtDescription.Text);
                    tb.Text = sb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (RadioAddRange.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Range numbering plan: from {0} to {1}\n"), txtRangeFrom.Value, txtRangeTo.Value);
                    }
                    else
                    {
                        sb.AppendFormat(Translate("Regular expression numbering plan: {0}\n"), NumberingPlanRegexp.Text);
                    }
                    tb.Text = sb.ToString();
                }
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (RadioAddCarrier.IsChecked.GetValueOrDefault())
            {
                Carrier res = core.Create<Carrier>();
                res.GroupKey = GroupKey;
                res.Description = txtDescription.Text;
                core.Carriers.Add(res);
                Created = res;
                DialogResult = true;
            }
            else
            {
                Carrier c = WizControl.Context as Carrier;
                if (RadioAddRange.IsChecked.GetValueOrDefault())
                {
                    NumberingPlanEntry npe = null;
                    List<NumberingPlanEntry> duplicates = new List<NumberingPlanEntry>();
                    List<NumberingPlanEntry> newNumberingPlanEntries = new List<NumberingPlanEntry>();
                    List<string> serverAdded = new List<string>();

                    List<string> tentativeNumbers = new List<string>();

                    for (long i = (long)(txtRangeFrom.Value); i <= (long)(txtRangeTo.Value); i++)
                    {
                        
                        NumberingPlanEntry duplicate = c.NumberingPlanEntries.FirstOrDefault( (a) => (a.Entry.Equals(i.ToString())) );
                        if (duplicate == null)
                        {
                            tentativeNumbers.Add(i.ToString());
                        }
                        else
                        {
                            duplicates.Add(duplicate);
                        }
                    }

                    WaitScreen ws = new WaitScreen();
                    ws.Owner = Application.Current.MainWindow;

                    ws.Show();



                    IEnumerable<string> result = core.CheckNumbers(c.Id, core.GetSessionId((Application.Current.MainWindow as IMainWindow).LoggedIn), tentativeNumbers,
                                ((a, b, cloc) => { ws.Progress = a; ws.Text = b; ws.ProgressDescription = cloc; Helpers.WaitForPriority(); })
                                );

                    ws.Progress = -1;
                    ws.Text = string.Empty;
                    Helpers.WaitForPriority();
                    ws.Close();

                    foreach (string str in result)
                    {
                        if (!tentativeNumbers.Remove(str))
                            serverAdded.Add(str);

                        npe = core.Create<NumberingPlanEntry>();
                        npe.Carrier.Target = c;
                        npe.Entry = str;
                        npe.EntryIsRegexp = false;
                        newNumberingPlanEntries.Add(npe);

                    }
                    

                    if (duplicates.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(Translate("This operation would create duplicate numbering plan entries:"));
                        foreach (NumberingPlanEntry a in duplicates)
                        {
                            sb.AppendFormat("\t{0}\n", a.DisplayText);
                        }
                        sb.AppendFormat(Translate("These entries will not be created.\n\nDo you want to add only the entries that are not generating duplicates?\nClick \"no\" to cancel the complete batch."));

                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = sb.ToString();
                        dlg.Owner = Application.Current.MainWindow;
                        if (!dlg.ShowDialog().GetValueOrDefault())
                        {
                            foreach (NumberingPlanEntry a in newNumberingPlanEntries)
                            {
                                core.Delete(a);
                            }
                            DialogResult = false;
                            return;
                        }

                    }

                    if (tentativeNumbers.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(Translate("The hosting platform refuses the following numbers:"));
                        foreach (string str in tentativeNumbers)
                        {
                            sb.AppendFormat("\t{0}\n", str);
                        }
                        if (serverAdded.Count > 0)
                        {
                            sb.AppendLine(Translate("...and added the following numbers:"));
                            foreach (string str in serverAdded)
                            {
                                sb.AppendFormat("\t{0}\n", str);
                            }
                            sb.AppendFormat(Translate("Do you want to add only the entries that are allowed?\nClick \"no\" to cancel the complete batch."));                            
                        }
                        else
                            sb.AppendFormat(Translate("These entries will not be created.\n\nDo you want to add only the entries that are allowed?\nClick \"no\" to cancel the complete batch."));

                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = sb.ToString();
                        dlg.Owner = Application.Current.MainWindow;
                        if (!dlg.ShowDialog().GetValueOrDefault())
                        {
                            foreach (NumberingPlanEntry a in newNumberingPlanEntries)
                            {
                                core.Delete(a);
                            }
                            DialogResult = false;
                            return;
                        }
                    }



                    c.NumberingPlanEntries.AddRange(newNumberingPlanEntries);

                    DialogResult = true;
                }
                else
                {
                    NumberingPlanEntry duplicate = c.NumberingPlanEntries.FirstOrDefault((a) => (a.Entry.Equals(NumberingPlanRegexp.Text)));

                    if (duplicate != null)
                    {
                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = string.Format(Translate("There is already a numbering plan entry using this code: {0}. Please, use another code."), duplicate.DisplayText);
                        dlg.Owner = Application.Current.MainWindow;
                        dlg.IsInfoDialog = true;
                        dlg.ShowDialog();
                        return;
                    }

                    WaitScreen ws = new WaitScreen();
                    ws.Owner = Application.Current.MainWindow;
                    ws.Show();

                    IEnumerable<string> res = core.CheckNumbers(c.Id, core.GetSessionId((Application.Current.MainWindow as IMainWindow).LoggedIn), new string[] { NumberingPlanRegexp.Text },
                                ((a, b, cloc) => { ws.Progress = a; ws.Text = b; ws.ProgressDescription = cloc; Helpers.WaitForPriority(); })
                                );

                    ws.Progress = -1;
                    ws.Text = string.Empty;
                    Helpers.WaitForPriority();
                    ws.Close();

                    if (!res.Contains(NumberingPlanRegexp.Text))
                    {
                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = string.Format(Translate("The hosting platform refuses the numbers {0}. Please, use another code."), NumberingPlanRegexp.Text);
                        dlg.Owner = Application.Current.MainWindow;
                        dlg.IsInfoDialog = true;
                        dlg.ShowDialog();
                        return;
                    }

                    NumberingPlanEntry npe = core.Create<NumberingPlanEntry>();
                    npe.Carrier.Target = c;
                    npe.Entry = NumberingPlanRegexp.Text;
                    npe.EntryIsRegexp = true;
                    c.NumberingPlanEntries.Add(npe);
                    Created = npe;
                    DialogResult = true;
                }
            }

        }
    }
}
