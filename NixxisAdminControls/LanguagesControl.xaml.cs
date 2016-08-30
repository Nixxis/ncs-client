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
using System.Reflection;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for SkillsControl.xaml
    /// </summary>
    public partial class LanguagesControl : UserControl
    {

        private static TranslationContext TranslationContext = new TranslationContext("LanguagesControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(LanguagesControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = selected;
                if (context is Agent)
                {
                    Agents.IsSelected = true;
                    DGAgents.UpdateLayout();
                    DGAgents.SelectedItem = ((AdminCheckedLinkList)DGAgents.ItemsSource).FindItem(context);
                }

                else if (context is Activity)
                {
                    Activities.IsSelected = true;
                    DGActivities.UpdateLayout();
                    DGActivities.SelectedItem = ((AdminCheckedLinkList)DGActivities.ItemsSource).FindItem(context);
                }
                else if (context is Campaign)
                {
                    Activities.IsSelected = true;
                    DGActivities.UpdateLayout();
                    if ("InboundTab".Equals(ExtraInformation))
                        DGActivities.SelectedItem = ((AdminCheckedLinkList)DGActivities.ItemsSource).FindItem(((Campaign)context).SystemInboundActivity);
                    else if ("MailTab".Equals(ExtraInformation))
                        DGActivities.SelectedItem = ((AdminCheckedLinkList)DGActivities.ItemsSource).FindItem(((Campaign)context).SystemEmailActivity);
                    else if ("ChatTab".Equals(ExtraInformation))
                        DGActivities.SelectedItem = ((AdminCheckedLinkList)DGActivities.ItemsSource).FindItem(((Campaign)context).SystemChatActivity);
                }
            }
        }


        public void SetSelected(AdminObject selected)
        {
            if (selected != null)
            {
                if (selected is Language)
                {
                    SelectedItem = selected;
                }
                else if (selected is AgentLanguage)
                {
                    Agents.IsSelected = true;
                    SelectedItem = ((AgentLanguage)(selected)).Language;
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


        public LanguagesControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(LanguagesControl_IsVisibleChanged);

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


        void LanguagesControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Language)));
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
                DlgAddLanguage dlg = new DlgAddLanguage();
                dlg.Owner = Application.Current.MainWindow;
                dlg.DataContext = DataContext;

                if(dlg.ShowDialog().GetValueOrDefault())
                {
                    SetSelected(dlg.Created);

                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate("Are you sure you want to delete the language \"{0}\"?"), ((Language)MainGrid.SelectedItem).DisplayText);
                dlg.Owner = Application.Current.MainWindow;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    ((Language)MainGrid.SelectedItem).Core.Delete((Language)MainGrid.SelectedItem);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                DlgPrint dlgPrint = new DlgPrint();

                List<string> tempList = new List<string>();

                foreach (Language obj in MainGrid.Items)
                {
                    tempList.Add(obj.Id);
                }
                dlgPrint.reportName = "Listing Languages";
                dlgPrint.Parameters.Add(string.Empty, string.Join(",", tempList));
                dlgPrint.Owner = Application.Current.MainWindow;
                dlgPrint.ShowDialog();
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    afs.ShowObjectWithContext(toShow, (Language)MainGrid.SelectedItem, toShow);
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
                e.CanExecute = (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable);
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
