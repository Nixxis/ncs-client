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
using Microsoft.Win32;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for AttachmentControl.xaml
    /// </summary>
    public partial class AttachmentControl : UserControl
    {

        #region Properties XAML
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(NixxisAttachmentCollection), typeof(AttachmentControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public NixxisAttachmentCollection ItemSource
        {
            get { return (NixxisAttachmentCollection)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }


        public static readonly DependencyProperty CanAddLocalAttachmentsProperty = DependencyProperty.Register("CanAddLocalAttachments", typeof(bool), typeof(AttachmentControl), new PropertyMetadata(false));
        public bool CanAddLocalAttachments
        {
            get { return (bool)GetValue(CanAddLocalAttachmentsProperty); }
            set { SetValue(CanAddLocalAttachmentsProperty, value); }
        }

        #endregion

        #region Constrcutors
        public AttachmentControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Members override        
        public override void BeginInit()
        {
            base.BeginInit();
            this.ItemSource = new NixxisAttachmentCollection();
        }
        #endregion

        #region Members
        public void SetDefaultAttachments(AttachmentCollection attachments)
        {
            this.ItemSource = new NixxisAttachmentCollection();

            foreach (AttachmentItem item in attachments)
            {
                this.ItemSource.Add(new NixxisAttachmentItem(item));
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        #endregion

        private void btnAddCustomAttachement_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.CheckFileExists = true;
            dlg.Multiselect = true;

            if (dlg.ShowDialog(Application.Current.MainWindow) == true)
            {

                foreach (string item in dlg.FileNames)
                {
                    this.ItemSource.Add(new NixxisAttachmentItem(item) { IsSelected = true });
                }
            }
        }
    }

}
