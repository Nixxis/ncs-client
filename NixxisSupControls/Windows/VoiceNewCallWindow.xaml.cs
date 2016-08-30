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

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for ManualCallWindow.xaml
    /// </summary>
    public partial class VoiceNewCallWindow : Window
    {
        public string SelectedItem { get; private set; }

        public VoiceNewCallWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
            NewCallCrtl.DialRequest += new VoiceNewCallDialHandeler(NewCallCrtl_DialRequest);
        }

        public void NewCallCrtl_DialRequest(string destination)
        {
            if (destination != null)
            {
                SelectedItem = destination;

                this.DialogResult = true;

                Close();
            }
        }

        public void SetItemSource(string[] collection)
        {
            NewCallCrtl.ItemSource = collection;
        }

        public void SetDefaultValue(string destination)
        {
            NewCallCrtl.SelectedItem = destination;
        }
    }
}
