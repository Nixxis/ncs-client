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
using Nixxis.Client.Controls;
using System.Data;
using Microsoft.Win32;
using Nixxis.Client.Reporting.ReportingService2010;
using System.Net;
using Nixxis.Client.Admin;
using Common.Tools;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Markup;

namespace Nixxis.Client.Reporting
{
    /// <summary>
    /// Interaction logic for ReportingFrameSet.xaml
    /// 
    /// TO DO:
    /// 1. Inbound and outbound list
    /// 2. Reading of the time parameter
    /// </summary>
    public partial class ReportingFrameSet : UserControl
    {
        #region Static
        public static TranslationContext TranslationContext = new TranslationContext("ReportingFrameSet");
        public static RoutedUICommand ExportReport { get; private set; }
        public static RoutedUICommand ViewReport { get; private set; }

        static ReportingFrameSet()
        {
            ExportReport = new RoutedUICommand(string.Empty, "ExportReport", typeof(ReportingFrameSet));
            ViewReport = new RoutedUICommand(string.Empty, "ViewReport", typeof(ReportingFrameSet));

        }
        #endregion

        #region Class data
        private string m_MainWindowTitle = string.Empty;
        private ContextDataClient m_ContextDataClient;

        private DateTime m_LastDate = DateTime.Now.Date.Subtract(new TimeSpan(24, 0, 0));
        private TimeSpan m_LastTime =new TimeSpan(24, 0, 0);
        private int m_LastCampaign = 0;
        private int m_LastActivity = 0;
        private int m_LastAgent = 0;
        #endregion

        #region Constrcutors
        public ReportingFrameSet(string serviceUri)
        {
            ReportCollection = new NixxisReportCollection();

            m_MainWindowTitle = Application.Current.MainWindow.Title;
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(ReportingFrameSet_IsVisibleChanged);

            InitializeComponent();

            GetStandardReports(TranslationContext.Translate("Standard reports"));
            GetCustomerExportReports(TranslationContext.Translate("Custom export reports"), serviceUri);
        }
        #endregion

        #region Properties
        public NixxisReportCollection ReportCollection { get; set; }
        public DisplayList ParametersAgents { get; set; }
        public DisplayList ParametersCampaigns { get; set; }
        public DisplayList ParametersActivities { get; set; }
        public DisplayList ParametersActivitiesInbound { get; set; }
        public DisplayList ParametersActivitiesOutbound { get; set; }

        void GetStandardReports(string categorie)
        {

            AgentLoginInfo loginInfo = (Application.Current.MainWindow as IAgentLoginInfoContainer).AgentLoginInfo;

            NixxisReportItem reports = ReportCollection.GetValueOfName(categorie);
            if (reports == null)
            {
                reports = new NixxisReportItem();
                reports.Name = categorie;

                ReportCollection.Add(reports);
            }
            reports.Children.Clear();


            ReportingService2010.ReportingService2010SoapClient cl = new ReportingService2010.ReportingService2010SoapClient(
                new System.ServiceModel.BasicHttpBinding() {
                        MaxReceivedMessageSize = 2147483647,
                        MaxBufferSize = 2147483647,
                        Security = new System.ServiceModel.BasicHttpSecurity() { 
                           Mode = loginInfo.ReportServerUrl.StartsWith("https", StringComparison.InvariantCultureIgnoreCase) ? System.ServiceModel.BasicHttpSecurityMode.Transport: System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly, 
                           Transport = new System.ServiceModel.HttpTransportSecurity() { 
                               ClientCredentialType=System.ServiceModel.HttpClientCredentialType.Ntlm} 
                       } 
                }, 
                
                new System.ServiceModel.EndpointAddress(string.Concat(loginInfo.ReportServerUrl, "/reportservice2010.asmx" )));
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(loginInfo.ReportServerCredentialsUser, loginInfo.ReportServerCredentialsPassword, loginInfo.ReportServerCredentialsDomain);
            cl.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            cl.Open();

            ReportingService2010.CatalogItem[] catalogItems;
            TrustedUserHeader t = new TrustedUserHeader();

            try
            {
                cl.ListChildren(t, loginInfo.ReportsStatsBasePath, true, out catalogItems);
            }
            catch(Exception ex)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = string.Format(TranslationContext.Translate(string.Format("Reporting is not properly configured!\n({0})", ex.Message.Replace(". ", ".\n"))) );
                dlg.Owner = Application.Current.MainWindow;
                dlg.IsInfoDialog = true;
                dlg.ShowDialog();
                return;

            }
            

            foreach (ReportingService2010.CatalogItem c in catalogItems)
            {
                if (c.TypeName.Equals("report", StringComparison.InvariantCultureIgnoreCase) && !c.Hidden)
                {                    
                    reports.AddChild(new NixxisReportItem() { Description = c.Name, Name = c.Name, ReportType = ReportType.ReportingServer, Path = c.Path });
                }
            }
        }

        void GetCustomerExportReports(string categorie, string serviceUri)
        {
            NixxisReportItem reports = ReportCollection.GetValueOfName(categorie);
            if (reports == null)
            {
                reports = new NixxisReportItem();
                reports.Name = categorie;

                ReportCollection.Add(reports);
            }
            reports.Children.Clear();

            string ConnectionInfo = null;

            string DnsServer = string.Empty;

            try
            {
                m_ContextDataClient = new ContextDataClient(serviceUri);
            }
            catch(Exception Ex)
            {
                Clipboard.SetText("Cat:" + (categorie ?? "") + "\r\nUri:" + (serviceUri ?? "") + "\r\n" + Ex.ToString());
                MessageBox.Show(ConnectionInfo, "Unable to connect session manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                DataSet DS = m_ContextDataClient.LoadDataSet("reporting", "NixxisCustomExtracts", null);

                foreach (DataRow Row in DS.Tables[0].Rows)
                {
                    NixxisReportItem report = new NixxisReportItem();
                    report.Name = string.IsNullOrEmpty(Row["title"].ToString()) ? Row["name"].ToString() : Row["title"].ToString();
                    report.Description = Row["description"].ToString();
                    report.ReportSettings = null;
                    report.Tag = Row;
                    reports.AddChild(report);
                }

                if (DS.Tables.Count >= 4)	// Admin data included
                {
                    ParametersAgents = DisplayList.ConverFromDataTable(DS.Tables[1], "id", new string[] { "Account", "LastName", "FirstName" }, "{0}, {1} {2}");
                    ParametersAgents.Sort( (a,b) => (a.Description.CompareTo(b.Description)));
                    ParametersCampaigns = DisplayList.ConverFromDataTable(DS.Tables[2]);
                    ParametersCampaigns.Sort((a, b) => (a.Description.CompareTo(b.Description)));
                    ParametersActivities = DisplayList.ConverFromDataTable(DS.Tables[3]);
                    ParametersActivities.Sort((a, b) => (a.Description.CompareTo(b.Description)));

                    //To do : inbound and outbound filter
                    ParametersActivitiesInbound = ParametersActivities;
                    ParametersActivitiesOutbound = ParametersActivities;
                }
            }
            catch
            {
            }
        }
        #endregion

        
        void ReportingFrameSet_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as ReportingFrameSet).IsVisible)
            {
                ReportingFrameSet_Loaded(sender, null);
            }
            else
            {
                ReportingFrameSet_Unloaded(sender, null);
            }
        }
        void ReportingFrameSet_Loaded(object sender, RoutedEventArgs e)
        {

            Focus();
            NixxisBaseExpandPanel nep = null;
            nep = FindResource("CoverflowAgent") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                {
                    NixxisGrid.SetPanelCommand.Execute(nep);
                }
            }

            nep = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                {
                    NixxisGrid.SetPanelCommand.Execute(nep);
                }
            }

            nep = FindResource("ReportingToolbarPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                NixxisGrid.SetPanelCommand.Execute(nep);
            }
        }
        void ReportingFrameSet_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = m_MainWindowTitle;

            NixxisBaseExpandPanel nep = null;
            nep = FindResource("CoverflowAgent") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }

            nep = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }

            nep = FindResource("ReportingToolbarPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                NixxisGrid.RemovePanelCommand.Execute(nep);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NixxisReportItem nri = null;
            if (trvReports.SelectedItem != null)
                nri = (NixxisReportItem)trvReports.SelectedItem;
            else
                return;

            if (e.Command == ReportingFrameSet.ExportReport)
            {

                if (nri.ReportType == ReportType.ReportingServer)
                {
                    ChoiceDialog dlg = new ChoiceDialog();

                    dlg.Owner = Application.Current.MainWindow;
                    dlg.MessageText = TranslationContext.Translate("Choose the export format:");
                    dlg.ItemsSource = ReportsSettings.Extensions.Where((a)=>(a.Visible)).Select((a) => (a.LocalizedName));
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {

                        WaitScreen ws = new WaitScreen();
                        ws.Text = TranslationContext.Translate("Please wait...");
                        ws.Owner = Application.Current.MainWindow;
                        ws.Show();

                        string strExtension;
                        byte[] buffer = ReportsSettings[nri.Path].Execute(ReportsSettings.Extensions.Where((a) => (a.Visible)).ElementAt(dlg.SelectedIndex).Name, out strExtension);

                        ws.Close();

                        string strPath = BrowseForFile(strExtension, ReportsSettings.Extensions.Where((a) => (a.Visible)).ElementAt(dlg.SelectedIndex).LocalizedName);

                        if (!string.IsNullOrEmpty(strPath))
                        {
                            using (FileStream fs = File.OpenWrite(strPath))
                            {
                                fs.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }


                }
                else
                {
                    WaitScreen ws = new WaitScreen();
                    ws.Text = TranslationContext.Translate("Please wait...");
                    ws.Owner = Application.Current.MainWindow;
                    ws.Show();

                    DoExportCustomReport( /*dlg.SelectedIndex==0 ? "Xml":*/ "Csv" );

                    ws.Close();
                }
            }
            else if(e.Command == ReportingFrameSet.ViewReport)
            {
                if (nri.ReportType == ReportType.ReportingServer)
                {
                    WaitScreen ws = new WaitScreen();
                    ws.Text = TranslationContext.Translate("Please wait...");
                    ws.Owner = Application.Current.MainWindow;
                    ws.Show();

                    DlgPrint dlgPrint = new DlgPrint();

                    dlgPrint.Title = TranslationContext.Translate("Report");



                    foreach (ParameterValue pv in ReportsSettings[nri.Path].CurrentParameterValues2010)
                    {
                        if (dlgPrint.Parameters.ContainsKey(pv.Name))
                            dlgPrint.Parameters[pv.Name] = string.Concat(dlgPrint.Parameters[pv.Name], ";", pv.Value);
                        else
                            dlgPrint.Parameters.Add(pv.Name, pv.Value);
                    }

                    dlgPrint.reportPath = nri.Path;

                    dlgPrint.Owner = Application.Current.MainWindow;
                    dlgPrint.ShowDialog();

                    ws.Close();

                }
                else
                {
                    
                }
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            NixxisReportItem nri = trvReports.SelectedItem as NixxisReportItem;

            if (nri == null)
                e.CanExecute = false;
            else
            {

                if (e.Command == ReportingFrameSet.ExportReport)
                {
                    e.CanExecute = nri.IsEnabled;
                }
                else if (e.Command == ReportingFrameSet.ViewReport)
                {
                    if (nri.ReportType == ReportType.CustomExport)
                        e.CanExecute = false; // TODO
                    else
                        e.CanExecute = nri.IsEnabled;
                }
            }


            e.Handled = true;
        }

        #region Export Custom Members

        private void trvReports_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            spParameters.Children.Clear();
            ReportingFrameSet parent = this;

            if (trvReports.SelectedItem != null && parent != null)
            {
                NixxisReportItem nri = ((NixxisReportItem)trvReports.SelectedItem);

                if (nri.ReportType == ReportType.ReportingServer)
                {
                    InitParameters(nri.Path);
                }
                else
                {
                    DataRow Row = nri.Tag as DataRow;

                    if (Row != null)
                    {
                        string[] Params = Row["parameters"].ToString().Split(';');
                        string[] Labels = (string.IsNullOrEmpty(Row["labels"].ToString())) ? null : Row["labels"].ToString().Split(';');

                        for (int i = 0; i < Params.Length; i++)
                        {
                            string Param = Params[i];
                            string[] Parts = Param.Split(',');

                            if (Parts.Length > 2)
                            {
                                Label ParamLabel = new Label();

                                if (Labels != null && Labels.Length > i)
                                    ParamLabel.Content = Labels[i];
                                else
                                    ParamLabel.Content = Parts[0];

                                spParameters.Children.Add(ParamLabel);

                                if (Parts[1].Equals("datetime", StringComparison.OrdinalIgnoreCase))
                                {
                                    DatePicker picker = new DatePicker();
                                    TimePicker timepicker = new TimePicker();
                                    bool addTimePicker = false;

                                    if (Parts[0].StartsWith("@time", StringComparison.OrdinalIgnoreCase))
                                    {
                                        //TO DO: give last selected time
                                        addTimePicker = true;
                                    }
                                    else
                                    {
                                        picker.SelectedDate = m_LastDate;
                                        picker.SelectedDateFormat = DatePickerFormat.Short;
                                    }
                                    spParameters.Children.Add(picker);

                                    if (addTimePicker)
                                        spParameters.Children.Add(timepicker);
                                }
                                else if (Parts[1].Equals("bit", StringComparison.OrdinalIgnoreCase))
                                {
                                    CheckBox CB = new CheckBox();

                                    spParameters.Children.Add(CB);
                                }
                                else
                                {
                                    Control Input;

                                    if (Parts[1].EndsWith("char", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Input = new TextBox();
                                        int ParamLength = int.Parse(Parts[2]);


                                        if (Parts[0].StartsWith("@Campaign", StringComparison.OrdinalIgnoreCase) && parent.ParametersCampaigns != null && (ParamLength >= 32 || ParamLength == -1))
                                        {
                                            #region Campaign parameter
                                            if (ParamLength == 32)
                                            {
                                                ComboBox list = new ComboBox();

                                                list.ItemsSource = parent.ParametersCampaigns;
                                                list.DisplayMemberPath = "Description";
                                                list.SelectedValuePath = "Id";
                                                list.SelectedIndex = m_LastCampaign;

                                                Input = list;
                                            }
                                            else
                                            {
                                                NixxisComboBox cCombo = new NixxisComboBox();

                                                cCombo.ItemSingleList = parent.ParametersCampaigns;
                                                cCombo.MultiSelection = true;
                                                cCombo.ItemListDescriptionProperty = "Description";
                                                cCombo.ItemListIdProperty = "Id";
                                                cCombo.IsEditable = false;
                                                cCombo.CollapseImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Collapse.png"));
                                                cCombo.ExpandImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Expand.png"));

                                                ContextMenu cx = new ContextMenu();
                                                MenuItem mi = new MenuItem();
                                                mi.Header = ReportingFrameSet.TranslationContext.Translate("Select all");
                                                mi.Click += (object snder, RoutedEventArgs args) =>
                                                {
                                                    cCombo.StopSelectedItemsIdChangeNotifications();
                                                    foreach (NixxisComboBoxItem ncbi in cCombo.Items)
                                                        ncbi.IsSelected = true;
                                                    cCombo.DoSelectedItemsIdChangeNotifications();
                                                };
                                                cx.Items.Add(mi);

                                                mi = new MenuItem();
                                                mi.Header = ReportingFrameSet.TranslationContext.Translate("Select none");
                                                mi.Click += (object snder, RoutedEventArgs args) =>
                                                {
                                                    cCombo.StopSelectedItemsIdChangeNotifications();
                                                    foreach (NixxisComboBoxItem ncbi in cCombo.Items)
                                                        ncbi.IsSelected = false;
                                                    cCombo.DoSelectedItemsIdChangeNotifications();
                                                };
                                                cx.Items.Add(mi);
                                                cCombo.ContextMenu = cx;

                                                Input = cCombo;
                                            }
                                            #endregion
                                        }
                                        else if (Parts[0].StartsWith("@Activit", StringComparison.OrdinalIgnoreCase) && parent.ParametersActivities != null && (ParamLength >= 32 || ParamLength == -1))
                                        {
                                            #region Activity parameter

                                            bool includeIn = true;
                                            bool includeOut = true;

                                            if (Parts[0].IndexOf("inbound", StringComparison.OrdinalIgnoreCase) > 0)
                                            {
                                                includeOut = false;
                                            }
                                            else if (Parts[0].IndexOf("outbound", StringComparison.OrdinalIgnoreCase) > 0)
                                            {
                                                includeIn = false;
                                            }

                                            if (ParamLength == 32)
                                            {
                                                ComboBox list = new ComboBox();

                                                if (includeIn && !includeOut)
                                                    list.ItemsSource = parent.ParametersActivitiesInbound;
                                                else if (!includeIn && includeOut)
                                                    list.ItemsSource = parent.ParametersActivitiesOutbound;
                                                else
                                                    list.ItemsSource = parent.ParametersActivities;

                                                list.DisplayMemberPath = "Description";
                                                list.SelectedValuePath = "Id";

                                                list.SelectedIndex = m_LastActivity;
                                                Input = list;
                                            }
                                            else
                                            {

                                                NixxisComboBox cCombo = new NixxisComboBox();

                                                if (includeIn && !includeOut)
                                                    cCombo.ItemSingleList = parent.ParametersActivitiesInbound;
                                                else if (!includeIn && includeOut)
                                                    cCombo.ItemSingleList = parent.ParametersActivitiesOutbound;
                                                else
                                                    cCombo.ItemSingleList = parent.ParametersActivities;

                                                cCombo.MultiSelection = true;
                                                cCombo.ItemListDescriptionProperty = "Description";
                                                cCombo.ItemListIdProperty = "Id";
                                                cCombo.IsEditable = false;
                                                cCombo.CollapseImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Collapse.png"));
                                                cCombo.ExpandImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Expand.png"));


                                                ContextMenu cx = new ContextMenu();
                                                MenuItem mi = new MenuItem();
                                                mi.Header = ReportingFrameSet.TranslationContext.Translate("Select all");
                                                mi.Click += (object snder, RoutedEventArgs args) =>
                                                {
                                                    cCombo.StopSelectedItemsIdChangeNotifications();
                                                    foreach (NixxisComboBoxItem ncbi in cCombo.Items)
                                                        ncbi.IsSelected = true;
                                                    cCombo.DoSelectedItemsIdChangeNotifications();
                                                };
                                                cx.Items.Add(mi);

                                                mi = new MenuItem();
                                                mi.Header = ReportingFrameSet.TranslationContext.Translate("Select none");
                                                mi.Click += (object snder, RoutedEventArgs args) =>
                                                {
                                                    cCombo.StopSelectedItemsIdChangeNotifications();
                                                    foreach (NixxisComboBoxItem ncbi in cCombo.Items)
                                                        ncbi.IsSelected = false;
                                                    cCombo.DoSelectedItemsIdChangeNotifications();
                                                };
                                                cx.Items.Add(mi);
                                                cCombo.ContextMenu = cx;

                                                Input = cCombo;
                                            }
                                            #endregion
                                        }
                                        else if (Parts[0].StartsWith("@Agent", StringComparison.OrdinalIgnoreCase) && parent.ParametersAgents != null && (ParamLength >= 32 || ParamLength == -1))
                                        {
                                            #region Agent parameter
                                            if (ParamLength == 32)
                                            {
                                                ComboBox list = new ComboBox();

                                                list.ItemsSource = parent.ParametersAgents;
                                                list.DisplayMemberPath = "Description";
                                                list.SelectedValuePath = "Id";
                                                list.SelectedIndex = m_LastAgent;

                                                Input = list;
                                            }
                                            else
                                            {
                                                NixxisComboBox cCombo = new NixxisComboBox();

                                                cCombo.ItemSingleList = parent.ParametersAgents;
                                                cCombo.MultiSelection = true;
                                                cCombo.ItemListDescriptionProperty = "Description";
                                                cCombo.ItemListIdProperty = "Id";
                                                cCombo.IsEditable = false;
                                                cCombo.CollapseImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Collapse.png"));
                                                cCombo.ExpandImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Expand.png"));


                                                ContextMenu cx = new ContextMenu();
                                                MenuItem mi = new MenuItem();
                                                mi.Header = ReportingFrameSet.TranslationContext.Translate("Select all");
                                                mi.Click += (object snder, RoutedEventArgs args) =>
                                                {
                                                    cCombo.StopSelectedItemsIdChangeNotifications();
                                                    foreach (NixxisComboBoxItem ncbi in cCombo.Items)
                                                        ncbi.IsSelected = true;
                                                    cCombo.DoSelectedItemsIdChangeNotifications();
                                                };
                                                cx.Items.Add(mi);

                                                mi = new MenuItem();
                                                mi.Header = ReportingFrameSet.TranslationContext.Translate("Select none");
                                                mi.Click += (object snder, RoutedEventArgs args) =>
                                                {
                                                    cCombo.StopSelectedItemsIdChangeNotifications();
                                                    foreach (NixxisComboBoxItem ncbi in cCombo.Items)
                                                        ncbi.IsSelected = false;
                                                    cCombo.DoSelectedItemsIdChangeNotifications();
                                                };
                                                cx.Items.Add(mi);
                                                cCombo.ContextMenu = cx;

                                                Input = cCombo;
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            if (ParamLength > 0)
                                                ((TextBox)Input).MaxLength = ParamLength;
                                        }
                                    }
                                    else
                                    {
                                        Input = new TextBox();
                                    }

                                    spParameters.Children.Add(Input);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DoExportCustomReport(string extension )
        {
            if (this.trvReports.SelectedItem != null)
            {
                DataRow Row = ((NixxisReportItem)trvReports.SelectedItem).Tag as DataRow;

                if (Row != null)
                {
                    StringBuilder SB = new StringBuilder("?action=remotedata&source=reporting&timeout=900&exec=").Append(Row["name"]);

                    try
                    {
                        string[] Params = Row["parameters"].ToString().Split(';');

                        int childCount = 0;
                        for (int i = 0; i < Params.Length; i++)
                        {
                            string Param = Params[i];
                            string[] Parts = Param.Split(',');

                            if (Parts.Length > 2)
                            {
                                UIElement Input = null;

                                while (childCount < spParameters.Children.Count && Input == null)
                                {
                                    if (spParameters.Children[childCount] is Label)
                                        childCount++;
                                    else
                                    {
                                        Input = spParameters.Children[childCount]; 
                                        childCount++;
                                    }
                                }

                                SB.Append("&").Append(Parts[0]).Append("=");

                                if (Input is DatePicker)
                                {
                                    m_LastDate = ((DatePicker)Input).SelectedDate == null ? DateTime.Now : (DateTime)((DatePicker)Input).SelectedDate;
                                    SB.Append("d,").Append(m_LastDate.ToString("yyyyMMdd"));
                                }
                                else if (Input is NixxisComboBox)
                                {
                                    SB.Append("s,");

                                    if (((NixxisComboBox)Input).SelectedItems != null)
                                    {
                                        for (int j = 0; j < ((NixxisComboBox)Input).SelectedItems.Length; j++)
                                        {
                                            if (j > 0)
                                                SB.Append(',');

                                            SB.Append(((DisplayItem)((NixxisComboBoxItem)((NixxisComboBox)Input).SelectedItems[j]).ReturnObject).Id);
                                        }
                                    }
                                }
                                else if (Input is ComboBox)
                                {
                                    if (Parts[0].StartsWith("@Campaign", StringComparison.OrdinalIgnoreCase))
                                    {
                                        m_LastCampaign = ((ComboBox)Input).SelectedIndex;
                                        SB.Append("s,").Append(((DisplayItem)((ComboBox)Input).SelectedItem).Id);
                                    }
                                    else if (Parts[0].StartsWith("@Activit", StringComparison.OrdinalIgnoreCase))
                                    {
                                        m_LastActivity = ((ComboBox)Input).SelectedIndex;
                                        SB.Append("s,").Append(((DisplayItem)((ComboBox)Input).SelectedItem).Id);
                                    }
                                    else if (Parts[0].StartsWith("@Agent", StringComparison.OrdinalIgnoreCase))
                                    {
                                        m_LastAgent = ((ComboBox)Input).SelectedIndex;
                                        SB.Append("s,").Append(((DisplayItem)((ComboBox)Input).SelectedItem).Id);
                                    }
                                    else
                                    {
                                        SB.Append("s,").Append(((ComboBox)Input).Text);
                                    }
                                }
                                else if (Input is CheckBox)
                                {
                                    SB.Append("b,").Append(((CheckBox)Input).IsChecked.ToString());
                                }
                                else if (Input is TextBox)
                                {
                                    if (Parts[1].EndsWith("char", StringComparison.OrdinalIgnoreCase))
                                    {
                                        SB.Append("s,").Append(((TextBox)Input).Text);
                                    }
                                    else if (Parts[1].Equals("int", StringComparison.OrdinalIgnoreCase) || Parts[1].Equals("long", StringComparison.OrdinalIgnoreCase))
                                    {
                                        long Value;

                                        long.TryParse(((TextBox)Input).Text, out Value);
                                        SB.Append("i,").Append(Value);
                                    }
                                    else
                                    {
                                        double Value;

                                        if (double.TryParse(((TextBox)Input).Text, out Value))
                                        {
                                            SB.Append("f,").Append(Value);
                                        }
                                        else
                                        {
                                            SB.Append("s,").Append(((TextBox)Input).Text);
                                        }
                                    }
                                }
                                else
                                {
                                    SB.Append("s,").Append(Input.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show(Ex.GetBaseException().Message);
                    }

                    string strTempFile = System.IO.Path.GetTempFileName();

                    if (extension.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SB.Append("&output=csv&csvline=\\");
                    }

                    using (System.Net.HttpWebResponse Response = m_ContextDataClient.DoRequest(SB.ToString()))
                    {
                        if (Response == null || Response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            MessageBox.Show((Response == null) ? TranslationContext.Translate("No response from server") : (Response.StatusDescription ?? Response.StatusCode.ToString()));
                        }
                        else
                        {
                            using (System.IO.Stream ResponseStream = Response.GetResponseStream())
                            {
                                using (System.IO.Stream Output = System.IO.File.Open(strTempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read))
                                {
                                    byte[] Buffer = new byte[1500];
                                    int Read;

                                    while ((Read = ResponseStream.Read(Buffer, 0, Buffer.Length)) > 0)
                                        Output.Write(Buffer, 0, Read);
                                }
                            }
                        }
                    }


                    SaveFileDialog Dlg = new SaveFileDialog();

                    if (extension.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    }
                    else if (extension.Equals("xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    }
                   
                    Dlg.OverwritePrompt = true;
                    Dlg.ValidateNames = true;
                    Dlg.RestoreDirectory = false;
                    Dlg.CheckPathExists = true;
                    Dlg.AddExtension = true;

                    if (Dlg.ShowDialog().GetValueOrDefault())
                    {

                        try
                        {
                            File.Copy(strTempFile, Dlg.FileName, true);
                        }
                        catch (Exception Ex)
                        {
                            MessageBox.Show(Ex.GetBaseException().Message);
                        }
                                                
                    }

                    File.Delete(strTempFile);
                }
                else
                {
                    MessageBox.Show(TranslationContext.Translate("No valid definition for selected report"));
                }
            }
            else
            {
                MessageBox.Show(TranslationContext.Translate("No selected report"));
            }
        }
        #endregion


        private ReportsSettings m_ReportsSettings = null;

        public ReportsSettings ReportsSettings
        {
            get
            {
                if (m_ReportsSettings == null)
                {
                    AgentLoginInfo loginInfo = (Application.Current.MainWindow as IAgentLoginInfoContainer).AgentLoginInfo;
                    m_ReportsSettings = new ReportsSettings(loginInfo.ReportServerUrl, loginInfo.ReportServerCredentialsUser, loginInfo.ReportServerCredentialsPassword, loginInfo.ReportServerCredentialsDomain, loginInfo.Id);
                }
                return m_ReportsSettings;
            }
        }
        
        void InitParameters(string reportPath)
        {
            spParameters.Children.Clear();
            foreach (FrameworkElement fe in ReportsSettings.Init(reportPath))
                spParameters.Children.Add(fe);
            try
            {
                ReportCollection.All( (a) => ( (a.Children.First( (b) => ( b.Path.Equals(reportPath) ) ).ReportSettings = ReportsSettings[reportPath] )!=null ));
            }
            catch
            {
            }
        }

        private string BrowseForFile(string extension, string extensionDesc)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = string.Concat(".", extension); // Default file extension
            dlg.Filter = string.Format(TranslationContext.Translate("{0}|*.{1}|All files|*.*"), extensionDesc, extension); 
            dlg.CheckFileExists = false;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                return dlg.FileName;
            }
            return null;
        }

        private void NixxisButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

    }

    public class ReportsSettings
    {
        private ReportingService2010.ReportingService2010SoapClient m_ReportingService;
        private ReportExecution2005.ReportExecutionServiceSoapClient m_ReportExecution;
        private ReportExecution2005.Extension[] m_Extensions;
        private string m_User;

        private SortedList<string, ReportSettings> m_ReportsSettings = new SortedList<string, ReportSettings>();

        public ReportsSettings(string reportServerUrl, string reportServerCredentialsUser, string reportServerCredentialsPassword, string reportServerCredentialsDomain, string user)
        {
            m_User = user;
            m_ReportingService = new ReportingService2010.ReportingService2010SoapClient(new System.ServiceModel.BasicHttpBinding() { Security = new System.ServiceModel.BasicHttpSecurity() { Mode = reportServerUrl.StartsWith("https", StringComparison.InvariantCultureIgnoreCase) ? System.ServiceModel.BasicHttpSecurityMode.Transport: System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly, Transport = new System.ServiceModel.HttpTransportSecurity() { ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Ntlm } } }, 
                    new System.ServiceModel.EndpointAddress(string.Concat(reportServerUrl, "/reportservice2010.asmx")));
            m_ReportingService.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(reportServerCredentialsUser, reportServerCredentialsPassword, reportServerCredentialsDomain);
            m_ReportingService.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            m_ReportingService.Endpoint.Behaviors.Add(new WcfLanguageHeaderBehavior());
            m_ReportingService.Open();

            

            m_ReportExecution = new ReportExecution2005.ReportExecutionServiceSoapClient(new System.ServiceModel.BasicHttpBinding() 
            { 
                SendTimeout = new TimeSpan(0,5,0),                
                MaxReceivedMessageSize = 2147483647,
                MaxBufferSize = 2147483647,
                Security = new System.ServiceModel.BasicHttpSecurity() 
                {                     
                    
                    Mode = reportServerUrl.StartsWith("https", StringComparison.InvariantCultureIgnoreCase) ? System.ServiceModel.BasicHttpSecurityMode.Transport: System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly, 
                    Transport = new System.ServiceModel.HttpTransportSecurity() { ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Ntlm} 
                } },
                new System.ServiceModel.EndpointAddress(string.Concat(reportServerUrl, "/reportexecution2005.asmx")));
            m_ReportExecution.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(reportServerCredentialsUser, reportServerCredentialsPassword, reportServerCredentialsDomain);
            m_ReportExecution.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            m_ReportExecution.Endpoint.Behaviors.Add(new WcfLanguageHeaderBehavior());
            m_ReportExecution.Open();

            m_ReportExecution.ListRenderingExtensions(new ReportExecution2005.TrustedUserHeader(), out m_Extensions);

        }

        public IEnumerable<FrameworkElement> Init(string reportPath)
        {
            ReportSettings rs;
            if (m_ReportsSettings.TryGetValue(reportPath, out rs))
            {
                if (rs == null)
                {
                    rs = new ReportSettings(m_ReportingService, m_ReportExecution, reportPath, m_User);
                    m_ReportsSettings[reportPath] = rs;
                }
            }
            else
            {
                rs = new ReportSettings(m_ReportingService,m_ReportExecution, reportPath, m_User);
                m_ReportsSettings.Add(reportPath, rs);
            }

            return rs.GenerateControls();
        }

        public ReportSettings this[string reportPath]
        {
            get
            {
                return m_ReportsSettings[reportPath];
            }
        }

        public ReportExecution2005.Extension[] Extensions
        {
            get
            {
                return m_Extensions;
            }
        }

    }

    public class ReportSettings : INotifyPropertyChanged
    {
        private ReportingService2010.ReportingService2010SoapClient m_ReportingServiceClient;
        private ReportExecution2005.ReportExecutionServiceSoapClient m_ReportExecution;
        private string m_ReportPath;
        private Dictionary<string, ReportParam> m_Params = new Dictionary<string, ReportParam>();
        private TrustedUserHeader m_TrustedUserHeader2010 = new TrustedUserHeader();
        private ReportExecution2005.TrustedUserHeader m_TrustedUserHeader2005 = new ReportExecution2005.TrustedUserHeader();
        private TypeConverter m_Converter = new TypeConverter();
        private string m_User;


        public ReportSettings(ReportingService2010.ReportingService2010SoapClient reportingServiceClient, ReportExecution2005.ReportExecutionServiceSoapClient reportExecution, string reportPath, string user)
        {
            m_User = user;
            m_ReportingServiceClient = reportingServiceClient;
            m_ReportExecution = reportExecution;
            m_ReportPath = reportPath;
            Init();
        }

        public ParameterValue[] CurrentParameterValues2010
        {
            get
            {
                List<ParameterValue> tempList = new List<ParameterValue>();
                foreach (ReportParam rp in m_Params.Values)
                {
                    if (rp.Value != null)
                    {
                        if (rp.IsMultiValue)
                        {
                            foreach (string str in rp.Value.Split(';'))
                            {
                                tempList.Add(new ParameterValue() { Label = rp.Name, Name = rp.Name, Value = str });
                            }
                        }
                        else if (rp.EmptyAllowed || rp.Value != string.Empty)
                        {
                            tempList.Add(new ParameterValue() { Label = rp.Name, Name = rp.Name, Value = rp.Value });
                        }

                    }
                    else if (rp.NullAllowed)
                    {
                        tempList.Add(new ParameterValue() { Label = rp.Name, Name = rp.Name, Value = rp.Value });
                    }

                }

                System.Diagnostics.Trace.WriteLine(string.Join("|", tempList.Select((a) => (string.Format("{0}={1}", a.Name, a.Value)))), "xxxxxxxxxx");
                return tempList.ToArray();
            }
        }

        public ReportExecution2005.ParameterValue[] CurrentParameterValues2005
        {
            get
            {
                List<ReportExecution2005.ParameterValue> tempList = new List<ReportExecution2005.ParameterValue>();
                foreach (ReportParam rp in m_Params.Values)
                {
                    if (rp.Value != null || rp.NullAllowed)
                    {
                        if (rp.IsMultiValue)
                        {
                            foreach(string str in rp.Value.Split(';'))
                                tempList.Add(new ReportExecution2005.ParameterValue() { Label = rp.Name, Name = rp.Name, Value = str });
                        }
                        else
                            tempList.Add(new ReportExecution2005.ParameterValue() { Label = rp.Name, Name = rp.Name, Value = rp.Value });
                    }
                }
                return tempList.ToArray();
            }
        }

        private void RefreshReportParams()
        {
            ReportingService2010.ItemParameter[] itemParameters;

            m_ReportingServiceClient.GetItemParameters(m_TrustedUserHeader2010, m_ReportPath, null, true, CurrentParameterValues2010, null, out itemParameters);

            System.Diagnostics.Trace.WriteLine(string.Join("|", itemParameters.Select((a) => (string.Format("{0}={1}({2})", a.Name,
                a.ValidValues == null ? "null" : string.Join(";", a.ValidValues.Select((b) => (b.Value) )), a.Nullable)
                ))), "yyyyy");

            foreach (ItemParameter ip in itemParameters)
            {
                ReportParam rp = m_Params[ip.Name];
                if (rp != null)
                {
                    if(ip.DefaultValues!=null && ip.DefaultValues.Length>0 && rp.Value!=ip.DefaultValues[0] && !rp.IsMultiValue )
                        rp.Value = ip.DefaultValues[0];

                    if (ip.ValidValues == null)
                    {
                        if (rp.RelatedControl is NixxisComboBox)
                        {

                            NixxisComboBox ncb = ((NixxisComboBox)rp.RelatedControl);

                            ObservableCollection<ValidValue> temp = new ObservableCollection<ValidValue>(rp.Choices);
                            List<string> wasSelected = new List<string>();
                            if (ncb.SelectedItems != null)
                            {
                                foreach (NixxisComboBoxItem ncbi in ncb.SelectedItems)
                                {
                                    wasSelected.Add(ncbi.Id);
                                    ncbi.IsSelected = false;
                                }
                            }
                            ncb.SelectedText = null;
                            rp.Value = null;
                            rp.Choices = new ObservableCollection<ValidValue>();


                            foreach (NixxisComboBoxItem ncbi in ncb.Items)
                            {
                                if (wasSelected.Contains(ncbi.Id))
                                {
                                    if (!ncbi.IsSelected.GetValueOrDefault())
                                        ncbi.IsSelected = true;
                                }
                            }

                        }
                        else
                        {
                            rp.Choices.Clear();
                        }
                    }
                    else
                    {
                        List<ValidValue> toAdd = new List<ValidValue>();
                        List<ValidValue> initial = new List<ValidValue>();
                        foreach (ValidValue vv in rp.Choices)
                            initial.Add(vv);

                        foreach(ValidValue vv in ip.ValidValues)
                        {
                            bool found = false;

                            for (int i = 0; i < initial.Count; i++)
                            {
                                ValidValue vv2 = initial[i];
                                if (vv.Label == vv2.Label && vv.Value == vv2.Value)
                                {
                                    found = true;
                                    initial.Remove(vv2);
                                    break;
                                }
                            }

                            if (!found)
                            {
                                toAdd.Add(vv);
                            }
                        }

                        bool somethingChanged = false;
                        foreach (ValidValue vv in initial)
                        {
                            rp.Choices.Remove(vv);
                            somethingChanged = true;
                        }
                        foreach (ValidValue vv in toAdd)
                        {
                            rp.Choices.Add(vv);
                            somethingChanged = true;
                        }
                        


                        if (rp.RelatedControl is NixxisComboBox && somethingChanged)
                        {
                            NixxisComboBox ncb = ((NixxisComboBox)rp.RelatedControl);

                            ObservableCollection<ValidValue> temp = new ObservableCollection<ValidValue>(rp.Choices);
                            List<string> wasSelected = new List<string>();
                            if (ncb.SelectedItems != null)
                            {
                                foreach (NixxisComboBoxItem ncbi in ncb.SelectedItems)
                                {
                                    wasSelected.Add(ncbi.Id);
                                    ncbi.IsSelected = false;
                                }
                            }
                            ncb.SelectedText = null;
                            rp.Value = null;
                            rp.Choices = temp;


                            foreach (NixxisComboBoxItem ncbi in ncb.Items)
                            {
                                if (wasSelected.Contains(ncbi.Id))
                                {
                                    if (!ncbi.IsSelected.GetValueOrDefault())
                                        ncbi.IsSelected = true;
                                }
                            }
                        }
                        
                    }
                    
                }
            }
        }

        private void Init()
        {
            ReportingService2010.ItemParameter[] itemParameters;

            m_ReportingServiceClient.GetItemParameters(m_TrustedUserHeader2010, m_ReportPath, null, true, null, null, out itemParameters);

            foreach (ItemParameter ip in itemParameters)
            {
                ReportParam rp = new ReportParam();
                rp.Name = ip.Name;
                rp.Prompt = ip.Prompt;
                rp.NullAllowed = ip.Nullable;
                
                rp.EmptyAllowed = ip.AllowBlank;
                rp.IsMultiValue = ip.MultiValue;

                if (ip.MultiValue)
                {
                    rp.m_Control = typeof(NixxisComboBox);
                    if (ip.ValidValues != null)
                    {
                        foreach (ValidValue vv in ip.ValidValues)
                            rp.Choices.Add(vv);
                    }

                }
                else if (ip.ParameterTypeName.Equals("DateTime", StringComparison.InvariantCultureIgnoreCase))
                {
                    rp.m_Control = typeof(DatePicker);
                }
                else if (ip.ParameterTypeName.Equals("Integer", StringComparison.InvariantCultureIgnoreCase) && !ip.ValidValuesQueryBased)
                {
                    rp.m_Control = typeof(NumericUpDown);
                }
                else if (ip.ParameterTypeName.Equals("Boolean", StringComparison.InvariantCultureIgnoreCase))
                {
                    rp.m_Control = typeof(CheckBox);
                }
                else
                {
                    rp.m_Control = typeof(ComboBox);
                    if (ip.ValidValues != null)
                    {
                        foreach (ValidValue vv in ip.ValidValues)
                            rp.Choices.Add(vv);
                    }

                }

                if (ip.DefaultValues != null && ip.DefaultValues.Length > 0)
                    rp.Value = ip.DefaultValues[0];

                rp.PropertyChanged += rp_PropertyChanged;

                m_Params.Add(rp.Name, rp);
            }

            foreach (ItemParameter ip in itemParameters)
            {
                if (ip.Dependencies != null)
                {
                    foreach (string str in ip.Dependencies)
                    {
                        m_Params[ip.Name].Dependencies.Add(m_Params[str]);
                        m_Params[str].Depended.Add(m_Params[ip.Name]);
                    }
                }

            }


            if (m_Params.ContainsKey("user"))
            {
                m_Params["user"].Value =  m_User;

            }
        }

        public static DateTime DateParameterFrom(string parameters)
        {
            int firstDayOfWeek = 1;
            DateTime dte = DateTime.Now;
            string[] list = parameters.Split(new char[] { ',' });

            if (list.Length < 3) return DateTime.Now;

            switch (list[0].ToLower())
            {
                case "d":
                    dte = DateTime.Now.AddDays(Convert.ToDouble(list[1]));
                    break;
                case "w":
                    dte = DateTime.Now.AddDays(Convert.ToDouble(list[1]) * 7);
                    int dayWeek = (int)dte.DayOfWeek;
                    int crossTime = 0;
                    if (dayWeek < firstDayOfWeek) crossTime = 7;
                    dte = dte.AddDays(firstDayOfWeek - dayWeek - crossTime);
                    break;
                case "m":
                    dte = DateTime.Now.AddMonths(Convert.ToInt32(list[1]));
                    dte = new DateTime(dte.Year, dte.Month, 1);
                    break;
                case "y":
                    dte = DateTime.Now.AddYears(Convert.ToInt32(list[1]));
                    dte = new DateTime(dte.Year, 1, 1);
                    break;
                default:
                    dte = DateTime.Now;
                    break;
            }

            return new DateTime(dte.Year, dte.Month, dte.Day, 0, 0, 0);
        }

        public static DateTime DateParameterTo(string parameters)
        {
            int firstDayOfWeek = 1;
            DateTime dte = DateTime.Now;
            string[] list = parameters.Split(new char[] { ',' });

            if (list.Length < 3) return DateTime.Now;

            switch (list[0].ToLower())
            {
                case "d":
                    dte = DateTime.Now.AddDays(Convert.ToDouble(list[2]));
                    break;
                case "w":
                    dte = DateTime.Now.AddDays(Convert.ToDouble(list[2]) * 7);
                    int dayWeek = (int)dte.DayOfWeek;
                    int crossTime = 0;
                    if (dayWeek < firstDayOfWeek) crossTime = 7;
                    dte = dte.AddDays(6 + firstDayOfWeek - dayWeek - crossTime);
                    break;
                case "m":
                    dte = DateTime.Now.AddMonths(Convert.ToInt32(list[2]));
                    dte = new DateTime(dte.Year, dte.Month, 1);
                    dte = dte.AddMonths(1);
                    dte = dte.Subtract(new TimeSpan(1, 0, 0, 0));
                    break;
                case "y":
                    dte = DateTime.Now.AddYears(Convert.ToInt32(list[2]));
                    dte = new DateTime(dte.Year + 1, 1, 1);
                    dte = dte.Subtract(new TimeSpan(1, 0, 0, 0));
                    break;
                default:
                    dte = DateTime.Now;
                    break;
            }

            return new DateTime(dte.Year, dte.Month, dte.Day, 0, 0, 0);
        }


        void rp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ReportParam rp = sender as ReportParam;
            if (e.PropertyName == "Value")
            {
                RefreshReportParams();
                if (rp.Name == "DateSelection")
                {
                    foreach (ReportParam rpz in m_Params.Values)
                    {
                        if (rpz.Name == "fromDate")
                        {
                            rpz.Value = m_Converter.Convert(DateParameterFrom(rp.Value), typeof(string), null, System.Threading.Thread.CurrentThread.CurrentUICulture) as string;
                            rpz.IsEnabledPrerequisite = rp.Value.StartsWith("d");
                        }
                        else if (rpz.Name == "toDate")
                        {
                            rpz.Value = m_Converter.Convert(DateParameterTo(rp.Value), typeof(string), null, System.Threading.Thread.CurrentThread.CurrentUICulture) as string;
                            rpz.IsEnabledPrerequisite = rp.Value.StartsWith("d");
                        }
                    }
                }
                NotifyPropertyChanged("IsEnabled");
            }
        }

        public List<FrameworkElement> GenerateControls()
        {
            List<FrameworkElement> returnValue = new List<FrameworkElement>();
            foreach (ReportParam rp in m_Params.Values)
            {
                if (!string.IsNullOrEmpty(rp.Prompt) && rp.Name!="user")
                {
                    Label lbl = new Label();
                    lbl.SetBinding(Label.IsEnabledProperty, new Binding() { Source = rp, Path = new PropertyPath("IsEnabled") });
                    lbl.Content = rp.Prompt;
                    returnValue.Add(lbl);

                    

                    if (rp.m_Control == typeof(ComboBox))
                    {
                        ComboBox cbo = new ComboBox();
                        cbo.SetBinding(ComboBox.IsEnabledProperty, new Binding() { Source = rp, Path = new PropertyPath("IsEnabled") });
                        cbo.IsEditable = false;
                        cbo.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = rp, Path = new PropertyPath("Choices") });
                        cbo.SetBinding(ComboBox.SelectedValueProperty, new Binding() { Source = rp, Path = new PropertyPath("Value") });
                        cbo.SelectedValuePath = "Value";
                        cbo.DisplayMemberPath = "Label";
                        rp.RelatedControl = cbo;
                        returnValue.Add(cbo);
                    }
                    else if (rp.m_Control == typeof(DatePicker))
                    {
                        DatePicker dp = new DatePicker();
                        dp.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);
                        dp.SetBinding(DatePicker.IsEnabledProperty, new Binding() { Source = rp, Path = new PropertyPath("IsEnabled") });
                        dp.SetBinding(DatePicker.SelectedDateProperty, new Binding() { Source = rp, Path = new PropertyPath("Value"), Converter=m_Converter});
                        rp.RelatedControl = dp;
                        returnValue.Add(dp);
                    }
                    else if (rp.m_Control == typeof(NumericUpDown))
                    {
                        NumericUpDown ud = new NumericUpDown();
                        ud.SetBinding(NumericUpDown.IsEnabledProperty, new Binding() { Source = rp, Path = new PropertyPath("IsEnabled") });
                        ud.NumberFormat = "0";
                        ud.SetBinding(NumericUpDown.ValueProperty, new Binding() { Source = rp, Path = new PropertyPath("Value"), FallbackValue=0.0 });
                        rp.RelatedControl = ud;
                        returnValue.Add(ud);
                    }
                    else if (rp.m_Control == typeof(CheckBox))
                    {
                        CheckBox ud = new CheckBox();
                        ud.SetBinding(CheckBox.IsEnabledProperty, new Binding() { Source = rp, Path = new PropertyPath("IsEnabled") });
                        ud.SetBinding(CheckBox.IsCheckedProperty, new Binding() { Source = rp, Path = new PropertyPath("Value"), FallbackValue = false });
                        rp.RelatedControl = ud;
                        returnValue.Add(ud);
                    }
                    else if (rp.m_Control == typeof(NixxisComboBox))
                    {
                        NixxisComboBox ncb = new NixxisComboBox();
                        ncb.MultiSelection = true;
                        ncb.SetBinding(NixxisComboBox.IsEnabledProperty, new Binding() { Source = rp, Path = new PropertyPath("IsEnabled") });
                        ncb.SetBinding(NixxisComboBox.ItemSingleListProperty, new Binding() { Source = rp, Path = new PropertyPath("Choices")});
                        ncb.SetBinding(NixxisComboBox.SelectedItemsIdProperty, new Binding() {Source = rp, Path = new PropertyPath("Value")});
                        ncb.ItemListDescriptionProperty = "Label";
                        ncb.ItemListIdProperty = "Value";
                        ncb.IsEditable = false;
                        ncb.CollapseImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Collapse.png"));
                        ncb.ExpandImage = new BitmapImage(new Uri("pack://application:,,,/NixxisReportingControls;component/Images/General_Expand.png"));

                        ContextMenu cx = new ContextMenu();
                        MenuItem mi = new MenuItem();
                        mi.Header = ReportingFrameSet.TranslationContext.Translate("Select all");
                        mi.Click += (object sender, RoutedEventArgs args) =>
                        {
                            ncb.StopSelectedItemsIdChangeNotifications();
                            foreach (NixxisComboBoxItem ncbi in ncb.Items)
                                ncbi.IsSelected = true;
                            ncb.DoSelectedItemsIdChangeNotifications();
                        };
                        cx.Items.Add(mi);

                        mi = new MenuItem();
                        mi.Header = ReportingFrameSet.TranslationContext.Translate("Select none");
                        mi.Click += (object sender, RoutedEventArgs args) =>
                        {
                            ncb.StopSelectedItemsIdChangeNotifications();
                            foreach (NixxisComboBoxItem ncbi in ncb.Items)
                                ncbi.IsSelected = false;
                            ncb.DoSelectedItemsIdChangeNotifications();
                        };
                        cx.Items.Add(mi);
                        ncb.ContextMenu = cx;

                        
                        rp.RelatedControl = ncb;
                        returnValue.Add(ncb);
                    }
                }
            }

            return returnValue;

        }


        public byte[] Execute(string format, out string extension)
        {
            byte[] result;
            string mimetype;
            string encoding;
            ReportExecution2005.Warning[] warnings;
            string[] streamIds;
            ReportExecution2005.ExecutionHeader executionHeader;
            ReportExecution2005.ServerInfoHeader serverInfoHeader;
            ReportExecution2005.ExecutionInfo executionInfo;            


            m_ReportExecution.LoadReport(m_TrustedUserHeader2005, m_ReportPath, null, out serverInfoHeader, out executionInfo);

            executionHeader = new ReportExecution2005.ExecutionHeader();
            executionHeader.ExecutionID = executionInfo.ExecutionID;

            m_ReportExecution.SetExecutionParameters(executionHeader, m_TrustedUserHeader2005, CurrentParameterValues2005, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, out executionInfo);

            m_ReportExecution.Render(executionHeader, m_TrustedUserHeader2005, format, null, out result, out extension, out mimetype, out encoding, out warnings, out streamIds);

            
            
            return result;
        }

        public bool IsEnabled
        {
            get
            {
                foreach (ReportParam rp in m_Params.Values)
                {
                    if (!rp.NullAllowed && rp.Value == null)
                        return false;
                    if (!rp.EmptyAllowed && rp.Value == string.Empty)
                        return false;
                }
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class ReportParam: INotifyPropertyChanged
    {
        private string m_Name;
        private string m_Prompt;
        private bool m_Hidden;
        private bool m_NullAllowed;
        private bool m_EmptyAllowed;
        private bool m_IsMultiValue;
        private bool m_EnabledPrerequisite = true;
        private string m_Value;
        private ObservableCollection<ValidValue> m_Choices = new ObservableCollection<ValidValue>();
        private ObservableCollection<ReportParam> m_Dependencies = new ObservableCollection<ReportParam>();
        private ObservableCollection<ReportParam> m_Depended = new ObservableCollection<ReportParam>();

        internal Type m_Control;

        public ReportParam()
        {            
        }

        public string Prompt
        {
            get
            {
                return m_Prompt;
            }
            set
            {
                m_Prompt = value;
                NotifyPropertyChanged("Prompt");
            }
        }
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                NotifyPropertyChanged("Name");
            }
        }
        public string Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                {
                    m_Value = value;
                    NotifyPropertyChanged("Value");
                    foreach (ReportParam rp in Depended)
                    {
                        rp.NotifyPropertyChanged("IsEnabled");
                    }
                }
            }
        }
        public bool NullAllowed
        {
            get
            {
                return m_NullAllowed;
            }
            set
            {
                m_NullAllowed = value;
                NotifyPropertyChanged("NullAllowed");
            }
        }
        public bool IsHidden
        {
            get
            {
                return m_Hidden;
            }
            set
            {
                m_Hidden = value;
                NotifyPropertyChanged("IsHidden");
            }
        }

        public bool EmptyAllowed
        {
            get
            {
                return m_EmptyAllowed;
            }
            set
            {
                m_EmptyAllowed = value;
                NotifyPropertyChanged("EmptyAllowed");
            }
        }
        public bool IsMultiValue
        {
            get
            {
                return m_IsMultiValue;
            }
            set
            {
                m_IsMultiValue = value;
                NotifyPropertyChanged("IsMultiValue");
            }
        }
        public bool IsEnabledPrerequisite
        {
            get
            {
                return m_EnabledPrerequisite;
            }
            set
            {
                m_EnabledPrerequisite = value;
            }
        }
        public bool IsEnabled
        {
            get
            {
                if (!m_EnabledPrerequisite)
                    return false;

                foreach (ReportParam rp in Dependencies)
                {
                    if (rp.Value == null && !rp.NullAllowed)
                        return false;
                }
                return true;
            }
        }
        public FrameworkElement RelatedControl {get;set;}

        public ObservableCollection<ValidValue> Choices
        {
            get
            {
                return m_Choices;
            }
            set
            {
                m_Choices = value;
                NotifyPropertyChanged("Choices");
            }
        }
        public ObservableCollection<ReportParam> Dependencies
        {
            get
            {
                return m_Dependencies;
            }
            set
            {
                m_Dependencies = value;
                NotifyPropertyChanged("Dependencies");
            }
        }
        public ObservableCollection<ReportParam> Depended
        {
            get
            {
                return m_Depended;
            }
            set
            {
                m_Depended = value;
                NotifyPropertyChanged("Depended");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Type t = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (t == typeof(DateTime))
            {
                if (value is string)
                {
                    DateTime dt = DateTime.Parse((string)value, culture.DateTimeFormat);
                    System.Diagnostics.Trace.WriteLine(string.Format("Converting string to DateTime: {0} -> {1}: {2}", value, culture.DateTimeFormat.ShortDatePattern, dt), "*******");
                    return dt;
                }
            }
            else if (t == typeof(string))
            {
                if (value is DateTime)
                {                    
                    string str = ((DateTime)value).ToString(culture.DateTimeFormat);
                    System.Diagnostics.Trace.WriteLine(string.Format("Converting DateTime to string: {0} -> {1}: {2}", value, culture.DateTimeFormat.ShortDatePattern, str), "*******");
                    return str;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (targetType == typeof(string))
                return value.ToString();

            else throw new NotImplementedException();


        }
    }


    public class ExtendedValidValueEnumerable : IEnumerable, INotifyCollectionChanged, IWeakEventListener, IEnumerable<ValidValue>
    {

        public ExtendedValidValueEnumerable(IEnumerable<ValidValue> list)
        {
            InternalList = list;
            if (list is INotifyCollectionChanged)
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)list, this);
        }

        public IEnumerable<ValidValue> InternalList
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            if (InternalList != null)
            {
                yield return new ValidValue() { Label = "(All)", Value = String.Empty};

                IEnumerator InternalEnumerator = InternalList.OrderBy((a) => (a.Label)).GetEnumerator();

                while (InternalEnumerator.MoveNext())
                    yield return InternalEnumerator.Current;
            }
        }

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    // TODO: workaround to avoid exceptions!!!!
                    NotifyCollectionChangedEventArgs args2 = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    CollectionChanged(this, args2);
                }
                else
                {
                    CollectionChanged(this, args);
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            FireCollectionChanged((NotifyCollectionChangedEventArgs)e);
            return true;
        }


        IEnumerator<ValidValue> IEnumerable<ValidValue>.GetEnumerator()
        {
            if (InternalList != null)
            {
                yield return new ValidValue() { Label = "(All)", Value = String.Empty };

                IEnumerator<ValidValue> InternalEnumerator = InternalList.OrderBy((a) => (a.Label)).GetEnumerator();

                while (InternalEnumerator.MoveNext())
                    yield return InternalEnumerator.Current;
            }
        }
    }

    public class NixxisComboboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new ExtendedValidValueEnumerable((IEnumerable<ValidValue>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
