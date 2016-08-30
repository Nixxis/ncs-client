using System;
using System.Collections.ObjectModel;

namespace Nixxis.Client
{
    public struct ProtocolOptions
    {
        public static char[] LineSeparator = new char[] { '\r', '\n' };
        public static char[] ValueSeparator = new char[] { '=' };
        public static char[] ListSeparator = new char[] { ',', ' ' };
        public static char[] EscapeSeparator = new char[] { '&' };
    }

    public enum CommandCodes
    {
        WaitForCall,
        WaitForMail,
        WaitForChat,
        Pause,
        ExtendWrapup,
        RequestAssistance,

        IdentifyCustomer,
        SetTopic,
        StartScript,
        LauchApplication,
        TerminateContact,

        VoiceHold,
        VoiceRetrieve,
        VoiceHangup,
        VoiceNewCall,
        VoiceTransfer,
        VoiceForward,
        VoicePickup,
        VoiceConference,

        VoiceRecord,
        VoiceListen,

        MailNewMail,
        MailReply,
        MailReplySender,
        MailForward,

        ChatNewCall,
        ChatHold,
        ChatRetrieve,
        ChatHangup,

        PriorityPickup,
        ChatForward,

        ChatSpy,
        SearchMode,

        Ready,
        SendMessage,
        ShowHistory,
        SetQualification,

        SupervisionSelect,
        
        ViewScreen,
        ExternalSpy,

        Custom = 999
    }

    public enum InfoCodes
    {
        SystemStatus,
        Qualifications,
        PredefinedTexts,
        Attachments,
        AgentsInfo,
        ContactProperty,
        AgentSubState,
        AgentProperty,
        SupervisionData,
        ContextData,
        CallbacksAgenda
    }

	public enum SupervisionInfoCodes
	{
		Unknown,
		SelectedAgent,
	}


    public enum AgentsStateCode
    {
        Undefined = 0,
        Off = 1,
        Login = 2,
        Pause = 3,
        Waiting = 4,
        OnLine = 5,
        WrapUp = 6,
        Logout = 7,
        Supervision = 8,
        Preview = 9
    }

    [Flags]
    public enum ClientRoles
    {
        None = 0,
        Agent = 1,
        Supervisor = 2,
        Administrator = 4,
        SysAdmin = 8,
        ReportViewer = 16,
        WallDisplay = 32
    }

    public enum EventCodes
    {
        CommandState,
        ClientState,
        ContactData,
        ContactState,
        SupervisionData,
        SupervisionItem,
        AgentQueueState,
        AgentWarning,
        TeamsChanged,
        AgentMessage,
        ChatSpyMessage,
        AgentDialInfoState,
        ViewServerOperation,
        Watch,
        SelectionRelatedCommands,
        OOBInfo,
        ForcePause
    }

    public class TeamInfo
    {
        private string m_TeamId;
        private bool m_Active;
        private string m_Description;

        public TeamInfo(string teamId, bool active, string description)
        {
            m_TeamId = teamId;
            m_Active = active;
            m_Description = description;
        }

        public string TeamId
        {
            get
            {
                return m_TeamId;
            }
        }

        public bool Active
        {
            get
            {
                return m_Active;
            }
        }

        public string Description
        {
            get
            {
                return m_Description;
            }
        }

        public void InternalRefresh(bool active, string description)
        {
            m_Active = active;
            m_Description = description;
        }
    }

    public class TeamCollection : KeyedCollection<string, TeamInfo>
    {
        protected override string GetKeyForItem(TeamInfo item)
        {
            return item.TeamId;
        }
    }

    public static class WellknownPauseIds
    {
        public static readonly string AutoReadyDelay = "00000000000000000000111000000001";     // 20x0 = System guid,  111 = pause, 00..001 = sequence
    }
}
