using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Nixxis.Client.Supervisor
{
    // This container will maintain a square aspect ratio
    public class SquareContainer : ContentControl
    {
        private Size CalculateMaxSize(Size availableSize)
        {
            double maxDimension = Math.Min(availableSize.Height,availableSize.Width);

            Size maxSize = new Size(maxDimension, maxDimension);

            return maxSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size maxSize = CalculateMaxSize(availableSize);

            this.Width = maxSize.Width;
            this.Height = maxSize.Height;
            
            if (Content != null && Content is FrameworkElement)
            {
                ((FrameworkElement)Content).Width = maxSize.Width;
                ((FrameworkElement)Content).Height = maxSize.Height; 
                ((FrameworkElement)Content).Measure(maxSize);
            }
            return maxSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size maxSize = CalculateMaxSize(finalSize);

            if (Content != null && Content is FrameworkElement)
            {
                ((FrameworkElement)Content).Arrange(new Rect(new Point(0, 0), maxSize));
            }

            return maxSize;
        }
    }

}
