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
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for SkillsControl.xaml
    /// </summary>
    public partial class AppointmentsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("AppointmentsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AppointmentsControl));

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
                if (selected is AppointmentsContext)
                {
                    SelectedItem = selected;
                }
                else if (selected is AppointmentsArea)
                {
                    tabAreas.IsSelected = true;
                    DGAreas.UpdateLayout();
                    DGAreas.SelectedItem = selected;
                }
                else if (selected is AppointmentsMember)
                {
                    tabMembers.IsSelected = true;
                    DGMembers.UpdateLayout();
                    DGMembers.SelectedItem = selected;
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

        public AppointmentsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(QueuesControl_IsVisibleChanged);

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



        void QueuesControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(AppointmentsContext)));
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
                DlgAddAppointmentContext dlg = new DlgAddAppointmentContext();
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
                if (tabAreas.IsSelected || tabMembers.IsSelected)
                {
                    dlg.WizControl.Context = SelectedItem;
                    dlg.WizControl.CurrentStep = "StartChoice";

                    if (!tabAreas.IsSelected)
                        dlg.RadioAddArea.Visibility = System.Windows.Visibility.Collapsed;

                    if (!tabMembers.IsSelected)
                        dlg.RadioAddMember.Visibility = System.Windows.Visibility.Collapsed;
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
                if (tabAreas.IsSelected && DGAreas.SelectedItem != null)
                {
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((AppointmentsArea)DGAreas.SelectedItem).TypedDisplayText,
                        ((AppointmentsContext)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            ((AppointmentsArea)DGAreas.SelectedItem).Core.Delete((AppointmentsArea)DGAreas.SelectedItem);
                        }
                        else
                        {
                            ((AppointmentsContext)MainGrid.SelectedItem).Core.Delete((AppointmentsContext)MainGrid.SelectedItem);
                        }
                    }
                }
                else if (tabMembers.IsSelected && DGMembers.SelectedItem != null)
                {
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((AppointmentsMember)DGMembers.SelectedItem).TypedDisplayText,
                        ((AppointmentsContext)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            ((AppointmentsMember)DGMembers.SelectedItem).Core.Delete((AppointmentsMember)DGMembers.SelectedItem);

                        }
                        else
                        {
                            ((AppointmentsContext)MainGrid.SelectedItem).Core.Delete((AppointmentsContext)MainGrid.SelectedItem);
                        }
                    }
                }
                else
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the appointment context \"{0}\"?"), ((AppointmentsContext)MainGrid.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((AppointmentsContext)MainGrid.SelectedItem).Core.Delete((AppointmentsContext)MainGrid.SelectedItem);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                int backup = (DGAreas.SelectedItem as AppointmentsArea).Sequence;
                AppointmentsArea previous = (DGAreas.SelectedItem as AppointmentsArea).Previous;
                (DGAreas.SelectedItem as AppointmentsArea).Sequence = previous.Sequence;
                previous.Sequence = backup;
                previous.ParentContext.Areas.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                int backup = (DGAreas.SelectedItem as AppointmentsArea).Sequence;
                AppointmentsArea next = (DGAreas.SelectedItem as AppointmentsArea).Next;
                (DGAreas.SelectedItem as AppointmentsArea).Sequence = next.Sequence;
                next.Sequence = backup;
                next.ParentContext.Areas.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                SetSelected(((AppointmentsContext)MainGrid.SelectedItem).Duplicate());
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
                e.CanExecute = (MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as AppointmentsContext).IsReadOnly) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable);
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem  as AppointmentsContext).IsReadOnly && (tabAreas.IsSelected && DGAreas.SelectedItem != null && !(DGAreas.SelectedItem as AppointmentsArea).IsFirst);
                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as AppointmentsContext).IsReadOnly && (tabAreas.IsSelected && DGAreas.SelectedItem != null && !(DGAreas.SelectedItem as AppointmentsArea).IsLast);
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
