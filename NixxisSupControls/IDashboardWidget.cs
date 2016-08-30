using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Defines the states allowed for widget.
    /// </summary>
    public enum WidgetDisplayMode
    {
        /// <summary>
        /// The widget is displayed at run-time.
        /// </summary>
        Run,
        /// <summary>
        /// The widget is displayed in design mode, the dashboard container grid being visible.
        /// </summary>
        Design,
        /// <summary>
        /// The widget is displayed as an icon in the dashboard editor toolbar, ready for being drag dropped to the design grid. 
        /// </summary>
        Icon
    }

    /// <summary>
    /// Expose minimal fonctionalities required for dashboard widgets.
    /// </summary>
    public interface IDashboardWidget
    {
        /// <summary>
        /// The widget title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Indicates if the title must be displayed.
        /// </summary>
        Visibility TitleVisibility { get; }

        /// <summary>
        /// Indicates if the widget's background must be displayed.
        /// </summary>
        bool BackgroundVisible { get; set; }

        /// <summary>
        /// The widget name.
        /// </summary>
        string WidgetName { get; }

        /// <summary>
        /// Allow the widget designer to display diferently at runtime, design-time or when displayed as "icon".
        /// </summary>
        WidgetDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Currently selected theme.
        /// </summary>
        string CurrentTheme { get; set; }

        /// <summary>
        /// Called by the dahsboard container to specify the size of the widget. This size is expressed in dashboard container grid units.
        /// </summary>
        /// <param name="size">The size requested by the dashboard container.</param>
        /// <returns>The resulting size according to the widget possibilities.</returns>
        /// <remarks>If the method is called with (2,3) as paramter, that means that the widget is being resized by the dahboard container to fill 2 columns and 3 rows. The returned value can be diferent that the requested size if the widget itself is not able to accept the request.</remarks>
        Size SetSize(Size size);

        /// <summary>
        /// Called by the dashboard container to allow the widget doing tasks at regular interval. This method is called every seconds by default.
        /// </summary>
        /// <param name="count">A counter incremented at each method call. This parameter allows the widget designer to react only to some methods calls, executing tasks slower than the default interval.</param>
        void Timer(int count);

    }

}
