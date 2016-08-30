using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContactRoute.Recording.Config;

namespace Nixxis.Client.Recording
{
    public class RecordingFileInformation
    {
        public int FileNumber { get; set; }
        public string Filename { get; set; }
        public int FileSize { get; set; }
        public string TempFilename { get; set; }
        public string Extension { get; set; }
        public TransferProfileData DownloadServer { get; set; }
        public string DownloadUrl { get; set; }
        public string StreamingUrl
        {
            get
            {
                string rtn = DownloadUrl;

                if (!string.IsNullOrEmpty(DownloadServer.User))
                {
                    if (rtn.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        rtn = rtn.Substring("http://".Length, rtn.Length - "http://".Length);
                        rtn = "http://" + DownloadServer.User.Trim() + ":" + DownloadServer.Password.Trim() + "@" + rtn;
                    }
                }
                return rtn;
            }
        }
        public string DispalyFileName
        {
            get
            {
                return string.Format("File{0}{1}", FileNumber > 0 ? "." + FileNumber.ToString() : "", Extension.StartsWith(".") ? Extension : "." + Extension);
            }
        }

        public RecordingFileInformation()
        {
            Filename = string.Empty;
            DownloadServer = new TransferProfileData();
            DownloadUrl = string.Empty;
            Extension = string.Empty;
            FileSize = -1;
        }
        public RecordingFileInformation(string filenname, string extension, TransferProfileData server, string url)
        {
            Filename = filenname;
            Extension = extension;
            DownloadServer = server;
            DownloadUrl = url;
            FileSize = -1;
        }
    }

    public class FtpFileInfo
    {
        #region Class data
        private DateTime m_StartDateTime = new DateTime();
        private string m_Originator = "";
        private string m_Destination = "";
        private string m_ScopServData = "";
        #endregion

        #region Constructors
        public FtpFileInfo()
        {
        }
        #endregion

        #region Properties
        public DateTime StartDateTime
        {
            get { return m_StartDateTime; }
            set { m_StartDateTime = value; }
        }
        public string Originator
        {
            get { return m_Originator; }
            set { m_Originator = value; }
        }
        public string Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }
        public string ScopServData
        {
            get { return m_ScopServData; }
            set { m_ScopServData = value; }
        }
        #endregion

        #region Members
        public void Clear()
        {
            m_Destination = "";
            m_Originator = "";
            m_ScopServData = "";
            m_StartDateTime = DateTime.MinValue;
        }
        public void Prase(string fileName)
        {
            this.Clear();
            string[] list = fileName.Split(new char[] { '_' });
            if (list.Length > 1)
            {
                try
                {
                    m_StartDateTime = new DateTime(int.Parse(list[0].Substring(0, 4)),
                        int.Parse(list[0].Substring(4, 2)),
                        int.Parse(list[0].Substring(6, 2)),
                        int.Parse(list[0].Substring(8, 2)),
                        int.Parse(list[0].Substring(10, 2)),
                        int.Parse(list[0].Substring(12, 2)));
                }
                catch
                { m_StartDateTime = DateTime.MinValue; }

                m_Originator = list[1];
                m_Destination = list[2];
                if (list.Length > 2) m_ScopServData = list[3];
                else m_ScopServData = "";
            }
        }
        #endregion
    }
}
