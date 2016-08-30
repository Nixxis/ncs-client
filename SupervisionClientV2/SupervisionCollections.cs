using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Threading;
using Nixxis.Client;
using Nixxis.Client.Controls;
using System.Windows;

namespace Nixxis.ClientV2
{
    #region Converters
    public class NumericToIntConverterFactory : SupervisionConverterFactory
    {
        public override Converter<string, object> GetConverter()
        {
            return (x) => {
                int result;
                if (int.TryParse(x, out result))
                {
                    if (result == 0)
                        return 0;
                    else
                        return result / 100000;
                }
                System.Diagnostics.Trace.WriteLine(x, "xxxxxxxxx");
                return Int32.MaxValue; };
        }
    }
    public class NumericToFloatConverterFactory : SupervisionConverterFactory
    {
        public override Converter<string, object> GetConverter()
        {

            return (x) => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture) == 0 ? 0 : float.Parse(x, System.Globalization.CultureInfo.InvariantCulture) / 100000;
        }
    }
    public class NumericToDoubleConverterFactory : SupervisionConverterFactory
    {
        public override Converter<string, object> GetConverter()
        {
            return (x) => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture) == 0 ? 0 : double.Parse(x, System.Globalization.CultureInfo.InvariantCulture) / 100000;
        }
    }
    #endregion

    #region Shared
    public class ProductionValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0, ConverterType = typeof(NumericToIntConverterFactory))]
        public int PositiveCount { get; internal set; }

        [SupervisionData(RawDataIndex = 1, ConverterType = typeof(NumericToIntConverterFactory))]
        public int PositiveSum { get; internal set; }

        [SupervisionData(RawDataIndex = 2, ConverterType = typeof(NumericToIntConverterFactory))]
        public int NegativeCount { get; internal set; }

        [SupervisionData(RawDataIndex = 3, ConverterType = typeof(NumericToIntConverterFactory))]
        public int NegativeSum { get; internal set; }

        [SupervisionData(RawDataIndex = 4, ConverterType = typeof(NumericToIntConverterFactory))]
        public int ArguedCount { get; internal set; }

        [SupervisionData(RawDataIndex = 5, ConverterType = typeof(NumericToIntConverterFactory))]
        public int TotalQualified { get; internal set; }

        [SupervisionData(RawDataIndex = 6, ConverterType = typeof(NumericToIntConverterFactory))]
        public int TotalNotQualified { get; internal set; }

        [SupervisionData(RawDataIndex = 7, TimeUnit=TimeUnit.MilliSeconds)]
        public TimeSpan DialingActionTime { get; internal set; }

        [SupervisionData(RawDataIndex = 8, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan DialingActionPosTime { get; internal set; }

        [SupervisionData(RawDataIndex = 9, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan DialingActionNegTime { get; internal set; }

        [SupervisionData(RawDataIndex = 10, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan DialingActionArgTime { get; internal set; }

        [SupervisionData(RawDataIndex = 11, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnlineActionTime { get; internal set; }

        [SupervisionData(RawDataIndex = 12, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnlineActionPosTime { get; internal set; }

        [SupervisionData(RawDataIndex = 13, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnlineActionNegTime { get; internal set; }

        [SupervisionData(RawDataIndex = 14, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnlineActionArgTime { get; internal set; }

        [SupervisionData(RawDataIndex = 15, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnHoldActionTime { get; internal set; }

        [SupervisionData(RawDataIndex = 16, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnHoldActionPosTime { get; internal set; }

        [SupervisionData(RawDataIndex = 17, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnHoldActionNegTime { get; internal set; }

        [SupervisionData(RawDataIndex = 18, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan OnHoldActionArgTime { get; internal set; }

        [SupervisionData(RawDataIndex = 19, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WrapUpActionTime { get; internal set; }

        [SupervisionData(RawDataIndex = 20, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WrapUpActionPosTime { get; internal set; }

        [SupervisionData(RawDataIndex = 21, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WrapUpActionNegTime { get; internal set; }

        [SupervisionData(RawDataIndex = 22, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WrapUpActionArgTime { get; internal set; }

        [SupervisionData(RawDataIndex = 23, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan CompletedContacts { get; internal set; }

        [SupervisionData(RawDataIndex = 24, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan CompletedContactsTime { get; internal set; }

        [SupervisionData(RawDataIndex = 25, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan CommunicationActionTime { get; internal set; }

        [SupervisionData(RawDataIndex = 26, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan CommunicationActionPosTime { get; internal set; }

        [SupervisionData(RawDataIndex = 27, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan CommunicationActionNegTime { get; internal set; }

        [SupervisionData(RawDataIndex = 28, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan CommunicationActionArgTime { get; internal set; }

        [SupervisionData(RawDataIndex = 29, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WorkActionTime { get; internal set; }

        [SupervisionData(RawDataIndex = 30, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WorkActionPosTime { get; internal set; }

        [SupervisionData(RawDataIndex = 31, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WorkActionNegTime { get; internal set; }

        [SupervisionData(RawDataIndex = 32, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan WorkActionArgTime { get; internal set; }

        [SupervisionData(RawDataIndex = 33, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgArgComTime { get; internal set; }

        [SupervisionData(RawDataIndex = 34, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgArgWorkTime { get; internal set; }

        [SupervisionData(RawDataIndex = 35, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgPosComTime { get; internal set; }

        [SupervisionData(RawDataIndex = 36, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgPosWorkTime { get; internal set; }

        [SupervisionData(RawDataIndex = 37, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgNegComTime { get; internal set; }

        [SupervisionData(RawDataIndex = 38, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgNegWorkTime { get; internal set; }

        [SupervisionData(RawDataIndex = 39, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioTotQual_Tot { get; internal set; }

        [SupervisionData(RawDataIndex = 40, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioArg_Tot { get; internal set; }

        [SupervisionData(RawDataIndex = 41, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioArg_TotQual { get; internal set; }

        [SupervisionData(RawDataIndex = 42, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioPos_Tot { get; internal set; }

        [SupervisionData(RawDataIndex = 43, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioPos_TotQual { get; internal set; }

        [SupervisionData(RawDataIndex = 44, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioPos_Arg { get; internal set; }

        [SupervisionData(RawDataIndex = 45, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioNeg_Tot { get; internal set; }

        [SupervisionData(RawDataIndex = 46, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RatioNeg_TotQual { get; internal set; }

        [SupervisionData(RawDataIndex = 47, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float RationNeg_Arg { get; internal set; }

        [SupervisionData(RawDataIndex = 48, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float AvgPosValue { get; internal set; }

        [SupervisionData(RawDataIndex = 49, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float AvgNegValue { get; internal set; }

        [SupervisionData(RawDataIndex = 50, ConverterType = typeof(NumericToIntConverterFactory))]
        public int sys_Reset { get; internal set; }

        [SupervisionData(RawDataIndex = 51, ConverterType = typeof(NumericToIntConverterFactory))]
        public int AgentHandledCount { get; internal set; }

        [SupervisionData(RawDataIndex = 52, TimeUnit = TimeUnit.MilliSeconds)]
        public TimeSpan AvgComTime { get; internal set; }


    }

    public class ContactListInfoValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int ContactCount { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public int ContactToDial { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int ContactNeverDialed { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public int ContactCallbacks { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int ContactToRedial { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public int ContactToNotRedial { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int AvgQuotaProgress { get; internal set; }

    }

    public class ContactListInfoTotal : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext();

        private int m_ContactCount = 0;
        private int m_ContactToDial = 0;
        private int m_ContactNeverDialed = 0;
        private int m_ContactCallbacks = 0;
        private int m_ContactToRedial = 0;
        private int m_ContactToNotRedial = 0;
        
        private ChartPieces m_ContactInfoGraph;

        public int ContactCount
        {
            get { return m_ContactCount; }
            set
            {
                m_ContactCount = value;
                FirePropertyChanged("ContactCount");
                m_ContactInfoGraph.UpdateValue("ContactCount", value);
            }
        }
        public int ContactToDial
        {
            get { return m_ContactToDial; }
            set
            {
                m_ContactToDial = value;
                FirePropertyChanged("ContactToDial");
                m_ContactInfoGraph.UpdateValue("ContactToDial", value);
            }
        }
        public int ContactNeverDialed
        {
            get { return m_ContactNeverDialed; }
            set
            {
                m_ContactNeverDialed = value;
                FirePropertyChanged("ContactNeverDialed");
                m_ContactInfoGraph.UpdateValue("ContactNeverDialed", value);
            }
        }
        public int ContactCallbacks
        {
            get { return m_ContactCallbacks; }
            set
            {
                m_ContactCallbacks = value;
                FirePropertyChanged("ContactCallbacks");
                m_ContactInfoGraph.UpdateValue("ContactCallbacks", value);
            }
        }
        public int ContactToRedial
        {
            get { return m_ContactToRedial; }
            set
            {
                m_ContactToRedial = value;
                FirePropertyChanged("ContactToRedial");
                m_ContactInfoGraph.UpdateValue("ContactToRedial", value);
            }
        }
        public int ContactToNotRedial
        {
            get { return m_ContactToNotRedial; }
            set
            {
                m_ContactToNotRedial = value;
                FirePropertyChanged("ContactToNotRedial");
                m_ContactInfoGraph.UpdateValue("ContactToNotRedial", value);
            }
        }
        
        public ChartPieces ContactInfoGraph
        {
            get { return m_ContactInfoGraph; }
            set { m_ContactInfoGraph = value; FirePropertyChanged("ContactInfoGraph"); }
        }
        public ContactListInfoTotal()
        {
            m_ContactInfoGraph = new ChartPieces();
            m_ContactInfoGraph.Add(new ChartPiece { Key = "ContactNeverDialed", Name = TranslationContext.Translate("Never Dialed"), Value = 0, Color = "#82ec5c" });
            m_ContactInfoGraph.Add(new ChartPiece { Key = "ContactToRedial", Name = TranslationContext.Translate("To Redial"), Value = 0, Color = "#e8ec5c" });
            m_ContactInfoGraph.Add(new ChartPiece { Key = "ContactCallbacks", Name = TranslationContext.Translate("Callbacks"), Value = 0, Color = "#ec9c5c" });
            m_ContactInfoGraph.Add(new ChartPiece { Key = "ContactToNotRedial", Name = TranslationContext.Translate("To Not Redial"), Value = 0, Color = "#608df8" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
    #endregion

    #region Agent
    #region Default Items
    public class AgentRealTimeValues : SupervisionDataCollection
	{
        [SupervisionData(RawDataIndex = 0)]
        public int StatusIndex { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Status { get; internal set; }

        [SupervisionData(RawDataIndex = 2, TimeUnit = TimeUnit.Seconds, LiveUpdate = true)]
        public TimeSpan StatusStartTime { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public string VoiceState { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public bool VoiceAvailable { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public string ChatState { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public bool ChatAvailable { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public string EmailState { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public bool EmailAvailable { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public string[] ListCurrentContactId { get; internal set; }

        [SupervisionData(RawDataIndex = 10)]
        public string ActiveContactId { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public int PerQueWaiting { get; internal set; }

        [SupervisionData(RawDataIndex = 12)]
        public int PerQueAbandoned { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public int PerQueProcessed { get; internal set; }

        [SupervisionData(RawDataIndex = 14)]
        public int PerQueEnQueue { get; internal set; }

        [SupervisionData(RawDataIndex = 15)]
        public int AlertLevel { get; internal set; }


        [SupervisionData(RawDataIndex = 16)]
        public int NumericCustom1 { get; internal set; }
        [SupervisionData(RawDataIndex = 17)]
        public int NumericCustom2 { get; internal set; }
        [SupervisionData(RawDataIndex = 18)]
        public int NumericCustom3 { get; internal set; }
        [SupervisionData(RawDataIndex = 19)]
        public int NumericCustom4 { get; internal set; }
        [SupervisionData(RawDataIndex = 20)]
        public int NumericCustom5 { get; internal set; }
        [SupervisionData(RawDataIndex = 21)]
        public int NumericCustom6 { get; internal set; }
        [SupervisionData(RawDataIndex = 22)]
        public int NumericCustom7 { get; internal set; }
        [SupervisionData(RawDataIndex = 23)]
        public int NumericCustom8 { get; internal set; }
        [SupervisionData(RawDataIndex = 24)]
        public int NumericCustom9 { get; internal set; }
        [SupervisionData(RawDataIndex = 25)]
        public int NumericCustom10 { get; internal set; }

        private string m_MonitoringSupervisors;
        private string m_ListeningSupervisors;
        private string m_RecordingSupervisors;
        private bool m_IsRecording;
        private string m_ViewingSupervisors;


        [SupervisionData(RawDataIndex = 26)]
        public string MonitoringSupervisors
        {
            get
            {
                return m_MonitoringSupervisors;
            }
            internal set
            {
                m_MonitoringSupervisors = value;
                FirePropertyChanged("MonitoredOthers");
                FirePropertyChanged("MonitoredMe");
            }
        }

        [SupervisionData(RawDataIndex = 27)]
        public string RecordingSupervisors
        {
            get
            {
                return m_RecordingSupervisors;
            }
            internal set
            {
                m_RecordingSupervisors = value;
                FirePropertyChanged("RecordingOthers");
                FirePropertyChanged("RecordingMe");
            }
        }
        [SupervisionData(RawDataIndex = 28)]
        public bool IsRecording
        {
            get
            {
                return m_IsRecording;
            }
            internal set
            {
                m_IsRecording = value;
            }
        }
        [SupervisionData(RawDataIndex = 29)]
        public string ViewingSupervisors
        {
            get
            {
                return m_ViewingSupervisors;
            }
            internal set
            {
                m_ViewingSupervisors = value;
                FirePropertyChanged("ViewingOthers");
                FirePropertyChanged("ViewingMe");
            }
        }

        [SupervisionData(RawDataIndex = 30)]
        public string SupervisorsRequestingMonitor
        {
            get
            {
                return m_ListeningSupervisors;
            }
            internal set
            {
                m_ListeningSupervisors = value;
                FirePropertyChanged("RequestingMonitorOthers");
                FirePropertyChanged("RequestingMonitorMe");
            }
        }

        public bool RequestingMonitorMe
        {
            get
            {
                if (string.IsNullOrEmpty(SupervisorsRequestingMonitor))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return SupervisorsRequestingMonitor.Contains(strLoggedIn);

            }
        }


        public bool RequestingMonitorOthers
        {
            get
            {
                if (string.IsNullOrEmpty(SupervisorsRequestingMonitor))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return !SupervisorsRequestingMonitor.Contains(strLoggedIn);
            }
        }

        public bool ViewingOthers
        {
            get
            {
                if (string.IsNullOrEmpty(ViewingSupervisors))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return !ViewingSupervisors.Contains(strLoggedIn);
            }
        }


        public bool ViewingMe
        {
            get
            {
                if (string.IsNullOrEmpty(ViewingSupervisors))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return ViewingSupervisors.Contains(strLoggedIn);

            }
        }

        public bool MonitoredOthers
        {
            get
            {
                if(string.IsNullOrEmpty(MonitoringSupervisors))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return !MonitoringSupervisors.Contains(strLoggedIn);
            }
        }


        public bool MonitoredMe
        {
            get
            {
                if (string.IsNullOrEmpty(MonitoringSupervisors))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return MonitoringSupervisors.Contains(strLoggedIn);

            }
        }
        public bool RecordingOthers
        {
            get
            {
                if (string.IsNullOrEmpty(RecordingSupervisors))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return !RecordingSupervisors.Contains(strLoggedIn);
            }
        }


        public bool RecordingMe
        {
            get
            {
                if (string.IsNullOrEmpty(RecordingSupervisors))
                    return false;

                string strLoggedIn = Application.Current.MainWindow.GetType().GetProperty("LoggedIn").GetGetMethod().Invoke(Application.Current.MainWindow, null) as string;

                return RecordingSupervisors.Contains(strLoggedIn);

            }
        }

	}

	public class AgentHistoryValues : SupervisionDataCollection
	{
        [SupervisionData(RawDataIndex = 0)]
        public int Undefined { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public int Pause { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int Off { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public int Waiting { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int Wrapup { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public int WrapupInbound { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int WrapupOutbound { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public int WrapupEMail { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public int WrapupChat { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public int Online { get; internal set; }

        [SupervisionData(RawDataIndex = 10)]
        public int HandlingInbound { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public int HandlingOutbound { get; internal set; }

        [SupervisionData(RawDataIndex = 12)]
        public int HandlingEMail { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public int HandlingChat { get; internal set; }

        [SupervisionData(RawDataIndex = 14)]
        public int ContactHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 15)]
        public int ContactInboundHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 16)]
        public int ContactOutboundHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 17)]
        public int ContactEMailHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 18)]
        public int ContactChatHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 19)]
        public int ContactMsgSend { get; internal set; }

        [SupervisionData(RawDataIndex = 20)]
        public int ContactMsgReceived { get; internal set; }

        [SupervisionData(RawDataIndex = 21)]
        public TimeSpan UndefinedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 22)]
        public TimeSpan PauseTime { get; internal set; }

        [SupervisionData(RawDataIndex = 23)]
        public TimeSpan OffTime { get; internal set; }

        [SupervisionData(RawDataIndex = 24)]
        public TimeSpan WaitingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 25)]
        public TimeSpan WrapupStateTime { get; internal set; }

        [SupervisionData(RawDataIndex = 26)]
        public TimeSpan WrapupInboundTime { get; internal set; }

        [SupervisionData(RawDataIndex = 27)]
        public TimeSpan WrapupOutboundTime { get; internal set; }

        [SupervisionData(RawDataIndex = 28)]
        public TimeSpan WrapupEMailTime { get; internal set; }

        [SupervisionData(RawDataIndex = 29)]
        public TimeSpan WrapupChatTime { get; internal set; }

        [SupervisionData(RawDataIndex = 30)]
        public TimeSpan OnlineTime { get; internal set; }

        [SupervisionData(RawDataIndex = 31)]
        public TimeSpan HandlingInboundTime { get; internal set; }

        [SupervisionData(RawDataIndex = 32)]
        public TimeSpan HandlingOutboundTime { get; internal set; }

        [SupervisionData(RawDataIndex = 33)]
        public TimeSpan HandlingEMailTime { get; internal set; }

        [SupervisionData(RawDataIndex = 34)]
        public TimeSpan HandlingChatTime { get; internal set; }

        [SupervisionData(RawDataIndex = 35)]
        public TimeSpan ContactHandledTime { get; internal set; }

        [SupervisionData(RawDataIndex = 36)]
        public TimeSpan ContactInboundHandledTime { get; internal set; }

        [SupervisionData(RawDataIndex = 37)]
        public TimeSpan ContactOutboundHandledTime { get; internal set; }

        [SupervisionData(RawDataIndex = 38)]
        public TimeSpan ContactEMailHandledTime { get; internal set; }

        [SupervisionData(RawDataIndex = 39)]
        public TimeSpan ContactChatHandledTime { get; internal set; }

        [SupervisionData(RawDataIndex = 40)]
        public int Preview { get; internal set; }

        [SupervisionData(RawDataIndex = 41)]
        public TimeSpan PreviewTime { get; internal set; }
		
		public int AverageWaiting
		{
            get
            {
                return 0;
            }
            internal set
            {
                throw new NotSupportedException();
            }
		}
	}

    public class TeamConverterFactory : SupervisionConverterFactory
    {
        public override Converter<string, object> GetConverter()
        {
            return (x) =>
            {
                return x;
            };
        }
    }


    [SupervisionDescriptionProperty("Account")]
	public class AgentItem : SupervisionItem
	{
        [SupervisionData(RawDataIndex = 0)]
        public string AgentId { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Account { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public string Firstname { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public string Lastname { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public string SiteId { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public string Server { get; internal set; }

        [SupervisionData(RawDataIndex = 6, ArraySeparator = ';', ConverterType = typeof(TeamConverterFactory))]
        public string[] TeamId { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public string PauseId { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public string PauseDescription { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public string Groupkey { get; internal set; }

        [SupervisionData(RawDataIndex = 10)]
        public string Active { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public TimeSpan LoginDateTime { get; internal set; }

        [SupervisionData(RawDataIndex = 12)]
        public TimeSpan LoginDateTimeUtc { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public string IpAddress { get; internal set; }

        [SupervisionData(RawDataIndex = 14)]
        public string Extension { get; internal set; }

        [SupervisionAutoInitProperty]
        public AgentRealTimeValues RealTime { get; private set; }

        [SupervisionAutoInitProperty]
        public AgentHistoryValues History { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues Production { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues PeriodProduction { get; private set; }

        public AgentItem()
        {
            RealTime.PropertyChanged += new PropertyChangedEventHandler(RealTime_PropertyChanged);
        }

        public delegate void AgentStatusChangedDelegate(AgentItem item);
        public event AgentStatusChangedDelegate AgentStatusChanged;

        private void RealTime_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (AgentStatusChanged != null && "StatusIndex" == e.PropertyName)
            {
                AgentStatusChanged(this);
            }
        }
	}

	public class AgentList : SupervisionList<AgentItem>
	{
        private AgentTotal m_AgentTotal;
        public AgentTotal AgentTotal 
        {
            get { return m_AgentTotal; }
            set { m_AgentTotal = value; }
        }

        public AgentList()
            : base()
        {
            m_AgentTotal = new AgentTotal();
        }
	}
    #endregion

    #region Agent Total
    public class AgentRealTimeTotal : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext();
        private int m_StatePauseCount = 0;
        private int m_StateWaitingCount = 0;
        private int m_StateOnlineCount = 0;
        private int m_StateWrapUpCount = 0;
        private int m_StatePreviewCount = 0;
        private int m_StateSupervisionCount = 0;
        private int m_AlertLevel = 0;
        private int m_NumericCustom1 = 0;
        private int m_NumericCustom2 = 0;
        private int m_NumericCustom3 = 0;
        private int m_NumericCustom4 = 0;
        private int m_NumericCustom5 = 0;
        private int m_NumericCustom6 = 0;
        private int m_NumericCustom7 = 0;
        private int m_NumericCustom8 = 0;
        private int m_NumericCustom9 = 0;
        private int m_NumericCustom10 = 0;


        private ChartPieces m_AgentStateGraph;

        public int StatePauseCount
        {
            get { return m_StatePauseCount; }
            set { 
                m_StatePauseCount = value; 
                FirePropertyChanged("StatePauseCount");
                m_AgentStateGraph.UpdateValue("StatePauseCount", value);
            }
        }
        public int StateWaitingCount
        {
            get { return m_StateWaitingCount; }
            set
            {
                m_StateWaitingCount = value; 
                FirePropertyChanged("StateWaitingCount");
                m_AgentStateGraph.UpdateValue("StateWaitingCount", value);
            }
        }
        public int StateOnlineCount
        {
            get { return m_StateOnlineCount; }
            set 
            { 
                m_StateOnlineCount = value; 
                FirePropertyChanged("StateOnlineCount"); 
                m_AgentStateGraph.UpdateValue("StateOnlineCount", value); 
            }
        }
        public int StateWrapUpCount
        {
            get { return m_StateWrapUpCount; }
            set
            {
                m_StateWrapUpCount = value; 
                FirePropertyChanged("StateWrapUpCount");
                m_AgentStateGraph.UpdateValue("StateWrapUpCount", value);
            }
        }
        public int StatePreviewCount
        {
            get { return m_StatePreviewCount; }
            set
            {
                m_StatePreviewCount = value; 
                FirePropertyChanged("StatePreviewCount");
                m_AgentStateGraph.UpdateValue("StatePreviewCount", value);
            }
        }
        public int StateSupervisionCount
        {
            get { return m_StateSupervisionCount; }
            set
            {
                m_StateSupervisionCount = value;
                FirePropertyChanged("StateSupervisionCount");
                m_AgentStateGraph.UpdateValue("StateSupervisionCount", value);
            }
        }

        public int AlertLevel
        {
            get { return m_AlertLevel; }
            set
            {
                m_AlertLevel = value; FirePropertyChanged("AlertLevel");
            }
        }

        public int NumericCustom1
        {
            get { return m_NumericCustom1; }
            set
            {
                m_NumericCustom1 = value; FirePropertyChanged("NumericCustom1");
            }
        }
        public int NumericCustom2
        {
            get { return m_NumericCustom2; }
            set
            {
                m_NumericCustom2 = value; FirePropertyChanged("NumericCustom2");
            }
        }
        public int NumericCustom3
        {
            get { return m_NumericCustom3; }
            set
            {
                m_NumericCustom3 = value; FirePropertyChanged("NumericCustom3");
            }
        }
        public int NumericCustom4
        {
            get { return m_NumericCustom4; }
            set
            {
                m_NumericCustom4 = value; FirePropertyChanged("NumericCustom4");
            }
        }
        public int NumericCustom5
        {
            get { return m_NumericCustom5; }
            set
            {
                m_NumericCustom5 = value; FirePropertyChanged("NumericCustom5");
            }
        }
        public int NumericCustom6
        {
            get { return m_NumericCustom6; }
            set
            {
                m_NumericCustom6 = value; FirePropertyChanged("NumericCustom6");
            }
        }
        public int NumericCustom7
        {
            get { return m_NumericCustom7; }
            set
            {
                m_NumericCustom7 = value; FirePropertyChanged("NumericCustom7");
            }
        }
        public int NumericCustom8
        {
            get { return m_NumericCustom8; }
            set
            {
                m_NumericCustom8 = value; FirePropertyChanged("NumericCustom8");
            }
        }
        public int NumericCustom9
        {
            get { return m_NumericCustom9; }
            set
            {
                m_NumericCustom9 = value; FirePropertyChanged("NumericCustom9");
            }
        }
        public int NumericCustom10
        {
            get { return m_NumericCustom10; }
            set
            {
                m_NumericCustom10 = value; FirePropertyChanged("NumericCustom10");
            }
        }

        public ChartPieces AgentStateGraph
        {
            get { return m_AgentStateGraph; }
            set { m_AgentStateGraph = value; FirePropertyChanged("AgentStateGraph"); }
        }
        public AgentRealTimeTotal()
        {
            m_AgentStateGraph = new ChartPieces();
            m_AgentStateGraph.Add(new ChartPiece { Key = "StatePauseCount", Name = TranslationContext.Translate("Pause"), Value = 0, Color = "#f84343" });
            m_AgentStateGraph.Add(new ChartPiece { Key = "StateWaitingCount", Name = TranslationContext.Translate("Waiting"), Value = 0, Color = "#fa9654" });
            m_AgentStateGraph.Add(new ChartPiece { Key = "StateOnlineCount", Name = TranslationContext.Translate("Online"), Value = 0, Color = "#b4e93c" });
            m_AgentStateGraph.Add(new ChartPiece { Key = "StateWrapUpCount", Name = TranslationContext.Translate("WrapUp"), Value = 0, Color = "#fff564" });
            m_AgentStateGraph.Add(new ChartPiece { Key = "StatePreviewCount", Name = TranslationContext.Translate("Preview"), Value = 0, Color = "#2ccdf2" });
            m_AgentStateGraph.Add(new ChartPiece { Key = "StateSupervisionCount", Name = TranslationContext.Translate("Supervision"), Value = 0, Color = "#d554f4" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }


    }
    public class AgentTotal
    {
        private AgentRealTimeTotal m_RealTime = new AgentRealTimeTotal();

        public AgentRealTimeTotal RealTime
        {
            get { return m_RealTime; }
            set { m_RealTime = value; }
        }

    }
    #endregion
    #endregion

    #region Inbound
    #region Default
    public class InboundRealTimeValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int ActiveContacts { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public int SystemPreprocessing { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int Closing { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public int Ivr { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int Waiting { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public int Online { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int WrapUp { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public int Overflowing { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public int Transfer { get; internal set; }

        [SupervisionData(RawDataIndex = 9,TimeUnit=TimeUnit.Seconds, LiveUpdate=true)]
        public TimeSpan MaxQueueTime { get; internal set; }

        [SupervisionData(RawDataIndex = 10)]
        public int AgentInReady { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public int ContactMsgSend { get; internal set; }

        [SupervisionData(RawDataIndex = 12)]
        public int ContactMsgReceived { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public int AlertLevel { get; internal set; }


        [SupervisionData(RawDataIndex = 14)]
        public int NumericCustom1 { get; internal set; }
        [SupervisionData(RawDataIndex = 15)]
        public int NumericCustom2 { get; internal set; }
        [SupervisionData(RawDataIndex = 16)]
        public int NumericCustom3 { get; internal set; }
        [SupervisionData(RawDataIndex = 17)]
        public int NumericCustom4 { get; internal set; }
        [SupervisionData(RawDataIndex = 18)]
        public int NumericCustom5 { get; internal set; }
        [SupervisionData(RawDataIndex = 19)]
        public int NumericCustom6 { get; internal set; }
        [SupervisionData(RawDataIndex = 20)]
        public int NumericCustom7 { get; internal set; }
        [SupervisionData(RawDataIndex = 21)]
        public int NumericCustom8 { get; internal set; }
        [SupervisionData(RawDataIndex = 22)]
        public int NumericCustom9 { get; internal set; }
        [SupervisionData(RawDataIndex = 23)]
        public int NumericCustom10 { get; internal set; }

    }

    public class InboundHistoryValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int Received { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public TimeSpan ReceivedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int Closed { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public TimeSpan ClosedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int EndInSystemProcessing { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public TimeSpan EndInSystemProcessingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int EndInIvr { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public TimeSpan EndInIvrTime { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public int IvrFinish { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public TimeSpan IvrFinishTime { get; internal set; }

        [SupervisionData(RawDataIndex = 10)]
        public int IvrAbandoned { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public TimeSpan IvrAbandonedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 12)]
        public int Abandoned { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public TimeSpan AbandonedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 14)]
        public int HandledByAgent { get; internal set; }

        [SupervisionData(RawDataIndex = 15)]
        public TimeSpan HandledByAgentTime { get; internal set; }

        [SupervisionData(RawDataIndex = 16)]
        public int OverflowedCount { get; internal set; }

        [SupervisionData(RawDataIndex = 17)]
        public int OverflowedContact { get; internal set; }

        [SupervisionData(RawDataIndex = 18)]
        public int Waiting { get; internal set; }

        [SupervisionData(RawDataIndex = 19)]
        public TimeSpan WaitingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 20)]
        public int Direct { get; internal set; }

        [SupervisionData(RawDataIndex = 21)]
        public TimeSpan DirectTime { get; internal set; }

        [SupervisionData(RawDataIndex = 22)]
        public int Transfer { get; internal set; }

        [SupervisionData(RawDataIndex = 23)]
        public TimeSpan TransferTime { get; internal set; }

        [SupervisionData(RawDataIndex = 24)]
        public int ContactMsgSend { get; internal set; }

        [SupervisionData(RawDataIndex = 25)]
        public int ContactMsgReceived { get; internal set; }

        [SupervisionData(RawDataIndex = 26)]
        public int ContactsHandledInSLATime { get; internal set; }

        [SupervisionData(RawDataIndex = 27, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float PercentageContactsHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 28, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float PercentageContactsHandledInTime { get; internal set; }
    }

    [SupervisionDescriptionProperty("Description")]
    public class InboundItem : SupervisionItem
    {
        [SupervisionData(RawDataIndex = 0)]
        public string Id { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Description { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public string GroupKey { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public string CampaignId { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public string CampaignName { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public string MediaType { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int MediaTypeId { get; internal set; }

        [SupervisionAutoInitProperty]
        public InboundRealTimeValues RealTime { get; private set; }

        [SupervisionAutoInitProperty]
        public InboundHistoryValues History { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues Production { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues PeriodProduction { get; private set; }
    }

    public class InboundList : SupervisionList<InboundItem>
    {
        private InboundTotal m_InboundTotal;
        public InboundTotal InboundTotal 
        {
            get { return m_InboundTotal; }
            set { m_InboundTotal = value; }
        }

        public InboundList()
            : base()
        {
            InboundTotal = new InboundTotal();
        }
    }
    #endregion

    #region Inbound Total
    public class InboundRealTimeTotal : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext();
        private int m_Closing = 0;
        private int m_Ivr = 0;
        private int m_Waiting = 0;
        private int m_Online = 0;
        private int m_WrapUp = 0;
        private int m_Overflowing = 0;
        private int m_AlertLevel = 0;
        private int m_NumericCustom1 = 0;
        private int m_NumericCustom2 = 0;
        private int m_NumericCustom3 = 0;
        private int m_NumericCustom4 = 0;
        private int m_NumericCustom5 = 0;
        private int m_NumericCustom6 = 0;
        private int m_NumericCustom7 = 0;
        private int m_NumericCustom8 = 0;
        private int m_NumericCustom9 = 0;
        private int m_NumericCustom10 = 0;

        private ChartPieces m_RealTimeGraph;

        public int Closing
        {
            get { return m_Closing; }
            set { m_Closing = value; FirePropertyChanged("Closing"); }
        }
        public int Ivr
        {
            get { return m_Ivr; }
            set
            {
                m_Ivr = value; FirePropertyChanged("Ivr");
                m_RealTimeGraph.UpdateValue("Ivr", value);
            }
        }
        public int Waiting
        {
            get { return m_Waiting; }
            set
            {
                m_Waiting = value;
                FirePropertyChanged("Waiting");
                m_RealTimeGraph.UpdateValue("Waiting", value);
            }
        }
        public int Online
        {
            get { return m_Online; }
            set
            {
                m_Online = value;
                FirePropertyChanged("Online");
                m_RealTimeGraph.UpdateValue("Online", value);
            }
        }
        public int WrapUp
        {
            get { return m_WrapUp; }
            set
            {
                m_WrapUp = value;
                FirePropertyChanged("WrapUp");
                m_RealTimeGraph.UpdateValue("WrapUp", value);
            }
        }
        public int Overflowing
        {
            get { return m_Overflowing; }
            set
            {
                m_Overflowing = value; FirePropertyChanged("Overflowing");
                m_RealTimeGraph.UpdateValue("Overflowing", value);
            }
        }
        public int AlertLevel
        {
            get { return m_AlertLevel; }
            set
            {
                m_AlertLevel = value; FirePropertyChanged("AlertLevel");
            }
        }

        public int NumericCustom1
        {
            get { return m_NumericCustom1; }
            set
            {
                m_NumericCustom1 = value; FirePropertyChanged("NumericCustom1");
            }
        }
        public int NumericCustom2
        {
            get { return m_NumericCustom2; }
            set
            {
                m_NumericCustom2 = value; FirePropertyChanged("NumericCustom2");
            }
        }
        public int NumericCustom3
        {
            get { return m_NumericCustom3; }
            set
            {
                m_NumericCustom3 = value; FirePropertyChanged("NumericCustom3");
            }
        }
        public int NumericCustom4
        {
            get { return m_NumericCustom4; }
            set
            {
                m_NumericCustom4 = value; FirePropertyChanged("NumericCustom4");
            }
        }
        public int NumericCustom5
        {
            get { return m_NumericCustom5; }
            set
            {
                m_NumericCustom5 = value; FirePropertyChanged("NumericCustom5");
            }
        }
        public int NumericCustom6
        {
            get { return m_NumericCustom6; }
            set
            {
                m_NumericCustom6 = value; FirePropertyChanged("NumericCustom6");
            }
        }
        public int NumericCustom7
        {
            get { return m_NumericCustom7; }
            set
            {
                m_NumericCustom7 = value; FirePropertyChanged("NumericCustom7");
            }
        }
        public int NumericCustom8
        {
            get { return m_NumericCustom8; }
            set
            {
                m_NumericCustom8 = value; FirePropertyChanged("NumericCustom8");
            }
        }
        public int NumericCustom9
        {
            get { return m_NumericCustom9; }
            set
            {
                m_NumericCustom9 = value; FirePropertyChanged("NumericCustom9");
            }
        }
        public int NumericCustom10
        {
            get { return m_NumericCustom10; }
            set
            {
                m_NumericCustom10 = value; FirePropertyChanged("NumericCustom10");
            }
        }


        public ChartPieces RealTimeGraph
        {
            get { return m_RealTimeGraph; }
            set { m_RealTimeGraph = value; FirePropertyChanged("RealTimeGraph"); }
        }
        public InboundRealTimeTotal()
        {
            m_RealTimeGraph = new ChartPieces();
            m_RealTimeGraph.Add(new ChartPiece { Key = "Ivr", Name = TranslationContext.Translate("Ivr"), Value = 0, Color = "#99d6e6" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "Waiting", Name = TranslationContext.Translate("Waiting"), Value = 0, Color = "#fa9654" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "Online", Name = TranslationContext.Translate("Online"), Value = 0, Color = "#b4e93c" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "WrapUp", Name = TranslationContext.Translate("WrapUp"), Value = 0, Color = "#fff564" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "Overflowing", Name = TranslationContext.Translate("Overflowing"), Value = 0, Color = "#f84343" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }


    }
    
    public class InboundTotal
    {
        private InboundRealTimeTotal m_RealTime = new InboundRealTimeTotal();

        public InboundRealTimeTotal RealTime
        {
            get { return m_RealTime; }
            set { m_RealTime = value; }
        }

    }
    #endregion
    #endregion

    #region Chart Info
    public class ChartPieces : ObservableCollection<ChartPiece>
    {
        public void UpdateValue(string key, int newValue)
        {
            foreach(ChartPiece item in this.Items)
            {
                if (item.Key == key)
                {
                    item.Value = newValue;
                    break;
                }
            }
        }
    }
    
    public class ChartPiece : INotifyPropertyChanged
    {
        private string m_Name;
        private string m_Key;
        private int m_Value;
        private string m_Color;

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; FirePropertyChanged("Name"); }
        }
        public string Key
        {
            get { return m_Key; }
            set { m_Key = value;  }
        }
        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; FirePropertyChanged("Value"); }
        }
        public string Color
        {
            get { return m_Color; }
            set { m_Color = value; FirePropertyChanged("Color"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            try
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.WriteLine(error.ToString());
            }
        }
    }
    #endregion

    #region Outbound
    #region Default
    public class OutboundRealTimeValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int SystemPreprocessing { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public int Closing { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int Ivr { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public int Waiting { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int Online { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public int WrapUp { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int Transfer { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public int Overflowing { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public int Preview { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public int AlertLevel { get; internal set; }


        [SupervisionData(RawDataIndex = 10)]
        public int NumericCustom1 { get; internal set; }
        [SupervisionData(RawDataIndex = 11)]
        public int NumericCustom2 { get; internal set; }
        [SupervisionData(RawDataIndex = 12)]
        public int NumericCustom3 { get; internal set; }
        [SupervisionData(RawDataIndex = 13)]
        public int NumericCustom4 { get; internal set; }
        [SupervisionData(RawDataIndex = 14)]
        public int NumericCustom5 { get; internal set; }
        [SupervisionData(RawDataIndex = 15)]
        public int NumericCustom6 { get; internal set; }
        [SupervisionData(RawDataIndex = 16)]
        public int NumericCustom7 { get; internal set; }
        [SupervisionData(RawDataIndex = 17)]
        public int NumericCustom8 { get; internal set; }
        [SupervisionData(RawDataIndex = 18)]
        public int NumericCustom9 { get; internal set; }
        [SupervisionData(RawDataIndex = 19)]
        public int NumericCustom10 { get; internal set; }

    }

    public class OutboundHistoryValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int Dialled { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public TimeSpan DialledTime { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int EndInSystemProcessing { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public TimeSpan EndInSystemProcessingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int EndInIvr { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public TimeSpan EndInIvrTime { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int IvrFinish { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public TimeSpan IvrFinishTime { get; internal set; }

        [SupervisionData(RawDataIndex = 8)]
        public int IvrAbandoned { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public TimeSpan IvrAbandonedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 10)]
        public int Abandoned { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public TimeSpan AbandonedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 12)]
        public int OverflowCount { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public int OverflowContact { get; internal set; }

        [SupervisionData(RawDataIndex = 14)]
        public int ToAgent { get; internal set; }

        [SupervisionData(RawDataIndex = 15)]
        public TimeSpan ToAgentTime { get; internal set; }

        [SupervisionData(RawDataIndex = 16)]
        public int Direct { get; internal set; }

        [SupervisionData(RawDataIndex = 17)]
        public TimeSpan DirectTime { get; internal set; }

        [SupervisionData(RawDataIndex = 18)]
        public int Waiting { get; internal set; }

        [SupervisionData(RawDataIndex = 19)]
        public TimeSpan WaitingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 20)]
        public int Transfer { get; internal set; }

        [SupervisionData(RawDataIndex = 21)]
        public TimeSpan TransferTime { get; internal set; }
    }

    public class OutboundRemarksValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public string DialRemark { get; internal set; }
    }

    [SupervisionDescriptionProperty("Description")]
    public class OutboundItem : SupervisionItem
    {

        [SupervisionData(RawDataIndex = 0)]
        public string Id { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Description { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public string GroupKey { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public string CampaignId { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public string CampaignName { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public string MediaType { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int MediaTypeId { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public string Mode { get; internal set; }


        [SupervisionData(RawDataIndex = 8)]
        public bool Running { get; internal set; }

        [SupervisionAutoInitProperty]
        public OutboundRealTimeValues RealTime { get; private set; }

        [SupervisionAutoInitProperty]
        public OutboundHistoryValues History { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues Production { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues PeriodProduction { get; private set; }

        [SupervisionAutoInitProperty]
        public ContactListInfoValues ContactListInfo { get; private set; }

        [SupervisionAutoInitProperty]
        public OutboundRemarksValues Remarks { get; private set; }
    }

    public class OutboundList : SupervisionList<OutboundItem>
    {        
        private OutboundTotal m_OutboundTotal;
        public OutboundTotal OutboundTotal 
        {
            get { return m_OutboundTotal; }
            set { m_OutboundTotal = value; }
        }

        public OutboundList()
            : base()
        {
            OutboundTotal = new OutboundTotal();
        }
    }
    #endregion

    #region Total Outbound
    public class OutboundRealTimeTotal : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext();  
        private int m_SystemPreprocessing = 0;
        private int m_Closing = 0;
        private int m_Ivr = 0;
        private int m_Waiting = 0;
        private int m_Online = 0;
        private int m_WrapUp = 0;
        private int m_Transfer = 0;
        private int m_Overflowing = 0;
        private int m_Preview = 0;
        private int m_AlertLevel = 0;
        private int m_NumericCustom1 = 0;
        private int m_NumericCustom2 = 0;
        private int m_NumericCustom3 = 0;
        private int m_NumericCustom4 = 0;
        private int m_NumericCustom5 = 0;
        private int m_NumericCustom6 = 0;
        private int m_NumericCustom7 = 0;
        private int m_NumericCustom8 = 0;
        private int m_NumericCustom9 = 0;
        private int m_NumericCustom10 = 0;


        private ChartPieces m_RealTimeGraph;

        public int SystemPreprocessing
        {
            get { return m_SystemPreprocessing; }
            set
            {
                m_SystemPreprocessing = value;
                FirePropertyChanged("SystemPreprocessing");
                m_RealTimeGraph.UpdateValue("SystemPreprocessing", value);
            }
        }
        public int Closing
        {
            get { return m_Closing; }
            set
            {
                m_Closing = value;
                FirePropertyChanged("Closing");
                m_RealTimeGraph.UpdateValue("Closing", value);
            }
        }
        public int Ivr
        {
            get { return m_Ivr; }
            set
            {
                m_Ivr = value;
                FirePropertyChanged("Ivr");
                m_RealTimeGraph.UpdateValue("Ivr", value);
            }
        }
        public int Waiting
        {
            get { return m_Waiting; }
            set
            {
                m_Waiting = value;
                FirePropertyChanged("Waiting");
                m_RealTimeGraph.UpdateValue("Waiting", value);
            }
        }
        public int Online
        {
            get { return m_Online; }
            set
            {
                m_Online = value;
                FirePropertyChanged("Online");
                m_RealTimeGraph.UpdateValue("Online", value);
            }
        }
        public int WrapUp
        {
            get { return m_WrapUp; }
            set
            {
                m_WrapUp = value;
                FirePropertyChanged("WrapUp");
                m_RealTimeGraph.UpdateValue("WrapUp", value);
            }
        }
        public int Transfer
        {
            get { return m_Transfer; }
            set
            {
                m_Transfer = value;
                FirePropertyChanged("Transfer");
                m_RealTimeGraph.UpdateValue("Transfer", value);
            }
        }
        public int Overflowing
        {
            get { return m_Overflowing; }
            set
            {
                m_Overflowing = value;
                FirePropertyChanged("Overflowing");
                m_RealTimeGraph.UpdateValue("Overflowing", value);
            }
        }
        public int Preview
        {
            get { return m_Preview; }
            set
            {
                m_Preview = value;
                FirePropertyChanged("Preview");
                m_RealTimeGraph.UpdateValue("Preview", value);
            }
        }
        public int AlertLevel
        {
            get { return m_AlertLevel; }
            set
            {
                m_AlertLevel = value; FirePropertyChanged("AlertLevel");
            }
        }

        public int NumericCustom1
        {
            get { return m_NumericCustom1; }
            set
            {
                m_NumericCustom1 = value; FirePropertyChanged("NumericCustom1");
            }
        }
        public int NumericCustom2
        {
            get { return m_NumericCustom2; }
            set
            {
                m_NumericCustom2 = value; FirePropertyChanged("NumericCustom2");
            }
        }
        public int NumericCustom3
        {
            get { return m_NumericCustom3; }
            set
            {
                m_NumericCustom3 = value; FirePropertyChanged("NumericCustom3");
            }
        }
        public int NumericCustom4
        {
            get { return m_NumericCustom4; }
            set
            {
                m_NumericCustom4 = value; FirePropertyChanged("NumericCustom4");
            }
        }
        public int NumericCustom5
        {
            get { return m_NumericCustom5; }
            set
            {
                m_NumericCustom5 = value; FirePropertyChanged("NumericCustom5");
            }
        }
        public int NumericCustom6
        {
            get { return m_NumericCustom6; }
            set
            {
                m_NumericCustom6 = value; FirePropertyChanged("NumericCustom6");
            }
        }
        public int NumericCustom7
        {
            get { return m_NumericCustom7; }
            set
            {
                m_NumericCustom7 = value; FirePropertyChanged("NumericCustom7");
            }
        }
        public int NumericCustom8
        {
            get { return m_NumericCustom8; }
            set
            {
                m_NumericCustom8 = value; FirePropertyChanged("NumericCustom8");
            }
        }
        public int NumericCustom9
        {
            get { return m_NumericCustom9; }
            set
            {
                m_NumericCustom9 = value; FirePropertyChanged("NumericCustom9");
            }
        }
        public int NumericCustom10
        {
            get { return m_NumericCustom10; }
            set
            {
                m_NumericCustom10 = value; FirePropertyChanged("NumericCustom10");
            }
        }

        public ChartPieces RealTimeGraph
        {
            get { return m_RealTimeGraph; }
            set { m_RealTimeGraph = value; FirePropertyChanged("OutboundRealTimeGraph"); }
        }
        public OutboundRealTimeTotal()
        {
            m_RealTimeGraph = new ChartPieces();
            m_RealTimeGraph.Add(new ChartPiece { Key = "SystemPreprocessing", Name = TranslationContext.Translate("Dialling"), Value = 0, Color = "#faf39d" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "Waiting", Name = TranslationContext.Translate("Waiting"), Value = 0, Color = "#fa9654" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "Online", Name = TranslationContext.Translate("Online"), Value = 0, Color = "#b4e93c" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "WrapUp", Name = TranslationContext.Translate("WrapUp"), Value = 0, Color = "#fff564" });
            m_RealTimeGraph.Add(new ChartPiece { Key = "Preview", Name = TranslationContext.Translate("Preview"), Value = 0, Color = "#2ccdf2" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
    
    public class OutboundTotal : INotifyPropertyChanged
    {        
        private int m_Count = 0;
        private int m_ModePreview = 0;
        private int m_ModeProgressive = 0;
        private int m_ModePredictive = 0;
        private int m_ModeOthers = 0;
        private OutboundRealTimeTotal m_RealTime = new OutboundRealTimeTotal();

        public int Count
        {
            get { return m_Count; }
            set
            {
                m_Count = value;
                FirePropertyChanged("Count");
            }
        }
        public int ModePreview
        {
            get { return m_ModePreview; }
            set
            {
                m_ModePreview = value;
                FirePropertyChanged("ModePreview");
            }
        }
        public int ModeProgressive
        {
            get { return m_ModeProgressive; }
            set
            {
                m_ModeProgressive = value;
                FirePropertyChanged("ModeProgressive");
            }
        }
        public int ModePredictive
        {
            get { return m_ModePredictive; }
            set
            {
                m_ModePredictive = value;
                FirePropertyChanged("ModePredictive");
            }
        }
        public int ModeOthers
        {
            get { return m_ModeOthers; }
            set
            {
                m_ModeOthers = value;
                FirePropertyChanged("ModeOthers");
            }
        }
        public OutboundRealTimeTotal RealTime
        {
            get { return m_RealTime; }
            set { m_RealTime = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
    #endregion
    #endregion

    #region Campaign
    #region Default

    [SupervisionDescriptionProperty("Description")]
    public class CampaignItem : SupervisionItem
    {
        [SupervisionData(RawDataIndex = 0)]
        public string Id { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Description { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public string Group { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public string Inbound { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int InboundCount { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public string Outbound { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int OutboundCount { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public int AlertLevel { get; internal set; }


        [SupervisionData(RawDataIndex = 8)]
        public int NumericCustom1 { get; internal set; }
        [SupervisionData(RawDataIndex = 9)]
        public int NumericCustom2 { get; internal set; }
        [SupervisionData(RawDataIndex = 10)]
        public int NumericCustom3 { get; internal set; }
        [SupervisionData(RawDataIndex = 11)]
        public int NumericCustom4 { get; internal set; }
        [SupervisionData(RawDataIndex = 12)]
        public int NumericCustom5 { get; internal set; }
        [SupervisionData(RawDataIndex = 13)]
        public int NumericCustom6 { get; internal set; }
        [SupervisionData(RawDataIndex = 14)]
        public int NumericCustom7 { get; internal set; }
        [SupervisionData(RawDataIndex = 15)]
        public int NumericCustom8 { get; internal set; }
        [SupervisionData(RawDataIndex = 16)]
        public int NumericCustom9 { get; internal set; }
        [SupervisionData(RawDataIndex = 17)]
        public int NumericCustom10 { get; internal set; }


        [SupervisionAutoInitProperty]
        public ProductionValues Production { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues PeriodProduction { get; private set; }

        [SupervisionAutoInitProperty]
        public ContactListInfoValues ContactListInfo { get; private set; }

    }

    public class CampaignList : SupervisionList<CampaignItem>
    {
        private CampaignTotal m_CampaignTotal;
        public CampaignTotal CampaignTotal 
        {
            get { return m_CampaignTotal; }
            set { m_CampaignTotal = value; }
        }

        public CampaignList()
            : base()
        {
            m_CampaignTotal = new CampaignTotal();
        }
    }
    #endregion

    #region Campaign Total
    public class CampaignTotal
    {
        private ContactListInfoTotal m_ContactListInfo = new ContactListInfoTotal();

        public ContactListInfoTotal ContactListInfo
        {
            get { return m_ContactListInfo; }
            set { m_ContactListInfo = value; }
        }

    }
    #endregion
    #endregion

    #region Queue
    #region Default
    public class QueueRealTimeValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int Waiting { get; internal set; }

        [SupervisionData(RawDataIndex = 1, TimeUnit=TimeUnit.Seconds, LiveUpdate=true)]
        public TimeSpan MaxWaiting { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int AgentInReady { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public int AlertLevel { get; internal set; }


        [SupervisionData(RawDataIndex = 4)]
        public int NumericCustom1 { get; internal set; }
        [SupervisionData(RawDataIndex = 5)]
        public int NumericCustom2 { get; internal set; }
        [SupervisionData(RawDataIndex = 6)]
        public int NumericCustom3 { get; internal set; }
        [SupervisionData(RawDataIndex = 7)]
        public int NumericCustom4 { get; internal set; }
        [SupervisionData(RawDataIndex = 8)]
        public int NumericCustom5 { get; internal set; }
        [SupervisionData(RawDataIndex = 9)]
        public int NumericCustom6 { get; internal set; }
        [SupervisionData(RawDataIndex = 10)]
        public int NumericCustom7 { get; internal set; }
        [SupervisionData(RawDataIndex = 11)]
        public int NumericCustom8 { get; internal set; }
        [SupervisionData(RawDataIndex = 12)]
        public int NumericCustom9 { get; internal set; }
        [SupervisionData(RawDataIndex = 13)]
        public int NumericCustom10 { get; internal set; }

    }

    public class QueueHistoryValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int Received { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public TimeSpan ReceivedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 1, AveragedBy = 0)]
        public TimeSpan ReceivedAvgTime { get; internal set; }




        [SupervisionData(RawDataIndex = 2)]
        public int Processed { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public TimeSpan ProcessedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 3, AveragedBy = 2)]
        public TimeSpan ProcessedAvgTime { get; internal set; }

        
        
        [SupervisionData(RawDataIndex = 4)]
        public int ProcessedDirect { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public TimeSpan ProcessedDirectTime { get; internal set; }

        [SupervisionData(RawDataIndex = 5, AveragedBy=4)]
        public TimeSpan ProcessedDirectAvgTime { get; internal set; }


        
        
        [SupervisionData(RawDataIndex = 6)]
        public int ProcessedWaiting { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public TimeSpan ProcessedWaitingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 7, AveragedBy=6)]
        public TimeSpan ProcessedWaitingAvgTime { get; internal set; }


        
        
        [SupervisionData(RawDataIndex = 8)]
        public int ProcessedOverflow { get; internal set; }

        [SupervisionData(RawDataIndex = 9)]
        public TimeSpan ProcessedOverflowTime { get; internal set; }

        [SupervisionData(RawDataIndex = 9, AveragedBy=8)]
        public TimeSpan ProcessedOverflowAvgTime { get; internal set; }


        
        
        [SupervisionData(RawDataIndex = 10)]
        public int Abandoned { get; internal set; }

        [SupervisionData(RawDataIndex = 11)]
        public TimeSpan AbandonedTime { get; internal set; }

        [SupervisionData(RawDataIndex = 11, AveragedBy=10)]
        public TimeSpan AbandonedAvgTime { get; internal set; }

        
        
        [SupervisionData(RawDataIndex = 12)]
        public int MaxQueueSize { get; internal set; }

        [SupervisionData(RawDataIndex = 13)]
        public TimeSpan MaxWaitingTime { get; internal set; }

        [SupervisionData(RawDataIndex = 14)]
        public int ContactsHandledInSLATime { get; internal set; }

        [SupervisionData(RawDataIndex = 15, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float PercentageContactsHandled { get; internal set; }

        [SupervisionData(RawDataIndex = 16, ConverterType = typeof(NumericToFloatConverterFactory))]
        public float PercentageContactsHandledInTime { get; internal set; }
    }

    [SupervisionDescriptionProperty("Description")]
    public class QueueItem : SupervisionItem
    {
        [SupervisionData(RawDataIndex = 0)]
        public string Id { get; set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Description { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public string GroupKey { get; internal set; }

        [SupervisionAutoInitProperty]
        public QueueRealTimeValues RealTime { get; private set; }

        [SupervisionAutoInitProperty]
        public QueueHistoryValues History { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues Production { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues PeriodProduction { get; private set; }
    }

    public class QueueList : SupervisionList<QueueItem>
    {       
        private QueueTotal m_QueueTotal;
        
        public QueueTotal QueueTotal 
        {
            get { return m_QueueTotal; }
            set { m_QueueTotal = value; }
        }

        public QueueList()
            : base()
        {
            m_QueueTotal = new QueueTotal();
        }
    }
    #endregion 

    #region Queue Total
    public class QueueRealTimeTotal : INotifyPropertyChanged
    {
        private int m_Waiting = 0;
        private int m_AgentInReady = 0;
        private int m_AlertLevel = 0;
        private int m_NumericCustom1 = 0;
        private int m_NumericCustom2 = 0;
        private int m_NumericCustom3 = 0;
        private int m_NumericCustom4 = 0;
        private int m_NumericCustom5 = 0;
        private int m_NumericCustom6 = 0;
        private int m_NumericCustom7 = 0;
        private int m_NumericCustom8 = 0;
        private int m_NumericCustom9 = 0;
        private int m_NumericCustom10 = 0;

        
        public int Waiting
        {
            get { return m_Waiting; }
            set
            {
                m_Waiting = value;
                FirePropertyChanged("Waiting");
            }
        }
        public int AgentInReady
        {
            get { return m_AgentInReady; }
            set
            {
                m_AgentInReady = value;
                FirePropertyChanged("AgentInReady");
            }
        }

        public int AlertLevel
        {
            get { return m_AlertLevel; }
            set
            {
                m_AlertLevel = value; FirePropertyChanged("AlertLevel");
            }
        }

        public int NumericCustom1
        {
            get { return m_NumericCustom1; }
            set
            {
                m_NumericCustom1 = value; FirePropertyChanged("NumericCustom1");
            }
        }
        public int NumericCustom2
        {
            get { return m_NumericCustom2; }
            set
            {
                m_NumericCustom2 = value; FirePropertyChanged("NumericCustom2");
            }
        }
        public int NumericCustom3
        {
            get { return m_NumericCustom3; }
            set
            {
                m_NumericCustom3 = value; FirePropertyChanged("NumericCustom3");
            }
        }
        public int NumericCustom4
        {
            get { return m_NumericCustom4; }
            set
            {
                m_NumericCustom4 = value; FirePropertyChanged("NumericCustom4");
            }
        }
        public int NumericCustom5
        {
            get { return m_NumericCustom5; }
            set
            {
                m_NumericCustom5 = value; FirePropertyChanged("NumericCustom5");
            }
        }
        public int NumericCustom6
        {
            get { return m_NumericCustom6; }
            set
            {
                m_NumericCustom6 = value; FirePropertyChanged("NumericCustom6");
            }
        }
        public int NumericCustom7
        {
            get { return m_NumericCustom7; }
            set
            {
                m_NumericCustom7 = value; FirePropertyChanged("NumericCustom7");
            }
        }
        public int NumericCustom8
        {
            get { return m_NumericCustom8; }
            set
            {
                m_NumericCustom8 = value; FirePropertyChanged("NumericCustom8");
            }
        }
        public int NumericCustom9
        {
            get { return m_NumericCustom9; }
            set
            {
                m_NumericCustom9 = value; FirePropertyChanged("NumericCustom9");
            }
        }
        public int NumericCustom10
        {
            get { return m_NumericCustom10; }
            set
            {
                m_NumericCustom10 = value; FirePropertyChanged("NumericCustom10");
            }
        }

        public QueueRealTimeTotal()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }

    public class QueueHistoryTotal : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext();
        private int m_Received = 0;
        private int m_Processed = 0;
        private int m_ProcessedDirect = 0;
        private int m_ProcessedWaiting = 0;
        private TimeSpan m_ProcessedWaitingTime = TimeSpan.Zero;
        private int m_ProcessedOverflow = 0;
        private int m_Abandoned = 0;
        private TimeSpan m_AbandonedTime = TimeSpan.Zero;       
        private ChartPieces m_HistoryGraph;
        
        public int Received
        {
            get { return m_Received; }
            set
            {
                m_Received = value;
                FirePropertyChanged("Received");
            }
        }
        public int Processed
        {
            get { return m_Processed; }
            set
            {
                m_Processed = value;
                FirePropertyChanged("Processed");
            }
        }
        public int ProcessedDirect
        {
            get { return m_ProcessedDirect; }
            set
            {
                m_ProcessedDirect = value;
                FirePropertyChanged("ProcessedDirect");
                m_HistoryGraph.UpdateValue("ProcessedDirect", value);
            }
        }
        public int ProcessedWaiting
        {
            get { return m_ProcessedWaiting; }
            set
            {
                m_ProcessedWaiting = value;
                FirePropertyChanged("ProcessedWaiting");
                m_HistoryGraph.UpdateValue("ProcessedWaiting", value);
            }
        }
        public TimeSpan ProcessedWaitingTime
        {
            get { return m_ProcessedWaitingTime; }
            set
            {
                m_ProcessedWaitingTime = value;
                FirePropertyChanged("ProcessedWaitingTime");
            }
        }
        public int ProcessedOverflow
        {
            get { return m_ProcessedOverflow; }
            set
            {
                m_ProcessedOverflow = value;
                FirePropertyChanged("ProcessedOverflow");
                m_HistoryGraph.UpdateValue("ProcessedOverflow", value);
            }
        }
        public int Abandoned
        {
            get { return m_Abandoned; }
            set
            {
                m_Abandoned = value;
                FirePropertyChanged("Abandoned");
                m_HistoryGraph.UpdateValue("Abandoned", value);
            }
        }
        public TimeSpan AbandonedTime
        {
            get { return m_AbandonedTime; }
            set
            {
                m_AbandonedTime = value;
                FirePropertyChanged("AbandonedTime");
            }
        }
        public ChartPieces HistoryGraph
        {
            get { return m_HistoryGraph; }
            set { m_HistoryGraph = value; FirePropertyChanged("QueueHistoryGraph"); }
        }
        
        public QueueHistoryTotal()
        {
            m_HistoryGraph = new ChartPieces();
            m_HistoryGraph.Add(new ChartPiece { Key = "ProcessedDirect", Name = TranslationContext.Translate("Direct to agent"), Value = 0, Color = "#b4e93c" });
            m_HistoryGraph.Add(new ChartPiece { Key = "ProcessedWaiting", Name = TranslationContext.Translate("Waiting to agent"), Value = 0, Color = "#fff564" });
            m_HistoryGraph.Add(new ChartPiece { Key = "ProcessedOverflow", Name = TranslationContext.Translate("Overflow"), Value = 0, Color = "#fa9654" });
            m_HistoryGraph.Add(new ChartPiece { Key = "Abandoned", Name = TranslationContext.Translate("Abandoned"), Value = 0, Color = "#f84343" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
    
    public class QueueTotal
    {
        private QueueRealTimeTotal m_RealTime = new QueueRealTimeTotal();
        private QueueHistoryTotal m_History = new QueueHistoryTotal();

        public QueueRealTimeTotal RealTime
        {
            get { return m_RealTime; }
            set { m_RealTime = value; }
        }
        public QueueHistoryTotal History
        {
            get { return m_History; }
            set { m_History = value; }
        }
    }
    #endregion
    #endregion

    #region Team
    public class TeamRealTimeValues : SupervisionDataCollection
    {
        [SupervisionData(RawDataIndex = 0)]
        public int AgentsLogon { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public int AgentsInPause { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public int AgentsInWaiting { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public int AgentsOnline { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public int AgentsInWrapup { get; internal set; }

        [SupervisionData(RawDataIndex = 5)]
        public int WaitingInQueue { get; internal set; }

        [SupervisionData(RawDataIndex = 6)]
        public int RealTimeGraph { get; internal set; }

        [SupervisionData(RawDataIndex = 7)]
        public int AlertLevel { get; internal set; }


        [SupervisionData(RawDataIndex = 8)]
        public int NumericCustom1 { get; internal set; }
        [SupervisionData(RawDataIndex = 9)]
        public int NumericCustom2 { get; internal set; }
        [SupervisionData(RawDataIndex = 10)]
        public int NumericCustom3 { get; internal set; }
        [SupervisionData(RawDataIndex = 11)]
        public int NumericCustom4 { get; internal set; }
        [SupervisionData(RawDataIndex = 12)]
        public int NumericCustom5 { get; internal set; }
        [SupervisionData(RawDataIndex = 13)]
        public int NumericCustom6 { get; internal set; }
        [SupervisionData(RawDataIndex = 14)]
        public int NumericCustom7 { get; internal set; }
        [SupervisionData(RawDataIndex = 15)]
        public int NumericCustom8 { get; internal set; }
        [SupervisionData(RawDataIndex = 16)]
        public int NumericCustom9 { get; internal set; }
        [SupervisionData(RawDataIndex = 17)]
        public int NumericCustom10 { get; internal set; }

    }                                             

    public class TeamItem : SupervisionItem
    {
        [SupervisionData(RawDataIndex = 0)]
        public string Id { get; internal set; }

        [SupervisionData(RawDataIndex = 1)]
        public string Description { get; internal set; }

        [SupervisionData(RawDataIndex = 2)]
        public string Group { get; internal set; }

        [SupervisionData(RawDataIndex = 3)]
        public string[] Agents { get; internal set; }

        [SupervisionData(RawDataIndex = 4)]
        public string[] Queues { get; internal set; }

        [SupervisionAutoInitProperty]
        public TeamRealTimeValues RealTime { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues Production { get; private set; }

        [SupervisionAutoInitProperty]
        public ProductionValues PeriodProduction { get; private set; }
    }

    public class TeamList : SupervisionList<TeamItem>
    {
    }
    #endregion

    #region Alerts
    public class AlertList : ObservableCollection<AlertItem>//, INotifyPropertyChanged
    {
        #region Class data
        private int m_UnReadItems = 0;
        #endregion

        #region Constructors
        public AlertList()
            : base()
        {
            this.CollectionChanged += new NotifyCollectionChangedEventHandler(AlertList_CollectionChanged);            
        }

        private void AlertList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AlertItem item in e.NewItems)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                }

                CheckUnreadCount();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AlertItem item in e.NewItems)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                }
                CheckUnreadCount();
            }
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("unread", StringComparison.InvariantCultureIgnoreCase))
            {
                CheckUnreadCount();
            }
        }
        #endregion

        #region Properties
        public int UnReadItems
        {
            get { return m_UnReadItems; }
            set { m_UnReadItems = value; OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("UnReadItems")); 
            }
        }
        #endregion

        #region Members
        private void CheckUnreadCount()
        {
            int unread = 0;
            foreach (AlertItem item in this)
            {
                if (item.UnRead)
                    unread++;
            }
            if (unread != m_UnReadItems)
                this.UnReadItems = unread;
        }
        #endregion

        
    }

    public class AlertItem : INotifyPropertyChanged
    {
        #region Class data
        private int m_TypeId = 0;
        private MessageType m_Type = MessageType.Default;
        private MessageDestinations m_Destinations = MessageDestinations.None;
        private string m_Message = string.Empty;
        private DateTime m_DateTime = DateTime.Now;
        private bool m_UnRead = true;
        private string m_Originator = string.Empty;
        private string m_OriginatorId = string.Empty;
        #endregion

        #region Properties
        public int TypeId
        {
            get { return m_TypeId; }
            set { m_TypeId = value; FirePropertyChanged("TypeId"); }
        }
        public MessageType Type
        {
            get { return m_Type; }
            set { m_Type = value; FirePropertyChanged("Type"); }
        }
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; FirePropertyChanged("Message"); }
        }
        public DateTime DateTime
        {
            get { return m_DateTime; }
            set { m_DateTime = value; FirePropertyChanged("DateTime"); }
        }
        public bool UnRead
        {
            get { return m_UnRead; }
            set { m_UnRead = value; FirePropertyChanged("UnRead"); }
        }
        public string Originator
        {
            get { return m_Originator; }
            set { m_Originator = value; FirePropertyChanged("Originator"); }
        }
        public string OriginatorId
        {
            get { return m_OriginatorId; }
            set { m_OriginatorId = value; FirePropertyChanged("OriginatorId"); }
        }
        public MessageDestinations Destinations
        {
            get { return m_Destinations; }
            set { m_Destinations = value; FirePropertyChanged("Destinations"); }
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
    #endregion

    public delegate void RefreshCollectionDelegate(); 


    public class Supervision : SupervisionSource
    {
        private void TraceText(string text)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("ClientSupervisionV2 --> {0}", text));
        }
        public event RefreshCollectionDelegate RefreshCollection;
        internal void OnRefreshCollection()
        {
            if (RefreshCollection != null)
                RefreshCollection();
        }

        [SupervisionSource("Agent")]
        public AgentList Agents { get; private set; }

        public AgentList AgentsFiltered { get; private set; }

        [SupervisionSource("Inbound")]
        public InboundList Inbounds { get; private set; }

        [SupervisionSource("Outbound")]
        public OutboundList Outbounds { get; private set; }

        [SupervisionSource("Queue")]
        public QueueList Queues { get; private set; }

        [SupervisionSource("Campaign")]
        public CampaignList Campaigns { get; private set; }

        [SupervisionSource("Team", Visible=false)]
        public TeamList Teams { get; private set; }


        public AlertList Alerts { get; private set; }

        public Nixxis.Client.HttpLink ClientLink { get; private set; }

        public Supervision(Nixxis.Client.HttpLink clientLink)
            : base(clientLink)
        {
            ClientLink = clientLink;

            Alerts = new AlertList();
            Agents = new AgentList();
            AgentsFiltered = new AgentList();
            AgentsFiltered.AgentTotal = Agents.AgentTotal;
            AgentMessage += new AgentMessageEventHandler(Supervision_AgentMessage);

            Inbounds = new InboundList();
            Outbounds = new OutboundList();
            Queues = new QueueList();
            Campaigns = new CampaignList();
            Teams = new TeamList();

            Inbounds.CollectionChanged += new NotifyCollectionChangedEventHandler(Inbounds_CollectionChanged);
            Agents.CollectionChanged += new NotifyCollectionChangedEventHandler(Agents_CollectionChanged);
            Outbounds.CollectionChanged +=new NotifyCollectionChangedEventHandler(Outbounds_CollectionChanged);
            Queues.CollectionChanged += new NotifyCollectionChangedEventHandler(Queues_CollectionChanged);
            Campaigns.CollectionChanged += new NotifyCollectionChangedEventHandler(Campaigns_CollectionChanged);
            Teams.CollectionChanged += new NotifyCollectionChangedEventHandler(Teams_CollectionChanged);
        }

        public void Connected()
        {
            SupervisionSettings.DateTimeFormat = ClientLink.DateTimeFormat;
        }

        #region Alerts and Messages
        void Supervision_AgentMessage(string[] info)
        {
            AlertItem item = new AlertItem();

            item.TypeId = int.Parse(info[2]);
            item.Type = (MessageType)Enum.Parse(typeof(MessageType), info[2]);
            item.Originator = Microsoft.JScript.GlobalObject.unescape(info[4]);
            item.Destinations = (MessageDestinations)int.Parse(info[3]);
            item.Message = Microsoft.JScript.GlobalObject.unescape(info[5]);

            string[] list = info[0].Split('_');

            if (list.Length > 1)
            {
                item.OriginatorId = list[1];
            }

            if (item.Type != MessageType.Chat && item.Type != MessageType.ChatEnd)
                Alerts.Add(item);

            OnMessageReceived(item);
        }

        public event NixxisMessageReceivedHandler MessageReceived;
        public void OnMessageReceived(AlertItem item)
        {
            if (MessageReceived != null)
                MessageReceived(item);
        }
        #endregion

        void Teams_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TeamItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged += new PropertyChangedEventHandler(QueueRealTime_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TeamItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged -= new PropertyChangedEventHandler(QueueRealTime_PropertyChanged);
                }
            }
        }

        #region Campaign
        void Campaigns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CampaignItem item in e.NewItems)
                {
                    item.ContactListInfo.PropertyChanged += new PropertyChangedEventHandler(CampaignsContactListInfo_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CampaignItem item in e.NewItems)
                {
                    item.ContactListInfo.PropertyChanged -= new PropertyChangedEventHandler(CampaignsContactListInfo_PropertyChanged);
                }
            }
        }

        void CampaignsContactListInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int tot = 0;
            if ("ContactCount" == e.PropertyName)
            {
                foreach (CampaignItem item in Campaigns)
                    tot += item.ContactListInfo.ContactCount;

                if (Campaigns.CampaignTotal.ContactListInfo.ContactCount != tot)
                    Campaigns.CampaignTotal.ContactListInfo.ContactCount = tot;
            }
            else if ("ContactToDial " == e.PropertyName)
            {
                foreach (CampaignItem item in Campaigns)
                    tot += item.ContactListInfo.ContactToDial;

                if (Campaigns.CampaignTotal.ContactListInfo.ContactToDial != tot)
                    Campaigns.CampaignTotal.ContactListInfo.ContactToDial = tot;
            }
            else if ("ContactNeverDialed" == e.PropertyName)
            {
                foreach (CampaignItem item in Campaigns)
                    tot += item.ContactListInfo.ContactNeverDialed;

                if (Campaigns.CampaignTotal.ContactListInfo.ContactNeverDialed != tot)
                    Campaigns.CampaignTotal.ContactListInfo.ContactNeverDialed = tot;
            }
            else if ("ContactCallbacks" == e.PropertyName)
            {
                foreach (CampaignItem item in Campaigns)
                    tot += item.ContactListInfo.ContactCallbacks;

                if (Campaigns.CampaignTotal.ContactListInfo.ContactCallbacks != tot)
                    Campaigns.CampaignTotal.ContactListInfo.ContactCallbacks = tot;
            }
            else if ("ContactToRedial" == e.PropertyName)
            {
                foreach (CampaignItem item in Campaigns)
                    tot += item.ContactListInfo.ContactToRedial;

                if (Campaigns.CampaignTotal.ContactListInfo.ContactToRedial != tot)
                    Campaigns.CampaignTotal.ContactListInfo.ContactToRedial = tot;
            }
            else if ("ContactToNotRedial" == e.PropertyName)
            {
                foreach (CampaignItem item in Campaigns)
                    tot += item.ContactListInfo.ContactToNotRedial;

                if (Campaigns.CampaignTotal.ContactListInfo.ContactToNotRedial != tot)
                    Campaigns.CampaignTotal.ContactListInfo.ContactToNotRedial = tot;
            }
        }
        #endregion

        #region Queue
        void Queues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (QueueItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged += new PropertyChangedEventHandler(QueueRealTime_PropertyChanged);
                    item.History.PropertyChanged += new PropertyChangedEventHandler(QueueHistory_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (QueueItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged -= new PropertyChangedEventHandler(QueueRealTime_PropertyChanged); 
                    item.History.PropertyChanged -= new PropertyChangedEventHandler(QueueHistory_PropertyChanged);
                }
            }
        }

        private void QueueRealTime_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int tot = 0;
            if ("Waiting" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.Waiting;

                if (Queues.QueueTotal.RealTime.Waiting != tot)
                    Queues.QueueTotal.RealTime.Waiting = tot;
            }
            else if ("AgentInReady" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.AgentInReady;

                if (Queues.QueueTotal.RealTime.AgentInReady != tot)
                    Queues.QueueTotal.RealTime.AgentInReady = tot;
            }
            else if ("AlertLevel" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.AlertLevel;

                if (Queues.QueueTotal.RealTime.AlertLevel != tot)
                    Queues.QueueTotal.RealTime.AlertLevel = tot;
            }
            else if ("NumericCustom1" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom1;

                if (Queues.QueueTotal.RealTime.NumericCustom1 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom1 = tot;
            }
            else if ("NumericCustom2" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom2;

                if (Queues.QueueTotal.RealTime.NumericCustom2 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom2 = tot;
            }
            else if ("NumericCustom3" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom3;

                if (Queues.QueueTotal.RealTime.NumericCustom3 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom3 = tot;
            }
            else if ("NumericCustom4" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom4;

                if (Queues.QueueTotal.RealTime.NumericCustom4 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom4 = tot;
            }
            else if ("NumericCustom5" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom5;

                if (Queues.QueueTotal.RealTime.NumericCustom5 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom5 = tot;
            }
            else if ("NumericCustom6" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom6;

                if (Queues.QueueTotal.RealTime.NumericCustom6 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom6 = tot;
            }
            else if ("NumericCustom7" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom7;

                if (Queues.QueueTotal.RealTime.NumericCustom7 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom7 = tot;
            }
            else if ("NumericCustom8" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom8;

                if (Queues.QueueTotal.RealTime.NumericCustom8 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom8 = tot;
            }
            else if ("NumericCustom9" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom9;

                if (Queues.QueueTotal.RealTime.NumericCustom9 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom9 = tot;
            }
            else if ("NumericCustom10" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.RealTime.NumericCustom10;

                if (Queues.QueueTotal.RealTime.NumericCustom10 != tot)
                    Queues.QueueTotal.RealTime.NumericCustom10 = tot;
            }
        }

        private void QueueHistory_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int tot = 0;
            TimeSpan tdMax = TimeSpan.Zero;

            if ("Received" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.History.Received;

                if (Queues.QueueTotal.History.Received != tot)
                    Queues.QueueTotal.History.Received = tot;
            }
            else if ("Processed" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.History.Processed;

                if (Queues.QueueTotal.History.Processed != tot)
                    Queues.QueueTotal.History.Processed = tot;
            }
            else if ("ProcessedDirect" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.History.ProcessedDirect;

                if (Queues.QueueTotal.History.ProcessedDirect != tot)
                    Queues.QueueTotal.History.ProcessedDirect = tot;
            }
            else if ("ProcessedWaiting" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.History.ProcessedWaiting;

                if (Queues.QueueTotal.History.ProcessedWaiting != tot)
                    Queues.QueueTotal.History.ProcessedWaiting = tot;
            }
            else if ("ProcessedWaitingTime " == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                {
                    if(item.History.ProcessedWaitingTime > tdMax)
                        tdMax = item.History.ProcessedWaitingTime;
                }

                if (Queues.QueueTotal.History.ProcessedWaitingTime != tdMax)
                    Queues.QueueTotal.History.ProcessedWaitingTime = tdMax;
            }
            else if ("ProcessedOverflow" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.History.ProcessedOverflow;

                if (Queues.QueueTotal.History.ProcessedOverflow != tot)
                    Queues.QueueTotal.History.ProcessedOverflow = tot;
            }
            else if ("Abandoned" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                    tot += item.History.Abandoned;

                if (Queues.QueueTotal.History.Abandoned != tot)
                    Queues.QueueTotal.History.Abandoned = tot;
            }
            else if ("AbandonedTime" == e.PropertyName)
            {
                foreach (QueueItem item in Queues)
                {
                    if (item.History.AbandonedTime > tdMax)
                        tdMax = item.History.AbandonedTime;
                }

                if (Queues.QueueTotal.History.AbandonedTime != tdMax)
                    Queues.QueueTotal.History.AbandonedTime = tdMax;
            }
        }
        #endregion

        #region Outbound
        private void Outbounds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (OutboundItem item in e.NewItems)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(OutboundItem_PropertyChanged);
                    item.RealTime.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OutboundRealTime_PropertyChanged);
                }
                if(Outbounds.OutboundTotal.Count != Outbounds.Count)
                    Outbounds.OutboundTotal.Count = Outbounds.Count;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (OutboundItem item in e.NewItems)
                {
                    item.PropertyChanged -=  new PropertyChangedEventHandler(OutboundItem_PropertyChanged);
                    item.RealTime.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OutboundRealTime_PropertyChanged);
                }
            }
        }

        private void OutboundItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                int tot = 0;
                if ("Mode" == e.PropertyName)
                {
                    int totPreview = 0;
                    int totProgress = 0;
                    int totPredictive = 0;
                    int totOthers = 0;

                    foreach (OutboundItem item in Outbounds)
                    {
                        switch (item.Mode.ToLower())
                        {
                            case "preview":
                                totPreview++;
                                break;
                            case "progressive":
                                totProgress++;
                                break;
                            case "predictive":
                                totPredictive++;
                                break;
                            default:
                                totOthers++;
                                break;
                        }
                    }

                    if (Outbounds.OutboundTotal.ModePreview != totPreview)
                        Outbounds.OutboundTotal.ModePreview = totPreview;

                    if (Outbounds.OutboundTotal.ModeProgressive != totProgress)
                        Outbounds.OutboundTotal.ModeProgressive = totProgress;

                    if (Outbounds.OutboundTotal.ModePredictive != totPredictive)
                        Outbounds.OutboundTotal.ModePredictive = totPredictive;

                    if (Outbounds.OutboundTotal.ModeOthers != totOthers)
                        Outbounds.OutboundTotal.ModeOthers = totOthers;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        private void OutboundRealTime_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int tot = 0;
            if ("SystemPreprocessing" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.SystemPreprocessing;

                if (Outbounds.OutboundTotal.RealTime.SystemPreprocessing != tot)
                    Outbounds.OutboundTotal.RealTime.SystemPreprocessing = tot;
            }            
            else if ("Closing" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Closing;

                if (Outbounds.OutboundTotal.RealTime.Closing != tot)
                    Outbounds.OutboundTotal.RealTime.Closing = tot;
            }
            else if ("Ivr" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Ivr;

                if (Outbounds.OutboundTotal.RealTime.Ivr != tot)
                    Outbounds.OutboundTotal.RealTime.Ivr = tot;
            }            
            else if ("Waiting" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Waiting;

                if (Outbounds.OutboundTotal.RealTime.Waiting != tot)
                    Outbounds.OutboundTotal.RealTime.Waiting = tot;
            }
            else if ("Online" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Online;

                if (Outbounds.OutboundTotal.RealTime.Online != tot)
                    Outbounds.OutboundTotal.RealTime.Online = tot;
            }
            else if ("WrapUp" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.WrapUp;

                if (Outbounds.OutboundTotal.RealTime.WrapUp != tot)
                    Outbounds.OutboundTotal.RealTime.WrapUp = tot;
            }
            else if ("Transfer" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Transfer;

                if (Outbounds.OutboundTotal.RealTime.Transfer != tot)
                    Outbounds.OutboundTotal.RealTime.Transfer = tot;
            }
            else if ("Overflowing" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Overflowing;

                if (Outbounds.OutboundTotal.RealTime.Overflowing != tot)
                    Outbounds.OutboundTotal.RealTime.Overflowing = tot;
            }
            else if ("Preview" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.Preview;

                if (Outbounds.OutboundTotal.RealTime.Preview != tot)
                    Outbounds.OutboundTotal.RealTime.Preview = tot;
            }
            else if ("AlertLevel" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.AlertLevel;

                if (Outbounds.OutboundTotal.RealTime.AlertLevel != tot)
                    Outbounds.OutboundTotal.RealTime.AlertLevel = tot;
            }
            else if ("NumericCustom1" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom1;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom1 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom1 = tot;
            }
            else if ("NumericCustom2" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom2;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom2 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom2 = tot;
            }
            else if ("NumericCustom3" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom3;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom3 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom3 = tot;
            }
            else if ("NumericCustom4" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom4;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom4 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom4 = tot;
            }
            else if ("NumericCustom5" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom5;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom5 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom5 = tot;
            }
            else if ("NumericCustom6" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom6;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom6 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom6 = tot;
            }
            else if ("NumericCustom7" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom7;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom7 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom7 = tot;
            }
            else if ("NumericCustom8" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom8;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom8 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom8 = tot;
            }
            else if ("NumericCustom9" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom9;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom9 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom9 = tot;
            }
            else if ("NumericCustom10" == e.PropertyName)
            {
                foreach (OutboundItem item in Outbounds)
                    tot += item.RealTime.NumericCustom10;

                if (Outbounds.OutboundTotal.RealTime.NumericCustom10 != tot)
                    Outbounds.OutboundTotal.RealTime.NumericCustom10 = tot;
            }
        }
        #endregion

        #region Agent
        private void Agents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Agents_CollectionChanged. Action {0} ", e.Action));

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AgentItem item in e.NewItems)
                {

                    item.RealTime.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(AgentRealTime_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AgentItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(AgentRealTime_PropertyChanged);
                }
            }
        }

        private void item_AgentStatusChanged(AgentItem item)
        {

            try
            {

                if (item.RealTime.StatusIndex == 0)
                {
                    TraceText("Status changed to 0 for Agent " + item.Account);
                    if (AgentsFiltered.Contains(item))
                    {
                        TraceText("Status changed remove Agent " + item.Account);
                        AgentsFiltered.Remove(item);
                    }
                }
                else
                {
                    TraceText("Status changed diff from 0 for Agent " + item.Account);
                    if (!AgentsFiltered.Contains(item))
                    {
                        TraceText("Status changed add Agent " + item.Account);
                        AgentsFiltered.Add(item);
                    }
                }

            }
            catch (Exception error)
            {
                TraceText("Status changed ERROR: " + error.ToString());
            }
        }

        private void AgentRealTime_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int tot = 0;
            if ("StatusIndex" == e.PropertyName)
            {

                int[] states = new int[10];

                foreach (AgentItem item in Agents)
                    states[item.RealTime.StatusIndex]++;

                if (Agents.AgentTotal.RealTime.StatePauseCount != states[3])
                    Agents.AgentTotal.RealTime.StatePauseCount = states[3]; 
                
                if (Agents.AgentTotal.RealTime.StateWaitingCount != states[4])
                    Agents.AgentTotal.RealTime.StateWaitingCount = states[4];

                if (Agents.AgentTotal.RealTime.StateOnlineCount != states[5])
                    Agents.AgentTotal.RealTime.StateOnlineCount = states[5];

                if (Agents.AgentTotal.RealTime.StateWrapUpCount != states[6])
                    Agents.AgentTotal.RealTime.StateWrapUpCount = states[6];

                if (Agents.AgentTotal.RealTime.StateSupervisionCount != states[8])
                    Agents.AgentTotal.RealTime.StateSupervisionCount = states[8];

                if (Agents.AgentTotal.RealTime.StatePreviewCount != states[9])
                    Agents.AgentTotal.RealTime.StatePreviewCount = states[9];

                OnRefreshCollection();
            }
            else if ("AlertLevel" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.AlertLevel;

                if (Agents.AgentTotal.RealTime.AlertLevel != tot)
                    Agents.AgentTotal.RealTime.AlertLevel = tot;
            }
            else if ("NumericCustom1" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom1;

                if (Agents.AgentTotal.RealTime.NumericCustom1 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom1 = tot;
            }
            else if ("NumericCustom2" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom2;

                if (Agents.AgentTotal.RealTime.NumericCustom2 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom2 = tot;
            }
            else if ("NumericCustom3" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom3;

                if (Agents.AgentTotal.RealTime.NumericCustom3 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom3 = tot;
            }
            else if ("NumericCustom4" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom4;

                if (Agents.AgentTotal.RealTime.NumericCustom4 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom4 = tot;
            }
            else if ("NumericCustom5" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom5;

                if (Agents.AgentTotal.RealTime.NumericCustom5 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom5 = tot;
            }
            else if ("NumericCustom6" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom6;

                if (Agents.AgentTotal.RealTime.NumericCustom6 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom6 = tot;
            }
            else if ("NumericCustom7" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom7;

                if (Agents.AgentTotal.RealTime.NumericCustom7 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom7 = tot;
            }
            else if ("NumericCustom8" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom8;

                if (Agents.AgentTotal.RealTime.NumericCustom8 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom8 = tot;
            }
            else if ("NumericCustom9" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom9;

                if (Agents.AgentTotal.RealTime.NumericCustom9 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom9 = tot;
            }
            else if ("NumericCustom10" == e.PropertyName)
            {
                foreach (AgentItem item in Agents)
                    tot += item.RealTime.NumericCustom10;

                if (Agents.AgentTotal.RealTime.NumericCustom10 != tot)
                    Agents.AgentTotal.RealTime.NumericCustom10 = tot;
            }

        }
        #endregion

        #region Inbound
        private void Inbounds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (InboundItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(InboundRealTime_PropertyChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (InboundItem item in e.NewItems)
                {
                    item.RealTime.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(InboundRealTime_PropertyChanged);
                }
            }
        }

        private void InboundRealTime_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            int tot = 0;
            if ("Waiting" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.Waiting;

                if (Inbounds.InboundTotal.RealTime.Waiting != tot)
                    Inbounds.InboundTotal.RealTime.Waiting = tot;
            }
            else if ("Ivr" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.Ivr;

                if (Inbounds.InboundTotal.RealTime.Ivr != tot)
                    Inbounds.InboundTotal.RealTime.Ivr = tot;
            }
            else if ("Online" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.Online;

                if (Inbounds.InboundTotal.RealTime.Online != tot)
                    Inbounds.InboundTotal.RealTime.Online = tot;
            }
            else if ("WrapUp" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.WrapUp;

                if (Inbounds.InboundTotal.RealTime.WrapUp != tot)
                    Inbounds.InboundTotal.RealTime.WrapUp = tot;
            }
            else if ("Overflowing" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.Overflowing;

                if (Inbounds.InboundTotal.RealTime.Overflowing != tot)
                    Inbounds.InboundTotal.RealTime.Overflowing = tot;
            }
            else if ("AlertLevel" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.AlertLevel;

                if (Inbounds.InboundTotal.RealTime.AlertLevel != tot)
                    Inbounds.InboundTotal.RealTime.AlertLevel = tot;
            }
            else if ("NumericCustom1" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom1;

                if (Inbounds.InboundTotal.RealTime.NumericCustom1 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom1 = tot;
            }
            else if ("NumericCustom2" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom2;

                if (Inbounds.InboundTotal.RealTime.NumericCustom2 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom2 = tot;
            }
            else if ("NumericCustom3" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom3;

                if (Inbounds.InboundTotal.RealTime.NumericCustom3 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom3 = tot;
            }
            else if ("NumericCustom4" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom4;

                if (Inbounds.InboundTotal.RealTime.NumericCustom4 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom4 = tot;
            }
            else if ("NumericCustom5" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom5;

                if (Inbounds.InboundTotal.RealTime.NumericCustom5 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom5 = tot;
            }
            else if ("NumericCustom6" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom6;

                if (Inbounds.InboundTotal.RealTime.NumericCustom6 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom6 = tot;
            }
            else if ("NumericCustom7" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom7;

                if (Inbounds.InboundTotal.RealTime.NumericCustom7 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom7 = tot;
            }
            else if ("NumericCustom8" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom8;

                if (Inbounds.InboundTotal.RealTime.NumericCustom8 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom8 = tot;
            }
            else if ("NumericCustom9" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom9;

                if (Inbounds.InboundTotal.RealTime.NumericCustom9 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom9 = tot;
            }
            else if ("NumericCustom10" == e.PropertyName)
            {
                foreach (InboundItem item in Inbounds)
                    tot += item.RealTime.NumericCustom10;

                if (Inbounds.InboundTotal.RealTime.NumericCustom10 != tot)
                    Inbounds.InboundTotal.RealTime.NumericCustom10 = tot;
            }
        }
        #endregion
    }

    public delegate void NixxisMessageReceivedHandler(AlertItem item);
}
