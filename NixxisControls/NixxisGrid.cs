using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;

namespace Nixxis.Client.Controls
{
    public class NixxisGrid : Grid
    {
        public static NixxisCommand SetPanelCommand { get; private set; }
        public static NixxisCommand RemovePanelCommand { get; private set; }


        public static readonly DependencyProperty MinimumPanelHeightProperty = DependencyProperty.Register("MinimumPanelHeight", typeof(double), typeof(NixxisGrid));
        public static readonly DependencyProperty MaximumPanelHeightProperty = DependencyProperty.Register("MaximumPanelHeight", typeof(double), typeof(NixxisGrid));
        public static readonly DependencyProperty MinimumWindowWidthProperty = DependencyProperty.Register("MinimumWindowWidth", typeof(double), typeof(NixxisGrid));

        public static readonly DependencyProperty PanelProperty = DependencyProperty.RegisterAttached("Panel", typeof(string), typeof(NixxisGrid), new PropertyMetadata(string.Empty, new PropertyChangedCallback(PanelChanged)));

        public static void PanelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisGrid grd = (obj as FrameworkElement).Parent as NixxisGrid;
            if (grd != null)
            {
                for (int i = 0; i < grd.ColumnDefinitions.Count; i++)
                {
                    NixxisColumnDefinition ncd = grd.ColumnDefinitions[i] as NixxisColumnDefinition;
                    if (ncd != null)
                    {
                        if (ncd.PanelName.Equals(args.NewValue))
                        {
                            if (((UIElement)obj).Visibility == Visibility.Visible)
                            {
                                foreach (UIElement uie in grd.Children)
                                {
                                    if (uie != (UIElement)obj && NixxisGrid.GetColumn(uie).Equals(i) && uie.Visibility == Visibility.Visible)
                                        uie.Visibility = Visibility.Collapsed;

                                }
                            }
                            if (obj is NixxisBaseExpandPanel)
                            {
                                if (((NixxisBaseExpandPanel)obj).Visibility == Visibility.Visible)
                                {
                                    ncd.SetBinding(ColumnDefinition.MinWidthProperty, new Binding() { Source = obj, Path = new PropertyPath(NixxisBaseExpandPanel.MinimumPanelWidthProperty) });                                       
                                }
                            }

                            NixxisGrid.SetColumn((UIElement)obj, i);
                        }
                    }
                }                
                grd.UpdateMinimumPanelSize();
            }
        }

        private void UpdateMinimumPanelSize()
        {
            double minHeight = 0;
            double maxHeight = 0;
            double minwidth = 0;
            
            foreach(object nbep in Children)
            {
                if (nbep is NixxisBaseExpandPanel)
                {

                    if (((NixxisBaseExpandPanel)nbep).Visibility == Visibility.Visible)
                    {
                        if (((NixxisBaseExpandPanel)nbep).MinimumPanelHeight > minHeight)
                            minHeight = ((NixxisBaseExpandPanel)nbep).MinimumPanelHeight;

                        if (((NixxisBaseExpandPanel)nbep).HeightToShowContent > maxHeight)
                            maxHeight = ((NixxisBaseExpandPanel)nbep).HeightToShowContent;

                        minwidth += ((NixxisBaseExpandPanel)nbep).MinimumPanelWidth;
                    }

                }
                else if(nbep is GridSplitter)
                {
                    minwidth += (nbep as GridSplitter).ActualWidth;
                }
            }
            MinimumPanelHeight = minHeight;
            MaximumPanelHeight = maxHeight;


            Window wnd  = Helpers.FindParent<Window>(this);
            MinimumWindowWidth = minwidth + wnd.ActualWidth - ActualWidth;
        }

        public double MinimumPanelHeight
        {
            get
            {
                return (double)GetValue(MinimumPanelHeightProperty);
            }
            set
            {
                SetValue(MinimumPanelHeightProperty, value);
            }
        }
        public double MaximumPanelHeight
        {
            get
            {
                return (double)GetValue(MaximumPanelHeightProperty);
            }
            set
            {
                SetValue(MaximumPanelHeightProperty, value);
            }
        }
        public double MinimumWindowWidth
        {
            get
            {
                return (double)GetValue(MinimumWindowWidthProperty);
            }
            set
            {
                SetValue(MinimumWindowWidthProperty, value);
            }
        }

        public static void SetPanel(UIElement element, string value)
        {
            element.SetValue(PanelProperty, value);
        }

        public static string GetPanel(UIElement element)
        {
            return (string)(element.GetValue(PanelProperty));
        }

        static NixxisGrid()
        {
            SetPanelCommand = new NixxisCommand(null,null, true);
            RemovePanelCommand = new NixxisCommand(null, null, true);

            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisGrid), new FrameworkPropertyMetadata(typeof(NixxisGrid)));
            EventManager.RegisterClassHandler(typeof(NixxisGrid), NixxisBaseExpandPanel.MinimumPanelHeightChangedEvent, new RoutedEventHandler(PanelHeightChanged));
            EventManager.RegisterClassHandler(typeof(NixxisGrid), NixxisBaseExpandPanel.HeightToShowContentChangedEvent, new RoutedEventHandler(PanelHeightChanged));
        }

        private static void PanelHeightChanged(object sender, RoutedEventArgs args)
        {
            NixxisBaseExpandPanel nbep = args.Source as NixxisBaseExpandPanel;
            
            if (nbep != null)
            {
                NixxisGrid ng = Helpers.FindParent<NixxisGrid>(nbep);
                if (ng != null)
                {
                    ng.UpdateMinimumPanelSize();
                }
            }
        }

        public static void SwapPanels(UIElement element1, UIElement element2)
        {
            if (element1.Visibility == System.Windows.Visibility.Visible && element2.Visibility == System.Windows.Visibility.Visible)
            {
                element1.Visibility = System.Windows.Visibility.Collapsed;
                string oldcol = NixxisGrid.GetPanel(element1);
                NixxisGrid.SetPanel(element1, NixxisGrid.GetPanel(element2));
                NixxisGrid.SetPanel(element2, oldcol);
                element1.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                string oldcol = NixxisGrid.GetPanel(element1);
                NixxisGrid.SetPanel(element1, NixxisGrid.GetPanel(element2));
                NixxisGrid.SetPanel(element2, oldcol);
            }
        }

        public void SwapPanels(string panel1, string panel2)
        {
            UIElement element1 = null;
            UIElement element2 = null;

            foreach (UIElement uie in Children)
            {

                if (NixxisGrid.GetPanel(uie).Equals(panel1))
                {
                    if (element1 == null || uie.Visibility == System.Windows.Visibility.Visible)
                        element1 = uie;
                }
                if (NixxisGrid.GetPanel(uie).Equals(panel2))
                {
                    if (element2 == null || uie.Visibility == System.Windows.Visibility.Visible)
                        element2 = uie;
                }
            }
            if (element1 != element2 && element1 != null && element2 != null)
                NixxisGrid.SwapPanels(element1, element2);
        }

        public void EnsurePanelVisible(string panel)
        {
            UIElement element = null;

            foreach (UIElement uie in Children)
            {

                if (NixxisGrid.GetPanel(uie).Equals(panel))
                {
                    if (uie.Visibility == System.Windows.Visibility.Visible)
                        return;

                    element = uie;
                }
            }

            if (element != null)
            {
                element.Visibility = System.Windows.Visibility.Visible;
            }

            UpdateMinimumPanelSize();
        }
    }

    public class NixxisColumnDefinition : ColumnDefinition
    {
        public static readonly DependencyProperty PanelNameProperty = DependencyProperty.Register("PanelName", typeof(string), typeof(NixxisColumnDefinition));

        public string PanelName
        {
            get
            {
                return (string)GetValue(PanelNameProperty);
            }
            set
            {
                SetValue(PanelNameProperty, value);
            }
        }

        static NixxisColumnDefinition()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisColumnDefinition), new FrameworkPropertyMetadata(typeof(NixxisColumnDefinition)));
        }
    }

    public class NixxisRowDefinition : RowDefinition
    {
        static NixxisRowDefinition()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisRowDefinition), new FrameworkPropertyMetadata(typeof(NixxisRowDefinition)));
        }

    }

    public class NixxisGridSplitter : GridSplitter
    {
        private Storyboard m_ExpandStoryboard = null;
        private GridLength m_LastRowHeight = new GridLength();

        public static readonly DependencyProperty PanelsContainerProperty = DependencyProperty.Register("PanelsContainer", typeof(NixxisGrid), typeof(NixxisGridSplitter));

        public static readonly DependencyProperty MaximizeAllowedProperty = DependencyProperty.Register("MaximizeAllowed", typeof(bool), typeof(NixxisGridSplitter), new PropertyMetadata(true));
        public static readonly DependencyProperty MinimizeAllowedProperty = DependencyProperty.Register("MinimizeAllowed", typeof(bool), typeof(NixxisGridSplitter), new PropertyMetadata(true));
        public static readonly DependencyProperty RestoreAllowedProperty = DependencyProperty.Register("RestoreAllowed", typeof(bool), typeof(NixxisGridSplitter), new PropertyMetadata(true));

        public static readonly DependencyProperty ExpandButtonProperty = DependencyProperty.Register("ExpandButton", typeof(Button), typeof(NixxisGridSplitter));
        public static readonly DependencyProperty RestoreButtonProperty = DependencyProperty.Register("RestoreButton", typeof(Button), typeof(NixxisGridSplitter));
        public static readonly DependencyProperty CollapseButtonProperty = DependencyProperty.Register("CollapseButton", typeof(Button), typeof(NixxisGridSplitter));

        public static readonly DependencyProperty ExpandButtonTemplateProperty = DependencyProperty.Register("ExpandButtonTemplate", typeof(ControlTemplate), typeof(NixxisGridSplitter), new PropertyMetadata(null, new PropertyChangedCallback(ExpandButtonTemplateChanged)));
        public static readonly DependencyProperty RestoreButtonTemplateProperty = DependencyProperty.Register("RestoreButtonTemplate", typeof(ControlTemplate), typeof(NixxisGridSplitter), new PropertyMetadata(null, new PropertyChangedCallback(RestoreButtonTemplateChanged)));
        public static readonly DependencyProperty CollapseButtonTemplateProperty = DependencyProperty.Register("CollapseButtonTemplate", typeof(ControlTemplate), typeof(NixxisGridSplitter), new PropertyMetadata(null, new PropertyChangedCallback(CollapseButtonTemplateChanged)));


        public static void ExpandButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisGridSplitter btn = obj as NixxisGridSplitter;
            if (btn != null && btn.ExpandButton != null)
                btn.ExpandButton.Template = args.NewValue as ControlTemplate;
        }
        public static void RestoreButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisGridSplitter btn = obj as NixxisGridSplitter;
            if (btn != null && btn.RestoreButton != null)
                btn.RestoreButton.Template = args.NewValue as ControlTemplate;
        }
        public static void CollapseButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisGridSplitter btn = obj as NixxisGridSplitter;
            if (btn != null && btn.CollapseButton != null)
                btn.CollapseButton.Template = args.NewValue as ControlTemplate;
        }


        public NixxisGrid PanelsContainer
        {
            get
            {
                return (NixxisGrid)GetValue(PanelsContainerProperty);
            }
            set
            {
                SetValue(PanelsContainerProperty, value);
            }
        }
        public bool MaximizeAllowed
        {
            get
            {
                return (bool)GetValue(MaximizeAllowedProperty);
            }
        }
        public bool MinimizeAllowed
        {
            get
            {
                return (bool)GetValue(MinimizeAllowedProperty);
            }
        }
        public bool RestoreAllowed
        {
            get
            {
                return (bool)GetValue(RestoreAllowedProperty);
            }
        }        
        public double CollapsedPanelHeight
        {
            get
            {
                foreach (UIElement elm in PanelsContainer.Children)
                {
                    if (elm is NixxisBaseExpandPanel)
                        return (elm as NixxisBaseExpandPanel).CollapsedPanelHeight;
                }
                return 0;
            }
        }
        public Button ExpandButton
        {
            get
            {
                return (Button)GetValue(ExpandButtonProperty);
            }
            set
            {
                SetValue(ExpandButtonProperty, value);
            }
        }
        public Button RestoreButton
        {
            get
            {
                return (Button)GetValue(RestoreButtonProperty);
            }
            set
            {
                SetValue(RestoreButtonProperty, value);
            }
        }
        public Button CollapseButton
        {
            get
            {
                return (Button)GetValue(CollapseButtonProperty);
            }
            set
            {
                SetValue(CollapseButtonProperty, value);
            }
        }
        public ControlTemplate ExpandButtonTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(ExpandButtonTemplateProperty);
            }
            set
            {
                SetValue(ExpandButtonTemplateProperty, value);
            }
        }
        public ControlTemplate RestoreButtonTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(RestoreButtonTemplateProperty);
            }
            set
            {
                SetValue(RestoreButtonTemplateProperty, value);
            }
        }
        public ControlTemplate CollapseButtonTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(CollapseButtonTemplateProperty);
            }
            set
            {
                SetValue(CollapseButtonTemplateProperty, value);
            }
        }

        static NixxisGridSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisGridSplitter), new FrameworkPropertyMetadata(typeof(NixxisGridSplitter)));
        }

        private Grid MainGrid
        {
            get
            {
                return this.Parent as Grid;
            }
        }
        private int PreviousRow
        {
            get
            {
                return Grid.GetRow(this) - 1;
            }
        }
        private int NextRow
        {
            get
            {
                return Grid.GetRow(this) + 1;
            }
        }

        public void MaximizeBottomPanels()
        {
            foreach (UIElement uie in PanelsContainer.Children)
            {
                if (uie is NixxisBaseExpandPanel)
                {
                    (uie as NixxisBaseExpandPanel).Minimized = false;
                }
            }

            SetValue(MaximizeAllowedProperty, false);
            SetValue(MinimizeAllowedProperty, true);
            SetValue(RestoreAllowedProperty, true);


            MainGrid.RowDefinitions[NextRow].SetBinding(RowDefinition.MaxHeightProperty, new Binding() { Source = PanelsContainer, Path = new PropertyPath(NixxisGrid.MaximumPanelHeightProperty) });
            MainGrid.RowDefinitions[NextRow].SetBinding(RowDefinition.MinHeightProperty, new Binding() { Source = PanelsContainer, Path = new PropertyPath(NixxisGrid.MaximumPanelHeightProperty) });

            this.Cursor = Cursors.Arrow;
            m_LastRowHeight = MainGrid.RowDefinitions[NextRow].Height;
            (m_ExpandStoryboard.Children[0] as GridLengthAnimation).To = new GridLength(MainGrid.RowDefinitions[NextRow].MinHeight);
            BeginStoryboard(m_ExpandStoryboard);


        }
        public void RestoreBottomPanel()
        {
            foreach (UIElement uie in PanelsContainer.Children)
            {
                if (uie is NixxisBaseExpandPanel)
                {
                    (uie as NixxisBaseExpandPanel).Minimized = false;
                }
            }


            SetValue(MaximizeAllowedProperty, true);
            SetValue(MinimizeAllowedProperty, true);
            SetValue(RestoreAllowedProperty, false);

            MainGrid.RowDefinitions[NextRow].SetBinding(RowDefinition.MaxHeightProperty, new Binding() { Source = PanelsContainer, Path = new PropertyPath(NixxisGrid.MaximumPanelHeightProperty) });
            MainGrid.RowDefinitions[NextRow].SetBinding(RowDefinition.MinHeightProperty, new Binding() { Source = PanelsContainer, Path = new PropertyPath(NixxisGrid.MinimumPanelHeightProperty) });

            this.Cursor = Cursors.SizeNS;
            (m_ExpandStoryboard.Children[0] as GridLengthAnimation).To = m_LastRowHeight;
            BeginStoryboard(m_ExpandStoryboard);

        }
        public void MinimizeBottomPanel()
        {
            SetValue(MaximizeAllowedProperty, true);
            SetValue(MinimizeAllowedProperty, false);
            SetValue(RestoreAllowedProperty, true);
            this.Cursor = Cursors.Arrow;
            m_LastRowHeight = MainGrid.RowDefinitions[NextRow].Height;
            MainGrid.RowDefinitions[NextRow].MinHeight = CollapsedPanelHeight;
            MainGrid.RowDefinitions[NextRow].MaxHeight = CollapsedPanelHeight;
            (m_ExpandStoryboard.Children[0] as GridLengthAnimation).To = new GridLength(0);
            BeginStoryboard(m_ExpandStoryboard);

            foreach (UIElement uie in PanelsContainer.Children)
            {
                if (uie is NixxisBaseExpandPanel)
                {
                    (uie as NixxisBaseExpandPanel).Minimized = true;
                }
            }

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            try
            {
                ResizeDirection = GridResizeDirection.Rows;
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                m_ExpandStoryboard = new Storyboard();
                GridLengthAnimation AnimateHeight = new GridLengthAnimation();
                AnimateHeight.Duration = TimeSpan.FromMilliseconds(50);
                Storyboard.SetTargetProperty(AnimateHeight, new PropertyPath(RowDefinition.HeightProperty));

                Storyboard.SetTarget(AnimateHeight, MainGrid.RowDefinitions[NextRow]);
                AnimateHeight.AccelerationRatio = 0.1;
                AnimateHeight.DecelerationRatio = 0.9;
                AnimateHeight.EasingFunction = new ElasticEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut, Oscillations = 1, Springiness = 5 };
                m_ExpandStoryboard.Children.Add(AnimateHeight);

                CollapseButton = new Button();
                CollapseButton.Template = CollapseButtonTemplate;
                CollapseButton.Click += new RoutedEventHandler(CollapseButton_Click);
                ExpandButton = new Button();
                ExpandButton.Template = ExpandButtonTemplate;
                ExpandButton.Click += new RoutedEventHandler(ExpandButton_Click);
                RestoreButton = new Button();
                RestoreButton.Template = RestoreButtonTemplate;
                RestoreButton.Click += new RoutedEventHandler(RestoreButton_Click);

            }
            catch
            {
            }

        }

        void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreBottomPanel();
        }

        void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeBottomPanels();
        }

        void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            MinimizeBottomPanel();
        }
    }

    public class NixxisSimpleGridSplitter : GridSplitter
    {
        public static readonly DependencyProperty MaximizeTopAllowedProperty = DependencyProperty.Register("MaximizeTopAllowed", typeof(bool), typeof(NixxisSimpleGridSplitter), new PropertyMetadata(true));
        public static readonly DependencyProperty MaximizeBottomAllowedProperty = DependencyProperty.Register("MaximizeBottomAllowed", typeof(bool), typeof(NixxisSimpleGridSplitter), new PropertyMetadata(true));
        public static readonly DependencyProperty RestoreAllowedProperty = DependencyProperty.Register("RestoreAllowed", typeof(bool), typeof(NixxisSimpleGridSplitter), new PropertyMetadata(false));

        public static readonly DependencyProperty MaximizeTopButtonProperty = DependencyProperty.Register("MaximizeTopButton", typeof(Button), typeof(NixxisSimpleGridSplitter));
        public static readonly DependencyProperty RestoreButtonProperty = DependencyProperty.Register("RestoreButton", typeof(Button), typeof(NixxisSimpleGridSplitter));
        public static readonly DependencyProperty MaximizeBottomButtonProperty = DependencyProperty.Register("MaximizeBottomButton", typeof(Button), typeof(NixxisSimpleGridSplitter));

        public static readonly DependencyProperty MaximizeTopButtonTemplateProperty = DependencyProperty.Register("MaximizeTopButtonTemplate", typeof(ControlTemplate), typeof(NixxisSimpleGridSplitter), new PropertyMetadata(null, new PropertyChangedCallback(MaximizeTopButtonTemplateChanged)));
        public static readonly DependencyProperty RestoreButtonTemplateProperty = DependencyProperty.Register("RestoreButtonTemplate", typeof(ControlTemplate), typeof(NixxisSimpleGridSplitter), new PropertyMetadata(null, new PropertyChangedCallback(RestoreButtonTemplateChanged)));
        public static readonly DependencyProperty MaximizeBottomButtonTemplateProperty = DependencyProperty.Register("MaximizeBottomButtonTemplate", typeof(ControlTemplate), typeof(NixxisSimpleGridSplitter), new PropertyMetadata(null, new PropertyChangedCallback(MaximizeBottomButtonTemplateChanged)));

        public static void MaximizeTopButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisSimpleGridSplitter btn = obj as NixxisSimpleGridSplitter;
            if (btn != null && btn.MaximizeTopButton != null)
                btn.MaximizeTopButton.Template = args.NewValue as ControlTemplate;
        }
        public static void RestoreButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisSimpleGridSplitter btn = obj as NixxisSimpleGridSplitter;
            if (btn != null && btn.RestoreButton != null)
                btn.RestoreButton.Template = args.NewValue as ControlTemplate;
        }
        public static void MaximizeBottomButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisSimpleGridSplitter btn = obj as NixxisSimpleGridSplitter;
            if (btn != null && btn.MaximizeBottomButton != null)
                btn.MaximizeBottomButton.Template = args.NewValue as ControlTemplate;
        }

        public bool MaximizeTopAllowed
        {
            get
            {
                return (bool)GetValue(MaximizeTopAllowedProperty);
            }
        }
        public bool MaximizeBottomAllowed
        {
            get
            {
                return (bool)GetValue(MaximizeBottomAllowedProperty);
            }
        }
        public bool RestoreAllowed
        {
            get
            {
                return (bool)GetValue(RestoreAllowedProperty);
            }
        }        

        public Button MaximizeTopButton
        {
            get
            {
                return (Button)GetValue(MaximizeTopButtonProperty);
            }
            set
            {
                SetValue(MaximizeTopButtonProperty, value);
            }
        }
        public Button RestoreButton
        {
            get
            {
                return (Button)GetValue(RestoreButtonProperty);
            }
            set
            {
                SetValue(RestoreButtonProperty, value);
            }
        }
        public Button MaximizeBottomButton
        {
            get
            {
                return (Button)GetValue(MaximizeBottomButtonProperty);
            }
            set
            {
                SetValue(MaximizeBottomButtonProperty, value);
            }
        }

        public ControlTemplate MaximizeTopButtonTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(MaximizeTopButtonTemplateProperty);
            }
            set
            {
                SetValue(MaximizeTopButtonTemplateProperty, value);
            }
        }
        public ControlTemplate RestoreButtonTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(RestoreButtonTemplateProperty);
            }
            set
            {
                SetValue(RestoreButtonTemplateProperty, value);
            }
        }
        public ControlTemplate MaximizeBottomButtonTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(MaximizeBottomButtonTemplateProperty);
            }
            set
            {
                SetValue(MaximizeBottomButtonTemplateProperty, value);
            }
        }


        private Grid MainGrid
        {
            get
            {
                return this.Parent as Grid;
            }
        }
        private int TopRow
        {
            get
            {
                return Grid.GetRow(this) - 1;
            }
        }
        private int BottomRow
        {
            get
            {
                return Grid.GetRow(this) + 1;
            }
        }


        private static SortedList<string, GridLength> m_TopRowHeights = new SortedList<string, GridLength>();
        private static SortedList<string, GridLength> m_BottomRowHeights = new SortedList<string, GridLength>();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


            if (m_TopRowHeights.ContainsKey(this.Name))
            {
                MainGrid.RowDefinitions[TopRow].Height = m_TopRowHeights[this.Name];
            }
            else
            {
                m_TopRowHeights.Add(this.Name, MainGrid.RowDefinitions[TopRow].Height);
            }

            if (m_BottomRowHeights.ContainsKey(this.Name))
            {
                MainGrid.RowDefinitions[BottomRow].Height = m_BottomRowHeights[this.Name];
            }
            else
            {
                m_BottomRowHeights.Add(this.Name, MainGrid.RowDefinitions[BottomRow].Height);
            }


            MaximizeBottomButton = new Button();
            MaximizeBottomButton.Template = MaximizeBottomButtonTemplate;
            MaximizeBottomButton.Click += new RoutedEventHandler(MaximizeBottomButton_Click);

            MaximizeTopButton = new Button();
            MaximizeTopButton.Template = MaximizeTopButtonTemplate;
            MaximizeTopButton.Click += new RoutedEventHandler(MaximizeTopButton_Click);

            RestoreButton = new Button();
            RestoreButton.Template = RestoreButtonTemplate;
            RestoreButton.Click += new RoutedEventHandler(RestoreButton_Click);
        }

        public void MaximizeTop()
        {
            SetValue(MaximizeTopAllowedProperty, false);
            SetValue(MaximizeBottomAllowedProperty, true);
            SetValue(RestoreAllowedProperty, true);


            MainGrid.RowDefinitions[TopRow].Height = m_TopRowHeights[this.Name];
            m_BottomRowHeights[this.Name] = MainGrid.RowDefinitions[BottomRow].Height;
            MainGrid.RowDefinitions[BottomRow].Height = new GridLength(0);
        }

        public void Restore()
        {
            SetValue(MaximizeTopAllowedProperty, true);
            SetValue(MaximizeBottomAllowedProperty, true);
            SetValue(RestoreAllowedProperty, false);

            MainGrid.RowDefinitions[TopRow].Height = m_TopRowHeights[this.Name];
            MainGrid.RowDefinitions[BottomRow].Height = m_BottomRowHeights[this.Name];
        }

        public void MaximizeBottom()
        {
            SetValue(MaximizeTopAllowedProperty, true);
            SetValue(MaximizeBottomAllowedProperty, false);
            SetValue(RestoreAllowedProperty, true);


            MainGrid.RowDefinitions[BottomRow].Height = m_BottomRowHeights[this.Name];
            m_TopRowHeights[this.Name] = MainGrid.RowDefinitions[TopRow].Height;

            MainGrid.RowDefinitions[TopRow].Height = new GridLength(0);
        }

        void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Restore();
        }

        void MaximizeBottomButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeBottom();
        }

        void MaximizeTopButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeTop();
        }

        protected override void OnDraggingChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnDraggingChanged(e);

            m_BottomRowHeights[this.Name] = new GridLength(MainGrid.RowDefinitions[BottomRow].Height.Value, GridUnitType.Star);
            m_TopRowHeights[this.Name] = new GridLength(MainGrid.RowDefinitions[TopRow].Height.Value, GridUnitType.Star);

            if (MainGrid.RowDefinitions[BottomRow].MinHeight == MainGrid.RowDefinitions[BottomRow].ActualHeight)
            {
                SetValue( MaximizeTopAllowedProperty, false);
            }
            else
            {
                SetValue(MaximizeTopAllowedProperty , true);
            }

            if (MainGrid.RowDefinitions[TopRow].MinHeight == MainGrid.RowDefinitions[TopRow].ActualHeight)
            {
                SetValue(MaximizeBottomAllowedProperty , false);
            }
            else
            {
                SetValue(MaximizeBottomAllowedProperty, true);
            }

            if (MaximizeBottomAllowed && MaximizeTopAllowed)
                SetValue(RestoreAllowedProperty, false);
        }

    }

    public class PresentationGrid : Grid
    {
        public enum Meaning
        {
            None,
            Filter,
            List,
            Properties
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(PresentationGrid), new PropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback( OrientationChanged)));

        public static readonly DependencyProperty MeaningProperty = DependencyProperty.RegisterAttached("Meaning", typeof(Meaning), typeof(PresentationGrid), new PropertyMetadata(Meaning.None, new PropertyChangedCallback(MeaningChanged)));

        public static void OrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            PresentationGrid grd = obj as PresentationGrid;
            if (grd != null)
            {
                grd.CheckLayout();
            }
        }

        public static void MeaningChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            PresentationGrid grd = (obj as FrameworkElement).Parent as PresentationGrid;
            if (grd != null)
            {
                UIElement uie = (UIElement)obj;
                Meaning meaning = GetMeaning(uie);
                switch (meaning)
                {
                    case Meaning.Filter:
                        grd.Children.Remove(uie);
                        grd.m_FilterStackPanel.Children.Add(new Label() { VerticalAlignment = System.Windows.VerticalAlignment.Center, Content = TranslationContext.Default.Translate("Filter: ") });
                        grd.m_FilterStackPanel.Children.Add(uie);
                        break;
                    case Meaning.None:
                        break;
                    case Meaning.List:
                        grd.Children.Remove(uie);
                        grd.m_FilterGrid.Children.Add(uie);
                        Grid.SetRow(uie, 1);
                        break;
                    case Meaning.Properties:
                        if (grd.Orientation == Orientation.Horizontal)
                        {
                            Grid.SetRow(uie, 0);
                            Grid.SetColumn(uie, 2);
                        }
                        else
                        {
                            Grid.SetColumn(uie, 0);
                            Grid.SetRow(uie, 2);
                        }
                        break;
                }
            }
        }

        public Orientation Orientation
        {
            get{
                return (Orientation) GetValue(OrientationProperty);
            }
            set{
                SetValue(OrientationProperty, value);
            }
        }

        public static void SetMeaning(UIElement element, Meaning value)
        {
            element.SetValue(MeaningProperty, value);
        }

        public static Meaning GetMeaning(UIElement element)
        {
            return (Meaning)(element.GetValue(MeaningProperty));
        }


        private StackPanel m_FilterStackPanel;
        private Grid m_FilterGrid;
        private GridSplitter m_Splitter;

        private void CheckLayout()
        {
            if (Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                RowDefinitions.Clear();
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(34, GridUnitType.Star) });
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(66, GridUnitType.Star) });

                if (m_Splitter != null)
                    Children.Remove(m_Splitter);
                m_Splitter = new GridSplitter();
                m_Splitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                Grid.SetColumn(m_Splitter, 1);
                m_Splitter.ResizeDirection = GridResizeDirection.Columns;
                m_Splitter.Width = 5;
                m_Splitter.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                m_Splitter.Background = new SolidColorBrush(Colors.Transparent);
                Children.Add(m_Splitter);

                foreach (UIElement elm in Children)
                {
                    if (GetRow(elm) == 2)
                    {
                        SetRow(elm, 0);
                        SetColumn(elm, 2);
                    }
                }
            }
            else
            {
                ColumnDefinitions.Clear();
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(34, GridUnitType.Star) });
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(66, GridUnitType.Star) });

                if(m_Splitter!=null)
                    Children.Remove(m_Splitter);
                m_Splitter = new GridSplitter();
                m_Splitter.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                Grid.SetRow(m_Splitter, 1);
                m_Splitter.ResizeDirection = GridResizeDirection.Rows;
                m_Splitter.Height = 5;
                m_Splitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                m_Splitter.Background = new SolidColorBrush(Colors.Transparent);
                Children.Add(m_Splitter);

                foreach (UIElement elm in Children)
                {
                    if (GetColumn(elm) == 2)
                    {
                        SetRow(elm, 2);
                        SetColumn(elm, 0);
                    }
                }
            }
        }
        public override void BeginInit()
        {
            base.BeginInit();

            CheckLayout();

            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            border.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x40, 0x40, 0x40));
            border.Padding = new Thickness(3);
            border.BorderThickness = new Thickness(0.6);
            border.CornerRadius = new CornerRadius(5);
            border.Margin = new Thickness(1, 1, 3, 3);
            m_FilterGrid = new Grid();
            border.Child = m_FilterGrid;
            m_FilterGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            m_FilterGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            m_FilterStackPanel = new StackPanel();
            m_FilterStackPanel.Orientation = Orientation.Horizontal;
            m_FilterGrid.Children.Add(m_FilterStackPanel);            
            Children.Add(border);

            
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
