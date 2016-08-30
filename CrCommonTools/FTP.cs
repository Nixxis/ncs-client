using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using Common.Tools.IO;
using System.Text.RegularExpressions;

namespace Common.Tools.Ftp
{
    public class FtpConnection
    {
        #region Class data
        //{0} -> Ip address
        private string m_Root = "ftp://{0}";
        //{0} -> Ip address
        //{1} -> File name
        private string m_FileRoot = "ftp://{0}/{1}";
        private string m_IpAddress;
        private string m_Password;
        private string m_UserName;
        private bool m_ActiveFtpConnection = false;

        private FtpWebRequest m_Request = null;
        //private DirectoryInformation m_RootDirectory = new DirectoryInformation();
        private FolderInformation m_RootFolder = new FolderInformation();
        private Exception m_LastException = null;
        #endregion

        #region Constructor
        public FtpConnection()
        {

        }
        public FtpConnection(string ipAddress, string userName, string password)
        {
            m_IpAddress = ipAddress;
            m_UserName = userName;
            m_Password = password;
        }
        #endregion

        #region Properties
        public string Root
        {
            get { return m_FileRoot; }
            set { m_FileRoot = value; }
        }
        public string IpAddress
        {
            get { return m_IpAddress; }
            set { m_IpAddress = value; }
        }
        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }
        public string UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }
        public bool ActiveFtpConnection
        {
            get { return m_ActiveFtpConnection; }
            set { m_ActiveFtpConnection = value; }
        }
        public Exception LastException
        {
            get { return m_LastException; }
        }
        public FolderInformation RootFolder
        {
            get { return m_RootFolder; }
            set { m_RootFolder = value; }
        }
        #endregion

        #region Members
        private void Log(string msg)
        {
            Trace.WriteLine("FTP >" + msg);
        }
        public bool GetRootFolder()
        {
            m_RootFolder.Name = m_IpAddress;
            BuildDirectoryList(GetFolderRawInfo(string.Format(m_Root, m_IpAddress)), m_RootFolder);
            return true;
        }
        public FolderInformation GetFolder(string location)
        {
            string[] list = location.Split(new char[] { '\\' });
            FolderInformation rootFolder = m_RootFolder;
            if (list.Length > 1)
            {
                string[] chkList = new string[list.Length - 1];
                //list.CopyTo(chkList, 2);
                Array.Copy(list, 1, chkList, 0, list.Length - 1);
                foreach (string item in chkList)
                {
                    foreach (FolderInformation folder in rootFolder.Folders.Values)
                    {
                        if (folder.Name.Trim().ToUpper() == item.Trim().ToUpper())
                        {
                            rootFolder = folder;
                            break;
                        }
                    }
                }
            }
            string result = GetFolderRawInfo(string.Format(m_Root, location.Replace("\\", "/")));
            if (result == null)
            {
                result = GetFolderRawInfo(string.Format(m_Root, location.Replace("\\", "/")));
            }
            if (result != null)
                BuildDirectoryList(result, rootFolder);


            return rootFolder;
        }
        public string GetFolderRawInfo(string location)
        {
            Log("Start Raw info." + location);
            FtpWebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                string listDetail = "";

                //
                //Location
                //
                if (m_Request != null)
                {
                    try { m_Request.Abort(); }
                    catch { }
                    m_Request = null;
                }
                Uri ftpUri = new Uri(location);
                m_Request = FtpWebRequest.Create(ftpUri) as FtpWebRequest;
                m_Request.Credentials = new NetworkCredential(m_UserName, m_Password);
                m_Request.UsePassive = !m_ActiveFtpConnection;
                //
                //Detail list of files
                //
                //m_Request.Method = '';
                m_Request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                response = m_Request.GetResponse() as FtpWebResponse;
                stream = response.GetResponseStream();
                reader = new StreamReader(stream, System.Text.Encoding.UTF8);

                listDetail = reader.ReadToEnd();
                try { reader.Close(); }
                catch { }
                try { stream.Close(); }
                catch { }
                try { response.Close(); }
                catch { }

                Log("End Raw info. Result: " + listDetail);
                return listDetail;
            }
            catch (Exception error)
            {
                m_LastException = error;
                try { reader.Close(); }
                catch { }
                try { stream.Close(); }
                catch { }
                try { response.Close(); }
                catch { }
                Log("End with error Raw info. Error: " + error.ToString());
                return null;
            }
        }
        public void BuildDirectoryList(string detailList, FolderInformation root)
        {
            string[] _DetailList = detailList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            root.SetFtpCheck();
            root.Files.Clear();
            foreach (string item in _DetailList)
            {
                string[] details = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string _FileName = string.Join(" ", details, 8, details.GetUpperBound(0) - 7);
                if (details[0].Substring(0, 1).ToLower() == "d")
                {
                    FolderInformation folderInfo;
                    if (root.Folders.TryGetValue(_FileName, out folderInfo))
                    {
                        folderInfo.FtpCheck = false;
                    }
                    else
                    {
                        folderInfo = new FolderInformation();
                        root.Folders.Add(_FileName, folderInfo);
                    }

                    folderInfo.Name = _FileName;
                    folderInfo.Parent = root;
                }
                else
                {
                    FileInformation fileInfo;
                    if (root.Files.TryGetValue(_FileName, out fileInfo))
                    {
                        fileInfo.FtpCheck = false;
                    }
                    else
                    {
                        fileInfo = new FileInformation();
                        root.Files.Add(_FileName, fileInfo);
                    }
                    fileInfo.Name = _FileName;
                    fileInfo.Size = Int64.Parse(details[4]);
                }
            }
            m_RootFolder.FtpClearCheck();
        }
        #endregion
    }

    public class Ftpconnect
    {
        #region Class data
        //{0} -> Ip address
        private string m_Root = "ftp://{0}";
        private string m_FolderName = "";
        private string m_IpAddress;
        private string m_Password;
        private string m_UserName;
        private bool m_ActiveFtpConnection = false;
        private bool m_Connect = false;
        private FtpWebRequest m_Request = null;
        private FolderInformation m_FolderContent = new FolderInformation();
        private Exception m_LastException = null;
        #endregion

        #region Constructor
        public Ftpconnect()
        {

        }
        #endregion

        #region Properties
        public string IpAddress
        {
            get { return m_IpAddress; }
            set { m_IpAddress = value; }
        }
        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }
        public string UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }
        public bool ActiveFtpConnection
        {
            get { return m_ActiveFtpConnection; }
            set { m_ActiveFtpConnection = value; }
        }
        public Exception LastException
        {
            get { return m_LastException; }
        }
        public FolderInformation FolderContent
        {
            get { return m_FolderContent; }
            set { m_FolderContent = value; }
        }
        #endregion

        #region Members
        private void Log(string msg)
        {
            Trace.WriteLine("FTP >" + msg);
        }

        public Common.Tools.IO.FolderInformation GetFolderContent(string folder)
        {
            if (!m_Connect) return new FolderInformation();
            //Folder name should be something like '/Folder' or '' for root folder
            m_FolderContent = new FolderInformation();
            m_FolderContent.Name = m_IpAddress;
            BuildDirectoryList(GetFolderRawInfo(string.Format(m_Root, m_IpAddress)) + folder, m_FolderContent);
            return m_FolderContent;
        }
        public string GetFolderRawInfo(string location)
        {
            if (!m_Connect) return "";
            Log("Start Raw info." + location);
            FtpWebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                string listDetail = "";

                //
                //Location
                //
                if (m_Request != null)
                {
                    try { m_Request.Abort(); }
                    catch { }
                    m_Request = null;
                }
                Uri ftpUri = new Uri(location);
                m_Request = FtpWebRequest.Create(ftpUri) as FtpWebRequest;
                m_Request.Credentials = new NetworkCredential(m_UserName, m_Password);
                m_Request.UsePassive = !m_ActiveFtpConnection;
                //
                //Detail list of files
                //
                m_Request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                response = m_Request.GetResponse() as FtpWebResponse;
                stream = response.GetResponseStream();
                reader = new StreamReader(stream, System.Text.Encoding.UTF8);

                listDetail = reader.ReadToEnd();
                try { reader.Close(); }
                catch { }
                try { stream.Close(); }
                catch { }
                try { response.Close(); }
                catch { }
                Log("End Raw info. Result: " + listDetail);
                return listDetail;
            }
            catch (Exception error)
            {
                m_LastException = error;
                try { reader.Close(); }
                catch { }
                try { stream.Close(); }
                catch { }
                try { response.Close(); }
                catch { }
                Log("End with error Raw info. Error: " + error.ToString());
                return null;
            }
        }
        private  void BuildDirectoryList(string detailList, FolderInformation root)
        {
            string[] _DetailList = detailList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            root.SetFtpCheck();
            root.Files.Clear();
            foreach (string item in _DetailList)
            {
                string[] details = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string _FileName = string.Join(" ", details, 8, details.GetUpperBound(0) - 7);
                if (details[0].Substring(0, 1).ToLower() == "d")
                {
                    FolderInformation folderInfo;
                    if (root.Folders.TryGetValue(_FileName, out folderInfo))
                    {
                        folderInfo.FtpCheck = false;
                    }
                    else
                    {
                        folderInfo = new FolderInformation();
                        root.Folders.Add(_FileName, folderInfo);
                    }

                    folderInfo.Name = _FileName;
                    folderInfo.Parent = root;
                }
                else
                {
                    FileInformation fileInfo;
                    if (root.Files.TryGetValue(_FileName, out fileInfo))
                    {
                        fileInfo.FtpCheck = false;
                    }
                    else
                    {
                        fileInfo = new FileInformation();
                        root.Files.Add(_FileName, fileInfo);
                    }
                    fileInfo.Name = _FileName;
                    fileInfo.Size = Int64.Parse(details[4]);
                }
            }
            m_FolderContent.FtpClearCheck();
        }

        public bool Connect(string ipAddress, string userName, string password)
        {
            m_IpAddress = ipAddress;
            m_UserName = userName;
            m_Password = password;
            m_Connect = true;
            return m_Connect;
        }
        #endregion
    }
}
