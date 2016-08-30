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
using System.Windows.Controls.Primitives;
using Nixxis.Client.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgStdMusicConfigure : Window, IPreprocessorConfigurationDialog
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgStdMusicConfigure");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private SimpleMusicOnHoldConfig m_Config = null;

        public DlgStdMusicConfigure()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public BasePreprocessorConfig Config
        {
            get
            {
                return m_Config;
            }
            set
            {
                if (value is SimpleMusicOnHoldConfig)
                    m_Config = (SimpleMusicOnHoldConfig)value;
                LoadSettings();
            }
        }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(bool)e.NewValue)
                return;

            TextBlock tb = sender as TextBlock;
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(Translate("Music: {0}\n"), cboWelcome.MessageDisplayText);
                tb.Text = sb.ToString();
            }

        }

        private void WizControl_WizardFinished(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
        }

        private string GetRelativePath(string promptId)
        {
            if (string.IsNullOrEmpty(promptId))
                return string.Empty;

            return ((AdminObject)WizControl.Context).Core.Prompts[promptId].RelativePath;
        }
        
        private void SaveSettings()
        {
            if (Config == null)
            {
                m_Config = ((AdminObject)WizControl.Context).Core.Create<SimpleMusicOnHoldConfig>();
            }

            m_Config.Music = cboWelcome.MessageId;
        }

        private void LoadSettings()
        {
            if (Config == null)
                return;

            cboWelcome.MessageId = m_Config.Music;

        }

        public object WizControlContext
        {
            get
            {
                return WizControl.Context;
            }
            set
            {
                WizControl.Context = value;
            }
        }

    }

}
