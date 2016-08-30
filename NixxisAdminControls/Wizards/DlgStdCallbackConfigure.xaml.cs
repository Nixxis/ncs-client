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
using System.Windows.Controls.Primitives;
using Nixxis.Client.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgStdCallbackConfigure : Window, IPreprocessorConfigurationDialog
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgStdCallbackConfigure");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        private string m_Settings = null;

        public DlgStdCallbackConfigure()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public string Settings
        {
            get
            {
                return m_Settings;
            }
            set
            {
                m_Settings = value;
                LoadSettings();
            }
        }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb == null)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Translate("Announce: {0}\n"), promptAnnounce.MessageDisplayText);
            sb.AppendFormat(Translate("Request phone: {0}\n"), promptPhone.MessageDisplayText);
            sb.AppendFormat(Translate("Repeat phone: {0}\n"), promptRepeatPhone.MessageDisplayText);
            sb.AppendFormat(Translate("Accept message (1): {0}\n"), promptAcceptPhone.MessageDisplayText);
            sb.AppendFormat(Translate("Re-enter message (2): {0}\n"), promptReenterPhone.MessageDisplayText);
            if (radioAskDate.IsChecked.GetValueOrDefault())
            {
                sb.AppendFormat(Translate("Call ASAP (1): {0}\n"), promptCallAsap.MessageDisplayText);
                sb.AppendFormat(Translate("Call later(2): {0}\n"), promptCallLater.MessageDisplayText);
                sb.AppendFormat(Translate("Ask date: {0}\n"), promptAskDate.MessageDisplayText);
                sb.AppendFormat(Translate("Repeat date/time: {0}\n"), promptRepeatDateTime.MessageDisplayText);
            }
            else if (radioAskTime.IsChecked.GetValueOrDefault())
            {
                sb.AppendFormat(Translate("Call ASAP (1): {0}\n"), promptCallAsap.MessageDisplayText);
                sb.AppendFormat(Translate("Call later(2): {0}\n"), promptCallLater.MessageDisplayText);
                sb.AppendFormat(Translate("Ask time: {0}\n"), promptAskTime.MessageDisplayText);
                sb.AppendFormat(Translate("Repeat date/time: {0}\n"), promptRepeatDateTime.MessageDisplayText);
            }
            else if (radioAskDateTime.IsChecked.GetValueOrDefault())
            {
                sb.AppendFormat(Translate("Call ASAP (1): {0}\n"), promptCallAsap.MessageDisplayText);
                sb.AppendFormat(Translate("Call later(2): {0}\n"), promptCallLater.MessageDisplayText);
                sb.AppendFormat(Translate("Ask date: {0}\n"), promptAskDate.MessageDisplayText);
                sb.AppendFormat(Translate("Ask time: {0}\n"), promptAskTime.MessageDisplayText);
                sb.AppendFormat(Translate("Repeat date/time: {0}\n"), promptRepeatDateTime.MessageDisplayText);
            }

            
            sb.AppendFormat(Translate("Not valid: {0}\n"), promptNotValid.MessageDisplayText);
            sb.AppendFormat(Translate("Thank you: {0}\n"), promptThankYou.MessageDisplayText);
            sb.AppendFormat(Translate("Sorry: {0}\n"), promptSorry.MessageDisplayText);
            
            tb.Text = sb.ToString();
            
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
        }

        private string GetRelativePath(string promptId)
        {
            if (string.IsNullOrEmpty(promptId))
                return string.Empty;

            if (DataContext is AdminCore)
            {
                return (DataContext as AdminCore).Prompts[promptId].RelativePath;
            }
            else if (DataContext is AdminObject)
            {
                return (DataContext as AdminObject).Core.Prompts[promptId].RelativePath;
            }
            throw new NotImplementedException();
        }

        private AdminCore Core
        {
            get
            {
                if (DataContext is AdminCore)
                {
                    return ((AdminCore)DataContext);
                }
                else if (DataContext is AdminObject)
                {
                    return ((AdminObject)DataContext).Core;
                }
                return null;
            }
        }

        private IDictionary<string, string> GetLanguagesAndPrompts(string messageId)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            Prompt p = Core.Prompts[promptAnnounce.MessageId];
            retVal.Add(p.Language.TargetId == null ? string.Empty : p.Language.TargetId, p.RelativePath);
            foreach (Prompt rp in p.RelatedPrompts)
            {
                if (!retVal.ContainsKey(rp.Language.TargetId == null ? string.Empty : rp.Language.TargetId))
                    retVal.Add(rp.Language.TargetId == null ? string.Empty : rp.Language.TargetId, rp.RelativePath);
            }
            return retVal;
        }

        public void SaveSettings()
        {
            if (Config == null)
            {
                m_Config = ((AdminObject)WizControl.Context).Core.Create<CallbackPreprocessorConfig>();
            }

            m_Config.AskDate = radioAskDate.IsChecked.GetValueOrDefault() || radioAskDateTime.IsChecked.GetValueOrDefault();
            m_Config.AskTime = radioAskTime.IsChecked.GetValueOrDefault() || radioAskDateTime.IsChecked.GetValueOrDefault();
            m_Config.EnqueueCallbackRequest = radioEnqueueRequest.IsChecked.GetValueOrDefault();
            m_Config.AlwaysAskPhone = radioAlwaysAskPhoneNum.IsChecked.GetValueOrDefault();

            m_Config.Announce.TargetId = promptAnnounce.MessageId;
            m_Config.AskPhone.TargetId = promptPhone.MessageId;
            m_Config.CallAsap.TargetId = promptCallAsap.MessageId;
            m_Config.CallLater.TargetId = promptCallLater.MessageId;
            m_Config.NotValid.TargetId = promptNotValid.MessageId;
            m_Config.PromptAskDate.TargetId = promptAskDate.MessageId;
            m_Config.PromptAskTime.TargetId = promptAskTime.MessageId;
            m_Config.Accept.TargetId = promptAcceptPhone.MessageId;
            m_Config.ReEnter.TargetId = promptReenterPhone.MessageId;
            m_Config.RepeatDateTime.TargetId = promptRepeatDateTime.MessageId;
            m_Config.RepeatPhone.TargetId = promptRepeatPhone.MessageId;
            m_Config.Sorry.TargetId = promptSorry.MessageId;
            m_Config.ThankYou.TargetId = promptThankYou.MessageId;
        }

        public void LoadSettings()
        {
            if (Config == null)
                return;

            radioAskDateTime.IsChecked = m_Config.AskDate && m_Config.AskTime && !m_Config.EnqueueCallbackRequest;
            radioAskDate.IsChecked = m_Config.AskDate && !m_Config.AskTime && !m_Config.EnqueueCallbackRequest;
            radioAskTime.IsChecked = m_Config.AskTime && !m_Config.AskDate && !m_Config.EnqueueCallbackRequest;
            radioEnqueueRequest.IsChecked = m_Config.EnqueueCallbackRequest;
            radioDoNotAskDate.IsChecked = !m_Config.AskDate && !m_Config.AskTime && !m_Config.EnqueueCallbackRequest;

            radioAlwaysAskPhoneNum.IsChecked = m_Config.AlwaysAskPhone;

            promptAnnounce.MessageId = m_Config.Announce.TargetId;
            promptPhone.MessageId = m_Config.AskPhone.TargetId;
            promptAcceptPhone.MessageId = m_Config.Accept.TargetId;
            promptCallAsap.MessageId = m_Config.CallAsap.TargetId;
            promptCallLater.MessageId = m_Config.CallLater.TargetId;
            promptNotValid.MessageId = m_Config.NotValid.TargetId;
            promptAskDate.MessageId = m_Config.PromptAskDate.TargetId;
            promptAskTime.MessageId = m_Config.PromptAskTime.TargetId;
            promptReenterPhone.MessageId = m_Config.ReEnter.TargetId;
            promptRepeatDateTime.MessageId = m_Config.RepeatDateTime.TargetId;
            promptRepeatPhone.MessageId = m_Config.RepeatPhone.TargetId ;
            promptSorry.MessageId = m_Config.Sorry.TargetId;
            promptThankYou.MessageId = m_Config.ThankYou.TargetId;
        }

        public object WizControlContext
        {
            get
            {
                return WizControl.Context;
            }
            set
            {
                WizControl.Context = value;
            }
        }

        private CallbackPreprocessorConfig m_Config = null;

        public BasePreprocessorConfig Config
        {
            get
            {
                return m_Config;
            }
            set
            {
                if(value is CallbackPreprocessorConfig)
                    m_Config = (CallbackPreprocessorConfig)value;
                LoadSettings();
            }
        }
    }
}
