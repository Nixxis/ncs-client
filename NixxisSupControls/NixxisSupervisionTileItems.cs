using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using Nixxis.Client.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml;
using System.Windows.Media.Imaging;
using Nixxis.ClientV2;


namespace Nixxis.Client.Supervisor
{
    //
    //Tiles
    //
    public class NixxisSupervisionTileItem : NixxisTileViewItem, INotifyPropertyChanged
    {
        #region Enums

        #endregion

        #region Class data
        private List<ColumnItemDescription> m_Columns = new List<ColumnItemDescription>();
        #endregion

        #region Properties
        internal ListBox ItemSelector { get; set; }
        internal Image ToggleImage { get; set; }
        public List<ColumnItemDescription> Columns
        {
            get { return m_Columns; }
            set { m_Columns = value; }
        }
        public object DataContextTotal { get; set; }
        private object m_LastSelectedItem;
        public object LastSelectedItem 
        {
            get
            {
                return m_LastSelectedItem;
            }
            set
            {
                m_LastSelectedItem = value;
                FirePropertyChanged("LastSelectedItem");
            }
        }
        private int m_LastSelectedColumn;
        public int LastSelectedColumn
        {
            get
            {
                return m_LastSelectedColumn;
            }
            set
            {
                m_LastSelectedColumn = value;
                FirePropertyChanged("LastSelectedColumn");
            }
        }

        #endregion

        #region Properties XAML

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisSupervisionTileItem));


        public static readonly DependencyProperty ToolbarPanelKeyProperty = DependencyProperty.Register("ToolbarPanelKey", typeof(string), typeof(NixxisSupervisionTileItem));
        public string ToolbarPanelKey
        {
            get { return GetValue(ToolbarPanelKeyProperty) as string; }
            set { SetValue(ToolbarPanelKeyProperty, value); }
        }

        public static readonly DependencyProperty RightClickShowColumnSelectorProperty = DependencyProperty.Register("RightClickShowColumnSelector", typeof(bool), typeof(NixxisSupervisionTileItem), new PropertyMetadata(true, new PropertyChangedCallback(RightClickShowColumnSelectorChanged)));
        public bool RightClickShowColumnSelector
        {
            get { return (bool)GetValue(RightClickShowColumnSelectorProperty); }
            set { SetValue(RightClickShowColumnSelectorProperty, value); }
        }
        public static void RightClickShowColumnSelectorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ColumnCollectionsProperty = DependencyProperty.Register("ColumnCollections", typeof(SupervisionColumnCollection), typeof(NixxisSupervisionTileItem));
        public SupervisionColumnCollection ColumnCollections
        {
            get { return (SupervisionColumnCollection)GetValue(ColumnCollectionsProperty); }
            set { SetValue(ColumnCollectionsProperty, value); }
        }
        #endregion

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }


        #region Members Override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            
        }

        public override void ApplyViewTemplate()
        {
            base.ApplyViewTemplate();

            if (this.ViewItemState == ViewItemStates.Large)
            {
                NixxisDataGrid grid = (NixxisDataGrid)this.LargeTemplate.FindName("MainDataGrid", this.ContentLarge);

                if (grid == null)
                    return;

                grid.ContextMenu = new ContextMenu();

                grid_SelectionChanged(grid, null);
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if(this.RightClickShowColumnSelector)
                this.ShowColumnSelector();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Context menu opening");

            NixxisDataGrid grid = (NixxisDataGrid)this.LargeTemplate.FindName("MainDataGrid", this.ContentLarge);

            grid.ContextMenu = null;

            DependencyObject dep = grid;

            while ((dep != null) && !(dep is SupFrameSet))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            SupFrameSet sfs = null;
            if (dep != null)
            {
                sfs = dep as SupFrameSet;

                System.Diagnostics.Trace.WriteLine("Context menu opening: pre raise event");
                this.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
                System.Diagnostics.Trace.WriteLine("Context menu opening: post raise event");

                if(sfs.Menu!=null)
                {
                    System.Diagnostics.Trace.WriteLine("Context menu opening: menu is there");
                    SupervisionItem si = this.LastSelectedItem as SupervisionItem;
                    if(si!=null && si.Id!=null && sfs.Menu.Tag!=null && sfs.Menu.Tag.Equals(si.Id))
                    {
                        System.Diagnostics.Trace.WriteLine("Context menu opening: menu is affected");
                        grid.ContextMenu = sfs.Menu;
                    }
                }
            }

            if (grid.ContextMenu == null)
                grid.ContextMenu = new ContextMenu();


            base.OnContextMenuOpening(e);

            System.Diagnostics.Trace.WriteLine("Context menu opening: end");

        }

        public override void CreateControl()
        {
            base.CreateControl();

            NixxisDataGrid grid = (NixxisDataGrid)this.LargeTemplate.FindName("MainDataGrid", this.ContentLarge);

            if (grid == null)
                return;

            grid.DataContextChanged += new DependencyPropertyChangedEventHandler(grid_DataContextChanged);
            grid.SelectionChanged += new SelectionChangedEventHandler(grid_SelectionChanged);

        }

        void grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(this.Name + " grid_DataContextChanged");
        }

        public void grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //For debug
            if(this.Name == "SupAgentPanel")
            {
            }
            if(e==null)
            {
                this.LastSelectedColumn = -1;

                if(((NixxisDataGrid)sender).SelectedItem != null)
                    this.LastSelectedItem = ((NixxisDataGrid)sender).SelectedItem;
                else
                    this.LastSelectedItem = null;

                this.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));

                return;
            }

            if ( e.AddedItems.Count == 0)
                return;
            
            if (((NixxisDataGrid)sender).SelectedCells.Count > 0 && ((NixxisDataGrid)sender).SelectedCells[0].Item != null)
            {
                this.LastSelectedColumn = ((NixxisDataGrid)sender).SelectedCells[0].Column.DisplayIndex;
                this.LastSelectedItem = ((NixxisDataGrid)sender).SelectedCells[0].Item;                
            }

            this.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
        }
        #endregion

        #region Members




        public void ShowColumnSelector()
        {
            if (this.ViewItemState == ViewItemStates.Large)
            {
                if (this.ItemSelector == null)
                    this.ItemSelector = (ListBox)this.LargeTemplate.FindName("ColumnSelector", this.CurrentContent);

                if (this.ItemSelector != null)
                {
                    if (this.ItemSelector.Visibility == System.Windows.Visibility.Collapsed)
                        this.ItemSelector.Visibility = System.Windows.Visibility.Visible;
                    else
                        this.ItemSelector.Visibility = System.Windows.Visibility.Collapsed;
                }
            };
        }
        public void CreateColumns()
        {

            NixxisDataGrid mainDataGrid = (NixxisDataGrid)this.LargeTemplate.FindName("MainDataGrid", this.CurrentContent);

            if (mainDataGrid == null)
                return;

            mainDataGrid.Columns.Clear();

            foreach (ColumnItemDescription item in this.m_Columns)
            {
                DataGridColumn col = null;
                Binding myBinding = null;

                if (item.VisibleForUser)
                {
                    switch (item.ColumnType)
                    {
                        case ColumnItemDescription.ColumnTypes.NixxisDataGridToggleDetailColumn:
                            col = new NixxisDataGridToggleDetailColumn();
                            ((NixxisDataGridToggleDetailColumn)col).SingleDetail = true;
                            ((NixxisDataGridToggleDetailColumn)col).ToggleDetailTemplate = (ControlTemplate)FindResource(item.ControlTemplateKey);
                            break;
                        case ColumnItemDescription.ColumnTypes.DataGridTemplateColumn:
                            col = new DataGridTemplateColumn();
                            break;
                        case ColumnItemDescription.ColumnTypes.DataGridTextColumn:
                        default:
                            col = new DataGridTextColumn();

                            myBinding = new Binding(item.BindingValue);
                            if (item.Convertor != null)
                                myBinding.Converter = item.Convertor;

                            ((DataGridTextColumn)col).Binding = myBinding;
                            break;
                    }
                    col.Visibility = item.Visible;
                    col.Header = item;

                    mainDataGrid.Columns.Add(col);
                }
            }
        }
        public void SetToolbarPanel()
        {
            NixxisBaseExpandPanel nep = FindResource(this.ToolbarPanelKey) as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                    NixxisGrid.SetPanelCommand.Execute(nep);
            }
        }
        public void RemoveToolbarPanel()
        {
            NixxisBaseExpandPanel nep = FindResource(this.ToolbarPanelKey) as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }
        }
        #endregion

        #region Members Workspace
        public SupervisionColumnCollection GetColumnSettings()
        {
            NixxisDataGrid grid = (NixxisDataGrid)this.LargeTemplate.FindName("MainDataGrid", this.ContentLarge);

            if (grid == null)
                return null;

            SupervisionColumnCollection collection = new SupervisionColumnCollection();

            for (int i = 0; i < grid.Columns.Count; i++)
            {
                DataGridColumn column = grid.Columns[i];

                string test = SupervisionColumnItem.GetId(column);

                

                SupervisionColumnItem item = new SupervisionColumnItem() { Index = i, Id = column.SortMemberPath, Visible = column.Visibility, Width = column.Width, DisplayIndex = column.DisplayIndex, SortDirection = column.SortDirection };

                collection.Add(item);
            }

            return collection;
        }

        public void SetColumnSettings(SupervisionColumnCollection collection)
        {
            NixxisDataGrid grid = (NixxisDataGrid)this.LargeTemplate.FindName("MainDataGrid", this.ContentLarge);

            if (grid == null)
                return;

            grid.Items.SortDescriptions.Clear();

            foreach (SupervisionColumnItem item in collection)
            {
                if (item.Index < grid.Columns.Count)
                {
                    grid.Columns[item.Index].Visibility = item.Visible;

                    grid.Columns[item.Index].Width = item.Width;

                    if( item.DisplayIndex >= 0)
                        grid.Columns[item.Index].DisplayIndex = item.DisplayIndex;

                    if (item.SortDirection.HasValue)
                    {
                        grid.Items.SortDescriptions.Add(new SortDescription(item.Id, item.SortDirection.Value));
                    }

                    grid.Columns[item.Index].SortDirection = item.SortDirection;

                    

                }
            }
            // to apply the sort!
            grid.Items.Refresh(); 

        }
        #endregion

        public virtual XmlNode GetExtraSettings()
        {
            return null;
        }

        public virtual void SetExtraSettings(XmlNode settings)
        {

        }

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class NixxisSupervisionTileInbound : NixxisSupervisionTileItem
    {
    }

    public class NixxisSupervisionTileAgent : NixxisSupervisionTileItem
    {

        public NixxisSupervisionTileAgent()
            : base()
        {
            InitColumnCollection();
        }

        private void InitColumnCollection()
        {
        }
    }

    public class NixxisSupervisionTileOutbound : NixxisSupervisionTileItem
    {        

    }

    public class NixxisSupervisionTileQueue : NixxisSupervisionTileItem
    {        
    }

    public class NixxisSupervisionTileCampaign : NixxisSupervisionTileItem
    {
    }

    public class NixxisSupervisionTileAlert : NixxisSupervisionTileItem
    {
        public NixxisSupervisionTileAlert()
            : base()
        {
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(NixxisSupervisionTileAlert_DataContextChanged);
        }

        void NixxisSupervisionTileAlert_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((Nixxis.ClientV2.AlertList)DataContext).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(NixxisSupervisionTileAlert_CollectionChanged);
        }

        void NixxisSupervisionTileAlert_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }
    }


    public class NixxisSupervisionTileDashboard: NixxisSupervisionTileItem
    {
        public override XmlNode GetExtraSettings()
        {
            DashboardWidgetsContainer dwc = this.LargeTemplate.FindName("dashboardWidgetsContainer", this.ContentLarge) as DashboardWidgetsContainer;
            return dwc.SaveDashboard().DocumentElement;
        }
        public override void SetExtraSettings(XmlNode settings)
        {
            DashboardWidgetsContainer dwc = this.LargeTemplate.FindName("dashboardWidgetsContainer", this.ContentLarge) as DashboardWidgetsContainer;

            XmlDocument doc = new XmlDocument();

            XmlNode nde  = doc.ImportNode(settings, true);

            doc.AppendChild(nde);

            dwc.LoadDashboard(doc);
        }
    }
    //
    //Converters
    //
    public class NixxisDisplayArray : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] list = value as string[];
            int rtn = 0;
            try { rtn = list.Length; }
            catch { rtn = 0; }
            return rtn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlertToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || DependencyProperty.UnsetValue == value ||  !(value is int))
                return new SolidColorBrush(Colors.Transparent);
            
            int alertlevel = (int)value;

            if(alertlevel==0)
                return new SolidColorBrush(Colors.Transparent);

            return new SolidColorBrush(Color.FromArgb( (byte)alertlevel, 255, 0,0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NixxisAgentStateIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {      
            //Undefined = 0,
            //Off = 1,
            //Login = 2,
            //Pause = 3,
            //Waiting = 4,
            //OnLine = 5,
            //WrapUp = 6,
            //Logout = 7,
            //Supervision = 8,
            //Preview = 9,
            //WaitingForVoice = 10,
            int state = (int)value;
            switch (state)
            {
                case 5:
                    return @"Images\AgentState\AgentState_OnLine.png";
                case 3:
                    return @"Images\AgentState\AgentState_Pause.png";
                case 9:
                    return @"Images\AgentState\AgentState_Preview.png";
                case 8:
                    return @"Images\AgentState\AgentState_Supervisor.png";
                case 4:
                case 19:
                    return @"Images\AgentState\AgentState_Waiting.png";
                case 6:
                    return @"Images\AgentState\AgentState_Wrapup.png";
                case 0:
                case 1:
                case 2:
                case 7:
                default:
                    return @"Images\AgentState\AgentState_Off.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NixxisInboundIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //MediaType:
            //======================
            //None = 0
            //Voice = 1
            //Chat = 2
            //Mail = 4
            //Custom1 = 8
            //Custom2 = 16
            //All = 31

            int state = (int)value;
            switch (state)
            {
                case 1:
                    return @"Images\MediaType\Activities_Inbound.png";
                case 2:
                    return @"Images\MediaType\Activities_Chat.png";
                case 4:
                    return @"Images\MediaType\Activities_Mail.png";
                default:
                    return @"Images\MediaType\Activities_Inbound.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NixxisOutboundIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //MediaType:
            //======================
            //None = 0
            //Voice = 1
            //Chat = 2
            //Mail = 4
            //Custom1 = 8
            //Custom2 = 16
            //All = 31

            int state = (int)value;
            switch (state)
            {
                case 1:
                    return @"Images\MediaType\Activities_Inbound.png";
                case 2:
                    return @"Images\MediaType\Activities_Chat.png";
                case 4:
                    return @"Images\MediaType\Activities_Mail.png";
                default:
                    return @"Images\MediaType\Activities_Inbound.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class AgentAvailableToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter as string == "voice")
                {
                    if ((bool)value)
                        return @"Images\MediaType\Activities_Inbound.png";
                    else
                        return @"Images\Other\BooleanOff.png";
                } 
                else if (parameter as string == "chat")
                {
                    if ((bool)value)
                        return @"Images\MediaType\Activities_Chat.png";
                    else
                        return @"Images\Other\BooleanOff.png";
                }
                else if (parameter as string == "mail")
                {
                    if ((bool)value)
                        return @"Images\MediaType\Activities_Mail.png";
                    else
                        return @"Images\Other\BooleanOff.png";
                }
                else
                {
                    if ((bool)value)
                        return @"Images\Other\BooleanOn.png";
                    else
                        return @"Images\Other\BooleanOff.png";
                }

            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RecordingIconConverter: IMultiValueConverter
    {
        // Not obvious! We can return a string if using a IValueConverter but not with a IMultiValueConveter; Here we have to return the right type!!!!!!
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool recordingMe = (bool)values[0];
            bool recordingOthers = (bool)values[1];
            bool isRecording = (bool)values[2];

            if(targetType == typeof(Visibility))
            {
                if (!recordingOthers & !recordingMe & !isRecording)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else if(targetType  == typeof(ImageSource))
            {

                BitmapImage img = new BitmapImage();
                img.BeginInit();
                if (!recordingOthers & !recordingMe & !isRecording)
                    img.UriSource = new Uri(@"Images\Recorded__Active.png", UriKind.Relative);
                else
                    img.UriSource = new Uri(string.Format(@"Images\Recorded_{0}_{1}.png", recordingMe ? "Me" : (recordingOthers ? "Others" : string.Empty), isRecording ? "Active" : "Inactive"), UriKind.Relative);
                img.EndInit();
                return img;
            }
            else
            {                
                if (isRecording)
                {
                    if (recordingMe)
                    {
                        return TranslationContext.Default.Translate("Currently recorded due to my request");
                    }
                    else if (recordingOthers)
                    {
                        return TranslationContext.Default.Translate("Currently recorded due to someone else's request");
                    }
                    else
                    {
                        return TranslationContext.Default.Translate("Currently recorded");
                    }
                }
                else
                {
                    if (recordingMe)
                    {
                        return TranslationContext.Default.Translate("Recording requested by me");
                    }
                    else if (recordingOthers)
                    {
                        return TranslationContext.Default.Translate("Recording requested by someone else");
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class MonitoringIconConverter : IMultiValueConverter
    {
        // Not obvious! We can return a string if using a IValueConverter but not with a IMultiValueConveter; Here we have to return the right type!!!!!!
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool requestingMonitorMe = (bool)values[0];
            bool requestingMonitorOthers = (bool)values[1];
            bool monitoringMe = (bool)values[2];
            bool monitoringOthers = (bool)values[3];

            if (targetType == typeof(Visibility))
            {
                if (!requestingMonitorOthers & !requestingMonitorMe & !monitoringMe & !monitoringOthers)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else if (targetType == typeof(ImageSource))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();

                img.UriSource = new Uri(@"Images\Monitored_Me.png", UriKind.Relative);



                if (!monitoringMe)
                {
                    if (monitoringOthers)
                    {
                        img.UriSource = new Uri(@"Images\Monitored_Others.png", UriKind.Relative);
                    }
                    else
                    {
                        if (requestingMonitorMe)
                        {
                            img.UriSource = new Uri(@"Images\MonitorRequest_Me.png", UriKind.Relative);
                        }
                        else if (requestingMonitorOthers)
                        {
                            img.UriSource = new Uri(@"Images\MonitorRequest_Others.png", UriKind.Relative);
                        }
                    }
                }

                img.EndInit();
                return img;
            }
            else
            {
                if(monitoringMe)
                {
                    return TranslationContext.Default.Translate("Currently monitored by me");
                }
                else
                {
                    if(monitoringOthers)
                    {
                        return TranslationContext.Default.Translate("Currently monitored by others");
                    }
                    else
                    {
                        if(requestingMonitorMe)
                        {
                            return TranslationContext.Default.Translate("Monitoring requested by me");
                        }
                        else if( requestingMonitorOthers)
                        {
                            return TranslationContext.Default.Translate("Monitoring requested by others");
                        }
                    }
                }

                return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomFilteredCollectionViewSource : CollectionViewSource
    {
        public CustomFilteredCollectionViewSource()
            : base()
        {
            this.Filter += CustomFilter;
        }

        private void CustomFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Nixxis.ClientV2.AgentItem)e.Item).RealTime.StatusIndex == 0 ? false : true;
        }
    }

    public class NixxisAlertIdIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MessageType type = (MessageType)value;
            switch (type)
            {
                case MessageType.Alert:
                    return @"Images\AltertType_Alert_24.png";
                case MessageType.HelpRequest:
                    return @"Images\AlertType_HelpRequest_24.png";
                case MessageType.Warning:
                    return @"Images\AlertType_Warning_24.png";
                case MessageType.Default:
                default:
                    return @"Images\AlterType_Default_24.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NixxisAlertUnReadForeColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool unread = (bool)value;

            if (unread)
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            else
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
