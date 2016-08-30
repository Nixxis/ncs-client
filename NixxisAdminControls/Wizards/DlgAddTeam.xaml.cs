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

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgAddTeam : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddTeam");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddTeam()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public Team Created { get; internal set; }

        public string GroupKey { get; set; }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Description: {0}\n"), txtDescription.Text);
                sb.AppendFormat(Translate("Team will contain {0} agents.\n"), listAgents.NixxisSelectedItems.Count);
                sb.AppendFormat(Translate("Team will have crosspoints with {0} queues.\n"), listQueues.NixxisSelectedItems.Count);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            Team team = core.Create<Team>();
            team.GroupKey = GroupKey;
            team.Description = txtDescription.Text;
            team.Agents.AddRange(listAgents.NixxisSelectedItems);
            team.Queues.AddRange(listQueues.NixxisSelectedItems);
            core.Teams.Add(team);
            Created = team;
            DialogResult = true;
        }
    }
}
