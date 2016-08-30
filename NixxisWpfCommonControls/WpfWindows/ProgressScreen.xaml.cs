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
using System.ComponentModel;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for ProgressScreen.xaml
    /// </summary>
    public partial class ProgressScreen : Window
    {
        #region Class Data
        private BackgroundWorker m_BkgWorker = null;
        private bool m_TaskFailed = false;
        private bool m_SetDefaultLabelFound = false;
        #endregion

        #region Constructors
        public ProgressScreen()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        #endregion

        #region Properties
        public bool TaskFailed
        {
            get { return m_TaskFailed; }
            set { m_TaskFailed = value; }
        }
        public string LabelHeader
        {
            get { return lblHeading.Content as string; }
            set { lblHeading.Content = value; }
        }
        public string LabelFound
        {
            get { return lblFound.Content as string; }
            set { lblFound.Content = value; }
        }
        public string LabelStatus
        {
            get { return lblStatus.Content as string; }
            set { lblStatus.Content = value; }
        }
        #endregion

        #region Members
        public void StartTask(DoWorkEventHandler taskToRun)
        {
            m_BkgWorker = new BackgroundWorker();

            m_BkgWorker.DoWork += taskToRun;
            m_BkgWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            m_BkgWorker.ProgressChanged += Worker_ProgressChanged;
            m_BkgWorker.RunWorkerAsync();
        }

        #endregion

        #region Handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_TaskFailed)
                this.DialogResult = false;
            else
                this.DialogResult = true;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorkerProgress progressState = null;

            if (e.UserState != null && e.UserState.GetType() == typeof(BackgroundWorkerProgress))
                progressState = (BackgroundWorkerProgress)e.UserState;


            if (progressState == null)
            {
                pbMain.IsIndeterminate = false;
                pbMain.Minimum = 0;
                pbMain.Maximum = 100;
                pbMain.Value = e.ProgressPercentage;

                if (string.IsNullOrEmpty(lblFound.Content as string))
                    m_SetDefaultLabelFound = true;

                if(m_SetDefaultLabelFound)
                    lblFound.Content = pbMain.Value + " %";

                    
            }
            else
            {
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
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
