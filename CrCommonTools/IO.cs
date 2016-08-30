using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools.IO
{
    public abstract class BaseFileInformation
    {
        #region Class data
        private DateTime m_CreationDate;
        private DateTime m_ModifyDateDate;
        private string m_Name;
        private string m_FullName;
        private bool m_FtpCheck = false;
        #endregion

        #region Constructor
        public BaseFileInformation()
        {
        }
        #endregion

        #region Properties
        public DateTime CreationDate
        {
            get { return m_CreationDate; }
            set { m_CreationDate = value; }
        }
        public DateTime ModifyDateDate
        {
            get { return m_ModifyDateDate; }
            set { m_ModifyDateDate = value; }
        }
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        public string FullName
        {
            get { return m_FullName; }
            set { m_FullName = value; }
        }
        public bool FtpCheck
        {
            get { return m_FtpCheck; }
            set { m_FtpCheck = value; }
        }
        #endregion
    }

    public class FolderInformation : BaseFileInformation
    {
        #region Class data
        private FolderInformation m_Parent = null;
        private SortedList<string, FolderInformation> m_Folders = new SortedList<string, FolderInformation>();
        private SortedList<string, FileInformation> m_Files = new SortedList<string, FileInformation>();
        #endregion

        #region Constructor
        public FolderInformation()
            : base()
        {
        }
        public FolderInformation(FolderInformation parent)
            : base()
        {
            m_Parent = parent;
        }
        #endregion

        #region Properties
        public FolderInformation Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }
        public SortedList<string, FolderInformation> Folders
        {
            get { return m_Folders; }
            set { m_Folders = value; }
        }
        public SortedList<string, FileInformation> Files
        {
            get { return m_Files; }
            set { m_Files = value; }
        }
        #endregion

        #region Members
        public void SetFtpCheck()
        {
            foreach (FolderInformation item in m_Folders.Values)
            {
                item.FtpCheck = true;
            }
        }
        public void FtpClearCheck()
        {

            try
            {
                IList<string> _Keys = m_Folders.Keys;
                foreach (string item in _Keys)
                {
                    if (m_Folders[item].FtpCheck)
                    {
                        m_Folders.Remove(item);
                    }
                }
            }
            catch { }
        }
        #endregion
    }

    public class FileInformation : BaseFileInformation
    {
        #region Class data
        private Int64 m_Size = 0;
        #endregion

        #region Constructor
        public FileInformation()
            : base()
        {
        }
        #endregion

        #region Properties
        public Int64 Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }
        #endregion
    }

    public class FileList : SortedList<string, FileInformation>
    {

        #region Members
        public FileList FindRecordings(string id)
        {
            FileList returnValue = new FileList();
            foreach (FileInformation file in this.Values)
            {
                if(file.Name.IndexOf(id) > -1) 
                    returnValue.Add(file);
            }
            return returnValue;
        }
        public void Add(FileInformation file)
        {
            this.Add(file.Name, file);
        }
        #endregion
    }
}
