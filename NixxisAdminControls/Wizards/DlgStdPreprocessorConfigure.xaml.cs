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
using ContactRoute;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// </summary>
    public partial class DlgStdPreprocessorConfigure : Window, IPreprocessorConfigurationDialog
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("DlgStdPreprocessorConfigure");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private SimplePreprocessorConfig m_Config = null;

        public DlgStdPreprocessorConfigure()
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
                if(value is SimplePreprocessorConfig)
                    m_Config = (SimplePreprocessorConfig)value;
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
                sb.AppendFormat(Translate("Welcome: {0}\n"), cboWelcome.MessageDisplayText);
                if (chkLanguage.IsChecked.GetValueOrDefault())
                {
                                PreprocessorMenu[] languages = new PreprocessorMenu[] 
            {
                (PreprocessorMenu)Resources["lan1"],
                (PreprocessorMenu)Resources["lan2"],
                (PreprocessorMenu)Resources["lan3"],
                (PreprocessorMenu)Resources["lan4"],
                (PreprocessorMenu)Resources["lan5"],
                (PreprocessorMenu)Resources["lan6"],
                (PreprocessorMenu)Resources["lan7"],
                (PreprocessorMenu)Resources["lan8"],
                (PreprocessorMenu)Resources["lan9"],
                (PreprocessorMenu)Resources["lanStar"],
                (PreprocessorMenu)Resources["lan0"],
                (PreprocessorMenu)Resources["lanHash"]
            };

                    sb.AppendFormat(Translate("Prompt for language choices:\n"));
                    if (tb1.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 1 -> {0}\n", languages[0].SkillDescription );
                    if (tb2.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 2 -> {0}\n", languages[1].SkillDescription);
                    if (tb3.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 3 -> {0}\n", languages[2].SkillDescription);
                    if (tb4.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 4 -> {0}\n", languages[3].SkillDescription);
                    if (tb5.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 5 -> {0}\n", languages[4].SkillDescription);
                    if (tb6.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 6 -> {0}\n", languages[5].SkillDescription);
                    if (tb7.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 7 -> {0}\n", languages[6].SkillDescription);
                    if (tb8.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 8 -> {0}\n", languages[7].SkillDescription);
                    if (tb9.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 9 -> {0}\n", languages[8].SkillDescription);
                    if (tbStar.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF * -> {0}\n", languages[9].SkillDescription);
                    if (tb0.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 0 -> {0}\n", languages[10].SkillDescription);
                    if (tbHash.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF # -> {0}\n", languages[11].SkillDescription);

                }
                if (chkMenu.IsChecked.GetValueOrDefault())
                {
                    PreprocessorMenu[] menus = new PreprocessorMenu[] 
                {
                    (PreprocessorMenu)Resources["menu1"],
                    (PreprocessorMenu)Resources["menu2"],
                    (PreprocessorMenu)Resources["menu3"],
                    (PreprocessorMenu)Resources["menu4"],
                    (PreprocessorMenu)Resources["menu5"],
                    (PreprocessorMenu)Resources["menu6"],
                    (PreprocessorMenu)Resources["menu7"],
                    (PreprocessorMenu)Resources["menu8"],
                    (PreprocessorMenu)Resources["menu9"],
                    (PreprocessorMenu)Resources["menuStar"],
                    (PreprocessorMenu)Resources["menu0"],
                    (PreprocessorMenu)Resources["menuHash"]
                };
                    sb.AppendFormat(Translate("Prompt for menu choices:\n"));
                    if (tbm1.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 1 -> {0}\n", menus[0].QueueSkillDescription);
                    if (tbm2.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 2 -> {0}\n", menus[1].QueueSkillDescription);
                    if (tbm3.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 3 -> {0}\n", menus[2].QueueSkillDescription);
                    if (tbm4.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 4 -> {0}\n", menus[3].QueueSkillDescription);
                    if (tbm5.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 5 -> {0}\n", menus[4].QueueSkillDescription);
                    if (tbm6.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 6 -> {0}\n", menus[5].QueueSkillDescription);
                    if (tbm7.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 7 -> {0}\n", menus[6].QueueSkillDescription);
                    if (tbm8.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 8 -> {0}\n", menus[7].QueueSkillDescription);
                    if (tbm9.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 9 -> {0}\n", menus[8].QueueSkillDescription);
                    if (tbmStar.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF * -> {0}\n", menus[9].QueueSkillDescription);
                    if (tbm0.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF 0 -> {0}\n", menus[10].QueueSkillDescription);
                    if (tbmHash.IsChecked.GetValueOrDefault()) sb.AppendFormat("\tDTMF # -> {0}\n", menus[11].QueueSkillDescription);
                }
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
                m_Config = ((AdminObject)WizControl.Context).Core.Create<SimplePreprocessorConfig>();
            }
            char[] DtmfFromPos = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '*', '0', '#' };
            ToggleButton[] lanDTMFS = new ToggleButton[] { tb1, tb2, tb3, tb4, tb5, tb6, tb7, tb8, tb9, tbStar, tb0, tbHash };
            PreprocessorMenu[] languages = new PreprocessorMenu[] 
            {
                (PreprocessorMenu)Resources["lan1"],
                (PreprocessorMenu)Resources["lan2"],
                (PreprocessorMenu)Resources["lan3"],
                (PreprocessorMenu)Resources["lan4"],
                (PreprocessorMenu)Resources["lan5"],
                (PreprocessorMenu)Resources["lan6"],
                (PreprocessorMenu)Resources["lan7"],
                (PreprocessorMenu)Resources["lan8"],
                (PreprocessorMenu)Resources["lan9"],
                (PreprocessorMenu)Resources["lanStar"],
                (PreprocessorMenu)Resources["lan0"],
                (PreprocessorMenu)Resources["lanHash"]
            };

            ToggleButton[] menuDTMFs = new ToggleButton[] { tbm1, tbm2, tbm3, tbm4, tbm5, tbm6, tbm7, tbm8, tbm9, tbmStar, tbm0, tbmHash };
            PreprocessorMenu[] menus = new PreprocessorMenu[] 
                {
                    (PreprocessorMenu)Resources["menu1"],
                    (PreprocessorMenu)Resources["menu2"],
                    (PreprocessorMenu)Resources["menu3"],
                    (PreprocessorMenu)Resources["menu4"],
                    (PreprocessorMenu)Resources["menu5"],
                    (PreprocessorMenu)Resources["menu6"],
                    (PreprocessorMenu)Resources["menu7"],
                    (PreprocessorMenu)Resources["menu8"],
                    (PreprocessorMenu)Resources["menu9"],
                    (PreprocessorMenu)Resources["menuStar"],
                    (PreprocessorMenu)Resources["menu0"],
                    (PreprocessorMenu)Resources["menuHash"]
                };
            m_Config.Welcome.TargetId = cboWelcome.MessageId;
            m_Config.AskMenu = chkMenu.IsChecked.GetValueOrDefault();
            m_Config.AskLanguage = chkLanguage.IsChecked.GetValueOrDefault();

            List<SimplePreprocessorConfig.StdPreprocessorLanguagePrompt> temp = new List<SimplePreprocessorConfig.StdPreprocessorLanguagePrompt>();
            for(int i=0; i< 12; i++)
            {
                ToggleButton tb = lanDTMFS[i];
                if (tb.IsChecked.GetValueOrDefault())
                {
                    SimplePreprocessorConfig.StdPreprocessorLanguagePrompt lan = m_Config.LanguagesPrompts.FirstOrDefault((a) => (a.Dtmf == DtmfFromPos[i]));
                    if (lan == null)
                    {
                        lan = m_Config.Core.Create<SimplePreprocessorConfig.StdPreprocessorLanguagePrompt>(m_Config);
                        lan.Dtmf = DtmfFromPos[i];
                        m_Config.LanguagesPrompts.Add(lan);
                    }

                    lan.Prompt.TargetId = languages[i].PromptId;
                    lan.MinValue = (int)languages[i].Value.Start;
                    lan.MaxValue = (int)languages[i].Value.End;
                    lan.Language.TargetId = languages[i].SkillId;

                    temp.Add(lan);
                }
            }

            for (int i = 0; i < m_Config.LanguagesPrompts.Count; i++)
            {
                if (!temp.Contains(m_Config.LanguagesPrompts[i]))
                {
                    m_Config.LanguagesPrompts.RemoveAt(i);
                    i--;
                }
            }


            List<SimplePreprocessorConfig.StdPreprocessorMenuPrompt> temp2 = new List<SimplePreprocessorConfig.StdPreprocessorMenuPrompt>();
            for (int i = 0; i < 12; i++)
            {
                ToggleButton tb = menuDTMFs[i];
                if (tb.IsChecked.GetValueOrDefault())
                {
                    SimplePreprocessorConfig.StdPreprocessorMenuPrompt menu = m_Config.MenuPrompts.FirstOrDefault((a) => (a.Dtmf == DtmfFromPos[i]));
                    if (menu == null)
                    {
                        menu = m_Config.Core.Create<SimplePreprocessorConfig.StdPreprocessorMenuPrompt>(m_Config);
                        menu.Dtmf = DtmfFromPos[i];
                        m_Config.MenuPrompts.Add(menu);
                    }

                    menu.Prompt.TargetId = menus[i].PromptId;
                    menu.MinValue = (int)menus[i].Value.Start;
                    menu.MaxValue = (int)menus[i].Value.End;
                    menu.Skill.TargetId = menus[i].SkillId;
                    menu.Queue.TargetId = menus[i].QueueId;

                    List<SimplePreprocessorConfig.StdPreprocessorLanguagePrompt> temp3 = new List<SimplePreprocessorConfig.StdPreprocessorLanguagePrompt>();

                    foreach (SimplePreprocessorConfig.StdPreprocessorLanguagePrompt plp in m_Config.LanguagesPrompts)
                    {
                        SimplePreprocessorConfig.StdPreprocessorLanguagePrompt lang = menu.LanguagesPrompts.FirstOrDefault((a) => (a.Language.TargetId == plp.Language.TargetId));
                        if (lang == null)
                        {
                            lang = m_Config.Core.Create<SimplePreprocessorConfig.StdPreprocessorLanguagePrompt>(m_Config);
                            lang.Language.TargetId = plp.Language.TargetId;
                            menu.LanguagesPrompts.Add(lang);
                        }
                        switch (plp.Dtmf)
                        {
                            case '0':
                                lang.Prompt.TargetId = menus[i].PromptId0;
                                break;
                            case '1':
                                lang.Prompt.TargetId = menus[i].PromptId1;
                                break;
                            case '2':
                                lang.Prompt.TargetId = menus[i].PromptId2;
                                break;
                            case '3':
                                lang.Prompt.TargetId = menus[i].PromptId3;
                                break;
                            case '4':
                                lang.Prompt.TargetId = menus[i].PromptId4;
                                break;
                            case '5':
                                lang.Prompt.TargetId = menus[i].PromptId5;
                                break;
                            case '6':
                                lang.Prompt.TargetId = menus[i].PromptId6;
                                break;
                            case '7':
                                lang.Prompt.TargetId = menus[i].PromptId7;
                                break;
                            case '8':
                                lang.Prompt.TargetId = menus[i].PromptId8;
                                break;
                            case '9':
                                lang.Prompt.TargetId = menus[i].PromptId9;
                                break;
                            case '*':
                                lang.Prompt.TargetId = menus[i].PromptIdStar;
                                break;
                            case '#':
                                lang.Prompt.TargetId = menus[i].PromptIdHash;
                                break;
                        }
                        temp3.Add(lang);
                    }

                    for (int j = 0; j < menu.LanguagesPrompts.Count; j++)
                    {
                        if (!temp3.Contains(menu.LanguagesPrompts[j]))
                        {
                            menu.LanguagesPrompts.RemoveAt(j);
                            j--;
                        }
                    }

                    temp2.Add(menu);
                }
            }

            for (int i = 0; i < m_Config.MenuPrompts.Count; i++)
            {
                if (!temp2.Contains(m_Config.MenuPrompts[i]))
                {
                    m_Config.MenuPrompts.RemoveAt(i);
                    i--;
                }
            }
        }

        private void LoadSettings()
        {
            if (Config == null)
                return;
            ToggleButton[] lanDTMFS = new ToggleButton[] { tb1, tb2, tb3, tb4, tb5, tb6, tb7, tb8, tb9, tbStar, tb0, tbHash };
            PreprocessorMenu[] languages = new PreprocessorMenu[] 
            {
                (PreprocessorMenu)Resources["lan1"],
                (PreprocessorMenu)Resources["lan2"],
                (PreprocessorMenu)Resources["lan3"],
                (PreprocessorMenu)Resources["lan4"],
                (PreprocessorMenu)Resources["lan5"],
                (PreprocessorMenu)Resources["lan6"],
                (PreprocessorMenu)Resources["lan7"],
                (PreprocessorMenu)Resources["lan8"],
                (PreprocessorMenu)Resources["lan9"],
                (PreprocessorMenu)Resources["lanStar"],
                (PreprocessorMenu)Resources["lan0"],
                (PreprocessorMenu)Resources["lanHash"]
            };

            ToggleButton[] menuDTMFs = new ToggleButton[] { tbm1, tbm2, tbm3, tbm4, tbm5, tbm6, tbm7, tbm8, tbm9, tbmStar, tbm0, tbmHash };
            PreprocessorMenu[] menus = new PreprocessorMenu[] 
                {
                    (PreprocessorMenu)Resources["menu1"],
                    (PreprocessorMenu)Resources["menu2"],
                    (PreprocessorMenu)Resources["menu3"],
                    (PreprocessorMenu)Resources["menu4"],
                    (PreprocessorMenu)Resources["menu5"],
                    (PreprocessorMenu)Resources["menu6"],
                    (PreprocessorMenu)Resources["menu7"],
                    (PreprocessorMenu)Resources["menu8"],
                    (PreprocessorMenu)Resources["menu9"],
                    (PreprocessorMenu)Resources["menuStar"],
                    (PreprocessorMenu)Resources["menu0"],
                    (PreprocessorMenu)Resources["menuHash"]
                };

            cboWelcome.MessageId = m_Config.Welcome.TargetId;
            chkLanguage.IsChecked = m_Config.AskLanguage;
            chkMenu.IsChecked = m_Config.AskMenu;

            foreach (SimplePreprocessorConfig.StdPreprocessorLanguagePrompt lp in m_Config.LanguagesPrompts)
            {

                switch (lp.Dtmf)
                {
                    case '0':
                        tb0.IsChecked = true;
                        break;
                    case '1':
                        tb1.IsChecked = true;
                        break;
                    case '2':
                        tb2.IsChecked = true;
                        break;
                    case '3':
                        tb3.IsChecked = true;
                        break;
                    case '4':
                        tb4.IsChecked = true;
                        break;
                    case '5':
                        tb5.IsChecked = true;
                        break;
                    case '6':
                        tb6.IsChecked = true;
                        break;
                    case '7':
                        tb7.IsChecked = true;
                        break;
                    case '8':
                        tb8.IsChecked = true;
                        break;
                    case '9':
                        tb9.IsChecked = true;
                        break;
                    case '*':
                        tbStar.IsChecked = true;
                        break;
                    case '#':
                        tbHash.IsChecked = true;
                        break;
                }

                for (int i = 0; i < 12; i++)
                {
                    if (languages[i].Dtmf[0].Equals(lp.Dtmf))
                    {
                        languages[i].SkillId = lp.Language.TargetId;
                        languages[i].Value.End =lp.MaxValue;
                        languages[i].Value.Start = lp.MinValue;
                        languages[i].PromptId = lp.Prompt.TargetId;
                        break;
                    }
                }
            }

            foreach (SimplePreprocessorConfig.StdPreprocessorMenuPrompt mp in m_Config.MenuPrompts)
            {
                switch (mp.Dtmf)
                {
                    case '0':
                        tbm0.IsChecked = true;
                        break;
                    case '1':
                        tbm1.IsChecked = true;
                        break;
                    case '2':
                        tbm2.IsChecked = true;
                        break;
                    case '3':
                        tbm3.IsChecked = true;
                        break;
                    case '4':
                        tbm4.IsChecked = true;
                        break;
                    case '5':
                        tbm5.IsChecked = true;
                        break;
                    case '6':
                        tbm6.IsChecked = true;
                        break;
                    case '7':
                        tbm7.IsChecked = true;
                        break;
                    case '8':
                        tbm8.IsChecked = true;
                        break;
                    case '9':
                        tbm9.IsChecked = true;
                        break;
                    case '*':
                        tbmStar.IsChecked = true;
                        break;
                    case '#':
                        tbmHash.IsChecked = true;
                        break;
                }
                for(int i=0; i< 12; i++)
                {
                    if (menus[i].Dtmf[0].Equals(mp.Dtmf))
                    {
                        menus[i].SkillId = mp.Skill.TargetId;
                        menus[i].QueueId = mp.Queue.TargetId;
                        menus[i].Value.End = mp.MaxValue;
                        menus[i].Value.Start = mp.MinValue;
                        menus[i].PromptId = mp.Prompt.TargetId;
                        foreach (SimplePreprocessorConfig.StdPreprocessorLanguagePrompt lp in mp.LanguagesPrompts)
                        {
                            // get the dtmf associated to that language...
                            char landtmf = m_Config.LanguagesPrompts.First((a) => (a.Language.TargetId == lp.Language.TargetId)).Dtmf;

                            switch (landtmf)
                            {
                                case '0':
                                    menus[i].PromptId0 = lp.Prompt.TargetId;
                                    break;
                                case '1':
                                    menus[i].PromptId1 = lp.Prompt.TargetId;
                                    break;
                                case '2':
                                    menus[i].PromptId2 = lp.Prompt.TargetId;
                                    break;
                                case '3':
                                    menus[i].PromptId3 = lp.Prompt.TargetId;
                                    break;
                                case '4':
                                    menus[i].PromptId4 = lp.Prompt.TargetId;
                                    break;
                                case '5':
                                    menus[i].PromptId5 = lp.Prompt.TargetId;
                                    break;
                                case '6':
                                    menus[i].PromptId6 = lp.Prompt.TargetId;
                                    break;
                                case '7':
                                    menus[i].PromptId7 = lp.Prompt.TargetId;
                                    break;
                                case '8':
                                    menus[i].PromptId8 = lp.Prompt.TargetId;
                                    break;
                                case '9':
                                    menus[i].PromptId9 = lp.Prompt.TargetId;
                                    break;
                                case '*':
                                    menus[i].PromptIdStar = lp.Prompt.TargetId;
                                    break;
                                case '#':
                                    menus[i].PromptIdHash = lp.Prompt.TargetId;
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem != null && WizControl.CurrentStepParam!=null)
                (WizControl.CurrentStepParam as PreprocessorMenu).SkillDescription = ((sender as ComboBox).SelectedItem as AdminObject).ShortDisplayText;
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

        private void ComboBox_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem != null && WizControl.CurrentStepParam != null)
                (WizControl.CurrentStepParam as PreprocessorMenu).QueueDescription = ((sender as ComboBox).SelectedItem as AdminObject).ShortDisplayText;

        }



    }

    public class PreprocessorMenu : INotifyPropertyChanged
    {
        private string m_Dtmf;
        private string m_PromptId;
        private string m_PromptId1;
        private string m_PromptId2;
        private string m_PromptId3;
        private string m_PromptId4;
        private string m_PromptId5;
        private string m_PromptId6;
        private string m_PromptId7;
        private string m_PromptId8;
        private string m_PromptId9;
        private string m_PromptIdStar;
        private string m_PromptId0;
        private string m_PromptIdHash;


        private string m_SkillDescription;
        private string m_QueueDescription;

        private string m_SkillId;
        private string m_QueueId;
        private DoubleRange m_Value = new DoubleRange();

        public string Dtmf
        {
            get
            {
                return m_Dtmf;
            }
            set
            {
                m_Dtmf = value;
                FirePropertyChanged("Dtmf");
            }
        }
        public string PromptId
        {
            get
            {
                return m_PromptId;
            }
            set
            {
                m_PromptId = value;
                FirePropertyChanged("PromptId");
            }
        }
        public string PromptId1
        {
            get
            {
                return m_PromptId1;
            }
            set
            {
                m_PromptId1 = value;
                FirePropertyChanged("PromptId1");
            }
        }
        public string PromptId2
        {
            get
            {
                return m_PromptId2;
            }
            set
            {
                m_PromptId2 = value;
                FirePropertyChanged("PromptId2");
            }
        }
        public string PromptId3
        {
            get
            {
                return m_PromptId3;
            }
            set
            {
                m_PromptId3 = value;
                FirePropertyChanged("PromptId3");
            }
        }
        public string PromptId4
        {
            get
            {
                return m_PromptId4;
            }
            set
            {
                m_PromptId4 = value;
                FirePropertyChanged("PromptId4");
            }
        }
        public string PromptId5
        {
            get
            {
                return m_PromptId5;
            }
            set
            {
                m_PromptId5 = value;
                FirePropertyChanged("PromptId5");
            }
        }
        public string PromptId6
        {
            get
            {
                return m_PromptId6;
            }
            set
            {
                m_PromptId6 = value;
                FirePropertyChanged("PromptId6");
            }
        }
        public string PromptId7
        {
            get
            {
                return m_PromptId7;
            }
            set
            {
                m_PromptId7 = value;
                FirePropertyChanged("PromptId7");
            }
        }
        public string PromptId8
        {
            get
            {
                return m_PromptId8;
            }
            set
            {
                m_PromptId8 = value;
                FirePropertyChanged("PromptId8");
            }
        }
        public string PromptId9
        {
            get
            {
                return m_PromptId9;
            }
            set
            {
                m_PromptId9 = value;
                FirePropertyChanged("PromptId9");
            }
        }
        public string PromptIdStar
        {
            get
            {
                return m_PromptIdStar;
            }
            set
            {
                m_PromptIdStar = value;
                FirePropertyChanged("PromptIdStar");
            }
        }
        public string PromptId0
        {
            get
            {
                return m_PromptId0;
            }
            set
            {
                m_PromptId0 = value;
                FirePropertyChanged("PromptId0");
            }
        }
        public string PromptIdHash
        {
            get
            {
                return m_PromptIdHash;
            }
            set
            {
                m_PromptIdHash = value;
                FirePropertyChanged("PromptIdHash");
            }
        }
        public string SkillId
        {
            get
            {
                return m_SkillId;
            }
            set
            {
                m_SkillId = value;
                FirePropertyChanged("SkillId");
            }
        }
        public string QueueId
        {
            get
            {
                return m_QueueId;
            }
            set
            {
                m_QueueId = value;
                FirePropertyChanged("QueueId");
            }
        }
        public string SkillDescription
        {
            get
            {
                return m_SkillDescription;
            }
            set
            {
                m_SkillDescription = value;
                FirePropertyChanged("SkillDescription");
            }
        }
        public string QueueDescription
        {
            get
            {
                return m_QueueDescription;
            }
            set
            {
                m_QueueDescription = value;
                FirePropertyChanged("QueueDescription");
            }
        }
        public string QueueSkillDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(QueueDescription) && !string.IsNullOrEmpty(SkillDescription))
                    return string.Concat(QueueDescription, ", ", SkillDescription);
                else if (!string.IsNullOrEmpty(QueueDescription))
                    return QueueDescription;
                else if (!string.IsNullOrEmpty(SkillDescription))
                    return SkillDescription;
                else
                    return string.Empty;
            }
        }
        public DoubleRange Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
                FirePropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }


}
