using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Security.Permissions;

namespace Nixxis
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    partial class LauncherWindow : Window
    {
        BackgroundWorker Worker = null;
        string DefaultServiceUri = null;
        string ForceServiceUri = null;
        string ForceExtension = "";
        LoginEncryption m_LoginEncrypter;
        XmlDocument Authentication;
        bool FirstTry = true;

        internal string AuthenticationToken { get; set; }

        public LauncherWindow()
        {
            System.Diagnostics.Trace.WriteLine("Launcher window initializing");

            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;


            InitializeComponent();

            Browser.ObjectForScripting = new BrowserExternal(this);

            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri("file:///" + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SplashImg.jpg"));
                bi.EndInit();

                ImgSplash.Source = bi;
            }
            catch
            {
            }

            System.Diagnostics.Trace.WriteLine("Launcher window start in progress");

            LoadDefaults();

            System.Diagnostics.Trace.WriteLine("Launcher window defaults loaded");

            if (!string.IsNullOrEmpty(ForceServiceUri))
            {
                DefaultServiceUri = ForceServiceUri;
            }
            else
            {
                if (Keyboard.Modifiers == (ModifierKeys.Shift | ModifierKeys.Control) || ((App)Application.Current).NoDefault)
                {
                    DefaultServicesLoaded();
                    return;
                }
            }

            System.Diagnostics.Trace.WriteLine("Launcher window defaults processed");

            if (!string.IsNullOrEmpty(DefaultServiceUri))
            {
                System.Diagnostics.Trace.WriteLine("Launcher window has service URI");

                if (((App)Application.Current).NoLogin)
                {
                    DefaultServicesLoaded();
                }
                else
                {
                    StartProgress("Loading default services...", 0, 0);

                    Worker = new BackgroundWorker();
                    Worker.DoWork += (object o, DoWorkEventArgs args) => args.Result = Nixxis.LauncherWindow.LoadServiceList(DefaultServiceUri, null);
                    Worker.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs args) => { StopProgress(); Services = args.Result as ServiceList; Worker = null; DefaultServicesLoaded(); };

                    Worker.RunWorkerAsync();
                }
            }
            else
            {
                DefaultServicesLoaded();
            }

            System.Diagnostics.Trace.WriteLine("Launcher window initialized");
        }

        void DefaultServicesLoaded()
        {
            if ( (Services != null && Services.Count > 0) || ((App)Application.Current).NoLogin)
            {
                ShowConnectControls(false);
                ShowLoginControls(true);
                ShowExtensionControls(string.IsNullOrEmpty(ForceExtension));

                if ((Services.LoginOptions & LoginOptions.TrustIdentification) == 0)
                    FirstTry = false;

                if(FirstTry)
                {
                    txtAccount.Text = Environment.UserName;
                    passwordBox.Password = Environment.UserName + ":7zx$Wj";

                    if(!string.IsNullOrEmpty(ForceExtension))
                    {
                        txtExtension.Text = ForceExtension;
                    }
                    else
                    {
                        txtExtension.Text = Environment.MachineName;
                    }

                    ShowLoginControls(false);
                    ShowExtensionControls(false);
                }
                else
                {
                }

                if (string.IsNullOrEmpty(txtAccount.Text))
                    txtAccount.Focus();
                else
                    passwordBox.Focus();

                if (!string.IsNullOrEmpty(Services.SsoUri))
                {
                    LoginDialog.Visibility = System.Windows.Visibility.Collapsed;
                    Browser.Visibility = System.Windows.Visibility.Visible;

                    Browser.Navigate(Services.SsoUri);
                    return;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ForceServiceUri))
                {
                    MessageBox.Show(this, "Unable to connect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                ShowConnectControls(true);
                ShowLoginControls(false);
                ShowExtensionControls(false);
            }

            if(!ShowOkCancel())
            {
                btnOk_Click(null, null);
            }
        }

        void ShowConnectControls(bool show)
        {
            lblService.Visibility = cboService.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
        }

        void ShowLoginControls(bool show)
        {
            lblAccount.Visibility = txtAccount.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            lblPassword.Visibility = passwordBox.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
        }

        void ShowExtensionControls(bool show)
        {
            lblExtension.Visibility = txtExtension.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
        }

        bool ShowOkCancel()
        {
            bool MustShow = false;

            if (cboService.Visibility == Visibility.Visible && cboService.IsEnabled)
                MustShow = true;

            if (txtAccount.Visibility == Visibility.Visible && txtAccount.IsEnabled)
                MustShow = true;

            if (passwordBox.Visibility == Visibility.Visible && passwordBox.IsEnabled)
                MustShow = true;

            if (txtExtension.Visibility == Visibility.Visible && txtExtension.IsEnabled)
                MustShow = true;

            btnOk.Visibility = (MustShow ? Visibility.Visible : Visibility.Hidden);
            btnCancel.Visibility = (MustShow ? Visibility.Visible : Visibility.Hidden);

            LoginDialog.Visibility = (MustShow ? Visibility.Visible : Visibility.Hidden);

            return MustShow;
        }

        void ServicesLoaded()
        {
            if (((App)Application.Current).NoLogin)
            {
                StopProgress();
                UpdatesDone(null);
            }
            else
            {
                if (Services == null || Services.Count == 0)
                {
                    bool skipLogin = false;
                    if (Boolean.TryParse(ConfigurationManager.AppSettings["SkipLogin"], out skipLogin) && skipLogin)
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        ((App)App.Current).LaunchHost((Services == null || Services.Count == 0) ? null : Services.Source, Authentication);
                    }
                    else
                    {
                        MessageBox.Show(this, "Services not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        DefaultServicesLoaded();
                    }
                }
                else
                {
                    string Account, Password, Extension;

                    string strTempFileName = null;
                    try
                    {
                        strTempFileName = Path.GetTempFileName();
                        if (DownloadRemoteImageFile(new Uri(new Uri(Services["provisioning"].Location), "Client/SplashImg.jpg").ToString(), strTempFileName))
                        {

                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.UriSource = new Uri("file:///" + strTempFileName); 
                            bi.EndInit();

                            ImgSplash.Source = bi;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        try
                        {
                            if (strTempFileName != null)
                                File.Delete(strTempFileName);
                        }
                        catch
                        {
                        }
                    }

                    ShowConnectControls(false);
                    ShowLoginControls(true);
                    ShowExtensionControls(true);

                    if (!ShowOkCancel())
                        btnOk_Click(null, null);

                }
            }
        }

        void LoginDone()
        {
        }

        private static bool DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download it
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                return true;
            }
            else
                return false;
        }

        static XmlDocument AuthenticateUser(string account, string password, string extension)
        {
            try
            {
                string AuthUrl = string.Concat(Services["signon"].Location, "?action=authenticate&username=", Uri.EscapeUriString(account), "&password=", password);

                HttpWebRequest Request = WebRequest.Create(AuthUrl) as HttpWebRequest;
                Request.Method = "GET";

                using (HttpWebResponse Response = Request.GetResponse() as HttpWebResponse)
                {
                    XmlDocument Doc = new XmlDocument();

                    using (Stream ResponseStream = Response.GetResponseStream())
                    {
                        Doc.Load(ResponseStream);
                    }

                    XmlNode LoginNode = Doc.SelectSingleNode("AgentLogin/agent");
                    
                    LoginNode.Attributes.Append(Doc.CreateAttribute("token")).Value = password;
                    LoginNode.Attributes.Append(Doc.CreateAttribute("extension")).Value = extension;
                    return Doc;
                }
            }
            catch
            {
            }

            return null;
        }

        List<FileVersionSettings> UpdatesList;

        void UserAuthenticated()
        {
            if (Authentication == null)
            {
                if (!FirstTry)
                {
                    MessageBox.Show(this, "Invalid user identification", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    FirstTry = false;
                    passwordBox.Clear();
                }

                LoginDialog.Visibility = Visibility.Visible;

                ShowLoginControls(true);
                ShowExtensionControls(string.IsNullOrEmpty(ForceExtension));
                
                ShowOkCancel();

                passwordBox.Focus();

            }
            else
            {
                ValidateLogin();

                if (Services.Contains("provisioning") && !((App)Application.Current).NoUpdate)
                {
                    StartProgress("Loading configuration...", 0, 0);

                    Worker = new BackgroundWorker();
                    Worker.DoWork += (object o, DoWorkEventArgs args) => args.Result = LoadUpdates();
                    Worker.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs args) => { StopProgress(); UpdatesList = args.Result as List<FileVersionSettings>; Worker = null; UpdatesLoaded(); };

                    Worker.RunWorkerAsync();
                }
                else
                {
                    StopProgress();
                    UpdatesDone(null);
                }
            }
        }

        List<FileVersionSettings> LoadUpdates()
        {
            List<FileVersionSettings> List = null;

            if (!LoadUpdateFileList(false, Services["provisioning"].Location, "Client", out List))
            {
                return null;
            }

            return List;
        }

        void UpdatesLoaded()
        {
            if (UpdatesList == null)
            {
                MessageBox.Show(this, "Unable to load system configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoginDialog.Visibility = Visibility.Visible;
                cboService.Focus();
            }
            else
            {
                bool SomeUpdates = false;

                foreach (FileVersionSettings ThisSettings in UpdatesList)
                {
                    if (ThisSettings.MustDownload)
                    {
                        SomeUpdates = true;
                        break;
                    }
                }

                if (SomeUpdates)
                {
                    StartProgress("Loading components...", 0, 100);

                    Worker = new BackgroundWorker();
                    Worker.WorkerReportsProgress = true;
                    Worker.DoWork += (object o, DoWorkEventArgs args) => args.Result = DoUpdates();
                    Worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) => SetProgress(e.ProgressPercentage, e.UserState as string);
                    Worker.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs args) => { StopProgress(); Worker = null; UpdatesDone(args.Result); };

                    Worker.RunWorkerAsync();
                }
                else
                {
                    UpdatesDone(null);
                }
            }
        }

        bool DoUpdates()
        {
            if (((App)Application.Current).NoUpdate)
                return true;

            return UpdateFromList(Worker, false, Services["provisioning"].Location, "Client", UpdatesList);
        }

        void UpdatesDone(object Result)
        {
            StopProgress();
            Close.Visibility = System.Windows.Visibility.Hidden;

            ((App)App.Current).LaunchHost((Services == null || Services.Count == 0) ? null : Services.Source, Authentication);
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void StartProgress(string description, int minimum, int maximum)
        {
            System.Diagnostics.Trace.WriteLine("Start progress: " + (description ?? string.Empty));

            lblProgress.Content = description ?? string.Empty;
            lblProgress.Visibility = Visibility.Visible;

            Progress.Minimum = minimum;
            Progress.Value = minimum;
            Progress.Maximum = maximum;
            Progress.IsIndeterminate = (minimum == maximum);
            Progress.Visibility = Visibility.Visible;
        }

        public void SetProgress(double value, string description)
        {
            Progress.Value = value;

            if (description != null)
                lblProgress.Content = description;
        }

        public void StopProgress()
        {
            lblProgress.Visibility = Visibility.Hidden;
            Progress.Visibility = Visibility.Hidden;
        }

        private void Event_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
                {
                }
                else
                {
                    btnOk_Click(sender, new RoutedEventArgs(e.RoutedEvent));                    
                }
            }
            else if(e.Key == Key.Escape)
            {
                btnCancel_Click(sender, new RoutedEventArgs(e.RoutedEvent));
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Services == null || Services.Count == 0 || (string.IsNullOrEmpty(ForceServiceUri) && DefaultServiceUri != cboService.Text))
            {
                LoginDialog.Visibility = Visibility.Hidden;

                if (((App)Application.Current).NoLogin)
                {
                    UpdatesDone(null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(ForceServiceUri))
                    {
                        MessageBox.Show(this, "Unable to connect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                        return;
                    }

                    StartProgress("Loading services...", 0, 0);

                    DefaultServiceUri = cboService.Text.Trim();

                    Worker = new BackgroundWorker();
                    Worker.DoWork += (object o, DoWorkEventArgs args) => args.Result = Nixxis.LauncherWindow.LoadServiceList(DefaultServiceUri, null);
                    Worker.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs args) => { StopProgress(); Services = args.Result as ServiceList; Worker = null; ServicesLoaded(); };

                    Worker.RunWorkerAsync();
                }
            }
            else
            {
                LoginDialog.Visibility = Visibility.Hidden;

                ValidateService();

                StartProgress("Validating...", 0, 0);

                Worker = new BackgroundWorker();

                if(string.IsNullOrEmpty(AuthenticationToken))
                {
                    string Account = txtAccount.Text;
                    string Extension = txtExtension.Text;
                    string Password = null;

                    using (LoginEncryption LoginEncrypter = new LoginEncryption(new byte[] { 0x06, 0x02, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0xCB, 0x89, 0x08, 0xDB, 0xFB, 0x19, 0x4F, 0xE9, 0x2C, 0xAF, 0x81, 0xC4, 0x4F, 0x74, 0x47, 0x49, 0x38, 0x5F, 0xF3, 0x98, 0x22, 0x59, 0x09, 0x69, 0xD6, 0x07, 0x1A, 0x70, 0x4B, 0xD6, 0x29, 0xF8, 0x6C, 0xDB, 0x10, 0xC1, 0xA2, 0x3C, 0x86, 0x20, 0xEE, 0xDA, 0xAF, 0xEB, 0x1C, 0xC5, 0xE8, 0x6E, 0x9F, 0x14, 0xD2, 0x4B, 0x58, 0xF2, 0x98, 0x41, 0x58, 0x34, 0x46, 0xDA, 0x98, 0x67, 0x5A, 0xD7, 0x51, 0x8C, 0x3F, 0x8D, 0x5D, 0x60, 0x5C, 0x39, 0x1B, 0x03, 0xE4, 0x13, 0xB3, 0x02, 0xC7, 0x23, 0xAF, 0x83, 0x3E, 0x92, 0x07, 0x2D, 0xA6, 0xE1, 0x3C, 0xF6, 0x42, 0x11, 0xCC, 0xDE, 0x4D, 0x8A, 0xD8, 0xF7, 0x3B, 0xC3, 0x92, 0xCF, 0x58, 0x82, 0xB7, 0x09, 0x63, 0xDB, 0xCC, 0x8F, 0x82, 0xA4, 0xC3, 0x89, 0xCB, 0x6D, 0xF4, 0x10, 0xD3, 0x4D, 0x12, 0xD2, 0xB2, 0x7E, 0x8C, 0x2C, 0xF5, 0xAB }))
                    {
                        Password = LoginEncrypter.EncryptPassword(passwordBox.Password, LoginEncryption.Purpose.Authentication);
                    }

                    Worker.DoWork += (object o, DoWorkEventArgs args) => args.Result = AuthenticateUser(Account, Password, Extension);
                }
                else
                {
                    string Extension = txtExtension.Text;

                    Worker.DoWork += (object o, DoWorkEventArgs args) => args.Result = AuthenticateUser("", AuthenticationToken, Extension);
                }

                Worker.RunWorkerCompleted += (object s, RunWorkerCompletedEventArgs args) => { StopProgress(); Authentication = args.Result as XmlDocument; Worker = null; UserAuthenticated(); };

                Worker.RunWorkerAsync();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void LoadDefaults()
        {
            bool NoUserProfile = false;
            string KeyName = "Software\\Nixxis\\NixxisClient";

            try
            {
                string SetKey = System.Configuration.ConfigurationManager.AppSettings["RegistryKey"];

                if(!string.IsNullOrWhiteSpace(SetKey))
                {
                    if (SetKey.StartsWith("\\"))
                        KeyName = SetKey;
                    else
                        KeyName = KeyName + "\\" + SetKey;
                }
            }
            catch
            {
            }

            try
            {
                Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KeyName, false);

                if (ClientKey != null)
                {
                    ForceExtension = ClientKey.GetValue("ForceExtension", "") as string;
                    ForceServiceUri = ClientKey.GetValue("ForceServiceUri", ForceServiceUri) as string;
                    
                    NoUserProfile = (Convert.ToInt32(ClientKey.GetValue("NoUserProfile", 0)) != 0);
                }
            }
            catch
            {
            }

            if (!NoUserProfile)
            {
                try
                {
                    Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false);

                    if (ClientKey != null)
                    {
                        bool ShowLastLogin = (Convert.ToInt32(ClientKey.GetValue("ShowLastLogin", 1)) != 0);
                        bool ShowServices = (Convert.ToInt32(ClientKey.GetValue("ShowServices", 1)) != 0);

                        ForceExtension = ClientKey.GetValue("ForceExtension", ForceExtension) as string;
                        ForceServiceUri = ClientKey.GetValue("ForceServiceUri", ForceServiceUri) as string;

                        if (ShowLastLogin)
                        {
                            txtAccount.Text = ClientKey.GetValue("UserAccount", string.Empty) as string;
                            txtExtension.Text = ClientKey.GetValue("Extension", string.Empty) as string;
                        }

                        if (!string.IsNullOrEmpty(ForceExtension))
                        {
                            txtExtension.Text = ForceExtension;

                            if (!string.IsNullOrEmpty(txtExtension.Text))
                                txtExtension.IsEnabled = false;
                        }

                        Microsoft.Win32.RegistryKey ListKey = ClientKey.OpenSubKey("RecentServices", false);

                        if (ListKey != null)
                        {
                            for (int i = 1; i <= 16; i++)
                            {
                                string Value = ListKey.GetValue(i.ToString(), null) as string;

                                if (!string.IsNullOrEmpty(Value))
                                {
                                    if (DefaultServiceUri == null)
                                    {
                                        DefaultServiceUri = Value;
                                        cboService.Text = Value;
                                    }

                                    if (ShowServices)
                                        cboService.Items.Add(Value);
                                }
                            }

                            ListKey.Close();
                        }

                        ClientKey.Close();
                    }
                }
                catch
                {
                }
            }
        }

        public void ValidateService()
        {
            ValidateService(this.DefaultServiceUri);
        }

        public void ValidateService(string service)
        {
            string KeyName = "Software\\Nixxis\\NixxisClient";

            try
            {
                string SetKey = System.Configuration.ConfigurationManager.AppSettings["RegistryKey"];

                if (!string.IsNullOrWhiteSpace(SetKey))
                {
                    if (SetKey.StartsWith("\\"))
                        KeyName = SetKey;
                    else
                        KeyName = KeyName + "\\" + SetKey;
                }
            }
            catch
            {
            }

            try
            {
                Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName);

                if (ClientKey != null)
                {
                    Microsoft.Win32.RegistryKey ListKey = ClientKey.CreateSubKey("RecentServices");

                    if (ListKey != null)
                    {
                        List<string> Values = new List<string>();
                        int i;

                        for (i = 1; i <= 16; i++)
                        {
                            string Value = ListKey.GetValue(i.ToString(), null) as string;

                            if (!string.IsNullOrEmpty(Value))
                            {
                                Values.Add(Value);
                            }
                        }

                        for (i = 0; i < Values.Count; i++)
                        {
                            if (Values[i] == service)
                            {
                                Values.RemoveAt(i);
                            }
                        }

                        Values.Insert(0, service);

                        for (i = 1; i <= 16; i++)
                        {
                            if (i <= Values.Count)
                            {
                                ListKey.SetValue(i.ToString(), Values[i - 1]);
                            }
                            else
                            {
                                if (ListKey.GetValue(i.ToString()) != null)
                                    ListKey.DeleteValue(i.ToString());
                            }
                        }

                        ListKey.Close();
                    }

                    ClientKey.Close();
                }
            }
            catch
            {
            }
        }

        public void ValidateLogin()
        {
            string KeyName = "Software\\Nixxis\\NixxisClient";

            try
            {
                string SetKey = System.Configuration.ConfigurationManager.AppSettings["RegistryKey"];

                if (!string.IsNullOrWhiteSpace(SetKey))
                {
                    if (SetKey.StartsWith("\\"))
                        KeyName = SetKey;
                    else
                        KeyName = KeyName + "\\" + SetKey;
                }
            }
            catch
            {
            }

            try
            {
                Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName);

                if (ClientKey != null)
                {
                    ClientKey.SetValue("UserAccount", txtAccount.Text, Microsoft.Win32.RegistryValueKind.String);
                    ClientKey.SetValue("Extension", txtExtension.Text, Microsoft.Win32.RegistryValueKind.String);

                    ClientKey.Close();
                }
            }
            catch
            {
            }
        }

        private void MySelf_MouseDown(object sender, MouseButtonEventArgs e)
        {

            DragMove();
        }

        public string ProductVersion
        {
            get
            {
                Version v = Assembly.GetEntryAssembly().GetName().Version;
                return string.Format("{0}.{1}", v.Major, v.Minor);
            }
        }

        public string Version
        {
            get
            {
                Version v = Assembly.GetEntryAssembly().GetName().Version;
                return string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build); 
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        internal void CloseBrowser()
        {
            Browser.Visibility = System.Windows.Visibility.Collapsed;
            LoginDialog.Visibility = System.Windows.Visibility.Visible;

            ServicesLoaded();
        }
    }

    public class NotEmptyOrNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FormatStringExtension : MarkupExtension
    {
        class ConcatStringSingleBinding : IValueConverter
        {
            public string FormatString { get; set; }
            public TranslationContext TranslationContext { get; set; }

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value == DependencyProperty.UnsetValue)
                    return string.Empty;

                if (TranslationContext == null)
                    return string.Format(FormatString, value);
                return string.Format(TranslationContext.Translate(FormatString), value);
            }

            public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class ConcatStringMultiBinding : IMultiValueConverter
        {
            public string FormatString { get; set; }
            public TranslationContext TranslationContext { get; set; }

            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                foreach (object obj in values)
                    if (obj == DependencyProperty.UnsetValue)
                        return string.Empty;

                if (TranslationContext == null)
                    return string.Format(FormatString, values);

                return string.Format(TranslationContext.Translate(FormatString), values);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        public BindingBase BindTo { get; set; }
        public string FormatString { get; set; }
        public TranslationContext TranslationContext { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (BindTo is MultiBinding)
                ((MultiBinding)BindTo).Converter = new ConcatStringMultiBinding { FormatString = FormatString, TranslationContext = TranslationContext };
            else
                ((Binding)BindTo).Converter = new ConcatStringSingleBinding { FormatString = FormatString, TranslationContext = TranslationContext };

            return BindTo.ProvideValue(serviceProvider);
        }
    }


    public class TranslationContext
    {
        private static TranslationContext m_TranslationContext = new TranslationContext();

        public static TranslationContext Default
        {
            get
            {
                return m_TranslationContext;
            }
        }

        public string Context { get; set; }

        public TranslationContext()
        {
        }
        public TranslationContext(string context)
        {
            Context = context;
        }




        public string Translate(string value)
        {

            return value;
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class TranslationConverter : IValueConverter
    {

        private static TranslationConverter m_TranslationConverter = new TranslationConverter();

        public static TranslationConverter Default
        {
            get
            {
                return m_TranslationConverter;
            }
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                TranslationContext context = null;
                if (value is TranslationContext)
                {
                    context = (TranslationContext)value;

                    return context.Translate(parameter.ToString());
                }
                else
                {
                    if (parameter != null && parameter is TranslationContext)
                    {
                        context = (TranslationContext)parameter;
                        return context.Translate(value.ToString());
                    }
                }
            }
            catch
            {
            }
            return parameter.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class BrowserExternal
    {
        LauncherWindow m_Owner;

        public BrowserExternal(LauncherWindow owner)
        {
            m_Owner = owner;
        }

        public void Done()
        {
            m_Owner.CloseBrowser();
        }

        public void SetAuthenticationToken(string token)
        { 
            m_Owner.AuthenticationToken = token;
        }
    }
}