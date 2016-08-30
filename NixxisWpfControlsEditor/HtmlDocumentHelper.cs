using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Nixxis.Client.Controls.HtmlHelpers
{
    //
    //Hyper link
    //
    public class HyperlinkObject
    {
        private string m_Text;
        private string m_Url;

        public string Url
        {
            get { return m_Url; }
            set { m_Url = value; }
        }
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }
    }

    //
    //Font
    //
    public static class NixxisFont
    {
        public static ObservableCollection<FontSizeItem> FontSizeCollection { get; private set; }

        static NixxisFont()
        {
            FontSizeCollection = new ObservableCollection<FontSizeItem>();

            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Size = 0 });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 1, Size = 8.5, Text = "8pt" });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 2, Size = 10.5, Text = "10pt" });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 3, Size = 12, Text = "12pt" });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 4, Size = 14, Text = "14pt" });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 5, Size = 18, Text = "18pt" });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 6, Size = 24, Text = "24pt" });
            NixxisFont.FontSizeCollection.Add(new FontSizeItem { Key = 7, Size = 36, Text = "36pt" });
        }

        public static FontSizeItem NO { get { return FontSizeCollection[0]; } }
        public static FontSizeItem XXSmall { get { return FontSizeCollection[1]; } }
        public static FontSizeItem XSmall { get { return FontSizeCollection[2]; } }
        public static FontSizeItem Small { get { return FontSizeCollection[3]; } }
        public static FontSizeItem Middle { get { return FontSizeCollection[4]; } }
        public static FontSizeItem Large { get { return FontSizeCollection[5]; } }
        public static FontSizeItem XLarge { get { return FontSizeCollection[6]; } }
        public static FontSizeItem XXLarge { get { return FontSizeCollection[7]; } }
    }

    public class FontSizeItem : INotifyPropertyChanged
    {
        #region Class data
        private int m_Key;
        private double m_Size;
        private string m_Text;
        #endregion

        #region Propeties
        public int Key
        {
            get { return m_Key; }
            set { m_Key = value; FirePropertyChanged("Key"); }
        }
        public double Size
        {
            get { return m_Size; }
            set { m_Size = value; FirePropertyChanged("Size"); }
        }
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; FirePropertyChanged("Text"); }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }

    //
    //
    //
}
