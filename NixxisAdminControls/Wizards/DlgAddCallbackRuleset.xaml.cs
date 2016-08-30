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
using Nixxis.Client.Controls;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgAddCallbackRuleset : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddCallbackRuleset");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddCallbackRuleset()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public string GroupKey { get; set; }

        public AdminObject Created { get; internal set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                if (RadioAddCbRuleSet.IsChecked.GetValueOrDefault())
                {
                    sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                }
                else
                {
                    sb.AppendFormat(Translate("Condition: {0} ended with reason {1} at least {2} consecutive times\n"), 
                        chkWasCallback.IsChecked.GetValueOrDefault()?Translate("Callback"): Translate("Call"), 
                        new DialDisconnectionReasonHelper().First ( (a) => (a.EnumValue==(DialDisconnectionReason)cboEndreason.SelectedValue) ).Description,
                        udConsecutive.Value
                        );
                    sb.AppendFormat(Translate("Actions:\n\t{0}{1}\n"), new QualificationActionHelper().First( (a) => (a.EnumValue== (QualificationAction)cboAction.SelectedValue) ).Description , 
                        (QualificationAction)cboAction.SelectedValue == QualificationAction.RetryAt || (QualificationAction)cboAction.SelectedValue== QualificationAction.RetryNotBefore ? string.Concat (" ", DurationHelpers.GetDefaultDurationString(durDelay.Duration, false) ): string.Empty );
                    if (chkResetTargetHandler.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("\tReset target handler\n"));

                }                
                tb.Text = sb.ToString();
            }

        }

        private static void AddRule(AdminCore core, CallbackRuleset cbrs, int sequence, int consecutivecount, DialDisconnectionReason endreason, bool callback,  QualificationAction action, int relativedelay, int validity, bool forceprogressive, int dialingModeOverride, bool loosetarget )
        {
            CallbackRule cb = null;

            cb = core.Create<CallbackRule>();
            cb.Sequence = sequence;
            cb.Action = action;
            cb.Callback = callback;
            cb.ConsecutiveStatusCount = consecutivecount;
            cb.EndReason = endreason;
            cb.ForceProgressive = forceprogressive;
            cb.DialingModeOverride = dialingModeOverride;
            cb.LooseTarget = loosetarget;
            cb.RelativeDelay = relativedelay;
            cb.Validity = validity;
            cb.CallbackRuleset.TargetId = cbrs.Id;
            cbrs.Rules.Add(cb);

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            if (RadioAddCbRuleSet.IsChecked.GetValueOrDefault())
            {
                CallbackRuleset cbrs = core.Create<CallbackRuleset>();
                cbrs.GroupKey = GroupKey;
                cbrs.Description = txtDescription.Text;                
                Created = cbrs;
                cbrs.MaxDialAttemptsLimited = false;
                cbrs.CallbackValidity = 4320;
                if (chkCreateDefaultRules.IsChecked.GetValueOrDefault())
                {
                    AddRule(core, cbrs, 0, 0, DialDisconnectionReason.Fax, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 1, 0, DialDisconnectionReason.Disturbed, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 2, 0, DialDisconnectionReason.NoAnswer, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 3, 0, DialDisconnectionReason.AnsweringMachine, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 4, 0, DialDisconnectionReason.Busy, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 5, 0, DialDisconnectionReason.Abandoned, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 6, 0, DialDisconnectionReason.Congestion, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 7, 0, DialDisconnectionReason.AgentUnavailable, true, QualificationAction.Callback, 120, 1320, false, 0, false);
                    AddRule(core, cbrs, 8, 2, DialDisconnectionReason.Disturbed, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 9, 0, DialDisconnectionReason.Disturbed, false, QualificationAction.RetryNotBefore, 120, 0, false, 0, false);
                    AddRule(core, cbrs, 10, 0, DialDisconnectionReason.Fax, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 11, 10, DialDisconnectionReason.NoAnswer, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 12, 0, DialDisconnectionReason.NoAnswer, false, QualificationAction.RetryNotBefore, 120, 0, false, 0, false);
                    AddRule(core, cbrs, 13, 10, DialDisconnectionReason.AnsweringMachine, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 14, 0, DialDisconnectionReason.AnsweringMachine, false, QualificationAction.RetryNotBefore, 120, 0, false, 0, false);
                    AddRule(core, cbrs, 15, 10, DialDisconnectionReason.Busy, false, QualificationAction.DoNotRetry, 10, 0, false, 0, false);
                    AddRule(core, cbrs, 16, 0, DialDisconnectionReason.Busy, false, QualificationAction.RetryNotBefore, 10, 0, false, 0, false);
                    AddRule(core, cbrs, 17, 0, DialDisconnectionReason.Abandoned, false, QualificationAction.RetryNotBefore, 60, 0, true, 3, false);
                    AddRule(core, cbrs, 18, 10, DialDisconnectionReason.ValidityEllapsed, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 19, 5, DialDisconnectionReason.ValidityEllapsed, false, QualificationAction.Callback, 1320, 4320, false, 0, true);
                    AddRule(core, cbrs, 20, 0, DialDisconnectionReason.ValidityEllapsed, false, QualificationAction.Callback, 1320, 4320, false, 0, false);
                    AddRule(core, cbrs, 21, 0, DialDisconnectionReason.AgentUnavailable, false, QualificationAction.RetryNotBefore, 10, 0, false, 0, false);
                    AddRule(core, cbrs, 22, 2, DialDisconnectionReason.Congestion, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 23, 0, DialDisconnectionReason.Congestion, false, QualificationAction.RetryNotBefore, 120, 0, false, 0, false);
                    AddRule(core, cbrs, 24, 2, DialDisconnectionReason.None, false, QualificationAction.DoNotRetry, 0, 0, false, 0, false);
                    AddRule(core, cbrs, 25, 0, DialDisconnectionReason.None, false, QualificationAction.RetryNotBefore, 120, 0, false, 0, false);
                }
                core.CallbackRulesets.Add(cbrs);
                DialogResult = true;
            }
            else
            {
                CallbackRule cb = core.Create<CallbackRule>();
                CallbackRuleset cbr = (CallbackRuleset)WizControl.Context;

                if (cbr.Rules.Count > 0)
                    cb.Sequence = cbr.Rules.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;


                cb.Action = (QualificationAction)cboAction.SelectedValue;
                cb.Callback = chkWasCallback.IsChecked.GetValueOrDefault();
                cb.ConsecutiveStatusCount = (int)udConsecutive.Value;
                cb.EndReason = (DialDisconnectionReason)cboEndreason.SelectedValue;

                cb.LooseTarget = chkResetTargetHandler.IsChecked.GetValueOrDefault();
                cb.RelativeDelay = (int)(durDelay.Duration / 60);
                cb.Validity = (int)(durValidity.Duration / 60);

                ((CallbackRuleset)WizControl.Context).Rules.Add(cb);

                Created = cb;
                DialogResult = true;
            }

        }
    }
}
