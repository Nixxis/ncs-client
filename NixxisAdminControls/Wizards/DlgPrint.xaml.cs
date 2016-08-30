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
using Microsoft.Reporting.WinForms;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgPrint : Window
    {


        private static TranslationContext m_TranslationContext = new TranslationContext("DlgPrint");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public SortedList<string, string> Parameters = new SortedList<string, string>(); 
        public string reportName = string.Empty;
        public string reportPath = string.Empty;
        public bool showParameterPrompt = false;

        public DlgPrint()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;


            InitializeComponent();

            _reportViewer.Load += ReportViewer_Load;

        }

        private void ReportViewer_Load(object sender, EventArgs e)
        {
            try
            {
                AgentLoginInfo loginInfo = (Application.Current.MainWindow as IAgentLoginInfoContainer).AgentLoginInfo;

                _reportViewer.PromptAreaCollapsed = !showParameterPrompt;
                _reportViewer.ShowCredentialPrompts = false;
                _reportViewer.ShowParameterPrompts = showParameterPrompt;
                _reportViewer.AutoSize = true;
                _reportViewer.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Remote;
                _reportViewer.ServerReport.ReportServerUrl = new Uri(loginInfo.ReportServerUrl);
                if(!string.IsNullOrEmpty(reportPath))
                    _reportViewer.ServerReport.ReportPath = reportPath;
                else
                    _reportViewer.ServerReport.ReportPath = string.Format("/{0}/{1}", loginInfo.ReportsBasePath, reportName);

                _reportViewer.ServerReport.ReportServerCredentials.NetworkCredentials = new System.Net.NetworkCredential(
                    loginInfo.ReportServerCredentialsUser,
                    loginInfo.ReportServerCredentialsPassword,
                    loginInfo.ReportServerCredentialsDomain);

                ReportParameterInfoCollection Params = _reportViewer.ServerReport.GetParameters();
                List<ReportParameter> ReportsParams = new List<ReportParameter>();
                
                foreach (ReportParameterInfo rpi in Params)
                {
                    if (rpi.Name == "user")
                    {
                        ReportsParams.Add(new ReportParameter(rpi.Name, loginInfo.Id ));
                    }
                    else
                    {
                        if (rpi.MultiValue)
                        {
                            if (Parameters.ContainsKey(rpi.Name))
                                ReportsParams.Add(new ReportParameter(rpi.Name, Parameters[rpi.Name].Split(';')));
                            else
                                ReportsParams.Add(new ReportParameter(rpi.Name, Parameters[string.Empty]));
                        }
                        else
                        {
                            if (Parameters.ContainsKey(rpi.Name))
                                ReportsParams.Add(new ReportParameter(rpi.Name, Parameters[rpi.Name]));
                            else
                                ReportsParams.Add(new ReportParameter(rpi.Name, Parameters[string.Empty]));
                        }
                    }

                }

                _reportViewer.ServerReport.SetParameters(ReportsParams);
               
                _reportViewer.RefreshReport();

                _reportViewer.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.FullPage;
            }
            catch(Exception ex)
            {
                
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

    }

}
