using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Text;

namespace Nixxis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Window LauncherWindow;

        public bool NoUpdate { get; private set; }
        public bool NoLogin { get; private set; }
        public bool NoDefault { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Client starting");

            foreach(string Arg in e.Args)
            {
                if(Arg.Length > 0 && "-/".IndexOf(Arg[0]) >= 0)
                {
                    string SwArg = Arg.Substring(1);

                    if(SwArg.Equals("noupdate", StringComparison.OrdinalIgnoreCase))
                    {
                        NoUpdate = true;
                    }
                    else if(SwArg.Equals("nologin", StringComparison.OrdinalIgnoreCase))
                    {
                        NoLogin = true;
                    }
                    else if (SwArg.Equals("nodefault", StringComparison.OrdinalIgnoreCase))
                    {
                        NoDefault = true;
                    }
                }
            }

            base.OnStartup(e);

            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(OnDispatcherUnhandledException);

            LauncherWindow = new LauncherWindow();
            LauncherWindow.ShowActivated = true;
            LauncherWindow.Show();

            System.Diagnostics.Trace.WriteLine("Client started");
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string ToTrace = null;

            try
            {
                StringBuilder SB = new StringBuilder(1000);

                SB.AppendLine("Fatal error");

                try
                {
                    SB.AppendLine("Environment:   " + Environment.OSVersion.VersionString);
                    SB.AppendLine("Running mode:  " + (Environment.Is64BitProcess ? "x64" : "x86") + "/" + (Environment.Is64BitOperatingSystem ? "x64" : "x86"));
                    SB.AppendLine("Context:       " + Environment.MachineName ?? "" + ", " + Environment.UserDomainName ?? "" + ", " + Environment.UserName ?? "");
                    SB.AppendLine("Command line:  " + Environment.CommandLine ?? "");
                    SB.AppendLine("Working dir:   " + Environment.CurrentDirectory ?? "");
                    SB.AppendLine("Working set:   " + Environment.WorkingSet.ToString());

                    SB.AppendLine();
                }
                catch
                {
                }

                if (sender != null)
                    SB.AppendLine("Sender:        " + sender.GetType().FullName);

                SB.AppendLine();

                if (e != null)
                {
                    for (Exception Ex = e.Exception; Ex != null; Ex = Ex.InnerException)
                    {
                        SB.AppendLine("Message:       " + Ex.Message ?? "<none>");

                        if (Ex.Source != null)
                            SB.AppendLine("Source:        " + Ex.Source);

                        if (Ex.TargetSite != null)
                            SB.AppendLine("Target site:   " + Ex.TargetSite.DeclaringType.FullName + "." + Ex.TargetSite.Name);

                        if (Ex.StackTrace != null)
                            SB.AppendLine("Stack trace:   " + Ex.StackTrace);

                        SB.AppendLine();
                    }
                }

                SB.AppendLine();

                try
                {
                    foreach (System.Reflection.Assembly Ass in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        SB.Append(Ass.FullName).Append("; ").Append(Ass.GetName().Version.ToString()).Append("; ").Append(Ass.IsFullyTrusted.ToString()).Append("; ").Append(Ass.IsDynamic.ToString()).Append("; ").Append(Ass.GlobalAssemblyCache.ToString()).Append("; ").Append("; ").Append(Ass.ImageRuntimeVersion);

                        SB.AppendLine();
                    }
                }
                catch
                {
                }

                SB.AppendLine();

                try
                {
                    foreach (string Name in System.IO.Directory.GetFiles(Environment.CurrentDirectory))
                    {
                        System.Diagnostics.FileVersionInfo Version = System.Diagnostics.FileVersionInfo.GetVersionInfo(Name);

                        if (Version != null)
                        {
                            SB.Append(Version.ToString());
                            SB.AppendLine();
                        }
                    }
                }
                catch
                {
                }

                SB.AppendLine();

                ToTrace = SB.ToString();
            }
            catch 
            { 
            }

            bool Notified = false;

            if (!string.IsNullOrEmpty(ToTrace))
            {
                try
                {
                    System.Diagnostics.Trace.WriteLine(ToTrace);
                }
                catch
                {
                }

                try
                {
                    Clipboard.SetText(ToTrace);

                    MessageBox.Show("An unexpected error occured and the application will shutdown.\r\n\r\nDetailed information about the error has been copied into the clipboard, please paste and send the text to the support team.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    Notified = true;
                }
                catch
                {
                }
            }

            if (!Notified)
            {
                MessageBox.Show("An unexpected error occured and the application will shutdown.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            Shutdown(666);
        }

        public void LaunchHost(XmlDocument servicesDoc, XmlDocument authentication)
        {
            if (servicesDoc != null)
            {
                foreach (XmlAttribute Attr in servicesDoc.DocumentElement.Attributes)
                {
                    AppDomain.CurrentDomain.SetData("service_" + Attr.Name, Attr.Value);
                }
            }


            MainWindow = Activator.CreateInstance(Type.GetType("Nixxis.Client.MainWindow, ApplicationHost"), servicesDoc, authentication) as Window;

            MainWindow.ShowActivated = true;
            MainWindow.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            MainWindow.Show();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LauncherWindow.Close();
            MainWindow.Activate();
        }
    }
}
