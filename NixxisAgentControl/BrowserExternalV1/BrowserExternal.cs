using System;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Xml;

namespace Nixxis.Client.Agent
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class BrowserExternal
    {


        private HttpLink m_ClientLink;
        private ContactInfo m_CInfo;
        private XmlDocument m_ContextData;

        public string ContactId { get { return m_CInfo.Id; } }

        public string Activity { get { return m_CInfo.Activity; } }
        public string UserName { get { return m_CInfo.AgentInfo.Account; } }
        public string UserAccount { get { return m_CInfo.AgentInfo.Name; } }
        public string To { get { return m_CInfo.To; } }
        public string From { get { return m_CInfo.From; } }
        public string Context { get { return m_CInfo.Context; } }
        public string Media { get { return m_CInfo.MediaCode.ToString(); } }
        public string AgentDescription { get { return m_CInfo.AgentInfo.Description; } }
        public string UUI { get { return m_CInfo.UUI; } }
        public string Language { get { return m_CInfo.Language; } }
        public string Queue { get { return m_CInfo.Queue; } }

        public BrowserExternal(HttpLink link, ContactInfo contact)
        {
            m_ClientLink = link;
            m_CInfo = contact;
        }

        public void SetWarningHandler(object handler)
        {
            dynamic callbackfunc = handler;

            HttpLinkServerEventDelegate hlsed = null;
            // UnauthorizedAccessException occur when script has been reloaded for example (so the delegate reference JS function not loaded anymore) 
            hlsed = ((eventargs) => { try { callbackfunc(eventargs.Parameters); } catch (UnauthorizedAccessException) { m_ClientLink.AgentWarning -= hlsed; } catch { };});

            m_ClientLink.AgentWarning += hlsed;
        }

        public void SetOOBInfoHandler(object handler)
        {
            dynamic callbackfunc = handler;

            HttpLinkServerEventDelegate hlsed = null;
            // UnauthorizedAccessException occur when script has been reloaded for example (so the delegate reference JS function not loaded anymore)
            hlsed = ((eventargs) => { try { callbackfunc(eventargs.Parameters); } catch (UnauthorizedAccessException) { m_ClientLink.OOBInfo -= hlsed; } catch { };});

            m_ClientLink.OOBInfo += hlsed;

        }

        public void SetPauseForcedHandler(object handler)
        {
            dynamic callbackfunc = handler;

            HttpLinkServerEventDelegate hlsed = null;
            // UnauthorizedAccessException occur when script has been reloaded for example (so the delegate reference JS function not loaded anymore)
            hlsed = ((eventargs) => { try { callbackfunc(eventargs.Parameters); } catch (UnauthorizedAccessException) { m_ClientLink.PauseForced -= hlsed; } catch { };});

            m_ClientLink.PauseForced += hlsed;

        }

        public void SetContactStateChangedHandler(object handler)
        {
            dynamic callbackfunc = handler;

            HttpLinkContactEventDelegate hlced = null;
            // UnauthorizedAccessException occur when script has been reloaded for example (so the delegate reference JS function not loaded anymore)
            hlced = ((eventargs) => { try { callbackfunc(eventargs.Contact.State); } catch (UnauthorizedAccessException) { m_ClientLink.ContactStateChanged -= hlced; } catch { };});

            m_ClientLink.ContactStateChanged += hlced;
        }


        public void voiceforward(string Destination, string ActiveContactId)
        {
            m_ClientLink.Commands.VoiceForward.Execute(Destination, ActiveContactId);
        }

        public void terminateContact()
        {
            try
            {
                if (m_CInfo.State == 'D' || m_CInfo.State == 'P') 
                    m_CInfo.Close(false, TerminateBehavior.None);
            }
            catch { }
        }

        public void voicenewcall(string destination, [DefaultParameterValue(null)] string contactId, [DefaultParameterValue(null)] string activity, [DefaultParameterValue(null)] string customerId, [DefaultParameterValue(null)] string contactListId)
        {
            m_ClientLink.Commands.VoiceNewCall.Execute(destination, contactId, string.Format("AcId={0}", activity), string.Format("CuId={0}", customerId), string.Format("LiId={0}", contactListId));
        }

        public void voiceconference()
        {
            m_ClientLink.Commands.VoiceConference.Execute();
        }

        public void externalspy([DefaultParameterValue(null)]string destination, [DefaultParameterValue(null)] string options)
        {
            m_ClientLink.Commands.ExternalSpy.Execute(destination, options);
        }

        public void voicehold()
        {
            m_CInfo.Hold();
        }

        public void voiceretrieve()
        {
            m_CInfo.Retrieve();
        }
        
        public void voicehangup()
        {
            m_CInfo.Hangup();
        }

        public void redial(string destination, string contactListId, string activityId)
        {
            if(!string.IsNullOrEmpty(contactListId) && !string.IsNullOrEmpty(activityId))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Redial to {0} for record {1} on activity {2}", destination, contactListId, activityId), "Scripting");

                m_CInfo.NewCall(destination, ContactMedia.Voice, null, activityId, null, contactListId, null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Redial to {0} without settings", destination), "Scripting");

                m_CInfo.Redial(destination);
            }
        }
        
        public string ExecuteCommand(string CommandName, [DefaultParameterValue(null)] string param1, [DefaultParameterValue(null)] string param2, [DefaultParameterValue(null)] string param3, [DefaultParameterValue(null)] string param4, [DefaultParameterValue(null)] string param5, [DefaultParameterValue(null)] string param6, [DefaultParameterValue(null)] string param7, [DefaultParameterValue(null)] string param8, [DefaultParameterValue(null)] string param9, [DefaultParameterValue(null)] string param10)
        {
            List<string> tempList = new List<string>();

            try
            {
                if (param1 != null)
                    tempList.Add(param1);

                if (param2 != null)
                    tempList.Add(param2);
                if (param3 != null)
                    tempList.Add(param3);
                if (param4 != null)
                    tempList.Add(param4);
                if (param5 != null)
                    tempList.Add(param5);
                if (param6 != null)
                    tempList.Add(param6);
                if (param7 != null)
                    tempList.Add(param7);
                if (param8 != null)
                    tempList.Add(param8);
                if (param9 != null)
                    tempList.Add(param9);
                if (param10 != null)
                    tempList.Add(param10);

                return m_ClientLink.ExecuteCommand(CommandName, tempList.ToArray());
            }
            catch { return string.Empty; }
        }
        
        public void voicetransfer()
        {
            m_CInfo.Transfer();
        }
        
        public void terminateContactAndGoReady()
        {
            m_CInfo.Close(false, TerminateBehavior.ForceReady);
        }
        
        public void setBrowserLocking(bool locked)
        {
        }

        public bool GetContextData(int historyLength)
        {
            try
            {
                m_ContextData = m_CInfo.GetContextData(historyLength);
            }
            catch
            {
                m_ContextData = null;
            }

            return (m_ContextData != null);
        }

        public bool SaveContextData()
        {
            if (m_ContextData != null)
            {
                XmlNode UpdateRoot = m_ContextData.SelectSingleNode("/contextdata/updatedata");

                if (UpdateRoot != null)
                {
                    XmlDocument NewData = m_CInfo.UpdateContextData(string.Concat("<contextdata><campaigndata>", UpdateRoot.InnerXml, "</campaigndata></contextdata>"));

                    if (NewData == null)
                        return false;

                    m_ContextData = NewData;
                }
            }

            return true;
        }

        public string GetFieldType(string name)
        {
            if (m_ContextData == null)
            {
                if (!GetContextData(-1))
                    return "";
            }

            if (name.StartsWith("DATE", StringComparison.OrdinalIgnoreCase))
                return "DATE";

            if (name.StartsWith("INT", StringComparison.OrdinalIgnoreCase))
                return "INT";

            return "TEXT";
        }

        public string GetFieldValue(string name)
        {
            if (m_ContextData == null)
            {
                if (!GetContextData(-1))
                    return "";
            }

            XmlNode FieldNode = m_ContextData.SelectSingleNode(string.Concat("/contextdata/updatedata/", name));

            if (FieldNode == null)
                FieldNode = m_ContextData.SelectSingleNode(string.Concat("/contextdata/campaigndata/", name));

            if (FieldNode == null)
                return "";

            return FieldNode.InnerText;
        }

        public void SetFieldValue(string name, string value)
        {
            if (m_ContextData != null)
            {
                string OriginalValue = null;

                XmlNode UpdateRoot = m_ContextData.SelectSingleNode("/contextdata/updatedata");
                XmlNode FieldNode = m_ContextData.SelectSingleNode(string.Concat("/contextdata/campaigndata/", name));

                if (FieldNode != null)
                    OriginalValue = FieldNode.InnerText;

                if (string.Equals(value, OriginalValue))
                {
                    if (UpdateRoot != null)
                    {
                        XmlNode UpdateField = UpdateRoot.SelectSingleNode(name);

                        if (UpdateField != null)
                            UpdateField.ParentNode.RemoveChild(UpdateField);
                    }
                }
                else
                {
                    if (UpdateRoot == null)
                    {
                        UpdateRoot = m_ContextData.CreateElement("updatedata");
                        m_ContextData.DocumentElement.AppendChild(UpdateRoot);
                    }

                    XmlNode UpdateField = UpdateRoot.SelectSingleNode(name);

                    if (UpdateField == null)
                    {
                        UpdateField = m_ContextData.CreateElement(name);
                        UpdateRoot.AppendChild(UpdateField);
                    }

                    UpdateField.InnerText = value;
                }
            }
        }

        private void RecurseQualifications(QualificationInfo item, System.Text.StringBuilder sb, bool includePositive, bool includeNegative, bool includeNeutral, bool includeArgued, bool includeNotArgued)
        {
            if (item.ListQualification.Count == 0)
            {
                if ((includePositive && item.Positive > 0) || (includeNegative && item.Positive < 0) || (includeNeutral && item.Positive == 0))
                {
                    if ((includeArgued && item.Argued) || (includeNotArgued && !item.Argued))
                    {
                        sb.Append(item.Id).Append(';').Append(Microsoft.JScript.GlobalObject.escape(item.Description)).Append(';').Append(item.Action).Append(';').Append(item.Argued).Append(';').Append(item.Positive).Append(';').Append(item.PositiveUpdatable ? "1" : "0").AppendLine();
                    }
                }
            }
            else
            {
                foreach (QualificationInfo Child in item.ListQualification)
                {
                    RecurseQualifications(Child, sb, includePositive, includeNegative, includeNeutral, includeArgued, includeNotArgued);
                }
            }
        }

        public string GetQualifications(bool includePositive, bool includeNegative, bool includeNeutral, bool includeArgued, bool includeNotArgued)
        {
            QualificationInfo QInfo = QualificationInfo.FromActivityId(m_ClientLink, m_CInfo.Activity);

            if (QInfo == null)
                return "";

            System.Text.StringBuilder SB = new System.Text.StringBuilder();

            RecurseQualifications(QInfo, SB, includePositive, includeNegative, includeNeutral, includeArgued, includeNotArgued);

            return SB.ToString();
        }

        public void setQualification(string id, string date, string number)
        {
            SetQualification(id, date, number);
        }

        public void SetQualification(string id, string date, string number)
        {
            m_ClientLink.SetQualification(m_CInfo.Id, id, string.IsNullOrEmpty(date) ? null : date, string.IsNullOrEmpty(number) ? null : number);
        }

        public void CallbackByPreviousAgent(string qualId)
        {
            //TO DO: we shouldn't use qualief by for the user.

            ContactHistory history = m_CInfo.GetHistory();

            if (history != null)
            {
                string number = "*";

                for (int i = 0; i < history.Count; i++)
                {
                    if (history[i].TalkTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(history[i].QualifiedById))
                    {
                        m_ClientLink.SetQualification(m_CInfo.Id, qualId, DateTime.Now.ToString("yyyyMMddHHmmss"), number, history[i].QualifiedById);
                    }
                }
            }
        }

        public string GetDataXmlFragment(string path)
        {
            if (m_ContextData == null)
            {
                if (!GetContextData(-1))
                    return "";
            }

            XmlNode Fragment = m_ContextData.DocumentElement.SelectSingleNode(path);

            if (Fragment != null)
                return Fragment.OuterXml;

            return "";
        }

        internal Dictionary<string, object> m_SessionValues = new Dictionary<string, object>();
        
        internal Stack<string> m_NavigationHistory = new Stack<string>();

        public object GetSessionValue(string name)
        {
            if (name.StartsWith("@"))
            {
                if (name.Equals("@back", StringComparison.OrdinalIgnoreCase))
                {
                    if (m_NavigationHistory.Count >= 2)
                    {
                        m_NavigationHistory.Pop();
                        return m_NavigationHistory.Pop();
                    }
                }
                else if (name.Equals("@this", StringComparison.OrdinalIgnoreCase))
                {
                    return m_NavigationHistory.Pop();
                }
                else if (name.Equals("@AgentDescription", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Concat(m_ClientLink.Account ?? "", ", ", m_ClientLink.Name ?? "");
                }
                else if (name.Equals("@AgentAccount", StringComparison.OrdinalIgnoreCase))
                {
                    return m_ClientLink.Account ?? "";
                }
                else if (name.Equals("@AgentName", StringComparison.OrdinalIgnoreCase))
                {
                    return m_ClientLink.Name ?? "";
                }
                else if (name.Equals("@ContactId", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.Id;
                }
                else if (name.Equals("@ContactListId", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.ContactListId;
                }
                else if (name.Equals("@Activity", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.Activity;
                }
                else if (name.Equals("@Context", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.Context;
                }
                else if (name.Equals("@CustomerId", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.CustomerId;
                }
                else if (name.Equals("@CustomerDescription", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.CustomerDescription;
                }
                else if (name.Equals("@Direction", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.Direction;
                }
                else if (name.Equals("@From", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.From;
                }
                else if (name.Equals("@To", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.To;
                }
                else if (name.Equals("@UUI", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.UUI;
                }
                else if (name.Equals("@Language", StringComparison.OrdinalIgnoreCase))
                {
                    return m_CInfo.Language;
                }
                else if (name.Equals("@ContactAddress", StringComparison.OrdinalIgnoreCase))
                {
                    return (m_CInfo.Direction == "I") ? m_CInfo.From : m_CInfo.To;
                }
            }
            else
            {
                if (m_SessionValues.ContainsKey(name))
                    return m_SessionValues[name];
            }

            return "";
        }

        public void SetSessionValue(string name, object value)
        {
            if (name.StartsWith("@"))
            {
                if (name.Equals("@back", StringComparison.OrdinalIgnoreCase))
                {
                    string Tmp = m_NavigationHistory.Pop();

                    if (m_NavigationHistory.Count > 0)
                        m_NavigationHistory.Pop();

                    m_NavigationHistory.Push(value.ToString());
                    m_NavigationHistory.Push(Tmp);
                }
                else if (name.Equals("@this", StringComparison.OrdinalIgnoreCase))
                {
                    m_NavigationHistory.Pop();
                    m_NavigationHistory.Push(value.ToString());
                }
            }
            else
            {
                m_SessionValues[name] = value;
            }
        }

        public void InitiateNewActivityCall(string activityId, string contactListId, string customerId, string phoneNumber, bool dialNow)
        {
            if (dialNow)
            {
                m_ClientLink.Commands.VoiceNewCall.Execute(phoneNumber, m_CInfo.Id, string.Format("AcId={0}",string.IsNullOrEmpty(activityId) ? m_CInfo.Activity : activityId), string.Format("CuId={0}",customerId), string.Format("LiId={0}",contactListId));
            }
            else
            {
                m_ClientLink.Commands.SearchMode.Execute(activityId);
            }
        }

        public void VoiceNewCall(string phoneNumber, bool inScript)
        {
            m_ClientLink.Commands.VoiceNewCall.Execute(phoneNumber, m_CInfo.Id, string.Format("AcId={0}", m_CInfo.Activity), string.Format("CuId={0}", m_CInfo.CustomerId), string.Format("LiId={0}", m_CInfo.ContactListId));
        }

        public void ShowTeamTransferWindow(string teamId)
        {
        }

        public void ShowTeamForwardWindow(string teamId)
        {
        }

        public void mailInsertText(string value)
        {
            ContactInfo mailInfo = m_ClientLink.Contacts[m_ClientLink.Contacts.ActiveContactId];

            if (mailInfo.Media != ContactMedia.Mail)
                return;

            MailPanelControl ctrl = mailInfo.UserData as MailPanelControl;

            if (ctrl == null)
                return;

            ctrl.MailInsertText(value);
        }

        public void mailForward(string destination, string contactId, int delay, bool sendResponseNow)
        {
            m_ClientLink.Commands.MailForward.Execute(destination, contactId, delay.ToString(),sendResponseNow.ToString());
        }
    }
}
