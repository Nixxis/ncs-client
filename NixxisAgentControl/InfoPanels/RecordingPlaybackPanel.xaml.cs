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
using System.ComponentModel;
using System.Windows.Threading;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for RecordingPlaybackPanel.xaml
    /// </summary>
    public partial class RecordingPlaybackPanel : NixxisPanelSelectorItem
    {
        #region Class data
        private BackgroundWorker m_BkgWorker;
        private BackgroundWorker m_BkgWorkerDownloading;
        private string m_OutputfileName;
        Nixxis.Client.Recording.RecordingFileInformation m_SelectedFile;
        private DispatcherTimer m_MediaPlayerTimer = new DispatcherTimer();
        private bool m_MediaPlayerSliderInUpdate = false;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty HistoryItemProperty = DependencyProperty.Register("HistoryItem", typeof(ContactHistoryItem), typeof(RecordingPlaybackPanel), new FrameworkPropertyMetadata(HistoryItemPropertyChanged));
        public ContactHistoryItem HistoryItem
        {
            get { return (ContactHistoryItem)GetValue(HistoryItemProperty); }
            set { SetValue(HistoryItemProperty, value); }
        }
        public static void HistoryItemPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RecordingPlaybackPanel ctrl = (RecordingPlaybackPanel)obj;

            if (args.NewValue == null)
            {
                ctrl.CleanHistory();
            }
            else
            {
                ctrl.RequestCheckRecordingFiles();
            }

        }

        public static readonly DependencyProperty ConfigProperty = DependencyProperty.Register("Config", typeof(Profile), typeof(RecordingPlaybackPanel), new FrameworkPropertyMetadata(ConfigPropertyChanged));
        public Profile Config
        {
            get { return (Profile)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }
        public static void ConfigPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RecordingPlaybackPanel ctrl = (RecordingPlaybackPanel)obj;
        }

        public static readonly DependencyProperty FileListProperty = DependencyProperty.Register("FileList", typeof(List<Nixxis.Client.Recording.RecordingFileInformation>), typeof(RecordingPlaybackPanel));
        public List<Nixxis.Client.Recording.RecordingFileInformation> FileList
        {
            get { return (List<Nixxis.Client.Recording.RecordingFileInformation>)GetValue(FileListProperty); }
            set { SetValue(FileListProperty, value); }
        }

        public static readonly DependencyProperty SelectedFileProperty = DependencyProperty.Register("SelectedFile", typeof(Nixxis.Client.Recording.RecordingFileInformation), typeof(RecordingPlaybackPanel));
        public Nixxis.Client.Recording.RecordingFileInformation SelectedFile
        {
            get { return (Nixxis.Client.Recording.RecordingFileInformation)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        public static readonly DependencyProperty WorkingProperty = DependencyProperty.Register("Working", typeof(bool), typeof(RecordingPlaybackPanel), new PropertyMetadata(false));
        public bool Working
        {
            get { return (bool)GetValue(WorkingProperty); }
            set { SetValue(WorkingProperty, value); }
        }

        public static readonly DependencyProperty DownloadingProperty = DependencyProperty.Register("Downloading", typeof(bool), typeof(RecordingPlaybackPanel), new PropertyMetadata(false));
        public bool Downloading
        {
            get { return (bool)GetValue(DownloadingProperty); }
            set { SetValue(DownloadingProperty, value); }
        }
        #endregion

        #region Constructors
        public RecordingPlaybackPanel()
        {
            InitializeComponent();

            m_MediaPlayerTimer = new DispatcherTimer();
            m_MediaPlayerTimer.Interval = TimeSpan.FromSeconds(1);
            m_MediaPlayerTimer.Tick += new EventHandler(MediaPlayerTimer_Tick);
        }
        #endregion

        #region Checking file count
        private ContactHistoryItem m_Th_HistoryItem;
        private Profile m_Th_Config;
        private List<Nixxis.Client.Recording.RecordingFileInformation> m_Th_List;

        private void RequestCheckRecordingFiles()
        {
            if (HistoryItem == null)
                return;
            if (Config == null)
                return;

            OnBringToFront();
            
            Working = true;        
            m_Th_HistoryItem = HistoryItem;
            m_Th_Config = Config;

            System.Diagnostics.Trace.WriteLine(string.Format("Checking contact {0} with recording id {1}.", m_Th_HistoryItem.ContactId, m_Th_HistoryItem.RecordingId));
            m_BkgWorker = new BackgroundWorker();
            m_BkgWorker.DoWork += new DoWorkEventHandler((DoWorkEventHandler)((a, b) =>
            {
                try
                {
                    CheckRecordingFiles(m_Th_HistoryItem, m_Th_Config);
                }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("Error getting file list. Error: {0}", error.ToString()));
                }
            }));
            m_BkgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_BkgWorker_RunWorkerCompleted);
            m_BkgWorker.RunWorkerAsync();
        }

        private void CleanHistory()
        {
            StopMedia();
            FileList = new List<Recording.RecordingFileInformation>();
            lstFiles.ItemsSource = null;
        }

        private void CheckRecordingFiles(ContactHistoryItem historyItem, Profile config)
        {
            List<Nixxis.Client.Recording.RecordingFileInformation> list = new List<Recording.RecordingFileInformation>();

            if (historyItem.Media == "V")
                list = Nixxis.Client.Recording.RecordingTools.GetFileList(historyItem.RecordingId, Recording.MediaTypes.Voice, config.Data);
            else if (historyItem.Media == "M")
                list = Nixxis.Client.Recording.RecordingTools.GetFileList(historyItem.RecordingId, Recording.MediaTypes.Mail, config.Data);
            else if (historyItem.Media == "C")
                list = Nixxis.Client.Recording.RecordingTools.GetFileList(historyItem.RecordingId, Recording.MediaTypes.Chat, config.Data);

            m_Th_List = list;
        }

        private void m_BkgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FileList = m_Th_List;
            lstFiles.ItemsSource = FileList;

            Working = false;
            m_Th_Config = null;
            m_Th_HistoryItem = null;
            m_Th_List = null;
        }

        #endregion

        #region Downloading
        private void RequestPlayFile()
        {
            Downloading = true;
            m_SelectedFile = SelectedFile;

            m_BkgWorkerDownloading = new BackgroundWorker();
            m_BkgWorkerDownloading.DoWork += new DoWorkEventHandler((DoWorkEventHandler)((a, b) =>
            {
                m_OutputfileName = DownLoadFile(a, b);

                if (string.IsNullOrEmpty(m_OutputfileName))
                {
                    m_OutputfileName = DownLoadFile(a, b);
                }
            }));
            m_BkgWorkerDownloading.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_BkgWorkerDownloading_RunWorkerCompleted);
            m_BkgWorkerDownloading.RunWorkerAsync();
        }
        
        private void m_BkgWorkerDownloading_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                mediaPlayer.Source = new Uri(m_OutputfileName);
                PlayMedia();
            }
            catch { }
            m_SelectedFile = null;
            Downloading = false;
        }
        
        private string DownLoadFile(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = ((BackgroundWorker)sender);

            worker.WorkerSupportsCancellation = true;

            string strRtn = string.Empty;

            strRtn = Nixxis.Client.Recording.TransferFile.DownloadFile(null, m_SelectedFile);

            return strRtn;
        }
        #endregion

        #region Media element controls
        public void PlayMedia()
        {
            mediaPlayer.Play();
            m_MediaPlayerTimer.Start();
        }

        public void StopMedia()
        {
            mediaPlayer.Stop();
            m_MediaPlayerTimer.Stop();
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            slTimeProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            slTimeProgress.Value = mediaPlayer.Position.TotalMilliseconds;
            lblCurrentTime.Text = Format.ToDisplayTimeSpan(mediaPlayer.Position);
            lblTotalTime.Text = Format.ToDisplayDuration(mediaPlayer.NaturalDuration);
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopMedia();
        }

        private void MediaPlayerTimer_Tick(object sender, EventArgs e)
        {
            if (!m_MediaPlayerSliderInUpdate)
            {
                m_MediaPlayerSliderInUpdate = true;

                slTimeProgress.Value = mediaPlayer.Position.TotalMilliseconds;
                lblCurrentTime.Text = Format.ToDisplayTimeSpan(mediaPlayer.Position);

                m_MediaPlayerSliderInUpdate = false;
            }
        }
        #endregion

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {

            RequestPlayFile();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopMedia();
        }

        private void slTimeProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!m_MediaPlayerSliderInUpdate)
            {
                m_MediaPlayerSliderInUpdate = true;

                Slider sl = (Slider)sender;

                int SliderValue = (int)sl.Value;

                TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
                mediaPlayer.Position = ts;

                m_MediaPlayerSliderInUpdate = false;
            }
        }

        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer == null)
                return;

            mediaPlayer.Volume = (double)slVolume.Value;
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer == null)
                return;

            if(mediaPlayer.Volume == 0)
                mediaPlayer.Volume = (double)slVolume.Value;
            else
                mediaPlayer.Volume = 0;
        }

    }
}
