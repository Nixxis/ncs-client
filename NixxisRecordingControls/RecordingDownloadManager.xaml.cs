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
using ContactRoute.Recording.Config;
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections;

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for RecordingDownloadManager.xaml
    /// </summary>
    public partial class RecordingDownloadManager : UserControl
    {
        #region Class data
        private Timer m_DownloadCheckTimer = new Timer();
        private int m_DownloadConcurrentFiles = 1;
        private int m_DownloadCheckInterval = 500;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty DownloadListProperty = DependencyProperty.Register("DownloadList", typeof(DownloadManagerList), typeof(RecordingDownloadManager));
        public DownloadManagerList DownloadList
        {
            get { return (DownloadManagerList)GetValue(DownloadListProperty); }
            set { SetValue(DownloadListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(RecordingDownloadManager));
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty Stat_TotalProperty = DependencyProperty.Register("Stat_Total", typeof(int), typeof(RecordingDownloadManager), new PropertyMetadata(0));
        public int Stat_Total
        {
            get { return (int)GetValue(Stat_TotalProperty); }
            set { SetValue(Stat_TotalProperty, value); }
        }

        public static readonly DependencyProperty Stat_SucccessProperty = DependencyProperty.Register("Stat_Succcess", typeof(int), typeof(RecordingDownloadManager), new PropertyMetadata(0));
        public int Stat_Succcess
        {
            get { return (int)GetValue(Stat_SucccessProperty); }
            set { SetValue(Stat_SucccessProperty, value); }
        }

        public static readonly DependencyProperty Stat_FinishedProperty = DependencyProperty.Register("Stat_Finished", typeof(int), typeof(RecordingDownloadManager), new PropertyMetadata(0));
        public int Stat_Finished
        {
            get { return (int)GetValue(Stat_FinishedProperty); }
            set { SetValue(Stat_FinishedProperty, value); }
        }

        public static readonly DependencyProperty Stat_FailedProperty = DependencyProperty.Register("Stat_Failed", typeof(int), typeof(RecordingDownloadManager), new PropertyMetadata(0));
        public int Stat_Failed
        {
            get { return (int)GetValue(Stat_FailedProperty); }
            set { SetValue(Stat_FailedProperty, value); }
        }
        //
        //Config values
        //
        public static readonly DependencyProperty ConfigProperty = DependencyProperty.Register("Config", typeof(Profile), typeof(RecordingDownloadManager));
        public Profile Config
        {
            get { return (Profile)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }
        #endregion

        #region constructor
        public RecordingDownloadManager()
        {
            InitializeComponent();
        }
        #endregion

        #region Members override
        public override void BeginInit()
        {
            base.BeginInit();

            this.DownloadList = new DownloadManagerList();
        }
        protected override void OnInitialized(EventArgs e)
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(RecordingDownloadManagerControl_IsVisibleChanged);
            
            base.OnInitialized(e);

            m_DownloadCheckTimer.Interval = m_DownloadCheckInterval;
            m_DownloadCheckTimer.Elapsed += new ElapsedEventHandler(DownloadCheckTimer_Elapsed);
        }
        #endregion

        #region Members
        #region GUI
        public void SetToolbarPanel()
        {
        }

        public void RemoveToolbarPanel()
        {
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Managment list
        public void AddDownloadItem(DownloadManagerItem item)
        {
            if (item.ContactItem.MediaType == MediaTypes.Voice)
                AddVoiceItem(item);
            if (item.ContactItem.MediaType == MediaTypes.Chat)
                AddChatItem(item);
            if (item.ContactItem.MediaType == MediaTypes.Mail)
                AddMailItem(item);

            m_DownloadCheckTimer.Start();

            CalculateDownloadStats();
        }

        public void AddVoiceItem(DownloadManagerItem downloadItem)
        {
            Tools.Log(string.Format("Adding 1 voice item. Item info: {0}", downloadItem.ContactItem.ContactId), Tools.LogType.Info);

            this.DownloadList.Add(downloadItem);
        }

        public void AddChatItem(DownloadManagerItem item)
        {
        }

        public void AddMailItem(DownloadManagerItem item)
        {
        }
        #endregion       

        #endregion

        #region Handlers
        private void RecordingDownloadManagerControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)sender).IsVisible)
            {
                SetToolbarPanel();
            }
            else
            {
                RemoveToolbarPanel();
            }
        }

        int timeCnt = 0;
        private void DownloadCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Tools.Log(string.Format("--> Timer {0}: DownloadCheckTimer_Elapsed", timeCnt), Tools.LogType.Info);

            int itemProcessing = 0;
            int itemWaiting = 0;

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    //Check how many item are currently downloaded
                    foreach (DownloadManagerItem item in this.DownloadList)
                    {
                        if (item.State == DownloadManagerItemStates.Downloading 
                            || item.State == DownloadManagerItemStates.CheckingItem
                            || item.State == DownloadManagerItemStates.Checked)
                            itemProcessing++;
                        else if (item.State == DownloadManagerItemStates.Waiting)
                            itemWaiting++;
                    }
                    Tools.Log(string.Format("--> Timer {0}: itemDownloading:{1}. itemWaiting:{2}", timeCnt, itemProcessing, itemWaiting), Tools.LogType.Info);

                    //Check if something has to be downloaded
                    if (itemProcessing < m_DownloadConcurrentFiles)
                    {
                        Tools.Log(string.Format("--> Timer {0}: find next to download", timeCnt), Tools.LogType.Info);
                        foreach (DownloadManagerItem item in this.DownloadList)
                        {
                            Tools.Log(string.Format("--> Timer {0}: checking item {1}", timeCnt, item.ContactItem.ContactId), Tools.LogType.Info);

                            if (item.State == DownloadManagerItemStates.Waiting)
                            {
                                item.CheckDownloadItem(Config.Data);

                                if (item.State == DownloadManagerItemStates.Checked)
                                {
                                    item.DownloadFiles(Config.Data);
                                }

                                break;
                            }
                        }
                    }

                    CalculateDownloadStats();
                }));

            if (itemWaiting <= 0 && itemProcessing <= 0)
            {
                m_DownloadCheckTimer.Stop();
            }
            
            Tools.Log(string.Format("--> Timer {0}: End of timer.", timeCnt), Tools.LogType.Info);

            timeCnt++;

            if (timeCnt > 10000000)
                timeCnt = 0;
        }

        private void CalculateDownloadStats()
        {
            int st_Su = 0;
            int st_Fi = 0;
            int st_Fa = 0;

            lock (DownloadList)
            {
                this.Stat_Total = DownloadList.Count;

                foreach (DownloadManagerItem item in this.DownloadList)
                {
                    if (item.State == DownloadManagerItemStates.Downloaded)
                        st_Su++;

                    if (item.State == DownloadManagerItemStates.Downloaded
                        || item.State == DownloadManagerItemStates.Failed
                        || item.State == DownloadManagerItemStates.NotFound)
                        st_Fi++;

                    if (item.State == DownloadManagerItemStates.Failed
                        || item.State == DownloadManagerItemStates.NotFound)
                        st_Fa++;
                }
            }
            this.Stat_Failed = st_Fa;
            this.Stat_Finished = st_Fi;
            this.Stat_Succcess = st_Su;
        }
        #endregion

        private void btnActionRetry_Click(object sender, RoutedEventArgs e)
        {
            //TO DO

            //this.DownloadList[this.DownloadList.Count - 1].StartDownload();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Tag == null || btn.Tag.Equals("Stop"))
            {
                m_DownloadCheckTimer.Stop();
                btn.Tag = "Start";
            }
            else
            {
                m_DownloadCheckTimer.Start();
                btn.Tag = "Stop";
            }

        }
    }
}
