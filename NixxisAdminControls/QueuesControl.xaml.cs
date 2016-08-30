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
    public partial class QueuesControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("QueuesControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(QueuesControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = selected;
                if (context is Team)
                {
                    Teams.IsSelected = true;
                    DGTeams.UpdateLayout();
                    DGTeams.SelectedItem = ((AdminCheckedLinkList)DGTeams.ItemsSource).FindItem(context);
                }
                else if (context is Prompt)
                {
                    TabPrompts.IsSelected = true;
                    DGPrompts.UpdateLayout();
                    DGPrompts.SelectedItem = ((Prompt)context).Links[0];
                }
            }
        }


        public void SetSelected(AdminObject selected)
        {
            if (selected != null)
            {
                if (selected is Queue)
                {
                    SelectedItem = selected;
                }
                else if (selected is QueueTeam)
                {
                    Teams.IsSelected = true;
                    SelectedItem = ((QueueTeam)(selected)).Queue;
                    DGTeams.UpdateLayout();
                    if (DGTeams.ItemsSource is AdminCheckedLinkList)
                    {
                        DGTeams.SelectedItem = ((AdminCheckedLinkList)DGTeams.ItemsSource).FindLink(selected);
                    }
                    else
                    {
                        DGTeams.SelectedItem = selected;
                    }

                }

                else if (selected is Team)
                {
                    AdminObject temp = selected;
                    while (!(temp is Queue) || temp.IsSystemWithValidOwner)
                    {
                        temp = temp.Owner;
                    }
                    SelectedItem = temp;

                    if (Teams.IsVisible)
                        Teams.IsSelected = true;

                }
                else if (selected is AgentTeam)
                {
                    AgentTeam at = selected as AgentTeam;
                    if (at.Team != null && at.Team.IsSystemWithValidOwner)
                    {
                        AdminObject temp = at.Team;
                        while (!(temp is Queue) || temp.IsSystemWithValidOwner)
                        {
                            temp = temp.Owner;
                        }
                        SelectedItem = temp;

                        if (Teams.IsVisible)
                            Teams.IsSelected = true;
                    }
                }
                else if (selected is Prompt)
                {
                    TabPrompts.IsSelected = true;
                    DGPrompts.UpdateLayout();
                    DGPrompts.SelectedItem = selected;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private Prompt m_SelectedRelatedPrompt = null;

        private void DataGridPrompts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            DataGrid dg = (DataGrid)sender;

            if (dg.Name == "DGRelatedPrompts")
            {
                m_SelectedRelatedPrompt = dg.SelectedItem as Prompt;
            }

            if (dg.SelectedItem == null)
                return;

            if (dg == DGPrompts)
            {
                Helpers.ApplyToVisualChildren<DataGrid>(DGPrompts, DataGrid.SelectedItemProperty, null, (de) => (!Helpers.FindVisualParent<DataGridRow>(de as UIElement).IsSelected));
            }
            else
            {
                Helpers.ApplyToChildren<DataGrid>(Helpers.FindParent<Grid>(Helpers.FindParent<Grid>(dg)), DataGrid.SelectedItemProperty, null, (fe) => (fe != dg));
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

        public QueuesControl()
        {

            IsVisibleChanged += new DependencyPropertyChangedEventHandler(QueuesControl_IsVisibleChanged);

            InitializeComponent();

            ((CollectionViewSource)(FindResource("List"))).Filter += new FilterEventHandler(List_Filter);
            
        }

        private bool BaseFilter(AdminObject obj, string groupkey)
        {
            return (!obj.IsSystem && !obj.IsDummy && !obj.IsPartial) && (groupkey== null || groupkey.Equals(obj.GroupKey));
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
            ((ComboBox)sender).ItemsSource = new ExtendedStringEnumerable((DataContext as AdminCore).GroupKeys(typeof(Queue)));
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
                DlgAddQueue dlg = new DlgAddQueue();

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

                if (TabPrompts.IsSelected && SelectedItem!=null)
                {
                    if (DGPrompts.SelectedItem != null)
                    {
                        dlg.Prompt = (DGPrompts.SelectedItem as PromptLink).Prompt;
                        dlg.PromptUnder = true;
                    }
                    dlg.WizControl.CurrentStep = "StartWithChoice";
                    dlg.WizControl.Context = SelectedItem;
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
                if (TabPrompts.IsSelected && DGPrompts.SelectedItem != null)
                {
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();
                    Prompt todelete = null;

                    if (m_SelectedRelatedPrompt != null)
                    {
                        todelete = m_SelectedRelatedPrompt;
                        dlg.ItemsSource = new List<string>() { 
                            string.Format("Selected prompt related to {0}", ((PromptLink)DGPrompts.SelectedItem).Prompt.DisplayText),
                            ((Queue)MainGrid.SelectedItem).TypedDisplayText
                        };
                    }
                    else
                    {
                        dlg.ItemsSource = new List<string>() { 
                            ((PromptLink)DGPrompts.SelectedItem).Prompt.TypedDisplayText,
                            ((Queue)MainGrid.SelectedItem).TypedDisplayText
                        };
                    }

                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        if (dlg.SelectedIndex == 0)
                        {
                            PromptLink pr = DGPrompts.SelectedItem as PromptLink;

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
                            ((Queue)MainGrid.SelectedItem).Core.Delete((Queue)MainGrid.SelectedItem);
                        }
                    }
                }
                else
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the queue \"{0}\"?"), ((Queue)MainGrid.SelectedItem).DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        ((Queue)MainGrid.SelectedItem).Core.Delete((Queue)MainGrid.SelectedItem);
                    }
                }

            }
            else if (e.Command == AdminFrameSet.AdminObjectPrintOperation)
            {
                DlgPrint dlgPrint = new DlgPrint();

                List<string> tempList = new List<string>();

                foreach (Queue obj in MainGrid.Items)
                {
                    tempList.Add(obj.Id);
                }
                dlgPrint.reportName = "Listing Queues";
                dlgPrint.Parameters.Add(string.Empty, string.Join(",", tempList));
                dlgPrint.Owner = Application.Current.MainWindow;
                dlgPrint.ShowDialog();
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
                Queue duplicatesrc = (Queue)MainGrid.SelectedItem;
                if (duplicatesrc != null)
                {
                    SetSelected(duplicatesrc.Duplicate());
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
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation || e.Command==AdminFrameSet.AdminObjectPrintOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                e.CanExecute = (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && ((AdminObject)MainGrid.SelectedItem).IsDeletable;
            }
            else
            {
                e.CanExecute = (MainGrid.SelectedItem != null);
            }
            e.Handled = true;
        }

        private void Thumb1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // X2Y2
            double posX = Canvas.GetLeft(sender as UIElement) + e.HorizontalChange;
            double posY = Canvas.GetTop(sender as UIElement) + e.VerticalChange;

            double height = DrawingCanvas.ActualHeight;
            double width = DrawingCanvas.ActualWidth;

            double maxCoef = 100;
            double margin = 5;
            double rightExtraMargin = width * 1 / 10;

            Queue q = (sender as Thumb).DataContext as Queue;
            
            double qTime = (float)((posX - margin) * q.Time3 / (width - 2 * margin - rightExtraMargin));
            double qCoef = (float)((height -posY - margin) * maxCoef / (height - 2 * margin));
            
            if (qTime < 0)
                q.Time0 = 0;
            else if (qTime > q.Time1)
                q.Time0 = q.Time1;
            else
                q.Time0 = (float)qTime;

            if (qCoef < 0)
                q.Coef0 = 0;
            else if (qCoef > 100)
                q.Coef0 = 100;
            else
                q.Coef0 = (float)qCoef;

        }
        private void Thumb2_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // X3Y3
            double posX = Canvas.GetLeft(sender as UIElement) + e.HorizontalChange;
            double posY = Canvas.GetTop(sender as UIElement) + e.VerticalChange;

            double height = DrawingCanvas.ActualHeight;
            double width = DrawingCanvas.ActualWidth;

            double maxCoef = 100;
            double margin = 5;
            double rightExtraMargin = width * 1 / 10;

            Queue q = (sender as Thumb).DataContext as Queue;

            double qTime = (float)((posX - margin) * q.Time3 / (width - 2 * margin - rightExtraMargin));
            double qCoef = (float)((height - posY - margin) * maxCoef / (height - 2 * margin));

            if (qTime < q.Time0)
                q.Time1 = q.Time0;
            else if (qTime > q.Time2)
                q.Time1 = q.Time2;
            else
                q.Time1 = (float)qTime;

            if (qCoef < 0)
                q.Coef1 = 0;
            else if (qCoef > 100)
                q.Coef1 = 100;
            else
                q.Coef1 = (float)qCoef;
        }
        private void Thumb3_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // X4Y4
            double posX = Canvas.GetLeft(sender as UIElement) + e.HorizontalChange;
            double posY = Canvas.GetTop(sender as UIElement) + e.VerticalChange;

            double height = DrawingCanvas.ActualHeight;
            double width = DrawingCanvas.ActualWidth;

            double maxCoef = 100;
            double margin = 5;
            double rightExtraMargin = width * 1 / 10;

            Queue q = (sender as Thumb).DataContext as Queue;

            double qTime = (float)((posX - margin) * q.Time3 / (width - 2 * margin - rightExtraMargin));
            double qCoef = (float)((height - posY - margin) * maxCoef / (height - 2 * margin));

            if (qTime < q.Time1)
                q.Time2 = q.Time1;
            else if (qTime > q.Time3)
                q.Time2 = q.Time3;
            else
                q.Time2 = (float)qTime;

            if (qCoef < 0)
                q.Coef2 = 0;
            else if (qCoef > 100)
                q.Coef2 = 100;
            else
                q.Coef2 = (float)qCoef;
        }
        private void Thumb4_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // X5Y5
            double posX = Canvas.GetLeft(sender as UIElement) + e.HorizontalChange;
            double posY = Canvas.GetTop(sender as UIElement) + e.VerticalChange;

            double height = DrawingCanvas.ActualHeight;
            double width = DrawingCanvas.ActualWidth;

            double maxCoef = 100;
            double margin = 5;
            double rightExtraMargin = width * 1 / 10;

            Queue q = (sender as Thumb).DataContext as Queue;

            double qTime = (float)((posX - margin) * q.Time3 / (width - 2 * margin - rightExtraMargin));
            double qCoef = (float)((height - posY - margin) * maxCoef / (height - 2 * margin));

            System.Diagnostics.Trace.WriteLine(string.Format("{0}", q.Time3));

            if (qTime < q.Time2)
                q.Time3 = q.Time2;
            else
                q.Time3 = (float)qTime;

            System.Diagnostics.Trace.WriteLine(string.Format("{0}", q.Time3));

            if (qCoef < 0)
                q.Coef3 = 0;
            else if (qCoef > 100)
                q.Coef3 = 100;
            else
                q.Coef3 = (float)qCoef;
        }

        private void Thumb1_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb1Popup.IsOpen = true;
        }
        private void Thumb1_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb1Popup.IsOpen = false;
        }
        private void Thumb2_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb2Popup.IsOpen = true;
        }
        private void Thumb2_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb2Popup.IsOpen = false;
        }
        private void Thumb3_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb3Popup.IsOpen = true;
        }
        private void Thumb3_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb3Popup.IsOpen = false;
        }
        private void Thumb4_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb4Popup.IsOpen = true;
        }
        private void Thumb4_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb4Popup.IsOpen = false;
        }

        private string m_BackupId;

        private void VoicePreprocessors_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item is NullAdminObject || ((Preprocessor)(e.Item)).MediaType == MediaType.Voice);
        }


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

    public class ProfitEvaluationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double width = (double)values[1];
            double height = (double)values[2];
            Queue q = values[0] as Queue;

            if (q == null)
                return 0.0;

            double maxTime = q.Time3;

            double maxCoef = 100;

            double margin = 5;
            double rightExtraMargin = width * 1 / 10;

            if (parameter.Equals("X1"))
            {
                return margin;
            }
            else if(parameter.Equals("Y1"))
            {
                return height - margin - q.Coef0 / maxCoef * (height - 2 * margin);
            }
            if (parameter.Equals("X2"))
            {
                return margin + q.Time0 / maxTime * (width - 2 * margin - rightExtraMargin);
            }
            else if (parameter.Equals("Y2"))
            {
                return height - margin - q.Coef0 / maxCoef * (height -2 * margin );
            }
            if (parameter.Equals("X3"))
            {
                return margin + q.Time1 / maxTime * (width - 2 * margin - rightExtraMargin);
            }
            else if (parameter.Equals("Y3"))
            {
                return height - margin - q.Coef1 / maxCoef * (height - 2 * margin);
            }
            if (parameter.Equals("X4"))
            {
                return margin + q.Time2 / maxTime * (width - 2 * margin - rightExtraMargin);
            }
            else if (parameter.Equals("Y4"))
            {
                return height - margin - q.Coef2 / maxCoef * (height - 2 * margin);
            }
            if (parameter.Equals("X5"))
            {
                return margin + q.Time3 / maxTime * (width - 2 * margin - rightExtraMargin);
            }
            else if (parameter.Equals("Y5"))
            {
                return height - margin - q.Coef3 / maxCoef * (height - 2 * margin);
            }
            if (parameter.Equals("X6"))
            {
                return width - margin;
            }
            else if (parameter.Equals("Y6"))
            {
                return height - margin - q.Coef3 / maxCoef * (height - 2 * margin);
            }
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }

    public class WaitTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = true;
            bool[] bParams = null;

            if (parameter != null)
            {
                string[] prms = parameter.ToString().Split(',');
                bParams = new bool[prms.Length];
                for (int i = 0; i < bParams.Length; i++)
                {
                    bParams[i] = bool.Parse(prms[i]);
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (DependencyProperty.UnsetValue.Equals(values[i]))
                {
                    if (bParams != null && i < bParams.Length)
                        if (bParams[i])
                            result = false;
                    else
                        result = false;
                }
                else
                {

                    if (bParams != null && i < bParams.Length)
                        if (!bParams[i])
                            result = result && !((bool)values[i]);
                        else
                            result = result && ((bool)values[i]);
                    else
                        result = result && ((bool)values[i]);
                }
            }

            if (targetType == typeof(Visibility))
                return result ? Visibility.Visible : Visibility.Collapsed;

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
