using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nixxis.Client.Agent
{
    public class WellKnownDestinationCollection : ObservableCollection<WellKnownDestinationItem>
    {
        public bool Contains(string destination)
        {
            foreach (WellKnownDestinationItem item in this)
            {
                if (item.Destination == destination)
                    return true;
            }

            return false;
        }
    }

    public class WellKnownDestinationItem : INotifyPropertyChanged
    {
        #region Class data
        private string m_Destination;
        private string m_DisplayText;
        private string m_Type;
        #endregion

        #region Constructors
        #endregion

        #region Propeties
        public string Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; FirePropertyChanged("Destination"); }
        }
        public string DisplayText
        {
            get { return m_DisplayText; }
            set { m_DisplayText = value; FirePropertyChanged("DisplayText"); }
        }
        public string Type
        {
            get { return m_Type; }
            set { m_Type = value; FirePropertyChanged("Type"); }
        }
        #endregion

        #region Members
        public override string ToString()
        {
            return m_DisplayText;
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
