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
using System.Data;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Reporting
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class Reports : UserControl
    {
        #region Class data
        private DateTime m_LastDateTime = DateTime.Now.Date.Subtract(new TimeSpan(24, 0, 0));
        private int m_LastCampaign = 0;
        private int m_LastActivity = 0;
        private int m_LastAgent = 0;

        #endregion

        #region Constructor
        public Reports()
        {
            InitializeComponent();
        }
        #endregion


        private void trvReports_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            spParameters.Children.Clear();
            ReportingFrameSet parent = this.DataContext as ReportingFrameSet;

            if (trvReports.SelectedItem != null && parent != null)
            {
                DataRow Row = ((NixxisReportItem)trvReports.SelectedItem).Tag as DataRow;

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
                                    picker.SelectedDate = m_LastDateTime;
                                    picker.SelectedDateFormat = DatePickerFormat.Short;
                                }
                                spParameters.Children.Add(picker);

                                if(addTimePicker)
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


                                    if (Parts[0].StartsWith("@Campaign", StringComparison.OrdinalIgnoreCase) && parent.ParametersCampaigns!= null && (ParamLength >= 32 || ParamLength == -1))
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
                                            cCombo.DisplayMemberPath = "Description";
                                            cCombo.SelectedValuePath = "Id";

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
                                                cCombo.ItemsSource = parent.ParametersActivitiesInbound;
                                            else if (!includeIn && includeOut)
                                                cCombo.ItemsSource = parent.ParametersActivitiesOutbound;
                                            else
                                                cCombo.ItemsSource = parent.ParametersActivities;

                                            cCombo.MultiSelection = true;
                                            cCombo.DisplayMemberPath = "Description";
                                            cCombo.SelectedValuePath = "Id";

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

                                            list.ItemsSource = parent.ParametersCampaigns;
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
                                            cCombo.DisplayMemberPath = "Description";
                                            cCombo.SelectedValuePath = "Id";

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
}
