using System;
using Newtonsoft.Json;
using Nixxis.Client.Agent.Reusable;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SocialMediaElements;
using DependencyObject = System.Windows.DependencyObject;

namespace Nixxis.Client.Agent
{
    public partial class SocialMediaPanelControl : IMailPanel
    {
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(SocialMediaPanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));   //Register a dependencyObject for the ClientLink property, this will call when the ClientLink changes 
        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfoMail), typeof(SocialMediaPanelControl), new FrameworkPropertyMetadata(ContactPropertyChanged));     //Register a dependencyObject for the Contact property, this will call when the Contact changes 
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SocialMediaPanelControl), new FrameworkPropertyMetadata(true, IsReadOnlyPropertyChanged)); //Register a dependencyObject for the IsReadOnly property, this will call when the IsReadOnly changes 
        public static readonly DependencyProperty MailMessageProperty = DependencyProperty.Register("MailMessage", typeof(MailMessage), typeof(SocialMediaPanelControl), new FrameworkPropertyMetadata(MailMessagePropertyChanged)); //Register a dependencyObject for the MailMessage property, this will call when the MailMessage changes 


        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) { }                     //Called when the ClientLink property was changed
        public static void IsReadOnlyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) { }                     //Called when the IsReadOnly property was changed
        public static void MailMessagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) { }                    //Called when the MailMessage property was changed

        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)                            //Called when the Contact property was changed
        {
            SocialMediaPanelControl ctrl = (SocialMediaPanelControl)obj;

            if (args.NewValue != null && args.NewValue != args.OldValue)
            {
                if (string.IsNullOrEmpty(ctrl.Contact.Script))
                    ctrl.contentSplitterHor.MaximizeBottom();                                                                               //We add a screensplitter and maximize the bottom

                if (args.NewValue != null && args.NewValue != args.OldValue)
                    ctrl.MailMessage = ((ContactInfoMail)args.NewValue).Message;

                ctrl.StartCheckingForUpdates();                                                                                             //We start checking for layoutupdates
            }
        }

        



        private const string REPLY_EMPTY = "Reply";                                                                                         //Default text for the reply textbox, we will use this to check if the reply textbox was clicked
        private const int CHECKCOMMENTUPDATES_TIMEOUT = 10000;                                                                              //How long do we want to wait to check for new comments


        private int _lastRowIndex;
        private Post _post;
        private CancellationTokenSource _cancellationTokenSource;


        public SocialMediaPanelControl()
        {
            InitializeComponent();

            _lastRowIndex = 2;                                                                                                              //Initialize lastRowIndex to 2, because we allready have 2 rows defined (for the picture and (username/message))
        }

        public SocialMediaPanelControl(HttpLink link, ContactInfoMail info) : this()
        {
            ClientLink = link;
            Contact = info;
        }


        /// <summary>
        /// Used to define if the controls are readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }                                                                          
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Get's the current contact were working with
        /// </summary>
        public ContactInfoMail Contact
        {
            get { return (ContactInfoMail) GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }

        /// <summary>
        /// Get's the default clientlink
        /// </summary>
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }

        /// <summary>
        /// Used to define a mailmessage
        /// </summary>
        public MailMessage MailMessage
        {
            get { return (MailMessage)GetValue(MailMessageProperty); }
            set { SetValue(MailMessageProperty, value); }
        }


        private void reply_mouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox replyTextBox = sender as TextBox;

            if (replyTextBox != null && replyTextBox.Tag.ToString() == REPLY_EMPTY)                                                         //Check if we clicked on the replyTextbox and if it contains the string defined in REPLY_EMPTY
                replyTextBox.Clear();                                                                                                       //If so, clear it's text
        }


        public void StartCheckingForUpdates()
        {
            using (_cancellationTokenSource = new CancellationTokenSource())
                Task.Factory.StartNew(CheckForUpdates);                                                                                     //check for new updates in a new thread
        }

        public void CheckForUpdates()
        {
            BackgroundWorker worker = new BackgroundWorker();                                                                               //We use a backgroundworker to check for new updates so the UI won't block

            worker.DoWork += (sender, args) =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)                                                                   //keep running until we cancel the CancelationTokenSource
                {
                    MakePostLayout();                                                                                                       //Update the layout

                    Thread.Sleep(CHECKCOMMENTUPDATES_TIMEOUT);                                                                              //Wait for the specified amout of time
                }
            };
            worker.RunWorkerAsync();
        }


        ///<summary>
        ///Creates the layout for all the posts, this is GRID based
        ///</summary>
        private void MakePostLayout()
        {
            Dispatcher.Invoke(
                new Action(() =>                                                                                                                       //Since we're working in another thread, we must invoke it on the GUI dispatcher
                    {
                        txtInformation.Content = "Fetching post from the server";

                        dynamic postJson = ReusableMethods.MakeRequest(ClientLink.BaseUri, Contact.ContentLink, "getpost");                 //We fetch the Post json string from the server

                        Post post = JsonConvert.DeserializeObject<Post>(postJson);                                                          //We deserialize the post so we can use it later on

                        if (post != null)
                        {
                            txtInformation.Content = "Adding post layout to the screen";
                            if (_post == null)                                                                                              //If it's the first time we received a Post we create the header layout
                            {
                                lblMessage.Text = post.Message;
                                lblName.Content = post.User.Name;
                                imgSocialMediaProfilePicture.Source = post.User.UserImage;

                                scvSocialMediaContent.Visibility = Visibility.Visible;                                                      //Show the elements that were hidden first
                                txtReply.Visibility = Visibility.Visible;
                            }

                            IEnumerable<Post> comments = GetNewOrAllComments(post.CommentList);                                             //Helper method to extract only the new Comments from the post, if it's the first time it will return all comments

                            if (comments.Any())
                            {
                                txtInformation.Content = "Adding comments";
                                MakeCommentsLayout(comments);                                                                               //Start generating the comment layout
                            }
                            

                            txtInformation.Content = string.Empty;                                                                          //We don't need any information, the post was loaded

                            _post = post;
                        }
                    }));
        }

        /// <summary>
        /// Extracts all new comments not in _post yet, if none returns an empty list
        /// </summary>
        /// <param name="commentList">The commentlist queried</param>
        /// <returns>All new comments</returns>
        private IEnumerable<Post> GetNewOrAllComments(IEnumerable<Post> commentList)
        {
            IEnumerable<Post> currentCommentList = _post == null ? new List<Post>() : _post.CommentList;                                    //if the current commentList is null for any reason we just create a new List, else we get the commentList of _post

            return commentList.Except(currentCommentList);                                                                                  //Except gets all comments which are NOT in _post.CommentList
        }

        private void MakeCommentsLayout(IEnumerable<Post> commentList)
        {
            foreach (Post commentItem_ in commentList)
            {
                UIElement userPicture = new Image                                                                                           //Creates the users Image
                {
                    Source = commentItem_.User.UserImage,
                    Margin = new Thickness(0, 15, 0, 0),                                                                                    //We want to space the image a bit, so we give it a margin:top of 15
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 50,
                    Height = 50,
                };

                UIElement userName = new Label                                                                                              //Create the label for the username
                {
                    Content = commentItem_.User.Name,
                    Margin = new Thickness(5, 15, 0, 0),
                    Foreground = new SolidColorBrush(Colors.Black),                                                                         //Let's make the color of the label black (I'm overriding the Nixxis's default, which is gray I think)
                    HorizontalAlignment = HorizontalAlignment.Stretch,                                                                      
                    FontSize = 13,                                                                                                          //make the text a bit larger 
                    FontWeight = FontWeights.Bold                                                                                           //Put it in bold, so it's nice and clear
                };

                UIElement commentMessage = new TextBox                                                                                      //Implement the comment message as a textbox, this will make it look nicer
                {
                    Text = commentItem_.Message,
                    Margin = new Thickness(2, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    IsReadOnly = true                                                   
                };

                grdSocialMediaContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                                   //Assign some rows to the GRID, so we can add the new elements
                grdSocialMediaContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                grdSocialMediaContent.AddElementToGrid(userPicture, ++_lastRowIndex, 2, 2);                                                 //Adds the elment to the Grid using an extention method, put it on the next row of the lastRowIndex we defined
                grdSocialMediaContent.AddElementToGrid(userName, _lastRowIndex, 3, 1);
                grdSocialMediaContent.AddElementToGrid(commentMessage, ++_lastRowIndex, 3, 1);
            }
        }


        /// <summary>
        /// Get's a list of all attachmentsId's specified for this type of media
        /// </summary>
        /// <returns>A list of attachmentId's</returns>
        public List<string> GetAttachmentIds()
        {
            return new List<string>();
        }

        /// <summary>
        /// Extracts the reply message send by the Agent
        /// </summary>
        /// <returns></returns>
        public string GetHtmlText()
        {
            return txtReply.Text;
        }
























       public void Connect(int connectionId, object target){}
    }
}
