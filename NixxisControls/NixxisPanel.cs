using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Collections;
using System.Collections.Specialized;

namespace Nixxis.Client.Controls
{

    public abstract class NixxisBasePanel : Panel
    {
        static NixxisBasePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisBasePanel), new FrameworkPropertyMetadata(typeof(NixxisBasePanel)));
        }

        public static readonly RoutedEvent HeightToShowContentChangedEvent = EventManager.RegisterRoutedEvent("HeightToShowContentChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBasePanel));

        public static readonly DependencyProperty PriorityProperty = DependencyProperty.RegisterAttached("Priority", typeof(int), typeof(NixxisBasePanel), new PropertyMetadata(0));
        public static readonly DependencyProperty HeightToShowContentProperty = DependencyProperty.Register("HeightToShowContent", typeof(double), typeof(NixxisBasePanel), new PropertyMetadata(new PropertyChangedCallback(HeightToShowContentChanging)));
        public static readonly DependencyProperty TopToShowContentProperty = DependencyProperty.Register("TopToShowContent", typeof(double), typeof(NixxisBasePanel));
        public static readonly DependencyProperty HiddenContentProperty = DependencyProperty.Register("HiddenContent", typeof(bool), typeof(NixxisBasePanel));
        public static readonly DependencyProperty ItemsWidthProperty = DependencyProperty.Register("ItemsWidth", typeof(double), typeof(NixxisBasePanel));
        public static readonly DependencyProperty ItemsHeightProperty = DependencyProperty.Register("ItemsHeight", typeof(double), typeof(NixxisBasePanel));
        public static readonly DependencyProperty MinimizedProperty = DependencyProperty.Register("Minimized", typeof(bool), typeof(NixxisBasePanel));


        public bool Minimized
        {
            get
            {
                return (bool)GetValue(MinimizedProperty);
            }
            set
            {
                SetValue(MinimizedProperty, value);
            }
        }
        public double ItemsWidth
        {
            get
            {
                return (double)GetValue(ItemsWidthProperty);
            }
            set
            {
                SetValue(ItemsWidthProperty, value);
            }
        }
        public double ItemsHeight
        {
            get
            {
                return (double)GetValue(ItemsHeightProperty);
            }
            set
            {
                SetValue(ItemsHeightProperty, value);
            }
        }
        public bool HiddenContent
        {
            get
            {
                return (bool)GetValue(HiddenContentProperty);
            }
            set
            {
                SetValue(HiddenContentProperty, value);
            }
        }
        public double HeightToShowContent
        {
            get
            {
                return (double)GetValue(HeightToShowContentProperty);
            }
            set
            {
                SetValue(HeightToShowContentProperty, value);
            }
        }
        public double TopToShowContent
        {
            get
            {
                return (double)GetValue(TopToShowContentProperty);
            }
            set
            {
                SetValue(TopToShowContentProperty, value);
            }
        }


        public static void HeightToShowContentChanging(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBasePanel nbp = obj as NixxisBasePanel;
            nbp.RaiseEvent(new RoutedEventArgs(NixxisBasePanel.HeightToShowContentChangedEvent));
        }

        public event RoutedEventHandler HeightToShowContentChanged
        {
            add { AddHandler(HeightToShowContentChangedEvent, value); }
            remove { RemoveHandler(HeightToShowContentChangedEvent, value); }
        }

        public static void SetPriority(UIElement element, Int32 value)
        {
            element.SetValue(PriorityProperty, value);
        }
        public static int GetPriority(UIElement element)
        {
            return (int)(element.GetValue(PriorityProperty));
        }
    }

    public class NixxisPriorityPanel : NixxisBasePanel
    {
        static NixxisPriorityPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisPriorityPanel), new FrameworkPropertyMetadata(typeof(NixxisPriorityPanel)));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int numbuttonVertical = (int)(finalSize.Height / ItemsHeight);
            int numbuttonHorizontal = (int)(finalSize.Width / ItemsWidth);
            int numVisible = numbuttonVertical * numbuttonHorizontal;

            int currentColumn = 0;
            int currentRow = 0;

            int collapsedChild = 0;

            foreach (UIElement uie in Children)
            {
                if (uie.Visibility == System.Windows.Visibility.Collapsed)
                    collapsedChild++;
                else if (uie is StackPanel && ((StackPanel)uie).Children.Count == 1 && ((StackPanel)uie).Children[0].Visibility==Visibility.Collapsed)
                    collapsedChild++;
            }

            if (numbuttonHorizontal > 0)
            {                
                HeightToShowContent = Math.Ceiling((double)(Children.Count -collapsedChild ) / numbuttonHorizontal) * ItemsHeight;
                TopToShowContent = -HeightToShowContent;
            }

            bool SomethingHidden = false;

            Dictionary<UIElement, bool> visibilities = new Dictionary<UIElement, bool>();

            foreach (UIElement ctrl in (Children.Cast<UIElement>()).OrderByDescending(elm => (int)(elm.GetValue(PriorityProperty))))
            {
                if (ctrl.Visibility != Visibility.Collapsed 
                   && !(ctrl is StackPanel && ((StackPanel)ctrl).Children.Count == 1 && ((StackPanel)ctrl).Children[0].Visibility == Visibility.Collapsed)
                    )
                {

                    if (numVisible > 0)
                    {
                        numVisible--;
                        visibilities.Add(ctrl, true);

                    }
                    else
                    {
                        visibilities.Add(ctrl, false);
                        SomethingHidden = true;
                    }
                }
            }

            currentColumn = 0;

            foreach (UIElement ctrl in Children)
            {
                if (ctrl.Visibility != Visibility.Collapsed 
                    && !(ctrl is StackPanel && ((StackPanel)ctrl).Children.Count == 1 && ((StackPanel)ctrl).Children[0].Visibility == Visibility.Collapsed)
                    && visibilities[ctrl])
                {
                    if (currentColumn >= numbuttonHorizontal)
                    {
                        currentRow++;
                        currentColumn = 0;
                    }

                    ctrl.Arrange(new Rect(new Point(currentColumn * ItemsWidth, currentRow * ItemsHeight), new Size(ItemsWidth, ItemsHeight)));

                    currentColumn++;
                }
                else
                {
                    ctrl.Arrange(new Rect(0, 0, 0, 0));
                }
            }

            HiddenContent = SomethingHidden;
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            return availableSize;
        }
    }

    public class NixxisCoverFlowPanelDefaultTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NotCovered { get; set; }
        public DataTemplate CoveredOnTop { get; set; }
        public DataTemplate CoveredOnBottom { get; set; }
        public DataTemplate FullView { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try
            {
                NixxisCoverFlowPanel.CoverFlowState cfs = NixxisCoverFlowPanel.GetCovered(container as UIElement);
                switch (cfs)
                {
                    case NixxisCoverFlowPanel.CoverFlowState.FullView:
                        return FullView;
                    case NixxisCoverFlowPanel.CoverFlowState.NotCovered:
                        return NotCovered;
                    case NixxisCoverFlowPanel.CoverFlowState.CoveredOnBottom:
                        return CoveredOnBottom;
                    case NixxisCoverFlowPanel.CoverFlowState.CoveredOnTop:
                        return CoveredOnTop;
                }
            }
            catch
            {
            }
            return base.SelectTemplate(item, container);
        }
    }

    public class NixxisCoverFlowPanel : NixxisBasePanel
    {

        static NixxisCoverFlowPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisCoverFlowPanel), new FrameworkPropertyMetadata(typeof(NixxisCoverFlowPanel)));
        }

        private int m_SelectedMargin = 5;

        public enum CoverFlowState
        {
            FullView,
            NotCovered,
            CoveredOnTop,
            CoveredOnBottom
        }


        public static readonly DependencyProperty CoveredProperty = DependencyProperty.RegisterAttached("Covered", typeof(CoverFlowState), typeof(NixxisCoverFlowPanel), new PropertyMetadata(CoverFlowState.NotCovered, new PropertyChangedCallback(CoveredChanged)));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(NixxisCoverFlowPanel), new PropertyMetadata(false, new PropertyChangedCallback(IsSelectedChanged)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(NixxisCoverFlowPanel), new PropertyMetadata(null, new PropertyChangedCallback(SelectedItemChanged)));

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisCoverFlowPanel));

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public static void CoveredChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DependencyObject dep = obj;
            DependencyObject nixxisCoverFlowPanelChild = null;

            while ((dep != null) && !(dep is NixxisCoverFlowPanel))
            {
                nixxisCoverFlowPanelChild = dep;
                dep = VisualTreeHelper.GetParent(dep);
            }

            NixxisCoverFlowPanel thisExpandPanel = (NixxisCoverFlowPanel)dep;

            if (nixxisCoverFlowPanelChild is ContentPresenter)
            {
                ContentPresenter cp = (ContentPresenter)nixxisCoverFlowPanelChild;
                DataTemplateSelector backup = cp.ContentTemplateSelector;
                cp.ContentTemplateSelector = null;
                cp.ContentTemplateSelector = backup;
            }
        }

        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisCoverFlowPanel thisExpandPanel = (NixxisCoverFlowPanel)obj;
            if (args.NewValue == null)
            {
                Helpers.ApplyToChildren<FrameworkElement>(thisExpandPanel, NixxisCoverFlowPanel.IsSelectedProperty, false, (fe) => (NixxisCoverFlowPanel.GetIsSelected(fe)));
            }
            else
            {
                Helpers.ApplyToChildren<FrameworkElement>(thisExpandPanel, NixxisCoverFlowPanel.IsSelectedProperty, true, (fe) => ((fe as ContentPresenter).Content == args.NewValue && !NixxisCoverFlowPanel.GetIsSelected(fe)));
            }
        }
        public static void IsSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DependencyObject dep = obj;
            DependencyObject nixxisCoverFlowPanelChild = null;
            while ((dep != null) && !(dep is NixxisCoverFlowPanel))
            {
                nixxisCoverFlowPanelChild = dep;
                dep = VisualTreeHelper.GetParent(dep);
            }

            NixxisCoverFlowPanel thisExpandPanel = (NixxisCoverFlowPanel)dep;

            bool selected = (bool)args.NewValue;
            if (selected)
            {
                foreach (UIElement ctrl in thisExpandPanel.Children)
                {
                    if (ctrl != nixxisCoverFlowPanelChild && GetIsSelected(ctrl))
                    {
                        SetIsSelected(ctrl, false);
                    }
                }
                ContentPresenter cp = nixxisCoverFlowPanelChild as ContentPresenter;
                if (cp!=null)
                {
                    thisExpandPanel.SelectedItem = cp.Content;
                }
            }
            else
            {
                if (thisExpandPanel.SelectedItem == ((ContentPresenter)nixxisCoverFlowPanelChild).Content && !GetIsSelected(nixxisCoverFlowPanelChild as UIElement))
                {
                    SetIsSelected(nixxisCoverFlowPanelChild as UIElement, true);
                    return;
                }

            }


            thisExpandPanel.RaiseEvent(new RoutedEventArgs(NixxisCoverFlowPanel.SelectionChangedEvent));

            thisExpandPanel.InvalidateVisual();
        }

        public static void SetCovered(UIElement element, CoverFlowState value)
        {
            element.SetValue(CoveredProperty, value);
        }
        public static CoverFlowState GetCovered(UIElement element)
        {
            return (CoverFlowState)(element.GetValue(CoveredProperty));
        }
        public static void SetIsSelected(UIElement element, bool value)
        {
            element.SetValue(IsSelectedProperty, value);
        }
        public static bool GetIsSelected(UIElement element)
        {
            return (bool)(element.GetValue(IsSelectedProperty));
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
        protected override Size ArrangeOverride(Size finalSize)
        {
            
            int indexSelected = -1;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement ctrl = Children[i];
                if (GetIsSelected(ctrl))
                {
                    indexSelected = i;
                    break;
                }
            }

            if (finalSize.Height < ItemsHeight)
                return finalSize;

            if (Children.Count == 1)
            {
                // will be done later...
            }
            else
            {
                HeightToShowContent = Math.Ceiling((double)Children.Count) * ItemsHeight;
                TopToShowContent = -HeightToShowContent;
            }

            double smallWidth = /*ActualWidth*/finalSize.Width - 2 * m_SelectedMargin;

            if (finalSize.Height >= HeightToShowContent)
            {
                if (Children.Count == 1)
                {
                    SetCovered(Children[0], CoverFlowState.FullView);

                    Children[0].Measure(new Size(finalSize.Width, double.MaxValue));
                    Size desire = Children[0].DesiredSize;
                    HeightToShowContent = desire.Height;
                    TopToShowContent = -HeightToShowContent;

                    Children[0].Arrange(new Rect(new Point(0, 0), finalSize));
                    HiddenContent = (desire.Height > finalSize.Height);
                }
                else
                {
                    HiddenContent = true;

                    int currentRow = 0;
                    foreach (UIElement ctrl in Children)
                    {
                        if (ctrl.Visibility != System.Windows.Visibility.Collapsed)
                        {
                            SetCovered(ctrl, CoverFlowState.NotCovered);
                            if (GetIsSelected(ctrl))
                            {
                                ctrl.Measure(new Size(/*ActualWidth*/finalSize.Width, ItemsHeight));
                                ctrl.Arrange(new Rect(new Point(0, currentRow * ItemsHeight), new Size(/*ActualWidth*/finalSize.Width, ItemsHeight)));
                            }
                            else
                            {
                                ctrl.Measure(new Size(smallWidth, ItemsHeight));
                                ctrl.Arrange(new Rect(new Point(m_SelectedMargin, currentRow * ItemsHeight), new Size(smallWidth, ItemsHeight)));
                            }
                            ctrl.RenderTransform = null;
                            currentRow++;
                        }
                    }
                }
            }
            else
            {
                if (Children.Count == 1)
                {
                    SetCovered(Children[0], CoverFlowState.FullView);

                    Children[0].Measure(new Size(finalSize.Width, double.MaxValue));
                    Size desire = Children[0].DesiredSize;
                    HeightToShowContent = desire.Height;
                    TopToShowContent = -HeightToShowContent;

                    Children[0].Arrange(new Rect(new Point(0, 0), finalSize));
                    HiddenContent = (desire.Height > finalSize.Height);
                }
                else
                {
                    HiddenContent = true;

                    int count = Children.Count;
                    int stepcount = 0;
                    double step = (finalSize.Height - ItemsHeight) / (Children.Count - 1);
                    int zIndex = Children.Count - indexSelected;

                    double min = 0.9;

                    for (int i = 0; i < indexSelected; i++)
                    {
                        UIElement ctrl = Children[i];
                        if (ctrl.Visibility != System.Windows.Visibility.Collapsed)
                        {
                            SetCovered(ctrl, CoverFlowState.CoveredOnBottom);
                            if (/*ActualWidth*/finalSize.Width == 0)
                                continue;

                            Panel.SetZIndex(ctrl, zIndex);
                            zIndex++;

                            ctrl.Measure(new Size(smallWidth, ItemsHeight));
                            double scale = (1 - min) / indexSelected * (i) + min;

                            ctrl.Arrange(new Rect(new Point(m_SelectedMargin, stepcount * step), new Size(smallWidth, ItemsHeight)));
                            if (indexSelected != -1)
                                ctrl.RenderTransform = new ScaleTransform(scale, scale, smallWidth / 2, ItemsHeight / 2);
                            count--;
                            stepcount++;
                        }

                    }
                    if (indexSelected != -1)
                    {
                        UIElement ctrl = Children[indexSelected];
                        if (ctrl.Visibility != System.Windows.Visibility.Collapsed)
                        {
                            SetCovered(ctrl, CoverFlowState.NotCovered);
                            Panel.SetZIndex(ctrl, zIndex);
                            zIndex--;

                            ctrl.Measure(new Size(/*ActualWidth*/finalSize.Width, ItemsHeight));

                            ctrl.Arrange(new Rect(new Point(0, stepcount * step), new Size(/*ActualWidth*/finalSize.Width, ItemsHeight)));
                            ctrl.RenderTransform = null;
                            count--;
                            stepcount++;
                        }
                    }
                    for (int i = indexSelected + 1; i < Children.Count; i++)
                    {
                        UIElement ctrl = Children[i];
                        if (ctrl.Visibility != System.Windows.Visibility.Collapsed)
                        {
                            if (/*ActualWidth*/finalSize.Width == 0)
                                continue;

                            SetCovered(ctrl, CoverFlowState.CoveredOnTop);
                            Panel.SetZIndex(ctrl, zIndex);
                            zIndex--;

                            ctrl.Measure(new Size(smallWidth, ItemsHeight));

                            double scale = (min - 1) / (Children.Count - 1 - indexSelected) * (i) + (1 - (min - 1) / (Children.Count - 1 - indexSelected) * indexSelected);

                            ctrl.Arrange(new Rect(new Point(m_SelectedMargin, stepcount * step), new Size(smallWidth, ItemsHeight)));
                            if (indexSelected != -1)
                                ctrl.RenderTransform = new ScaleTransform(scale, scale, smallWidth / 2, ItemsHeight / 2);

                            count--;
                            stepcount++;
                        }
                    }

                }
            }


            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            Size returnVal = new Size(availableSize.Width, availableSize.Height);

            if (returnVal.Height == double.PositiveInfinity)
                returnVal.Height = double.MaxValue;

            if (returnVal.Width == double.PositiveInfinity)
                returnVal.Width = double.MaxValue;

            return returnVal;
        }
    }

    public abstract class NixxisBaseExpandPanel : Grid
    {
        public class NixxisUIElementCollection : UIElementCollection
        {
            public delegate int ElementAddedEventHandler(object sender, ElementAddedEventsArgs args);

            public class ElementAddedEventsArgs : EventArgs
            {
                public UIElement element { get; set; }
            }

            public NixxisUIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
                : base(visualParent, logicalParent)
            {
            }

            public override int Add(UIElement e)
            {
                if (Added != null)
                    return Added(this, new ElementAddedEventsArgs() { element = e });
                return base.Add(e);
            }

            public event ElementAddedEventHandler Added;

        }

        public static readonly RoutedEvent MinimumPanelHeightChangedEvent = EventManager.RegisterRoutedEvent("MinimumPanelHeightChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));
        public static readonly RoutedEvent HeightToShowContentChangedEvent = EventManager.RegisterRoutedEvent("HeightToShowContentChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(NixxisBaseExpandPanel), new PropertyMetadata(null, new PropertyChangedCallback(ItemTemplateSelectorChanged)));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(NixxisBaseExpandPanel), new PropertyMetadata(null, new PropertyChangedCallback(ItemTemplateChanged)));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(NixxisBaseExpandPanel), new PropertyMetadata(null, new PropertyChangedCallback(ItemsSourceChanged)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(NixxisBaseExpandPanel), new PropertyMetadata(null, new PropertyChangedCallback(SelectedItemChanged)));
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(UIElementCollection), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty ExpandButtonTemplateProperty = DependencyProperty.Register("ExpandButtonTemplate", typeof(ControlTemplate), typeof(NixxisBaseExpandPanel), new PropertyMetadata(null, new PropertyChangedCallback(ExpandButtonTemplateChanged)));
        public static readonly DependencyProperty HeightToShowContentProperty = DependencyProperty.Register("HeightToShowContent", typeof(double), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty ItemsWidthProperty = DependencyProperty.Register("ItemsWidth", typeof(double), typeof(NixxisBaseExpandPanel), new PropertyMetadata( new PropertyChangedCallback(ItemsWidthChanged)));
        public static readonly DependencyProperty ItemsHeightProperty = DependencyProperty.Register("ItemsHeight", typeof(double), typeof(NixxisBaseExpandPanel), new PropertyMetadata( new PropertyChangedCallback(ItemsHeightChanged)));
        public static readonly DependencyProperty MinimumNumberOfHorizontalItemsProperty = DependencyProperty.Register("MinimumNumberOfHorizontalItems", typeof(int), typeof(NixxisBaseExpandPanel), new PropertyMetadata(1));
        public static readonly DependencyProperty MinimumNumberOfVerticalItemsProperty = DependencyProperty.Register("MinimumNumberOfVerticalItems", typeof(int), typeof(NixxisBaseExpandPanel), new PropertyMetadata(1));        
        public static readonly DependencyProperty MinimumPanelWidthProperty = DependencyProperty.Register("MinimumPanelWidth", typeof(double), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty MinimumPanelHeightProperty = DependencyProperty.Register("MinimumPanelHeight", typeof(double), typeof(NixxisBaseExpandPanel), new PropertyMetadata(new PropertyChangedCallback(MinimumPanelHeightChanging)));
        public static readonly DependencyProperty CollapsedPanelHeightProperty = DependencyProperty.Register("CollapsedPanelHeight", typeof(double), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty MinimizedProperty = DependencyProperty.Register("Minimized", typeof(bool), typeof(NixxisBaseExpandPanel), new PropertyMetadata(false, new PropertyChangedCallback(MinimizedChanged)));
        public static readonly DependencyProperty MinimizedToolTipContentProperty = DependencyProperty.Register("MinimizedToolTipContent", typeof(ToolTip), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty ToolTipContentProperty = DependencyProperty.Register("ToolTipContent", typeof(ToolTip), typeof(NixxisBaseExpandPanel));
        public static readonly DependencyProperty ToolTipPreviewImageProperty = DependencyProperty.Register("ToolTipPreviewImage", typeof(ImageSource), typeof(NixxisBaseExpandPanel));

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public static readonly RoutedEvent ExpandStartingEvent = EventManager.RegisterRoutedEvent("ExpandStarting", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));

        public event RoutedEventHandler ExpandStarting
        {
            add { AddHandler(ExpandStartingEvent, value); }
            remove { RemoveHandler(ExpandStartingEvent, value); }
        }


        public static readonly RoutedEvent ExpandCompletedEvent = EventManager.RegisterRoutedEvent("ExpandCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));

        public event RoutedEventHandler ExpandCompleted
        {
            add { AddHandler(ExpandCompletedEvent, value); }
            remove { RemoveHandler(ExpandCompletedEvent, value); }
        }


        public static readonly RoutedEvent CollapseCompletedEvent = EventManager.RegisterRoutedEvent("CollapseCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));

        public event RoutedEventHandler CollapseCompleted
        {
            add { AddHandler(CollapseCompletedEvent, value); }
            remove { RemoveHandler(CollapseCompletedEvent, value); }
        }


        public static readonly RoutedEvent CollapseStartedEvent = EventManager.RegisterRoutedEvent("CollapseStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisBaseExpandPanel));

        public event RoutedEventHandler CollapseStarted
        {
            add { AddHandler(CollapseStartedEvent, value); }
            remove { RemoveHandler(CollapseStartedEvent, value); }
        }
        public static void ItemTemplateSelectorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBaseExpandPanel thisExpandPanel = (NixxisBaseExpandPanel)obj;
            DataTemplateSelector templateSelector = args.NewValue as DataTemplateSelector;
            foreach (ContentPresenter cp in thisExpandPanel.m_Panel.Children)
            {
                if (cp != null)
                {
                    cp.SetBinding(ContentPresenter.ContentTemplateSelectorProperty, new Binding() { Source = templateSelector });
                }
            }
        }
        public static void ItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBaseExpandPanel thisExpandPanel = (NixxisBaseExpandPanel)obj;
            DataTemplate template = args.NewValue as DataTemplate;
            foreach (ContentPresenter cp in thisExpandPanel.m_Panel.Children)
            {
                if (cp != null)
                {
                    cp.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding() { Source = template });
                }
            }
        }
        public static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBaseExpandPanel thisExpandPanel = (NixxisBaseExpandPanel)obj;
            IEnumerable collection = args.NewValue as IEnumerable;

            if (collection is INotifyCollectionChanged)
            {
                INotifyCollectionChanged notcol = (INotifyCollectionChanged)collection;

                thisExpandPanel.ItemsSourceCollection_CollectionChanged(collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                notcol.CollectionChanged += new NotifyCollectionChangedEventHandler(thisExpandPanel.ItemsSourceCollection_CollectionChanged);
            }
            else
            {
                object[] objs = new object[]{args.NewValue};
                thisExpandPanel.ItemsSourceCollection_CollectionChanged(objs, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBaseExpandPanel thisExpandPanel = (NixxisBaseExpandPanel)obj;
            if (thisExpandPanel != null)
            {
                if (thisExpandPanel.m_Panel is NixxisCoverFlowPanel)
                {
                    ((NixxisCoverFlowPanel)(thisExpandPanel.m_Panel)).SelectedItem = args.NewValue;
                }
            }
        }
        public static void MinimizedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((NixxisBaseExpandPanel)obj).CheckMinimumSizeForPreview();
            ((NixxisBaseExpandPanel)obj).m_Panel.Minimized = ((NixxisBaseExpandPanel)obj).Minimized;
        }
        public static void ItemsWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((NixxisBaseExpandPanel)obj).m_Panel.ItemsWidth = (double)args.NewValue;

            double dbl = (double)((NixxisBaseExpandPanel)obj).GetValue(NixxisBaseExpandPanel.MinimumPanelWidthProperty);
            int num = (int)((NixxisBaseExpandPanel)obj).GetValue(NixxisBaseExpandPanel.MinimumNumberOfHorizontalItemsProperty);
            if ((double)args.NewValue * num > dbl)
                ((NixxisBaseExpandPanel)obj).SetValue(NixxisBaseExpandPanel.MinimumPanelWidthProperty, num * (double)args.NewValue);
        }
        public static void ItemsHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((NixxisBaseExpandPanel)obj).m_Panel.ItemsHeight = (double)args.NewValue;

            double dbl = (double)((NixxisBaseExpandPanel)obj).GetValue(NixxisBaseExpandPanel.MinimumPanelHeightProperty);
            int num = (int)((NixxisBaseExpandPanel)obj).GetValue(NixxisBaseExpandPanel.MinimumNumberOfVerticalItemsProperty);

            if ((double)args.NewValue * num + ((NixxisBaseExpandPanel)obj).m_ToggleButton.ActualHeight > dbl)
            {
                ((NixxisBaseExpandPanel)obj).SetValue(NixxisBaseExpandPanel.MinimumPanelHeightProperty, ((NixxisBaseExpandPanel)obj).m_ToggleButton.ActualHeight + num * (double)args.NewValue);
            }
        }
        public static void ExpandButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBaseExpandPanel nbep = ((NixxisBaseExpandPanel)obj);
            ControlTemplate template = args.NewValue as ControlTemplate;
            if (template != null && nbep != null)
                nbep.m_ToggleButton.Template = template;
        }
        public static void MinimumPanelHeightChanging(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisBaseExpandPanel nbep = ((NixxisBaseExpandPanel)obj);
            nbep.RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.MinimumPanelHeightChangedEvent)); 
        }

        public event RoutedEventHandler MinimumPanelHeightChanged
        {
            add { AddHandler(MinimumPanelHeightChangedEvent, value); }
            remove { RemoveHandler(MinimumPanelHeightChangedEvent, value); }
        }
        public event RoutedEventHandler HeightToShowContentChanged
        {
            add { AddHandler(HeightToShowContentChangedEvent, value); }
            remove { RemoveHandler(HeightToShowContentChangedEvent, value); }
        }


        void ItemsSourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object obj in e.NewItems)
                    {
                        ContentPresenter contentPresenter = new ContentPresenter();

                        contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding() { Source = obj });
                        contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding() { Source = ItemTemplate });
                        contentPresenter.SetBinding(ContentPresenter.ContentTemplateSelectorProperty, new Binding() { Source = ItemTemplateSelector });

                        m_Panel.Children.Add(contentPresenter);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException("TO DO");
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object obj in e.OldItems)
                    {
                        foreach (ContentPresenter cc in m_Panel.Children)
                        {
                            if (cc != null)
                            {
                                Binding binding = BindingOperations.GetBinding(cc, ContentPresenter.ContentProperty);
                                if (binding != null && binding.Source == obj)
                                {
                                    m_Panel.Children.Remove(cc);
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("TO DO");
                    break;

                case NotifyCollectionChangedAction.Reset:

                    for (int i = 0; i < m_Panel.Children.Count; i++)
                    {
                        ContentPresenter cc = m_Panel.Children[i] as ContentPresenter;
                        if (cc != null)
                        {
                            Binding binding = BindingOperations.GetBinding(cc, ContentPresenter.ContentProperty);
                            if (binding != null)
                            {
                                // Not correct when static items added
                                m_Panel.Children.Remove(cc);
                                i--;
                            }
                        }
                    }

                    if (sender != null)
                    {
                        foreach (object obj in (IEnumerable)sender)
                        {
                            ContentPresenter contentPresenter = new ContentPresenter();

                            contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding() { Source = obj });
                            contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding() { Source = ItemTemplate });
                            contentPresenter.SetBinding(ContentPresenter.ContentTemplateSelectorProperty, new Binding() { Source = ItemTemplateSelector });

                            m_Panel.Children.Add(contentPresenter);
                        }
                    }

                    break;
            }

        }

        static NixxisBaseExpandPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisBaseExpandPanel), new FrameworkPropertyMetadata(typeof(NixxisBaseExpandPanel)));
        }

        public object ItemsSource
        {
            get
            {
                return GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
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
        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ItemTemplateProperty);
            }
            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }
        public DataTemplateSelector ItemTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
            }
            set
            {
                SetValue(ItemTemplateSelectorProperty, value);
            }
        }
        public new UIElementCollection Children
        {
            get
            {
                return (UIElementCollection)GetValue(ChildrenProperty);
            }
            set
            {
                SetValue(ChildrenProperty, value);
            }
        }
        public ToolTip MinimizedToolTipContent
        {
            get
            {
                return (ToolTip)GetValue(MinimizedToolTipContentProperty);
            }
            set
            {
                SetValue(MinimizedToolTipContentProperty, value);
            }
        }
        public ToolTip ToolTipContent
        {
            get
            {
                return (ToolTip)GetValue(ToolTipContentProperty);
            }
            set
            {
                SetValue(ToolTipContentProperty, value);
            }
        }
        public ImageSource ToolTipPreviewImage
        {
            get
            {
                return (ImageSource)GetValue(ToolTipPreviewImageProperty);
            }
            set
            {
                SetValue(ToolTipPreviewImageProperty, value);
            }
        }
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }
        public bool Minimized
        {
            get
            {
                return (bool)GetValue(MinimizedProperty);
            }
            set
            {
                SetValue(MinimizedProperty, value);
            }
        }
        public double MinimumPanelWidth
        {
            get
            {
                return (double)GetValue(MinimumPanelWidthProperty);
            }
            set
            {
                SetValue(MinimumPanelWidthProperty, value);
            }
        }
        public int MinimumNumberOfHorizontalItems
        {
            get
            {
                return (int)GetValue(MinimumNumberOfHorizontalItemsProperty);
            }
            set
            {
                SetValue(MinimumNumberOfHorizontalItemsProperty, value);
            }
        }
        public int MinimumNumberOfVerticalItems
        {
            get
            {
                return (int)GetValue(MinimumNumberOfVerticalItemsProperty);
            }
            set
            {
                SetValue(MinimumNumberOfVerticalItemsProperty, value);
            }
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
        public double CollapsedPanelHeight
        {
            get
            {
                return (double)GetValue(CollapsedPanelHeightProperty);
            }
        }
        public double ItemsWidth
        {
            get
            {
                return (double)GetValue(ItemsWidthProperty);
            }
            set
            {
                SetValue(ItemsWidthProperty, value);
            }
        }
        public double ItemsHeight
        {
            get
            {
                return (double)GetValue(ItemsHeightProperty);
            }
            set
            {
                SetValue(ItemsHeightProperty, value);
            }
        }
        public double HeightToShowContent
        {
            get
            {
                return m_Panel.HeightToShowContent + m_ToggleButton.ActualHeight;
            }
        }
        public ControlTemplate ExpandButtonTemplate
        {
            get
            {
                return GetValue(ExpandButtonTemplateProperty) as ControlTemplate;
            }
            set
            {
                SetValue(ExpandButtonTemplateProperty, value);
            }
        }

        private bool m_Expanding = false;
        private Grid m_InnerGrid = new Grid();
        private Storyboard m_CollapseStoryboard = new Storyboard();
        private Storyboard m_ExpandStoryboard = new Storyboard();
        protected NixxisBasePanel m_Panel;
        private ToggleButton m_ToggleButton = new ToggleButton();
        private double m_BackupMinHeight = 0;
        private int m_Opened_Menu = 0;
        private bool m_CloseRequired = false;
        private void CheckMinimumSizeForPreview()
        {
            if (Minimized)
            {
                MinHeight = HeightToShowContent;
            }
            else
            {
                MinHeight = 0;
            }
        }

        public NixxisBaseExpandPanel(NixxisBasePanel childPanel)
        {
            m_Panel = childPanel;
            if (m_Panel is NixxisCoverFlowPanel)
            {
                ((NixxisCoverFlowPanel)(m_Panel)).SelectionChanged += new RoutedEventHandler(NixxisBaseExpandPanel_SelectionChanged);
            }
            m_Panel.HeightToShowContentChanged += new RoutedEventHandler(m_Panel_HeightToShowContentChanged);

            NixxisUIElementCollection nuiec = new NixxisUIElementCollection(this, this);
            nuiec.Added += new NixxisUIElementCollection.ElementAddedEventHandler(nuiec_Added);
            Children = nuiec;

            DoubleAnimation AnimateHeightCollapse = new DoubleAnimation();
            AnimateHeightCollapse.Duration = new Duration(TimeSpan.FromMilliseconds(0));
            Storyboard.SetTargetProperty(AnimateHeightCollapse, new PropertyPath(HeightProperty));
            Storyboard.SetTarget(AnimateHeightCollapse, m_InnerGrid);
            m_CollapseStoryboard.Children.Add(AnimateHeightCollapse);
            m_CollapseStoryboard.Completed += new EventHandler(m_CollapseStoryboard_Completed);

            DoubleAnimation AnimateTopCollapse = new DoubleAnimation();
            AnimateTopCollapse.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            Storyboard.SetTargetProperty(AnimateTopCollapse, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTarget(AnimateTopCollapse, m_InnerGrid);
            AnimateTopCollapse.AccelerationRatio = 0.1;
            AnimateTopCollapse.DecelerationRatio = 0.9;
            AnimateTopCollapse.EasingFunction = new ElasticEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut, Oscillations = 1, Springiness = 5 };
            m_CollapseStoryboard.Children.Add(AnimateTopCollapse);

            DoubleAnimation AnimateHeightExpand = new DoubleAnimation();
            AnimateHeightExpand.Duration = new Duration(TimeSpan.FromMilliseconds(0));
            Storyboard.SetTargetProperty(AnimateHeightExpand, new PropertyPath(HeightProperty));
            Storyboard.SetTarget(AnimateHeightExpand, m_InnerGrid);
            m_ExpandStoryboard.Children.Add(AnimateHeightExpand);
            m_ExpandStoryboard.Completed += new EventHandler(m_ExpandStoryboard_Completed);
            DoubleAnimation AnimateTopExpand = new DoubleAnimation();
            AnimateTopExpand.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            Storyboard.SetTargetProperty(AnimateTopExpand, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTarget(AnimateTopExpand, m_InnerGrid);
            AnimateTopExpand.AccelerationRatio = 0.1;
            AnimateTopExpand.DecelerationRatio = 0.9;
            AnimateTopExpand.EasingFunction = new ElasticEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut, Oscillations = 1, Springiness = 5 };
            m_ExpandStoryboard.Children.Add(AnimateTopExpand);
        }

        void NixxisBaseExpandPanel_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(m_Panel is NixxisCoverFlowPanel)
            {
                this.SelectedItem = ((NixxisCoverFlowPanel)(m_Panel)).SelectedItem;
            }
            RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.SelectionChangedEvent));
        }

        void m_Panel_HeightToShowContentChanged(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.HeightToShowContentChangedEvent));
        }

        int nuiec_Added(object sender, NixxisUIElementCollection.ElementAddedEventsArgs args)
        {
            if (args.element is NixxisButton)
            {
                ((NixxisButton)args.element).DropDownOpening += new RoutedEventHandler(ExNixxisPanel_ContextMenuOpening);
                ((NixxisButton)args.element).DropDownClosing += new RoutedEventHandler(ExNixxisPanel_ContextMenuClosing);
            }

            return m_Panel.Children.Add(args.element);
        }

        void m_ExpandStoryboard_Completed(object sender, EventArgs e)
        {
            m_Panel.Minimized = false;
            m_Expanding = false;
            if (!m_InnerGrid.IsMouseOver)
            {
                grd_MouseLeave(this, null);
            }

            RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.ExpandCompletedEvent));
        }

        void m_CollapseStoryboard_Completed(object sender, EventArgs e)
        {
            MinHeight = m_BackupMinHeight;
            if (Minimized)
            {
                m_Panel.Minimized = true;
            }

            RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.CollapseCompletedEvent));

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Canvas cnv = new Canvas();


            if (!IsItemsHost)
                InternalChildren.Add(cnv);

            m_InnerGrid.MouseLeave += new MouseEventHandler(grd_MouseLeave);

            m_InnerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) { } });
            m_InnerGrid.RowDefinitions.Add(new RowDefinition());

            cnv.Children.Add(m_InnerGrid);
            Canvas.SetTop(m_InnerGrid, 0);


            m_InnerGrid.SetBinding(BackgroundProperty, new Binding("Background") { Source = this });
            m_InnerGrid.SetBinding(HeightProperty, new Binding("ActualHeight") { Source = this });
            m_InnerGrid.SetBinding(WidthProperty, new Binding("ActualWidth") { Source = this });


            m_ToggleButton.SizeChanged += new SizeChangedEventHandler(m_ToggleButton_SizeChanged);


            if (ExpandButtonTemplate != null)
                m_ToggleButton.Template = ExpandButtonTemplate;


            MultiBinding mb = new MultiBinding();
            mb.Bindings.Add(new Binding() { Source = m_Panel, Path = new PropertyPath(NixxisPriorityPanel.HiddenContentProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(NixxisBaseExpandPanel.MinimizedProperty) });
            mb.Converter = new ExpandableConverter();
            m_ToggleButton.SetBinding(TagProperty, mb);

            mb = new MultiBinding();
            mb.Bindings.Add(new Binding() { Source = m_ToggleButton, Path = new PropertyPath(ToggleButton.IsCheckedProperty) });
            mb.Bindings.Add(new Binding() { Source = m_ToggleButton, Path = new PropertyPath(ToggleButton.TagProperty) });
            mb.Converter = new ExpandableVisibilityConverter();
            m_ToggleButton.SetBinding(IsEnabledProperty, mb);
            

            m_InnerGrid.Children.Add(m_ToggleButton);
            Grid.SetRow(m_ToggleButton, 0);
            m_ToggleButton.Checked += new RoutedEventHandler(tb_Checked);
            m_ToggleButton.Unchecked += new RoutedEventHandler(tb_Unchecked);
            m_ToggleButton.ToolTip = new object();
            m_ToggleButton.ToolTipOpening += new ToolTipEventHandler(m_ToggleButton_ToolTipOpening);

            Grid.SetRow(m_Panel, 1);
            m_InnerGrid.Children.Add(m_Panel);
        }

        void ExNixxisPanel_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            m_Opened_Menu++;
        }

        void ExNixxisPanel_ContextMenuClosing(object sender, RoutedEventArgs e)
        {
            m_Opened_Menu--;
            if (m_Opened_Menu == 0 && m_CloseRequired)
            {
                if (!m_InnerGrid.IsMouseOver)
                {
                    m_ToggleButton.IsChecked = false;
                }
                m_CloseRequired = false;
            }
        }

        void m_ToggleButton_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            try
            {
                if (Minimized && m_Panel.Minimized)
                {
                    ToolTipPreviewImage = CaptureScreenBitmap(m_InnerGrid);
                    MinimizedToolTipContent.SetBinding(DataContextProperty, new Binding() { Source = this });
                    m_ToggleButton.ToolTip = MinimizedToolTipContent;
                }
                else
                {
                    ToolTipContent.SetBinding(DataContextProperty, new Binding() { Source = this });
                    m_ToggleButton.ToolTip = ToolTipContent;
                }
            }
            catch
            {
            }

        }

        void m_ToggleButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double minHeight = m_ToggleButton.ActualHeight + ItemsHeight * MinimumNumberOfVerticalItems;
            if( ((double)(GetValue(MinimumPanelHeightProperty))) < minHeight)
                SetValue(MinimumPanelHeightProperty, minHeight);

            SetValue(CollapsedPanelHeightProperty, m_ToggleButton.ActualHeight);
            CheckMinimumSizeForPreview();

            this.RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.HeightToShowContentChangedEvent));
        }

        void tb_Unchecked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.CollapseStartedEvent));
            (m_CollapseStoryboard.Children[0] as DoubleAnimation).From = m_Panel.HeightToShowContent;
            BeginStoryboard(m_CollapseStoryboard);
        }

        void tb_Checked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(NixxisBaseExpandPanel.ExpandStartingEvent));

            m_BackupMinHeight = MinHeight;
            MinHeight = 0;

            UpdateLayout();

            m_Expanding = true;
            (m_ExpandStoryboard.Children[0] as DoubleAnimation).By = m_Panel.HeightToShowContent;
            (m_ExpandStoryboard.Children[1] as DoubleAnimation).By = m_Panel.TopToShowContent + m_Panel.ActualHeight;
            BeginStoryboard(m_ExpandStoryboard);
            
        }

        void grd_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!m_Expanding)
            {
                if (m_Opened_Menu == 0)
                    m_ToggleButton.IsChecked = false;
                else
                    m_CloseRequired = true;
            }
        }

        private BitmapSource CaptureScreenBitmap(Panel panel)
        {

            BitmapSource bs = CaptureScreenBitmap(panel,
            (int)panel.ActualWidth,
            (int)panel.ActualHeight);

            return bs;
        }

        private BitmapSource CaptureScreenBitmap(Visual target, int width, int height)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(target);
                context.DrawRectangle(brush, null, new Rect(new Point(), bounds.Size));
            }
            renderBitmap.Render(visual);
            return renderBitmap;
        }

        public FrameworkElement FindByName(string name)
        {
            return FindByName(name, false);
        }

        public FrameworkElement FindByName(string name, bool recurseDropDownButtons)
        {
            foreach (FrameworkElement fe in (m_InnerGrid.Children[1] as NixxisBasePanel).Children)
            {
                if (fe.Name.Equals(name))
                    return fe;

                if (recurseDropDownButtons && fe is NixxisButton)
                {
                    NixxisButton Btn = (NixxisButton)fe;

                    if (Btn.DropDown != null)
                    {
                        foreach (FrameworkElement Item in Btn.DropDown.Items)
                        {
                            if (Item.Name.Equals(name))
                                return Item;
                        }
                    }
                }
            }
            return null;
        }

    }

    public class ExpandableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)(values[0]) || (bool)(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ExpandableVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool showDownArrow = true;

            try
            {
                if (parameter != null)
                    showDownArrow = bool.Parse(parameter as string);
            }
            catch
            {
            }

            bool isChecked = (bool)(values[0]);
            bool hasHiddenContent = (bool)(values[1]);

            if (targetType == typeof(Visibility))
            {
                if (hasHiddenContent)
                {
                    if (isChecked)
                        return Visibility.Visible;
                    else
                        if (!showDownArrow)
                            return Visibility.Hidden;
                        else
                            return Visibility.Visible;
                }
                else
                {
                    if (isChecked)
                        return Visibility.Visible;
                    else
                        return Visibility.Hidden;
                }
            }
            else
            {
                if (hasHiddenContent)
                {
                    if (isChecked)
                        return true;
                    else
                        if (!showDownArrow)
                            return false;
                        else
                            return true;
                }
                else
                {
                    if (isChecked)
                        return true;
                    else
                        return false;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NixxisExpandPanel : NixxisBaseExpandPanel
    {
        public NixxisExpandPanel()
            : base(new NixxisPriorityPanel())
        {
        }

        static NixxisExpandPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisExpandPanel), new FrameworkPropertyMetadata(typeof(NixxisExpandPanel)));
        }

    }

    public class NixxisExpandCoverFlowPanel : NixxisBaseExpandPanel
    {

        public NixxisExpandCoverFlowPanel()
            : base(new NixxisCoverFlowPanel())
        {
        }

        static NixxisExpandCoverFlowPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisExpandCoverFlowPanel), new FrameworkPropertyMetadata(typeof(NixxisExpandCoverFlowPanel)));
        }
    }

    public class NixxisStackPanel : Panel
    {
        public static readonly DependencyProperty ActAsLabelProperty = DependencyProperty.RegisterAttached("ActAsLabel", typeof(bool), typeof(NixxisStackPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty ActAsLabeledProperty = DependencyProperty.RegisterAttached("ActAsLabeled", typeof(bool), typeof(NixxisStackPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty KeepNextAttachedProperty = DependencyProperty.RegisterAttached("KeepNextAttached", typeof(bool), typeof(NixxisStackPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty GranularityProperty = DependencyProperty.Register("Granularity", typeof(double), typeof(NixxisStackPanel), new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetKeepNextAttached(UIElement element, bool value)
        {
            element.SetValue(KeepNextAttachedProperty, value);
        }
        public static bool GetKeepNextAttached(UIElement element)
        {
            return (bool)(element.GetValue(KeepNextAttachedProperty));
        }

        public static void SetActAsLabel(UIElement element, bool value)
        {
            element.SetValue(ActAsLabelProperty, value);
        }
        public static bool GetActAsLabel(UIElement element)
        {
            return (bool)(element.GetValue(ActAsLabelProperty));
        }

        public static void SetActAsLabeled(UIElement element, bool value)
        {
            element.SetValue(ActAsLabeledProperty, value);
        }
        public static bool GetActAsLabeled(UIElement element)
        {
            return (bool)(element.GetValue(ActAsLabeledProperty));
        }


        public double Granularity
        {
            get
            {
                return (double)GetValue(GranularityProperty);
            }
            set
            {
                SetValue(GranularityProperty, value);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size returnValue = new Size(finalSize.Width, 0);
            int counter = 0;
            bool isAttached = false;


            for (int i = 0; i < Children.Count; i++)
            {
                UIElement uie = (UIElement)Children[i];

                if (uie.Visibility != System.Windows.Visibility.Collapsed)
                {
                    if (!isAttached)
                    {
                        if (!uie.IsMeasureValid)    
                            uie.Measure(finalSize);
                        Size sz = uie.DesiredSize;
                        bool oneRowToSkip = ((uie is Label || GetActAsLabel(uie)) && counter % 2 == 1) || (counter % 2 == 0 && GetActAsLabeled(uie));
                        
                        if (oneRowToSkip)
                            counter++;
                        double computedHeight = ((int)(sz.Height / Granularity) + (oneRowToSkip ? 2 : 1)) * Granularity;
                        double attachedWidth = 0;
                        double attachedWidth2 = 0;
                        UIElement tempuie = null;
                        UIElement tempuie2 = null;
                        Size tempsz = Size.Empty;
                        Size tempsz2 = new Size(0,0);

                        if (GetKeepNextAttached(uie))
                        {
                            tempuie = (UIElement)Children[i + 1];
                            if (!tempuie.IsMeasureValid)
                                tempuie.Measure(finalSize);
                            tempsz = tempuie.DesiredSize;
                            attachedWidth = tempsz.Width;

                            if (GetKeepNextAttached(tempuie))
                            {
                                tempuie2 = (UIElement)Children[i + 2];
                                if (!tempuie2.IsMeasureValid)
                                    tempuie2.Measure(finalSize);
                                tempsz2 = tempuie2.DesiredSize;
                                attachedWidth2 = tempsz2.Width;
                                if (tempuie2.Visibility != System.Windows.Visibility.Visible)
                                {
                                    tempuie2 = null;
                                }
                            }

                            if (tempuie.Visibility != System.Windows.Visibility.Visible)
                            {
                                tempuie = null;
                            }
                            if (tempuie2!=null && tempuie2.Visibility != System.Windows.Visibility.Visible)
                            {
                                tempuie2 = null;
                            }

                        }
                        if (uie.Visibility == System.Windows.Visibility.Visible)
                        {
                            if (uie is Label || GetActAsLabel(uie))
                            {
                                uie.Arrange(new Rect(0, returnValue.Height + computedHeight - sz.Height, returnValue.Width - attachedWidth -attachedWidth2, sz.Height));
                                if(tempuie!=null)
                                    tempuie.Arrange(new Rect(returnValue.Width - tempsz.Width -tempsz2.Width, returnValue.Height + computedHeight - sz.Height, tempsz.Width, sz.Height));
                                if (tempuie2 != null)
                                    tempuie2.Arrange(new Rect(returnValue.Width - tempsz2.Width, returnValue.Height + computedHeight - sz.Height, tempsz2.Width, sz.Height));
                            }
                            else
                            {
                                if (uie is CheckBox || uie is RadioButton || uie is NixxisDetailedCheckBox)
                                {
                                    uie.Arrange(new Rect(0, oneRowToSkip ? returnValue.Height + Granularity + (computedHeight - Granularity - sz.Height) /2  : returnValue.Height + (computedHeight - sz.Height) /2 , returnValue.Width - attachedWidth -attachedWidth2, sz.Height));
                                }
                                else
                                { 
                                    uie.Arrange(new Rect(0, oneRowToSkip ? returnValue.Height + Granularity  : returnValue.Height , returnValue.Width > attachedWidth + attachedWidth2 ?  returnValue.Width - attachedWidth -attachedWidth2: 0, sz.Height));
                                }
    
                                if (tempuie != null)
                                    tempuie.Arrange(new Rect(returnValue.Width - tempsz.Width -tempsz2.Width, oneRowToSkip ? returnValue.Height + Granularity: returnValue.Height, tempsz.Width, sz.Height));
                                
                                if (tempuie2 != null)
                                    tempuie2.Arrange(new Rect(returnValue.Width - tempsz2.Width, oneRowToSkip ? returnValue.Height + Granularity : returnValue.Height, tempsz2.Width, sz.Height));

                            }
                        }
                        counter++;
                        returnValue.Height += computedHeight;
                    }
                    isAttached = GetKeepNextAttached(uie);                    
                }
            }

            if (Double.IsPositiveInfinity(returnValue.Height))
                returnValue.Height = 0;

            if (double.IsPositiveInfinity(returnValue.Width))
                returnValue.Width = 0;

            if (returnValue.Height > finalSize.Height)
                returnValue.Height = finalSize.Height;

            if (returnValue.Width > finalSize.Width)
                returnValue.Width = finalSize.Width;

            return returnValue;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size returnValue = new Size( availableSize.Width, 0);
            bool isAttached = false;
            int counter = 0;
            for(int i=0; i<Children.Count; i++)
            {
                UIElement uie = (UIElement)Children[i];

                if (uie.Visibility != System.Windows.Visibility.Collapsed)
                {
                    if (!isAttached)
                    {
                        // This one seems nice but it is not ok as a TextBlock with active Trimming will return true and will prevent taking care of new size...
                        uie.Measure(new Size(returnValue.Width, Double.PositiveInfinity));
                        Size sz = uie.DesiredSize;
                        bool oneRowToSkip = ((uie is Label || GetActAsLabel(uie)) && counter % 2 == 1) || (counter % 2 == 0 && GetActAsLabeled(uie));

                        if (oneRowToSkip)
                            counter++;

                        double computedHeight = ((int)(sz.Height / Granularity) + (oneRowToSkip ? 2 : 1)) * Granularity;
                        returnValue.Height += computedHeight;

                        counter++;
                    }

                    isAttached = GetKeepNextAttached(uie);
                }
            }

            if (Double.IsPositiveInfinity(returnValue.Height))
                returnValue.Height = 0;

            if (double.IsPositiveInfinity(returnValue.Width))
                returnValue.Width = 0;

            if (returnValue.Height > availableSize.Height)
                returnValue.Height = availableSize.Height;

            if (returnValue.Width > availableSize.Width)
                returnValue.Width = availableSize.Width;

            //System.Diagnostics.Trace.WriteLine(string.Format("Measure return {0}x{1}", returnValue.Width, returnValue.Height), "++++++++");
            return returnValue;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(NixxisStackPanel_IsVisibleChanged);
        }

        void NixxisStackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                BringIntoView();
            }
        }
    }

}
