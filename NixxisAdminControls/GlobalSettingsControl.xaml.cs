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

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for SkillsControl.xaml
    /// </summary>
    public partial class GlobalSettingsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("GlobalSettingsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }


        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(GlobalSettingsControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = selected;
            }
        }

        public void SetSelected(AdminObject selected)
        {

            if (selected != null)
            {
                if (selected is Resource)
                {
                    SelectedItem = selected;
                }
                else if (selected is CallbackRule)
                {
                    SelectedItem = ((CallbackRule)selected).ParentRuleSet;
                    tabCBRules.IsSelected = true;
                    DGCallbackRules.UpdateLayout();
                    DGCallbackRules.SelectedItem = selected;
                }
                else if (selected is NumberingRule)
                {
                    SelectedItem = ((NumberingRule)selected).ParentLocation;
                    tabNumRules.IsSelected = true;
                    DGNumberingRules.UpdateLayout();
                    DGNumberingRules.SelectedItem = selected;
                }
                else if (selected is AmdSettings)
                {
                    SelectedItem = selected;
                }
                else if (selected is Carrier)
                {
                    SelectedItem = selected;
                }
                else if (selected is NumberingPlanEntry)
                {
                    SelectedItem = ((NumberingPlanEntry)selected).Carrier.Target;
                    tabNumberingPlan.IsSelected = true;
                    DGNumberingPlan.UpdateLayout();
                    DGNumberingPlan.SelectedItem = selected;
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


        public GlobalSettingsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(PreprocessorsControl_IsVisibleChanged);

            InitializeComponent();

            AdminFrameSet.Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Settings_PropertyChanged);
        }

        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsFullVersion"))
            {
                AdminCore core = (AdminCore)DataContext;
                core.FirePropertyChanged("GlobalSettings");
                core.FirePropertyChanged("ExtendedGlobalSettings");                
            }
        }

        void PreprocessorsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeysOfType(new Type[]{typeof(Location), typeof(CallbackRuleset), typeof(Resource), typeof(AmdSettings)}));
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
            CollectionViewSource.GetDefaultView(MainGrid.ItemsSource).Filter = (
                (a) 
                =>                
                (
                    ((ComboBox)sender).SelectedValue == null || ((ComboBox)sender).SelectedValue.Equals(((AdminObject)a).GroupKey))
                    &&
                    (
                    (!AdminFrameSet.Settings.IsFullVersion ||
                     ( !(a is Resource) && !(a is CallbackRuleset) && !( a is Location) && !(a is AmdSettings) )
                    )
                )
                
                );
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                if (SelectedItem is Location)
                {
                    // not used anymore!
                    DlgAddNumberingRule dlg = new DlgAddNumberingRule();

                    dlg.Owner = Application.Current.MainWindow;
                    dlg.WizControl.Context = ((AdminCore)DataContext).Locations[0];
                    dlg.DataContext = DataContext;

                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        SetSelected(dlg.Created);
                    }
                }
                if (SelectedItem is CallbackRuleset)
                {
                    DlgAddCallbackRuleset dlg = new DlgAddCallbackRuleset();
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.WizControl.Context = ((AdminCore)DataContext).CallbackRulesets[0];
                    dlg.DataContext = DataContext;

                    dlg.RadioAddCbRule.IsChecked = true;
                    dlg.WizControl.CurrentStep = "CBRuleSettings";
                    dlg.Title = "Callback rule creation...";

                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        SetSelected(dlg.Created);
                    }
                }
                if (SelectedItem is Carrier)
                {
                    DlgAddCarrier dlg = new DlgAddCarrier();
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.DataContext = DataContext;
                    dlg.WizControl.Context = SelectedItem;
                    dlg.WizControl.CurrentStep = "NumberingPlanSettings";
                    dlg.RadioAddNumberingPlan.IsChecked = true;

                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        SetSelected(dlg.Created);
                    }

                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                if (SelectedItem is Location)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the numbering rule \"{0}\"?"), ((NumberingRule)DGNumberingRules.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((NumberingRule)DGNumberingRules.SelectedItem).Core.Delete((NumberingRule)DGNumberingRules.SelectedItem);
                    }
                }
                if (SelectedItem is CallbackRuleset)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the callback rule \"{0}\"?"), ((CallbackRule)DGCallbackRules.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((CallbackRule)DGCallbackRules.SelectedItem).Core.Delete((CallbackRule)DGCallbackRules.SelectedItem);
                    }
                }
                if (SelectedItem is Carrier)
                {
                    DlgRemoveCarrier dlg = new DlgRemoveCarrier();

                    dlg.WizControl.Context = DGNumberingPlan.SelectedItem;

                    dlg.Owner = Application.Current.MainWindow;

                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                    }

                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {

                if (SelectedItem is Location)
                {
                    SetSelected(((NumberingRule)DGNumberingRules.SelectedItem).Duplicate());
                }
                if (SelectedItem is CallbackRuleset)
                {
                    SetSelected(((CallbackRule)DGCallbackRules.SelectedItem).Duplicate());
                }
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    afs.ShowObjectWithContext(toShow, (AdminObject)MainGrid.SelectedItem);

                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                if (SelectedItem is Location)
                {
                    int backup = (DGNumberingRules.SelectedItem as NumberingRule).Sequence;
                    NumberingRule previous = (DGNumberingRules.SelectedItem as NumberingRule).Previous;
                    (DGNumberingRules.SelectedItem as NumberingRule).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.ParentLocation.NumberingRules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (SelectedItem is CallbackRuleset)
                {
                    int backup = (DGCallbackRules.SelectedItem as CallbackRule).Sequence;
                    CallbackRule previous = (DGCallbackRules.SelectedItem as CallbackRule).Previous;
                    (DGCallbackRules.SelectedItem as CallbackRule).Sequence = previous.Sequence;
                    previous.Sequence = backup;
                    previous.ParentRuleSet.Rules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                if (SelectedItem is Location)
                {
                    int backup = (DGNumberingRules.SelectedItem as NumberingRule).Sequence;
                    NumberingRule next = (DGNumberingRules.SelectedItem as NumberingRule).Next;
                    (DGNumberingRules.SelectedItem as NumberingRule).Sequence = next.Sequence;
                    next.Sequence = backup;
                    next.ParentLocation.NumberingRules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
                if (SelectedItem is CallbackRuleset)
                {
                    int backup = (DGCallbackRules.SelectedItem as CallbackRule).Sequence;
                    CallbackRule next = (DGCallbackRules.SelectedItem as CallbackRule).Next;
                    (DGCallbackRules.SelectedItem as CallbackRule).Sequence = next.Sequence;
                    next.Sequence = backup;
                    next.ParentRuleSet.Rules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
                }
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SelectedItem is Location)
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
                else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
                {
                    e.CanExecute = true;
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
                {
                    e.CanExecute = !(SelectedItem as Location).IsReadOnly && (DGNumberingRules.SelectedItem != null) && (((AdminObject)DGNumberingRules.SelectedItem).IsDeletable);
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
                {
                    e.CanExecute = !(SelectedItem as Location).IsReadOnly && (DGNumberingRules.SelectedItem != null && !(DGNumberingRules.SelectedItem as NumberingRule).IsFirst);
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
                {
                    e.CanExecute = !(SelectedItem as Location).IsReadOnly && (DGNumberingRules.SelectedItem != null && !(DGNumberingRules.SelectedItem as NumberingRule).IsLast);
                    e.Handled = true;
                }
                else
                {
                    e.CanExecute = false;
                    e.Handled = true;
                }
            }
            else if (SelectedItem is Carrier)
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
                else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
                {
                    e.CanExecute = true;
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
                {
                    e.CanExecute = (tabNumberingPlan.IsSelected && DGNumberingPlan.SelectedItem != null && (((AdminObject)DGNumberingPlan.SelectedItem).IsDeletable));
                    e.Handled = true;
                }
                else
                {
                    e.CanExecute = false;
                    e.Handled = true;
                }
            }
            else if (SelectedItem is CallbackRuleset)
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
                else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
                {
                    e.CanExecute = true;
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
                {
                    e.CanExecute = !(SelectedItem as CallbackRuleset).IsReadOnly && (tabCBRules.IsSelected && DGCallbackRules.SelectedItem != null && (((AdminObject)DGCallbackRules.SelectedItem).IsDeletable));
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
                {
                    e.CanExecute = !(SelectedItem as CallbackRuleset).IsReadOnly && (tabCBRules.IsSelected && DGCallbackRules.SelectedItem != null && !(DGCallbackRules.SelectedItem as CallbackRule).IsFirst);
                    e.Handled = true;
                }
                else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
                {
                    e.CanExecute = !(SelectedItem as CallbackRuleset).IsReadOnly && (tabCBRules.IsSelected && DGCallbackRules.SelectedItem != null && !(DGCallbackRules.SelectedItem as CallbackRule).IsLast);
                    e.Handled = true;
                }
                else
                {
                    e.CanExecute = false;
                    e.Handled = true;
                }
            }
            else if(SelectedItem is Setting)
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
                e.Handled = true;

            }
            else
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (AdminFrameSet.Settings.IsNotCloudOrSuperUser)
            {
                // super user mode

                if (AdminFrameSet.Settings.IsFullVersion)
                {
                    e.Accepted = (!(e.Item is Resource) && !(e.Item is CallbackRuleset) && !(e.Item is Location) && !(e.Item is AmdSettings) && !(e.Item is Carrier));
                }
                else
                {
                    e.Accepted = true;
                }
            }
            else
            {
                // normal user in cloud

                if (AdminFrameSet.Settings.IsFullVersion)
                {
                    e.Accepted = !(e.Item is Resource) && !(e.Item is Carrier) && !(e.Item is CallbackRuleset) ;
                }
                else
                {
                    e.Accepted = !(e.Item is Resource) && !(e.Item is Carrier);
                }
            }
        }

        private string m_BackupId;

        public void BackupSelection()
        {
            AdminObject agt = MainGrid.SelectedItem as AdminObject;
            if (agt != null)
                m_BackupId = agt.Id;
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

        private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            ndcbQueueLoop.IsChecked = true;
            ndcbOutboundGateway.IsChecked = true;
            ndcbIvrPlayer.IsChecked = true;
            ndcbHoldMusicPlayer.IsChecked = true;
            ndcbAnsweringMachineDetector.IsChecked = true;
            ndcbRinger.IsChecked = true;
            ndcbAnnouncer.IsChecked = true;
            ndcbConferenceBridge.IsChecked = true;
            ndcbRecording.IsChecked = true;
            ndcbRecordingPlayer.IsChecked = true;
            ndcbMonitoring.IsChecked = true;
        }

        private void DGRights_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGRights.SelectedIndex < 0)
                    DGRights.SelectedIndex = 0;
            }
            catch
            {
            }
        }

        private void Scripts_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject || ((Preprocessor)(e.Item)).MediaType == MediaType.None);
        }
    }

    public class SourceChooserConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (DependencyProperty.UnsetValue != values[0] && values[0] is AdminObject &&  ((AdminObject)(values[0])).Id.Equals(parameter))
            {
                return values[0];
            }
            else
            {
                return values[1];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
