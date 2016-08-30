using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for SimpleDashboardControl.xaml
    /// </summary>
    public partial class DashboardWidget : UserControl, IXmlSerializable
    {

        public Point dragStartPoint = new Point();
        public bool newControl = false;

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register("ContentMargin", typeof(Thickness), typeof(DashboardWidget), new FrameworkPropertyMetadata(new Thickness(0,0,0,0)));


        public static readonly DependencyProperty WidgetProperty = DependencyProperty.Register("Widget", typeof(IDashboardWidget), typeof(DashboardWidget), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(WidgetPropertyChanged)));

        public static readonly DependencyProperty IsMovingProperty = DependencyProperty.Register("IsMoving", typeof(bool), typeof(DashboardWidget), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsResizingProperty = DependencyProperty.Register("IsResizing", typeof(bool), typeof(DashboardWidget), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DashboardWidget), new FrameworkPropertyMetadata(false));



        public DashboardWidget()
        {
            InitializeComponent();
        }

        public static void WidgetPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidget dbWidget = obj as DashboardWidget;
            if (obj != null)
            {
                if (args.OldValue is INotifyPropertyChanged)
                {
                    (args.OldValue as INotifyPropertyChanged).PropertyChanged -= dbWidget.DashboardWidget_PropertyChanged;
                }

                if (args.NewValue is INotifyPropertyChanged)
                {
                    (args.NewValue as INotifyPropertyChanged).PropertyChanged += dbWidget.DashboardWidget_PropertyChanged;
                }

                if(args.NewValue is UIElement)
                {
                    for (int i = 0; i < dbWidget.MainGrid.Children.Count; i++ )
                    {
                        if (dbWidget.MainGrid.Children[i] is IDashboardWidget)
                        {
                            dbWidget.MainGrid.Children.RemoveAt(i);
                            i--;
                        }
                    }
                    dbWidget.MainGrid.Children.Add(args.NewValue as UIElement);
                }
            }
        }

        public IDashboardWidget Widget
        {
            get 
            { 
                return (IDashboardWidget)GetValue(WidgetProperty); 
            }
            set 
            {
                SetValue(WidgetProperty, value); 
            }
        }

        public Thickness ContentMargin
        {
            get
            {
                return (Thickness)GetValue(ContentMarginProperty);
            }
            set
            {
                SetValue(ContentMarginProperty, value);
            }
        }
        public bool IsMoving
        {
            get
            {
                return (bool)GetValue(IsMovingProperty);
            }
            set
            {
                SetValue(IsMovingProperty, value);
            }
        }
        public bool IsResizing
        {
            get
            {
                return (bool)GetValue(IsResizingProperty);
            }
            set
            {
                SetValue(IsResizingProperty, value);
            }
        }
        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }


        private void DashboardWidget_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IDashboardWidget idbc = Widget;
            ClearValue(WidgetProperty);
            Widget = idbc;
        }

        private void ResizeThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as Thumb).Tag = null;
            dragStartPoint = e.GetPosition(null);
            IsResizing = true;
        }
        private void ResizeThumb_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);

            if (e.LeftButton == MouseButtonState.Pressed )
            {
                Thumb thumb = sender as Thumb;

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("resize", thumb);
                DragDrop.DoDragDrop(thumb, dragData, DragDropEffects.Copy);
            } 
        }
        private void MoveThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject temp = sender as DependencyObject;
            (sender as Thumb).Tag = null;
            while (!(temp is DashboardWidgetsContainer))
                temp = VisualTreeHelper.GetParent(temp);

            if(temp!=null)
            { 
                DashboardWidgetsContainer dwc = temp as DashboardWidgetsContainer;
                

                temp = sender as DependencyObject;
                while (temp!=null && !(temp is GridControl))
                    temp = VisualTreeHelper.GetParent(temp);

                if (temp != null)
                {
                    dwc.SelectedWidget = this;

                    GridControl gc = (GridControl)temp;

                    dragStartPoint = gc.GetColumnRow(e.GetPosition(gc));

                    dragStartPoint = new Point(dragStartPoint.X - GridControl.GetColumn(this), dragStartPoint.Y - GridControl.GetRow(this));

                    newControl = false;
                }
                else
                {
                    dragStartPoint = new Point(0, 0);

                    newControl = true;
                }
            }

            IsMoving = true;
        }
        private void MoveThumb_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Thumb thumb = sender as Thumb;

                if(newControl)
                {
                    // duplicate the currentcontrol

                    IDashboardWidget idbw = (IDashboardWidget)(Widget.GetType().GetConstructor(new Type[] { }).Invoke(null));
                    DashboardWidget dbw = new DashboardWidget();
                    dbw.Widget = idbw;
                    idbw.DisplayMode = WidgetDisplayMode.Design;

                    thumb = dbw.MoveThumb;
                }

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("move", thumb);

                DragDrop.DoDragDrop(thumb, dragData, DragDropEffects.Move);
            } 

        }

        private void WidgetDelete_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject temp = sender as DependencyObject;
            while (!(temp is DashboardWidgetsContainer))
                temp = VisualTreeHelper.GetParent(temp);

            DashboardWidgetsContainer dwc = temp as DashboardWidgetsContainer;
            dwc.DeleteWidget(this);
        }

        private void MySelf_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }       

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            string strWidgetType = reader.GetAttribute("FullType");
            string strAtt;

            strAtt = reader.GetAttribute("Column");
            if (string.IsNullOrEmpty(strAtt))
                GridControl.SetColumn(this, 0);
            else
                GridControl.SetColumn(this, System.Xml.XmlConvert.ToInt32(strAtt));

            strAtt = reader.GetAttribute("Row");
            if (string.IsNullOrEmpty(strAtt))
                GridControl.SetRow(this, 0);
            else
                GridControl.SetRow(this, System.Xml.XmlConvert.ToInt32(strAtt));

            strAtt = reader.GetAttribute("ColumnSpan");
            if (string.IsNullOrEmpty(strAtt))
                GridControl.SetColumnSpan(this, 1);
            else
                GridControl.SetColumnSpan(this, System.Xml.XmlConvert.ToInt32(strAtt));

            strAtt = reader.GetAttribute("RowSpan");
            if (string.IsNullOrEmpty(strAtt))
                GridControl.SetRowSpan(this, 1);
            else
                GridControl.SetRowSpan(this, System.Xml.XmlConvert.ToInt32(strAtt));


            Type widgetType = Type.GetType(strWidgetType);
            XmlSerializer serializer = new XmlSerializer(widgetType);
            reader.ReadStartElement();
            IDashboardWidget idbw = serializer.Deserialize(reader) as IDashboardWidget;
            Widget = idbw;
            idbw.DisplayMode = WidgetDisplayMode.Run;

            if(!reader.IsEmptyElement)
                reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("FullType", Widget.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Column", System.Xml.XmlConvert.ToString( GridControl.GetColumn(this)));
            writer.WriteAttributeString("Row", System.Xml.XmlConvert.ToString(GridControl.GetRow(this)));
            writer.WriteAttributeString("ColumnSpan", System.Xml.XmlConvert.ToString(GridControl.GetColumnSpan(this)));
            writer.WriteAttributeString("RowSpan", System.Xml.XmlConvert.ToString(GridControl.GetRowSpan(this)));

            XmlSerializer serializer = new XmlSerializer(Widget.GetType());
            serializer.Serialize(writer, Widget);
        }
    }


}
