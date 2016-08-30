using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Nixxis
{
    /// <summary>
    /// Interaction logic for StartPageControl.xaml
    /// </summary>
    public partial class StartPageControl : UserControl
    {
        public StartPageControl()
        {
            InitializeComponent();

            try
            {
                string[] ImageFiles = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), GetImagePrefix() + "*.png");

                foreach(string FileName in ImageFiles.Where(f => new FileInfo(f).Length > 3))
                {
                    try
                    {
                        BitmapImage Bmp = new BitmapImage();

                        Bmp.BeginInit();
                        Bmp.CacheOption = BitmapCacheOption.OnLoad;
                        Bmp.UriSource = new Uri("file:///" + FileName);
                        Bmp.EndInit();

                        Image Img = new Image();

                        string[] Details = Path.GetFileName(FileName).Split('.');

                        System.Windows.HorizontalAlignment HorizontalAlignment;
                        System.Windows.VerticalAlignment VerticalAlignment;
                        System.Windows.Media.Stretch Stretch;

                        if(Details.Length < 2 || !Enum.TryParse<System.Windows.VerticalAlignment>(Details[1], out VerticalAlignment))
                            VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        if(Details.Length < 3 || !Enum.TryParse<System.Windows.HorizontalAlignment>(Details[2], out HorizontalAlignment))
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        if(Details.Length < 4 || !Enum.TryParse<System.Windows.Media.Stretch>(Details[3], out Stretch))
                            Stretch = System.Windows.Media.Stretch.None;

                        Img.Source = Bmp;
                        Img.Stretch = Stretch;
                        Img.HorizontalAlignment = HorizontalAlignment;
                        Img.VerticalAlignment = VerticalAlignment;

                        ImgContainer.Children.Add(Img);
                        ImgLanding.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    catch
                    {}
                }
            }
            catch
            {
            }
        }

        protected virtual string GetImagePrefix()
        {
            return "LandingImg";
        }
    }
}
