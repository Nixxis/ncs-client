using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace Nixxis.Client.Supervisor
{

    public class ColumnItemDescription
    {
        #region Enums
        public enum ColumnTypes
        {
            DataGridTextColumn,
            NixxisDataGridToggleDetailColumn,
            DataGridTemplateColumn,
        }
        public enum Categories
        {
            Default,
            Parameter,
            RealTime,
            History,
            Production,
            PeriodProduction,
            System,
        }
        #endregion

        #region Class Data
        private ColumnTypes m_Columntype = ColumnTypes.DataGridTextColumn;
        private IValueConverter m_Converter = null;
        private Visibility m_Visible = Visibility.Visible;
        private Categories m_Category = Categories.Default;
        private bool m_VisibleForUser = true;
        private string m_ControlTemplateKey;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the column typically use as the name of the header in the datagrid
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// The description of the column. Extra information about this column, can be used as tooltip
        /// </summary>
        public string ColumnDescription { get; set; }
        /// <summary>
        /// Is the column default visible
        /// </summary>
        public Visibility Visible
        {
            get { return m_Visible; }
            set { m_Visible = value; }
        }
        /// <summary>
        /// Can a use select this field to make it visible
        /// </summary>
        public bool VisibleForUser
        {
            get { return m_VisibleForUser; }
            set { m_VisibleForUser = value; }
        }
        /// <summary>
        /// What is the binding value for this column.
        /// </summary>
        public string BindingValue { get; set; }
        /// <summary>
        /// >What is the category of this column
        /// </summary>
        public Categories Category
        {
            get { return m_Category; }
            set { m_Category = value; }
        }
        /// <summary>
        /// The converter to use for displaying this value
        /// </summary>
        public IValueConverter Convertor
        {
            get { return m_Converter; }
            set { m_Converter = value; }
        }
        /// <summary>
        /// What is the type of this column. This is the datagrid column type
        /// </summary>
        public ColumnTypes ColumnType
        {
            get { return m_Columntype; }
            set { m_Columntype = value; }
        }
        /// <summary>
        /// If this templatekey is filled it will be used to dispaly the data. For some columntypes it is mandatory (NixxisDataGridToggleDetailColumn and DataGridTemplateColumn)
        /// </summary>
        public string ControlTemplateKey
        {
            get { return m_ControlTemplateKey; }
            set { m_ControlTemplateKey = value; }
        }
        #endregion

        #region Members
        /// <summary>
        /// What is the text to display in the column selector
        /// </summary>
        /// <returns>Text to display</returns>
        public string ColumnSelectorText()
        {
            return this.Category + " " + this.ColumnName;
        }
        /// <summary>
        /// The override of the ToString value
        /// </summary>
        /// <returns>Returns the name of the column</returns>
        public override string ToString()
        {
            return this.ColumnName;
        }
        #endregion
    }

    public static class SupervisionColumns
    {
        private static List<ColumnItemDescription> m_Inbound = new List<ColumnItemDescription>();
        public static List<ColumnItemDescription> Inbound { get { return m_Inbound; } }

        static SupervisionColumns()
        {
            InitInbound();
        }
        private static void InitInbound()
        {
            m_Inbound.Clear();

            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.System,
                ColumnName = "",               
                ColumnDescription = "",
                ColumnType = ColumnItemDescription.ColumnTypes.NixxisDataGridToggleDetailColumn,
                ControlTemplateKey = "NixxisInboundToggleButtonTemplate"
            });

            #region Parameter
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "Id",
                BindingValue = "Id",
                Visible = Visibility.Hidden,
                ColumnDescription = "The unique identifier of the inbound activity."
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "Description",
                BindingValue = "Description",
                ColumnDescription = "Description of the activity."
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "GroupKey",
                BindingValue = "GroupKey",
                Visible = Visibility.Hidden,
                VisibleForUser = false,
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "CampaignId",
                BindingValue = "CampaignId",
                Visible = Visibility.Hidden,
                ColumnDescription = "The campaign identifier."
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "CampaignName",
                BindingValue = "CampaignName",
                ColumnDescription = "The name of the campaign."
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "MediaType",
                BindingValue = "MediaType",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.Parameter,
                ColumnName = "MediaTypeId",
                BindingValue = "MediaTypeId",
                Visible = Visibility.Hidden,
                ColumnDescription = ""
            });
            #endregion

            #region RealTime
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "ActiveContacts",
                BindingValue = "RealTime.ActiveContacts",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "Closing",
                BindingValue = "RealTime.Closing",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "SystemPreprocessing",
                BindingValue = "RealTime.SystemPreprocessing",
                Visible = Visibility.Hidden,
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "Ivr",
                BindingValue = "RealTime.Ivr",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "Waiting",
                BindingValue = "RealTime.Waiting",

                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "Online",
                BindingValue = "RealTime.Online",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "WrapUp",
                BindingValue = "RealTime.WrapUp",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "Overflowing",
                BindingValue = "RealTime.Overflowing",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "Transfer",
                BindingValue = "RealTime.Transfer",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "MaxQueueTime",
                BindingValue = "RealTime.MaxQueueTime",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "ContactMsgSend",
                BindingValue = "RealTime.ContactMsgSend",
                Visible = Visibility.Hidden,
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                ColumnName = "ContactMsgReceived",
                BindingValue = "RealTime.ContactMsgReceived",
                Visible = Visibility.Hidden,
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "AgentInReady",
                BindingValue = "RealTime.AgentInReady",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "AlertLevel",
                BindingValue = "RealTime.AlertLevel",
                ColumnDescription = ""
            });



            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom1",
                BindingValue = "RealTime.NumericCustom1",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom2",
                BindingValue = "RealTime.NumericCustom2",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom3",
                BindingValue = "RealTime.NumericCustom3",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom4",
                BindingValue = "RealTime.NumericCustom4",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom5",
                BindingValue = "RealTime.NumericCustom5",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom6",
                BindingValue = "RealTime.NumericCustom6",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom7",
                BindingValue = "RealTime.NumericCustom7",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom8",
                BindingValue = "RealTime.NumericCustom8",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom9",
                BindingValue = "RealTime.NumericCustom9",
                ColumnDescription = ""
            });
            m_Inbound.Add(new ColumnItemDescription()
            {
                Category = ColumnItemDescription.Categories.RealTime,
                ColumnName = "NumericCustom10",
                BindingValue = "RealTime.NumericCustom10",
                ColumnDescription = ""
            });

            #endregion
            //<DataGridTextColumn Header="AgentId" Binding="{Binding AgentId}" />


        }

    }
}
