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
using System.Xml;
using System.ComponentModel;

namespace Nixxis.Client.Admin
{
    //test
    /// <summary>
    /// Interaction logic for DlgCampaignOrActivity.xaml
    /// </summary>
    public partial class DlgAddCampaignOrActivity : Window, INotifyPropertyChanged
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddCampaignOrActivity");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private string m_PreprocessorParameters = null;

        public DlgAddCampaignOrActivity()
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

            // We do that here because doing through binding will be heavier...
            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder builder = new StringBuilder();
                StringBuilder builderWarning = new StringBuilder();
                if (RadioAddCamp.IsChecked.GetValueOrDefault())
                {

                    if (chkInbound.IsChecked.GetValueOrDefault())
                    {
                        if(cboDestination.SelectedIndex>-1)
                            builder.AppendFormat(Translate("Inbound is active on destination {0}\n"), (cboDestination.SelectedItem as NumberingPlanEntry).DisplayText);
                        else
                            builder.AppendFormat(Translate("Inbound is active on destination {0}\n"), cboDestination.Text);

                        if (!string.IsNullOrEmpty(cboPreprocessor.Text))
                            builder.AppendFormat(Translate("\tPreprocessor: {0}\n"), cboPreprocessor.Text);
                        else
                            builder.AppendFormat(Translate("\tNo preprocessor\n"));
                        if (!string.IsNullOrEmpty(cboScripts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboScripts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));

                        if (chkInAutoRecord.IsChecked == null)
                        {
                            builder.AppendFormat(Translate("\tConversations recording settings is unchanged\n"));
                        }
                        else
                        {
                            if (chkInAutoRecord.IsChecked.GetValueOrDefault())
                                builder.AppendFormat(Translate("\tConversations are automatically recorded\n"));
                            else
                                builder.AppendFormat(Translate("\tConversations are not automatically recorded\n"));
                        }

                    }
                    if (chkOutbound.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("Outbound is active\n"));
                        builder.AppendFormat(Translate("\tOriginator: {0}\n"), txtOriginator.Text);
                        builder.AppendFormat(Translate("\tDialing mode: {0}\n"), cboDialingMode.Text);

                        if (!string.IsNullOrEmpty(cboScriptOuts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboScriptOuts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));

                        if (chkOutAutoRecord.IsChecked.GetValueOrDefault())
                            builder.AppendFormat(Translate("\tConversations are automatically recorded\n"));
                        else
                            builder.AppendFormat(Translate("\tConversations are not automatically recorded\n"));
                    }
                    if (chkChatWithProviders.IsChecked.GetValueOrDefault() || chkChatWithoutProvider.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("Chat is active on destination {0}\n"), txtChatDestination.Text);
                        if (!string.IsNullOrEmpty(cboChatPreprocessor.Text))
                            builder.AppendFormat(Translate("\tPreprocessor: {0}\n"), cboChatPreprocessor.Text);
                        else
                            builder.AppendFormat(Translate("\tNo preprocessor\n"));
                        if (!string.IsNullOrEmpty(cboChatScripts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboChatScripts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));

                    }
                    if (chkEmailWithProviders.IsChecked.GetValueOrDefault() || chkEmailWithoutProvider.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("Mail is active on destination {0}\n"), txtMailDestination.Text);
                        if (!string.IsNullOrEmpty(cboMailPreprocessor.Text))
                            builder.AppendFormat(Translate("\tPreprocessor: {0}\n"), cboMailPreprocessor.Text);
                        else
                            builder.AppendFormat(Translate("\tNo preprocessor\n"));
                        if (!string.IsNullOrEmpty(cboMailScripts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboMailScripts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));

                    }
                    if (chkSearch.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("Search mode is active\n"));
                        builder.AppendFormat(Translate("\tScript: {0}\n"), cboScriptSearch.Text);

                        if (chkSearchAutoRecord.IsChecked.GetValueOrDefault())
                            builder.AppendFormat(Translate("\tConversations are automatically recorded\n"));
                        else
                            builder.AppendFormat(Translate("\tConversations are not automatically recorded\n"));


                    }

                    builder.AppendFormat(Translate("{0} associated agents\n"), listAgents.NixxisSelectedItems.Count);

                }
                else if(RadioAddAct.IsChecked.GetValueOrDefault())
                {
                    if (radioInbound.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("The new activity will be inbound\n"));
                        if(cboDestination.SelectedIndex>-1)
                            builder.AppendFormat(Translate("\tDestination: {0}\n"), (cboDestination.SelectedItem as NumberingPlanEntry).DisplayText);
                        else
                            builder.AppendFormat(Translate("\tDestination: {0}\n"), cboDestination.Text );

                        if (!string.IsNullOrEmpty(cboPreprocessor.Text))
                            builder.AppendFormat(Translate("\tPreprocessor: {0}\n"), cboPreprocessor.Text);
                        else
                            builder.AppendFormat(Translate("\tNo preprocessor\n"));
                        if (!string.IsNullOrEmpty(cboScripts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboScripts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));

                        if (chkInAutoRecord.IsChecked == true)
                        {
                            builder.AppendFormat(Translate("\tConversations recording settings is unchanged\n"));
                        }
                        else
                        {
                            if (chkInAutoRecord.IsChecked.GetValueOrDefault())
                                builder.AppendFormat(Translate("\tConversations are automatically recorded\n"));
                            else
                                builder.AppendFormat(Translate("\tConversations are not automatically recorded\n"));
                        }


                    }
                    else if (radioOutbound.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("The new activity will be outbound\n"));

                        builder.AppendFormat(Translate("\tOriginator: {0}\n"), txtOriginator.Text);
                        builder.AppendFormat(Translate("\tDialing mode: {0}\n"), cboDialingMode.Text);

                        if (!string.IsNullOrEmpty(cboScriptOuts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboScriptOuts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));

                        if (chkOutAutoRecord.IsChecked.GetValueOrDefault())
                            builder.AppendFormat(Translate("\tConversations are automatically recorded\n"));
                        else
                            builder.AppendFormat(Translate("\tConversations are not automatically recorded\n"));

                    }
                    else if (radioChatWithoutProvider.IsChecked.GetValueOrDefault() || radioChatWithProviders.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("The new activity media will be chat\n"));

                        builder.AppendFormat(Translate("\tDestination: {0}\n"), txtChatDestination.Text);
                        if (!string.IsNullOrEmpty(cboChatPreprocessor.Text))
                            builder.AppendFormat(Translate("\tPreprocessor: {0}\n"), cboChatPreprocessor.Text);
                        else
                            builder.AppendFormat(Translate("\tNo preprocessor\n"));
                        if (!string.IsNullOrEmpty(cboChatScripts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboChatScripts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));
                    }
                    else if (radioEmailWithoutProvider.IsChecked.GetValueOrDefault() || radioEmailWithProviders.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("The new activity media will be email\n"));


                        builder.AppendFormat(Translate("\tDestination: {0}\n"), txtMailDestination.Text);
                        if (!string.IsNullOrEmpty(cboMailPreprocessor.Text))
                            builder.AppendFormat(Translate("\tPreprocessor: {0}\n"), cboMailPreprocessor.Text);
                        else
                            builder.AppendFormat(Translate("\tNo preprocessor\n"));
                        if (!string.IsNullOrEmpty(cboMailScripts.Text))
                            builder.AppendFormat(Translate("\tScript: {0}\n"), cboMailScripts.Text);
                        else
                            builder.AppendFormat(Translate("\tNo script\n"));
                    }
                    else if (radioSearch.IsChecked.GetValueOrDefault())
                    {
                        builder.AppendFormat(Translate("The new activity will be a search mode activity\n"));
                        builder.AppendFormat(Translate("\tScript: {0}\n"), cboScriptSearch.Text);

                        if (chkSearchAutoRecord.IsChecked.GetValueOrDefault())
                            builder.AppendFormat(Translate("\tConversations are automatically recorded\n"));
                        else
                            builder.AppendFormat(Translate("\tConversations are not automatically recorded\n"));

                    }

                }
                else if (RadioAddQualif.IsChecked.GetValueOrDefault())
                {
                    if(((SelectionHelper)(WizControl.Context)).NewQualification.Positive>0)
                        builder.AppendFormat(Translate("\tNew qualification will be positive\n"));
                    else if(((SelectionHelper)(WizControl.Context)).NewQualification.Positive<0)
                        builder.AppendFormat(Translate("\tNew qualification will be negative\n"));
                    else
                        builder.AppendFormat(Translate("\tNew qualification will be neutral\n"));

                    if(((SelectionHelper)(WizControl.Context)).NewQualification.Argued)
                        builder.AppendFormat(Translate("\tNew qualification will be argued\n"));
                    else
                        builder.AppendFormat(Translate("\tNew qualification will be not argued\n"));

                    if (((SelectionHelper)(WizControl.Context)).Campaign.Advanced)
                    {
                        if (chkSpecifyQualificationsAffectation.IsChecked.GetValueOrDefault())
                        {
                            if (listQualificationsExclusion.NixxisSelectedItems.Count > 0)
                            {
                                builder.AppendFormat(Translate("\tNew qualification will be affected to:\n"));
                                foreach (Activity act in listQualificationsExclusion.NixxisSelectedItems)
                                    builder.AppendFormat("\t\t{0}", act.DisplayText);
                            }
                            else
                                builder.AppendFormat(Translate("\tNew qualification will not be affected to any activity\n"));
                        }
                        else
                        {
                            builder.AppendFormat(Translate("\tNew qualification will be affected to all activities\n"));
                        }
                    }
                }
                else if (RadioAddQualifUnder.IsChecked.GetValueOrDefault())
                {
                    if(((SelectionHelper)(WizControl.Context)).NewQualification.Positive>0)
                        builder.AppendFormat(Translate("\tNew qualification will be positive\n"));
                    else if(((SelectionHelper)(WizControl.Context)).NewQualification.Positive<0)
                        builder.AppendFormat(Translate("\tNew qualification will be negative\n"));
                    else
                        builder.AppendFormat(Translate("\tNew qualification will be neutral\n"));

                    if(((SelectionHelper)(WizControl.Context)).NewQualification.Argued)
                        builder.AppendFormat(Translate("\tNew qualification will be argued\n"));
                    else
                        builder.AppendFormat(Translate("\tNew qualification will be not argued\n"));

                    if (((SelectionHelper)(WizControl.Context)).Campaign.Advanced)
                    {
                        if (chkSpecifyQualificationsAffectation.IsChecked.GetValueOrDefault())
                        {
                            if (listQualificationsExclusion.NixxisSelectedItems.Count > 0)
                            {
                                builder.AppendFormat(Translate("\tNew qualification will be affected to:\n"));
                                foreach (Activity act in listQualificationsExclusion.NixxisSelectedItems)
                                    builder.AppendFormat("\t\t{0}", act.DisplayText);
                            }
                            else
                                builder.AppendFormat(Translate("\tNew qualification will not be affected to any activity\n"));
                        }
                        else
                        {
                            builder.AppendFormat(Translate("\tNew qualification will be affected to all activities\n"));
                        }
                    }


                    builder.AppendFormat(Translate("\tQualification {0} will be created as a sub-qualification of {1}\n"), ((SelectionHelper)(WizControl.Context)).NewQualification.Description, ((SelectionHelper)(WizControl.Context)).Qualification.Description );
                }
                else if (RadioAddQualifSister.IsChecked.GetValueOrDefault())
                {
                    if (((SelectionHelper)(WizControl.Context)).NewQualification.Positive > 0)
                        builder.AppendFormat(Translate("\tNew qualification will be positive\n"));
                    else if (((SelectionHelper)(WizControl.Context)).NewQualification.Positive < 0)
                        builder.AppendFormat(Translate("\tNew qualification will be negative\n"));
                    else
                        builder.AppendFormat(Translate("\tNew qualification will be neutral\n"));

                    if (((SelectionHelper)(WizControl.Context)).NewQualification.Argued)
                        builder.AppendFormat(Translate("\tNew qualification will be argued\n"));
                    else
                        builder.AppendFormat(Translate("\tNew qualification will be not argued\n"));

                    if (((SelectionHelper)(WizControl.Context)).Campaign.Advanced)
                    {
                        if (chkSpecifyQualificationsAffectation.IsChecked.GetValueOrDefault())
                        {
                            if (listQualificationsExclusion.NixxisSelectedItems.Count > 0)
                            {
                                builder.AppendFormat(Translate("\tNew qualification will be affected to:\n"));
                                foreach (Activity act in listQualificationsExclusion.NixxisSelectedItems)
                                    builder.AppendFormat("\t\t{0}", act.DisplayText);
                            }
                            else
                                builder.AppendFormat(Translate("\tNew qualification will not be affected to any activity\n"));
                        }
                        else
                        {
                            builder.AppendFormat(Translate("\tNew qualification will be affected to all activities\n"));
                        }
                    }


                    builder.AppendFormat(Translate("\tQualification {0} will be created as a sister of {1}\n"), ((SelectionHelper)(WizControl.Context)).NewQualification.Description, ((SelectionHelper)(WizControl.Context)).Qualification.Description);
                }
                else if (RadioAddField.IsChecked.GetValueOrDefault())
                {

                    builder.AppendFormat(Translate("Field {0} will be added to the data structure\n"), ((SelectionHelper)(WizControl.Context)).NewField.Name);
                    builder.AppendFormat(Translate("\tInitial name: {0} \n"), ((SelectionHelper)(WizControl.Context)).NewField.InitialName);
                    builder.AppendFormat(Translate("\tDatabase type: {0} \n"), new DBTypeHelper().Single((dt) => (dt.EnumValue == ((SelectionHelper)(WizControl.Context)).NewField.DBType)).Description);
                    builder.AppendFormat(Translate("\tMeaning: {0} \n"), new UserFieldMeaningHelper().Single( (fm) => (fm.EnumValue == ((SelectionHelper)(WizControl.Context)).NewField.FieldMeaning)).Description) ;
                    if (((SelectionHelper)(WizControl.Context)).NewField.IsUniqueConstraint)
                    {
                        builderWarning.AppendFormat(Translate("Beware that unique constraint on field {0} will be ignored if incompatible with field content.\n"), ((SelectionHelper)(WizControl.Context)).NewField.Name);
                        builder.AppendFormat(Translate("\tDefines a unique constraint\n"));
                    }
                    if (((SelectionHelper)(WizControl.Context)).NewField.IsIndexed)
                    {
                        builderWarning.AppendFormat(Translate("Beware that index requested on field {0} can be refused by database server depending on field type.\n"), ((SelectionHelper)(WizControl.Context)).NewField.Name);
                        builder.AppendFormat(Translate("\tIndexed\n"));
                    }
                    
                }
                else if (RadioAddFilter.IsChecked.GetValueOrDefault())
                {
                    builder.AppendFormat(Translate("A new filter part will be added to {0}\n"), ((SelectionHelper)(WizControl.Context)).Outbound.DisplayText);
                    Operator op = Operator.IsNotNull;

                    if (cboOperatorAnsiString.Visibility == System.Windows.Visibility.Visible)
                        op = (Operator)cboOperatorAnsiString.SelectedValue;
                    if (cboOperatorString.Visibility == System.Windows.Visibility.Visible)
                        op = (Operator)cboOperatorString.SelectedValue;
                    if (cboOperatorInt.Visibility == System.Windows.Visibility.Visible)
                        op = (Operator)cboOperatorInt.SelectedValue;
                    if (cboOperatorSingle.Visibility == System.Windows.Visibility.Visible)
                        op = (Operator)cboOperatorSingle.SelectedValue;
                    if (cboOperatorBoolean.Visibility == System.Windows.Visibility.Visible)
                        op = (Operator)cboOperatorBoolean.SelectedValue;
                    if (cboOperatorDatetime.Visibility == System.Windows.Visibility.Visible)
                        op = (Operator)cboOperatorDatetime.SelectedValue;

                    string strOperand = null;

                    if(cboAnsiStrOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboAnsiStrOperand.SelectedValue != null)
                        {
                            strOperand = ((Field)cboAnsiStrOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboAnsiStrOperand.Text;
                        }

                    if (cboStrOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboStrOperand.SelectedValue != null)
                        {
                            strOperand = ((Field)cboStrOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboStrOperand.Text;
                        }


                    if (cboStrAgentsOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboStrAgentsOperand.SelectedValue != null)
                        {
                            strOperand = ((AdminObject)cboStrAgentsOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboStrOperand.Text;
                        }


                    if (cboStrActivitiessOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboStrActivitiessOperand.SelectedValue != null)
                        {
                            strOperand = ((AdminObject)cboStrActivitiessOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboStrOperand.Text;
                        }


                    if (cboStrQualificationsOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboStrQualificationsOperand.SelectedValue != null)
                        {
                            strOperand = ((AdminObject)cboStrQualificationsOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboStrOperand.Text;
                        }


                    if (cboStrAreasOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboStrAreasOperand.SelectedValue != null)
                        {
                            strOperand = ((AdminObject)cboStrAreasOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboStrOperand.Text;
                        }
                    if (cboIntOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboIntOperand.SelectedValue != null)
                        {
                            strOperand = ((Field)cboIntOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboIntOperand.Text;
                        }
                    if (cboSingleOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboSingleOperand.SelectedValue != null)
                        {
                            strOperand = ((Field)cboSingleOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboSingleOperand.Text;
                        }
                    if (cboBoolOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboBoolOperand.SelectedValue != null)
                        {
                            strOperand = ((Field)cboBoolOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboBoolOperand.Text;
                        }
                    if (cboDateTimeOperand.Visibility == System.Windows.Visibility.Visible)
                        if (cboDateTimeOperand.SelectedValue != null)
                        {
                            strOperand = ((Field)cboDateTimeOperand.SelectedItem).TypedDisplayText;
                        }
                        else
                        {
                            strOperand = cboDateTimeOperand.Text;
                        }

                    if (durationPickerOperand.Visibility == System.Windows.Visibility.Visible)
                    {
                        strOperand = DurationHelpers.GetDefaultDurationString(durationPickerOperand.Duration, false);
                    }

                    string strAggreg = string.Empty;
                    if (cboAggregator.SelectedValue != null && (Aggregator)cboAggregator.SelectedValue!=Aggregator.None)
                    {
                        strAggreg = string.Format(" (aggregated using {0})", cboAggregator.SelectedValue);
                    }

                    builder.AppendFormat("\t{0}{1} {2} {3}\n", ((Field)cboNewFilter.SelectedItem).TypedDisplayText, 
                        strAggreg,
                        new OperatorHelper().First((a) => (a.EnumValue == op)).Description, strOperand);

                }
                else if (RadioAddSortField.IsChecked.GetValueOrDefault())
                {
                    SelectionHelper sh = (SelectionHelper)WizControl.Context;

                    SortField sf = null;

                    builder.AppendFormat(Translate("A new sort order will be added to {0}\n"), ((SelectionHelper)(WizControl.Context)).Outbound.DisplayText);

                    string strAggreg = string.Empty;
                    if (cboSortAggregator.SelectedValue != null && (Aggregator)cboSortAggregator.SelectedValue!=Aggregator.None)
                    {
                        strAggreg = string.Format(" (aggregated using {0})", cboSortAggregator.SelectedValue);
                    }

                    builder.AppendFormat("\t{0}{1} {2}\n", ((Field)cboNewSort.SelectedItem).TypedDisplayText, strAggreg, (SortOrder)cboNewSortOrder.SelectedValue);
                }

                else if (RadioAddPrompt.IsChecked.GetValueOrDefault())
                {
                    builder.AppendFormat(Translate("Prompt {0} will be added from {1}\n"), txtPromptDescription.Text, txtFilePath.Text);
                }
                else if (RadioAddPromptUnder.IsChecked.GetValueOrDefault())
                {
                    builder.AppendFormat(Translate("Prompt {0} will be added to the related prompts of {2} from {1}\n"), txtPromptDescription.Text, txtFilePath.Text, ((SelectionHelper)(WizControl.Context)).Prompt.Prompt.Description);
                }
                else if (RadioAddPredefinedText.IsChecked.GetValueOrDefault())
                {
                    builder.AppendFormat(Translate("Predefined text {0} will be created\n"), txtPredefinedTextDesc.Text);
                    builder.AppendFormat(Translate("Content:\n{0}\n"), txtPredefinedTextContent.Text);
                }
                else if (RadioAddAttachment.IsChecked.GetValueOrDefault())
                {
                    builder.AppendFormat(Translate("Attachment {0} will be created\n"), txtAttachmentDesc.Text);
                    if(radioAttPublicUrl.IsChecked.GetValueOrDefault())
                        builder.AppendFormat(Translate("Attachment content will be defined by url {0}\n"), txtAttUrl.Text);
                    else
                        builder.AppendFormat(Translate("Attachment content will be the file imported from {0}\n"), txtAttPath.Text);
                }

                tb.Text = builder.ToString();
                txtWarning.Text = builderWarning.ToString();
            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (RadioAddCamp.IsChecked.GetValueOrDefault())
            {
                using (UndoManager.Global.BeginTransaction(UndoOperation.Add, string.Concat("campaign ", txtCamp.Text)))
                {
                    NumberingPlanEntry createdNPE = null;

                    if (chkInbound.IsChecked.GetValueOrDefault() && cboDestination.SelectedIndex == -1)
                    {
                        Carrier c = core.GetAdminObject<Carrier>("defaultcarrier++++++++++++++++++");

                        createdNPE = c.CreateNumberingPlanEntry(cboDestination.Text);

                        if (createdNPE == null)
                        {
                            ConfirmationDialog dlg = new ConfirmationDialog();
                            dlg.MessageText = string.Format(Translate("The number {0} chosen for inbound is already defined. Please, use another code."), cboDestination.Text);
                            dlg.Owner = Application.Current.MainWindow;
                            dlg.IsInfoDialog = true;
                            dlg.ShowDialog();
                            return;
                        }
                    }


                    Campaign newCamp = core.Create<Campaign>();
                    newCamp.GroupKey = GroupKey;
                    newCamp.Description = txtCamp.Text;

                    //  find out why !
                    try
                    {
                        newCamp.NumberFormat.TargetId = core.GetAdminObject<Location>("defaultlocation+++++++++++++++++").NumberFormat.TargetId;
                    }
                    catch { }

                    Team newTeam = core.Create<Team>();
                    newTeam.GroupKey = GroupKey;
                    newTeam.OwnerId = newCamp.Id;
                    newTeam.Description = txtCamp.Text;
                    core.Teams.Add(newTeam);

                    Queue q = core.Create<Queue>();
                    q.Description = txtCamp.Text;
                    q.GroupKey = GroupKey;
                    q.OwnerId = newCamp.Id;
                    core.Queues.Add(q);

                    newTeam.Queues.Add(q);

                    core.Campaigns.Add(newCamp);

                    if (chkInbound.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemInboundActivity = true;

                        InboundActivity inAct = newCamp.SystemInboundActivity;
                        if (cboDestination.SelectedIndex > -1)
                            inAct.Destination = cboDestination.SelectedValue as string;
                        else
                        {
                            createdNPE.Activity.Target = inAct;
                            inAct.Destination = createdNPE.Id;
                        }

                        Preprocessor waitMusicPreproc = null;

                        try
                        {
                            waitMusicPreproc = core.Preprocessors.Single((a) => (a.MediaType == MediaType.Custom1));
                        }
                        catch
                        {
                        }
                        
                        if(waitMusicPreproc != null)
                            inAct.WaitMusicProcessor.TargetId = waitMusicPreproc.Id;

                        inAct.Preprocessor.TargetId = cboPreprocessor.SelectedValue as string;
                        inAct.PreprocessorParams = m_PreprocessorParameters;
                        if (!string.IsNullOrEmpty(cboScripts.SelectedValue as string))
                        {
                            inAct.Script.TargetId = cboScripts.SelectedValue as string;
                        }
                        inAct.ScriptUrl = cboScripts.Text;
                        inAct.AutomaticRecording = chkInAutoRecord.IsChecked;
                    }

                    if (chkOutbound.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemOutboundActivity = true;

                        OutboundActivity outAct = newCamp.SystemOutboundActivity;
                        outAct.Originator = txtOriginator.Text;
                        outAct.OutboundMode = (DialingMode)cboDialingMode.SelectedValue;
                        if (!string.IsNullOrEmpty(cboScriptOuts.SelectedValue as string))
                        {
                            outAct.Script.TargetId = cboScriptOuts.SelectedValue as string;
                        }
                        outAct.ScriptUrl = cboScriptOuts.Text;
                        outAct.AutomaticRecording = chkOutAutoRecord.IsChecked.GetValueOrDefault();
                    }

                    if (chkEmailWithProviders.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemEmailActivity = true;

                        InboundActivity inAct = newCamp.SystemEmailActivity;
                        inAct.ProviderConfigSettings = (txtMailDestination.Tag as XmlDocument).CloneNode(true) as XmlDocument; ;
                        inAct.Destination = inAct.ProviderConfig.ConfigDescription;                    

                        inAct.Preprocessor.TargetId = cboMailPreprocessor.SelectedValue as string;
                        inAct.PreprocessorParams = m_PreprocessorParameters;
                        if (!string.IsNullOrEmpty(cboMailScripts.SelectedValue as string))
                        {
                            inAct.Script.TargetId = cboMailScripts.SelectedValue as string;
                        }
                        inAct.ScriptUrl = cboMailScripts.Text;
                    }

                    if (chkEmailWithoutProvider.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemEmailActivity = true;

                        InboundActivity inAct = newCamp.SystemEmailActivity;
                        inAct.Destination = txtMailDestination.Text;
                        inAct.Preprocessor.TargetId = cboMailPreprocessor.SelectedValue as string;
                        inAct.PreprocessorParams = m_PreprocessorParameters;
                        if (!string.IsNullOrEmpty(cboMailScripts.SelectedValue as string))
                        {
                            inAct.Script.TargetId = cboMailScripts.SelectedValue as string;
                        }
                        inAct.ScriptUrl = cboMailScripts.Text;
                    }


                    if (chkChatWithProviders.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemChatActivity = true;

                        InboundActivity inAct = newCamp.SystemChatActivity;
                        inAct.ProviderConfigSettings = (txtChatDestination.Tag as XmlDocument).CloneNode(true) as XmlDocument;
                        inAct.Destination = inAct.ProviderConfig.ConfigDescription;

                        inAct.Preprocessor.TargetId = cboChatPreprocessor.SelectedValue as string;
                        inAct.PreprocessorParams = m_PreprocessorParameters;
                        if (!string.IsNullOrEmpty(cboChatScripts.SelectedValue as string))
                        {
                            inAct.Script.TargetId = cboChatScripts.SelectedValue as string;
                        }
                        inAct.ScriptUrl = cboChatScripts.Text;
                    }
                    if (chkChatWithoutProvider.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemChatActivity = true;

                        InboundActivity inAct = newCamp.SystemChatActivity;
                        inAct.Destination = txtChatDestination.Text;

                        inAct.Preprocessor.TargetId = cboChatPreprocessor.SelectedValue as string;
                        inAct.PreprocessorParams = m_PreprocessorParameters;
                        if (!string.IsNullOrEmpty(cboChatScripts.SelectedValue as string))
                        {
                            inAct.Script.TargetId = cboChatScripts.SelectedValue as string;
                        }
                        inAct.ScriptUrl = cboChatScripts.Text;
                    }

                    if (chkSearch.IsChecked.GetValueOrDefault())
                    {
                        newCamp.HasSystemSearchActivity = true;

                        OutboundActivity inAct = newCamp.SystemSearchActivity;

                        if (cboScriptSearch.SelectedItem == null)
                        {
                            inAct.ScriptUrl = cboScriptSearch.Text;
                        }
                        else if (cboScriptSearch.SelectedItem is NullAdminObject)
                        {
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(cboScriptSearch.SelectedValue as string))
                            {
                                inAct.Script.TargetId = cboScriptSearch.SelectedValue as string;
                            }
                        }

                        inAct.AutomaticRecording = chkSearchAutoRecord.IsChecked.GetValueOrDefault();

                    }



                    newCamp.SystemTeam.Agents.AddRange(listAgents.NixxisSelectedItems);

                    Created = newCamp;
                }
            }
            else if(RadioAddAct.IsChecked.GetValueOrDefault())
            {
                if (radioInbound.IsChecked.GetValueOrDefault())
                {
                    NumberingPlanEntry createdNPE = null;

                    if (cboDestination.SelectedIndex == -1)
                    {
                        Carrier c = core.GetAdminObject<Carrier>("defaultcarrier++++++++++++++++++");

                        if (c == null)
                        {
                            c = core.GetAdminObject<Carrier>("dummycarrier++++++++++++++++++++");
                        }

                        createdNPE = c.CreateNumberingPlanEntry(cboDestination.Text);

                        if (createdNPE == null)
                        {
                            ConfirmationDialog dlg = new ConfirmationDialog();
                            dlg.MessageText = string.Format(Translate("The number {0} is already defined. Please, use another code."), cboDestination.Text);
                            dlg.Owner = Application.Current.MainWindow;
                            dlg.IsInfoDialog = true;
                            dlg.ShowDialog();
                            return;
                        }
                    }


                    InboundActivity inAct = core.Create<InboundActivity>();

                    Preprocessor waitMusicPreproc = null;

                    try
                    {
                        waitMusicPreproc = core.Preprocessors.Single((a) => (a.MediaType == MediaType.Custom1));
                    }
                    catch
                    {
                    }

                    if (waitMusicPreproc != null)
                        inAct.WaitMusicProcessor.TargetId = waitMusicPreproc.Id;

                    inAct.Description = txtAct.Text;                    
                    inAct.MediaType = MediaType.Voice;
                    if(cboDestination.SelectedIndex>-1)
                        inAct.Destination = cboDestination.SelectedValue as string;
                    else
                    {
                        createdNPE.Activity.Target = inAct;
                        inAct.Destination = createdNPE.Id;
                    }

                    inAct.Preprocessor.TargetId = cboPreprocessor.SelectedValue as string;
                    inAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    if (!string.IsNullOrEmpty(cboScripts.SelectedValue as string))
                    {
                        inAct.Script.TargetId = cboScripts.SelectedValue as string;
                    }
                    inAct.ScriptUrl = cboScripts.Text;
                    inAct.AutomaticRecording = chkInAutoRecord.IsChecked;
                    core.InboundActivities.Add(inAct);
                    ((SelectionHelper)WizControl.Context).Campaign.InboundActivities.Add(inAct);

                }
                else if (radioOutbound.IsChecked.GetValueOrDefault())
                {
                    OutboundActivity outAct = core.Create<OutboundActivity>();
                    outAct.Originator = txtOriginator.Text;
                    outAct.Description = txtAct.Text;
                    outAct.Location.TargetId = "defaultlocation+++++++++++++++++";
                    outAct.Carrier.TargetId = "defaultcarrier++++++++++++++++++";
                    outAct.CallbackRules.TargetId = "defaultcbrules++++++++++++++++++";
                    outAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    outAct.OutboundMode = (DialingMode)cboDialingMode.SelectedValue;
                    if (!string.IsNullOrEmpty(cboScriptOuts.SelectedValue as string))
                    {
                        outAct.Script.TargetId = cboScriptOuts.SelectedValue as string;
                    }
                    outAct.ScriptUrl = cboScriptOuts.Text;
                    outAct.AutomaticRecording = chkOutAutoRecord.IsChecked.GetValueOrDefault();
                    core.OutboundActivities.Add(outAct);
                    ((SelectionHelper)WizControl.Context).Campaign.OutboundActivities.Add(outAct);

                }
                else if (radioChatWithoutProvider.IsChecked.GetValueOrDefault())
                {
                    InboundActivity inAct = core.Create<InboundActivity>();
                    inAct.Description = txtAct.Text;
                    inAct.Destination = txtChatDestination.Text;
                    inAct.MediaType = MediaType.Chat;
                    inAct.Preprocessor.TargetId = cboChatPreprocessor.SelectedValue as string;
                    inAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    if (!string.IsNullOrEmpty(cboChatScripts.SelectedValue as string))
                    {
                        inAct.Script.TargetId = cboChatScripts.SelectedValue as string;
                    }
                    inAct.ScriptUrl = cboChatScripts.Text;
                    core.InboundActivities.Add(inAct);
                    ((SelectionHelper)WizControl.Context).Campaign.InboundActivities.Add(inAct);
                }
                else if (radioChatWithProviders.IsChecked.GetValueOrDefault())
                {
                    InboundActivity inAct = core.Create<InboundActivity>();
                    inAct.Description = txtAct.Text;
                    inAct.ProviderConfigSettings = (txtChatDestination.Tag as XmlDocument).CloneNode(true) as XmlDocument; ;
                    inAct.Destination = inAct.ProviderConfig.ConfigDescription;
                    inAct.MediaType = MediaType.Chat;
                    inAct.Preprocessor.TargetId = cboChatPreprocessor.SelectedValue as string;
                    inAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    if (!string.IsNullOrEmpty(cboChatScripts.SelectedValue as string))
                    {
                        inAct.Script.TargetId = cboChatScripts.SelectedValue as string;
                    }
                    inAct.ScriptUrl = cboChatScripts.Text;
                    core.InboundActivities.Add(inAct);
                    ((SelectionHelper)WizControl.Context).Campaign.InboundActivities.Add(inAct);
                }

                else if (radioEmailWithProviders.IsChecked.GetValueOrDefault())
                {
                    InboundActivity inAct = core.Create<InboundActivity>();
                    inAct.Description = txtAct.Text;
                    inAct.ProviderConfigSettings = (txtMailDestination.Tag as XmlDocument).CloneNode(true) as XmlDocument; ;
                    inAct.Destination = inAct.ProviderConfig.ConfigDescription;                    
                    inAct.MediaType = MediaType.Mail;
                    inAct.Preprocessor.TargetId = cboMailPreprocessor.SelectedValue as string;
                    inAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    if (!string.IsNullOrEmpty(cboMailScripts.SelectedValue as string))
                    {
                        inAct.Script.TargetId = cboMailScripts.SelectedValue as string;
                    }
                    inAct.ScriptUrl = cboMailScripts.Text;
                    core.InboundActivities.Add(inAct);
                    ((SelectionHelper)WizControl.Context).Campaign.InboundActivities.Add(inAct);
                }
                else if (radioEmailWithoutProvider.IsChecked.GetValueOrDefault())
                {

                    InboundActivity inAct = core.Create<InboundActivity>();
                    inAct.Description = txtAct.Text;
                    inAct.Destination = txtMailDestination.Text;
                    inAct.MediaType = MediaType.Mail;
                    inAct.Preprocessor.TargetId = cboMailPreprocessor.SelectedValue as string;
                    inAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    if (!string.IsNullOrEmpty(cboMailScripts.SelectedValue as string))
                    {
                        inAct.Script.TargetId = cboMailScripts.SelectedValue as string;
                    }
                    inAct.ScriptUrl = cboMailScripts.Text;
                    core.InboundActivities.Add(inAct);
                    ((SelectionHelper)WizControl.Context).Campaign.InboundActivities.Add(inAct);
                }
                else if (radioSearch.IsChecked.GetValueOrDefault())
                {
                    OutboundActivity outAct = core.Create<OutboundActivity>();
                    outAct.Description = txtAct.Text;
                    outAct.OutboundMode = DialingMode.Search;
                    outAct.Campaign.TargetId = ((SelectionHelper)WizControl.Context).Campaign.Id;
                    outAct.Location.TargetId = "defaultlocation+++++++++++++++++";
                    outAct.Carrier.TargetId = "defaultcarrier++++++++++++++++++";
                    outAct.CallbackRules.TargetId = "defaultcbrules++++++++++++++++++";

                    if (cboScriptSearch.SelectedItem == null)
                    {
                        outAct.ScriptUrl = cboScriptSearch.Text;
                    }
                    else if (cboScriptSearch.SelectedItem is NullAdminObject)
                    {
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(cboScriptSearch.SelectedValue as string))
                            outAct.Script.TargetId = cboScriptSearch.SelectedValue as string;
                    }

                    outAct.AutomaticRecording = chkSearchAutoRecord.IsChecked.GetValueOrDefault();

                    core.OutboundActivities.Add(outAct);
                    ((SelectionHelper)WizControl.Context).Campaign.OutboundActivities.Add(outAct);
                }


            }
            else if (RadioAddQualif.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                if (!sh.Campaign.Qualification.HasTarget)
                {
                    sh.Campaign.Qualification.Target = core.Create<Qualification>(sh.Campaign, System.Guid.NewGuid().ToString("N"));
                    sh.Campaign.Qualification.Target.ParentId = null;
                    sh.Campaign.Qualifications.Add(sh.Campaign.Qualification);
                    sh.NewQualification.DisplayOrder = 0;
                }
                else
                {
                    if (sh.Campaign.Qualification.Target.Children.Count == 0)
                        sh.NewQualification.DisplayOrder = 0;
                    else
                        sh.NewQualification.DisplayOrder = sh.Campaign.Qualification.Target.Children.OrderBy((a) => (a.DisplayOrder)).Last().DisplayOrder + 1;
                }


                sh.NewQualification.ParentId = sh.Campaign.Qualification.TargetId;
                sh.Campaign.Qualification.Target.Children.Add(sh.NewQualification);
                sh.Campaign.Qualifications.Add(sh.NewQualification);

                if (sh.Campaign.Advanced && chkSpecifyQualificationsAffectation.IsChecked.GetValueOrDefault())
                {
                    foreach(Activity act in sh.Campaign.Activities )
                    {
                        if(!listQualificationsExclusion.NixxisSelectedItems.Contains(act))
                            sh.NewQualification.ActivitiesExclusions.Add(act);
                    }
                }


                Created = sh.NewQualification;
            }
            else if (RadioAddQualifUnder.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                if (sh.Qualification.Children.Count == 0)
                    sh.NewQualification.DisplayOrder = 0;
                else
                    sh.NewQualification.DisplayOrder = sh.Qualification.Children.OrderBy((a) => (a.DisplayOrder)).Last().DisplayOrder + 1;

                sh.NewQualification.ParentId = sh.Qualification.Id;
                sh.Qualification.Children.Add(sh.NewQualification);
                sh.Campaign.Qualifications.Add(sh.NewQualification);

                if (sh.Campaign.Advanced && chkSpecifyQualificationsAffectation.IsChecked.GetValueOrDefault())
                {
                    foreach (Activity act in sh.Campaign.Activities)
                    {
                        if (!listQualificationsExclusion.NixxisSelectedItems.Contains(act))
                            sh.NewQualification.ActivitiesExclusions.Add(act);
                    }
                }

                Created = sh.NewQualification;
            }
            else if (RadioAddQualifSister.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                sh.NewQualification.DisplayOrder = sh.Qualification.ParentQualification.Children.OrderBy((a) => (a.DisplayOrder)).Last().DisplayOrder + 1;

                sh.NewQualification.ParentId = sh.Qualification.ParentQualification.Id;
                sh.Qualification.ParentQualification.Children.Add(sh.NewQualification);
                sh.Campaign.Qualifications.Add(sh.NewQualification);
                if (sh.Campaign.Advanced && chkSpecifyQualificationsAffectation.IsChecked.GetValueOrDefault())
                {
                    foreach (Activity act in sh.Campaign.Activities)
                    {
                        if (!listQualificationsExclusion.NixxisSelectedItems.Contains(act))
                            sh.NewQualification.ActivitiesExclusions.Add(act);
                    }
                }

                Created = sh.NewQualification;
            }
            else if (RadioAddField.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;
                sh.Campaign.UserFields.Add(sh.NewField);

                Created = sh.NewField;
            }
            else if (RadioAddSortField.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                SortField sf = null;

                sf = (SortField)(sh.Outbound.SortFields.AddLinkItem(core.GetAdminObject<Field>((string)(cboNewSort.SelectedValue))));

                if(sh.Outbound.SortFields.Count>1)
                    sf.Sequence = sh.Outbound.SortFields.OrderBy((temp) => (temp.Sequence)).Last((temp) => (temp != sf)).Sequence + 1;
                sf.SortOrder = (SortOrder)cboNewSortOrder.SelectedValue;
    
                if(cboSortAggregator.SelectedValue!=null)
                    sf.Aggregator = (Aggregator)cboSortAggregator.SelectedValue;
                
                // TODO: should not be needed!!!!
                sh.Outbound.SortFields.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (RadioAddFilter.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                FilterPart sf = core.Create<FilterPart>(sh.Outbound);


                sf.Field.TargetId = (string)cboNewFilter.SelectedValue;



                if (cboOperatorAnsiString.Visibility == System.Windows.Visibility.Visible)
                    sf.Operator = (Operator)cboOperatorAnsiString.SelectedValue;
                if (cboOperatorString.Visibility == System.Windows.Visibility.Visible)
                    sf.Operator = (Operator)cboOperatorString.SelectedValue;
                if (cboOperatorInt.Visibility == System.Windows.Visibility.Visible)
                    sf.Operator = (Operator)cboOperatorInt.SelectedValue;
                if (cboOperatorSingle.Visibility == System.Windows.Visibility.Visible)
                    sf.Operator = (Operator)cboOperatorSingle.SelectedValue;
                if (cboOperatorBoolean.Visibility == System.Windows.Visibility.Visible)
                    sf.Operator = (Operator)cboOperatorBoolean.SelectedValue;
                if (cboOperatorDatetime.Visibility == System.Windows.Visibility.Visible)
                    sf.Operator = (Operator)cboOperatorDatetime.SelectedValue;


                if (cboAnsiStrOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboAnsiStrOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboAnsiStrOperand.SelectedValue);
                    else
                        sf.OperandText = cboAnsiStrOperand.Text;

                if (cboStrOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboStrOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboStrOperand.SelectedValue);
                    else
                        sf.OperandText = cboStrOperand.Text;

                if (cboStrAgentsOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboStrAgentsOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboStrAgentsOperand.SelectedValue);
                    else
                        sf.OperandText = cboStrOperand.Text;
                
                if (cboStrActivitiessOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboStrActivitiessOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboStrActivitiessOperand.SelectedValue);
                    else
                        sf.OperandText = cboStrOperand.Text;

                if (cboStrAreasOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboStrAreasOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboStrAreasOperand.SelectedValue);
                    else
                        sf.OperandText = cboStrOperand.Text;

                if (cboStrQualificationsOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboStrQualificationsOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboStrQualificationsOperand.SelectedValue);
                    else
                        sf.OperandText = cboStrOperand.Text;
                
                if (cboIntOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboIntOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboIntOperand.SelectedValue);
                    else
                        sf.OperandText = cboIntOperand.Text;
                if (cboSingleOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboSingleOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboSingleOperand.SelectedValue);
                    else
                        sf.OperandText = cboSingleOperand.Text;
                if (cboBoolOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboBoolOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboBoolOperand.SelectedValue);
                    else
                        sf.OperandText = cboBoolOperand.Text;
                if (cboDateTimeOperand.Visibility == System.Windows.Visibility.Visible)
                    if (cboDateTimeOperand.SelectedValue != null)
                        sf.OperandField.TargetId = ((string)cboDateTimeOperand.SelectedValue);
                    else
                        sf.OperandText = cboDateTimeOperand.Text;

                if (durationPickerOperand.Visibility == System.Windows.Visibility.Visible)
                    sf.OperandText = (durationPickerOperand.Duration / 60).ToString();


                sf.Activity.TargetId= sh.Outbound.Id;

                if (sh.Outbound.FilterParts.Count > 0)
                    sf.Sequence = sh.Outbound.FilterParts.OrderBy((temp) => (temp.Sequence)).Last((temp) => (temp != sf)).Sequence + 1;

                if (cboAggregator.SelectedValue != null && (Aggregator)cboAggregator.SelectedValue != Aggregator.None)
                {
                    sf.Aggregator = (Aggregator)cboAggregator.SelectedValue;
                }

                sh.Outbound.FilterParts.Add(sf);
            }
            else if (RadioAddPrompt.IsChecked.GetValueOrDefault() || RadioAddPromptUnder.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                Prompt pr = core.Create<Prompt>();
                pr.GroupKey = GroupKey;
                pr.Description = txtPromptDescription.Text;
                pr.Language.TargetId = (string)cboPromptLan.SelectedValue;
                if(sh.Activity != null)
                    pr.ComputePath(sh.Activity.Id, System.IO.Path.GetExtension(txtFilePath.Text));
                else
                    pr.ComputePath(sh.Campaign.Id, System.IO.Path.GetExtension(txtFilePath.Text));


                pr.LocalPath = txtFilePath.Text;


                if (RadioAddPromptUnder.IsChecked.GetValueOrDefault())
                {
                    pr.RelatedTo.TargetId = sh.Prompt.Prompt.Id;
                    sh.Prompt.Prompt.RelatedPrompts.Add(pr);
                }
                else
                {
                    if (sh.Activity != null)
                        sh.Activity.Prompts.Add(pr);
                    else
                        sh.Campaign.Prompts.Add(pr);
                }


                core.Prompts.Add(pr);

                Created = pr;
            }
            else if (RadioAddPredefinedText.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                PredefinedText pr = core.Create<PredefinedText>();

                if (sh.Campaign.PredefinedTexts.Count > 0)
                    pr.Sequence = sh.Campaign.PredefinedTexts.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;

                pr.Description = txtPredefinedTextDesc.Text;
                pr.Language.TargetId = (string)cboPredefinedTextLan.SelectedValue;
                pr.TextContent = txtPredefinedTextContent.Text;
                pr.Campaign.TargetId = sh.Campaign.Id;
                sh.Campaign.PredefinedTexts.Add(pr);

                Created = pr;
            }
            else if (RadioAddAttachment.IsChecked.GetValueOrDefault())
            {
                SelectionHelper sh = (SelectionHelper)WizControl.Context;

                Attachment pr = core.Create<Attachment>();

                if (sh.Campaign.Attachments.Count > 0)
                    pr.Sequence = sh.Campaign.Attachments.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;

                pr.Description = txtAttachmentDesc.Text;
                pr.Language.TargetId = (string)cboAttachmentLan.SelectedValue;
                pr.Campaign.TargetId = sh.Campaign.Id;
                if (radioAttPublicUrl.IsChecked.GetValueOrDefault())
                {
                    pr.LocationIsLocal = false;
                    pr.Location = txtAttUrl.Text;
                }
                else
                {
                    pr.LocationIsLocal = true;
                    pr.LocalPath = txtAttPath.Text;
                }


                sh.Campaign.Attachments.Add(pr);

                Created = pr;
            }
            DialogResult = true;
        }

        private void BrowseForFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".wav"; // Default file extension
            dlg.Filter = "Wave sound files|*.wav|Alaw sound files|*.alaw|All files|*.*"; // Filter files by extension
            dlg.Multiselect = false;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                txtFilePath.Text = dlg.FileName;
                txtPromptDescription.Text = dlg.SafeFileName;


                WaveInfo wi = new WaveInfo(txtFilePath.Text);
                (Helpers.FindChild<TextBlock>(lblFileInfo, (a) => (true)) as TextBlock).Text = wi.ToString();

                if (!wi.IsSupported)
                    lblFileInfo.Foreground = Brushes.Red;
                else
                    lblFileInfo.Foreground = lbl.Foreground;

                lblFileInfo.Visibility = System.Windows.Visibility.Visible;

            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        public AdminObject Created
        {
            get;
            internal set;
        }

        public string GroupKey { get; set; }

        private void VoicePreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is NullAdminObject || (((Preprocessor)e.Item).MediaType == MediaType.Voice);
        }

        private void ChatPreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is NullAdminObject || (((Preprocessor)e.Item).MediaType == MediaType.Chat);
        }

        private void MailPreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is NullAdminObject || (((Preprocessor)e.Item).MediaType == MediaType.Mail);
        }

        private void Scripts_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = e.Item is NullAdminObject || (((Preprocessor)e.Item).MediaType == MediaType.None);
        }

        private void StringOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithString(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void IntOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithInt(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void DateTimeOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithDate(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void BoolOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithBool(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void CombineOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (((EnumHelper<CombineOperator>)(e.Item)).EnumValue != CombineOperator.None);
        }

        private void BrowseForAttachment(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All files (*.*)|*.*"; // Filter files by extension
            dlg.Multiselect = false;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                txtAttPath.Text = dlg.FileName;
            }

        }

        public IMediaProviderConfigurator MailProvider
        {
            get
            {
                Nixxis.Client.Admin.IMediaProviderConfigurator impc = null;

                impc = Activator.CreateInstance(Type.GetType((EmailProviders.SelectedItem as MediaProvider).PluginType)) as Nixxis.Client.Admin.IMediaProviderConfigurator;
                impc.InitializeProvider((AdminCore)DataContext, (EmailProviders.SelectedItem as MediaProvider));

                return impc;
            }
        }

        public IMediaProviderConfigurator ChatProvider
        {
            get
            {
                Nixxis.Client.Admin.IMediaProviderConfigurator impc = null;

                impc = Activator.CreateInstance(Type.GetType((ChatProviders.SelectedItem as MediaProvider).PluginType)) as Nixxis.Client.Admin.IMediaProviderConfigurator;
                impc.InitializeProvider((AdminCore)DataContext, (ChatProviders.SelectedItem as MediaProvider));

                return impc;
            }
        }

        private void WizControl_WizardStepChanging(object sender, RoutedEventArgs e)
        {
            if (WizControl.CurrentStep == "EmailSettings2" && WizControl.PreviousStep == "EmailSettings")
            {
                IMediaProviderConfigurator impc = MailProvider;

                if (txtMailDestination.Tag != null && impc!=null)
                    impc.Config = (XmlDocument)txtMailDestination.Tag;

                if (impc!=null && impc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                {
                    txtMailDestination.Text = impc.ConfigDescription;
                    txtMailDestination.Tag = impc.Config;
                }
                else
                    WizControl.CurrentStep = "EmailSettings";

            }else if (WizControl.CurrentStep == "ChatSettings2" && WizControl.PreviousStep == "ChatSettings")
            {
                IMediaProviderConfigurator impc = ChatProvider;

                if (txtChatDestination.Tag != null && impc != null)
                    impc.Config = (XmlDocument)txtChatDestination.Tag;

                if (impc != null && impc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                {
                    txtChatDestination.Text = impc.ConfigDescription;
                    txtChatDestination.Tag = impc.Config;
                }
                else
                    WizControl.CurrentStep = "ChatSettings";
            }
            FirePropertyChanged("SelectedItem");

        }

        private void EmailMediaProvider_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((MediaProvider)e.Item).MediaType == MediaType.Mail;
        }

        private void ChatMediaProvider_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((MediaProvider)e.Item).MediaType == MediaType.Chat;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = !((NumberingPlanEntry)e.Item).Activity.HasTarget;
        }

        private void NoQuota_Filter(object sender, FilterEventArgs e)
        {
            EnumHelper<UserFieldMeanings> temp = e.Item as EnumHelper<UserFieldMeanings>;
            e.Accepted = (temp.EnumValue != UserFieldMeanings.Quota);
        }

        private void ChkUniqueConstraint_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SelectionHelper sh = (SelectionHelper)WizControl.Context;
            if (sh.NewField != null && sh.NewField.DBType == DBTypes.String && sh.NewField.Length == -1)
                ChkUniqueConstraint.IsChecked = false;
        }

        private void ChkIndexed_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SelectionHelper sh = (SelectionHelper)WizControl.Context;
            if (sh.NewField != null && sh.NewField.DBType == DBTypes.String && sh.NewField.Length == -1)
                (sender as CheckBox).IsChecked = false;
        }

        public object SelectedItem
        {
            get
            {
                if (RadioAddQualif.IsChecked.GetValueOrDefault())
                {
                    return (WizControl.Context as SelectionHelper).NewQualification;
                }
                else if (RadioAddQualifUnder.IsChecked.GetValueOrDefault())
                {
                    return (WizControl.Context as SelectionHelper).NewQualification;
                }
                else if (RadioAddQualifSister.IsChecked.GetValueOrDefault())
                {
                    return (WizControl.Context as SelectionHelper).NewQualification;
                }
                else if (RadioAddField.IsChecked.GetValueOrDefault())
                {
                    return (WizControl.Context as SelectionHelper).NewField;
                }
                return null;
            }
        }

        public void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

    
    }

    public class AggregatorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DBTypes paramdbtype = (DBTypes)parameter;
            
            if (values[0] == null || DependencyProperty.UnsetValue.Equals(values[0]))
                return Visibility.Collapsed;

            DBTypes valuedbtype = (DBTypes)values[0];


            if (values[1] == null || DependencyProperty.UnsetValue.Equals(values[1]) || (Aggregator)values[1]==Aggregator.None)
            {
                if (paramdbtype == valuedbtype)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
            {
                if (paramdbtype == DBTypes.Integer)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }
}
