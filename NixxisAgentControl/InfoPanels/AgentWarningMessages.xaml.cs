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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for AgentWarningMessages.xaml
    /// </summary>
    public partial class AgentWarningMessages : NixxisPanelSelectorItem
    {
        #region Class data
        #endregion

        #region Properties XAML
        #endregion

        #region Constructors
        public AgentWarningMessages()
        {
            InitializeComponent();
        }

        public AgentWarningMessages(HttpLink link)
        {
            ClientLink = link;

            InitializeComponent();
        }
        #endregion

        #region Member Override
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == NixxisPanelSelectorItem.ParentFormProperty)
            {
                InitParentForm();
            }
        }        
        #endregion

        #region Members
        private void InitParentForm()
        {
            if (this.ParentForm == null || ((AgentFrameSet)this.ParentForm).AgtWarningMsg == null)
            {
                try
                {
                    ((AgentFrameSet)this.ParentForm).AgtWarningMsg.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AgtWarningMsg_CollectionChanged);
                }
                catch { }

            }
            else
            {
                ((AgentFrameSet)this.ParentForm).AgtWarningMsg.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AgtWarningMsg_CollectionChanged);
            }
        }

        private void AgtWarningMsg_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                OnBringToFront();
            }
        }
        #endregion

        #region Events
        #endregion
    }
}
