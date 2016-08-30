using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using System.Windows.Media;

namespace Nixxis.Client.Controls.HtmlHelpers
{
    public class NixxisHtmlDocument
    {
        #region enums
        public enum ReadyStates
        {
            Loading,
            Loaded,
            Interactive,
            Complete,
            Uninitialized,
        }
        #endregion

        #region Class data
        private IHTMLDocument2 m_HtmlDocument;
        #endregion

        #region Constructor
        public NixxisHtmlDocument(IHTMLDocument2 htmlDocumentSource)
        {
            m_HtmlDocument = htmlDocumentSource;
        }
        #endregion

        #region Properties
        public IHTMLDocument2 HtmlDocumentSource
        {
            get { return m_HtmlDocument; }
            set { m_HtmlDocument = value; }
        }
        public bool DesignMode
        {
            /*
             * Options:
             * On
             * Off
             */
            get
            {
                return m_HtmlDocument.designMode == "On" ? true : false;
            }
            set
            {
                if (value)
                    m_HtmlDocument.designMode = "On";
                else
                    m_HtmlDocument.designMode = "Off";
            }
        }
        public ReadyStates ReadyState
        {
            get
            {
                switch (m_HtmlDocument.readyState.ToLower())
                {
                    case "complete": return ReadyStates.Complete;
                    case "interactive": return ReadyStates.Interactive;
                    case "loaded": return ReadyStates.Loaded;
                    case "loading": return ReadyStates.Loading;
                    default: return ReadyStates.Uninitialized;
                }
            }
        }
        #endregion

        #region Members
        //Basic Interface to IHTMLDocument2
        public bool ExecuteCommand(string cmdID, bool showUI, object value)
        {
            return m_HtmlDocument.execCommand(cmdID, showUI, value);
        }
        public bool QueryCommandEnabled(string cmdID)
        {
            return m_HtmlDocument.queryCommandEnabled(cmdID);
        }
        public bool QueryCommandState(string cmdID)
        {
            return m_HtmlDocument.queryCommandState(cmdID);
        }
        public object QueryCommandValue(string cmdID)
        {
            return m_HtmlDocument.queryCommandValue(cmdID);
        }

        //Interaction members
        public void SetHtmlContent(string html)
        {
            try
            {
                m_HtmlDocument.body.innerHTML = html;
            }
            catch { }
        }
        public void SetHtmlText(string text)
        {
            try
            {
                m_HtmlDocument.body.innerHTML = "<html><body>" + text + "</body></html>";
            }
            catch { }
        }
        public string GetHtmlText()
        {
            try
            {
                return m_HtmlDocument.body.innerHTML;
            }
            catch { return string.Empty; }
        }
        public void InsertHTML(string html)
        {
            IHTMLTxtRange range = m_HtmlDocument.selection.createRange() as IHTMLTxtRange;
            range.pasteHTML(html);
        }

        //Action
        public void InsertBubbledList()
        {
            ExecuteCommand("InsertUnorderedList", false, null);
        }
        public void InsertNumericList()
        {
            ExecuteCommand("InsertOrderedList", false, null);
        }
        public void InsertHyperlink()
        {
            ExecuteCommand("CreateLink", true, "");
        }
        public void InsertHyperlink(HyperlinkObject link)
        {
            string url = HtmlEncoding(link.Url);
            string text = HtmlEncoding(link.Text);

            if (string.IsNullOrEmpty(text))
                text = url;

            string hyperlink = string.Format("<a href=\"{0}\">{1}</a>", url, text);
            InsertHTML(hyperlink);
        }
        public void RemoveHyperlink()
        {
            ExecuteCommand("Unlink", false, "");
        }

        //Style
        public bool IsBold()
        {
            return this.QueryCommandState("Bold");
        }
        public bool IsItalic()
        {
            return this.QueryCommandState("Italic");
        }
        public bool IsUnderline()
        {
            return this.QueryCommandState("Underline");
        }
        public bool IsSubscript()
        {
            return this.QueryCommandState("Subscript");
        }
        public bool IsSuperscript()
        {
            return this.QueryCommandState("Superscript");
        }
        public bool IsNumericList()
        {
            return this.QueryCommandState("InsertOrderedList");
        }
        public bool IsBubbledList()
        {
            return this.QueryCommandState("InsertUnorderedList");
        }
        public bool IsJustifyLeft()
        {
            return this.QueryCommandState("JustifyLeft");
        }
        public bool IsJustifyCenter()
        {
            return this.QueryCommandState("JustifyCenter");
        }
        public bool IsJustifyRight()
        {
            return this.QueryCommandState("JustifyRight");
        }
        public bool IsJustifyFull()
        {
            return this.QueryCommandState("JustifyFull");
        }

        public void SetForeColor(Color value)
        {
            this.ExecuteCommand("ForeColor", false, string.Format("#{0:X2}{1:X2}{2:X2}", value.R, value.G, value.B));
        }
        public Color GetForeColor()
        {
            if (this.ReadyState != ReadyStates.Complete)
                return Colors.White;

            string color = this.QueryCommandValue("ForeColor").ToString();

            if (string.IsNullOrEmpty(color))
                return Colors.White;

            return NixxisColorCollection.ConvertToColor(color);
        }
        public void SetBackColor(Color value)
        {
            this.ExecuteCommand("BackColor", false, string.Format("#{0:X2}{1:X2}{2:X2}", value.R, value.G, value.B));
        }
        public Color GetBackColor()
        {
            if (this.ReadyState != ReadyStates.Complete)
                return Colors.Black;

            string color = this.QueryCommandValue("BackColor").ToString();

            if (string.IsNullOrEmpty(color))
                return Colors.Black;

            return NixxisColorCollection.ConvertToColor(color);
        }

        public void SetFontFamily(FontFamily value)
        {
            Tools.Log("Change font family to " + value.ToString(), Tools.LogType.Debug);
            this.ExecuteCommand("FontName", false, value.ToString());
        }
        public FontFamily GetFontFamily()
        {
            if (this.ReadyState != ReadyStates.Complete)
                return null;

            string name = this.QueryCommandValue("FontName") as string;

            if (name == null)
                return null;

            return new FontFamily(name);
        }
        public void SetFontSize(FontSizeItem value)
        {
            Tools.Log("Change font size to " + value.Text, Tools.LogType.Debug);
            this.ExecuteCommand("FontSize", false, value.Key);
        }
        public FontSizeItem GetFontSize()
        {
            if (this.ReadyState != ReadyStates.Complete)
                return NixxisFont.NO;

            switch (this.QueryCommandValue("FontSize").ToString())
            {
                case "1": return NixxisFont.XXSmall;
                case "2": return NixxisFont.XSmall;
                case "3": return NixxisFont.Small;
                case "4": return NixxisFont.Middle;
                case "5": return NixxisFont.Large;
                case "6": return NixxisFont.XLarge;
                case "7": return NixxisFont.XXLarge;
                default: return NixxisFont.NO;
            }
        }
        #endregion

        #region Members Static
        public static string HtmlEncoding(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("<", "&lt;");
                value = value.Replace(">", "&gt;");
                value = value.Replace(" ", "&nbsp;");
                value = value.Replace("\"", "&quot;");
                value = value.Replace("\'", "&#39;");
                value = value.Replace("&", "&amp;");
                
                return value;
            }

            return string.Empty;
        }

        public static string HtmlDecoding(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("&lt;", "<");
                value = value.Replace("&gt;", ">");
                value = value.Replace("&nbsp;", " ");
                value = value.Replace("&quot;", "\"");
                value = value.Replace("&#39;", "\'");
                value = value.Replace("&amp;", "&");
                
                return value;
            }
            
            return string.Empty;
        }
        #endregion
    }
}
