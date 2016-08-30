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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Net;
using ContactRoute;
using Nixxis.Admin;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgDataManagement : Window
    {
        private string m_FilePath = null;
        private bool m_FilePathIsTemp = false;
        private FilterPartsCollection m_AdvancedFilters = new FilterPartsCollection();

        private static TranslationContext m_TranslationContext = new TranslationContext("DlgManageData");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgDataManagement()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            
            InitializeComponent();

            switch (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
            {
                case ".":
                    cboDecimalSep.SelectedIndex = 0;
                    cboDecimalSepExport.SelectedIndex = 0;
                    break;
                case ",":
                    cboDecimalSep.SelectedIndex = 1;
                    cboDecimalSepExport.SelectedIndex = 1;
                    break;
                default:
                    cboDecimalSep.SelectedIndex = 2;
                    cboDecimalSepExport.SelectedIndex = 2;
                    txtDecimalSeparator.Text = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    txtDecimalSeparatorExport.Text = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    break;
            }
        }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            txtWarning.Text = string.Empty;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                #region Maintenance
                if (radioMaintainDB.IsChecked.GetValueOrDefault())
                {
                    if (chkReorganizeIndexes.IsChecked.GetValueOrDefault())
                        sb.AppendLine(Translate("Reorganize indexes."));
                    if (chkUpdateStatistics.IsChecked.GetValueOrDefault())
                        sb.AppendLine(Translate("Update SQL statistics."));

                    txtWarning.Text = Translate("Applying maintenances actions during production is not recommended as it can affect performances dramatically.");
                }
                #endregion

                #region Recycle
                if (radioRecycle.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoRecycle.IsChecked.GetValueOrDefault()))
                {
                    if (radioRemoveTimeConstraint.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Remove time constraints on {0} records (at maximum).\n"), (Resources["Helper"] as DataManagementHelper).Affected);
                    }
                    else
                    {
                        if (chkRecycleToSpecificActivity.IsChecked.GetValueOrDefault())
                        {
                            sb.AppendFormat(Translate("Recycle {0} records and affect them to {1}.\n"), (Resources["Helper"] as DataManagementHelper).Affected, (cboActivity.SelectedItem as Activity).DisplayText);
                        }
                        else
                        {
                            sb.AppendFormat(Translate("Recycle {0} records.\n"), (Resources["Helper"] as DataManagementHelper).Affected);
                        }
                    }
                }
                #endregion

                #region Re-affect
                else if (radioReAffectCb.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoReAffect.IsChecked.GetValueOrDefault()))
                {
                    if (radioReaffectGlobal.IsChecked.GetValueOrDefault())
                    {
                        sb.AppendFormat(Translate("Remove target of {0} callbacks.\n"), (Resources["Helper"] as DataManagementHelper).Affected);
                    }
                    else
                    {
                        sb.AppendFormat(Translate("Change target of {0} callbacks to {1}.\n"), (Resources["Helper"] as DataManagementHelper).Affected, (cboReaffectToAgent.SelectedItem as Agent).DisplayText);
                    }
                }
                #endregion

                #region Exclude
                else if ( (radioInclude.IsChecked.GetValueOrDefault() && rdioExclude.IsChecked.GetValueOrDefault()) || (radioPreview.IsChecked.GetValueOrDefault() && radioDoInclude.IsChecked.GetValueOrDefault() && rdioExclude.IsChecked.GetValueOrDefault() ) )
                {
                    sb.AppendFormat(Translate("Exclude {0} records.\n"),(Resources["Helper"] as DataManagementHelper).Affected);
                }
                #endregion

                #region Include
                else if ((radioInclude.IsChecked.GetValueOrDefault() && rdioInclude.IsChecked.GetValueOrDefault()) || (radioPreview.IsChecked.GetValueOrDefault() && radioDoInclude.IsChecked.GetValueOrDefault() && rdioInclude.IsChecked.GetValueOrDefault()))
                {
                    sb.AppendFormat(Translate("Include {0} records.\n"), (Resources["Helper"] as DataManagementHelper).Affected);
                }
                #endregion

                #region Export
                else if (radioExport.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoExport.IsChecked.GetValueOrDefault()))
                {
                    sb.AppendFormat(Translate("Export {0} records to {1}.\n"), (Resources["Helper"] as DataManagementHelper).Affected, txtFileExportPath.Text);
                }
                #endregion

                #region Delete
                else if (radioDelete.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoDelete.IsChecked.GetValueOrDefault()))
                {
                    sb.AppendFormat(Translate("Delete {0} records.\n"), (Resources["Helper"] as DataManagementHelper).Affected);
                }
                #endregion

                #region Import
                else if(radioAdd.IsChecked.GetValueOrDefault())
                {
                    if(SaveNeeded)
                        sb.AppendFormat(Translate("Commit all changes to ensure database structure\n"));

                    if (chkSetPreferredAgent.IsChecked.GetValueOrDefault() && cboPreferredAgents.SelectedItems.Count()>0)
                    {
                        sb.AppendFormat(Translate("Set prefered agents:\n"));

                        for(int i=0; i< cboPreferredAgents.SelectedItems.Count(); i++)
                        {
                            AdminObject ao = (AdminObject)cboPreferredAgents.SelectedItemsList[i].ReturnObject;
                            

                            if (ao is Agent)
                            {
                                sb.AppendFormat("\t{0}\n", ao.DisplayText);
                            }
                            else if (ao is Team)
                            {
                                sb.AppendFormat("\tAgents from {0}\n", ao.TypedDisplayText);
                            }
                        }
                    }

                    if(!string.IsNullOrEmpty((string)cboAddToActivity.SelectedValue))
                        sb.AppendFormat(Translate("Add {0} records to database and affect them to {1}.\n"), (Resources["Helper"] as DataManagementHelper).ToAdd, cboAddToActivity.Text);
                    else
                        sb.AppendFormat(Translate("Add {0} records to database.\n"), (Resources["Helper"] as DataManagementHelper).ToAdd);

                    sb.AppendLine(Translate("Mappings used:"));
                    FieldImportSettings fis = Resources["fieldImportSettings"] as FieldImportSettings;
                    bool phoneFound = false;
                    foreach (FieldImportSetting f in fis)
                    {
                        switch (f.Action)
                        {
                            case ImportAction.DoNotImport:
                                break;
                            
                            case ImportAction.ImportInExistingField:
                                if (f.FieldMeaning == UserFieldMeanings.BusinessFax ||
                                    f.FieldMeaning == UserFieldMeanings.BusinessMobileNumber ||
                                    f.FieldMeaning == UserFieldMeanings.BusinessPhoneNumber ||
                                    f.FieldMeaning == UserFieldMeanings.HomeFax ||
                                    f.FieldMeaning == UserFieldMeanings.HomeMobileNumber ||
                                    f.FieldMeaning == UserFieldMeanings.HomePhoneNumber)
                                    phoneFound |= true;
                                sb.AppendFormat("\t{0} -> {1}\n", f.Name, f.FieldDisplayText);
                                break;
                            
                            case ImportAction.CreateNewField:
                                sb.AppendFormat(Translate("\t{0} -> New field {1}\n"), f.Name, f.NewFieldDescription);
                                if (f.NewFieldMeaning == UserFieldMeanings.BusinessFax ||
                                    f.NewFieldMeaning == UserFieldMeanings.BusinessMobileNumber ||
                                    f.NewFieldMeaning == UserFieldMeanings.BusinessPhoneNumber ||
                                    f.NewFieldMeaning == UserFieldMeanings.HomeFax ||
                                    f.NewFieldMeaning == UserFieldMeanings.HomeMobileNumber ||
                                    f.NewFieldMeaning == UserFieldMeanings.HomePhoneNumber)
                                    phoneFound |= true;
                                break;
                        }
                    }

                    if (!phoneFound)
                    {
                        txtWarning.Text = Translate("There is no phone field involved in the data import! Please check if it is the desired behavior.");
                    }

                    sb.AppendLine(Translate("Phone numbers validation:"));

                    if(cboNumFormats.SelectedItem!=null)
                        sb.AppendFormat(Translate("\tPhone format in source: {0}\n"), (cboNumFormats.SelectedItem as NumberFormat).DisplayText);

                    if (chkRemoveNonNumeric.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("\tRemove non numeric characters\n"));
                    else
                        sb.AppendFormat(Translate("\tDo not remove non numeric characters\n"));

                    if (chkRemovePrefix.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("\tRemove prefix {0}\n"), txtPrefixToRemove.Text);
                    else
                        sb.AppendFormat(Translate("\tNo prefix is removed\n"));

                    if (chkAddPrefix.IsChecked.GetValueOrDefault())
                        sb.AppendFormat(Translate("\tAdd prefix {0}\n"), txtPrefixToAdd.Text);
                    else
                        sb.AppendFormat(Translate("\tNo prefix is added\n"));


                    tb.Text = sb.ToString();
                    return;
                }
                #endregion

                else if (radioPreview.IsChecked.GetValueOrDefault() && radioDoNothing.IsChecked.GetValueOrDefault())
                {
                    tb.Text = string.Empty;
                    return;
                }

                bool bNoFilter = true;
                if (chkFilterBatch.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records belongs to batch n° {0}.\n"),(int)nudImportSeq.Value);
                }
                if (chkFilterTag.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records are tagged '{0}'.\n"), txtTag.Text);
                }

                if (chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked)
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records belongs to export n° {0}.\n"), (int)nudExportSeq.Value);
                }
                if (chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked)
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records were not exported yet.\n"));
                }
                if (chkFilterDialingResult.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records latest dial results was {0}.\n"), cboDialingResult.SelectedText);
                }
                if (chkFilterExcluded.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records are excluded.\n"));
                }
                if (chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked)
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records are exportable.\n"));
                }
                if (chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked)
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records have been qualified using negative qualifications.\n"));
                }
                if (chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked)
                {
                    bNoFilter = false;
                    sb.AppendFormat(Translate("Filtered records have been qualified using these qualifications only: {0}.\n"), cboQualificationsFilter.SelectedText);
                }
                if (chkFilterState.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;

                    sb.AppendFormat(Translate("Filtered records states are: {0}.\n"), cboState.SelectedText );

                    if (chkFilterCallbackAgent.IsChecked.GetValueOrDefault() && chkFilterCallbackAgent.Visibility == System.Windows.Visibility.Visible)
                    {
                        sb.AppendFormat(Translate("Targeted callbacks agents are: {0}.\n"), cboAgent.SelectedText);
                    }
                }
                if (chkFilterActivities.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;

                    sb.AppendFormat(Translate("Filtered records activities are: {0}.\n"), cboActivities.SelectedText);
                }
                if (chkFilterDateRange.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;
                    DateTime dtFrom = LastHandlingTimeDtFrom.SelectedDate.Value.Add(LastHandlingTimeTmFrom.SelectedTime);
                    
                    DateTime dtTo = LastHandlingTimeDtTo.SelectedDate.Value.Add(LastHandlingTimeTmTo.SelectedTime);

                    sb.AppendFormat(Translate("Filtered records have been handled by agents between {0} and {1}"), dtFrom, dtTo);
                }
                if(chkFilterAdvanced.IsChecked.GetValueOrDefault())
                {
                    bNoFilter = false;

                    sb.AppendFormat(Translate("Records filtering is based on advanced condition.\n"), cboActivities.SelectedText);

                }
                if(bNoFilter)
                    sb.AppendLine(Translate("No filter was set: all records will be affected!"));

                tb.Text = sb.ToString();
            }
        }

        private XmlDocument AdvancedFilterDocument
        {
            get
            {
                if (!chkFilterAdvanced.IsChecked.GetValueOrDefault())
                    return null;

                if (m_AdvancedFilters.Count == 0)
                    return null;

                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateElement("AdvancedFilter"));
                foreach (FilterPart fp in m_AdvancedFilters.OrderBy((seq) => (seq.Sequence)))
                {
                    fp.SaveNodeForDataManage(doc);
                }
                return doc;
            }
        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {

            DateTime? dtFrom = null;

            DateTime? dtTo = null;
            if (chkFilterDateRange.IsChecked.GetValueOrDefault())
            {
                dtFrom = LastHandlingTimeDtFrom.SelectedDate.Value.Add(LastHandlingTimeTmFrom.SelectedTime);
                dtTo = LastHandlingTimeDtTo.SelectedDate.Value.Add(LastHandlingTimeTmTo.SelectedTime);
            }

            int[] resultArray = null;
            int result = -1;
            WaitScreen ws = new WaitScreen();
            ws.Owner = this;
            ws.Show();
            ConfirmationDialog dlg = new ConfirmationDialog();


            try
            {
                if (radioRecycle.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoRecycle.IsChecked.GetValueOrDefault()))
                {
                    if (radioRemoveTimeConstraint.IsChecked.GetValueOrDefault())
                    {
                        result = ((Campaign)DataContext).DataManageRemoveTimeConstraints(
                            chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                            chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                            chkFilterExcluded.IsChecked.GetValueOrDefault(),
                            chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                            chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                            chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                            chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                            chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                            chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("Time constraints removed on {0} records."), result);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                    else
                    {
                        if (chkRecycleToSpecificActivity.IsChecked.GetValueOrDefault())
                        {
                            result = ((Campaign)DataContext).DataManageRecycle(
                                (string)cboActivity.SelectedValue,
                                chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                                chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                                chkFilterExcluded.IsChecked.GetValueOrDefault(),
                                chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                                chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                                chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                                chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                                chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                                chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                                chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                                dtFrom, dtTo, AdvancedFilterDocument);

                            if (result >= 0)
                                dlg.MessageText = string.Format(Translate("{0} records have been recycled."), result);
                            else
                                dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                        }
                        else
                        {
                            result = ((Campaign)DataContext).DataManageRecycle(
                                null,
                                chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                                chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                                chkFilterExcluded.IsChecked.GetValueOrDefault(),
                                chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                                chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                                chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                                chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                                chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                                chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                            if (result >= 0)
                                dlg.MessageText = string.Format(Translate("{0} records have been recycled."), result);
                            else
                                dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                        }
                    }
                }
                else if (radioMaintainDB.IsChecked.GetValueOrDefault())
                {
                    ((Campaign)DataContext).MaintainDatabase(
                        chkReorganizeIndexes.IsChecked.GetValueOrDefault(), chkUpdateStatistics.IsChecked.GetValueOrDefault(),
                        ((a, b, c) => { ws.Progress = a; ws.Text = b; ws.ProgressDescription = c; Helpers.WaitForPriority(); })
                        );
                    dlg.MessageText = Translate("Maintenance complete.");

                }
                else if (radioReAffectCb.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoReAffect.IsChecked.GetValueOrDefault()))
                {
                    if (radioReaffectGlobal.IsChecked.GetValueOrDefault())
                    {
                        result = ((Campaign)DataContext).DataManageReaffectCB(
                            null,
                            chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                            chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                            chkFilterExcluded.IsChecked.GetValueOrDefault(),
                            chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                            chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                            chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                            chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                            chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                            chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("{0} callbacks have been converted to global callbacks."), result);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                    else
                    {
                        result = ((Campaign)DataContext).DataManageReaffectCB(
                            (string)cboReaffectToAgent.SelectedValue,
                            chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                            chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                            chkFilterExcluded.IsChecked.GetValueOrDefault(),
                            chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                            chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                            chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                            chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                            chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                            chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("{0} callbacks have been affected to {1}."), result, cboReaffectToAgent.Text);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                }
                else if ((radioInclude.IsChecked.GetValueOrDefault() && rdioExclude.IsChecked.GetValueOrDefault()) || (radioPreview.IsChecked.GetValueOrDefault() && radioDoInclude.IsChecked.GetValueOrDefault() && rdioExclude.IsChecked.GetValueOrDefault()))
                {
                    try
                    {
                        result = ((Campaign)DataContext).DataManageExclude(
                            chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                            chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                            chkFilterExcluded.IsChecked.GetValueOrDefault(),
                            chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                            chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                            chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                            chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                            chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                            chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                            chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("{0} records have been excluded."), result);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                    catch (Exception ex)
                    {
                        dlg.MessageText = ex.Message;
                    }
                }
                else if ((radioInclude.IsChecked.GetValueOrDefault() && rdioInclude.IsChecked.GetValueOrDefault()) || (radioPreview.IsChecked.GetValueOrDefault() && radioDoInclude.IsChecked.GetValueOrDefault() && rdioInclude.IsChecked.GetValueOrDefault()))
                {
                    try
                    {
                        result = ((Campaign)DataContext).DataManageInclude(
                                chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                                chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                                chkFilterExcluded.IsChecked.GetValueOrDefault(),
                                chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                                chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                                chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                                chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                                chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                                chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);
                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("{0} records have been included."), result);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                    catch (Exception ex)
                    {
                        dlg.MessageText = ex.Message;
                    }
                }
                else if (radioExport.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoExport.IsChecked.GetValueOrDefault()))
                {
                    dlg.MessageText = string.Empty;

                    List<int> exportablefields = new List<int>();

                    foreach (DataGridBoundColumn lbi in lbExportedFields.NixxisSelectedItems)
                    {
                        exportablefields.Add(lbi.DisplayIndex);
                    }

                    exportablefields.Sort();


                    char separator = ',';
                    switch (cboSepExport.SelectedIndex)
                    {
                        case 0:
                            separator = ',';
                            break;
                        case 1:
                            separator = ';';
                            break;
                        case 2:
                            separator = '\t';
                            break;
                        case 3:
                            separator = string.IsNullOrEmpty(txtSeparatorExport.Text) ? ',' : txtSeparatorExport.Text.First();
                            break;
                    }
                    string decimalSeparator = ".";
                    switch (cboDecimalSepExport.SelectedIndex)
                    {
                        case 0:
                            decimalSeparator = ".";
                            break;
                        case 1:
                            decimalSeparator = ",";
                            break;
                        case 2:
                            decimalSeparator = string.IsNullOrEmpty(txtDecimalSeparatorExport.Text) ? "." : txtDecimalSeparatorExport.Text;
                            break;
                    }

                    string strExportPath = null;
                    bool tempFileUsed = false;
                    if (txtFileExportPath.Text.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) || txtFileExportPath.Text.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        strExportPath = System.IO.Path.GetTempFileName();
                        tempFileUsed = true;
                    }
                    else
                    {
                        strExportPath = txtFileExportPath.Text;
                    }

                    System.Diagnostics.Trace.WriteLine("Export requested...", "Export");
                    result = ((Campaign)DataContext).ExportData(
                        strExportPath,
                        separator,
                        string.IsNullOrEmpty(txtStringDelimiterExport.Text) ? (char)0 : txtStringDelimiterExport.Text.First(),
                        chkFirstLineContainsNamesExport.IsChecked.GetValueOrDefault(),
                        exportablefields,
                        chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                        chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                        chkFilterExcluded.IsChecked.GetValueOrDefault(),
                        chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                        chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                        chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                        chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                        chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                        chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                        chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                        chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                        chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                        dtFrom, dtTo, AdvancedFilterDocument,
                        ((a, b, c) => { ws.Progress = a; ws.Text = b; ws.ProgressDescription = c; Helpers.WaitForPriority(); }),
                        decimalSeparator
                        );

                    if (tempFileUsed)
                    {
                        ws.Progress = -1;
                        ws.Text = Translate("Saving in Excel Format...");
                        ws.ProgressDescription = string.Empty;
                        Helpers.WaitForPriority();

                        try
                        {

                            Nixxis.Client.Admin.ExcelHandler eh = new Nixxis.Client.Admin.ExcelHandler(txtFileExportPath.Text);
                            List<Type> types = new List<Type>();
                            Campaign c = (Campaign)DataContext;
                            foreach (int i in exportablefields)
                            {
                                foreach (Field f in c.Fields)
                                {
                                    if (((Nixxis.Client.Controls.NixxisAdvListBoxItem)(lbExportedFields.Items[i])).Text.Equals(f.Name))
                                    {
                                        if (f.DBType == DBTypes.ActivityString ||
                                            f.DBType == DBTypes.AgentString ||
                                            f.DBType == DBTypes.AreaString ||
                                            f.DBType == DBTypes.QualificationString ||
                                            f.DBType == DBTypes.String ||
                                            f.DBType == DBTypes.Char ||
                                            f.DBType == DBTypes.Unknown)
                                            types.Add(typeof(string));
                                        else if (f.DBType == DBTypes.Boolean)
                                            types.Add(typeof(bool));
                                        else if (f.DBType == DBTypes.Datetime || f.DBType == DBTypes.Time)
                                            types.Add(typeof(DateTime));
                                        else if (f.DBType == DBTypes.Float)
                                            types.Add(typeof(float));
                                        else if (f.DBType == DBTypes.Integer)
                                            types.Add(typeof(int));
                                        else
                                            types.Add(typeof(string));
                                        break;
                                    }
                                }

                            }


                            eh.CreateNewSheet().SaveFromCsv(strExportPath, types);

                            try
                            {
                                System.IO.File.Delete(strExportPath);
                            }
                            catch
                            {
                            }
                        }
                        catch (Exception excelex)
                        {
                            string strText = null;
                            if (excelex is System.Runtime.InteropServices.COMException)
                                strText = string.Format(Translate("Excel is not available. Please ensure it is correctly installed on this machine.\n{0}"), excelex.Message);
                            else
                                strText = excelex.Message;
                            dlg.MessageText = strText;
                        }
                    }

                    System.Diagnostics.Trace.WriteLine(string.Format("Export result = {0}", result), "Export");
                    if (string.IsNullOrEmpty(dlg.MessageText))
                    {
                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("{0} records have been exported."), result);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                }
                else if (radioDelete.IsChecked.GetValueOrDefault() || (radioPreview.IsChecked.GetValueOrDefault() && radioDoDelete.IsChecked.GetValueOrDefault()))
                {
                    try
                    {
                        result = ((Campaign)DataContext).DataManageDelete(
                                chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                                chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                                chkFilterExcluded.IsChecked.GetValueOrDefault(),
                                chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                                chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                                chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                                chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                                chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                                chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                                chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                        if (result >= 0)
                            dlg.MessageText = string.Format(Translate("{0} records have been deleted."), result);
                        else
                            dlg.MessageText = Translate("Unexpected result: no action has been applied.");
                    }
                    catch (Exception ex)
                    {
                        dlg.MessageText = ex.Message;
                    }

                }
                else if (radioAdd.IsChecked.GetValueOrDefault())
                {
                    resultArray = null;
                    try
                    {
                        FieldImportSettings fis = Resources["fieldImportSettings"] as FieldImportSettings;

                        DataTable table = new DataTable("Import");
                        List<string> destinationFields = new List<string>();

                        foreach (FieldImportSetting f in fis)
                        {
                            DataColumn added = null;
                            switch (f.Action)
                            {
                                case ImportAction.DoNotImport:
                                    table.Columns.Add(f.Name, FieldImportSetting.Convert(f.FieldDBType));
                                    table.Columns[table.Columns.Count - 1].ReadOnly = true;
                                    break;
                                case ImportAction.ImportInExistingField:
                                    destinationFields.Add(f.FieldName);
                                    added = table.Columns.Add(f.Name, FieldImportSetting.Convert(f.FieldDBType));
                                    if (f.FieldDBType == DBTypes.String)
                                        added.MaxLength = f.FieldLength;
                                    if (
                                        f.FieldMeaning == UserFieldMeanings.BusinessFax ||
                                        f.FieldMeaning == UserFieldMeanings.BusinessMobileNumber ||
                                        f.FieldMeaning == UserFieldMeanings.BusinessPhoneNumber ||
                                        f.FieldMeaning == UserFieldMeanings.HomeFax ||
                                        f.FieldMeaning == UserFieldMeanings.HomeMobileNumber ||
                                        f.FieldMeaning == UserFieldMeanings.HomePhoneNumber)
                                        added.ExtendedProperties.Add("phonenum", f.FieldMeaning.ToString());
                                    break;
                                case ImportAction.CreateNewField:
                                    UserField newone = f.Campaign.Core.Create<UserField>();
                                    newone.Campaign.TargetId = f.Campaign.Id;
                                    newone.Name = f.NewFieldName;
                                    newone.InitialName = f.NewFieldInitialName;
                                    newone.DBType = f.NewFieldDBType;
                                    newone.Length = f.NewFieldLength;
                                    newone.FieldMeaning = f.NewFieldMeaning;
                                    newone.IsIndexed = f.IsIndexed;
                                    newone.IsUniqueConstraint = f.IsUniqueConstraint;
                                    f.Campaign.UserFields.Add(newone);
                                    destinationFields.Add(f.NewFieldName);
                                    added = table.Columns.Add(f.Name, FieldImportSetting.Convert(f.NewFieldDBType));
                                    if (f.NewFieldDBType == DBTypes.String)
                                        added.MaxLength = f.NewFieldLength;
                                    if (
                                        f.NewFieldMeaning == UserFieldMeanings.BusinessFax ||
                                        f.NewFieldMeaning == UserFieldMeanings.BusinessMobileNumber ||
                                        f.NewFieldMeaning == UserFieldMeanings.BusinessPhoneNumber ||
                                        f.NewFieldMeaning == UserFieldMeanings.HomeFax ||
                                        f.NewFieldMeaning == UserFieldMeanings.HomeMobileNumber ||
                                        f.NewFieldMeaning == UserFieldMeanings.HomePhoneNumber)
                                        added.ExtendedProperties.Add("phonenum", f.NewFieldMeaning.ToString());

                                    break;
                            }
                        }


                        char separator = ',';
                        switch (cboSep.SelectedIndex)
                        {
                            case 0:
                                separator = ',';
                                break;
                            case 1:
                                separator = ';';
                                break;
                            case 2:
                                separator = '\t';
                                break;
                            case 3:
                                separator = string.IsNullOrEmpty(txtSeparator.Text) ? ',' : txtSeparator.Text.First();
                                break;
                        }


                        string decimalSeparator = ".";
                        switch (cboDecimalSep.SelectedIndex)
                        {
                            case 0:
                                decimalSeparator = ".";
                                break;
                            case 1:
                                decimalSeparator = ",";
                                break;
                            case 2:
                                decimalSeparator = string.IsNullOrEmpty(txtDecimalSeparator.Text) ? "." : txtDecimalSeparator.Text;
                                break;
                        }
                        if (SaveNeeded)
                        {
                            ws.Text = Translate("Saving changes...");

                            if (Nixxis.Client.Admin.AdminFrameSet.CommitChanges.CanExecute(null, ((IMainWindow)Application.Current.MainWindow).AdminFrameSet))
                            {
                                Campaign oldCamp = ((Campaign)DataContext);
                                string campId = oldCamp.Id;
                                string numFormatId = cboNumFormats.SelectedValue as string;

                                DataContext = null;
                                oldCamp.FirePropertyChanged("Core");

                                Nixxis.Client.Admin.AdminFrameSet.CommitChanges.Execute(null, ((IMainWindow)Application.Current.MainWindow).AdminFrameSet);

                                DataContext = ((IMainWindow)Application.Current.MainWindow).Core.GetAdminObject(campId);
                                (DataContext as Campaign).FirePropertyChanged("Core");
                                cboNumFormats.SelectedValue = numFormatId;
                            }
                        }


                        List<string> preferredAgents = new List<string>();

                        if (chkSetPreferredAgent.IsChecked.GetValueOrDefault() && cboPreferredAgents.SelectedItems.Count() > 0)
                        {

                            for (int i = 0; i < cboPreferredAgents.SelectedItems.Count(); i++)
                            {
                                AdminObject ao = (AdminObject)cboPreferredAgents.SelectedItemsList[i].ReturnObject;


                                if (ao is Agent)
                                {
                                    preferredAgents.Add(ao.Id);
                                }
                                else if (ao is Team)
                                {
                                    foreach (AgentTeam at in ((Team)ao).Agents)
                                        preferredAgents.Add(at.AgentId);
                                }
                            }
                        }

                        resultArray = ((Campaign)DataContext).ImportData(
                            m_FilePath,
                            separator,
                            string.IsNullOrEmpty(txtStringDelimiter.Text) ? (char)0 : txtStringDelimiter.Text.First(),
                            chkFirstLineContainsNames.IsChecked.GetValueOrDefault(),
                            table,
                            destinationFields,
                            chkUseTag.IsChecked.GetValueOrDefault() ? txtImportTag.Text : null,
                            (string)cboAddToActivity.SelectedValue,
                            chkSetMaxDialAttempts.IsChecked.GetValueOrDefault() ? (int)nudMaxDialAttempts.Value : -1,
                            chkRemoveNonNumeric.IsChecked.GetValueOrDefault(),
                            chkRemovePrefix.IsChecked.GetValueOrDefault() ? txtPrefixToRemove.Text : null,
                            chkAddPrefix.IsChecked.GetValueOrDefault() ? txtPrefixToAdd.Text : null,
                            preferredAgents.Count == 0 ? null : preferredAgents,
                            ((a, b, c) => { ws.Progress = a; ws.Text = b; ws.ProgressDescription = c; Helpers.WaitForPriority(); }),
                            cboNumFormats.SelectedValue as string,
                            decimalSeparator
                            );

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.ToString());
                        resultArray = null;
                    }

                    if (resultArray != null)
                    {
                        if (resultArray[0] > 0)
                            dlg.MessageText = string.Format(Translate("{0} records have been added.\nAdded sequence was {1}."), resultArray[0], resultArray[1]);
                        else
                            dlg.MessageText = string.Format(Translate("{0} records have been added."), resultArray[0]);
                    }
                    else
                        dlg.MessageText = Translate("Unexpected result.");

                }
            }
            catch (WebException webex)
            {
                ConfirmationDialog dlgConf = new ConfirmationDialog();
                dlgConf.MessageText = string.Format(Translate("There was a web exception during the communication with the server. Please verify your network connectivity and DNS.\n({0})"), webex.Message);
                dlgConf.Owner = Application.Current.MainWindow;
                dlgConf.IsInfoDialog = true;
                dlgConf.ShowDialog();
                return;
            }
            finally
            {
                ws.Close();
            }

            if (!radioPreview.IsChecked.GetValueOrDefault() || !radioDoNothing.IsChecked.GetValueOrDefault())
            {
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog().GetValueOrDefault();
            }
            else
            {
                dlg.Close();
            }

            if (!chkKeepOpen.IsChecked.GetValueOrDefault())
                DialogResult = true;
            else
                WizControl.Reset();
                
        }

        private void RefreshPhoneVerification()
        {
            WaitScreen ws = new WaitScreen();
            ws.Owner = this;
            ws.Show();

            try
            {
                FieldImportSettings fis = Resources["fieldImportSettings"] as FieldImportSettings;

                DataTable table = new DataTable("Import");

                foreach (FieldImportSetting f in fis)
                {
                    DataColumn added = null;
                    switch (f.Action)
                    {
                        case ImportAction.DoNotImport:
                            table.Columns.Add(f.Name, FieldImportSetting.Convert(f.FieldDBType));
                            table.Columns[table.Columns.Count - 1].ReadOnly = true;
                            break;
                        case ImportAction.ImportInExistingField:
                            added = table.Columns.Add(f.Name, FieldImportSetting.Convert(f.FieldDBType));
                            if (f.FieldDBType == DBTypes.String)
                                added.MaxLength = f.FieldLength;
                            if (
                                f.FieldMeaning == UserFieldMeanings.BusinessFax ||
                                f.FieldMeaning == UserFieldMeanings.BusinessMobileNumber ||
                                f.FieldMeaning == UserFieldMeanings.BusinessPhoneNumber ||
                                f.FieldMeaning == UserFieldMeanings.HomeFax ||
                                f.FieldMeaning == UserFieldMeanings.HomeMobileNumber ||
                                f.FieldMeaning == UserFieldMeanings.HomePhoneNumber)
                                added.ExtendedProperties.Add("phonenum", f.FieldMeaning.ToString());
                            break;
                        case ImportAction.CreateNewField:
                            added = table.Columns.Add(f.Name, FieldImportSetting.Convert(f.NewFieldDBType));
                            if (f.NewFieldDBType == DBTypes.String)
                                added.MaxLength = f.NewFieldLength;
                            if (
                                f.NewFieldMeaning == UserFieldMeanings.BusinessFax ||
                                f.NewFieldMeaning == UserFieldMeanings.BusinessMobileNumber ||
                                f.NewFieldMeaning == UserFieldMeanings.BusinessPhoneNumber ||
                                f.NewFieldMeaning == UserFieldMeanings.HomeFax ||
                                f.NewFieldMeaning == UserFieldMeanings.HomeMobileNumber ||
                                f.NewFieldMeaning == UserFieldMeanings.HomePhoneNumber)
                                added.ExtendedProperties.Add("phonenum", f.NewFieldMeaning.ToString());

                            break;
                    }
                }


                char separator = ',';
                switch (cboSep.SelectedIndex)
                {
                    case 0:
                        separator = ',';
                        break;
                    case 1:
                        separator = ';';
                        break;
                    case 2:
                        separator = '\t';
                        break;
                    case 3:
                        separator = string.IsNullOrEmpty(txtSeparator.Text) ? ',' : txtSeparator.Text.First();
                        break;
                }

                string decimalSeparator = ".";
                switch (cboDecimalSep.SelectedIndex)
                {
                    case 0:
                        decimalSeparator = ".";
                        break;
                    case 1:
                        decimalSeparator = ",";
                        break;
                    case 2:
                        decimalSeparator = string.IsNullOrEmpty(txtDecimalSeparator.Text) ? "." : txtSeparator.Text;
                        break;
                }


                txtPhoneNumVerification.Text =
                ((Campaign)DataContext).PreviewPhoneNumbers(
                    m_FilePath,
                    m_BytesCountForAddPreview - 1,
                    separator,
                    string.IsNullOrEmpty(txtStringDelimiter.Text) ? (char)0 : txtStringDelimiter.Text.First(),
                    chkFirstLineContainsNames.IsChecked.GetValueOrDefault(),
                    table,
                    chkRemoveNonNumeric.IsChecked.GetValueOrDefault(),
                    chkRemovePrefix.IsChecked.GetValueOrDefault() ? txtPrefixToRemove.Text : null,
                    chkAddPrefix.IsChecked.GetValueOrDefault() ? txtPrefixToAdd.Text : null,
                    cboNumFormats.SelectedValue as string,
                    decimalSeparator
                    );

            }
            catch (WebException webex)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate("There was a web exception during the communication with the server. Please verify your network connectivity and DNS.\n({0})"), webex.Message);
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog();
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            finally
            {
                ws.Close();
            }
        }

        private void RefreshPreview()
        {
            lblPreviewCount.Visibility = System.Windows.Visibility.Hidden;
            DGDataManage.Visibility = System.Windows.Visibility.Hidden;

            DateTime? dtFrom = null;

            DateTime? dtTo = null;
            if (chkFilterDateRange.IsChecked.GetValueOrDefault())
            {
                dtFrom = LastHandlingTimeDtFrom.SelectedDate.Value.Add(LastHandlingTimeTmFrom.SelectedTime);
                dtTo = LastHandlingTimeDtTo.SelectedDate.Value.Add(LastHandlingTimeTmTo.SelectedTime);
            }


            WaitScreen ws = new WaitScreen();
            ws.Owner = this;
            ws.Show();
            try
            {
                (Resources["Helper"] as DataManagementHelper).Affected = ((Campaign)DataContext).GetDataCount(
                    chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                    chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                    chkFilterExcluded.IsChecked.GetValueOrDefault(),
                    chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                    chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                    chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                                chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                                chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                    chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                    chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                    chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);
                lblPreviewCount.Visibility = System.Windows.Visibility.Visible;

                DGDataManage.ItemsSource = null;


                // DO NOT REMOVE!!!! THIS IS A WORKAROUND TO AVOID EXCEPTION WHEN RELOADING THE GRID CONTENT. IF REMOVED:
                /*
Message:       Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index
Source:        mscorlib
Target site:   System.Collections.Generic.List`1.get_Item
Stack trace:      at System.Collections.Generic.List`1.get_Item(Int32 index)
   at System.Windows.Controls.DataGridCellsPanel.VirtualizeChildren(List`1 blockList, IItemContainerGenerator generator)
   at System.Windows.Controls.DataGridCellsPanel.GenerateAndMeasureChildrenForRealizedColumns(Size constraint)
   at System.Windows.Controls.DataGridCellsPanel.MeasureOverride(Size constraint)
   at System.Windows.FrameworkElement.MeasureCore(Size availableSize)
   at System.Windows.UIElement.Measure(Size availableSize)
   at MS.Internal.Helper.MeasureElementWithSingleChild(UIElement element, Size constraint)
   at System.Windows.Controls.ItemsPresenter.MeasureOverride(Size constraint)
   at System.Windows.FrameworkElement.MeasureCore(Size availableSize)
   at System.Windows.UIElement.Measure(Size availableSize)
   at System.Windows.Controls.Grid.MeasureOverride(Size constraint)
   at System.Windows.FrameworkElement.MeasureCore(Size availableSize)
   at System.Windows.UIElement.Measure(Size availableSize)

                 */
                DGDataManage.ItemsSource = "test";

                DataSet ds = ((Campaign)DataContext).GetData(
                    true,
                    chkFilterQualifs.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked ? cboQualificationsFilter.SelectedItemsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : null,
                    chkFilterNegative.IsChecked.GetValueOrDefault() && RestrictOnQualifs.IsChecked,
                    chkFilterExcluded.IsChecked.GetValueOrDefault(),
                    chkFilterExportable.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                    chkFilterBatch.IsChecked.GetValueOrDefault() ? (int)nudImportSeq.Value : int.MinValue,
                    chkFilterTag.IsChecked.GetValueOrDefault() ? txtTag.Text : null,
                                chkFilterExportBatch.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked ? (int)nudExportSeq.Value : int.MinValue,
                                chkFilterNotExported.IsChecked.GetValueOrDefault() && RestrictOnExport.IsChecked,
                    chkFilterDialingResult.IsChecked.GetValueOrDefault() && cboDialingResult.SelectedValue != null ? (IEnumerable<DialDisconnectionReason>)(cboDialingResult.SelectedItems.Select((a) => ((EnumHelper<DialDisconnectionReason>)a.ReturnObject).EnumValue)) : null,
                    chkFilterState.IsChecked.GetValueOrDefault() && cboState.SelectedValue != null ? (IEnumerable<QualificationAction>)(cboState.SelectedItems.Select((a) => ((EnumHelper<QualificationAction>)a.ReturnObject).EnumValue)) : null,
                    chkFilterCallbackAgent.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboAgent.SelectedItems.Select((a) => (((Agent)(a.ReturnObject)).Id)) : null,
                            chkFilterActivities.IsChecked.GetValueOrDefault() ? (IEnumerable<string>)cboActivities.SelectedItems.Select((a) => (((Activity)(a.ReturnObject)).Id)) : null,
                            dtFrom, dtTo, AdvancedFilterDocument);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DGDataManage.ItemsSource = ds.Tables[0].DefaultView;
                }

                DGDataManage.Visibility = System.Windows.Visibility.Visible;

            }
            catch (WebException webex)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(Translate("There was a web exception during the communication with the server. Please verify your network connectivity and DNS.\n({0})"), webex.Message);
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog();
                return;
            }
            finally
            {
                ws.Close();
            }
        }

        private bool SaveNeeded
        {
            get;
            set;
        }

 
        private bool CheckSaveNeeded()
        {

            if (!((Campaign)DataContext).HasBeenLoaded)
            {
                SaveNeeded = true;
                return SaveNeeded;
            }

            FieldImportSettings fis = Resources["fieldImportSettings"] as FieldImportSettings;

            DataTable table = new DataTable("Import");
            List<string> destinationFields = new List<string>();

            foreach (FieldImportSetting f in fis)
            {
                if(f.Action == ImportAction.CreateNewField)
                {
                    SaveNeeded = true;
                    return SaveNeeded;
                }
            }

            System.Xml.XmlDocument tempDoc = new System.Xml.XmlDocument();
            tempDoc.AppendChild(tempDoc.CreateElement("temp"));
            foreach(UserField uf in ((Campaign)DataContext).UserFields)
                uf.Save(tempDoc);

            if (tempDoc.DocumentElement.ChildNodes.Count > 0)
            {
                SaveNeeded = true;
                return SaveNeeded;
            }

            if (((Campaign)DataContext).NumberFormat.IsModified)
            {
                SaveNeeded = true;
                return SaveNeeded;
            }



            SaveNeeded = false;
            return SaveNeeded;
        }

        private void WizControl_WizardStepChanging(object sender, RoutedEventArgs e)
        {
            if (((WizardControl)sender).CurrentStep == "PhoneNumVerification" && ((WizardControl)sender).PreviousStep == "PhoneNumValidation")
            {
                RefreshPhoneVerification();
                if (CheckSaveNeeded())
                    WizardControl.SetNextStep(PhoneNumVerification, "SaveNeededWarning");
                else
                    WizardControl.SetNextStep(PhoneNumVerification, null);
            }
            else if (((WizardControl)sender).CurrentStep == "Preview" && (((WizardControl)sender).PreviousStep == "Filter" || ((WizardControl)sender).PreviousStep == "AdvancedFilter"))
            {
                RefreshPreview();
                if (radioRecycle.IsChecked.GetValueOrDefault())
                {
                    WizardControl.SetNextStep(Preview, "RecycleDetails");
                }
                else if (radioReAffectCb.IsChecked.GetValueOrDefault())
                {
                    WizardControl.SetNextStep(Preview, "ReaffectDetails");
                }
                else if (radioExport.IsChecked.GetValueOrDefault())
                {
                    WizardControl.SetNextStep(Preview, "ExportData");
                }
                else if (radioInclude.IsChecked.GetValueOrDefault())
                {
                    WizardControl.SetNextStep(Preview, "IncludeExclude");
                }
                else if (radioDelete.IsChecked.GetValueOrDefault() )
                {
                    WizardControl.SetNextStep(Preview, null);
                }
                else
                {
                    WizardControl.SetNextStep(Preview, "DoSomethingOnSelection");


                    if (radioPreview.IsChecked.GetValueOrDefault() && chkFilterState.IsChecked.GetValueOrDefault() &&
                        cboState.SelectedItems.Count( (a) => ( ((EnumHelper<QualificationAction>) a.ReturnObject).EnumValue==QualificationAction.TargetedCallback || ((EnumHelper<QualificationAction>) a.ReturnObject).EnumValue==QualificationAction.Callback ) ) > 0)
                    {
                        radioDoReAffect.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        radioDoReAffect.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
            else if (((WizardControl)sender).CurrentStep == "Filter" && ((WizardControl)sender).PreviousStep == "Start")
            {

                if (radioReAffectCb.IsChecked.GetValueOrDefault())
                {
                    chkFilterState.IsChecked = true;
                    foreach (NixxisComboBoxItem ncbi in cboState.Items)
                    {
                        if (((EnumHelper<QualificationAction>)ncbi.ReturnObject).EnumValue == QualificationAction.TargetedCallback)
                            ncbi.IsSelected = true;
                    }
                    chkFilterCallbackAgent.IsChecked = true;
                    chkFilterState.IsEnabled = false;
                    cboState.IsEnabled = false;
                    chkFilterCallbackAgent.IsEnabled = false;
                    WizardControl.SetRequired(cboAgent, true);
                }
                else
                {
                    chkFilterState.IsEnabled = true;
                    cboState.IsEnabled = true;
                    chkFilterCallbackAgent.IsEnabled = true;
                    WizardControl.SetRequired(cboAgent, false);
                }
                if (radioExport.IsChecked.GetValueOrDefault())
                {
                    chkFilterExportable.IsChecked = true;
                    RestrictOnExport.IsChecked = true;
                }
            }
            else if (((WizardControl)sender).CurrentStep == "PreviewToAdd" && ((WizardControl)sender).PreviousStep == "AddData")
            {
                FillAddPreview();
            }
            else if (((WizardControl)sender).CurrentStep == "AddDataOptions" && ((WizardControl)sender).PreviousStep == "SelectColumns")
            {
                FieldImportSettings fis = Resources["fieldImportSettings"] as FieldImportSettings;
                SortedList<string, string> tempSl = new SortedList<string, string>();
                List<string> remarks = new List<string>();
                foreach (FieldImportSetting f in fis)
                {
                    switch (f.Action)
                    {
                        case ImportAction.ImportInExistingField:
                            if (!tempSl.ContainsKey(f.FieldDisplayText))
                                tempSl.Add(f.FieldDisplayText, f.Name);
                            else
                            {
                                remarks.Add(string.Format (Translate("{0} -> {1} (but already linked to {2})"), f.Name, f.FieldDisplayText, tempSl[f.FieldDisplayText]));
                            }
                            break;

                        case ImportAction.CreateNewField:
                            if (!tempSl.ContainsKey(f.NewFieldName))
                                tempSl.Add(f.NewFieldName, f.Name);
                            else
                            {
                                remarks.Add(string.Format(Translate("{0} -> {1} (but already linked to {2})"), f.Name, f.NewFieldName, tempSl[f.NewFieldName]));
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (remarks.Count > 0)
                {
                    AddDataOptions.Visibility = System.Windows.Visibility.Hidden;
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.IsInfoDialog = true;
                    dlg.MessageText = String.Format(Translate("Multiple values are affected to the same field!\n{0}"), string.Join("\n", remarks) );
                    dlg.Owner = this;
                    dlg.ShowDialog();
                    AddDataOptions.Visibility = System.Windows.Visibility.Visible;
                    WizControl.PreviousClick(null, null);
                    
                }
            }
            else if (((WizardControl)sender).CurrentStep == "End")
            {
                chkKeepOpen.IsChecked = false;
            }
            else if (((WizardControl)sender).CurrentStep == "SelectExportColumns" && ((WizardControl)sender).PreviousStep == "ExportData")
            {
                if (lbExportedFields.NixxisSelectedItems.Count == 0)
                {
                    if (string.IsNullOrEmpty(((Campaign)DataContext).ExportFields))
                    {
                        int count = 0;
                        for (int i = lbExportedFields.Items.Count-1; i >= 0; i--)
                        {
                            if (count < 4)
                            {
                                if (!lbExportedFields.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item))
                                    lbExportedFields.NixxisSelectedItems.Add(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item);
                            }
                            else if (count < 47)
                            {
                                if (lbExportedFields.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item))
                                    lbExportedFields.NixxisSelectedItems.Remove(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item);
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    if (lbExportedFields.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item))
                                        lbExportedFields.NixxisSelectedItems.Remove(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item);
                                }
                                else
                                {
                                    if (!lbExportedFields.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item))
                                        lbExportedFields.NixxisSelectedItems.Add(((NixxisAdvListBoxItem)lbExportedFields.Items[i]).Item);
                                }
                            }
                            count++;
                        }
                    }
                    else
                    {
                        IEnumerable<string> tmp = ((Campaign)DataContext).ExportFieldsCollection;

                        foreach (object obj in lbExportedFields.Items)
                        {
                            if ( ((NixxisAdvListBoxItem) obj).Item  is DataGridTextColumn)
                            {
                                if (tmp.Contains((((NixxisAdvListBoxItem)obj).Item as DataGridTextColumn).Header))
                                {
                                    if (!lbExportedFields.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)obj).Item))
                                        lbExportedFields.NixxisSelectedItems.Add(((NixxisAdvListBoxItem)obj).Item);
                                }
                            }
                            else if (((NixxisAdvListBoxItem)obj).Item is DataGridCheckBoxColumn)
                            {
                                if (tmp.Contains((((NixxisAdvListBoxItem)obj).Item as DataGridCheckBoxColumn).Header))
                                {
                                    if (!lbExportedFields.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)obj).Item))
                                        lbExportedFields.NixxisSelectedItems.Add(((NixxisAdvListBoxItem)obj).Item);
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
        }

        private void CheckRequired(object sender, RoutedEventArgs e)
        {
            WizControl.CheckRequired();
        }

        private void CheckRequired(object sender, DependencyPropertyChangedEventArgs e)
        {
            WizControl.CheckRequired();
        }

        private void BrowseForFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = Translate("Supported files|*.csv;*.txt;*.xls;*.xlsx|Text files|*.csv;*.txt|Excel files|*.xls;*.xlsx|All files|*.*"); // Filter files by extension
            dlg.Multiselect = false;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                txtFilePath.Text = dlg.FileName;
                if (txtFilePath.Text.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) || txtFilePath.Text.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    cboSep.SelectedIndex = 1;
                    txtStringDelimiter.Text = "\"";
                    lblSeparator.Visibility = System.Windows.Visibility.Collapsed;
                    cboSep.Visibility = System.Windows.Visibility.Collapsed;
                    lblStringDelimiter.Visibility = System.Windows.Visibility.Collapsed;
                    txtStringDelimiter.Visibility = System.Windows.Visibility.Collapsed;
                    lblSheet.Visibility = System.Windows.Visibility.Visible;
                    cboSheet.Visibility = System.Windows.Visibility.Visible;
                    WaitScreen ws = new WaitScreen();
                    try
                    {
                        ws.Text = Translate("Please wait while opening Excel file...");
                        ws.Owner = this;
                        ws.Show();

                        Nixxis.Client.Admin.ExcelHandler eh = new Nixxis.Client.Admin.ExcelHandler(txtFilePath.Text);
                        cboSheet.ItemsSource = eh.Sheets;
                    }
                    catch (Exception)
                    {
                        ConfirmationDialog dlgException = new ConfirmationDialog();
                        dlgException.IsInfoDialog = true;
                        dlgException.MessageText = Translate("Error while using Excel component.\nVerify that Excel is properly installed on your machine.");
                        dlgException.Owner = this;
                        dlgException.ShowDialog();
                        txtFilePath.Text = string.Empty;
                    }
                    finally
                    {
                        ws.Close();
                    }
                }
                else
                {
                    lblSeparator.Visibility = System.Windows.Visibility.Visible;
                    cboSep.Visibility = System.Windows.Visibility.Visible;
                    lblStringDelimiter.Visibility = System.Windows.Visibility.Visible;
                    txtStringDelimiter.Visibility = System.Windows.Visibility.Visible;
                    lblSheet.Visibility = System.Windows.Visibility.Collapsed;
                    cboSheet.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private long m_BytesCountForAddPreview = -1;

        static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        static private string CleanFieldName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
                return string.Empty;

            string retValue = RemoveDiacritics(rawName);
            retValue = retValue.Trim();

            if ("0123456789".IndexOf(retValue[0]) >= 0)
            {
                retValue = string.Concat('_', retValue);
            }

            string newRetValue = string.Empty;

            foreach (char c in retValue)
            {
                if (c == 65279)
                {
                    // do nothing... There is a ZERO WIDTH NO-BREAK SPACE leading char....
                    // BUG 635
                }
                else if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789".IndexOf(c) < 0)
                {
                    newRetValue = string.Concat(newRetValue, '_');
                }
                else
                {
                    newRetValue = string.Concat(newRetValue, c);
                }
            }
            return newRetValue;
        }

        private DataView GetTopDataViewRows(DataView dv, Int32 n)
        {
            DataTable dt = dv.Table.Clone();

            for (int i = 0; i < n - 1; i++)
            {
                if (i >= dv.Count)
                {
                    break;
                }
                dt.ImportRow(dv[i].Row);
            }
            return new DataView(dt, dv.RowFilter, dv.Sort, dv.RowStateFilter);
        }

        private void FillAddPreview()
        {
            WaitScreen ws = new WaitScreen();
            ws.Owner = this;
            ws.Show();

            try
            {
                lblCountPreviewToAdd.Visibility = System.Windows.Visibility.Hidden;
                DGDataAdd.Visibility = System.Windows.Visibility.Hidden;

                DataSet ds = new DataSet();
                int LineCount = 0;
                DGDataAdd.ItemsSource = null;
                DGDataAdd.ItemsSource = "test";


                FieldImportSettings fis = Resources["fieldImportSettings"] as FieldImportSettings;
                fis.Clear();


                try
                {
                    if (m_FilePathIsTemp)
                        System.IO.File.Delete(m_FilePath);
                }
                catch
                {
                }

                
                if (txtFilePath.Text.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) || txtFilePath.Text.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {

                    m_FilePath = System.IO.Path.GetTempFileName();
                    m_FilePathIsTemp = true;

                    Nixxis.Client.Admin.ExcelHandler.ExcelSheet sheet = cboSheet.SelectedItem as Nixxis.Client.Admin.ExcelHandler.ExcelSheet;
                    sheet.SaveAsCsv(m_FilePath);
                }
                else
                {
                    m_FilePath = txtFilePath.Text;
                    m_FilePathIsTemp = false;
                }


                char separator = ',';
                switch (cboSep.SelectedIndex)
                {
                    case 0:
                        separator = ',';
                        break;
                    case 1:
                        separator = ';';
                        break;
                    case 2:
                        separator = '\t';
                        break;
                    case 3:
                        separator = string.IsNullOrEmpty(txtSeparator.Text) ? ',' : txtSeparator.Text.First();
                        break;
                }
                using (CsvReader R = new CsvReader(File.OpenRead(m_FilePath), Encoding.UTF8, separator, string.IsNullOrEmpty(txtStringDelimiter.Text) ? (char)0 : txtStringDelimiter.Text.First()))
                {
                    string[] Line;

                    if (chkFirstLineContainsNames.IsChecked.GetValueOrDefault())
                    {
                        (Resources["Helper"] as DataManagementHelper).ToAdd = (int)R.GetLineCount() - 1;


                        if ((Line = R.ReadLine()) != null)
                        {
                            DataTable DT = ds.Tables.Add();

                            foreach (string Col in Line)
                            {
                                int nCounter = 0;
                                string tempCol = CleanFieldName(Col);
                                while (DT.Columns.Contains(tempCol))
                                {
                                    nCounter++;
                                    tempCol = String.Concat(CleanFieldName(Col), nCounter.ToString());
                                }

                                DT.Columns.Add(tempCol);
                                fis.Add(new FieldImportSetting(DataContext as Campaign, tempCol));
                            }

                            DataRow DR;
                            LineCount = 0;
                            while ((DR = R.ReadDataRow(DT)) != null && LineCount < 50)
                            {
                                LineCount++;
                                DT.Rows.Add(DR);
                            }
                            m_BytesCountForAddPreview = R.GlobalPosition;
                        }
                    }
                    else
                    {
                        (Resources["Helper"] as DataManagementHelper).ToAdd = (int)R.GetLineCount();

                        if ((Line = R.ReadLine()) != null)
                        {
                            DataTable DT = ds.Tables.Add();

                            for (int i = 0; i < Line.Length; i++)
                            {
                                DT.Columns.Add(string.Format("Col{0}", i));
                                fis.Add(new FieldImportSetting(DataContext as Campaign, string.Format("Col{0}", i)));

                            }

                            R.Reset();
                            DataRow DR;

                            LineCount = 0;
                            while ((DR = R.ReadDataRow(DT)) != null && LineCount < 50)
                            {
                                LineCount++;
                                DT.Rows.Add(DR);
                            }
                            m_BytesCountForAddPreview = R.GlobalPosition;
                        }

                    }

                    DGSelectFields.Columns[0].Width = new DataGridLength(100);
                    DGSelectFields.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);

                }

                if (ds.Tables.Count > 0)
                    DGDataAdd.ItemsSource = ds.Tables[0].DefaultView;


                lblCountPreviewToAdd.Visibility = System.Windows.Visibility.Visible;

                DGDataAdd.Visibility = System.Windows.Visibility.Visible;

                ws.Close();
            }
            catch (Exception ex)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = ex.ToString();
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog();

            }
            finally
            {
                ws.Close();
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if((sender as ComboBox).IsVisible)
                (sender as ComboBox).IsDropDownOpen = true;

        }

        private void BrowseForExportFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = Translate("Supported files|*.csv;*.txt;*.xls;*.xlsx|Text files|*.csv;*.txt|Excel files|*.xls;*.xlsx|All files|*.*"); // Filter files by extension
            dlg.AddExtension = true;
            dlg.CheckFileExists = false;
            dlg.Multiselect = false;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                txtFileExportPath.Text = dlg.FileName;

                if (txtFileExportPath.Text.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) || txtFileExportPath.Text.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    cboSepExport.SelectedIndex = 1;
                    txtStringDelimiterExport.Text = "\"";
                    lblSeparatorExport.Visibility = System.Windows.Visibility.Collapsed;
                    cboSepExport.Visibility = System.Windows.Visibility.Collapsed;
                    lblStringDelimiterExport.Visibility = System.Windows.Visibility.Collapsed;
                    txtStringDelimiterExport.Visibility = System.Windows.Visibility.Collapsed;

                }
                else
                {
                    lblSeparatorExport.Visibility = System.Windows.Visibility.Visible;
                    cboSepExport.Visibility = System.Windows.Visibility.Visible;
                    lblStringDelimiterExport.Visibility = System.Windows.Visibility.Visible;
                    txtStringDelimiterExport.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void lbExportedFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> tmp = new List<string>();
            foreach (object lbi in lbExportedFields.NixxisSelectedItems)
            {
                if (lbi is DataGridCheckBoxColumn)
                {
                    tmp.Add((string)(lbi as DataGridCheckBoxColumn).Header);
                }
                else if (lbi is DataGridTextColumn)
                {
                    tmp.Add((string)(lbi as DataGridTextColumn).Header);
                }
                else
                { 
                }
            }

            ((Campaign)DataContext).ExportFields = string.Join(",", tmp);
        }

        private void RestrictOnQualifs_Checked(object sender, RoutedEventArgs e)
        {
            NixxisDetailedCheckBox ndcb = (NixxisDetailedCheckBox)sender;
        }

        private void MySelf_Closed(object sender, EventArgs e)
        {
            try
            {
                if (m_FilePathIsTemp)
                    System.IO.File.Delete(m_FilePath);
            }
            catch
            {
            }

        }

        private void chkFilterAdvanced_Checked(object sender, RoutedEventArgs e)
        {
            WizardControl.SetNextStep(Filter, "AdvancedFilter");
        }

        private void chkFilterAdvanced_Unchecked(object sender, RoutedEventArgs e)
        {
            WizardControl.SetNextStep(Filter, "Preview");
        }

        private void StringOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithString(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void IntOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithInt(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void DateTimeOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithDate(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void BoolOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = OperatorEnumHelper.CompatibilityWithBool(((OperatorEnumHelper)(e.Item)).EnumValue);
        }

        private void StringOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.String;
        }

        private void IntOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.Integer || ((Field)(e.Item)).DBType == DBTypes.Float;
        }

        private void DateTimeOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.Datetime;
        }

        private void BoolOperands_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Field)(e.Item)).DBType == DBTypes.Boolean;
        }

        private void CombineOperators_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (((EnumHelper<CombineOperator>)(e.Item)).EnumValue != CombineOperator.None);
        }

        public ObservableCollection<FilterPart> AdvancedFilters
        {
            get
            {
                return m_AdvancedFilters;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cboNewFilter.SelectedValue as string))
                return;

            AdminCore core = (DataContext as Campaign).Core;
            FilterPart fp = core.Create(typeof(FilterPart), core, null) as FilterPart;
            fp.Field.TargetId = cboNewFilter.SelectedValue as string;
            fp.Operator = Operator.IsNotNull;
            fp.Campaign.Target = (DataContext as Campaign);

            if (m_AdvancedFilters.Count > 0)
            {
                m_AdvancedFilters.OrderBy((temp) => (temp.Sequence)).Last((temp) => (temp != fp)).CombineOperator = CombineOperator.And;
                fp.Sequence = m_AdvancedFilters.OrderBy((temp) => (temp.Sequence)).Last((temp) => (temp != fp)).Sequence + 1;
            }
            


            fp.CombineOperator = CombineOperator.None;


            m_AdvancedFilters.Add(fp);

            EvaluateButtonsEnableStates();

        }

        private void Move_Down_Click(object sender, RoutedEventArgs e)
        {
            

            int backup = (DGFilter.SelectedItem as FilterPart).Sequence;
            FilterPart next = Next(DGFilter.SelectedItem as FilterPart);
            if (next == null)
                return;

            if ( IsLast(next) )
            {
                (DGFilter.SelectedItem as FilterPart).CombineOperator = CombineOperator.None;
                next.CombineOperator = CombineOperator.And;
            }


            (DGFilter.SelectedItem as FilterPart).Sequence = next.Sequence;
            next.Sequence = backup;



            m_AdvancedFilters.Refresh();

            EvaluateButtonsEnableStates();
        }

        private void Move_Up_Click(object sender, RoutedEventArgs e)
        {



            int backup = (DGFilter.SelectedItem as FilterPart).Sequence;
            FilterPart previous = Previous(DGFilter.SelectedItem as FilterPart);
            if (previous == null)
                return;

            if ( IsLast(DGFilter.SelectedItem as FilterPart))
            {
                //if we move up the last item...
                (DGFilter.SelectedItem as FilterPart).CombineOperator = CombineOperator.And;
                previous.CombineOperator = CombineOperator.None;
            }

            (DGFilter.SelectedItem as FilterPart).Sequence = previous.Sequence;
            previous.Sequence = backup;

            m_AdvancedFilters.Refresh();

            EvaluateButtonsEnableStates();
        }


        public FilterPart Previous(FilterPart part)
        {
            int prevSequence = m_AdvancedFilters.Max((a) => (a.Sequence < part.Sequence ? a.Sequence : -1));
            return m_AdvancedFilters.FirstOrDefault((a) => (a.Sequence == prevSequence));
        }

        public FilterPart Next(FilterPart part)
        {
            int nextSequence = m_AdvancedFilters.Min((a) => (a.Sequence > part.Sequence ? a.Sequence : int.MaxValue));
            return m_AdvancedFilters.FirstOrDefault((a) => (a.Sequence == nextSequence));
        }


        public bool IsLast(FilterPart part)
        {
            return part == m_AdvancedFilters.OrderBy((temp) => (temp.Sequence)).Last();
        }

        public bool IsFirst(FilterPart part)
        {
            return part == m_AdvancedFilters.OrderBy((temp) => (temp.Sequence)).First();
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            m_AdvancedFilters.Remove(DGFilter.SelectedItem as FilterPart);

            if (m_AdvancedFilters.Count > 0)
            {
                if (m_AdvancedFilters.OrderBy((temp) => (temp.Sequence)).Last().CombineOperator != CombineOperator.None)
                    m_AdvancedFilters.OrderBy((temp) => (temp.Sequence)).Last().CombineOperator = CombineOperator.None;
                
            }
        }

        private void EvaluateButtonsEnableStates()
        {
            if (DGFilter.SelectedItem != null)
            {
                btnDelete.IsEnabled = true;
                if (IsLast(DGFilter.SelectedItem as FilterPart) && IsFirst(DGFilter.SelectedItem as FilterPart))
                {
                    btnMoveDown.IsEnabled = false;
                    btnMoveUp.IsEnabled = false;
                }
                else if (IsLast(DGFilter.SelectedItem as FilterPart))
                {
                    btnMoveDown.IsEnabled = false;
                    btnMoveUp.IsEnabled = true;
                }
                else if (IsFirst(DGFilter.SelectedItem as FilterPart))
                {
                    btnMoveUp.IsEnabled = false;
                    btnMoveDown.IsEnabled = true;
                }
                else
                {
                    btnMoveDown.IsEnabled = true;
                    btnMoveUp.IsEnabled = true;
                }
            }
            else
            {
                btnDelete.IsEnabled = false;
                btnMoveDown.IsEnabled = false;
                btnMoveUp.IsEnabled = false;
            }
        }

        private void DGFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EvaluateButtonsEnableStates();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            Campaign camp = DataContext as Campaign;
            if(camp!=null)
            {
                if(camp.IsReadOnly)
                {
                    if( ((EnumHelper<ImportAction>)(e.Item)).EnumValue == ImportAction.CreateNewField)
                    {
                        e.Accepted = false;
                        return;
                    }
                }
            }
            e.Accepted = true;
        }
    }

    public class DataManagementHelper : INotifyPropertyChanged
    {
        private int m_Affected = 0;
        private int m_ToAdd = 0;

        public DataManagementHelper()
        {
        }

        public int Affected
        {
            get
            {
                return m_Affected;
            }
            set
            {
                m_Affected = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Affected"));                
            }
        }
        public int ToAdd
        {
            get
            {
                return m_ToAdd;
            }
            set
            {
                m_ToAdd = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ToAdd"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }



    public enum ImportAction
    {
        DoNotImport,
        ImportInExistingField,
        CreateNewField
    }

    public class FieldImportSetting : INotifyPropertyChanged
    {
        public static Type Convert(DBTypes type)
        {
            switch (type)
            {
                case DBTypes.String:
                    return typeof(string);
                case DBTypes.Boolean:
                    return typeof(bool);
                case DBTypes.Char:
                    return typeof(char);
                case DBTypes.Datetime:
                    return typeof(DateTime);
                case DBTypes.Float:
                    return typeof(double);
                case DBTypes.Integer:
                    return typeof(int);
            }
            return typeof(object);
        }

        public FieldImportSetting(Campaign camp)
        {
            m_Campaign = camp;
        }

        public FieldImportSetting(Campaign camp, string name, string newName): this (camp)
        {
            Name = name;
            if (camp.UserFields.Count == 0)
            {
                try
                {
                    NewFieldName = newName;
                }
                catch
                {
                }
                Action = ImportAction.CreateNewField;

                NewFieldInitialName = name;

                if (name.Equals("phone", StringComparison.OrdinalIgnoreCase) || name.Equals("telephone", StringComparison.OrdinalIgnoreCase) || name.Equals("tel", StringComparison.OrdinalIgnoreCase))
                {
                    NewFieldMeaning = UserFieldMeanings.HomePhoneNumber;
                }
                else if (name.Equals("mobile", StringComparison.OrdinalIgnoreCase) || name.Equals("gsm", StringComparison.OrdinalIgnoreCase))
                {
                    NewFieldMeaning = UserFieldMeanings.HomeMobileNumber;
                }
                else if (name.Equals("id", StringComparison.OrdinalIgnoreCase) || name.Equals("indice", StringComparison.OrdinalIgnoreCase))
                {
                    NewFieldMeaning = UserFieldMeanings.CustomerId;
                }
                else if (name.Equals("city", StringComparison.OrdinalIgnoreCase) || name.Equals("ville", StringComparison.OrdinalIgnoreCase))
                {
                    NewFieldMeaning = UserFieldMeanings.City;
                }
                else if (name.Equals("zip", StringComparison.OrdinalIgnoreCase) || name.Equals("cp", StringComparison.OrdinalIgnoreCase))
                {
                    NewFieldMeaning = UserFieldMeanings.PostalCode;
                }
            }
            else
            {
                UserField foundField = camp.UserFields.FirstOrDefault((a) => (a.InitialName.Equals(name, StringComparison.OrdinalIgnoreCase)));
                if (foundField == null)
                {
                    foundField = camp.UserFields.FirstOrDefault((a) => (a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
                }

                if (foundField != null)
                {
                    Action = ImportAction.ImportInExistingField;
                    FieldId = foundField.Id;
                }

            }
        }

        public FieldImportSetting(Campaign camp, string name)
            : this(camp, name, name)
        {

        }

        private string m_FieldId;
        private Campaign m_Campaign;
        private string m_Name;
        private string m_NewFieldName;
        private string m_NewFieldInitialName;
        private DBTypes m_NewFieldDBType = DBTypes.String;
        private int m_NewFieldLength = -1;
        private bool m_NewFieldUniqueConstraint = false;
        private bool m_NewFieldIndexed = false;
        private UserFieldMeanings m_NewFieldMeaning = UserFieldMeanings.None;
        private ImportAction m_Action = ImportAction.DoNotImport;

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                FirePropertyChanged("Name");
                
            }
        }
        public ImportAction Action
        {
            get
            {
                return m_Action;
            }
            set
            {
                m_Action = value;
                if (value == ImportAction.CreateNewField && NewFieldName ==null && NewFieldInitialName==null)
                {
                    try
                    {
                        NewFieldName = Name;
                    }
                    catch
                    {
                    }
                    NewFieldInitialName = Name;
                }
                FirePropertyChanged("Action");
                FirePropertyChanged("ActionIsImportInFieldAndFieldIsChosen");
            }
        }
        public bool ActionIsImportInFieldAndFieldIsChosen
        {
            get
            {
                return Action == ImportAction.ImportInExistingField && !string.IsNullOrEmpty(FieldId);
            }
        }

        public string NewFieldName
        {
            get
            {
                return m_NewFieldName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if ("0123456789".IndexOf(value[0]) >= 0)
                        throw new NotSupportedException();

                    foreach (char c in value)
                    {
                        if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789".IndexOf(c) < 0)
                            throw new NotSupportedException();
                    }
                }

                if (this.m_Campaign != null)
                {
                    if (this.m_Campaign.UserFields.Select((uf) => (uf.Name)).Contains(value))
                        throw new NotSupportedException();
                }

                if (NewFieldInitialName == null || NewFieldInitialName.Equals(NewFieldName))
                    NewFieldInitialName = value;

                m_NewFieldName = value;
                FirePropertyChanged("NewFieldName");
            }
        }
        public string NewFieldInitialName
        {
            get
            {
                return m_NewFieldInitialName;
            }
            set
            {
                m_NewFieldInitialName = value;
                FirePropertyChanged("NewFieldInitialName");
            }
        }
        public DBTypes NewFieldDBType
        {
            get
            {
                return m_NewFieldDBType;
            }
            set
            {
                m_NewFieldDBType = value;

                if (NewFieldDBType == DBTypes.String && NewFieldLength == -1 && Action == ImportAction.CreateNewField)
                {
                    IsUniqueConstraint = false;
                    IsIndexed = false;                    
                }

                FirePropertyChanged("NewFieldDBType");
                FirePropertyChanged("NewIsIndexAllowed");
            }
        }
        public int NewFieldLength
        {
            get
            {
                return m_NewFieldLength;
            }
            set
            {
                m_NewFieldLength = value;

                if (NewFieldDBType == DBTypes.String && NewFieldLength == -1 && Action == ImportAction.CreateNewField)
                {
                    IsUniqueConstraint = false;
                    IsIndexed = false;                    
                }


                FirePropertyChanged("NewFieldLength");
                FirePropertyChanged("NewIsIndexAllowed");
            }
        }
        public UserFieldMeanings NewFieldMeaning
        {
            get
            {
                return m_NewFieldMeaning;
            }
            set
            {
                m_NewFieldMeaning = value;
                FirePropertyChanged("NewFieldMeaning");
            }
        }
        public string NewFieldDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(NewFieldName);
                if (NewFieldMeaning != UserFieldMeanings.None)
                    sb.AppendFormat(" ({0})", new UserFieldMeaningHelper().First((a) => (a.EnumValue == NewFieldMeaning)).Description);
                if (NewFieldDBType == DBTypes.String)
                {
                    if(NewFieldLength==-1)
                        sb.AppendFormat(" {0}(max)", new DBTypeHelper().First((a) => (a.EnumValue == NewFieldDBType)).Description);
                    else
                        sb.AppendFormat(" {0}({1})", new DBTypeHelper().First((a) => (a.EnumValue == NewFieldDBType)).Description, NewFieldLength);
                }
                else
                {
                    sb.AppendFormat(" {0}", new DBTypeHelper().First((a) => (a.EnumValue == NewFieldDBType)).Description);
                }
                if (m_NewFieldUniqueConstraint)
                    sb.Append(" unique");
                else if (m_NewFieldIndexed)
                    sb.Append(" indexed");

                return sb.ToString();
            }
        }

        public string FieldId
        {
            get
            {
                return m_FieldId;
            }
            set
            {
                m_FieldId = value;
                FirePropertyChanged("FieldId");
                FirePropertyChanged("FieldDisplayText");
                FirePropertyChanged("FieldInitialName");
                FirePropertyChanged("FieldDBType");
                FirePropertyChanged("ActionIsImportInFieldAndFieldIsChosen");
                FirePropertyChanged("FieldLength");
                FirePropertyChanged("FieldMeaning");
                FirePropertyChanged("IsUniqueConstraint");
                FirePropertyChanged("IsIndexed");  
            }
        }
        public string FieldDisplayText
        {
            get
            {
                if (m_FieldId == null)
                    return "<Please select>";
                return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).DisplayText;
            }
        }
        public string FieldName
        {
            get
            {
                if (m_FieldId == null)
                    return null;
                return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).Name;
            }
        }

        public string FieldInitialName
        {
            get
            {
                if (m_FieldId == null)
                    return null;
                return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).InitialName;
            }
        }
        public DBTypes FieldDBType
        {
            get
            {
                if (m_FieldId == null)
                    return DBTypes.String;
                return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).DBType;
            }
        }
        
        public bool NewIsIndexAllowed
        {
            get
            {
                return NewFieldDBType != DBTypes.String || NewFieldLength != -1;
            }
        }
 
        public int FieldLength
        {
            get
            {
                if (m_FieldId == null)
                    return -1;
                return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).Length;
            }
        }
        public UserFieldMeanings FieldMeaning
        {
            get
            {
                if (m_FieldId == null)
                    return UserFieldMeanings.None;
                return (Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))) as UserField).FieldMeaning;
            }
        }
        public Campaign Campaign
        {
            get
            {
                return m_Campaign;
            }
            set
            {
                m_Campaign = value;
            }
        }

        public bool IsUniqueConstraint
        {
            get
            {
                if(Action == ImportAction.ImportInExistingField && m_FieldId != null)
                    return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).IsUniqueConstraint;
                return m_NewFieldUniqueConstraint;
            }
            set
            {
                System.Diagnostics.Debug.Assert(Action == ImportAction.CreateNewField);
                m_NewFieldUniqueConstraint = value;
                IsIndexed = true;
                FirePropertyChanged("IsUniqueConstraint");
            }
        }
        public bool IsIndexed
        {
            get
            {
                if (Action == ImportAction.ImportInExistingField && m_FieldId != null)
                    return Campaign.Fields.First((f) => (f.Id.Equals(m_FieldId))).IsIndexed;
                return m_NewFieldIndexed;
            }
            set
            {
                System.Diagnostics.Debug.Assert(Action == ImportAction.CreateNewField);
                m_NewFieldIndexed = value;
                FirePropertyChanged("IsIndexed");
            }
        }

        private void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class FieldImportSettings : ObservableCollection<FieldImportSetting>
    {
        public FieldImportSettings()
        {
        }
    }

    public class ActionHelper : ObservableCollection<EnumHelper<ImportAction>>
    {
        public ActionHelper()
        {
            Add(new EnumHelper<ImportAction>(ImportAction.DoNotImport, TranslationContext.Default.Translate("Do not import")));
            Add(new EnumHelper<ImportAction>(ImportAction.ImportInExistingField, TranslationContext.Default.Translate("Import in field")));
            Add(new EnumHelper<ImportAction>(ImportAction.CreateNewField, TranslationContext.Default.Translate("Create new field")));
        }
    }

    public class ActionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new ActionHelper().First((a) => (a.EnumValue == (ImportAction)value)).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new ActionHelper().First((a) => (a.Description == (string)value)).EnumValue;
        }
    }

    public class FieldMeaningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new UserFieldMeaningHelper().First((a) => (a.EnumValue == (UserFieldMeanings)value)).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DBTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new DBTypeHelper().First((a) => (a.EnumValue == (DBTypes)value)).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DlgManageDataConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b;
            if(values[0] == null)
                b = true;
            else
                b = (bool)values[0];

            IList<NixxisComboBoxItem> l = (IList<NixxisComboBoxItem>)values[1];

            if (b && l!=null && l.Count((a) => ((a.ReturnObject as EnumHelper<QualificationAction>).EnumValue == QualificationAction.TargetedCallback)) > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilterPartsCollection : ObservableCollection<FilterPart>
    {
        public FilterPartsCollection()
        {
        }

        public void Refresh()
        {
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }
    }

}
