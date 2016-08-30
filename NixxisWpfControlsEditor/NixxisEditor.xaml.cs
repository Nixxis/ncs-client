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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Nixxis.Client.Controls.HtmlHelpers;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for NixxisEditor.xaml  
    /// </summary>
    public partial class NixxisEditor : UserControl
    {
        #region Commands
        static NixxisEditor()
        {
            HtmlEditorActionCut = new RoutedUICommand(string.Empty, "HtmlEditorActionCut", typeof(NixxisEditor));
            HtmlEditorActionCopy = new RoutedUICommand(string.Empty, "HtmlEditorActionCopy", typeof(NixxisEditor));
            HtmlEditorActionPaste = new RoutedUICommand(string.Empty, "HtmlEditorActionPaste", typeof(NixxisEditor));

            HtmlEditorActionBold = new RoutedUICommand(string.Empty, "HtmlEditorActionBold", typeof(NixxisEditor));
            HtmlEditorActionItalic = new RoutedUICommand(string.Empty, "HtmlEditorActionItalic", typeof(NixxisEditor));
            HtmlEditorActionUnderline = new RoutedUICommand(string.Empty, "HtmlEditorActionUnderline", typeof(NixxisEditor));
            HtmlEditorActionSubscript = new RoutedUICommand(string.Empty, "HtmlEditorActionSubscript", typeof(NixxisEditor));
            HtmlEditorActionSuperscript = new RoutedUICommand(string.Empty, "HtmlEditorActionSuperscript", typeof(NixxisEditor));

            HtmlEditorActionNumericList = new RoutedUICommand(string.Empty, "HtmlEditorActionNumericList", typeof(NixxisEditor));
            HtmlEditorActionBubbledList = new RoutedUICommand(string.Empty, "HtmlEditorActionBubbledList", typeof(NixxisEditor));
            HtmlEditorActionIndent = new RoutedUICommand(string.Empty, "HtmlEditorActionIndent", typeof(NixxisEditor));
            HtmlEditorActionOutdent = new RoutedUICommand(string.Empty, "HtmlEditorActionOutdent", typeof(NixxisEditor));

            HtmlEditorActionJustifyLeft = new RoutedUICommand(string.Empty, "HtmlEditorActionJustifyLeft", typeof(NixxisEditor));
            HtmlEditorActionJustifyCenter = new RoutedUICommand(string.Empty, "HtmlEditorActionJustifyCenter", typeof(NixxisEditor));
            HtmlEditorActionJustifyRight = new RoutedUICommand(string.Empty, "HtmlEditorActionJustifyRight", typeof(NixxisEditor));
            HtmlEditorActionJustifyFull = new RoutedUICommand(string.Empty, "HtmlEditorActionJustifyFull", typeof(NixxisEditor));

            HtmlEditorActionLink = new RoutedUICommand(string.Empty, "HtmlEditorActionLink", typeof(NixxisEditor));
            HtmlEditorActionUnLink = new RoutedUICommand(string.Empty, "HtmlEditorActionUnLink", typeof(NixxisEditor));
        }
        //
        //Editing commands
        //
        private void EditingCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_HtmlEditor != null && !DisplayPlainText;
            e.Handled = true;
        }

        public static RoutedUICommand HtmlEditorActionBold { get; set; }
        private void OnExecutedBold(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Bold", false, null);
        }

        public static RoutedUICommand HtmlEditorActionItalic { get; set; }
        private void OnExecutedItalic(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Italic", false, null);
        }

        public static RoutedUICommand HtmlEditorActionUnderline { get; set; }
        private void OnExecutedUnderline(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Underline", false, null);
        }

        public static RoutedUICommand HtmlEditorActionSubscript { get; set; }
        private void OnExecutedSubscript(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Subscript", false, null);
        }

        public static RoutedUICommand HtmlEditorActionSuperscript { get; set; }
        private void OnExecutedSuperscript(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Superscript", false, null);
        }

        public static RoutedUICommand HtmlEditorActionNumericList { get; set; }
        private void OnExecutedNumericList(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.InsertNumericList();
        }

        public static RoutedUICommand HtmlEditorActionBubbledList { get; set; }
        private void OnExecutedBubbledList(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.InsertBubbledList();
        }

        public static RoutedUICommand HtmlEditorActionIndent { get; set; }
        private void OnExecutedIndent(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Indent", false, null);
        }

        public static RoutedUICommand HtmlEditorActionOutdent { get; set; }
        private void OnExecutedOutdent(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Outdent", false, null);
        }

        public static RoutedUICommand HtmlEditorActionJustifyLeft { get; set; }
        private void OnExecutedJustifyLeft(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("JustifyLeft", false, null);
        }

        public static RoutedUICommand HtmlEditorActionJustifyCenter { get; set; }
        private void OnExecutedJustifyCenter(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("JustifyCenter", false, null);
        }

        public static RoutedUICommand HtmlEditorActionJustifyRight { get; set; }
        private void OnExecutedJustifyRight(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("JustifyRight", false, null);
        }

        public static RoutedUICommand HtmlEditorActionJustifyFull { get; set; }
        private void OnExecutedJustifyFull(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("JustifyFull", false, null);
        }

        public static RoutedUICommand HtmlEditorActionLink { get; set; }
        private void OnExecutedLink(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.InsertHyperlink();
        }

        public static RoutedUICommand HtmlEditorActionUnLink { get; set; }
        private void OnExecutedUnLink(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.RemoveHyperlink();
        }

        //
        //Other commands
        //
        public static RoutedUICommand HtmlEditorActionCut { get; set; }
        private void OnExecutedCut(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Cut", false, null);
        }
        private void OnCanExecutedCut(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                !DisplayPlainText &&
                m_HtmlEditor != null &&
                m_HtmlEditor.QueryCommandEnabled("Cut");
        }

        public static RoutedUICommand HtmlEditorActionCopy { get; set; }
        private void OnExecutedCopy(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Copy", false, null);
        }
        private void OnCanExecutedCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                !DisplayPlainText &&
                m_HtmlEditor != null &&
                m_HtmlEditor.QueryCommandEnabled("Cut");
        }

        public static RoutedUICommand HtmlEditorActionPaste { get; set; }
        private void OnExecutedPaste(object sender, ExecutedRoutedEventArgs e)
        {
            m_HtmlEditor.ExecuteCommand("Paste", false, null);
        }
        private void OnCanExecutedPaste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                !DisplayPlainText &&
                m_HtmlEditor != null &&
                m_HtmlEditor.QueryCommandEnabled("Paste");
        }
        #endregion

        #region Enums
        #endregion

        #region Class data
        private NixxisHtmlDocument m_HtmlEditor;
        private DispatcherTimer styleTimer;
        private bool m_UpdatingFontFamily = false;
        private bool m_UpdatingFontSize = false;
        private bool m_IsHtmlTextUpdated = false;
        private bool m_UpdatingHtmlText = false;

        // Each time when charaters is type cursor jump to the end. 
        // Have to see what m_IsHtmlTextUpdated and m_UpdatingHtmlText is used for. (No time now to find out if I can use 1 of these to solve it I create a new one)
        //
        /// <summary>
        /// Is used to indicate that the system is update the htmlText and not an extarnal source. So "SetHtmlText" doesn't need to be done.
        /// </summary>
        private bool m_HtmlTextUpdateBySystem = false;
        #endregion

        #region Constructors
        public NixxisEditor()
        {
            InitializeComponent();

            if (webb.Document == null)
                webb.NavigateToString("<html><body></body></html>");

            m_HtmlEditor = new NixxisHtmlDocument((mshtml.IHTMLDocument2)webb.Document);
            m_HtmlEditor.DesignMode = true;

            InitEditor();
        }
        private void InitEditor()
        {
            Tools.DebugActive = true;
            this.Loaded += new RoutedEventHandler(OnNixxisEditorLoaded);
            this.Unloaded += new RoutedEventHandler(OnNixxisEditorUnloaded);     
            styleTimer = new DispatcherTimer();
            styleTimer.Interval = TimeSpan.FromMilliseconds(200);
            styleTimer.Tick += new EventHandler(OnTimerTick);
        }
        #endregion

        #region Properties XAML
        [Browsable(true)]
        [Description("Set Html Text associated to the control")]
        [Category("Appearance")]
        public string HtmlText
        {
            get { return (string)GetValue(HtmlTextProperty); }
            set { SetValue(HtmlTextProperty, value); }
        }
        public static readonly DependencyProperty HtmlTextProperty = DependencyProperty.Register("HtmlText", typeof(string), typeof(NixxisEditor), new PropertyMetadata("", new PropertyChangedCallback(HtmlTextChanged)));
        public static void HtmlTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisEditor ctrl = (NixxisEditor)obj;

            if (!ctrl.m_HtmlTextUpdateBySystem)
            {
                if (args.NewValue != args.OldValue)
                    ctrl.m_IsHtmlTextUpdated = true;

                ctrl.m_UpdatingHtmlText = true;
                ctrl.SetHtmlText(args.NewValue as string);
                ctrl.m_UpdatingHtmlText = false;
            }
        }

        [Browsable(true)]
        [Description("Select the default text format")]
        [Category("Appearance")]
        public bool DisplayPlainText
        {
            get { return (bool)GetValue(DisplayPlainTextProperty); }
            set { SetValue(DisplayPlainTextProperty, value); }
        }
        public static readonly DependencyProperty DisplayPlainTextProperty = DependencyProperty.Register("DisplayPlainText", typeof(bool), typeof(NixxisEditor), new PropertyMetadata(false, new PropertyChangedCallback(DisplayPlainTextChanged)));
        public static void DisplayPlainTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisEditor ctrl = (NixxisEditor)obj;
        }

        public FontFamily EditorFontFamilySelected
        {
            get { return (FontFamily)GetValue(EditorFontFamilySelectedProperty); }
            set { SetValue(EditorFontFamilySelectedProperty, value); }
        }
        public static readonly DependencyProperty EditorFontFamilySelectedProperty = DependencyProperty.Register("EditorFontFamilySelected", typeof(FontFamily), typeof(NixxisEditor), new PropertyMetadata(new PropertyChangedCallback(EditorFontFamilySelectedChanged)));
        public static void EditorFontFamilySelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisEditor ctrl = (NixxisEditor)obj;
            if(!ctrl.m_UpdatingFontFamily && args.NewValue != null)
                ctrl.SetHtmlEditorFontFamily((FontFamily)args.NewValue);
        }

        public object FontSizeListSource
        {
            get { return (object)GetValue(FontSizeListSourceProperty); }
            set { SetValue(FontSizeListSourceProperty, value); }
        }
        public static readonly DependencyProperty FontSizeListSourceProperty = DependencyProperty.Register("FontSizeListSource", typeof(object), typeof(NixxisEditor));

        public FontSizeItem EditorFontSizeSelected
        {
            get { return (FontSizeItem)GetValue(EditorFontSizeSelectedProperty); }
            set { SetValue(EditorFontSizeSelectedProperty, value); }
        }
        public static readonly DependencyProperty EditorFontSizeSelectedProperty = DependencyProperty.Register("EditorFontSizeSelected", typeof(FontSizeItem), typeof(NixxisEditor), new PropertyMetadata(new PropertyChangedCallback(EditorFontSizeSelectedChanged)));
        public static void EditorFontSizeSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisEditor ctrl = (NixxisEditor)obj;
            if (!ctrl.m_UpdatingFontSize && args.NewValue != null)
                ctrl.SetHtmlEditorFontSize((FontSizeItem)args.NewValue);
        }

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(NixxisEditor), new PropertyMetadata(true, new PropertyChangedCallback(IsEditableChanged)));
        public static void IsEditableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisEditor ctrl = (NixxisEditor)obj;
            ctrl.SetEditable();
        }
        #endregion

        #region Members Override
        public override void EndInit()
        {
            base.EndInit();
            FontSizeListSource = NixxisFont.FontSizeCollection;
        }
        #endregion

        #region Members
        private void OnNixxisEditorLoaded(object sender, RoutedEventArgs e)
        {
            styleTimer.Start();

            if (m_IsHtmlTextUpdated)
                SetHtmlText(this.HtmlText);
        }
        private void OnNixxisEditorUnloaded(object sender, RoutedEventArgs e)
        {
            styleTimer.Stop();
        }
        private void OnTimerTick(object sender, EventArgs e)
        {
            if (m_HtmlEditor.ReadyState != NixxisHtmlDocument.ReadyStates.Complete)
                return;
            
            CommandManager.InvalidateRequerySuggested();
            
            btnBold.IsChecked = m_HtmlEditor.IsBold();
            btnItalic.IsChecked = m_HtmlEditor.IsItalic();
            btnUnderline.IsChecked = m_HtmlEditor.IsUnderline();
            btnSubscript.IsChecked = m_HtmlEditor.IsSubscript();
            btnSuperscript.IsChecked = m_HtmlEditor.IsSuperscript();
            btnBubbledList.IsChecked = m_HtmlEditor.IsBubbledList();
            btnNumericList.IsChecked = m_HtmlEditor.IsNumericList();
            btnJustifyLeft.IsChecked = m_HtmlEditor.IsJustifyLeft();
            btnJustifyCenter.IsChecked = m_HtmlEditor.IsJustifyCenter();
            btnJustifyRight.IsChecked = m_HtmlEditor.IsJustifyRight();
            btnJustifyFull.IsChecked = m_HtmlEditor.IsJustifyFull();

            m_UpdatingFontFamily = true;
            cboFontFamily.SelectedItem = m_HtmlEditor.GetFontFamily();
            m_UpdatingFontFamily = false;

            m_UpdatingFontSize = true;
            cboFontSize.SelectedItem = m_HtmlEditor.GetFontSize();
            m_UpdatingFontSize = false;
            
            Color currentForeColor = m_HtmlEditor.GetForeColor();
            if (ncpForeColor.SelectedItem == null || currentForeColor != ncpForeColor.SelectedItem.Color)
                ncpForeColor.SelectedItem = ncpForeColor.StandardColors.GetForeColor(currentForeColor);

            Color currentBackColor = m_HtmlEditor.GetBackColor();
            if (ncpBackColor.SelectedItem == null || currentBackColor != ncpBackColor.SelectedItem.Color)
                ncpBackColor.SelectedItem = ncpBackColor.StandardColors.GetBackColor(currentBackColor);

            if (IsHtmlTextChanged())
            {
                m_HtmlTextUpdateBySystem = true;
                this.HtmlText = GetHtmlText();
                m_HtmlTextUpdateBySystem = false;
            }
        }

        public void InsertHtmlText(string html)
        {
            try
            {
                if (webb.IsLoaded)
                {
                    m_HtmlEditor.InsertHTML(html);
                }
            }
            catch { }
        }
        private void SetHtmlText(string text)
        {
            try
            {
                if (webb.IsLoaded)
                {
                    m_IsHtmlTextUpdated = false;
                    m_HtmlEditor.SetHtmlText(text);
                }
            }
            catch { }
        }
        public string GetHtmlText()
        {
            try
            {
                if (webb.IsLoaded)
                {
                    return m_HtmlEditor.GetHtmlText(); 
                }
            }
            catch { }

            return string.Empty;
        }
        private bool IsHtmlTextChanged()
        {            
            try
            {
                if (webb.IsLoaded)
                {
                    string currentText = GetHtmlText();
                    if (!m_UpdatingHtmlText && currentText != null && this.HtmlText != currentText)
                        return true;
                }
            }
            catch { }
            return false;
        }
        private void SetHtmlEditorFontFamily(FontFamily value)
        {
            m_HtmlEditor.ExecuteCommand("FontName", false, value.ToString());
        }
        private void SetHtmlEditorFontSize(FontSizeItem value)
        {
            m_HtmlEditor.ExecuteCommand("FontSize", false, value.Key);
        }
        private void SetEditable()
        {
            m_HtmlEditor.DesignMode = IsEditable;
            tbEditorMain.IsEnabled = IsEditable;
            webb.IsEnabled = IsEditable;
        }
        #endregion

        private void webb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            SetHtmlText(HtmlText);
        }

        private void btnBackColor_Click(object sender, RoutedEventArgs e)
        {
            if (ncpBackColor.SelectedItem != null)
                m_HtmlEditor.SetBackColor(ncpBackColor.SelectedItem.Color);
        }

        private void ncpBackColor_ColorSelected(object sender, NixxisColor e)
        {
            if (e != null)
                m_HtmlEditor.SetBackColor(e.Color);
        }

        private void btnForeColor_Click(object sender, RoutedEventArgs e)
        {
            if (ncpForeColor.SelectedItem != null)
                m_HtmlEditor.SetForeColor(ncpForeColor.SelectedItem.Color);
        }

        private void ncpForeColor_ColorSelected(object sender, NixxisColor e)
        {
            if (e != null)
                m_HtmlEditor.SetForeColor(e.Color);
        }

        private void webb_LostFocus(object sender, RoutedEventArgs e)
        {
            this.HtmlText = GetHtmlText();
        }

        private void webb_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.HtmlText = GetHtmlText();
        }

        private void MySelf_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

}
