using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nixxis.Client
{

    public enum ScheduleOccupationLevel
    {
        Free,
        Light,
        Medium,
        Heavy,
        Busy
    }

    public class CalendarEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime m_Start;
        private double m_Filled;
        private bool m_Enabled;
        private bool m_Visible;

        public DateTime Start
        {
            get
            {
                return m_Start;
            }
            set
            {
                m_Start = value;
                FirePropertyChanged("Start");
                FirePropertyChanged("StartTime");
            }
        }
        public TimeSpan StartTime
        {
            get
            {
                if (m_Enabled)
                    return m_Start.TimeOfDay;
                else
                    return TimeSpan.MinValue;
            }
        }
        public double Filled
        {
            get
            {
                return m_Filled;
            }
            set
            {
                m_Filled = value;
                FirePropertyChanged("Filled");
            }
        }
        public bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                m_Enabled = value;
                FirePropertyChanged("Enabled");
            }
        }
        public bool Visible
        {
            get
            {
                return m_Visible;
            }
            set
            {
                m_Visible = value;
                FirePropertyChanged("Visible");
            }
        }

        public void Load(DateTime day, string strEntry)
        {
            string[] strParts = strEntry.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
            Start = day + TimeSpan.Parse(strParts[0]);

            ScheduleOccupationLevel level = (ScheduleOccupationLevel)Enum.Parse(typeof(ScheduleOccupationLevel), strParts[1]);

            switch (level)
            {
                case ScheduleOccupationLevel.Free:
                    Filled = 0;
                    break;
                case ScheduleOccupationLevel.Light:
                    Filled = 0.25;
                    break;
                case ScheduleOccupationLevel.Medium:
                    Filled = 0.5;
                    break;
                case ScheduleOccupationLevel.Heavy:
                    Filled = 0.75;
                    break;
                case ScheduleOccupationLevel.Busy:
                    Filled = 1;
                    break;
            }

            Enabled = (int.Parse(strParts[2]) == 0);
            Visible = (int.Parse(strParts[2]) != 2);
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class Day : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_FirstNonClosed;
        private int m_LastNonClosed;
        private TimeSpan m_SelectedTime;
        private DateTime m_Day;
        private ObservableCollection<CalendarEntry> m_Entries = new ObservableCollection<CalendarEntry>();

        public TimeSpan SelectedTime
        {
            get
            {
                return m_SelectedTime;
            }
            set
            {
                m_SelectedTime = value;
                FirePropertyChanged("SelectedTime");
                FirePropertyChanged("Filled");
                FirePropertyChanged("Enabled");
            }
        }
        public DateTime DayValue
        {
            get
            {
                return m_Day;
            }
            set
            {
                m_Day = value;
                FirePropertyChanged("Day");
            }
        }
        public double Filled
        {
            get
            {
                double avg = 0;
                foreach (CalendarEntry ce in m_Entries)
                {
                    if (ce.Start.TimeOfDay.Equals(m_SelectedTime))
                    {
                        return ce.Filled;
                    }
                    avg += ce.Filled;
                }

                return avg / m_Entries.Count;
            }
        }
        public bool Enabled
        {
            get
            {
                bool enabled = false;
                foreach (CalendarEntry ce in m_Entries)
                {
                    if (ce.Start.TimeOfDay.Equals(m_SelectedTime))
                    {
                        return ce.Enabled;
                    }
                    enabled |= ce.Enabled;
                }
                return enabled;
            }
        }
        public ObservableCollection<CalendarEntry> Entries
        {
            get
            {
                return m_Entries;
            }
            set
            {
                m_Entries = value;
                FirePropertyChanged("Entries");
            }
        }
        public int FirstNonClosed
        {
            get
            {
                return m_FirstNonClosed;
            }
        }
        public int LastNonClosed
        {
            get
            {
                return m_LastNonClosed;
            }
        }
        public void Load(string strDay)
        {
            string[] strSplit = strDay.Substring(2, strDay.Length-4).Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries);


            // Server is doing:
            //(TheDay.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000

            DayValue = new DateTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks + (long.Parse(strSplit[0])) * 10000).ToLocalTime().Date;

            m_FirstNonClosed = strSplit.Length;
            m_LastNonClosed = -1;

            for (int i = 1; i < strSplit.Length; i++ )
            {
                CalendarEntry c = new CalendarEntry();
                c.Load(DayValue, strSplit[i]);
                if (c.Visible)
                {
                    if (m_FirstNonClosed == strSplit.Length)
                        m_FirstNonClosed = i-1;
                    m_LastNonClosed = i;
                }
                
                Entries.Add(c);
            }

        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class Calendar : INotifyPropertyChanged
    {
        public delegate void GetDaysDelegate(Nixxis.Client.Calendar cal, string contactId, string qualificationId, DateTime? startTime, DateTime? endTime);

        private GetDaysDelegate GetDays;
        private string m_ContactId;
        private string m_QualificationId;
        private TimeSpan m_StartTime;
        private TimeSpan m_EndTime;
        private TimeSpan m_SelectedTime;
        private DateTime m_SelectedDate;
        private DateTime m_LastSelectedDate;
        private ObservableCollection<Day> m_Days = new ObservableCollection<Day>();

        public Calendar(GetDaysDelegate getDaysDelegate, string contactId)
        {
            m_StartTime = new TimeSpan(0, 0, 0);
            m_EndTime = new TimeSpan(24, 0, 0);
            GetDays = getDaysDelegate;
            m_ContactId = contactId;
            m_SelectedTime = DateTime.Now.AddHours(1).TimeOfDay;
        }

        
        public event PropertyChangedEventHandler PropertyChanged;

        public TimeSpan StartTime
        {
            get
            {
                return m_StartTime;
            }
            set
            {
                m_StartTime = value;
                FirePropertyChanged("StartTime");
            }
        }
        public TimeSpan EndTime
        {
            get
            {
                return m_EndTime;
            }
            set
            {
                m_EndTime = value;
                FirePropertyChanged("EndTime");
            }
        }
        public TimeSpan SelectedTime
        {
            get
            {
                return m_SelectedTime;
            }
            set
            {
                m_SelectedTime = value;
                FirePropertyChanged("SelectedTime");

                foreach (Day d in m_Days)
                {
                    d.SelectedTime = value;
                }
            }
        }
        public DateTime SelectedDate
        {
            get
            {
                return m_SelectedDate;
            }
            set
            {
                if (m_SelectedDate != null)
                {
                    m_LastSelectedDate = m_SelectedDate;
                    FirePropertyChanged("LastSelectedDate");
                }
                m_SelectedDate = value;
                FirePropertyChanged("SelectedDate");
            }
        }
        public DateTime LastSelectedDate
        {
            get
            {
                return m_LastSelectedDate;
            }
        }
        public ObservableCollection<Day> Days
        {
            get
            {
                return m_Days;
            }
            set
            {
                m_Days = value;
                FirePropertyChanged("Days");
            }
        }
        public string QualificationId
        {
            get
            {
                return m_QualificationId;
            }
            set
            {
                m_QualificationId = value;
                FirePropertyChanged("QualificationId");
                GetDays(this, m_ContactId, m_QualificationId, Days.Count>0 ? Days[0].DayValue as DateTime?: null, null);
            }
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }



        public void Load(string[] days)
        {
            Days.Clear();
            // ['1391554800000','08:00,0,0,','08:15,0,0,','08:30,0,0,','08:45,0,0,','09:00,0,0,','09:15,0,0,','09:30,0,0,','09:45,0,0,','10:00,0,0,','10:15,0,0,','10:30,0,0,','10:45,0,0,','11:00,0,0,','11:15,0,0,','11:30,0,0,','11:45,0,0,','12:00,0,0,','12:15,0,0,','12:30,0,0,','12:45,0,0,','13:00,0,0,','13:15,0,0,','13:30,0,0,','13:45,0,0,','14:00,0,0,','14:15,0,0,','14:30,0,0,','14:45,0,0,','15:00,0,0,','15:15,0,0,','15:30,0,0,','15:45,0,0,','16:00,0,0,','16:15,0,0,','16:30,0,0,','16:45,0,0,','17:00,0,0,','17:15,0,0,','17:30,0,0,','17:45,0,0,','18:00,0,0,','18:15,0,0,','18:30,0,0,','18:45,0,0,','19:00,0,0,','19:15,0,0,','19:30,0,0,','19:45,0,0,','20:00,0,0,','20:15,0,0,','20:30,0,0,','20:45,0,0,','21:00,0,0,','21:15,0,0,','21:30,0,0,','21:45,0,0,']

            int minNonClosed = int.MaxValue;
            int maxNonClosed = int.MinValue;

            foreach (string strDay in days)
            {
                Day d = new Day();
                d.Load(strDay);
                if (d.FirstNonClosed != d.LastNonClosed) // check if it is not closed the complete day...
                {
                    if (d.FirstNonClosed < minNonClosed)
                        minNonClosed = d.FirstNonClosed;
                    if (d.LastNonClosed > maxNonClosed)
                        maxNonClosed = d.LastNonClosed;
                }
                Days.Add(d);
            }

            // OK let's try to remove the closed zones... 


            foreach (Day d in Days)
            {
                if (maxNonClosed != int.MinValue)
                {
                    int dEntriesCount = d.Entries.Count;
                    for (int i = maxNonClosed; i < dEntriesCount ; i++)
                    {
                        d.Entries.RemoveAt(d.Entries.Count - 1);
                    }
                }

                if (minNonClosed != int.MaxValue)
                {
                    for (int i = 0; i < minNonClosed; i++)
                    {
                        d.Entries.RemoveAt(0);
                    }
                }
            }

            FirePropertyChanged("Loaded");
        }
        public void LoadPreviousDays()
        {
            GetDays(this, m_ContactId,m_QualificationId, Days[0].DayValue.AddDays(-7), null);
        }
        public void LoadNextDays()
        {
            GetDays(this, m_ContactId, m_QualificationId, Days[0].DayValue.AddDays(+7), null);
        }
    }
}
