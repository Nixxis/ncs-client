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
using ContactRoute.Recording.Config;
using System.ComponentModel;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for CallCenterDownloadingFile.xaml
    /// </summary>
    public partial class CallCenterDownloadingFile : Window
    {
        public static TranslationContext TranslationContext = new TranslationContext("CallCenterDownloadingFile");
        #region Class Data
        private ProfileData m_ConfigData = null;
        private ContactData m_ContactData = null;
        private RecordingFileInformation m_RecordingFileInfo = null;

        private string m_OutputfileName = string.Empty;
        
        private BackgroundWorker m_BkgWorker = null;
        private BackgroundWorkerProgress m_ProgressState = null;
        private bool m_Error = false;
        #endregion
        
        #region Properties
        public ProfileData ConfigData
        {
            get { return m_ConfigData; }
            set { m_ConfigData = value; }
        }
        public ContactData ContactData
        {
            get { return m_ContactData; }
            set { m_ContactData = value; }
        }
        public RecordingFileInformation RecordingFileInfo
        {
            get { return m_RecordingFileInfo; }
            set { m_RecordingFileInfo = value; }
        }

        public string OutputfileName
        {
            get { return m_OutputfileName; }
            set { m_OutputfileName = value; }
        }
        #endregion

        #region Constructor
        public CallCenterDownloadingFile()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        #endregion

        #region Members

        public void GetFile()
        {
            Tools.Log(string.Format("Call Center Download file. Getfile {0}", m_RecordingFileInfo.DownloadUrl), Tools.LogType.Info);

            lblHeading.Content = TranslationContext.Translate("Downloading file ...");

            m_BkgWorker = new BackgroundWorker();
            m_BkgWorker.DoWork += new DoWorkEventHandler((DoWorkEventHandler)((a, b) =>
            {
                Tools.Log(string.Format("BackgroundWorker started."), Tools.LogType.Info);

                m_OutputfileName = DownLoadFile(a, b);

                if (string.IsNullOrEmpty(m_OutputfileName))
                {
                    Tools.Log(string.Format("Retry to download file {0}", m_RecordingFileInfo.DownloadUrl), Tools.LogType.Info);
                    m_OutputfileName = DownLoadFile(a, b);
                }

                Tools.Log(string.Format("BackgroundWorker Stopped."), Tools.LogType.Info);
            }));
            m_BkgWorker.ProgressChanged += new ProgressChangedEventHandler(LoadContactsWorker_ProgressChanged);
            m_BkgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadContactsWorker_RunWorkerCompleted);
            m_BkgWorker.RunWorkerAsync();
        }

        private string DownLoadFile(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = ((BackgroundWorker)sender);

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            m_ProgressState = new BackgroundWorkerProgress();
            m_ProgressState.InPrecent = false;
            m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4}");
            m_ProgressState.Description = m_RecordingFileInfo.DownloadUrl;

            string strRtn = string.Empty;

            TransferFile.TransferFileProgress += new TransferFileProgressEventHandler(TransferFile_TransferFileProgress);
            TransferFile.TransferFileStart += new TransferFileStartEventHandler(TransferFile_TransferFileStart);
            strRtn = TransferFile.DownloadFile(m_ContactData, m_RecordingFileInfo);
            TransferFile.TransferFileProgress -= new TransferFileProgressEventHandler(TransferFile_TransferFileProgress);
            TransferFile.TransferFileStart -= new TransferFileStartEventHandler(TransferFile_TransferFileStart);
            
            return strRtn;
        }

        private void TransferFile_TransferFileStart(long size, long startByte)
        {
            int m_size = (int)size;
            if (m_RecordingFileInfo.FileSize > 0)
                m_size = m_RecordingFileInfo.FileSize;

            if (size > 0)
            {
                m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4} of {5}. ({6}%)");
                m_ProgressState.Minimum = 0;
                m_ProgressState.Maximum = (int)size;
                m_ProgressState.CurrentProgress = (int)startByte;
            }
            else
            {
                m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4}");
                m_ProgressState.Maximum =  -1;
                m_ProgressState.CurrentProgress = (int)startByte;
            }
            m_BkgWorker.ReportProgress(-2, m_ProgressState);
            
            Tools.Log(string.Format("TransferFile_TransferFileStart --> Size: {0}", m_size), Tools.LogType.Debug);
        }

        private void TransferFile_TransferFileProgress(long currentBytes)
        {
            m_ProgressState.CurrentProgress = (int)currentBytes;

            m_BkgWorker.ReportProgress(-2, m_ProgressState);
        }
        #endregion

        #region Handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Tools.Log(string.Format("Call Center Download file. Request"), Tools.LogType.Info);

            if(m_ContactData == null)
                DialogResult = false;
            if (m_RecordingFileInfo == null)
                DialogResult = false;

            if(m_ConfigData != null && m_RecordingFileInfo != null)
                GetFile();
        }

        private void loadContactsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

            if (m_Error)
                this.DialogResult = false;
            else
                this.DialogResult = true;
        }

        private void LoadContactsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            BackgroundWorkerProgress progressState = null;
            if (e.UserState.GetType() == typeof(BackgroundWorkerProgress))
                progressState = (BackgroundWorkerProgress)e.UserState;


            if (e.ProgressPercentage == -1) //running progressbar
            {
                pbMain.IsIndeterminate = true;
            }
            else if (e.ProgressPercentage < -1) //Custom progressbar
            {
                if (progressState != null)
                {
                    if (progressState.Maximum == -1)
                    {
                        pbMain.IsIndeterminate = true;
                    }
                    else
                    {
                        pbMain.IsIndeterminate = false;
                        pbMain.Minimum = progressState.Minimum;
                        pbMain.Maximum = progressState.Maximum;

                        pbMain.Value = progressState.CurrentProgress;
                    }
                    lblFound.Content = progressState.GetProgressLabel();
                    lblStatus.Content = progressState.Description;
                }

            }
            else if (e.ProgressPercentage == 0) //Resetting progressbar
            {
                pbMain.IsIndeterminate = false;
                pbMain.Minimum = 0;
                pbMain.Maximum = 0;
                pbMain.Value = 0;
            }
            else if (e.ProgressPercentage > 0) //Progress
            {
                pbMain.Value = e.ProgressPercentage;
            }
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
