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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Nixxis.Client.Controls
{

    public class NixxisScoreList : ListBox
    {
        #region Class data
        private NixxisScoreArray m_List = new NixxisScoreArray();
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(int), typeof(NixxisScoreList), new PropertyMetadata(5, LengthPropertyChanged));
        public int Length
        {
            get { return (int)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }
        public static void LengthPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreList item = (NixxisScoreList)obj;

            item.UpdateLength();
        }

        public static readonly DependencyProperty ActiveImageSourceProperty = DependencyProperty.Register("ActiveImageSource", typeof(ImageSource), typeof(NixxisScoreList), new PropertyMetadata(ActiveImageSourcePropertyChanged));
        public ImageSource ActiveImageSource
        {
            get { return (ImageSource)GetValue(ActiveImageSourceProperty); }
            set { SetValue(ActiveImageSourceProperty, value); }
        }
        public static void ActiveImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreList item = (NixxisScoreList)obj;

            item.UpdateActiveImageSource();
        }

        public static readonly DependencyProperty InactiveImageSourceProperty = DependencyProperty.Register("InactiveImageSource", typeof(ImageSource), typeof(NixxisScoreList), new PropertyMetadata(InactiveImageSourcePropertyChanged));
        public ImageSource InactiveImageSource
        {
            get { return (ImageSource)GetValue(InactiveImageSourceProperty); }
            set { SetValue(InactiveImageSourceProperty, value); }
        }
        public static void InactiveImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreList item = (NixxisScoreList)obj;

            item.UpdateInactiveImageSource();
        }

        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register("Labels", typeof(string[]), typeof(NixxisScoreList), new PropertyMetadata(LabelsPropertyChanged));
        public string[] Labels
        {
            get { return (string[])GetValue(LabelsProperty); }
            set { SetValue(LabelsProperty, value); }
        }
        public static void LabelsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreList item = (NixxisScoreList)obj;

            item.SetLabels();
        }

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(string), typeof(NixxisScoreList), new PropertyMetadata(ScorePropertyChanged));
        public string Score
        {
            get { return (string)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }
        public static void ScorePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreList item = (NixxisScoreList)obj;

            item.SetScore();
        }

        #endregion

        #region Constructors
        static NixxisScoreList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisScoreList), new FrameworkPropertyMetadata(typeof(NixxisScoreList)));
        }
        #endregion

        #region Members override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.ItemsSource = m_List;
        }
        #endregion

        #region Members
        private bool m_SettingScore = false;

        private void SetScore()
        {
            m_SettingScore = true;
            
            int currentI = 0;

            if (Score != null)
            {
                for (int i = 0; i < Score.Length; i++)
                {
                    if (i >= m_List.Count)
                    {
                        m_List.Add(new NixxisScoreArrayElement() { Label = "Score " + i.ToString(), Length = this.Length, Value = this.Score[i], ActiveImageSource = this.ActiveImageSource, InactiveImageSource = this.InactiveImageSource });
                        m_List[i].PropertyChanged += new PropertyChangedEventHandler(NixxisScoreList_PropertyChanged);
                    }
                    else
                    {
                        int tmpInt = m_List[i].Value;
                        int.TryParse(this.Score[i].ToString(), out tmpInt);
                        m_List[i].Value = tmpInt;
                    }
                    currentI = i;
                }
            }

            for (int i = currentI; i < m_List.Count; i++)
            {
                m_List[i].Value = 0;
            }

            m_SettingScore = false;
        }

        private void UpdateScore()
        {
            if (m_SettingScore)
                return;

            string tmpStr = string.Empty;

            foreach (NixxisScoreArrayElement item in m_List)
            {
                tmpStr += item.Value.ToString();
            }

            this.Score = tmpStr;
        }

        private void SetLabels()
        {
            if(Labels == null)
                return;
            
            for (int i = 0; i < Labels.Length; i++)
            {
                if (i >= m_List.Count)
                {
                    m_List.Add(new NixxisScoreArrayElement() { Label = Labels[i], Length = this.Length, Value = 0, ActiveImageSource = this.ActiveImageSource, InactiveImageSource = this.InactiveImageSource });
                    m_List[i].PropertyChanged += new PropertyChangedEventHandler(NixxisScoreList_PropertyChanged);
                }
                else
                {
                    m_List[i].Label = Labels[i];
                }
            }
        }

        private void UpdateActiveImageSource()
        {
            foreach (NixxisScoreArrayElement item in m_List)
            {
                item.ActiveImageSource = this.ActiveImageSource;
            }
        }

        private void UpdateInactiveImageSource()
        {
            foreach (NixxisScoreArrayElement item in m_List)
            {
                item.InactiveImageSource = this.InactiveImageSource;
            }
        }

        private void UpdateLength()
        {
            foreach (NixxisScoreArrayElement item in m_List)
            {
                item.Length = this.Length;
            }
        }
        #endregion

        #region Handlers
        private void NixxisScoreList_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                this.UpdateScore();
            }
        }
        #endregion
    }

    public class NixxisScoreArray : ObservableCollection<NixxisScoreArrayElement>
    {
    }

    public class NixxisScoreArrayElement : INotifyPropertyChanged
    {
        private string m_Label;
        public string Label
        {
            get { return m_Label; }
            set { m_Label = value; FirePropertyChanged("Label"); }
        }

        private int m_Value;
        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; FirePropertyChanged("Value"); }
        }

        private int m_Length;
        public int Length
        {
            get { return m_Length; }
            set { m_Length = value; FirePropertyChanged("Length"); }
        }

        private ImageSource m_ActiveImageSource;
        public ImageSource ActiveImageSource
        {
            get { return m_ActiveImageSource; }
            set { m_ActiveImageSource = value; FirePropertyChanged("ActiveImageSource"); }
        }

        private ImageSource m_InactiveImageSource;
        public ImageSource InactiveImageSource
        {
            get { return m_InactiveImageSource; }
            set { m_InactiveImageSource = value; FirePropertyChanged("InactiveImageSource"); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        }
    }
}
