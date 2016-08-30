using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nixxis.Client.Recording
{
    public class CustomerDataList : ObservableCollection<CustomerData>
    {
    }

    public class CustomerData : INotifyPropertyChanged
    {
        #region Class data
        private string m_FieldName = string.Empty;
        private string m_RawValue = string.Empty;
        private Type m_ValueType = typeof(string);
        private object m_Value;
        #endregion

        #region Properties
        public string FieldName
        {
            get { return m_FieldName; }
            set { m_FieldName = value; FirePropertyChanged("FieldName"); }
        }
        public string RawValue
        {
            get { return m_RawValue; }
        }
        public Type ValueType
        {
            get { return m_ValueType; }
            set { m_ValueType = value; FirePropertyChanged("ValueType"); }
        }
        public object Value
        {
            get { return m_Value; }
            set { m_Value = value; FirePropertyChanged("Value"); }
        }
        public string DisplayValue
        {
            get
            {
                if (m_Value == null)
                    return string.Empty;

                return m_Value.ToString();
            }
        }
        #endregion

        #region COnstructors
        public CustomerData()
        {
        }
        public CustomerData(string fieldName, Type valueType)
        {
            m_FieldName = fieldName;
            m_ValueType = valueType;
        }
        public CustomerData(string fieldName, Type valueType, string rawValue)
        {
            m_FieldName = fieldName;
            m_ValueType = valueType;
            m_RawValue = rawValue;
            SetRawData(rawValue);
        }
        public CustomerData(string fieldName, object value)
        {
            m_FieldName = fieldName;
            m_ValueType = value.GetType();
            m_Value = value;
        }
        #endregion

        #region Members
        public void SetRawData(string value)
        {
            try
            {
                if (m_ValueType == typeof(int))
                    m_Value = System.Xml.XmlConvert.ToInt32(value);
                else if (m_ValueType == typeof(DateTime))
                    m_Value = System.Xml.XmlConvert.ToDateTime(value);
                else if (m_ValueType == typeof(bool))
                    m_Value = System.Xml.XmlConvert.ToBoolean(value);
                else if (m_ValueType == typeof(long))
                    m_Value = System.Xml.XmlConvert.ToInt64(value);
                else if (m_ValueType == typeof(double))
                    m_Value = System.Xml.XmlConvert.ToDouble(value);
                else if (m_ValueType == typeof(float))
                    m_Value = System.Xml.XmlConvert.ToSingle(value);
                else if (m_ValueType == typeof(TimeSpan))
                    m_Value = System.Xml.XmlConvert.ToTimeSpan(value);
                else
                    m_Value = value;
            }
            catch
            {
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
