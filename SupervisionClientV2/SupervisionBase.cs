using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Specialized;
using System.Globalization;


namespace Nixxis.ClientV2
{
	public static class SupervisionSettings
	{
		private static CultureInfo m_CultureInfo;
		private static NumberFormatInfo m_NumberFormat;
        public static DateTimeFormatInfo DateTimeFormat;

		public static readonly DateTime ProtocolDateTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public static readonly TimeSpan MaxRawTimeSpan = DateTime.Now.AddYears(-10).Subtract(ProtocolDateTimeStart);
		public static long ServerTimeOffset = 0;
		public static bool? UseOldNumbersEncoding = null;

		public static CultureInfo CultureInfo
		{
			get
			{
				return m_CultureInfo;
			}
		}

		public static IFormatProvider NumberFormatProvider
		{
			get
			{
				return (IFormatProvider)m_NumberFormat;
			}
		}

		public static int ParseInt(string rawValue)
		{
			int Value = 0;

			if (int.TryParse(rawValue, out Value) && UseOldNumbersEncoding == null && Value > 0)
				UseOldNumbersEncoding = (Value > 100000);

			return (UseOldNumbersEncoding.GetValueOrDefault()) ? (Value / 100000) : Value;
		}

		public static long ParseLong(string rawValue)
		{
			long Value = 0;

			if (long.TryParse(rawValue, out Value) && UseOldNumbersEncoding == null && Value > 0)
				UseOldNumbersEncoding = (Value > 100000);

			return (UseOldNumbersEncoding.GetValueOrDefault()) ? (Value / 100000) : Value;
		}

		public static float ParseFloat(string rawValue)
		{
			float Value = 0;

			if (float.TryParse(rawValue, NumberStyles.Float, NumberFormatProvider, out Value) && UseOldNumbersEncoding == null && Value > 0)
				UseOldNumbersEncoding = (Value > 100000);

			return (UseOldNumbersEncoding.GetValueOrDefault()) ? (Value / 100000) : Value;
		}

		public static double ParseDouble(string rawValue)
		{
			double Value = 0;

			if (double.TryParse(rawValue, NumberStyles.Float, NumberFormatProvider, out Value) && UseOldNumbersEncoding == null && Value > 0)
				UseOldNumbersEncoding = (Value > 100000);

			return (UseOldNumbersEncoding.GetValueOrDefault()) ? (Value / 100000) : Value;
		}

		public static bool ParseBool(string rawValue)
		{
			bool Value = false;

			if (!string.IsNullOrEmpty(rawValue))
			{
				if (!bool.TryParse(rawValue, out Value))
				{
				}
			}

			return Value;
		}

		public static DateTime ParseDateTime(string rawValue, long ticksMult)
		{

            System.Diagnostics.Trace.WriteLine(string.Format("{0}", rawValue), "ParseDateTime");

			DateTime RawDateTime = DateTime.MinValue;

			if (!string.IsNullOrEmpty(rawValue))
			{
                if (DateTimeFormat == null)   // Try to find out how the server formats DateTime strings.  TODO: Know in advance or don't use strings !!!
                {
                    if (DateTime.TryParse(rawValue, m_CultureInfo.DateTimeFormat, DateTimeStyles.AssumeLocal, out RawDateTime))
                    {
                        DateTimeFormat = m_CultureInfo.DateTimeFormat;
                    }
                    else if (DateTime.TryParse(rawValue, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
                    }
                    else if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeLocal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
                    }
                    else if (DateTime.TryParse(rawValue, new CultureInfo("en").DateTimeFormat, DateTimeStyles.AssumeLocal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
                    }
                }

                if (!DateTime.TryParse(rawValue, DateTimeFormat ?? m_CultureInfo.DateTimeFormat, DateTimeStyles.AssumeLocal, out RawDateTime) && RawDateTime.Year <= 1)
				{
					long RawInt = 0;

					if (long.TryParse(rawValue, out RawInt))
					{
                        if (RawInt >= 315537897599)
                            RawDateTime = DateTime.MaxValue;
                        else
                            RawDateTime = (new DateTime(RawInt * ticksMult));//.AddTicks(ServerTimeOffset);    
					}
				}
			}

            System.Diagnostics.Trace.WriteLine(string.Format("{0}", RawDateTime), "ParseDateTime");

			return RawDateTime;
		}

        public static DateTime ParseDateTimeUtc(string rawValue, long ticksMult)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("{0}", rawValue), "ParseDateTimeUtc");

            DateTime RawDateTime = DateTime.MinValue;

            if (!string.IsNullOrEmpty(rawValue))
            {
                if (DateTimeFormat == null)   // Try to find out how the server formats DateTime strings.  TODO: Know in advance or don't use strings !!!
                {
                    if (DateTime.TryParse(rawValue, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
                    }
                    else if (DateTime.TryParse(rawValue, m_CultureInfo.DateTimeFormat, DateTimeStyles.AssumeUniversal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = m_CultureInfo.DateTimeFormat;
                    }
                    else if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
                    }
                    else if (DateTime.TryParse(rawValue, new CultureInfo("en").DateTimeFormat, DateTimeStyles.AssumeUniversal, out RawDateTime) && RawDateTime.Year > 1)
                    {
                        DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
                    }
                }

                if (!DateTime.TryParse(rawValue, DateTimeFormat ?? m_CultureInfo.DateTimeFormat, DateTimeStyles.AssumeUniversal, out RawDateTime) && RawDateTime.Year <= 1)
                {
                    long RawInt = 0;

                    if (long.TryParse(rawValue, out RawInt))
                    {
                        if (RawInt >= 315537897599)
                            RawDateTime = DateTime.MaxValue;
                        else
                            RawDateTime = (new DateTime(RawInt * ticksMult, DateTimeKind.Utc).ToLocalTime());//.AddTicks(ServerTimeOffset);
                    }
                }
            }
            System.Diagnostics.Trace.WriteLine(string.Format("{0}", RawDateTime), "ParseDateTimeUtc");

            return RawDateTime;
        }

		public static TimeSpan ParseTimeSpan(string rawValue, long ticksMult)
		{
			TimeSpan RawTimeSpan = TimeSpan.Zero;
			long RawInt = 0;

			if (!string.IsNullOrEmpty(rawValue))
			{
				if (long.TryParse(rawValue, out RawInt))
				{
					RawTimeSpan = new TimeSpan(RawInt * ticksMult);
				}
				else
				{
					if (!TimeSpan.TryParse(rawValue, out RawTimeSpan))
						RawTimeSpan = TimeSpan.Zero;
				}

			}

			return RawTimeSpan;
		}

		static SupervisionSettings()
		{
			m_CultureInfo = new CultureInfo("");
			
			m_NumberFormat = new NumberFormatInfo();
			m_NumberFormat.NumberGroupSeparator = "";
			m_NumberFormat.CurrencyGroupSeparator = "";

            DateTimeFormat = null;
		}
	}

	internal class RollingValue<T>
	{
		public T Current;
		public T Minimum;
		public T Maximum;
		public T Sum;
		public int Samples;
	}

	public enum TimeUnit
	{
		Seconds,
		MilliSeconds,
		Tenths,
		Ticks
	}

    [Flags] public enum ProductVersion
    {
        Express = 1,
        Ncs = 2,
    }

	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public class SupervisionDataAttribute : Attribute
	{
		public int RawDataIndex = -1;
		public Type ConverterType;
		public TimeUnit TimeUnit = TimeUnit.MilliSeconds;
		public bool LiveUpdate = false;
        public ProductVersion AllowedProduct = ProductVersion.Express | ProductVersion.Ncs;
        public char ArraySeparator = ';';
        public int AveragedBy = -1;
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public class SupervisionAutoInitPropertyAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public class SupervisionSourceAttribute : Attribute
	{
		public string Name;
        public bool Visible = true;

		public SupervisionSourceAttribute(string name)
		{
			Name = name;
		}
	}

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SupervisionDescriptionPropertyAttribute: Attribute
    {
		public string Name;

        public SupervisionDescriptionPropertyAttribute(string name)
		{
			Name = name;
		}

    }



	public abstract class SupervisionConverterFactory
	{
		public abstract Converter<string, object> GetConverter();
	}

	public abstract class SupervisionDataCollection : System.ComponentModel.INotifyPropertyChanged, IDisposable
	{
		protected enum LiveUpdateType
		{
			None,
			Custom,
			TimeSpan
		}

		protected class SupervisionDataStructureElement
		{
			public int RawDataIndex;
			public string FieldName;
			public Type FieldType;
			public bool IsArray;
            public char ArraySeparator = ';';
			public PropertyInfo Property;
			public Converter<string, object> Converter;
			public long TicksMult = 10000;
			public LiveUpdateType LiveUpdate = LiveUpdateType.None;
            public int AveragedBy;
		}

		static private int m_LastLiveUpdateIndex;
		static private int m_LastLiveUpdateAdded;
		static private List<WeakReference>[] m_LiveUpdateTargets = new List<WeakReference>[] 
		{ 
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>(),
			new List<WeakReference>()
		};

		static SupervisionDataCollection()
		{
			m_UpdateTimer = new System.Threading.Timer(UpdateTimerCallback, null, 0, 10);
		}

		static void UpdateTimerCallback(object state)
		{
			int UpdateIndex = m_LastLiveUpdateIndex;

			System.Threading.Monitor.Enter(m_LiveUpdateTargets[UpdateIndex]);

			if (++m_LastLiveUpdateIndex >= m_LiveUpdateTargets.Length)
				m_LastLiveUpdateIndex = 0;

			try
			{
				for (int i = 0; i < m_LiveUpdateTargets[UpdateIndex].Count; i++)
				{
					WeakReference Ref = m_LiveUpdateTargets[UpdateIndex][i];
					object Target = Ref.Target;

					if (!Ref.IsAlive || Target == null)
					{
						m_LiveUpdateTargets[UpdateIndex].RemoveAt(i--);
					}
					else
					{
						System.Threading.Monitor.Exit(m_LiveUpdateTargets[UpdateIndex]);

						try
						{
							((SupervisionDataCollection)Target).LiveUpdate();
						}
						catch
						{
						}

						System.Threading.Monitor.Enter(m_LiveUpdateTargets[UpdateIndex]);
					}
				}
			}
			finally
			{
				System.Threading.Monitor.Exit(m_LiveUpdateTargets[UpdateIndex]);
			}
		}

		static void RegisterLiveUpdate(SupervisionDataCollection target)
		{
			lock (m_LiveUpdateTargets[m_LastLiveUpdateAdded])
			{
				m_LiveUpdateTargets[m_LastLiveUpdateAdded].Add(new WeakReference(target));

				if (++m_LastLiveUpdateAdded >= m_LiveUpdateTargets.Length)
					m_LastLiveUpdateAdded = 0;
			}
		}

		static void UnregisterLiveUpdate(SupervisionDataCollection target)
		{
			foreach (List<WeakReference> List in m_LiveUpdateTargets)
			{
				lock (List)
				{
					for (int i = 0; i < List.Count; i++)
					{
						if (object.ReferenceEquals(List[i].Target, target))
						{
							List.RemoveAt(i);
							return;
						}
					}
				}
			}
		}

		private static System.Threading.Timer m_UpdateTimer;

		private static SortedList<string, SupervisionDataStructureElement[]> m_AllDataStructures = new SortedList<string, SupervisionDataStructureElement[]>();

		private static SupervisionDataStructureElement[] PrepareDataStructure(SupervisionDataStructureElement[] dataStructure)
		{
			foreach (SupervisionDataStructureElement Element in dataStructure)
			{
				if (Element.Converter == null)
				{
					if (Element.FieldType == typeof(int))
					{
						Element.Converter = (x) => SupervisionSettings.ParseInt(x);
					}
					else if (Element.FieldType == typeof(long))
					{
						Element.Converter = (x) => SupervisionSettings.ParseLong(x);
					}
					else if (Element.FieldType == typeof(float))
					{
						Element.Converter = (x) => SupervisionSettings.ParseFloat(x);
					}
					else if (Element.FieldType == typeof(double))
					{
						Element.Converter = (x) => SupervisionSettings.ParseDouble(x);
					}
					else if (Element.FieldType == typeof(bool))
					{
						Element.Converter = (x) => SupervisionSettings.ParseBool(x);
					}
					else if (Element.FieldType == typeof(TimeSpan))
					{
						Element.Converter = (x) => SupervisionSettings.ParseTimeSpan(x, Element.TicksMult);
					}
					else if (Element.FieldType == typeof(DateTime))
					{
						Element.Converter = (x) => SupervisionSettings.ParseDateTime(x, Element.TicksMult);
					}
					else if (!Element.FieldType.IsAssignableFrom(typeof(string)))
					{
						Element.Converter = (x) => Convert.ChangeType(x, Element.FieldType);
					}
				}
			}

			return dataStructure;
		}

		private static SupervisionDataStructureElement[] PrepareDataStructure(Type owner)
		{
			int NextRawDataIndex = 0;
			List<SupervisionDataStructureElement> Elements = new List<SupervisionDataStructureElement>();

			foreach (PropertyInfo PInfo in owner.GetProperties())
			{
				object[] Attrs = PInfo.GetCustomAttributes(typeof(SupervisionDataAttribute), true);

				if (Attrs.Length > 0)
				{
					SupervisionDataAttribute Attr = (SupervisionDataAttribute)Attrs[0];
					SupervisionDataStructureElement Element = new SupervisionDataStructureElement();

					if (Attr.RawDataIndex >= 0)
						NextRawDataIndex = Attr.RawDataIndex;

                    Element.AveragedBy = Attr.AveragedBy;

					Element.RawDataIndex = NextRawDataIndex;
					Element.FieldName = PInfo.Name;

					if (PInfo.PropertyType.IsArray)
					{
						Element.IsArray = true;
						Element.FieldType = PInfo.PropertyType.GetElementType();
                        Element.ArraySeparator = Attr.ArraySeparator;
					}
					else
					{
						Element.FieldType = PInfo.PropertyType;
					}

					Element.Property = PInfo;

					if (Attr.ConverterType != null)
					{
						Element.Converter = ((SupervisionConverterFactory)Activator.CreateInstance(Attr.ConverterType)).GetConverter();
					}

					switch (Attr.TimeUnit)
					{
						case TimeUnit.Ticks:
							Element.TicksMult = 1;
							break;

						case TimeUnit.MilliSeconds:
							Element.TicksMult = 10000;
							break;

						case TimeUnit.Tenths:
							Element.TicksMult = 1000000;
							break;

						case TimeUnit.Seconds:
							Element.TicksMult = 10000000;
							break;
					}

					if (Attr.LiveUpdate)
					{
						if (Element.FieldType.Equals(typeof(TimeSpan)))
							Element.LiveUpdate = LiveUpdateType.TimeSpan;
						else
							Element.LiveUpdate = LiveUpdateType.Custom;
					}

					NextRawDataIndex++;
					Elements.Add(Element);
				}
			}

			return PrepareDataStructure(Elements.ToArray());
		}

		protected SupervisionDataStructureElement[] DataStructure { get; private set; }
        
		protected string[] RawValues;
		protected object[] CurrentValues;
        protected Dictionary<int, int> Indexes = new Dictionary<int, int>();
		protected object[] BaseValues;
		protected int Disposed = 0;

		private SupervisionDataStructureElement[] LiveUpdateElements;

        public string OwnerPrefix { get; private set; }
        public SupervisionItem OwnerItem { get; private set; }

		internal void SetOwner(SupervisionItem item, string prefix)
		{
			OwnerItem = item;
			OwnerPrefix = prefix;
		}

		protected SupervisionDataCollection()
		{
			string ThisTypeName = this.GetType().FullName;

			lock (m_AllDataStructures)
			{

				if (m_AllDataStructures.ContainsKey(ThisTypeName))
				{
					DataStructure = m_AllDataStructures[ThisTypeName];
				}
				else
				{
					DataStructure = PrepareDataStructure(this.GetType());
					m_AllDataStructures.Add(ThisTypeName, DataStructure);
				}
			}
            
			RawValues = new string[DataStructure.Length];
			CurrentValues = new object[DataStructure.Length];
			BaseValues = new object[DataStructure.Length];

			List<SupervisionDataStructureElement> ToUpdate = new List<SupervisionDataStructureElement>();

			foreach(SupervisionDataStructureElement Element in DataStructure)
			{
				if (Element.LiveUpdate != LiveUpdateType.None)
				{
					ToUpdate.Add(Element);
				}
			}

			if (ToUpdate.Count > 0)
			{
				LiveUpdateElements = ToUpdate.ToArray();
				RegisterLiveUpdate(this);
			}
		}

		~SupervisionDataCollection()
		{
			Dispose();
		}

		public T GetValue<T>(int index)
		{
			return (T)(CurrentValues[index] ?? default(T));
		}

		internal void SetRawValue(int index, string value)
		{
			if (!string.Equals(RawValues[index], value))
			{

				RawValues[index] = value;

                value = System.Net.WebUtility.HtmlDecode(value);

				switch (DataStructure[index].LiveUpdate)
				{
					case LiveUpdateType.TimeSpan:
						BaseValues[index] = SupervisionSettings.ParseDateTimeUtc(value, DataStructure[index].TicksMult);
						break;
				}

				if (DataStructure[index].IsArray)
				{
					string[] Parts = value.Split(DataStructure[index].ArraySeparator);

					Array Value = Array.CreateInstance(DataStructure[index].FieldType, Parts.Length);

					for (int i = 0; i < Parts.Length; i++)
					{
                        Value.SetValue((DataStructure[index].Converter == null) ? Parts[i] : DataStructure[index].Converter.Invoke(Parts[i]), i);
					}

					CurrentValues[index] = Value;
				}
				else
				{
					CurrentValues[index] = (DataStructure[index].Converter == null) ? value : DataStructure[index].Converter.Invoke(value);
				}

				if (DataStructure[index].Property != null)
					DataStructure[index].Property.SetValue(this, CurrentValues[index], null);

                if(DataStructure[index].LiveUpdate!=LiveUpdateType.TimeSpan)
    				FirePropertyChanged(index);
			}
		}


		internal void SetRawValue(string[] value, int offset)
		{
			for (int i = 0; i < DataStructure.Length; i++)
			{

                if (DataStructure[i].AveragedBy >= 0 )
                {
                    if (CurrentValues[Indexes[DataStructure[i].RawDataIndex]].GetType() == typeof(TimeSpan) && (CurrentValues[Indexes[DataStructure[i].AveragedBy]]).GetType() == typeof(int))
                    {
                        TimeSpan ts = (TimeSpan)(CurrentValues[Indexes[DataStructure[i].RawDataIndex] ]);

                        int count = (int)(CurrentValues[Indexes[DataStructure[i].AveragedBy ]]);

                        if (count > 0)
                            CurrentValues[i] = new TimeSpan(ts.Ticks / count);
                        else
                            CurrentValues[i] = TimeSpan.Zero;

                        if (DataStructure[i].Property != null)
                            DataStructure[i].Property.SetValue(this, CurrentValues[i], null);

                        if (DataStructure[i].LiveUpdate != LiveUpdateType.TimeSpan)
                            FirePropertyChanged(i);
                    }

                }
                else
                {
                    Indexes[DataStructure[i].RawDataIndex] = i;
                    SetRawValue(i, value[DataStructure[i].RawDataIndex + offset]);
                }
			}
		}

		internal void SetRawValue(string value, int offset)
		{
			SetRawValue(value.Split(','), offset);
		}

		protected void FirePropertyChanged(int index)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(DataStructure[index].FieldName));
		}

        protected void FirePropertyChanged(string strPropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strPropertyName));
        }

		protected virtual void UpdateElement(SupervisionDataStructureElement element)
		{
		}

		private void LiveUpdate()
		{
			foreach (SupervisionDataStructureElement Element in LiveUpdateElements)
			{
				switch (Element.LiveUpdate)
				{
					case LiveUpdateType.TimeSpan:
						if (BaseValues[Element.RawDataIndex] != null)
						{
                            TimeSpan NewValue = new TimeSpan();
                            DateTime dt = (DateTime)BaseValues[Element.RawDataIndex];
                            if(dt.Year < 9000 && dt.Year > 100)
    							NewValue = new TimeSpan(((DateTime.Now.Subtract(dt).Ticks - SupervisionSettings.ServerTimeOffset) / Element.TicksMult) * Element.TicksMult);

							if (((TimeSpan)CurrentValues[Element.RawDataIndex]) != NewValue)
							{
								CurrentValues[Element.RawDataIndex] = NewValue;

								if (Element.Property != null)
									Element.Property.SetValue(this, NewValue, null);
		
								FirePropertyChanged(Element.RawDataIndex);
							}
						}
						break;

					case LiveUpdateType.Custom:
						UpdateElement(Element);
						break;
				}
			}
		}

		#region INotifyPropertyChanged Members

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (System.Threading.Interlocked.Increment(ref Disposed) == 1)
			{
				UnregisterLiveUpdate(this);
				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}

	public abstract class SupervisionItem : SupervisionDataCollection, System.ComponentModel.ICustomTypeDescriptor, System.ComponentModel.INotifyPropertyChanged, IDisposable
	{
		private class PropertyDescriptor : System.ComponentModel.PropertyDescriptor
		{
			private PropertyInfo m_ChildProperty;
			private PropertyInfo m_Property;

			public PropertyDescriptor(PropertyInfo childProperty, PropertyInfo property)
				: base(string.Concat((childProperty == null) ? string.Empty : childProperty.Name, property.Name), new Attribute[0])
			{
				m_ChildProperty = childProperty;
				m_Property = property;
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override Type ComponentType
			{
				get
				{
					return (m_ChildProperty ?? m_Property).DeclaringType;
				}
			}

			public override object GetValue(object component)
			{
				object Target = ((m_ChildProperty != null) ? m_ChildProperty.GetValue(component, null) : component);

				return m_Property.GetValue(Target, null);
			}

			public override bool IsReadOnly
			{
				get
				{
					return !(m_ChildProperty ?? m_Property).CanWrite;
				}
			}

			public override Type PropertyType
			{
				get
				{
					return (m_ChildProperty ?? m_Property).PropertyType;
				}
			}

			public override void ResetValue(object component)
			{
				object Target = ((m_ChildProperty != null) ? m_ChildProperty.GetValue(component, null) : component);

				m_Property.SetValue(Target, Activator.CreateInstance(m_Property.PropertyType), null);
			}

			public override void SetValue(object component, object value)
			{
				object Target = ((m_ChildProperty != null) ? m_ChildProperty.GetValue(component, null) : component);

				m_Property.SetValue(Target, value, null);
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}
		}

		private class EventDescriptor : System.ComponentModel.EventDescriptor
		{
			private PropertyInfo m_ChildProperty;
			private EventInfo m_EventInfo;

			public EventDescriptor(PropertyInfo childProperty, EventInfo eventInfo)
				: base(string.Concat((childProperty == null) ? string.Empty : childProperty.Name, eventInfo.Name), new Attribute[0])
			{
				m_ChildProperty = childProperty;
				m_EventInfo = eventInfo;
			}

			public override void AddEventHandler(object component, Delegate value)
			{
				object Target = ((m_ChildProperty != null) ? m_ChildProperty.GetValue(component, null) : component);

				m_EventInfo.AddEventHandler(Target, value);
			}

			public override Type ComponentType
			{
				get
				{
					return ((m_ChildProperty != null) ? m_ChildProperty.DeclaringType : m_EventInfo.DeclaringType);
				}
			}

			public override Type EventType
			{
				get
				{
					return m_EventInfo.EventHandlerType;
				}
			}

			public override bool IsMulticast
			{
				get
				{
					return m_EventInfo.IsMulticast;
				}
			}

			public override void RemoveEventHandler(object component, Delegate value)
			{
				object Target = ((m_ChildProperty != null) ? m_ChildProperty.GetValue(component, null) : component);

				m_EventInfo.RemoveEventHandler(Target, value);
			}
		}

		private System.ComponentModel.PropertyDescriptorCollection m_Properties = new System.ComponentModel.PropertyDescriptorCollection(new System.ComponentModel.PropertyDescriptor[0]);
		private System.ComponentModel.EventDescriptorCollection m_Events = new System.ComponentModel.EventDescriptorCollection(new System.ComponentModel.EventDescriptor[0]);
		protected int Disposed = 0;

        protected SupervisionItem()
        {
            AddDescriptors(this, this.GetType(), null);
        }

		public string Id { get; set; }

		private void AddDescriptors(object owner, Type type, PropertyInfo childProperty)
		{
			foreach (PropertyInfo Property in type.GetProperties())
			{
				if (Property.PropertyType.IsSubclassOf(typeof(SupervisionDataCollection)))
				{
					if (Property.GetCustomAttributes(typeof(SupervisionAutoInitPropertyAttribute), true).Length > 0)
					{
						object Value = Property.GetValue(owner, null);

						AddDescriptors(Value, Property.PropertyType, Property);

						if (Value == null)
						{
							Value = Property.PropertyType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
							Property.SetValue(owner, Value, null);
						}

						((SupervisionDataCollection)Value).SetOwner(this, Property.Name);

						continue;
					}
				}

				m_Properties.Add(new PropertyDescriptor(childProperty, Property));
			}

			foreach (EventInfo Event in type.GetEvents())
			{
				m_Events.Add(new EventDescriptor(childProperty, Event));
			}
		}


		#region ICustomTypeDescriptor Members

		public System.ComponentModel.AttributeCollection GetAttributes()
		{
			return new System.ComponentModel.AttributeCollection();
		}

		public string GetClassName()
		{
			return this.GetType().Name;
		}

		public string GetComponentName()
		{
			return null;
		}

		public System.ComponentModel.TypeConverter GetConverter()
		{
			return null;
		}

		public System.ComponentModel.EventDescriptor GetDefaultEvent()
		{
			return null;
		}

		public System.ComponentModel.PropertyDescriptor GetDefaultProperty()
		{
			return null;
		}

		public object GetEditor(Type editorBaseType)
		{
			return null;
		}

		public System.ComponentModel.EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			if (attributes != null)
				System.Diagnostics.Debug.WriteLine("attributes collection ignored in GetEvents");

			return m_Events;
		}

		public System.ComponentModel.EventDescriptorCollection GetEvents()
		{
			return GetEvents(null);
		}

		public System.ComponentModel.PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			if (attributes != null)
				System.Diagnostics.Debug.WriteLine("attributes collection ignored in GetProperties");

			return m_Properties;
		}

		public System.ComponentModel.PropertyDescriptorCollection GetProperties()
		{
			return GetProperties(null);
		}

		public object GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
		{
			return this;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (System.Threading.Interlocked.Increment(ref Disposed) == 1)
			{
				foreach (PropertyInfo Property in this.GetType().GetProperties())
				{
					if (Property.PropertyType.IsSubclassOf(typeof(SupervisionDataCollection)))
					{
						SupervisionDataCollection Value = Property.GetValue(this, null) as SupervisionDataCollection;

						if (Value != null)
						{
							Property.SetValue(this, null, null);
							Value.Dispose();
						}
					}
				}

				GC.SuppressFinalize(this);
			}
		}

		#endregion

	}

	public abstract class SupervisionList
	{
		internal abstract void SetRawValue(string id, string property, string[] data, int offset);
	}

	public class SupervisionList<T> : SupervisionList, IList<T>, IDisposable, INotifyCollectionChanged where T : SupervisionItem
	{
		protected int Disposed = 0;
		protected List<T> InnerList = new List<T>();
		protected SortedList<string, T> InnerSorted = new SortedList<string, T>();

		internal static Type ItemType
		{
			get
			{
				return typeof(T);
			}
		}

		private void SetRawValue(T item, string property, string[] data, int offset)
		{
			if (string.IsNullOrEmpty(property))
			{
				item.SetRawValue(data, offset);
			}
			else
			{
				PropertyInfo PInfo = item.GetType().GetProperty(property);

				if (PInfo != null)
				{
					((SupervisionDataCollection)PInfo.GetValue(item, null)).SetRawValue(data, offset);
				}
			}
		}

		internal override void SetRawValue(string id, string property, string[] data, int offset)
		{
			T Item = null;

            if (id == null)
            {
            }
            if (this.GetType() == typeof(QueueList))
            {
            }

			if (InnerSorted.ContainsKey(id))
			{
				Item = InnerSorted[id];
				SetRawValue(Item, property, data, offset);
			}
			else
			{
				Item = CreateNewItem(id, property, data, offset);

				if (Item != null)
				{
					SetRawValue(Item, property, data, offset);
					Add(Item);
				}
			}
		}

		public virtual T CreateNewItem(string id, string property, string[] data, int offset)
		{
            if(this.GetType() == typeof(QueueList))
            {
            }
			T Item = (T)Activator.CreateInstance(typeof(T));

			Item.Id = id;

			return Item;
		}

		#region IList<T> Members

		public int IndexOf(T item)
		{
			return InnerList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			InnerList.Insert(index, item);
			InnerSorted.Add(item.Id, item);

			FireCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}

		public void RemoveAt(int index)
		{
			T OldValue = InnerList[index];
			InnerList.RemoveAt(index);
			InnerSorted.Remove(OldValue.Id);

			FireCollectionChanged(NotifyCollectionChangedAction.Remove, OldValue, index);
		}

		public T this[int index]
		{
			get
			{
				lock (InnerList)
				{
                    try
                    {

                        return InnerList[index];
                    }
                    catch
                    {
                        return null;
                    }
				}
			}
			set
			{
				lock (InnerList)
				{
					T OldValue = InnerList[index];
					InnerList[index] = value;

					InnerSorted.Remove(OldValue.Id);
					InnerSorted.Add(value.Id, value);

					FireCollectionChanged(NotifyCollectionChangedAction.Replace, OldValue, index, value);
				}
			}
		}

		public T this[string id]
		{
			get
			{
				lock (InnerList)
				{
                    try
                    {
                        return InnerSorted[id];
                    }
                    catch
                    {
                        return null;
                    }
				}
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		#endregion

        public void FireCollectionChanged(NotifyCollectionChangedAction action)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
		}

		public void FireCollectionChanged(NotifyCollectionChangedAction action, T item)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
		}

		protected void FireCollectionChanged(NotifyCollectionChangedAction action, T item, int index)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
		}

		protected void FireCollectionChanged(NotifyCollectionChangedAction action, T oldItem, int index, T newItem)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}

		#region ICollection<T> Members

		public void Add(T item)
		{
			int Index = -1;

			lock(InnerList)
			{
				InnerList.Add(item);
				Index = InnerList.Count - 1;

				InnerSorted.Add(item.Id, item);
			}

			FireCollectionChanged(NotifyCollectionChangedAction.Add, item, Index);
		}

		public void Clear()
		{
			lock (InnerList)
			{
				InnerList.Clear();
				InnerSorted.Clear();
			}

			FireCollectionChanged(NotifyCollectionChangedAction.Reset);
		}

		public bool Contains(T item)
		{
			lock (InnerList)
			{
				return InnerSorted.ContainsKey(item.Id);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (InnerList)
			{
				InnerList.CopyTo(array, arrayIndex);
			}
		}

		public int Count
		{
			get
			{
				lock (InnerList)
				{
					return InnerList.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool Remove(T item)
		{
			lock (InnerList)
			{
				if (!InnerList.Remove(item))
					return false;

				InnerSorted.Remove(item.Id);
			}

			FireCollectionChanged(NotifyCollectionChangedAction.Remove, item);

			return true;
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			lock (InnerList)
			{
				return InnerList.GetEnumerator();
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			lock (InnerList)
			{
				return InnerList.GetEnumerator();
			}
		}

		#endregion

		#region INotifyCollectionChanged Members

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (System.Threading.Interlocked.Increment(ref Disposed) == 1)
			{
				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}

    public delegate void AgentWarningEventHandler(string message);
    public delegate void AgentMessageEventHandler(string[] info);

	public class SupervisionSource
	{
        public event AgentWarningEventHandler AgentWarningMsg;
        internal void OnAgentWarningMsg(string message)
        {
            if (AgentWarningMsg != null)
            {
                try
                {
                    AgentWarningMsg(message);
                }
                catch
                {
                }
            }
        }

        public event AgentMessageEventHandler AgentMessage;
        internal void OnAgentMessage(string[] info)
        {
            if (AgentMessage != null)
            {
                try
                {
                    AgentMessage(info);
                }
                catch
                {
                }
            }
        }

		private SortedList<string, PropertyInfo> m_Lists = new SortedList<string, PropertyInfo>();

		public SupervisionSource(Nixxis.Client.HttpLink clientLink)
		{
            try
            {
                SupervisionSettings.ServerTimeOffset = clientLink.GetServerTimeOffset();
                SupervisionSettings.DateTimeFormat = clientLink.DateTimeFormat;

                System.Diagnostics.Trace.WriteLine("Server time offset: " + (SupervisionSettings.ServerTimeOffset / 10000).ToString() + " ms");
            }
            catch
            {
            }

			clientLink.ServerEvent += new Nixxis.Client.HttpLinkServerEventDelegate(OnClientLinkServerEvent);
            clientLink.AgentMessage += new Client.HttpLinkServerEventDelegate(OnClientLinkAgentMessage);
			Type ThisType = this.GetType();

			foreach (PropertyInfo PInfo in ThisType.GetProperties())
			{
				object[] Attrs = PInfo.GetCustomAttributes(typeof(SupervisionSourceAttribute), true);

				if (Attrs.Length > 0)
				{
					m_Lists.Add(((SupervisionSourceAttribute)Attrs[0]).Name, PInfo);
				}
				else
				{
					for (Type PType = PInfo.PropertyType; PType != null; PType = PType.BaseType)
					{
						if (PType.IsGenericType)
						{
							Type[] PArgs = PType.GetGenericArguments();

							if (PArgs.Length == 1 && PArgs[0].IsSubclassOf(typeof(SupervisionItem)))
							{
								m_Lists.Add(PArgs[0].Name, PInfo);
								break;
							}
						}
					}
				}
			}
		}

        void OnClientLinkAgentMessage(Client.ServerEventArgs eventArgs)
        {
            string[] Parts = eventArgs.Parameters.Split(',');

            // Simulate @@Message, to be done better some day ;-)

            string[] Sim = new string[6];

            Sim[0] = "Agent_" + Parts[1];
            Sim[1] = "@@Message";
            Sim[2] = Parts[0];
            Sim[3] = "1";
            Sim[4] = Microsoft.JScript.GlobalObject.unescape(Parts[2]);
            Sim[5] = Microsoft.JScript.GlobalObject.unescape(Parts[5]);

            OnAgentMessage(Sim);
        }

		void OnClientLinkServerEvent(Nixxis.Client.ServerEventArgs eventArgs)
		{
			if (eventArgs.EventCode == Nixxis.Client.EventCodes.SupervisionItem || eventArgs.EventCode == Nixxis.Client.EventCodes.SupervisionData)
			{
				string[] Parts = eventArgs.Parameters.Split(',');

				if (Parts.Length > 1)
				{
                    if (Parts[1] == "@@Message")
                    {
                        OnAgentMessage(Parts);
                    }
                    else
                    {
                        string[] TypeAndId = Parts[0].Split('_');

                        if (TypeAndId.Length == 2)
                        {
                            if (m_Lists.ContainsKey(TypeAndId[0]))
                            {
                                if (eventArgs.EventCode == Nixxis.Client.EventCodes.SupervisionItem)
                                {
                                    ((SupervisionList)m_Lists[TypeAndId[0]].GetValue(this, null)).SetRawValue(TypeAndId[1], null, Parts, 1);
                                }
                                else
                                {
                                    ((SupervisionList)m_Lists[TypeAndId[0]].GetValue(this, null)).SetRawValue(TypeAndId[1], Parts[1].Substring(2), Parts, 2);
                                }
                            }
                        }
                    }
				}
			}
            else if (eventArgs.EventCode == Client.EventCodes.AgentWarning)
            {
                string msg = eventArgs.Parameters.Substring(eventArgs.Parameters.IndexOf(',') + 1);
                System.Diagnostics.Trace.WriteLine("Info msg: " + msg);
                OnAgentWarningMsg(msg);
            }
            else if (eventArgs.EventCode == Client.EventCodes.AgentMessage)
            {
                string msg = eventArgs.Parameters.Substring(eventArgs.Parameters.IndexOf(',') + 1);
                System.Diagnostics.Trace.WriteLine("Info msg: " + msg);
            }
		}
	}

}
