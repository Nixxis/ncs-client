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
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;
using System.Windows.Media.Effects;

namespace Nixxis.Client.Controls
{

    public class NixxisChart : DockPanel
    {
        #region Enums 
        public enum ChartTypes
        {
            Bar,
            Bar100,
        }
        public enum ChartFillTypes
        {
            CustomBrush,
            Cylinder,
            SolidColor,
        }
        public enum ChartEffectTypes
        {
            None,
            Glass,
        }
        public enum LegendLocationTypes
        {
            //Top,
            //Bottom,
            Right,
            Left,
        }
        #endregion

        #region Class data
        private Grid mainContainer = null;
        private Grid EffectLayer = null;
        private Panel legendControl = null;
        private Grid graphControl = null;
        private Color m_EmptyColor = Colors.Transparent; // Color.FromScRgb(255, 208, 242, 223);
        #endregion

        #region Properties
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ChartTypeProperty = DependencyProperty.Register("ChartType", typeof(ChartTypes), typeof(NixxisChart), new PropertyMetadata(ChartTypes.Bar, new PropertyChangedCallback(ChartTypeChanged)));
        public ChartTypes ChartType
        {
            get { return (ChartTypes)GetValue(ChartTypeProperty); }
            set { SetValue(ChartTypeProperty, value); }
        }
        public static void ChartTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //To do
        }

        public static readonly DependencyProperty ChartEffectTypeProperty = DependencyProperty.Register("ChartEffectType", typeof(ChartEffectTypes), typeof(NixxisChart), new PropertyMetadata(ChartEffectTypes.None, new PropertyChangedCallback(ChartEffectTypeChanged)));
        public ChartEffectTypes ChartEffectType
        {
            get { return (ChartEffectTypes)GetValue(ChartEffectTypeProperty); }
            set { SetValue(ChartEffectTypeProperty, value); }
        }
        public static void ChartEffectTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //To do
        }

        public static readonly DependencyProperty ChartFillTypeProperty = DependencyProperty.Register("ChartFillType", typeof(ChartFillTypes), typeof(NixxisChart), new PropertyMetadata(ChartFillTypes.SolidColor, new PropertyChangedCallback(ChartFillTypeChanged)));
        public ChartFillTypes ChartFillType
        {
            get { return (ChartFillTypes)GetValue(ChartFillTypeProperty); }
            set { SetValue(ChartFillTypeProperty, value); }
        }
        public static void ChartFillTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //To do
        }

        public static readonly DependencyProperty ChartPiecesProperty = DependencyProperty.Register("ChartPieces", typeof(NixxisChartPieces), typeof(NixxisChart), new PropertyMetadata(new PropertyChangedCallback(ChartPiecesChanged)));
        public NixxisChartPieces ChartPieces
        {
            get { return (NixxisChartPieces)GetValue(ChartPiecesProperty); }
            set { SetValue(ChartPiecesProperty, value); }
        }
        public static void ChartPiecesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChart chart = obj as NixxisChart;

            if(args.NewValue != null && ((NixxisChartPieces)args.NewValue).Count > 0)
                chart.UpdateGraph();
        }

        public static readonly DependencyProperty LegendVisibleProperty = DependencyProperty.Register("LegendVisible", typeof(bool), typeof(NixxisChart), new PropertyMetadata(false, new PropertyChangedCallback(LegendVisibleChanged)));
        public bool LegendVisible
        {
            get { return (bool)GetValue(LegendVisibleProperty); }
            set { SetValue(LegendVisibleProperty, value); }
        }
        public static void LegendVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChart chart = obj as NixxisChart;
        }

        public static readonly DependencyProperty LegendLocationProperty = DependencyProperty.Register("LegendLocation", typeof(LegendLocationTypes), typeof(NixxisChart), new PropertyMetadata(LegendLocationTypes.Right, new PropertyChangedCallback(LegendLocationChanged)));
        public LegendLocationTypes LegendLocation
        {
            get { return (LegendLocationTypes)GetValue(LegendLocationProperty); }
            set { SetValue(LegendLocationProperty, value); }
        }
        public static void LegendLocationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChart chart = obj as NixxisChart;
        }

        public static readonly DependencyProperty LegendLabelOrientationProperty = DependencyProperty.Register("LegendLabelOrientation", typeof(Orientation), typeof(NixxisChart), new PropertyMetadata(Orientation.Vertical, new PropertyChangedCallback(LegendLabelOrientationChanged)));
        public Orientation LegendLabelOrientation
        {
            get { return (Orientation)GetValue(LegendLabelOrientationProperty); }
            set { SetValue(LegendLabelOrientationProperty, value); }
        }
        public static void LegendLabelOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChart chart = obj as NixxisChart;
        }

        public static readonly DependencyProperty LegendLabelWrapProperty = DependencyProperty.Register("LegendLabelWrap", typeof(bool), typeof(NixxisChart), new PropertyMetadata(true, new PropertyChangedCallback(LegendLabelWrapChanged)));
        public bool LegendLabelWrap
        {
            get { return (bool)GetValue(LegendLabelWrapProperty); }
            set { SetValue(LegendLabelWrapProperty, value); }
        }
        public static void LegendLabelWrapChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChart chart = obj as NixxisChart;
        }

        public static readonly DependencyProperty LegendWidthProperty = DependencyProperty.Register("LegendWidth", typeof(int), typeof(NixxisChart), new PropertyMetadata(-1));
        public int LegendWidth
        {
            get { return (int)GetValue(LegendWidthProperty); }
            set { SetValue(LegendWidthProperty, value); }
        }

        public static readonly DependencyProperty Bar100MaxValueProperty = DependencyProperty.Register("Bar100MaxValue", typeof(int), typeof(NixxisChart), new PropertyMetadata(100, new PropertyChangedCallback(Bar100MaxValueChanged)));
        public int Bar100MaxValue
        {
            get { return (int)GetValue(Bar100MaxValueProperty); }
            set { SetValue(Bar100MaxValueProperty, value); }
        }
        public static void Bar100MaxValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Members Override
        public override void BeginInit()
        {
            base.BeginInit();
            this.ChartPieces = new NixxisChartPieces();
        }

        protected override void OnInitialized(EventArgs e)
        {
            DataContextChanged += new DependencyPropertyChangedEventHandler(NixxisChart_DataContextChanged);

            base.OnInitialized(e);

            if (this.DataContext != null && mainContainer == null)
            {
                ChartPiecesChanged(this, new DependencyPropertyChangedEventArgs(ChartPiecesProperty, null, this.ChartPieces));
            }
        }
        #endregion

        #region Members
        void NixxisChart_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && mainContainer == null)
            {
                ChartPiecesChanged(this, new DependencyPropertyChangedEventArgs(ChartPiecesProperty, null, this.ChartPieces));
            }
        }

        private void CreateContainer()
        {
            mainContainer = new Grid();
            mainContainer.RowDefinitions.Add(new RowDefinition());
            mainContainer.ColumnDefinitions.Add(new ColumnDefinition());
            mainContainer.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            mainContainer.ColumnDefinitions.Add(new ColumnDefinition());
            mainContainer.ColumnDefinitions.Add(new ColumnDefinition());
            mainContainer.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Auto);

            graphControl = new Grid();
            graphControl.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(graphControl, 1);
            Grid.SetRow(graphControl, 0);
            mainContainer.Children.Add(graphControl);

            EffectLayer = new Grid();
            Grid.SetColumn(EffectLayer, 1);
            Grid.SetRow(EffectLayer, 0);
            mainContainer.Children.Add(EffectLayer);

            this.Children.Add(mainContainer);
        }
        internal void UpdateBarGraph()
        {
            NixxisChartPieces data = this.ChartPieces;

            if (data == null)
                return;

            if (data.Count != graphControl.ColumnDefinitions.Count)
            {
                double totalValue = 0;
                //chart data
                foreach (NixxisChartPiece piece in data)
                {
                    graphControl.ColumnDefinitions.Add(new ColumnDefinition());

                    Binding myBinding = new Binding(piece.BindingValue);
                    myBinding.Source = this.DataContext;
                    myBinding.Converter = new AbsoluteToRelativeConverter();
                    graphControl.ColumnDefinitions[graphControl.ColumnDefinitions.Count - 1].SetBinding(ColumnDefinition.WidthProperty, myBinding);

                    Canvas can = new Canvas();
                    can.Background = GetChartPieceBrush(piece);
                    try
                    {
                        can.Style = (Style)FindResource("NixxisChartBarPieceStyle");
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.ToString(), "NixxisChart");
                    }
                    can.ToolTip = piece.Label;
                    //can.DataContext

                    Grid.SetColumn(can, graphControl.ColumnDefinitions.Count - 1);
                    Grid.SetRow(can, 0);
                    graphControl.Children.Add(can);
                }
                //Legend
                if (this.LegendVisible)
                {
                    legendControl = SetLegend();

                    if (this.LegendLocation == LegendLocationTypes.Left)
                    {
                        Grid.SetColumn(legendControl, 0);
                    }
                    else
                    {
                        Grid.SetColumn(legendControl, 2);
                    }
                    Grid.SetRow(legendControl, 0);
                    mainContainer.Children.Add(legendControl);
                }
                //Effect
                AddEffect();
                //No data dispaly
                if (totalValue == 0 && graphControl.ColumnDefinitions.Count > 1)
                {
                    //Draw empty bar if it is needed
                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    //graphControl.ColumnDefinitions[1].Width = new GridLength(System.Convert.ToDouble(data[i].ValueObject), GridUnitType.Star);
                }
            }
        }
        internal void UpdateBar100Graph()
        {
            NixxisChartPieces data = this.ChartPieces;

            if (data == null)
                return;

            if (data.Count <= 0) 
                return;

            NixxisChartPiece piece = data[0];
            Binding myBinding = null;
            Canvas can = null;

            //
            //Data part
            //
            graphControl.ColumnDefinitions.Add(new ColumnDefinition());

            myBinding = new Binding(piece.BindingValue);
            myBinding.Source = this.DataContext;
            myBinding.Converter = new AbsoluteToRelativeBar100Converter();
            myBinding.ConverterParameter = Bar100MaxValue;
            graphControl.ColumnDefinitions[graphControl.ColumnDefinitions.Count - 1].SetBinding(ColumnDefinition.WidthProperty, myBinding);

            can = new Canvas();
            can.Background = GetChartPieceBrush(piece);
            try
            {
                can.Style = (Style)FindResource("NixxisChartBarPieceStyle");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString(), "NixxisChart");
            }

            can.ToolTip = piece.Label;

            Grid.SetColumn(can, graphControl.ColumnDefinitions.Count - 1);
            Grid.SetRow(can, 0);
            graphControl.Children.Add(can);

            //
            //Rest fill part
            //
            graphControl.ColumnDefinitions.Add(new ColumnDefinition());

            myBinding = new Binding(piece.BindingValue);
            myBinding.Source = this.DataContext;
            myBinding.Converter = new AbsoluteToRelativeBar100RestConverter();
            myBinding.ConverterParameter = Bar100MaxValue;
            graphControl.ColumnDefinitions[graphControl.ColumnDefinitions.Count - 1].SetBinding(ColumnDefinition.WidthProperty, myBinding);

            can = new Canvas();
            can.Background = GetChartPieceBrush(null);
            try
            {
                can.Style = (Style)FindResource("NixxisChartBarPieceStyle");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString(), "NixxisChart");
            }


            Grid.SetColumn(can, graphControl.ColumnDefinitions.Count - 1);
            Grid.SetRow(can, 0);
            graphControl.Children.Add(can);
            //Effect
            AddEffect();
            //
            //Legend
            //
            if (this.LegendVisible)
            {
                legendControl = SetLegend();

                if (this.LegendLocation == LegendLocationTypes.Left)
                {
                    Grid.SetColumn(legendControl, 0);
                }
                else
                {
                    Grid.SetColumn(legendControl, 2);
                }
                Grid.SetRow(legendControl, 0);
                mainContainer.Children.Add(legendControl);
            }
        }
        internal void UpdateGraph()
        {
            if (mainContainer == null)
                CreateContainer();

            if (this.ChartType == ChartTypes.Bar)
                this.UpdateBarGraph();
            else
                this.UpdateBar100Graph();
        }

        internal void AddEffect()
        {
            if (ChartEffectType == ChartEffectTypes.Glass && EffectLayer.Children.Count <= 0)
            {
                EffectLayer.Margin = new Thickness(0, 2, 0, 2);
                EffectLayer.RowDefinitions.Add(new RowDefinition());
                EffectLayer.RowDefinitions.Add(new RowDefinition());
                //Diffuse glow
                Rectangle rect = new Rectangle();
                rect.Opacity = 0.20;
                rect.Margin = new Thickness(2);
                rect.Fill = new SolidColorBrush(Colors.White);
                rect.Effect = new BlurEffect();
                ((BlurEffect)rect.Effect).Radius = 8;

                Grid.SetColumn(rect, 0);
                Grid.SetRow(rect, 0);
                Grid.SetRowSpan(rect, 2);
                EffectLayer.Children.Add(rect);

                //Specular highloght
                Rectangle rect2 = new Rectangle();
                rect2.Margin = new Thickness(1);
                LinearGradientBrush fill = new LinearGradientBrush();
                fill.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#88ffffff"), 0));
                fill.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#22ffffff"), 1));
                rect2.Fill = fill;

                Grid.SetColumn(rect2, 0);
                Grid.SetRow(rect2, 0);
                EffectLayer.Children.Add(rect2);
            }
        }

        private Brush GetChartPieceBrush(NixxisChartPiece piece)
        {
            if (piece == null)
            {
                Color color = m_EmptyColor;

                switch (this.ChartFillType)
                {
                    case ChartFillTypes.CustomBrush:
                        return new SolidColorBrush(color);
                    case ChartFillTypes.Cylinder:
                        GradientStopCollection cylCollection = new GradientStopCollection();
                        cylCollection.Add(new GradientStop(new Color() { A = 76, B = color.B, G = color.G, R = color.R }, 0.681));
                        cylCollection.Add(new GradientStop(new Color() { A = 55, B = color.B, G = color.G, R = color.R }, 0.952));
                        cylCollection.Add(new GradientStop(new Color() { A = 152, B = color.B, G = color.G, R = color.R }, 0.463));
                        cylCollection.Add(new GradientStop(new Color() { A = 255, B = color.B, G = color.G, R = color.R }, 0));
                        cylCollection.Add(new GradientStop(new Color() { A = 191, B = color.B, G = color.G, R = color.R }, 0.23));
                        cylCollection.Add(new GradientStop(new Color() { A = 203, B = color.B, G = color.G, R = color.R }, 0.83));

                        LinearGradientBrush cylBrush = new LinearGradientBrush(cylCollection, new Point(0.5, 0), new Point(0.5, 1));
                        return cylBrush;
                    case ChartFillTypes.SolidColor:
                    default:
                        return new SolidColorBrush(color);
                }
            }
            else
            {
                switch (this.ChartFillType)
                {
                    case ChartFillTypes.CustomBrush:
                        return piece.Fill;
                    case ChartFillTypes.Cylinder:
                        GradientStopCollection cylCollection = new GradientStopCollection();
                        cylCollection.Add(new GradientStop(new Color() { A = 76, B = piece.Color.B, G = piece.Color.G, R = piece.Color.R }, 0.681));
                        cylCollection.Add(new GradientStop(new Color() { A = 55, B = piece.Color.B, G = piece.Color.G, R = piece.Color.R }, 0.952));
                        cylCollection.Add(new GradientStop(new Color() { A = 152, B = piece.Color.B, G = piece.Color.G, R = piece.Color.R }, 0.463));
                        cylCollection.Add(new GradientStop(new Color() { A = 255, B = piece.Color.B, G = piece.Color.G, R = piece.Color.R }, 0));
                        cylCollection.Add(new GradientStop(new Color() { A = 191, B = piece.Color.B, G = piece.Color.G, R = piece.Color.R }, 0.23));
                        cylCollection.Add(new GradientStop(new Color() { A = 203, B = piece.Color.B, G = piece.Color.G, R = piece.Color.R }, 0.83));

                        LinearGradientBrush cylBrush = new LinearGradientBrush(cylCollection, new Point(0.5, 0), new Point(0.5, 1));
                        return cylBrush;
                    case ChartFillTypes.SolidColor:
                    default:
                        return new SolidColorBrush(piece.Color);
                }
            }
        }

        private Panel SetLegend()
        {
            Panel mainPanel = null;
            if (this.LegendLabelWrap)
            {
                mainPanel = new WrapPanel();
                ((WrapPanel)mainPanel).Orientation = this.LegendLabelOrientation;
            }
            else
            {
                mainPanel = new StackPanel();
                ((StackPanel)mainPanel).Orientation = this.LegendLabelOrientation;
            }
            if (this.LegendWidth > -1)
                mainPanel.Width = this.LegendWidth;
            
            if (this.LegendLocation == LegendLocationTypes.Left)
                mainPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            else
                mainPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            foreach (NixxisChartPiece piece in this.ChartPieces)
            {
                StackPanel legendItem = new StackPanel();
                legendItem.Orientation = Orientation.Horizontal;
                legendItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                legendItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                Canvas legendIdent = new Canvas();
                legendIdent.Height = 10;
                legendIdent.Width = 10;
                legendIdent.Opacity = 0.7;
                legendIdent.Margin = new Thickness(2, 0, 2, 0);
                legendIdent.Background = GetChartPieceBrush(piece);

                TextBlock legendLabel = new TextBlock();
                legendLabel.Text = piece.Label;
                legendLabel.Style = (Style)FindResource("NixxisChartBarLegendStyle");
                legendItem.Children.Add(legendIdent);
                legendItem.Children.Add(legendLabel);

                mainPanel.Children.Add(legendItem);
            }

            return mainPanel;
        }
        #endregion
    }

    public class NixxisChartPieces : ObservableCollection<NixxisChartPiece>
    {
    }
    public class NixxisChartPiece : DependencyObject, System.ComponentModel.INotifyPropertyChanged
    {

        private Brush m_Fill;
        public Brush Fill
        {
            get { return m_Fill; }
            set { m_Fill = value; FirePropertyChanged("Fill"); }
        }

        private Color m_Color;
        public Color Color
        {
            get { return m_Color; }
            set { m_Color = value; FirePropertyChanged("Color"); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(NixxisChartPiece), new PropertyMetadata(new PropertyChangedCallback(LabelChanged)));
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static void LabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

        }

        private string m_BindingValue;
        public string BindingValue
        {
            get { return m_BindingValue; }
            set { m_BindingValue = value; FirePropertyChanged("BindingValue"); }
        }

        private NixxisChartPieces m_Owner;
        public NixxisChartPieces Owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }
       
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        }

        public override string ToString()
        {
            return Label;
        }
    }


    public class AbsoluteToRelativeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = 0;

            if (value.GetType() == typeof(double))
                val = (double)value;
            else if (value.GetType() == typeof(TimeSpan))
            {
                TimeSpan ts = (TimeSpan)value;
                val = ts.TotalSeconds;
            }
            else
            {
                try
                {
                    val = System.Convert.ToDouble(value);
                }
                catch { val = 0; }

            }

            return string.Format("{0}*", val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AbsoluteToRelativeBar100Converter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = 0;
            int maxValue = 100;
            try
            {
                maxValue = (int)parameter;
            }
            catch { }

            try
            {
                val = System.Convert.ToDouble(value);
            }
            catch { val = 0; }

            if (val > maxValue)
                val = maxValue;

            return string.Format("{0}*", val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AbsoluteToRelativeBar100RestConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = 0;
            int maxValue = 100;
            try
            {
                maxValue = (int)parameter;
            }
            catch { }

            try
            {
                val = maxValue - System.Convert.ToDouble(value);
            }
            catch { val = 0; }

            if (val < 0)
                val = 0;

            return string.Format("{0}*", val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
