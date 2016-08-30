using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Net;
using System.Data;
using System.Collections;
using System.Reflection;
using ContactRoute;
using Nixxis.Admin;


namespace Nixxis.Client.Admin
{
    //public enum DialDisconnectionReason
    //{
    //    None = 0,
    //    Fax = 1,
    //    Disturbed = 2,
    //    NoAnswer = 3,
    //    AnsweringMachine = 4,
    //    Busy = 5,
    //    Abandoned = 6,
    //    ValidityEllapsed = 7,
    //    NormalClearing = 8,
    //    Agent = 9,
    //    AgentUnavailable = 10,
    //    Congestion = 11
    //}

    //public enum QualificationAction
    //{
    //    None = 0,
    //    DoNotRetry = 1,
    //    RetryAt = 2,
    //    RetryNotBefore = 3,
    //    Callback = 4,
    //    TargetedCallback = 5,
    //    ChangeActivity = 6,
    //    BlackList = 7
    //}

    public class ServerLinkClient
    {
        private string m_MohSync;
        private string m_SoundsSync;
        private string m_PathMoh;
        private string m_PathConfigs;
        private string m_PathPublicUpload;
        private string m_PathLocalUpload;
        private string m_adminDbPrefix = string.Empty; // TODO
        private string m_Host;


#if CRAPPSERVER
        public static ContactRoute.ApplicationServer.Diagnostics.IMonitoring Monitoring;
#endif

        private static Dictionary<string, Dictionary<string, string>> m_Translations = new Dictionary<string, Dictionary<string, string>>();
        public static void ResetTranslations(string connection)
        {
            #if CRAPPSERVER
            lock (m_Translations)
            {
                m_Translations.Clear();
            }
            #endif
        }
        private static string Translate(string text, string culture, string connection)
        {
#if CRAPPSERVER
            // TODO:
            Dictionary<string, string> dict = null;
            string headLang = culture.ToUpper().Substring(0, 2);
            lock (m_Translations)
            {
                if (!m_Translations.TryGetValue(headLang, out dict))
                {
                    dict = new Dictionary<string, string>();
                    m_Translations.Add(headLang, dict);
                }
            }
            string translation;
            if (!dict.TryGetValue(text, out translation))
            {
                translation = text;
                try
                {
                    using (SqlConnection con = new SqlConnection(connection))
                    {
                        con.Open();
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = string.Format("Select {0} from AgentTranslations where EntryContext = '' and EntryKey = @key", headLang);
                            cmd.Parameters.AddWithValue("@key", text);
                            object obj = cmd.ExecuteScalar();
                            if (obj != null && obj != System.DBNull.Value)
                            {
                                translation = (string)obj;
                            }
                        }
                        con.Close();
                    }
                }
                catch(Exception ex)
                {
                    Trace(ex.ToString(), "ServerLink");
                }
                dict.Add(text, translation);
            }

            return translation;
#else
            return Nixxis.TranslationContext.Default.Translate(text);
#endif
        }

        private static void Trace(string message, string category)
        {
#if CRAPPSERVER
            Monitoring.Log(message, category);
#else
            System.Diagnostics.Trace.WriteLine(message, category);
#endif
        }

        public ServerLinkClient()
        {
#if CRAPPSERVER
#else
            m_MohSync = ConfigurationManager.AppSettings["MohSync"];
            m_SoundsSync = ConfigurationManager.AppSettings["SoundsSync"];

            if(ConfigurationManager.AppSettings["PathConfigs"]!=null)
                m_PathConfigs = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", ConfigurationManager.AppSettings["PathConfigs"]);
            else
                m_PathConfigs = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", AdminCore.PathSounds ,@"\Configs\");

            if (ConfigurationManager.AppSettings["PathMoh"]!=null)
                m_PathMoh =  string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", ConfigurationManager.AppSettings["PathMoh"]);
            else
                m_PathMoh = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", AdminCore.PathSounds, @"\Moh\");

            m_PathPublicUpload = ConfigurationManager.AppSettings["PathUpload"];
            m_PathLocalUpload = ConfigurationManager.AppSettings["PathUpload"];
#endif
        }

        public ServerLinkClient(string mohSync, string soundsSync, string pathMoh, string pathConfigs, string pathPublicUpload, string pathLocalUpload, string host)
        {
            m_MohSync = mohSync;
            m_SoundsSync = soundsSync;
            m_PathConfigs = pathConfigs;
            m_PathMoh = pathMoh;
            m_PathPublicUpload = pathPublicUpload;
            m_PathLocalUpload = pathLocalUpload;
            m_Host = host;
        }

        private static List<string> TablesWithActiveFlag = new List<string>()
        {
            "Activities",
            "Agents",
            "Campaigns",
            "Pauses",
            "Qualifications",
            "Queues",
            "Topics"
        };

        private static List<string> TablesWithNoDelete = new List<string>()
        {
            "InboundActivities",
            "OutboundActivities"
        };


        private static Dictionary<string, List<string>> FieldsMappings = new Dictionary<string, List<string>>()
        {
            {"ActivityLanguage",  new List<string>(){ "ActivitiesLanguages"}},
            {"ActivityLanguage.CreatorId",  new List<string>()},
            {"ActivityLanguage.LastModifiedBy",  new List<string>()},
            {"ActivityLanguage.MinimumLevel",  new List<string>(){ "ActivitiesLanguages.MinLevel"}},
            
            {"ActivitySkill",  new List<string>(){ "ActivitiesSkills"}},
            {"ActivitySkill.CreatorId",  new List<string>()},
            {"ActivitySkill.LastModifiedBy",  new List<string>()},
            {"ActivitySkill.MinimumLevel",  new List<string>(){ "ActivitiesSkills.MinLevel"}},

            {"Agent",  new List<string>() {"Agents"}},
            {"Agent.AgentLevel",  new List<string>()},

            {"AgentLanguage",  new List<string>(){ "AgentsLanguages"}},
            {"AgentLanguage.CreatorId",  new List<string>()},
            {"AgentLanguage.LastModifiedBy",  new List<string>()},            

            {"AgentSkill",  new List<string>(){ "AgentsSkills"}},
            {"AgentSkill.CreatorId",  new List<string>()},
            {"AgentSkill.LastModifiedBy",  new List<string>()},
            
            {"AgentTeam",  new List<string>(){ "AgentsTeams"}},
            {"AgentTeam.CreatorId",  new List<string>()},
            {"AgentTeam.LastModifiedBy",  new List<string>()},

            {"AmdSettings",  new List<string>(){ "AmdSettings"}},

            {"AppointmentsArea.CreatorId",  new List<string>()},
            {"AppointmentsArea.LastModifiedBy",  new List<string>()},

            {"AppointmentsContext.Planning",  new List<string>(){ "AppointmentsContexts.PlanningId"}},

            {"AppointmentsMember.Planning",  new List<string>(){ "AppointmentsMembers.PlanningId"}},
            {"AppointmentsMember.CreatorId",  new List<string>()},
            {"AppointmentsMember.LastModifiedBy",  new List<string>()},

            {"AppointmentsRelation.appointmentsmemberid",  new List<string>(){ "AppointmentsRelations.MemberId"}},
            {"AppointmentsRelation.appointmentsareaid",  new List<string>(){ "AppointmentsRelations.AreaId"}},
            {"AppointmentsRelation.CreatorId",  new List<string>()},
            {"AppointmentsRelation.LastModifiedBy",  new List<string>()},

            {"Attachment.Campaign", new List<string>() {"Attachments.CampaignId"}},
            {"Attachment.CreatorId",  new List<string>()},
            {"Attachment.LastModifiedBy",  new List<string>()},

            {"CallbackRuleset", new List<string>(){ "CallbackRules"}},

            {"CallbackRule", new List<string>(){ "CallbackRuleActions"}},
            {"CallbackRule.CreatorId",  new List<string>()},
            {"CallbackRule.LastModifiedBy",  new List<string>()},
            {"CallbackRule.id", new List<string>()},
            {"CallbackRule.Sequence", new List<string>(){"CallbackRuleActions.-Precedence"}},
            {"CallbackRule.precedenceid", new List<string>(){"CallbackRuleActions.Precedence"}},
            {"CallbackRule.callbackrulesetid", new List<string>(){"CallbackRuleActions.CallbackRule"}},

            {"Carrier",  new List<string>(){ "Carriers"}},


            {"ClosedAction.NoAgentActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"ClosedAction.CreatorId",  new List<string>()},
            {"ClosedAction.LastModifiedBy",  new List<string>()},
            {"ClosedAction.NoAgentParam", new List<string>(){ "ClosedActions.Param"}},
            {"ClosedAction.NoAgentPreprocessorParams", new List<string>(){ "ClosedActions.PreprocessorParams"}},
            {"ClosedAction.NoAgentReroutePrompt", new List<string>(){ "ClosedActions.RerouteParam"}},

            {"ClosedAction.ClosedActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"ClosedAction.ClosedParam", new List<string>(){ "ClosedActions.Param"}},
            {"ClosedAction.ClosedPreprocessorParams", new List<string>(){ "ClosedActions.PreprocessorParams"}},
            {"ClosedAction.ClosedReroutePrompt", new List<string>(){ "ClosedActions.RerouteParam"}},

            {"InboundActivity", new List<string>(){ "InboundActivities" }},
            {"InboundActivity.CreatorId",  new List<string>()},
            {"InboundActivity.LastModifiedBy",  new List<string>()},

            {"InboundActivity.Active", new List<string>(){ "Activities.Active"}},
            {"InboundActivity.RecordingPlaybackLevel", new List<string>(){ "Activities.RecordingPlaybackLevel"}},
            {"InboundActivity.DisableManualQualification", new List<string>(){ "Activities.DisableManualQualification"}},
            {"InboundActivity.WrapupTime", new List<string>(){ "Activities.WrapupTime"}},
            {"InboundActivity.QualificationRequired", new List<string>(){ "Activities.QualificationRequired"}},
            {"InboundActivity.OwnerId", new List<string>(){ "Activities.Owner"}},
            {"InboundActivity.Description", new List<string>(){ "Activities.Description"}},
            {"InboundActivity.AutomaticRecording", new List<string>(){ "Activities.AutomaticRecording"}},
            {"InboundActivity.ListenAllowed", new List<string>(){ "Activities.ListenAllowed"}},
            {"InboundActivity.WaitResource", new List<string>(){ "Activities.WaitResource"}},
            {"InboundActivity.Lines", new List<string>(){ "Activities.Lines" }},
            {"InboundActivity.MediaType", new List<string>(){ "Activities.MediaType"}},
            {"InboundActivity.ScriptUrl", new List<string>(){ "Activities.Script"}},
            {"InboundActivity.Script", new List<string>(){ "Activities.ScriptId"}},
            {"InboundActivity.Campaign", new List<string>(){ "Activities.CampaignId"}},
            {"InboundActivity.Queue", new List<string>(){ "Activities.QueueId"}},
            {"InboundActivity.MusicPrompt", new List<string>(){ "Activities.MusicPrompt"}},
            {"InboundActivity.Preprocessor", new List<string>(){ "Activities.PreprocessorId"}},
            {"InboundActivity.PreprocessorParams", new List<string>(){ "Activities.PreprocessorParams"}},
            {"InboundActivity.Postprocessor", new List<string>(){ "Activities.PostprocessorId"}},
            {"InboundActivity.PostprocessorParams", new List<string>(){ "Activities.PostprocessorParams"}},
            {"InboundActivity.WrapupExtendable", new List<string>(){ "Activities.WrapupExtendable"}},
            {"InboundActivity.TimeOffset", new List<string>(){ "Activities.TimeOffset"}},
            {"InboundActivity.ProviderConfigSettings", new List<string>(){ "Activities.ProviderConfigSettings"}},


            {"InboundActivity.id", new List<string>(){ "InboundActivities.Id", "Activities.Id" }},       
            {"InboundActivity.CallbackActivity", new List<string>(){ "InboundActivities.CallbackActivityId"}},
            {"InboundActivity.OverflowReroutePrompt", new List<string>(){ "InboundActivities.OverflowRerouteParam"}},
            
            {"InboundActivitySpecialDayAction", new List<string>(){ "ClosedActions"}},            
            {"InboundActivitySpecialDayAction.CreatorId",  new List<string>()},
            {"InboundActivitySpecialDayAction.LastModifiedBy",  new List<string>()},
            {"InboundActivitySpecialDayAction.specialdayid", new List<string>(){ "ClosedActions.ClosedReasonId"}},
            {"InboundActivitySpecialDayAction.inboundactivityid", new List<string>(){ "ClosedActions.ActivityId"}},
            {"InboundActivitySpecialDayAction.OverflowActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"InboundActivitySpecialDayAction.OverflowParam", new List<string>(){ "ClosedActions.Param"}},
            {"InboundActivitySpecialDayAction.OverflowPreprocessorParams", new List<string>(){ "ClosedActions.PreprocessorParams"}},
            {"InboundActivitySpecialDayAction.OverflowReroutePrompt", new List<string>(){ "ClosedActions.RerouteParam"}},

            {"InboundActivityTimeSpanAction", new List<string>(){ "ClosedActions"}},
            {"InboundActivityTimeSpanAction.CreatorId",  new List<string>()},
            {"InboundActivityTimeSpanAction.LastModifiedBy",  new List<string>()},
            {"InboundActivityTimeSpanAction.planningtimespanid", new List<string>(){ "ClosedActions.ClosedReasonId"}},
            {"InboundActivityTimeSpanAction.inboundactivityid", new List<string>(){ "ClosedActions.ActivityId"}},
            {"InboundActivityTimeSpanAction.OverflowActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"InboundActivityTimeSpanAction.OverflowParam", new List<string>(){ "ClosedActions.Param"}},
            {"InboundActivityTimeSpanAction.OverflowPreprocessorParams", new List<string>(){ "ClosedActions.PreprocessorParams"}},
            {"InboundActivityTimeSpanAction.OverflowReroutePrompt", new List<string>(){ "ClosedActions.RerouteParam"}},
            
            {"LocationCost.id", new List<string>()},
            {"LocationCost.CreatorId",  new List<string>()},
            {"LocationCost.LastModifiedBy",  new List<string>()},
            {"LocationCost.FromLocation", new List<string>()},
            {"LocationCost.ToLocation", new List<string>()},
            {"LocationCost.fromlocationid", new List<string>(){"LocationCosts.SourceLocation"}},
            {"LocationCost.tolocationid", new List<string>(){"LocationCosts.DestinationLocation"}},

            {"NumberFormat",  new List<string>(){ "NumberFormats"}},


            {"NumberingRule.Sequence", new List<string>(){ "NumberingRules.SortOrder"}},
            {"NumberingRule.NumberingCallType", new List<string>(){ "NumberingRules.CallType"}},


            {"OutboundActivity", new List<string>(){ "OutboundActivities"}},
            {"OutboundActivity.CreatorId",  new List<string>()},
            {"OutboundActivity.LastModifiedBy",  new List<string>()},

            {"OutboundActivity.Active", new List<string>(){ "Activities.Active"}},
            {"OutboundActivity.RecordingPlaybackLevel", new List<string>(){ "Activities.RecordingPlaybackLevel"}},
            {"OutboundActivity.DisableManualQualification", new List<string>(){ "Activities.DisableManualQualification"}},
            {"OutboundActivity.QualificationRequired", new List<string>(){ "Activities.QualificationRequired"}},
            {"OutboundActivity.OwnerId", new List<string>(){ "Activities.Owner"}},
            {"OutboundActivity.Description", new List<string>(){ "Activities.Description"}},
            {"OutboundActivity.AutomaticRecording", new List<string>(){ "Activities.AutomaticRecording"}},
            {"OutboundActivity.ListenAllowed", new List<string>(){ "Activities.ListenAllowed"}},
            {"OutboundActivity.WaitResource", new List<string>(){ "Activities.WaitResource"}},
            {"OutboundActivity.Lines", new List<string>(){ "Activities.Lines" }},
            {"OutboundActivity.Campaign", new List<string>(){ "Activities.CampaignId"}},
            {"OutboundActivity.Queue", new List<string>(){ "Activities.QueueId"}},
            {"OutboundActivity.MediaType", new List<string>(){ "Activities.MediaType"}},
            {"OutboundActivity.ScriptUrl", new List<string>(){ "Activities.Script"}},
            {"OutboundActivity.Script", new List<string>(){ "Activities.ScriptId"}},
            {"OutboundActivity.Preprocessor", new List<string>(){ "Activities.PreprocessorId"}},
            {"OutboundActivity.PreprocessorParams", new List<string>(){ "Activities.PreprocessorParams"}},
            {"OutboundActivity.Postprocessor", new List<string>(){ "Activities.PostprocessorId"}},
            {"OutboundActivity.PostprocessorParams", new List<string>(){ "Activities.PostprocessorParams"}},
            {"OutboundActivity.WrapupTime", new List<string>(){ "Activities.WrapupTime"}},
            {"OutboundActivity.WrapupExtendable", new List<string>(){ "Activities.WrapupExtendable"}},
            {"OutboundActivity.TimeOffset", new List<string>(){ "Activities.TimeOffset"}},
            {"OutboundActivity.ClosedParam", new List<string>()},
            {"OutboundActivity.CurrentActivityFilters", new List<string>()},
            {"OutboundActivity.OrderByFields", new List<string>()},
            {"OutboundActivity.ProviderConfigSettings", new List<string>(){ "Activities.ProviderConfigSettings"}},

            {"OutboundActivity.id", new List<string>(){ "OutboundActivities.Id", "Activities.Id" }},
            {"OutboundActivity.Location", new List<string>(){ "OutboundActivities.LocationId"}},
            {"OutboundActivity.Carrier", new List<string>(){ "OutboundActivities.CarrierId"}},
            {"OutboundActivity.CallbackRules", new List<string>(){ "OutboundActivities.CallbackRuleId"}},
            {"OutboundActivity.AmdSettings", new List<string>(){ "OutboundActivities.AmdSettingsId"}},

            {"OutboundActivitySpecialDayAction", new List<string>(){ "ClosedActions"}},    
            {"OutboundActivitySpecialDayAction.CreatorId",  new List<string>()},
            {"OutboundActivitySpecialDayAction.LastModifiedBy",  new List<string>()},
            {"OutboundActivitySpecialDayAction.specialdayid", new List<string>(){ "ClosedActions.ClosedReasonId"}},
            {"OutboundActivitySpecialDayAction.outboundactivityid", new List<string>(){ "ClosedActions.ActivityId"}},
            {"OutboundActivitySpecialDayAction.OverflowActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"OutboundActivitySpecialDayAction.OverflowParam", new List<string>(){ "ClosedActions.Param"}},
            {"OutboundActivitySpecialDayAction.ClosedActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"OutboundActivitySpecialDayAction.ClosedParam", new List<string>(){ "ClosedActions.Param"}},
            

            {"OutboundActivityTimeSpanAction", new List<string>(){ "ClosedActions"}},
            {"OutboundActivityTimeSpanAction.CreatorId",  new List<string>()},
            {"OutboundActivityTimeSpanAction.LastModifiedBy",  new List<string>()},
            {"OutboundActivityTimeSpanAction.planningtimespanid", new List<string>(){ "ClosedActions.ClosedReasonId"}},
            {"OutboundActivityTimeSpanAction.outboundactivityid", new List<string>(){ "ClosedActions.ActivityId"}},
            {"OutboundActivityTimeSpanAction.OverflowActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"OutboundActivityTimeSpanAction.OverflowParam", new List<string>(){ "ClosedActions.Param"}},
            {"OutboundActivityTimeSpanAction.ClosedActionType", new List<string>(){ "ClosedActions.ActionType"}},
            {"OutboundActivityTimeSpanAction.ClosedParam", new List<string>(){ "ClosedActions.Param"}},


            {"Phone.Location", new List<string>(){ "Phones.LocationId"}},

            {"Planning", new List<string>(){ "Planning"}},

            {"PlanningTimeSpan", new List<string>(){ "PlanningTimeSpans"}},
            {"PlanningTimeSpan.CreatorId",  new List<string>()},
            {"PlanningTimeSpan.LastModifiedBy",  new List<string>()},           

            {"PredefinedText.Campaign", new List<string>() {"PredefinedTexts.CampaignId"}},
            {"PredefinedText.CreatorId",  new List<string>()},
            {"PredefinedText.LastModifiedBy",  new List<string>()},

            {"PromptLink.adminobjectid", new List<string>(){ "PromptsLinks.LinkId"}},
            {"PromptLink.CreatorId",  new List<string>()},
            {"PromptLink.LastModifiedBy",  new List<string>()},
            {"PromptLink.promptid", new List<string>(){ "PromptsLinks.PromptId"}},

            {"QualificationExclusion.activityid", new List<string>(){ "ActivitiesQualificationsExclusions.ActivityId"}},
            {"QualificationExclusion.CreatorId",  new List<string>()},
            {"QualificationExclusion.LastModifiedBy",  new List<string>()},
            {"QualificationExclusion.qualificationid", new List<string>(){ "ActivitiesQualificationsExclusions.QualificationId"}},

            {"Qualification.ParentId", new List<string>(){ "Qualifications.Parent"}},

            {"Queue.OwnerId", new List<string>(){ "Queues.Owner"}},
            {"Queue.OverflowReroutePrompt", new List<string>(){ "Queues.OverflowRerouteParam"}},

            {"QueueTeam", new List<string>(){ "TeamsCrossPoints"}},
            {"QueueTeam.CreatorId",  new List<string>()},
            {"QueueTeam.LastModifiedBy",  new List<string>()},

            {"Resource.Location", new List<string>(){ "Resources.LocationId"}},

            {"SpecialDay.From", new List<string>(){ "SpecialDays.StartRange"}},
            {"SpecialDay.CreatorId",  new List<string>()},
            {"SpecialDay.LastModifiedBy",  new List<string>()},
            {"SpecialDay.To", new List<string>(){ "SpecialDays.EndRange"}},

            {"Team.OwnerId", new List<string>(){ "Teams.Owner"}},

            {"ViewRestriction.id", new List<string>()},
            {"ViewRestriction.CreatorId",  new List<string>()},
            {"ViewRestriction.LastModifiedBy",  new List<string>()},
            {"ViewRestriction.Precedence", new List<string>() {"ViewRestrictions.-Precedence"}},
            {"ViewRestriction.precedenceid", new List<string>(){"ViewRestrictions.Precedence"}},
            {"ViewRestriction.Target", new List<string>(){"ViewRestrictions.TargetId"}}

        };

        public static long ConvertTimeStamp(byte[] timestamp)
        {
            long returnValue = 0;

            returnValue += timestamp[0];
            returnValue = returnValue << 8;
            returnValue += timestamp[1];
            returnValue = returnValue << 8;
            returnValue += timestamp[2];
            returnValue = returnValue << 8;
            returnValue += timestamp[3];
            returnValue = returnValue << 8;
            returnValue += timestamp[4];
            returnValue = returnValue << 8;
            returnValue += timestamp[5];
            returnValue = returnValue << 8;
            returnValue += timestamp[6];
            returnValue = returnValue << 8;
            returnValue += timestamp[7];

            return returnValue;
        }

        public XmlDocument LoadXml(string connectionString, long minTimeStamp)
        {
            System.Xml.XmlDocument Doc = new System.Xml.XmlDocument();

            //SqlConnection C = new SqlConnection("Data Source=t61poli;Initial Catalog=Admin;User Id=sa;Password=nixxis00");
            //SqlConnection C = new SqlConnection("Data Source=t61tom;Initial Catalog=Admin;User Id=sa;Password=nixxis00");

            if (!string.IsNullOrEmpty(connectionString))
            {
                using (SqlConnection C = new SqlConnection(connectionString))
                {

                    C.Open();


                    Doc.AppendChild(Doc.CreateElement("Admin"));

                    XmlNode nde = null;
                    

                    if (minTimeStamp > 0)
                    {
                        using (SqlCommand Cmd = C.CreateCommand())
                        {

                            Cmd.CommandText = "select Target \"*\" from Removed where TimeStamp > @mints FOR XML PATH ('') , ELEMENTS, TYPE, ROOT('Removed')";
                            Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                            try
                            {

                                using (XmlReader Reader = Cmd.ExecuteXmlReader())
                                {
                                    nde = Doc.ReadNode(Reader);
                                    if (nde != null)
                                    {
                                        Doc.DocumentElement.AppendChild(nde);
                                    }
                                }
                            }
                            catch (TargetInvocationException tie)
                            {
                            }

                        }
                    }
                    else
                    {
                        using (SqlCommand Cmd = C.CreateCommand())
                        {

                            Cmd.CommandText = "select name GuiLanguage from syscolumns where id in (select id from sysobjects where xtype = 'U' and name = 'AgentTranslations') and name not in ('EntryContext', 'EntryKey') FOR XML PATH ('') , ELEMENTS, TYPE, ROOT('GuiLanguages')";

                            try
                            {

                                using (XmlReader Reader = Cmd.ExecuteXmlReader())
                                {
                                    nde = Doc.ReadNode(Reader);
                                    if (nde != null)
                                    {
                                        Doc.DocumentElement.AppendChild(nde);
                                    }
                                }
                            }
                            catch (TargetInvocationException tie)
                            {
                            }
                        }
                    }

                    // TODO: include real values...

                    /*XmlDocument tempDoc = new XmlDocument();
                    tempDoc.LoadXml(@"<MediaProviders>
                      <MediaProvider Id=""Pop3"" PluginType=""Nixxis.Client.Admin.Pop3ProviderConfig, MediaProviderPlugins"" MediaType=""4"" >Pop3</MediaProvider>
                      <MediaProvider Id=""Facebook"" PluginType=""Nixxis.Client.Admin.FacebookProviderConfig, MediaProviderPlugins"" MediaType=""4"" >Facebook</MediaProvider>
                    </MediaProviders>");
                    Doc.DocumentElement.AppendChild(Doc.ImportNode(tempDoc.DocumentElement, true));*/

                    #region ============================= Activities -> see InboundActivities and OutboundActivities =============================
                    #endregion

                    #region ============================= ActivitiesLanguages =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select al.ActivityId \"@activityid\", al.LanguageId \"@languageid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", al.Level, al.MinLevel MinimumLevel from ActivitiesLanguages al inner join Activities act on act.id = al.ActivityId where (act.Active is null or act.active = 1) and al.timestamp > @mints FOR XML PATH ('ActivityLanguage'), ELEMENTS{0}, TYPE, ROOT('ActivitiesLanguages')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {
                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }
                    }
                    #endregion

                    #region ============================= ActivitiesQualificationsExclusions =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select aqe.ActivityId \"@activityid\", aqe.QualificationId \"@qualificationid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\" from ActivitiesQualificationsExclusions aqe inner join Activities act on act.id = aqe.ActivityId where ( act.Active is null or act.Active = 1 ) and aqe.timestamp > @mints FOR XML PATH ('QualificationExclusion'), ELEMENTS{0}, TYPE, ROOT('QualificationsExclusions')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {
                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= ActivitiesQueues: TODO =============================
                    #endregion

                    #region ============================= ActivitiesSkills =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select ass.ActivityId \"@activityid\", ass.SkillId \"@skillid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", ass.Level, ass.MinLevel MinimumLevel from ActivitiesSkills ass inner join activities act on act.id = ass.ActivityId where (act.Active is null or act.Active = 1) and ass.timestamp > @mints FOR XML PATH ('ActivitySkill'), ELEMENTS{0}, TYPE, ROOT('ActivitiesSkills')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {
                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Agents =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, Account,  FirstName, LastName, GroupKey, GuiLanguage, ExternalData, AdministrationLevel, SupervisionLevel, ReportingLevel, RecordingPlaybackLevel, WrapupExtendable, WrapupTime, CustomerVisibilityLevel, CallerIdentification, Expert from Agents WHERE (Active is null or Active=1) and timestamp > @mints FOR XML PATH ('Agent'), ELEMENTS{0}, TYPE, ROOT('Agents')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= AgentsLanguages =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select al.AgentId \"@agentid\", al.LanguageId \"@languageid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", al.Level from AgentsLanguages al inner join Agents ag on ag.id = al.AgentId where (ag.Active is null or ag.active = 1) and al.timestamp > @mints FOR XML PATH ('AgentLanguage'), ELEMENTS{0}, TYPE, ROOT('AgentsLanguages')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= AgentsSkills =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select ass.AgentId \"@agentid\", ass.SkillId \"@skillid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", ass.Level from AgentsSkills ass inner join Agents ag on ag.id = ass.AgentId where (ag.Active is null or ag.active = 1) and ass.timestamp > @mints FOR XML PATH ('AgentSkill'), ELEMENTS{0}, TYPE, ROOT('AgentsSkills')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= AgentsTeams =============================

                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select at.AgentId \"@agentid\", at.TeamId \"@teamid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", at.BaseLevel  from AgentsTeams at inner join Agents ag on ag.id = at.AgentId where (ag.Active is null or ag.Active=1) and at.timestamp > @mints FOR XML PATH ('AgentTeam'), ELEMENTS{0}, TYPE, ROOT('AgentsTeams')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= AmdSettings =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, ServerUrl, InitialSilence, Greeting, AfterGreetingSilence, TotalAnalysisTime, MinimumWordLength, BetweenWordsSilence, MaximumNumberOfWords, SilenceTreshold, DropMachine, DropUnsure, PromptMachine, PromptHuman, PromptUnsure, DropHuman from AmdSettings where timestamp > @mints FOR XML PATH ('AmdSettings'), ELEMENTS{0}, TYPE, ROOT('AmdSettings')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= AppointmentsAreas =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", Context \"@context\",  null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", Description, Context, Area, AssociatedFieldMeaning, Sequence, MaxAppointments, MaxConcurentAppointments, AreaWithoutMembers from AppointmentsAreas where Context in (select id from AppointmentsContexts ) and timestamp > @mints FOR XML PATH ('AppointmentsArea'), ELEMENTS{0}, TYPE, ROOT('AppointmentsAreas')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= AppointmentsContexts =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, Code,  BaseUri,  PlanningId Planning, StartTime, EndTime, Granularity, InitialDelay, DefaultAppointmentDuration, AllowedDays, OrderingRule, OrderingRuleNumberOfDays, OrderingRuleTargetFillFactor from AppointmentsContexts where timestamp > @mints FOR XML PATH ('AppointmentsContext'), ELEMENTS{0}, TYPE, ROOT('AppointmentsContexts')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= AppointmentsLanguages: TODO? =============================
                    #endregion

                    #region ============================= AppointmentsMemberLanguages: TODO? =============================
                    #endregion

                    #region ============================= AppointmentsMembers =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", Context \"@context\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", Description, Context, MailboxId, Password, PlanningId Planning from AppointmentsMembers where Context in (select id from AppointmentsContexts ) and timestamp > @mints FOR XML PATH ('AppointmentsMember'), ELEMENTS{0}, TYPE, ROOT('AppointmentsMembers')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= AppointmentsMemberSkill: TODO? =============================
                    #endregion

                    #region ============================= AgentsSkills =============================
                    #endregion

                    #region ============================= AppointmentsRelations: TODO: clear potential duplicates in this table... =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select 1 \"@active\", MemberId \"@appointmentsmemberid\", AreaId \"@appointmentsareaid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\" from AppointmentsRelations where memberid in (select id from AppointmentsMembers where Context in (select id from AppointmentsContexts)) and timestamp > @mints and areaid in (select id from AppointmentsAreas where Context in (select id from AppointmentsContexts)) FOR XML PATH ('AppointmentsRelation'), ELEMENTS{0}, TYPE, ROOT('AppointmentsRelations')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }

                    #endregion

                    #region ============================= AppointmentsSkills: TODO? =============================
                    #endregion

                    #region ============================= Attachments =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", CampaignId \"@campaignid\", Description, CampaignId Campaign, CompatibleMedias, Language, Location, Target, LocationIsLocal, InlineDisposition, Sequence from Attachments where timestamp > @mints FOR XML PATH ('Attachment'), ELEMENTS{0}, TYPE, ROOT('Attachments')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }

                    #endregion

                    #region ============================= CallbackRulesAction =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select 1 \"@active\", CallbackRule \"@callbackrule\" , null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", CallbackRule CallbackRuleset, Precedence Sequence, ConsecutiveStatusCount, EndReason, Callback, Action, RelativeDelay, FixedTime, Validity, ForceProgressive, DialingModeOverride, LooseTarget from CallbackRuleActions where timestamp > @mints FOR XML PATH ('CallbackRule'), ELEMENTS{0}, TYPE, ROOT('CallbackRules')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= CallbackRules =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, CallbackValidity, MaxDialAttempts from CallbackRules where timestamp > @mints FOR XML PATH ('CallbackRuleset'), ELEMENTS{0}, TYPE, ROOT('CallbackRulesets')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Campaigns =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Advanced, Description, GroupKey, FieldsConfig, CustomConfig, ExternalData, ScriptDefinition, Qualification, AppointmentsContext, ImportExportPlugin, QualificationLinked, ExportFields, NumberFormat, AutomaticRecording from Campaigns where active = 1 and timestamp > @mints FOR XML PATH ('Campaign'), ELEMENTS{0}, TYPE, ROOT('Campaigns')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);



                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= Carriers =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, Prefix, Code from Carriers where timestamp > @mints FOR XML PATH ('Carrier'), ELEMENTS{0}, TYPE, ROOT('Carriers')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= ClosedActions: see InboundActivityTimeSpanActions and OutboundActivityTimeSpanActions. the default closed actions are joined with inbound (and outbound) activities and are not loaded here =============================
                    #endregion

                    #region ============================= InboundActivities =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select act.Id \"@id\", act.CampaignId \"@campaignid\", Owner \"@owner\", act.Active \"@active\", act.CreatorId \"@creator\", act.CreationDate \"@created\", act.LastModifiedBy \"@modificator\", act.LastModification \"@modified\", act.CampaignId Campaign, act.ProviderConfigSettings, act.Description, act.GroupKey, act.MediaType, act.PreprocessorId Preprocessor, act.PreprocessorParams, inact.OverflowRerouteParam OverflowReroutePrompt, inact.OverflowPreprocessorParams, act.QueueId Queue, act.WaitResource,  act.Script ScriptUrl, act.ScriptId Script,  act.WrapupTime, act.WrapupExtendable, act.AutomaticRecording, act.ListenAllowed, act.QualificationRequired, act.PostprocessorId Postprocessor, act.PostprocessorParams, act.MusicPrompt, act.Lines, act.RecordingPlaybackLevel, act.DisableManualQualification, act.TimeOffset, inact.Destination, inact.Reject, inact.Ring, inact.TransmitEWT, inact.TransmitPosition, inact.WaitMusicLength, inact.PreprocessorReplacesSkills, inact.PreprocessorReplacesLanguages, inact.InitialProfit, inact.CallbackActivityId CallbackActivity, inact.QueueMusicDelay, inact.AlternateInitialProfit, inact.AlternateInitialProfitRule, inact.OverflowPrompt, inact.OverflowPromptStartingLoop, inact.OverflowActiveDTMFs, inact.OverflowActionType, inact.OverflowParam, inact.PromptForOverflow, inact.SlaPercentageHandledInTime, inact.SlaPercentageToHandle, inact.SlaTime, inact.UsePreferredAgent, inact.PreferredAgentQueueTime, inact.PreferredAgentValidity, ca.ActionType ClosedActionType, ca.Param ClosedParam, ca.PreprocessorParams ClosedPreprocessorParams, ca.RerouteParam ClosedReroutePrompt, cae.ActionType NoAgentActionType, cae.Param NoAgentParam, cae.PreprocessorParams NoAgentPreprocessorParams, cae.RerouteParam NoAgentReroutePrompt, ca.PlanningId Planning from Activities act inner join InboundActivities inact on inact.Id = act.Id left join ClosedActions ca on ca.ActivityId = act.Id and ca.ClosedReasonId is null and ca.PlanningId is not null left join ClosedActions cae on cae.ActivityId = act.Id and cae.ClosedReasonId is null and cae.PlanningId is null where ( act.active = 1 or act.Active is null) and (act.timestamp > @mints or inact.timestamp > @mints or (ca.timestamp is not null and ca.timestamp > @mints ) or ( cae.timestamp is not null and cae.timestamp > @mints ) ) FOR XML PATH ('InboundActivity'), ELEMENTS{0}, TYPE, ROOT('InboundActivities')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= InboundActivityTimeSpanActions: the default closed actions are joined with inbound (and outbound) activities and are not loaded here =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select 1 \"@active\", ClosedReasonId \"@planningtimespanid\", ActivityId \"@inboundactivityid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", PlanningId, ClosedReasonId, ActivityId, ActionType OverflowActionType, Param OverflowParam, PreprocessorParams OverflowPreprocessorParams, RerouteParam OverflowReroutePrompt from ClosedActions where timestamp > @mints and ActivityId in (select Id from Activities where Active is null or Active = 1) and ClosedReasonId is not null and ActivityId in (select id from InboundActivities) and ClosedReasonId not in (select id from specialdays) and ClosedReasonId in (select id from planningtimespans where DaysOfWeek=0) FOR XML PATH ('InboundActivityTimeSpanAction'), ELEMENTS{0}, TYPE, ROOT('InboundActivityTimeSpanActions')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= InboundActivitySpecialDayActions =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select 1 \"@active\", ClosedReasonId \"@specialdayid\", ActivityId \"@inboundactivityid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", PlanningId, ClosedReasonId, ActivityId, ActionType OverflowActionType, Param OverflowParam, PreprocessorParams OverflowPreprocessorParams, RerouteParam OverflowReroutePrompt from ClosedActions where timestamp > @mints and ActivityId in (select id from activities where Active is null or Active = 1) and ClosedReasonId is not null and ActivityId in (select id from InboundActivities) and ClosedReasonId in (select id from specialdays) FOR XML PATH ('InboundActivitySpecialDayAction'), ELEMENTS{0}, TYPE, ROOT('InboundActivitySpecialDayActions')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Languages =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, ExternalData from Languages where timestamp > @mints FOR XML PATH ('Language'), ELEMENTS{0}, TYPE, ROOT('Languages')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= LocationCosts =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select  1 \"@active\", SourceLocation \"@fromlocation\", DestinationLocation \"@tolocation\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", SourceLocation FromLocation, DestinationLocation ToLocation, Cost from LocationCosts where timestamp > @mints FOR XML PATH ('LocationCost'), ELEMENTS{0}, TYPE, ROOT('LocationCosts')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Locations =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, DefaultCost, NumberFormat from Locations where timestamp > @mints FOR XML PATH ('Location'), ELEMENTS{0}, TYPE, ROOT('Locations')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= NumberFormats =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, InternationalDirectDialing, CountryCode, TrunkPrefix, HandlerType from NumberFormats where timestamp > @mints FOR XML PATH ('NumberFormat'), ELEMENTS{0}, TYPE, ROOT('NumberFormats')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= NumberingRules =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", LocationId \"@locationid\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, CallType NumberingCallType, SourceIsRegexp, DestinationIsRegexp, Source, Destination, SourceReplace, DestinationReplace, Carrier , CarrierSelection, LocationId, Description, SortOrder Sequence, Allowed from NumberingRules where timestamp > @mints FOR XML PATH ('NumberingRule'), ELEMENTS{0}, TYPE, ROOT('NumberingRules')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= OutboundActivities =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select act.Id \"@id\", act.CampaignId \"@campaignid\", Owner \"@owner\", act.Active \"@active\", act.CreatorId \"@creator\", act.CreationDate \"@created\", act.LastModifiedBy \"@modificator\", act.LastModification \"@modified\", act.CampaignId Campaign, act.Description, act.GroupKey, act.MediaType, act.PreprocessorId Preprocessor, act.ProviderConfigSettings, act.PreprocessorParams, act.QueueId Queue, act.WaitResource, act.Script ScriptUrl, act.ScriptId Script,  act.WrapupTime, act.RecordingPlaybackLevel, act.DisableManualQualification, act.WrapupExtendable, act.AutomaticRecording, act.ListenAllowed, act.QualificationRequired, act.PostprocessorId Postprocessor, act.PostprocessorParams, act.Lines, act.TimeOffset, outact.OutboundMode, outact.AbandonRateMode, outact.MaxAbandons, outact.MaxDialPerAgent, outact.DialPerAgent, (select 'SYS' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, nref.value('@FieldOperand', 'varchar(max)') FieldOperand, nref.value('@Operand', 'varchar(max)') Operand, nref.value('@DBType', 'varchar(max)') DBType, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction, nref.value('@NextPart', 'varchar(max)') NextPart, nref.value('@LookupOperand', 'varchar(max)') LookupOperand, nref.value('@Operator', 'varchar(max)') Operator from systemfilter.nodes('/SystemFilter/FilterPart') as R(nref) for xml path('FilterPart'), TYPE ) 'Filters', ( select 'USR' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, nref.value('@FieldOperand', 'varchar(max)') FieldOperand, nref.value('@Operand', 'varchar(max)') Operand, nref.value('@DBType', 'varchar(max)') DBType, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction, nref.value('@NextPart', 'varchar(max)') NextPart, nref.value('@LookupOperand', 'varchar(max)') LookupOperand, nref.value('@Operator', 'varchar(max)') Operator from datafilter.nodes('/UserFilter/FilterPart') as R(nref) for xml path('FilterPart'), TYPE ) 'Filters' , (select case nref.value('@Operator', 'varchar(max)') when 'Equal' then nref.value('@Operand', 'varchar(max)') when 'IsNotNull' then '_0this' when 'IsNull' then '_1null' when 'Superior' then '_2any' when 'Like' then '_3dnr' end \"@currentactivityfilterid\", act.Id \"@outboundactivityid\" from currentactivityfilter.nodes('/CurrentActivityFilter/FilterPart') as R(nref) for xml path('CurrentActivityFilterPart'), TYPE) 'CurrentActivityFilters' , (select act.Id \"@outboundactivityid\", 'SYS' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, case nref.value('@Asc', 'varchar(max)') when 'True' then 'Ascending' else 'Descending' end SortOrder, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction from systemsortorder.nodes('/SystemSortOrders/SortPart') as R(nref) for xml path('SortField'), TYPE ) 'OrderByFields', ( select act.Id \"@outboundactivityid\", 'USR' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, case nref.value('@Asc', 'varchar(max)') when 'True' then 'Ascending' else 'Descending' end SortOrder, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction from datasortorder.nodes('/UserSortOrders/SortPart') as R(nref) for xml path('SortField'), TYPE ) 'OrderByFields', outact.AmdSettingsId AmdSettings, outact.Paused, outact.InformationalLanguage, outact.LocationId Location, outact.CarrierId Carrier, outact.Originator, outact.TargetAbandons, outact.RingTime, outact.BlackListCategory, outact.CallbackRuleId CallbackRules, outact.TargetedCallbackNeverPaused, outact.TargetedCallbacksRequireTeamMembership, ca.ActionType ClosedActionType, ca.Param ClosedParam, ca.PlanningId Planning from Activities act inner join OutboundActivities outact on outact.Id = act.Id left join ClosedActions ca on ca.ActivityId = act.Id and ca.ClosedReasonId is null and ca.PlanningId is not null where (act.Active is null or act.active = 1) and (act.timestamp > @mints or outact.timestamp > @mints or (ca.timestamp is not null and ca.timestamp > @mints)) FOR XML PATH ('OutboundActivity'), ELEMENTS{0}, TYPE, ROOT('OutboundActivities')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= OutboundActivityTimeSpanActions: the default closed actions are joined with inbound (and outbound) activities and are not loaded here =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select  1 \"@active\", ClosedReasonId \"@planningtimespanid\", ActivityId \"@outboundactivityid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", PlanningId, ClosedReasonId, ActivityId, ActionType ClosedActionType, Param ClosedParam from ClosedActions where timestamp > @mints and ClosedReasonId is not null and ActivityId in (select id from OutboundActivities) and ActivityId in (select id from activities where active is null or active = 1) and ClosedReasonId not in (select id from specialdays) and ClosedReasonId in (select id from planningtimespans where DaysOfWeek=0) FOR XML PATH ('OutboundActivityTimeSpanAction'), ELEMENTS{0}, TYPE, ROOT('OutboundActivityTimeSpanActions')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= OutboundActivitySpecialDayActions: the default closed actions are joined with inbound (and outbound) activities and are not loaded here =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select  1 \"@active\", ClosedReasonId \"@specialdayid\", ActivityId \"@outboundactivityid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", PlanningId, ClosedReasonId, ActivityId, ActionType ClosedActionType, Param ClosedParam from ClosedActions where timestamp > @mints and ClosedReasonId is not null and ActivityId in (select id from OutboundActivities) and ActivityId in (select id from activities where active is null or active = 1) and ClosedReasonId in (select id from specialdays) FOR XML PATH ('OutboundActivitySpecialDayAction'), ELEMENTS{0}, TYPE, ROOT('OutboundActivitySpecialDayActions')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= Pauses =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, GroupId, ExternalData from Pauses where active = 1 and timestamp > @mints FOR XML PATH ('Pause'), ELEMENTS{0}, TYPE, ROOT('Pauses')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Phones =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\",  LocationId \"@locationid\",  CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, ShortCode, Address, LocationId Location,  KeepConnected, MacAddress, UserAgent, Register, AgentAssociation, AutoAnswer, CallerIdentification, ExternalLine from Phones where timestamp > @mints FOR XML PATH ('Phone'), ELEMENTS{0}, TYPE, ROOT('Phones')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Plannings =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, WorkTimePlanning from Planning where timestamp > @mints FOR XML PATH ('Planning'), ELEMENTS{0}, TYPE, ROOT('Plannings')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= PlanningTimeSpans: =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Dictionary<string, SortedList<int, bool>> work = new Dictionary<string, SortedList<int, bool>>();

                        Cmd.CommandText = "select pts.planningId, pts.StartTime, pts.EndTime, pts.daysofweek, p.WorkTimePlanning from PlanningTimeSpans pts inner join Planning p on p.id=pts.planningid where DaysOfWeek <> 0 and planningid not in (select planningid from PlanningTimeSpans where DaysOfWeek = 0) order by planningId";

                        try
                        {

                            using (SqlDataReader reader = Cmd.ExecuteReader())
                            {
                                string currentPlanning = null;
                                SortedList<int, bool> sl = new SortedList<int, bool>();
                                int[] days = new int[] { 1, 2, 4, 8, 16, 32, 64 };

                                while (reader.Read())
                                {
                                    if (currentPlanning != reader.GetString(0) && currentPlanning != null)
                                    {
                                        work.Add(currentPlanning, sl);
                                        sl = new SortedList<int, bool>();
                                    }

                                    currentPlanning = reader.GetString(0);

                                    for (int i = 0; i < 7; i++)
                                    {
                                        if ((reader.GetInt32(3) & days[i]) == days[i])
                                        {
                                            if (sl.ContainsKey(i * 24 * 60 + reader.GetInt32(1)))
                                            {
                                                if (sl[i * 24 * 60 + reader.GetInt32(1)] == reader.GetBoolean(4))
                                                    sl.Remove(i * 24 * 60 + reader.GetInt32(1));
                                            }
                                            else
                                                sl.Add(i * 24 * 60 + reader.GetInt32(1), !reader.GetBoolean(4));


                                            if (sl.ContainsKey(i * 24 * 60 + reader.GetInt32(2)))
                                            {
                                                if (sl[i * 24 * 60 + reader.GetInt32(2)] != reader.GetBoolean(4))
                                                    sl.Remove(i * 24 * 60 + reader.GetInt32(2));
                                            }
                                            else
                                                sl.Add(i * 24 * 60 + reader.GetInt32(2), reader.GetBoolean(4));

                                        }
                                    }
                                }

                                if (currentPlanning != null)
                                {
                                    // TODO
                                    work.Add(currentPlanning, sl);

                                }

                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }



                        foreach (KeyValuePair<string, SortedList<int, bool>> kvp in work)
                        {
                            for (int i = 0; i < kvp.Value.Values.Count; i++)
                            {
                                using (SqlCommand InsertCmd = C.CreateCommand())
                                {
                                    InsertCmd.CommandText = "Insert into PlanningTimeSpans (Id, PlanningId, StartTime, EndTime, DaysOfWeek, Closed) values (@id, @planningid, @starttime, @endtime, 0, @closed)";
                                    InsertCmd.Parameters.AddWithValue("@id", System.Guid.NewGuid().ToString("N"));
                                    InsertCmd.Parameters.AddWithValue("@planningid", kvp.Key);
                                    InsertCmd.Parameters.AddWithValue("@starttime", kvp.Value.Keys[i]);
                                    InsertCmd.Parameters.AddWithValue("@endtime", 0);
                                    InsertCmd.Parameters.AddWithValue("@closed", kvp.Value.Values[i]);
                                    InsertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", PlanningId \"@planningid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", Description, PlanningId, StartTime, DaysOfWeek, Closed from PlanningTimeSpans where timestamp > @mints and DaysOfWeek=0 FOR XML PATH ('PlanningTimeSpan'), ELEMENTS{0}, TYPE, ROOT('PlanningTimeSpans')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= PredefinedTexts =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", CampaignId \"@campaignid\", Description, CampaignId Campaign, CompatibleMedias, Language, TextContent, HtmlContent, Sequence from PredefinedTexts where timestamp > @mints FOR XML PATH ('PredefinedText'), ELEMENTS{0}, TYPE, ROOT('PredefinedTexts')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }

                    #endregion

                    #region ============================= Preprocessors =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, MediaType, Resource, EditorUrl, EditorParams, DropAfter from Preprocessors where timestamp > @mints FOR XML PATH ('Preprocessor'), ELEMENTS{0}, TYPE, ROOT('Preprocessors')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Prompts =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, Path, Language, (select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, Path, Language, RelatedTo from Prompts where RelatedTo = p.Id FOR XML PATH ('Prompt'), ELEMENTS, TYPE  ) RelatedPrompts from Prompts p where RelatedTo is null and (timestamp > @mints or id in (select RelatedTo from Prompts where RelatedTo is not null and timestamp > @mints) ) FOR XML PATH ('Prompt'), ELEMENTS{0}, TYPE, ROOT('Prompts')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);



                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= PromptsLinks =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select PromptId \"@promptid\", LinkId \"@adminobjectid\", 1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\" from PromptsLinks where timestamp > @mints FOR XML PATH ('PromptLink'), ELEMENTS{0}, TYPE, ROOT('PromptsLinks')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= Qualifications =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Parent \"@parentid\", Parent ParentId, Description, Argued, Positive, PositiveUpdatable, DisplayOrder, GroupKey, ExternalData, Action, ActionParameters, SystemMapping, CustomValue, NewActivity, TriggerHangup, Exportable, IgnoreMaxDialAttempts, Delay from Qualifications where timestamp > @mints and active = 1 FOR XML PATH ('Qualification'), ELEMENTS{0}, TYPE, ROOT('Qualifications')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Queues =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", Owner \"@owner\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, MediaType, WaitResource, ShortCode, GroupKey, EWTDefaultServiceTime, EWTHistoryLength, EWTHistoryDuration, MaxAllowedEWT, HighPriority, ProfitEvaluatorActive, TimeCoef, Time0, Time1, Time2, Time3, Coef0, Coef1, Coef2, Coef3, OverflowActionType, OverflowParam, OverflowCondition, OverflowConditionParam, WrapupTime, DistributionMethod, WrapupExtendable, ExternalData, OverflowPreprocessorParams, OverflowRerouteParam OverflowReroutePrompt from Queues WHERE (Active is null or Active=1) and (timestamp > @mints) FOR XML PATH ('Queue'), ELEMENTS{0}, TYPE, ROOT('Queues')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Resources =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, BaseUri, UserAgent, HoldMusicPlayer, Ringer, Announcer, QueueLoopPlayer, OutboundGateway, AnsweringMachineDetector, ConferenceBridge, IvrPlayer, Monitoring, Recording, RecordingPlayer, LocationId Location, Cost, Enabled, FtpUrl, FtpUser, FtpPassword, AddonsCompatibility,  MaxLines, MaxOutLines, MaxInLines from Resources where timestamp > @mints FOR XML PATH ('Resource'), ELEMENTS{0}, TYPE, ROOT('Resources')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= Settings =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, (select R.nref.query('.') from Value.nodes('/Settings/*') as R(nref) for XML path(''),TYPE  ) from Settings where timestamp > @mints FOR XML PATH ('Setting'), ELEMENTS{0}, TYPE, ROOT('Settings')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Skills =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, ExternalData from Skills where timestamp > @mints FOR XML PATH ('Skill'), ELEMENTS{0}, TYPE, ROOT('Skills')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= SpecialDays =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", PlanningId \"@planningid\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", Description,  PlanningId, PlanningTopApply, Day, Month, Year, Closed, StartRange [From], EndRange [To]  from SpecialDays where timestamp > @mints FOR XML PATH ('SpecialDay'), ELEMENTS{0}, TYPE, ROOT('SpecialDays')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Teams =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", Owner \"@owner\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, Cost, WrapupTime, WrapupExtendable, PauseGroup,  ExternalData from Teams where timestamp > @mints FOR XML PATH ('Team'), ELEMENTS{0}, TYPE, ROOT('Teams')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= TeamsCrossPoints =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select tcp.QueueId \"@queueid\", tcp.TeamId \"@teamid\",  tcp.PlanningId \"@planningid\" ,1 \"@active\", null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", tcp.BaseLevel, tcp.MaxWaitTime, tcp.MinWaitTime from TeamsCrossPoints tcp inner join Queues q on q.id=tcp.queueid where (q.Active is null or q.active=1) and tcp.timestamp > @mints FOR XML PATH ('QueueTeam'), ELEMENTS{0}, TYPE, ROOT('QueuesTeams')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= ViewRestrictions =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select UserId \"@agentid\", 1 \"@active\",  null \"@creator\", null \"@created\", null \"@modificator\", null \"@modified\", UserId, Precedence, InformationLevel, Allowed, TargetType, TargetId Target, IncludeChildren from ViewRestrictions where timestamp > @mints FOR XML PATH ('ViewRestriction'), ELEMENTS{0}, TYPE, ROOT('ViewRestrictions')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    Doc.DocumentElement.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    XmlNode InactiveItems = Doc.DocumentElement.AppendChild(Doc.CreateElement("InactiveItems"));

                    #region ============================= Inactive Agents =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, Account, FirstName, LastName from Agents WHERE Active is not null and Active<>1 and TimeStamp > @mints FOR XML PATH ('Agent'), ELEMENTS{0}, TYPE, ROOT('Agents')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {
                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Inactive queues =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", Owner \"@owner\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, MediaType, WaitResource, ShortCode, GroupKey, EWTDefaultServiceTime, EWTHistoryLength, EWTHistoryDuration, MaxAllowedEWT, HighPriority, ProfitEvaluatorActive, TimeCoef, Time0, Time1, Time2, Time3, Coef0, Coef1, Coef2, Coef3, OverflowActionType, OverflowParam, OverflowCondition, OverflowConditionParam, WrapupTime, DistributionMethod, WrapupExtendable, ExternalData, OverflowPreprocessorParams, OverflowRerouteParam OverflowReroutePrompt from Queues WHERE Active is not null and Active<>1 and (timestamp > @mints) FOR XML PATH ('Queue'), ELEMENTS{0}, TYPE, ROOT('Queues')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= Inactive pauses =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select Id \"@id\", 1 \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Description, GroupKey, GroupId, ExternalData from Pauses where active is not null and active <> 1 and timestamp > @mints FOR XML PATH ('Pause'), ELEMENTS{0}, TYPE, ROOT('Pauses')", minTimeStamp > 0 ? " XSINIL" : string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion


                    #region ============================= Inactive Campaigns =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText =string.Format("select Id \"@id\", Active \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Advanced, Description, GroupKey, FieldsConfig, CustomConfig, ExternalData, ScriptDefinition, Qualification, AppointmentsContext, ImportExportPlugin, QualificationLinked, ExportFields from Campaigns where active is not null and active <> 1 and timestamp > @mints FOR XML PATH ('Campaign'), ELEMENTS{0}, TYPE, ROOT('Campaigns')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }


                    }
                    #endregion

                    #region ============================= Inactive InboundActivities =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {

                        Cmd.CommandText = string.Format("select act.Id \"@id\", act.CampaignId \"@campaignid\", Owner \"@owner\", act.Active \"@active\", act.CreatorId \"@creator\", act.CreationDate \"@created\", act.LastModifiedBy \"@modificator\", act.LastModification \"@modified\", act.CampaignId Campaign, act.Description, act.GroupKey, act.MediaType, act.PreprocessorId Preprocessor, act.PreprocessorParams, inact.OverflowPreprocessorParams, inact.OverflowRerouteParam OverflowReroutePrompt, act.QueueId Queue, act.WaitResource,  act.Script ScriptUrl, act.ScriptId Script,  act.WrapupTime,  act.WrapupExtendable, act.AutomaticRecording, act.ListenAllowed, act.QualificationRequired, act.PostprocessorId Postprocessor, act.PostprocessorParams, act.MusicPrompt, act.Lines, act.TimeOffset, inact.Destination, inact.Reject, inact.Ring, inact.TransmitEWT, inact.TransmitPosition, inact.WaitMusicLength, inact.PreprocessorReplacesSkills, inact.PreprocessorReplacesLanguages, inact.InitialProfit, inact.CallbackActivityId CallbackActivity, inact.QueueMusicDelay, inact.AlternateInitialProfit, inact.AlternateInitialProfitRule, inact.OverflowPrompt, inact.OverflowPromptStartingLoop, inact.OverflowActiveDTMFs, inact.OverflowActionType, inact.OverflowParam, inact.PromptForOverflow, ca.ActionType ClosedActionType, ca.Param ClosedParam, ca.PreprocessorParams ClosedPreprocessorParams, ca.RerouteParam ClosedReroutePrompt, cae.ActionType NoAgentActionType, cae.Param NoAgentParam, cae.PreprocessorParams NoAgentPreprocessorParams, cae.RerouteParam NoAgentReroutePrompt, ca.PlanningId Planning from Activities act inner join InboundActivities inact on inact.Id = act.Id left join ClosedActions ca on ca.ActivityId = act.Id and ca.ClosedReasonId is null and ca.PlanningId is not null left join ClosedActions cae on cae.ActivityId = act.Id and cae.ClosedReasonId is null and cae.PlanningId is null where act.active is not null and act.active <> 1 and (act.TimeStamp > @mints or inact.TimeStamp > @mints) FOR XML PATH ('InboundActivity'), ELEMENTS{0}, TYPE, ROOT('InboundActivities')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);

                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Inactive OutboundActivities =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select act.Id \"@id\", act.CampaignId \"@campaignid\", Owner \"@owner\", act.Active \"@active\", act.CreatorId \"@creator\", act.CreationDate \"@created\", act.LastModifiedBy \"@modificator\", act.LastModification \"@modified\", act.CampaignId Campaign, act.Description, act.GroupKey, act.MediaType, act.PreprocessorId Preprocessor, act.PreprocessorParams, act.QueueId Queue, act.WaitResource, act.Script ScriptUrl, act.ScriptId Script,  act.WrapupTime, act.WrapupExtendable, act.AutomaticRecording, act.ListenAllowed, act.QualificationRequired, act.PostprocessorId Postprocessor, act.PostprocessorParams, act.Lines, act.TimeOffset, outact.OutboundMode, outact.AbandonRateMode, outact.MaxAbandons, outact.MaxDialPerAgent, outact.DialPerAgent, (select 'SYS' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, nref.value('@FieldOperand', 'varchar(max)') FieldOperand, nref.value('@Operand', 'varchar(max)') Operand, nref.value('@DBType', 'varchar(max)') DBType, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction, nref.value('@NextPart', 'varchar(max)') NextPart, nref.value('@LookupOperand', 'varchar(max)') LookupOperand, nref.value('@Operator', 'varchar(max)') Operator from systemfilter.nodes('/SystemFilter/FilterPart') as R(nref) for xml path('FilterPart'), TYPE ) 'Filters', ( select 'USR' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, nref.value('@FieldOperand', 'varchar(max)') FieldOperand, nref.value('@Operand', 'varchar(max)') Operand, nref.value('@DBType', 'varchar(max)') DBType, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction, nref.value('@NextPart', 'varchar(max)') NextPart, nref.value('@LookupOperand', 'varchar(max)') LookupOperand, nref.value('@Operator', 'varchar(max)') Operator from datafilter.nodes('/UserFilter/FilterPart') as R(nref) for xml path('FilterPart'), TYPE ) 'Filters' , (select case nref.value('@Operator', 'varchar(max)') when 'Equal' then nref.value('@Operand', 'varchar(max)') when 'IsNotNull' then '_0this' when 'IsNull' then '_1null' when 'Superior' then '_2any' when 'Like' then '_3dnr' end \"@currentactivityfilterid\", act.Id \"@outboundactivityid\" from currentactivityfilter.nodes('/CurrentActivityFilter/FilterPart') as R(nref) for xml path('CurrentActivityFilterPart'), TYPE) 'CurrentActivityFilters' , (select act.Id \"@outboundactivityid\", 'SYS' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, case nref.value('@Asc', 'varchar(max)') when 'True' then 'Ascending' else 'Descending' end SortOrder, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction from systemsortorder.nodes('/SystemSortOrders/SortPart') as R(nref) for xml path('SortField'), TYPE ) 'OrderByFields', ( select act.Id \"@outboundactivityid\", 'USR' \"@type\", nref.value('@FieldName', 'varchar(max)') FieldName, case nref.value('@Asc', 'varchar(max)') when 'True' then 'Ascending' else 'Descending' end SortOrder, nref.value('@AppointmentFunction', 'varchar(max)') AppointmentFunction from datasortorder.nodes('/UserSortOrders/SortPart') as R(nref) for xml path('SortField'), TYPE ) 'OrderByFields', outact.AmdSettingsId AmdSettings, outact.Paused, outact.LocationId Location, outact.Originator, outact.TargetAbandons, outact.RingTime, outact.BlackListCategory, outact.CallbackRuleId CallbackRules, outact.TargetedCallbackNeverPaused, outact.TargetedCallbacksRequireTeamMembership, ca.ActionType ClosedActionType, ca.Param ClosedParam, ca.PlanningId Planning from Activities act inner join OutboundActivities outact on outact.Id = act.Id left join ClosedActions ca on ca.ActivityId = act.Id and ca.ClosedReasonId is null and ca.PlanningId is not null where act.active is not null and act.active <> 1 and (act.timestamp > @mints or outact.timestamp > @mints) FOR XML PATH ('OutboundActivity'), ELEMENTS{0}, TYPE, ROOT('OutboundActivities')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {

                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }
                    #endregion

                    #region ============================= Inactive Qualifications =============================
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = string.Format("select Id \"@id\", Active \"@active\", CreatorId \"@creator\", CreationDate \"@created\", LastModifiedBy \"@modificator\", LastModification \"@modified\", Parent \"@parentid\", Parent ParentId, Description, Argued, Positive, PositiveUpdatable, DisplayOrder, GroupKey, ExternalData, Action, ActionParameters, SystemMapping, CustomValue, NewActivity, TriggerHangup, Exportable, Delay from Qualifications where active is not null and active <> 1 and timestamp > @mints FOR XML PATH ('Qualification'), ELEMENTS{0}, TYPE, ROOT('Qualifications')", minTimeStamp>0 ? " XSINIL": string.Empty);
                        Cmd.Parameters.AddWithValue("@mints", minTimeStamp);


                        try
                        {
                            using (XmlReader Reader = Cmd.ExecuteXmlReader())
                            {
                                nde = Doc.ReadNode(Reader);
                                if (nde != null)
                                {
                                    InactiveItems.AppendChild(nde);
                                }
                            }
                        }
                        catch (TargetInvocationException tie)
                        {
                        }

                    }

                    #endregion



                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = "select @@DBTS";
                        object obj = Cmd.ExecuteScalar();

                        XmlAttribute att = Doc.CreateAttribute("maxtimestamp");
                        att.Value = System.Convert.ToBase64String((byte[])obj);
                        Doc.DocumentElement.Attributes.Append(att);

                    }
                }
            }
            else
            {
                Doc.AppendChild(Doc.CreateElement("Admin"));
            }

            Doc.Save(@"load.xml");

            return Doc;
        }

        private string m_UserId = null;

        private bool m_needSoundReload = false;

        public class PostSaveAction
        {
            public string Description { get; set; }
            public string MethodName { get; set; }
            public object[] MethodParams { get; set; }

            public PostSaveAction(string description, string methodname, object[] methodParams)
            {
                Description = description;
                MethodName = methodname;
                MethodParams = methodParams;
            }
        }


        public XmlDocument Save(XmlDocument Doc, string connectionString, HandleNumberFormatDelegate handleNumberFormat, string culture)
        {
            XmlDocument postSaveActions = new XmlDocument();
            XmlElement elm = postSaveActions.CreateElement("PostSaveActions");
            postSaveActions.AppendChild(elm);
            string tempPostQuery = null;
            List<string> postQueries = new List<string>();

            Doc.Save(@"save.xml");

            XmlNode node = Doc.SelectSingleNode("admin");

            m_UserId = node.Attributes["user"].Value;

            bool needMohReload = false;
            m_needSoundReload = false;

            XmlNode temp = null;

            using (SqlConnection C = new SqlConnection(connectionString))
            {

                C.Open();

                List<XmlNode> tempList = new List<XmlNode>();
                foreach (XmlNode nde in node.ChildNodes)
                {
                    if (nde.Name.Equals("Prompt"))
                    {
                        m_needSoundReload = true;
                        tempList.Insert(0, nde);
                    }
                    else if ((nde.Name.Equals("ViewRestriction") || nde.Name.Equals("CallbackRule")) && nde.Attributes["operation"].Value.Equals("delete"))
                        tempList.Insert(0, nde);
                    else
                        tempList.Add(nde);
                }

                foreach (XmlNode nde in tempList)
                {

                    if (nde.Name.Equals("Setting"))
                    {
                        XmlDocument xdoc = new XmlDocument();
                        XmlElement root = xdoc.AppendChild(xdoc.CreateElement("Settings") as XmlNode) as XmlElement;
                        foreach(XmlNode n in nde.ChildNodes)
                        {
                            root.AppendChild(xdoc.ImportNode(n, true));
                        }

                        using (SqlCommand cmd = C.CreateCommand())
                        {
                            cmd.CommandText = "Update Settings set Value=@val where id=@id";
                            cmd.Parameters.AddWithValue("@id", nde.Attributes["id"].Value);
                            cmd.Parameters.AddWithValue("@val", xdoc.OuterXml);
                            cmd.ExecuteNonQuery();
                        }

                    }
                    else if (nde.Name.Equals("SortField"))
                    {
                        // TODO!!!
                    }
                    else if (nde.Name.Equals("OutboundActivityCurrentActivityFilter"))
                    {
                        // TODO!!!
                    }
                    else if (nde.Name.Equals("Campaign") && nde.Attributes["operation"].Value.Equals("create"))
                    {
                        #region Campaign
                        EnsureCampaignSystemDataStructure(nde.Attributes["id"].Value, C, "SystemData"); 
                        tempPostQuery = SaveNode(nde, C);
                        if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                            postQueries.Add(tempPostQuery);
                        #endregion
                    }
                    else if (nde.Name.Equals("UserField"))
                    {
                        // here we have to alter the DataStructure depending on the request...
                        EnsureCampaignDataStructure(nde, C, "SystemData");
                    }
                    else if (nde.Name.Equals("Attachment") &&
                        (nde.Attributes["operation"].Value.Equals("delete") ||
                        (nde.Attributes["operation"].Value.Equals("update") && nde.SelectSingleNode("Location") != null)))
                    {
                        // read the value from db
                        string path = (string)GetFieldValue(connectionString, "Attachments", "Location", nde.Attributes["id"].Value);

                        tempPostQuery = SaveNode(nde, C);
                        if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                            postQueries.Add(tempPostQuery);


                        // delete the file   
                        CleanFile(path);
                    }

                    else if (nde.Name.Equals("Prompt") &&
                            (nde.Attributes["operation"].Value.Equals("delete") ||
                            (nde.Attributes["operation"].Value.Equals("update") && nde.SelectSingleNode("Path") != null)))
                    {
                        // read the value from db
                        string path = (string)GetFieldValue(connectionString, "Prompts", "Path", nde.Attributes["id"].Value);

                        tempPostQuery = SaveNode(nde, C);
                        if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                            postQueries.Add(tempPostQuery);


                        // delete the file   
                        CleanFile(path);
                    }
                    else if (nde.Name.Equals("Queue"))
                    {
                        #region Queue
                        if (nde.Attributes["operation"].Value.Equals("delete"))
                        {
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            CleanFile(ComputeFilePath(nde.Attributes["id"].Value));
                        }
                        else if (nde.Attributes["operation"].Value.Equals("update"))
                        {
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(nde.Attributes["id"].Value, ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(nde.Attributes["id"].Value));
                                }
                            }

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                        }
                        else if (nde.Attributes["operation"].Value.Equals("create"))
                        {
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(nde.Attributes["id"].Value, ovfPreprocParam.InnerText);
                                }
                            }

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                        }
                        #endregion
                    }
                    else if (nde.Name.Equals("InboundActivity"))
                    {
                        if (nde.Attributes["operation"].Value.Equals("delete"))
                        {
                            #region Delete
                            // read the value from db
                            string oldMohPrompId = (string)GetFieldValue(connectionString, "Activities", "MusicPrompt", nde.Attributes["id"].Value);
                            string oldMohPromptPath = null;
                            if (!string.IsNullOrEmpty(oldMohPrompId))
                            {
                                oldMohPromptPath = (string)GetFieldValue(connectionString, "Prompts", "Path", oldMohPrompId);
                                needMohReload = true;
                            }

                            #region Closed action and No agent action
                            XmlElement tempNde = nde.OwnerDocument.CreateElement("ClosedAction");
                            tempNde.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                            tempNde.Attributes["operation"].Value = "delete";
                            tempNde.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                            tempNde.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                            tempPostQuery = SaveNode(tempNde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            XmlNode temNode = nde.SelectSingleNode("ClosedActionType");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("Planning");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("ClosedParam");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("ClosedPreprocessorParams");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("ClosedReroutePrompt");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("NoAgentActionType");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("NoAgentParam");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("NoAgentPreprocessorParams");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("NoAgentReroutePrompt");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            #endregion

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            // delete the old moh   
                            CleanFile(oldMohPromptPath);

                            #region IVR files
                            CleanFile(ComputeFilePath(string.Concat("c", nde.Attributes["id"].Value)));
                            CleanFile(ComputeFilePath(string.Concat("n", nde.Attributes["id"].Value)));
                            CleanFile(ComputeFilePath(string.Concat("p", nde.Attributes["id"].Value)));
                            CleanFile(ComputeFilePath(string.Concat("o", nde.Attributes["id"].Value)));
                            CleanFile(ComputeFilePath(nde.Attributes["id"].Value));
                            #endregion


                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("update"))
                        {
                            #region update
                            // read the value from db
                            string oldMohPrompId = (string)GetFieldValue(connectionString, "Activities", "MusicPrompt", nde.Attributes["id"].Value);
                            string oldMohPromptPath = null;

                            temp = nde.SelectSingleNode("MusicPrompt");
                            if (temp != null && !string.IsNullOrEmpty(temp.InnerText))
                            {
                                if (!string.IsNullOrEmpty(oldMohPrompId))
                                {
                                    oldMohPromptPath = ComputeMohPath(oldMohPrompId, (string)GetFieldValue(connectionString, "Prompts", "Path", oldMohPrompId));
                                }

                                // copy the moh 
                                CopyMoh(temp.InnerText, (string)GetFieldValue(connectionString, "Prompts", "Path", temp.InnerText));
                                needMohReload = true;
                            }

                            XmlNode noAgentPreprocessorParams = null;
                            XmlNode closedPreprocessorParams = null;
                            XmlNode noAgentReroutePrompt = null;
                            XmlNode closedReroutePrompt = null;

                            #region ClosedActions
                            XmlNode planning = nde.SelectSingleNode("Planning");
                            if (planning != null)
                            {
                                XmlAttribute NilAttr = planning.Attributes == null ? null : planning.Attributes["nil"];

                                // There is a planning selected on the activity...
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    string oldPlanningId = GetOldPlanningValue(connectionString, nde.Attributes["id"].Value);

                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "update";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));

                                    XmlNode relatedNode = null;
                                    if (!string.IsNullOrEmpty(oldPlanningId) && !oldPlanningId.Equals(planning.InnerText))
                                    {
                                        // planning has changed
                                        AddedClosedActionNode.Attributes["planningid"].Value = oldPlanningId;
                                        XmlNode tmpPlanning = nde.OwnerDocument.CreateElement("PlanningId");
                                        tmpPlanning.InnerText = planning.InnerText;
                                        AddedClosedActionNode.AppendChild(tmpPlanning);

                                        relatedNode = nde.SelectSingleNode("ClosedActionType");
                                        if (relatedNode != null)
                                        {
                                            nde.RemoveChild(relatedNode);
                                            AddedClosedActionNode.AppendChild(relatedNode);
                                        }
                                    }
                                    else
                                    {
                                        AddedClosedActionNode.Attributes["planningid"].Value = planning.InnerText;
                                        relatedNode = nde.SelectSingleNode("ClosedActionType");
                                        if (relatedNode != null)
                                        {
                                            nde.RemoveChild(relatedNode);
                                            AddedClosedActionNode.AppendChild(relatedNode);
                                        }
                                        else
                                        {
                                            XmlNode tmp = nde.OwnerDocument.CreateElement("ClosedActionType");
                                            tmp.InnerText = "0";
                                            AddedClosedActionNode.AppendChild(tmp);
                                        }
                                    }



                                    relatedNode = nde.SelectSingleNode("ClosedParam");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedPreprocessorParams");
                                    if (relatedNode != null)
                                    {
                                        closedPreprocessorParams = relatedNode;
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedReroutePrompt");
                                    if (relatedNode != null)
                                    {
                                        closedReroutePrompt = relatedNode;
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);
                                }
                                else
                                {
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "delete";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = GetOldPlanningValue(connectionString, nde.Attributes["id"].Value);

                                    XmlNode relatedNode = nde.SelectSingleNode("ClosedActionType");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedParam");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedPreprocessorParams");
                                    if (relatedNode != null)
                                    {
                                        closedPreprocessorParams = relatedNode;
                                        nde.RemoveChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedReroutePrompt");
                                    if (relatedNode != null)
                                    {
                                        closedReroutePrompt = relatedNode;
                                        nde.RemoveChild(relatedNode);
                                    }

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }

                                nde.RemoveChild(planning);
                            }
                            else
                            {

                                // so we have to update the closedAction identified by activityid and planning id 
                                string oldPlanningId = GetOldPlanningValue(connectionString, nde.Attributes["id"].Value);


                                XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                AddedClosedActionNode.Attributes["operation"].Value = "update";
                                AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                AddedClosedActionNode.Attributes["planningid"].Value = oldPlanningId;

                                bool closedRelatedChanged = false;
                                XmlNode relatedNode = nde.SelectSingleNode("ClosedActionType");
                                if (relatedNode != null)
                                {
                                    nde.RemoveChild(relatedNode);
                                    AddedClosedActionNode.AppendChild(relatedNode);
                                    closedRelatedChanged = true;
                                }
                                //else
                                //{
                                //    XmlNode tmp = nde.OwnerDocument.CreateElement("ClosedActionType");
                                //    tmp.InnerText = "0";
                                //    AddedClosedActionNode.AppendChild(tmp);
                                //}
                                relatedNode = nde.SelectSingleNode("ClosedParam");
                                if (relatedNode != null)
                                {
                                    nde.RemoveChild(relatedNode);
                                    AddedClosedActionNode.AppendChild(relatedNode);
                                    closedRelatedChanged = true;
                                }
                                relatedNode = nde.SelectSingleNode("ClosedPreprocessorParams");
                                if (relatedNode != null)
                                {
                                    closedPreprocessorParams = relatedNode;
                                    nde.RemoveChild(relatedNode);
                                    AddedClosedActionNode.AppendChild(relatedNode);
                                    closedRelatedChanged = true;
                                }
                                relatedNode = nde.SelectSingleNode("ClosedReroutePrompt");
                                if (relatedNode != null)
                                {
                                    closedReroutePrompt = relatedNode;
                                    nde.RemoveChild(relatedNode);
                                    AddedClosedActionNode.AppendChild(relatedNode);
                                    closedRelatedChanged = true;
                                }

                                if (closedRelatedChanged)
                                {
                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }
                            }
                            #endregion

                            #region No agent action

                            XmlNode temporaryNode = nde.SelectSingleNode("NoAgentActionType");
                            if (temporaryNode != null)
                            {
                                if (temporaryNode.InnerText != "0")
                                {
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "update";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = string.Empty;

                                    nde.RemoveChild(temporaryNode);
                                    AddedClosedActionNode.AppendChild(temporaryNode);

                                    temporaryNode = nde.SelectSingleNode("NoAgentParam");
                                    if (temporaryNode != null)
                                    {
                                        nde.RemoveChild(temporaryNode);
                                        AddedClosedActionNode.AppendChild(temporaryNode);
                                    }
                                    temporaryNode = nde.SelectSingleNode("NoAgentPreprocessorParams");
                                    if (temporaryNode != null)
                                    {
                                        noAgentPreprocessorParams = temporaryNode;
                                        nde.RemoveChild(temporaryNode);
                                        AddedClosedActionNode.AppendChild(temporaryNode);
                                    }
                                    temporaryNode = nde.SelectSingleNode("NoAgentReroutePrompt");
                                    if (temporaryNode != null)
                                    {
                                        noAgentReroutePrompt = temporaryNode;
                                        nde.RemoveChild(temporaryNode);
                                        AddedClosedActionNode.AppendChild(temporaryNode);
                                    }

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }
                                else
                                {
                                    nde.RemoveChild(temporaryNode);

                                    temporaryNode = nde.SelectSingleNode("NoAgentParam");
                                    if (temporaryNode != null)
                                    {
                                        nde.RemoveChild(temporaryNode);
                                    }
                                    temporaryNode = nde.SelectSingleNode("NoAgentPreprocessorParams");
                                    if (temporaryNode != null)
                                    {
                                        noAgentPreprocessorParams = temporaryNode;
                                        nde.RemoveChild(temporaryNode);
                                    }
                                    temporaryNode = nde.SelectSingleNode("NoAgentReroutePrompt");
                                    if (temporaryNode != null)
                                    {
                                        noAgentReroutePrompt = temporaryNode;
                                        nde.RemoveChild(temporaryNode);
                                    }


                                    // ensure there is no entry for no agent action...

                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "delete";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = string.Empty;

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }
                            }
                            else
                            {
                                // so, we r updating a ClosedAction identified by planningid null
                                temporaryNode = nde.SelectSingleNode("NoAgentParam");
                                if (temporaryNode != null)
                                {
                                    nde.RemoveChild(temporaryNode);
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "update";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = string.Empty;
                                    AddedClosedActionNode.AppendChild(temporaryNode);
                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);


                                }
                                temporaryNode = nde.SelectSingleNode("NoAgentPreprocessorParams");
                                if (temporaryNode != null)
                                {
                                    noAgentPreprocessorParams = temporaryNode;
                                    nde.RemoveChild(temporaryNode);
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "update";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = string.Empty;
                                    AddedClosedActionNode.AppendChild(temporaryNode);
                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }

                                temporaryNode = nde.SelectSingleNode("NoAgentReroutePrompt");
                                if (temporaryNode != null)
                                {
                                    noAgentReroutePrompt = temporaryNode;
                                    nde.RemoveChild(temporaryNode);
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "update";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = string.Empty;
                                    AddedClosedActionNode.AppendChild(temporaryNode);
                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }

                            }

                            #endregion

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            #region IVR files
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("ClosedPreprocessorParams");
                            if (ovfPreprocParam == null)
                                ovfPreprocParam = closedPreprocessorParams;
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("c", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(string.Concat("c", nde.Attributes["id"].Value)));
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("NoAgentPreprocessorParams");
                            if (ovfPreprocParam == null)
                                ovfPreprocParam = noAgentPreprocessorParams;
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("n", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(string.Concat("n", nde.Attributes["id"].Value)));
                                }

                            }

                            ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("o", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(string.Concat("o", nde.Attributes["id"].Value)));
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("PreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(nde.Attributes["id"].Value, ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(nde.Attributes["id"].Value));
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("PostprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("p", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(string.Concat("p", nde.Attributes["id"].Value)));
                                }
                            }
                            #endregion

                            // delete the old moh   
                            if (oldMohPromptPath != null)
                                CleanFile(oldMohPromptPath);


                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("create"))
                        {
                            #region create
                            temp = nde.SelectSingleNode("MusicPrompt");
                            if (temp != null && !string.IsNullOrEmpty(temp.InnerText))
                            {
                                // copy the moh 
                                CopyMoh(temp.InnerText, (string)GetFieldValue(connectionString, "Prompts", "Path", temp.InnerText));
                                needMohReload = true;
                            }

                            XmlNode noAgentPreprocessorParams = null;
                            XmlNode closedPreprocessorParams = null;
                            XmlNode noAgentReroutePrompt = null;
                            XmlNode closedReroutePrompt = null;


                            #region ClosedActions
                            XmlNode planning = nde.SelectSingleNode("Planning");
                            if (planning != null)
                            {
                                XmlAttribute NilAttr = planning.Attributes == null ? null : planning.Attributes["nil"];

                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "create";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = planning.InnerText;

                                    XmlNode relatedNode = nde.SelectSingleNode("ClosedActionType");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }
                                    else
                                    {
                                        XmlNode tmp = nde.OwnerDocument.CreateElement("ClosedActionType");
                                        tmp.InnerText = "0";
                                        AddedClosedActionNode.AppendChild(tmp);
                                    }

                                    relatedNode = nde.SelectSingleNode("ClosedParam");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedPreprocessorParams");
                                    if (relatedNode != null)
                                    {
                                        closedPreprocessorParams = relatedNode;
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedReroutePrompt");
                                    if (relatedNode != null)
                                    {
                                        closedReroutePrompt = relatedNode;
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }

                                nde.RemoveChild(planning);
                            }

                            XmlNode ClosedActionType = nde.SelectSingleNode("ClosedActionType");
                            if (ClosedActionType != null)
                            {
                                nde.RemoveChild(ClosedActionType);
                            }

                            #endregion

                            #region No agent action

                            XmlNode temporaryNode = nde.SelectSingleNode("NoAgentActionType");
                            if (temporaryNode != null)
                            {
                                if (temporaryNode.InnerText != "0")
                                {
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "create";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;

                                    nde.RemoveChild(temporaryNode);
                                    AddedClosedActionNode.AppendChild(temporaryNode);

                                    temporaryNode = nde.SelectSingleNode("NoAgentParam");
                                    if (temporaryNode != null)
                                    {
                                        nde.RemoveChild(temporaryNode);
                                        AddedClosedActionNode.AppendChild(temporaryNode);
                                    }
                                    temporaryNode = nde.SelectSingleNode("NoAgentPreprocessorParams");
                                    if (temporaryNode != null)
                                    {
                                        noAgentPreprocessorParams = temporaryNode;
                                        nde.RemoveChild(temporaryNode);
                                        AddedClosedActionNode.AppendChild(temporaryNode);
                                    }
                                    temporaryNode = nde.SelectSingleNode("NoAgentReroutePrompt");
                                    if (temporaryNode != null)
                                    {
                                        noAgentReroutePrompt = temporaryNode;
                                        nde.RemoveChild(temporaryNode);
                                        AddedClosedActionNode.AppendChild(temporaryNode);
                                    }

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }
                                else
                                {
                                    nde.RemoveChild(temporaryNode);
                                }
                            }

                            #endregion

                            #region IVR files
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("ClosedPreprocessorParams");
                            if (ovfPreprocParam == null)
                                ovfPreprocParam = closedPreprocessorParams;
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("c", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("NoAgentPreprocessorParams");
                            if (ovfPreprocParam == null)
                                ovfPreprocParam = noAgentPreprocessorParams;
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("n", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("o", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("PreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(nde.Attributes["id"].Value, ovfPreprocParam.InnerText);
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("PostprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("p", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                            }
                            #endregion

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                            #endregion
                        }
                    }
                    else if (nde.Name.Equals("OutboundActivity"))
                    {
                        if (nde.Attributes["operation"].Value.Equals("delete"))
                        {
                            #region Delete
                            // read the value from db
                            string oldMohPrompId = (string)GetFieldValue(connectionString, "Activities", "MusicPrompt", nde.Attributes["id"].Value);
                            string oldMohPromptPath = null;
                            if (!string.IsNullOrEmpty(oldMohPrompId))
                            {
                                oldMohPromptPath = (string)GetFieldValue(connectionString, "Prompts", "Path", oldMohPrompId);
                                needMohReload = true;
                            }

                            #region Closed action
                            XmlElement tempNde = nde.OwnerDocument.CreateElement("ClosedAction");
                            tempNde.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                            tempNde.Attributes["operation"].Value = "delete";
                            tempNde.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                            tempNde.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                            tempPostQuery = SaveNode(tempNde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            XmlNode temNode = nde.SelectSingleNode("ClosedActionType");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("Planning");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            temNode = nde.SelectSingleNode("ClosedParam");
                            if (temNode != null)
                                nde.RemoveChild(temNode);
                            #endregion

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            // delete the old moh   
                            CleanFile(oldMohPromptPath);

                            #region IVR files
                            CleanFile(ComputeFilePath(string.Concat("p", nde.Attributes["id"].Value)));
                            CleanFile(ComputeFilePath(nde.Attributes["id"].Value));
                            #endregion


                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("update"))
                        {
                            #region update
                            // read the value from db
                            string oldMohPrompId = (string)GetFieldValue(connectionString, "Activities", "MusicPrompt", nde.Attributes["id"].Value);
                            string oldMohPromptPath = null;

                            temp = nde.SelectSingleNode("MusicPrompt");
                            if (temp != null && !string.IsNullOrEmpty(temp.InnerText))
                            {
                                if (!string.IsNullOrEmpty(oldMohPrompId))
                                {
                                    oldMohPromptPath = ComputeMohPath(oldMohPrompId, (string)GetFieldValue(connectionString, "Prompts", "Path", oldMohPrompId));
                                }

                                // copy the moh 
                                CopyMoh(temp.InnerText, (string)GetFieldValue(connectionString, "Prompts", "Path", temp.InnerText));
                                needMohReload = true;
                            }

                            #region ClosedActions
                            XmlNode planning = nde.SelectSingleNode("Planning");
                            if (planning != null)
                            {
                                XmlAttribute NilAttr = planning.Attributes == null ? null : planning.Attributes["nil"];

                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    string oldPlanningId = GetOldPlanningValue(connectionString, nde.Attributes["id"].Value);

                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "update";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));

                                    XmlNode relatedNode = null;

                                    if (!string.IsNullOrEmpty(oldPlanningId) && !oldPlanningId.Equals(planning.InnerText))
                                    {
                                        // planning has changed
                                        AddedClosedActionNode.Attributes["planningid"].Value = oldPlanningId;
                                        XmlNode tmpPlanning = nde.OwnerDocument.CreateElement("PlanningId");
                                        tmpPlanning.InnerText = planning.InnerText;
                                        AddedClosedActionNode.AppendChild(tmpPlanning);

                                        relatedNode = nde.SelectSingleNode("ClosedActionType");
                                        if (relatedNode != null)
                                        {
                                            nde.RemoveChild(relatedNode);
                                            AddedClosedActionNode.AppendChild(relatedNode);
                                        }

                                    }
                                    else
                                    {
                                        AddedClosedActionNode.Attributes["planningid"].Value = planning.InnerText;

                                        relatedNode = nde.SelectSingleNode("ClosedActionType");
                                        if (relatedNode != null)
                                        {
                                            nde.RemoveChild(relatedNode);
                                            AddedClosedActionNode.AppendChild(relatedNode);
                                        }
                                        else
                                        {
                                            XmlNode tmp = nde.OwnerDocument.CreateElement("ClosedActionType");
                                            tmp.InnerText = "0";
                                            AddedClosedActionNode.AppendChild(tmp);
                                        }

                                    }

                                    relatedNode = nde.SelectSingleNode("ClosedParam");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }



                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }
                                else
                                {
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "delete";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = GetOldPlanningValue(connectionString, nde.Attributes["id"].Value);

                                    XmlNode relatedNode = nde.SelectSingleNode("ClosedActionType");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                    }
                                    relatedNode = nde.SelectSingleNode("ClosedParam");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                    }
                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }

                                nde.RemoveChild(planning);
                            }
                            else
                            {
                                // so we have to update the closedAction identified by activityid and planning id 
                                string oldPlanningId = GetOldPlanningValue(connectionString, nde.Attributes["id"].Value);


                                XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                AddedClosedActionNode.Attributes["operation"].Value = "update";
                                AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                AddedClosedActionNode.Attributes["planningid"].Value = oldPlanningId;

                                XmlNode relatedNode = nde.SelectSingleNode("ClosedActionType");
                                if (relatedNode != null)
                                {
                                    nde.RemoveChild(relatedNode);
                                    AddedClosedActionNode.AppendChild(relatedNode);
                                }
                                //else
                                //{
                                //    XmlNode tmp = nde.OwnerDocument.CreateElement("ClosedActionType");
                                //    tmp.InnerText = "0";
                                //    AddedClosedActionNode.AppendChild(tmp);
                                //}
                                relatedNode = nde.SelectSingleNode("ClosedParam");
                                if (relatedNode != null)
                                {
                                    nde.RemoveChild(relatedNode);
                                    AddedClosedActionNode.AppendChild(relatedNode);
                                }
                                tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                    postQueries.Add(tempPostQuery);

                            }
                            #endregion


                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            #region IVR files
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("PreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(nde.Attributes["id"].Value, ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(nde.Attributes["id"].Value));
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("PostprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("p", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                                else
                                {
                                    CleanFile(ComputeFilePath(string.Concat("p", nde.Attributes["id"].Value)));
                                }
                            }
                            #endregion

                            // delete the old moh  
                            if (oldMohPromptPath != null)
                                CleanFile(oldMohPromptPath);


                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("create"))
                        {
                            #region create
                            temp = nde.SelectSingleNode("MusicPrompt");
                            if (temp != null && !string.IsNullOrEmpty(temp.InnerText))
                            {
                                // copy the moh 
                                CopyMoh(temp.InnerText, (string)GetFieldValue(connectionString, "Prompts", "Path", temp.InnerText));
                                needMohReload = true;
                            }

                            #region ClosedActions
                            XmlNode planning = nde.SelectSingleNode("Planning");
                            if (planning != null)
                            {
                                XmlAttribute NilAttr = planning.Attributes == null ? null : planning.Attributes["nil"];

                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    XmlElement AddedClosedActionNode = nde.OwnerDocument.CreateElement("ClosedAction");
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("operation"));
                                    AddedClosedActionNode.Attributes["operation"].Value = "create";
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("activityid"));
                                    AddedClosedActionNode.Attributes["activityid"].Value = nde.Attributes["id"].Value;
                                    AddedClosedActionNode.Attributes.Append(nde.OwnerDocument.CreateAttribute("planningid"));
                                    AddedClosedActionNode.Attributes["planningid"].Value = planning.InnerText;

                                    XmlNode relatedNode = nde.SelectSingleNode("ClosedActionType");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }
                                    else
                                    {
                                        XmlNode tmp = nde.OwnerDocument.CreateElement("ClosedActionType");
                                        tmp.InnerText = "0";
                                        AddedClosedActionNode.AppendChild(tmp);
                                    }

                                    relatedNode = nde.SelectSingleNode("ClosedParam");
                                    if (relatedNode != null)
                                    {
                                        nde.RemoveChild(relatedNode);
                                        AddedClosedActionNode.AppendChild(relatedNode);
                                    }

                                    tempPostQuery = SaveNode(AddedClosedActionNode, C);
                                    if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                        postQueries.Add(tempPostQuery);

                                }

                                nde.RemoveChild(planning);
                            }

                            XmlNode ClosedActionType = nde.SelectSingleNode("ClosedActionType");
                            if (ClosedActionType != null)
                            {
                                nde.RemoveChild(ClosedActionType);
                            }

                            #endregion

                            #region IVR files
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("PreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(nde.Attributes["id"].Value, ovfPreprocParam.InnerText);
                                }
                            }

                            ovfPreprocParam = nde.SelectSingleNode("PostprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat("p", nde.Attributes["id"].Value), ovfPreprocParam.InnerText);
                                }
                            }
                            #endregion

                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                            #endregion
                        }
                    }
                    else if (nde.Name.Equals("InboundActivityTimeSpanAction"))
                    {
                        if (nde.Attributes["operation"].Value.Equals("delete"))
                        {
                            #region delete
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                            CleanFile(ComputeFilePath(string.Concat(nde.Attributes["inboundactivityid"].Value, nde.Attributes["planningtimespanid"].Value)));
                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("update"))
                        {
                            #region update
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            XmlNode ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat(nde.Attributes["inboundactivityid"].Value, nde.Attributes["planningtimespanid"].Value), ovfPreprocParam.InnerText);
                                }
                            }
                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("create"))
                        {
                            #region create
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat(nde.Attributes["inboundactivityid"].Value, nde.Attributes["planningtimespanid"].Value), ovfPreprocParam.InnerText);
                                }
                            }
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                            #endregion
                        }
                    }
                    else if (nde.Name.Equals("InboundActivitySpecialDayAction"))
                    {
                        if (nde.Attributes["operation"].Value.Equals("delete"))
                        {
                            #region delete
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                            CleanFile(ComputeFilePath(string.Concat(nde.Attributes["inboundactivityid"].Value, nde.Attributes["specialdayid"].Value)));
                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("update"))
                        {
                            #region update
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);


                            XmlNode ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat(nde.Attributes["inboundactivityid"].Value, nde.Attributes["specialdayid"].Value), ovfPreprocParam.InnerText);
                                }
                            }
                            #endregion
                        }
                        else if (nde.Attributes["operation"].Value.Equals("create"))
                        {
                            #region create
                            XmlNode ovfPreprocParam = nde.SelectSingleNode("OverflowPreprocessorParams");
                            if (ovfPreprocParam != null)
                            {
                                XmlAttribute NilAttr = ovfPreprocParam.Attributes == null ? null : ovfPreprocParam.Attributes["nil"];
                                if (NilAttr == null || !bool.Parse(NilAttr.Value))
                                {
                                    SaveParamsFile(string.Concat(nde.Attributes["inboundactivityid"].Value, nde.Attributes["specialdayid"].Value), ovfPreprocParam.InnerText);
                                }
                            }
                            tempPostQuery = SaveNode(nde, C);
                            if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                                postQueries.Add(tempPostQuery);

                            #endregion
                        }
                    }
                    else if (nde.Name.Equals("Campaign") && nde.Attributes["operation"].Value.Equals("update"))
                    {
                        // ensure to touch the related activities
                        try
                        {
                            using (SqlCommand cmd = C.CreateCommand())
                            {
                                cmd.CommandText = "Update Activities set description=description where campaignid = @campid";
                                cmd.Parameters.AddWithValue("@campid", nde.Attributes["id"].Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception extouch)
                        {
                            Trace(extouch.ToString(), "ServerLink");
                        }


                        string oldNumberFormat = (string)GetFieldValue(connectionString, "Campaigns", "NumberFormat", nde.Attributes["id"].Value);

                        tempPostQuery = SaveNode(nde, C);
                        if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                            postQueries.Add(tempPostQuery);
                        XmlNode tempNode = null;
                        if ((tempNode = nde.SelectSingleNode("NumberFormat")) != null)
                        {
                            bool noPostSave = false;
                            string campDescription = (string)GetFieldValue(connectionString, "Campaigns", "Description", nde.Attributes["id"].Value);
                            string sourceFormat = null;
                            if ("Global++++++++++++++++++++++++++".Equals(oldNumberFormat))
                            {
                                sourceFormat = "Global";
                            }
                            else if ("Neutral+++++++++++++++++++++++++".Equals(oldNumberFormat))
                            {
                                sourceFormat = "Neutral";
                                noPostSave = true;
                            }
                            else
                                sourceFormat = (string)GetFieldValue(connectionString, "NumberFormats", "Description", oldNumberFormat);

                            string destinationFormat = null;
                            if ("Global++++++++++++++++++++++++++".Equals(tempNode.InnerText))
                            {
                                destinationFormat = "Global";
                            }
                            else if ("Neutral+++++++++++++++++++++++++".Equals(tempNode.InnerText))
                            {
                                destinationFormat = "Neutral";
                                noPostSave = true;
                            }
                            else
                                destinationFormat = (string)GetFieldValue(connectionString, "NumberFormats", "Description", tempNode.InnerText);

                            if (!noPostSave && oldNumberFormat != null && tempNode.InnerText != null && !oldNumberFormat.Equals(tempNode.InnerText))
                            {

                                XmlElement elm1 = postSaveActions.CreateElement("PostSaveAction");
                                postSaveActions.DocumentElement.AppendChild(elm1);
                                XmlElement elm2 = postSaveActions.CreateElement("Description");
                                elm1.AppendChild(elm2);
                                elm2.InnerText = string.Format(Translate("Number format associated to campaign {0} has been changed from {1} to {2}.\nDo you want to convert the phone numbers in the database?", culture, connectionString), campDescription, sourceFormat, destinationFormat);

                                elm2 = postSaveActions.CreateElement("MethodName");
                                elm1.AppendChild(elm2);
                                elm2.InnerText = "UpdateNumbersFormat";

                                elm2 = postSaveActions.CreateElement("Parameters");
                                elm1.AppendChild(elm2);

                                elm1 = postSaveActions.CreateElement("CampaignId");
                                elm2.AppendChild(elm1);
                                elm1.InnerText = nde.Attributes["id"].Value;

                                elm1 = postSaveActions.CreateElement("OldNumberFormat");
                                elm2.AppendChild(elm1);
                                elm1.InnerText = oldNumberFormat;

                                elm1 = postSaveActions.CreateElement("NewNumberFormat");
                                elm2.AppendChild(elm1);
                                elm1.InnerText = tempNode.InnerText;
                            }
                        }
                    }
                    else
                    {
                        tempPostQuery = SaveNode(nde, C);
                        if (tempPostQuery != null && !postQueries.Contains(tempPostQuery))
                            postQueries.Add(tempPostQuery);

                    }
                }
                if (postQueries.Count > 0)
                {
                    foreach (string query in postQueries)
                    {
                        using (SqlCommand cmd = C.CreateCommand())
                        {

                            cmd.CommandText = query;
                            Trace(cmd.CommandText, "ServerLink");
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch(Exception ex)
                            {
                                Trace(ex.ToString(), "ServerLink");
                                
                                // temp to be improved...
                                // UPDATE {0} SET {1}=-{1} WHERE {1}<0
                                if (cmd.CommandText.StartsWith("UPDATE ") && cmd.CommandText.EndsWith("<0"))
                                {
                                    Trace("Exception when giving positive values to precedences -> cleaning the negative entry", "ServerLink");
                                    // security...
                                    string[] tempAr = cmd.CommandText.Split(' ');
                                    cmd.CommandText = string.Format("DELETE FROM {0} WHERE {1}", tempAr[1], tempAr[5] );
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                if (m_needSoundReload)
                {
                    using (SqlCommand cmd = C.CreateCommand())
                    {
                        cmd.CommandText = "select baseUri from Resources where Enabled = 1";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    WebRequest req = WebRequest.Create(string.Format(m_SoundsSync, ExtractIPFromBaseUri(reader.GetString(0))));
                                    Trace(req.RequestUri.ToString(), "ServerLink");
                                    req.GetResponse();
                                }
                                catch (Exception ex)
                                {
                                    Trace(ex.ToString(), "ServerLink");
                                }
                            }
                        }
                    }
                }
                if (needMohReload)
                {
                    using (SqlCommand cmd = C.CreateCommand())
                    {
                        cmd.CommandText = "select baseUri from Resources where Enabled = 1";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    WebRequest req = WebRequest.Create(string.Format(m_MohSync, ExtractIPFromBaseUri(reader.GetString(0))));
                                    Trace(req.RequestUri.ToString(), "ServerLink");
                                    req.GetResponse();
                                }
                                catch (Exception ex)
                                {
                                    Trace(ex.ToString(), "ServerLink");
                                }
                            }
                        }
                    }
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "delete from qualifications where parent is not null and parent not in (select id from qualifications)";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} qualifications cleaned due to invalid parent", affected), "ServerLink");
                }


                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "delete from agentsteams where agentid not in (select id from agents)";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} AgentsTeams cleaned due to invalid agentid", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "delete from agentsteams where teamid not in (select id from teams)";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} AgentsTeams cleaned due to invalid teamid", affected), "ServerLink");
                }
                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "delete from activities where id not in (select id from outboundactivities) and id not in (select id from inboundactivities)";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} activities cleaned due to invalid outbound and inbound", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY teamid, queueid ORDER BY timestamp ASC) FROM teamscrosspoints ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate teamscrosspoints cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY teamid, agentid ORDER BY timestamp ASC) FROM AgentsTeams ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate agentsteams cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY languageid, agentid ORDER BY timestamp ASC) FROM AgentsLanguages ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate AgentsLanguages cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY skillid, agentid ORDER BY timestamp ASC) FROM AgentsSkills ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate AgentsSkills cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY languageid, activityid ORDER BY timestamp ASC) FROM ActivitiesLanguages ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate ActivitiesLanguages cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY qualificationid, activityid ORDER BY timestamp ASC) FROM ActivitiesQualificationsExclusions ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate ActivitiesQualificationsExclusions cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY queueid, activityid ORDER BY timestamp ASC) FROM ActivitiesQueues ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate ActivitiesQueues cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY skillid, activityid ORDER BY timestamp ASC) FROM ActivitiesSkills ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate ActivitiesSkills cleaned", affected), "ServerLink");
                }

                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = "WITH CTE(Ranking) AS (SELECT Ranking = DENSE_RANK() OVER(PARTITION BY planningid, closedreasonid, activityid ORDER BY timestamp ASC) FROM closedactions ) delete from CTE where Ranking>1";
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                        Trace(string.Format("{0} duplicate ClosedActions cleaned", affected), "ServerLink");
                }
                
            }

            return postSaveActions;
        }

        private string SaveNode(XmlNode node, SqlConnection C)
        {
            string objectName = node.Name;
            Dictionary<string, List<string>> fieldshelper = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> idshelper = new Dictionary<string, List<string>>();

            string invertedField = null;
            string invertedTable = null;


            switch (node.Attributes["operation"].Value)
            {
                case "create":
                    #region create
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        List<string> fields = new List<string>();
                        List<string> parms = new List<string>();

                        List<string> creatorMapping = GetMapping(string.Concat(objectName, ".CreatorId"));
                        List<string> modifierMapping = GetMapping(string.Concat(objectName, ".LastModifiedBy"));

                        #region CreatorId and LastModifiedBy

                        List<string> translations = GetMapping(string.Concat(objectName, ".CreatorId"));

                        string tableName = null;
                        string fieldName = null;

                        if (translations == null)
                        {
                            tableName = string.Concat(objectName, "s");
                            fieldName = "CreatorId";

                            if (fieldshelper.ContainsKey(tableName))
                                fieldshelper[tableName].Add(fieldName);
                            else
                                fieldshelper.Add(tableName, new List<string>() { fieldName });
                        }
                        else
                        {
                            foreach (string translation in translations)
                            {
                                string[] strArr = translation.Split('.');
                                tableName = strArr[0];
                                if (strArr.Length > 1)
                                    fieldName = strArr[1];
                                else
                                    fieldName = "CreatorId";

                                if (fieldName.StartsWith("-"))
                                {
                                    invertedField = fieldName.Substring(1);
                                    invertedTable = tableName;
                                    if (fieldshelper.ContainsKey(tableName))
                                        fieldshelper[tableName].Add(fieldName.Substring(1));
                                    else
                                        fieldshelper.Add(tableName, new List<string>() { fieldName.Substring(1) });
                                }
                                else
                                {
                                    if (fieldshelper.ContainsKey(tableName))
                                        fieldshelper[tableName].Add(fieldName);
                                    else
                                        fieldshelper.Add(tableName, new List<string>() { fieldName });
                                }

                            }
                        }
                        if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), m_UserId);



                        translations = GetMapping(string.Concat(objectName, ".LastModifiedBy"));

                        tableName = null;
                        fieldName = null;

                        if (translations == null)
                        {
                            tableName = string.Concat(objectName, "s");
                            fieldName = "LastModifiedBy";

                            if (fieldshelper.ContainsKey(tableName))
                                fieldshelper[tableName].Add(fieldName);
                            else
                                fieldshelper.Add(tableName, new List<string>() { fieldName });
                        }
                        else
                        {
                            foreach (string translation in translations)
                            {
                                string[] strArr = translation.Split('.');
                                tableName = strArr[0];
                                if (strArr.Length > 1)
                                    fieldName = strArr[1];
                                else
                                    fieldName = "LastModifiedBy";

                                if (fieldName.StartsWith("-"))
                                {
                                    invertedField = fieldName.Substring(1);
                                    invertedTable = tableName;
                                    if (fieldshelper.ContainsKey(tableName))
                                        fieldshelper[tableName].Add(fieldName.Substring(1));
                                    else
                                        fieldshelper.Add(tableName, new List<string>() { fieldName.Substring(1) });
                                }
                                else
                                {
                                    if (fieldshelper.ContainsKey(tableName))
                                        fieldshelper[tableName].Add(fieldName);
                                    else
                                        fieldshelper.Add(tableName, new List<string>() { fieldName });
                                }

                            }
                        }
                        if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), m_UserId);

                        #endregion

                        foreach (XmlNode subNode in node.ChildNodes)
                        {
                            translations = GetMapping(string.Concat(objectName, ".", subNode.Name));

                            tableName = null;
                            fieldName = null;

                            if (translations == null)
                            {
                                tableName = string.Concat(objectName, "s");
                                fieldName = subNode.Name;

                                if (fieldshelper.ContainsKey(tableName))
                                    fieldshelper[tableName].Add(fieldName);
                                else
                                    fieldshelper.Add(tableName, new List<string>() { fieldName });
                            }
                            else
                            {
                                foreach (string translation in translations)
                                {
                                    string[] strArr = translation.Split('.');
                                    tableName = strArr[0];
                                    if (strArr.Length > 1)
                                        fieldName = strArr[1];
                                    else
                                        fieldName = subNode.Name;

                                    if (fieldName.StartsWith("-"))
                                    {
                                        invertedField = fieldName.Substring(1);
                                        invertedTable = tableName;
                                        if (fieldshelper.ContainsKey(tableName))
                                            fieldshelper[tableName].Add(fieldName.Substring(1));
                                        else
                                            fieldshelper.Add(tableName, new List<string>() { fieldName.Substring(1) });
                                    }
                                    else
                                    {
                                        if (fieldshelper.ContainsKey(tableName))
                                            fieldshelper[tableName].Add(fieldName);
                                        else
                                            fieldshelper.Add(tableName, new List<string>() { fieldName });
                                    }

                                }
                            }

                            if (fieldName == null)
                                continue;

                            XmlAttribute NilAttr = subNode.Attributes == null ? null : subNode.Attributes["nil"];

                            if (NilAttr != null && bool.Parse(NilAttr.Value))
                            {
                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), DBNull.Value);
                            }
                            else
                            {

                                if (subNode.Attributes.GetNamedItem("type") != null)
                                {
                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                    {
                                        switch (subNode.Attributes["type"].Value)
                                        {
                                            case "Int32":
                                                if (fieldName.StartsWith("-"))
                                                    Cmd.Parameters.AddWithValue(string.Concat("@", fieldName.Substring(1)), -System.Xml.XmlConvert.ToInt32(subNode.InnerText));
                                                else
                                                    Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.Xml.XmlConvert.ToInt32(subNode.InnerText));
                                                break;
                                            case "Single":
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.Xml.XmlConvert.ToSingle(subNode.InnerText));
                                                break;
                                            case "Boolean":
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.Xml.XmlConvert.ToBoolean(subNode.InnerText));
                                                break;
                                            default:
                                                Trace(string.Format("Type {0} is not handled!", subNode.Attributes["type"].Value), "ServerLink");
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                        Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), subNode.InnerText);
                                }
                            }
                        }

                        List<string> queries = new List<string>();

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            if (att.Name.EndsWith("id"))
                            {
                                translations = GetMapping(string.Concat(objectName, ".", att.Name));
                                tableName = null;
                                fieldName = null;

                                if (translations == null)
                                {
                                    tableName = string.Concat(objectName, "s");
                                    fieldName = att.Name;

                                    if (fieldshelper.ContainsKey(tableName))
                                    {
                                        if (!fieldshelper[tableName].Contains(fieldName))
                                            fieldshelper[tableName].Add(fieldName);
                                    }
                                    else
                                    {
                                        fieldshelper.Add(tableName, new List<string>() { fieldName });
                                    }

                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                        if (string.IsNullOrEmpty(att.Value))
                                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.DBNull.Value);
                                        else
                                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), att.Value);
                                }
                                else
                                {
                                    foreach (string translation in translations)
                                    {
                                        string[] strArr = translation.Split('.');
                                        tableName = strArr[0];
                                        if (strArr.Length > 1)
                                            fieldName = strArr[1];
                                        else
                                            fieldName = att.Name;

                                        if (fieldshelper.ContainsKey(tableName))
                                        {
                                            if (!fieldshelper[tableName].Contains(fieldName))
                                                fieldshelper[tableName].Add(fieldName);
                                        }
                                        else
                                            fieldshelper.Add(tableName, new List<string>() { fieldName });

                                        if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                            if (string.IsNullOrEmpty(att.Value))
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.DBNull.Value);
                                            else
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), att.Value);
                                    }
                                }
                            }
                        }

                        Cmd.Parameters.AddWithValue("@null", System.DBNull.Value);

                        foreach (KeyValuePair<string, List<string>> kvp in fieldshelper)
                        {
                            List<string> fieldNames = new List<string>(kvp.Value/*.Union(added.Keys)*/);
                            List<string> paramNames = new List<string>(kvp.Value/*.Union(added.Values)*/);


                            Cmd.CommandText = string.Format("SET ANSI_NULLS OFF;insert into {0} ([{1}]) values (@{2})", kvp.Key, string.Join("],[", fieldNames.ToArray()), string.Join(",@", paramNames.ToArray()));

                            List<string> desc = new List<string>();
                            foreach (SqlParameter sp in Cmd.Parameters)
                                if (!sp.ParameterName.Equals("@null")) desc.Add(string.Format("{0}:{1}", sp.ParameterName, sp.SqlValue.ToString()));
                            Trace(string.Format("{0} | {1}", Cmd.CommandText, string.Join(", ", desc.ToArray())), "ServerLink");

                            try
                            {
                                Cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Trace(ex.ToString(), "ServerLink");
                            }

                        }
                    }

                    if (invertedField != null)
                    {
                        return string.Format("UPDATE {0} SET {1}=-{1} WHERE {1}<0", invertedTable, invertedField);
                    }

                    #endregion
                    break;

                case "delete":
                    #region delete
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        List<string> fields = new List<string>();
                        List<string> parms = new List<string>();
                        List<string> queries = new List<string>();

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            if (att.Name.EndsWith("id"))
                            {
                                List<string> translations = GetMapping(string.Concat(objectName, ".", att.Name));
                                string tableName = null;
                                string fieldName = null;

                                if (translations == null)
                                {
                                    tableName = string.Concat(objectName, "s");
                                    fieldName = att.Name;

                                    if (idshelper.ContainsKey(tableName))
                                        idshelper[tableName].Add(fieldName);
                                    else
                                        idshelper.Add(tableName, new List<string>() { fieldName });

                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                        if (string.IsNullOrEmpty(att.Value))
                                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.DBNull.Value);
                                        else
                                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), att.Value);
                                }
                                else
                                {
                                    foreach (string translation in translations)
                                    {
                                        string[] strArr = translation.Split('.');
                                        tableName = strArr[0];
                                        if (strArr.Length > 1)
                                            fieldName = strArr[1];
                                        else
                                            fieldName = att.Name;

                                        if (idshelper.ContainsKey(tableName))
                                            idshelper[tableName].Add(fieldName);
                                        else
                                            idshelper.Add(tableName, new List<string>() { fieldName });

                                        if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                            if (string.IsNullOrEmpty(att.Value))
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.DBNull.Value);
                                            else
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), att.Value);
                                    }
                                }
                            }
                        }

                        Cmd.Parameters.AddWithValue("@null", System.DBNull.Value);

                        foreach (KeyValuePair<string, List<string>> kvp in idshelper)
                        {

                            //Dictionary<string, string> added = CheckRequiredFields(idshelper.ElementAt(i).Key, idshelper.ElementAt(i).Value, idshelper);

                            List<string> fieldNames = new List<string>(kvp.Value/*.Union(added.Keys)*/);
                            List<string> paramNames = new List<string>(kvp.Value/*.Union(added.Values)*/);

                            List<string> temp = new List<string>();
                            for (int j = 0; j < fieldNames.Count; j++)
                                temp.Add(string.Concat("[", fieldNames[j], "]=@", paramNames[j]));

                            if (TablesWithActiveFlag.Contains(kvp.Key))
                            {
                                Cmd.Parameters.AddWithValue("@LastModifiedBy", m_UserId);
                                Cmd.Parameters.AddWithValue("@LastModification", DateTime.Now);
                                Cmd.CommandText = string.Format("SET ANSI_NULLS OFF;Update {0} set active = 0, LastModifiedBy=@LastModifiedBy, LastModification=@LastModification  where {1}", kvp.Key, string.Join(" AND ", temp.ToArray()));
                            }
                            else if (TablesWithNoDelete.Contains(kvp.Key))
                                Cmd.CommandText = null;
                            else
                                Cmd.CommandText = string.Format("SET ANSI_NULLS OFF;delete from {0} where {1}", kvp.Key, string.Join(" AND ", temp.ToArray()));

                            if (!string.IsNullOrEmpty(Cmd.CommandText))
                            {
                                List<string> desc = new List<string>();
                                foreach (SqlParameter sp in Cmd.Parameters)
                                    if (!sp.ParameterName.Equals("@null")) desc.Add(string.Format("{0}:{1}", sp.ParameterName, sp.SqlValue.ToString()));
                                Trace(string.Format("{0} | {1}", Cmd.CommandText, string.Join(", ", desc.ToArray())), "ServerLink");
                                try
                                {
                                    Cmd.ExecuteNonQuery();
                                }
                                catch (Exception exdel)
                                {
                                    Trace(exdel.ToString(), "ServerLink");
                                }
                            }
                        }

                    }
                    #endregion
                    break;

                case "update":
                    #region update
                    //List<string> mustDelete = new List<string>();
                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        List<string> fields = new List<string>();
                        List<string> parms = new List<string>();

                        #region LastModifiedBy
                        List<string> translations = GetMapping(string.Concat(objectName, ".LastModifiedBy"));
                        string tableName = null;
                        string fieldName = null;

                        if (translations == null)
                        {
                            tableName = string.Concat(objectName, "s");
                            fieldName = "LastModifiedBy";

                            if (fieldshelper.ContainsKey(tableName))
                            {
                                fieldshelper[tableName].Add(fieldName);
                                fieldshelper[tableName].Add("LastModification");
                            }
                            else
                            {
                                fieldshelper.Add(tableName, new List<string>() { fieldName });
                                fieldshelper[tableName].Add("LastModification");
                            }
                        }
                        else
                        {
                            foreach (string translation in translations)
                            {
                                string[] strArr = translation.Split('.');
                                tableName = strArr[0];
                                if (strArr.Length > 1)
                                    fieldName = strArr[1];
                                else
                                    fieldName = "LastModifiedBy";

                                if (fieldshelper.ContainsKey(tableName))
                                {
                                    fieldshelper[tableName].Add(fieldName);
                                    fieldshelper[tableName].Add("LastModification");
                                }
                                else
                                {
                                    fieldshelper.Add(tableName, new List<string>() { fieldName });
                                    fieldshelper[tableName].Add("LastModification");
                                }

                            }
                        }

                        if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                        {
                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), m_UserId);
                            Cmd.Parameters.AddWithValue("@LastModification", DateTime.Now);
                        }

                        #endregion

                        foreach (XmlNode subNode in node.ChildNodes)
                        {
                            translations = GetMapping(string.Concat(objectName, ".", subNode.Name));
                            tableName = null;
                            fieldName = null;

                            if (translations == null)
                            {
                                tableName = string.Concat(objectName, "s");
                                fieldName = subNode.Name;

                                if (fieldshelper.ContainsKey(tableName))
                                    fieldshelper[tableName].Add(fieldName);
                                else
                                    fieldshelper.Add(tableName, new List<string>() { fieldName });
                            }
                            else
                            {
                                foreach (string translation in translations)
                                {
                                    string[] strArr = translation.Split('.');
                                    tableName = strArr[0];
                                    if (strArr.Length > 1)
                                        fieldName = strArr[1];
                                    else
                                        fieldName = subNode.Name;

                                    if (fieldshelper.ContainsKey(tableName))
                                        fieldshelper[tableName].Add(fieldName);
                                    else
                                        fieldshelper.Add(tableName, new List<string>() { fieldName });

                                }
                            }

                            //string strMustDelete = null;

                            //if (string.IsNullOrEmpty(subNode.InnerText) && !string.IsNullOrEmpty(strMustDelete = GetMustDelete(string.Concat(objectName, ".", subNode.Name))))
                            //    mustDelete.Add(strMustDelete);

                            XmlAttribute NilAttr = subNode.Attributes == null ? null : subNode.Attributes["nil"];

                            if (NilAttr != null && bool.Parse(NilAttr.Value))
                            {
                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), DBNull.Value);
                            }
                            else
                            {
                                if (subNode.Attributes.GetNamedItem("type") != null)
                                {
                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                    {
                                        switch (subNode.Attributes["type"].Value)
                                        {
                                            case "Int32":
                                                if (fieldName.StartsWith("-"))
                                                    Cmd.Parameters.AddWithValue(string.Concat("@", fieldName.Substring(1)), -System.Xml.XmlConvert.ToInt32(subNode.InnerText));
                                                else
                                                    Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.Xml.XmlConvert.ToInt32(subNode.InnerText));
                                                break;
                                            case "Single":
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.Xml.XmlConvert.ToSingle(subNode.InnerText));
                                                break;
                                            case "Boolean":
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), System.Xml.XmlConvert.ToBoolean(subNode.InnerText));
                                                break;
                                            case "XmlDocument":
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), subNode.InnerText);
                                                break;
                                            default:
                                                System.Diagnostics.Trace.WriteLine(string.Format("Type {0} is not handled!", subNode.Attributes["type"].Value), "ServerLink");
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName)))
                                        Cmd.Parameters.AddWithValue(string.Concat("@", fieldName), subNode.InnerText);
                                }
                            }
                        }

                        List<string> queries = new List<string>();

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            if (att.Name.EndsWith("id"))
                            {
                                translations = GetMapping(string.Concat(objectName, ".", att.Name));
                                tableName = null;
                                fieldName = null;

                                if (translations == null)
                                {
                                    tableName = string.Concat(objectName, "s");
                                    fieldName = att.Name;

                                    if (idshelper.ContainsKey(tableName))
                                        idshelper[tableName].Add(fieldName);
                                    else
                                        idshelper.Add(tableName, new List<string>() { fieldName });

                                    if (!Cmd.Parameters.Contains(string.Concat("@", fieldName, "_id")))
                                        if (string.IsNullOrEmpty(att.Value))
                                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName, "_id"), System.DBNull.Value);
                                        else
                                            Cmd.Parameters.AddWithValue(string.Concat("@", fieldName, "_id"), att.Value);
                                }
                                else
                                {
                                    foreach (string translation in translations)
                                    {
                                        string[] strArr = translation.Split('.');
                                        tableName = strArr[0];
                                        if (strArr.Length > 1)
                                            fieldName = strArr[1];
                                        else
                                            fieldName = att.Name;

                                        if (idshelper.ContainsKey(tableName))
                                            idshelper[tableName].Add(fieldName);
                                        else
                                            idshelper.Add(tableName, new List<string>() { fieldName });

                                        if (!Cmd.Parameters.Contains(string.Concat("@", fieldName, "_id")))
                                            if (string.IsNullOrEmpty(att.Value))
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName, "_id"), System.DBNull.Value);
                                            else
                                                Cmd.Parameters.AddWithValue(string.Concat("@", fieldName, "_id"), att.Value);
                                    }
                                }
                            }
                        }

                        Cmd.Parameters.AddWithValue("@null", System.DBNull.Value);



                        foreach (KeyValuePair<string, List<string>> kvpi in fieldshelper)
                        {
                            foreach (KeyValuePair<string, List<string>> kvpj in idshelper)
                            {
                                if (kvpj.Key.Equals(kvpi.Key))
                                {
                                    //Dictionary<string, string> added = CheckRequiredFields(idshelper.ElementAt(j).Key, idshelper.ElementAt(j).Value, idshelper);

                                    List<string> fieldNames = new List<string>(kvpj.Value/*.Union(added.Keys)*/);
                                    List<string> paramNames = new List<string>(kvpj.Value/*.Union(added.Values)*/);


                                    List<string> whereParts = new List<string>();
                                    for (int k = 0; k < fieldNames.Count; k++)
                                        whereParts.Add(string.Concat("[", fieldNames[k], "]=@", string.Concat(paramNames[k], "_id")));

                                    List<string> updateParts = new List<string>();
                                    foreach (string str in kvpi.Value)
                                    {
                                        if (!str.Equals("Active") || TablesWithActiveFlag.Contains(kvpi.Key))
                                        {
                                            if (str.StartsWith("-"))
                                            {
                                                updateParts.Add(string.Concat("[", str.Substring(1), "]=@", str.Substring(1)));
                                                invertedTable = kvpi.Key;
                                                invertedField = str.Substring(1);
                                            }
                                            else
                                                updateParts.Add(string.Concat("[", str, "]=@", str));
                                        }
                                    }



                                    Cmd.CommandText = string.Format("SET ANSI_NULLS OFF;update {0} set {1} where {2}", kvpi.Key, string.Join(",", updateParts.ToArray()), string.Join(" AND ", whereParts.ToArray()));


                                    List<string> desc = new List<string>();
                                    foreach (SqlParameter sp in Cmd.Parameters)
                                        if (!sp.ParameterName.Equals("@null")) desc.Add(string.Format("{0}:{1}", sp.ParameterName, sp.SqlValue.ToString()));
                                    Trace(string.Format("{0} | {1}", Cmd.CommandText, string.Join(", ", desc.ToArray())), "ServerLink");

                                    bool breakHere = false;
                                    if (kvpi.Key.Equals("Activities"))
                                    {
                                        for (int i = 0; i < updateParts.Count; i++)
                                        {
                                            if (updateParts[i].Equals("[CampaignId]") && (Cmd.Parameters["@CampaignId"].Value == null || Cmd.Parameters["@CampaignId"].Value == DBNull.Value))
                                            {
                                                Trace(string.Format("Rejecting command resetting the campaignid!"), "ServerLink");
                                                breakHere = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (breakHere)
                                        break;



                                    if (Cmd.ExecuteNonQuery() == 0)
                                    {
                                        if (tableName.Equals("ClosedActions"))
                                        {
                                            Cmd.CommandText = GenerateInsertQuery(Cmd.CommandText);

                                            desc.Clear();
                                            foreach (SqlParameter sp in Cmd.Parameters)
                                                if (!sp.ParameterName.Equals("@null")) desc.Add(string.Format("{0}:{1}", sp.ParameterName, sp.SqlValue.ToString()));
                                            Trace(string.Format("{0} | {1}", Cmd.CommandText, string.Join(", ", desc.ToArray())), "ServerLink");

                                            Cmd.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            Trace(string.Format("Update failed!"), "ServerLink");
                                        }
                                    }
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                    if (invertedField != null)
                    {
                        return string.Format("UPDATE {0} SET {1}=-{1} WHERE {1}<0", invertedTable, invertedField);
                    }
                    #endregion
                    break;

                default:
                    Trace(string.Format("Operation {0} is not handled!", node.Attributes["operation"].Value), "ServerLink");
                    break;
            }
            return null;
        }

        private List<string> GetMapping(string str)
        {
            if (FieldsMappings.ContainsKey(str))
                return FieldsMappings[str];
            str = str.Split('.')[0];
            if (FieldsMappings.ContainsKey(str))
                return FieldsMappings[str];
            return null;
        }

        private string GenerateInsertQuery(string updateQuery)
        {
            // update query is in the form 
            // update tablename set field1=@param1,field2=@param2 where id1=@paramid1 AND id2=@param2
            string[] split = updateQuery.Split(new string[] { "SET ANSI_NULLS OFF;update ", " set ", " where " }, StringSplitOptions.RemoveEmptyEntries);
            string[] fieldaffectations = split[1].Split(',');
            string[] whereparts = split[2].Split(new string[] { " AND " }, StringSplitOptions.None);

            List<string> fields = new List<string>();
            List<string> parms = new List<string>();

            foreach (string str in fieldaffectations)
            {
                string[] temp = str.Split('=');
                fields.Add(temp[0]);
                parms.Add(temp[1]);
            }
            foreach (string str in whereparts)
            {
                string[] temp = str.Split('=');
                fields.Add(temp[0]);
                parms.Add(temp[1]);
            }

            return string.Format("SET ANSI_NULLS OFF;insert into {0} ({1}) values ({2})", split[0], string.Join(",", fields.ToArray()), string.Join(",", parms.ToArray()));

        }

        private object GetFieldValue(string connectionString, string tableName, string fieldName, string id)
        {
            try
            {
                using (SqlConnection C = new SqlConnection(connectionString))
                {
                    C.Open();
                    using (SqlCommand cmd = C.CreateCommand())
                    {
                        cmd.CommandText = string.Format("Select {0} from {1} where Id = @Id", fieldName, tableName);
                        cmd.Parameters.AddWithValue("@Id", id);
                        object obj = cmd.ExecuteScalar();
                        if (obj != null && obj == System.DBNull.Value)
                            return null;
                        return obj;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private string GetOldPlanningValue(string connectionString, string id)
        {
            try
            {
                using (SqlConnection C = new SqlConnection(connectionString))
                {
                    C.Open();
                    using (SqlCommand cmd = C.CreateCommand())
                    {
                        cmd.CommandText = "Select planningid from ClosedActions where ActivityId = @Id and PlanningId is not null";
                        cmd.Parameters.AddWithValue("@Id", id);
                        object obj = cmd.ExecuteScalar();
                        if (obj != null && obj == System.DBNull.Value)
                            return null;
                        return obj as string;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private void CleanFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            m_needSoundReload = true;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                if (path.Contains("{0}"))
                    path = string.Format(path, m_Host);
                RequestServerDelete(path);
            }
            else
            {
                path = path.Replace("/", "\\");
                path = Uri.UnescapeDataString(path);

                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        Trace(string.Format("File {0} deleted", path), "ServerLink");

                    }
                }
                catch (Exception ex)
                {
                    Trace(ex.ToString(), "ServerLink");
                }
                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(path)) && Directory.GetFiles(Path.GetDirectoryName(path)).Length == 0)
                    {
                        Directory.Delete(Path.GetDirectoryName(path));
                        Trace(string.Format("Directory {0} deleted", Path.GetDirectoryName(path)), "ServerLink");
                    }
                }
                catch
                {
                }
            }
        }

        private string ExtractIPFromBaseUri(string baseUri)
        {
            try
            {
                string withoutsip = baseUri.Split(new string[] { "sip:" }, StringSplitOptions.RemoveEmptyEntries)[0];
                return withoutsip.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            catch
            {
            }
            return null;
        }

        private string ComputeMohPath(string id, string path)
        {
            return string.Concat(m_PathMoh, id, @"/", id, System.IO.Path.GetExtension(path));
        }

        private string ComputeFilePath(string name)
        {
            return string.Concat(m_PathConfigs, name, ".cfg");
        }

        private void SaveParamsFile(string name, string strContent)
        {
            m_needSoundReload = true;
            string complete = ComputeFilePath(name);
            if (complete.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                UploadContent(Encoding.Default.GetBytes(strContent), complete);
            }
            else
            {
                complete = complete.Replace("/", "\\");
                complete = Uri.UnescapeDataString(complete);


                try
                {
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(complete)))
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(complete));
                }
                catch
                {
                }
                File.WriteAllBytes(complete, Encoding.Default.GetBytes(strContent));
                Trace(string.Format("Settings file written to {0}", complete), "ServerLink");
            }
        }


        private void CopyMoh(string id, string path)
        {
            m_needSoundReload = true;
            string mohPath = ComputeMohPath(id, path);
            if (m_PathPublicUpload.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                int start = m_PathPublicUpload.Length;
                RequestServerCopy(path.Substring(start), mohPath.Substring(start));
            }
            else
            {
                mohPath = mohPath.Replace("/", "\\");
                mohPath = Uri.UnescapeDataString(mohPath);

                path = string.Concat(m_PathLocalUpload, path.Substring(path.IndexOf(m_PathPublicUpload) + m_PathPublicUpload.Length));
                path = path.Replace("/", "\\");
                path = Uri.UnescapeDataString(path);

                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(mohPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(mohPath));
                }
                catch
                {
                }
                try
                {
                    File.Copy(path, mohPath);
                    Trace(string.Format("File copied from {0} to {1}", path, mohPath), "ServerLink");
                }
                catch (Exception ex)
                {
                    Trace(ex.ToString(), "ServerLink");
                }

            }
        }


        public static long UpdateNumbersFormat(string connectionString, string campaignid, string systemDataTableName, string oldFormat, string newFormat, HandleNumberFormatDelegate handleNumberFormat, progressReportDelegate progress, string culture)
        {
            using (SqlConnection C = new SqlConnection(connectionString))
            {
                long affected = 0;
                int lastProgress = 0;
                long totalLines = 0;
                long currentPosition = 0;
                DateTime lastProgressTime = DateTime.Now;


                if (progress != null)
                {
                    progress(-1, Translate("Initializing....", culture, connectionString), string.Empty);
                }

                C.Open();

                try
                {
                    XmlNodeList list = null;
                    List<string> phoneNums = new List<string>();
                    using (SqlCommand cmd = C.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select FieldsConfig from Campaigns where Id = '{0}'", campaignid);

                        Trace(cmd.CommandText, "ServerLink");

                        object obj = cmd.ExecuteScalar();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(obj as string);
                        if (doc != null)
                        {
                            list = doc.SelectNodes(@"FieldsConfig/FieldConfig[Meaning=""4"" or Meaning=""5"" or Meaning=""6"" or Meaning=""7"" or Meaning=""12""]");
                            if (list != null && list.Count > 0)
                                foreach (XmlNode nde in list)
                                    phoneNums.Add(nde.SelectSingleNode("NewFieldName").InnerText);
                        }
                    }


                    if (phoneNums.Count > 0)
                    {
                        using (SqlConnection C2 = new SqlConnection(connectionString))
                        {
                            C2.Open();
                            using (SqlCommand updateCmd = C2.CreateCommand())
                            {
                                List<string> tempList = new List<string>();
                                int counter = 0;
                                foreach (string str in phoneNums)
                                {
                                    tempList.Add(string.Format("{0}=@prm{1}", str, counter));
                                    updateCmd.Parameters.AddWithValue(string.Format("@prm{0}", counter), DBNull.Value);
                                    counter++;
                                }

                                updateCmd.Parameters.AddWithValue("@internal__id__", DBNull.Value);

                                updateCmd.CommandText = string.Format("update data_{0}..data set {1} where internal__id__ =@internal__id__", campaignid, string.Join(", ", tempList.ToArray()));


                                using (SqlCommand cmd = C.CreateCommand())
                                {
                                    cmd.CommandText = string.Format("select count(*) from data_{0}..Data", campaignid);
                                    try
                                    {
                                        totalLines = (int)cmd.ExecuteScalar();
                                    }
                                    catch
                                    {
                                    }


                                    if (progress != null)
                                        progress(-1, Translate("Updating data...", culture, connectionString), string.Format(Translate("{0}/{1} records", culture, connectionString), currentPosition, totalLines));


                                    cmd.CommandText = string.Format("select {0}, internal__id__ from data_{1}..Data", string.Join(", ", phoneNums.ToArray()), campaignid);


                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            try
                                            {
                                                string tempConverted = null;
                                                for (int i = 0; i < reader.FieldCount - 1; i++)
                                                {
                                                    tempConverted = handleNumberFormat(reader[i] as string, oldFormat, newFormat);
                                                    if (tempConverted == null)
                                                        updateCmd.Parameters[i].Value = DBNull.Value;
                                                    else
                                                        updateCmd.Parameters[i].Value = tempConverted;
                                                }

                                                updateCmd.Parameters[reader.FieldCount - 1].Value = reader[reader.FieldCount - 1];

                                                List<string> desc = new List<string>();
                                                foreach (SqlParameter sp in updateCmd.Parameters)
                                                    if (!sp.ParameterName.Equals("@null")) desc.Add(string.Format("{0}:{1}", sp.ParameterName, sp.SqlValue.ToString()));
                                                Trace(string.Format("{0} | {1}", updateCmd.CommandText, string.Join(", ", desc.ToArray())), "ServerLink");

                                                updateCmd.ExecuteNonQuery();


                                                currentPosition++;


                                                if (progress != null)
                                                {
                                                    long temp = currentPosition * 100 / totalLines;
                                                    if (temp > lastProgress || DateTime.Now.Subtract(lastProgressTime).TotalMilliseconds > 2000)
                                                    {
                                                        lastProgress = (int)temp;
                                                        lastProgressTime = DateTime.Now;
                                                        progress(lastProgress, Translate("Updating data...", culture, connectionString), string.Format(Translate("{0}/{1} records", culture, connectionString), currentPosition, totalLines));
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Trace(ex.ToString(), "ServerLink");
                                            }
                                        }
                                    }



                                    affected += currentPosition;


                                    lastProgress = 0;
                                    totalLines = 0;
                                    currentPosition = 0;
                                    lastProgressTime = DateTime.Now;

                                    cmd.CommandText = string.Format("select count(*) from data_{0}..[{1}] where TargetDestination is not null and targetDestination <>''", campaignid, systemDataTableName);
                                    try
                                    {
                                        totalLines = (int)cmd.ExecuteScalar();
                                    }
                                    catch
                                    {
                                    }



                                    if (progress != null)
                                        progress(-1, Translate("Updating system data...", culture, connectionString), string.Format(Translate("{0}/{1} records", culture, connectionString), currentPosition, totalLines));


                                    cmd.CommandText = string.Format("select TargetDestination, internal__id__ from data_{0}..{1} where TargetDestination is not null and targetDestination <>''", campaignid, systemDataTableName);

                                    updateCmd.CommandText = string.Format("update data_{0}..{1} set TargetDestination = @TargetDestination where internal__id__ = @internal__id__", campaignid, systemDataTableName);
                                    updateCmd.Parameters.Clear();
                                    updateCmd.Parameters.AddWithValue("@TargetDestination", DBNull.Value);
                                    updateCmd.Parameters.AddWithValue("@internal__id__", DBNull.Value);

                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            try
                                            {
                                                string tempConverted = handleNumberFormat(reader[0] as string, oldFormat, newFormat);
                                                if (tempConverted == null)
                                                    updateCmd.Parameters["@TargetDestination"].Value = DBNull.Value;
                                                else
                                                    updateCmd.Parameters["@TargetDestination"].Value = tempConverted;

                                                updateCmd.Parameters["@internal__id__"].Value = reader[1];


                                                List<string> desc = new List<string>();
                                                foreach (SqlParameter sp in updateCmd.Parameters)
                                                    if (!sp.ParameterName.Equals("@null")) desc.Add(string.Format("{0}:{1}", sp.ParameterName, sp.SqlValue.ToString()));
                                                Trace(string.Format("{0} | {1}", updateCmd.CommandText, string.Join(", ", desc.ToArray())), "ServerLink");

                                                updateCmd.ExecuteNonQuery();

                                                currentPosition++;


                                                if (progress != null)
                                                {
                                                    long temp = currentPosition * 100 / totalLines;
                                                    if (temp > lastProgress || DateTime.Now.Subtract(lastProgressTime).TotalMilliseconds > 2000)
                                                    {
                                                        lastProgress = (int)temp;
                                                        lastProgressTime = DateTime.Now;
                                                        progress(lastProgress, Translate("Updating system data...", culture, connectionString), string.Format(Translate("{0}/{1} records", culture, connectionString), currentPosition, totalLines));
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Trace(ex.ToString(), "ServerLink");
                                            }
                                        }
                                    }
                                    affected += currentPosition;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Trace(ex2.ToString(), "ServerLink");
                }
                return affected;
            }

        }

        private void EnsureCampaignDataBase(string campaignid, SqlConnection C)
        {
            bool mustCreate = false;
            using (SqlCommand cmd = C.CreateCommand())
            {
                cmd.CommandText = string.Concat("select * from master..sysdatabases where name ='Data_", campaignid, "'");
                object retVal = cmd.ExecuteScalar();
                mustCreate = (retVal == null || retVal == System.DBNull.Value);
            }

            if (mustCreate)
            {
                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = string.Concat("create database Data_", campaignid);

                    Trace(cmd.CommandText, "ServerLink");

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void EnsureCampaignDataStructure(XmlNode node, SqlConnection C, string systemDataTableName)
        {
            XmlAttribute att = node.Attributes["campaignid"];
            if (att == null)
                return;

            string backupDb = C.Database;

            bool mustCreate = false;

            EnsureCampaignSystemDataStructure(att.Value, C, systemDataTableName);

            C.ChangeDatabase(string.Concat("Data_", att.Value));

            using (SqlCommand cmd = C.CreateCommand())
            {
                cmd.CommandText = "select object_id ('Data', 'table')";
                object retVal = cmd.ExecuteScalar();
                mustCreate = (retVal == null || retVal == System.DBNull.Value);
            }
            if (mustCreate)
            {
                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"CREATE TABLE Data(
                        [Internal__id__] [char](32) COLLATE Latin1_General_BIN NOT NULL 
                        CONSTRAINT [PK_Data_{0}] PRIMARY KEY CLUSTERED
                        (
                            [Internal__id__] ASC
                        )WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]) 
                        ON [PRIMARY]", att.Value);

                    Trace(cmd.CommandText, "ServerLink");

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = string.Format(@"ALTER TABLE [{0}] WITH NOCHECK ADD CONSTRAINT [FK_{0}_Data] FOREIGN KEY (Internal__id__) REFERENCES Data (Internal__id__) ON DELETE CASCADE", systemDataTableName);

                    Trace(cmd.CommandText, "ServerLink");

                    cmd.ExecuteNonQuery();


                    #region progress report

                    StringBuilder tempBuilder = new StringBuilder(100000);
                    tempBuilder.AppendLine("CREATE PROCEDURE [dbo].[ProgressReport]");
                    tempBuilder.AppendLine("AS");
                    tempBuilder.AppendLine("BEGIN");
                    tempBuilder.AppendLine("SET NOCOUNT ON;");
                    tempBuilder.AppendLine("if (SELECT OBJECT_ID ('Data')) is null");
                    tempBuilder.AppendLine("begin");
                    tempBuilder.AppendLine("select");
                    tempBuilder.AppendLine("sum(counter) Total,");
                    tempBuilder.AppendLine("sum(HandledByAgent) HandledByAgent ,");
                    tempBuilder.AppendLine("sum(DoNotRecall) DoNotRecall,");
                    tempBuilder.AppendLine("sum(Fresh) Fresh,");
                    tempBuilder.AppendLine("sum(ToRedial) ToRedial,");
                    tempBuilder.AppendLine("sum(ToBeCalledBack) ToBeCalledBack,");
                    tempBuilder.AppendLine("sum(ToBeCalledBackForSpecificAgent) ToBeCalledBackForSpecificAgent,");
                    tempBuilder.AppendLine("sum(SystemLocked) SystemLocked,");
                    tempBuilder.AppendLine("sum(UserQualified) UserQualified,");
                    tempBuilder.AppendLine("sum(SystemQualifiedExceptCallbacks) SystemQualifiedExceptCallbacks,");
                    tempBuilder.AppendLine("sum(CountPositive) CountPositive,");
                    tempBuilder.AppendLine("sum(CountNegative) CountNegative,");
                    tempBuilder.AppendLine("sum(SumPositive) SumPositive,");
                    tempBuilder.AppendLine("sum(SumNegative) SumNegative,");
                    tempBuilder.AppendLine("sum(argued) argued,");
                    tempBuilder.AppendLine("sum(fax) faxes,");
                    tempBuilder.AppendLine("sum(disturbed) disturbed,");
                    tempBuilder.AppendLine("sum(noanswer) noanswer,");
                    tempBuilder.AppendLine("sum(answeringmachine) answeringmachine,");
                    tempBuilder.AppendLine("sum(busy) busy,");
                    tempBuilder.AppendLine("sum(abandon) abandon,");
                    tempBuilder.AppendLine("sum(ToNotRedialMaxDialAttempts) ToNotRedialMaxDialAttempts,");
                    tempBuilder.AppendLine("sum(Excluded) Excluded");
                    tempBuilder.AppendLine("from");
                    tempBuilder.AppendLine("(");
                    tempBuilder.AppendLine("select");
                    tempBuilder.AppendLine("1 as Counter,");

                    tempBuilder.AppendLine("(case when (sd.excluded = 1) then 1 else 0 end) Excluded,");

                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.State=14 then 1 else 0 end) SystemLocked,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.currentActivity like '_AGT' and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) HandledByAgent,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.currentActivity like '_DNR' and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) DoNotRecall,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.totalDialed=0 and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) Fresh,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.totalDialed<>0 and (sd.State is null or sd.state<>14 ) and (sd.currentActivity is null or (sd.currentActivity not like '_AGT' and sd.currentActivity not like '_DNR')) and (sd.State is null or sd.State<>15 ) and (sd.MaxDialAttempts is null or sd.MaxDialAttempts<0 or sd.MaxDialAttempts > sd.TotalDialed) then 1 else 0 end ) ToRedial,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and (sd.currentActivity is null or substring(sd.CurrentActivity,1,1) <> '_' ) and sd.State=15 and sd.targetHandler is null and (sd.MaxDialAttempts is null or sd.MaxDialAttempts<0 or sd.MaxDialAttempts > sd.TotalDialed) then 1 else 0 end ) ToBeCalledBack,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and (sd.totalDialed<>0 and (sd.State is null or sd.state<>14 ) and (sd.currentActivity is null or (sd.currentActivity not like '_AGT' and sd.currentActivity not like '_DNR')) and (sd.State is null or sd.State<>15 ) and (sd.MaxDialAttempts > 0 and sd.MaxDialAttempts <= sd.TotalDialed) ) or ( (sd.currentActivity is null or substring(sd.CurrentActivity,1,1) <> '_' ) and sd.State=15 and sd.targetHandler is null and (sd.MaxDialAttempts > 0 and sd.MaxDialAttempts < sd.TotalDialed) ) then 1 else 0 end ) ToNotRedialMaxDialAttempts,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and (sd.currentActivity is null or substring(sd.CurrentActivity,1,1) <> '_' ) and sd.State=15 and sd.targetHandler is not null then 1 else 0 end ) ToBeCalledBackForSpecificAgent,");

                    tempBuilder.AppendLine("(case when q.Id is not null and (sd.State is null or sd.state<>14 ) and (q.SystemMapping is null or q.SystemMapping =0) then 1 else 0 end ) UserQualified,");
                    tempBuilder.AppendLine("(case when (sd.State is null or sd.State<>15) and (sd.State is null or sd.state<>14 ) and q.Id is not null and q.SystemMapping is not null and q.SystemMapping>0 then 1 else 0 end ) SystemQualifiedExceptCallbacks,");
                    tempBuilder.AppendLine("(case when q.Positive>0 and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as CountPositive,");
                    tempBuilder.AppendLine("(case when q.Positive<0 and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as CountNegative,");
                    tempBuilder.AppendLine("(case when q.Positive>0 and (sd.State is null or sd.state<>14 ) then q.Positive else 0 end) as SumPositive,");
                    tempBuilder.AppendLine("(case when q.Positive<0 and (sd.State is null or sd.state<>14 ) then q.Positive else 0 end) as SumNegative,");
                    tempBuilder.AppendLine("(case when q.Argued=1 and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) as Argued,");
                    tempBuilder.AppendLine("(case when (sd.State = 1 or (q.Id is not null and q.SystemMapping=1 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Fax,");
                    tempBuilder.AppendLine("(case when (sd.State = 2 or (q.Id is not null and q.SystemMapping=2 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Disturbed,");
                    tempBuilder.AppendLine("(case when (sd.State = 3 or (q.Id is not null and q.SystemMapping=3 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as NoAnswer,");
                    tempBuilder.AppendLine("(case when (sd.State = 4 or (q.Id is not null and q.SystemMapping=4 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as AnsweringMachine,");
                    tempBuilder.AppendLine("(case when (sd.State = 5 or (q.Id is not null and q.SystemMapping=5 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Busy,");
                    tempBuilder.AppendLine("(case when (sd.State = 6 or (q.Id is not null and q.SystemMapping=6 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Abandon");
                    tempBuilder.AppendLine("from");
                    tempBuilder.AppendFormat("[{0}] sd", systemDataTableName);
                    tempBuilder.AppendFormat("left join {0}[{1}].dbo.vwQualifications q on sd.LastQualification=q.Id", m_adminDbPrefix, backupDb);
                    tempBuilder.AppendLine(") temp");
                    tempBuilder.AppendLine("end");
                    tempBuilder.AppendLine("ELSE");
                    tempBuilder.AppendLine("begin");
                    tempBuilder.AppendLine("select");
                    tempBuilder.AppendLine("sum(counter) Total,");
                    tempBuilder.AppendLine("sum(HandledByAgent) HandledByAgent ,");
                    tempBuilder.AppendLine("sum(DoNotRecall) DoNotRecall,");
                    tempBuilder.AppendLine("sum(Fresh) Fresh,");
                    tempBuilder.AppendLine("sum(ToRedial) ToRedial,");
                    tempBuilder.AppendLine("sum(ToBeCalledBack) ToBeCalledBack,");
                    tempBuilder.AppendLine("sum(ToBeCalledBackForSpecificAgent) ToBeCalledBackForSpecificAgent,");
                    tempBuilder.AppendLine("sum(SystemLocked) SystemLocked,");
                    tempBuilder.AppendLine("sum(UserQualified) UserQualified,");
                    tempBuilder.AppendLine("sum(SystemQualifiedExceptCallbacks) SystemQualifiedExceptCallbacks,");
                    tempBuilder.AppendLine("sum(CountPositive) CountPositive,");
                    tempBuilder.AppendLine("sum(CountNegative) CountNegative,");
                    tempBuilder.AppendLine("sum(SumPositive) SumPositive,");
                    tempBuilder.AppendLine("sum(SumNegative) SumNegative,");
                    tempBuilder.AppendLine("sum(argued) argued,");
                    tempBuilder.AppendLine("sum(fax) faxes,");
                    tempBuilder.AppendLine("sum(disturbed) disturbed,");
                    tempBuilder.AppendLine("sum(noanswer) noanswer,");
                    tempBuilder.AppendLine("sum(answeringmachine) answeringmachine,");
                    tempBuilder.AppendLine("sum(busy) busy,");
                    tempBuilder.AppendLine("sum(abandon) abandon,");
                    tempBuilder.AppendLine("sum(ToNotRedialMaxDialAttempts) ToNotRedialMaxDialAttempts,");
                    tempBuilder.AppendLine("sum(Excluded) Excluded");
                    tempBuilder.AppendLine("from");
                    tempBuilder.AppendLine("(");
                    tempBuilder.AppendLine("select");
                    tempBuilder.AppendLine("1 as Counter,");

                    tempBuilder.AppendLine("(case when (sd.excluded = 1) then 1 else 0 end) Excluded,");

                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.State=14 then 1 else 0 end) SystemLocked,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.currentActivity like '_AGT' and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) HandledByAgent,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.currentActivity like '_DNR' and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) DoNotRecall,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.totalDialed=0 and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) Fresh,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and sd.totalDialed<>0 and (sd.State is null or sd.state<>14 ) and (sd.currentActivity is null or (sd.currentActivity not like '_AGT' and sd.currentActivity not like '_DNR')) and (sd.State is null or sd.State<>15 ) and (sd.MaxDialAttempts is null or sd.MaxDialAttempts<0 or sd.MaxDialAttempts > sd.TotalDialed) then 1 else 0 end ) ToRedial,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and (sd.currentActivity is null or substring(sd.CurrentActivity,1,1) <> '_' ) and sd.State=15 and sd.targetHandler is null and (sd.MaxDialAttempts is null or sd.MaxDialAttempts<0 or sd.MaxDialAttempts > sd.TotalDialed) then 1 else 0 end ) ToBeCalledBack,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and (sd.totalDialed<>0 and (sd.State is null or sd.state<>14 ) and (sd.currentActivity is null or (sd.currentActivity not like '_AGT' and sd.currentActivity not like '_DNR')) and (sd.State is null or sd.State<>15 ) and (sd.MaxDialAttempts > 0 and sd.MaxDialAttempts <= sd.TotalDialed) ) or ( (sd.currentActivity is null or substring(sd.CurrentActivity,1,1) <> '_' ) and sd.State=15 and sd.targetHandler is null and (sd.MaxDialAttempts > 0 and sd.MaxDialAttempts < sd.TotalDialed) ) then 1 else 0 end ) ToNotRedialMaxDialAttempts,");
                    tempBuilder.AppendLine("(case when (sd.excluded is null or sd.excluded <> 1) and (sd.currentActivity is null or substring(sd.CurrentActivity,1,1) <> '_' ) and sd.State=15 and sd.targetHandler is not null then 1 else 0 end ) ToBeCalledBackForSpecificAgent,");

                    tempBuilder.AppendLine("(case when q.Id is not null and (sd.State is null or sd.state<>14 ) and (q.SystemMapping is null or q.SystemMapping =0) then 1 else 0 end ) UserQualified,");
                    tempBuilder.AppendLine("(case when (sd.State is null or sd.State<>15) and (sd.State is null or sd.state<>14 ) and q.Id is not null and q.SystemMapping is not null and q.SystemMapping>0 then 1 else 0 end ) SystemQualifiedExceptCallbacks,");
                    tempBuilder.AppendLine("(case when q.Positive>0 and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as CountPositive,");
                    tempBuilder.AppendLine("(case when q.Positive<0 and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as CountNegative,");
                    tempBuilder.AppendLine("(case when q.Positive>0 and (sd.State is null or sd.state<>14 ) then q.Positive else 0 end) as SumPositive,");
                    tempBuilder.AppendLine("(case when q.Positive<0 and (sd.State is null or sd.state<>14 ) then q.Positive else 0 end) as SumNegative,");
                    tempBuilder.AppendLine("(case when q.Argued=1 and (sd.State is null or sd.state<>14 ) then 1 else 0 end ) as Argued,");
                    tempBuilder.AppendLine("(case when (sd.State = 1 or (q.Id is not null and q.SystemMapping=1 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Fax,");
                    tempBuilder.AppendLine("(case when (sd.State = 2 or (q.Id is not null and q.SystemMapping=2 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Disturbed,");
                    tempBuilder.AppendLine("(case when (sd.State = 3 or (q.Id is not null and q.SystemMapping=3 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as NoAnswer,");
                    tempBuilder.AppendLine("(case when (sd.State = 4 or (q.Id is not null and q.SystemMapping=4 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as AnsweringMachine,");
                    tempBuilder.AppendLine("(case when (sd.State = 5 or (q.Id is not null and q.SystemMapping=5 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Busy,");
                    tempBuilder.AppendLine("(case when (sd.State = 6 or (q.Id is not null and q.SystemMapping=6 and (state is null or state <> 15 ))) and (sd.State is null or sd.state<>14 ) then 1 else 0 end) as Abandon");
                    tempBuilder.AppendLine("from");
                    tempBuilder.AppendFormat("[{0}] sd", systemDataTableName);
                    tempBuilder.AppendLine("inner join Data dt on sd.Internal__Id__=dt.internal__Id__");
                    tempBuilder.AppendFormat("left join {0}[{1}].dbo.vwQualifications q on sd.LastQualification=q.Id", m_adminDbPrefix, backupDb);
                    tempBuilder.AppendLine(") temp");
                    tempBuilder.AppendLine("end");
                    tempBuilder.AppendLine("END");


                    cmd.CommandText = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProgressReport]') AND type in (N'P', N'PC'))DROP PROCEDURE [dbo].[ProgressReport];";
                    Trace(cmd.CommandText, "ServerLink");
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = tempBuilder.ToString();
                    Trace(cmd.CommandText, "ServerLink");
                    cmd.ExecuteNonQuery();

                    #endregion

                }
            }


            switch (node.Attributes["operation"].Value)
            {
                case "create":
                    #region create
                    using (SqlCommand cmd = C.CreateCommand())
                    {

                        string strType = null;

                        switch (node.SelectSingleNode("DBType").InnerText)
                        {
                            case "0": // unknown
                                break;
                            case "1": // Boolean
                                strType = "bit";
                                break;
                            case "2": // Integer 
                                strType = "int";
                                break;
                            case "3": // Datetime
                                strType = "datetime";
                                break;
                            case "4": // Float
                                strType = "float";
                                break;
                            case "5": // char
                                strType = "byte";
                                break;
                            case "6": // string
                                strType = string.Format("nvarchar({0})", node.SelectSingleNode("Length").InnerText.Equals("-1") ? "max" : node.SelectSingleNode("Length").InnerText);
                                break;
                            case "7": // time
                                strType = "datetime";
                                break;
                            case "8": // AgentString
                                strType = "char(32)";
                                break;
                            case "9": // ActivityString
                                strType = "char(32)";
                                break;
                            case "10": // AreaString
                                strType = "char(32)";
                                break;
                            case "11": // qualificationstring
                                strType = "char(32)";
                                break;

                        }
                        XmlNode nde = node.SelectSingleNode("Name");
                        if (nde != null)
                        {
                            cmd.CommandText = string.Format("ALTER TABLE Data Add [{0}] {1}", nde.InnerText, strType);

                            Trace(cmd.CommandText, "ServerLink");
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Trace(ex.ToString(), "ServerLink");
                            }

                            XmlNode nIndexed = node.SelectSingleNode("IsIndexed");
                            if (nIndexed != null)
                            {
                                if (XmlConvert.ToBoolean(nIndexed.InnerText))
                                {
                                    XmlNode nUnique = node.SelectSingleNode("IsUniqueConstraint");
                                    bool unique = false;
                                    if (nUnique != null)
                                    {
                                        unique = XmlConvert.ToBoolean(nUnique.InnerText);
                                    }

                                    cmd.CommandText = string.Format("CREATE {1} NONCLUSTERED INDEX IX_Data_{0} ON dbo.Data([{0}]) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]", nde.InnerText, unique? "UNIQUE": string.Empty );

                                    Trace(cmd.CommandText, "ServerLink");
                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Trace(ex.ToString(), "ServerLink");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Trace(string.Format("There is a userfield without name: {0}", node.OuterXml), "ServerLink");
                        }
                    }
                    #endregion
                    break;

                case "delete":
                    #region delete
                    using (SqlCommand cmd = C.CreateCommand())
                    {
                        cmd.CommandText = string.Format("ALTER TABLE Data DROP COLUMN {0}", node.Attributes["name"].Value);
                        Trace(cmd.CommandText, "ServerLink");
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Trace(ex.ToString(), "ServerLink");
                        }
                    }
                    #endregion
                    break;

                case "update":
                    #region update
                    using (SqlCommand cmd = C.CreateCommand())
                    {

                        string strType = null;
                        if (node.SelectSingleNode("DBType") != null)
                        {
                            switch (node.SelectSingleNode("DBType").InnerText)
                            {
                                case "0": // unknown
                                    break;
                                case "1": // Boolean
                                    strType = "bit";
                                    break;
                                case "2": // Integer 
                                    strType = "int";
                                    break;
                                case "3": // Datetime
                                    strType = "datetime";
                                    break;
                                case "4": // Float
                                    strType = "float";
                                    break;
                                case "5": // char
                                    strType = "byte";
                                    break;
                                case "6": // string
                                    strType = string.Format("nvarchar({0})", node.SelectSingleNode("Length").InnerText.Equals("-1") ? "max" : node.SelectSingleNode("Length").InnerText);
                                    break;
                                case "7": // time
                                    strType = "datetime";
                                    break;
                                case "8": // AgentString
                                    strType = "char(32)";
                                    break;
                                case "9": // ActivityString
                                    strType = "char(32)";
                                    break;
                                case "10": // AreaString
                                    strType = "char(32)";
                                    break;
                                case "11": // qualificationstring
                                    strType = "char(32)";
                                    break;

                            }
                            cmd.CommandText = string.Format("ALTER TABLE Data ALTER COLUMN {0} {1}", node.SelectSingleNode("Name").InnerText, strType);
                            Trace(cmd.CommandText, "ServerLink");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    #endregion
                    break;

                default:
                    Trace(string.Format("Operation {0} is not handled!", node.Attributes["operation"].Value), "ServerLink");
                    break;
            }

            C.ChangeDatabase(backupDb);
        }

        private void EnsureCampaignSystemDataStructure(string campaignid, SqlConnection C, string systemDataTableName)
        {
            EnsureCampaignDataBase(campaignid, C);

            string backupDb = C.Database;

            C.ChangeDatabase(string.Concat("Data_", campaignid));

            bool mustCreate = false;

            using (SqlCommand cmd = C.CreateCommand())
            {
                cmd.CommandText = string.Format("select object_id ('{0}', 'table')", systemDataTableName);
                object retVal = cmd.ExecuteScalar();
                mustCreate = (retVal == null || retVal == System.DBNull.Value);
            }
            if (mustCreate)
            {
                using (SqlCommand cmd = C.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"CREATE TABLE [dbo].[{1}](
	                    [Internal__Id__] [char](32) COLLATE Latin1_General_BIN NOT NULL,
	                    [CurrentActivity] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [PreviousActivity] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [LastHandlerActivity] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [LastContactId] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [LastActivityChange] [datetime] NULL,
	                    [LastHandler] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [LastHandlingTime] [datetime] NULL,
	                    [LastHandlingDuration] [int] NULL,
	                    [TotalHandlingDuration] [int] NULL CONSTRAINT [DF_{0}_TotalHandlingDuration]  DEFAULT ((0)),
	                    [TotalHandlers] [int] NULL CONSTRAINT [DF_{0}_TotalHandlers]  DEFAULT ((0)),
	                    [State] [int] NULL CONSTRAINT [DF_{0}_State]  DEFAULT ((0)),
	                    [PreviousState] [int] NULL CONSTRAINT [DF_{0}_PreviousState]  DEFAULT ((0)),
	                    [SortInfo] [int] NULL CONSTRAINT [DF_{0}_SortInfo]  DEFAULT ((0)),
	                    [CustomSortInfo] [int] NULL CONSTRAINT [DF_{0}_CustomSortInfo]  DEFAULT ((0)),
	                    [Priority]  AS (case when [SortInfo]<(0) OR [CustomSortInfo] IS NULL then [SortInfo] else [CustomSortInfo]*(10000)+[SortInfo] end) PERSISTED,
	                    [DialingModeOverride] [int] NULL,
	                    [DialStartDate] [datetime] NULL,
	                    [DialEndDate] [datetime] NULL,
	                    [CreationTime] [datetime] NULL,
	                    [ImportSequence] [int] NULL,
	                    [ImportTag] [nvarchar](250) COLLATE Latin1_General_CI_AI NULL,
	                    [ExportSequence] [int] NULL,
	                    [RecycleCount] [int] NULL,
	                    [LastRecycle] [datetime] NULL,
	                    [ExportTime] [datetime] NULL,
	                    [TargetHandler] [char](32) COLLATE Latin1_General_BIN NULL,
                        [PreferredAgent] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [TargetDestination] [varchar](100) COLLATE Latin1_General_CI_AI NULL,
	                    [TargetMedia] [int] NULL,
	                    [DialedCurrentActivity] [int] NULL CONSTRAINT [DF_{0}_DialedCurrentActivity]  DEFAULT ((0)),
	                    [TotalDialed] [int] NULL CONSTRAINT [DF_{0}_TotalDialed]  DEFAULT ((0)),
	                    [MaxDialAttempts] [int] NULL CONSTRAINT [DF_{0}_MaxDialAttempts]  DEFAULT ((-1)),
	                    [ExpectedProfit] [int] NULL CONSTRAINT [DF_{0}_ExpectedProfit]  DEFAULT ((1)),
	                    [LastDialStatus] [int] NULL,
	                    [LastDialStatusCount] [int] NULL CONSTRAINT [DF_{0}_LastDialStatusCount]  DEFAULT ((0)),
	                    [LastDialedDestination] [varchar](100) COLLATE Latin1_General_CI_AI NULL,
	                    [LastQualification] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [LastQualificationArgued] [bit] NULL CONSTRAINT [DF_{0}_LastQualificationArgued]  DEFAULT ((0)),
	                    [LastQualificationPositive] [int] NULL CONSTRAINT [DF_{0}_LastQualificationPositive]  DEFAULT ((0)),
	                    [LastQualificationExportable] [bit] NULL CONSTRAINT [DF_{0}_LastQualificationExportable]  DEFAULT ((0)),
	                    [AreaId] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [AppointmentId] [char](32) COLLATE Latin1_General_BIN NULL,
	                    [Excluded] [bit] NULL CONSTRAINT [DF_{0}_Excluded]  DEFAULT ((0)),
	                    [VMFlagged] [int] NULL CONSTRAINT [DF_{0}_VMFlagged]  DEFAULT ((0)),
                     CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
                    (
	                    [Internal__Id__] ASC
                    )WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
                    ) ON [PRIMARY]", campaignid, systemDataTableName);

                    Trace(cmd.CommandText, "ServerLink");

                    cmd.ExecuteNonQuery();

                    #region Dialer stored procedures
                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetStatus]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetStatus](
                        @id char(32),
                        @status int,
                        @lastHandlerActivity char(32),
                        @lastContactId char(32),
                        @destination varchar(100)
                        )
                        AS
                        BEGIN

                        update [{0}] set LastDialStatus=@status, LastDialedDestination=@destination, LastDialStatusCount = case when LastDialStatus is not null and LastDialStatus=@status then LastDialStatusCount + 1 else 1 end, lasthandlerActivity = @lastHandlerActivity, lastcontactid = @lastContactId WHERE Internal__Id__=@id
                        

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetQualification]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetQualification](
                        @id char(32),
                        @qualification char(32),
                        @argued bit,
                        @positive int,
                        @exportable bit
                        )
                        AS
                        BEGIN

                        update [{0}] set LastQualification=@qualification, LastQualificationArgued=@argued, LastQualificationPositive=@positive, LastQualificationExportable=@exportable WHERE Internal__Id__=@id
                        

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetRetryNotBefore]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetRetryNotBefore](
                        @id char(32),
                        @state int,
                        @currentActivity char(32),
                        @now datetime,
                        @dialStartDate datetime,
                        @increaseDialed int,
                        @dialingModeOverride int
                        )
                        AS
                        BEGIN

                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set State=@state, CurrentActivity=case when @currentActivity is null then case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end else @currentActivity end, LastActivityChange = case when substring(@currentActivity,1,1)<>'_' and @currentActivity<>CurrentActivity then @now else LastActivityChange end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, DialStartDate=@dialStartDate, SortInfo=case when SortInfo>=0 then SortInfo+1 else 1 end, TotalDialed = TotalDialed +@increaseDialed,DialedCurrentActivity = DialedCurrentActivity+@increaseDialed , DialingModeOverride=@dialingModeOverride WHERE Internal__Id__=@id else insert into [{0}] (State, CurrentActivity, DialStartDate, SortInfo, TotalDialed, DialedCurrentActivity, DialingModeOverride, Internal__Id__) values (@state, @currentActivity, @dialStartDate, 1,  1, 1 , @dialingModeOverride, @id)

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetNewActivity]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetNewActivity](
                        @id char(32),
                        @state int,
                        @currentActivity char(32),
                        @now datetime,
                        @agent char(32),
                        @lastHandlerActivity char(32)
                        )
                        AS
                        BEGIN

                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set State=@state, DialStartDate=@now, DialEndDate=null, CurrentActivity=@currentActivity, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, DialedCurrentActivity=0, SortInfo=0, LastActivityChange=@now, LastHandler=@agent WHERE Internal__Id__=@id else insert into [SystemData] (State, DialStartDate, DialEndDate, CurrentActivity, DialedCurrentActivity, LastActivityChange, LastHandlerActivity, LastHandler, Internal__Id__) values (@state, null, null, @currentActivity, 0, @now, @lastHandlerActivity, @agent, @id)

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }


                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetDoNotRetry]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetDoNotRetry](
                        @id char(32),
                        @state int,                        
                        @now datetime,
                        @increaseDialed int
                        )
                        AS
                        BEGIN
                            
                        if @state=17 
                        begin
                            update [{0}] set State=@state, DialStartDate=null, DialEndDate=null, CurrentActivity='_MAX', PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, TotalDialed = TotalDialed +@increaseDialed, DialedCurrentActivity= DialedCurrentActivity+@increaseDialed WHERE Internal__Id__=@id
                        end
                        else
                        begin
                            update [{0}] set State=@state, DialStartDate=null, DialEndDate=null, CurrentActivity='_DNR', PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, TotalDialed = TotalDialed +@increaseDialed, DialedCurrentActivity= DialedCurrentActivity+@increaseDialed WHERE Internal__Id__=@id
                        end
                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetDoNotRetryAgent]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetDoNotRetryAgent](
                        @id char(32),
                        @state int,                        
                        @now datetime,
                        @agent char(32),
                        @increaseDialed int
                        )
                        AS
                        BEGIN
                        
                        if @state=17 
                        begin
                            update [{0}] set State=@state, DialStartDate=null, DialEndDate=null, CurrentActivity='_MAX', PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, TotalDialed = TotalDialed +@increaseDialed, DialedCurrentActivity= DialedCurrentActivity+@increaseDialed WHERE Internal__Id__=@id
                        end
                        else
                        begin
                            update [{0}] set State=@state, LastHandler=@agent, LastHandlingTime=@now, DialStartDate=null, DialEndDate=null, CurrentActivity=case when @state=16 then '_PRV' else '_AGT' end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, TotalDialed = case when @state=16 then TotalDialed else TotalDialed +1 end, DialedCurrentActivity = case when @state=16 then DialedCurrentActivity else DialedCurrentActivity+1 end, TotalHandlers = TotalHandlers +1 WHERE Internal__Id__=@id
                        end

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }



                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetRetryAt]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetRetryAt](
                        @id char(32),
                        @state int,      
                        @currentActivity char(32),
                        @now datetime,
                        @dialStartDate datetime,
                        @dialingModeOverride int,
                        @increaseDialed int
                        )
                        AS
                        BEGIN

                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set State=@state, CurrentActivity=case when @currentActivity is null then case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end else @currentActivity end, LastActivityChange = case when substring(@currentActivity,1,1)<>'_' and @currentActivity<>CurrentActivity then @now else LastActivityChange end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end  , DialStartDate=@dialStartDate, SortInfo=-1, TotalDialed = TotalDialed + @increaseDialed,DialedCurrentActivity= DialedCurrentActivity+@increaseDialed , DialingModeOverride=@dialingModeOverride WHERE Internal__Id__=@id else insert into [SystemData] (State, CurrentActivity, DialStartDate, SortInfo, TotalDialed, DialedCurrentActivity, DialingModeOverride, Internal__Id__) values (@state, @currentActivity, @dialStartDate, -1, 1, 1 , @dialingModeOverride, @id)                        

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InternalSetCallback]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[InternalSetCallback](
                        @id char(32),
                        @state int,                           
                        @currentActivity char(32),
                        @previousActivity char(32),
                        @now datetime,
                        @dialStartDate datetime,
                        @dialEndDate datetime,
                        @targetHandler char(32),
                        @targetDestination varchar(100),
                        @dialingModeOverride int,
                        @increaseDialed int
                        )
                        AS
                        BEGIN

                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set State=@state, DialStartDate=@dialStartDate, DialEndDate=@dialEndDate, CurrentActivity=case when @currentActivity is null then case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end else @currentActivity end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, LastActivityChange = case when substring(@currentActivity,1,1)<>'_' and @currentActivity<>CurrentActivity then @now else LastActivityChange end, TargetHandler=@targetHandler, [TargetDestination]=@targetDestination, SortInfo=-2, TotalDialed = TotalDialed + @increaseDialed,DialedCurrentActivity= DialedCurrentActivity+@increaseDialed , DialingModeOverride=@dialingModeOverride WHERE Internal__Id__=@id else insert into [SystemData] (State, DialStartDate, DialEndDate, CurrentActivity, PreviousActivity, TargetHandler, [TargetDestination], SortInfo, DialingModeOverride, Internal__Id__, DialedCurrentActivity, LastActivityChange, LastHandlingTime) values (@state, @dialStartDate, @dialEndDate, @currentActivity, @previousActivity, @targetHandler, @targetDestination, -2, @dialingModeOverride, @id, 0, @now, @now)

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }
                    #endregion

                    #region Public dialer stored procedures
                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetRetryNotBefore]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[SetRetryNotBefore](
                        @id char(32),
                        @agent char(32),
                        @currentActivity char(32),
                        @now datetime,
                        @dialStartDate datetime,
                        @dialingModeOverride int
                        )
                        AS
                        BEGIN
                        if(@now is null)
                        begin
	                        set @now = getdate();
                        end
                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set LastHandlingTime=@now, LastHandler=@agent, State=9, PreviousState=State, TotalHandlers=TotalHandlers+1, CurrentActivity=case when @currentActivity is null then case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end else @currentActivity end, LastActivityChange = case when substring(@currentActivity,1,1)<>'_' and @currentActivity<>CurrentActivity then @now else LastActivityChange end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, DialStartDate=@dialStartDate, SortInfo=case when SortInfo>=0 then SortInfo+1 else 1 end, DialingModeOverride=@dialingModeOverride WHERE Internal__Id__=@id else insert into [SystemData] (LastHandler, TotalHandlers, State, CurrentActivity, DialStartDate, SortInfo, TotalDialed, DialedCurrentActivity, DialingModeOverride, Internal__Id__) values (@agent, 1, 9, @currentActivity, @dialStartDate, 1,  0, 0 , @dialingModeOverride, @id)
                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }


                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetDoNotRetry]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[SetDoNotRetry](
                        @id char(32),
                        @agent char(32),
                        @now datetime
                        )
                        AS
                        BEGIN

                        if(@now is null)
                        begin
	                        set @now = getdate();
                        end

                        if exists (select * from [{0}] where Internal__Id__=@id) update [Systemdata] set LastHandlingTime=@now, LastHandler=@agent, State=9, PreviousState=State, TotalHandlers=TotalHandlers+1, DialStartDate=null, DialEndDate=null, CurrentActivity='_DNR', PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end WHERE Internal__Id__=@id else insert into [SystemData] (LastHandler, TotalHandlers, State, CurrentActivity, TotalDialed, DialedCurrentActivity, Internal__Id__) values (@agent, 1, 9, '_DNR', 0, 0 , @id)

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetRetryAt]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[SetRetryAt](
                        @id char(32),
                        @agent char(32),
                        @currentActivity char(32),
                        @now datetime,
                        @dialStartDate datetime,
                        @dialingModeOverride int
                        )
                        AS
                        BEGIN

                        if(@now is null)
                        begin
	                        set @now = getdate();
                        end

                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set LastHandlingTime=@now, LastHandler=@agent, State=9, PreviousState=State, TotalHandlers=TotalHandlers+1, CurrentActivity=case when @currentActivity is null then case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end else @currentActivity end, LastActivityChange = case when substring(@currentActivity,1,1)<>'_' and @currentActivity<>CurrentActivity then @now else LastActivityChange end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end  , DialStartDate=@dialStartDate, SortInfo=-1, DialingModeOverride=@dialingModeOverride WHERE Internal__Id__=@id else insert into [SystemData] (LastHandler, TotalHandlers, State, CurrentActivity, DialStartDate, SortInfo, TotalDialed, DialedCurrentActivity, DialingModeOverride, Internal__Id__) values (@agent, 1, 9, @currentActivity, @dialStartDate, -1, 0, 0 , @dialingModeOverride, @id)

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }

                    try
                    {
                        cmd.CommandText = "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetCallback]') AND type in (N'P', N'PC')";
                        object retVal = cmd.ExecuteScalar();
                        string CreateOrAlter = null;
                        if (retVal == null || System.DBNull.Value == retVal)
                        {
                            CreateOrAlter = "CREATE";
                        }
                        else
                        {
                            CreateOrAlter = "ALTER";
                        }
                        cmd.CommandText = CreateOrAlter + string.Format( @" PROCEDURE [dbo].[SetCallback](
                        @id char(32),
                        @agent char(32),
                        @currentActivity char(32),
                        @now datetime,
                        @dialStartDate datetime,
                        @dialEndDate datetime,
                        @targetHandler char(32),
                        @targetDestination varchar(100),
                        @dialingModeOverride int
                        )
                        AS
                        BEGIN

                        if(@now is null)
                        begin
	                        set @now = getdate();
                        end

                        if exists (select * from [{0}] where Internal__Id__=@id) update [SystemData] set LastHandlingTime=@now, LastHandler=@agent, State=15, PreviousState=State, TotalHandlers=TotalHandlers+1, DialStartDate=@dialStartDate, DialEndDate=@dialEndDate, CurrentActivity=case when @currentActivity is null then case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end else @currentActivity end, PreviousActivity=case when substring(CurrentActivity,1,1)='_' then PreviousActivity else CurrentActivity end, LastActivityChange = case when substring(@currentActivity,1,1)<>'_' and @currentActivity<>CurrentActivity then @now else LastActivityChange end, TargetHandler=@targetHandler, [TargetDestination]=@targetDestination, SortInfo=-2, DialingModeOverride=@dialingModeOverride WHERE Internal__Id__=@id else insert into [SystemData] (LastHandler, TotalHandlers, State, DialStartDate, DialEndDate, CurrentActivity, TargetHandler, [TargetDestination], SortInfo, DialingModeOverride, Internal__Id__, DialedCurrentActivity, LastActivityChange, LastHandlingTime) values (@agent, 1, 15, @dialStartDate, @dialEndDate, @currentActivity, @targetHandler, @targetDestination, -2, @dialingModeOverride, @id, 0, @now, @now)

                        END", systemDataTableName);

                        cmd.ExecuteNonQuery();

                    }
                    catch
                    {
                    }
                    #endregion

                }
            }

            C.ChangeDatabase(backupDb);

        }

        private void UploadContent(byte[] content, string Path)
        {

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Path);
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Put;

            // not needed when using chunks
            //webRequest.ContentLength = new FileInfo(LocalPath).Length;

            webRequest.SendChunked = true;

            // we don't want OOM!!!
            webRequest.AllowWriteStreamBuffering = false;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                webRequestStream.Write(content, 0, content.Length);
                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Trace(ex.ToString(), "ServerLink");
            }
        }

        private void RequestServerCopy(string source, string destination)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=copy&src={1}&dst={2}", m_PathPublicUpload, source, destination));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Get;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Trace(ex.ToString(), "ServerLink");
            }
        }

        private void RequestServerDelete(string file)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(file);
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = "DELETE";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Trace(ex.ToString(), "ServerLink");
            }
        }


        private static int GetEnumerableCount(IEnumerable e)
        {
            if (e == null)
                return 0;
            int count = 0;
            foreach (object o in e)
                count++;
            return count;
        }

        private static string StringJoin(string separator, IEnumerable<string> strings)
        {
            List<string> lst = new List<string>(strings);
            return string.Join(separator, lst.ToArray());
        }

        private static System.Data.SqlClient.SqlParameter CreateParam(string paramName, string dbtype, object paramValue)
        {
            System.Data.SqlClient.SqlParameter param = new System.Data.SqlClient.SqlParameter();
            param.ParameterName = paramName;
            param.Direction = System.Data.ParameterDirection.Input;
            switch (dbtype)
            {
                case "Boolean":
                    param.Value = paramValue;
                    try
                    {
                        param.Value = System.Convert.ToBoolean(paramValue);
                        param.SqlDbType = System.Data.SqlDbType.Bit;
                    }
                    catch
                    {
                        param.Value = paramValue;
                    }
                    break;
                case "Char":
                    try
                    {
                        param.Value = System.Convert.ToChar(paramValue);
                        param.SqlDbType = System.Data.SqlDbType.Char;
                    }
                    catch
                    {
                        param.Value = paramValue;
                    }
                    break;
                case "Datetime":
                    try
                    {
                        param.Value = System.Convert.ToDateTime(paramValue);
                        param.SqlDbType = System.Data.SqlDbType.DateTime;
                    }
                    catch
                    {
                        param.Value = paramValue;
                    }
                    break;
                case "Float":
                    try
                    {
                        param.Value = System.Convert.ToDouble(paramValue);
                        param.SqlDbType = System.Data.SqlDbType.Float;
                    }
                    catch
                    {
                        param.Value = paramValue;
                    }
                    break;
                case "Integer":
                    try
                    {
                        param.Value = System.Convert.ToInt32(paramValue);
                        param.SqlDbType = System.Data.SqlDbType.Int;
                    }
                    catch
                    {
                        param.Value = paramValue;
                    }
                    break;
                case "String":
                    try
                    {
                        param.Value = System.Convert.ToString(paramValue);
                        param.SqlDbType = System.Data.SqlDbType.VarChar;
                        param.Size = 250;
                    }
                    catch
                    {
                        param.Value = paramValue;
                        param.Size = 250;
                    }
                    break;
                case "Unknown":
                    param.Value = paramValue;
                    break;
                default:
                    break;
            }
            return param;
        }

        private static string GenerateWherePart(SqlCommand cmd, string campaignId, IEnumerable<string> qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            cmd.Parameters.Clear();
            List<string> filters = new List<string>();

            if (qualifications != null && GetEnumerableCount(qualifications) > 0)
            {
                filters.Add(string.Format("s.LastQualification in ('{0}')", StringJoin("','", qualifications)));
            }
            if (excludedonly)
            {
                filters.Add("s.Excluded = 1");
            }
            if (exportableonly)
            {
                filters.Add("s.LastQualificationExportable = 1");
            }
            if (From != null)
            {
                filters.Add("s.LastHandlingTime >= @From");
                cmd.Parameters.AddWithValue("@From", From);
            }
            if (To != null)
            {
                filters.Add("s.LastHandlingTime <= @To");
                cmd.Parameters.AddWithValue("@To", To);
            }
            if (!string.IsNullOrEmpty(tag))
            {
                filters.Add(string.Format("s.ImportTag = '{0}'", tag.Replace("'", "''")));
            }
            if (negativeonly)
            {
                filters.Add("s.LastQualificationPositive < 0");
            }
            if (disconnectReasons != null && GetEnumerableCount(disconnectReasons) > 0)
            {
                string[] tempDR = new string[GetEnumerableCount(disconnectReasons)];
                int i = 0;
                foreach (DialDisconnectionReason ddr in disconnectReasons)
                {
                    tempDR[i] = ((int)(ddr)).ToString();
                    i++;
                }

                filters.Add(string.Format("s.LastDialStatus in ({0})", string.Join(",", tempDR)));
            }
            if (batchnumber > int.MinValue)
            {
                filters.Add(string.Format("s.ImportSequence = {0}", batchnumber));
            }

            if (exportnumber > int.MinValue)
            {
                filters.Add(string.Format("s.ExportSequence = {0}", exportnumber));
            }

            if (notexportedyet)
            {
                filters.Add("s.ExportTime is null");
            }

            if (activities != null && GetEnumerableCount(activities) > 0)
            {
                filters.Add(string.Format("s.CurrentActivity in ('{0}')", StringJoin("','", activities)));
            }

            List<String> sb = new List<string>();

            if (dialStates != null)
            {
                foreach (QualificationAction qualac in dialStates)
                {
                    switch (qualac)
                    {
                        case QualificationAction.BlackList:
                            break;
                        case QualificationAction.Callback:
                            sb.Add("(s.State = 15 and s.targetHandler is null and ( s.currentActivity is null or substring(currentActivity,1,1)<>'_'))");
                            break;
                        case QualificationAction.ChangeActivity:
                            break;
                        case QualificationAction.DoNotRetry:
                            sb.Add("(s.currentActivity = '_DNR')");
                            break;
                        case QualificationAction.None:
                            sb.Add("(s.State = 0 and totaldialed = 0 and ( s.currentActivity is null or substring(currentActivity,1,1)<>'_'))");
                            break;
                        case QualificationAction.RetryAt:
                            sb.Add("(s.priority = -1 and ( s.currentActivity is null or substring(currentActivity,1,1)<>'_'))");
                            break;
                        case QualificationAction.RetryNotBefore:
                            sb.Add("(s.priority >= 0 and dialstartdate is not null and ( s.currentActivity is null or substring(s.currentActivity,1,1)<>'_'))");
                            break;
                        case QualificationAction.TargetedCallback:

                            if (agentids != null)
                            {
                                sb.Add(string.Format("(s.State = 15 and s.targetHandler is not null and ( s.currentActivity is null or substring(currentActivity,1,1)<>'_') and s.targetHandler in ('{0}'))", StringJoin("','", agentids)));
                            }
                            else
                            {
                                sb.Add("(s.State = 15 and s.targetHandler is not null and ( s.currentActivity is null or substring(s.currentActivity,1,1)<>'_'))");
                            }
                            break;
                    }
                }
            }
            if (sb.Count > 0)
            {
                filters.Add(string.Concat("(", StringJoin(" OR ", sb), ")"));
            }

            if (advancedFilter != null)
            {
                int paramcounter = 0;
                SqlParameter param = null;
                StringBuilder advFilterBuilder = new StringBuilder();
                foreach (XmlNode node in advancedFilter.SelectNodes("AdvancedFilter/FilterPart"))
                {
                    string strName = node.Attributes["FieldName"].Value;
                    string strOperator = node.Attributes["Operator"].Value;
                    string strDBType = node.Attributes["DBType"] == null ? null : node.Attributes["DBType"].Value;
                    string strNextPart = node.Attributes["NextPart"] == null ? null : node.Attributes["NextPart"].Value;
                    string strLookupOperand = node.Attributes["LookupOperand"] == null ? null : node.Attributes["LookupOperand"].Value;
                    string strOperand = node.Attributes["Operand"] == null ? null : node.Attributes["Operand"].Value;
                    string strFieldOperand = node.Attributes["FieldOperand"]==null ? null : node.Attributes["FieldOperand"].Value;

                    if (!string.IsNullOrEmpty(strLookupOperand))
                        strOperand = strLookupOperand;

                    switch (strOperator)
                    {
                        case "Equal":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} = {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} = {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "IsBefore":
                        case "Inferior":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} < {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} < {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "InferiorOrEqual":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} <= {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} <= {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "IsFalse":
                            advFilterBuilder.AppendFormat("{0} = 0", strName);
                            break;
                        case "IsNotNull":
                            advFilterBuilder.AppendFormat("not {0} is null", strName);
                            break;
                        case "IsNull":
                            advFilterBuilder.AppendFormat("{0} is null", strName);
                            break;
                        case "IsEmpty":
                            advFilterBuilder.AppendFormat("{0} = ''", strName);
                            break;
                        case "IsNotEmpty":
                            advFilterBuilder.AppendFormat("({0} is null or {0}<>'')", strName);
                            break;
                        case "IsNullOrEmpty":
                            advFilterBuilder.AppendFormat("({0} is null or {0}='')", strName);
                            break;
                        case "IsNotNullAndNotEmpty":
                            advFilterBuilder.AppendFormat("(not {0} is null and {0}<>'')", strName);
                            break;
                        case "IsTrue":
                            advFilterBuilder.AppendFormat("{0} = 1", strName);
                            break;
                        case "Like":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} LIKE {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} like {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "NotEqual":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} <> {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} <> {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "IsAfter":
                        case "Superior":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} > {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} > {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "SuperiorOrEqual":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("{0} >= {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), strDBType, strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("{0} >= {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "IsInTheFuture":
                            advFilterBuilder.AppendFormat("{0} > getdate()", strName);
                            break;
                        case "IsInThePast":
                            advFilterBuilder.AppendFormat("{0} < getdate()", strName);
                            break;
                        case "IsXMinutesInThePast":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("DATEDIFF(minute, {0}, getdate()) > {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), "Integer", strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("DATEDIFF( minute, {0}, getdate()) > {1} ", strName, param.ParameterName);
                            }
                            break;
                        case "IsXMinutesInTheFuture":
                            if (!string.IsNullOrEmpty(strFieldOperand))
                            {
                                advFilterBuilder.AppendFormat("DATEDIFF(minute, getdate(), {0}) > {1}", strName, strFieldOperand);
                            }
                            else
                            {
                                param = CreateParam(string.Format("@filter{0}", paramcounter++), "Integer", strOperand);
                                cmd.Parameters.Add(param);
                                advFilterBuilder.AppendFormat("DATEDIFF( minute, getdate(), {0}) > {1} ", strName, param.ParameterName);
                            }
                            break;
                    }
                    if (!string.IsNullOrEmpty(strNextPart))
                    {
                        if (strNextPart == "0")
                            advFilterBuilder.Append(" AND ");
                        else
                            advFilterBuilder.Append(" OR ");
                    }
                }

                filters.Add(string.Concat("(",advFilterBuilder.ToString(), ")"));
            }

            if (filters.Count > 0)
                return StringJoin(" AND ", filters);
            else
                return string.Empty;
        }
        private static void GenerateSelectionQuery(SqlCommand cmd, string campaignId, string adminDb, string systemDataTableName, string start, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);

            if (!String.IsNullOrEmpty(wherePart))
                cmd.CommandText = string.Format("select {2} from data_{0}..data d inner join data_{0}..[{4}] s on d.internal__id__ = s.internal__id__ left join {3}..activities a on a.id = s.lasthandleractivity left join {3}..agents ag on ag.id = s.lasthandler left join {3}..agents ag2 on ag2.id = s.targethandler left join {3}..qualifications q on q.id = s.lastqualification  where {1}", campaignId, wherePart, start, adminDb, systemDataTableName);
            else
                cmd.CommandText = string.Format("select {1} from data_{0}..data d inner join data_{0}..[{3}] s on d.internal__id__ = s.internal__id__ left join {2}..activities a on a.id = s.lasthandleractivity left join {2}..agents ag on ag.id = s.lasthandler left join {2}..agents ag2 on ag2.id = s.targethandler left join {2}..qualifications q on q.id = s.lastqualification ", campaignId, start, adminDb, systemDataTableName);

        }

        public static DataSet GetData(string connectionString, string campaignId, string systemDataTableName, bool onlyTop, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    GenerateSelectionQuery(cmd, campaignId,
                        connection.Database, systemDataTableName,
                        onlyTop ?
                        "top 50 d.*, s.CurrentActivity, s.PreviousActivity, s.LastHandlerActivity, s.LastContactId, s.LastActivityChange, s.LastHandler, s.LastHandlingTime, s.LastHandlingDuration, s.TotalHandlingDuration, s.TotalHandlers, s.State, s.PreviousState, s.SortInfo, s.CustomSortInfo, s.Priority, s.DialingModeOverride, s.DialStartDate, s.DialEndDate, s.CreationTime, s.ImportSequence, s.ImportTag, s.ExportSequence, s.RecycleCount, s.LastRecycle, s.ExportTime, s.TargetHandler, s.PreferredAgent, s.TargetDestination, s.TargetMedia, s.DialedCurrentActivity, s.TotalDialed, s.MaxDialAttempts, s.ExpectedProfit, s.LastDialStatus, s.LastDialStatusCount, s.LastDialedDestination, s.LastQualification, s.LastQualificationArgued, s.LastQualificationPositive, s.LastQualificationExportable, s.AreaId, s.AppointmentId, s.Excluded, s.VMFlagged, a.description DescriptionLastHandlerActivity, ag.account + ' ' + ag.FirstName + ' ' + ag.LastName DescriptionLastHandler, ag2.account + ' ' + ag2.FirstName + ' ' + ag2.LastName DescriptionTargetHandler, q.description DescriptionLastQualification"
                        : "d.*, s.CurrentActivity, s.PreviousActivity, s.LastHandlerActivity, s.LastContactId, s.LastActivityChange, s.LastHandler, s.LastHandlingTime, s.LastHandlingDuration, s.TotalHandlingDuration, s.TotalHandlers, s.State, s.PreviousState, s.SortInfo, s.CustomSortInfo, s.Priority, s.DialingModeOverride, s.DialStartDate, s.DialEndDate, s.CreationTime, s.ImportSequence, s.ImportTag, s.ExportSequence, s.RecycleCount, s.LastRecycle, s.ExportTime, s.TargetHandler, s.PreferredAgent, s.TargetDestination, s.TargetMedia, s.DialedCurrentActivity, s.TotalDialed, s.MaxDialAttempts, s.ExpectedProfit, s.LastDialStatus, s.LastDialStatusCount, s.LastDialedDestination, s.LastQualification, s.LastQualificationArgued, s.LastQualificationPositive, s.LastQualificationExportable, s.AreaId, s.AppointmentId, s.Excluded, s.VMFlagged, a.description DescriptionLastHandlerActivity, ag.account + ' ' + ag.FirstName + ' ' + ag.LastName DescriptionLastHandler, ag2.account + ' ' + ag2.FirstName + ' ' + ag2.LastName DescriptionTargetHandler, q.description DescriptionLastQualification"
                        , qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    try
                    {
                        adapter.Fill(ds);
                        return ds;
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }
        public static int GetDataCount(string connectionString, string campaignId, string systemDataTableName, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    GenerateSelectionQuery(cmd, campaignId, connection.Database, systemDataTableName, "count(*)", qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    try
                    {
                        return (int)cmd.ExecuteScalar();
                    }
                    catch
                    {
                    }

                }
            }
            return -1;
        }
        public static int DataManageDelete(string connectionString, string campaignId, string systemDataTableName, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    string wherePart = GenerateWherePart(cmd,campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (string.IsNullOrEmpty(wherePart))
                        cmd.CommandText = string.Format("delete data_{0}..data from data_{0}..data inner join data_{0}..[{1}] s on data_{0}..data.internal__id__ = s.internal__id__", campaignId, systemDataTableName);
                    else
                        cmd.CommandText = string.Format("delete data_{0}..data from data_{0}..data inner join data_{0}..[{2}] s on data_{0}..data.internal__id__ = s.internal__id__ where {1}", campaignId, wherePart, systemDataTableName);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static int DataManageInclude(string connectionString, string campaignId, string systemDataTableName, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (string.IsNullOrEmpty(wherePart))
                        cmd.CommandText = string.Format("update s set excluded = 0 from data_{0}..data d inner join data_{0}..[{1}] s on d.internal__id__ = s.internal__id__", campaignId, systemDataTableName);
                    else
                        cmd.CommandText = string.Format("update s set excluded = 0 from data_{0}..data d inner join data_{0}..[{2}] s on d.internal__id__ = s.internal__id__ where {1}", campaignId, wherePart, systemDataTableName);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static int DataManageExclude(string connectionString, string campaignId, string systemDataTableName, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (string.IsNullOrEmpty(wherePart))
                        cmd.CommandText = string.Format("update s set excluded = 1 from data_{0}..data d inner join data_{0}..[{1}] s on d.internal__id__ = s.internal__id__", campaignId, systemDataTableName);
                    else
                        cmd.CommandText = string.Format("update s set excluded = 1 from data_{0}..data d inner join data_{0}..[{2}] s on d.internal__id__ = s.internal__id__ where {1}", campaignId, wherePart, systemDataTableName);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static int DataManageRemoveTimeConstraints(string connectionString, string campaignId, string systemDataTableName, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (string.IsNullOrEmpty(wherePart))
                        cmd.CommandText = string.Format("update s set dialstartdate = null, dialenddate = null from data_{0}..data d inner join data_{0}..[{1}] s on d.internal__id__ = s.internal__id__ where s.priority >= 0", campaignId, systemDataTableName);
                    else
                        cmd.CommandText = string.Format("update s set dialstartdate = null, dialenddate = null from data_{0}..data d inner join data_{0}..[{2}] s on d.internal__id__ = s.internal__id__ where {1} and s.priority >= 0", campaignId, wherePart, systemDataTableName);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static int DataManageRecycle(string connectionString, string campaignId, string systemDataTableName, string activityid, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (string.IsNullOrEmpty(wherePart))
                        cmd.CommandText = string.Format("UPDATE s set {2}, previousactivity = null, recyclecount = case when recyclecount is null then 1 else recyclecount + 1 end, lastrecycle = getdate(), lastqualificationpositive = null, lastqualificationargued=null, lastqualificationexportable=null,  lasthandleractivity = null, lastcontactid=null, lastactivitychange = null, lasthandler = null, lasthandlingtime = null, totalhandlingduration = 0, totalhandlers = 0, State = 0, PreviousState = 0, SortInfo = 0, DialingModeOverride = null, DialStartDate = null, DialEndDate = null, TargetHandler = null, TargetDestination = null, TargetMedia = null, DialedCurrentActivity = 0, TotalDialed = 0, ExpectedProfit = 1, LastDialStatus = null, LastQualification = null, AppointmentId = null, LastDialStatusCount = 0 , LastDialedDestination = null FROM data_{0}..Data d INNER JOIN data_{0}..[{3}] s on s.internal__id__ = d.internal__id__", campaignId, wherePart, string.IsNullOrEmpty(activityid) ? "CurrentActivity=null" : string.Concat("CurrentActivity='", activityid, "'"), systemDataTableName);
                    else
                        cmd.CommandText = string.Format("UPDATE s set {2}, previousactivity = null, recyclecount = case when recyclecount is null then 1 else recyclecount + 1 end, lastrecycle = getdate(), lastqualificationpositive = null, lastqualificationargued=null, lastqualificationexportable=null,  lasthandleractivity = null, lastcontactid=null, lastactivitychange = null, lasthandler = null, lasthandlingtime = null, totalhandlingduration = 0, totalhandlers = 0, State = 0, PreviousState = 0, SortInfo = 0, DialingModeOverride = null, DialStartDate = null, DialEndDate = null, TargetHandler = null, TargetDestination = null, TargetMedia = null, DialedCurrentActivity = 0, TotalDialed = 0, ExpectedProfit = 1, LastDialStatus = null, LastQualification = null, AppointmentId = null, LastDialStatusCount = 0 , LastDialedDestination = null FROM data_{0}..Data d INNER JOIN data_{0}..[{3}] s on s.internal__id__ = d.internal__id__ WHERE {1}", campaignId, wherePart, string.IsNullOrEmpty(activityid) ? "CurrentActivity=null" : string.Concat("CurrentActivity='", activityid, "'"), systemDataTableName);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static int DataManageReaffectCB(string connectionString, string campaignId, string systemDataTableName, string agentId, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 600;
                    string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (string.IsNullOrEmpty(wherePart))
                        cmd.CommandText = string.Format("update s set targethandler = {1} from data_{0}..data d inner join data_{0}..[{2}] s on d.internal__id__ = s.internal__id__ where s.State = 15", campaignId, string.IsNullOrEmpty(agentId) ? "null" : string.Concat("'", agentId, "'"), systemDataTableName);
                    else
                        cmd.CommandText = string.Format("update s set targethandler = {2} from data_{0}..data d inner join data_{0}..[{3}] s on d.internal__id__ = s.internal__id__ where {1} and s.state=15", campaignId, wherePart, string.IsNullOrEmpty(agentId) ? "null" : string.Concat("'", agentId, "'"), systemDataTableName);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public delegate void progressReportDelegate(int percent, string message, string progressMessage);
        public static int[] ImportData(string connectionString, string systemDataTableName, string file, char separator, char stringDelimiter, bool firstRowContainsHeader, string campaignId, DataTable table, List<string> fields, string importTag, string activityid, int maxdialAttemps, bool removeNonNumeric, string prefixToRemove, string prefixToAdd, IEnumerable<string> preferredAgents, progressReportDelegate progress, string culture, string srcNumberFormat, string dstNumberFormat, HandleNumberFormatDelegate handleNumberFormat, string decimalSeparator)
        {
            int result = 0;
            int maxSequence = -1;
            if (!connectionString.Contains("Asynchronous Processing="))
                connectionString = connectionString + ";Asynchronous Processing=true";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand executing = null;
                SqlCommand preparing = null;
                IAsyncResult iasyncres = null;

                connection.Open();

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    using (SqlCommand cmd2 = connection.CreateCommand())
                    {
                        cmd.CommandTimeout = 600;
                        cmd2.CommandTimeout = 600;

                        List<string> prms = new List<string>();
                        foreach (DataColumn col in table.Columns)
                        {
                            if (!col.ReadOnly)
                            {
                                if (col.DataType == typeof(string))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd.Parameters.Add(string.Format("@P{0}", cmd.Parameters.Count), SqlDbType.NVarChar, col.MaxLength)).Direction = ParameterDirection.Input;
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.NVarChar, col.MaxLength)).Direction = ParameterDirection.Input;
                                }
                                else if (col.DataType == typeof(int))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd.Parameters.Add(string.Format("@P{0}", cmd.Parameters.Count), SqlDbType.Int)).Direction = ParameterDirection.Input;
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.Int)).Direction = ParameterDirection.Input;
                                }
                                else if (col.DataType == typeof(bool))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd.Parameters.Add(string.Format("@P{0}", cmd.Parameters.Count), SqlDbType.Bit)).Direction = ParameterDirection.Input;
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.Bit)).Direction = ParameterDirection.Input;
                                }
                                else if (col.DataType == typeof(DateTime))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd.Parameters.Add(string.Format("@P{0}", cmd.Parameters.Count), SqlDbType.DateTime)).Direction = ParameterDirection.Input;
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.DateTime)).Direction = ParameterDirection.Input;
                                }
                                else if (col.DataType == typeof(char))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd.Parameters.Add(string.Format("@P{0}", cmd.Parameters.Count), SqlDbType.Char)).Direction = ParameterDirection.Input;
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.Char)).Direction = ParameterDirection.Input;
                                }
                                else if (col.DataType == typeof(double))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd.Parameters.Add(string.Format("@P{0}", cmd.Parameters.Count), SqlDbType.Float)).Direction = ParameterDirection.Input;
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.Float)).Direction = ParameterDirection.Input;
                                }
                                else if (col.DataType == typeof(float))
                                {
                                    prms.Add(string.Format("@P{0}", cmd.Parameters.Count));
                                    (cmd2.Parameters.Add(string.Format("@P{0}", cmd2.Parameters.Count), SqlDbType.Float)).Direction = ParameterDirection.Input;
                                }
                            }
                        }

                        cmd.CommandText = string.Format("insert into data_{0}..data (Internal__id__, [{1}]) values (@InternalId, {2})", campaignId, StringJoin("],[", fields), StringJoin(",", prms));
                        cmd2.CommandText = cmd.CommandText;

                        (cmd.Parameters.Add("@InternalId", SqlDbType.Char, 32)).Direction = ParameterDirection.Input;
                        (cmd2.Parameters.Add("@InternalId", SqlDbType.Char, 32)).Direction = ParameterDirection.Input;

                        cmd.Prepare();
                        cmd2.Prepare();

                        preparing = cmd;
                        executing = cmd2;

                        using (CsvReader R = new CsvReader(File.OpenRead(file), Encoding.Default, separator, stringDelimiter, decimalSeparator))
                        {

                            long totalLines = 0;
                            int lastProgress = 0;
                            DateTime lastProgressTime = DateTime.Now;
                            long linecounter = 0;
                            if (progress != null)
                            {
                                progress(-1, Translate("Initializing...", culture, connectionString), string.Empty);
                                totalLines = R.GetLineCount();
                                progress(lastProgress, Translate("Processing lines...", culture, connectionString), string.Format(Translate("{0}/{1} lines", culture, connectionString), linecounter, totalLines));
                            }

                            string[] Line;
                            DataRow DR;

                            if (firstRowContainsHeader)
                            {
                                if ((Line = R.ReadLine()) != null)
                                {

                                }
                            }
                            else
                            {
                                if ((Line = R.ReadLine()) != null)
                                {
                                    R.Reset();
                                }
                            }

                            DateTime now = DateTime.Now;

                            while ((DR = R.ReadDataRow(table)) != null)
                            {
                                linecounter++;
                                Object[] objs = DR.ItemArray;

                                int counter = 0;
                                for (int i = 0; i < objs.Length; i++)
                                {
                                    if (!table.Columns[i].ReadOnly)
                                    {

                                        if (objs[i] != null)
                                        {
                                            if (table.Columns[i].ExtendedProperties.ContainsKey("phonenum") && table.Columns[i].DataType == typeof(string))
                                            {
                                                if (handleNumberFormat != null)
                                                {
                                                    preparing.Parameters[counter].Value = handleNumberFormat(ProcessPhoneNum(objs[i] as string, removeNonNumeric, prefixToRemove, prefixToAdd), srcNumberFormat, dstNumberFormat);
                                                    if (preparing.Parameters[counter].Value == null)
                                                        preparing.Parameters[counter].Value = DBNull.Value;
                                                }
                                            }
                                            else
                                            {
                                                preparing.Parameters[counter].Value = objs[i];
                                            }



                                            //if (removeNonNumeric || !string.IsNullOrEmpty(prefixToAdd) || !string.IsNullOrEmpty(prefixToRemove))
                                            //{
                                            //    if (table.Columns[i].ExtendedProperties.ContainsKey("phonenum") && table.Columns[i].DataType == typeof(string))
                                            //    {
                                            //        preparing.Parameters[counter].Value = ProcessPhoneNum(objs[i] as string, removeNonNumeric, prefixToRemove, prefixToAdd);
                                            //    }
                                            //    else
                                            //    {
                                            //        preparing.Parameters[counter].Value = objs[i];
                                            //    }
                                            //}
                                            //else
                                            //    preparing.Parameters[counter].Value = objs[i];
                                        }
                                        else
                                            preparing.Parameters[counter].Value = DBNull.Value;
                                        counter++;
                                    }
                                }
                                preparing.Parameters[cmd.Parameters.Count - 1].Value = System.Guid.NewGuid().ToString("N");

                                try
                                {
                                    // wait for "executing" to complete
                                    if (iasyncres != null)
                                    {
                                        result += executing.EndExecuteNonQuery(iasyncres);
                                    }

                                }
                                catch
                                {
                                }

                                SqlCommand backup = executing;
                                executing = preparing;
                                preparing = backup;

                                iasyncres = executing.BeginExecuteNonQuery();


                                if (progress != null)
                                {
                                    long temp = linecounter * 100 / totalLines;
                                    if (temp > lastProgress || DateTime.Now.Subtract(lastProgressTime).TotalMilliseconds > 2000)
                                    {
                                        lastProgress = (int)temp;
                                        lastProgressTime = DateTime.Now;
                                        progress(lastProgress, Translate("Processing lines...", culture, connectionString), string.Format(Translate("{0}/{1} lines", culture, connectionString), linecounter, totalLines));
                                    }
                                }
                            }
                            try
                            {
                                if (iasyncres != null)
                                {
                                    result += executing.EndExecuteNonQuery(iasyncres);
                                }
                            }
                            catch
                            {
                            }
                        }
                        if (progress != null)
                        {
                            progress(-1, Translate("Generating system data...", culture, connectionString), string.Empty);
                            // this is on purpose, else there is no refresh of the GUI...
                            // TODO: find better approach
                            progress(-1, Translate("Generating system data...", culture, connectionString), string.Empty);
                        }


                        cmd.Parameters.Clear();


                        cmd.Parameters.AddWithValue("@MaxDialAttemps", maxdialAttemps);
                        if (importTag != null)
                            cmd.Parameters.AddWithValue("@ImportTag", importTag);
                        else
                            cmd.Parameters.AddWithValue("@ImportTag", DBNull.Value);

                        string preferredAgentPart = null;
                        int preferredCount = 0;
                        if ((preferredCount = GetEnumerableCount(preferredAgents)) > 0)
                        {
                            StringBuilder builder = new StringBuilder(1024);
                            builder.Append("case row_Number() over( order by internal__id__) %");
                            builder.Append(preferredCount.ToString());
                            foreach (string agent in preferredAgents)
                            {
                                preferredCount--;
                                builder.AppendFormat(" when {0} then '{1}'", preferredCount, agent);

                            }
                            builder.Append(" end ");
                            preferredAgentPart = builder.ToString();
                        }

                        if (string.IsNullOrEmpty(activityid))
                        {
                            cmd.CommandText = string.Format("insert into data_{0}..[{3}] (Internal__Id__, ImportSequence, ImportTag, CreationTime, MaxDialAttempts {1}) (select Internal__Id__, case when (select max(ImportSequence) from data_{0}..systemdata) is null then 1 else (select max(ImportSequence) from data_{0}..systemdata) + 1 end ,@ImportTag, getdate(), @MaxDialAttemps {2} from data_{0}..Data where Internal__Id__ not in (select Internal__Id__ from data_{0}..SystemData));select max(ImportSequence) from data_{0}..systemdata", campaignId,
                                preferredAgentPart == null ? string.Empty : ", PreferredAgent",
                                preferredAgentPart == null ? string.Empty : "," + preferredAgentPart, systemDataTableName);
                        }
                        else
                        {
                            cmd.CommandText = string.Format("insert into data_{0}..[{4}] (Internal__Id__, ImportSequence, ImportTag, CurrentActivity, CreationTime, MaxDialAttempts {2}) (select Internal__Id__, case when (select max(ImportSequence) from data_{0}..systemdata) is null then 1 else (select max(ImportSequence) from data_{0}..systemdata) + 1 end, @ImportTag,'{1}', getdate(),@MaxDialAttemps {3} from data_{0}..Data where Internal__Id__ not in (select Internal__Id__ from data_{0}..SystemData));select max(ImportSequence) from data_{0}..systemdata", campaignId, activityid,
                                preferredAgentPart == null ? string.Empty : ", PreferredAgent",
                                preferredAgentPart == null ? string.Empty : "," + preferredAgentPart, systemDataTableName);
                        }

                        object obj = cmd.ExecuteScalar();
                        if (obj != null && System.DBNull.Value != obj)
                            maxSequence = (int)obj;
                    }
                }
            }
            return new int[]{ result, maxSequence};

        }
        public delegate string HandleNumberFormatDelegate(string phone, string srcNumberFormat, string dstNumberFormat);
        public static string PreviewPhoneNumbers(string file, char separator, char stringDelimiter, bool firstRowContainsHeader, string dstNumberFormat, DataTable table, bool removeNonNumeric, string prefixToRemove, string prefixToAdd, string srcNumberFormat, string culture, HandleNumberFormatDelegate handleNumberFormat, string decimalSeparator)
        {
            StringBuilder sb = new StringBuilder();

            using (CsvReader R = new CsvReader(File.OpenRead(file), Encoding.Default, separator, stringDelimiter, decimalSeparator))
            {

                DateTime lastProgressTime = DateTime.Now;
                long linecounter = 0;

                string[] Line;
                DataRow DR;

                if (firstRowContainsHeader)
                {
                    if ((Line = R.ReadLine()) != null)
                    {

                    }
                }
                else
                {
                    if ((Line = R.ReadLine()) != null)
                    {
                        R.Reset();
                    }
                }

                DateTime now = DateTime.Now;

                while ((DR = R.ReadDataRow(table)) != null)
                {
                    linecounter++;
                    Object[] objs = DR.ItemArray;

                    int counter = 0;
                    for (int i = 0; i < objs.Length; i++)
                    {
                        if (!table.Columns[i].ReadOnly)
                        {

                            if (objs[i] != null)
                            {
                                if (table.Columns[i].ExtendedProperties.ContainsKey("phonenum") && table.Columns[i].DataType == typeof(string))
                                {
                                    if (handleNumberFormat != null)
                                    {
                                        sb.AppendFormat("{0}\t->\t{1}\n", objs[i] as string, handleNumberFormat(ProcessPhoneNum(objs[i] as string, removeNonNumeric, prefixToRemove, prefixToAdd), srcNumberFormat, dstNumberFormat));
                                    }
                                }

                            }
                            counter++;
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private static string ProcessPhoneNum(string phone, bool removeNonNumeric, string prefixToRemove, string prefixToAdd)
        {
            if (removeNonNumeric)
            {
                string strNewPhone = string.Empty;

                if (
                    phone.Length > 0 &&
                    (
                    phone[0] == '0' ||
                    phone[0] == '1' ||
                    phone[0] == '2' ||
                    phone[0] == '3' ||
                    phone[0] == '4' ||
                    phone[0] == '5' ||
                    phone[0] == '6' ||
                    phone[0] == '7' ||
                    phone[0] == '8' ||
                    phone[0] == '9' ||
                    phone[0] == '+'
                    )
                    )
                    strNewPhone = string.Concat(strNewPhone, phone[0]);

                for (int i = 1; i < phone.Length; i++)
                {
                    if (
                        phone[i] == '0' ||
                        phone[i] == '1' ||
                        phone[i] == '2' ||
                        phone[i] == '3' ||
                        phone[i] == '4' ||
                        phone[i] == '5' ||
                        phone[i] == '6' ||
                        phone[i] == '7' ||
                        phone[i] == '8' ||
                        phone[i] == '9'
                        )
                        strNewPhone = string.Concat(strNewPhone, phone[i]);
                }
                phone = strNewPhone;
            }
            if (!string.IsNullOrEmpty(prefixToRemove))
            {
                if (phone.StartsWith(prefixToRemove))
                    phone.Remove(0, prefixToRemove.Length);
            }
            if (!string.IsNullOrEmpty(prefixToAdd))
            {
                phone = string.Concat(prefixToAdd, phone);
            }
            return phone;
        }


        public static int ExportData(string connectionString, string systemDataTableName, string file, char separator, char stringDelimiter, bool firstRowContainsHeader, List<int> exportablefields, string campaignId, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter, progressReportDelegate progress, string culture, string decimalSeparator)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    long totalLines = 0;
                    if (progress != null)
                    {
                        progress(-1, Translate("Initializing...", culture, connectionString), string.Empty);
                        GenerateSelectionQuery(cmd, campaignId,
                        connection.Database, systemDataTableName,
                        "count(*)"
                        , qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);

                        object obj = cmd.ExecuteScalar();
                        if (obj != null && obj != System.DBNull.Value)
                        {
                            if (obj is int)
                                totalLines = (int)obj;
                            else if (obj is long)
                                totalLines = (long)obj;
                        }

                    }

                    GenerateSelectionQuery(cmd, campaignId,
                        connection.Database, systemDataTableName,
                        "d.*, s.CurrentActivity, s.PreviousActivity, s.LastHandlerActivity, s.LastContactId, s.LastActivityChange, s.LastHandler, s.LastHandlingTime, s.LastHandlingDuration, s.TotalHandlingDuration, s.TotalHandlers, s.State, s.PreviousState, s.SortInfo, s.CustomSortInfo, s.Priority, s.DialingModeOverride, s.DialStartDate, s.DialEndDate, s.CreationTime, s.ImportSequence, s.ImportTag, s.ExportSequence, s.RecycleCount, s.LastRecycle, s.ExportTime, s.TargetHandler, s.PreferredAgent, s.TargetDestination, s.TargetMedia, s.DialedCurrentActivity, s.TotalDialed, s.MaxDialAttempts, s.ExpectedProfit, s.LastDialStatus, s.LastDialStatusCount, s.LastDialedDestination, s.LastQualification, s.LastQualificationArgued, s.LastQualificationPositive, s.LastQualificationExportable, s.AreaId, s.AppointmentId, s.Excluded, s.VMFlagged, a.description DescriptionLastHandlerActivity, ag.account + ' ' + ag.FirstName + ' ' + ag.LastName DescriptionLastHandler, ag2.account + ' ' + ag2.FirstName + ' ' + ag2.LastName DescriptionTargetHandler, q.description DescriptionLastQualification"
                        , qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);

                    string wherePart = GenerateWherePart(cmd, campaignId, qualifications, negativeonly, excludedonly, exportableonly, batchnumber, tag, exportnumber, notexportedyet, disconnectReasons, dialStates, agentids, activities, From, To, advancedFilter);
                    if (exportnumber > int.MinValue)
                    {
                        cmd.CommandText = string.Concat("declare @MaxSequence int; declare @now datetime; select @now = getdate();",
                            "UPDATE s SET ExportTime = @now FROM data_",
                            campaignId, "..data d inner join data_", campaignId, "..[",systemDataTableName,"] s on d.internal__id__ = s.internal__id__",
                            string.IsNullOrEmpty(wherePart) ? string.Empty : " WHERE ",
                            wherePart,
                            ";",
                            cmd.CommandText);

                    }
                    else
                        cmd.CommandText = string.Concat("declare @MaxSequence int; declare @now datetime; select @now = getdate();",
                            "SELECT @MaxSequence=max(case when exportSequence is null then 0 else exportSequence end)+1 FROM data_", campaignId, "..[",systemDataTableName,"];",
                            "UPDATE s SET ExportTime = @now, ExportSequence = @MaxSequence FROM data_",
                            campaignId, "..data d inner join data_", campaignId, "..[",systemDataTableName,"] s on d.internal__id__ = s.internal__id__",
                            string.IsNullOrEmpty(wherePart) ? string.Empty : " WHERE ",
                            wherePart,
                            ";",
                            cmd.CommandText);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    try
                    {
                        adapter.FillSchema(ds, SchemaType.Mapped);
                    }
                    catch
                    {
                    }


                    DataView dv = ds.Tables[0].DefaultView;
                    dv.Table.PrimaryKey = null;

                    for (int i = dv.Table.Columns.Count - 1; i >= 0; i--)
                    {
                        if (!exportablefields.Contains(i))
                            dv.Table.Columns.RemoveAt(i);
                    }


                    try
                    {
                        int lastProgress = 0;
                        DateTime lastProgressTime = DateTime.Now;
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (!Directory.Exists(Path.GetDirectoryName(file)))
                            Directory.CreateDirectory(Path.GetDirectoryName(file));

                        using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (CsvWriter R = new CsvWriter(fs, Encoding.Default, separator, stringDelimiter, decimalSeparator))
                            {
                                if (firstRowContainsHeader)
                                {
                                    string[] strs = new string[dv.Table.Columns.Count];
                                    int i = 0;
                                    foreach (DataColumn col in dv.Table.Columns)
                                    {
                                        strs[i] = col.ColumnName;
                                        i++;
                                    }
                                    R.WriteLine(strs);
                                }


                                while (reader.Read())
                                {
                                    object[] values = new object[exportablefields.Count];
                                    for (int i = 0; i < exportablefields.Count; i++)
                                        values[i] = reader.GetValue(exportablefields[i]);
                                    R.WriteLine(values);
                                    result++;
                                    if (progress != null)
                                    {
                                        long temp = result * 100 / totalLines;
                                        if (temp > lastProgress || DateTime.Now.Subtract(lastProgressTime).TotalMilliseconds > 2000)
                                        {
                                            lastProgress = (int)temp;
                                            lastProgressTime = DateTime.Now;
                                            progress(lastProgress, Translate("Writing lines...", culture, connectionString), string.Format(Translate("{0}/{1} lines", culture, connectionString), result, totalLines));
                                        }
                                    }
                                }

                            }
                        }
                    }
                    catch
                    {
                        result = -1;
                    }
                }
            }
            return result;
        }
        public static void MaintainDatabase(string connectionString, string campaignId, bool reorganizeIndexes, bool updateStatistics, progressReportDelegate progress, string culture)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                bool backupFireInfoMessageEventOnUserErrors = connection.FireInfoMessageEventOnUserErrors;
                connection.FireInfoMessageEventOnUserErrors = true;

                SqlInfoMessageEventHandler evtHandler = (s, e) => progress(-1, e.Message, string.Empty);

                connection.InfoMessage += evtHandler;
                try
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandTimeout = 600;

                        StringBuilder builder = new StringBuilder(50000);

                        builder.AppendLine("declare @name sysname");
                        builder.AppendLine("declare @count int");
                        builder.AppendLine("declare @cmd varchar(max)");
                        builder.AppendLine("declare @fragments float");
                        builder.AppendLine("declare @fileName sysname");
                        builder.AppendLine("declare @tableName sysname");
                        builder.AppendLine("declare @indexName sysname");
                        builder.AppendLine("set @name = 'data_" + campaignId + "'");

                        if (reorganizeIndexes)
                        {
                            builder.AppendLine("print 'Retrieving index fragmentation';");
                            builder.AppendLine("create table #tmpIdx (databasename sysname, tableName sysname, indexName sysname, fragments float)");
                            builder.AppendLine("set @cmd = 'use [' +@name + ']; insert into #tmpIdx select ''' + @name + ''', so.name, si.name, ps.avg_fragmentation_in_percent from sys.dm_db_index_physical_stats (DB_ID(), null, null, NULL, NULL) ps , sys.sysindexes si, sys.sysobjects so where so.type=''U'' and so.id=si.id and si.indid>0 and si.id = ps.object_id and si.indid = ps.index_id and si.root is not null and ps.avg_fragmentation_in_percent > 5 and ps.page_count > 10'");
                            builder.AppendLine("set @count=@count+1");
                            builder.AppendLine("exec (@cmd)");
                            builder.AppendLine(string.Format("print '{0}';", Translate("Reorganizing indexes", culture, connectionString)));
                            builder.AppendLine("declare dblist cursor for select * from #tmpIdx");
                            builder.AppendLine("open dblist");
                            builder.AppendLine("fetch next from dblist into @name, @tableName, @indexName, @fragments");
                            builder.AppendLine("while @@FETCH_STATUS=0");
                            builder.AppendLine("begin");
                            builder.AppendLine("if @fragments > 30");
                            builder.AppendLine("begin");
                            builder.AppendLine("begin try");
                            builder.AppendLine("set @cmd = 'ALTER INDEX [' + @indexName + '] ON [' + @name + ']..[' + @tableName + '] REBUILD WITH (ONLINE = ON)'");
                            builder.AppendLine("exec (@cmd)");
                            builder.AppendLine("end try");
                            builder.AppendLine("begin catch");
                            builder.AppendLine("print ERROR_MESSAGE()");
                            builder.AppendLine("begin try");
                            builder.AppendLine("set @cmd = 'ALTER INDEX [' + @indexName + '] ON [' + @name + ']..[' + @tableName + '] REBUILD WITH (ONLINE = OFF)'");
                            builder.AppendLine("exec (@cmd)");
                            builder.AppendLine("end try");
                            builder.AppendLine("begin catch");
                            builder.AppendLine("print ERROR_MESSAGE()");
                            builder.AppendLine("end catch");
                            builder.AppendLine("end catch");
                            builder.AppendLine("end");
                            builder.AppendLine("else");
                            builder.AppendLine("begin");
                            builder.AppendLine("begin try");
                            builder.AppendLine("set @cmd = 'ALTER INDEX [' + @indexName + '] ON [' + @name + ']..[' + @tableName + '] REORGANIZE'");
                            builder.AppendLine("exec (@cmd)");
                            builder.AppendLine("end try");
                            builder.AppendLine("begin catch");
                            builder.AppendLine("print ERROR_MESSAGE() ");
                            builder.AppendLine("end catch");
                            builder.AppendLine("end");
                            builder.AppendLine("fetch next from dblist into @name, @tableName, @indexName, @fragments");
                            builder.AppendLine("end");
                            builder.AppendLine("close dblist");
                            builder.AppendLine("deallocate dblist");
                            builder.AppendLine("drop table #tmpIdx");

                        }
                        if (updateStatistics)
                        {
                            builder.AppendLine(string.Format("print '{0}';", Translate("Updating statistics", culture, connectionString)));
                            builder.AppendLine("set @name = 'data_" + campaignId + "'");
                            builder.AppendLine("set @cmd = 'USE ' + @name + ';exec sp_updatestats'");
                            builder.AppendLine("set @count=@count+1");
                            builder.AppendLine("exec (@cmd)");
                        }

                        cmd.CommandText = builder.ToString();
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.FireInfoMessageEventOnUserErrors = backupFireInfoMessageEventOnUserErrors;
                    connection.InfoMessage -= evtHandler;
                }
            }
        }

        public static string LoadTranslationTable(string connectionString, string language)
        {
            try
            {
                string HeadLanguage = null;

                if (language.Length > 2)
                    HeadLanguage = language.Substring(2);

                using (SqlConnection C = new SqlConnection(connectionString))
                {

                    C.Open();

                    using (SqlCommand Cmd = C.CreateCommand())
                    {
                        Cmd.CommandText = "SELECT * FROM AgentTranslations";
                        Cmd.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader R = Cmd.ExecuteReader())
                        {
                            StringBuilder SB = new StringBuilder(50000);
                            int OrdContext = -1;
                            int OrdKey = -1;
                            int OrdHeadLanguage = -1;
                            int OrdLanguage = -1;
                            int OrdEN = -1;

                            OrdContext = R.GetOrdinal("EntryContext");
                            OrdKey = R.GetOrdinal("EntryKey");
                            OrdEN = R.GetOrdinal("EN");

                            try { OrdHeadLanguage = R.GetOrdinal(HeadLanguage); }
                            catch { }
                            try { OrdLanguage = R.GetOrdinal(language); }
                            catch { }

                            while (R.Read())
                            {
                                string Value = (OrdLanguage >= 0 && R[OrdLanguage] != DBNull.Value) ? R.GetString(OrdLanguage) : ((OrdHeadLanguage >= 0 && R[OrdHeadLanguage] != DBNull.Value) ? R.GetString(OrdHeadLanguage) : ((R[OrdEN] != DBNull.Value) ? R.GetString(OrdEN) : null));

                                if (Value != null)
                                {
                                    SB.Append(Microsoft.JScript.GlobalObject.escape(R.GetString(OrdContext))).Append(',');
                                    SB.Append(Microsoft.JScript.GlobalObject.escape(R.GetString(OrdKey))).Append(',');
                                    SB.AppendLine(Microsoft.JScript.GlobalObject.escape(Value));
                                }
                            }

                            return SB.ToString();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Trace(Ex.ToString(), "ServerLink");
            }

            return null;
        }

    }

}
