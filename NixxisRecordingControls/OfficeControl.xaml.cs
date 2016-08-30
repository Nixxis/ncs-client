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

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for OfficeControl.xaml
    /// </summary>
    public partial class OfficeControl : UserControl
    {
        public OfficeControl()
        {
            InitializeComponent();
        }        
        
        #region Members override
        protected override void OnInitialized(EventArgs e)
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(OfficeControl_IsVisibleChanged);
            
            base.OnInitialized(e);
        }
        #endregion

        #region Members
        #region GUI
        public void SetToolbarPanel()
        {
        }

        public void RemoveToolbarPanel()
        {
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            e.Handled = true;
        }
        #endregion
        #endregion

        #region Handlers
        private void OfficeControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)sender).IsVisible)
            {
                SetToolbarPanel();
            }
            else
            {
                RemoveToolbarPanel();
            }
        }

        #endregion
    }
}
