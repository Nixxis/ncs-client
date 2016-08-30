using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using Nixxis.Client.Controls;
using System.IO;
using System.Configuration;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    public interface IMainWindow
    {
        string LoggedIn { get; }
        AdminFrameSet AdminFrameSet { get; }
        AdminCore Core { get; set; }
        void ReloadCore(AdminCore core);
    }

    /// <summary>
    /// Interface defining the properties related to processor configuration dialog.
    /// </summary>
    public interface IPreprocessorConfigurationDialog
    {
        /// <summary>
        /// Get or set the <see cref="Window"/> object owning the dialog.
        /// </summary>
        Window Owner{get;set;}
        /// <summary>
        /// Get or set the context related to the selected <see cref="AdminObject"/> instance.
        /// </summary>
        object WizControlContext { get; set; }
        /// <summary>
        /// Get or set the global context (<see cref="AdminCore"/> instance) related to the configuration dialog.
        /// </summary>
        object DataContext {get;set;}
        /// <summary>
        /// Get or set the <see cref="BasePreprocessorConfig"/> object specifying the configuration to be displayed.
        /// </summary>
        BasePreprocessorConfig Config{get;set;}
    }

    public class GeneralSettings: INotifyPropertyChanged
    {
        private bool m_IsOrientationHorizontal = true;
        private bool m_IsFullVersion = false;
        private bool m_IsDebug = false;
        private IMainWindow m_MainWindow;

        public GeneralSettings(IMainWindow mainWindow)
        {
            m_MainWindow = mainWindow;
        }

        public string CurrentUserInfo
        {
            get
            {
                Agent agt = null;
                if (m_MainWindow.AdminFrameSet != null && m_MainWindow.AdminFrameSet.TheCore != null)
                {
                        agt = m_MainWindow.AdminFrameSet.TheCore.Agents.FirstOrDefault((a) => (a.Id.Equals(m_MainWindow.LoggedIn)));                    
                }
                else
                {
                        agt = m_MainWindow.Core.Agents.FirstOrDefault((a) => (a.Id.Equals(m_MainWindow.LoggedIn)));
                }
                if (agt != null)
                {
                    return string.Format("{0}, {1}", agt.DisplayText, agt.Id == "defaultagent++++++++++++++++++++" ? "Super User" : string.Empty);
                }
                return string.Empty;

            }
        }
        public bool IsFullVersion
        {
            get
            {
                return m_IsFullVersion;
            }
            set
            {
                m_IsFullVersion = value;
                FirePropertyChanged("IsFullVersion");
                if (m_MainWindow.AdminFrameSet != null && m_MainWindow.AdminFrameSet.TheCore != null)
                {
                    m_MainWindow.AdminFrameSet.TheCore.Campaigns.FireCollectionChanged(NotifyCollectionChangedAction.Reset);
                }
            }
        }
        public bool IsVersionSwitchAllowed
        {
            get
            {

                string version = ConfigurationManager.AppSettings["Version"];

                if (string.IsNullOrEmpty(version))
                {

                    object flags = AppDomain.CurrentDomain.GetData("service_flags");

                    if (flags == null)
                    {
                        version = "NCS";
                    }
                    else
                    {
                        if (flags is int && ((int)flags > 0) || flags is string && Int32.Parse((string)flags) > 0)
                            version = "NCS";
                        else
                            version = "Express";
                    }                   

                }

                if (string.IsNullOrEmpty(version) || version.Equals("Demo"))
                {
                    return true;
                }
                else
                {
                    if (version.Equals("Express"))
                    {
                        IsFullVersion = false;
                    }
                    else
                    {
                        IsFullVersion = true;
                    }
                    return false;
                }

            }
        }
        public bool IsDebug
        {
            get
            {
                return m_IsDebug;
            }
            set
            {
                m_IsDebug = value;
                FirePropertyChanged("IsDebug");
            }

        }
        public bool IsCloud
        {
            get
            {
                object mode = AppDomain.CurrentDomain.GetData("service_mode");
                return mode != null && mode is string && ((string)mode).Equals("CLOUD", StringComparison.InvariantCultureIgnoreCase);
            }
        }
        public bool IsSuperUser
        {
            get
            {
                bool issuperuser = false;
                Agent agt = null;
                if (m_MainWindow.AdminFrameSet != null && m_MainWindow.AdminFrameSet.TheCore != null)
                {
                    agt = m_MainWindow.AdminFrameSet.TheCore.Agents.FirstOrDefault((a) => (a.Id.Equals(m_MainWindow.LoggedIn)));
                    issuperuser = agt != null && agt.AdministrationLevel>999;
                }
                else
                {
                    agt = m_MainWindow.Core.Agents.FirstOrDefault((a) => (a.Id.Equals(m_MainWindow.LoggedIn)));
                    issuperuser = agt != null && agt.AdministrationLevel>999;
                }

                return issuperuser;
            }
        }
        public bool IsNotCloudOrSuperUser
        {
            get
            {
                return !IsCloud || IsSuperUser;
            }
        }

        public bool IsOrientationHorizontal
        {
            get
            {
                return m_IsOrientationHorizontal;
            }
            set
            {
                m_IsOrientationHorizontal = value;
                FirePropertyChanged("IsOrientationHorizontal");
                FirePropertyChanged("PanelsOrientation");
            }
        }
        public Orientation PanelsOrientation
        {
            get
            {
                if (IsOrientationHorizontal)
                    return Orientation.Horizontal;
                else
                    return Orientation.Vertical;
            }
        }

        public void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class RightsManagement : UIElement
    {
        public static readonly DependencyProperty RightsProperty = DependencyProperty.RegisterAttached("Rights", typeof(string), typeof(RightsManagement), new PropertyMetadata(null, new PropertyChangedCallback(RightsChanged)));
        public static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached("Binding", typeof(AdminObject), typeof(RightsManagement), new PropertyMetadata(null, new PropertyChangedCallback(BindingChanged)));

        public static void SetRights(UIElement element, string value)
        {
            element.SetValue(RightsProperty, value);
        }
        public static string GetRights(UIElement element)
        {
            return (string)(element.GetValue(RightsProperty));
        }

        public static void SetBinding(UIElement element, string value)
        {
            element.SetValue(BindingProperty, value);
        }
        public static string GetBinding(UIElement element)
        {
            return (string)(element.GetValue(BindingProperty));
        }

        public static void RightsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                if (obj is UIElement)
                {
                    UIElement uie = (UIElement)obj;
                    if (uie is DataGrid)
                        ((DataGrid)uie).IsReadOnly = false;

                    string userLoggedIn = ((IMainWindow)Application.Current.MainWindow).LoggedIn;

                    if (userLoggedIn == "defaultagent++++++++++++++++++++")
                        return;

                    Agent agt = ((IMainWindow)Application.Current.MainWindow).Core.Agents[userLoggedIn];

                    string strRights = GetRights(uie);

                    if (strRights != null)
                    {
                        string strType = null;
                        string typedCheck = null;
                        AdminObject aobj = null;

                        if (strRights.StartsWith("Application."))
                        {
                            if (strRights.Equals("Application.Agents"))
                            {
                                typedCheck = "adminagents+++++++++++++++++++++";

                                aobj = agt.Core.Agents.FirstOrDefault((q) => (!q.IsDummy && !q.IsPartial && q.Id != agt.Id));
                            }
                            else if (strRights.Equals("Application.Teams"))
                            {
                                typedCheck = "adminteams++++++++++++++++++++++";

                                aobj = agt.Core.Teams.FirstOrDefault((q) => (!q.IsDummy && !q.IsPartial));
                            }
                            else if (strRights.Equals("Application.Campaigns"))
                            {
                                typedCheck = "admincampaigns++++++++++++++++++";

                                aobj = agt.Core.Campaigns.FirstOrDefault((q) => (!q.IsDummy && !q.IsPartial));
                            }
                            else if (strRights.Equals("Application.Queues"))
                            {
                                typedCheck = "adminqueues+++++++++++++++++++++";

                                aobj = agt.Core.Queues.FirstOrDefault((q) => (!q.IsDummy && !q.IsPartial));
                            }
                            else
                            {
                                typedCheck = "adminothers+++++++++++++++++++++";
                            }


                            bool? defaultsetting = (agt.Core.DefaultSettings).Is_Visible("admin+++++++++++++++++++++++++++");
                            bool? defaultsetting2 = (agt.Core.DefaultSettings).Is_Visible(typedCheck);


                            if (!defaultsetting.HasValue)
                            {
                                if (!defaultsetting2.HasValue || !defaultsetting2.Value)
                                {
                                    if (aobj == null)
                                    {
                                        uie.Visibility = Visibility.Collapsed;
                                        uie.IsVisibleChanged += uie_IsVisibleChanged;
                                    }
                                }

                            }
                            else if (defaultsetting.Value)
                            {
                                if (defaultsetting2.HasValue && !defaultsetting2.Value)
                                {
                                    if (aobj == null)
                                    {
                                        uie.Visibility = Visibility.Collapsed;
                                        uie.IsVisibleChanged += uie_IsVisibleChanged;
                                    }
                                }
                            }
                            else
                            {
                                if (aobj == null)
                                {
                                    uie.Visibility = Visibility.Collapsed;
                                    uie.IsVisibleChanged += uie_IsVisibleChanged;
                                }
                            }                            
                        }

                        if (strRights.EndsWith(".Add") || strRights.EndsWith(".Duplicate"))
                        {
                            // the begining indicates the type.

                            if(strRights.EndsWith(".Add"))
                                strType = strRights.Substring(0, strRights.Length - ".Add".Length);
                            else
                                strType = strRights.Substring(0, strRights.Length - ".Duplicate".Length);

                            
                            switch (strType)
                            {
                                case "Agent":
                                    typedCheck = "adminagents+++++++++++++++++++++";
                                    break;
                                case "Queue":
                                    typedCheck = "adminqueues+++++++++++++++++++++";
                                    break;
                                case "Campaign":
                                    typedCheck = "admincampaigns++++++++++++++++++";
                                    break;
                                case "Team":
                                    typedCheck = "adminteams++++++++++++++++++++++";
                                    break;
                                default:
                                    typedCheck = "adminothers+++++++++++++++++++++";
                                    break;
                            }


                            if (strType == "Campaign")
                            {
                            }
                            else
                            {

                                bool? defaultsetting = (agt.Core.DefaultSettings).TypedComputedCreatability("admin+++++++++++++++++++++++++++");
                                bool? defaultsetting2 = (agt.Core.DefaultSettings).TypedComputedCreatability(typedCheck);

                                if (defaultsetting.HasValue)
                                {
                                    if (defaultsetting.Value)
                                    {
                                        if (defaultsetting2.HasValue && !defaultsetting2.Value)
                                        {
                                            uie.Visibility = Visibility.Collapsed;
                                            uie.IsVisibleChanged += uie_IsVisibleChanged;
                                        }
                                        else
                                        {
                                            // visible cause defaultsetting.value is true
                                        }
                                    }
                                    else
                                    {
                                        uie.Visibility = Visibility.Collapsed;
                                        uie.IsVisibleChanged += uie_IsVisibleChanged;
                                    }
                                }
                                else
                                {
                                    if (!defaultsetting2.HasValue || !defaultsetting2.Value)
                                    {
                                        uie.Visibility = Visibility.Collapsed;
                                        uie.IsVisibleChanged += uie_IsVisibleChanged;
                                    }
                                }
                            }
                            // special case: campaigns and activities...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        public static void BindingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is UIElement)
            {
                UIElement uie = (UIElement)obj;
                string str = GetRights(uie);
                AdminObject aobj = args.NewValue as AdminObject;
                if (aobj != null)
                {
                    bool enabled = aobj.CheckEnabled(str);

                    if (uie is DataGrid)
                        ((DataGrid)uie).IsReadOnly = enabled;
                    else
                        uie.IsEnabled = enabled;
                }
            }
        }

        static void uie_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.NewValue) == true)
                (sender as UIElement).Visibility = Visibility.Collapsed;
        }

    }


    public class ExtendedEnumerable :  IEnumerable, INotifyCollectionChanged, IWeakEventListener
    {
        private bool m_AddNullObject = false;

        public ExtendedEnumerable(IEnumerable list): this(list, true)
        {

        }

        public ExtendedEnumerable(IEnumerable list, bool addNullObject)
        {
            m_AddNullObject = addNullObject;
            InternalList = list;
            if(list is INotifyCollectionChanged)
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)list, this);
        }


        public IEnumerable InternalList
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            if (InternalList != null)
            {
                if(m_AddNullObject)
                {
                    yield return new NullAdminObject();
                }

                IEnumerator InternalEnumerator = System.Linq.Queryable.AsQueryable<AdminObject>((IEnumerable<AdminObject>)InternalList).OrderBy((a) => (a.DisplayText)).GetEnumerator();

                while (InternalEnumerator.MoveNext())
                    if(InternalEnumerator.Current!=null)
                        yield return InternalEnumerator.Current;
            }
        }

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                if (m_AddNullObject && args.Action == NotifyCollectionChangedAction.Remove)
                {
                    // TODO: workaround to avoid exceptions!!!!
                    NotifyCollectionChangedEventArgs args2 = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    CollectionChanged(this, args2);
                }
                else
                {
                    CollectionChanged(this, args);
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            FireCollectionChanged((NotifyCollectionChangedEventArgs)e);
            return true;
        }
        
    }

    public class ExtendedEnumerableNoDummies : IEnumerable, INotifyCollectionChanged, IWeakEventListener
    {
        private bool m_AddNullObject = false;

        public ExtendedEnumerableNoDummies(IEnumerable list)
            : this(list, true)
        {

        }

        public ExtendedEnumerableNoDummies(IEnumerable list, bool addNullObject)
        {
            m_AddNullObject = addNullObject;
            InternalList = list;
            if (list is INotifyCollectionChanged)
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)list, this);
        }


        public IEnumerable InternalList
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            if (InternalList != null)
            {
                if (m_AddNullObject)
                {
                    yield return new NullAdminObject();
                }

                IEnumerator InternalEnumerator = System.Linq.Queryable.AsQueryable<AdminObject>((IEnumerable<AdminObject>)InternalList).OrderBy((a) => (a.DisplayText)).GetEnumerator();


                while (InternalEnumerator.MoveNext())
                    if (InternalEnumerator.Current != null && !((AdminObject)InternalEnumerator.Current).IsDummy)
                        yield return InternalEnumerator.Current;
            }
        }

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    // TODO: workaround to avoid exceptions!!!!
                    NotifyCollectionChangedEventArgs args2 = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    CollectionChanged(this, args2);
                }
                else
                {
                    CollectionChanged(this, args);
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            FireCollectionChanged((NotifyCollectionChangedEventArgs)e);
            return true;
        }

    }


    public class ExtendedStringEnumerable : IEnumerable, INotifyCollectionChanged, IWeakEventListener
    {
        public class StringHelper
        {
            private string m_DisplayText;
            private string m_Id;

            public string DisplayText
            {
                get
                {
                    return m_DisplayText;
                }
            }
            public string Id
            {
                get
                {
                    return m_Id;
                }
            }

            public StringHelper(string description)
            {
                if (description == null)
                    m_DisplayText =  TranslationContext.Default.Translate("<none>");
                else
                    m_DisplayText = description;

                m_Id = description;
            }
        }


        public ExtendedStringEnumerable(IEnumerable list)
            : this(list, true)
        {

        }

        public ExtendedStringEnumerable(IEnumerable list, bool addNullObject)
        {
            InternalList = list;
            if (list is INotifyCollectionChanged)
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)list, this);
        }

        public IEnumerable InternalList
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            if (InternalList != null)
            {
                yield return new StringHelper(null);

                IEnumerator InternalEnumerator = System.Linq.Queryable.AsQueryable<string>((IEnumerable<string>)InternalList).OrderBy((a) => (a)).GetEnumerator();

                while (InternalEnumerator.MoveNext())
                    yield return new StringHelper(InternalEnumerator.Current as string);
            }
        }

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    // TODO: workaround to avoid exceptions!!!!
                    NotifyCollectionChangedEventArgs args2 = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    CollectionChanged(this, args2);
                }
                else
                {
                    CollectionChanged(this, args);
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            FireCollectionChanged((NotifyCollectionChangedEventArgs)e);
            return true;
        }

    }

    public class TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ComboListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new ExtendedEnumerable((IEnumerable)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ComboListConverterNoDummies : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null)
            {
                try
                {
                    return new ExtendedEnumerableNoDummies((IEnumerable)value, bool.Parse(parameter as string));
                }
                catch
                {
                }
            }
            return new ExtendedEnumerableNoDummies((IEnumerable)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsDummyIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strId = value as string;
            bool inverted = false;

            if (parameter != null)
            {
                try
                {
                    inverted = bool.Parse((string)parameter);
                }
                catch
                {
                }
            }

            if (strId == null)
                strId = string.Empty;

            if (targetType == typeof(Visibility))
            {
                if (inverted)
                    return !AdminObject.IsDummyId(strId) ? Visibility.Visible : Visibility.Collapsed;
                else
                    return AdminObject.IsDummyId(strId) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                if (inverted)
                    return !AdminObject.IsDummyId(strId);
                else
                    return AdminObject.IsDummyId(strId);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class ComboStringListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new ExtendedStringEnumerable((IEnumerable)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OperatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new OperatorHelper().First( (a) => (a.EnumValue == (Operator)value) ).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new OperatorHelper().First((a) => (a.Description == (string)value)).EnumValue;
        }
    }

    public class DurationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is Single)
                {
                    Single s = (Single)value;
                    return 60 * (Decimal)s;
                }

                if (string.IsNullOrEmpty(value as string))
                    return Decimal.Zero;

                Decimal temp = Decimal.Zero;
                Decimal.TryParse((string)value, out temp);
                return 60 * temp;
            }
            catch
            {
                return Decimal.Zero;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Decimal)(value)/60).ToString();
        }
    }

    public class DurationConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return DurationHelpers.GetDefaultDurationString(Decimal.Zero,false);
            if (value is Single)
            {
                Single s = (Single)value;                
                return DurationHelpers.GetDefaultDurationString(60 * (Decimal)s, false);
            }
            if (value is Int32)
            {
                Single s = (int)value;
                return DurationHelpers.GetDefaultDurationString(60 * (Decimal)s, false);

            }
            Decimal temp = Decimal.Zero;
            Decimal.TryParse((string)value, out temp);
            return DurationHelpers.GetDefaultDurationString(60 * temp, false);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DurationConverter3 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return DurationHelpers.GetDefaultDurationString(Decimal.Zero, false);
            if (value is Single)
            {
                Single s = (Single)value;
                return DurationHelpers.GetDefaultDurationString( (Decimal)s, false);
            }
            Decimal temp = Decimal.Zero;
            Decimal.TryParse((string)value, out temp);
            return DurationHelpers.GetDefaultDurationString(temp, false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class OperatorTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (DependencyProperty.UnsetValue.Equals(values[0]))
                return Visibility.Collapsed;

            DBTypes dbt = (DBTypes)values[0];
            Operator op = (Operator)values[1];
            LookupTypes lt = LookupTypes.None;
            if (values.Length >= 3 && !DependencyProperty.UnsetValue.Equals(values[2]))
            {
                lt = (LookupTypes)values[2];
            }
            if (lt != LookupTypes.None)
            {
                OperandType ot = new OperatorHelper().First((a) => (a.EnumValue == op)).OperandType;

                switch (ot)
                {
                    case OperandType.Duration:
                        throw new NotSupportedException("We don't expect a lookup on a duration!");

                    case OperandType.None:
                        return Visibility.Collapsed;

                    case OperandType.Same:
                        if ((DBTypes)parameter == dbt)
                            return Visibility.Collapsed;
                        else
                        {
                            if (dbt == DBTypes.String || dbt == DBTypes.Char)
                                if ((DBTypes)parameter == DBTypes.AgentString && lt == LookupTypes.Agents || (DBTypes)parameter == DBTypes.ActivityString && lt == LookupTypes.Activities || (DBTypes)parameter == DBTypes.AreaString && lt == LookupTypes.Area || (DBTypes)parameter == DBTypes.QualificationString && lt == LookupTypes.Qualifications)
                                    return Visibility.Visible;
                            return Visibility.Collapsed;                       
                        }
                }
                return null;
            }
            else
            {
                OperandType ot = new OperatorHelper().First((a) => (a.EnumValue == op)).OperandType;
                switch (ot)
                {
                    case OperandType.Duration:
                        if ((DBTypes)parameter == DBTypes.Time)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;

                    case OperandType.None:
                        return Visibility.Collapsed;

                    case OperandType.Same:
                        if ((DBTypes)parameter == dbt)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;
                }
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DlgOperatorTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (DependencyProperty.UnsetValue.Equals(values[0]))
                return Visibility.Collapsed;

            DBTypes dbt = (DBTypes)values[0];

            if (values[1] == null || DependencyProperty.UnsetValue.Equals(values[1]) || (Aggregator)values[1] == Aggregator.None)
            {
            }
            else
            {
                dbt = DBTypes.Integer;
            }

            Operator op = Operator.IsNotNull;
            LookupTypes lt = LookupTypes.None;
            if (values.Length >=9 && !DependencyProperty.UnsetValue.Equals(values[8]))
            {
                lt = (LookupTypes)values[8];
            }



            switch (dbt)
            {
                case DBTypes.Boolean:
                    if (values[7] == null)
                        return Visibility.Collapsed;
                    op = (Operator)values[7];
                    break;

                case DBTypes.Datetime:
                    if (values[6] == null)
                        return Visibility.Collapsed;
                    op = (Operator)values[6];
                    break;
                case DBTypes.Integer:
                    if (values[4] == null)
                        return Visibility.Collapsed;
                    op = (Operator)values[4];
                    break;

                case DBTypes.Float:
                    if (values[5] == null)
                        return Visibility.Collapsed;
                    op = (Operator)values[5];
                    break;

                case DBTypes.String:
                    if (values[2] == null)
                        return Visibility.Collapsed;
                    op = (Operator)values[2];
                    break;
                case DBTypes.Char:
                    if (values[3] == null)
                        return Visibility.Collapsed;
                    op = (Operator)values[3];
                    break;
            }


            if (lt != LookupTypes.None)
            {
                OperandType ot = new OperatorHelper().First((a) => (a.EnumValue == op)).OperandType;

                switch (ot)
                {
                    case OperandType.Duration:
                        throw new NotSupportedException("We don't expect a lookup on a duration!");

                    case OperandType.None:
                        return Visibility.Collapsed;

                    case OperandType.Same:
                        if ((DBTypes)parameter == dbt)
                            return Visibility.Collapsed;
                        else
                        {
                            if (dbt == DBTypes.String || dbt == DBTypes.Char) 
                                if ( (DBTypes)parameter == DBTypes.AgentString && lt==LookupTypes.Agents || (DBTypes)parameter == DBTypes.ActivityString && lt==LookupTypes.Activities || (DBTypes)parameter == DBTypes.AreaString && lt==LookupTypes.Area || (DBTypes)parameter == DBTypes.QualificationString && lt==LookupTypes.Qualifications)
                                    return Visibility.Visible;      
                            return Visibility.Collapsed;
                        }
                }
                return null;
            }
            else
            {

                OperandType ot = new OperatorHelper().First((a) => (a.EnumValue == op)).OperandType;
                switch (ot)
                {
                    case OperandType.Duration:
                        if (parameter == null || (DBTypes)parameter == DBTypes.Time)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;

                    case OperandType.None:
                        return Visibility.Collapsed;

                    case OperandType.Same:
                        if (parameter == null || (DBTypes)parameter == dbt)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;
                }
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class OperatorTypeConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)(values[0]))
                return Visibility.Collapsed;
            
            Operator op = (Operator)values[1];

            OperandType ot = new OperatorHelper().First((a) => (a.EnumValue == op)).OperandType;
            switch (ot)
            {
                case OperandType.Duration:
                    if (parameter != null && (DBTypes)parameter == DBTypes.Time)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;

                case OperandType.None:
                case OperandType.Same:
                    if (parameter == null || (DBTypes)parameter != DBTypes.Time)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [AdminSave(SkipSave = true)]
    [DialogType(Type="Nixxis.Client.Admin.DlgStdCallbackConfigure, NixxisAdminControls")]
    public class CallbackPreprocessorConfig : BasePreprocessorConfig
    {
        public CallbackPreprocessorConfig(AdminObject parent)
            : base(parent)
        {
        }

        public CallbackPreprocessorConfig(AdminCore core)
            : base(core)
        {
        }


        public bool EnqueueCallbackRequest
        {
            get
            {
                return GetFieldValue<bool>("Enqueue");
            }
            set
            {
                SetFieldValue<bool>("Enqueue", value);
            }
        }
        public bool AskDate
        {
            get
            {
                return GetFieldValue<bool>("AskDate");
            }
            set
            {
                SetFieldValue<bool>("AskDate", value);
            }
        }
        public bool AskTime
        {
            get
            {
                return GetFieldValue<bool>("AskTime");
            }
            set
            {
                SetFieldValue<bool>("AskTime", value);
            }
        }
        public bool AlwaysAskPhone
        {
            get
            {
                return GetFieldValue<bool>("AlwaysAskPhone");
            }
            set
            {
                SetFieldValue<bool>("AlwaysAskPhone", value);
            }
        }

        public AdminObjectReference<Prompt> Announce
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> AskPhone
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> RepeatPhone
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> Accept
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> ReEnter
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> CallAsap
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> CallLater
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> PromptAskDate
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> PromptAskTime
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> RepeatDateTime
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> NotValid
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> Sorry
        {
            get;
            internal set;
        }
        public AdminObjectReference<Prompt> ThankYou
        {
            get;
            internal set;
        }

        public override void DeserializeFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            using (StringReader reader = new StringReader(text))
            {
                bool AnnounceLoaded = false;
                bool AskPhoneLoaded = false;
                bool CallAsapLoaded = false;
                bool PromptAskDateLoaded = false;
                bool PromptAskTimeLoaded = false;
                bool NotValidLoaded = false;
                bool SorryLoaded = false;
                bool ThankYouLoaded = false;
                bool CallLaterLoaded = false;
                bool ReEnterLoaded = false;
                bool AcceptLoaded = false;
                bool RepeatDateTimeLoaded = false;
                bool RepeatPhoneLoaded = false;

                string line = null;
                string temp = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("ASKDATE="))
                    {
                        temp = line.Substring("ASKDATE=".Length);
                        AskDate = (temp == "1");
                    }
                    else if (line.StartsWith("ENQUEUE="))
                    {
                        temp = line.Substring("ENQUEUE=".Length);
                        EnqueueCallbackRequest = (temp == "1");
                    }
                    else if (line.StartsWith("ASKTIME="))
                    {
                        temp = line.Substring("ASKTIME=".Length);
                        AskTime = (temp == "1");
                    }
                    else if (line.StartsWith("ASKNUMBER="))
                    {
                        temp = line.Substring("ASKNUMBER=".Length);
                        AlwaysAskPhone = (temp == "1");
                    }
                    else if (line.StartsWith("MSGANNOUNCE") && !AnnounceLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            Announce.TargetId = temp.Split('/')[1];
                        else
                            Announce.TargetId = null;
                        AnnounceLoaded = true;
                    }
                    else if (line.StartsWith("MSGREQUESTNR") && !AskPhoneLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            AskPhone.TargetId = temp.Split('/')[1];
                        else
                            AskPhone.TargetId = null;
                        AskPhoneLoaded = true;
                    }
                    else if (line.StartsWith("MSGCALLASAP") && !CallAsapLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            CallAsap.TargetId = temp.Split('/')[1];
                        else
                            CallAsap.TargetId = null;
                        CallAsapLoaded = true;
                    }
                    else if (line.StartsWith("MSGCALLLATER") && !CallLaterLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            CallLater.TargetId = temp.Split('/')[1];
                        else
                            CallLater.TargetId = null;
                        CallLaterLoaded = true;
                    }
                    else if (line.StartsWith("MSGNOTVALID") && !NotValidLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            NotValid.TargetId = temp.Split('/')[1];
                        else
                            NotValid.TargetId = null;
                        NotValidLoaded = true;
                    }
                    else if (line.StartsWith("MSGASKDATE") && !PromptAskDateLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            PromptAskDate.TargetId = temp.Split('/')[1];
                        else
                            PromptAskDate.TargetId = null;
                        PromptAskDateLoaded = true;
                    }
                    else if (line.StartsWith("MSGASKTIME") && !PromptAskTimeLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            PromptAskTime.TargetId = temp.Split('/')[1];
                        else
                            PromptAskTime.TargetId = null;
                        PromptAskTimeLoaded = true;
                    }
                    else if (line.StartsWith("MSGTOREENTER") && !ReEnterLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            ReEnter.TargetId = temp.Split('/')[1];
                        else
                            ReEnter.TargetId = null;
                        ReEnterLoaded = true;
                    }
                    else if (line.StartsWith("MSGTOACCEPT") && !AcceptLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            Accept.TargetId = temp.Split('/')[1];
                        else
                            Accept.TargetId = null;
                        AcceptLoaded = true;
                    }
                    else if (line.StartsWith("MSGREPEAT") && !RepeatDateTimeLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            RepeatDateTime.TargetId = temp.Split('/')[1];
                        else
                            RepeatDateTime.TargetId = null;
                        RepeatDateTimeLoaded = true;
                    }
                    else if (line.StartsWith("MSGCURRENTNR") && !RepeatPhoneLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            RepeatPhone.TargetId = temp.Split('/')[1];
                        else
                            RepeatPhone.TargetId = null;
                        RepeatPhoneLoaded = true;
                    }
                    else if (line.StartsWith("MSGSORRY") && !SorryLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            Sorry.TargetId = temp.Split('/')[1];
                        else
                            Sorry.TargetId = null;
                        SorryLoaded = true;
                    }
                    else if (line.StartsWith("MSGTHANKYOU") && !ThankYouLoaded)
                    {
                        temp = line.Split('=')[1];
                        if (temp.Contains(@"/"))
                            ThankYou.TargetId = temp.Split('/')[1];
                        else
                            ThankYou.TargetId = null;
                        ThankYouLoaded = true;
                    }
                }
            }
        }

        private void SavePrompt(StringBuilder sb, AdminObjectReference<Prompt> prompt, string label)
        {
            if (prompt.HasTarget)
            {
                sb.AppendFormat("{0}{1}={2}\n", label, prompt.Target.Language.HasTarget ? prompt.Target.Language.TargetId : string.Empty, prompt.Target.RelativePath);
                foreach (Prompt pr in prompt.Target.RelatedPrompts)
                    sb.AppendFormat("{0}{1}={2}\n", label, pr.Language.HasTarget ? pr.Language.TargetId : string.Empty, pr.RelativePath);
            }
        }

        protected override string SerializeToText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("ASKDATE={0}\n", AskDate ? "1" : "0");
            sb.AppendFormat("ENQUEUE={0}\n", EnqueueCallbackRequest ? "1" : "0");
            sb.AppendFormat("ASKTIME={0}\n", AskTime ? "1" : "0");
            sb.AppendFormat("ASKNUMBER={0}\n", AlwaysAskPhone ? "1" : "0");

            SavePrompt(sb, Announce, "MSGANNOUNCE");
            SavePrompt(sb, AskPhone, "MSGREQUESTNR");
            SavePrompt(sb, CallAsap, "MSGCALLASAP");
            SavePrompt(sb, CallLater, "MSGCALLLATER");
            SavePrompt(sb, NotValid, "MSGNOTVALID");
            SavePrompt(sb, PromptAskDate, "MSGASKDATE");
            SavePrompt(sb, PromptAskTime, "MSGASKTIME");
            SavePrompt(sb, ReEnter, "MSGTOREENTER");
            SavePrompt(sb, Accept, "MSGTOACCEPT");
            SavePrompt(sb, RepeatDateTime, "MSGREPEAT");
            SavePrompt(sb, RepeatPhone, "MSGCURRENTNR");
            SavePrompt(sb, Sorry, "MSGSORRY");
            SavePrompt(sb, ThankYou, "MSGTHANKYOU");
            return sb.ToString();
        }

    }




}
