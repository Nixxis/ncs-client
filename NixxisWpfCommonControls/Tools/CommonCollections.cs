using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Collections;
using System.Reflection;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// For selecting a array of objects
    /// </summary>
    public class NixxisSelectedItemCollection : ObservableCollection<object>
    {
    }

    /// <summary>
    /// Warning message collections
    /// </summary>
    public class AgentWarningMessageCollection : ObservableCollection<AgentWarningMessageItem>
    {
        private int m_MaxItems = 5;
        public int MaxItems
        {
            get { return m_MaxItems; }
            set { m_MaxItems = value; }
        }

        public void AddMessage(string message)
        {
            if (this.Count == m_MaxItems)
                this.RemoveAt(m_MaxItems - 1);

            this.Insert(0, new AgentWarningMessageItem(message));
        }
    }

    /// <summary>
    /// Warning messahe item
    /// </summary>
    public class AgentWarningMessageItem : INotifyPropertyChanged
    {
        #region Class data
        private string m_Text = string.Empty;
        private DateTime m_ReceivedDateTime = DateTime.Now;
        #endregion

        #region Constructors
        public AgentWarningMessageItem()
        {
        }
        public AgentWarningMessageItem(string message)
        {
            this.Text = message;
        }
        #endregion

        #region Propeties
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; FirePropertyChanged("Text"); }
        }
        public DateTime ReceivedDateTime
        {
            get { return m_ReceivedDateTime; }
            set { m_ReceivedDateTime = value; FirePropertyChanged("ReceivedDateTime"); }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }

    /// <summary>
    /// Simple collection to display a key value pair
    /// </summary>
    public class DisplayList : ObservableCollection<DisplayItem>
    {
        public void Sort(Comparison<DisplayItem> comparison)
        {
            List<DisplayItem> list = this.ToList<DisplayItem>();
            list.Sort(comparison);

            this.Clear();
            foreach (DisplayItem item in list)
                this.Add(item);
        }
        public void Sort(IComparer<DisplayItem> comparer)
        {
            List<DisplayItem> list = this.ToList<DisplayItem>();
            list.Sort(comparer);

            this.Clear();
            foreach (DisplayItem item in list)
                this.Add(item);
        }

        public static DisplayList ConverFromDataTable(DataTable data)
        {
            return ConverFromDataTable(data, "Id", "Description");
        }
        public static DisplayList ConverFromDataTable(DataTable data, string idString, string descriptionString)
        {
            return ConverFromDataTable(data, "Id", new string[] { "Description" }, "{0}");
        }
        public static DisplayList ConverFromDataTable(DataTable data, string idString, string[] descriptionString, string descriptionFormat)
        {
            DisplayList rtn = new DisplayList();

            for (int i = 0; i < data.Rows.Count; i++)
            {
                string id = data.Rows[i][idString] as string;
                string[] description = new string[descriptionString.Length];

                for (int j = 0; j < descriptionString.Length; j++)
                {
                    description[j] = data.Rows[i][descriptionString[j]].ToString();
                }

                rtn.Add(new DisplayItem(id, string.Format( descriptionFormat, description)));
            }
            return rtn;
        }
        public static DisplayList ConvertFromEnum(IEnumerable collection, string idProperty, string descriptionProperty)
        {
            DisplayList rtn = new DisplayList();
            Type tpe = null;
            PropertyInfo idprop = null;
            PropertyInfo descprop = null;
            if (collection != null)
            {
                foreach (object obj in collection)
                {
                    if (tpe == null)
                    {
                        tpe = obj.GetType();
                        idprop = tpe.GetProperty(idProperty);
                        descprop = tpe.GetProperty(descriptionProperty);
                    }
                    if (tpe != null)
                        rtn.Add(new DisplayItem(idprop.GetValue(obj, null) as string, descprop.GetValue(obj, null) as string));
                }
            }
            return rtn;
        }

    }

    /// <summary>
    /// Dispaly item 
    /// </summary>
    public class DisplayItem : INotifyPropertyChanged
    {
        #region Class data
        private string m_Id;
        private string m_Description;
        #endregion

        #region Constructors
        public DisplayItem()
        {
        }
        public DisplayItem(string id, string description)
        {
            m_Id = id;
            m_Description = description;
        }
        #endregion

        #region Propeties
        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; FirePropertyChanged("Id"); }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; FirePropertyChanged("Description"); }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
