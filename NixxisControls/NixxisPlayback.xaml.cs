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
using System.Net;
using System.Configuration;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for NixxisPlayback.xaml
    /// </summary>
    public partial class NixxisPlayback : UserControl
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(Uri), typeof(NixxisPlayback), new PropertyMetadata(null, new PropertyChangedCallback(PathChanged)));

        public static void PathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisPlayback np = (NixxisPlayback)obj;

            np.myMediaElement.Stop();

            if (args.NewValue == null)
            {
                np.PlayPause.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                if (!string.IsNullOrEmpty(np.Path.OriginalString) /*&&  Helpers.GetFileSize(np.Path.AbsoluteUri) >=0*/)
                {
                    np.PlayPause.Visibility = System.Windows.Visibility.Visible;
                    np.btnError.Visibility = Visibility.Collapsed;
                    np.btnPlay.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    np.PlayPause.Visibility = System.Windows.Visibility.Collapsed;

            }
        }

        public Uri Path
        {
            get
            {
                return (Uri)GetValue(PathProperty);
            }
            set
            {
                SetValue(PathProperty, value);
            }
        }

        public NixxisPlayback()
        {
            InitializeComponent();
        }

        bool playing = false;

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (!playing)
            {
                myMediaElement.Source = Path;
                myMediaElement.Play();
                playing = true;
                btnStop.Visibility = System.Windows.Visibility.Visible;
                btnPlay.Visibility = System.Windows.Visibility.Collapsed;

            }
            else
            {
                myMediaElement.Stop();                
                playing = false;
                btnStop.Visibility = System.Windows.Visibility.Collapsed;
                btnPlay.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void myMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("ended");
            myMediaElement.Stop();
            playing = false;
            btnStop.Visibility = System.Windows.Visibility.Collapsed;
            btnPlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void myMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("failed");
            myMediaElement.Stop();
            playing = false;
            btnStop.Visibility = System.Windows.Visibility.Collapsed;
            btnError.Visibility = System.Windows.Visibility.Visible;
            PlayPause.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void myMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("opened");
        }

    }
}
