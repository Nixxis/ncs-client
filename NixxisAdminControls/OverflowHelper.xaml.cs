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
using Nixxis.Client.Controls;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for OverflowHelper.xaml
    /// </summary>
    public partial class OverflowHelper : UserControl
    {
        public static readonly DependencyProperty OverflowActionTypeProperty = DependencyProperty.Register("OverflowActionType", typeof(object), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OverflowActionChanged)));
        public static readonly DependencyProperty OverflowPreprocessorProperty = DependencyProperty.Register("OverflowPreprocessor", typeof(AdminObjectReference<Preprocessor>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowPreprocessorParamsProperty = DependencyProperty.Register("OverflowPreprocessorParams", typeof(string), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowPreprocessorConfigProperty = DependencyProperty.Register("OverflowPreprocessorConfig", typeof(BasePreprocessorConfig), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowMessageProperty = DependencyProperty.Register("OverflowMessage", typeof(AdminObjectReference<Prompt>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OverflowMessageChanged)));
        public static readonly DependencyProperty OverflowReroutePromptProperty = DependencyProperty.Register("OverflowReroutePrompt", typeof(AdminObjectReference<Prompt>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OverflowReroutePromptChanged)));
        public static readonly DependencyProperty OverflowRerouteDestinationProperty = DependencyProperty.Register("OverflowRerouteDestination", typeof(string), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OverflowHelper));
        public static readonly DependencyProperty PromptsProperty = DependencyProperty.Register("Prompts", typeof(IEnumerable<PromptLink>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(PromptsChanged)));
        public static readonly DependencyProperty PromptsCopyProperty = DependencyProperty.Register("PromptsCopy", typeof(IEnumerable<PromptLink>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty PreprocessorsProperty = DependencyProperty.Register("Preprocessors", typeof(IEnumerable<Preprocessor>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(PreprocessorsChanged)));
        public static readonly DependencyProperty PreprocessorsCopyProperty = DependencyProperty.Register("PreprocessorsCopy", typeof(IEnumerable<Preprocessor>), typeof(OverflowHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public static void OverflowActionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public static void OverflowMessageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public static void OverflowReroutePromptChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public static void PromptsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OverflowHelper oh = (OverflowHelper)obj;
            if (args.NewValue != null)
            {
                // Not easy
                // Workaround here! Break the binding before changing the ItemsSource, else the SelectedItem is lost
                oh.cboMessage.SelectedValuePath = null;
                oh.cboMessageRerouting.SelectedValuePath = null;

                oh.PromptsCopy = oh.Prompts;
                
                oh.cboMessage.SelectedValuePath = "Id";
                oh.cboMessageRerouting.SelectedValuePath = "Id";
            }
        }
        public IEnumerable<PromptLink> Prompts
        {
            get
            {
                return (IEnumerable<PromptLink>)GetValue(PromptsProperty);
            }
            set
            {
                SetValue(PromptsProperty, value);
            }
        }
        public IEnumerable<PromptLink> PromptsCopy
        {
            get
            {
                return (IEnumerable<PromptLink>)GetValue(PromptsCopyProperty);
            }
            set
            {
                SetValue(PromptsCopyProperty, value);
            }
        }
        public static void PreprocessorsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OverflowHelper oh = (OverflowHelper)obj;
            if (args.NewValue != null)
            {
                oh.PreprocessorsCopy = oh.Preprocessors;
            }
        }
        public IEnumerable<Preprocessor> Preprocessors
        {
            get
            {
                return (IEnumerable<Preprocessor>)GetValue(PreprocessorsProperty);
            }
            set
            {
                SetValue(PreprocessorsProperty, value);
            }
        }
        public IEnumerable<Preprocessor> PreprocessorsCopy
        {
            get
            {
                return (IEnumerable<Preprocessor>)GetValue(PreprocessorsCopyProperty);
            }
            set
            {
                SetValue(PreprocessorsCopyProperty, value);
            }
        }
        public object OverflowActionType
        {
            get
            {
                return GetValue(OverflowActionTypeProperty);
            }
            set
            {
                SetValue(OverflowActionTypeProperty, value);
            }
        }
        public string OverflowPreprocessorParams
        {
            get
            {
                return (string)GetValue(OverflowPreprocessorParamsProperty);
            }
            set
            {
                SetValue(OverflowPreprocessorParamsProperty, value);
            }
        }
        public BasePreprocessorConfig OverflowPreprocessorConfig
        {
            get
            {
                return (BasePreprocessorConfig)GetValue(OverflowPreprocessorConfigProperty);
            }
            set
            {
                SetValue(OverflowPreprocessorConfigProperty, value);
            }
        }
        public AdminObjectReference<Preprocessor> OverflowPreprocessor
        {
            get
            {
                return (AdminObjectReference <Preprocessor>) GetValue(OverflowPreprocessorProperty);
            }
            set
            {

                SetValue(OverflowPreprocessorProperty, value);

            }
        }
        public AdminObjectReference<Prompt> OverflowMessage
        {
            get
            {
                return (AdminObjectReference<Prompt>)GetValue(OverflowMessageProperty);
            }
            set
            {
                SetValue(OverflowMessageProperty, value);
            }
        }
        public AdminObjectReference<Prompt> OverflowReroutePrompt
        {
            get
            {
                return (AdminObjectReference<Prompt>)GetValue(OverflowReroutePromptProperty);
            }
            set
            {
                SetValue(OverflowReroutePromptProperty, value);
            }
        }
        public string OverflowRerouteDestination
        {
            get
            {
                return (string)GetValue(OverflowRerouteDestinationProperty);
            }
            set
            {
                SetValue(OverflowRerouteDestinationProperty, value);
            }
        }
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public OverflowHelper()
        {
            InitializeComponent();
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

        private void AddPromptToOverflowReroutePrompt(object sender, RoutedEventArgs e)
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
                cboMessageRerouting.SelectedValue = dlg.Created.Id;
            }

        }

        private void PreprocessorConfigure(object sender, RoutedEventArgs e)
        {
            AdminObject related = null;
            Type tpe = null;

            if (DataContext is CollectionViewSource)
            {
                CollectionViewSource cvs = DataContext as CollectionViewSource;
                if (cvs.View.CurrentItem is Activity)
                    related = cvs.View.CurrentItem as Activity;
                else if (cvs.View.CurrentItem is Campaign)
                    related = (cvs.View.CurrentItem as Campaign).SystemInboundActivity;
                else if (cvs.View.CurrentItem is Queue)
                    related = cvs.View.CurrentItem as Queue;
                else
                    throw new NotImplementedException();
            }
            else if (DataContext is Activity)
                related = DataContext as Activity;
            else if (DataContext is Queue)
                related = DataContext as Queue;
            else
                throw new NotImplementedException();





            WaitScreen ws = new WaitScreen();
            ws.Owner = Application.Current.MainWindow;
            ws.Show();

            IPreprocessorConfigurationDialog preprocdlg = (IPreprocessorConfigurationDialog)Activator.CreateInstance(OverflowPreprocessor.Target.DialogType);

            ws.Close();

            preprocdlg.Owner = Application.Current.MainWindow;
            preprocdlg.WizControlContext = related;
            preprocdlg.DataContext = DataContext;
            preprocdlg.Config = OverflowPreprocessorConfig;

            if (((Window)preprocdlg).ShowDialog().GetValueOrDefault())
            {
                OverflowPreprocessorConfig = preprocdlg.Config;


                if (preprocdlg.Config != null)
                {
                    OverflowPreprocessorConfig.saveToTextStorage = ((t) => { OverflowPreprocessorParams = t; }); 
                    preprocdlg.Config.Save();
                }
                else
                    OverflowPreprocessorParams = null;
            }
        }
    }

    public class AtivityPromptsToPromptsEnumerable : IEnumerable, INotifyCollectionChanged, IWeakEventListener, IEnumerable<AdminObject>
    {

        public AtivityPromptsToPromptsEnumerable(AdminObjectList<PromptLink> list)
        {
            InternalList = list;
            if (list is INotifyCollectionChanged)
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)list, this);
        }

        public AtivityPromptsToPromptsEnumerable(IEnumerable<PromptLink> list)
        {
            InternalIEnum = list;
            if (list is INotifyCollectionChanged)
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)list, this);
        }


        public AdminObjectList<PromptLink> InternalList
        {
            get;
            set;
        }
        public IEnumerable<PromptLink> InternalIEnum
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerator<PromptLink> InternalEnumerator = null;

            if (InternalList != null)
            {
                InternalEnumerator = InternalList.GetEnumerator();
            }
            else if (InternalIEnum != null)
            {
                InternalEnumerator = InternalIEnum.GetEnumerator();
            }

            if (InternalEnumerator != null)
            {
                while (InternalEnumerator.MoveNext())
                    if(InternalEnumerator.Current!=null)
                        yield return InternalEnumerator.Current.Prompt;
            }
        }

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {

            List<Prompt> NewItems = new List<Prompt>();
            List<Prompt> OldItems = new List<Prompt>();

            if (args.NewItems != null)
            {
                for (int i = 0; i < args.NewItems.Count; i++)
                    NewItems.Insert(i, ((PromptLink)args.NewItems[i]).Prompt);
            }
            if (args.OldItems != null)
            {
                for (int i = 0; i < args.OldItems.Count; i++)
                    OldItems.Insert(i, ((PromptLink)args.OldItems[i]).Prompt);
            }


            NotifyCollectionChangedEventArgs newArgs = null;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if(args.OldItems == null && args.NewStartingIndex==-1 && args.OldStartingIndex==-1)
                        newArgs = new NotifyCollectionChangedEventArgs(args.Action, NewItems);
                    else
                        throw new NotImplementedException();
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // TODO: check that!!! called when removing a prompt from the inbound tab!!! 
                    newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (args.NewItems == null && args.OldItems == null && args.NewStartingIndex == -1 && args.OldStartingIndex == -1)
                        newArgs = new NotifyCollectionChangedEventArgs(args.Action);
                    else
                        throw new NotImplementedException();
                    break;
            }


            if (CollectionChanged != null)
                CollectionChanged(this, newArgs);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            FireCollectionChanged((NotifyCollectionChangedEventArgs)e);
            return true;
        }


        IEnumerator<AdminObject> IEnumerable<AdminObject>.GetEnumerator()
        {
            IEnumerator<PromptLink> InternalEnumerator = null;

            if (InternalList != null)
            {
                InternalEnumerator = InternalList.GetEnumerator();
            }
            else if (InternalIEnum != null)
            {
                InternalEnumerator = InternalIEnum.GetEnumerator();
            }

            if (InternalEnumerator != null)
            {
                while (InternalEnumerator.MoveNext())
                    if(InternalEnumerator.Current!=null)
                        yield return InternalEnumerator.Current.Prompt;
            }
        }
    }

    public class QuotaComputationMehodTranslator : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            QuotaComputationMethod qtc = (QuotaComputationMethod)value;

            return new QuotaComputationMethodsHelper().First((a) => (a.EnumValue == qtc)).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuotaTargetComputationMehodTranslator : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            QuotaTargetComputationMethod qtc = (QuotaTargetComputationMethod)value;

            return new QuotaTargetComputationMethodsHelper().First((a) => (a.EnumValue == qtc)).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ContainsItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<AdminObject> fields = (IEnumerable<AdminObject>)value;
            return fields.Count() > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AtivityPromptsToPromptsConverter : IValueConverter
    {
        private bool m_IncludeNone = false;

        public bool IncludeNone
        {
            get
            {
                return m_IncludeNone;
            }
            set
            {
                m_IncludeNone = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (value is AdminObjectList<PromptLink>)
                {
                    if(IncludeNone)
                        return new ExtendedEnumerable(new AtivityPromptsToPromptsEnumerable((AdminObjectList < PromptLink >) value));
                    else
                        return new AtivityPromptsToPromptsEnumerable((AdminObjectList<PromptLink>)value);
                }
                if (value is IEnumerable<PromptLink>)
                {
                    if (IncludeNone)
                        return new ExtendedEnumerable(new AtivityPromptsToPromptsEnumerable((IEnumerable<PromptLink>)value));
                    else
                        return new AtivityPromptsToPromptsEnumerable((IEnumerable<PromptLink>)value);
                }
                else if (value is IEnumerable<Prompt>)
                {
                    if(IncludeNone)
                        return new ExtendedEnumerable((IEnumerable)value);
                    else
                        return (IEnumerable)value;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
