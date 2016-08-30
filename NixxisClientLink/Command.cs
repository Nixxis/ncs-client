using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Threading;
using System.ComponentModel;

namespace Nixxis.Client
{
	public delegate void CommandStateChangedDelegate(ICommand command);
//    public delegate void HttpLinkCommandDelegate(

	internal struct WellKnownCommands
	{
		public static WellKnownCommands[] List = new WellKnownCommands[] 
		{ 
			new WellKnownCommands(CommandCodes.Ready,		    	typeof(WaitForContactCommand),		new object[] { null, CommandCodes.Ready } ),
			new WellKnownCommands(CommandCodes.WaitForCall,			typeof(WaitForContactCommand),		new object[] { null, CommandCodes.WaitForCall } ),
			new WellKnownCommands(CommandCodes.WaitForMail,			typeof(WaitForContactCommand),		new object[] { null, CommandCodes.WaitForMail } ),
			new WellKnownCommands(CommandCodes.WaitForChat,			typeof(WaitForContactCommand),		new object[] { null, CommandCodes.WaitForChat } ),
			new WellKnownCommands(CommandCodes.Pause,				typeof(PauseCommand),				new object[] { null } ),
			new WellKnownCommands(CommandCodes.ExtendWrapup,		typeof(WrapupCommand),				new object[] { null } ),
			new WellKnownCommands(CommandCodes.RequestAssistance,	typeof(AssistanceCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceNewCall,		typeof(VoiceNewCallCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceHold,			typeof(VoiceHoldCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceRetrieve,		typeof(VoiceRetrieveCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceTransfer,		typeof(VoiceTransferCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceForward,		typeof(VoiceForwardCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceConference,		typeof(VoiceConferenceCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceHangup,			typeof(VoiceHangupCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceRecord,			typeof(VoiceRecordCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.VoiceListen,			typeof(VoiceListenCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.MailNewMail,			typeof(MailNewMailCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.MailReply,			typeof(MailReplyCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.MailReplySender,		typeof(MailReplySenderCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.MailForward,			typeof(MailForwardCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.ChatNewCall,			typeof(ChatNewCallCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.ChatHold,			typeof(ChatHoldCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.ChatRetrieve,		typeof(ChatRetrieveCommand),		new object[] { null } ),
			new WellKnownCommands(CommandCodes.ChatHangup,			typeof(ChatHangupCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.TerminateContact,	typeof(TerminateContactCommand),	new object[] { null } ),
			new WellKnownCommands(CommandCodes.PriorityPickup,		typeof(PriorityPickupCommand),		new object[] { null } ),
            new WellKnownCommands(CommandCodes.StartScript,		    typeof(StartScriptCommand),		    new object[] { null } ),
			new WellKnownCommands(CommandCodes.ChatForward,			typeof(ChatForwardCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.ChatSpy,				typeof(ChatSpyCommand),				new object[] { null } ),
			new WellKnownCommands(CommandCodes.SearchMode,			typeof(SearchModeCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.SendMessage,			typeof(SendMessageCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.ShowHistory,			typeof(ShowHistoryCommand),			new object[] { null } ),
			new WellKnownCommands(CommandCodes.SetQualification,	typeof(SetQualificationCommand),	new object[] { null } ),
            new WellKnownCommands(CommandCodes.SupervisionSelect,	typeof(SupervisionSelectCommand),	new object[] { null } ),
            new WellKnownCommands(CommandCodes.ViewScreen,	        typeof(ViewScreenCommand),	        new object[] { null } ),
            new WellKnownCommands(CommandCodes.ExternalSpy,		    typeof(ExternalSpyCommand),		    new object[] { null } ),
		};

		public CommandCodes Code;
		public Type Type;
		public Object[] ActivationData;

		private WellKnownCommands(CommandCodes code, Type type, object[] activationData)
		{
			Code = code;
			Type = type;
			ActivationData = activationData;
		}
	}

	public interface ICommand : System.Windows.Input.ICommand
	{
		HttpLink Link { get; }
		CommandCodes Code { get; }
		bool Authorized { get; }
		bool Active { get; }
		ToolStripItem LinkedItem { get; set; }

		event CommandStateChangedDelegate StateChanged;

		ResponseData Execute();
		ResponseData Execute(bool parameter);
		ResponseData Execute(int parameter);
		ResponseData Execute(string parameter);
		ResponseData Execute(params string[] parameters);

		int LockState();
		int ReleaseState();
		int ReleaseState(bool changed);
	}

	public class BaseCommand : System.Windows.DependencyObject, ICommand, INotifyPropertyChanged
	{
		private HttpLink m_Link;
		private CommandCodes m_Code;
        private bool? m_Authorized = null;
        private bool? m_Active = null;
		private int m_StateLockCount;
		private bool m_StateChanged;
		private ToolStripItem m_LinkedItem;

		protected string m_ActionCode;
		protected bool m_SomeStateChanged;

		public event CommandStateChangedDelegate StateChanged;
		private CommandStateChangedDelegate m_InternalStateChanged;

        public static readonly System.Windows.DependencyProperty ActiveProperty = System.Windows.DependencyProperty.Register("Active", typeof(bool?), typeof(BaseCommand), new System.Windows.PropertyMetadata(null, new System.Windows.PropertyChangedCallback(OnActiveChanged)));
        public static readonly System.Windows.DependencyProperty AuthorizedProperty = System.Windows.DependencyProperty.Register("Authorized", typeof(bool?), typeof(BaseCommand), new System.Windows.PropertyMetadata(null, new System.Windows.PropertyChangedCallback(OnAuthorizedChanged)));

        private static void OnActiveChanged(System.Windows.DependencyObject target, System.Windows.DependencyPropertyChangedEventArgs args)
        {
        }

        private static void OnAuthorizedChanged(System.Windows.DependencyObject target, System.Windows.DependencyPropertyChangedEventArgs args)
        {
        }
        
        private BaseCommand()
		{
		}

		protected BaseCommand(HttpLink link, CommandCodes code)
		{
			m_Link = link;
			m_Code = code;
			m_ActionCode = code.ToString().ToLowerInvariant();

			m_InternalStateChanged = new CommandStateChangedDelegate(InternalStateChanged);
            
        }

		private void InternalStateChanged(ICommand target)
		{
			if (LinkedItem != null)
			{
				try
				{
					LinkedItem.Enabled = target.Authorized;

					if (LinkedItem is ToolStripButton)
					{
						((ToolStripButton)target.LinkedItem).Checked = Active;
					}
					else if (LinkedItem is ToolStripMenuItem)
					{
						((ToolStripMenuItem)target.LinkedItem).Checked = Active;
					}
				}
				catch
				{
				}
			}

            if (m_Active != (bool?)GetValue(ActiveProperty))
            {
                Debug.WriteLine(string.Format("{0} {1} active: {2}", this.GetType().Name, this.Code.ToString(), m_Active), "CommandState");

                SetValue(ActiveProperty, m_Active);
                OnPropertyChanged(new PropertyChangedEventArgs("Active"));
            }

            if (m_Authorized != (bool?)GetValue(AuthorizedProperty))
            {
                Debug.WriteLine(string.Format("{0} {1} enabled: {2}", this.GetType().Name, this.Code.ToString(), m_Authorized), "CommandState");

                SetValue(AuthorizedProperty, m_Authorized);
                OnPropertyChanged(new PropertyChangedEventArgs("Authorized"));

                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }

			if (StateChanged != null)
				StateChanged(target);
		}

		private void FireStateChanged()
		{
            m_Link.Invoke(new CommandStateChangedDelegate(InternalStateChanged), this);
		}

        public int LockState()
		{
			lock (this)
			{
				return ++m_StateLockCount;
			}
		}

		public int ReleaseState()
		{
			return ReleaseState(true);
		}

		public int ReleaseState(bool changed)
		{
			int NewLockCount;

			lock(this)
			{
				if (changed)
					m_StateChanged = true;

				NewLockCount = --m_StateLockCount;
			}

			if (NewLockCount == 0)
			{
				bool DoFire;

				lock (this)
				{
					DoFire = m_StateChanged;
					m_StateChanged = false;
				}

				if(DoFire)
					FireStateChanged();
			}

			return NewLockCount;
		}

		internal void SetAuthorized(bool? value)
		{
			if (m_Authorized != value)
			{
				LockState();

				if (m_Authorized != value)
				{
                    m_Authorized = value;

                    ReleaseState(true);
				}
				else
				{
					ReleaseState(false);
				}
			}
		}

		internal void SetActive(bool? value)
		{
			if (m_Active != value)
			{
				LockState();

				if (m_Active != value)
				{
                    m_Active = value;

                    ReleaseState(true);
				}
				else
				{
					ReleaseState(false);
				}
			}
		}

		#region ICommand Members

		public HttpLink Link
		{
			get
			{
				return m_Link;
			}
		}

		public CommandCodes Code
		{
			get
			{
				return m_Code;
			}
		}

		public bool Authorized
		{
			get
			{
                return ((m_Authorized.HasValue) ? m_Authorized.Value : false);
			}
		}

		public bool Active
		{
			get
			{
                return ((m_Active.HasValue) ? m_Active.Value : false);
			}
		}

        public bool? Active3S
        {
            get
            {
                return m_Active;
            }
        }

		public ToolStripItem LinkedItem
		{
			get
			{
				return m_LinkedItem;
			}
			set
			{
				LockState();
				m_LinkedItem = value;
				ReleaseState(true);
			}
		}

        public virtual ResponseData ExecuteDirect(string[] urlParameters, string dataParameters)
        {
            return m_Link.GetResponseData(m_Link.BuildRequest(m_ActionCode, urlParameters, dataParameters));
        }

		protected virtual ResponseData Execute(string[] urlParameters, string dataParameters)
		{
			if(!Authorized)
				return ResponseData.Forbidden;

			return m_Link.GetResponseData(m_Link.BuildRequest(m_ActionCode, urlParameters, dataParameters));
		}

		public ResponseData Execute(string parameter)
		{
			return Execute((string[])null, parameter);
		}

		public ResponseData Execute()
		{
			return Execute((string[])null, (string)null);
		}

		public ResponseData Execute(bool parameter)
		{
			return Execute(parameter ? "1" : "0", null);
		}

		public ResponseData Execute(int parameter)
		{
			return Execute(parameter.ToString(), null);
		}

		public ResponseData Execute(params string[] parameters)
		{
			return Execute((string[])null, string.Join("\n", parameters));
		}

		#endregion

        public bool CanExecute(object parameter)
        {
            return Authorized;
        }

        public event EventHandler CanExecuteChanged;
        public event CustomCommandExecuteHandler BeforeExecute;

        public void Execute(object parameter)
        {
            if (BeforeExecute != null && BeforeExecute(parameter))
                return;

            if (parameter == null)
                Execute((string[])null, (string)null);
            else if (parameter is string)
                Execute((string[])null, (string)parameter);
            else if (parameter is string[])
                Execute((string[])null, string.Join("\n", (string[])parameter));
            else if (parameter is int)
                Execute((int)parameter);
            else if (parameter is bool)
                Execute((bool)parameter);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public delegate bool CustomCommandExecuteHandler(object parameter);

    public class InternalCommand : System.Windows.DependencyObject, ICommand, INotifyPropertyChanged
    {
        Action<InternalCommand, object> m_Action;
        HttpLink m_Link;
        bool m_Authorized;
        bool m_Active;
        ICommand m_LinkedCommand;

        public InternalCommand(HttpLink link, Action<InternalCommand, object> action)
        {
            m_Link = link;
            m_Action = action;
        }

        public HttpLink Link
        {
            get 
            { 
                return m_Link; 
            }
        }

        public ICommand LinkedCommand
        {
            get
            {
                return m_LinkedCommand;
            }
            set
            {
                if (m_LinkedCommand != null)
                    m_LinkedCommand.StateChanged -= new CommandStateChangedDelegate(LinkedCommand_StateChanged);

                m_LinkedCommand = value;

                if (m_LinkedCommand != null)
                    m_LinkedCommand.StateChanged += new CommandStateChangedDelegate(LinkedCommand_StateChanged);
            }
        }

        void Sync_LinkedCommand_StateChanged()
        {
            this.Authorized = m_LinkedCommand.Authorized;
            this.Active = m_LinkedCommand.Active;
        }

        void LinkedCommand_StateChanged(ICommand command)
        {
            m_Link.Invoke(new HttpLinkEventDelegate(Sync_LinkedCommand_StateChanged), null);
        }

        public CommandCodes Code
        {
            get { return (CommandCodes)(-1); }
        }

        public bool Authorized
        {
            get 
            { 
                return m_Authorized; 
            }
            set
            {
                if (m_Authorized != value)
                {
                    m_Authorized = value;

                    OnPropertyChanged(new PropertyChangedEventArgs("Authorized"));
                    OnCanExecuteChanged();
                }
            }
        }

        public bool Active
        {
            get
            { 
                return m_Active; 
            }
            set
            {
                if (m_Active != value)
                {
                    m_Active = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Active"));
                }
            }
        }

        public ToolStripItem LinkedItem
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event CommandStateChangedDelegate StateChanged;

        public ResponseData Execute()
        {
            throw new NotImplementedException();
        }

        public ResponseData Execute(bool parameter)
        {
            throw new NotImplementedException();
        }

        public ResponseData Execute(int parameter)
        {
            throw new NotImplementedException();
        }

        public ResponseData Execute(string parameter)
        {
            throw new NotImplementedException();
        }

        public ResponseData Execute(params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public int LockState()
        {
            throw new NotImplementedException();
        }

        public int ReleaseState()
        {
            throw new NotImplementedException();
        }

        public int ReleaseState(bool changed)
        {
            throw new NotImplementedException();
        }


        public bool CanExecute(object parameter)
        {
            return Authorized;
        }

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            m_Action.Invoke(this, parameter);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

	public class CustomCommand : BaseCommand
	{
		public CustomCommand(HttpLink link, string action)
			: base(link, CommandCodes.Custom)
		{
			m_ActionCode = action;
		}
	}

	public class WaitForContactCommand : BaseCommand
	{
		public WaitForContactCommand(HttpLink link, CommandCodes code)
			: base(link, code)
		{

		}

        protected override ResponseData Execute(string[] urlParameters, string dataParameters)
        {
            if (urlParameters == null || urlParameters.Length == 0)
            {
                // Force to execute the expected behavior even if transmitting multiple times
                urlParameters = new string[] { (this.Active) ? "0" : "1" };
            }

            return base.Execute(urlParameters, dataParameters);
        }
	}

	public class PauseCommand : BaseCommand
	{
		public PauseCommand(HttpLink link)
			: base(link, CommandCodes.Pause)
		{

		}
	}

	public class VoiceHoldCommand : BaseCommand
	{
		public VoiceHoldCommand(HttpLink link)
			: base(link, CommandCodes.VoiceHold)
		{

		}
	}

	public class VoiceRetrieveCommand : BaseCommand
	{
		public VoiceRetrieveCommand(HttpLink link)
			: base(link, CommandCodes.VoiceRetrieve)
		{

		}
	}

	public class VoiceNewCallCommand : BaseCommand
	{
		public VoiceNewCallCommand(HttpLink link)
			: base(link, CommandCodes.VoiceNewCall)
		{

		}
	}

	public class VoiceHangupCommand : BaseCommand
	{
		public VoiceHangupCommand(HttpLink link)
			: base(link, CommandCodes.VoiceHangup)
		{

		}
	}

	public class VoiceTransferCommand : BaseCommand
	{
		public VoiceTransferCommand(HttpLink link)
			: base(link, CommandCodes.VoiceTransfer)
		{
		}
	}

	public class VoiceForwardCommand : BaseCommand
	{
		public VoiceForwardCommand(HttpLink link)
			: base(link, CommandCodes.VoiceForward)
		{
		}
	}

	public class VoiceConferenceCommand : BaseCommand
	{
		public VoiceConferenceCommand(HttpLink link)
			: base(link, CommandCodes.VoiceConference)
		{
		}
	}

    public class ExternalSpyCommand : BaseCommand
    {
        public ExternalSpyCommand(HttpLink link)
            : base(link, CommandCodes.ExternalSpy)
        {
        }
    }

	public class WrapupCommand : BaseCommand
	{
		public WrapupCommand(HttpLink link)
			: base(link, CommandCodes.ExtendWrapup)
		{
		}
	}

	public class AssistanceCommand : BaseCommand
	{
		public AssistanceCommand(HttpLink link)
			: base(link, CommandCodes.RequestAssistance)
		{
		}
	}

	public class VoiceRecordCommand : BaseCommand
	{
		public VoiceRecordCommand(HttpLink link)
			: base(link, CommandCodes.VoiceRecord)
		{
		}
	}

	public class VoiceListenCommand : BaseCommand
	{
		public VoiceListenCommand(HttpLink link)
			: base(link, CommandCodes.VoiceListen)
		{
		}
	}

    public class ViewScreenCommand : BaseCommand
    {
        public ViewScreenCommand(HttpLink link)
            : base(link, CommandCodes.ViewScreen)
        {
        }
    }


	public class MailNewMailCommand : BaseCommand
	{
		public MailNewMailCommand(HttpLink link)
			: base(link, CommandCodes.MailNewMail)
		{
		}
	}

	public class MailReplyCommand : BaseCommand
	{
		public MailReplyCommand(HttpLink link)
			: base(link, CommandCodes.MailReply)
		{
		}
	}

	public class MailReplySenderCommand : BaseCommand
	{
		public MailReplySenderCommand(HttpLink link)
			: base(link, CommandCodes.MailReplySender)
		{
		}
	}

	public class MailForwardCommand : BaseCommand
	{
		public MailForwardCommand(HttpLink link)
			: base(link, CommandCodes.MailForward)
		{
		}
	}

	public class ChatNewCallCommand : BaseCommand
	{
		public ChatNewCallCommand(HttpLink link)
			: base(link, CommandCodes.ChatNewCall)
		{
		}
	}

	public class ChatHoldCommand : BaseCommand
	{
		public ChatHoldCommand(HttpLink link)
			: base(link, CommandCodes.ChatHold)
		{
		}
	}

	public class ChatRetrieveCommand : BaseCommand
	{
		public ChatRetrieveCommand(HttpLink link)
			: base(link, CommandCodes.ChatRetrieve)
		{
		}
	}

	public class ChatHangupCommand : BaseCommand
	{
		public ChatHangupCommand(HttpLink link)
			: base(link, CommandCodes.ChatHangup)
		{
		}
	}

	public class ChatForwardCommand : BaseCommand
	{
		public ChatForwardCommand(HttpLink link)
			: base(link, CommandCodes.ChatForward)
		{
		}
	}

	public class TerminateContactCommand : BaseCommand
	{
		public TerminateContactCommand(HttpLink link)
			: base(link, CommandCodes.TerminateContact)
		{
		}
	}

	public class PriorityPickupCommand : BaseCommand
	{
		public PriorityPickupCommand(HttpLink link)
			: base(link, CommandCodes.PriorityPickup)
		{
		}
	}

	public class StartScriptCommand : BaseCommand
	{
		public StartScriptCommand(HttpLink link)
			: base(link, CommandCodes.StartScript)
		{
		}
	}

	public class ChatSpyCommand : BaseCommand
	{
		public ChatSpyCommand(HttpLink link)
			: base(link, CommandCodes.ChatSpy)
		{
		}
	}

	public class SearchModeCommand : BaseCommand
	{
		public SearchModeCommand(HttpLink link)
			: base(link, CommandCodes.SearchMode)
		{
		}
	}

    public class SendMessageCommand : BaseCommand
    {
        public SendMessageCommand(HttpLink link)
            : base(link, CommandCodes.SendMessage)
        {
        }
    }

    public class RequestAssistanceCommand : BaseCommand
    {
        public RequestAssistanceCommand(HttpLink link)
            : base(link, CommandCodes.RequestAssistance)
        {
        }
    }

    public class ShowHistoryCommand : BaseCommand
    {
        public ShowHistoryCommand(HttpLink link)
            : base(link, CommandCodes.ShowHistory)
        {
        }
    }

    public class SetQualificationCommand : BaseCommand
    {
        public SetQualificationCommand(HttpLink link)
            : base(link, CommandCodes.SetQualification)
        {
        }
    }

    public class SupervisionSelectCommand : BaseCommand
    {
        public SupervisionSelectCommand(HttpLink link)
            : base(link, CommandCodes.SupervisionSelect)
        {
        }
    }

}
