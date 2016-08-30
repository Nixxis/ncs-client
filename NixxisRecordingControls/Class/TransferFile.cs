using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContactRoute.Recording.Config;
using System.IO;
using System.Net;

namespace Nixxis.Client.Recording
{
    public delegate void TransferFileProgressEventHandler(long currenBytes);
    public delegate void TransferFileStartEventHandler(long size, long startByte);

    public static class TransferFile
    {
        #region Events
        public static event TransferFileStartEventHandler TransferFileStart;
        public static event TransferFileProgressEventHandler TransferFileProgress;

        public static void OnTransferFileStart(long size, long startByte)
        {
            try
            {
                if (TransferFileStart != null)
                    TransferFileStart(size, startByte);
            }
            catch (Exception err)
            {
                Tools.Log(string.Format("OnTransferFileStart event ERR: {0}", err.ToString()), Tools.LogType.Warning);
            }
        }
        public static void OnTransferFileProgress(long currenBytes)
        {
            try
            {
                if (TransferFileProgress != null)
                    TransferFileProgress(currenBytes);
            }
            catch (Exception err)
            {
                Tools.Log(string.Format("OnTransferFileProgress event ERR: {0}", err.ToString()), Tools.LogType.Warning);
            }
        }
        #endregion

        public static string GetTransferAppServerUri(string root, string server, string filename, string ext, string serviceroot)
        {
            server = GetTransferServer(server, serviceroot, TransferProfileData.Types.AppServer);

            return string.Format(root, server, filename, ext);
            
        }
        public static string GetTransferHttpUri(string root, string server, string filename, string ext)
        {
            server = GetTransferServer(server, root, TransferProfileData.Types.Http);

            string rtnValue = string.Format(root, server, filename, ext);

            if (!rtnValue.ToLower().StartsWith("http://"))
                rtnValue = "http://" + rtnValue;

            return rtnValue;
        }
        public static string GetTransferStrictHttpUri(string root, string server, string filename, string ext)
        {
            server = GetTransferServer(server, root, TransferProfileData.Types.StrictHttp);

            string rtnValue = string.Format(root, server, filename, ext);

            if (!rtnValue.ToLower().StartsWith("http://") && !rtnValue.ToLower().StartsWith("https://"))
                rtnValue = "http://" + rtnValue;

            return rtnValue;
        }
        public static string GetTransferFtpUri(string root, string server, string filename, string ext)
        {
            server = GetTransferServer(server, root, TransferProfileData.Types.Ftp);

            string rtnValue = string.Format(root, server, filename, ext);

            if (!rtnValue.ToLower().StartsWith("ftp://"))
                rtnValue = "ftp://" + rtnValue;

            return rtnValue;
        }

        public static string GetTransferServer(string server, string serviceroot, TransferProfileData.Types type)
        {
            if (type == TransferProfileData.Types.AppServer)
            {
                if (server.Length >= "http://".Length)
                {
                    if (server.ToLower().Substring(0, "http://".Length) != "http://")
                    {
                        if (serviceroot.LastIndexOf('/') == serviceroot.Length - 1)
                            server = serviceroot + server;
                        else
                            server = serviceroot + '/' + server;
                    }
                }
                else
                {
                    if (serviceroot.LastIndexOf('/') == serviceroot.Length)
                        server = serviceroot + server;
                    else
                        server = serviceroot + '/' + server;
                }
                return server;
            }

            else if (type == TransferProfileData.Types.Http)
            {
                return server;
            }
            else if (type == TransferProfileData.Types.StrictHttp)
            {
                return server;
            }
            else
            {
                return server;
            }
        }

        public static string GetInternalFileName(ContactData contactData)
        {
            string fileName = "{0}";

            if (contactData == null) return "";

            string _RecordingId = contactData.RecordingId; 

            return string.Format(fileName, _RecordingId);
        }

        public static string GetExportFileName(string fileFormat, ContactData contact)
        {
            if (contact == null) return "";
            if (string.IsNullOrEmpty(fileFormat)) fileFormat = "{0} {1} - {3} {4}";

            string[] formatList = new string[19];

            try { formatList[0] = contact.LocalDateTime.ToString(); }
            catch { }
            try { formatList[1] = contact.Originator; }
            catch { }
            try { formatList[2] = contact.Destination; }
            catch { }
            try { formatList[3] = contact.FirstName; }
            catch { }
            try { formatList[4] = contact.LastName; }
            catch { }
            try { formatList[5] = contact.Account; }
            catch { }
            try { formatList[6] = contact.Extension; }
            catch { }
            try { formatList[7] = contact.CampDescription; }
            catch { }
            try { formatList[8] = contact.Description; }
            catch { }
            try { formatList[9] = contact.MediaTypeDescription; }
            catch { }
            try { formatList[10] = contact.EndReason; }
            catch { }
            try { formatList[11] = contact.QualificationDescription; }
            catch { }
            try { formatList[12] = contact.Positive.ToString(); }
            catch { }
            try { formatList[13] = contact.Argued.ToString(); }
            catch { }
            try { formatList[14] = contact.ComDuration.ToString(); }
            catch { }
            try { formatList[15] = contact.SetupDuration.ToString(); }
            catch { }
            try { formatList[16] = contact.Duration.ToString(); }
            catch { }
            try { formatList[17] = contact.RecordingId; }
            catch { }
            try { formatList[18] = contact.ContactListId; }
            catch { }

            return string.Format(fileFormat, formatList).Replace("\\", "").Replace("/", "").Replace(":", "");
        }

        public static string DownloadFile(ContactData contactData, RecordingFileInformation recFileInfo)
        {
            string strRtn = string.Empty;

            string outFile = Path.GetTempFileName();
            string extDot = recFileInfo.Extension.StartsWith(".") ? recFileInfo.Extension : "." + recFileInfo.Extension;
            outFile = outFile.Replace(".tmp", extDot);
            if ((outFile.LastIndexOf(extDot) + extDot.Length) != outFile.Length) outFile = outFile + extDot;

            switch (recFileInfo.DownloadServer.Type)
            {
                case TransferProfileData.Types.Ftp:
                    strRtn = TransferFile.DownloadFileFromFtp(recFileInfo.DownloadUrl, outFile, recFileInfo.DownloadServer.User, recFileInfo.DownloadServer.Password, recFileInfo.DownloadServer.UseActiveFtp, recFileInfo.DownloadServer.RequestTimeOut * 1000);
                    break;
                case TransferProfileData.Types.AppServer:
                    strRtn = TransferFile.DownloadFileAppServer(recFileInfo.DownloadUrl, outFile, recFileInfo.DownloadServer.User, recFileInfo.DownloadServer.Password, recFileInfo.DownloadServer.RequestTimeOut * 1000);
                    break;
                case TransferProfileData.Types.StrictHttp:
                    strRtn = TransferFile.DownloadFileFromHttp(recFileInfo.DownloadUrl, outFile, recFileInfo.DownloadServer.User, recFileInfo.DownloadServer.Password, recFileInfo.DownloadServer.RequestTimeOut * 1000);
                    break;
                default:
                    strRtn = TransferFile.DownloadFileFromHttp(recFileInfo.DownloadUrl, outFile, recFileInfo.DownloadServer.User, recFileInfo.DownloadServer.Password, recFileInfo.DownloadServer.RequestTimeOut * 1000);
                    break;
            }
            return strRtn;
        }

        public static string DownloadFileFromServerList(string filename, bool checkExt, ProfileData profileData, TransferProfileData.Medias media)
        {
            TransferProfileData[] serverList = null;

            switch (media)
            {
                case TransferProfileData.Medias.Voice:
                    serverList = profileData.VoiceServerList;
                    break;
                case TransferProfileData.Medias.Chat:
                    serverList = profileData.ChatServerList;
                    break;
                case TransferProfileData.Medias.Office:
                    serverList = profileData.OfficeServerList;
                    break;
            }
            //Downlooad the file
            string rtnValue = null;
            if (media == TransferProfileData.Medias.Office)
            {
                foreach (TransferProfileData server in serverList)
                {
                    rtnValue = DownloadFileFromPbxDirect(filename, checkExt, server);

                    if (!string.IsNullOrEmpty(rtnValue)) break;
                }
            }
            else
            {
                foreach (TransferProfileData server in serverList)
                {
                    rtnValue = DownloadFileCc(filename, checkExt, server, profileData.ServiceRelayRoot);

                    if (!string.IsNullOrEmpty(rtnValue)) break;
                }
            }
            return rtnValue;

        }
        private static string DownloadFileCc(string filename, bool checkExt, TransferProfileData server, string appServiceRoot)
        {
            //Setting extension list
            string[] extlist = new string[] { "wav" };
            if (checkExt)
            {
                if (server.Extension.Length > 0)
                    extlist = server.Extension;
            }
            else
            {
                int loc = filename.LastIndexOf('.');
                if (loc >= 0)
                {
                    extlist = new string[] { filename.Substring(loc + 1) };
                    filename = filename.Substring(0, loc);
                }

            }
            //Downloading file
            foreach (string fileExt in extlist)
            {
                string inFile; 
                if (server.Type == TransferProfileData.Types.Ftp)
                    inFile = TransferFile.GetTransferFtpUri(server.Root, server.Host, filename, fileExt);
                else if (server.Type == TransferProfileData.Types.AppServer)
                    inFile = TransferFile.GetTransferAppServerUri(server.Root, server.Host, filename, fileExt, appServiceRoot);
                else if (server.Type == TransferProfileData.Types.StrictHttp)
                    inFile = TransferFile.GetTransferStrictHttpUri(server.Root, server.Host, filename, fileExt);
                else
                    inFile = TransferFile.GetTransferHttpUri(server.Root, server.Host, filename, fileExt);

                string outFile = Path.GetTempFileName();
                string extDot = "." + fileExt;
                outFile = outFile.Replace(".tmp", extDot);
                if ((outFile.LastIndexOf(extDot) + extDot.Length) != outFile.Length) outFile = outFile + extDot;

                string file = null;
                if (server.Type == TransferProfileData.Types.Ftp)
                    file = TransferFile.DownloadFileFromFtp(inFile, outFile, server.User, server.Password, server.UseActiveFtp, server.RequestTimeOut * 1000);
                else if (server.Type == TransferProfileData.Types.AppServer)
                    file = TransferFile.DownloadFileAppServer(inFile, outFile, server.User, server.Password, server.RequestTimeOut * 1000);
                else
                    file = TransferFile.DownloadFileFromHttp(inFile, outFile, server.User, server.Password, server.RequestTimeOut * 1000);

                if (!string.IsNullOrEmpty(file))
                    return file;
            }
            return string.Empty;

        }

        private static string DownloadFileFromPbxDirect(string filename, bool checkExt, TransferProfileData server)
        {
            FtpFileInfo fileInfo = new FtpFileInfo();
            fileInfo.Prase(filename);

            string inFile = string.Format(server.Root, server.Host, fileInfo.StartDateTime.ToString("yyyyMMdd") + "/" + filename);
            string outFile = Path.GetTempFileName();

            string extDot = ".wav";
            outFile = outFile.Replace(".tmp", extDot);
            if ((outFile.LastIndexOf(extDot) + extDot.Length) != outFile.Length) outFile = outFile + extDot;

            string file = null;
            if (server.Type == TransferProfileData.Types.Ftp)
                file = TransferFile.DownloadFileFromFtp(inFile, outFile, server.User, server.Password, server.UseActiveFtp, server.RequestTimeOut * 1000);
            else
                file = TransferFile.DownloadFileFromHttp(inFile, outFile, server.User, server.Password, server.RequestTimeOut * 1000);

            if (!string.IsNullOrEmpty(file))
                return file;

            return string.Empty;
        }

        public static string DownloadFileFromFtp(string inFile, string outFile, string userName, string password, bool active, int timeout)
        {
            FtpWebResponse response = null;
            Stream str = null;
            BinaryReader sr = null;

            Tools.Log(string.Format("FTP >Request to download {0} file into file {1}.", inFile, outFile), Tools.LogType.Info);

            try
            {
                string[] list = inFile.Split('@');
                string iName = userName;
                string iPass = password;
                string iFile = inFile;

                if (list.Length > 1)
                {
                    string[] lst = list[0].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (lst.Length > 2)
                    {
                        iName = lst[1];
                        iPass = lst[2];
                        iFile = string.Concat(lst[0], "://", list[1]);
                        Tools.Log("In string credentials found!!", Tools.LogType.Info);
                    }
                }
                System.Net.WebRequest request = System.Net.FtpWebRequest.Create(iFile);
                ((FtpWebRequest)request).UsePassive = !active;
                request.Credentials = new NetworkCredential(iName, iPass);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                ((FtpWebRequest)request).UseBinary = true;
                request.Timeout = timeout;
                response = request.GetResponse() as FtpWebResponse;
                str = response.GetResponseStream();

                sr = new BinaryReader(str);
                FileStream fs = File.Create(outFile, 4096, FileOptions.SequentialScan);

                byte[] buffer = new byte[1024];
                int count;
                long totalCount = 0;
                OnTransferFileStart(response.ContentLength, 0);
                while (true)
                {
                    count = sr.Read(buffer, 0, 1024);
                    totalCount += count;
                    OnTransferFileProgress(totalCount);
                    if (count > 0)
                    {
                        fs.Write(buffer, 0, count);
                    }
                    if (count == 0)
                        break;
                }
                System.Threading.Thread.Sleep(1);
                OnTransferFileProgress(totalCount);
                System.Threading.Thread.Sleep(1);
                Tools.Log("FTP > Download end size: " + totalCount, Tools.LogType.Info);
                fs.Close();
                return outFile;
            }
            catch (Exception error)
            {
                Tools.Log("FTP >Error while downloading file " + inFile + " to " + outFile + ". " + error.ToString(), Tools.LogType.Info);
                return "";
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (str != null)
                    str.Close();
                if (response != null)
                    response.Close();
            }
        }
        public static string DownloadFileAppServer(string inFile, string outFile, string userName, string password, int timeout)
        {
            Tools.Log(string.Format("(REQ:AppSerrver) Request to download {0} file into file {1}.", inFile, outFile), Tools.LogType.Info);

            return DownloadFileFromHttp(inFile + AppServerRequest.RelayAction.DownloadFile, outFile, userName, password, timeout);
        }
        public static string DownloadFileFromHttp(string inFile, string outFile, string userName, string password, int timeout)
        {
            HttpWebResponse response = null;
            Stream str = null;
            BinaryReader sr = null;

            Tools.Log(string.Format("(REQ:HTTP) Request to download {0} file into file {1}.", inFile, outFile), Tools.LogType.Info);

            try
            {

                WebRequest request = HttpWebRequest.Create(inFile);
                request.Credentials = new NetworkCredential(userName, password);
                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = timeout;
                response = request.GetResponse() as HttpWebResponse;
                str = response.GetResponseStream();

                sr = new BinaryReader(str);
                FileStream fs = File.Create(outFile, 4096, FileOptions.SequentialScan);

                byte[] buffer = new byte[1024];
                int count;
                long totalCount = 0;
                OnTransferFileStart(response.ContentLength, 0);
                while (true)
                {
                    count = sr.Read(buffer, 0, 1024);
                    totalCount += count;
                    OnTransferFileProgress(totalCount);
                    if (count > 0)
                    {
                        fs.Write(buffer, 0, count);
                    }
                    if (count == 0)
                        break;
                }
                System.Threading.Thread.Sleep(1);
                OnTransferFileProgress(totalCount); 
                System.Threading.Thread.Sleep(1);
                Tools.Log("(REQ:HTTP) Download end size: " + totalCount, Tools.LogType.Info);

                fs.Close();
                return outFile;
            }
            catch (Exception error)
            {
                Tools.Log("(REQ:HTTP) Can't download file " + inFile + " to " + outFile, Tools.LogType.Error);
                Tools.Log("(REQ:HTTP) Error: " + error.ToString(), Tools.LogType.Debug);
                return "";
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (str != null)
                    str.Close();
                if (response != null)
                    response.Close();
            }
        }


        #region Tools

        #endregion
    }

    public static class RecordingTools
    {
        public static List<RecordingFileInformation> GetFileList(string fileId, MediaTypes media, ProfileData configData)
        {
            List<RecordingFileInformation> m_RecordingFiles = new List<RecordingFileInformation>();

            m_RecordingFiles = RecordingTools.GetNumberOfFiles(fileId, media, configData);

            if (m_RecordingFiles.Count == 0) m_RecordingFiles = RecordingTools.GetNumberOfFiles(fileId, media, configData);

            return m_RecordingFiles;
        }

        public static List<RecordingFileInformation> GetNumberOfFiles(string fileId, MediaTypes media, ProfileData configData)
        {
            List<RecordingFileInformation> fileNames = new List<RecordingFileInformation>();

            //
            //get server list
            //
            TransferProfileData[] serverList = null;
            switch (media)
            {
                case MediaTypes.Voice:
                    serverList = configData.VoiceServerList;
                    break;
                case MediaTypes.Chat:
                    serverList = configData.ChatServerList;
                    break;
            }

            if (serverList == null) return fileNames;

            //
            //Get files HTTP and FTP
            //
            int currentFileNumber = 0;
            bool nextFileNumber = true;

            while (nextFileNumber)
            {
                string fileName = fileId;
                if (currentFileNumber > 0)
                    fileName = fileName + "." + currentFileNumber.ToString();

                bool fileFound = false;
                foreach (TransferProfileData server in serverList)
                {
                    RecordingFileInformation recordingFile = null;

                    if (server.Type == TransferProfileData.Types.AppServer)
                    {
                        recordingFile = GetNumberOfFilesCcAppServer(fileName, server, configData);
                    }
                    else if (server.Type == TransferProfileData.Types.Ftp)
                    {
                        recordingFile = GetNumberOfFilesCcFtp(fileName, server, configData);
                    }

                    if (recordingFile != null)
                    {
                        fileFound = true;
                        recordingFile.FileNumber = currentFileNumber;
                        fileNames.Add(recordingFile);
                        break;
                    }
                }

                if (fileFound)
                    currentFileNumber++;
                else
                    nextFileNumber = false;

            }
            //
            //HTTP Nixxis
            //
            foreach (TransferProfileData server in serverList)
            {
                List<RecordingFileInformation> recordingFiles = new List<RecordingFileInformation>();

                if (server.Type == TransferProfileData.Types.Http)
                {
                    recordingFiles = GetNumberOfFilesCcHttp(fileId, server, configData);
                }
                else if (server.Type == TransferProfileData.Types.StrictHttp)
                {
                    recordingFiles = GetNumberOfFilesCcStrictHttp(fileId, server, configData);
                }

                if (recordingFiles.Count > 0)
                {
                    foreach (RecordingFileInformation item in recordingFiles)
                    {
                        item.FileNumber = currentFileNumber;
                        fileNames.Add(item);
                        currentFileNumber++;
                    }
                }
            }
            return fileNames;
        }

        public static RecordingFileInformation GetNumberOfFilesCcAppServer(string baseFilename, TransferProfileData server, ProfileData configData)
        {
            RecordingFileInformation fileName = null;

            string[] extlist = new string[] { "mp3" };
            if (server.Extension.Length > 0)
                extlist = server.Extension;

            foreach (string extension in extlist)
            {
                HttpWebResponse response = null;
                StreamReader reader = null;
                string fileReq = TransferFile.GetTransferAppServerUri(server.Root, server.Host, baseFilename, extension, configData.ServiceRelayRoot) + AppServerRequest.RelayAction.Size;
                try
                {
                    Tools.Log(string.Format("GetNumberOfFilesCcAppServer. Check file " + fileReq), Tools.LogType.Debug);

                    WebRequest request = HttpWebRequest.Create(fileReq);
                    request.Credentials = new NetworkCredential(server.User, server.Password);
                    response = request.GetResponse() as HttpWebResponse;

                    fileName = new RecordingFileInformation(baseFilename, extension, server, TransferFile.GetTransferAppServerUri(server.Root, server.Host, baseFilename, extension, configData.ServiceRelayRoot));

                    try
                    {
                        reader = new StreamReader(response.GetResponseStream());
                        int size = -1;
                        int.TryParse(reader.ReadToEnd(), out size);
                        fileName.FileSize = size;
                    }
                    catch (Exception error)
                    {
                        Tools.Log(string.Format("GetNumberOfFilesCcAppServer. Problem getting file size. {0}", error.ToString()), Tools.LogType.Debug);
                    }

                    Tools.Log(string.Format("GetNumberOfFilesCcAppServer. File is found {0}.", fileReq), Tools.LogType.Info);
                    break; //If file is found we should exit the for loop
                }
                catch (Exception error)
                {
                    Tools.Log(string.Format("GetNumberOfFilesCcAppServer. File is not found {0}.", fileReq), Tools.LogType.Info);
                    Tools.Log(error.ToString(), Tools.LogType.Debug);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();

                    if (response != null)
                        response.Close();
                }
            }
            return fileName;
        }

        public static RecordingFileInformation GetNumberOfFilesCcFtp(string baseFilename, TransferProfileData server, ProfileData configData)
        {
            RecordingFileInformation fileName = null;

            string[] extlist = new string[] { "mp3" };
            if (server.Extension.Length > 0)
                extlist = server.Extension;

            foreach (string extension in extlist)
            {
                FtpWebResponse response = null;
                Stream stream = null;
                StreamReader reader = null;

                string fileReq = TransferFile.GetTransferFtpUri(server.Root, server.Host, baseFilename, extension);
                try
                {
                    string[] list = fileReq.Split('@'); //For compatibillity
                    string iName = server.User;
                    string iPass = server.Password;
                    string iFile = fileReq;

                    if (list.Length > 1)
                    {
                        string[] lst = list[0].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);

                        if (lst.Length > 2)
                        {
                            iName = lst[1];
                            iPass = lst[2];
                            iFile = string.Concat(lst[0], "://", list[1]);
                            Tools.Log("WRN: In string credentials found!! Pleasse used the credentials for each transfer string", Tools.LogType.Info);
                        }
                    }
                    Tools.Log(string.Format("GetNumberOfFilesCcFtp. Check file " + iFile), Tools.LogType.Debug);
                    WebRequest request = FtpWebRequest.Create(iFile);
                    request.Credentials = new NetworkCredential(iName, iPass);
                    ((FtpWebRequest)request).UsePassive = !server.UseActiveFtp;
                    request.Method = WebRequestMethods.Ftp.GetFileSize;

                    response = request.GetResponse() as FtpWebResponse;

                    fileName = new RecordingFileInformation(baseFilename, extension, server, TransferFile.GetTransferFtpUri(server.Root, server.Host, baseFilename, extension));

                    try
                    {
                        int size = -1;
                        int.TryParse(response.ContentLength.ToString(), out size);
                        fileName.FileSize = size;
                    }
                    catch (Exception error)
                    {
                        Tools.Log(string.Format("GetNumberOfFilesCcFtp. Problem getting file size. {0}", error.ToString()), Tools.LogType.Debug);
                    }

                    Tools.Log(string.Format("GetNumberOfFilesCcFtp. File is found {0}.", fileReq), Tools.LogType.Info);
                }
                catch (WebException error)
                {
                    Tools.Log(string.Format("GetNumberOfFilesCcAppServer(WebException). File is not found {0}.", fileReq), Tools.LogType.Info);
                    Tools.Log(error.ToString(), Tools.LogType.Debug);
                }
                catch (Exception error)
                {
                    Tools.Log(string.Format("GetNumberOfFilesCcFtp(Exception). File is not found {0}.", fileReq), Tools.LogType.Info);
                    Tools.Log(error.ToString(), Tools.LogType.Debug);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();

                    if (stream != null)
                        stream.Close();

                    if (response != null)
                        response.Close();
                }
            }
            return fileName;
        }

        public static List<RecordingFileInformation> GetNumberOfFilesCcHttp(string baseFilename, TransferProfileData server, ProfileData configData)
        {
            //TO DO: retrieve filesize for this type of download.

            List<RecordingFileInformation> fileNames = new List<RecordingFileInformation>();

            if (string.IsNullOrEmpty(baseFilename))
                return fileNames;

            string[] extlist = new string[] { "mp3" };
            if (server.Extension.Length > 0)
                extlist = server.Extension;

            foreach (string extension in extlist)
            {
                HttpWebResponse response = null;
                Stream stream = null;
                StreamReader reader = null;
                int nof = 0;
                string fileReq = TransferFile.GetTransferHttpUri(server.Root, server.Host, baseFilename, extension) + Http.Action.Count;
                try
                {
                    Tools.Log(string.Format("GetNumberOfFilesCcHttp. Check file " + fileReq), Tools.LogType.Info);

                    WebRequest request = HttpWebRequest.Create(fileReq);
                    request.Credentials = new NetworkCredential(server.User, server.Password);
                    response = request.GetResponse() as HttpWebResponse;
                    stream = response.GetResponseStream();
                    reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    string strTemp = reader.ReadToEnd();
                    Tools.Log("File content " + strTemp, Tools.LogType.Info);
                    string[] text = strTemp.Split(new string[] { "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Tools.Log("File content start " + text[0], Tools.LogType.Info);
                    int.TryParse(text[0], out nof);

                    Tools.Log(string.Format("GetNumberOfFilesCcHttp. There are {0} File(s).", nof), Tools.LogType.Info);
                }
                catch (Exception error)
                {
                    Tools.Log(string.Format("GetNumberOfFilesCcHttp. File is not found {0}.", fileReq), Tools.LogType.Info);
                    Tools.Log(error.ToString(), Tools.LogType.Debug);
                }
                finally
                {
                    if (response != null)
                        response.Close();
                    try { stream.Close(); }
                    catch { }
                    try { response.Close(); }
                    catch { }
                }

                for (int i = 0; i < nof; i++)
                {
                    if (i == 0)
                    {
                        fileNames.Add(new RecordingFileInformation(baseFilename, "." + extension, server, TransferFile.GetTransferHttpUri(server.Root, server.Host, baseFilename, extension)));
                    }

                    else
                    {
                        fileNames.Add(new RecordingFileInformation(baseFilename + "." + i.ToString(), "." + extension, server, TransferFile.GetTransferHttpUri(server.Root, server.Host, string.Concat(baseFilename, ".", i.ToString()), extension)));
                    }
                }
            }
            return fileNames;
        }

        public static List<RecordingFileInformation> GetNumberOfFilesCcStrictHttp(string baseFilename, TransferProfileData server, ProfileData configData)
        {
            //TO DO: retrieve filesize for this type of download.

            List<RecordingFileInformation> fileNames = new List<RecordingFileInformation>();

            if (string.IsNullOrEmpty(baseFilename))
                return fileNames;

            string[] extlist = new string[] { "mp3" };
            if (server.Extension.Length > 0)
                extlist = server.Extension;

            foreach (string extension in extlist)
            {
                HttpWebResponse response = null;
                Stream stream = null;
                StreamReader reader = null;

                for (int nof = 0, err = 0; nof < 100 && err < 2; nof++ )
                {
                    string fileReq = "";
                    string ThisFileName = "";

                    try
                    {
                        if (nof == 0)
                        {
                            ThisFileName = baseFilename;
                        }

                        else
                        {
                            ThisFileName = baseFilename + "." + nof.ToString();
                        }

                        fileReq = TransferFile.GetTransferStrictHttpUri(server.Root, server.Host, ThisFileName, extension);

                        Tools.Log(string.Format("GetNumberOfFilesCcStrictHttp. Check file " + fileReq), Tools.LogType.Info);

                        WebRequest request = HttpWebRequest.Create(fileReq);
                        request.Credentials = new NetworkCredential(server.User, server.Password);

                        request.Method = "HEAD";
                        response = request.GetResponse() as HttpWebResponse;

                        Tools.Log(string.Format("GetNumberOfFilesCcStrictHttp. Found {0}", ThisFileName), Tools.LogType.Info);

                        RecordingFileInformation rfi = new RecordingFileInformation(ThisFileName, "." + extension, server, fileReq);

                        try
                        {
                            if (response.Headers.AllKeys.Contains("Content-Length"))
                                rfi.FileSize = int.Parse(response.Headers["Content-Length"]);
                        }
                        catch
                        {
                        }

                        fileNames.Add(rfi);
                    }
                    catch (Exception error)
                    {
                        err++;

                        Tools.Log(string.Format("GetNumberOfFilesCcStrictHttp. File is not found {0}.", ThisFileName), Tools.LogType.Info);
                        Tools.Log(error.ToString(), Tools.LogType.Debug);
                    }
                    finally
                    {
                        if (response != null)
                        {
                            try
                            {
                                response.Close();
                            }
                            catch { }
                        }
                        try 
                        { 
                            if(stream != null)
                                stream.Close(); 
                        }
                        catch { }
                    }
                }
            }
            return fileNames;
        }
    }
}
