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
using ContactRoute.Recording.Config;
using System.Net;
using System.IO;

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for CallCenterWavFileSelector.xaml
    /// </summary>
    public partial class CallCenterWavFileSelector : Window
    {        
        #region Class Data
        private ProfileData m_ConfigData = null;
        private ContactData m_ContactData = null;
        private RecordingFileInformation m_SelectedFileInformation = null;
        private List<RecordingFileInformation> m_RecordingFiles = null;

        private string m_FileId;
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
        public RecordingFileInformation SelectedFileInformation
        {
            get { return m_SelectedFileInformation; }
        }
        public List<RecordingFileInformation> RecordingFiles
        {
            get { return m_RecordingFiles; }
        }
        #endregion

        #region Constructors
        public CallCenterWavFileSelector()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        #endregion

        #region Members
        public void GetFileName(string fileId)
        {
            m_RecordingFiles = new List<RecordingFileInformation>();

            m_FileId = fileId;
            m_RecordingFiles = RecordingTools.GetNumberOfFiles(m_FileId, m_ContactData.MediaType, m_ConfigData);

            if (m_RecordingFiles.Count == 0) m_RecordingFiles = RecordingTools.GetNumberOfFiles(m_FileId, m_ContactData.MediaType, m_ConfigData);

            if (m_RecordingFiles.Count <= 0)
            {
                m_SelectedFileInformation = null;
            }
            else if (m_RecordingFiles.Count == 1)
            {
                m_SelectedFileInformation = m_RecordingFiles[0];
            }
            else
            {
                this.ShowDialog();
            }
        }

        public List<RecordingFileInformation> GetAllFileNames(string fileId)
        {

            m_RecordingFiles = new List<RecordingFileInformation>();

            m_FileId = fileId;
            m_RecordingFiles = RecordingTools.GetFileList(m_FileId, m_ContactData.MediaType, m_ConfigData);

            return m_RecordingFiles;

        }


        private void DisplayInfo()
        {
            try
            {
                mainGrid.DataContext = m_ContactData;
                lbFiles.ItemsSource = m_RecordingFiles;
            }
            catch (Exception err)
            {
                Tools.Log("Error in WavSelection:" + err.ToString(), Tools.LogType.Error);
            }
        }
        #endregion

        #region Handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayInfo();
        }

        private void btnPlayFile_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles.SelectedItem == null)
            {
                MessageBox.Show("Please select a file to play!");
                return;
            }

            m_SelectedFileInformation = (RecordingFileInformation)lbFiles.SelectedItem;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        #endregion


    }
}
