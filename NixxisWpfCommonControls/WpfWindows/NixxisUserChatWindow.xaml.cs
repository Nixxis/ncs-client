using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for NixxisUserChatWindow.xaml
    /// </summary>
    public partial class NixxisUserChatWindow : Window
    {
        #region Enums
        public enum Modes
        {
            Agent,
            Supervision,
        }
        #endregion

        #region Events
        public event NixxisUserChatSendMsgHandler SendMessage;
        public void OnSendMessage(string msg, string userid)
        {
            if (SendMessage != null)
                SendMessage(msg, userid);
        }
        
        public event NixxisUserChatSendMsgHandler EndConversation;
        public void OnEndConversation(string msg, string userid)
        {
            if (EndConversation != null)
                EndConversation(msg, userid);
        }
        #endregion

        #region Class data
        #endregion

        #region Constructors
        public NixxisUserChatWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(NixxisUserChatCollection), typeof(NixxisUserChatWindow), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public NixxisUserChatCollection ItemSource
        {
            get { return (NixxisUserChatCollection)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisUserChatWindow item = (NixxisUserChatWindow)obj;

            if (args.OldValue != null && args.OldValue.GetType() == typeof(NixxisUserChatCollection))
            {
                try
                {
                    ((NixxisUserChatCollection)args.OldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(item.ItemSource_CollectionChanged);
                }
                catch { }
            }

            item.InitItemSource();
        }

        public static readonly DependencyProperty AgentImageSourceProperty = DependencyProperty.Register("AgentImageSource", typeof(ImageSource), typeof(NixxisUserChatWindow), new PropertyMetadata(AgentImageSourcePropertyChanged));
        public ImageSource AgentImageSource
        {
            get { return (ImageSource)GetValue(AgentImageSourceProperty); }
            set { SetValue(AgentImageSourceProperty, value); }
        }
        public static void AgentImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty CustomerImageSourceProperty = DependencyProperty.Register("CustomerImageSource", typeof(ImageSource), typeof(NixxisUserChatWindow), new PropertyMetadata(CustomerImageSourcePropertyChanged));
        public ImageSource CustomerImageSource
        {
            get { return (ImageSource)GetValue(CustomerImageSourceProperty); }
            set { SetValue(CustomerImageSourceProperty, value); }
        }
        public static void CustomerImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty RemoveImageSourceProperty = DependencyProperty.Register("RemoveImageSource", typeof(ImageSource), typeof(NixxisUserChatWindow), new PropertyMetadata(RemoveImageSourcePropertyChanged));
        public ImageSource RemoveImageSource
        {
            get { return (ImageSource)GetValue(RemoveImageSourceProperty); }
            set { SetValue(RemoveImageSourceProperty, value); }
        }
        public static void RemoveImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(Modes), typeof(NixxisUserChatWindow), new PropertyMetadata(ModePropertyChanged));
        public Modes Mode
        {
            get { return (Modes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        public static void ModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Properties

        #endregion

        #region Members Override
        protected override void OnClosed(EventArgs e)
        {
            if (ItemSource != null)
            {
                try
                {
                    ItemSource.CollectionChanged -= new NotifyCollectionChangedEventHandler(ItemSource_CollectionChanged);
                }
                catch { }
            }

            base.OnClosed(e);
        }
        #endregion

        #region Members
        public void AddMessage(string userId, string from, string msg, NixxisUserChatLine.ChatLineOriginators lineType)
        {
            NixxisUserChatLine line = new NixxisUserChatLine { Time = DateTime.Now, EventType = NixxisUserChatLine.ChatEventType.Say, LineTypeEnum = lineType, From = from, Message = msg };

            NixxisUserChatItem user = this.ItemSource.GetItem(userId);

            if (user == null)
            {
                user = new NixxisUserChatItem();
                user.UserId = userId;
                user.UserDescription = from;
                user.Conversation.AddLine(from, msg, lineType);
                
                this.ItemSource.Add(user);
            }
            else
            {
                user.Conversation.AddLine(from, msg, lineType);

                this.ItemSource.Add(user);
            }
        }

        public void InitItemSource()
        {
            lstUserList.SelectedIndex = 0;

            if (ItemSource != null)
                ItemSource.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemSource_CollectionChanged);

        }

        private void ItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (lstUserList.Items.Count == 1)
                {
                    lstUserList.SelectedIndex = 0;
                }
            }
        }
        #endregion

        private void btnSendChatMsg_Click(object sender, RoutedEventArgs e)
        {
            if (lstUserList.SelectedItem == null)
                return;

            if (!string.IsNullOrWhiteSpace(txtAgentText.Text))
            {
                OnSendMessage(txtAgentText.Text, ((NixxisUserChatItem)lstUserList.SelectedItem).UserId);

                ((NixxisUserChatItem)lstUserList.SelectedItem).Conversation.AddLine(((NixxisUserChatItem)lstUserList.SelectedItem).UserDescription, txtAgentText.Text, NixxisUserChatLine.ChatLineOriginators.User);

                txtAgentText.Text = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            NixxisUserChatItem conversation = ItemSource.GetItem(btn.Tag.ToString());

            if (conversation != null)
            {
                EndConversation("Chat ended", btn.Tag.ToString());
                ItemSource.Remove(conversation);
            }
        }

        private void lstUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                foreach (NixxisUserChatItem item in e.AddedItems)
                {
                    item.ActionRequest = false;
                }
            }
            if (e.RemovedItems != null)
            {
                foreach (NixxisUserChatItem item in e.RemovedItems)
                {
                    item.ActionRequest = false;
                }
            }

            if(lstUserList.SelectedItem == null && lstUserList.Items.Count > 0)
            {
                lstUserList.SelectedIndex = 0;
            }
        }
    }

    public delegate void NixxisUserChatSendMsgHandler(string msg, string userid);

    public class NixxisUserChatCollection : ObservableCollection<NixxisUserChatItem>
    {
        public void AddRange(IEnumerable<NixxisUserChatItem> collection)
        {
            foreach (NixxisUserChatItem item in collection)
                this.Add(item);
        }

        public void AddUnique(NixxisUserChatItem item)
        {
            if(!this.ContainsId(item.UserId))
            {
                this.Add(item);
            }
        }

        public bool ContainsItem(object value)
        {
            foreach (NixxisUserChatItem item in this)
            {
                if (item.Equals(value))
                    return true;
            }
            return false;
        }

        public bool ContainsId(string id)
        {
            foreach (NixxisUserChatItem item in this)
            {
                if (item.UserId == id)
                    return true;
            }
            return false;
        }

        public void AddMessage(string msg, string userId, string userDescription, NixxisUserChatLine.ChatLineOriginators originatorType)
        {
            AddMessage(msg, userId, userDescription, originatorType, NixxisUserChatLine.ChatEventType.Say);
        }

        public void AddMessage(string msg, string userId, string userDescription, NixxisUserChatLine.ChatLineOriginators originatorType, NixxisUserChatLine.ChatEventType eventType)
        {
            NixxisUserChatItem user = this.GetItem(userId);

            if (user == null)
            {
                user = new NixxisUserChatItem();
                user.UserId = userId;

                this.Add(user);
            }

            NixxisUserChatLine line = new NixxisUserChatLine { Time = DateTime.Now, EventType = eventType, LineTypeEnum = originatorType, From = user.UserDescription, Message = msg };
            user.UserDescription = userDescription;

            if (eventType == NixxisUserChatLine.ChatEventType.Leave)
                user.ConversationEnded = true;
            else
                user.ConversationEnded = false;

            user.Conversation.AddLine(line);
        }

        public NixxisUserChatItem GetItem(string userId)
        {
            foreach (NixxisUserChatItem item in this)
            {
                if (item.UserId.Equals(userId))
                    return item;
            }
            return null;
        }

    }

    public class NixxisUserChatItem : INotifyPropertyChanged
    {
        #region Class data
        private NixxisUserChatConversation m_Conversation = new NixxisUserChatConversation();
        private string m_UserDescription = string.Empty;
        private string m_UserId = string.Empty;
        private bool m_ConversationEnded = false;
        private bool m_ActionRequest = false;
        #endregion

        #region Constructors
        public NixxisUserChatItem()
        {
            m_Conversation.CollectionChanged += new NotifyCollectionChangedEventHandler(Conversation_CollectionChanged);
        }

        private void Conversation_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.ActionRequest = true;
            }
        }

        #endregion

        #region Propeties
        public NixxisUserChatConversation Conversation
        {
            get { return m_Conversation; }
            set { m_Conversation = value; FirePropertyChanged("Conversation"); }
        }
        public string UserDescription
        {
            get { return m_UserDescription; }
            set { m_UserDescription = value; FirePropertyChanged("UserDescription"); }
        }
        public string UserId
        {
            get { return m_UserId; }
            set { m_UserId = value; FirePropertyChanged("UserId"); }
        }
        public bool ConversationEnded
        {
            get { return m_ConversationEnded; }
            set { m_ConversationEnded = value; FirePropertyChanged("ConversationEnded"); }
        }
        public bool ActionRequest
        {
            get { return m_ActionRequest; }
            set { m_ActionRequest = value; FirePropertyChanged("ActionRequest"); }
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

    /// <summary>
    /// A nixxis user chat conversation object. Based on the version of the media chat.
    /// </summary>
    public class NixxisUserChatConversation : IEnumerable<NixxisUserChatLine>, INotifyCollectionChanged
    {
        List<NixxisUserChatLine> m_History = new List<NixxisUserChatLine>();

        internal int LastTransmit = 0;
        internal int LastKnownTransmit = 0;
        internal int LastReceive = -1;

        public NixxisUserChatConversation()
        {
        }

        public void AddLine(NixxisUserChatLine line)
        {
            m_History.Add(line);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, line));
        }
        public void AddLine(string from, string msg, NixxisUserChatLine.ChatLineOriginators lineType)
        {
            NixxisUserChatLine line = new NixxisUserChatLine { Time = DateTime.Now, EventType = NixxisUserChatLine.ChatEventType.Say, LineTypeEnum = lineType, From = from, Message = msg };

            AddLine(line);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<NixxisUserChatLine> GetEnumerator()
        {
            return m_History.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_History.GetEnumerator();
        }
    }

    public class NixxisUserChatLine
    {
        #region Enums
        public enum ChatEventType
	    {
		    Enter,
		    Leave,
		    Say,
		    Whisper,
		    Activate,
		    Wait,
		    Hold,
		    Retrieve
	    }
        public enum ChatLineOriginators
        {
            System = 1,
            Customer = 2,
            Start = 5,
            User = 770,
        }
        #endregion

        #region Class data
        private ChatLineOriginators m_LineType;
        int m_Flags;
        int m_EventType;
        #endregion

        public string From { get; set; }
        public string To { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }

        //Line Types
        public int LineType
        {
            get { return (int)m_LineType; }
        }
        public ChatLineOriginators LineTypeEnum
        {
            get { return m_LineType; }
            set { m_LineType = value; }
        }

        public ChatEventType EventType 
        { 
            get { return (ChatEventType)m_EventType; }
            set { m_EventType = (int)value; }
        }
    }

    //
    //Converters
    //

    [ValueConversion(typeof(object), typeof(bool))]
    public class NixxisUserChatEnableSendConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
                return false;

            if (values.Length < 2)
                return false;

            if (values[0] == null || values[1] == null)
                return false;

            NixxisUserChatItem item = (NixxisUserChatItem)values[0];
            NixxisUserChatWindow.Modes mode = (NixxisUserChatWindow.Modes)values[1];

            if (item.ConversationEnded && mode == NixxisUserChatWindow.Modes.Agent)
                return false;
            else
                return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
