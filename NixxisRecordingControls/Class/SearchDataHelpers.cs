using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Nixxis.Client.Admin;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Recording
{
    //
    //Parameters
    //
    public class SearchParameters : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext("SearchDataHelpers");

        #region Class Data
        private DateTime m_FromDate = DateTime.Now;
        private TimeSpan m_FromTime = TimeSpan.Zero;
        private DateTime m_ToDate = DateTime.Now;
        private TimeSpan m_ToTime = new TimeSpan(23, 59, 59);
        private DisplayItem m_TeamItem;
        private DisplayItem m_AgentItem;
        private DisplayItem m_CampaignItem;
        private DisplayItem m_ActivityItem;
        private string m_Originator = string.Empty;
        private string m_Destination = string.Empty;
        private string m_Extension = string.Empty;
        private bool m_IncludeQualification;
        private string m_Qualification;
        private string m_QualificationDescription;
        
        private string m_FilterOnContactTypeDescription = string.Empty;
        private bool m_FilterOnContactTypes = false;
        private bool m_IncludeInbound = true;
        private bool m_IncludeManual = true;
        private bool m_IncludeOutbound = true;
        private bool m_IncludeDirect = true;
        private bool m_IncludeChat = true;

        private bool m_PositiveCheck = false;
        private bool m_IsQualificationValuePositive = true;
        private bool m_IsQualificationValueNeutral = false;
        private bool m_IsQualificationValueNegative = false;
        private int m_Positive = 1;


        private bool m_ArguedCheck = false;
        private bool m_Argued = true;
        #endregion

        #region constructor
        public SearchParameters()
        {
            SetFilterOnContactTypeDescription();
            SetPositiveValue();
        }
        #endregion

        #region Properties
        public DateTime FromDate
        {
            get { return m_FromDate; }
            set { m_FromDate = value; }
        }
        public TimeSpan FromTime
        {
            get { return m_FromTime; }
            set { m_FromTime = value; }
        }
        public DateTime ToDate
        {
            get { return m_ToDate; }
            set { m_ToDate = value; }
        }
        public TimeSpan ToTime
        {
            get { return m_ToTime; }
            set { m_ToTime = value; }
        }
        public DisplayItem TeamItem
        {
            get { return m_TeamItem; }
            set { m_TeamItem = value; }
        }
        public DisplayItem AgentItem
        {
            get { return m_AgentItem; }
            set { m_AgentItem = value; }
        }
        public DisplayItem CampaignItem
        {
            get { return m_CampaignItem; }
            set { m_CampaignItem = value; FirePropertyChanged("CampaignItem"); }
        }
        public DisplayItem ActivityItem
        {
            get { return m_ActivityItem; }
            set { m_ActivityItem = value; FirePropertyChanged("ActivityItem"); }
        }
        public string Originator
        {
            get { return m_Originator; }
            set { m_Originator = value; }
        }
        public string Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }
        public string Extension
        {
            get { return m_Extension; }
            set { m_Extension = value; }
        }
        public string Qualification
        {
            get { return m_Qualification; }
            set { m_Qualification = value; FirePropertyChanged("Qualification"); }
        }
        public string QualificationDescription
        {
            get { return m_QualificationDescription; }
            set { m_QualificationDescription = value; FirePropertyChanged("QualificationDescription"); }
        }
        public bool IncludeQualification
        {
            get { return m_IncludeQualification; }
            set { m_IncludeQualification = value; }
        }
        
        public string FilterOnContactTypeDescription
        {
            get { return m_FilterOnContactTypeDescription; }
        }
        public bool FilterOnContactTypes
        {
            get { return m_FilterOnContactTypes; }
            set { m_FilterOnContactTypes = value; FirePropertyChanged("FilterOnContactTypes"); }
        }
        public bool IncludeInbound
        {
            get { return m_IncludeInbound; }
            set
            {
                m_IncludeInbound = value;
                SetFilterOnContactTypeDescription();
                FirePropertyChanged("IncludeInbound");
            }
        }
        public bool IncludeManual
        {
            get { return m_IncludeManual; }
            set
            {
                m_IncludeManual = value;
                SetFilterOnContactTypeDescription();
                FirePropertyChanged("IncludeManual");
            }
        }
        public bool IncludeOutbound
        {
            get { return m_IncludeOutbound; }
            set
            {
                m_IncludeOutbound = value;
                SetFilterOnContactTypeDescription();
                FirePropertyChanged("IncludeOutbound");
            }
        }
        public bool IncludeDirect
        {
            get { return m_IncludeDirect; }
            set
            {
                m_IncludeDirect = value;
                SetFilterOnContactTypeDescription();
                FirePropertyChanged("IncludeDirect");
            }
        }
        public bool IncludeChat
        {
            get { return m_IncludeChat; }
            set
            {
                m_IncludeChat = value;
                SetFilterOnContactTypeDescription();
                FirePropertyChanged("IncludeChat");
            }
        }

        public bool PositiveCheck
        {
            get { return m_PositiveCheck; }
            set { m_PositiveCheck = value; FirePropertyChanged("PositiveCheck"); }
        }
        public bool IsQualificationValuePositive
        {
            get { return m_IsQualificationValuePositive; }
            set
            {
                m_IsQualificationValuePositive = value;
                FirePropertyChanged("IsQualificationValuePositive");
                SetPositiveValue();
            }
        }
        public bool IsQualificationValueNeutral
        {
            get { return m_IsQualificationValueNeutral; }
            set
            {
                m_IsQualificationValueNeutral = value;
                FirePropertyChanged("IsQualificationValueNeutral");
                SetPositiveValue();
            }
        }
        public bool IsQualificationValueNegative
        {
            get { return m_IsQualificationValueNegative; }
            set
            {
                m_IsQualificationValueNegative = value;
                FirePropertyChanged("IsQualificationValueNegative");
                SetPositiveValue();
            }
        }
        public int Positive
        {
            get { return m_Positive; }
            set { m_Positive = value; FirePropertyChanged("Positive"); }
        }

        public bool ArguedCheck
        {
            get { return m_ArguedCheck; }
            set { m_ArguedCheck = value; }
        }
        public bool Argued
        {
            get { return m_Argued; }
            set { m_Argued = value; }
        }
        #endregion

        #region Members
        private void SetFilterOnContactTypeDescription()
        {
            StringBuilder sb = new StringBuilder();

            if (IncludeInbound)
                sb.Append(TranslationContext.Translate("Inbound"));

            if (IncludeOutbound)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(TranslationContext.Translate("Outbound"));
            }

            if (IncludeChat)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(TranslationContext.Translate("Chat"));
            }

            if (IncludeManual)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(TranslationContext.Translate("Manual"));
            }

            if (IncludeDirect)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(TranslationContext.Translate("Direct"));
            }

            if (!m_FilterOnContactTypeDescription.Equals(sb.ToString()))
            {
                m_FilterOnContactTypeDescription = sb.ToString();
                FirePropertyChanged("FilterOnContactTypeDescription");
            }
        }
        private void SetPositiveValue()
        {
            int newValue = 0;

            if (m_IsQualificationValuePositive)
                newValue = 1;
            else if (m_IsQualificationValueNeutral)
                newValue = 0;
            else
                newValue = -1;

            if (!newValue.Equals(m_Positive))
            {
                Positive = newValue;
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
    
    public static class CreateDisplayLists
    {
        public static TranslationContext TranslationContext = new TranslationContext("SearchDataHelpers");

        public static string NoSelection = TranslationContext.Translate("- No selection -");
        public static string InactiveSuffix = TranslationContext.Translate(" (Inactive)");

        public static DisplayList CreateTeamList(AdminLight adminLight, bool includeInactive)
        {
            DisplayList list = new DisplayList();

            if (adminLight == null)
                return list;
            //
            //Active list
            foreach (AdminObjectLight item in adminLight.Teams)
                list.Add(new DisplayItem(item.Id, item.Description));

            list.Sort((a, b) => a.Description.CompareTo(b.Description));
            list.Insert(0, new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection));

            return list;
        }

        public static DisplayList CreateAgentList(AdminLight adminLight, bool includeInactive)
        {
            return CreateAgentList(adminLight, includeInactive, string.Empty);
        }

        public static DisplayList CreateAgentList(AdminLight adminLight, bool includeInactive, string teamId)
        {
            DisplayList list = new DisplayList();

            if (adminLight == null)
                return list;

            foreach (AdminObjectLight item in adminLight.Agents)
                if(string.IsNullOrEmpty(teamId) || item.Related.Contains(teamId))
                    if(includeInactive || item.State>0)
                        list.Add(new DisplayItem(item.Id, item.Description + (item.State==0?InactiveSuffix:string.Empty)));

            list.Sort((a, b) => a.Description.CompareTo(b.Description));
            list.Insert(0, new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection));

            return list;
        }

        public static DisplayList CreateCampaignList(AdminLight adminLight, bool includeInactive)
        {
            DisplayList list = new DisplayList();

            if (adminLight == null)
                return list;
            //
            //Active list
            foreach (AdminObjectLight item in adminLight.Campaigns)
                if (includeInactive || item.State>0)
                    list.Add(new DisplayItem(item.Id, item.Description + (item.State==0 ? InactiveSuffix : string.Empty)));
            
            list.Sort((a, b) => a.Description.CompareTo(b.Description));
            list.Insert(0, new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection));

            return list;
        }

        public static DisplayList CreateActivitiesList(AdminLight adminLight, bool includeInactive, string campaignId)
        {
            DisplayList list = new DisplayList();

            if (adminLight == null)
                return list;
            //
            //Active list
            foreach (AdminObjectLight item in adminLight.Activities)
            {
                if(includeInactive || item.State>0)
                    if(item.Related.Contains(campaignId))
                        list.Add(new DisplayItem(item.Id, string.Format("{0} - {1}", item.Description, item.Type)+ (item.State==0?InactiveSuffix:string.Empty)));
            }

            list.Sort((a, b) => a.Description.CompareTo(b.Description));
            list.Insert(0, new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection));

            return list;
        }
    }

}
