using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Nixxis.Client;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Nixxis.ClientV2
{
    

    public class PropertyChangeArg
    {
        private SupervisionItemDescription m_OldItem = null;
        private SupervisionItemDescription m_NewItem = null;
        private SupervisionItemTypes m_Type = SupervisionItemTypes.Undefined;
        private SupervisionKeys m_Key = SupervisionKeys.Undefined;

        public PropertyChangeArg(SupervisionItemDescription oldItem, SupervisionItemDescription newItem)
        {
            m_OldItem = oldItem;
            m_NewItem = newItem;
        }

        public SupervisionItemDescription OldItem
        {
            get { return m_OldItem; }
            set { m_OldItem = value; }
        }
        public SupervisionItemDescription NewItem
        {
            get { return m_NewItem; }
            set { m_NewItem = value; }
        }
        public SupervisionItemTypes Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        public SupervisionKeys Key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }
    }
    public enum AgentParametersArray
    {
        AgentId = 0,
        Account = 1,
        Firstname = 2,
        Lastname = 3,
        SiteId = 4,
        Server = 5,
        TeamId = 6,
        PauseId = 7,
        PauseDescription = 8,
        Groupkey = 9,
        Active = 10,
        LoginDateTime = 11,
        LoginDateTimeUtc = 12,
        IpAddress = 13,
        Extension = 14
    }
    public enum AgentRealtimeArray
    {
        StatusIndex = 0,
        Status = 1,
        StatusStartTime = 2,
        VoiceState = 3,
        VoiceAvailable = 4,
        ChatState = 5,
        ChatAvailable = 6,
        EmailState = 7,
        EmailAvailable = 8,
        ListCurrentContactId = 9,
        ActiveContactId = 10,
        PerQueWaiting = 11,
        PerQueAbandoned = 12,
        PerQueProcessed = 13,
        PerQueEnQueue = 14
    }

    public class SupervisionItemDescription
    {
        #region Class data
        private int m_Index;
        private string m_RawValue = string.Empty;
        private Type m_Type;
        private int[] m_PeakValues = new int[12];
        private int[] m_RollingSum = new int[60];
        #endregion

        #region Constructors
        public SupervisionItemDescription(Type type)
        {
            m_Type = type;
        }
        public SupervisionItemDescription(Type type, int index)
        {
            m_Type = type;
            m_Index = index;
        }
        private void InitObject()
        {
            for (int i = 0; i < m_PeakValues.Length; i++)
                m_PeakValues[i] = new int();

            for (int i = 0; i < m_RollingSum.Length; i++)
                m_RollingSum[i] = new int();
        }
        #endregion

        #region Properties
        public string RawValue
        {
            get { return m_RawValue; }
            internal set { m_RawValue = value; }
        }
        public Type Type
        {
            get { return m_Type; }
            internal set { m_Type = value; }
        }
        public int Index
        {
            get { return m_Index; }
            internal set { m_Index = value; }
        }
        public object Value
        {
            get
            {
                if (m_Type == typeof(bool))
                    return BaseSupervisionObjectList.BoolValue(m_RawValue);
                else if (m_Type == typeof(DateTime))
                    return BaseSupervisionObjectList.DateTimeValue(m_RawValue);
                else if (m_Type == typeof(double))
                    return BaseSupervisionObjectList.DoubleValue(m_RawValue);
                else if (m_Type == typeof(int))
                    return BaseSupervisionObjectList.IntValue(m_RawValue);
                else if (m_Type == typeof(string))
                    return BaseSupervisionObjectList.StringValue(m_RawValue);
                else if (m_Type == typeof(TimeSpan))
                    return BaseSupervisionObjectList.TimeSpanValue(m_RawValue);
                else
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("Type {0} is not support (SupervisionItemDescription.Value)"), m_Type.ToString());
                    return null;
                }
            }
        }
        public int[] PeakValues
        {
            get { return m_PeakValues; }
        }
        public int[] RollingSum
        {
            get { return m_RollingSum; }
            set { m_RollingSum = value;}
        }
        #endregion

        #region Members
        internal void AddPeakValue(int value)
        {
            for (int i = m_PeakValues.Length - 2; i > 0; i--)
            {
                m_PeakValues[i + 1] = m_PeakValues[i];
            }

            m_PeakValues[0] = value;
        }
        #endregion
    }

    public delegate void SupervisionObjectDelegate(SupervisionItemDescription newValue, SupervisionItemDescription oldValue);
    public class BaseSupervisionObjectList
    {
        #region Events
        public event SupervisionObjectDelegate SupervisionObjectPropertyChange;
        internal void OnSupervisionObjectPropertyChange(SupervisionItemDescription newValue, SupervisionItemDescription oldValue)
        {
            if (SupervisionObjectPropertyChange != null)
            {
                try
                {
                    SupervisionObjectPropertyChange(newValue, oldValue);
                }
                catch
                {
                }
            }
        }
        #endregion

        #region Enum
        protected enum SumTypes
        {
            None,
            Peak,
            Diff
        }
        #endregion

        #region Class data
        internal SortedList<int, SupervisionItemDescription> m_Items = new SortedList<int, SupervisionItemDescription>();
        internal string[] m_RawData;
        protected SumTypes m_SumType = SumTypes.None;
        #endregion

        #region Constructor
        public BaseSupervisionObjectList(Type classType, Type enumType)
        {
            if (classType.BaseType != typeof(BaseSupervisionObjectList)) return;
            if (enumType.BaseType != typeof(Enum)) return;

            StringCollection list = new StringCollection();
            list.AddRange(Enum.GetNames(enumType));

            foreach (PropertyInfo propertyInfo in classType.GetProperties())
            {
                if (list.Contains(propertyInfo.Name))
                {
                    m_Items.Add(Enum.Parse(enumType, propertyInfo.Name).GetHashCode(), new SupervisionItemDescription(propertyInfo.PropertyType, Enum.Parse(enumType, propertyInfo.Name).GetHashCode()));
                }
            }
        }
        #endregion

        #region Properties
        public virtual SupervisionItemDescription this[int i]
        {
            get 
            {
                if (m_Items.ContainsKey(i))
                    return m_Items[i];
                else
                    return null;
            }
        }
        #endregion

        #region Members
        #region Public
        public object GetValue(int index)
        {
            if(!m_Items.ContainsKey(index)) return null;

            return m_Items[index].Value;
        }
        public string GetRawValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                return m_RawData[index];
            }
            return string.Empty;
        }

        internal PropertyChangeArg[] SetRawData(string[] rawData)
        {
            List<PropertyChangeArg> properties = new List<PropertyChangeArg>();

            lock (this)
            {
                bool firstRun = false;
                if (m_RawData == null)
                {
                    firstRun = true;
                    m_RawData = new string[rawData.Length];
                    
                    for (int i = 0; i < m_RawData.Length; i++)
                        m_RawData[i] = "";
                }
               
                foreach(int i in m_Items.Keys)
                {
                    //Update rolling values
                    if (m_SumType == SumTypes.Peak)
                    {
                        if (m_Items[i].Type.Equals(typeof(int)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_Items[i].RollingSum[0] = 0;
                                }
                                else
                                {
                                    int Value = int.Parse(rawData[i]);

                                    if (Value > m_Items[i].RollingSum[0])
                                        m_Items[i].RollingSum[0] = Value;
                                }
                            }
                            catch { }
                        }
                        else if (m_Items[i].Type.Equals(typeof(TimeSpan)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_Items[i].RollingSum[0] = 0;
                                }
                                else
                                {
                                    int Value = int.Parse(rawData[i]);

                                    if (Value > m_Items[i].RollingSum[0])
                                        m_Items[i].RollingSum[0] = Value;
                                }

                            }
                            catch { }
                        }
                    }
                    else if (m_SumType == SumTypes.Diff)
                    {

                        if (m_Items[i].Type.Equals(typeof(int)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_Items[i].RollingSum[0] = 0;
                                }
                                else
                                {
                                    //TO BE CHECKED: first we do + of value and then we to - the same value?
                                    m_Items[i].RollingSum[0] = m_Items[i].RollingSum[0] + int.Parse(rawData[i]) - int.Parse(m_RawData[i]);
                                }
                            }
                            catch { }
                        }
                        else if (m_Items[i].Type.Equals(typeof(TimeSpan)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_Items[i].RollingSum[0] = 0;
                                }
                                else
                                {
                                    int NewValue = ((int)((TimeSpan)m_Items[i].Value).TotalMilliseconds);
                                    m_Items[i].RollingSum[0] = m_Items[i].RollingSum[0] + int.Parse(rawData[i]) - NewValue;

                                    int Value = m_Items[i].RollingSum[0];
                                    int counter = 0;

                                    while (Value > 0 || counter < 60)
                                    {
                                        if (counter > 0) Value += m_Items[i].RollingSum[counter];
                                        if (Value > 60000)
                                        {
                                            m_Items[i].RollingSum[counter] = 60000;
                                            Value -= 60000;
                                        }
                                        else
                                        {
                                            m_Items[i].RollingSum[counter] = Value;
                                            Value -= Value;
                                        }
                                        counter++;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    //Update value
                    string oldValue = m_Items[i].RawValue;
                    m_Items[i].RawValue = rawData[i];

                    if (m_Items[i].RawValue != oldValue)
                    {
                        SupervisionItemDescription oldItem = new SupervisionItemDescription(m_Items[i].Type, i);
                        oldItem.RawValue = oldValue;

                        properties.Add(new PropertyChangeArg(oldItem, m_Items[i]));
                        OnSupervisionObjectPropertyChange(m_Items[i], oldItem);
                    }
                }
                //Update values
                m_RawData = rawData;
            }
            return properties.ToArray();
        }       
        internal void SetPeakData(string[] peakData)
        {
            for(int i = 0; i < peakData.Length; i++)
            {
                if(m_Items.ContainsKey(i))
                {
                    int val;
                    if(int.TryParse(peakData[i], out val))
                    {
                        m_Items[i].AddPeakValue(val);
                    }
                }
            }

        }
        #endregion

        #region Static
        public static DateTime DateTimeValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    int MSecs;
                    string[] Parts = value.Split('|');

                    if (Parts.Length >= 6)
                        return new DateTime(int.Parse(Parts[0]), int.Parse(Parts[1]), int.Parse(Parts[2]), int.Parse(Parts[3]), int.Parse(Parts[4]), int.Parse(Parts[5]));

                    if (int.TryParse(value, out MSecs))
                        return new DateTime(MSecs * 10000);
                }
                catch
                {
                }

                DateTime Dte;

                if (DateTime.TryParse(value, out Dte))
                    return Dte.ToLocalTime();

                try
                {
                    string[] List = value.Split(' ');
                    if (List.Length >= 2)
                    {
                        string[] lstDate = List[0].Split('/');
                        string[] lstTime = List[1].Split(':');
                        string _TimeRange = "";

                        if (List.Length > 2) _TimeRange = List[2];

                        if (lstDate.Length >= 3 && lstTime.Length >= 3)
                        {
                            int hour = int.Parse(lstTime[0]);
                            if (_TimeRange.ToUpper() == "PM")
                            {
                                if (hour < 12)
                                    hour += 12;
                            }

                            Dte = new DateTime(int.Parse(lstDate[2]),
                                int.Parse(lstDate[0]),
                                int.Parse(lstDate[1]),
                                hour,
                                int.Parse(lstTime[1]),
                                int.Parse(lstTime[2]));

                            return Dte.ToLocalTime(); ;
                        };
                    }
                }
                catch
                {
                }

            }
            return DateTime.MinValue;
        }
        public static TimeSpan TimeSpanValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int MSecs;

                if (int.TryParse(value, out MSecs))
                    return new TimeSpan(0, 0, 0, 0, MSecs);
            }

            return TimeSpan.Zero;
        }
        public static int IntValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int Value;

                if (int.TryParse(value, out Value))
                    return Value;
            }

            return 0;
        }
        public static Double DoubleValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Double Value;

                if (Double.TryParse(value, out Value))
                {
                    if (Value == 0)
                        return Value;
                    else
                        return Value / 100000;
                }
            }

            return 0;
        }
        public static string StringValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return "";
        }
        public static bool BoolValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                bool Value;

                if (bool.TryParse(value, out Value))
                    return Value;
            }

            return false;
        }
        #endregion
        #endregion
    }
    public class AgentParametersV2 : BaseSupervisionObjectList
    {
        public AgentParametersV2()
            : base(typeof(AgentParametersV2), typeof(AgentParametersArray))
        {

        }

        public string AgentId { get { return StringValue(GetRawValue((int)AgentParametersArray.AgentId)); } }
        public string Account { get { return StringValue(GetRawValue((int)AgentParametersArray.Account)); } }
        public string Firstname { get { return StringValue(GetRawValue((int)AgentParametersArray.Firstname)); } }
        public string Lastname { get { return StringValue(GetRawValue((int)AgentParametersArray.Lastname)); } }
        public string SiteId { get { return StringValue(GetRawValue((int)AgentParametersArray.SiteId)); } }
        public string Server { get { return StringValue(GetRawValue((int)AgentParametersArray.Server)); } }
        public string TeamId { get { return StringValue(GetRawValue((int)AgentParametersArray.TeamId)); } }
        public string PauseId { get { return StringValue(GetRawValue((int)AgentParametersArray.PauseId)); } }
        public string PauseDescription { get { return StringValue(GetRawValue((int)AgentParametersArray.PauseDescription)); } }
        public string Groupkey { get { return StringValue(GetRawValue((int)AgentParametersArray.Groupkey)); } }
        public int Active { get { return IntValue(GetRawValue((int)AgentParametersArray.Active)); } }
        public DateTime LoginDateTime { get { return DateTimeValue(GetRawValue((int)AgentParametersArray.LoginDateTime)); } }
        public int LoginDateTimeUtc { get { return IntValue(GetRawValue((int)AgentParametersArray.LoginDateTimeUtc)); } }
        public string IpAddress { get { return StringValue(GetRawValue((int)AgentParametersArray.IpAddress)); } }
        public string Extension { get { return StringValue(GetRawValue((int)AgentParametersArray.Extension)); } }
    }
    public class AgentRealtimeV2 : BaseSupervisionObjectList
    {
        public AgentRealtimeV2()
            : base(typeof(AgentRealtimeV2), typeof(AgentRealtimeArray))
        {
            m_SumType = SumTypes.Peak;
        }

        public int StatusIndex { get { return IntValue(GetRawValue((int)AgentRealtimeArray.StatusIndex)); } }
        public string Status { get { return StringValue(GetRawValue((int)AgentRealtimeArray.Status)); } }
        public DateTime StatusStartTime { get { return DateTimeValue(GetRawValue((int)AgentRealtimeArray.StatusStartTime)); } }
        public string VoiceState { get { return StringValue(GetRawValue((int)AgentRealtimeArray.VoiceState)); } }
        public bool VoiceAvailable { get { return BoolValue(GetRawValue((int)AgentRealtimeArray.VoiceAvailable)); } }
        public string ChatState { get { return StringValue(GetRawValue((int)AgentRealtimeArray.ChatState)); } }
        public bool ChatAvailable { get { return BoolValue(GetRawValue((int)AgentRealtimeArray.ChatAvailable)); } }
        public string EmailState { get { return StringValue(GetRawValue((int)AgentRealtimeArray.EmailState)); } }
        public bool EmailAvailable { get { return BoolValue(GetRawValue((int)AgentRealtimeArray.EmailAvailable)); } }
        public int ListCurrentContactId { get { return IntValue(GetRawValue((int)AgentRealtimeArray.ListCurrentContactId)); } }
        public int ActiveContactId { get { return IntValue(GetRawValue((int)AgentRealtimeArray.ActiveContactId)); } }
        public int PerQueWaiting { get { return IntValue(GetRawValue((int)AgentRealtimeArray.PerQueWaiting)); } }
        public int PerQueAbandoned { get { return IntValue(GetRawValue((int)AgentRealtimeArray.PerQueAbandoned)); } }
        public int PerQueProcessed { get { return IntValue(GetRawValue((int)AgentRealtimeArray.PerQueProcessed)); } }
        public int PerQueEnQueue { get { return IntValue(GetRawValue((int)AgentRealtimeArray.PerQueEnQueue)); } }
    }


    
    public abstract class BaseItemDataV2
    {   
        #region Class data
        internal string m_Key;
        internal BaseSupervisionObjectList m_Parameters = null;
        internal BaseSupervisionObjectList m_RealTime = null;
        internal BaseSupervisionObjectList m_History = null;
        internal BaseSupervisionObjectList m_Production = null;
        internal BaseSupervisionObjectList m_PeriodProduction = null;
        internal BaseSupervisionObjectList m_ContactListInfo = null;
        #endregion

        #region Constructor
        public BaseItemDataV2()
        {
            m_Key = string.Empty;
        }
        public BaseItemDataV2(string key)
        {
            m_Key = key;
        }
        #endregion

        #region Properties
        public BaseSupervisionObjectList this[int i]
        {
            get
            {
                if (i == (int)SupervisionKeys.Parameters)
                    return m_Parameters;
                else if (i == (int)SupervisionKeys.RealTime)
                    return m_RealTime;
                else
                    return null;
            }
        }
        public string Key { get { return m_Key; } }
        #endregion

        #region Members
        internal virtual PropertyChangeArg[] UpdateData(SupervisionKeys key, string[] rawData)
        {
            if (key == SupervisionKeys.Parameters && m_Parameters != null)
                return m_Parameters.SetRawData(rawData);
            else if (key == SupervisionKeys.RealTime && m_RealTime != null)
                return m_RealTime.SetRawData(rawData);
            else if (key == SupervisionKeys.History && m_History != null)
                return m_History.SetRawData(rawData);
            else if (key == SupervisionKeys.Production && m_Production != null)
                return m_Production.SetRawData(rawData);
            else if (key == SupervisionKeys.PeriodProduction && m_PeriodProduction != null)
                return m_PeriodProduction.SetRawData(rawData);
            else if (key == SupervisionKeys.ContactListInfo && m_ContactListInfo != null)
                return m_ContactListInfo.SetRawData(rawData);
            else
                return null;

        }
        #endregion
    }
    public class AgentDataV2 : BaseItemDataV2
    {
        #region Constructors
        public AgentDataV2()
        {
            Init();
        }
        public AgentDataV2(string key)
            : base(key)
        {
            Init();
        }
        private void Init()
        {
            m_Parameters = new AgentParametersV2();
            m_RealTime = new AgentRealtimeV2();
        }
        #endregion

        #region Properties

        public BaseSupervisionObjectList this[SupervisionKeys key]
        {
            get
            {
                return this[(int)key];
            }
        }
        public AgentParametersV2 Parameters { get { return (AgentParametersV2)m_Parameters; } }
        public AgentRealtimeV2 RealTime { get { return (AgentRealtimeV2)m_RealTime; } }
        #endregion

        #region Members
        #region Overrides

        #endregion
        #endregion


   }

    public delegate void SupervisionPropertyChange(PropertyChangeArg info);
    public class BaseItemCollectionV2<TItem> : KeyedCollection<string, TItem> where TItem : BaseItemDataV2, new()
    {
        #region Events
        public event SupervisionPropertyChange DataPropertyChange;
        internal void OnDataPropertyChange(PropertyChangeArg info)
        {
            if (DataPropertyChange != null)
            {
                try
                {
                    DataPropertyChange(info);
                }
                catch
                {
                }
            }
        }
        #endregion

        #region Member
        #region Overrides
        protected override string GetKeyForItem(TItem item)
        {
            return item.m_Key;
        }
        #endregion

        #region Internal
        internal TItem GetOrAdd(string key)
        {
            if (this.Contains(key))
                return this[key];

            TItem NewItem = new TItem();
            NewItem.m_Key = key;

            this.Add(NewItem);

            return NewItem;
        }
        #endregion

        #region Public
        public TItem Get(string key)
        {
            if (this.Contains(key))
                return this[key];

            return null;
        }
        #endregion
        #endregion

    }
    public delegate void AgentDataChangedDelegateV2(AgentDataV2 agent);
    public delegate void AgentDataRemoveDelegateV2(string agentId);
    public class AgentCollectionV2 : BaseItemCollectionV2<AgentDataV2>
    {
        public event AgentDataChangedDelegateV2 AgentDataChanged;
        public event AgentDataRemoveDelegateV2 AgentDataRemove;

        internal void OnAgentDataChanged(AgentDataV2 agent)
        {
            if (AgentDataChanged != null)
            {
                try
                {
                    AgentDataChanged(agent);
                }
                catch
                {
                }
            }
        }

        internal void OnAgentDataRemove(string agentId)
        {
            if (AgentDataRemove != null)
            {
                try
                {
                    AgentDataRemove(agentId);
                }
                catch
                {
                }
            }
        }

    }
    
    
  
    
    
    
    
    public class BaseSupervisionObject<TItem> : SupervisionObjectReferenceManager where TItem : class, new()
    {
        protected enum SumTypes
        {
            None,
            Peak,
            Diff
        }

        private PropertyInfo[] m_Properties;
        protected SumTypes m_SumType = SumTypes.None;

        protected void SetDataValue(int index, string value)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                m_RawData[index] = value;
            }
        }

        protected DateTime DateTimeValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                try
                {
                    int MSecs;
                    string[] Parts = m_RawData[index].Split('|');

                    if (Parts.Length >= 6)
                        return new DateTime(int.Parse(Parts[0]), int.Parse(Parts[1]), int.Parse(Parts[2]), int.Parse(Parts[3]), int.Parse(Parts[4]), int.Parse(Parts[5]));

                    if (int.TryParse(m_RawData[index], out MSecs))
                        return new DateTime(MSecs * 10000);
                }
                catch
                {
                }

                DateTime Dte;

                if (DateTime.TryParse(m_RawData[index], out Dte))
                    return Dte.ToLocalTime();

                try
                {
                    string[] List = m_RawData[index].Split(' ');
                    if (List.Length >= 2)
                    {
                        string[] lstDate = List[0].Split('/');
                        string[] lstTime = List[1].Split(':');
                        string _TimeRange = "";

                        if (List.Length > 2) _TimeRange = List[2];

                        if (lstDate.Length >= 3 && lstTime.Length >= 3)
                        {
                            int hour = int.Parse(lstTime[0]);
                            if (_TimeRange.ToUpper() == "PM")
                            {
                                if (hour < 12)
                                    hour += 12;
                            }

                            Dte = new DateTime(int.Parse(lstDate[2]),
                                int.Parse(lstDate[0]),
                                int.Parse(lstDate[1]),
                                hour,
                                int.Parse(lstTime[1]),
                                int.Parse(lstTime[2]));

                            return Dte.ToLocalTime(); ;
                        };
                    }
                }
                catch
                {
                }
            }

            return DateTime.MinValue;
        }
        protected TimeSpan TimeSpanValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                int MSecs;

                if (int.TryParse(m_RawData[index], out MSecs))
                    return new TimeSpan(0, 0, 0, 0, MSecs);
            }

            return TimeSpan.Zero;
        }
        protected int IntValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                int Value;

                if (int.TryParse(m_RawData[index], out Value))
                    return Value;
            }

            return 0;
        }
        protected Double DoubleValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                Double Value;

                if (Double.TryParse(m_RawData[index], out Value))
                {
                    if (Value == 0)
                        return Value;
                    else
                        return Value / 100000;
                }
            }

            return 0;
        }
        protected string StringValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                return m_RawData[index];
            }

            return "";
        }
        protected bool BoolValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                bool Value;

                if (bool.TryParse(m_RawData[index], out Value))
                    return Value;
            }

            return false;
        }

        private string[] m_RawData;
        private int[][] m_RollingSum = new int[60][];

        internal string[] RawData
        {
            get { return m_RawData; }
        }
        internal void SetRawData(string[] rawData)
        {
            lock (this)
            {
                bool firstRun = false;
                if (m_RawData == null)
                {
                    firstRun = true;
                    m_RawData = new string[rawData.Length];
                    for (int i = 0; i < m_RawData.Length; i++)
                        m_RawData[i] = "";
                }

                if (m_RollingSum[0] == null || rawData.Length > m_RollingSum[0].Length)
                {
                    for (int i = 0; i < 60; i++)
                        m_RollingSum[i] = new int[rawData.Length];
                }

                if (m_Properties == null)
                {
                    m_Properties = GetType().GetProperties();
                }

                if (typeof(TItem) == typeof(InboundHistory))
                {
                    int ii = 0;
                    ii++;
                }

                if (m_SumType == SumTypes.Peak)
                {
                    for (int i = 0; i < m_Properties.Length && i < m_RollingSum[0].Length; i++)
                    {
                        if (m_Properties[i].PropertyType.Equals(typeof(int)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_RollingSum[0][i] = 0;
                                }
                                else
                                {
                                    int Value = int.Parse(rawData[i]);

                                    if (Value > m_RollingSum[0][i])
                                        m_RollingSum[0][i] = Value;
                                }
                            }
                            catch { }
                        }
                        else if (m_Properties[i].PropertyType.Equals(typeof(TimeSpan)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_RollingSum[0][i] = 0;
                                }
                                else
                                {
                                    int Value = int.Parse(rawData[i]); 

                                    if (Value > m_RollingSum[0][i])
                                        m_RollingSum[0][i] = Value;
                                }

                            }
                            catch { }
                        }
                    }
                }
                else if (m_SumType == SumTypes.Diff)
                {
                    for (int i = 0; i < m_Properties.Length && i < m_RollingSum[0].Length; i++)
                    {
                        if (m_Properties[i].PropertyType.Equals(typeof(int)))
                        {
                            try
                            {
                                if (firstRun)
                                {
                                    m_RollingSum[0][i] = 0;
                                }
                                else
                                {
                                    m_RollingSum[0][i] = m_RollingSum[0][i] + int.Parse(rawData[i]) - int.Parse(m_RawData[i]);
                                } 
                            }
                            catch { }
                        }
                        else if (m_Properties[i].PropertyType.Equals(typeof(TimeSpan)))
                        {
                            try
                            {
                                
                                if (firstRun)
                                { m_RollingSum[0][i] = 0; }
                                else
                                {
                                    int NewValue = ((int)((TimeSpan)m_Properties[i].GetValue(this, null)).TotalMilliseconds);
                                    m_RollingSum[0][i] = m_RollingSum[0][i] + int.Parse(rawData[i]) - NewValue;

                                    int Value = m_RollingSum[0][i];
                                    int counter = 0;

                                    while (Value > 0 || counter < 60)
                                    {
                                        if (counter > 0) Value += m_RollingSum[counter][i];
                                        if (Value > 60000)
                                        {
                                            m_RollingSum[counter][i] = 60000;
                                            Value -= 60000;
                                        }
                                        else
                                        {
                                            m_RollingSum[counter][i] = Value;
                                            Value -= Value;
                                        }
                                        counter++;
                                    }
                                }
                                

                            }
                            catch { }
                        }
                    }

                }

                m_RawData = rawData;
            }
        }

        public TItem GetTotal(int minutes)
        {
            lock (this)
            {
                BaseSupervisionObject<TItem> Item = new TItem() as BaseSupervisionObject<TItem>;
                int[] m_Totals;

                if (m_RollingSum == null || m_RollingSum.Length == 0 || m_RollingSum[0] == null || m_RollingSum[0].Length == 0 || m_RawData == null)
                {
                    return null;
                }
                else
                {
                    m_Totals = new int[m_RollingSum[0].Length];
                }

                if (m_SumType == SumTypes.Peak)
                {
                    for (int i = 0; i < m_RollingSum[0].Length; i++)
                    {
                        for (int j = 0; j < minutes; j++)
                        {
                            if (m_RollingSum[j][i] > m_Totals[i])
                                m_Totals[i] = m_RollingSum[j][i];
                        }
                    }
                }
                else if (m_SumType == SumTypes.Diff)
                {
                    for (int i = 0; i < m_RollingSum[0].Length; i++)
                    {
                        for (int j = 0; j < minutes; j++)
                        {
                            m_Totals[i] += m_RollingSum[j][i];
                        }
                    }
                }

                string[] RawData = new string[m_Totals.Length];

                for (int i = 0; i < m_Totals.Length; i++)
                    RawData[i] = m_Totals[i].ToString();

                Item.SetRawData(RawData);

                return Item as TItem;
            }
        }
        public TItem AddTo(TItem data)
        {
            lock (this)
            {
                BaseSupervisionObject<TItem> Item = new TItem() as BaseSupervisionObject<TItem>;
                int[] m_Totals = new int[m_RawData.Length];

                if (m_Properties == null)
                {
                    m_Properties = GetType().GetProperties();
                }

                for (int i = 0; i < m_Properties.Length && i < m_RawData.Length; i++)
                {
                    if (m_Properties[i].PropertyType.Equals(typeof(int)))
                    {
                        try { m_Totals[i] += int.Parse(m_RawData[i]); }
                        catch { }
                    }
                }

                string[] RawData = new string[m_Totals.Length];

                for (int i = 0; i < m_Totals.Length; i++)
                    RawData[i] = m_Totals[i].ToString();

                Item.SetRawData(RawData);

                return Item as TItem;
            }
        }
        protected override void UpdateTimers()
        {
            lock (this)
            {
                Array.ConstrainedCopy(m_RollingSum, 0, m_RollingSum, 1, m_RollingSum.Length - 2);
                m_RollingSum[0] = new int[m_RollingSum[1].Length];

                if (m_SumType == SumTypes.Diff)
                {
                    for (int i = 0; i < m_RollingSum[0].Length; i++)
                        m_RollingSum[0][i] = 0;
                }
            }
        }
    }
    public class AgentParameters : BaseSupervisionObject<AgentParameters>
    {
        public string AgentId { get { return StringValue((int)AgentParametersArray.AgentId); } }
        public string Account { get { return StringValue((int)AgentParametersArray.Account); } }
        public string Firstname { get { return StringValue((int)AgentParametersArray.Firstname); } }
        public string Lastname { get { return StringValue((int)AgentParametersArray.Lastname); } }
        public string SiteId { get { return StringValue((int)AgentParametersArray.SiteId); } }
        public string Server { get { return StringValue((int)AgentParametersArray.Server); } }
        public string TeamId { get { return StringValue((int)AgentParametersArray.TeamId); } }
        public string PauseId { get { return StringValue((int)AgentParametersArray.PauseId); } }
        public string PauseDescription { get { return StringValue((int)AgentParametersArray.PauseDescription); } }
        public string Groupkey { get { return StringValue((int)AgentParametersArray.Groupkey); } }
        public int Active { get { return IntValue((int)AgentParametersArray.Active); } }
        public DateTime LoginDateTime { get { return DateTimeValue((int)AgentParametersArray.LoginDateTime); } }
        public int LoginDateTimeUtc { get { return IntValue((int)AgentParametersArray.LoginDateTimeUtc); } }
        public string IpAddress { get { return StringValue((int)AgentParametersArray.IpAddress); } }
        public string Extension { get { return StringValue((int)AgentParametersArray.Extension); } }
    }


    public class AgentData : BaseItemData
    {
        private AgentParameters m_Parameters = new AgentParameters();
        private AgentRealtime m_RealTime = new AgentRealtime();
        private AgentHistory m_History = new AgentHistory();
        private AgentProduction m_Production = new AgentProduction();
        private AgentPeriodProduction m_PeriodProduction = new AgentPeriodProduction();

        #region Constructors
        public AgentData()
        {
            Init();
        }

        public AgentData(string key)
            : base(key)
        {
            Init();
        }
        private void Init()
        {
            
        }
        #endregion

        #region Members
        #region Overrides
         
        #endregion
        #endregion
        public AgentParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public AgentRealtime RealTime
        {
            get
            {
                return m_RealTime;
            }
        }

        public AgentHistory History
        {
            get
            {
                return m_History;
            }
        }

        public AgentProduction Production
        {
            get
            {
                return m_Production;
            }
        }

        public AgentPeriodProduction PeriodProduction
        {
            get
            {
                return m_PeriodProduction;
            }
        }

    }
    public class InboundData : BaseItemData
    {
        private AgentParameters m_Parameters = new AgentParameters();
        private AgentRealtime m_RealTime = new AgentRealtime();
        private AgentHistory m_History = new AgentHistory();
        private AgentProduction m_Production = new AgentProduction();
        private AgentPeriodProduction m_PeriodProduction = new AgentPeriodProduction();

        #region Constructors
        public InboundData()
        {
            Init();
        }

        public InboundData(string key)
            : base(key)
        {
            Init();
        }
        private void Init()
        {

        }
        #endregion

        #region Members
        #region Overrides

        #endregion
        #endregion
        public AgentParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public AgentRealtime RealTime
        {
            get
            {
                return m_RealTime;
            }
        }

        public AgentHistory History
        {
            get
            {
                return m_History;
            }
        }

        public AgentProduction Production
        {
            get
            {
                return m_Production;
            }
        }

        public AgentPeriodProduction PeriodProduction
        {
            get
            {
                return m_PeriodProduction;
            }
        }

    }
      
    public abstract class BaseItemData
    {
        internal string m_Key;

        public BaseItemData()
        {
            m_Key = string.Empty;
        }

        public BaseItemData(string key)
        {
            m_Key = key;
        }

        public string Key
        {
            get
            {
                return m_Key;
            }
        }
    }
    public class BaseItemCollection<TItem> : KeyedCollection<string, TItem> where TItem : BaseItemData, new()
    {
        protected override string GetKeyForItem(TItem item)
        {
            return item.m_Key;
        }

        internal TItem GetOrAdd(string key)
        {
            if (this.Contains(key))
                return this[key];

            TItem NewItem = new TItem();
            NewItem.m_Key = key;

            this.Add(NewItem);

            return NewItem;
        }

        public TItem Get(string key)
        {
            if (this.Contains(key))
                return this[key];

            return null;
        }

    }

    public delegate void AgentDataChangedDelegate(AgentData agent);
    public delegate void AgentDataRemoveDelegate(string agentId);
    public class AgentCollection : BaseItemCollection<AgentData>
    {
        public event AgentDataChangedDelegate AgentDataChanged;
        public event AgentDataRemoveDelegate AgentDataRemove;

        internal void OnAgentDataChanged(AgentData agent)
        {
            if (AgentDataChanged != null)
            {
                try
                {
                    AgentDataChanged(agent);
                }
                catch
                {
                }
            }
        }
        internal void OnAgentDataRemove(string agentId)
        {
            if (AgentDataRemove != null)
            {
                try
                {
                    AgentDataRemove(agentId);
                }
                catch
                {
                }
            }
        }
    }
    
    public delegate void InboundDataChangedDelegate(InboundData inbound);
    public class InboundCollection : BaseItemCollection<InboundData>
    {
        public event InboundDataChangedDelegate InboundDataChanged;

        internal void OnInboundDataChanged(InboundData inbound)
        {
            if (InboundDataChanged != null)
            {
                try
                {
                    InboundDataChanged(inbound);
                }
                catch
                {
                }
            }
        }

        public InboundData GetTotal()
        {
            InboundData m_Total = new InboundData();

            return m_Total;
        }
    }

    public class ClientSupervisionV2
    {
        #region Class data
        private WeakReference m_LinkReference;

        private AgentCollectionV2 m_Agents = new AgentCollectionV2();
        //private InboundCollection m_Inbounds = new InboundCollection();
        #endregion

        #region Constructor
        public ClientSupervisionV2(HttpLink link)
        {
            m_LinkReference = new WeakReference(link, false);

            link.ServerEvent += new HttpLinkServerEventDelegate(Link_ServerEvent);
        }

        #endregion

        #region Properties 
        public object this[int i]
        {
            get
            {
                if (i == (int)SupervisionItemTypes.Agent)
                    return m_Agents;
                else
                    return null;
            }
        }
        public AgentCollectionV2 Agents
        {
            get { return m_Agents; }
        }
        #endregion

        #region Members
        public SupervisionItemDescription GetValue(int[] item, string itemId)
        {
            if (item.Length != 3) return null;
            if (string.IsNullOrEmpty(itemId)) return null;
            if (itemId.Length != 32) return null;

            if (item[0] == 0)
            {
                try
                {
                    return m_Agents[itemId][item[1]][item[2]];
                }
                catch { return null; }
            }

            return null;
        }


        private bool CheckItem(string itemName, out SupervisionItemTypes itemType, out string itemId)
        {
            itemType = SupervisionItemTypes.Undefined;
            itemId = string.Empty;

            string[] Parts = itemName.Split(new char[] { '_' }, 2);
            if (Parts.Length == 2)
            {
                try
                {
                    itemType = (SupervisionItemTypes)Enum.Parse(typeof(SupervisionItemTypes), Parts[0], true);
                    itemId = Parts[1];

                    return true;
                }
                catch
                {

                }
            }

            return false;
        }

        private void Link_ServerEvent(ServerEventArgs eventArgs)
        {
            int SepPos = eventArgs.Parameters.IndexOf(',');
            System.Diagnostics.Trace.WriteLine(string.Format("Info: rx. Type: {0}, parameter {1} ", eventArgs.EventCode.ToString(), eventArgs.Parameters.ToString()));

            if (eventArgs.EventCode == EventCodes.AgentWarning)
            {
                #region AgentWarning
                string msg = eventArgs.Parameters.Substring(SepPos + 1);
                
                System.Diagnostics.Trace.WriteLine("Info msg: " + msg);
                
                #endregion
            }

            if (SepPos > 0)
            {
                SupervisionItemTypes ItemType;
                string ItemId;

                if (CheckItem(eventArgs.Parameters.Substring(0, SepPos), out ItemType, out ItemId))
                {
                    if (eventArgs.EventCode == EventCodes.SupervisionItem)
                    {
                        #region SupervisionItem
                        string[] RawData = eventArgs.Parameters.Substring(SepPos + 1).Split(',');

                        switch (ItemType)
                        {
                            case SupervisionItemTypes.Agent:
                                AgentDataV2 Agent = m_Agents.GetOrAdd(ItemId);
                                PropertyChangeArg[] list = Agent.UpdateData(SupervisionKeys.Parameters, RawData);
                                if (list != null)
                                {
                                    foreach (PropertyChangeArg item in list)
                                        m_Agents.OnDataPropertyChange(item);
                                }
                                break;
                        }
                        #endregion
                    }
                    else if (eventArgs.EventCode == EventCodes.SupervisionData)
                    {
                        #region SupervisionData
                        int NextSep = eventArgs.Parameters.IndexOf(',', SepPos + 1);

                        if (NextSep > SepPos)
                        {
                            string DataType = eventArgs.Parameters.Substring(SepPos + 1, NextSep - (SepPos + 1));
                            string[] RawData = eventArgs.Parameters.Substring(NextSep + 1).Split(',');

                            switch (ItemType)
                            {
                                case SupervisionItemTypes.Agent:
                                    AgentDataV2 Agent = m_Agents.Get(ItemId);
                                    PropertyChangeArg[] list = null;

                                    if (Agent != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            list = Agent.UpdateData(SupervisionKeys.RealTime, RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                            list = Agent.UpdateData(SupervisionKeys.History, RawData);
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            list = Agent.UpdateData(SupervisionKeys.Production, RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            list = Agent.UpdateData(SupervisionKeys.PeriodProduction, RawData);
                                        //TO DO: Only trigger this when really something has changed
                                        m_Agents.OnAgentDataChanged(Agent);

                                        if (list != null)
                                        {
                                            foreach (PropertyChangeArg item in list)
                                                m_Agents.OnDataPropertyChange(item);
                                        }
                                    }
                                    break;
                                
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Wrn: Can't handle eventcode");
                    }
                }
            }
        }
        #endregion
    }

    public class SupervisionModel
    {

        private AgentCollection m_Agents = new AgentCollection();

        public SupervisionModel()
        {
        }


        public AgentCollection Agents
        {
            get { return m_Agents; }
        }
    }
}
