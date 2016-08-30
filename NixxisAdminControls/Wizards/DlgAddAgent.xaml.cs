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
using System.ComponentModel;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgAddAgent : Window, INotifyPropertyChanged
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddAgent");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private bool m_CreateViewRestriction = false;
        private Agent m_Agent = null;

        public bool CreateViewRestriction
        {
            get
            {
                return m_CreateViewRestriction;
            }
            set
            {
                m_CreateViewRestriction = value;
                FirePropertyChanged("CreateViewRestriction");
            }
        }

        public Agent Agent
        {
            get
            {
                return m_Agent;
            }
            set
            {
                m_Agent = value;
                FirePropertyChanged("Agent");
            }
        }

        public DlgAddAgent()
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
                if (radioOneAgent.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Account: {0}\n"), txtAccount.Text);
                    sb.AppendFormat(Translate("First name: {0}\n"), txtFirstName.Text);
                    sb.AppendFormat(Translate("Last name: {0}\n"), txtLastName.Text);
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);

                    sb.AppendFormat(Translate("{0} affectations specified\n"), listTeams.NixxisSelectedItems.Count);

                    tb.Text = sb.ToString();
                }
                else if (radioCreateViewRestriction.IsChecked.GetValueOrDefault())
                {
                    if (radioRestrictionAny.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Target: any\n"));                        
                    }
                    else if (radioRestrictionCampaign.IsChecked.GetValueOrDefault())
                    {                        
                        if(chkTaregtCampChildren.IsChecked.GetValueOrDefault())
                            sb.AppendFormat(Translate("Target: campaign {0} and its related activities\n"), cboTargetCamp.Text);                        
                        else
                            sb.AppendFormat(Translate("Target: campaign {0}\n"), cboTargetCamp.Text);                        
                    }
                    else if (radioRestrictionQueue.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Target: queue {0}\n"), cboTargetQueue.Text);
                    }
                    else if (radioRestrictionActivity.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Target: activity {0}\n"), cboTargetActivity.Text);
                    }
                    else if (radioRestrictionTeam.IsChecked.GetValueOrDefault())
                    {
                        if(chkTaregtTeamChildren.IsChecked.GetValueOrDefault())
                            sb.AppendFormat(Translate("Target: team {0} and its related agents\n"), cboTargetTeam.Text);
                        else 
                            sb.AppendFormat(Translate("Target: team {0}\n"), cboTargetTeam.Text);                        
                    }
                    else if (radioRestrictionMyTeam.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Target: teams related to '{0}'\n"), Agent.DisplayText);
                    }

                    if (radioExludeData.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Data is excluded\n"));
                    }
                    else
                    {
                        sb.AppendFormat(Translate("Data is included\n"));
                    }

                    tb.Text = sb.ToString();
                }
                else
                {
                    sb.AppendFormat(Translate("Account range from {0} to {1}\n"), string.Format(txtAccountP.Text, txtRangeFrom.Value), string.Format(txtAccountP.Text, txtRangeTo.Value));
                    sb.AppendFormat(Translate("First names from '{0}' to '{1}'\n"), string.Format(txtFirstNameP.Text, txtRangeFrom.Value), string.Format(txtFirstNameP.Text, txtRangeTo.Value));
                    sb.AppendFormat(Translate("Last names from '{0}' to '{1}'\n"), string.Format(txtLastNameP.Text, txtRangeFrom.Value), string.Format(txtLastNameP.Text, txtRangeTo.Value));
                    sb.AppendFormat(Translate("Descriptions from '{0}' to '{1}'\n"), string.Format(txtDescriptionP.Text, txtRangeFrom.Value), string.Format(txtDescriptionP.Text, txtRangeTo.Value));

                    sb.AppendFormat(Translate("{0} affectations specified\n"), listTeams.NixxisSelectedItems.Count);

                    tb.Text = sb.ToString();
                }
            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (radioOneAgent.IsChecked.GetValueOrDefault())
            {
                Agent agt = core.Agents.FirstOrDefault((a) => (a.Account!=null && a.Account.Equals(txtAccount.Text) && a.State>0));
                if(agt!=null)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("There is already an agent using this account: {0}. Please, use another account."), agt.DisplayText );
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.IsInfoDialog = true;
                    dlg.ShowDialog();
                    return;
                }

                agt = core.Create<Agent>();

               
                agt.Account = txtAccount.Text;
                agt.FirstName = txtFirstName.Text;
                agt.LastName = txtLastName.Text;
                agt.Description = txtDescription.Text;
                agt.GroupKey = GroupKey;

                foreach(NixxisAdvListBoxItem t in listTeams.Items)
                {
                    if (t.IsSelected.GetValueOrDefault())
                        agt.Teams.Add(t.Item as Team);

                }

                core.Agents.Add(agt);

                Created = agt;
                DialogResult = true;
            }
            else if (radioCreateViewRestriction.IsChecked.GetValueOrDefault())
            {
                ViewRestriction vr = core.Create<ViewRestriction>();

                if (radioRestrictionAny.IsChecked.GetValueOrDefault())
                {
                    vr.TargetType = ViewRestrictionTargetType.Any;                    
                }
                else if (radioRestrictionCampaign.IsChecked.GetValueOrDefault())
                {
                    vr.TargetType = ViewRestrictionTargetType.Campaign;
                    vr.Target.TargetId = cboTargetCamp.SelectedValue as string;
                    vr.IncludeChildren = chkTaregtCampChildren.IsChecked.GetValueOrDefault();
                }
                else if (radioRestrictionQueue.IsChecked.GetValueOrDefault())
                {
                    vr.TargetType = ViewRestrictionTargetType.Queue;
                    vr.Target.TargetId = cboTargetQueue.SelectedValue as string;
                    vr.IncludeChildren = true;
                }
                else if (radioRestrictionActivity.IsChecked.GetValueOrDefault())
                {
                    Activity ac = core.GetAdminObject<Activity>(cboTargetActivity.SelectedValue as string);
                    if (ac is OutboundActivity)
                        vr.TargetType = ViewRestrictionTargetType.Outbound;
                    else if (ac.MediaType == MediaType.Chat)
                        vr.TargetType = ViewRestrictionTargetType.Chat;
                    else if (ac.MediaType == MediaType.Mail)
                        vr.TargetType = ViewRestrictionTargetType.Mail;
                    else
                        vr.TargetType = ViewRestrictionTargetType.Inbound;
                        
                    vr.Target.TargetId = ac.Id;
                    vr.IncludeChildren = true;
                }
                else if (radioRestrictionTeam.IsChecked.GetValueOrDefault())
                {
                    vr.TargetType = ViewRestrictionTargetType.Team;
                    vr.Target.TargetId = cboTargetTeam.SelectedValue as string;
                    vr.IncludeChildren = chkTaregtTeamChildren.IsChecked.GetValueOrDefault();
                }
                else if (radioRestrictionMyTeam.IsChecked.GetValueOrDefault())
                {
                    vr.TargetType = ViewRestrictionTargetType.MyTeam;
                    vr.IncludeChildren = true;
                }


                if (radioExludeData.IsChecked.GetValueOrDefault())
                {
                    vr.Allowed = false;
                }
                else
                {
                    vr.Allowed = true;

                    vr.InformationLevel = ViewRestrictionInformationLevel.None;

                    if (chkRealTime.IsChecked.GetValueOrDefault() && chkHistory.IsChecked.GetValueOrDefault() && chkProduction.IsChecked.GetValueOrDefault()
                        && chkPeriodProduction.IsChecked.GetValueOrDefault() && chkContactList.IsChecked.GetValueOrDefault() && chkPeakRealTime.IsChecked.GetValueOrDefault()
                        && chkPeakHistory.IsChecked.GetValueOrDefault() && chkPeakProduction.IsChecked.GetValueOrDefault() && chkRealTime.IsChecked.GetValueOrDefault()
                        && chkHistory.IsChecked.GetValueOrDefault() && chkProduction.IsChecked.GetValueOrDefault() && chkPeriodProduction.IsChecked.GetValueOrDefault()
                        && chkContactList.IsChecked.GetValueOrDefault() && chkPeakRealTime.IsChecked.GetValueOrDefault() && chkPeakHistory.IsChecked.GetValueOrDefault()
                        && chkPeakProduction.IsChecked.GetValueOrDefault())
                    {
                        vr.InformationLevel = ViewRestrictionInformationLevel.All;
                    }
                    else
                    {
                         if (chkRealTime.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.Realtime;
                        if (chkHistory.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.History;
                        if (chkProduction.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.Production;
                        if (chkPeriodProduction.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.PeriodProduction;
                        if (chkContactList.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.ContactListInfo;
                        if (chkPeakRealTime.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.PeakRealTime;
                        if (chkPeakHistory.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.PeakHistory;
                        if (chkPeakProduction.IsChecked.GetValueOrDefault())
                            vr.InformationLevel |= ViewRestrictionInformationLevel.PeakProduction;
                    }

                }

                if (Agent.ViewRestrictions.Count == 0)
                    vr.Precedence = 0;
                else
                    vr.Precedence = Agent.ViewRestrictions.OrderBy((a) => (a.Precedence)).Last().Precedence + 1;

                Agent.ViewRestrictions.Add(vr);

                Created = vr;

                DialogResult = true;

            }
            else
            {
                List<Agent> NewAgents = new List<Agent>();
                List<Agent> Duplicates = new List<Agent>();

                for (int i = (int)(txtRangeFrom.Value); i <= (int)(txtRangeTo.Value); i++)
                {
                    string strAccount = string.Format(txtAccountP.Text, i);
                    Agent agt = core.Agents.FirstOrDefault((a) => (a.Account!=null && a.Account.Equals(strAccount) && a.State>0));
                    if (agt != null)
                    {
                        Duplicates.Add(agt);
                    }
                    else
                    {
                        agt = core.Create<Agent>();

                        agt.Account = strAccount;
                        agt.FirstName = string.Format(txtFirstNameP.Text, i);
                        agt.LastName = string.Format(txtLastNameP.Text, i);
                        agt.Description = string.Format(txtDescriptionP.Text, i);
                        agt.GroupKey = GroupKey;
                        //agt.Teams.AddRange(listTeams.NixxisSelectedItems);
                        foreach (NixxisAdvListBoxItem t in listTeams.Items)
                        {
                            if (t.IsSelected.GetValueOrDefault())
                                agt.Teams.Add(t.Item as Team);

                        }


                        NewAgents.Add(agt);

                        if (Created == null)
                            Created = agt;
                    }
                }

                if (Duplicates.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(Translate("This operation would create duplicate agents accounts:"));
                    foreach (Agent a in Duplicates)
                    {
                        sb.AppendFormat("\t{0}\n", a.DisplayText);
                    }
                    sb.AppendFormat(Translate("These agents account will not be created.\n\nDo you want to add only agents that are not generating duplicates?\nClick \"no\" to cancel the complete batch."));

                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = sb.ToString();
                    dlg.Owner = Application.Current.MainWindow;
                    if (!dlg.ShowDialog().GetValueOrDefault())
                    {
                        foreach (Agent a in NewAgents)
                        {
                            core.Delete(a);
                        }
                        DialogResult = false;
                        return;
                    }

                }

                core.Agents.AddRange(NewAgents);

                DialogResult = true;
            }
        }

        private void txtRangeFrom_Error(object sender, ValidationErrorEventArgs e)
        {

        }

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item as Activity).State>0;
        }

        private void listTeams_Loaded(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            foreach (NixxisAdvListBoxItem nalbi in listTeams.Items)
            {
                Team t = nalbi.Item as Team;


                if (t.Agents.FirstOrDefault((at) => (at.AgentId == (Application.Current.MainWindow as IMainWindow).LoggedIn)) != null)
                {
                    nalbi.SetIsSelected(true);
                }
            }

        }
    }
}
