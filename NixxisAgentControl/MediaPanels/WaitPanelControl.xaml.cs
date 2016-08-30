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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for WaitPanelControl.xaml
    /// </summary>
    public partial class WaitPanelControl : UserControl
    {
        #region Class data
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(WaitPanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructors
        public WaitPanelControl()
        {
            InitializeComponent();
        }

        public WaitPanelControl(HttpLink link)
        {
            ClientLink = link;

            InitializeComponent();

            ClientLink.Queues.PropertyChanged += Queues_PropertyChanged;
        }

        void Queues_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DialInfo")
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {

                    TextBlock tb = new TextBlock();
                    tb.TextAlignment = TextAlignment.Center;
                    tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tb.FontSize = 20;
                    if((sender as QueueList).DialInfoIsTargetedCallback)
                        tb.Foreground = Brushes.Red; // new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));                    
                    else
                        tb.Foreground = Brushes.White; // new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));                    
                    tb.Text = (sender as QueueList).DialInfo;
                    tb.Effect = new DropShadowEffect();

                    if (!string.IsNullOrEmpty((sender as QueueList).DialInfoActivity))
                    {
                        Run r = new Run();
                        r.FontSize = 12;
                        r.Text = "\n" + (sender as QueueList).DialInfoActivity;
                        tb.Inlines.Add(r);
                    }

                    Point[] pts = GetPointsForAnimation(50, tb.ActualWidth);


                    DoubleAnimation da = new DoubleAnimation();
                    da.Duration = new Duration(TimeSpan.FromSeconds(10));                                
                    da.From = pts[0].X;
                    da.To = pts[1].X;
                    Storyboard.SetTargetProperty(da, new PropertyPath(Canvas.LeftProperty));
                    Storyboard.SetTarget(da, tb);
                    da.EasingFunction = new CircleEase();

                    DoubleAnimation da2 = new DoubleAnimation();
                    da2.Duration = new Duration(TimeSpan.FromSeconds(10));
                    da2.From = pts[0].Y;
                    da2.To = pts[1].Y;
                    Storyboard.SetTargetProperty(da2, new PropertyPath(Canvas.TopProperty));
                    Storyboard.SetTarget(da2, tb);
                    da2.EasingFunction = new CircleEase();

                    DoubleAnimation da3 = new DoubleAnimation();
                    da3.BeginTime = TimeSpan.FromSeconds(10);
                    da3.Duration = new Duration(TimeSpan.FromSeconds(10));
                    da3.From = 1;
                    da3.To = 0;
                    da3.Completed += da3_Completed;
                    Storyboard.SetTargetProperty(da3, new PropertyPath(TextBlock.OpacityProperty));
                    Storyboard.SetTarget(da3, tb);

                    BeginStoryboard bs = new System.Windows.Media.Animation.BeginStoryboard();
                    bs.Storyboard = new Storyboard();
                    bs.Storyboard.Children.Add(da);
                    bs.Storyboard.Children.Add(da2);
                    bs.Storyboard.Children.Add(da3);

                    EventTrigger et = new EventTrigger();
                    et.RoutedEvent = TextBlock.LoadedEvent;
                    et.Actions.Add(bs);
                    tb.Triggers.Add(et);

                    RenderOptions.SetBitmapScalingMode(tb, BitmapScalingMode.HighQuality);
                    RenderOptions.SetClearTypeHint(tb, ClearTypeHint.Enabled);
                    RenderOptions.SetEdgeMode(tb, EdgeMode.Aliased);
                    drawingCanvas.Children.Add(tb);                    
                }));
            }
        }

        private Point[] GetPointsForAnimation(int distance, double tbWidth)
        {
            Point[] pts = new Point[2];

            Random rnd = new Random();
            int tempX = rnd.Next(distance, (int)(drawingCanvas.ActualWidth - distance -tbWidth));
            int tempY = rnd.Next(distance, (int)(drawingCanvas.ActualHeight -distance));

            pts[0] = new Point(tempX, tempY);

            int length = rnd.Next(distance -5, distance + 5);
            double angle = rnd.NextDouble() * 2 * Math.PI;


            tempX = (int)( tempX + Math.Cos(angle) * length );
            tempY = (int)( tempY + Math.Sin(angle) * length );

            pts[1] = new Point(tempX, tempY);

            return pts;
        }

        void da3_Completed(object sender, EventArgs e)
        {
            drawingCanvas.Children.Remove(Storyboard.GetTarget((sender as AnimationClock).Timeline) as TextBlock);
        }
        #endregion


        private void MySelf_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
