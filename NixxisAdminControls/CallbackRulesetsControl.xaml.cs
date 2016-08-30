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
    public partial class CallbackRulesetsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("CallbackRulesetsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }


        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CallbackRulesetsControl));

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
                if (selected is CallbackRuleset)
                {
                    SelectedItem = selected;
                }
                else if (selected is CallbackRule)
                {
                    tabRules.IsSelected = true;
                    DGCallbackRules.UpdateLayout();
                    DGCallbackRules.SelectedItem = selected;
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


        public CallbackRulesetsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(PreprocessorsControl_IsVisibleChanged);

            InitializeComponent();
            ((CollectionViewSource)(FindResource("List"))).Filter += new FilterEventHandler(List_Filter);
        }

        private bool BaseFilter(AdminObject obj, string groupkey)
        {
            return (!obj.IsSystem && !obj.IsDummy && !obj.IsPartial) && (groupkey == null || groupkey.Equals(obj.GroupKey));
        }


        void List_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = BaseFilter(e.Item as AdminObject, null);
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(CallbackRuleset)));
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
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                DlgAddCallbackRuleset dlg = new DlgAddCallbackRuleset();
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
                dlg.DataContext = DataContext;

                if (tabRules.IsSelected)
                {
                    dlg.WizControl.Context = SelectedItem;
                    dlg.WizControl.CurrentStep = "StartChoice";
                }
                else
                {
                    dlg.WizControl.CurrentStep = "Start";                    
                }

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    SetSelected(dlg.Created);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {

                if (((MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable))
                    && (DGCallbackRules.SelectedItem != null && tabRules.IsSelected))
                {
                    // ask what to do
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((CallbackRule)DGCallbackRules.SelectedItem).TypedDisplayText,
                        ((CallbackRuleset)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            ((CallbackRule)DGCallbackRules.SelectedItem).Core.Delete((CallbackRule)DGCallbackRules.SelectedItem);
                        }
                        else
                        {
                            ((CallbackRuleset)MainGrid.SelectedItem).Core.Delete((CallbackRuleset)MainGrid.SelectedItem);
                        }
                    }

                }
                else if (DGCallbackRules.SelectedItem != null && tabRules.IsSelected)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the rule \"{0}\"?"), ((CallbackRule)DGCallbackRules.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((CallbackRule)DGCallbackRules.SelectedItem).Core.Delete((CallbackRule)DGCallbackRules.SelectedItem);
                    }
                }
                else
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the callback ruleset \"{0}\"?"), ((CallbackRuleset)MainGrid.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((CallbackRuleset)MainGrid.SelectedItem).Core.Delete((CallbackRuleset)MainGrid.SelectedItem);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                int backup = (DGCallbackRules.SelectedItem as CallbackRule).Sequence;
                CallbackRule previous = (DGCallbackRules.SelectedItem as CallbackRule).Previous;
                (DGCallbackRules.SelectedItem as CallbackRule).Sequence = previous.Sequence;
                previous.Sequence = backup;
                previous.ParentRuleSet.Rules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                int backup = (DGCallbackRules.SelectedItem as CallbackRule).Sequence;
                CallbackRule next = (DGCallbackRules.SelectedItem as CallbackRule).Next;
                (DGCallbackRules.SelectedItem as CallbackRule).Sequence = next.Sequence;
                next.Sequence = backup;
                next.ParentRuleSet.Rules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {

                if ( MainGrid.SelectedItem != null  && DGCallbackRules.SelectedItem != null && tabRules.IsSelected)
                {
                    // ask what to do
                    DuplicateTargetConfirmationDialog dlg = new DuplicateTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((CallbackRule)DGCallbackRules.SelectedItem).TypedDisplayText,
                        ((CallbackRuleset)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            SetSelected( ((CallbackRule)DGCallbackRules.SelectedItem).Duplicate());
                        }
                        else
                        {
                            SetSelected(((CallbackRuleset)MainGrid.SelectedItem).Duplicate());
                        }
                    }

                }
                else if (DGCallbackRules.SelectedItem != null && tabRules.IsSelected)
                {
                    SetSelected( ((CallbackRule)DGCallbackRules.SelectedItem).Duplicate());
                }
                else
                {
                    SetSelected(((CallbackRuleset)MainGrid.SelectedItem).Duplicate());
                }
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    afs.ShowObjectWithContext(toShow, (AdminObject) MainGrid.SelectedItem);
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
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                e.CanExecute = ((MainGrid.SelectedItem != null) && !(MainGrid.SelectedItem as CallbackRuleset).IsReadOnly && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable))
                            || (DGCallbackRules.SelectedItem != null && tabRules.IsVisible && (MainGrid.SelectedItem != null) && !(MainGrid.SelectedItem as CallbackRuleset).IsReadOnly);

            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as CallbackRuleset).IsReadOnly && (tabRules.IsSelected && DGCallbackRules.SelectedItem != null && !(DGCallbackRules.SelectedItem as CallbackRule).IsFirst);
                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as CallbackRuleset).IsReadOnly && (tabRules.IsSelected && DGCallbackRules.SelectedItem != null && !(DGCallbackRules.SelectedItem as CallbackRule).IsLast);
                e.Handled = true;
            }
            else
            {
                e.CanExecute = (MainGrid.SelectedItem != null);
            }
            e.Handled = true;
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
    }
}
