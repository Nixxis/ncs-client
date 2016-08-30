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
using System.Windows.Media.Animation;

namespace Nixxis.Client.Controls
{
    public class NixxisTileView : Panel
    {
        #region Enums
        public enum DisplayModes
        {
            LargeOnly,
            LargeSmallList,
            MediumOnly,
        }
        #endregion

        #region Class data
        private DisplayModes m_DisplayMode = DisplayModes.LargeSmallList;
        private NixxisTileViewItem m_SelectedItem = null;
        private TimeSpan _AnimationLength = TimeSpan.FromMilliseconds(300);
        private double defaultStandardMargin = 5;
        private double defaultMediumMargin = 25;

        #endregion

        #region Properties
        public DisplayModes DisplayMode
        {
            get { return m_DisplayMode; }
            set { m_DisplayMode = value; }
        }
        public NixxisTileViewItem SelectedItem
        {
            get { return m_SelectedItem; }
            set { m_SelectedItem = value; }
        }
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty SmallContentWidthProperty = DependencyProperty.Register("SmallContentWidth", typeof(double), typeof(NixxisTileView), new PropertyMetadata((double)150));
        public double SmallContentWidth
        {
            get { return (double)GetValue(SmallContentWidthProperty); }
            set { SetValue(SmallContentWidthProperty, value); }
        }

        public static readonly DependencyProperty SmallContentHeightProperty = DependencyProperty.Register("SmallContentHeight", typeof(double), typeof(NixxisTileView), new PropertyMetadata((double)128));
        public double SmallContentHeight
        {
            get { return (double)GetValue(SmallContentHeightProperty); }
            set { SetValue(SmallContentHeightProperty, value); }
        }

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(ControlTemplate), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(HeaderTemplateChanged)));
        public ControlTemplate HeaderTemplate
        {
            get { return GetValue(HeaderTemplateProperty) as ControlTemplate; }
            set { SetValue(HeaderTemplateProperty, value); }
        }
        public static void HeaderTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileView view = obj as NixxisTileView;

            foreach (UIElement item in view.Children)
            {
                NixxisTileViewItem tile = item as NixxisTileViewItem;
                tile.ApplyHeaderTemplate();
            }
        }

        public static readonly DependencyProperty LargeCommandImageProperty = DependencyProperty.Register("LargeCommandImage", typeof(ImageSource), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(LargeCommandImageChanged)));
        public ImageSource LargeCommandImage
        {
            get { return GetValue(LargeCommandImageProperty) as ImageSource; }
            set { SetValue(LargeCommandImageProperty, value); }
        }
        public static void LargeCommandImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty MediumCommandImageProperty = DependencyProperty.Register("MediumCommandImage", typeof(ImageSource), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(MediumCommandImageChanged)));
        public ImageSource MediumCommandImage
        {
            get { return GetValue(MediumCommandImageProperty) as ImageSource; }
            set { SetValue(MediumCommandImageProperty, value); }
        }
        public static void MediumCommandImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SmallCommandImageProperty = DependencyProperty.Register("SmallCommandImage", typeof(ImageSource), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(SmallCommandImageChanged)));
        public ImageSource SmallCommandImage
        {
            get { return GetValue(SmallCommandImageProperty) as ImageSource; }
            set { SetValue(SmallCommandImageProperty, value); }
        }
        public static void SmallCommandImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty MediumModeReverseOrderProperty = DependencyProperty.Register("MediumModeReverseOrder", typeof(bool), typeof(NixxisTileViewItem), new PropertyMetadata(false, new PropertyChangedCallback(MediumModeReverseOrderChanged)));
        public bool MediumModeReverseOrder
        {
            get { return (bool)GetValue(MediumModeReverseOrderProperty); }
            set { SetValue(MediumModeReverseOrderProperty, value); }
        }
        public static void MediumModeReverseOrderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        int ChildrenCount
        {
            get
            {
                return this.Children.Cast<NixxisTileViewItem>().Count((ui) => (ui.Visibility != System.Windows.Visibility.Collapsed));
            }
        }

        #region Members Override
        private double CheckSmallContentHeight(Size availableSize, double desiredHeight, double margin)
        {
            if ((desiredHeight + margin) * (this.ChildrenCount - 1) <= (availableSize.Height - margin))
            {
                return desiredHeight;
            }
            else
            {
                return ((availableSize.Height + margin) / (this.ChildrenCount - 1)) - margin;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double defaultMargin = DisplayMode == DisplayModes.LargeSmallList || DisplayMode == DisplayModes.LargeOnly ? defaultStandardMargin : defaultMediumMargin;
            double availableSmallContentHeight = DisplayMode == DisplayModes.LargeSmallList ? CheckSmallContentHeight(availableSize, SmallContentHeight, defaultMargin) : 0;

            double wBigContent = availableSize.Width - (3 * defaultMargin) - ((DisplayMode == DisplayModes.LargeOnly) ? 0 : SmallContentWidth);
            double hBigContent = availableSize.Height - (2 * defaultMargin);

            double wMediumContent = availableSize.Width - (2 * defaultMargin);
            double hMediomContent = availableSize.Height - (2 * defaultMargin);

            int[] dim = new int[] { 0, 0 };
            if (DisplayMode == DisplayModes.MediumOnly)
            {
                if (this.ChildrenCount <= 4)
                    dim = MathFunction.GetBestDimensions(this.ChildrenCount, true);
                else
                    dim = MathFunction.GetBestDimensions(this.ChildrenCount, false);

                int row = this.MediumModeReverseOrder ? dim[1] : dim[0];
                int col = this.MediumModeReverseOrder ? dim[0] : dim[1];
                wMediumContent = ((availableSize.Width - defaultMargin) / col) - defaultMargin;
                hMediomContent = ((availableSize.Height - defaultMargin) / row) - defaultMargin;
            }

            foreach (UIElement child in Children)
            {
                NixxisTileViewItem item = child as NixxisTileViewItem;
                if (item.Visibility != System.Windows.Visibility.Collapsed)
                {
                    if (DisplayMode == DisplayModes.LargeSmallList)
                    {
                        if (item.IsSelected)
                        {
                            item.Width = wBigContent;
                            item.Height = hBigContent;
                        }
                        else
                        {
                            item.Width = SmallContentWidth;
                            item.Height = availableSmallContentHeight;
                        }
                        item.Measure(new Size(item.Width, item.Height));
                    }
                    else if (DisplayMode == DisplayModes.LargeOnly)
                    {
                        if (item.IsSelected)
                        {
                            item.Width = wBigContent;
                            item.Height = hBigContent;
                        }
                        else
                        {
                            item.Width = 0;
                            item.Height = 0;
                        }
                        item.Measure(new Size(item.Width, item.Height));
                    }
                    else if (DisplayMode == DisplayModes.MediumOnly)
                    {
                        item.Width = wMediumContent;
                        item.Height = hMediomContent;

                        item.Measure(new Size(item.Width, item.Height));
                    }
                }
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {            

            try
            {

                if (this.Children == null || this.ChildrenCount == 0)
                    return finalSize;

                double defaultMargin = DisplayMode == DisplayModes.LargeSmallList || DisplayMode == DisplayModes.LargeOnly ? defaultStandardMargin : defaultMediumMargin;

                double xBigContent = defaultMargin;
                double yBigContent = defaultMargin;
                double xSmallcontent = this.DesiredSize.Width - defaultMargin - ((DisplayMode == DisplayModes.LargeOnly) ? 0 : SmallContentWidth);
                double ySmallcontent = defaultMargin;

                double xContent = defaultMargin;
                double yContent = defaultMargin;

                int[] dim = new int[] { 0, 0 };
                double row = 0, col = 0;
                if (DisplayMode == DisplayModes.MediumOnly)
                {
                    if (this.ChildrenCount <= 4)
                        dim = MathFunction.GetBestDimensions(this.ChildrenCount, true);
                    else
                        dim = MathFunction.GetBestDimensions(this.ChildrenCount, false);

                    row = this.MediumModeReverseOrder ? dim[1] : dim[0];
                    col = this.MediumModeReverseOrder ? dim[0] : dim[1];
                }

                double rowCount = 0;
                double colCount = 0;
                TranslateTransform trans = null;
                foreach (UIElement child in Children)
                {
                    NixxisTileViewItem item = child as NixxisTileViewItem;

                    if (item != null && child != null && child.DesiredSize != null  && item.Visibility!=System.Windows.Visibility.Collapsed)
                    {

                        trans = child.RenderTransform as TranslateTransform;
                        if (trans == null)
                        {
                            child.RenderTransformOrigin = new Point(0, 0);
                            trans = new TranslateTransform();
                            child.RenderTransform = trans;
                        }

                        if (DisplayMode == DisplayModes.LargeSmallList)
                        {
                            if (item.IsSelected)
                            {
                                DoubleAnimation da = new DoubleAnimation(xBigContent, _AnimationLength);

                                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                                trans.BeginAnimation(TranslateTransform.XProperty, da, HandoffBehavior.Compose);
                                trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(yBigContent, _AnimationLength), HandoffBehavior.Compose);
                                //trans.hol
                            }
                            else
                            {
                                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                                trans.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(xSmallcontent, _AnimationLength), HandoffBehavior.Compose);
                                trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(ySmallcontent, _AnimationLength), HandoffBehavior.Compose);
                                ySmallcontent += child.DesiredSize.Height + defaultMargin;
                            }
                        }
                        else if (DisplayMode == DisplayModes.LargeOnly)
                        {
                            if (item.IsSelected)
                            {
                                DoubleAnimation da = new DoubleAnimation(xBigContent, _AnimationLength);

                                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                                trans.BeginAnimation(TranslateTransform.XProperty, da, HandoffBehavior.Compose);
                                trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(yBigContent, _AnimationLength), HandoffBehavior.Compose);

                            }
                            else
                            {
                                child.Arrange(new Rect(0, 0, 0, 0));
                                trans.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(xSmallcontent, _AnimationLength), HandoffBehavior.Compose);
                                trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(ySmallcontent, _AnimationLength), HandoffBehavior.Compose);
                                ySmallcontent += defaultMargin;
                            }
                        }
                        else if (DisplayMode == DisplayModes.MediumOnly)
                        {
                            child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                            trans.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(xContent + (child.DesiredSize.Width * colCount) + (defaultMargin * colCount), _AnimationLength), HandoffBehavior.Compose);
                            trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(yContent + (child.DesiredSize.Height * rowCount) + (defaultMargin * rowCount), _AnimationLength), HandoffBehavior.Compose);

                            colCount++;
                            if (colCount >= col)
                            {
                                colCount = 0;
                                rowCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString(), "NixxisTileView");
            }
            return finalSize;
        }

        void NixxisTileView_Completed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void sb_Completed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (m_SelectedItem == null && this.ChildrenCount > 0)
            {
                SelectItem((NixxisTileViewItem)this.Children[0]);
            }
        }
        #endregion

        #region Members
        public void SelectItem(NixxisTileViewItem item)
        {           
            if (item == null)
                return;
            if (item.Equals(m_SelectedItem))
                return;

            NixxisTileViewItem m_CurrenSelectedItem = m_SelectedItem;

            m_SelectedItem = null;

            foreach (UIElement element in this.Children)
            {
                if (element.Equals(item))
                {
                    m_SelectedItem = item;
                    m_SelectedItem.IsSelected = true;
                    m_DisplayMode =  item.SmallTemplate == null ?  DisplayModes.LargeOnly: DisplayModes.LargeSmallList;
                }
                else
                {
                    ((NixxisTileViewItem)element).IsSelected = false;
                    ((NixxisTileViewItem)element).ViewItemState = NixxisTileViewItem.ViewItemStates.Small;
                }
            }

            this.InvalidateMeasure();

            foreach (UIElement element in this.Children)
            {
                if (element.Equals(item))
                {
                    m_SelectedItem.ViewItemState = NixxisTileViewItem.ViewItemStates.Large;
                }
                else
                {
                    ((NixxisTileViewItem)element).ViewItemState = NixxisTileViewItem.ViewItemStates.Small;
                }
            }

            OnViewStateChanged(this, new NixxisTileViewStateChangedEventArgs(m_SelectedItem, m_CurrenSelectedItem));

        }

        public void UnSelectItem(NixxisTileViewItem item)
        {
            if (item == null)
                return;

            NixxisTileViewItem m_CurrenSelectedItem = m_SelectedItem;

            int i = this.Children.IndexOf(item);

            if (i >= 0)
            {
                ((NixxisTileViewItem)this.Children[i]).IsSelected = false;
            }

            if (m_SelectedItem != null && m_SelectedItem.Equals(item))
                m_SelectedItem = null;

            if (m_SelectedItem == null)
            {
                m_DisplayMode = DisplayModes.MediumOnly;

            }
            else
            {
                m_DisplayMode = DisplayModes.LargeSmallList;
            }

            this.InvalidateMeasure();

            if (m_SelectedItem == null)
            {
                foreach (UIElement element in this.Children)
                {
                    ((NixxisTileViewItem)element).ViewItemState = NixxisTileViewItem.ViewItemStates.Medium;
                }
            }

            OnViewStateChanged(this, new NixxisTileViewStateChangedEventArgs(null, m_CurrenSelectedItem));
        }

        public bool IsSelectedById(string id)
        {
            if(m_SelectedItem == null)
                return false;

            if(id.Equals(m_SelectedItem.Id, StringComparison.OrdinalIgnoreCase))
                return true;
            else
                return false;
        }
        
        public string GetSelectedId()
        {
            if (m_SelectedItem == null)
                return string.Empty;
            else
                return m_SelectedItem.Id;
        }

        public NixxisTileViewItem GetItemById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (NixxisTileViewItem item in this.Children)
            {
                if (item.Id == id)
                    return item;
            }

            return null;
        }
        #endregion

        #region Events
        public event NixxisTileViewStateChangedEventHndl ViewStateChanged;
        private void OnViewStateChanged(object sender, NixxisTileViewStateChangedEventArgs e)
        {            
            if (ViewStateChanged != null)
                ViewStateChanged(sender, e);
        }
        #endregion
    }

    public class NixxisTileViewItem : ContentControl
    {
        #region Enums
        public enum ViewItemStates
        {         
            Large,
            Medium,
            Small,
        }
        #endregion

        #region Class data
        private Grid m_BaseControl = new Grid();
        private ContentControl m_ContentLarge = new ContentControl();
        private ContentControl m_ContentMedium = new ContentControl();
        private ContentControl m_ContentSmall = new ContentControl();
        private NixxisTileViewHeader m_Header = new NixxisTileViewHeader();
        private bool m_IsSelected = false;
        #endregion

        #region Properties
        public ContentControl CurrentContent
        {
            get
            {
                switch (this.ViewItemState)
                {
                    case ViewItemStates.Large:
                        return m_ContentLarge;
                    case ViewItemStates.Medium:
                        return m_ContentMedium;
                    default:
                        return m_ContentSmall;
                }
            }
        }
        public NixxisTileViewHeader Header
        {
            get { return m_Header; }
            set { m_Header = value; }
        }
        public ContentControl ContentLarge
        {
            get { return this.m_ContentLarge; }
            internal set { this.m_ContentLarge = value; }
        }
        public ContentControl ContentMedium
        {
            get { return this.m_ContentMedium; }
            internal set { this.m_ContentMedium = value; }
        }
        public ContentControl ContentSmall
        {
            get { return this.m_ContentSmall; }
            internal set { this.m_ContentSmall = value; }
        }
        public bool IsSelected
        {
            get { return m_IsSelected; }
            internal set { m_IsSelected = value; }
        }

        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ViewItemStateProperty = DependencyProperty.Register("ViewItemState", typeof(ViewItemStates), typeof(NixxisTileViewItem), new PropertyMetadata(ViewItemStates.Small, new PropertyChangedCallback(ViewItemStateChanged)));
        public ViewItemStates ViewItemState
        {
            get { return (ViewItemStates)GetValue(ViewItemStateProperty); }
            set { SetValue(ViewItemStateProperty, value); }
        }
        public static void ViewItemStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileViewItem item = obj as NixxisTileViewItem;

            item.ApplyViewTemplate();
        }

        public static readonly DependencyProperty LargeTemplateProperty = DependencyProperty.Register("LargeTemplate", typeof(ControlTemplate), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(LargeTemplateChanged)));
        public ControlTemplate LargeTemplate
        {
            get { return GetValue(LargeTemplateProperty) as ControlTemplate; }
            set { SetValue(LargeTemplateProperty, value); }
        }
        public static void LargeTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        
        public static readonly DependencyProperty MediumTemplateProperty = DependencyProperty.Register("MediumTemplate", typeof(ControlTemplate), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(MediumTemplateChanged)));
        public ControlTemplate MediumTemplate
        {
            get { return GetValue(MediumTemplateProperty) as ControlTemplate; }
            set { SetValue(MediumTemplateProperty, value); }
        }
        public static void MediumTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SmallTemplateProperty = DependencyProperty.Register("SmallTemplate", typeof(ControlTemplate), typeof(NixxisTileViewItem), new PropertyMetadata(null, new PropertyChangedCallback(SmallTemplateChanged)));
        public ControlTemplate SmallTemplate
        {
            get { return GetValue(SmallTemplateProperty) as ControlTemplate; }
            set { SetValue(SmallTemplateProperty, value); }
        }
        public static void SmallTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty HeaderVisibleLargeProperty = DependencyProperty.Register("HeaderVisibleLarge", typeof(Visibility), typeof(NixxisTileViewItem), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(HeaderVisibleLargeChanged)));
        public Visibility HeaderVisibleLarge
        {
            get { return (Visibility)GetValue(HeaderVisibleLargeProperty); }
            set { SetValue(HeaderVisibleLargeProperty, value); }
        }
        public static void HeaderVisibleLargeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileViewItem tile = obj as NixxisTileViewItem;

            if (args.NewValue != null)
                tile.Header.Visibility = (Visibility)args.NewValue;
        }

        public static readonly DependencyProperty HeaderVisibleMediumProperty = DependencyProperty.Register("HeaderVisibleMedium", typeof(Visibility), typeof(NixxisTileViewItem), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(HeaderVisibleMediumChanged)));
        public Visibility HeaderVisibleMedium
        {
            get { return (Visibility)GetValue(HeaderVisibleMediumProperty); }
            set { SetValue(HeaderVisibleMediumProperty, value); }
        }
        public static void HeaderVisibleMediumChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileViewItem tile = obj as NixxisTileViewItem;

            if (args.NewValue != null)
                tile.Header.Visibility = (Visibility)args.NewValue;
        }

        public static readonly DependencyProperty HeaderVisibleSmallProperty = DependencyProperty.Register("HeaderVisibleSmall", typeof(Visibility), typeof(NixxisTileViewItem), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(HeaderVisibleSmallChanged)));
        public Visibility HeaderVisibleSmall
        {
            get { return (Visibility)GetValue(HeaderVisibleSmallProperty); }
            set { SetValue(HeaderVisibleSmallProperty, value); }
        }
        public static void HeaderVisibleSmallChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileViewItem tile = obj as NixxisTileViewItem;

            if (args.NewValue != null)
                tile.Header.Visibility = (Visibility)args.NewValue;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(NixxisTileViewItem), new PropertyMetadata(new PropertyChangedCallback(TitleChanged)));
        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }
        public static void TitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileViewItem tile = obj as NixxisTileViewItem;

            if (args.NewValue != null)
                    tile.Header.Title = args.NewValue.ToString();
        }

        public static readonly DependencyProperty TileIconProperty = DependencyProperty.Register("TileIcon", typeof(ImageSource), typeof(NixxisTileViewItem), new PropertyMetadata(new PropertyChangedCallback(TileIconChanged)));
        public ImageSource TileIcon
        {
            get { return GetValue(TileIconProperty) as ImageSource; }
            set { SetValue(TileIconProperty, value); }
        }
        public static void TileIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTileViewItem tile = obj as NixxisTileViewItem;

            if (args.NewValue != null)
                tile.Header.TileIcon = (ImageSource)args.NewValue;
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(NixxisTileViewItem));
        public string Id
        {
            get { return GetValue(IdProperty) as string; }
            set { SetValue(IdProperty, value); }
        }
        #endregion

        #region Members Override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CreateControl();
            ApplyViewTemplate();
            ApplyHeaderTemplate();
        }
        #endregion

        #region Members
        public virtual void CreateControl()
        {
            m_BaseControl = new Grid();

            m_BaseControl.RowDefinitions.Add(new RowDefinition());
            m_BaseControl.RowDefinitions.Add(new RowDefinition());
            m_BaseControl.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);
            
            //
            //Header
            //
            m_Header.Owner = this;
            Grid.SetRow(m_Header, 0);
            Grid.SetColumn(m_Header, 0);
            m_BaseControl.Children.Add(m_Header);

            //
            //Large
            //
            m_ContentLarge.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            m_ContentLarge.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            Grid.SetRow(m_ContentLarge, 1);
            Grid.SetColumn(m_ContentLarge, 0);
            m_BaseControl.Children.Add(m_ContentLarge);
            m_ContentLarge.Template = this.LargeTemplate;
            m_ContentLarge.ApplyTemplate();
            //
            //Medium
            //
            m_ContentMedium.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            m_ContentMedium.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            Grid.SetRow(m_ContentMedium, 1);
            Grid.SetColumn(m_ContentMedium, 0);
            m_BaseControl.Children.Add(m_ContentMedium);
            m_ContentMedium.Template = this.MediumTemplate;
            m_ContentMedium.ApplyTemplate();
            //
            //Small
            //
            m_ContentSmall.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            m_ContentSmall.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            Grid.SetRow(m_ContentSmall, 1);
            Grid.SetColumn(m_ContentSmall, 0);
            m_BaseControl.Children.Add(m_ContentSmall);
            m_ContentSmall.Template = this.SmallTemplate;
            m_ContentSmall.ApplyTemplate();

            this.AddChild(m_BaseControl);

            OnItemCreated(new NixxisTileItemEventArgs(m_ContentLarge, m_ContentMedium, m_ContentSmall));
        }

        public virtual void ApplyViewTemplate()
        {
            NixxisTileView view = this.Parent as NixxisTileView;

            switch (this.ViewItemState)
            {
                case ViewItemStates.Large:
                    m_Header.CommandImage = view.LargeCommandImage;                    
                    if (this.LargeTemplate != null)
                    {
                        m_Header.Visibility = this.HeaderVisibleLarge;
                        m_ContentLarge.Visibility = System.Windows.Visibility.Visible;
                        this.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        m_Header.Visibility = System.Windows.Visibility.Collapsed;
                        m_ContentLarge.Visibility = System.Windows.Visibility.Collapsed;
                        this.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    m_ContentMedium.Visibility = System.Windows.Visibility.Collapsed;
                    m_ContentSmall.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case ViewItemStates.Medium:
                    m_Header.CommandImage = view.MediumCommandImage;
                    
                    m_ContentLarge.Visibility = System.Windows.Visibility.Collapsed;
                    if (this.MediumTemplate != null)
                    {
                        m_Header.Visibility = this.HeaderVisibleMedium;
                        m_ContentMedium.Visibility = System.Windows.Visibility.Visible;
                        this.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        m_Header.Visibility = System.Windows.Visibility.Collapsed;
                        m_ContentMedium.Visibility = System.Windows.Visibility.Collapsed;
                        this.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    m_ContentSmall.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                default:
                    m_Header.CommandImage = view.SmallCommandImage;
                    
                    m_ContentLarge.Visibility = System.Windows.Visibility.Collapsed;
                    m_ContentMedium.Visibility = System.Windows.Visibility.Collapsed;
                    if (this.SmallTemplate != null)
                    {

                        m_ContentSmall.Visibility = System.Windows.Visibility.Visible;
                        m_Header.Visibility = this.HeaderVisibleSmall;
                        this.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        m_ContentSmall.Visibility = System.Windows.Visibility.Collapsed;
                        m_Header.Visibility  = System.Windows.Visibility.Collapsed;
                        this.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    break;
            }

        }

        public virtual void ApplyHeaderTemplate()
        {
            if (m_Header != null)
            {
                NixxisTileView view = this.Parent as NixxisTileView;
                m_Header.Template = view.HeaderTemplate;
                m_Header.ApplyTemplate();
            }
        }

        public void OnSelectItem()
        {
            NixxisTileView view = this.Parent as NixxisTileView;

            if(this.IsSelected)
                view.UnSelectItem(this);
            else
                view.SelectItem(this);
        }
        #endregion

        #region Events
        public event NixxisTileItemEventHndl ItemCreated;
        private void OnItemCreated(NixxisTileItemEventArgs e)
        {
            if (ItemCreated != null)
                ItemCreated(this, e);
        }
        #endregion
    }

    public class NixxisTileViewHeader : ContentControl
    {
        #region Properties
        public NixxisTileViewItem Owner { get; set; }
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(NixxisTileViewHeader), new PropertyMetadata(new PropertyChangedCallback(TitleChanged)));
        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }
        public static void TitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty CommandImageProperty = DependencyProperty.Register("CommandImage", typeof(ImageSource), typeof(NixxisTileViewHeader), new PropertyMetadata(new PropertyChangedCallback(CommandImageChanged)));
        public ImageSource CommandImage
        {
            get { return GetValue(CommandImageProperty) as ImageSource; }
            set { SetValue(CommandImageProperty, value); }
        }
        public static void CommandImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty TileIconProperty = DependencyProperty.Register("TileIcon", typeof(ImageSource), typeof(NixxisTileViewHeader));
        public ImageSource TileIcon
        {
            get { return GetValue(TileIconProperty) as ImageSource; }
            set { SetValue(TileIconProperty, value); }
        }
        #endregion

        #region Members Override
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            Owner.OnSelectItem();
        }
        #endregion
    }

    public class NixxisTileViewStateChangedEventArgs
    {
        #region Class Data
        private NixxisTileViewItem m_NewLagreItem = null;
        private NixxisTileViewItem m_OldLagreItem = null;
        #endregion

        #region Constructor
        public NixxisTileViewStateChangedEventArgs()
        { }
        public NixxisTileViewStateChangedEventArgs(NixxisTileViewItem newLagreItem, NixxisTileViewItem oldLagreItem)
        {
            m_NewLagreItem = newLagreItem;
            m_OldLagreItem = oldLagreItem;
        }
        #endregion

        #region Properties
        public NixxisTileViewItem NewLagreItem
        {
            get { return this.m_NewLagreItem; }
            internal set { this.m_NewLagreItem = value; }
        }
        public NixxisTileViewItem OldLagreItem
        {
            get { return this.m_OldLagreItem; }
            internal set { this.m_OldLagreItem = value; }
        }
        #endregion
    }
    public delegate void NixxisTileViewStateChangedEventHndl(object sender, NixxisTileViewStateChangedEventArgs e);

    public class NixxisTileItemEventArgs
    {
        #region Class Data
        private ContentControl m_ContentLarge = new ContentControl();
        private ContentControl m_ContentMedium = new ContentControl();
        private ContentControl m_ContentSmall = new ContentControl();
        #endregion

        #region Constructor
        public NixxisTileItemEventArgs()
        { }
        public NixxisTileItemEventArgs(ContentControl contentLarge, ContentControl contentMedium, ContentControl contentSmall)
        {
            m_ContentLarge = contentLarge;
            m_ContentMedium = contentMedium;
            m_ContentSmall = contentSmall;
        }
        #endregion

        #region Properties
        public ContentControl ContentLarge
        {
            get { return this.m_ContentLarge; }
            internal set { this.m_ContentLarge = value; }
        }
        public ContentControl ContentMedium
        {
            get { return this.m_ContentMedium; }
            internal set { this.m_ContentMedium = value; }
        }
        public ContentControl ContentSmall
        {
            get { return this.m_ContentSmall; }
            internal set { this.m_ContentSmall = value; }
        }
        #endregion
    }
    public delegate void NixxisTileItemEventHndl(object sender, NixxisTileItemEventArgs e);
}
