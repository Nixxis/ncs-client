using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Nixxis.Client
{
    #region Attributes
    internal class SupervisionItemAttribute : Attribute
    {
        #region Class data
        private string _Size = string.Empty;
        private SupervisionSumTypes _SumType = SupervisionSumTypes.None;
        private SupervisionTotalTypes _TotalType = SupervisionTotalTypes.None;
        #endregion

        #region Properties
        public string Size
        {
            get { return _Size; }
            set { _Size = value; }
        }
        public SupervisionSumTypes SumType
        {
            get { return _SumType; }
            set { _SumType = value; }
        }

        #endregion

        public SupervisionItemAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class SupervisionItemFieldAttribute : Attribute
    {
        #region Class data
        private string _Size = string.Empty;
        private SupervisionSumTypes _SumType = SupervisionSumTypes.None;
        private SupervisionTotalTypes _TotalType = SupervisionTotalTypes.None;
        #endregion

        #region Properties
        public string Size
        {
            get { return _Size; }
            set { _Size = value; }
        }
        public SupervisionSumTypes SumType
        {
            get { return _SumType; }
            set { _SumType = value; }
        }

        #endregion

        public SupervisionItemFieldAttribute()
        {
        }
    }
    #endregion

    public abstract class SupervisionObjectReferenceManager
    {
        private static List<WeakReference> m_References = new List<WeakReference>();
        private static System.Threading.Timer m_Timer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimer), null, 60000, 60000);

        public static System.Threading.TimerCallback RollingDataChanged;

        protected SupervisionObjectReferenceManager()
        {
            AddObjRef(this);
        }

        private static void OnTimer(object state)
        {
            System.Threading.Monitor.Enter(m_References);

            try
            {
                for (int i = 0; i < m_References.Count; i++)
                {
                    if (m_References[i].IsAlive)
                    {
                        System.Threading.Monitor.Exit(m_References);

                        try
                        {
                            ((SupervisionObjectReferenceManager)m_References[i].Target).UpdateTimers();
                        }
                        catch
                        {
                        }
                        finally
                        {
                            System.Threading.Monitor.Enter(m_References);
                        }
                    }
                    else
                    {
                        m_References.RemoveAt(i);
                        i--;
                    }
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(m_References);
            }

            if (RollingDataChanged != null)
            {
                try
                {
                    RollingDataChanged(null);
                }
                catch
                {
                }
            }
        }

        protected void AddObjRef(SupervisionObjectReferenceManager obj)
        {
            lock (m_References)
            {
                m_References.Add(new WeakReference(obj, false));
            }
        }

        protected abstract void UpdateTimers();
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

        protected long LongValue(int index)
        {
            if (m_RawData != null && m_RawData.Length > index)
            {
                long Value;

                if (long.TryParse(m_RawData[index], out Value))
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
                                    int Value = int.Parse(rawData[i]);// (int)m_Properties[i].GetValue(this, null);

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
                                    int Value = int.Parse(rawData[i]); //(int)((TimeSpan)m_Properties[i].GetValue(this, null)).TotalMilliseconds;

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

    #region enums

    #region other
    internal enum SupervisionSumTypes
    {
        None,
        Peak,
        Diff
    }
    internal enum SupervisionTotalTypes
    {
        None,
        Max,
        Min,
        Sum,
        Avg
    }
    #endregion

    #region Array enums

    internal enum AgentParametersArray
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

    internal enum AgentRealtimeArray
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
        PerQueEnQueue = 14,
        AlertLevel = 15,
        NumericCustom1 = 16,
        NumericCustom2 = 17,
        NumericCustom3 = 18,
        NumericCustom4 = 19,
        NumericCustom5 = 20,
        NumericCustom6 = 21,
        NumericCustom7 = 22,
        NumericCustom8 = 23,
        NumericCustom9 = 24,
        NumericCustom10 = 25,
        MonitoringSupervisors = 26,
        RecordingSupervisors = 27,
        IsRecording = 28,
        ViewingSupervisors = 29,
        ListeningSupervisors = 30


    }

    internal enum AgentHistoryArray
    {
        Undefined = 0,
        Pause = 1,
        Off = 2,
        Waiting = 3,
        Wrapup = 4,
        WrapupInbound = 5,
        WrapupOutbound = 6,
        WrapupEMail = 7,
        WrapupChat = 8,
        Online = 9,
        HandelingInbound = 10,
        HandelingOutbound = 11,
        HandelingEMail = 12,
        HandelingChat = 13,
        ContactHandled = 14,
        ContactInboundHandled = 15,
        ContactOutboundHandled = 16,
        ContactEMailHandled = 17,
        ContactChatHandled = 18,

        ContactMsgSend = 19,
        ContactMsgReceived = 20,

        UndefinedTime = 21,
        PauseTime = 22,
        OffTime = 23,
        WaitingTime = 24,
        WrapupStateTime = 25,
        WrapupInboundTime = 26,
        WrapupOutboundTime = 27,
        WrapupEMailTime = 28,
        WrapupChatTime = 29,
        OnlineTime = 30,
        HandelingInboundTime = 31,
        HandelingOutboundTime = 32,
        HandelingEMailTime = 33,
        HandelingChatTime = 34,
        ContactHandledTime = 35,
        ContactInboundHandledTime = 36,
        ContactOutboundHandledTime = 37,
        ContactEMailHandledTime = 38,
        ContactChatHandledTime = 39,
        Preview = 40,
        PreviewTime = 41,
        HistoryTimeGraph = 42
    }

    internal enum AgentProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum AgentPeriodProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum InboundParametersArray
    {
        Id = 0,
        Description = 1,
        GroupKey = 2,
        CampaignId = 3,
        CampaignName = 4,
        MediaType = 5,
        MediaTypeId = 6
    }

    internal enum InboundRealtimeArray
    {
        ActiveContacts = 0,
        SystemPreprocessing = 1,
        Closing = 2,
        Ivr = 3,
        Waiting = 4,
        Online = 5,
        WrapUp = 6,
        Overflowing = 7,
        Transfer = 8,
        MaxQueueTime = 9,
        AgentInReady = 10,
        ContactMsgSend = 11,
        ContactMsgReceived = 12,
        RealTimeGraph = 13,
        AlertLevel = 14,
        NumericCustom1 = 15,
        NumericCustom2 = 16,
        NumericCustom3 = 17,
        NumericCustom4 = 18,
        NumericCustom5 = 19,
        NumericCustom6 = 20,
        NumericCustom7 = 21,
        NumericCustom8 = 22,
        NumericCustom9 = 23,
        NumericCustom10 = 24
    }

    internal enum InboundHistoryArray
    {
        Received = 0,
        ReceivedTime = 1,
        Closed = 2,
        ClosedTime = 3,
        EndInSystemProcessing = 4,
        EndInSystemProcessingTime = 5,
        EndInIvr = 6,
        EndInIvrTime = 7,
        IvrFinish = 8,
        IvrFinishTime = 9,
        IvrAbandoned = 10,
        IvrAbandonedTime = 11,
        Abandoned = 12,
        AbandonedTime = 13,
        HandledByAgent = 14,
        HandledByAgentTime = 15,
        Overflowed = 16,
        OverflowedTime = 17,
        Waiting = 18,
        WaitingTime = 19,
        Direct = 20,
        DirectTime = 21,
        Transfer = 22,
        TransferTime = 23,
        ContactMsgSend = 24,
        ContactMsgReceived = 25,
        ContactsHandledInSLATime = 26,
        PercentageContactsHandled = 27,
        PercentageContactsHandledInTime = 28,
        HistoryGraph = 29
    }

    internal enum InboundProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum InboundPeriodProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum OutboundParametersArray
    {
        Mode = 0,
        Id = 1,
        Description = 2,
        GroupKey = 3,
        CampaignId = 4,
        CampaignName = 5,
        MediaType = 6,
        MediaTypeId = 7
    }

    internal enum OutboundRealtimeArray
    {
        SystemPreprocessing = 0,
        Closing = 1,
        Ivr = 2,
        Waiting = 3,
        Online = 4,
        WrapUp = 5,
        Overflowing = 6,
        Transfer = 7,
        Preview = 8,
        RealTimeGraph = 9,
        AlertLevel = 10,
        NumericCustom1 = 11,
        NumericCustom2 = 12,
        NumericCustom3 = 13,
        NumericCustom4 = 14,
        NumericCustom5 = 15,
        NumericCustom6 = 16,
        NumericCustom7 = 17,
        NumericCustom8 = 18,
        NumericCustom9 = 19,
        NumericCustom10 = 20

    }

    internal enum OutboundHistoryArray
    {
        Dialled = 0,
        DialledTime = 1,
        EndInSystemProcessing = 2,
        EndInSystemProcessingTime = 3,
        EndInIvr = 4,
        EndInIvrTime = 5,
        IvrFinish = 6,
        IvrFinishTime = 7,
        IvrAbandoned = 8,
        IvrAbandonedTime = 9,
        Abandoned = 10,
        AbandonedTime = 11,
        Overflow = 12,
        OverflowTime = 13,
        ToAgent = 14,
        ToAgentTime = 15,
        Direct = 16,
        DirectTime = 17,
        Waiting = 18,
        WaitingTime = 19,
        Transfer = 20,
        TransferTime = 21,
        ToAgentGraph = 22,
        HistoryGraph = 23
    }

    internal enum OutboundProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum OutboundPeriodProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum OutboundContactListInfoArray
    {
        ContactCount = 0,
        ContactToDial = 1,
        ContactNeverDialed = 2,
        ContactCallbacks = 3,
        ContactToRedial = 4,
        ContactToNotRedial = 5,
        ContactListGraph = 6
    }

    internal enum QueueParametersArray
    {
        Id = 0,
        Description = 1,
        GroupKey = 2
    }

    internal enum QueueRealtimeArray
    {
        Waiting = 0,
        MaxWaiting = 1,
        AgentInReady = 2,
        AlertLevel = 3,
        NumericCustom1 = 4,
        NumericCustom2 = 5,
        NumericCustom3 = 6,
        NumericCustom4 = 7,
        NumericCustom5 = 8,
        NumericCustom6 = 9,
        NumericCustom7 = 10,
        NumericCustom8 = 11,
        NumericCustom9 = 12,
        NumericCustom10 = 13

    }

    internal enum QueueHistoryArray
    {
        Received = 0,
        ReceivedTime = 1,
        Processed = 2,
        ProcessedTime = 3,
        ProcessedDirect = 4,
        ProcessedDirectTime = 5,
        ProcessedWaiting = 6,
        ProcessedWaitingTime = 7,
        ProcessedOverflow = 8,
        ProcessedOverflowTime = 9,
        Abandoned = 10,
        AbandonedTime = 11,
        MaxQueueSize = 12,
        MaxWaitingTime = 13,
        ContactsHandledInSLATime = 14,
        PercentageContactsHandled = 15,
        PercentageContactsHandledInTime = 16,
        HistoryGraph = 17
    }

    internal enum TeamParametersArray
    {
        Id = 0,
        Description = 1,
        Group = 2,
        Agents = 3,
        Queues = 4,
        AgentsLogonGraph = 5
    }

    internal enum TeamRealtimeArray
    {
        AgentsLogon = 0,
        AgentsInPause = 1,
        AgentsInWaiting = 2,
        AgentsOnline = 3,
        AgentsInWrapup = 4,
        WaitingInQueue = 5,
        RealTimeGraph = 6,
        AlertLevel = 7,
        NumericCustom1 = 8,
        NumericCustom2 = 9,
        NumericCustom3 = 10,
        NumericCustom4 = 11,
        NumericCustom5 = 12,
        NumericCustom6 = 13,
        NumericCustom7 = 14,
        NumericCustom8 = 15,
        NumericCustom9 = 16,
        NumericCustom10 = 17

    }

    internal enum TeamProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum TeamPeriodProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum CampaignParametersArray
    {
        Id = 0,
        Description = 1,
        Group = 2,
        Inbound = 3,
        InboundCount = 4,
        Outbound = 5,
        OutboundCount = 6,
        AlertLevel = 7,
        NumericCustom1 = 8,
        NumericCustom2 = 9,
        NumericCustom3 = 10,
        NumericCustom4 = 11,
        NumericCustom5 = 12,
        NumericCustom6 = 13,
        NumericCustom7 = 14,
        NumericCustom8 = 15,
        NumericCustom9 = 16,
        NumericCustom10 = 17


    }

    internal enum CampaignProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum CampaignPeriodProductionArray
    {
        PositiveCount = 0,
        PositiveSum = 1,
        NegativeCount = 2,
        NegativeSum = 3,
        ArguedCount = 4,
        TotalQualified = 5,
        TotalNotQualified = 6,
        DialingActionTime = 7,
        DialingActionPosTime = 8,
        DialingActionNegTime = 9,
        DialingActionArgTime = 10,
        OnlineActionTime = 11,
        OnlineActionPosTime = 12,
        OnlineActionNegTime = 13,
        OnlineActionArgTime = 14,
        OnHoldActionTime = 15,
        OnHoldActionPosTime = 16,
        OnHoldActionNegTime = 17,
        OnHoldActionArgTime = 18,
        WrapUpActionTime = 19,
        WrapUpActionPosTime = 20,
        WrapUpActionNegTime = 21,
        WrapUpActionArgTime = 22,
        CompletedContacts = 23,
        CompletedContactsTime = 24,
        CommunicationActionTime = 25,
        CommunicationActionPosTime = 26,
        CommunicationActionNegTime = 27,
        CommunicationActionArgTime = 28,
        WorkActionTime = 29,
        WorkActionPosTime = 30,
        WorkActionNegTime = 31,
        WorkActionArgTime = 32,
        AvgArgComTime = 33,
        AvgArgWorkTime = 34,
        AvgPosComTime = 35,
        AvgPosWorkTime = 36,
        AvgNegComTime = 37,
        AvgNegWorkTime = 38,
        RatioTotQual_Tot = 39,
        RatioArg_Tot = 40,
        RatioArg_TotQual = 41,
        RatioPos_Tot = 42,
        RatioPos_TotQual = 43,
        RatioPos_Arg = 44,
        RatioNeg_Tot = 45,
        RatioNeg_TotQual = 46,
        RationNeg_Arg = 47,
        AvgPosValue = 48,
        AvgNegValue = 49,
        sys_Reset = 50,
        AgentHandledCount = 51,
        AvgComTime = 52,
        RatioTotQual_TotGraph = 53,
        RatioArg_TotQualGraph = 54,
        RatioPos_TotQualGraph = 55,
        RatioNeg_TotQualGraph = 56
    }

    internal enum CampaignContactListInfoArray
    {
        ContactCount = 0,
        ContactToDial = 1,
        ContactNeverDialed = 2,
        ContactCallbacks = 3,
        ContactToRedial = 4,
        ContactToNotRedial = 5,
        AvgQuotaProgress = 6        
    }

    #endregion
    #endregion

    #region Array classes

    public class AgentParameters : BaseSupervisionObject<AgentParameters>
    {
        public string AgentId
        {
            get
            {
                return StringValue((int)AgentParametersArray.AgentId);
            }
        }

        public string Account
        {
            get
            {
                return StringValue((int)AgentParametersArray.Account);
            }
        }

        public string Firstname
        {
            get
            {
                return StringValue((int)AgentParametersArray.Firstname);
            }
        }

        public string Lastname
        {
            get
            {
                return StringValue((int)AgentParametersArray.Lastname);
            }
        }

        public string SiteId
        {
            get
            {
                return StringValue((int)AgentParametersArray.SiteId);
            }
        }

        public string Server
        {
            get
            {
                return StringValue((int)AgentParametersArray.Server);
            }
        }

        public string TeamId
        {
            get
            {
                return StringValue((int)AgentParametersArray.TeamId);
            }
        }

        public string PauseId
        {
            get
            {
                return StringValue((int)AgentParametersArray.PauseId);
            }
        }

        public string PauseDescription
        {
            get
            {
                return StringValue((int)AgentParametersArray.PauseDescription);
            }
        }

        public string Groupkey
        {
            get
            {
                return StringValue((int)AgentParametersArray.Groupkey);
            }
        }

        public int Active
        {
            get
            {
                return IntValue((int)AgentParametersArray.Active);
            }
        }

        public DateTime LoginDateTime
        {
            get
            {
                return DateTimeValue((int)AgentParametersArray.LoginDateTime);
            }
        }

        public int LoginDateTimeUtc
        {
            get
            {
                return IntValue((int)AgentParametersArray.LoginDateTimeUtc);
            }
        }

        public string IpAddress
        {
            get
            {
                return StringValue((int)AgentParametersArray.IpAddress);
            }
        }

        public string Extension
        {
            get
            {
                return StringValue((int)AgentParametersArray.Extension);
            }
        }

    }

    public class AgentRealtime : BaseSupervisionObject<AgentRealtime>
    {
        public AgentRealtime()
        {
            m_SumType = SumTypes.Peak;
        }

        public int StatusIndex
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.StatusIndex);
            }
        }

        public string Status
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.Status);
            }
        }

        public DateTime StatusStartTime
        {
            get
            {
                return DateTimeValue((int)AgentRealtimeArray.StatusStartTime);
            }
        }

        public string VoiceState
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.VoiceState);
            }
        }

        public bool VoiceAvailable
        {
            get
            {
                return BoolValue((int)AgentRealtimeArray.VoiceAvailable);
            }
        }

        public string ChatState
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.ChatState);
            }
        }

        public bool ChatAvailable
        {
            get
            {
                return BoolValue((int)AgentRealtimeArray.ChatAvailable);
            }
        }

        public string EmailState
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.EmailState);
            }
        }

        public bool EmailAvailable
        {
            get
            {
                return BoolValue((int)AgentRealtimeArray.EmailAvailable);
            }
        }

        public int ListCurrentContactId
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.ListCurrentContactId);
            }
        }

        public int ActiveContactId
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.ActiveContactId);
            }
        }

        public int PerQueWaiting
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.PerQueWaiting);
            }
        }

        public int PerQueAbandoned
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.PerQueAbandoned);
            }
        }

        public int PerQueProcessed
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.PerQueProcessed);
            }
        }

        public int PerQueEnQueue
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.PerQueEnQueue);
            }
        }


        public int AlertLevel
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.AlertLevel);
            }
        }
        public int NumericCustom1
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom1);
            }
        }
        public int NumericCustom2
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom2);
            }
        }
        public int NumericCustom3
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom3);
            }
        }
        public int NumericCustom4
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom4);
            }
        }
        public int NumericCustom5
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom5);
            }
        }
        public int NumericCustom6
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom6);
            }
        }
        public int NumericCustom7
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom7);
            }
        }
        public int NumericCustom8
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom8);
            }
        }
        public int NumericCustom9
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom9);
            }
        }
        public int NumericCustom10
        {
            get
            {
                return IntValue((int)AgentRealtimeArray.NumericCustom10);
            }
        }

        public string MonitoringSupervisors
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.MonitoringSupervisors);
            }
        }
        public string RecordingSupervisors
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.RecordingSupervisors);
            }
        }
        public bool IsRecording
        {
            get
            {
                return BoolValue((int)AgentRealtimeArray.IsRecording);
            }
        }
        public string ViewingSupervisors
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.ViewingSupervisors);
            }
        }
        public string ListeningSupervisors
        {
            get
            {
                return StringValue((int)AgentRealtimeArray.ListeningSupervisors);
            }
        }

    }

    public class AgentHistory : BaseSupervisionObject<AgentHistory>
    {
        public AgentHistory()
        {
            m_SumType = SumTypes.Diff;
        }

        public int Undefined
        {
            get
            {
                return IntValue((int)AgentHistoryArray.Undefined);
            }
        }

        public int Pause
        {
            get
            {
                return IntValue((int)AgentHistoryArray.Pause);
            }
        }

        public int Off
        {
            get
            {
                return IntValue((int)AgentHistoryArray.Off);
            }
        }

        public int Waiting
        {
            get
            {
                return IntValue((int)AgentHistoryArray.Waiting);
            }
        }

        public int Wrapup
        {
            get
            {
                return IntValue((int)AgentHistoryArray.Wrapup);
            }
        }

        public int WrapupInbound
        {
            get
            {
                return IntValue((int)AgentHistoryArray.WrapupInbound);
            }
        }

        public int WrapupOutbound
        {
            get
            {
                return IntValue((int)AgentHistoryArray.WrapupOutbound);
            }
        }

        public int WrapupEMail
        {
            get
            {
                return IntValue((int)AgentHistoryArray.WrapupEMail);
            }
        }

        public int WrapupChat
        {
            get
            {
                return IntValue((int)AgentHistoryArray.WrapupChat);
            }
        }

        public int Online
        {
            get
            {
                return IntValue((int)AgentHistoryArray.Online);
            }
        }

        public int HandelingInbound
        {
            get
            {
                return IntValue((int)AgentHistoryArray.HandelingInbound);
            }
        }

        public int HandelingOutbound
        {
            get
            {
                return IntValue((int)AgentHistoryArray.HandelingOutbound);
            }
        }

        public int HandelingEMail
        {
            get
            {
                return IntValue((int)AgentHistoryArray.HandelingEMail);
            }
        }

        public int HandelingChat
        {
            get
            {
                return IntValue((int)AgentHistoryArray.HandelingChat);
            }
        }

        public int ContactHandled
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactHandled);
            }
        }

        public int ContactInboundHandled
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactInboundHandled);
            }
        }

        public int ContactOutboundHandled
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactOutboundHandled);
            }
        }

        public int ContactEMailHandled
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactEMailHandled);
            }
        }

        public int ContactChatHandled
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactChatHandled);
            }
        }

        public int ContactMsgSend
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactMsgSend);
            }
        }

        public int ContactMsgReceived
        {
            get
            {
                return IntValue((int)AgentHistoryArray.ContactMsgReceived);
            }
        }

        public TimeSpan UndefinedTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.UndefinedTime);
            }
        }

        public TimeSpan PauseTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.PauseTime);
            }
        }

        public TimeSpan OffTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.OffTime);
            }
        }

        public TimeSpan WaitingTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.WaitingTime);
            }
        }

        public TimeSpan WrapupStateTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.WrapupStateTime);
            }
        }

        public TimeSpan WrapupInboundTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.WrapupInboundTime);
            }
        }

        public TimeSpan WrapupOutboundTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.WrapupOutboundTime);
            }
        }

        public TimeSpan WrapupEMailTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.WrapupEMailTime);
            }
        }

        public TimeSpan WrapupChatTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.WrapupChatTime);
            }
        }

        public TimeSpan OnlineTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.OnlineTime);
            }
        }

        public TimeSpan HandelingInboundTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.HandelingInboundTime);
            }
        }

        public TimeSpan HandelingOutboundTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.HandelingOutboundTime);
            }
        }

        public TimeSpan HandelingEMailTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.HandelingEMailTime);
            }
        }

        public TimeSpan HandelingChatTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.HandelingChatTime);
            }
        }

        public TimeSpan ContactHandledTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.ContactHandledTime);
            }
        }

        public TimeSpan ContactInboundHandledTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.ContactInboundHandledTime);
            }
        }

        public TimeSpan ContactOutboundHandledTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.ContactOutboundHandledTime);
            }
        }

        public TimeSpan ContactEMailHandledTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.ContactEMailHandledTime);
            }
        }

        public TimeSpan ContactChatHandledTime
        {
            get
            {
                return TimeSpanValue((int)AgentHistoryArray.ContactChatHandledTime);
            }
        }

        public int HistoryTimeGraph
        {
            get
            {
                return IntValue((int)AgentHistoryArray.HistoryTimeGraph);
            }
        }

    }

    public class AgentProduction : BaseSupervisionObject<AgentProduction>
    {
        public AgentProduction()
        {
            m_SumType = SumTypes.Diff;
        }

        public double PositiveCount
        {
            get
            {
                return DoubleValue((int)AgentProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)AgentProductionArray.PositiveSum);
            }
        }

        public double NegativeCount
        {
            get
            {
                return DoubleValue((int)AgentProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)AgentProductionArray.NegativeSum);
            }
        }

        public double ArguedCount
        {
            get
            {
                return DoubleValue((int)AgentProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)AgentProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)AgentProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)AgentProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgComTime);
            }
        }


        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)AgentProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)AgentProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)AgentProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)AgentProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)AgentProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)AgentProductionArray.AgentHandledCount);
            }
        }

        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)AgentProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class AgentPeriodProduction : BaseSupervisionObject<AgentPeriodProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgComTime);
            }
        }


        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)AgentPeriodProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)AgentPeriodProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class InboundParameters : BaseSupervisionObject<InboundParameters>
    {
        public string Id
        {
            get
            {
                return StringValue((int)InboundParametersArray.Id);
            }
        }

        public string Description
        {
            get
            {
                return StringValue((int)InboundParametersArray.Description);
            }
        }

        public string GroupKey
        {
            get
            {
                return StringValue((int)InboundParametersArray.GroupKey);
            }
        }

        public string CampaignId
        {
            get
            {
                return StringValue((int)InboundParametersArray.CampaignId);
            }
        }

        public string CampaignName
        {
            get
            {
                return StringValue((int)InboundParametersArray.CampaignName);
            }
        }

    }

    public class InboundRealtime : BaseSupervisionObject<InboundRealtime>
    {
        public int ActiveContacts
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.ActiveContacts);
            }
        }

        public int SystemPreprocessing
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.SystemPreprocessing);
            }
        }

        public int Closing
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.Closing);
            }
        }

        public int Ivr
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.Ivr);
            }
        }

        public int Waiting
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.Waiting);
            }
        }

        public int Online
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.Online);
            }
        }

        public int WrapUp
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.WrapUp);
            }
        }

        public int Overflowing
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.Overflowing);
            }
        }

        public int AlertLevel
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.AlertLevel);
            }
        }
        public int NumericCustom1
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom1);
            }
        }
        public int NumericCustom2
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom2);
            }
        }
        public int NumericCustom3
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom3);
            }
        }
        public int NumericCustom4
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom4);
            }
        }
        public int NumericCustom5
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom5);
            }
        }
        public int NumericCustom6
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom6);
            }
        }
        public int NumericCustom7
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom7);
            }
        }
        public int NumericCustom8
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom8);
            }
        }
        public int NumericCustom9
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom9);
            }
        }
        public int NumericCustom10
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.NumericCustom10);
            }
        }

        public int Transfer
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.Transfer);
            }
        }

        public TimeSpan MaxQueueTime
        {
            get
            {
                return TimeSpanValue((int)InboundRealtimeArray.MaxQueueTime);
            }
            set
            {
                SetDataValue((int)InboundRealtimeArray.MaxQueueTime, value.TotalMilliseconds.ToString());
            }
        }

        public int RealTimeGraph
        {
            get
            {
                return IntValue((int)InboundRealtimeArray.RealTimeGraph);
            }
        }

    }

    public class InboundHistory : BaseSupervisionObject<InboundHistory>
    {

        public InboundHistory()
        {
            m_SumType = SumTypes.Diff;
        }

        public int Received
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Received);
            }
        }

        public TimeSpan ReceivedTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.ReceivedTime);
            }
        }

        public int Closed
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Closed);
            }
        }

        public TimeSpan ClosedTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.ClosedTime);
            }
        }

        public int EndInSystemProcessing
        {
            get
            {
                return IntValue((int)InboundHistoryArray.EndInSystemProcessing);
            }
        }

        public TimeSpan EndInSystemProcessingTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.EndInSystemProcessingTime);
            }
        }

        public int EndInIvr
        {
            get
            {
                return IntValue((int)InboundHistoryArray.EndInIvr);
            }
        }

        public TimeSpan EndInIvrTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.EndInIvrTime);
            }
        }

        public int IvrFinish
        {
            get
            {
                return IntValue((int)InboundHistoryArray.IvrFinish);
            }
        }

        public TimeSpan IvrFinishTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.IvrFinishTime);
            }
        }

        public int IvrAbandoned
        {
            get
            {
                return IntValue((int)InboundHistoryArray.IvrAbandoned);
            }
        }

        public TimeSpan IvrAbandonedTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.IvrAbandonedTime);
            }
        }

        public int Abandoned
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Abandoned);
            }
        }

        public TimeSpan AbandonedTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.AbandonedTime);
            }
        }

        public int HandledByAgent
        {
            get
            {
                return IntValue((int)InboundHistoryArray.HandledByAgent);
            }
        }

        public TimeSpan HandledByAgentTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.HandledByAgentTime);
            }
        }

        public int Overflowed
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Overflowed);
            }
        }

        public TimeSpan OverflowedTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.OverflowedTime);
            }
        }

        public int Waiting
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Waiting);
            }
        }

        public TimeSpan WaitingTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.WaitingTime);
            }
        }

        public int Direct
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Direct);
            }
        }

        public TimeSpan DirectTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.DirectTime);
            }
        }

        public int Transfer
        {
            get
            {
                return IntValue((int)InboundHistoryArray.Transfer);
            }
        }

        public TimeSpan TransferTime
        {
            get
            {
                return TimeSpanValue((int)InboundHistoryArray.TransferTime);
            }
        }

        public int HistoryGraph
        {
            get
            {
                return IntValue((int)InboundHistoryArray.HistoryGraph);
            }
        }

    }

    public class InboundProduction : BaseSupervisionObject<InboundProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)InboundProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)InboundProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)InboundProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)InboundProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)InboundProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)InboundProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)InboundProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)InboundProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgComTime);
            }
        }


        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)InboundProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)InboundProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)InboundProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)InboundProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)InboundProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)InboundProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)InboundProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class InboundPeriodProduction : BaseSupervisionObject<InboundPeriodProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgComTime);
            }
        }


        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)InboundPeriodProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)InboundPeriodProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class OutboundParameters : BaseSupervisionObject<OutboundParameters>
    {
        public int Mode
        {
            get
            {
                return IntValue((int)OutboundParametersArray.Mode);
            }
        }

        public string Id
        {
            get
            {
                return StringValue((int)OutboundParametersArray.Id);
            }
        }

        public string Description
        {
            get
            {
                return StringValue((int)OutboundParametersArray.Description);
            }
        }

        public string GroupKey
        {
            get
            {
                return StringValue((int)OutboundParametersArray.GroupKey);
            }
        }

        public string CampaignId
        {
            get
            {
                return StringValue((int)OutboundParametersArray.CampaignId);
            }
        }

        public string CampaignName
        {
            get
            {
                return StringValue((int)OutboundParametersArray.CampaignName);
            }
        }

    }

    public class OutboundRealtime : BaseSupervisionObject<OutboundRealtime>
    {
        public int SystemPreprocessing
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.SystemPreprocessing);
            }
        }

        public int Closing
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.Closing);
            }
        }

        public int Ivr
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.Ivr);
            }
        }

        public int Waiting
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.Waiting);
            }
        }

        public int Online
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.Online);
            }
        }

        public int WrapUp
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.WrapUp);
            }
        }

        public int Overflowing
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.Overflowing);
            }
        }

        public int Transfer
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.Transfer);
            }
        }

        public int RealTimeGraph
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.RealTimeGraph);
            }
        }

        public int AlertLevel
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.AlertLevel);
            }
        }
        public int NumericCustom1
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom1);
            }
        }
        public int NumericCustom2
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom2);
            }
        }
        public int NumericCustom3
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom3);
            }
        }
        public int NumericCustom4
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom4);
            }
        }
        public int NumericCustom5
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom5);
            }
        }
        public int NumericCustom6
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom6);
            }
        }
        public int NumericCustom7
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom7);
            }
        }
        public int NumericCustom8
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom8);
            }
        }
        public int NumericCustom9
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom9);
            }
        }
        public int NumericCustom10
        {
            get
            {
                return IntValue((int)OutboundRealtimeArray.NumericCustom10);
            }
        }

    }

    public class OutboundHistory : BaseSupervisionObject<OutboundHistory>
    {
        public int Dialled
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.Dialled);
            }
        }

        public TimeSpan DialledTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.DialledTime);
            }
        }

        public int EndInSystemProcessing
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.EndInSystemProcessing);
            }
        }

        public TimeSpan EndInSystemProcessingTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.EndInSystemProcessingTime);
            }
        }

        public int EndInIvr
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.EndInIvr);
            }
        }

        public TimeSpan EndInIvrTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.EndInIvrTime);
            }
        }

        public int IvrFinish
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.IvrFinish);
            }
        }

        public TimeSpan IvrFinishTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.IvrFinishTime);
            }
        }

        public int IvrAbandoned
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.IvrAbandoned);
            }
        }

        public TimeSpan IvrAbandonedTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.IvrAbandonedTime);
            }
        }

        public int Abandoned
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.Abandoned);
            }
        }

        public TimeSpan AbandonedTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.AbandonedTime);
            }
        }

        public int Overflow
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.Overflow);
            }
        }

        public TimeSpan OverflowTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.OverflowTime);
            }
        }

        public int ToAgent
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.ToAgent);
            }
        }

        public TimeSpan ToAgentTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.ToAgentTime);
            }
        }

        public int Direct
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.Direct);
            }
        }

        public TimeSpan DirectTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.DirectTime);
            }
        }

        public int Waiting
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.Waiting);
            }
        }

        public TimeSpan WaitingTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.WaitingTime);
            }
        }

        public int Transfer
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.Transfer);
            }
        }

        public TimeSpan TransferTime
        {
            get
            {
                return TimeSpanValue((int)OutboundHistoryArray.TransferTime);
            }
        }

        public int ToAgentGraph
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.ToAgentGraph);
            }
        }

        public int HistoryGraph
        {
            get
            {
                return IntValue((int)OutboundHistoryArray.HistoryGraph);
            }
        }

    }

    public class OutboundProduction : BaseSupervisionObject<OutboundProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)OutboundProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)OutboundProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)OutboundProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)OutboundProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)OutboundProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)OutboundProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)OutboundProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)OutboundProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgComTime);
            }
        }

        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)OutboundProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)OutboundProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)OutboundProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)OutboundProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)OutboundProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)OutboundProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class OutboundPeriodProduction : BaseSupervisionObject<OutboundPeriodProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgComTime);
            }
        }

        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)OutboundPeriodProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)OutboundPeriodProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class OutboundContactListInfo : BaseSupervisionObject<OutboundContactListInfo>
    {
        public int ContactCount
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactCount);
            }
        }

        public int ContactToDial
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactToDial);
            }
        }

        public int ContactNeverDialed
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactNeverDialed);
            }
        }

        public int ContactCallbacks
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactCallbacks);
            }
        }

        public int ContactToRedial
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactToRedial);
            }
        }

        public int ContactToNotRedial
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactToNotRedial);
            }
        }

        public int ContactListGraph
        {
            get
            {
                return IntValue((int)OutboundContactListInfoArray.ContactListGraph);
            }
        }

    }

    public class QueueParameters : BaseSupervisionObject<QueueParameters>
    {
        public string Id
        {
            get
            {
                return StringValue((int)QueueParametersArray.Id);
            }
        }

        public string Description
        {
            get
            {
                return StringValue((int)QueueParametersArray.Description);
            }
        }

        public string GroupKey
        {
            get
            {
                return StringValue((int)QueueParametersArray.GroupKey);
            }
        }

    }

    public class QueueRealtime : BaseSupervisionObject<QueueRealtime>
    {
        public int Waiting
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.Waiting);
            }
        }

        public long MaxWaiting
        {
            get
            {
                return LongValue((int)QueueRealtimeArray.MaxWaiting);
            }
        }

        public int AlertLevel
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.AlertLevel);
            }
        }
        public int NumericCustom1
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom1);
            }
        }
        public int NumericCustom2
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom2);
            }
        }
        public int NumericCustom3
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom3);
            }
        }
        public int NumericCustom4
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom4);
            }
        }
        public int NumericCustom5
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom5);
            }
        }
        public int NumericCustom6
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom6);
            }
        }
        public int NumericCustom7
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom7);
            }
        }
        public int NumericCustom8
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom8);
            }
        }
        public int NumericCustom9
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom9);
            }
        }
        public int NumericCustom10
        {
            get
            {
                return IntValue((int)QueueRealtimeArray.NumericCustom10);
            }
        }

    }

    public class QueueHistory : BaseSupervisionObject<QueueHistory>
    {
        public int Received
        {
            get
            {
                return IntValue((int)QueueHistoryArray.Received);
            }
        }

        public TimeSpan ReceivedTime
        {
            get
            {
                return TimeSpanValue((int)QueueHistoryArray.ReceivedTime);
            }
        }

        public int Processed
        {
            get
            {
                return IntValue((int)QueueHistoryArray.Processed);
            }
        }

        public TimeSpan ProcessedTime
        {
            get
            {
                return TimeSpanValue((int)QueueHistoryArray.ProcessedTime);
            }
        }

        public int ProcessedDirect
        {
            get
            {
                return IntValue((int)QueueHistoryArray.ProcessedDirect);
            }
        }

        public TimeSpan ProcessedDirectTime
        {
            get
            {
                return TimeSpanValue((int)QueueHistoryArray.ProcessedDirectTime);
            }
        }

        public int ProcessedWaiting
        {
            get
            {
                return IntValue((int)QueueHistoryArray.ProcessedWaiting);
            }
        }

        public TimeSpan ProcessedWaitingTime
        {
            get
            {
                return TimeSpanValue((int)QueueHistoryArray.ProcessedWaitingTime);
            }
        }

        public int Abandoned
        {
            get
            {
                return IntValue((int)QueueHistoryArray.Abandoned);
            }
        }

        public TimeSpan AbandonedTime
        {
            get
            {
                return TimeSpanValue((int)QueueHistoryArray.AbandonedTime);
            }
        }

        public int MaxQueueSize
        {
            get
            {
                return IntValue((int)QueueHistoryArray.MaxQueueSize);
            }
        }

        public TimeSpan MaxWaitingTime
        {
            get
            {
                return TimeSpanValue((int)QueueHistoryArray.MaxWaitingTime);
            }
        }

        public int HistoryGraph
        {
            get
            {
                return IntValue((int)QueueHistoryArray.HistoryGraph);
            }
        }

    }

    public class TeamParameters : BaseSupervisionObject<TeamParameters>
    {
        public string Id
        {
            get
            {
                return StringValue((int)TeamParametersArray.Id);
            }
        }

        public string Description
        {
            get
            {
                return StringValue((int)TeamParametersArray.Description);
            }
        }

        public string Group
        {
            get
            {
                return StringValue((int)TeamParametersArray.Group);
            }
        }

        public string Agents
        {
            get
            {
                return StringValue((int)TeamParametersArray.Agents);
            }
        }

        public string Queues
        {
            get
            {
                return StringValue((int)TeamParametersArray.Queues);
            }
        }

        public int AgentsLogonGraph
        {
            get
            {
                return IntValue((int)TeamParametersArray.AgentsLogonGraph);
            }
        }

    }

    public class TeamRealtime : BaseSupervisionObject<TeamRealtime>
    {
        public int AgentsLogon
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.AgentsLogon);
            }
        }

        public int AgentsInPause
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.AgentsInPause);
            }
        }

        public int AgentsInWaiting
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.AgentsInWaiting);
            }
        }

        public int AgentsOnline
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.AgentsOnline);
            }
        }

        public int AgentsInWrapup
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.AgentsInWrapup);
            }
        }

        public int WaitingInQueue
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.WaitingInQueue);
            }
        }

        public int RealTimeGraph
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.RealTimeGraph);
            }
        }

        public int AlertLevel
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.AlertLevel);
            }
        }
        public int NumericCustom1
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom1);
            }
        }
        public int NumericCustom2
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom2);
            }
        }
        public int NumericCustom3
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom3);
            }
        }
        public int NumericCustom4
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom4);
            }
        }
        public int NumericCustom5
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom5);
            }
        }
        public int NumericCustom6
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom6);
            }
        }
        public int NumericCustom7
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom7);
            }
        }
        public int NumericCustom8
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom8);
            }
        }
        public int NumericCustom9
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom9);
            }
        }
        public int NumericCustom10
        {
            get
            {
                return IntValue((int)TeamRealtimeArray.NumericCustom10);
            }
        }

    }

    public class TeamProduction : BaseSupervisionObject<TeamProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)TeamProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)TeamProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)TeamProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)TeamProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)TeamProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)TeamProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)TeamProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)TeamProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgComTime);
            }
        }


        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)TeamProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)TeamProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)TeamProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)TeamProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)TeamProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)TeamProductionArray.AgentHandledCount);
            }
        }

        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)TeamProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class TeamPeriodProduction : BaseSupervisionObject<TeamPeriodProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgComTime);
            }
        }


        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)TeamPeriodProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.AgentHandledCount);
            }
        }

        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)TeamPeriodProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class CampaignParameters : BaseSupervisionObject<CampaignParameters>
    {
        public string Id
        {
            get
            {
                return StringValue((int)CampaignParametersArray.Id);
            }
        }

        public string Description
        {
            get
            {
                return StringValue((int)CampaignParametersArray.Description);
            }
        }

        public string Group
        {
            get
            {
                return StringValue((int)CampaignParametersArray.Group);
            }
        }

        public string Inbound
        {
            get
            {
                return StringValue((int)CampaignParametersArray.Inbound);
            }
        }

        public int InboundCount
        {
            get
            {
                return IntValue((int)CampaignParametersArray.InboundCount);
            }
        }

        public string Outbound
        {
            get
            {
                return StringValue((int)CampaignParametersArray.Outbound);
            }
        }

        public int OutboundCount
        {
            get
            {
                return IntValue((int)CampaignParametersArray.OutboundCount);
            }
        }

        public int AlertLevel
        {
            get
            {
                return IntValue((int)CampaignParametersArray.AlertLevel);
            }
        }
        public int NumericCustom1
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom1);
            }
        }
        public int NumericCustom2
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom2);
            }
        }
        public int NumericCustom3
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom3);
            }
        }
        public int NumericCustom4
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom4);
            }
        }
        public int NumericCustom5
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom5);
            }
        }
        public int NumericCustom6
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom6);
            }
        }
        public int NumericCustom7
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom7);
            }
        }
        public int NumericCustom8
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom8);
            }
        }
        public int NumericCustom9
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom9);
            }
        }
        public int NumericCustom10
        {
            get
            {
                return IntValue((int)CampaignParametersArray.NumericCustom10);
            }
        }

    }

    public class CampaignProduction : BaseSupervisionObject<CampaignProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)CampaignProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)CampaignProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)CampaignProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)CampaignProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)CampaignProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)CampaignProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)CampaignProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)CampaignProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgComTime);
            }
        }

        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)CampaignProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)CampaignProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)CampaignProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)CampaignProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)CampaignProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)CampaignProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class CampaignPeriodProduction : BaseSupervisionObject<CampaignPeriodProduction>
    {
        public int PositiveCount
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.PositiveCount);
            }
        }

        public int PositiveSum
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.PositiveSum);
            }
        }

        public int NegativeCount
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.NegativeCount);
            }
        }

        public int NegativeSum
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.NegativeSum);
            }
        }

        public int ArguedCount
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.ArguedCount);
            }
        }

        public int TotalQualified
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.TotalQualified);
            }
        }

        public int TotalNotQualified
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.TotalNotQualified);
            }
        }

        public TimeSpan DialingActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.DialingActionTime);
            }
        }

        public TimeSpan DialingActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.DialingActionPosTime);
            }
        }

        public TimeSpan DialingActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.DialingActionNegTime);
            }
        }

        public TimeSpan DialingActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.DialingActionArgTime);
            }
        }

        public TimeSpan OnlineActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnlineActionTime);
            }
        }

        public TimeSpan OnlineActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnlineActionPosTime);
            }
        }

        public TimeSpan OnlineActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnlineActionNegTime);
            }
        }

        public TimeSpan OnlineActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnlineActionArgTime);
            }
        }

        public TimeSpan OnHoldActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnHoldActionTime);
            }
        }

        public TimeSpan OnHoldActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnHoldActionPosTime);
            }
        }

        public TimeSpan OnHoldActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnHoldActionNegTime);
            }
        }

        public TimeSpan OnHoldActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.OnHoldActionArgTime);
            }
        }

        public TimeSpan WrapUpActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WrapUpActionTime);
            }
        }

        public TimeSpan WrapUpActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WrapUpActionPosTime);
            }
        }

        public TimeSpan WrapUpActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WrapUpActionNegTime);
            }
        }

        public TimeSpan WrapUpActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WrapUpActionArgTime);
            }
        }

        public TimeSpan CommunicationActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.CommunicationActionTime);
            }
        }

        public TimeSpan CommunicationActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.CommunicationActionPosTime);
            }
        }

        public TimeSpan CommunicationActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.CommunicationActionNegTime);
            }
        }

        public TimeSpan CommunicationActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.CommunicationActionArgTime);
            }
        }

        public TimeSpan WorkActionTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WorkActionTime);
            }
        }

        public TimeSpan WorkActionPosTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WorkActionPosTime);
            }
        }

        public TimeSpan WorkActionNegTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WorkActionNegTime);
            }
        }

        public TimeSpan WorkActionArgTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.WorkActionArgTime);
            }
        }

        public int CompletedContacts
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.CompletedContacts);
            }
        }

        public TimeSpan CompletedContactsTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.CompletedContactsTime);
            }
        }

        public TimeSpan AvgArgComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgArgComTime);
            }
        }

        public TimeSpan AvgArgWorkTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgArgWorkTime);
            }
        }

        public TimeSpan AvgPosComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgPosComTime);
            }
        }

        public TimeSpan AvgComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgComTime);
            }
        }

        public TimeSpan AvgPosWorkTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgPosWorkTime);
            }
        }

        public TimeSpan AvgNegComTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgNegComTime);
            }
        }

        public TimeSpan AvgNegWorkTime
        {
            get
            {
                return TimeSpanValue((int)CampaignPeriodProductionArray.AvgNegWorkTime);
            }
        }

        public int RatioTotQual_Tot
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioTotQual_Tot);
            }
        }

        public int RatioArg_Tot
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioArg_Tot);
            }
        }

        public int RatioArg_TotQual
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioArg_TotQual);
            }
        }

        public int RatioPos_Tot
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioPos_Tot);
            }
        }

        public int RatioPos_TotQual
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioPos_TotQual);
            }
        }

        public int RatioPos_Arg
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioPos_Arg);
            }
        }

        public int RatioNeg_Tot
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioNeg_Tot);
            }
        }

        public int RatioNeg_TotQual
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioNeg_TotQual);
            }
        }

        public int RationNeg_Arg
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RationNeg_Arg);
            }
        }

        public int AvgPosValue
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.AvgPosValue);
            }
        }

        public int AvgNegValue
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.AvgNegValue);
            }
        }

        public int sys_Reset
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.sys_Reset);
            }
        }

        public int AgentHandledCount
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.AgentHandledCount);
            }
        }


        public int RatioTotQual_TotGraph
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioTotQual_TotGraph);
            }
        }

        public int RatioArg_TotQualGraph
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioArg_TotQualGraph);
            }
        }

        public int RatioPos_TotQualGraph
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioPos_TotQualGraph);
            }
        }

        public int RatioNeg_TotQualGraph
        {
            get
            {
                return IntValue((int)CampaignPeriodProductionArray.RatioNeg_TotQualGraph);
            }
        }
    }

    public class CampaignContactListInfo : BaseSupervisionObject<CampaignContactListInfo>
    {
        public int ContactCount
        {
            get
            {
                return IntValue((int)CampaignContactListInfoArray.ContactCount);
            }
        }

        public int ContactToDial
        {
            get
            {
                return IntValue((int)CampaignContactListInfoArray.ContactToDial);
            }
        }

        public int ContactNeverDialed
        {
            get
            {
                return IntValue((int)CampaignContactListInfoArray.ContactNeverDialed);
            }
        }

        public int ContactCallbacks
        {
            get
            {
                return IntValue((int)CampaignContactListInfoArray.ContactCallbacks);
            }
        }

        public int ContactToRedial
        {
            get
            {
                return IntValue((int)CampaignContactListInfoArray.ContactToRedial);
            }
        }

        public int ContactToNotRedial
        {
            get
            {
                return IntValue((int)CampaignContactListInfoArray.ContactToNotRedial);
            }
        }

    }

    #endregion

    #region Elements

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

        internal abstract void UpdateParameters(string[] rawData);
        internal abstract void UpdateRealTime(string[] rawData);
        internal abstract void UpdateHistory(string[] rawData);
        internal abstract void UpdateProduction(string[] rawData);
        internal abstract void UpdatePeriodProduction(string[] rawData);

        internal virtual void UpdateTimers()
        {
        }
    }

    public class AgentData : BaseItemData
    {
        private AgentParameters m_Parameters = new AgentParameters();
        private AgentRealtime m_RealTime = new AgentRealtime();
        private AgentHistory m_History = new AgentHistory();
        private AgentProduction m_Production = new AgentProduction();
        private AgentPeriodProduction m_PeriodProduction = new AgentPeriodProduction();

        public AgentData()
        {
        }

        public AgentData(string key)
            : base(key)
        {
        }

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

        internal override void UpdateParameters(string[] rawData)
        {
            m_Parameters.SetRawData(rawData);
        }

        internal override void UpdateRealTime(string[] rawData)
        {
            m_RealTime.SetRawData(rawData);
        }

        internal override void UpdateHistory(string[] rawData)
        {
            m_History.SetRawData(rawData);
        }

        internal override void UpdateProduction(string[] rawData)
        {
            m_Production.SetRawData(rawData);
        }

        internal override void UpdatePeriodProduction(string[] rawData)
        {
            m_PeriodProduction.SetRawData(rawData);
        }
    }
    public class InboundData : BaseItemData
    {
        private InboundParameters m_Parameters = new InboundParameters();
        private InboundRealtime m_RealTime = new InboundRealtime();
        private InboundHistory m_History = new InboundHistory();
        private InboundProduction m_Production = new InboundProduction();
        private InboundPeriodProduction m_PeriodProduction = new InboundPeriodProduction();

        public InboundData()
        {
        }

        public InboundData(string key)
            : base(key)
        {
        }

        public InboundParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public InboundRealtime RealTime
        {
            get
            {
                return m_RealTime;
            }
        }

        public InboundHistory History
        {
            get
            {
                return m_History;
            }
        }

        public InboundProduction Production
        {
            get
            {
                return m_Production;
            }
        }

        public InboundPeriodProduction PeriodProduction
        {
            get
            {
                return m_PeriodProduction;
            }
        }

        internal override void UpdateParameters(string[] rawData)
        {
            m_Parameters.SetRawData(rawData);
        }

        internal override void UpdateRealTime(string[] rawData)
        {
            m_RealTime.SetRawData(rawData);
        }

        internal override void UpdateHistory(string[] rawData)
        {
            m_History.SetRawData(rawData);
        }

        internal override void UpdateProduction(string[] rawData)
        {
            m_Production.SetRawData(rawData);
        }

        internal override void UpdatePeriodProduction(string[] rawData)
        {
            m_PeriodProduction.SetRawData(rawData);
        }
    }
    public class OutboundData : BaseItemData
    {
        private OutboundParameters m_Parameters = new OutboundParameters();
        private OutboundRealtime m_RealTime = new OutboundRealtime();
        private OutboundHistory m_History = new OutboundHistory();
        private OutboundProduction m_Production = new OutboundProduction();
        private OutboundPeriodProduction m_PeriodProduction = new OutboundPeriodProduction();
        private OutboundContactListInfo m_ContactListInfo = new OutboundContactListInfo();

        public OutboundData()
        {
        }

        public OutboundData(string key)
            : base(key)
        {
        }

        public OutboundParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public OutboundRealtime RealTime
        {
            get
            {
                return m_RealTime;
            }
        }

        public OutboundHistory History
        {
            get
            {
                return m_History;
            }
        }

        public OutboundProduction Production
        {
            get
            {
                return m_Production;
            }
        }

        public OutboundPeriodProduction PeriodProduction
        {
            get
            {
                return m_PeriodProduction;
            }
        }

        public OutboundContactListInfo ContactListInfo
        {
            get
            {
                return m_ContactListInfo;
            }
        }

        internal override void UpdateParameters(string[] rawData)
        {
            m_Parameters.SetRawData(rawData);
        }

        internal override void UpdateRealTime(string[] rawData)
        {
            m_RealTime.SetRawData(rawData);
        }

        internal override void UpdateHistory(string[] rawData)
        {
            m_History.SetRawData(rawData);
        }

        internal override void UpdateProduction(string[] rawData)
        {
            m_Production.SetRawData(rawData);
        }

        internal override void UpdatePeriodProduction(string[] rawData)
        {
            m_PeriodProduction.SetRawData(rawData);
        }
    }
    public class QueueData : BaseItemData
    {
        private QueueParameters m_Parameters = new QueueParameters();
        private QueueRealtime m_RealTime = new QueueRealtime();
        private QueueHistory m_History = new QueueHistory();

        public QueueData()
        {
        }

        public QueueData(string key)
            : base(key)
        {
        }

        public QueueParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public QueueRealtime RealTime
        {
            get
            {
                return m_RealTime;
            }
        }

        public QueueHistory History
        {
            get
            {
                return m_History;
            }
        }

        internal override void UpdateParameters(string[] rawData)
        {
            m_Parameters.SetRawData(rawData);
        }

        internal override void UpdateRealTime(string[] rawData)
        {
            m_RealTime.SetRawData(rawData);
        }

        internal override void UpdateHistory(string[] rawData)
        {
            m_History.SetRawData(rawData);
        }

        internal override void UpdateProduction(string[] rawData)
        {
            //m_Production.SetRawData(rawData);
        }

        internal override void UpdatePeriodProduction(string[] rawData)
        {
            //m_PeriodProduction.SetRawData(rawData);
        }
    }
    public class TeamData : BaseItemData
    {
        private TeamParameters m_Parameters = new TeamParameters();
        private TeamRealtime m_RealTime = new TeamRealtime();
        private TeamProduction m_Production = new TeamProduction();
        private TeamPeriodProduction m_PeriodProduction = new TeamPeriodProduction();

        public TeamData()
        {
        }

        public TeamData(string key)
            : base(key)
        {
        }

        public TeamParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public TeamRealtime RealTime
        {
            get
            {
                return m_RealTime;
            }
        }

        public TeamProduction Production
        {
            get
            {
                return m_Production;
            }
        }

        public TeamPeriodProduction PeriodProduction
        {
            get
            {
                return m_PeriodProduction;
            }
        }

        internal override void UpdateParameters(string[] rawData)
        {
            m_Parameters.SetRawData(rawData);
        }

        internal override void UpdateRealTime(string[] rawData)
        {
            m_RealTime.SetRawData(rawData);
        }

        internal override void UpdateHistory(string[] rawData)
        {
            //m_History.SetRawData(rawData);
        }

        internal override void UpdateProduction(string[] rawData)
        {
            m_Production.SetRawData(rawData);
        }

        internal override void UpdatePeriodProduction(string[] rawData)
        {
            m_PeriodProduction.SetRawData(rawData);
        }
    }
    public class CampaignData : BaseItemData
    {
        private CampaignParameters m_Parameters = new CampaignParameters();
        private CampaignProduction m_Production = new CampaignProduction();
        private CampaignPeriodProduction m_PeriodProduction = new CampaignPeriodProduction();
        private CampaignContactListInfo m_ContactListInfo = new CampaignContactListInfo();

        public CampaignData()
        {
        }

        public CampaignData(string key)
            : base(key)
        {
        }

        public CampaignParameters Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        public CampaignProduction Production
        {
            get
            {
                return m_Production;
            }
        }

        public CampaignPeriodProduction PeriodProduction
        {
            get
            {
                return m_PeriodProduction;
            }
        }

        public CampaignContactListInfo ContactListInfo
        {
            get
            {
                return m_ContactListInfo;
            }
        }

        internal override void UpdateParameters(string[] rawData)
        {
            m_Parameters.SetRawData(rawData);
        }

        internal override void UpdateRealTime(string[] rawData)
        {
            //m_RealTime.SetRawData(rawData);
        }

        internal override void UpdateHistory(string[] rawData)
        {
            //m_History.SetRawData(rawData);
        }

        internal override void UpdateProduction(string[] rawData)
        {
            m_Production.SetRawData(rawData);
        }

        internal override void UpdatePeriodProduction(string[] rawData)
        {
            m_PeriodProduction.SetRawData(rawData);
        }
    }
    #endregion

    #region Collections

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

            for (int i = 0; i < this.Count; i++)
            {
                m_Total.Parameters.SetRawData(this[i].Parameters.AddTo(m_Total.Parameters).RawData);
                m_Total.RealTime.SetRawData(this[i].RealTime.AddTo(m_Total.RealTime).RawData);
                m_Total.History.SetRawData(this[i].History.AddTo(m_Total.History).RawData);
                m_Total.Production.SetRawData(this[i].Production.AddTo(m_Total.Production).RawData);
                m_Total.PeriodProduction.SetRawData(this[i].PeriodProduction.AddTo(m_Total.PeriodProduction).RawData);
            }

            return m_Total;
        }
    }

    public delegate void OutboundDataChangedDelegate(OutboundData outbound);
    public class OutboundCollection : BaseItemCollection<OutboundData>
    {
        public event OutboundDataChangedDelegate OutboundDataChanged;

        internal void OnOutboundDataChanged(OutboundData outbound)
        {
            if (OutboundDataChanged != null)
            {
                try
                {
                    OutboundDataChanged(outbound);
                }
                catch
                {
                }
            }
        }
    }

    public delegate void QueueDataChangedDelegate(QueueData queue);
    public class QueueCollection : BaseItemCollection<QueueData>
    {
        public event QueueDataChangedDelegate QueueDataChanged;

        internal void OnQueueDataChanged(QueueData queue)
        {
            if (QueueDataChanged != null)
            {
                try
                {
                    QueueDataChanged(queue);
                }
                catch
                {
                }
            }
        }

        public QueueData GetTotal()
        {
            QueueData m_Total = new QueueData();

            for (int i = 0; i < this.Count; i++)
            {
                m_Total.Parameters.SetRawData(this[i].Parameters.AddTo(m_Total.Parameters).RawData);
                m_Total.RealTime.SetRawData(this[i].RealTime.AddTo(m_Total.RealTime).RawData);
                m_Total.History.SetRawData(this[i].History.AddTo(m_Total.History).RawData);
            }

            return m_Total;
        }
    }

    public delegate void TeamDataChangedDelegate(TeamData team);
    public class TeamDataCollection : BaseItemCollection<TeamData>
    {
        public event TeamDataChangedDelegate TeamDataChanged;

        internal void OnTeamDataChanged(TeamData team)
        {
            if (TeamDataChanged != null)
            {
                try
                {
                    TeamDataChanged(team);
                }
                catch
                {
                }
            }
        }

        public string[] getName(string[] id)
        {
            string[] _Names = new string[id.Length];

            for (int i = 0; i < id.Length; i++)
            {
                if (this.Contains(id[i]))
                    _Names[i] = this[id[i]].Parameters.Description;
                else
                    _Names[i] = "";
            }

            return _Names;
        }
    }

    public delegate void CampaignDataChangedDelegate(CampaignData campaign);
    public class CampaignCollection : BaseItemCollection<CampaignData>
    {
        public event CampaignDataChangedDelegate CampaignDataChanged;

        internal void OnCampaignDataChanged(CampaignData campaign)
        {
            if (CampaignDataChanged != null)
            {
                try
                {
                    CampaignDataChanged(campaign);
                }
                catch
                {
                }
            }
        }
    }

    #endregion

    [Flags]
    public enum SupervisionItemTypes
    {
        Undefined = 0,
        Agent = 1,
        Team = 2,
        Queue = 4,
        Campaign = 8,
        Inbound = 16,
        Outbound = 32,
        SupervisionConfig = 16384
    }


    public delegate void AgentWarningEventHandler(string message);
    public class ClientSupervision
    {
        #region Events
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
        #endregion

        #region Class data
        private WeakReference m_LinkReference;
        protected AgentCollection m_Agents = new AgentCollection();
        protected InboundCollection m_Inbounds = new InboundCollection();
        protected OutboundCollection m_Outbounds = new OutboundCollection();
        protected QueueCollection m_Queues = new QueueCollection();
        protected TeamDataCollection m_Teams = new TeamDataCollection();
        protected CampaignCollection m_Campaigns = new CampaignCollection();

        #endregion

        #region Constructor
        public ClientSupervision(HttpLink link)
        {
            m_LinkReference = new WeakReference(link, false);

            link.ServerEvent += new HttpLinkServerEventDelegate(Link_ServerEvent);

        }
        #endregion

        #region Properties
        public AgentCollection Agents
        {
            get { return m_Agents; }
        }
        public InboundCollection Inbounds
        {
            get { return m_Inbounds; }
        }
        public OutboundCollection Outbounds
        {
            get { return m_Outbounds; }
        }
        public QueueCollection Queues
        {
            get { return m_Queues; }
        }
        public TeamDataCollection Teams
        {
            get { return m_Teams; }
        }
        public CampaignCollection Campaigns
        {
            get { return m_Campaigns; }
        }
        #endregion

        #region Members
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

            if (eventArgs.EventCode == EventCodes.AgentWarning)
            {
                #region AgentWarning
                string msg = eventArgs.Parameters.Substring(SepPos + 1);
                System.Diagnostics.Trace.WriteLine("Info msg: " + msg);
                OnAgentWarningMsg(msg);
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
                                AgentData Agent = m_Agents.GetOrAdd(ItemId);
                                Agent.UpdateParameters(RawData);
                                break;
                            case SupervisionItemTypes.Inbound:
                                InboundData Inbound = m_Inbounds.GetOrAdd(ItemId);
                                Inbound.UpdateParameters(RawData);
                                break;
                            case SupervisionItemTypes.Outbound:
                                OutboundData Outbound = m_Outbounds.GetOrAdd(ItemId);
                                Outbound.UpdateParameters(RawData);
                                break;
                            case SupervisionItemTypes.Queue:
                                QueueData Queue = m_Queues.GetOrAdd(ItemId);
                                Queue.UpdateParameters(RawData);
                                break;
                            case SupervisionItemTypes.Team:
                                TeamData Team = m_Teams.GetOrAdd(ItemId);
                                Team.UpdateParameters(RawData);
                                break;
                            case SupervisionItemTypes.Campaign:
                                CampaignData Campaign = m_Campaigns.GetOrAdd(ItemId);
                                Campaign.UpdateParameters(RawData);
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
                                    AgentData Agent = m_Agents.Get(ItemId);

                                    if (Agent != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            Agent.UpdateRealTime(RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Agent.UpdateHistory(RawData);
                                        }
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            Agent.UpdateProduction(RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            Agent.UpdatePeriodProduction(RawData);

                                        m_Agents.OnAgentDataChanged(Agent);
                                    }
                                    break;
                                case SupervisionItemTypes.Inbound:
                                    InboundData Inbound = m_Inbounds.Get(ItemId);

                                    if (Inbound != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            Inbound.UpdateRealTime(RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                            Inbound.UpdateHistory(RawData);
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            Inbound.UpdateProduction(RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            Inbound.UpdatePeriodProduction(RawData);

                                        m_Inbounds.OnInboundDataChanged(Inbound);
                                    }
                                    break;
                                case SupervisionItemTypes.Outbound:
                                    OutboundData Outbound = m_Outbounds.Get(ItemId);

                                    if (Outbound != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            Outbound.UpdateRealTime(RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                            Outbound.UpdateHistory(RawData);
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            Outbound.UpdateProduction(RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            Outbound.UpdatePeriodProduction(RawData);
                                        if (DataType.Equals("@@ContactListInfo ", StringComparison.OrdinalIgnoreCase))
                                            Outbound.UpdatePeriodProduction(RawData);

                                        m_Outbounds.OnOutboundDataChanged(Outbound);
                                    }
                                    break;
                                case SupervisionItemTypes.Queue:
                                    QueueData Queue = m_Queues.Get(ItemId);

                                    if (Queue != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            Queue.UpdateRealTime(RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                            Queue.UpdateHistory(RawData);
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            Queue.UpdateProduction(RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            Queue.UpdatePeriodProduction(RawData);

                                        m_Queues.OnQueueDataChanged(Queue);
                                    }
                                    break;
                                case SupervisionItemTypes.Team:
                                    TeamData Team = m_Teams.Get(ItemId);

                                    if (Team != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            Team.UpdateRealTime(RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                            Team.UpdateHistory(RawData);
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            Team.UpdateProduction(RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            Team.UpdatePeriodProduction(RawData);

                                        m_Teams.OnTeamDataChanged(Team);
                                    }
                                    break;
                                case SupervisionItemTypes.Campaign:
                                    CampaignData Campaign = m_Campaigns.Get(ItemId);

                                    if (Campaign != null)
                                    {
                                        if (DataType.Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
                                            Campaign.UpdateRealTime(RawData);
                                        if (DataType.Equals("@@History", StringComparison.OrdinalIgnoreCase))
                                            Campaign.UpdateHistory(RawData);
                                        if (DataType.Equals("@@Production", StringComparison.OrdinalIgnoreCase))
                                            Campaign.UpdateProduction(RawData);
                                        if (DataType.Equals("@@PeriodProduction", StringComparison.OrdinalIgnoreCase))
                                            Campaign.UpdatePeriodProduction(RawData);
                                        if (DataType.Equals("@@ContactListInfo ", StringComparison.OrdinalIgnoreCase))
                                            Campaign.UpdatePeriodProduction(RawData);

                                        m_Campaigns.OnCampaignDataChanged(Campaign);
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
}
