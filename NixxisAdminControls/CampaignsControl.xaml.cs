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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nixxis.Client.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.IO;
using System.Xml;
using System.Threading;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for SkillsControl.xaml
    /// </summary>
    public partial class CampaignsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("CampaignsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        private Prompt m_SelectedRelatedCampPrompt = null;
        private Prompt m_SelectedRelatedActPrompt = null;

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CampaignsControl));
        private static bool test(DependencyObject obj)
        {
            System.Diagnostics.Trace.WriteLine(obj.GetType());
            return false;
        }

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                if (selected is Campaign)
                {
                    Helpers.ApplyToVisualChildren<DataGrid>(MainGrid, DataGrid.SelectedItemProperty, null);
                    SelectedItem = null;
                    SelectedItem = selected;

                    SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
                    sh.Reset();
                    sh.Inbound = null;
                    sh.Outbound = null;

                    if (context is Agent)
                    {
                        AgentsTab.IsSelected = true;
                        DGAgents.UpdateLayout();
                        DGAgents.SelectedItem = ((AdminCheckedLinkList)DGAgents.ItemsSource).FindItem(context);
                    }
                    else if (context is Prompt)
                    {
                        TabCampPrompts.IsSelected = true;
                        DGCampPrompts.UpdateLayout();
                        DGCampPrompts.SelectedItem = ((Prompt)context).Links[0];
                    }
                    else if (context is Team)
                    {
                        TeamsTab.IsSelected = true;
                        DGTeams.UpdateLayout();
                        DGTeams.SelectedItem = ((AdminCheckedLinkList)DGTeams.ItemsSource).FindItem(context);
                    }
                    else if (context is Field)
                    {
                        DataTab.IsSelected = true;
                        DGData.UpdateLayout();
                        DGData.SelectedItem = context;
                    }
                    else if (context is Qualification)
                    {
                        QualificationsTab.IsSelected = true;
                        QualifsTree.UpdateLayout();

                        List<object> tempList = new List<object>();
                        Qualification temp = (Qualification)context;

                        tempList.Insert(0, temp);
                        while (!string.IsNullOrEmpty(temp.ParentId))
                        {
                            temp = ((AdminCore)DataContext).GetAdminObject<Qualification>(temp.ParentId);
                            tempList.Insert(0, temp);
                        }

                        tempList.RemoveAt(0);

                        QualifsTree.SelectItem(tempList);
                    }
                    else if (context is Skill)
                    {
                        Activity act = (Activity)ExtraInformation;
                        if (act.MediaType == MediaType.Chat)
                        {
                            ChatTab.IsSelected = true;
                            Helpers.WaitForPriority();
                            tabChatSkills.IsSelected = true;
                            DGChatSkills.UpdateLayout();
                            DGChatSkills.SelectedItem = ((AdminCheckedLinkList)DGChatSkills.ItemsSource).FindItem(context);

                        }
                        else if (act.MediaType == MediaType.Mail)
                        {
                            MailTab.IsSelected = true;
                            Helpers.WaitForPriority();
                            tabMailSkills.IsSelected = true;
                            DGMailSkills.UpdateLayout();
                            DGMailSkills.SelectedItem = ((AdminCheckedLinkList)DGMailSkills.ItemsSource).FindItem(context);
                        }
                        else if (act.MediaType == MediaType.Voice)
                        {
                            InboundTab.IsSelected = true;
                            Helpers.WaitForPriority();
                            tabInboundSkills.IsSelected = true;
                            DGInboundSkills.UpdateLayout();
                            DGInboundSkills.SelectedItem = ((AdminCheckedLinkList)DGInboundSkills.ItemsSource).FindItem(context);
                        }
                    }
                    else if (context is Language)
                    {
                        Activity act = (Activity)ExtraInformation;
                        if (act.MediaType == MediaType.Chat)
                        {
                            ChatTab.IsSelected = true;
                            Helpers.WaitForPriority();
                            tabChatLanguages.IsSelected = true;
                            DGChatLanguages.UpdateLayout();
                            DGChatLanguages.SelectedItem = ((AdminCheckedLinkList)DGChatLanguages.ItemsSource).FindItem(context);

                        }
                        else if (act.MediaType == MediaType.Mail)
                        {
                            MailTab.IsSelected = true;
                            Helpers.WaitForPriority();
                            tabMailLanguages.IsSelected = true;
                            DGMailLanguages.UpdateLayout();
                            DGMailLanguages.SelectedItem = ((AdminCheckedLinkList)DGMailLanguages.ItemsSource).FindItem(context);
                        }
                        else if (act.MediaType == MediaType.Voice)
                        {
                            InboundTab.IsSelected = true;
                            Helpers.WaitForPriority();
                            tabInboundLanguages.IsSelected = true;
                            DGInboundLanguages.UpdateLayout();
                            DGInboundLanguages.SelectedItem = ((AdminCheckedLinkList)DGInboundLanguages.ItemsSource).FindItem(context);
                        }
                    }
                }
                else if (selected is Activity)
                {
                    Campaign camp = ((Activity)selected).Campaign.Target;

                    SelectedItem = camp;

                    if (camp.Advanced)
                    {

                        Helpers.ApplyToVisualChildren<ToggleButton>(MainGrid, ToggleButton.IsCheckedProperty, true, (de) => ((de as UIElement).IsVisible && Helpers.FindVisualParent<DataGridRow>(de as UIElement).IsSelected && !(de as ToggleButton).IsChecked.GetValueOrDefault()));

                        // This one is necessary!!!
                        Helpers.WaitForPriority();

                        DataGridRow row = (DataGridRow)(MainGrid.ItemContainerGenerator.ContainerFromItem(MainGrid.SelectedItem));

                        // Getting the ContentPresenter of the row details
                        DataGridDetailsPresenter presenter = Helpers.FindVisualChild<DataGridDetailsPresenter>(row);

                        // Finding Remove button from the DataTemplate that is set on that ContentPresenter
                        DataTemplate template = presenter.ContentTemplate;
                        NixxisDataGrid ndg = null;
                        // exception is sometimes triggered: This operation is valid only on elements that have this template applied.???
                        try
                        {
                            if (selected is InboundActivity)
                                ndg = template.FindName("InboundGrid", presenter) as NixxisDataGrid;
                            else if (selected is OutboundActivity)
                                ndg = template.FindName("OutboundGrid", presenter) as NixxisDataGrid;
                        }
                        catch
                        {
                        }

                        if (ndg != null)
                            ndg.SelectedItem = selected;
                    }
                    else
                    {
                        if (selected is InboundActivity)
                            InboundTab.IsSelected = true;
                        else if (selected is OutboundActivity)
                            OutboundTab.IsSelected = true;
                    }
                    Helpers.WaitForPriority();

                    if (context is Skill)
                    {
                        TabInSkills.IsSelected = true;
                        DGInboundActivitySkills.UpdateLayout();
                        DGInboundActivitySkills.SelectedItem = ((AdminCheckedLinkList)DGInboundActivitySkills.ItemsSource).FindItem(context);
                    }
                    else if (context is Language)
                    {
                        TabInLanguages.IsSelected = true;
                        DGInboundActivityLanguages.UpdateLayout();
                        DGInboundActivityLanguages.SelectedItem = ((AdminCheckedLinkList)DGInboundActivityLanguages.ItemsSource).FindItem(context);
                    }
                    else if (context is Prompt)
                    {
                        TabInActivityPrompts.IsSelected = true;
                        DGInActPrompts.UpdateLayout();
                        DGInActPrompts.SelectedItem = ((Prompt)context).Links[0];
                    }
                }
            }
        }

        
        public void SetSelected(AdminObject selected)
        {
            if (selected != null)
            {
                if (selected is Campaign)
                {
                    Helpers.ApplyToVisualChildren<DataGrid>(MainGrid, DataGrid.SelectedItemProperty, null);
                    SelectedItem = null;                     
                    SelectedItem = selected;

                    SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
                    sh.Reset();
                    sh.Inbound = null;
                    sh.Outbound = null;
                }
                else if (selected is Team || selected is AgentTeam)
                {
                    AdminObject temp = selected;

                    if (selected is AgentTeam)
                        temp = ((AgentTeam)selected).Team;
                    
                    while (!(temp is Campaign) )
                    {
                        temp = temp.Owner;
                    }
                    SelectedItem = temp;

                    if (AgentsTab.Visibility == System.Windows.Visibility.Visible)
                    {
                        AgentsTab.IsSelected = true;
                        if (selected is AgentTeam)
                        {

                            if (DGAgents.ItemsSource is AdminCheckedLinkList)
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGAgents.SelectedItem = ((AdminCheckedLinkList)DGAgents.ItemsSource).FindItem(((AgentTeam)selected).Agent); }));
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGAgents.SelectedItem = ((AgentTeam)selected).Agent; }));
                            }
                        }
                    }
                }
                else if (selected is Activity)
                {
                    Campaign camp = ((Activity)selected).Campaign.Target;

                    SelectedItem = camp;

                    Helpers.ApplyToVisualChildren<ToggleButton>(MainGrid, ToggleButton.IsCheckedProperty, true, (de) => ((de as UIElement).IsVisible && Helpers.FindVisualParent<DataGridRow>(de as UIElement).IsSelected && !(de as ToggleButton).IsChecked.GetValueOrDefault()));

                    // This one is necessary!!!
                    Helpers.WaitForPriority();

                    DataGridRow row = (DataGridRow)(MainGrid.ItemContainerGenerator.ContainerFromItem(MainGrid.SelectedItem));

                    // Getting the ContentPresenter of the row details
                    DataGridDetailsPresenter presenter = Helpers.FindVisualChild<DataGridDetailsPresenter>(row);

                    // Finding Remove button from the DataTemplate that is set on that ContentPresenter
                    DataTemplate template = presenter.ContentTemplate;
                    NixxisDataGrid ndg = null;
                    // exception is sometimes triggered: This operation is valid only on elements that have this template applied.???
                    try
                    {
                        if (selected is InboundActivity)
                            ndg = template.FindName("InboundGrid", presenter) as NixxisDataGrid;
                        else if (selected is OutboundActivity)
                            ndg = template.FindName("OutboundGrid", presenter) as NixxisDataGrid;
                    }
                    catch
                    {
                    }

                    if (ndg != null)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { ndg.SelectedItem = selected; }));
                    }
                }
                else if (selected is Qualification)
                {
                    List<object> tempList = new List<object>();
                    Qualification temp = (Qualification)selected;

                    tempList.Insert(0, temp);
                    while (!string.IsNullOrEmpty(temp.ParentId))
                    {
                        temp = ((AdminCore)DataContext).GetAdminObject<Qualification>(temp.ParentId);
                        tempList.Insert(0, temp);
                    }

                    tempList.RemoveAt(0);

                    Helpers.ApplyToVisualChildren<DataGrid>(MainGrid, DataGrid.SelectedItemProperty, null);

                    SelectedItem = ((Qualification)selected).MainCampaign;

                    if (QualificationsTab.Visibility == System.Windows.Visibility.Visible)
                    {
                        QualificationsTab.IsSelected = true;

                        QualifsTree.SelectItem(tempList);
                    }

                }
                else if (selected is Field)
                {
                    Field field = (Field)selected;

                    SetSelected(field.Campaign.Target);

                    if (DataTab.Visibility == System.Windows.Visibility.Visible)
                    {
                        DataTab.IsSelected = true;

                        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGData.SelectedItem = selected; }));
                    }

                }
                else if (selected is Queue)
                {
                    SetSelected(((Queue)selected).Owner);
                }
                else if (selected is QueueTeam)
                {
                    AdminObject temp = ((QueueTeam)selected).Team;

                    while (temp != null && !(temp is Campaign))
                    {
                        temp = temp.Owner;
                    }
                    SelectedItem = temp;

                    if (TeamsTab.Visibility == System.Windows.Visibility.Visible)
                    {
                        TeamsTab.IsSelected = true;

                        if (DGTeams.ItemsSource is AdminCheckedLinkList)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGTeams.SelectedItem = ((AdminCheckedLinkList)DGTeams.ItemsSource).FindItem(((QueueTeam)selected).Team); }));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => {DGTeams.SelectedItem = ((QueueTeam)selected).Team;}));
                        }
                    }

                }
                else if (selected is Prompt)
                {
                    Prompt pr = (Prompt)selected;
                    if (pr.Links != null && pr.Links.Count > 0)
                    {
                        System.Diagnostics.Debug.Assert(pr.Links.Count == 1);

                        PromptLink pl = (PromptLink)pr.Links[0];

                        if (pl.Link is Activity)
                        {
                            SetSelected(pl.Link);
                            TabInActivityPrompts.IsSelected = true;
                            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGInActPrompts.SelectedItem = pl; }));
                        }
                        else if (pl.Link is Campaign)
                        {
                            SetSelected(pl.Link);
                            TabCampPrompts.IsSelected = true;
                            DGCampPrompts.UpdateLayout();
                            DGCampPrompts.ScrollIntoView(pl);
                            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGCampPrompts.ScrollIntoView(pl); DGCampPrompts.SelectedItem = pl; }));
                        }
                    }
                }
                else if (selected is PredefinedText)
                {
                    PredefinedTab.IsSelected = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGPredefinedTexts.SelectedItem = selected; }));
                }
                else if (selected is Attachment)
                {
                    AttachmentsTab.IsSelected = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGAttachments.SelectedItem = selected; }));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }


        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }


        public CampaignsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(CampaignsControl_IsVisibleChanged);

            InitializeComponent();
            
            ((CollectionViewSource)(FindResource("List"))).Filter += new FilterEventHandler(List_Filter);
        }

        void List_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = BaseFilter(e.Item as AdminObject, null);
        }


        private bool BaseFilter(AdminObject obj, string groupkey)
        {
            return obj==null || (!obj.IsSystem && !obj.IsDummy && !obj.IsPartial) && (groupkey == null || groupkey.Equals(obj.GroupKey));
        }

        
        void CampaignsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)sender).IsVisible)
            {

                NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.CanExecute(nep))
                        Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.Execute(nep);
                }
                nep = FindResource("DetailsPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.CanExecute(nep))
                        Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.Execute(nep);
                }

            }
            else
            {
                NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                        NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("DetailsPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                        NixxisGrid.RemovePanelCommand.Execute(nep);
                }

            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            object selection = ((ComboBox)sender).SelectedValue;
            ((ComboBox)sender).ItemsSource = null;
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Campaign)));
            try
            {
                ((ComboBox)sender).SelectedValue = selection;
            }
            catch
            {
            }
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            CollectionViewSource.GetDefaultView(MainGrid.ItemsSource).Filter = ((a) => (BaseFilter((AdminObject)a, ((ComboBox)sender).SelectedValue as string)));
            SetSelected(MainGrid.SelectedItem as AdminObject);
        }
      

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                #region Add
                
                DlgAddCampaignOrActivity dlg = new DlgAddCampaignOrActivity();
                try
                {
                    if (cboFilter.SelectedValue != null)
                        dlg.GroupKey = cboFilter.SelectedValue as string;
                    else
                        dlg.GroupKey = ((IMainWindow)Application.Current.MainWindow).Core.Agents[(Application.Current.MainWindow as IMainWindow).LoggedIn].GroupKey;
                }
                catch
                {

                }

                SelectionHelper tempHelper = Resources["selectionHelper"] as SelectionHelper;
                tempHelper.Reset();
                dlg.WizControl.Context = tempHelper;

                ContextMenu cm = e.Parameter as ContextMenu;
                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);
                    if (obj != null)
                    {
                        return;
                    }
                    obj = NixxisTreeView.GetClickedData(cm);
                    if (obj != null)
                    {
                        if (obj is Qualification)
                        {
                            tempHelper.Qualification = (Qualification)obj;
                            tempHelper.NewQualification = (DataContext as AdminCore).Create<Qualification>(tempHelper.Campaign);

                            dlg.RadioAddQualifUnder.IsChecked = true;
                            dlg.WizControl.CurrentStep = "QualSettings";
                            dlg.Title = Translate("Qualification creation...");
                            dlg.DataContext = DataContext;
                            dlg.Owner = Application.Current.MainWindow;

                            if (dlg.ShowDialog().GetValueOrDefault())
                            {
                                SetSelected(dlg.Created);
                            }
                            else
                            {
                                (DataContext as AdminCore).Delete(tempHelper.NewQualification);
                            }
                        }
                        return;
                    }
                    else if (cm.PlacementTarget == QualifsTree)
                    {
                        tempHelper.NewQualification = (DataContext as AdminCore).Create<Qualification>(tempHelper.Campaign);

                        dlg.RadioAddQualif.IsChecked = true;
                        dlg.WizControl.CurrentStep = "QualSettings";
                        dlg.Title = Translate("Qualification creation...");
                        dlg.DataContext = DataContext;
                        dlg.Owner = Application.Current.MainWindow;

                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            SetSelected(dlg.Created);
                        }
                        else
                        {
                            (DataContext as AdminCore).Delete(tempHelper.NewQualification);
                        }
                        return;
                    }
                }


                if (!IsExpertUser((dlg.WizControl.Context as SelectionHelper).Campaign) || (dlg.WizControl.Context as SelectionHelper).Campaign == null || !(dlg.WizControl.Context as SelectionHelper).Campaign.Advanced)
                {
                    tempHelper.ProposeCampaignCreation = CanCreateNewCampaign();

                    if (QualificationsTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.Qualification = QualifsTree.SelectedItem as Qualification;
                        tempHelper.NewQualification = (DataContext as AdminCore).Create<Qualification>(tempHelper.Campaign);
                        if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and qualification creation...");
                        }
                        else
                        {
                            dlg.RadioAddQualif.IsChecked = true;
                            dlg.WizControl.CurrentStep = "QualSettings";
                            dlg.Title = Translate("Qualification creation...");
                        }
                        
                    }
                    else if (DataTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.NewField = (DataContext as AdminCore).Create<UserField>(tempHelper.Campaign);
                        tempHelper.NewField.Campaign.TargetId = tempHelper.Campaign.Id;
                        if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and field creation...");
                        }
                        else
                        {
                            dlg.RadioAddField.IsChecked = true;
                            dlg.WizControl.CurrentStep = "FieldSettings";
                            dlg.Title = Translate("Field creation...");
                        }
                    }
                    else if (SortOrderTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {                        
                        tempHelper.ProposeNewSort = true;
                        if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and data sort order...");

                        }
                        else
                        {
                            dlg.RadioAddSortField.IsChecked = true;
                            dlg.WizControl.CurrentStep = "SortFieldSettings";
                            dlg.Title = Translate("Data sort order...");

                        }
                    }
                    else if (FilterTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewFilter = true;
                        if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and data filter...");
                        }
                        else
                        {
                            dlg.RadioAddFilter.IsChecked = true;
                            dlg.WizControl.CurrentStep = "FilterSettings";
                            dlg.Title = Translate("Data filter...");
                        }
                        
                    }
                    else if (TabCampPrompts.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewPrompt = true;
                        tempHelper.Prompt = DGCampPrompts.SelectedItem as PromptLink;
                        
                        if(tempHelper.Prompt!=null)
                            tempHelper.ProposeNewPromptUnder = true;
                        if (!tempHelper.ProposeCampaignCreation && tempHelper.Prompt == null)
                        {
                            dlg.RadioAddPrompt.IsChecked = true;
                            dlg.WizControl.CurrentStep = "PromptSettings";
                            dlg.Title = Translate("Prompts...");
                        }
                        else
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and prompts...");
                        }
                        
                    }
                    else if (PredefinedTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewPredefinedText = true;
                        if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and predefined texts...");
                        }
                        else
                        {
                            dlg.RadioAddPredefinedText.IsChecked = true;
                            dlg.WizControl.CurrentStep = "PredefinedTextSettings";
                            dlg.Title = Translate("Predefined texts...");
                        }

                        
                    }
                    else if (AttachmentsTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewAttachment = true;
                        if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and attachments...");
                        }
                        else
                        {
                            dlg.RadioAddAttachment.IsChecked = true;
                            dlg.WizControl.CurrentStep = "AttachmentSettings";
                            dlg.Title = Translate("Attachments...");
                        }
                        
                    }
                    else if (TabInActivityPrompts.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewPrompt = true;
                        tempHelper.Prompt = DGInActPrompts.SelectedItem as PromptLink;
                        if (tempHelper.Prompt != null)
                            tempHelper.ProposeNewPromptUnder = true;
                        if (!tempHelper.ProposeCampaignCreation && tempHelper.Prompt == null)
                        {
                            dlg.RadioAddPrompt.IsChecked = true;
                            dlg.WizControl.CurrentStep = "PromptSettings";
                            dlg.Title = Translate("Prompts...");
                        }
                        else
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and prompts...");
                        }
                        
                    }
                    else
                    {
                        dlg.WizControl.CurrentStep = "CampSettings";
                        dlg.Title = Translate("Campaign creation...");
                    }
                }
                else
                {
                    tempHelper.ProposeCampaignCreation = CanCreateNewCampaign();

                    tempHelper.ProposeActivityCreation = CanCreateNewActivities() && !tempHelper.Campaign.IsReadOnly;

                    if (QualificationsTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.Qualification = QualifsTree.SelectedItem as Qualification;
                        tempHelper.NewQualification = (DataContext as AdminCore).Create<Qualification>(tempHelper.Campaign);

                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and qualifications creation...");
                        }
                        else if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and qualifications creation...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and qualifications creation...");
                        }
                        else
                        {
                            dlg.RadioAddQualif.IsChecked = true;
                            dlg.WizControl.CurrentStep = "QualSettings";
                            dlg.Title = Translate("Qualifications creation...");

                        }
                    }
                    else if (DataTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.NewField = (DataContext as AdminCore).Create<UserField>(tempHelper.Campaign);
                        tempHelper.NewField.Campaign.TargetId = tempHelper.Campaign.Id;
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and field creation...");
                        }
                        else if (tempHelper.ProposeCampaignCreation )
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and field creation...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and field creation...");
                        }
                        else
                        {
                            dlg.RadioAddField.IsChecked = true;
                            dlg.WizControl.CurrentStep = "FieldSettings";
                            dlg.Title = Translate("Field creation...");
                        }

                    }
                    else if (SortOrderTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewSort = true;
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and data sort order...");
                        }
                        else if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and data sort order...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and data sort order...");
                        }
                        else
                        {
                            dlg.RadioAddSortField.IsChecked = true;
                            dlg.WizControl.CurrentStep = "SortFieldSettings";
                            dlg.Title = Translate("Data sort order...");
                        }

                        
                    }
                    else if (FilterTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewFilter = true;
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and data filter...");
                        }
                        else if (tempHelper.ProposeCampaignCreation )
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and data filter...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and data filter...");
                        }
                        else
                        {
                            dlg.RadioAddFilter.IsChecked = true;
                            dlg.WizControl.CurrentStep = "FilterSettings";
                            dlg.Title = Translate("Data filter...");
                        }

                    }
                    else if (TabInActivityPrompts.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewPrompt = true;
                        tempHelper.Prompt = DGInActPrompts.SelectedItem as PromptLink;
                        if (tempHelper.Prompt != null)
                            tempHelper.ProposeNewPromptUnder = true;



                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and prompts...");
                        }
                        else if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and prompts...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and prompts...");
                        }
                        else
                        {
                            if (tempHelper.Prompt == null)
                            {
                                dlg.RadioAddPrompt.IsChecked = true;
                                dlg.WizControl.CurrentStep = "PromptSettings";
                            }
                            else
                            {
                                dlg.WizControl.CurrentStep = "Start";
                            }
                            dlg.Title = Translate("Prompts...");
                        }
                    }
                    else if (TabCampPrompts.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewPrompt = true;
                        tempHelper.Prompt = DGCampPrompts.SelectedItem as PromptLink;
                        if (tempHelper.Prompt != null)
                            tempHelper.ProposeNewPromptUnder = true;
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and prompts...");
                        }
                        else if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and prompts...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and prompts...");
                        }
                        else
                        {
                            if (tempHelper.Prompt == null)
                            {
                                dlg.RadioAddPrompt.IsChecked = true;
                                dlg.WizControl.CurrentStep = "PromptSettings";
                            }
                            else
                            {
                                dlg.WizControl.CurrentStep = "Start";
                            }
                            dlg.Title = Translate("Prompts...");
                        }

                    }
                    else if (PredefinedTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewPredefinedText = true;
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and predefined texts...");
                        }
                        else if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and predefined texts...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and predefined texts...");
                        }
                        else
                        {
                            dlg.RadioAddPredefinedText.IsChecked = true;
                            dlg.WizControl.CurrentStep = "PredefinedTextSettings";
                            dlg.Title = Translate("Predefined texts...");
                        }
                    }
                    else if (AttachmentsTab.IsSelected && !tempHelper.Campaign.IsReadOnly)
                    {
                        tempHelper.ProposeNewAttachment = true;
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign, activity and attachments...");
                        }
                        else if (tempHelper.ProposeCampaignCreation )
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and attachments...");
                        }
                        else if (tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Activity and attachments...");
                        }
                        else
                        {
                            dlg.RadioAddAttachment.IsChecked = true;
                            dlg.WizControl.CurrentStep = "AttachmentSettings";
                            dlg.Title = Translate("Attachments...");
                        }
                    }
                    else
                    {
                        if (tempHelper.ProposeCampaignCreation && tempHelper.ProposeActivityCreation)
                        {
                            dlg.WizControl.CurrentStep = "Start";
                            dlg.Title = Translate("Campaign and activity creation...");
                        }
                        else if (tempHelper.ProposeCampaignCreation)
                        {
                            dlg.RadioAddCamp.IsChecked = true;
                            dlg.WizControl.CurrentStep = "CampSettings";
                            dlg.Title = Translate("Campaign creation...");
                        }
                        else
                        {
                            dlg.RadioAddAct.IsChecked = true;
                            dlg.WizControl.CurrentStep = "ActSettings";
                            dlg.Title = Translate("Activity creation...");
                        }
                    }
                }


                dlg.DataContext = DataContext;

                dlg.Owner = Application.Current.MainWindow;


                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    // that one is needed to prevent exception:
                    /*
Message:       Cannot call StartAt when content generation is in progress.
Source:        PresentationFramework
Target site:   System.Windows.Controls.ItemContainerGenerator.System.Windows.Controls.Primitives.IItemContainerGenerator.StartAt
Stack trace:      at System.Windows.Controls.ItemContainerGenerator.System.Windows.Controls.Primitives.IItemContainerGenerator.StartAt(GeneratorPosition position, GeneratorDirection direction, Boolean allowStartAtRealizedItem)
                     */

                    /*
                     * The correction is not really here... it is in SetSelected stuff. Look at
                     *                     
                    Prompt pr = (Prompt)selected;
                    if (pr.Links != null && pr.Links.Count > 0)
                    {
                        System.Diagnostics.Debug.Assert(pr.Links.Count == 1);

                        PromptLink pl = (PromptLink)pr.Links[0];

                        if (pl.Link is Activity)
                        {
                            SetSelected(pl.Link);
                            TabInActivityPrompts.IsSelected = true;
                            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGInActPrompts.SelectedItem = pl; }));
                        }
                        else if (pl.Link is Campaign)
                        {
                            SetSelected(pl.Link);
                            TabCampPrompts.IsSelected = true;
                            DGCampPrompts.ScrollIntoView(pl);
                            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { DGCampPrompts.ScrollIntoView(pl); DGCampPrompts.SelectedItem = pl; }));
                        }
                    }

                     */
                    if (dlg.Created is UserField || dlg.Created is Prompt)
                    {

                        SetSelected(dlg.Created);
                        
                    }
                    else
                    {
                        SetSelected(dlg.Created);
                    }
                }
                else
                {
                    if(tempHelper.NewQualification!=null)
                        (DataContext as AdminCore).Delete(tempHelper.NewQualification);
                    if(tempHelper.NewField != null)
                        (DataContext as AdminCore).Delete(tempHelper.NewField);
                }
                #endregion
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                #region Delete
                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);

                if (sh.Inbound != null)
                {
                    if (TabInActivityPrompts.IsSelected && DGInActPrompts.SelectedItem != null)
                    {
                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();

                        Prompt todelete = null;

                        if (m_SelectedRelatedActPrompt != null)
                        {
                            todelete = m_SelectedRelatedActPrompt;
                            dlg.ItemsSource = new List<string>() { 
                                string.Format("Selected prompt related to {0}", ((PromptLink)DGInActPrompts.SelectedItem).Prompt.DisplayText),
                                sh.Inbound.TypedDisplayText
                            };
                        }
                        else
                        {
                            dlg.ItemsSource = new List<string>() { 
                                ((PromptLink)DGInActPrompts.SelectedItem).Prompt.TypedDisplayText,
                                sh.Inbound.TypedDisplayText
                            };

                        }


                        dlg.Owner = Application.Current.MainWindow;

                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                PromptLink pr = DGInActPrompts.SelectedItem as PromptLink;

                                if (todelete != null)
                                {
                                    (DataContext as AdminCore).Delete(todelete);
                                }
                                else
                                {
                                    (DataContext as AdminCore).Delete(pr.Prompt);
                                }
                            }
                            else
                            {
                                sh.Inbound.Core.Delete(sh.Inbound);
                            }
                        }

                    }
                    else
                    {
                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = string.Format(Translate("Are you sure you want to delete the activity \"{0}\"?"), sh.Inbound.DisplayText);
                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            sh.Inbound.Core.Delete(sh.Inbound);
                        }
                    }
                }
                else if (sh.Outbound != null)
                {
                    if (SortOrderTab.IsSelected && DGUserSortOrder.SelectedItem!=null)
                    {
                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                        dlg.ItemsSource = new List<string>() { 
                            ((SortField)DGUserSortOrder.SelectedItem).TypedDisplayText,
                            sh.Outbound.TypedDisplayText
                        };

                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                SortField sf = (SortField)DGUserSortOrder.SelectedItem;
                                sh.Outbound.SortFields.Remove(sf);
                            }
                            else
                            {
                                sh.Outbound.Core.Delete(sh.Outbound);
                            }
                        }
                    }
                    else if (FilterTab.IsSelected && DGFilter.SelectedItem!=null)
                    {

                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                        dlg.ItemsSource = new List<string>() { 
                            ((FilterPart)DGFilter.SelectedItem).TypedDisplayText,
                            sh.Outbound.TypedDisplayText
                        };

                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                FilterPart fp = (FilterPart)DGFilter.SelectedItem;
                                sh.Outbound.FilterParts.Remove(fp);
                            }
                            else
                            {
                                sh.Outbound.Core.Delete(sh.Outbound);
                            }
                        }
                    }
                    else
                    {
                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = string.Format(Translate("Are you sure you want to delete the activity \"{0}\"?"), sh.Outbound.DisplayText);
                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            sh.Outbound.Core.Delete(sh.Outbound);
                        }
                    }
                }
                else
                {
                    if (PredefinedTab.IsSelected & DGPredefinedTexts.SelectedItem != null)
                    {
                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();

                        dlg.ItemsSource = new List<string>() { 
                            ((PredefinedText)DGPredefinedTexts.SelectedItem).TypedDisplayText,
                            ((Campaign)MainGrid.SelectedItem).TypedDisplayText
                            };


                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                (DataContext as AdminCore).Delete((PredefinedText)DGPredefinedTexts.SelectedItem);
                            }
                            else
                            {
                                ((Campaign)MainGrid.SelectedItem).Core.Delete((Campaign)MainGrid.SelectedItem);
                            }
                        }
                    }
                    else if (AttachmentsTab.IsSelected & DGAttachments.SelectedItem != null)
                    {
                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();

                        dlg.ItemsSource = new List<string>() { 
                            ((Attachment)DGAttachments.SelectedItem).TypedDisplayText,
                            ((Campaign)MainGrid.SelectedItem).TypedDisplayText
                            };


                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                (DataContext as AdminCore).Delete((Attachment)DGAttachments.SelectedItem);
                            }
                            else
                            {
                                ((Campaign)MainGrid.SelectedItem).Core.Delete((Campaign)MainGrid.SelectedItem);
                            }
                        }
                    }
                    else if (TabCampPrompts.IsSelected && DGCampPrompts.SelectedItem != null)
                    {
                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();

                        Prompt todelete = null;

                        if (m_SelectedRelatedCampPrompt != null)
                        {
                            todelete = m_SelectedRelatedCampPrompt;
                            dlg.ItemsSource = new List<string>() { 
                                string.Format("Selected prompt related to {0}", ((PromptLink)DGCampPrompts.SelectedItem).Prompt.DisplayText),
                                ((Campaign)MainGrid.SelectedItem).TypedDisplayText
                                };
                        }
                        else
                        {
                            dlg.ItemsSource = new List<string>() { 
                                ((PromptLink)DGCampPrompts.SelectedItem).Prompt.TypedDisplayText,
                                ((Campaign)MainGrid.SelectedItem).TypedDisplayText
                                };
                        }


                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                PromptLink pr = DGCampPrompts.SelectedItem as PromptLink;

                                if (todelete != null)
                                {
                                    (DataContext as AdminCore).Delete(todelete);
                                }
                                else
                                {
                                    (DataContext as AdminCore).Delete(pr.Prompt);
                                }
                                
                            }
                            else
                            {
                                ((Campaign)MainGrid.SelectedItem).Core.Delete((Campaign)MainGrid.SelectedItem);
                            }
                        }
                    }
                    else 
                    if (DataTab.IsSelected && DGData.SelectedItem!=null)
                    {

                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                        dlg.ItemsSource = new List<string>() { 
                            ((Field)DGData.SelectedItem).TypedDisplayText,
                            ((Campaign)MainGrid.SelectedItem).TypedDisplayText
                        };

                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                Campaign camp = ((Field)DGData.SelectedItem).Campaign.Target;
                                camp.Delete((Field)DGData.SelectedItem);
                            }
                            else
                            {
                                ((Campaign)MainGrid.SelectedItem).Core.Delete((Campaign)MainGrid.SelectedItem);
                            }
                        }
                    }
                    else if (QualificationsTab.IsSelected && QualifsTree.SelectedItem != null)
                    {

                        DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                        dlg.ItemsSource = new List<string>() { 
                            string.Concat( (QualifsTree.SelectedItem as Qualification).TypedDisplayText, (QualifsTree.SelectedItem as Qualification).Children.Count==0 ? string.Empty: " and all its children" ) ,
                            ((Campaign)MainGrid.SelectedItem).TypedDisplayText
                        };

                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            if (dlg.SelectedIndex == 0)
                            {
                                (MainGrid.SelectedItem as Campaign).RemoveQualification(QualifsTree.SelectedItem as Qualification);
                            }
                            else
                            {                                
                                ((Campaign)MainGrid.SelectedItem).Core.Delete((Campaign)MainGrid.SelectedItem);
                            }
                        }
                    }
                    else
                    {
                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = string.Format(Translate("Are you sure you want to delete the campaign \"{0}\"?"), sh.Campaign.DisplayText);
                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            ((Campaign)MainGrid.SelectedItem).Core.Delete(((Campaign)MainGrid.SelectedItem));
                        }
                    }


                }
                #endregion
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                #region Move up
                if (QualificationsTab.IsSelected)
                {
                    Qualification selection = (Qualification)QualifsTree.SelectedItem;

                    int backup = selection.DisplayOrder;
                    Qualification previous = selection.Previous;
                    selection.DisplayOrder = previous.DisplayOrder;
                    previous.DisplayOrder = backup;
                    previous.ParentQualification.Children.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);

                    List<object> tempList = new List<object>();
                    Qualification temp = selection;
                    tempList.Insert(0, temp);
                    while (!string.IsNullOrEmpty(temp.ParentId))
                    {
                        temp = ((AdminCore)DataContext).GetAdminObject<Qualification>(temp.ParentId);
                        tempList.Insert(0, temp);
                    }
                    tempList.RemoveAt(0);
                    QualifsTree.SelectItem(tempList);
                }
                if (FilterTab.IsSelected && DGFilter.SelectedItem != null)
                {
                    int backup = (DGFilter.SelectedItem as FilterPart).Sequence;
                    FilterPart previous = (DGFilter.SelectedItem as FilterPart).Previous;
                    (DGFilter.SelectedItem as FilterPart).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Activity.Target.FilterParts.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (SortOrderTab.IsSelected && DGUserSortOrder.SelectedItem != null)
                {
                    int backup = (DGUserSortOrder.SelectedItem as SortField).Sequence;
                    SortField previous = (DGUserSortOrder.SelectedItem as SortField).Previous;
                    (DGUserSortOrder.SelectedItem as SortField).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Activity.SortFields.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (PredefinedTab.IsSelected && DGPredefinedTexts.SelectedItem != null)
                {
                    int backup = (DGPredefinedTexts.SelectedItem as PredefinedText).Sequence;
                    PredefinedText previous = (DGPredefinedTexts.SelectedItem as PredefinedText).Previous;
                    (DGPredefinedTexts.SelectedItem as PredefinedText).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Campaign.Target.PredefinedTexts.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (AttachmentsTab.IsSelected && DGAttachments.SelectedItem != null)
                {
                    int backup = (DGAttachments.SelectedItem as Attachment).Sequence;
                    Attachment previous = (DGAttachments.SelectedItem as Attachment).Previous;
                    (DGAttachments.SelectedItem as Attachment).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Campaign.Target.Attachments.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }

                #endregion
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                #region Move down
                if (QualificationsTab.IsSelected)
                {
                    Qualification selection = (Qualification)QualifsTree.SelectedItem;

                    int backup = selection.DisplayOrder;
                    Qualification next = selection.Next;
                    selection.DisplayOrder = next.DisplayOrder;
                    next.DisplayOrder = backup;
                    next.ParentQualification.Children.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);


                    List<object> tempList = new List<object>();
                    Qualification temp = selection;
                    tempList.Insert(0, temp);
                    while (!string.IsNullOrEmpty(temp.ParentId))
                    {
                        temp = ((AdminCore)DataContext).GetAdminObject<Qualification>(temp.ParentId);
                        tempList.Insert(0, temp);
                    }
                    tempList.RemoveAt(0);
                    QualifsTree.SelectItem(tempList);
                } 
                if (FilterTab.IsSelected && DGFilter.SelectedItem != null)
                {
                    int backup = (DGFilter.SelectedItem as FilterPart).Sequence;
                    FilterPart previous = (DGFilter.SelectedItem as FilterPart).Next;
                    (DGFilter.SelectedItem as FilterPart).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Activity.Target.FilterParts.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (SortOrderTab.IsSelected && DGUserSortOrder.SelectedItem != null)
                {
                    int backup = (DGUserSortOrder.SelectedItem as SortField).Sequence;
                    SortField previous = (DGUserSortOrder.SelectedItem as SortField).Next;
                    (DGUserSortOrder.SelectedItem as SortField).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Activity.SortFields.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (PredefinedTab.IsSelected && DGPredefinedTexts.SelectedItem != null)
                {
                    int backup = (DGPredefinedTexts.SelectedItem as PredefinedText).Sequence;
                    PredefinedText previous = (DGPredefinedTexts.SelectedItem as PredefinedText).Next;
                    (DGPredefinedTexts.SelectedItem as PredefinedText).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Campaign.Target.PredefinedTexts.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (AttachmentsTab.IsSelected && DGAttachments.SelectedItem != null)
                {
                    int backup = (DGAttachments.SelectedItem as Attachment).Sequence;
                    Attachment previous = (DGAttachments.SelectedItem as Attachment).Next;
                    (DGAttachments.SelectedItem as Attachment).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.Campaign.Target.Attachments.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }

                #endregion
            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                DlgPrint dlgPrint = new DlgPrint();

                List<string> tempList = new List<string>();

                foreach (Campaign obj in MainGrid.Items)
                {
                    tempList.Add(obj.Id);
                }
                dlgPrint.reportName = "Listing Campaigns";
                dlgPrint.Parameters.Add(string.Empty, string.Join(",", tempList));
                dlgPrint.Owner = Application.Current.MainWindow;
                dlgPrint.ShowDialog();
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                #region Duplicate
                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);


                if (sh.Inbound != null)
                {
                    DuplicateTargetConfirmationDialog dlgConfirm = new DuplicateTargetConfirmationDialog();
                    dlgConfirm.ItemsSource = new List<string>() { 
                            (MainGrid.SelectedItem as Campaign).TypedDisplayText,
                            sh.Inbound.TypedDisplayText
                        };

                    dlgConfirm.Owner = Application.Current.MainWindow;
                    if (dlgConfirm.ShowDialog().GetValueOrDefault())
                    {
                        if (dlgConfirm.SelectedIndex == 0)
                        {
                            // duplicate the campaign
                        }
                        else
                        {
                            // duplicate the inbound activity

                            // we move activity prompts to campaign level
                            while (sh.Inbound.Prompts.Count > 0)
                            {
                                PromptLink pl = sh.Inbound.Prompts[0];
                                Activity act = ((Activity)(pl.Link));
                                pl.Prompt.Description = string.Concat(pl.Prompt.Description, "*");
                                sh.Inbound.Campaign.Target.Prompts.Add(pl.Prompt);
                                sh.Inbound.Prompts.Remove(pl.Prompt);
                            }


                            SetSelected(sh.Inbound.Duplicate(true));
                            return;
                        }
                    }
                }
                else if (sh.Outbound != null)
                {
                    DuplicateTargetConfirmationDialog dlgConfirm = new DuplicateTargetConfirmationDialog();
                    dlgConfirm.ItemsSource = new List<string>() { 
                            (MainGrid.SelectedItem as Campaign).TypedDisplayText,
                            sh.Outbound.TypedDisplayText
                        };

                    dlgConfirm.Owner = Application.Current.MainWindow;
                    if (dlgConfirm.ShowDialog().GetValueOrDefault())
                    {
                        if (dlgConfirm.SelectedIndex == 0)
                        {
                            // duplicate the campaign
                        }
                        else
                        {
                            // duplicate the outbound activity
                            // we move activity prompts to campaign level
                            while (sh.Outbound.Prompts.Count > 0)
                            {
                                PromptLink pl = sh.Outbound.Prompts[0];
                                Activity act = ((Activity)(pl.Link));
                                pl.Prompt.Description = string.Concat(pl.Prompt.Description, "*");
                                sh.Outbound.Campaign.Target.Prompts.Add(pl.Prompt);
                                sh.Outbound.Prompts.Remove(pl.Prompt);
                            }
                            SetSelected(sh.Outbound.Duplicate(true));
                            return;
                        }
                    }
                }
                // duplicate campaign

                Campaign camp = (Campaign)MainGrid.SelectedItem;
                List<string> choices = new List<string>();

                if (camp != null)
                {

                    if (camp.Qualifications.Count > 0)
                    {
                        choices.Add(Translate("Copy qualifications"));
                    }

                    if (camp.Attachments.Count > 0)
                    {
                        choices.Add(Translate("Copy attachments"));
                    }

                    if (camp.PredefinedTexts.Count > 0)
                    {
                        choices.Add(Translate("Copy predefined texts"));
                    }

                    if (camp.InboundActivities.Count > 0)
                    {
                        choices.Add(Translate("Copy inbound activities"));
                    }

                    if (camp.OutboundActivities.Count > 0)
                    {
                        choices.Add(Translate("Copy outbound activities"));
                    }

                    if (camp.UserFields.Count > 0)
                    {
                        choices.Add(Translate("Copy data structure"));
                    }

                    if (!camp.Advanced)
                    {
                        choices.Add(Translate("Copy agents affectations"));
                    }

                    DuplicateOptionsDialog dlgOption = new DuplicateOptionsDialog();
                    dlgOption.ItemsSource = choices;
                    dlgOption.Owner = Application.Current.MainWindow;
                    if (dlgOption.ShowDialog().GetValueOrDefault())
                    {
                        bool dupQual = false;
                        bool dupAtt = false;
                        bool dupPred = false;
                        bool dupIn = false;
                        bool dupOut = false;
                        bool dupStruct = false;
                        bool copyAffectations = false;

                        for (int i = 0; i < choices.Count; i++)
                        {
                            if (dlgOption.SelectedIndexes.Contains(i))
                            {
                                if(choices[i]==Translate("Copy qualifications"))
                                {
                                    dupQual = true;
                                }
                                else if(choices[i]==Translate("Copy attachments"))
                                {
                                    dupAtt = true;
                                }
                                else if(choices[i]==Translate("Copy predefined texts"))
                                {
                                    dupPred = true;
                                }
                                else if(choices[i]==Translate("Copy inbound activities"))
                                {
                                    dupIn = true;
                                }
                                else if(choices[i]==Translate("Copy outbound activities"))
                                {
                                    dupOut = true;
                                }
                                else if(choices[i]==Translate("Copy data structure"))
                                {
                                    dupStruct = true;
                                }
                                else if (choices[i] == Translate("Copy agents affectations"))
                                {
                                    copyAffectations = true;
                                }
                            }
                        }

                        SetSelected(camp.Duplicate(dupQual, dupAtt, dupPred, dupIn, dupOut, dupStruct, copyAffectations));

                    }


                }



                #endregion
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);


                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);


                    SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);

                    if (sh.Inbound != null)
                    {
                        afs.ShowObjectWithContext(toShow, sh.Inbound);
                    }
                    else if (sh.Outbound != null)
                    {
                        afs.ShowObjectWithContext(toShow, sh.Outbound);
                    }
                    else
                    {
                        afs.ShowObjectWithContext(toShow, (AdminObject)MainGrid.SelectedItem, CampTabControl.SelectedItem==null? null :((NixxisTabItem)CampTabControl.SelectedItem).Name);
                    }

                }
            }
            else if (e.Command == AdminFrameSet.DataManagement)
            {
                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
                DlgDataManagement dlg = new DlgDataManagement();
                dlg.DataContext = sh.Campaign;               
                dlg.Owner = Application.Current.MainWindow;
                dlg.ShowDialog();
            }
        }

        private bool IsExpertUser( Campaign camp)
        {
            if (camp != null)
            {
                return camp.CheckEnabled("Campaign.IsExpert");
            }

            if ( (((AdminCore)DataContext).DefaultSettings).CheckEnabled("Campaign.IsExpert"))
                return true;

            return false;

        }


        private bool CanCreateNewCampaign()
        {
            bool? defaultsetting = (((AdminCore)DataContext).DefaultSettings).TypedComputedCreatability("admin+++++++++++++++++++++++++++");
            bool? defaultsetting2 = (((AdminCore)DataContext).DefaultSettings).TypedComputedCreatability("admincampaigns++++++++++++++++++");

            if (defaultsetting.HasValue)
            {
                if (defaultsetting.Value)
                {
                    if (defaultsetting2.HasValue && !defaultsetting2.Value)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (!defaultsetting2.HasValue || !defaultsetting2.Value)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


        }

        private bool CanCreateNewActivities()
        {
            bool? defaultsetting = (((AdminCore)DataContext).DefaultSettings).TypedComputedCreatability("admin+++++++++++++++++++++++++++");
            bool? defaultsetting2 = (((AdminCore)DataContext).DefaultSettings).TypedComputedCreatability("admincampaigns++++++++++++++++++");
            bool? defaultsetting3 = (((AdminCore)DataContext).DefaultSettings).TypedComputedCreatability("adminactivities+++++++++++++++++");

            if (defaultsetting.HasValue)
            {
                if (defaultsetting.Value)
                {
                    if (defaultsetting2.HasValue && !defaultsetting2.Value && defaultsetting3.HasValue && !defaultsetting3.Value)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if ((!defaultsetting2.HasValue || !defaultsetting2.Value) && (!defaultsetting3.HasValue || !defaultsetting3.Value))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {   
            if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);


                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    e.CanExecute = afs.CanShowObject(toShow);

                    e.Handled = true;
                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                if (MainGrid.SelectedItem == null)
                {
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
                
                if (sh.Inbound != null || sh.Outbound!=null )
                {
                    e.CanExecute = CanCreateNewActivities();
                    e.Handled = true;
                    return;
                }

                // duplicate campaign

                Campaign camp = (Campaign)MainGrid.SelectedItem;
                List<string> choices = new List<string>();

                if (camp != null)
                {
                    e.CanExecute = CanCreateNewCampaign();
                    e.Handled = true;
                    return;
                }

                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                SelectionHelper tempHelper = Resources["selectionHelper"] as SelectionHelper;

                if (!IsExpertUser(tempHelper.Campaign) || tempHelper.Campaign == null || !tempHelper.Campaign.Advanced)
                {
                    if (QualificationsTab.IsSelected)
                    {
                        // Campaign and qualification creation...
                        if ( (tempHelper.Campaign==null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (DataTab.IsSelected)
                    {
                        // Campaign and field creation...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (SortOrderTab.IsSelected)
                    {
                        // Campaign and data sort order...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (FilterTab.IsSelected)
                    {
                        // Campaign and data filter...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (TabCampPrompts.IsSelected)
                    {
                        // Campaign and prompts...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (PredefinedTab.IsSelected)
                    {
                        // Campaign and predefined texts...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (AttachmentsTab.IsSelected)
                    {
                        // Campaign and attachments...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (TabInActivityPrompts.IsSelected)
                    {
                        // Campaign and prompts...
                        if ((tempHelper.Campaign == null || tempHelper.Campaign.IsReadOnly) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else
                    {
                        // Campaign creation...
                        if (!CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                }
                else
                {

                    if (QualificationsTab.IsSelected)
                    {
                        // Campaign, activity and qualifications creation...
                        if (tempHelper.Campaign.IsReadOnly && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (DataTab.IsSelected)
                    {
                        // Campaign, activity and field creation...
                        if (tempHelper.Campaign.IsReadOnly && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }
                    }
                    else if (SortOrderTab.IsSelected)
                    {
                        // Campaign, activity and data sort order...
                        if ((tempHelper.Outbound==null || tempHelper.Outbound.IsReadOnly) && (tempHelper.Campaign.IsReadOnly || !CanCreateNewActivities()) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (FilterTab.IsSelected)
                    {
                        // Campaign, activity and data filter...
                        if ((tempHelper.Outbound==null || tempHelper.Outbound.IsReadOnly) && (tempHelper.Campaign.IsReadOnly || !CanCreateNewActivities()) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (TabInActivityPrompts.IsSelected)
                    {
                        // Campaign, activity and prompts...
                        if ((tempHelper.Inbound==null || tempHelper.Inbound.IsReadOnly) && (tempHelper.Campaign.IsReadOnly || !CanCreateNewActivities()) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (TabCampPrompts.IsSelected)
                    {
                        // Campaign, activity and prompts...
                        if (tempHelper.Campaign.IsReadOnly  && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (PredefinedTab.IsSelected)
                    {
                        // Campaign, activity and predefined texts...
                        if (tempHelper.Campaign.IsReadOnly && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else if (AttachmentsTab.IsSelected)
                    {
                        // Campaign, activity and attachments...
                        if (tempHelper.Campaign.IsReadOnly && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                    else
                    {
                        if( (tempHelper.Campaign.IsReadOnly || !CanCreateNewActivities()) && !CanCreateNewCampaign())
                        {
                            e.CanExecute = false;
                            return;
                        }

                    }
                }




                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);

                if (sh.Inbound != null)
                {
                    e.CanExecute = sh.Inbound.IsDeletable;
                }
                else if (sh.Outbound != null)
                {
                    e.CanExecute = sh.Outbound.IsDeletable;
                }
                else
                {
                    e.CanExecute = ((MainGrid.SelectedItem != null) && (((AdminObject)MainGrid.SelectedItem).IsDeletable)) ||
                        MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as Campaign).IsReadOnly &&
                    ((QualificationsTab.IsSelected && QualifsTree.SelectedItem != null && QualifsTree.SelectedItem is Qualification)
                        || (SortOrderTab.IsSelected && DGUserSortOrder.SelectedItem != null)
                        || (PredefinedTab.IsSelected && DGPredefinedTexts.SelectedItem != null)
                        || (AttachmentsTab.IsSelected && DGAttachments.SelectedItem != null)
                        || (FilterTab.IsSelected && DGFilter.SelectedItem != null));
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as Campaign).IsReadOnly &&
                    ((QualificationsTab.IsSelected && QualifsTree.SelectedItem != null && QualifsTree.SelectedItem is Qualification && !(QualifsTree.SelectedItem as Qualification).IsFirst)
                        || (SortOrderTab.IsSelected && DGUserSortOrder.SelectedItem != null && !(DGUserSortOrder.SelectedItem as SortField).Activity.IsReadOnly &&  !(DGUserSortOrder.SelectedItem as SortField).IsFirst)
                        || (PredefinedTab.IsSelected && DGPredefinedTexts.SelectedItem != null && !(DGPredefinedTexts.SelectedItem as PredefinedText).IsFirst)
                        || (AttachmentsTab.IsSelected && DGAttachments.SelectedItem != null && !(DGAttachments.SelectedItem as Attachment).IsFirst)
                        || (FilterTab.IsSelected && DGFilter.SelectedItem != null && !(DGFilter.SelectedItem as FilterPart).Activity.IsReadOnly && !(DGFilter.SelectedItem as FilterPart).IsFirst));

            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as Campaign).IsReadOnly &&
                    ((QualificationsTab.IsSelected && QualifsTree.SelectedItem != null && QualifsTree.SelectedItem is Qualification && !(QualifsTree.SelectedItem as Qualification).IsLast)
                        || (SortOrderTab.IsSelected && DGUserSortOrder.SelectedItem != null && !(DGUserSortOrder.SelectedItem as SortField).Activity.IsReadOnly && !(DGUserSortOrder.SelectedItem as SortField).IsLast)
                        || (PredefinedTab.IsSelected && DGPredefinedTexts.SelectedItem != null && !(DGPredefinedTexts.SelectedItem as PredefinedText).IsLast)
                        || (AttachmentsTab.IsSelected && DGAttachments.SelectedItem != null && !(DGAttachments.SelectedItem as Attachment).IsLast)
                        || (FilterTab.IsSelected && DGFilter.SelectedItem != null && !(DGFilter.SelectedItem as FilterPart).Activity.IsReadOnly && !(DGFilter.SelectedItem as FilterPart).IsLast));
            }
            else if (e.Command == AdminFrameSet.DataManagement)
            {
                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);

                if (sh.Inbound != null)
                {
                    e.CanExecute = (sh.Inbound.Campaign.Target != null && sh.Inbound.Campaign.Target.CheckEnabled("Campaign.DataManage"));
                }
                else if (sh.Outbound != null)
                {
                    e.CanExecute = (sh.Outbound.Campaign.Target!=null && sh.Outbound.Campaign.Target.CheckEnabled("Campaign.DataManage"));
                }
                else
                {
                    e.CanExecute = (MainGrid.SelectedItem != null) && (((AdminObject)MainGrid.SelectedItem).CheckEnabled("Campaign.DataManage"));
                }

            }
            else
            {
                e.CanExecute = (MainGrid.SelectedItem != null);
            }
            e.Handled = true;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            e.Handled = true;
            DataGrid dg = (DataGrid)sender;


            if (dg.Name == "InboundGrid")
            {
                (Resources["selectionHelper"] as SelectionHelper).Inbound = dg.SelectedItem as InboundActivity;
            }
            else if (dg.Name == "OutboundGrid")
            {
                (Resources["selectionHelper"] as SelectionHelper).Outbound = dg.SelectedItem as OutboundActivity;
            }
            else if (dg.Name == "MainGrid")
            {
                (Resources["selectionHelper"] as SelectionHelper).Campaign = dg.SelectedItem as Campaign;
            }


            if (dg.SelectedItem == null)
                return;


            if (dg == MainGrid)
            {                
                // clicked on maingrid, deselect all children except the ones under myself
                Helpers.ApplyToVisualChildren<DataGrid>(MainGrid, DataGrid.SelectedItemProperty, null, (de) => (! Helpers.FindVisualParent<DataGridRow>(de as UIElement).IsSelected) );

                // ensure the detail is collapsed for rows that are not selected
                Helpers.ApplyToVisualChildren<ToggleButton>(MainGrid, ToggleButton.IsCheckedProperty, false, (tb) => ((tb as ToggleButton).Name == "DetailsVisibilityToggle" && !Helpers.FindVisualParent<DataGridRow>(tb as ToggleButton).IsSelected));
            }
            else
            {                
                // clicked on child, deselect other childs, deselect main
                Helpers.ApplyToChildren<DataGrid>(Helpers.FindParent<Grid>(Helpers.FindParent<Grid>(dg)) , DataGrid.SelectedItemProperty, null, (fe) => (fe != dg));
            }            
        }

        private void DataGridCampPrompts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            DataGrid dg = (DataGrid)sender;


            if (dg.Name == "DGCampRelatedPrompts")
            {
                m_SelectedRelatedCampPrompt = dg.SelectedItem as Prompt;
            }

            if (dg.SelectedItem == null)
                return;

            if (dg == DGCampPrompts)
            {
                Helpers.ApplyToVisualChildren<DataGrid>(DGCampPrompts, DataGrid.SelectedItemProperty, null, (de) => (!Helpers.FindVisualParent<DataGridRow>(de as UIElement).IsSelected));
                
                Helpers.ApplyToVisualChildren<ToggleButton>(DGCampPrompts, ToggleButton.IsCheckedProperty, false, (tb) => ((tb as ToggleButton).Name == "DetailsVisibilityToggle" && !Helpers.FindVisualParent<DataGridRow>(tb as ToggleButton).IsSelected));
            }
            else
            {
                Helpers.ApplyToChildren<DataGrid>(Helpers.FindParent<Grid>(Helpers.FindParent<Grid>(dg)), DataGrid.SelectedItemProperty, null, (fe) => (fe != dg));
            }
        }

        private void DataGridActPrompts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            DataGrid dg = (DataGrid)sender;

            if (dg.Name == "DGInActRelatedPrompts")
            {
                m_SelectedRelatedActPrompt = dg.SelectedItem as Prompt;
            }

            if (dg.SelectedItem == null)
                return;

            if (dg == DGInActPrompts)
            {
                Helpers.ApplyToVisualChildren<DataGrid>(DGInActPrompts, DataGrid.SelectedItemProperty, null, (de) => (!Helpers.FindVisualParent<DataGridRow>(de as UIElement).IsSelected));

                Helpers.ApplyToVisualChildren<ToggleButton>(DGInActPrompts, ToggleButton.IsCheckedProperty, false, (tb) => ((tb as ToggleButton).Name == "DetailsVisibilityToggle" && !Helpers.FindVisualParent<DataGridRow>(tb as ToggleButton).IsSelected));
            }
            else
            {
                Helpers.ApplyToChildren<DataGrid>(Helpers.FindParent<Grid>(Helpers.FindParent<Grid>(dg)), DataGrid.SelectedItemProperty, null, (fe) => (fe != dg));
            }
        }

        private void DetailToggle_Checked(object sender, RoutedEventArgs e)
        {
            Helpers.ApplyToVisualChildren<ToggleButton>(Helpers.FindVisualParent<NixxisTreeView>((UIElement) sender), ToggleButton.IsCheckedProperty, false, (dep)=>(!dep.Equals(sender)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dlg = new ConfirmationDialog();            
            dlg.MessageText = Translate("This operation cannot be reverted.\nAre you sure you want to continue?");
            dlg.Owner = Application.Current.MainWindow;
            if(dlg.ShowDialog().GetValueOrDefault())
                (MainGrid.SelectedItem as Campaign).Advanced = true;
        }

        private void VoicePreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject || (((Preprocessor)(e.Item)).MediaType > 0 && (((Preprocessor)(e.Item)).MediaType & MediaType.Voice) == MediaType.Voice));
        }

        private void Scripts_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject ||  ((Preprocessor)(e.Item)).MediaType == MediaType.None);
        }

        private void ChatPreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject || (((Preprocessor)(e.Item)).MediaType > 0 && (((Preprocessor)(e.Item)).MediaType & MediaType.Chat) == MediaType.Chat));

        }

        private void MailPreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject || (((Preprocessor)(e.Item)).MediaType > 0 && (((Preprocessor)(e.Item)).MediaType & MediaType.Mail) == MediaType.Mail));
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            OvfTabItem.IsSelected = true;
        }

        private void ShowTeams_Checked(object sender, RoutedEventArgs e)
        {
            if(TeamsTab.Visibility== System.Windows.Visibility.Visible)
                TeamsTab.IsSelected = true;
        }

        private void DGSpecialDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionHelper2 tempHelper = Resources["selectionHelperInbound"] as SelectionHelper2;

            if (DGSpecialDays.SelectedValue != null)
                InboundPlanning.SelectedValue = null;

            tempHelper.Selection = DGSpecialDays.SelectedValue;
        }

        private void DGOutSpecialDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionHelper2 tempHelper = Resources["selectionHelperOutbound"] as SelectionHelper2;

            if (DGOutSpecialDays.SelectedValue != null)
                OutboundPlanning.SelectedValue = null;

            tempHelper.Selection = DGOutSpecialDays.SelectedValue;
        }


        private void InboundPlanning_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectionHelper2 tempHelper = Resources["selectionHelperInbound"] as SelectionHelper2;

            if (InboundPlanning.SelectedValue != null)
                DGSpecialDays.SelectedIndex = -1;

             tempHelper.Selection = InboundPlanning.SelectedValue;
        }

        private void OutboundPlanning_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectionHelper2 tempHelper = Resources["selectionHelperOutbound"] as SelectionHelper2;

            if (OutboundPlanning.SelectedValue != null)
                DGSpecialDays.SelectedIndex = -1;

            tempHelper.Selection = OutboundPlanning.SelectedValue;
        }


        private void chkSysInboundPlanning_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null && SelectedItem is Campaign && ((Campaign)SelectedItem).SystemInboundActivity != null && ((Campaign)SelectedItem).SystemInboundActivity.Planning!=null && !((Campaign)SelectedItem).SystemInboundActivity.Planning.HasTarget)
                (sender as NixxisDetailedCheckBox).IsDetailChecked = true;

        }

        private void chkInboundPlanning_Checked(object sender, RoutedEventArgs e)
        {

            if((Resources["selectionHelper"] as SelectionHelper).Inbound !=null && !(Resources["selectionHelper"] as SelectionHelper).Inbound.Planning.HasTarget)
                (sender as NixxisDetailedCheckBox).IsDetailChecked = true;
        }


        private void chkSysInboundNoAgent_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null && SelectedItem is Campaign && ((Campaign)SelectedItem).SystemInboundActivity != null && ((Campaign)SelectedItem).SystemInboundActivity.NoAgentActionType == OverflowActions.None)
                (sender as NixxisDetailedCheckBox).IsDetailChecked = true;
        }

        private void chkInboundNoAgent_Checked(object sender, RoutedEventArgs e)
        {
            if ((Resources["selectionHelper"] as SelectionHelper).Inbound != null && (Resources["selectionHelper"] as SelectionHelper).Inbound.NoAgentActionType == OverflowActions.None)
                (sender as NixxisDetailedCheckBox).IsDetailChecked = true;
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

        private void StringOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.String;
        }

        private void IntOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.Integer || ((Field)(e.Item)).DBType == DBTypes.Float;
        }

        private void DateTimeOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.Datetime;
        }

        private void BoolOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.Boolean;
        }

        private void CombineOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (((EnumHelper<CombineOperator>)(e.Item)).EnumValue != CombineOperator.None);
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item != OutActTabControl.DataContext);
        }

        private void ConfigureVoicePreprocessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            Activity act = null;

            if (sh.Activity != null)
                act = sh.Activity;
            else
                act = sh.Campaign.SystemInboundActivity;

            if (string.IsNullOrEmpty(act.Preprocessor.Target.EditorUrl))
                return;

            act.PreprocessorConfiguration = ConfigurePreprocessor(act.Preprocessor.Target.DialogType, act, act.PreprocessorConfiguration);
            

            if (act.PreprocessorConfiguration != null)
            {
                act.PreprocessorConfiguration.saveToTextStorage = ((t) => { act.PreprocessorParams = t; });
                act.PreprocessorConfiguration.Save();
            }
            else
                act.PreprocessorParams = null;

        }

        private void ConfigureVoicePostprocessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            Activity act = null;

            if (sh.Activity != null)
                act = sh.Activity;
            else
                act = sh.Campaign.SystemInboundActivity;

            if (string.IsNullOrEmpty(act.Postprocessor.Target.EditorUrl))
                return;


            act.PostprocessorConfiguration = ConfigurePreprocessor(act.Postprocessor.Target.DialogType, act, act.PostprocessorConfiguration);

            if (act.PostprocessorConfiguration != null)
            {
                act.PostprocessorConfiguration.saveToTextStorage = ((t) => { act.PostprocessorParams = t; });
                act.PostprocessorConfiguration.Save();
            }
            else
                act.PostprocessorParams = null;
        }

        private void ConfigureMailPreprocessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            Activity act = null;

            if (sh.Activity != null)
                act = sh.Activity;
            else
                act = sh.Campaign.SystemEmailActivity;

            if (string.IsNullOrEmpty(act.Preprocessor.Target.EditorUrl))
                return;


            act.PreprocessorConfiguration = ConfigurePreprocessor(act.Preprocessor.Target.DialogType, act, act.PreprocessorConfiguration);


            if (act.PostprocessorConfiguration != null)
            {
                act.PreprocessorConfiguration.saveToTextStorage = ((t) => { act.PreprocessorParams = t; }); 
                act.PreprocessorConfiguration.Save();
            }
            else
                act.PreprocessorParams = null;
        }


        private BasePreprocessorConfig ConfigurePreprocessor(Type tpe, AdminObject act, BasePreprocessorConfig preprocConfig)
        {

            if(tpe == null)
                return null;

            WaitScreen ws = new WaitScreen();
            ws.Owner = Application.Current.MainWindow;
            ws.Show();

            IPreprocessorConfigurationDialog preprocdlg = (IPreprocessorConfigurationDialog)Activator.CreateInstance(tpe);

            ws.Close();

            preprocdlg.Owner = Application.Current.MainWindow;
            preprocdlg.WizControlContext = act;
            preprocdlg.DataContext = DataContext;
            preprocdlg.Config = preprocConfig;

            if (((Window)preprocdlg).ShowDialog().GetValueOrDefault())
            {
                return preprocdlg.Config;
            }
            return preprocConfig;
        }

        private void ConfigureMailPostprocessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            Activity act = null;

            if (sh.Activity != null)
                act = sh.Activity;
            else
                act = sh.Campaign.SystemEmailActivity;

            if (string.IsNullOrEmpty(act.Postprocessor.Target.EditorUrl))
                return;


            act.PostprocessorConfiguration = ConfigurePreprocessor(act.Postprocessor.Target.DialogType, act, act.PostprocessorConfiguration);
            

            if (act.PostprocessorConfiguration != null)
            {
                act.PostprocessorConfiguration.saveToTextStorage = ((t) => { act.PostprocessorParams = t; });
                act.PostprocessorConfiguration.Save();
            }
            else
                act.PostprocessorParams = null;
        }


        private void ConfigureChatPreprocessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            Activity act = null;

            if (sh.Activity != null)
                act = sh.Activity;
            else
                act = sh.Campaign.SystemChatActivity;

            if (string.IsNullOrEmpty(act.Preprocessor.Target.EditorUrl))
                return;

            act.PreprocessorConfiguration = ConfigurePreprocessor(act.Preprocessor.Target.DialogType, act, act.PreprocessorConfiguration);

            if (act.PostprocessorConfiguration != null)
            {
                act.PreprocessorConfiguration.saveToTextStorage = ((t) => { act.PreprocessorParams = t; });
                act.PreprocessorConfiguration.Save();
            }
            else
                act.PreprocessorParams = null;

            ///Newly added
            InboundActivity ia = sh.Inbound;
            IMediaProviderConfigurator mpc = null;

            if (ia == null)
            {
                ia = sh.Campaign.SystemEmailActivity;
            }

            if (ia != null)
            {
                mpc = ia.ProviderConfig;

                if (mpc == null)
                {
                    ChoiceDialog newDlg = new ChoiceDialog();
                    SortedList<string, MediaProvider> choices = new SortedList<string, MediaProvider>();
                    foreach (MediaProvider mp in ((AdminCore)DataContext).MediaProviders)
                    {
                        if (mp.MediaType == MediaType.Mail)
                            choices.Add(mp.Description, mp);
                    }
                    newDlg.ItemsSource = choices.Keys;
                    newDlg.Owner = Application.Current.MainWindow;
                    newDlg.MessageText = Translate("Please specify the mail provider");

                    if (newDlg.ShowDialog().GetValueOrDefault())
                    {
                        MediaProvider mp = choices.Values[newDlg.SelectedIndex];

                        IMediaProviderConfigurator impc = Activator.CreateInstance(Type.GetType(mp.PluginType)) as Nixxis.Client.Admin.IMediaProviderConfigurator;
                        if (impc != null)
                        {
                            impc.InitializeProvider((AdminCore)DataContext, mp);
                            if (impc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                            {
                                ia.ProviderConfigSettings = impc.Config.CloneNode(true) as XmlDocument;
                                ia.Destination = impc.ConfigDescription;

                            }

                        }
                    }

                }
                else
                {
                    if (mpc != null)
                    {
                        if (mpc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                        {
                            ia.ProviderConfigSettings = mpc.Config.CloneNode(true) as XmlDocument;
                            ia.Destination = mpc.ConfigDescription;
                        }
                    }
                }
            }
            ///Newly added
        }

        private void ConfigureChatPostprocessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            Activity act = null;

            if (sh.Activity != null)
                act = sh.Activity;
            else
                act = sh.Campaign.SystemChatActivity;

            if (string.IsNullOrEmpty(act.Postprocessor.Target.EditorUrl))
                return;

            act.PostprocessorConfiguration = ConfigurePreprocessor(act.Postprocessor.Target.DialogType, act, act.PostprocessorConfiguration);
            act.PostprocessorConfiguration.saveToTextStorage = ((t) => { act.PostprocessorParams = t; }); 
            if (act.PostprocessorConfiguration != null)
                act.PostprocessorConfiguration.Save();
            else
                act.PostprocessorParams = null;

        }

        private void AddPromptToOverflowPrompt(object sender, RoutedEventArgs e)
        {
            DlgAddPrompt dlg = new DlgAddPrompt();
            try
            {
                if (cboFilter.SelectedValue != null)
                    dlg.GroupKey = cboFilter.SelectedValue as string;
                else
                    dlg.GroupKey = ((IMainWindow)Application.Current.MainWindow).Core.Agents[(Application.Current.MainWindow as IMainWindow).LoggedIn].GroupKey;
            }
            catch
            {

            }

            dlg.Owner = Application.Current.MainWindow;

            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            if (sh.Activity != null)
                dlg.WizControl.Context = sh.Activity;
            else
                dlg.WizControl.Context = sh.Campaign.SystemInboundActivity;

            dlg.DataContext = DataContext;

            dlg.WizControl.CurrentStep = "Start";

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                (dlg.WizControl.Context as InboundActivity).OverflowPrompt.TargetId = dlg.Created.Id;
            }


        }

        private void chkOutboundPlanning_Checked(object sender, RoutedEventArgs e)
        {
            if (
                (Resources["selectionHelper"] as SelectionHelper).Outbound!=null && 
                !(Resources["selectionHelper"] as SelectionHelper).Outbound.Planning.HasTarget
                )
                (sender as NixxisDetailedCheckBox).IsDetailChecked = true;

        }

        private void chkSysOutboundPlanning_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedItem!=null && ((Campaign)SelectedItem).SystemOutboundActivity !=null && !((Campaign)SelectedItem).SystemOutboundActivity.Planning.HasTarget)
                (sender as NixxisDetailedCheckBox).IsDetailChecked = true;
        }

        public string m_BackupId;

        public void BackupSelection()
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);

            if (sh.Inbound != null)
                m_BackupId = sh.Inbound.Id;
            else if (sh.Outbound != null)
                m_BackupId = sh.Outbound.Id;
            else if (sh.Campaign != null)
                m_BackupId = sh.Campaign.Id;
            else
                m_BackupId = null;

        }

        public void RestoreSelection()
        {
            if (m_BackupId != null)
            {
                SetSelected((DataContext as AdminCore).GetAdminObject(m_BackupId));
            }
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            MainGrid.AddHandler(MouseWheelEvent, new RoutedEventHandler(MyMouseWheelH), true);
        }

        private void MyMouseWheelH(object sender, RoutedEventArgs e)
        {
            MouseWheelEventArgs eargs = (MouseWheelEventArgs)e;

            // this one to ensure the scroll is constant
            double x = (double)eargs.Delta > 0 ? 24 : -24;

            double y = MainScrollViewer.VerticalOffset;

            MainScrollViewer.ScrollToVerticalOffset(y - x);
        }

        private void PreviewAttachment(object sender, RoutedEventArgs e)
        {
            Attachment att = DGAttachments.SelectedItem as Attachment;
            if (att != null)
            {
                try
                {
                    if (att.LocationIsLocal)
                    {
                        if (att.LocalPath == null)
                        {
                            Process.Start(att.UploadLocation);
                        }
                        else
                        {
                            Process.Start(att.LocalPath);
                        }
                    }
                    else
                    {
                        Process.Start(txtAttPath.Text);
                    }
                }
                catch
                {
                }
            }
        }

        private void CollectionViewSource_Filter_1(object sender, FilterEventArgs e)
        {
            if (AdminFrameSet.Settings.IsFullVersion)
                e.Accepted = true;
            else
                e.Accepted = !((Campaign)e.Item).Advanced;
        }

           
        private void ShortCodeUpdate_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                ((Control)sender).ToolTip = "Invalid XML document!";
            }
            else
            {
                ((Control)sender).ToolTip = null;
            }
        }

        private void ConfigureMailProvider_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            InboundActivity ia = sh.Inbound;
            IMediaProviderConfigurator mpc = null;

            if (ia == null)
            {
                ia = sh.Campaign.SystemEmailActivity;
            }

            if (ia != null)
            {
                mpc = ia.ProviderConfig;

                if (mpc == null)
                {
                    ChoiceDialog newDlg = new ChoiceDialog();
                    SortedList<string, MediaProvider> choices = new SortedList<string, MediaProvider>();
                    foreach (MediaProvider mp in ((AdminCore)DataContext).MediaProviders)
                    {
                        if (mp.MediaType == MediaType.Mail)
                            choices.Add(mp.Description, mp);
                    }
                    newDlg.ItemsSource = choices.Keys;
                    newDlg.Owner = Application.Current.MainWindow;
                    newDlg.MessageText = Translate("Please specify the mail provider");

                    if (newDlg.ShowDialog().GetValueOrDefault())
                    {
                        MediaProvider mp = choices.Values[newDlg.SelectedIndex];

                        IMediaProviderConfigurator impc = Activator.CreateInstance(Type.GetType(mp.PluginType)) as Nixxis.Client.Admin.IMediaProviderConfigurator;
                        if (impc != null )
                        {
                            impc.InitializeProvider((AdminCore)DataContext, mp);
                            if (impc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                            {
                                ia.ProviderConfigSettings = impc.Config.CloneNode(true) as XmlDocument;
                                ia.Destination = impc.ConfigDescription;

                            }
                            
                        }
                    }

                }
                else
                {
                    if (mpc != null )
                    {
                        if (mpc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                        {
                            ia.ProviderConfigSettings = mpc.Config.CloneNode(true) as XmlDocument;
                            ia.Destination = mpc.ConfigDescription;
                        }
                    }
                }
            }
        }

        private void ConfigureChatProvider_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            InboundActivity ia = sh.Inbound;
            IMediaProviderConfigurator mpc = null;

            if (ia == null)
            {
                ia = sh.Campaign.SystemEmailActivity;
            }

            if (ia != null)
            {
                mpc = ia.ProviderConfig;

                if (mpc == null)
                {
                    ChoiceDialog newDlg = new ChoiceDialog();
                    SortedList<string, MediaProvider> choices = new SortedList<string, MediaProvider>();
                    foreach (MediaProvider mp in ((AdminCore)DataContext).MediaProviders)
                    {
                        if (mp.MediaType == MediaType.Chat)
                            choices.Add(mp.Description, mp);
                    }
                    newDlg.ItemsSource = choices.Keys;
                    newDlg.Owner = Application.Current.MainWindow;
                    newDlg.MessageText = Translate("Please specify the chat provider");

                    if (newDlg.ShowDialog().GetValueOrDefault())
                    {
                        MediaProvider mp = choices.Values[newDlg.SelectedIndex];

                        try
                        {
                            IMediaProviderConfigurator impc = Activator.CreateInstance(Type.GetType(mp.PluginType)) as Nixxis.Client.Admin.IMediaProviderConfigurator;
                            if (impc != null)
                            {
                                impc.InitializeProvider((AdminCore)DataContext, mp);
                                if (impc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                                {
                                    ia.ProviderConfigSettings = impc.Config.CloneNode(true) as XmlDocument;
                                    ia.Destination = impc.ConfigDescription;

                                }

                            }
                        }
                        catch (ArgumentNullException)
                        {
                            Debug.WriteLine("Argument was null in: ConfigureChatProvider_Click");
                        }
                    }

                }
                else
                {
                    if (mpc != null)
                    {
                        if (mpc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                        {
                            ia.ProviderConfigSettings = mpc.Config.CloneNode(true) as XmlDocument;
                            ia.Destination = mpc.ConfigDescription;
                        }
                    }
                }
            }
        }

        private void EmailMediaProvider_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((MediaProvider)e.Item).MediaType == MediaType.Mail;
        }
        private void BlackListProvider_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is MediaProvider)
                e.Accepted = ((MediaProvider)e.Item).MediaType == MediaType.Voice;
            else
                e.Accepted = true;
        }

        private void ChatMediaProvider_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((MediaProvider)e.Item).MediaType == MediaType.Chat;
        }

        private void DGCampPrompts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
        }

        private void ConfigureBlackListProvider_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            OutboundActivity ia = sh.Outbound;
            IMediaProviderConfigurator mpc = null;

            if (ia == null)
            {
                ia = sh.Campaign.SystemOutboundActivity;
            }

            if (ia != null)
            {
                mpc = ia.BlackListProviderConfig;

                if (mpc == null)
                {
                    ChoiceDialog newDlg = new ChoiceDialog();
                    SortedList<string, MediaProvider> choices = new SortedList<string, MediaProvider>();
                    foreach (MediaProvider mp in ((AdminCore)DataContext).MediaProviders)
                    {
                        if (mp.MediaType == MediaType.Voice)
                            choices.Add(mp.Description, mp);
                    }
                    newDlg.ItemsSource = choices.Keys;
                    newDlg.Owner = Application.Current.MainWindow;
                    newDlg.MessageText = Translate("Please specify the black list provider");

                    if (newDlg.ShowDialog().GetValueOrDefault())
                    {
                        MediaProvider mp = choices.Values[newDlg.SelectedIndex];

                        try
                        {
                            IMediaProviderConfigurator impc = Activator.CreateInstance(Type.GetType(mp.PluginType)) as Nixxis.Client.Admin.IMediaProviderConfigurator;
                            if (impc != null)
                            {
                                impc.InitializeProvider((AdminCore)DataContext, mp);
                                if (impc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                                {
                                    ia.BlackListProvider = impc.Config.CloneNode(true) as XmlDocument;

                                }

                            }
                        }
                        catch (ArgumentNullException)
                        {
                            Debug.WriteLine("Argument was null in: ConfigureBlackListProvider_Click");
                        }
                    }

                }
                else
                {
                    if (mpc != null)
                    {
                        if (mpc.ShowConfigurationDialog(Application.Current.MainWindow).GetValueOrDefault())
                        {
                            ia.BlackListProvider = mpc.Config.CloneNode(true) as XmlDocument;
                        }
                    }
                }
            }
        }

        private void AddPromptToAmdPrompt(object sender, RoutedEventArgs e)
        {
            DlgAddPrompt dlg = new DlgAddPrompt();
            try
            {
                if (cboFilter.SelectedValue != null)
                    dlg.GroupKey = cboFilter.SelectedValue as string;
                else
                    dlg.GroupKey = ((IMainWindow)Application.Current.MainWindow).Core.Agents[(Application.Current.MainWindow as IMainWindow).LoggedIn].GroupKey;
            }
            catch
            {

            }

            dlg.Owner = Application.Current.MainWindow;

            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            if (sh.Activity != null)
                dlg.WizControl.Context = sh.Activity;
            else
                dlg.WizControl.Context = sh.Campaign.SystemOutboundActivity;

            dlg.DataContext = DataContext;

            dlg.WizControl.CurrentStep = "Start";

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                (dlg.WizControl.Context as OutboundActivity).AmdPrompt.TargetId = dlg.Created.Id;
            }

        }

        private void WaitMusicProcessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject || (((Preprocessor)(e.Item)).MediaType > 0 && (((Preprocessor)(e.Item)).MediaType & MediaType.Custom1) == MediaType.Custom1));
        }

        private void ConfigureWaitMusicProcessor_Click(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            InboundActivity act = null;

            if (sh.Activity != null)
                act = sh.Activity as InboundActivity;
            else
                act = sh.Campaign.SystemInboundActivity;

            if (string.IsNullOrEmpty(act.WaitMusicProcessor.Target.EditorUrl))
                return;

            act.WaitMusicProcessorConfiguration = ConfigurePreprocessor(act.WaitMusicProcessor.Target.DialogType, act, act.WaitMusicProcessorConfiguration);


            if (act.WaitMusicProcessorConfiguration != null)
            {
                act.WaitMusicProcessorConfiguration.saveToTextStorage = ((t) => { act.MusicPrompt = t; });
                act.WaitMusicProcessorConfiguration.Save();
            }
            else
                act.MusicPrompt = null;

        }

 
        private void ChooseSalesforceCampaign(object sender, RoutedEventArgs e)
        {
            
            string connection = null;
            string login = null;
            string password = null;
            Campaign camp = null;
            OutboundActivity oa = null;

            try
            {
                SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);

                camp = sh.Campaign;

                if (sh.Outbound != null)
                {
                    oa = sh.Outbound;
                }
                else
                {
                    oa = camp.SystemOutboundActivity;
                }

                
                connection = camp.SalesforceConnection;
                login = camp.SalesforceLogin;
                password = camp.SalesforcePassword;
            }
            catch
            {
            }

            

            if (string.IsNullOrEmpty(connection) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate("Please, provide values for connection, login and password."));
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog();
                return;
            }

            WaitScreen ws = new WaitScreen();
            ws.Text = TranslationContext.Translate("Please wait...");
            ws.Owner = Application.Current.MainWindow;
            ws.Show();


            try
            {
                SFS.SforceService partnerApi = new SFS.SforceService();
                partnerApi.Url = connection;

                SFS.LoginResult loginResult = partnerApi.login(login, password);

                partnerApi.SessionHeaderValue = new SFS.SessionHeader();
                partnerApi.SessionHeaderValue.sessionId = loginResult.sessionId;
                partnerApi.Url = loginResult.serverUrl;

                string query = "SELECT Id, Name FROM Campaign";

                var queryResult = partnerApi.query(query);

                List<string> srcDescriptions = new List<string>();
                List<string> srcIds = new List<string>();
                foreach (var item in queryResult.records.OrderBy( (a) => (a.Any[1].InnerText ) ) )
                {
                    srcIds.Add(item.Any[0].InnerText);
                    if (oa.SalesforceMode == SalesforceCampaignMode.Activities)
                        srcDescriptions.Add("Campaign " + item.Any[1].InnerText);
                    else
                        srcDescriptions.Add( item.Any[1].InnerText);
                }

                if (oa.SalesforceMode == SalesforceCampaignMode.Activities)
                {
                    query = "SELECT Id, Name FROM Account";

                    var queryResult2 = partnerApi.query(query);

                    foreach (var item in queryResult2.records.OrderBy((a) => (a.Any[1].InnerText)))
                    {
                        srcIds.Add(item.Any[0].InnerText);
                        srcDescriptions.Add("Account " + item.Any[1].InnerText);
                    }
                }


                ChoiceDialog newDlg = new ChoiceDialog();

                newDlg.ItemsSource = srcDescriptions;
                newDlg.Owner = Application.Current.MainWindow;
                newDlg.MessageText = Translate("Please select the campaign");

                if (newDlg.ShowDialog().GetValueOrDefault())
                {
                    oa.SalesforceCampaign = srcIds[newDlg.SelectedIndex];
                    oa.SalesforceCampaignDescription = srcDescriptions[newDlg.SelectedIndex];
                }
            }
            catch (Exception ex)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate(string.Format("Please, provide valid values for connection, login and password.\nAn exception has occured: {0}", ex.Message)));
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog();
                return;
            }
            finally
            {
                ws.Close();
            }
        }
 
 



        private void DGRights_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if ( (sender as NixxisDataGrid).SelectedIndex < 0)
                    (sender as NixxisDataGrid).SelectedIndex = 0;
            }
            catch
            {
            }
        }

        private void TakeOwnerShip(object sender, RoutedEventArgs e)
        {
            SelectionHelper sh = (Resources["selectionHelper"] as SelectionHelper);
            if (sh != null)
            {
                if (sh.Activity != null)
                    sh.Activity.TakeOwnerShip();
                else if (sh.Campaign != null)
                    sh.Campaign.TakeOwnerShip();
            }
            
        }

        private void NoQuota_Filter(object sender, FilterEventArgs e)
        {
             EnumHelper<UserFieldMeanings> temp = e.Item as EnumHelper<UserFieldMeanings>;
             e.Accepted = (temp.EnumValue != UserFieldMeanings.Quota);
        }

 
    }

    public class SelectionHelper2 : INotifyPropertyChanged
    {
        private object m_Selection = null;

        public object Selection
        {
            get
            {
                return m_Selection;
            }
            set
            {
                m_Selection = value;
                FirePropertyChanged("Selection");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

    }

    public class TreeStateFalseWhenNull : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectionHelper : INotifyPropertyChanged
    {
        private bool m_ProposeNewSort;
        private bool m_ProposeNewFilter;
        private bool m_ProposeNewPrompt;
        private bool m_ProposeNewPredefinedText;
        private bool m_ProposeNewAttachment;
        private bool m_ProposeNewPromptUnder;
        private bool m_ProposeActivityCreation;
        private bool m_ProposeCampaignCreation;
        private InboundActivity m_Inbound;
        private OutboundActivity m_Outbound;
        private Campaign m_Campaign;
        private AdminObject m_LastSelected;
        private Qualification m_Qualification;
        private Qualification m_NewQualification;
        private UserField m_NewField;
        private PromptLink m_Prompt;

        public bool ProposeActivityCreation
        {
            get
            {
                return m_ProposeActivityCreation;
            }
            set
            {
                m_ProposeActivityCreation = value;
                FirePropertyChanged("ProposeActivityCreation");
            }
        }

        public bool ProposeCampaignCreation
        {
            get
            {
                return m_ProposeCampaignCreation;
            }
            set
            {
                m_ProposeCampaignCreation = value;
                FirePropertyChanged("ProposeCampaignCreation");
            }
        }
        public bool ProposeNewSort
        {
            get
            {
                return m_ProposeNewSort;
            }
            set
            {
                m_ProposeNewSort = value;
                FirePropertyChanged("ProposeNewSort");
            }
        }
        public bool ProposeNewFilter
        {
            get
            {
                return m_ProposeNewFilter;
            }
            set
            {
                m_ProposeNewFilter = value;
                FirePropertyChanged("ProposeNewFilter");
            }
        }
        public bool ProposeNewPrompt
        {
            get
            {
                return m_ProposeNewPrompt;
            }
            set
            {
                m_ProposeNewPrompt = value;
                FirePropertyChanged("ProposeNewPrompt");
            }
        }
        public bool ProposeNewPredefinedText
        {
            get
            {
                return m_ProposeNewPredefinedText;
            }
            set
            {
                m_ProposeNewPredefinedText = value;
                FirePropertyChanged("ProposeNewPredefinedText");
            }
        }
        public bool ProposeNewAttachment
        {
            get
            {
                return m_ProposeNewAttachment;
            }
            set
            {
                m_ProposeNewAttachment = value;
                FirePropertyChanged("ProposeNewAttachment");
            }
        }
        public bool ProposeNewPromptUnder
        {
            get
            {
                return m_ProposeNewPromptUnder;
            }
            set
            {
                m_ProposeNewPromptUnder = value;
                FirePropertyChanged("ProposeNewPromptUnder");
            }
        }
        public Qualification Qualification
        {
            get
            {
                return m_Qualification;
            }
            set
            {
                m_Qualification = value;
                FirePropertyChanged("Qualification");
            }
        }
        public Qualification NewQualification
        {
            get
            {
                return m_NewQualification;
            }
            set
            {
                m_NewQualification = value;
                FirePropertyChanged("NewQualification");
            }
        }
        public InboundActivity Inbound
        {
            get
            {
                return m_Inbound;
            }
            set
            {
                m_Inbound = value;
                FirePropertyChanged("Inbound");
                FirePropertyChanged("Activity");
                LastSelected = m_Inbound;
            }
        }
        public OutboundActivity Outbound
        {
            get
            {
                return m_Outbound;
            }
            set
            {
                m_Outbound = value;
                FirePropertyChanged("Outbound");
                FirePropertyChanged("Activity");
                LastSelected = m_Outbound;
            }
        }
        public Activity Activity
        {
            get
            {
                return (Inbound as Activity ?? Outbound);
                    
            }
        }
        public Campaign Campaign
        {
            get
            {
                return m_Campaign;
            }
            set
            {
                m_Campaign = value;
                FirePropertyChanged("Campaign");
                LastSelected = m_Campaign;
            }
        }
        public AdminObject LastSelected
        {
            get
            {
                return m_LastSelected;
            }
            set
            {
                m_LastSelected = value;
                FirePropertyChanged("LastSelected");
            }
        }
        public UserField NewField
        {
            get
            {
                return m_NewField;
            }
            set
            {
                m_NewField = value;
                FirePropertyChanged("NewField");
            }
        }
        public PromptLink Prompt
        {
            get
            {
                return m_Prompt;
            }
            set
            {
                m_Prompt = value;
                FirePropertyChanged("Prompt");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public void Reset()
        {
            ProposeActivityCreation = false;
            ProposeCampaignCreation = false;
            ProposeNewSort = false;
            m_ProposeNewPrompt = false;
            m_ProposeNewPredefinedText = false;
            m_ProposeNewAttachment = false;
            m_ProposeNewPromptUnder = false;
            m_ProposeNewFilter = false;
            Qualification = null;
            NewQualification = null;
            NewField = null;
            Prompt = null;
        }
    }

    public class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate ActivityTemplate
        {
            get;
            set;
        }
        public DataTemplate CampaignTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Activity)
                return ActivityTemplate;
            else if (item is Campaign)
                return CampaignTemplate;
            return null;
        }
    }

    public class SpecialConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] != null && !DependencyProperty.UnsetValue.Equals(values[i]))
                {
                    if (values[i] is Campaign && "Camp".Equals(parameter))
                        return Visibility.Visible;
                    if (values[i] is InboundActivity && "In".Equals(parameter))
                        return Visibility.Visible;
                    if (values[i] is OutboundActivity && "Out".Equals(parameter))
                        return Visibility.Visible;
                    break;
                }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SortQualificationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is System.Linq.EnumerableQuery<Nixxis.Client.Admin.AdminCheckedLinkList<Nixxis.Client.Admin.Qualification, Nixxis.Client.Admin.QualificationExclusion>.AdminCheckedLinkItem>)
            {
                System.Linq.EnumerableQuery<Nixxis.Client.Admin.AdminCheckedLinkList<Nixxis.Client.Admin.Qualification, Nixxis.Client.Admin.QualificationExclusion>.AdminCheckedLinkItem> qualifs = value as System.Linq.EnumerableQuery<Nixxis.Client.Admin.AdminCheckedLinkList<Nixxis.Client.Admin.Qualification, Nixxis.Client.Admin.QualificationExclusion>.AdminCheckedLinkItem>;
                ICollectionView lcv = CollectionViewSource.GetDefaultView(value);
                lcv.SortDescriptions.Add(new SortDescription("Item.DisplayOrder", ListSortDirection.Ascending));
                return lcv;
            }
            else
            {
                IEnumerable<Qualification> qualifs = value as IEnumerable<Qualification>;
                ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(qualifs);
                lcv.SortDescriptions.Add(new SortDescription("DisplayOrder", ListSortDirection.Ascending));
                return lcv;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WarningConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
                return new SolidColorBrush(Colors.Transparent);
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilterConverter: IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[1] == DependencyProperty.UnsetValue)
                return values[0];

            if (values[0] is CollectionView)
                (values[0] as CollectionView).Filter = ((o) => (o is NullAdminObject || (o is AdminObject && (o as AdminObject).IsDummy ) || (o is OutboundActivity &&  (o as OutboundActivity).Campaign.TargetId == values[1] as string)));

            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
