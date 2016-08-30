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
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for WizardControl.xaml
    /// </summary>
    public partial class WizardControl : UserControl
    {
        public object Context { get; set; }
        public Stack<string> m_Nexts = new Stack<string>();
        public Stack<object> m_ParamsNexts = new Stack<object>();
        public Dictionary<string, string> m_Relations = new Dictionary<string, string>();
        public Stack<string> m_History = new Stack<string>();
        public Stack<object> m_ParamsHistory = new Stack<object>();

        public static readonly RoutedEvent WizardFinishedEvent = EventManager.RegisterRoutedEvent("WizardFinished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WizardControl));
        public event RoutedEventHandler WizardFinished
        {
            add { AddHandler(WizardFinishedEvent, value); }
            remove { RemoveHandler(WizardFinishedEvent, value); }
        }

        public static readonly RoutedEvent WizardStepChangingEvent = EventManager.RegisterRoutedEvent("WizardStepChanging", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WizardControl));
        public event RoutedEventHandler WizardStepChanging
        {
            add { AddHandler(WizardStepChangingEvent, value); }
            remove { RemoveHandler(WizardStepChangingEvent, value); }
        }



        public static readonly DependencyProperty EndStepProperty = DependencyProperty.Register("EndStep", typeof(string), typeof(WizardControl), new PropertyMetadata(new PropertyChangedCallback(EndStepChanged)));
        public static readonly DependencyProperty NextStepProperty = DependencyProperty.RegisterAttached("NextStep", typeof(string), typeof(WizardControl), new PropertyMetadata(new PropertyChangedCallback(NextStepChanged)));
        public static readonly DependencyProperty NextStepParamProperty = DependencyProperty.RegisterAttached("NextStepParam", typeof(object), typeof(WizardControl), new PropertyMetadata(new PropertyChangedCallback(NextStepParamChanged)));
        public static readonly DependencyProperty RequiredProperty = DependencyProperty.RegisterAttached("Required", typeof(bool), typeof(WizardControl), new PropertyMetadata(false, new PropertyChangedCallback(RequiredChanged)));
        public static readonly DependencyProperty CurrentStepProperty = DependencyProperty.Register("CurrentStep", typeof(string), typeof(WizardControl), new PropertyMetadata(new PropertyChangedCallback(CurrentStepChanged)));
        public static readonly DependencyProperty CurrentStepParamProperty = DependencyProperty.Register("CurrentStepParam", typeof(object), typeof(WizardControl), new PropertyMetadata(new PropertyChangedCallback(CurrentStepParamChanged)));
        public static readonly DependencyProperty PreviousStepProperty = DependencyProperty.Register("PreviousStep", typeof(string), typeof(WizardControl));
        public static readonly DependencyProperty WizardGridProperty = DependencyProperty.Register("WizardGrid", typeof(Grid), typeof(WizardControl), new PropertyMetadata(new PropertyChangedCallback(WizardGridChanged)));

        public static void RequiredChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            
        }

        public static void EndStepChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            WizardControl wiz = (WizardControl)obj;

            if (args.NewValue != null && !args.NewValue.Equals(wiz.CurrentStep ?? ""))
            {
                wiz.m_Nexts.Push((string)(args.NewValue));
                wiz.m_ParamsNexts.Push(null);
            }
        }
        public static void NextStepChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public static void NextStepParamChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        private bool initialized = false;

        public static void CurrentStepChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            WizardControl wiz = (WizardControl)obj;

            if (wiz.WizardGrid != null)
            {
                
                wiz.initialized = true;
                
                foreach (FrameworkElement fe in wiz.WizardGrid.Children)
                {
                    if (fe.Name.Equals(args.NewValue))
                    {
                        fe.Visibility = Visibility.Visible;
                        Helpers.SetFocusOnFirstTabStop(fe);
                    }
                    else
                    {
                        if (wiz.EndStep.Equals(fe.Name))
                            fe.Visibility = Visibility.Collapsed;
                        else
                            fe.Visibility = Visibility.Hidden;
                    }
                }


                wiz.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    try
                    {
                        FrameworkElement[] fes = Helpers.FindChildren<ToggleButton>(wiz.WizardGrid, (tb) => (((ToggleButton)tb).IsChecked.GetValueOrDefault() && tb.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(tb))));

                        if ((fes != null && fes.Length > 0) || wiz.m_Nexts.Count > 0 || Helpers.FindChild<Panel>(wiz.WizardGrid, (fe) => (fe.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(fe)))) != null)
                        {
                            wiz.btnNext.Content = TranslationContext.Default.Translate("Next");
                        }
                        else
                        {
                            wiz.btnNext.Content =  TranslationContext.Default.Translate("Finish");
                        }

                        FrameworkElement[] required = Helpers.FindChildren<FrameworkElement>(wiz.WizardGrid, (tb) => (/*tb.IsVisible &&*/ WizardControl.GetRequired(tb)));
                        if (required != null && required.Length > 0)
                        {
                            MultiBinding mb = new MultiBinding();
                            foreach (FrameworkElement fe in required)
                            {

                                if (fe is TextBox)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(TextBox.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(TextBox.TextProperty) });
                                }
                                else if (fe is ComboBox)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ComboBox.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ComboBox.TextProperty) });
                                }
                                else if (fe is ToggleButton)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ToggleButton.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ToggleButton.IsCheckedProperty) });
                                }
                                else if(fe is NumericUpDown)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(NumericUpDown.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(NumericUpDown.IsInExceptionProperty), Converter = new BoolInverterConverter() });
                                }
                                else if (fe is DatePicker)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(DatePicker.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(DatePicker.SelectedDateProperty) });
                                }
                                else if (fe is PasswordBox)
                                {
                                    // TODO
                                }
                                else
                                    throw new NotImplementedException();
                            }
                            mb.Converter = new RequiredConverter();
                            BindingOperations.SetBinding(wiz.btnNext, Button.IsEnabledProperty, mb);
                        }
                        else
                        {
                            BindingOperations.ClearBinding(wiz.btnNext, Button.IsEnabledProperty);
                            wiz.btnNext.IsEnabled = true;
                        }

                        Window dialogWindow = Helpers.GetTopParentWindow(wiz);
                        if (dialogWindow != null)
                        {
                            if (dialogWindow.SizeToContent == System.Windows.SizeToContent.WidthAndHeight)
                            {
                                double ah = dialogWindow.ActualHeight;
                                double aw = dialogWindow.ActualWidth;
                                dialogWindow.SizeToContent = System.Windows.SizeToContent.Manual;
                                dialogWindow.Height = ah;
                                dialogWindow.Width = aw;
                            }
                        }

                    }
                    catch
                    {
                    }

                }));
            }
            wiz.PreviousStep = args.OldValue as string;
            wiz.RaiseEvent(new RoutedEventArgs(WizardControl.WizardStepChangingEvent));
        }
        public static void CurrentStepParamChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static void WizardGridChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {


            WizardControl wiz = (WizardControl)obj;

            if (wiz.WizardGrid != null && !wiz.initialized)
            {

                wiz.initialized = true;

                foreach (FrameworkElement fe in wiz.WizardGrid.Children)
                {
                    if (fe.Name.Equals(wiz.CurrentStep))
                    {
                        fe.Visibility = Visibility.Visible;
                        Helpers.SetFocusOnFirstTabStop(fe);

                    }
                    else
                    {
                        if (wiz.EndStep.Equals(fe.Name))
                            fe.Visibility = Visibility.Collapsed;
                        else
                            fe.Visibility = Visibility.Hidden;
                    }
                }


                wiz.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    try
                    {
                        FrameworkElement[] fes = Helpers.FindChildren<ToggleButton>(wiz.WizardGrid, (tb) => (((ToggleButton)tb).IsChecked.GetValueOrDefault() && tb.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(tb))));

                        if ((fes != null && fes.Length > 0) || wiz.m_Nexts.Count > 0 || Helpers.FindChild<Panel>(wiz.WizardGrid, (fe) => (fe.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(fe)))) != null)
                        {
                            wiz.btnNext.Content =  TranslationContext.Default.Translate("Next");
                        }
                        else
                        {
                            wiz.btnNext.Content =  TranslationContext.Default.Translate("Finish");
                        }

                        FrameworkElement[] required = Helpers.FindChildren<FrameworkElement>(wiz.WizardGrid, (tb) => (/*tb.IsVisible && */WizardControl.GetRequired(tb)));
                        if (required != null && required.Length > 0)
                        {
                            MultiBinding mb = new MultiBinding();
                            foreach (FrameworkElement fe in required)
                            {

                                if (fe is TextBox)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(TextBox.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(TextBox.TextProperty) });
                                }
                                else if (fe is ComboBox)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ComboBox.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ComboBox.TextProperty) });
                                }
                                else if (fe is NumericUpDown)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(NumericUpDown.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(NumericUpDown.IsInExceptionProperty), Converter = new BoolInverterConverter() });
                                }
                                else if (fe is ToggleButton)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ToggleButton.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(ToggleButton.IsCheckedProperty) });
                                }
                                else if (fe is DatePicker)
                                {
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(DatePicker.IsVisibleProperty) });
                                    mb.Bindings.Add(new Binding() { Source = fe, Path = new PropertyPath(DatePicker.SelectedDateProperty) });
                                }
                                else if (fe is PasswordBox)
                                {
//                                      TODO
                                }
                                else
                                    throw new NotImplementedException();
                            }
                            mb.Converter = new RequiredConverter();
                            BindingOperations.SetBinding(wiz.btnNext, Button.IsEnabledProperty, mb);
                        }
                        else
                        {
                            BindingOperations.ClearBinding(wiz.btnNext, Button.IsEnabledProperty);
                            wiz.btnNext.IsEnabled = true;
                        }

                        Window dialogWindow = Helpers.GetTopParentWindow(wiz);
                        if (dialogWindow != null)
                        {
                            if (dialogWindow.SizeToContent == System.Windows.SizeToContent.WidthAndHeight)
                            {
                                double ah = dialogWindow.ActualHeight;
                                double aw = dialogWindow.ActualWidth;
                                dialogWindow.SizeToContent = System.Windows.SizeToContent.Manual;
                                dialogWindow.Height = ah;
                                dialogWindow.Width = aw;
                            }
                        }

                    }
                    catch
                    {
                    }

                }));
            }

        }

        public static void SetNextStep(UIElement element, string value)
        {
            element.SetValue(NextStepProperty, value);
        }
        public static string GetNextStep(UIElement element)
        {
            return (string)(element.GetValue(NextStepProperty));
        }

        public static void SetNextStepParam(UIElement element, object value)
        {
            element.SetValue(NextStepParamProperty, value);
        }
        public static object GetNextStepParam(UIElement element)
        {
            return (element.GetValue(NextStepParamProperty));
        }

        public static void SetRequired(UIElement element, bool value)
        {
            element.SetValue(RequiredProperty, value);
        }
        public static bool GetRequired(UIElement element)
        {
            return (bool)(element.GetValue(RequiredProperty));
        }

        public string EndStep
        {
            get
            {
                return (string)GetValue(EndStepProperty);
            }
            set
            {
                SetValue(EndStepProperty, value);
            }
        }
        public string CurrentStep
        {
            get
            {
                return (string)GetValue(CurrentStepProperty);
            }
            set
            {
                SetValue(CurrentStepProperty, value);                
            }
        }
        public object CurrentStepParam
        {
            get
            {
                return GetValue(CurrentStepParamProperty);
            }
            set
            {
                SetValue(CurrentStepParamProperty, value);
            }
        }
        public string PreviousStep
        {
            get
            {
                return (string)GetValue(PreviousStepProperty);
            }
            set
            {
                SetValue(PreviousStepProperty, value);
            }
        }
        public Grid WizardGrid
        {
            get
            {
                return (Grid)GetValue(WizardGridProperty);
            }
            set
            {
                Grid grd = value as Grid;
                if (grd.Parent is ContentControl)
                {
                    (grd.Parent as ContentControl).Content = null;
                }

                SetValue(WizardGridProperty, value);
            }
        }

        public void Reset()
        {
            BindingOperations.ClearBinding(btnNext, Button.IsEnabledProperty);
            btnNext.IsEnabled = true;

            while (btnPrevious.IsEnabled)
            {
                if (m_History.Count > 0)
                {
                    m_Nexts.Push(CurrentStep);
                    m_ParamsNexts.Push(CurrentStepParam);
                    CurrentStep = m_History.Pop();
                    CurrentStepParam = m_ParamsHistory.Pop();
                }

                if (m_History.Count <= 0)
                    btnPrevious.IsEnabled = false;

                while (m_Nexts.Count > 0 && m_Relations.ContainsKey(m_Nexts.Peek()) && m_Relations[m_Nexts.Peek()].Equals(CurrentStep))
                {
                    m_Nexts.Pop();
                    m_ParamsNexts.Pop();
                }

            }
        }

        public WizardControl()
        {
            InitializeComponent();
        }

        public WizardControl(object context, string startStep, string endStep)
            : this()
        {
            Context = context;
            CurrentStep = startStep;
            EndStep = endStep;
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


        }

        public void PreviousClick(object sender, RoutedEventArgs e)
        {
            if (m_History.Count > 0)
            {
                m_Nexts.Push(CurrentStep);
                m_ParamsNexts.Push(CurrentStepParam);
                CurrentStep = m_History.Pop();
                CurrentStepParam = m_ParamsHistory.Pop();
            }

            if(m_History.Count <= 0)
                btnPrevious.IsEnabled = false;

            while (m_Nexts.Count > 0 && m_Relations.ContainsKey(m_Nexts.Peek()) && m_Relations[m_Nexts.Peek()].Equals(CurrentStep))
            {
                m_Nexts.Pop();
                m_ParamsNexts.Pop();
            }
        }
        private void NextClick(object sender, RoutedEventArgs e)
        {

            FrameworkElement[] fes = Helpers.FindChildren<ToggleButton>(WizardGrid, (tb) => (((ToggleButton)tb).IsChecked.GetValueOrDefault() && tb.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(tb))));
            if (fes.Length > 0)
            {
                string strBackup = CurrentStep;

                // we check that here to prevent CurrentStep from making the panel invisible
                FrameworkElement fe = Helpers.FindChild<Panel>(WizardGrid, (tb) => (tb.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(tb))));

                if (fe != null)
                {
                    string strToDo = WizardControl.GetNextStep(fe);
                    m_Nexts.Push(strToDo);
                    m_ParamsNexts.Push(WizardControl.GetNextStepParam(fe));
                    if (!m_Relations.ContainsKey(strToDo))
                        m_Relations.Add(strToDo, strBackup);
                    else
                        m_Relations[strToDo] = strBackup;
                }

                // we go on the first one and add the others on the todo list
                
                m_History.Push(CurrentStep);
                m_ParamsHistory.Push(CurrentStepParam);
                CurrentStep = WizardControl.GetNextStep(fes[0]);
                CurrentStepParam = WizardControl.GetNextStepParam(fes[0]);
                if (!m_Relations.ContainsKey(CurrentStep))
                    m_Relations.Add(CurrentStep, strBackup);
                else
                    m_Relations[CurrentStep] = strBackup;

                btnPrevious.IsEnabled = true;

                for (int i = fes.Length-1; i>=1; i--)
                {
                    string strToDo = WizardControl.GetNextStep(fes[i]);
                    m_Nexts.Push(strToDo);
                    m_ParamsNexts.Push(WizardControl.GetNextStepParam(fes[i]));
                    if (!m_Relations.ContainsKey(strToDo))
                        m_Relations.Add(strToDo, strBackup);
                    else
                        m_Relations[strToDo] = strBackup;
                }
            }
            else
            {
                FrameworkElement fe = Helpers.FindChild<Panel>(WizardGrid, (tb) => (tb.IsVisible && !string.IsNullOrEmpty(WizardControl.GetNextStep(tb))));

                if (fe != null)
                {
                    string strBackup = CurrentStep;
                    m_History.Push(CurrentStep);
                    m_ParamsHistory.Push(CurrentStepParam);
                    CurrentStep = WizardControl.GetNextStep(fe);
                    CurrentStepParam = WizardControl.GetNextStepParam(fe);
                    if (!m_Relations.ContainsKey(CurrentStep))
                        m_Relations.Add(CurrentStep, strBackup);
                    else
                        m_Relations[CurrentStep] = strBackup;

                    btnPrevious.IsEnabled = true;

                }
                else
                {
                    if (m_Nexts.Count > 0)
                    {
                        m_History.Push(CurrentStep);
                        m_ParamsHistory.Push(CurrentStepParam);
                        CurrentStep = m_Nexts.Pop();
                        CurrentStepParam = m_ParamsNexts.Pop();
                        btnPrevious.IsEnabled = true;
                    }
                    else
                    {
                        BindingOperations.ClearBinding(btnNext, Button.IsEnabledProperty);
                        btnNext.IsEnabled = false;
                        RaiseEvent(new RoutedEventArgs(WizardControl.WizardFinishedEvent)); 
                    }
                }
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {

        }

        public void CheckRequired()
        {
            CurrentStepChanged(this, new DependencyPropertyChangedEventArgs(CurrentStepProperty, this.CurrentStep, this.CurrentStep));
        }

        public T GetControl<T>(string name) where T: FrameworkElement
        {
            return Helpers.FindChild<T>(WizardGrid, (fe) => (fe.Name.Equals(name))) as T;
        }    }
}
