﻿using System;
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
    public partial class RolesControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("RolesControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(RolesControl));


        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                if (selected is Role)
                {
                    SelectedItem = selected;
                    if (context is Agent)
                    {
                        Agents.IsSelected = true;
                        DGAgents.UpdateLayout();
                        DGAgents.SelectedItem = ((AdminCheckedLinkList)DGAgents.ItemsSource).FindItem(context);
                    }
                }
                else if (selected is ObjectSecurity)
                {
                    ObjectSecurity os = (ObjectSecurity) selected;
                    SelectedItem = os.Core.Roles[os.RoleId];                    
                }
                else if (selected is ObjectSecurityHelper)
                {
                    ObjectSecurityHelper os = (ObjectSecurityHelper)selected;
                    SelectedItem = os.Role.Target;
                }
                else 
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void SetSelected(AdminObject selected)
        {

            if (selected != null)
            {
                if (selected is Role)
                {
                    SelectedItem = selected;
                }
                else if (selected is RoleMember)
                {
                    Agents.IsSelected = true;
                    SelectedItem = ((RoleMember)(selected)).Role;
                    DGAgents.UpdateLayout();
                    if (DGAgents.ItemsSource is AdminCheckedLinkList)
                    {
                        DGAgents.SelectedItem = ((AdminCheckedLinkList)DGAgents.ItemsSource).FindLink(selected);
                    }
                    else
                    {
                        DGAgents.SelectedItem = selected;
                    }

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


        public RolesControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(SkillsControl_IsVisibleChanged);

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


        void SkillsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Role)));
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
                DlgAddRole dlg = new DlgAddRole();
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

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    SetSelected(dlg.Created);
                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate("Are you sure you want to delete the role \"{0}\"?"), ((Role)MainGrid.SelectedItem).DisplayText);
                dlg.Owner = Application.Current.MainWindow;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    ((Role)MainGrid.SelectedItem).Core.Delete((Role)MainGrid.SelectedItem);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                DlgPrint dlgPrint = new DlgPrint();

                List<string> tempList = new List<string>();

                foreach (Role obj in MainGrid.Items)
                {
                    tempList.Add(obj.Id);
                }
                dlgPrint.reportName = "Listing Roles";
                dlgPrint.Parameters.Add(string.Empty, string.Join(",", tempList));
                dlgPrint.Owner = Application.Current.MainWindow;
                dlgPrint.ShowDialog();
            }

            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                Role duplicateSource = (Role)MainGrid.SelectedItem;
                if (duplicateSource != null)
                {
                    SetSelected(duplicateSource.Duplicate());
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
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation || e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                e.CanExecute = (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner && (((AdminObject)MainGrid.SelectedItem).IsDeletable));
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

        private void DGAdminObjects_SourceUpdated(object sender, DataTransferEventArgs e)
        {
        }
        
    }
}
