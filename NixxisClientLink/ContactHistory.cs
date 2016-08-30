using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Nixxis.Client
{
    public class ContactHistoryItem : INotifyPropertyChanged
    {
        private DateTime m_ContactTime;
        private ContactMedia m_Media;
        private char m_Direction;
        private string m_Status;
        private string m_RecordingId;
        private string m_ContactId;
        private string m_QualifiedBy;

        public string[] GetRecordings()
        {
            return new string[0];
        }

        public string ContactId
        {
            get { return m_ContactId; }
            set { m_ContactId = value; }
        }

        public string ActivityId { get; set; }

        public string Activity { get; set; }

        public string ContactTime { get; set; }

        public DateTime LocalDateTime { get; set; }

        public string Media { get; set; }

        public string Direction { get; set; }

        public TimeSpan SetupTime { get; set; }

        public TimeSpan ComTime { get; set; }

        public TimeSpan QueueTime { get; set; }

        public TimeSpan TalkTime { get; set; }

        public string QualificationId { get; set; }

        public string Qualification { get; set; }

        public string QualifiedBy
        {
            get { return m_QualifiedBy; }
            set { m_QualifiedBy = value; FirePropertyChanged("QualifiedBy"); }
        }

        public string QualifiedById { get; set; }

        public string RecordingId { get; set; }

        public TimeSpan RecordingMarker { get; set; }

        public string CurrentContactId { get; internal set; }        
        
        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }

	public class ContactHistory : BindingList<ContactHistoryItem>
	{
		private PropertyDescriptor m_SortProperty;
		private ListSortDirection m_SortDirection;

		public ContactHistory()
		{
			this.AllowEdit = false;
			this.AllowNew = false;
			this.AllowRemove = false;
		}

		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			m_SortProperty = prop;
			m_SortDirection = direction;

		}

		protected override bool SupportsSearchingCore
		{
			get
			{
				return false;
			}
		}

		protected override bool SupportsSortingCore
		{
			get
			{
				return false;
			}
		}

		protected override bool IsSortedCore
		{
			get
			{
				return (m_SortProperty != null);
			}
		}

		protected override ListSortDirection SortDirectionCore
		{
			get
			{
				return m_SortDirection;
			}
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get
			{
				return m_SortProperty;
			}
		}

		protected override void RemoveSortCore()
		{
			m_SortProperty = null;
		}
	}
}
