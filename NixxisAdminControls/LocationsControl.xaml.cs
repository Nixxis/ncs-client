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
    public partial class LocationsControl : UserControl
    {

        private static TranslationContext TranslationContext = new TranslationContext("LocationsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(LocationsControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = selected;
                if (context is Phone)
                {
                    Phones.IsSelected = true;
                    DGPhones.UpdateLayout();
                    DGPhones.SelectedItem = ((AdminCheckedLinkList)DGPhones.ItemsSource).FindItem(context);
                }
                else if (context is Resource)
                {
                    Resources.IsSelected = true;
                    DGResources.UpdateLayout();
                    DGResources.SelectedItem = ((AdminCheckedLinkList)DGResources.ItemsSource).FindItem(context);
                }

            }
        }

        public void SetSelected(AdminObject selected)
        {

            if (selected != null)
            {
                if (selected is Location)
                {
                    SelectedItem = selected;
                }
                else if (selected is NumberingRule)
                {
                    tabNumRules.IsSelected = true;
                    DGNumberingRules.UpdateLayout();
                    DGNumberingRules.SelectedItem = selected;
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


        public LocationsControl()
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
                DlgAddLocation dlg = new DlgAddLocation();

                dlg.Owner = Application.Current.MainWindow;
                dlg.DataContext = DataContext;

                if (tabNumRules.IsSelected && SelectedItem!=null)
                {
                    dlg.WizControl.Context = SelectedItem;
                    dlg.WizControl.CurrentStep = "StartChoice";
                    dlg.Title = "Location and numbering rules creation...";
                }
                else
                {
                    dlg.WizControl.CurrentStep = "Start";
                    dlg.Title = "Location creation...";
                }


                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    SetSelected(dlg.Created);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                if (( (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable) )
                    && (DGNumberingRules.SelectedItem != null && tabNumRules.IsSelected))
                {
                    // ask what to do
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((NumberingRule)DGNumberingRules.SelectedItem).TypedDisplayText,
                        ((Location)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            ((NumberingRule)DGNumberingRules.SelectedItem).Core.Delete((NumberingRule)DGNumberingRules.SelectedItem);
                        }
                        else
                        {
                            ((Location)MainGrid.SelectedItem).Core.Delete((Location)MainGrid.SelectedItem);
                        }
                    }

                }
                else if (DGNumberingRules.SelectedItem != null && tabNumRules.IsSelected)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the numbering rule \"{0}\"?"), ((NumberingRule)DGNumberingRules.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((NumberingRule)DGNumberingRules.SelectedItem).Core.Delete((NumberingRule)DGNumberingRules.SelectedItem);
                    }
                }
                else
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the location \"{0}\"?"), ((Location)MainGrid.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((Location)MainGrid.SelectedItem).Core.Delete((Location)MainGrid.SelectedItem);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                int backup = (DGNumberingRules.SelectedItem as NumberingRule).Sequence;
                NumberingRule previous = (DGNumberingRules.SelectedItem as NumberingRule).Previous;
                (DGNumberingRules.SelectedItem as NumberingRule).Sequence = previous.Sequence;
                previous.Sequence = backup;
                previous.ParentLocation.NumberingRules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                int backup = (DGNumberingRules.SelectedItem as NumberingRule).Sequence;
                NumberingRule next = (DGNumberingRules.SelectedItem as NumberingRule).Next;
                (DGNumberingRules.SelectedItem as NumberingRule).Sequence = next.Sequence;
                next.Sequence = backup;
                next.ParentLocation.NumberingRules.FireCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);

            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                DlgPrint dlgPrint = new DlgPrint();

                List<string> tempList = new List<string>();

                foreach (Location obj in MainGrid.Items)
                {
                    tempList.Add(obj.Id);
                }
                dlgPrint.reportName = "Listing Locations";
                dlgPrint.Parameters.Add(string.Empty, string.Join(",", tempList));
                dlgPrint.Owner = Application.Current.MainWindow;
                dlgPrint.ShowDialog();
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {


                if (((MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) )
                    && (DGNumberingRules.SelectedItem != null && tabNumRules.IsSelected))
                {
                    // ask what to do


                    DuplicateTargetConfirmationDialog dlg = new DuplicateTargetConfirmationDialog();
                    dlg.ItemsSource = new List<string>() { 
                        ((NumberingRule)DGNumberingRules.SelectedItem).TypedDisplayText,
                        ((Location)MainGrid.SelectedItem).TypedDisplayText
                    };

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            SetSelected(((NumberingRule)DGNumberingRules.SelectedItem).Duplicate());
                        }
                        else
                        {
                            SetSelected(((Location)MainGrid.SelectedItem).Duplicate());
                        }
                    }

                }
                else if (DGNumberingRules.SelectedItem != null && tabNumRules.IsSelected)
                {
                    SetSelected(((NumberingRule)DGNumberingRules.SelectedItem).Duplicate());
                }
                else
                {
                    SetSelected(((Location)MainGrid.SelectedItem).Duplicate());
                }

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

                    if (obj is LocationCost)
                    {
                        if (((LocationCost)obj).ToLocation.Target != MainGrid.SelectedItem)
                        {
                            toShow = ((LocationCost)obj).ToLocation.Target;
                        }
                        else
                        {
                            toShow = ((LocationCost)obj).FromLocation.Target;
                        }
                    }
                    else if (obj is AdminObject)
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

                    if (obj is LocationCost)
                    {
                        if (((LocationCost)obj).ToLocation.Target != MainGrid.SelectedItem)
                        {
                            toShow = ((LocationCost)obj).ToLocation.Target;
                        }
                        else
                        {
                            toShow = ((LocationCost)obj).FromLocation.Target;
                        }
                    }
                    else if (obj is AdminObject)
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
                e.CanExecute = ((MainGrid.SelectedItem != null) && !(MainGrid.SelectedItem as Location).IsReadOnly && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable))
                    || (DGNumberingRules.SelectedItem != null && tabNumRules.IsSelected);
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveUpOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as Location).IsReadOnly && (tabNumRules.IsSelected && DGNumberingRules.SelectedItem != null && !(DGNumberingRules.SelectedItem as NumberingRule).IsFirst);
                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectMoveDownOperation)
            {
                e.CanExecute = MainGrid.SelectedItem != null && !(MainGrid.SelectedItem as Location).IsReadOnly && (tabNumRules.IsSelected && DGNumberingRules.SelectedItem != null && !(DGNumberingRules.SelectedItem as NumberingRule).IsLast);
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
