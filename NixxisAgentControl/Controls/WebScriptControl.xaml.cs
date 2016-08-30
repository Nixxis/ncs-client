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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for WebScriptControl.xaml
    /// </summary>
    public partial class WebScriptControl : UserControl
    {
        #region Class data
        private BrowserExternal m_BrowserExternal;
        private bool m_BrowserNavigateStarted;
        private bool m_BrowserLoaded;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(WebScriptControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            WebScriptControl ws = (WebScriptControl)obj;
            ws.DisplayScript();
        }

        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfo), typeof(WebScriptControl), new FrameworkPropertyMetadata(ContactPropertyChanged));
        public ContactInfo Contact
        {
            get { return (ContactInfo)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            WebScriptControl ws = (WebScriptControl)obj;
            ws.DisplayScript();
        }
        #endregion

        public WebScriptControl()
        {
            InitializeComponent();

            Browser.KeyDown += Browser_KeyDown;

        }

        void Browser_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                try
                {
                    dynamic document = Browser.Document;
                    document.ExecCommand("Delete", false, null);
                    e.Handled = true;
                }
                catch
                {
                }
                return;
            } 

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                try
                {
                    dynamic document = Browser.Document;

                    if (e.Key == Key.C)
                    {
                        document.ExecCommand("Copy", false, null);
                        e.Handled = true;
                    }
                    else if (e.Key == Key.A)
                    {
                        document.ExecCommand("SelectAll", false, null);
                        e.Handled = true;
                    }
                    else if (e.Key == Key.V)
                    {
                        document.ExecCommand("Paste", false, null);
                        e.Handled = true;
                    }
                }
                catch
                {
                }
                return;
            }
        }


        internal ImageSource GetSnapshot(FrameworkElement target)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)target.ActualWidth, (int)target.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(target);
                context.DrawRectangle(brush, null, new Rect(new Point(), bounds.Size));
            }

            renderBitmap.Render(visual);
            
            return renderBitmap;
        }
        private void DisplayScript()
        {
            try
            {
               
                if (Contact != null && ClientLink != null && m_BrowserLoaded && !m_BrowserNavigateStarted && !string.IsNullOrEmpty(Contact.Script))
                {
                    m_BrowserNavigateStarted = true;
                    
                    string strTemp =
                    string.Format(Contact.Script,
                                Microsoft.JScript.GlobalObject.escape(Contact.CustomerId),
                                Microsoft.JScript.GlobalObject.escape(Contact.Id),
                                Microsoft.JScript.GlobalObject.escape(Contact.Activity),
                                Microsoft.JScript.GlobalObject.escape(Contact.From),
                                Microsoft.JScript.GlobalObject.escape(Contact.To),
                                Microsoft.JScript.GlobalObject.escape(ClientLink.AgentId),
                                Microsoft.JScript.GlobalObject.escape(Contact.ContactListId),
                                Microsoft.JScript.GlobalObject.escape(ClientLink.Account),
                                Microsoft.JScript.GlobalObject.escape(ClientLink.SessionId),
                                Microsoft.JScript.GlobalObject.escape(Contact.Campaign),
                                Microsoft.JScript.GlobalObject.escape(Contact.UUI),
                                Microsoft.JScript.GlobalObject.escape(Contact.Queue),
                                Microsoft.JScript.GlobalObject.escape(Contact.Language)
                                );

                    if (strTemp.StartsWith("shell://"))
                    {
                        System.Diagnostics.Process.Start(strTemp.Substring(8));
                        m_BrowserNavigateStarted = true;
                    }
                    else
                    {
                        m_BrowserExternal = new BrowserExternal(ClientLink, Contact);
                        Browser.ObjectForScripting = m_BrowserExternal;

                        Browser.Navigate(strTemp);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        public void Navigate(string url)
        {
            Browser.Navigate(url);
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                m_BrowserLoaded = true;
                DisplayScript();

            }
            catch
            {
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void Browser_Navigated(object sender, NavigationEventArgs e)
        {
        }

    }
}
