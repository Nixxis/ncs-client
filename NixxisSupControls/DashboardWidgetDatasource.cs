using Nixxis.ClientV2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Class representing a single element from a datasource. 
    /// </summary>
    /// <conceptualLink target="383d4419-8d78-432c-847b-178b0c1f5347"/>
    public class DashboardDataItem : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the <see cref="DataContext"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(DashboardDataItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Identifies the <see cref="ObjectId"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectIdProperty = DependencyProperty.Register("ObjectId", typeof(string), typeof(DashboardDataItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ObjectIdPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="ObjectType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectTypeProperty = DependencyProperty.Register("ObjectType", typeof(string), typeof(DashboardDataItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ObjectTypePropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="ObjectProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectPropertyProperty = DependencyProperty.Register("ObjectProperty", typeof(string), typeof(DashboardDataItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ObjectPropertyPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DashboardDataItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ObjectValuePropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(DashboardDataItem), new FrameworkPropertyMetadata("---", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(DescriptionPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="DescriptionPropertyName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionPropertyNameProperty = DependencyProperty.Register("DescriptionPropertyName", typeof(string), typeof(DashboardDataItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(DescriptionPropertyNamePropertyChanged)));

        private static void ObjectIdPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardDataItem dataItem = obj as DashboardDataItem;
            dataItem.Evaluate();
        }

        private static void ObjectTypePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardDataItem dataItem = obj as DashboardDataItem;
            dataItem.Evaluate();
        }

        private static void ObjectPropertyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardDataItem dataItem = obj as DashboardDataItem;
            dataItem.Evaluate();
        }

        private static void ObjectValuePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardDataItem dataItem = obj as DashboardDataItem;
            dataItem.FirePropertyChanged("Value");
        }

        private static void DescriptionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardDataItem dataItem = obj as DashboardDataItem;
            dataItem.FirePropertyChanged("Description");
        }

        private static void DescriptionPropertyNamePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardDataItem dataItem = obj as DashboardDataItem;
            dataItem.Evaluate();
        }

        private void Evaluate()
        {
            if (!string.IsNullOrEmpty(ObjectType) && !string.IsNullOrEmpty(ObjectId) && !string.IsNullOrEmpty(ObjectProperty))
            {
                Binding bindingValue = new Binding(string.Format("{0}[{1}].{2}", ObjectType, ObjectId, ObjectProperty));
                bindingValue.Source = DataContext;
                BindingOperations.SetBinding(this, DashboardDataItem.ValueProperty, bindingValue);
            }

            if (!string.IsNullOrEmpty(ObjectType) && !string.IsNullOrEmpty(ObjectId) && !string.IsNullOrEmpty(DescriptionPropertyName))
            {
                Binding bindingValue = new Binding(string.Format("{0}[{1}].{2}", ObjectType, ObjectId, DescriptionPropertyName));
                bindingValue.Source = DataContext;
                BindingOperations.SetBinding(this, DashboardDataItem.DescriptionProperty, bindingValue);
                BindingOperations.GetBindingExpression(this, DashboardDataItem.DescriptionProperty).UpdateTarget();

            }

        }

        /// <summary>
        /// Get or set the data context used for bindings.
        /// </summary>
        /// <value>The data context object used for internal bindings.</value>
        /// <remarks>
        /// Related to <see cref="DataContextProperty"/> dependency property.
        /// </remarks>
        public object DataContext
        {
            get
            {
                return GetValue(DataContextProperty);
            }
            set
            {
                SetValue(DataContextProperty, value);
            }
        }

        /// <summary>
        /// Get or set the identifier of the supervision object.
        /// </summary>
        /// <value>Identifier of the supervision object.</value>
        /// <remarks>
        /// Related to <see cref="ObjectIdProperty"/> dependency property.
        /// </remarks>
        public string ObjectId
        {
            get
            {
                return (string)GetValue(ObjectIdProperty);
            }
            set
            {
                SetValue(ObjectIdProperty, value);
            }
        }

        /// <summary>
        /// Get or set the type of supervision object.
        /// </summary>
        /// <value>Supervision object type.</value>
        /// <remarks>
        /// Related to <see cref="ObjectTypeProperty"/> dependency property.
        /// </remarks>
        public string ObjectType
        {
            get
            {
                return (string)GetValue(ObjectTypeProperty);
            }
            set
            {
                SetValue(ObjectTypeProperty, value);
            }
        }

        /// <summary>
        /// Get or set the identifier of the supervision object property.
        /// </summary>
        /// <value>Identifier of the supervision obejct property.</value>
        /// <remarks>
        /// Related to <see cref="ObjectPropertyProperty"/> dependency property.
        /// </remarks>
        public string ObjectProperty
        {
            get
            {
                return (string)GetValue(ObjectPropertyProperty);
            }
            set
            {
                SetValue(ObjectPropertyProperty, value);
            }
        }

        /// <summary>
        /// Get or set the supervision object property value.
        /// </summary>
        /// <value>Value of the supervision object property.</value>
        /// <remarks>
        /// Related to <see cref="ValueProperty"/> dependency property.
        /// </remarks>
        public object Value
        {
            get
            {
                return GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                FirePropertyChanged("Value");
            }
        }

        /// <summary>
        /// Get or set the description of the supervision object.
        /// </summary>
        /// <value>Description of the supervision object.</value>
        /// <remarks>
        /// Related to <see cref="DescriptionProperty"/> dependency property.
        /// </remarks>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
                FirePropertyChanged("Description");
            }
        }

        /// <summary>
        /// Get or set the description of the property identified by <see cref="ObjectProperty"/>.
        /// </summary>
        /// <value>Description of the supervision object property.</value>
        /// <remarks>
        /// Related to <see cref="DescriptionPropertyNameProperty"/> dependency property.
        /// </remarks>
        public string DescriptionPropertyName
        {
            get
            {
                return (string)GetValue(DescriptionPropertyNameProperty);
            }
            set
            {
                SetValue(DescriptionPropertyNameProperty, value);
                FirePropertyChanged("DescriptionPropertyName");
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DashboardDataItem()
        {
        }

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Event for a property change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


    }

    /// <summary>
    /// Class describing a widget's datasource.
    /// </summary>
    /// <conceptualLink target="383d4419-8d78-432c-847b-178b0c1f5347"/>
    public class DashboardWidgetDataSource : DependencyObject, INotifyPropertyChanged, IXmlSerializable
    {
        /// <summary>
        /// Identifies the <see cref="DataContext"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(DashboardWidgetDataSource), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(DataContextPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="ObjectType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectTypeProperty = DependencyProperty.Register("ObjectType", typeof(string), typeof(DashboardWidgetDataSource), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ObjectTypePropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="ObjectIds"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectIdsProperty = DependencyProperty.Register("ObjectIds", typeof(ObservableCollection<String>), typeof(DashboardWidgetDataSource), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ObjectIdsPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="ObjectProperties"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectPropertiesProperty = DependencyProperty.Register("ObjectProperties", typeof(ObservableCollection<String>), typeof(DashboardWidgetDataSource), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ObjectPropertiesPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="Objects"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectsProperty = DependencyProperty.Register("Objects", typeof(ObservableCollection<DashboardDataSerie>), typeof(DashboardWidgetDataSource), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Identifies the <see cref="Properties"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(ObservableCollection<DashboardDataSerie>), typeof(DashboardWidgetDataSource), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        
        private static void ObjectTypePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetDataSource dataSource = obj as DashboardWidgetDataSource;

            if (dataSource.DataContext == null)
                return;


            if (!string.IsNullOrEmpty(args.OldValue as string))
            {
                object data = dataSource.DataContext.GetType().GetProperty(args.OldValue as string).GetValue(dataSource.DataContext, null);
                if (data is INotifyCollectionChanged)
                    ((INotifyCollectionChanged)data).CollectionChanged -= dataSource.DashboardWidgetDataSource_CollectionChanged;
            }

            if (!string.IsNullOrEmpty(args.NewValue as string))
            {
                object data = dataSource.DataContext.GetType().GetProperty(args.NewValue as string).GetValue(dataSource.DataContext, null);
                if (data is INotifyCollectionChanged)
                    ((INotifyCollectionChanged)data).CollectionChanged += dataSource.DashboardWidgetDataSource_CollectionChanged;
            }

            dataSource.Evaluate();

            dataSource.FirePropertyChanged("Description");
        }

        private static void ObjectIdsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetDataSource dataSource = obj as DashboardWidgetDataSource;


            if (args.OldValue != null)
                (args.OldValue as ObservableCollection<string>).CollectionChanged -= dataSource.ObjectIds_CollectionChanged;

            if (args.NewValue != null)
                (args.NewValue as ObservableCollection<string>).CollectionChanged += dataSource.ObjectIds_CollectionChanged;

            dataSource.Evaluate();
        }

        private void ObjectIds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Evaluate();
        }

        private static void ObjectPropertiesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetDataSource dataSource = obj as DashboardWidgetDataSource;

            if (args.OldValue != null)
                (args.OldValue as ObservableCollection<string>).CollectionChanged -= dataSource.ObjectProperties_CollectionChanged;

            if (args.NewValue != null)
                (args.NewValue as ObservableCollection<string>).CollectionChanged += dataSource.ObjectProperties_CollectionChanged;


            dataSource.Evaluate();
        }

        private static void DataContextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetDataSource dataSource = obj as DashboardWidgetDataSource;

            dataSource.Evaluate();
        }

        private void ObjectProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Evaluate();
        }

        /// <summary>
        /// Get or set the data context used for bindings.
        /// </summary>
        /// <value>The data context object used for internal bindings.</value>
        /// <remarks>
        /// Related to <see cref="DataContextProperty"/> dependency property.
        /// </remarks>
        public object DataContext
        {
            get
            {
                return GetValue(DataContextProperty);
            }
            set
            {
                SetValue(DataContextProperty, value);
            }
        }

        /// <summary>
        /// Get or set the type of supervision objects.
        /// </summary>
        /// <value>Supervision objects type.</value>
        /// <remarks>
        /// Related to <see cref="ObjectTypeProperty"/> dependency property.
        /// </remarks>
        public string ObjectType
        {
            get
            {
                return (string)GetValue(ObjectTypeProperty);
            }
            set
            {
                SetValue(ObjectTypeProperty, value);
            }
        }

        /// <summary>
        /// Get or set the collection of supervision objects identifiers.
        /// </summary>
        /// <value>Identifiers of the supervision objects.</value>
        /// <remarks>
        /// Related to <see cref="ObjectIdsProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<string> ObjectIds
        {
            get
            {
                return (ObservableCollection<string>)GetValue(ObjectIdsProperty);
            }
            set
            {
                SetValue(ObjectIdsProperty, value);
            }
        }

        /// <summary>
        /// Get or set the collection of supervision object properties identifiers.
        /// </summary>
        /// <value>Identifiers of the supervision obejct properties.</value>
        /// <remarks>
        /// Related to <see cref="ObjectPropertiesProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<string> ObjectProperties
        {
            get
            {
                return (ObservableCollection<string>)GetValue(ObjectPropertiesProperty);
            }
            set
            {
                SetValue(ObjectPropertiesProperty, value);
            }

        }

        private void Evaluate()
        {
            if (DataContext == null)
                return;

            if (Objects != null)
            {
                Objects.Clear();


                if (!string.IsNullOrEmpty(ObjectType) && ObjectProperties!=null && ObjectProperties.Count > 0)
                {
                    Type[] genericArguments = DataContext.GetType().GetProperty(ObjectType).PropertyType.BaseType.GetGenericArguments();
                    string descriptionProperty = "Description";
                    if(genericArguments.Length>0)
                        descriptionProperty = ((SupervisionDescriptionPropertyAttribute)(genericArguments[0].GetCustomAttributes(typeof(SupervisionDescriptionPropertyAttribute), true).FirstOrDefault())).Name;

                    List<string> objids = new List<string>(ObjectIds);

                    if (ObjectIds.Count == 0)
                    {
                        foreach (object obj in DataContext.GetType().GetProperty(ObjectType).GetValue(DataContext, null) as IEnumerable<object>)
                        {
                            objids.Add(obj.GetType().GetProperty("Id").GetValue(obj, null) as string);
                        }
                    }

                    foreach (string strObjId in objids)
                    {
                        Objects.Add(new DashboardDataSerie());


                        if (!string.IsNullOrEmpty(ObjectType) && !string.IsNullOrEmpty(strObjId) && !string.IsNullOrEmpty(descriptionProperty))
                        {
                            Binding bindingValue = new Binding(string.Format("{0}[{1}].{2}", ObjectType, strObjId, descriptionProperty));
                            bindingValue.Source = DataContext;
                            
                            BindingOperations.SetBinding(Objects[Objects.Count - 1], DashboardDataSerie.DescriptionProperty, bindingValue);

                            BindingOperations.GetBindingExpression(Objects[Objects.Count - 1], DashboardDataSerie.DescriptionProperty).UpdateTarget();

                        }
                        


                        for (int i = 0; i < ObjectProperties.Count; i++)
                        {
                            string strObjProp = ObjectProperties[i];
                            Objects[Objects.Count - 1].Values.Add(new DashboardDataItem() { DataContext = this.DataContext, ObjectType = this.ObjectType, ObjectProperty = strObjProp, ObjectId = strObjId, Description = PropertySelector.GetPropertyDescription(this.DataContext, this.ObjectType, strObjProp) });
                        }

                    }
                }
            }

            if (Properties != null)
            {
                Properties.Clear();

                if (!string.IsNullOrEmpty(ObjectType) && ObjectProperties!=null && ObjectProperties.Count > 0)
                {
                    List<string> objids = new List<string>(ObjectIds);

                    Type[] genericArguments = DataContext.GetType().GetProperty(ObjectType).PropertyType.BaseType.GetGenericArguments();
                    string descriptionProperty = "Description";
                    if(genericArguments.Length>0)
                        descriptionProperty = ((SupervisionDescriptionPropertyAttribute)(genericArguments[0].GetCustomAttributes(typeof(SupervisionDescriptionPropertyAttribute), true).FirstOrDefault())).Name;

                    if (ObjectIds.Count == 0)
                    {
                        foreach (object obj in DataContext.GetType().GetProperty(ObjectType).GetValue(DataContext, null) as IEnumerable<object>)
                        {
                            objids.Add(obj.GetType().GetProperty("Id").GetValue(obj, null) as string);
                        }
                    }


                    foreach (string strObjProp in ObjectProperties)
                    {
                        Properties.Add(new DashboardDataSerie());

                        Properties[Properties.Count - 1].Description = PropertySelector.GetPropertyDescription(DataContext, this.ObjectType, strObjProp);


                        for (int i = 0; i < objids.Count; i++)
                        {
                            string strObjId = objids[i];

                            Properties[Properties.Count - 1].Values.Add(new DashboardDataItem() { DataContext = this.DataContext, ObjectType = this.ObjectType, ObjectProperty = strObjProp, ObjectId = strObjId, DescriptionPropertyName = descriptionProperty });
                        }
                    }
                }
            }
        }

        private void DashboardWidgetDataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // nothing to do except if all ids have been selected...
            if (ObjectIds.Count == 0)
                Evaluate();
        }

        /// <summary>
        /// Get or set the collection of <see cref="DashboardDataSerie"/> representing the selected supervision objects. This property can be seen as a view on the datasource items grouped by objects, each serie being attached to one particular supervision object.
        /// </summary>
        /// <value>Collection of <see cref="DashboardDataSerie"/> representing the selected supervision objects.</value>
        /// <remarks>
        /// Related to <see cref="ObjectsProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<DashboardDataSerie> Objects
        {
            get
            {
                return (ObservableCollection<DashboardDataSerie>)GetValue(ObjectsProperty);
            }
            set
            {
                SetValue(ObjectsProperty, value);
            }
        }

        /// <summary>
        /// Get or set the collection of <see cref="DashboardDataSerie"/> representing the selected properties on supervision objects. This property can be seen as a view on the datasource items grouped by properties, each serie being attached to one particular supervision object property.
        /// </summary>
        /// <value>Collection of <see cref="DashboardDataSerie"/> representing the selected properties on supervision objects.</value>
        /// <remarks>
        /// Related to <see cref="PropertiesProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<DashboardDataSerie> Properties
        {
            get
            {
                return (ObservableCollection<DashboardDataSerie>)GetValue(PropertiesProperty);
            }
            set
            {
                SetValue(PropertiesProperty, value);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DashboardWidgetDataSource()
        {
            ObjectIds = new ObservableCollection<string>();

            ObjectProperties = new ObservableCollection<string>();

            Objects = new ObservableCollection<DashboardDataSerie>();
            Properties = new ObservableCollection<DashboardDataSerie>();
        }

        /// <summary>
        /// Description of the datasource.
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(ObjectType))
                    return "Not configured";
                return string.Format("Based on {0}", ObjectType);
            }
        }

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Event for a property change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/>. This is needed in order to be able to save and load datasource configuration.
        /// </summary>
        /// <returns>The <see cref="System.Xml.Schema.XmlSchema"/> corresponding to the datasource.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/>. This is needed in order to be able to save and load datasource configuration.
        /// Called by the dashboard framework to load the datasource.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> instance needed to load the datasource configuration.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            ObjectType = reader.GetAttribute("ObjectType");

            reader.ReadStartElement();

            List<string> strObjectIds = new List<string>();

            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                while (reader.MoveToContent() != System.Xml.XmlNodeType.EndElement)
                {
                    strObjectIds.Add(reader.ReadElementString("ObjectId"));
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }

            ObjectIds = new ObservableCollection<string>(strObjectIds);
                

            List<string> strProperties = new List<string>();

            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                while (reader.MoveToContent() != System.Xml.XmlNodeType.EndElement)
                {
                    strProperties.Add(reader.ReadElementString("Property"));
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }

            ObjectProperties = new ObservableCollection<string>(strProperties);              

            reader.ReadEndElement();
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/>. This is needed in order to be able to save and load datasource configuration.
        /// Called by the dashboard framework to save the datasource.
        /// </summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> instance needed to save the datasource configuration.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("ObjectType", ObjectType);
            writer.WriteStartElement("ObjectIds");
            foreach (string str in ObjectIds)
                writer.WriteElementString("ObjectId", str);
            writer.WriteEndElement();
            writer.WriteStartElement("Properties");
            foreach (string str in ObjectProperties)
                writer.WriteElementString("Property", str);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Class representing a serie of elements in datasource.
    /// </summary>
    /// <conceptualLink target="383d4419-8d78-432c-847b-178b0c1f5347"/>
    public class DashboardDataSerie : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the <see cref="Values"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register("Values", typeof(ObservableCollection<DashboardDataItem>), typeof(DashboardDataSerie), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(DashboardDataSerie), new FrameworkPropertyMetadata("---", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Get or set the <see cref="DashboardDataItem"/> collection related to the serie.
        /// </summary>
        /// <value>Collection of <see cref="DashboardDataItem"/> in the serie.</value>
        /// <remarks>
        /// Related to <see cref="ValuesProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<DashboardDataItem> Values
        {
            get
            {
                return (ObservableCollection<DashboardDataItem>)GetValue(ValuesProperty);
            }
            set
            {
                SetValue(ValuesProperty, value);
                FirePropertyChanged("Values");
            }
        }

        /// <summary>
        /// Get or set the description of the serie. The description of a serie can be the description of the supervision object it represents or the description of a supervision object property, depending on the context.
        /// </summary>
        /// <value>The description of the serie.</value>
        /// <remarks>
        /// Related to <see cref="DescriptionProperty"/> dependency property.
        /// </remarks>
        public string Description
        { 
            get 
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
                FirePropertyChanged("Description");
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DashboardDataSerie()
        {
            Values = new ObservableCollection<DashboardDataItem>();
        }

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Event for a property change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

    }

}
