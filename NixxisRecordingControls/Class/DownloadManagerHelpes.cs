using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using ContactRoute.Recording.Config;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Recording
{
    public class DownloadManagerList : ObservableCollection<DownloadManagerItem>
    {
    }

    public class DownloadManagerItemOld : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext("DownloadManagerHelpes");
        #region Class data
        private string m_OutputfileName = string.Empty;
        private ContactData m_ContactItem;
        private RecordingFileInformation m_RecordingInformation;
        private FileInfo m_File;
        private DownloadStates m_State = DownloadStates.Waiting;
        private bool m_CheckingNumberOfFiles = true;
        private bool m_Error = false;
        private bool m_ProgressbarIsIndeterminate = true;
        private double m_ProgressbarMinimum = 0;
        private double m_ProgressbarMaximum = 0;
        private double m_ProgressbarValue = 0;
        private string m_ProgressbarDescription = string.Empty;
        private BackgroundWorkerProgress m_ProgressState = null;
        #endregion

        #region Properties
        public ContactData ContactItem
        {
            get { return m_ContactItem; }
            set { m_ContactItem = value; FirePropertyChanged("ContactItem"); }
        }
        public RecordingFileInformation RecordingInformation
        {
            get { return m_RecordingInformation; }
            set { m_RecordingInformation = value; FirePropertyChanged("RecordingInformation"); }
        }
        public string FileName
        {
            get { return m_File.FullName; }
            set { m_File = new FileInfo(value); FirePropertyChanged("File"); FirePropertyChanged("FileName"); }
        }
        public FileInfo File
        {
            get { return m_File; }
            set { m_File = value; FirePropertyChanged("File"); FirePropertyChanged("FileName"); }
        }
        public DownloadStates State
        {
            get { return m_State; }
            set { m_State = value; FirePropertyChanged("State"); }
        }
        public bool CheckingNumberOfFiles
        {
            get { return m_CheckingNumberOfFiles; }
            set { m_CheckingNumberOfFiles = value; FirePropertyChanged("CheckingNumberOfFiles"); }
        }
        public bool Error
        {
            get { return m_Error; }
            set { m_Error = value; FirePropertyChanged("Error"); }
        }
        public BackgroundWorkerProgress ProgressState
        {
            get { return m_ProgressState; }
            set { m_ProgressState = value; FirePropertyChanged("ProgressState"); }
        }
        public bool ProgressbarIsIndeterminate
        {
            get { return m_ProgressbarIsIndeterminate; }
            set { m_ProgressbarIsIndeterminate = value; FirePropertyChanged("ProgressbarIsIndeterminate"); }
        }
        public double ProgressbarMinimum
        {
            get { return m_ProgressbarMinimum; }
            set { m_ProgressbarMinimum = value; FirePropertyChanged("ProgressbarMinimum"); }
        }
        public double ProgressbarMaximum
        {
            get { return m_ProgressbarMaximum; }
            set { m_ProgressbarMaximum = value; FirePropertyChanged("ProgressbarMaximum"); }
        }
        public double ProgressbarValue
        {
            get { return m_ProgressbarValue; }
            set { m_ProgressbarValue = value; FirePropertyChanged("ProgressbarValue"); }
        }
        public string ProgressbarDescription
        {
            get { return m_ProgressbarDescription; }
            set { m_ProgressbarDescription = value; FirePropertyChanged("ProgressbarDescription"); }
        }
        #endregion

        #region Download Part
        #region Class data
        public void StartDownload()
        {
            Tools.Log(string.Format("Download request for file {0}", m_ContactItem.ContactId), Tools.LogType.Info);

            if (m_ContactItem == null)
                return; 
            if (m_RecordingInformation == null)
                return; 
        }
        
        public void GetFile()
        {
            Tools.Log(string.Format("Download file. Getfile {0}", m_RecordingInformation.DownloadUrl), Tools.LogType.Info);

            this.State = DownloadStates.Downloading;

        }

        private string DownLoadFile(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = ((BackgroundWorker)sender);

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            m_ProgressState = new BackgroundWorkerProgress();
            m_ProgressState.InPrecent = false;
            m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4}");
            m_ProgressState.Description = m_RecordingInformation.DownloadUrl;

            string strRtn = string.Empty;

            TransferFile.TransferFileProgress += new TransferFileProgressEventHandler(TransferFile_TransferFileProgress);
            TransferFile.TransferFileStart += new TransferFileStartEventHandler(TransferFile_TransferFileStart);
            strRtn = TransferFile.DownloadFile(m_ContactItem, m_RecordingInformation);
            TransferFile.TransferFileProgress -= new TransferFileProgressEventHandler(TransferFile_TransferFileProgress);
            TransferFile.TransferFileStart -= new TransferFileStartEventHandler(TransferFile_TransferFileStart);

            return strRtn;
        }

        private void TransferFile_TransferFileStart(long size, long startByte)
        {
            int m_size = (int)size;
            if (m_RecordingInformation.FileSize > 0)
                m_size = m_RecordingInformation.FileSize;

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
                m_ProgressState.Maximum = -1;
                m_ProgressState.CurrentProgress = (int)startByte;
            }

            Tools.Log(string.Format("TransferFile_TransferFileStart --> Size: {0}", m_size), Tools.LogType.Debug);
        }

        private void TransferFile_TransferFileProgress(long currentBytes)
        {
            m_ProgressState.CurrentProgress = (int)currentBytes;

        }

        private void loadContactsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //TO DO: check if a next download should start

            if (m_Error)
                this.State = DownloadStates.Failed;
            else
            {
                this.State = DownloadStates.Downloaded;
            }
        }

        private DateTime m_LastProgressUpdate = DateTime.Now;
        private void LoadContactsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (DateTime.Now.Subtract(m_LastProgressUpdate).TotalMilliseconds > 10)
            {
                m_LastProgressUpdate = DateTime.Now;

                BackgroundWorkerProgress progressState = null;
                if (e.UserState.GetType() == typeof(BackgroundWorkerProgress))
                    progressState = (BackgroundWorkerProgress)e.UserState;


                if (e.ProgressPercentage == -1) //running progressbar
                {
                    this.ProgressbarIsIndeterminate = true;
                }
                else if (e.ProgressPercentage < -1) //Custom progressbar
                {
                    if (progressState != null)
                    {
                        if (progressState.Maximum == -1)
                        {
                            this.ProgressbarIsIndeterminate = true;
                        }
                        else
                        {
                            this.ProgressbarIsIndeterminate = false;
                            this.ProgressbarMinimum = progressState.Minimum;
                            this.ProgressbarMaximum = progressState.Maximum;

                            this.ProgressbarValue = progressState.CurrentProgress;
                        }
                        this.ProgressbarDescription = progressState.GetProgressLabel();
                    }

                }
                else if (e.ProgressPercentage == 0) //Resetting progressbar
                {
                    this.ProgressbarIsIndeterminate = false;
                    this.ProgressbarMinimum = 0;
                    this.ProgressbarMaximum = 0;
                    this.ProgressbarValue = 0;
                }
                else if (e.ProgressPercentage > 0) //Progress
                {
                    this.ProgressbarValue = e.ProgressPercentage;
                }
            }
        }
        #endregion
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

    public enum DownloadStates
    {
        Waiting,
        Failed,
        NotFound,
        CheckingRecording,
        Downloaded,
        Downloading,
    }

    //
    //New Stuff
    //Class: Download Item
    //
    public class DownloadManagerItem : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext("DownloadManagerHelpes");
        #region Class data
        private ContactData m_ContactItem;
        private FileInfo m_File;
        private DownloadManagerItemStates m_State = DownloadManagerItemStates.Waiting;
        private bool m_CheckingNumberOfFiles = true;
        private bool m_Error = false;

        private BackgroundWorker m_BkgWorker = null;
        private BackgroundWorkerProgress m_ProgressState = null;
        private bool m_ProgressbarIsIndeterminate = true;
        private double m_ProgressbarMinimum = 0;
        private double m_ProgressbarMaximum = 0;
        private double m_ProgressbarValue = 0;
        private string m_ProgressbarDescription = string.Empty;
        private string m_FileProgressDescription = string.Empty;

        private DateTime m_LastProgressUpdate = DateTime.Now;
        private ProfileData m_Config = null;
        #endregion

        #region Properties
        public DownloadFileList FileList { get; set; }

        public ContactData ContactItem
        {
            get { return m_ContactItem; }
            set { m_ContactItem = value; FirePropertyChanged("ContactItem"); }
        }
        public string FileName
        {
            get { return m_File.FullName; }
            set { m_File = new FileInfo(value); FirePropertyChanged("FileInfo"); FirePropertyChanged("FileName"); }
        }
        public FileInfo FileInfo
        {
            get { return m_File; }
            set { m_File = value; FirePropertyChanged("FileInfo"); FirePropertyChanged("FileName"); }
        }
        public DownloadManagerItemStates State
        {
            get { return m_State; }
            set { m_State = value; FirePropertyChanged("State"); }
        }
        public bool CheckingNumberOfFiles
        {
            get { return m_CheckingNumberOfFiles; }
            set { m_CheckingNumberOfFiles = value; FirePropertyChanged("CheckingNumberOfFiles"); }
        }
        public bool Error
        {
            get { return m_Error; }
            set { m_Error = value; FirePropertyChanged("Error"); }
        }
        public bool ProgressbarIsIndeterminate
        {
            get { return m_ProgressbarIsIndeterminate; }
            set { m_ProgressbarIsIndeterminate = value; FirePropertyChanged("ProgressbarIsIndeterminate"); }
        }
        public double ProgressbarMinimum
        {
            get { return m_ProgressbarMinimum; }
            set { m_ProgressbarMinimum = value; FirePropertyChanged("ProgressbarMinimum"); }
        }
        public double ProgressbarMaximum
        {
            get { return m_ProgressbarMaximum; }
            set { m_ProgressbarMaximum = value; FirePropertyChanged("ProgressbarMaximum"); }
        }
        public double ProgressbarValue
        {
            get { return m_ProgressbarValue; }
            set { m_ProgressbarValue = value; FirePropertyChanged("ProgressbarValue"); }
        }
        public string ProgressbarDescription
        {
            get { return m_ProgressbarDescription; }
            set { m_ProgressbarDescription = value; FirePropertyChanged("ProgressbarDescription"); }
        }
        public string FileProgressDescription
        {
            get { return m_FileProgressDescription; }
            set { m_FileProgressDescription = value; FirePropertyChanged("FileProgressDescription"); }
        }
        #endregion

        #region Checking
        public void CheckDownloadItem(ProfileData configData)
        {
            if (this.State != DownloadManagerItemStates.Waiting)
                return;

            this.State = DownloadManagerItemStates.CheckingItem;

            this.ProgressbarDescription = TranslationContext.Translate("Finding recording(s)...");

            Tools.Log(string.Format("Checking number of files for recording {0}", this.ContactItem.ContactId), Tools.LogType.Info);

            CallCenterWavFileSelector frm = new CallCenterWavFileSelector();
            frm.ConfigData = configData;
            frm.ContactData = this.ContactItem;

            List<RecordingFileInformation> recFileInfo = frm.GetAllFileNames(TransferFile.GetInternalFileName(this.ContactItem));
            frm = null;

            Tools.Log(string.Format("{1} File(s) found for recording {0}", this.ContactItem.ContactId, recFileInfo.Count), Tools.LogType.Info);
            this.FileList = new DownloadFileList();

            if (recFileInfo.Count > 0)
            {
                foreach (RecordingFileInformation item in recFileInfo)
                {
                    DownloadFileItem file = new DownloadFileItem();
                    file.Parent = this;
                    file.RecordingInformation = item;
                    
                    this.FileList.Add(file);

                    if (this.FileList.Count == 1)
                        this.ProgressbarDescription = string.Format(TranslationContext.Translate("Found {0} recoding"), this.FileList.Count);
                    else
                        this.ProgressbarDescription = string.Format(TranslationContext.Translate("Found {0} recodings"), this.FileList.Count);
                }
            }

            if (this.FileList.Count > 0)
                this.State = DownloadManagerItemStates.Checked;
            else
                this.State = DownloadManagerItemStates.NotFound;

        }       
        #endregion
        
        #region Download
        private DownloadFileItem m_CurrentDownloadItem = null;

        public void DownloadFiles(ProfileData configData)
        {
            Tools.Log(string.Format("Attempt to download {0} file(s).", FileList.Count), Tools.LogType.Info);
            if (m_Config == null)
                m_Config = configData;

            this.State = DownloadManagerItemStates.Downloading;

            int cnt = 0;
            bool allDownloaded = true;
            foreach (DownloadFileItem item in FileList)
            {
                cnt++;
                if (item.State == DownloadFileStates.Waiting)
                {
                    allDownloaded = false;
                    FileProgressDescription = string.Format(TranslationContext.Translate("Downloading file {0} of {1}"), cnt, FileList.Count);
                    InitDownloadFile(configData, item);
                    break;
                }
            }

            //If all items have been downloaded check the end state of the downloads.
            if (allDownloaded)
            {
                int failed = 0;
                int notfound = 0;
                int downloaded = 0;

                foreach (DownloadFileItem item in FileList)
                {
                    switch (item.State)
                    {
                        case DownloadFileStates.Failed:
                            failed++;
                            break;
                        case DownloadFileStates.NotFound:
                            notfound++;
                            break;
                        default:
                            downloaded++;
                            break;
                    }
                }

                if (failed == 0 && notfound == 0)
                {
                    this.State = DownloadManagerItemStates.Downloaded;
                }
                else if (failed > 0)
                {
                    this.State = DownloadManagerItemStates.Failed;
                }
                else
                {
                    this.State = DownloadManagerItemStates.NotFound;
                }
            }
        }

        private void InitDownloadFile(ProfileData configData, DownloadFileItem item)
        {
            Tools.Log(string.Format("Download file. Getfile {0}", item.RecordingInformation.DownloadUrl), Tools.LogType.Info);

            item.State = DownloadFileStates.Downloading;

            m_BkgWorker = new BackgroundWorker();
            m_BkgWorker.DoWork += new DoWorkEventHandler((DoWorkEventHandler)((a, b) =>
            {
                Tools.Log(string.Format("BackgroundWorker started."), Tools.LogType.Info);
                //
                //Downloading
                //
                m_CurrentDownloadItem = item;
                item.OutputfileName = DownLoadFile(a, b, configData, item);

                if (string.IsNullOrEmpty(item.OutputfileName))
                {
                    Tools.Log(string.Format("Retry to download file {0}", item.RecordingInformation.DownloadUrl), Tools.LogType.Info);
                    item.OutputfileName = DownLoadFile(a, b, configData, item);
                }

                //
                //Post processing
                //
                this.FileProgressDescription = string.Empty;
                this.ProgressbarDescription = string.Empty;

                if (string.IsNullOrEmpty(item.OutputfileName))
                {
                    item.State = DownloadFileStates.Failed;
                }
                else
                {
                    Tools.Log(string.Format("Copying file ({0})...", item.OutputfileName), Tools.LogType.Info);
                    this.ProgressbarDescription = TranslationContext.Translate("Copying file...");

                    System.IO.FileInfo realFile = new System.IO.FileInfo(this.FileName);
                    string realFileName = realFile.Name;
                    string realFileLocation = realFile.DirectoryName;

                    System.IO.FileInfo outpuFile = new System.IO.FileInfo(item.OutputfileName);

                    //Check extension
                    if (string.IsNullOrEmpty(realFile.Extension))
                    {
                        realFileName = realFileName + outpuFile.Extension;
                    }
                    else if (outpuFile.Extension != realFile.Extension)
                    {
                        realFileName = realFileName.Substring(0, realFileName.Length - realFile.Extension.Length) + outpuFile.Extension;
                    }


                    //Check if file name already exists
                    int count = 0;
                    string fileName = Path.Combine(realFileLocation, realFileName);

                    while (File.Exists(fileName))
                    {
                        count++;
                        fileName = Path.Combine(realFileLocation, realFileName.Substring(0, realFileName.LastIndexOf('.')) + "(" + count + ")" + fileName.Substring(fileName.LastIndexOf('.')));
                    }

                    //Write the downloaded file to the correct location
                    try 
                    { 
                        File.Copy(item.OutputfileName, fileName);
                        Tools.Log(string.Format("file copied ({0})...", fileName), Tools.LogType.Info);
                    }
                    catch (Exception err) { Tools.Log(string.Format("ERR. Copying file: {0}", err.ToString()), Tools.LogType.Error); }

                    

                    //Remove tmp file
                    try { File.Delete(item.OutputfileName); }
                    catch (Exception err) { Tools.Log(string.Format("ERR. Deleting tmp file: {0}", err.ToString()), Tools.LogType.Error); }
                }

                Tools.Log(string.Format("BackgroundWorker Stopped."), Tools.LogType.Info);
            }));
            m_BkgWorker.ProgressChanged += new ProgressChangedEventHandler(DownLoadWorker_ProgressChanged);
            m_BkgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownLoadWorker_RunWorkerCompleted);
            m_BkgWorker.RunWorkerAsync();

        }
        
        private string DownLoadFile(object sender, DoWorkEventArgs e, ProfileData configData, DownloadFileItem item)
        {
            BackgroundWorker worker = ((BackgroundWorker)sender);

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            m_ProgressState = new BackgroundWorkerProgress();
            m_ProgressState.InPrecent = false;
            m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4}");
            m_ProgressState.Description = item.RecordingInformation.DownloadUrl;

            string strRtn = string.Empty;

            TransferFile.TransferFileProgress += new TransferFileProgressEventHandler(TransferFile_TransferFileProgress);
            TransferFile.TransferFileStart += new TransferFileStartEventHandler(TransferFile_TransferFileStart);
            strRtn = TransferFile.DownloadFile(m_ContactItem, item.RecordingInformation);
            TransferFile.TransferFileProgress -= new TransferFileProgressEventHandler(TransferFile_TransferFileProgress);
            TransferFile.TransferFileStart -= new TransferFileStartEventHandler(TransferFile_TransferFileStart);

            return strRtn;
        }
        
        private void TransferFile_TransferFileStart(long size, long startByte)
        {
            int m_size = (int)size;
            if (m_CurrentDownloadItem.RecordingInformation.FileSize > 0)
                m_size = m_CurrentDownloadItem.RecordingInformation.FileSize;

            if (m_size > 0)
            {
                m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4} of {5}. ({6}%)");
                m_ProgressState.Minimum = 0;
                m_ProgressState.Maximum = (int)m_size;
                m_ProgressState.CurrentProgress = (int)startByte;
            }
            else
            {
                m_ProgressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4}");
                m_ProgressState.Maximum = -1;
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

        private void DownLoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_CurrentDownloadItem.Error)
                m_CurrentDownloadItem.State = DownloadFileStates.Failed;
            else
                m_CurrentDownloadItem.State = DownloadFileStates.Downloaded;

            DownloadFiles(m_Config);
        }
        
        private void DownLoadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (DateTime.Now.Subtract(m_LastProgressUpdate).TotalMilliseconds > 1000)
            {
                m_LastProgressUpdate = DateTime.Now;

                BackgroundWorkerProgress progressState = null;
                if (e.UserState.GetType() == typeof(BackgroundWorkerProgress))
                    progressState = (BackgroundWorkerProgress)e.UserState;


                if (e.ProgressPercentage == -1) //running progressbar
                {
                    this.ProgressbarIsIndeterminate = true;
                }
                else if (e.ProgressPercentage < -1) //Custom progressbar
                {
                    if (progressState != null)
                    {
                        if (progressState.Maximum == -1)
                        {
                            this.ProgressbarIsIndeterminate = true;
                        }
                        else
                        {
                            this.ProgressbarIsIndeterminate = false;
                            this.ProgressbarMinimum = progressState.Minimum;
                            this.ProgressbarMaximum = progressState.Maximum;

                            this.ProgressbarValue = progressState.CurrentProgress;
                        }
                        this.ProgressbarDescription = progressState.GetProgressLabel();
                    }

                }
                else if (e.ProgressPercentage == 0) //Resetting progressbar
                {
                    this.ProgressbarIsIndeterminate = false;
                    this.ProgressbarMinimum = 0;
                    this.ProgressbarMaximum = 0;
                    this.ProgressbarValue = 0;
                }
                else if (e.ProgressPercentage > 0) //Progress
                {
                    this.ProgressbarValue = e.ProgressPercentage;
                }
            }
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

    public enum DownloadManagerItemStates
    {
        /// <summary>
        /// Item is added but not yet checked
        /// </summary>
        Waiting,
        /// <summary>
        /// Item is being checked and file or listed to download
        /// </summary>
        CheckingItem,
        /// <summary>
        /// Item is checked and files or found
        /// </summary>
        Checked,
        /// <summary>
        /// Item is checked but no file are found
        /// </summary>
        NotFound,
        /// <summary>
        /// Downloading of files is in progress
        /// </summary>        
        Downloading,
        /// <summary>
        /// Files have been downloaded. No problems found.
        /// </summary>
        Downloaded,
        /// <summary>
        /// Some or all files couldn't be donwload
        /// </summary>
        Failed,
    }

    //
    //Class: download File item
    //
    public class DownloadFileList : ObservableCollection<DownloadFileItem>
    {
    }

    public class DownloadFileItem : INotifyPropertyChanged
    {
        #region Class data
        private string m_OutputfileName = string.Empty;
        private DownloadManagerItem m_Parent = null;
        private RecordingFileInformation m_RecordingInformation;
        private DownloadFileStates m_State = DownloadFileStates.Waiting;
        private bool m_Error = false;
        #endregion

        #region Properties
        public string OutputfileName
        {
            get { return m_OutputfileName; }
            set { m_OutputfileName = value; FirePropertyChanged("OutputfileName"); }
        }
        public DownloadManagerItem Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; FirePropertyChanged("Parent"); }
        }
        public RecordingFileInformation RecordingInformation
        {
            get { return m_RecordingInformation; }
            set { m_RecordingInformation = value; FirePropertyChanged("RecordingInformation"); }
        }
        public DownloadFileStates State
        {
            get { return m_State; }
            set { m_State = value; FirePropertyChanged("State"); }
        }
        public bool Error
        {
            get { return m_Error; }
            set { m_Error = value; FirePropertyChanged("Error"); }
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

    public enum DownloadFileStates
    {
        /// <summary>
        /// Wait to start downloading file
        /// </summary>
        Waiting,
        /// <summary>
        /// File is not found
        /// </summary>
        NotFound,
        /// <summary>
        /// Downloading of file is in progress
        /// </summary>        
        Downloading,
        /// <summary>
        /// File is downloaded
        /// </summary>
        Downloaded,
        /// <summary>
        /// File couldn't be donwloaded
        /// </summary>
        Failed,
    }
}
