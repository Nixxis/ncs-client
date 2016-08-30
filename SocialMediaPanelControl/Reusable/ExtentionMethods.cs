using System.Windows;
using System.Windows.Controls;

namespace Nixxis.Client.Agent.Reusable
{
    public static class ExtentionMethods
    {
        public static void AddElementToGrid(this Grid grid, UIElement element, int row, int column, int rowspan)
        {
            Grid.SetRowSpan(element, rowspan);
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            grid.Children.Add(element);
        }
    }
}
