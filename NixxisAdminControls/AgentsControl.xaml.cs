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
using Nixxis.Client.Admin;
using Nixxis.Client.Controls;
using System.ComponentModel;
using System.Diagnostics;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for AgentsControl.xaml
    /// </summary>
    public partial class AgentsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("AgentsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }


        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AgentsControl));
        

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = selected;
                if (context is Skill)
                {
                    Skills.IsSelected = true;
                    DGSkills.UpdateLayout();
                    DGSkills.SelectedItem = ((AdminCheckedLinkList)DGSkills.ItemsSource).FindItem(context);
                }
                else if (context is Language)
                {
                    Languages.IsSelected = true;
                    DGLanguages.UpdateLayout();
                    DGLanguages.SelectedItem = ((AdminCheckedLinkList)DGLanguages.ItemsSource).FindItem(context);
                }
                else if (context is Team)
                {
                    Affectations.IsSelected = true;
                    DGAffectations.UpdateLayout();
                    DGAffectations.SelectedItem = ((AdminCheckedLinkList)DGAffectations.ItemsSource).FindItem(context);
                }
                else if (context is Role)
                {
                    Roles.IsSelected = true;
                    DGRoles.UpdateLayout();
                    DGRoles.SelectedItem = ((AdminCheckedLinkList)DGRoles.ItemsSource).FindItem(context);
                }
                else if (context is Campaign)
                {
                    Affectations.IsSelected = true;
                    DGAffectations.UpdateLayout();
                    DGAffectations.SelectedItem = ((AdminCheckedLinkList)DGAffectations.ItemsSource).FindItem(((Campaign)context).SystemTeam);
                }
            }
        }

        public void SetSelected(AdminObject selected)
        {
            if (selected != null)
            {
                if (selected is Agent)
                {
                    SelectedItem = selected;
                }
                else if (selected is AgentSkill)
                {
                    Skills.IsSelected = true;
                    SelectedItem = ((AgentSkill)(selected)).Agent;

                    DGSkills.UpdateLayout();

                    if (DGSkills.ItemsSource is AdminCheckedLinkList)
                    {
                        DGSkills.SelectedItem = ((AdminCheckedLinkList)DGSkills.ItemsSource).FindLink(selected);
                    }
                    else
                    {
                        DGSkills.SelectedItem = selected;
                    }
                }
                else if (selected is AgentLanguage)
                {
                    Languages.IsSelected = true;
                    SelectedItem = ((AgentLanguage)(selected)).Agent;

                    DGLanguages.UpdateLayout();
                    if (DGLanguages.ItemsSource is AdminCheckedLinkList)
                    {
                        DGLanguages.SelectedItem = ((AdminCheckedLinkList)DGLanguages.ItemsSource).FindLink(selected);
                    }
                    else
                    {
                        DGLanguages.SelectedItem = selected;
                    }
                }
                else if (selected is AgentTeam)
                {
                    Affectations.IsSelected = true;
                    SelectedItem = ((AgentTeam)(selected)).Agent;
                    DGAffectations.UpdateLayout();

                    if (DGAffectations.ItemsSource is AdminCheckedLinkList)
                    {
                        DGAffectations.SelectedItem = ((AdminCheckedLinkList)DGAffectations.ItemsSource).FindLink(selected);
                    }
                    else
                    {
                        DGAffectations.SelectedItem = selected;
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

        public AgentsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(AgentsControl_IsVisibleChanged);
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

        void AgentsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)sender).IsVisible)
            {
                NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                        NixxisGrid.SetPanelCommand.Execute(nep);
                }
                nep = FindResource("DetailsPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                        NixxisGrid.SetPanelCommand.Execute(nep);
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Agent)));
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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((Agent)MainGrid.SelectedItem).PassKey = (DataContext as AdminCore).LoginEncrypter.EncryptPassword(((PasswordBox)sender).Password, LoginEncryption.Purpose.Storage);
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NixxisDataGrid ndg = MainGrid;

            if (e.Parameter != null)
            {
                ndg = e.Parameter as NixxisDataGrid;
            }

            if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                if (ndg == lstViewRestrictions)
                {
                    DlgAddAgent dlg = new DlgAddAgent();

                    dlg.Agent = MainGrid.SelectedItem as Agent;

                    dlg.CreateViewRestriction = true;


                    dlg.Owner = Application.Current.MainWindow;
                    dlg.DataContext = DataContext;

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

                    dlg.radioCreateViewRestriction.IsChecked = true;
                    dlg.WizControl.CurrentStep = "ViewRestriction";
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        Action action = () =>                        
                        {
                            lstViewRestrictions.SelectedItem = dlg.Created;
                        };
                        Dispatcher.BeginInvoke(action, System.Windows.Threading.DispatcherPriority.Background, null);
                    }
                }
                else
                {
                    DlgAddAgent dlg = new DlgAddAgent();

                    if (ndg.SelectedItem != null)
                        dlg.Agent = ndg.SelectedItem as Agent;

                    dlg.CreateViewRestriction = (lstViewRestrictions.IsVisible && ndg.SelectedItem != null);

                    dlg.Owner = Application.Current.MainWindow;
                    dlg.DataContext = DataContext;

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


                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.Created is Agent)
                            SetSelected(dlg.Created);
                        else
                            {
                                Action action = () =>                        
                                {
                                    lstViewRestrictions.SelectedItem = dlg.Created;
                                };
                                Dispatcher.BeginInvoke(action, System.Windows.Threading.DispatcherPriority.Background, null);
                            }
                    }
                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {                
                if (ndg.SelectedItem is Agent)
                {
                    if (lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null)
                    {

                        DeleteTargetConfirmationDialog dlgConfirm = new DeleteTargetConfirmationDialog();
                        dlgConfirm.ItemsSource = new List<string>() { 
                            (MainGrid.SelectedItem as Agent).TypedDisplayText,
                            (lstViewRestrictions.SelectedItem as ViewRestriction).DisplayText
                        };

                        dlgConfirm.Owner = Application.Current.MainWindow;
                        if (dlgConfirm.ShowDialog().GetValueOrDefault())
                        {
                            if (dlgConfirm.SelectedIndex == 0)
                            {
                                ((Agent)MainGrid.SelectedItem).Core.Delete((Agent)MainGrid.SelectedItem);
                            }
                            else
                            {
                                ((ViewRestriction)lstViewRestrictions.SelectedItem).Core.Delete((ViewRestriction)lstViewRestrictions.SelectedItem);
                            }
                        }
                    }
                    else
                    {
                        ConfirmationDialog dlg = new ConfirmationDialog();
                        dlg.MessageText = string.Format(Translate( "Are you sure you want to delete the agent \"{0}\"?"), ((Agent)ndg.SelectedItem).DisplayText);
                        dlg.Owner = Application.Current.MainWindow;
                        if (dlg.ShowDialog().GetValueOrDefault())
                        {
                            ((Agent)ndg.SelectedItem).Core.Delete(((Agent)ndg.SelectedItem));
                        }
                    }
                }
                else if(ndg.SelectedItem is ViewRestriction)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate( "Are you sure you want to delete the restriction \"{0}\"?"), ((ViewRestriction)ndg.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (ndg.ItemsSource is AdminObjectList)
                        {
                            ((ViewRestriction)ndg.SelectedItem).Core.Delete((ViewRestriction)ndg.SelectedItem);
                        }

                    }
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                DlgPrint dlgPrint = new DlgPrint();

                List<string> tempList = new List<string>();

                foreach (Agent obj in MainGrid.Items)
                {
                    tempList.Add(obj.Id);
                }
                dlgPrint.reportName = "Listing Agents";
                dlgPrint.Parameters.Add(string.Empty, string.Join(",", tempList));
                dlgPrint.Owner = Application.Current.MainWindow;
                dlgPrint.ShowDialog();
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                if (ndg.SelectedItem is Agent)
                {
                    if (lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null)
                    {
                        DuplicateTargetConfirmationDialog dlgConfirm = new DuplicateTargetConfirmationDialog();
                        dlgConfirm.ItemsSource = new List<string>() { 
                            (MainGrid.SelectedItem as Agent).TypedDisplayText,
                            (lstViewRestrictions.SelectedItem as ViewRestriction).DisplayText
                        };

                        dlgConfirm.Owner = Application.Current.MainWindow;
                        if (dlgConfirm.ShowDialog().GetValueOrDefault())
                        {
                            if (dlgConfirm.SelectedIndex == 0)
                            {
                                // duplicate the agent
                            }
                            else
                            {
                                // duplicate the viewrestriction...
                                lstViewRestrictions.SelectedItem = ((ViewRestriction)(lstViewRestrictions.SelectedItem)).Duplicate();
                                return;
                            }
                        }
                    }

                    SetSelected(((Agent)ndg.SelectedItem).Duplicate());
                }
                else if (ndg.SelectedItem is ViewRestriction)
                {
                    // should not happen
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                int backup = (lstViewRestrictions.SelectedItem as ViewRestriction).Precedence;
                ViewRestriction next = (lstViewRestrictions.SelectedItem as ViewRestriction).Next;
                (lstViewRestrictions.SelectedItem as ViewRestriction).Precedence = next.Precedence;
                next.Precedence = backup;
                next.Agent.ViewRestrictions.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);

            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                int backup = (lstViewRestrictions.SelectedItem as ViewRestriction).Precedence;
                ViewRestriction next = (lstViewRestrictions.SelectedItem as ViewRestriction).Previous;
                (lstViewRestrictions.SelectedItem as ViewRestriction).Precedence = next.Precedence;
                next.Precedence = backup;
                next.Agent.ViewRestrictions.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    afs.ShowObjectWithContext(toShow, (Agent)MainGrid.SelectedItem);
                }
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            NixxisDataGrid ndg = MainGrid;

            if (e.Parameter != null)
            {
                ndg = e.Parameter as NixxisDataGrid;
            }

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
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation || e.Command==AdminFrameSet.AdminObjectPrintOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                if (ndg.SelectedItem != null && ndg.SelectedItem is ViewRestriction)
                {
                    e.CanExecute = ndg.SelectedItem != null && !(ndg.SelectedItem as ViewRestriction).Agent.IsReadOnly && (lstViewRestrictions != null && lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null && !(lstViewRestrictions.SelectedItem as ViewRestriction).IsLast);
                }
                else
                {
                    e.CanExecute = ndg.SelectedItem != null && !(ndg.SelectedItem as Agent).IsReadOnly && (lstViewRestrictions != null && lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null && !(lstViewRestrictions.SelectedItem as ViewRestriction).IsLast);
                }
                e.Handled = true;
                return;
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                if (ndg.SelectedItem != null && ndg.SelectedItem is ViewRestriction)
                {
                    e.CanExecute = ndg.SelectedItem != null && !(ndg.SelectedItem as ViewRestriction).Agent.IsReadOnly && (lstViewRestrictions != null && lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null && !(lstViewRestrictions.SelectedItem as ViewRestriction).IsFirst);
                }
                else
                {
                    e.CanExecute = ndg.SelectedItem != null && !(ndg.SelectedItem as Agent).IsReadOnly && (lstViewRestrictions != null && lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null && !(lstViewRestrictions.SelectedItem as ViewRestriction).IsFirst);
                }
                e.Handled = true;
                return;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {

                if (ndg.SelectedItem != null && ndg.SelectedItem is ViewRestriction)
                {
                    e.CanExecute =
                        (lstViewRestrictions != null && ndg.SelectedItem != null && !(ndg.SelectedItem as ViewRestriction).Agent.IsReadOnly && lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null)
                        || ((MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable));
                }
                else
                {
                    e.CanExecute =
                        (lstViewRestrictions != null && ndg.SelectedItem != null && !(ndg.SelectedItem as Agent).IsReadOnly && lstViewRestrictions.IsVisible && lstViewRestrictions.SelectedItem != null)
                        || ((MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable));
                }
                e.Handled = true;
                return;

            }
            else
            {
                e.CanExecute = (ndg.SelectedItem != null);
            }

            e.Handled = true;
        }

        private void NixxisDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void NixxisDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (((ComboBox)sender).Visibility == System.Windows.Visibility.Visible)
                ((ComboBox)sender).IsDropDownOpen = true;
        }

        private void AccountUpdate_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                ((Control)sender).ToolTip = "Account is already in use";
            }
            else
            {
                ((Control)sender).ToolTip = null;
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

    }

    public class AdminCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ViewRestriction vr = (ViewRestriction)value;

            if (targetType == typeof(Visibility))
            {

                switch (vr.TargetType)
                {
                    case ViewRestrictionTargetType.Campaign:
                        return Visibility.Visible;
                        break;
                    case ViewRestrictionTargetType.Inbound:
                        break;
                    case ViewRestrictionTargetType.Outbound:
                        break;
                    case ViewRestrictionTargetType.Mail:
                        break;
                    case ViewRestrictionTargetType.Chat:
                        break;
                    case ViewRestrictionTargetType.Queue:
                        return Visibility.Visible;
                    case ViewRestrictionTargetType.Team:
                        return Visibility.Visible;
                    case ViewRestrictionTargetType.Agent:
                        return Visibility.Visible;
                    default:
                        break;
                }
                return Visibility.Collapsed;
            }
            else
            {
                switch (vr.TargetType)
                {
                    case ViewRestrictionTargetType.Campaign:
                        return vr.Core.Campaigns;
                    case ViewRestrictionTargetType.Inbound:
                        break;
                    case ViewRestrictionTargetType.Outbound:
                        break;
                    case ViewRestrictionTargetType.Mail:
                        break;
                    case ViewRestrictionTargetType.Chat:
                        break;
                    case ViewRestrictionTargetType.Queue:
                        return vr.Core.Queues;
                    case ViewRestrictionTargetType.Team:
                        return vr.Core.Teams;
                    case ViewRestrictionTargetType.Agent:
                        return vr.Core.Agents;
                    default:
                        break;
                }
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
