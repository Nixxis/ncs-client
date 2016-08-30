using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Xml;

namespace ContactRoute
{

    public enum SupervisionCategory
    {
        Unknwon,
        Campaign,
        Agent,
        Queue,
        Inbound,
        Outbound,
        Team,
        Alert
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumDefaultValueAttribute : Attribute
    {
        public object Value;

        public EnumDefaultValueAttribute(object value)
        {
            Value = value;
        }
    }

    // TODO:synchronize that with client
    // here we have a duplicate of enums
    public enum SystemFieldMeanings
    {
        Internal__Id__,
        CurrentActivity,
        PreviousActivity,
        LastHandlerActivity,
        LastContactId,
        LastActivityChange,
        LastHandler,
        LastHandlingTime,
        LastHandlingDuration,
        TotalHandlingDuration,
        TotalHandlers,
        State,
        PreviousState,
        SortInfo,
        CustomSortInfo,
        Priority,
        DialingModeOverride,
        DialStartDate,
        DialEndDate,
        CreationTime,
        ImportSequence,
        ImportTag,
        ExportSequence,
        RecycleCount,
        LastRecycle,
        ExportTime,
        TargetHandler,
        PreferredAgent,
        TargetDestination,
        TargetMedia,
        DialedCurrentActivity,
        TotalDialed,
        MaxDialAttempts,
        ExpectedProfit,
        LastDialStatus,
        LastDialStatusCount,
        LastDialedDestination,
        LastQualification,
        LastQualificationArgued,
        LastQualificationPositive,
        LastQualificationExportable,
        AreaId,
        AppointmentId,
        Excluded,
        VMFlagged
    }

    public enum QuotaComputationMethod
    {
        Unknown,
        DirrectValue,
        Condition,
        FieldValue,
        Workflow,
        Formula
    }


    public enum RecordingMode
    {
        Standard = 0,
        ManagerOnCustomerConnected = 1
    }
    /// <summary>
    /// Defines the methods that can be used for distributing items waiting in queue.
    /// </summary>
    public enum DistributionMethod
    {
        /// <summary>
        /// Gives the item to agent waiting for the longest time.
        /// </summary>
        LongestIdle = 0,    
        /// <summary>
        /// Gives the item to agent with biggest total wait duration.
        /// </summary>
        MostIdle = 1,       
        /// <summary>
        /// Gives the item to agent with bigger ratio total wait duration / total session time.
        /// </summary>
        MostIdleRatio = 2           
    }
    /// <summary>
    /// Defines actions that can be linked with qualifications.
    /// </summary>
    public enum QualificationAction
    {
        /// <summary>
        /// No action is executed.
        /// </summary>
        None,
        /// <summary>
        /// The record corresponding to the current contatct will not be called anymore.
        /// </summary>
        DoNotRetry,
        /// <summary>
        /// The record corresponding to the current contact will be called back at a particular moment.
        /// </summary>
        RetryAt,
        /// <summary>
        /// The record corresponding to the current contact will be called again but not before some delay.
        /// </summary>
        RetryNotBefore,
        /// <summary>
        /// The record corresponding to the current contact will be associated to a callback.
        /// </summary>
        Callback,
        /// <summary>
        /// The record corresponding to the current contact will be associated to a targeted callback.
        /// </summary>
        TargetedCallback,
        /// <summary>
        /// The record corresponding to the current contact will be associated to an activity change.
        /// </summary>
        ChangeActivity,
        /// <summary>
        /// The record corresponding to the current contact will be black listed.
        /// </summary>
        BlackList
    }
    /// <summary>
    /// Defines how the adbandon rate must be computed.
    /// </summary>
    public enum AbandonRateMode
    {
        /// <summary>
        /// The abandon rate is the ratio of abandoned calls in regard to the sum of abandoned calls and calls handled by agents.
        /// </summary>
        Standard =   0,
        /// The abandon rate is the ratio of abandoned calls in regard to the sum of abandoned calls, calls handled by agents and calls considered to be answering machines.
        AnsweringMachineIncluded = 1,
        /// <summary>
        /// The abandon rate is the ratio of abandoned calls in regard to the total number of dialed calls.
        /// </summary>
        EveryDialedIncluded = 2
    }

    /// <summary>
    /// Defines the voice allowed agents actions.
    /// </summary>
    [Flags]
    public enum VoiceAllowedActions
    {
        /// <summary>
        /// No action can be performed.
        /// </summary>
        None = 0,
        /// <summary>
        /// Hangup can be performed.
        /// </summary>
        Hangup = 1,
        /// <summary>
        /// Hold can be performed.
        /// </summary>
        Hold = 2,
        /// <summary>
        /// Retrieve an held call can be performed.
        /// </summary>
        Retrieve = 4,
        /// <summary>
        /// Transfer can be performed.
        /// </summary>
        Transfer = 8,
        /// <summary>
        /// Forward can be performed.
        /// </summary>
        Forward = 16,
        /// <summary>
        /// Conference can be performed.
        /// </summary>
        Conference = 32,
        /// <summary>
        /// New call can be performed.
        /// </summary>
        NewCall = 64
    }

    /// <summary>
    /// Defines the media types.
    /// </summary>
    [Flags]
    public enum MediaType
    {
        /// <summary>
        /// No media.
        /// </summary>
        None = 0,
        /// <summary>
        /// Voice media.
        /// </summary>
        Voice = 1,
        /// <summary>
        /// Chat media.
        /// </summary>
        Chat = 2,
        /// <summary>
        /// Mail media.
        /// </summary>
        Mail = 4,
        /// <summary>
        /// Reserved for integrations.
        /// </summary>
        Custom1 = 8,
        /// <summary>
        /// Reserved for integrations.
        /// </summary>
        Custom2 = 16,
        /// <summary>
        /// All medias.
        /// </summary>
        All = Voice | Chat | Mail | Custom1 | Custom2
    }

    /// <summary>
    /// Defines the contact states.
    /// </summary>
    public enum ContactState
    {
        /// <summary>
        /// None or unknown state.
        /// </summary>
        None = 0,
        /// <summary>
        /// A fax has been detected.
        /// </summary>
        Fax = 1,
        /// <summary>
        /// A wrong number has been detected.
        /// </summary>
        Disturbed = 2,
        /// <summary>
        /// The dial attempt was not answered.
        /// </summary>
        NoAnswer = 3,
        /// <summary>
        /// An answering machine has been detected.
        /// </summary>
        AnsweringMachine = 4,
        /// <summary>
        /// The dial attempt ended in a busy signal.
        /// </summary>
        Busy = 5,
        /// <summary>
        /// The dial attempt was connected but was dropped while waiting in queue.
        /// </summary>
        Abandoned = 6,
        /// <summary>
        /// The callback validity was elapsed.
        /// </summary>
        ValidityEllapsed = 7,
        /// <summary>
        /// The call was dropped by a normal clearing.
        /// </summary>
        NormalClearing = 8,
        /// <summary>
        /// The call was handled by an agent.
        /// </summary>
        Agent = 9,
        /// <summary>
        /// The agent did not dial the previewed record.
        /// </summary>
        AgentUnavailable = 10,
        /// <summary>
        /// The dialing ended in a congestion.
        /// </summary>
        Congestion = 11,
        /// <summary>
        /// Not used anymore. Kept by backward compatibility.
        /// </summary>
        NegativePlus = 12,
        /// <summary>
        /// Not used anymore. Kept by backward compatibility.
        /// </summary>
        NegativeMinus = 13,
        /// <summary>
        /// The record is locked by the dialer in order to be dialed.
        /// </summary>
        Locked = 14,
        /// <summary>
        /// A callback is associated to the record.
        /// </summary>
        Callback = 15,
        /// <summary>
        /// The contact has been previewed.
        /// </summary>
        Preview = 16,
        MaxAttempts = 17,
        BlackListed = 18
    }

    /// <summary>
    /// Defines dialing states.
    /// </summary>
    public enum DialingState
    {
        /// <summary>
        /// Not connected (terminated).
        /// </summary>
        Disconnected,      
        /// <summary>
        /// Dialing has been launched.
        /// </summary>
        Progressing,    
        /// <summary>
        /// Ringing.
        /// </summary>
        Ringing,    
        /// <summary>
        /// Answered but not yet connected to agent.
        /// </summary>
        Connected,   
        /// <summary>
        /// Talking with agent.
        /// </summary>
        Agent               
    }

    /// <summary>
    /// Defines the dial attempts disconnection reasons.
    /// </summary>
    public enum DialDisconnectionReason
    {
        /// <summary>
        /// None or unknown state.
        /// </summary>
        None,
        /// <summary>
        /// A fax has been detected.
        /// </summary>
        Fax,
        /// <summary>
        /// A wrong number has been detected.
        /// </summary>
        Disturbed,
        /// <summary>
        /// The dial attempt was not answered.
        /// </summary>
        NoAnswer,
        /// <summary>
        /// An answering machine has been detected.
        /// </summary>
        AnsweringMachine,
        /// <summary>
        /// The dial attempt ended in a busy signal.
        /// </summary>
        Busy,
        /// <summary>
        /// The dial attempt was connected but was dropped while waiting in queue.
        /// </summary>
        Abandoned,
        /// <summary>
        /// The callback validity was elapsed.
        /// </summary>
        ValidityEllapsed,
        /// <summary>
        /// The call was dropped by a normal clearing.
        /// </summary>
        NormalClearing,
        /// <summary>
        /// The call was handled by an agent.
        /// </summary>
        Agent,
        /// <summary>
        /// The agent did not dial the previewed record.
        /// </summary>
        AgentUnavailable,
        /// <summary>
        /// The dialing ended in a congestion.
        /// </summary>
        Congestion
    }
    /// <summary>
    /// Defines the contact destination types.
    /// </summary>
    public enum ContactDestinationType
    {
        /// <summary>
        /// The destination type is unknown.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The destination is a mobile phone.
        /// </summary>
        MobilePhone = 1,
        /// <summary>
        /// The destination is a fixed phone.
        /// </summary>
        FixedPhone = 2
    }

    /// <summary>
    /// Defines the destination roles.
    /// </summary>
    public enum ContactDestinationRole
    {
        /// <summary>
        /// The destination role is unknown.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The destination is private.
        /// </summary>
        Residential = 1,
        /// <summary>
        /// The destination is work related.
        /// </summary>
        Business = 2
    }
    /// <summary>
    /// Defines frequencies used when prompting information in a loop.
    /// </summary>
    public enum Frequency
    {
        /// <summary>
        /// The prompt never happens.
        /// </summary>
        Never = 0,
        /// <summary>
        /// The prompt happens once at the begining of the loop.
        /// </summary>
        Once = 1,
        /// <summary>
        /// The prompt happens each time when the prompted value is changing.
        /// </summary>
        WhenChanged = 2,
        /// <summary>
        /// The prompt happens at the begining of each loop.
        /// </summary>
        Continuously = 3
    }

    /// <summary>
    /// Specify the type implenting the configuration dialog related to the processor configuration class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DialogTypeAttribute : Attribute
    {
        /// <summary>
        /// Specify the fully qualified name of the type implementing the configuration dialog.
        /// </summary>
        public string Type
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Specify the type implenting the server addon related to the processor configuration class. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ServerAddonTypeAttribute : Attribute
    {
        /// <summary>
        /// Specify the fully qualified name of the type implementing the server addon.
        /// </summary>
        public string Type
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Defines the method needed to implement a custom "Music On Hold" provider.
    /// </summary>
    public interface IMohAddon
    {        
        /// <summary>
        /// Returns the list of identifiers and description of available "Music On Hold".
        /// </summary>
        IDictionary<string, string> Entries { get; }
    }

    public class DoubleRange : INotifyPropertyChanged
    {
        private double m_Start = 0d;
        private double m_End = 100d;

        public DoubleRange()
        {
        }

        public DoubleRange(double start, double end)
        {
            m_Start = start;
            m_End = end;
        }

        public double Start
        {
            get
            {
                return m_Start;
            }
            set
            {
                m_Start = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Start"));
            }
        }
        public double End
        {
            get
            {
                return m_End;
            }
            set
            {
                m_End = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("End"));

            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public interface ISession 
    {
        Uri BaseUri
        {
            get;
        }

        NetworkCredential Credential
        {
            get;
        }

        string Extension
        {
            get;
        }

        int DebugMode
        {
            get;
        }

        IServiceDescription this[int index]
        {
            get;
        }

        IServiceDescription this[string index]
        {
            get;
        }

        Uri MakeUri(string relativeUri);

        void Trace(string strTace);


    }

    public interface IServiceDescription
    {
        string ServiceID { get; }
        string Location { get; }
        System.Collections.Specialized.NameValueCollection Settings { get; }
    }

    public interface IViewServerAddon
    {
        void SetClientIpAdresses(string[] ipaddresses);
        void Start();
        void Stop();
    }

    public interface IViewClientAddon
    {
        string ConnectionInfo { get; set; }
    }

    public static class DiagnosticHelpers
    { 
        public static void DebugIfPossible()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            else
                try
                {
                    System.Diagnostics.Trace.WriteLine((new System.Diagnostics.TraceEventCache()).Callstack.Replace('\n', ' ').Replace('\r', ' '), "Break request!");
                }
                catch
                {
                }
        }
    }

}
