using System;

namespace Nixxis.Client
{
	public interface IClientApplication
	{
		System.Windows.Forms.Control UserControl { get; }

		bool StartScript(IContactInfo contactInfo, IScriptControl scriptControl);
		bool TerminateScript();
	}

	public interface IToolbarImageProvider
	{
		System.Drawing.Image ToolbarImage { get; }
		System.Windows.Forms.ImageLayout ToolbarImageLayout { get; }
		System.Drawing.Image[] TabImages { get; }
	}

	public enum TransferFailureBehavior
	{
		None,
		QueueBack
	}

	public enum TerminateBehavior
	{
		None,
		ForceReady,
		ForcePause
	}

	public enum ContactMedia
	{
		Unknown,
		Voice,
		Mail,
		Chat
	}

	public interface IContactInfo
	{
		string Activity { get; }
		string Context { get; }
		string CustomerId { get; }
		string Direction { get; }
		string From { get; }
		string Id { get; }
		ContactMedia Media { get; }
		string Queue { get; }
		char State { get; }
		DateTime LastStateChange { get; }
		string To { get; }
		string UUI { get; }
        string Language { get; }
		string ContactListId { get; }
		string ContentLink { get; }
        string ContentHandler { get; }
		string CustomerDescription { get; }

		void SetCustomerId(string customerId);
		void SetContactListId(string customerId);
		void SetCustomerDescription(string customerDescription);

		string GetPrivateInfo(string key);
		void SetPrivateInfo(string key, string value);

		System.Xml.XmlDocument GetContextData();
		System.Xml.XmlDocument GetContextData(int historyLength);
		System.Xml.XmlDocument UpdateContextData(System.Xml.XmlDocument data);
        System.Xml.XmlDocument UpdateContextData(string data);
	}

	public delegate void ContactStateChangedDelegate(ContactInfo contactInfo);
	public delegate void ContactContentChangedDelegate(ContactInfo contactInfo);

	public interface IScriptControl
	{
		bool NewCall(string destination);
		bool Hold();
		bool Retrieve();
		bool Transfer();
		bool Transfer(TransferFailureBehavior failureBehavior);
		bool Forward(string destination);
		bool Forward(string destination, TransferFailureBehavior failureBehavior);
		bool Qualify(QualificationInfo qualification);
		bool Close();
		bool Close(TerminateBehavior terminateBehavior);

		event ContactStateChangedDelegate ContactStateChanged;
	}

	public interface IAgentInfo
	{
		string AgentId { get; }
		string Account { get; }
		string Name { get; }
		string Description { get; }
		ClientRoles Roles { get; }
		TeamCollection Teams { get; }
		int AutoReady { get; }
		PauseCodeCollection PauseCodes { get; }
		ContactsList Contacts { get; }

		bool HasClientRole(ClientRoles clientRole);

		string GetPrivateInfo(string key);
		void SetPrivateInfo(string key, string value);
	}

    public enum ScriptContainerToolStripPanel
    {
        Top,
        Left,
        Bottom,
        Right
    }

    public enum LoginScriptButtonMode
    {
        Hidden,
        ToolStrip,
        Tab
    }

	public interface IScriptContainer
	{
        System.Windows.Forms.ToolStrip CreateToolStrip(int index, ScriptContainerToolStripPanel panel);
		System.Windows.Forms.ToolStripStatusLabel CreateStatusLabel(int index, int width);
		IClientApplication LoginScript { get; }
        LoginScriptButtonMode LoginScriptButtonMode { get; set; }
	}

	public interface IScriptControl2 : IScriptControl
	{
		bool NewCall(string destination, ContactMedia media);
		bool NewCall(string destination, ContactMedia media, string contactId, string activity, string customerId, string contactListId, string onBehalfOf);
		bool SetTextContent(string content);
		string GetTextContent();
		bool Close(bool sendTextContent);
		bool Close(bool sendTextContent, TerminateBehavior terminateBehavior);
		IAgentInfo AgentInfo { get; }
		IScriptContainer ScriptContainer { get; }
		System.Collections.Specialized.NameValueCollection ClientSettings { get; }
		bool Qualify(string customValue);
        bool Hangup();
        bool Redial(string destination);
		event ContactContentChangedDelegate ContactContentChanged;
	}
}
