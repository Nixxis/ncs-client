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
using System.Collections;
using System.Collections.Specialized;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for OverflowHelper.xaml
    /// </summary>
    public partial class PromptHelper : UserControl
    {
        public static readonly DependencyProperty MessageDisplayTextProperty = DependencyProperty.Register("MessageDisplayText", typeof(string), typeof(PromptHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MessageIdProperty = DependencyProperty.Register("MessageId", typeof(string), typeof(PromptHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string MessageRelativePath
        {
            get
            {
                return (DataContext as AdminObject).Core.Prompts[MessageId].RelativePath;
            }
        }

        public string MessageId
        {
            get
            {
                return (string)GetValue(MessageIdProperty);
            }
            set
            {
                SetValue(MessageIdProperty, value);
            }
        }

        public string MessageDisplayText
        {
            get
            {
                return (string)GetValue(MessageDisplayTextProperty);
            }
            set
            {
                SetValue(MessageDisplayTextProperty, value);
            }
        }

        public PromptHelper()
        {
            InitializeComponent();
        }


        public bool IncludeNone
        {
            get
            {
                AtivityPromptsToPromptsConverter aptpc = (Resources["AtivityPromptsToPromptsConverter"] as AtivityPromptsToPromptsConverter);
                return aptpc.IncludeNone;
            }
            set
            {
                AtivityPromptsToPromptsConverter aptpc = (Resources["AtivityPromptsToPromptsConverter"] as AtivityPromptsToPromptsConverter);
                aptpc.IncludeNone = value;
            }
        }


        private void AddPromptToOverflowPrompt(object sender, RoutedEventArgs e)
        {
            DlgAddPrompt dlg = new DlgAddPrompt();
            dlg.Owner = Application.Current.MainWindow;

            if (DataContext is CollectionViewSource)
            {
                CollectionViewSource cvs = DataContext as CollectionViewSource;
                if (cvs.View.CurrentItem is Activity)
                {
                    dlg.WizControl.Context = cvs.View.CurrentItem as Activity;
                    dlg.DataContext = (dlg.WizControl.Context as Activity).Core;
                }
                else if (cvs.View.CurrentItem is Campaign)
                {
                    dlg.WizControl.Context = (cvs.View.CurrentItem as Campaign).SystemInboundActivity;
                    dlg.DataContext = (dlg.WizControl.Context as Activity).Core;
                }
                else if (cvs.View.CurrentItem is Queue)
                {
                    dlg.WizControl.Context = cvs.View.CurrentItem as Queue;
                    dlg.DataContext = (dlg.WizControl.Context as Queue).Core;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (DataContext is Activity)
            {
                dlg.WizControl.Context = DataContext as Activity;
                dlg.DataContext = (dlg.WizControl.Context as Activity).Core;
            }
            else if (DataContext is Queue)
            {
                dlg.WizControl.Context = DataContext as Queue;
                dlg.DataContext = (dlg.WizControl.Context as Queue).Core;
            }
            else
                throw new NotImplementedException();

            dlg.WizControl.CurrentStep = "Start";

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                cboMessage.SelectedValue = dlg.Created.Id;
            }


        }

    }

}
