﻿using System;
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
    public partial class DlgAddSkill : Window
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgAddSkill");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public DlgAddSkill()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;
 
            InitializeComponent();
        }

        public Skill Created { get; internal set; }

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
                sb.AppendFormat(Translate("Skill will be known by {0} agents.\n"), listAgents.NixxisSelectedItems.Count);
                sb.AppendFormat(Translate("Skill will be requested by {0} activity contexts.\n"), listActivities.NixxisSelectedItems.Count);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            AdminCore core = DataContext as AdminCore;

            Skill skl = core.Create<Skill>();
            skl.GroupKey = GroupKey;
            skl.Description = txtDescription.Text;
            skl.Agents.AddRange(listAgents.NixxisSelectedItems);
            skl.Activities.AddRange(listActivities.NixxisSelectedItems);
            core.Skills.Add(skl);
            Created = skl;
            DialogResult = true;

        }
    }
}
