using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Nixxis.Windows.Controls
{
    public partial class ScoreSelection : UserControl
    {
        #region Class data
        private int m_Length = 5;
        private Image m_ActiveImage = global::crCustomControl.Resource.rated_16;
        private Image m_InActiveImage = global::crCustomControl.Resource.unrated_16;
        private int m_Value = 0;

        private PictureBox[] m_Score; 
        #endregion

        public ScoreSelection()
        {
            InitializeComponent();
        }

        #region Properties
        public int Length
        {
            get { return m_Length; }
            set { m_Length = value;  }
        }
        public Image ActiveImage
        {
            get { return m_ActiveImage; }
            set { m_ActiveImage = value; }
        }
        public Image InActiveImage
        {
            get { return m_InActiveImage; }
            set { m_InActiveImage = value; }
        }
        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; SetValue(value); }
        }
        #endregion

        private void ScoreSelection_Load(object sender, EventArgs e)
        {
            m_Score = new PictureBox[m_Length];

            for (int i = 0; i < m_Score.Length; i++)
                m_Score[i] = CreateScoreItem();

            Draw();
        }
        private PictureBox CreateScoreItem()
        {
            PictureBox item = new PictureBox();
            item.Image = m_InActiveImage;
            item.Height = m_InActiveImage.Height + 2;
            item.Width = m_InActiveImage.Width + 2;
            item.Click += new EventHandler(Score_Click);
            item.SizeMode = PictureBoxSizeMode.CenterImage;

            return item;
        }

        void Score_Click(object sender, EventArgs e)
        {
            PictureBox item = (PictureBox)sender;
            SetValue(((int)item.Tag) + 1);
            
        }
        private void SetValue(int value)
        {
            //if (value == null) return;
            if (m_Score == null) return;

            for (int i = 0; i < m_Score.Length; i++)
            {
                if (i < value)
                    m_Score[i].Image = m_ActiveImage;
                else
                    m_Score[i].Image = m_InActiveImage;
            }
            m_Value = value;
        }
        private void Draw()
        {
            for (int i = 0; i < m_Score.Length; i++)
            {
                m_Score[i].Left = i * (m_InActiveImage.Width + 1);
                m_Score[i].Top = 0;
                m_Score[i].Tag = i;
                this.Controls.Add(m_Score[i]);
            }
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }
    }
}