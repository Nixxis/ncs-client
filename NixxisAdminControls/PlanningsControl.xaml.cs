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
    public partial class PlanningsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("PlanningsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(PlanningsControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                if (selected is Planning)
                {
                    SelectedItem = selected;
                }
                else if (selected is SpecialDay)
                {
                    SelectedItem = selected.Core.GetAdminObject<Planning>(((SpecialDay)selected).PlanningId);

                    TabSpecialDays.IsSelected = true;
                    DGSpecialDays.UpdateLayout();
                    DGSpecialDays.SelectedValue = selected;
                }
            }
        }

        public void SetSelected(AdminObject selected)
        {

            if (selected != null)
            {
                if (selected is Planning)
                {
                    SelectedItem = selected;
                }
                else if (selected is SpecialDay)
                {
                    TabSpecialDays.IsSelected = true;
                    DGSpecialDays.UpdateLayout();
                    DGSpecialDays.SelectedValue = selected;
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


        public PlanningsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(PlanningsControl_IsVisibleChanged);

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


        void PlanningsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Planning)));
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
                DlgAddPlanning dlg = new DlgAddPlanning();
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

                if (SelectedItem != null && TabSpecialDays.IsSelected)
                {
                    dlg.WizControl.CurrentStep = "Start";
                    dlg.WizControl.Context = SelectedItem;
                }
                else
                {
                    dlg.WizControl.CurrentStep = "AddPlanning";
                }


                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    if (dlg.Created is AdminObject)
                        SetSelected((AdminObject)(dlg.Created));
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {

                // check what to delete...
                if (TabSpecialDays.IsSelected && DGSpecialDays.SelectedItem != null)
                {
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((SpecialDay)DGSpecialDays.SelectedItem).TypedDisplayText,
                        ((Planning)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            ((SpecialDay)DGSpecialDays.SelectedItem).Core.Delete((SpecialDay)DGSpecialDays.SelectedItem);
                        }
                        else
                        {
                            ((Planning)MainGrid.SelectedItem).Core.Delete((Planning)MainGrid.SelectedItem);
                        }
                    }
                }
                else
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the planning \"{0}\"?"), ((Planning)MainGrid.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((Planning)MainGrid.SelectedItem).Core.Delete((Planning)MainGrid.SelectedItem);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                SetSelected(((Planning)MainGrid.SelectedItem).Duplicate());
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

                    e.CanExecute = afs.CanShowObject(toShow); ;
                    e.Handled = true;

                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                e.CanExecute = (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable);
            }
            else
            {
                e.CanExecute = (MainGrid.SelectedItem != null);
            }
            e.Handled = true;
        }

        private void NixxisPlanning_NewTime(object sender, RoutedEventArgs e)
        {
            PlanningTimeSpan se = (DataContext as AdminCore).Create<PlanningTimeSpan>();
            se.PlanningId = (SelectedItem as Planning).Id;
            se.StartTime = (int)((object[])e.OriginalSource)[0];
            se.Closed = !(bool)((object[])e.OriginalSource)[1];
            (SelectedItem as Planning).TimeSpans.Add(se);
        }

        private void NixxisPlanning_RemoveTime(object sender, RoutedEventArgs e)
        {
            ((PlanningTimeSpan)e.OriginalSource).Core.Delete((PlanningTimeSpan)e.OriginalSource);
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
