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

namespace Nixxis.Client
{
	public class HttpLinkCommands : IDisposable
	{
		private bool m_Disposed;
		private HttpLink m_Link;
		private SortedDictionary<CommandCodes, ICommand> m_StandardCommands = new SortedDictionary<CommandCodes, ICommand>();
		private SortedDictionary<string, ICommand> m_CustomCommands = new SortedDictionary<string, ICommand>();

		private HttpLinkCommands()
		{
		}

		internal HttpLinkCommands(HttpLink link)
		{
			m_Link = link;
		}

		internal void ResetCommands()
		{
			m_StandardCommands.Clear();
			m_CustomCommands.Clear();
		}

		internal void BuildCommands(string commandList)
		{
			string[] Commands = commandList.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string Command in Commands)
			{
				string[] Details = Command.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);
				CommandCodes Code;

				try
				{
					if (char.IsDigit(Details[0][0]))
					{
						int IntCode = Convert.ToInt32(Details[0]);

						if (Enum.IsDefined(typeof(CommandCodes), IntCode))
						{
							Code = (CommandCodes)IntCode;
						}
						else
						{
							continue;
						}
					}
					else
					{
						Code = (CommandCodes)Enum.Parse(typeof(CommandCodes), Details[0], true);
					}

					lock (WellKnownCommands.List)
					{
						for (int i = 0; i < WellKnownCommands.List.Length; i++)
						{
							if (WellKnownCommands.List[i].Code == Code)
							{
								try
								{
									WellKnownCommands.List[i].ActivationData[0] = m_Link;
									BaseCommand NewCommand = Activator.CreateInstance(WellKnownCommands.List[i].Type, WellKnownCommands.List[i].ActivationData) as BaseCommand;

									if (NewCommand != null)
									{
                                        int IsActive = Convert.ToInt32(Details[1]);

										m_StandardCommands.Add(Code, NewCommand);

										NewCommand.SetAuthorized((Convert.ToInt32(Details[1]) != 0));
                                        NewCommand.SetActive((IsActive == 0) ? false : ((IsActive < 0) ? (bool?)null : true));
									}

									WellKnownCommands.List[i].ActivationData[0] = null;

									break;
								}
								catch(Exception Ex)
								{
                                    System.Diagnostics.Trace.WriteLine(Ex.ToString());
								}
							}
						}
					}
				}
				catch
				{
					m_CustomCommands.Add(Command, new CustomCommand(m_Link, Command));
				}
			}
		}

		internal ICommand FindCommand(string commandCode)
		{
			ICommand Command = null;

			try
			{
				Command = m_StandardCommands[(CommandCodes)Enum.Parse(typeof(CommandCodes), commandCode, true)];
			}
			catch
			{
				m_CustomCommands.TryGetValue(commandCode.ToLowerInvariant(), out Command);
			}

			return Command;
		}

        public ICommand[] StandardCommands
        {
            get
            {
                ICommand[] Result = new ICommand[m_StandardCommands.Count];
                m_StandardCommands.Values.CopyTo(Result, 0);

                return Result;
            }
        }

        public ICommand[] CustomCommands
        {
            get
            {
                ICommand[] Result = new ICommand[m_CustomCommands.Count];
                m_CustomCommands.Values.CopyTo(Result, 0);

                return Result;
            }
        }

        public ICommand CustomCommand(string action)
		{
			ICommand Command;

			m_CustomCommands.TryGetValue(action, out Command);

			return Command;
		}

		public WaitForContactCommand WaitForCall
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.WaitForCall] as WaitForContactCommand;
			}
		}

		public WaitForContactCommand WaitForMail
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.WaitForMail] as WaitForContactCommand;
			}
		}

		public WaitForContactCommand WaitForChat
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.WaitForChat] as WaitForContactCommand;
			}
		}

		public PauseCommand Pause
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.Pause] as PauseCommand;
			}
		}

		public VoiceNewCallCommand VoiceNewCall
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceNewCall] as VoiceNewCallCommand;
			}
		}

        public VoiceRecordCommand VoiceRecord
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.VoiceRecord] as VoiceRecordCommand;
            }
        }

        public VoiceListenCommand VoiceListen
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.VoiceListen] as VoiceListenCommand;
            }
        }

        public ViewScreenCommand ViewScreen
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ViewScreen] as ViewScreenCommand;
            }
        }


		public VoiceHoldCommand VoiceHold
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceHold] as VoiceHoldCommand;
			}
		}

		public VoiceRetrieveCommand VoiceRetrieve
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceRetrieve] as VoiceRetrieveCommand;
			}
		}

		public VoiceHangupCommand VoiceHangup
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceHangup] as VoiceHangupCommand;
			}
		}

		public VoiceTransferCommand VoiceTransfer
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceTransfer] as VoiceTransferCommand;
			}
		}

		public VoiceForwardCommand VoiceForward
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceForward] as VoiceForwardCommand;
			}
		}

        public ChatHoldCommand ChatHold
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ChatHold] as ChatHoldCommand;
            }
        }

        public ChatRetrieveCommand ChatRetrieve
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ChatRetrieve] as ChatRetrieveCommand;
            }
        }

        public ChatHangupCommand ChatHangup
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ChatHangup] as ChatHangupCommand;
            }
        }

        public ChatForwardCommand ChatForward
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ChatForward] as ChatForwardCommand;
            }
        }

        public MailReplyCommand MailReply
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.MailReply] as MailReplyCommand;
            }
        }

        public MailForwardCommand MailForward
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.MailForward] as MailForwardCommand;
            }
        }

		public VoiceConferenceCommand VoiceConference
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.VoiceConference] as VoiceConferenceCommand;
			}
		}

        public ExternalSpyCommand ExternalSpy
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ExternalSpy] as ExternalSpyCommand;
            }
        }

		public TerminateContactCommand TerminateContact
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.TerminateContact] as TerminateContactCommand;
			}
		}

		public PriorityPickupCommand PriorityPickup
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.PriorityPickup] as PriorityPickupCommand;
			}
		}

		public StartScriptCommand StartScript
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.StartScript] as StartScriptCommand;
			}
		}

		public ChatSpyCommand ChatSpy
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.ChatSpy] as ChatSpyCommand;
			}
		}

		public SearchModeCommand SearchMode
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_StandardCommands[CommandCodes.SearchMode] as SearchModeCommand;
			}
		}

        public RequestAssistanceCommand RequestAssistance
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.RequestAssistance] as RequestAssistanceCommand;
            }
        }

        public ShowHistoryCommand ShowHistory
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.ShowHistory] as ShowHistoryCommand;
            }
        }

        public SetQualificationCommand SetQualification
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                    return m_StandardCommands[CommandCodes.SetQualification] as SetQualificationCommand;
            }
        }

        public SupervisionSelectCommand SupervisionSelect
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);

                return m_StandardCommands[CommandCodes.SupervisionSelect] as SupervisionSelectCommand;
            }
        }


		#region IDisposable Members

		public void Dispose()
		{
			if (!m_Disposed)
			{
				m_Disposed = true;

				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}
}
