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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for ChatPanelControl.xaml
    /// </summary>
    public partial class ChatPanelControl : UserControl
    {
        #region Class data
        /// <summary>
        /// Defines the behavior when the return/enter key is pressed
        /// 
        /// 0 = Default
        /// 1 = Send text
        /// 2 = New line
        /// </summary>
        private int ReturnBehavior = 0;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(ChatPanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set 
            { 
                SetValue(ClientLinkProperty, value);

                if (value != null)
                {
                    if ("send".Equals(value.ClientSettings["ChatEnterKeyBehavior"], StringComparison.OrdinalIgnoreCase))
                    {
                        ReturnBehavior = 1;
                    }
                    else if ("newline".Equals(value.ClientSettings["ChatEnterKeyBehavior"], StringComparison.OrdinalIgnoreCase))
                    {
                        ReturnBehavior = 2;
                    }
                }
            }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ChatPanelControl panel = (ChatPanelControl)obj;
            panel.GetPredefinedTexts();
        }

        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfoChat), typeof(ChatPanelControl), new FrameworkPropertyMetadata(ContactPropertyChanged));
        public ContactInfoChat Contact
        {
            get { return (ContactInfoChat)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ChatPanelControl panel = (ChatPanelControl)obj;


            panel.GetPredefinedTexts();
        }

        public static readonly DependencyProperty PredefinedTextSourceProperty = DependencyProperty.Register("PredefinedTextSource", typeof(PredefinedTextCollection), typeof(ChatPanelControl), new FrameworkPropertyMetadata(PredefinedTextSourcePropertyChanged));
        public PredefinedTextCollection PredefinedTextSource
        {
            get { return (PredefinedTextCollection)GetValue(PredefinedTextSourceProperty); }
            set { SetValue(PredefinedTextSourceProperty, value); }
        }
        public static void PredefinedTextSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //ChatPanelControl ws = (ChatPanelControl)obj;

        }

        #endregion

        #region Properties
        public PauseCodeInfo SelectedItem { get; private set; }
        #endregion

        #region Constructors
        public ChatPanelControl()
        {
            InitializeComponent();
            InitControl();
        }

        public ChatPanelControl(HttpLink link, ContactInfoChat info)
        {
            ClientLink = link;
            Contact = info;

            InitializeComponent();

            if (string.IsNullOrEmpty(Contact.Script))
            {
                ContentSplitterHor.MaximizeBottom();
            }

            InitControl();
        }

        private void InitControl()
        {
            predefCrtl.SelectedItem = null;
            predefCrtl.TextSelected +=new PredefinedTextSelectHandeler(predefCrtl_TextSelected);

            Contact.Conversation.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Conversation_CollectionChanged);

            if (ReturnBehavior == 1)
            {
                txtAgentText.AcceptsReturn = false;
            }
            else if (ReturnBehavior == 2)
            {
                txtAgentText.AcceptsReturn = true;
            }

        }

        void Conversation_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    if (e.NewItems.Count > 0)
                    {
                        if (((ChatLine)e.NewItems[0]).LineType == 2)
                        {
                            Contact.RequestAgentAction = true;
                        }
                    }
                }
            }
        }
        #endregion

        #region Members
        public void GetPredefinedTexts()
        {
            if (PredefinedTextSource == null && ClientLink != null && Contact != null)
            {
                PredefinedTextSource = ClientLink.GetPredefinedTexts(Contact.Activity);
            }
        }
        #endregion

        private void predefCrtl_TextSelected(object sender, PredefinedTextItem text)
        {
            if(text != null)
                txtAgentText.Text = text.FullText;

            txtAgentText.Focus();
        }

        private void btnSendChatMsg_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtAgentText.Text))
            {
                Contact.Conversation.Say(txtAgentText.Text);
                txtAgentText.Text = string.Empty;
                txtAgentText.Focus();
            }
        }

        private void txtAgentText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (ReturnBehavior == 1)
                {
                    e.Handled = true;

                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        string NewText = txtAgentText.Text;
                        int SelStart = txtAgentText.SelectionStart;

                        if(txtAgentText.SelectionLength > 0)
                            NewText = NewText.Remove(SelStart, txtAgentText.SelectionLength);

                        NewText = NewText.Insert(SelStart, Environment.NewLine);

                        txtAgentText.Text = NewText;
                        txtAgentText.Select(SelStart + Environment.NewLine.Length, 0);
                    }
                    else
                    {
                        btnSendChatMsg_Click(sender, null);
                    }
                }
                else if (ReturnBehavior == 2)
                {
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        btnSendChatMsg_Click(sender, null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
