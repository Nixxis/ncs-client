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
    public partial class DlgAddPhone : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddPhone");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddPhone()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public Phone Created { get; internal set; }

        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (radioOnePhone.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Short code: {0}\n"), txtShortCode.Text);
                    sb.AppendFormat(Translate("Address: {0}\n"), txtAddress.Text);
                    sb.AppendFormat(Translate("MAC Address: {0}\n"), txtMacAddress.Text);
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);

                    if (radioRegisterOnAppserver.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Phone registers on application server\n"));
                    }
                    else if (radioRegisterOnResource.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(string.Format(Translate("Phone registers on {0}\n" ), (cboResource.SelectedItem as Resource).DisplayText));

                        if (chkExternalLine.IsChecked.GetValueOrDefault())
                        {
                            sb.AppendFormat(Translate("Phone is counted as an external line\n"));
                        }
                    }
                    else if (radioExternal.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Phone is external\n"));
                    }


                    if (chkAutoAnswer.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("Phone answers automatically\n"));
                    else
                        sb.AppendFormat(Translate("Phone does not answer automatically\n"));

                    if (chkKeepConnected.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("Phone's line stays opened\n"));
                    else
                        sb.AppendFormat(Translate("Phone's line is dropped after each call\n"));

                }
                else
                {
                    sb.AppendFormat(Translate("Short code range from {0} to {1}\n"), string.Format(txtShortCodeP.Text, (int)(txtRangeFrom.Value)), string.Format(txtShortCodeP.Text, (int)(txtRangeTo.Value)));
                    sb.AppendFormat(Translate("Addresses from '{0}' to '{1}'\n"), string.Format(txtAddressP.Text, (int)(txtRangeFrom.Value)), string.Format(txtAddressP.Text, (int)(txtRangeTo.Value)));
                    sb.AppendFormat(Translate("MAC Addresses from '{0}' to '{1}'\n"), string.Format(txtMacAddressP.Text, (int)(txtRangeFrom.Value)), string.Format(txtMacAddressP.Text, (int)(txtRangeTo.Value)));
                    sb.AppendFormat(Translate("Descriptions from '{0}' to '{1}'\n"), string.Format(txtDescriptionP.Text, (int)(txtRangeFrom.Value)), string.Format(txtDescriptionP.Text, (int)(txtRangeTo.Value)));

                    if (radioRegisterOnAppserver.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Phones register on application server\n"));
                    }
                    else if (radioRegisterOnResource.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(string.Format(Translate("Phones register on {0}\n"), (cboResource.SelectedItem as Resource).DisplayText));

                        if (chkExternalLine.IsChecked.GetValueOrDefault())
                        {
                            sb.AppendFormat(Translate("Phones are counted as external lines\n"));
                        }
                    }
                    else if (radioExternal.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Phones are external\n"));
                    }

                    if (chkAutoAnswer.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("Phones answer automatically\n"));
                    else
                        sb.AppendFormat(Translate("Phones do not answer automatically\n"));

                    if (chkKeepConnected.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("Phones lines stay opened\n"));
                    else
                        sb.AppendFormat(Translate("Phones lines are dropped after each call\n"));


                }


                tb.Text = sb.ToString();

            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {


            AdminCore core = DataContext as AdminCore;

            if (radioOnePhone.IsChecked.GetValueOrDefault())
            {
                Phone p = core.Phones.FirstOrDefault((a) => (a.ShortCode.Equals(txtShortCode.Text) && a.State>0));
                if (p != null)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("There is already a phone using this code: {0}. Please, use another code."), p.DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.IsInfoDialog = true;
                    dlg.ShowDialog();
                    return;
                }


                Phone ph = core.Create<Phone>();
                ph.GroupKey = GroupKey;
                ph.Description = txtDescription.Text;                
                ph.Address = txtAddress.Text;
                ph.MacAddress = txtMacAddress.Text;
                ph.ShortCode = txtShortCode.Text;

                ph.AutoAnswer = chkAutoAnswer.IsChecked.GetValueOrDefault();
                                
                ph.KeepConnected = chkKeepConnected.IsChecked.GetValueOrDefault();

                if (radioRegisterOnResource.IsChecked.GetValueOrDefault())
                {
                    ph.Location.TargetId = null;
                    ph.Register = false;
                    ph.ExternalLine = chkExternalLine.IsChecked.GetValueOrDefault();
                    ph.Resource.TargetId = cboResource.SelectedValue as string;
                }
                else if (radioRegisterOnAppserver.IsChecked.GetValueOrDefault())
                {
                    ph.Location.TargetId = cboLocation.SelectedValue as string;
                    ph.Register = true;
                    ph.ExternalLine = false;
                    ph.Resource.TargetId = null;
                }
                else if (radioExternal.IsChecked.GetValueOrDefault())
                {
                    ph.Location.TargetId = cboLocation.SelectedValue as string;
                    ph.Carrier.TargetId = cboCarrier.SelectedValue as string;
                    ph.Register = false;
                    ph.ExternalLine = true;
                    ph.Resource.TargetId = null;
                }


                core.Phones.Add(ph);
                Created = ph;
                DialogResult = true;

            }
            else
            {
                List<Phone> NewPhones = new List<Phone>();
                List<Phone> Duplicates = new List<Phone>();

                for (int i = (int)(txtRangeFrom.Value); i <= (int)(txtRangeTo.Value); i++)
                {
                    string strCode = string.Format(txtShortCodeP.Text, i);
                    Phone p = core.Phones.FirstOrDefault((a) => (a.ShortCode.Equals(strCode) && a.State>0));
                    if (p != null)
                    {
                        Duplicates.Add(p);
                    }
                    else
                    {
                        Phone ph = core.Create<Phone>();
                        ph.GroupKey = GroupKey;
                        ph.ShortCode = strCode;
                        ph.Address = string.Format(txtAddressP.Text, i);
                        try
                        {
                            ph.MacAddress = string.Format(txtMacAddressP.Text, i).Substring(0, 12);
                        }
                        catch
                        {
                        }
                        ph.Description = string.Format(txtDescriptionP.Text, i);

                        ph.AutoAnswer = chkAutoAnswer.IsChecked.GetValueOrDefault();
                        ph.KeepConnected = chkKeepConnected.IsChecked.GetValueOrDefault();

                        if (radioRegisterOnResource.IsChecked.GetValueOrDefault())
                        {
                            ph.Location.TargetId = null;
                            ph.Register = false;
                            ph.ExternalLine = chkExternalLine.IsChecked.GetValueOrDefault();
                            ph.Resource.TargetId = cboResource.SelectedValue as string;
                        }
                        else if (radioRegisterOnAppserver.IsChecked.GetValueOrDefault())
                        {
                            ph.Location.TargetId = cboLocation.SelectedValue as string;
                            ph.Register = true;
                            ph.ExternalLine = false;
                            ph.Resource.TargetId = null;
                        }
                        else if (radioExternal.IsChecked.GetValueOrDefault())
                        {
                            ph.Location.TargetId = cboLocation.SelectedValue as string;
                            ph.Carrier.TargetId = cboCarrier.SelectedValue as string;
                            ph.Register = false;
                            ph.ExternalLine = true;
                            ph.Resource.TargetId = null;
                        }


                        NewPhones.Add(ph);

                        if (Created == null)
                            Created = ph;
                    }
                }


                if (Duplicates.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(Translate("This operation would create duplicate phones:"));
                    foreach (Phone a in Duplicates)
                    {
                        sb.AppendFormat("\t{0}\n", a.DisplayText);
                    }
                    sb.AppendFormat(Translate("These phones will not be created.\n\nDo you want to add only phones that are not generating duplicates?\nClick \"no\" to cancel the complete batch."));

                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = sb.ToString();
                    dlg.Owner = Application.Current.MainWindow;
                    if (!dlg.ShowDialog().GetValueOrDefault())
                    {
                        foreach (Phone a in NewPhones)
                        {
                            core.Delete(a);
                        }
                        DialogResult = false;
                        return;
                    }

                }

                core.Phones.AddRange(NewPhones);

                DialogResult = true;

            }

        }

        

        protected void WizControl_WizardWindowStepChanging(object sender, RoutedEventArgs e)
        {
            if ((radioOnePhone.IsChecked.GetValueOrDefault() && (txtAddress.Text.StartsWith("sip:") || txtAddress.Text.Contains("@")))||(!radioOnePhone.IsChecked.GetValueOrDefault() && (txtAddressP.Text.StartsWith("sip:") || txtAddressP.Text.Contains("@"))))
            {
                if (radioRegisterOnResource.IsChecked.GetValueOrDefault())
                    radioExternal.IsChecked = true;

                radioRegisterOnResource.IsEnabled = false;
                
            }
            else
            {
                radioRegisterOnResource.IsEnabled = true;
            }


        }

    }

   

}
