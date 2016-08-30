using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Nixxis.Client.Controls;
using System.Data;

namespace Nixxis.Client.Reporting
{
    public class NixxisReportCollection : ObservableCollection<NixxisReportItem>
    {      

        public bool Contains(object value)
        {
            foreach (NixxisReportItem item in this)
            {
                if (item.Equals(value))
                    return true;
            }
            return false;
        }

        public NixxisReportItem GetValueOfName(string value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    return this[i];
            }
            return null;
        }

        public int GetIndexOfName(string value)
        {
            for (int i = 0; i < this.Count; i++ )
            {
                if (this[i].Name.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }
            return -1;
        }
    }

    public enum ReportType
    {
        ReportingServer,
        CustomExport
    }

    public class NixxisReportItem : INotifyPropertyChanged
    {
        #region Class data
        private ReportSettings m_ReportSettings;
        private bool m_IsEnabled = false;
        private string m_Path = string.Empty;
        private string m_Name = string.Empty;
        private string m_Description = string.Empty;
        private NixxisReportItem m_Parent = null;
        private NixxisReportCollection m_Children = new NixxisReportCollection();
        private ReportType m_Type = ReportType.CustomExport;

        private object m_Tag = null;
        #endregion

        #region Constructors
        public NixxisReportItem()
        {
        }
        #endregion

        #region Propeties
        public ReportType ReportType
        {
            get { return m_Type; }
            set { m_Type = value; FirePropertyChanged("ReportType"); }
        }
        public string Path
        {
            get
            {
                return m_Path;
            }
            set
            {
                m_Path = value; FirePropertyChanged("Path");
            }
        }
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; FirePropertyChanged("Name"); }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; FirePropertyChanged("Description"); }
        }
        public object Tag
        {
            get { return m_Tag; }
            set { m_Tag = value; FirePropertyChanged("Tag"); }
        }
        public NixxisReportItem Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; FirePropertyChanged("Parent"); }
        }
        public NixxisReportCollection Children
        {
            get { return m_Children; }
            set { m_Children = value; FirePropertyChanged("Children"); }
        }
        public bool IsEnabled
        {
            get
            {
                if (m_ReportSettings == null)
                    return true;
                return m_ReportSettings.IsEnabled;
            }
        }
        public ReportSettings ReportSettings
        {
            get
            {
                return m_ReportSettings;
            }
            set
            {
                m_ReportSettings = value;
            }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        public void AddChild(NixxisReportItem item)
        {
            if (item == null)
                return;

            item.Parent = this;
            Children.Add(item);
        }
        #endregion
    }

    public static class CreateDisplayLists
    {
        public static TranslationContext TranslationContext = new TranslationContext("SearchDataHelpers");

        public static string NoSelection = TranslationContext.Translate("- No selection -");
        public static string InactiveSuffix = TranslationContext.Translate(" (Inactive)");

        public static DisplayList CreateTeamList(DataSet theCore, bool includeInactive)
        {
            DisplayList list = new DisplayList();

            return list;
        }

        public static DisplayList CreateAgentList(DataSet theCore, bool includeInactive)
        {
            return CreateAgentList(theCore, includeInactive, string.Empty);
        }

        public static DisplayList CreateAgentList(DataSet theCore, bool includeInactive, string teamId)
        {
            DisplayList list = new DisplayList();

            return list;
        }

        public static DisplayList CreateCampaignList(DataSet theCore, bool includeInactive)
        {
            DisplayList list = new DisplayList();

            return list;
        }

        public static DisplayList CreateActivitiesList(DataSet campaign, bool includeInactive)
        {
            DisplayList list = new DisplayList();

            return list;
        }
    }
}
