using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Nixxis.Client.Agent
{
    public class NixxisAttachmentCollection : ObservableCollection<NixxisAttachmentItem>
    {
        public NixxisAttachmentItem ContainsId(string id)
        {
            foreach (NixxisAttachmentItem item in this)
            {
                if (item.Attachment.Id == id)
                    return item;
            }

            return null;
        }
    }

    public class NixxisAttachmentItem : INotifyPropertyChanged
    {
        #region Class data
        private AttachmentItem m_Attachment;
        private bool m_IsSelected;
        private string m_LocalId = string.Empty;
        #endregion

        #region Constructors
        public NixxisAttachmentItem(AttachmentItem attachment)
        {
            this.Attachment = attachment;
            this.IsSelected = attachment.InitialChecked;
        }
        public NixxisAttachmentItem(string localFile)
        {
            this.Attachment = new AttachmentItem(localFile);
            this.IsSelected = false;
        }
        #endregion

        #region Propeties
        public AttachmentItem Attachment
        {
            get { return m_Attachment; }
            set { m_Attachment = value; FirePropertyChanged("Attachment"); }
        }
        public bool IsSelected
        {
            get { return m_IsSelected; }
            set { m_IsSelected = value; FirePropertyChanged("IsSelected"); }
        }
        public string LocalId
        {
            get { return m_LocalId; }
            set { m_LocalId = value; FirePropertyChanged("LocalId"); }
        }
        #endregion

        #region Members
        public string GetId()
        {
            if (this.Attachment.LocationIsLocal && string.IsNullOrEmpty(this.LocalId))
            {
                this.LocalId = Guid.NewGuid().ToString("N");
            }

            return this.Attachment.LocationIsLocal ? this.LocalId : this.Attachment.Id;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
