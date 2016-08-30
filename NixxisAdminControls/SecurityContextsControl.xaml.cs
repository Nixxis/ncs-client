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
    public partial class SecurityContextsControl : UserControl
    {

        private static TranslationContext TranslationContext = new TranslationContext("SecurityContextsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(SecurityContextsControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = selected;
                if (context is AdminObject)
                {
                    AdminObjects.IsSelected = true;
                    DGAdminObjects.UpdateLayout();
                    DGAdminObjects.SelectedItem = ((AdminCheckedLinkList)DGAdminObjects.ItemsSource).FindItem(context);
                }

            }
        }

        public void SetSelected(AdminObject selected)
        {

            if (selected != null)
            {
                if (selected is SecurityContext)
                {
                    SelectedItem = selected;
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


        public SecurityContextsControl()
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Location)));
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
                DlgAddSecurityContext dlg = new DlgAddSecurityContext();

                dlg.Owner = Application.Current.MainWindow;
                dlg.DataContext = DataContext;


                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    SetSelected(dlg.Created);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate("Are you sure you want to delete the security context \"{0}\"?"), ((SecurityContext)MainGrid.SelectedItem).DisplayText);
                dlg.Owner = Application.Current.MainWindow;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    ((SecurityContext)MainGrid.SelectedItem).Core.Delete((SecurityContext)MainGrid.SelectedItem);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {

                    SetSelected(((SecurityContext)MainGrid.SelectedItem).Duplicate());
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                        else if (cm.PlacementTarget is TreeView)
                        {
                            obj = NixxisTreeView.GetClickedData(cm);
                        }
                    }

                    AdminObject toShow = null;

                    if (obj is AdminObject)
                        toShow = (AdminObject)obj;
                    else if (obj is BaseAdminCheckedLinkItem)
                    {
                        toShow = ((BaseAdminCheckedLinkItem)obj).Item;
                    }

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
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                        else if (cm.PlacementTarget is TreeView)
                        {
                            obj = NixxisTreeView.GetClickedData(cm);
                        }
                    }

                    AdminObject toShow = null;

                    if (obj is AdminObject)
                        toShow = (AdminObject)obj;
                    else if (obj is BaseAdminCheckedLinkItem)
                    {
                        toShow = ((BaseAdminCheckedLinkItem)obj).Item;
                    }

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    e.CanExecute = afs.CanShowObject(toShow); ;
                    e.Handled = true;

                }


            }
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation || e.Command==AdminFrameSet.AdminObjectPrintOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                e.CanExecute = ( (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable) );
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

        private void TakeOwnerShip(object sender, RoutedEventArgs e)
        {
            SecuredAdminObject obj = (SecuredAdminObject)MainGrid.SelectedItem;
            obj.TakeOwnerShip();
        }

    }
}
