using Nixxis.Client.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Controls.WpfPropertyGrid;
using System.ComponentModel;
using System.Windows.Threading;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Windows.Markup;
using System.Windows.Baml2006;
using System.Xaml;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using Nixxis.ClientV2;

namespace Nixxis.Client.Supervisor
{
    [BrowsableProperty("*", false)]
    [BrowsableProperty("Columns", true)]
    [BrowsableProperty("Rows", true)]
    [BrowsableProperty("OverflowMode", true)]
    [BrowsableProperty("Description", true)]
    [BrowsableProperty("BackgroundColor", true)]
    [BrowsableProperty("WidgetTypes", true)]
    [BrowsableProperty("CurrentTheme", true)]
    public partial class DashboardWidgetsContainer : UserControl, IXmlSerializable
    {
        private static string DefaultThemeName = "Default";
        

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty ThemesProperty = DependencyProperty.Register("Themes", typeof(IDictionary<string, ResourceDictionary>), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ThemesPropertyChanged)));
        public static readonly DependencyProperty CurrentThemeProperty = DependencyProperty.Register("CurrentTheme", typeof(ResourceDictionary), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(CurrentThemePropertyChanged)));
        public static readonly DependencyProperty OverflowModeProperty = DependencyProperty.Register("OverflowMode", typeof(OverflowMode), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(OverflowMode.Crop, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OverflowModePropertyChanged)));
        public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register("DisplayMode", typeof(WidgetDisplayMode), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(WidgetDisplayMode.Run, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(DisplayModePropertyChanged)));
        public static readonly DependencyProperty SelectedWidgetProperty = DependencyProperty.Register("SelectedWidget", typeof(DashboardWidget), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(SelectedWidgetPropertyChanged)));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata("Dashboard", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(DescriptionPropertyChanged)));
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ColumnsPropertyChanged)));
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(int), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(RowsPropertyChanged)));
        public static readonly DependencyProperty WidgetTypesProperty = DependencyProperty.Register("WidgetTypes", typeof(IEnumerable<DashboardWidget>), typeof(DashboardWidgetsContainer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(WidgetTypesPropertyChanged)));

        public static readonly RoutedEvent HideEvent = EventManager.RegisterRoutedEvent("Hide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DashboardWidgetsContainer));
        public static readonly RoutedEvent ShowEvent = EventManager.RegisterRoutedEvent("Show", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DashboardWidgetsContainer));

        public event RoutedEventHandler Hide
        {
            add { AddHandler(HideEvent, value); }
            remove { RemoveHandler(HideEvent, value); }
        }
        public event RoutedEventHandler Show
        {
            add { AddHandler(ShowEvent, value); }
            remove { RemoveHandler(ShowEvent, value); }
        }

        public static void CurrentThemePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

            if (args.NewValue != args.OldValue && args.OldValue!=null)
            {
                dbWidget.ApplyTheme(args.NewValue as ResourceDictionary);
            }
        }
        public static void ThemesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

        }
        public static void OverflowModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;


        }
        public static void SelectedWidgetPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

            if (args.OldValue != null)
            {
                (args.OldValue as DashboardWidget).IsSelected = false;
            }

            if (args.NewValue != null)
            {
                (args.NewValue as DashboardWidget).IsSelected = true;
                dbWidget.dashboardProperties.SelectedObject = (args.NewValue as DashboardWidget).Widget;
            }
            else
                dbWidget.dashboardProperties.SelectedObject = dbWidget;

        }
        public static void DisplayModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

            foreach (UIElement uie in dbWidget.MainGrid.Children)
            {
                if (uie is DashboardWidget)
                {
                    (uie as DashboardWidget).Widget.DisplayMode = (WidgetDisplayMode)args.NewValue;
                }
            }

            if ((WidgetDisplayMode)args.NewValue == WidgetDisplayMode.Design)
            {
                dbWidget.DesignToolbox.Visibility = Visibility.Visible;
            }
            else
            {
                dbWidget.DesignToolbox.Visibility = Visibility.Collapsed;
            }

        }
        public static void DescriptionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

        }
        public static void ColumnsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

            int newcols = (int)args.NewValue;

            if (newcols > dbWidget.MainGrid.ColumnDefinitions.Count)
            {
                while (dbWidget.MainGrid.ColumnDefinitions.Count != newcols)
                {
                    dbWidget.MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(128) });
                }
            }
            else
            {
                while (dbWidget.MainGrid.ColumnDefinitions.Count != newcols)
                {
                    if (dbWidget.MainGrid.HasChildrenInColumn(dbWidget.MainGrid.ColumnDefinitions.Count - 1, true))
                    {
                        dbWidget.Columns = dbWidget.MainGrid.ColumnDefinitions.Count;
                        return;
                    }

                    dbWidget.MainGrid.ColumnDefinitions.RemoveAt(dbWidget.MainGrid.ColumnDefinitions.Count - 1);
                }

            }
        }
        public static void RowsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

            int newrows = (int)args.NewValue;

            if (newrows > dbWidget.MainGrid.RowDefinitions.Count)
            {
                while (dbWidget.MainGrid.RowDefinitions.Count != newrows)
                {
                    dbWidget.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(128) });
                }
            }
            else
            {
                while (dbWidget.MainGrid.RowDefinitions.Count != newrows)
                {
                    if (dbWidget.MainGrid.HasChildrenInRow(dbWidget.MainGrid.RowDefinitions.Count - 1, true))
                    {
                        dbWidget.Rows = dbWidget.MainGrid.RowDefinitions.Count;
                        return;
                    }
                    dbWidget.MainGrid.RowDefinitions.RemoveAt(dbWidget.MainGrid.RowDefinitions.Count - 1);
                }

            }
        }
        public static void WidgetTypesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetsContainer dbWidget = obj as DashboardWidgetsContainer;

        }

        public DashboardWidget SelectedWidget
        {
            get
            {
                return (DashboardWidget)GetValue(SelectedWidgetProperty);
            }
            set
            {
                SetValue(SelectedWidgetProperty, value);
            }
        }

        [Category("Appearance")]
        [Description("Dashboard container background color")]
        public Brush BackgroundColor
        {
            get
            {
                return (Brush)GetValue(BackgroundColorProperty);
            }
            set
            {
                SetValue(BackgroundColorProperty, value);
            }
        }
        public WidgetDisplayMode DisplayMode
        {
            get
            {
                return (WidgetDisplayMode)GetValue(DisplayModeProperty);
            }
            set
            {
                SetValue(DisplayModeProperty, value);
            }
        }

        [Category("Dashboard base")]
        [Description("Dashboard's name")]
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        [Category("Size")]
        [Description("Dashboard's columns count")]
        [PropertyOrder(1)]
        public int Columns
        {
            get
            {
                return (int)GetValue(ColumnsProperty);
            }
            set
            {
                SetValue(ColumnsProperty, value);
            }
        }

        [Category("Size")]
        [Description("Dashboard's row count")]
        [PropertyOrder(0)]
        public int Rows
        {
            get
            {
                return (int)GetValue(RowsProperty);
            }
            set
            {
                SetValue(RowsProperty, value);
            }
        }

        [Category("Size")]
        [Description("Dashboard's behavior when there is not enough space to display the entire content")]
        public OverflowMode OverflowMode
        {
            get
            {
                return (OverflowMode)GetValue(OverflowModeProperty);
            }
            set
            {
                SetValue(OverflowModeProperty, value);
            }
        }

        [Category("Widgets")]
        public IEnumerable<DashboardWidget> WidgetTypes
        {
            get
            {
                return (IEnumerable<DashboardWidget>)GetValue(WidgetTypesProperty);
            }
            set
            {
                SetValue(WidgetTypesProperty, value);
            }
        }

        public IDictionary<string, ResourceDictionary> Themes
        {
            get
            {
                return (IDictionary<string, ResourceDictionary>)GetValue(ThemesProperty);
            }
            set
            {
                SetValue(ThemesProperty, value);
            }
        }

        [Category("Appearance")]
        [Description("Dashboard's current theme")]
        public ResourceDictionary CurrentTheme
        {
            get
            {
                return (ResourceDictionary)GetValue(CurrentThemeProperty);
            }
            set
            {
                SetValue(CurrentThemeProperty, value);
            }
        }

        public DashboardWidgetsContainer()
        {
            InitializeComponent();

            Rows = 5;
            Columns = 5;

            dashboardProperties.SelectedObject = this;

            List<DashboardWidget> temp = new List<DashboardWidget>();

            DirectoryInfo di = new DirectoryInfo("Dashboard");

            List<Stream> skinBamlStreams = new List<Stream>();

            SortedList<string, ResourceDictionary> tempThemes = new SortedList<string, ResourceDictionary>();

            if (di != null)
            {
                try
                {
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        try
                        {
                            int counter = 0;
                            Assembly asbly = Assembly.LoadFile(fi.FullName);
                            string[] resourcesNames = asbly.GetManifestResourceNames();
                            foreach (string resourceName in resourcesNames)
                            {
                                ManifestResourceInfo resourceInfo = asbly.GetManifestResourceInfo(resourceName);
                                if (resourceInfo.ResourceLocation != ResourceLocation.ContainedInAnotherAssembly)
                                {
                                    Stream resourceStream = asbly.GetManifestResourceStream(resourceName);
                                    using (ResourceReader resourceReader = new ResourceReader(resourceStream))
                                    {
                                        foreach (DictionaryEntry entry in resourceReader)
                                        {

                                            if (IsRelevantResource(entry, string.Empty))
                                            {
                                                try
                                                {
                                                    ResourceDictionary skinResource = BamlHelper.LoadBaml<ResourceDictionary>(entry.Value as Stream);
                                                    if (skinResource != null)
                                                    {
                                                        tempThemes.Add(System.IO.Path.GetFileNameWithoutExtension(entry.Key as string), skinResource);

                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        catch { }
                    }
                }
                catch
                {

                }
            }

            foreach (Type t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => { try { return x.GetTypes(); } catch { return Type.EmptyTypes; } }).Where(mytype => mytype != null && typeof(IDashboardWidget).IsAssignableFrom(mytype) && mytype.GetInterfaces().Contains(typeof(IDashboardWidget))))
            {
                IDashboardWidget idw = (IDashboardWidget)(t.GetConstructor(new Type[] { }).Invoke(null));
                if (!string.IsNullOrEmpty(idw.WidgetName))
                {
                    DashboardWidget dbw = new DashboardWidget() { Widget = idw };
                    dbw.Widget.DisplayMode = WidgetDisplayMode.Icon;
                    temp.Add(dbw);
                }
            }

            WidgetTypes = temp;


            tempThemes.Add(DefaultThemeName, this.Resources.MergedDictionaries[0]);
            Themes = tempThemes;
            CurrentTheme = this.Resources.MergedDictionaries[0];

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            
            
        }

        private SavedContexts SavedDashboards
        {
            get
            {
                if(m_SavedDashboards==null)
                {
                    m_SavedDashboards = new SavedContexts(this.DataContext as Supervision, "Dashboards");
                }
                return m_SavedDashboards;
            }
        }
        
        private SavedContexts m_SavedDashboards = null;

        private int counter = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            foreach(UIElement uie in MainGrid.Children)
            {
                if (uie is DashboardWidget)
                {
                    try
                    {
                        ((DashboardWidget)uie).Widget.Timer(counter);
                    }
                    catch
                    {

                    }
                }
            }
            counter++;
            if (counter == int.MaxValue)
            {
                counter = 0;
            }
        }

        private void ApplyTheme(ResourceDictionary rd)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(rd);

            foreach (UIElement obj in MainGrid.Children)
            {
                if(obj is DashboardWidget)
                {
                    DashboardWidget dw = (DashboardWidget)obj;
                    if (rd.Source == null)
                        dw.Widget.CurrentTheme = string.Empty;
                    else
                        dw.Widget.CurrentTheme = rd.Source.AbsolutePath;
                }
            }

            foreach(DashboardWidget dw in WidgetTypes)
            {
                if (rd.Source == null)
                        dw.Widget.CurrentTheme = string.Empty;
                    else
                        dw.Widget.CurrentTheme = rd.Source.AbsolutePath;
            }
        }
        private static bool IsRelevantResource(DictionaryEntry entry, string resourceName)
        {
            string entryName = entry.Key as string;
            string extension = System.IO.Path.GetExtension(entryName);
            return
                string.Compare(extension, ".baml", true) == 0 &&											// the resource has a .baml extension
                entry.Value is Stream &&											// the resource is a Stream
                (string.IsNullOrEmpty(resourceName) || string.Compare(resourceName, entryName, true) == 0);	// the resource name requested equals to current resource name
        }

        static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
            DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object arg)
            {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },
            frame
            );
            Dispatcher.PushFrame(frame);
        }

        private void MainGrid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("resize"))
            {
                Thumb thumb = e.Data.GetData("resize") as Thumb;
                List<Point> allowed = null;

                DependencyObject depobj = thumb;
                while (!(depobj is DashboardWidget))
                    depobj = VisualTreeHelper.GetParent(depobj);

                DashboardWidget widget = depobj as DashboardWidget;

                Point widgetPosition = new Point(GridControl.GetColumn(widget), GridControl.GetRow(widget));

                if (thumb.Tag == null)
                {
                    // let's compute the list of allowed points                    
                    allowed = new List<Point>();
                    bool stop = false;
                    int maxRow = MainGrid.RowDefinitions.Count;
                    for (int x = (int)(widgetPosition.X); x < MainGrid.ColumnDefinitions.Count; x++)
                    {
                        for (int y = (int)(widgetPosition.Y); y < maxRow; y++)
                        {
                            bool nextColumn = false;
                            // we are testing the point x, y
                            foreach (UIElement uie in MainGrid.Children)
                            {
                                if (uie is DashboardWidget)
                                {
                                    Point pt = new Point(GridControl.GetColumn(uie), GridControl.GetRow(uie));
                                    if (pt.X <= x && pt.Y <= y && pt.X + GridControl.GetColumnSpan(uie) - 1 >= x && pt.Y + GridControl.GetRowSpan(uie) - 1 >= y)
                                    {
                                        // there is something under x, y
                                        if (uie != widget)
                                        {
                                            nextColumn = true;
                                            maxRow = y;
                                            if (y == (int)(widgetPosition.Y))
                                                stop = true;
                                        }
                                        break;
                                    }
                                }
                            }
                            if (nextColumn)
                            {
                                break;
                            }
                            else
                            {
                                allowed.Add(new Point(x, y));
                            }
                        }
                        if (stop)
                            break;
                    }

                    thumb.Tag = allowed;
                }
                else
                {
                    allowed = thumb.Tag as List<Point>;
                }


                Point dropPosition = MainGrid.GetColumnRow(e.GetPosition(MainGrid));

                foreach (Point pt in allowed)
                {
                    if (dropPosition.X == pt.X && dropPosition.Y == pt.Y)
                    {
                        Vector resulting = dropPosition - widgetPosition;
                        Size idealSize = new Size((int)resulting.X + 1, (int)resulting.Y + 1);

                        Size allowedSize = widget.Widget.SetSize(idealSize);

                        if (allowedSize.Equals(idealSize))
                        {
                            GridControl.SetColumnSpan(widget, (int)allowedSize.Width);
                            GridControl.SetRowSpan(widget, (int)allowedSize.Height);

                            e.Effects = DragDropEffects.Copy;
                            e.Handled = true;
                            return;
                        }
                        else
                        {
                            e.Effects = DragDropEffects.None;
                            e.Handled = true;
                            return;

                        }
                    }
                }



                e.Effects = DragDropEffects.None;
                e.Handled = true;

            }
            else if (e.Data.GetDataPresent("move"))
            {
                Thumb thumb = e.Data.GetData("move") as Thumb;
                List<Point> prohibited = null;

                DependencyObject depobj = thumb;
                DependencyObject lastvaliddepobj = thumb;
                while (!(depobj is DashboardWidget))
                {
                    depobj = VisualTreeHelper.GetParent(depobj);
                    if (depobj != null)
                        lastvaliddepobj = depobj;
                    else
                        break;
                }

                DashboardWidget widget = null;

                if (depobj == null)
                {
                    widget = (lastvaliddepobj as FrameworkElement).Parent as DashboardWidget;


                    MainGrid.Children.Add(widget);

                    ApplyTheme(CurrentTheme);

                    DoEvents();

                    e.Data.SetData("move", widget.MoveThumb);
                }
                else
                {
                    widget = depobj as DashboardWidget;
                }


                if (thumb.Tag == null)
                {
                    // let's compute the list of allowed points                    
                    prohibited = new List<Point>();


                    Point span = new Point(GridControl.GetColumnSpan(widget), GridControl.GetRowSpan(widget));

                    for (int x = 0; x < MainGrid.ColumnDefinitions.Count; x++)
                    {
                        prohibited.Add(new Point(x, MainGrid.RowDefinitions.Count));
                        prohibited.Add(new Point(x, -1));
                    }
                    for (int y = 0; y < MainGrid.RowDefinitions.Count; y++)
                    {
                        prohibited.Add(new Point(MainGrid.ColumnDefinitions.Count, y));
                        prohibited.Add(new Point(-1, y));
                    }

                    foreach (UIElement uie in MainGrid.Children)
                    {
                        if (uie is DashboardWidget && uie != widget)
                        {
                            Point pt = new Point(GridControl.GetColumn(uie), GridControl.GetRow(uie));
                            for (int x = 0; x < GridControl.GetColumnSpan(uie); x++)
                                for (int y = 0; y < GridControl.GetRowSpan(uie); y++)
                                    prohibited.Add(new Point(pt.X + x, pt.Y + y));
                        }
                    }
                    // this result would be correct if our widget was not span over multiple cells.
                    // let's check that...
                    for (int x = 0; x < MainGrid.ColumnDefinitions.Count; x++)
                    {
                        for (int y = 0; y < MainGrid.RowDefinitions.Count; y++)
                        {
                            for (int i = 0; i < span.X; i++)
                            {
                                for (int j = 0; j < span.Y; j++)
                                {
                                    foreach (Point pt in prohibited)
                                    {
                                        if (pt.X == x + i && pt.Y == y + j)
                                        {
                                            prohibited.Add(new Point(x, y));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    thumb.Tag = prohibited;
                }
                else
                {
                    prohibited = thumb.Tag as List<Point>;
                }


                Point dropPosition = MainGrid.GetColumnRow(e.GetPosition(MainGrid));

                dropPosition = new Point(dropPosition.X - widget.dragStartPoint.X, dropPosition.Y - widget.dragStartPoint.Y);

                if (dropPosition.X < 0 || dropPosition.X >= MainGrid.ColumnDefinitions.Count || dropPosition.Y < 0 || dropPosition.Y >= MainGrid.RowDefinitions.Count)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }


                foreach (Point pt in prohibited)
                {
                    if (dropPosition.X == pt.X && dropPosition.Y == pt.Y)
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                }


                GridControl.SetColumn(widget, (int)dropPosition.X);
                GridControl.SetRow(widget, (int)dropPosition.Y);


                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

        }

        private void MainGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("resize"))
            {
                Thumb thumb = e.Data.GetData("resize") as Thumb;

                DependencyObject depobj = thumb;
                while (!(depobj is DashboardWidget))
                    depobj = VisualTreeHelper.GetParent(depobj);

                DashboardWidget widget = depobj as DashboardWidget;
                widget.IsResizing = false;

            }
            else if (e.Data.GetDataPresent("move"))
            {
                Thumb thumb = e.Data.GetData("move") as Thumb;

                DashboardWidget widget = null;

                DependencyObject depobj = thumb;
                while (depobj != null && !(depobj is DashboardWidget))
                    depobj = VisualTreeHelper.GetParent(depobj);

                if (depobj == null)
                {
                    // try to get it from something else
                    DependencyObject fmkElm = thumb;
                    while (fmkElm != null && (fmkElm is FrameworkElement) && !(fmkElm is DashboardWidget))
                        fmkElm = ((FrameworkElement)fmkElm).Parent;

                    if (fmkElm != null)
                        widget = fmkElm as DashboardWidget;
                }
                else
                {
                    widget = depobj as DashboardWidget;
                }

                if (widget != null)
                {
                    widget.IsMoving = false;
                    widget.ApplyTemplate();
                }
            }

            SynchronizationContext.Current.Post(((obj) => Mouse.UpdateCursor()), null);

        }

        private void MainGrid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Move)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
            }
            else if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.SizeNWSE);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedWidget = null;
        }

        private void HideToolbox(object sender, RoutedEventArgs e)
        {
            RaiseHideEvent(DesignToolbox);
        }
        private void CloseToolbox(object sender, RoutedEventArgs e)
        {
            DesignToolbox.Visibility = System.Windows.Visibility.Collapsed;
            DisplayMode = WidgetDisplayMode.Run;
        }
        private void ShowToolbox(object sender, RoutedEventArgs e)
        {
            if (DesignToolbox.Visibility == System.Windows.Visibility.Visible)
            {
                RaiseShowEvent(DesignToolbox);
            }
            else
            {
                DesignToolbox.Visibility = System.Windows.Visibility.Visible;
                DisplayMode = WidgetDisplayMode.Design;
            }
        }

        public void DeleteWidget(DashboardWidget widget)
        {
            ConfirmationDialog dlg = new ConfirmationDialog();
            dlg.MessageText = string.Format("Are you sure you want to delete widget \"{0}\"?", widget.Widget.Title);
            dlg.Owner = Application.Current.MainWindow;
            if (dlg.ShowDialog().GetValueOrDefault())
                MainGrid.Children.Remove(widget);
        }
        internal static RoutedEventArgs RaiseHideEvent(DependencyObject target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = HideEvent;

            if (target is UIElement)
            {
                (target as UIElement).RaiseEvent(args);
            }
            else if (target is ContentElement)
            {
                (target as ContentElement).RaiseEvent(args);
            }

            return args;
        }

        internal static RoutedEventArgs RaiseShowEvent(DependencyObject target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ShowEvent;

            if (target is UIElement)
            {
                (target as UIElement).RaiseEvent(args);
            }
            else if (target is ContentElement)
            {
                (target as ContentElement).RaiseEvent(args);
            }

            return args;
        }

        private void ConfigureDataSource(object sender, RoutedEventArgs e)
        {
            Button snder = sender as Button;
            System.Windows.Controls.WpfPropertyGrid.PropertyItemValue btnParam = snder.CommandParameter as System.Windows.Controls.WpfPropertyGrid.PropertyItemValue;

            if (btnParam.Value is DashboardWidgetDataSource)
            {
                DashboardWidgetDatasourceDialog datasourceEditor = new DashboardWidgetDatasourceDialog();
                DashboardWidgetDataSource datasource = btnParam.Value as DashboardWidgetDataSource;

                datasourceEditor.DataContext = DataContext;

                datasourceEditor.Owner = Application.Current.MainWindow;

                datasourceEditor.ObjType = datasource.ObjectType;

                datasourceEditor.ObjIds =  new List<string>(datasource.ObjectIds);

                datasourceEditor.ObjProperties = new List<string>(datasource.ObjectProperties);


                if (datasourceEditor.ShowDialog().GetValueOrDefault())
                {
                    datasource.ObjectType = null;
                    datasource.ObjectIds = null;
                    datasource.ObjectProperties = null;


                    datasource.ObjectType = datasourceEditor.ObjType as string;

                    datasource.ObjectIds = new System.Collections.ObjectModel.ObservableCollection<string>(datasourceEditor.ObjIds);

                    datasource.ObjectProperties = new System.Collections.ObjectModel.ObservableCollection<string>(datasourceEditor.ObjProperties);                         
                }
            }

        }

        private void ConfigurePalette(object sender, RoutedEventArgs e)
        {
            Button snder = sender as Button;
            System.Windows.Controls.WpfPropertyGrid.PropertyItemValue btnParam = snder.CommandParameter as System.Windows.Controls.WpfPropertyGrid.PropertyItemValue;

            if (btnParam.Value is DashboardWidgetPalette)
            {
                DashboardWidgetPaletteDialog paletteEditor = new DashboardWidgetPaletteDialog();

                DashboardWidgetPalette palette = btnParam.Value as DashboardWidgetPalette;

                paletteEditor.Owner = Application.Current.MainWindow;
                paletteEditor.FillPalettes(CurrentTheme);

                paletteEditor.PaletteName = palette.Name;
                if (string.IsNullOrEmpty(palette.Name))
                {
                    paletteEditor.Brushes = new System.Collections.ObjectModel.ObservableCollection<Brush>( palette.Colors.Select((c)=>( new SolidColorBrush(c)) ));
                }
                else
                {
                    paletteEditor.FillOrder(palette.Order);
                }



                if (paletteEditor.ShowDialog().GetValueOrDefault())
                {
                    palette.Name = paletteEditor.PaletteName;
                    if (string.IsNullOrEmpty(palette.Name))
                    {
                        palette.Colors = new System.Collections.ObjectModel.ObservableCollection<Color>(paletteEditor.Brushes.Select((b) => (((SolidColorBrush)b).Color)));
                        palette.Order = new System.Collections.ObjectModel.ObservableCollection<int>();
                    }
                    else
                    {
                        palette.Colors = new System.Collections.ObjectModel.ObservableCollection<Color>();
                        palette.Order = new System.Collections.ObjectModel.ObservableCollection<int>(paletteEditor.Order);
                    }
                }
            }
        }

        private void ConfigureCollection(object sender, RoutedEventArgs e)
        {
            Button snder = sender as Button;
            System.Windows.Controls.WpfPropertyGrid.PropertyItemValue btnParam = snder.CommandParameter as System.Windows.Controls.WpfPropertyGrid.PropertyItemValue;
            if(btnParam.Value is Collection<int>)
            {
                DashboardWidgetLevelsDialog dlg = new DashboardWidgetLevelsDialog();
                dlg.Owner = Application.Current.MainWindow;
                Collection<int> collection = (Collection<int>)(btnParam.Value);

                dlg.FillValues(collection);

                if(dlg.ShowDialog().GetValueOrDefault())
                {
                    int counter = 0;
                    foreach(int i in dlg.GetValues())
                    {
                        if (counter < collection.Count)
                            collection[counter] = i;
                        else
                            collection.Add(i);
                        counter++;
                    }
                    while (collection.Count > counter)
                        collection.RemoveAt(collection.Count - 1);
                }
            }
        }


        public static Type CollectionOfInt
        {
            get
            {
                return typeof(Collection<int>);
            }
        }

        private void Open(object sender, RoutedEventArgs e)
        {

            SavedContextsOpenDialog dlg = new SavedContextsOpenDialog();

            dlg.Owner = Application.Current.MainWindow;
            dlg.DataContext = SavedDashboards;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
               
                for (int i = 0; i < MainGrid.Children.Count; i++)
                {
                    if (MainGrid.Children[i] is DashboardWidget)
                    {
                        MainGrid.Children.RemoveAt(i);
                        i--;
                    }
                }

                LoadDashboard((dlg.lstContexts.SelectedValue as SavedContext).Content);

            }

        }

        public void LoadDashboard(XmlDocument doc)
        {
            for (int i = 0; i < MainGrid.Children.Count; i++)
            {
                if (MainGrid.Children[i] is DashboardWidget)
                {
                    MainGrid.Children.RemoveAt(i);
                    i--;
                }
            }


            using (XmlReader reader = new XmlNodeReader(doc))
            {
                reader.MoveToContent();
                reader.ReadStartElement();
                this.ReadXml(reader);


                while (reader.MoveToContent() != XmlNodeType.EndElement)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DashboardWidget));
                    DashboardWidget dbw = serializer.Deserialize(reader) as DashboardWidget;
                    MainGrid.Children.Add(dbw);
                }

                ApplyTheme(CurrentTheme);

                DoEvents();

            }

        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SavedContextsSaveDialog dlg = new SavedContextsSaveDialog();

            dlg.Owner = Application.Current.MainWindow;
            SavedDashboards.DefaultNewNameBase = Description;
            dlg.DataContext = SavedDashboards;            

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                
                SavedContext context = null;

                if (dlg.radioNew.IsChecked.GetValueOrDefault())
                {
                    context = SavedDashboards.Add(dlg.TxtName.Text, dlg.IsShared.IsChecked.GetValueOrDefault());
                }
                else
                {
                    context = (dlg.lstContexts.SelectedValue as SavedContext);
                }

                context.Content = SaveDashboard();            
            }

        }

        public XmlDocument SaveDashboard()
        {
            XmlDocument doc = new XmlDocument();

            XmlNode root = doc.AppendChild(doc.CreateElement("DashboardContext"));

            System.Xml.XPath.XPathNavigator navigator = root.CreateNavigator();

            XmlSerializer serializer = new XmlSerializer(this.GetType());

            using (XmlWriter writer = navigator.AppendChild())
            {
                writer.WriteWhitespace("");
                serializer.Serialize(writer, this);
                writer.Close();

            }


            foreach (UIElement obj in MainGrid.Children)
            {
                if (obj is DashboardWidget)
                {
                    DashboardWidget dw = (DashboardWidget)obj;

                    serializer = new XmlSerializer(dw.GetType());

                    using (XmlWriter writer = navigator.AppendChild())
                    {
                        writer.WriteWhitespace("");
                        serializer.Serialize(writer, dw);
                        writer.Close();
                    }
                }
            }

            return doc;            

        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            Columns = System.Xml.XmlConvert.ToInt32( reader.GetAttribute("Columns"));
            Rows = System.Xml.XmlConvert.ToInt32(reader.GetAttribute("Rows"));
            try
            {
                OverflowMode = (OverflowMode)(System.Xml.XmlConvert.ToInt32(reader.GetAttribute("OverflowMode")));
            }
            catch
            {
            }
            try
            {
                Description = reader.GetAttribute("Description");
            }
            catch
            {
            }


            string strCurrentTheme = reader.GetAttribute("Theme");

            if (string.IsNullOrEmpty(strCurrentTheme))
            {
                CurrentTheme = Themes[DefaultThemeName];
            }
            else
            {
                try
                {
                    CurrentTheme = Themes[strCurrentTheme];
                }
                catch
                {

                }
            }


            reader.ReadStartElement();
            if (reader.HasAttributes)
            {
                BackgroundColor = new SolidColorBrush(Color.FromArgb(
                    System.Xml.XmlConvert.ToByte(reader.GetAttribute("A")),
                    System.Xml.XmlConvert.ToByte(reader.GetAttribute("R")),
                    System.Xml.XmlConvert.ToByte(reader.GetAttribute("G")),
                    System.Xml.XmlConvert.ToByte(reader.GetAttribute("B"))
                ));
            }
            else
            {
                BackgroundColor = null;
            }
            reader.Skip();


            
            if(!reader.IsEmptyElement)
                reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Columns", System.Xml.XmlConvert.ToString(Columns));
            writer.WriteAttributeString("Rows", System.Xml.XmlConvert.ToString(Rows));
            writer.WriteAttributeString("OverflowMode", System.Xml.XmlConvert.ToString((int)OverflowMode));
            writer.WriteAttributeString("Description", Description);

            foreach (var th in Themes)
            {
                if (th.Value == CurrentTheme)
                {
                    writer.WriteAttributeString("Theme", th.Key);
                    break;
                }
            }

            if (BackgroundColor != null)
            {
                writer.WriteStartElement("Background");
                writer.WriteAttributeString("A", System.Xml.XmlConvert.ToString((BackgroundColor as SolidColorBrush).Color.A));
                writer.WriteAttributeString("R", System.Xml.XmlConvert.ToString((BackgroundColor as SolidColorBrush).Color.R));
                writer.WriteAttributeString("G", System.Xml.XmlConvert.ToString((BackgroundColor as SolidColorBrush).Color.G));
                writer.WriteAttributeString("B", System.Xml.XmlConvert.ToString((BackgroundColor as SolidColorBrush).Color.B));
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteStartElement("Background");
                writer.WriteEndElement();
            }

        }

        private void DetachWindow(object sender, RoutedEventArgs e)
        {
            StandAloneWindow wnd = new StandAloneWindow();

            wnd.Height = this.Height;
            
            wnd.Width = this.Width;

            DashboardWidgetsContainer dwc = new DashboardWidgetsContainer();

            dwc.DataContext = this.DataContext;

            wnd.Content = dwc;

            wnd.Show();

            dwc.LoadDashboard(this.SaveDashboard());

        }
    }

    public enum OverflowMode
    {
        [Description("Crop")]
        Crop,
        [Description("Scroll")]
        Scroll,
        [Description("Scale")]
        Scale
    }


    public class GridControl : Grid
    {
        #region Properties
        public bool ShowCustomGridLines
        {
            get { return (bool)GetValue(ShowCustomGridLinesProperty); }
            set { SetValue(ShowCustomGridLinesProperty, value); }
        }

        public static readonly DependencyProperty ShowCustomGridLinesProperty =
            DependencyProperty.Register("ShowCustomGridLines", typeof(bool), typeof(GridControl), new UIPropertyMetadata(false, new PropertyChangedCallback(ShowCustomGridLinesPropertyChanged)));

        public Brush GridLineBrush
        {
            get { return (Brush)GetValue(GridLineBrushProperty); }
            set { SetValue(GridLineBrushProperty, value); }
        }

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register("GridLineBrush", typeof(Brush), typeof(GridControl), new UIPropertyMetadata(Brushes.Black));

        public double GridLineThickness
        {
            get { return (double)GetValue(GridLineThicknessProperty); }
            set { SetValue(GridLineThicknessProperty, value); }
        }

        public static readonly DependencyProperty GridLineThicknessProperty =
            DependencyProperty.Register("GridLineThickness", typeof(double), typeof(GridControl), new UIPropertyMetadata(1.0));
        #endregion

        public static void ShowCustomGridLinesPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GridControl gc = sender as GridControl;

            gc.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (ShowCustomGridLines)
            {
                foreach (var rowDefinition in RowDefinitions)
                {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), new Point(0, rowDefinition.Offset), new Point(ActualWidth, rowDefinition.Offset));
                }

                foreach (var columnDefinition in ColumnDefinitions)
                {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), new Point(columnDefinition.Offset, 0), new Point(columnDefinition.Offset, ActualHeight));
                }
                dc.DrawRectangle(Brushes.Transparent, new Pen(GridLineBrush, GridLineThickness), new Rect(0, 0, ActualWidth, ActualHeight));
            }
            base.OnRender(dc);
        }
        static GridControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(typeof(GridControl)));
        }
    }


    public static class GridExtentions
    {
        public static T Parent<T>(this DependencyObject root) where T : class
        {
            if (root is T) { return root as T; }

            DependencyObject parent = VisualTreeHelper.GetParent(root);
            return parent != null ? parent.Parent<T>() : null;
        }

        public static Point GetColumnRow(this Grid obj, Point relativePoint) { return new Point(GetColumn(obj, relativePoint.X), GetRow(obj, relativePoint.Y)); }
        private static int GetRow(Grid obj, double relativeY) { return GetData(obj.RowDefinitions, relativeY); }
        private static int GetColumn(Grid obj, double relativeX) { return GetData(obj.ColumnDefinitions, relativeX); }

        private static int GetData<T>(IEnumerable<T> list, double value) where T : DefinitionBase
        {
            var start = 0.0;
            var result = 0;

            var property = typeof(T).GetProperties().FirstOrDefault(p => p.Name.StartsWith("Actual"));
            if (property == null) { return result; }

            foreach (var definition in list)
            {
                start += (double)property.GetValue(definition, null);
                if (value < start) { break; }

                result++;
            }

            return result;
        }

        public static bool HasChildrenInRow(this Grid obj, int row, bool handleSpan)
        {
            foreach (UIElement uie in obj.Children)
            {
                int r = Grid.GetRow(uie);
                if (r == row)
                    return true;

                if (handleSpan && r <= row && row <= r + Grid.GetRowSpan(uie) - 1)
                    return true;

            }
            return false;
        }
        public static bool HasChildrenInColumn(this Grid obj, int col, bool handleSpan)
        {
            foreach (UIElement uie in obj.Children)
            {
                int c = Grid.GetColumn(uie);
                if (c == col)
                    return true;

                if (handleSpan && c <= col && col <= c + Grid.GetColumnSpan(uie) - 1)
                    return true;
            }
            return false;
        }
    }

    public class DisplayModeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((WidgetDisplayMode)value == WidgetDisplayMode.Design)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return WidgetDisplayMode.Design;
            else
                return WidgetDisplayMode.Run;
        }
    }

    public class DecimalConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ChangeType(value, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(parameter!=null)
                return System.Convert.ChangeType(value, parameter as Type);

            return System.Convert.ChangeType(value, typeof(int));
        }
    }


    public class StretchConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            OverflowMode ovfMode = (OverflowMode)values[0];
            WidgetDisplayMode dispMode = (WidgetDisplayMode)values[1];

            if (dispMode == WidgetDisplayMode.Design)
                return Stretch.None;
            if (ovfMode == OverflowMode.Scale)
                return Stretch.Uniform;
            return Stretch.None;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
    public class ScrollVisibilityConverter : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            OverflowMode ovfMode = (OverflowMode)values[0];
            WidgetDisplayMode dispMode = (WidgetDisplayMode)values[1];

            if (dispMode == WidgetDisplayMode.Design)
                return ScrollBarVisibility.Auto;
            if (ovfMode == OverflowMode.Scroll)
                return ScrollBarVisibility.Auto;
            return ScrollBarVisibility.Disabled;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class BamlHelper
    {
        public static TRoot LoadBaml<TRoot>(Stream stream)
        {
            var reader = new Baml2006Reader(stream);
            var writer = new XamlObjectWriter(reader.SchemaContext);
            while (reader.Read())
            {
                writer.WriteNode(reader);
            }
            return (TRoot)writer.Result;
        }

    }

    public class BooleanToSBVisibilityConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return ScrollBarVisibility.Visible;
            return ScrollBarVisibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
