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

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for NixxisToolbarPanelSelector.xaml
    /// </summary>
    public partial class NixxisPanelSelector : UserControl
    {
        #region Class data
        private List<string> m_PanelList = new List<string>();
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(NixxisPanelSelector), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ShowPanelProperty = DependencyProperty.Register("ShowPanel", typeof(string), typeof(NixxisPanelSelector), new FrameworkPropertyMetadata(ShowPanelPropertyChanged));
        public string ShowPanel
        {
            get { return (string)GetValue(ShowPanelProperty); }
            set { SetValue(ShowPanelProperty, value); }
        }
        public static void ShowPanelPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ParentFormProperty = DependencyProperty.Register("ParentForm", typeof(FrameworkElement), typeof(NixxisPanelSelector), new FrameworkPropertyMetadata(ParentFormPropertyChanged));
        public FrameworkElement ParentForm
        {
            get { return (FrameworkElement)GetValue(ParentFormProperty); }
            set { SetValue(ParentFormProperty, value); }
        }
        public static void ParentFormPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ControlsProperty = DependencyProperty.Register("Controls", typeof(FrameworkElement), typeof(NixxisPanelSelector), new FrameworkPropertyMetadata(ControlsPropertyChanged));
        public FrameworkElement Controls
        {
            get { return (FrameworkElement)this.GetValue(ControlsProperty); }
            set { this.SetValue(ControlsProperty, value); }
        }
        public static void ControlsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisPanelSelector item = (NixxisPanelSelector)obj;
            item.InitControls();
        }
        #endregion

        #region Constructors
        public NixxisPanelSelector()
        {
            InitializeComponent();
        }
        #endregion

        #region Members Override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        void NixxisPanelSelector_RequestAction(object sender, NixxisPanelSelectorEventArgs e)
        {

        }

        void NixxisPanelSelector_BringToFront(object sender, NixxisPanelSelectorEventArgs e)
        {
            try
            {
                string newKey = ((INixxisPanelSelectorItem)sender).NixxisPanelKey;
                int newIndex = -1;

                foreach (string item in m_PanelList)
                {
                    newIndex++;
                    if (item == newKey)
                        break;
                } 
                SetPanel(newIndex);
            }
            catch { }
        }

        public override void EndInit()
        {
            base.EndInit();

            if (this.Controls != null)
                this.InitControls();
        }


        #endregion

        #region Members
        private void InitControls()
        {
            m_PanelList = new List<string>();
            ShowPanel = string.Empty;

            if (this.ContextMenu == null)
                this.ContextMenu = new ContextMenu();
            else
                this.ContextMenu.Items.Clear();

            if (Controls == null)
                return;

            if (Controls is Panel)
            {
                for (int i = 0; i < ((Panel)Controls).Children.Count; i++)
                {
                    UIElement item = ((Panel)Controls).Children[i];
                    item.Visibility = System.Windows.Visibility.Collapsed;

                    if (item is INixxisPanelSelectorItem && item is FrameworkElement)
                    {
                        string itemKey = ((INixxisPanelSelectorItem)item).NixxisPanelKey;
                        string itemDescription = ((INixxisPanelSelectorItem)item).NixxisPanelDescription;

                        if (string.IsNullOrEmpty(itemKey))
                        {
                            itemKey = ((INixxisPanelSelectorItem)item).NixxisPanelDescription;
                            ((INixxisPanelSelectorItem)item).NixxisPanelKey = itemKey;
                        }

                        if (string.IsNullOrEmpty(itemDescription))
                            itemDescription = itemKey;

                        if (!m_PanelList.Contains(itemKey))
                        {
                            m_PanelList.Add(itemKey);
                            ((INixxisPanelSelectorItem)item).BringToFront += new NixxisPanelSelectorEventHandler(NixxisPanelSelector_BringToFront);
                            ((INixxisPanelSelectorItem)item).RequestAction += new NixxisPanelSelectorEventHandler(NixxisPanelSelector_RequestAction);

                            //Binding Visibility
                            Binding bVis = new Binding("ShowPanel");
                            bVis.ElementName = "MySelf";
                            bVis.Converter = new Nixxis.Client.Controls.ObjectCompairToVisibilityConverter();
                            bVis.ConverterParameter = itemKey;
                            ((FrameworkElement)item).SetBinding(FrameworkElement.VisibilityProperty, bVis);

                            if (item is NixxisPanelSelectorItem)
                            {
                                //Binding ClientLink
                                Binding bCl = new Binding("ClientLink");
                                bCl.ElementName = "MySelf";
                                ((NixxisPanelSelectorItem)item).SetBinding(NixxisPanelSelectorItem.ClientLinkProperty, bCl);

                                //Binding ParentForm
                                Binding bPf = new Binding("ParentForm");
                                bPf.ElementName = "MySelf";
                                ((NixxisPanelSelectorItem)item).SetBinding(NixxisPanelSelectorItem.ParentFormProperty, bPf);

                                MenuItem mi = new MenuItem();
                                mi.Header = itemDescription;
                                mi.Tag = m_PanelList.IndexOf(itemKey);
                                mi.IsCheckable = true;
                                mi.Click += new RoutedEventHandler(MenuItem_Click);
                                this.ContextMenu.Items.Add(mi);
                            }
                        }
                    }
                }
            }

            if (m_PanelList.Count > 0)
                SetPanel(-1, 0);
        }


        private void SetPanel(int oldIndex, int newIndex)
        {
            try
            {
                if (oldIndex >= 0)
                {
                    ((MenuItem)this.ContextMenu.Items[oldIndex]).IsChecked = false;
                }
            }
            catch { }
            try
            {
                if (newIndex >= 0)
                {
                    ((MenuItem)this.ContextMenu.Items[newIndex]).IsChecked = true;
                }
            }
            catch { }

            this.ShowPanel = m_PanelList[newIndex];
        }
        private void SetPanel(int newIndex)
        {
            int oldIndex = -1;
            try
            {
                foreach (string item in m_PanelList)
                {
                    oldIndex++;
                    if (item == this.ShowPanel)
                        break;
                }
            }
            catch { }

            SetPanel(oldIndex, newIndex);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int index = (int)((FrameworkElement)sender).Tag;

            
            SetPanel(index);
        }

        private void MySelf_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_PanelList.Count > 1)
            {
                //Find index back
                int currentIndex = 0;
                int oldIndex = -1;
                foreach (string item in m_PanelList)
                {
                    currentIndex++;
                    oldIndex++;
                    if (item == this.ShowPanel)
                        break;
                }

                //Go to new index
                if (currentIndex >= m_PanelList.Count)
                    currentIndex = 0;

                SetPanel(oldIndex, currentIndex);

            }
        }
        #endregion
    }

    public interface INixxisPanelSelectorItem
    {
        event NixxisPanelSelectorEventHandler BringToFront;
        event NixxisPanelSelectorEventHandler RequestAction;

        HttpLink ClientLink { get; set; }
        FrameworkElement ParentForm { get; set; }
        String NixxisPanelKey { get; set; }
        String NixxisPanelDescription { get; set; }
    }

    public class NixxisPanelSelectorItem : UserControl, INixxisPanelSelectorItem
    {
        #region Class data
        private string m_NixxisPanelDescription = string.Empty;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(NixxisPanelSelectorItem), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ParentFormProperty = DependencyProperty.Register("ParentForm", typeof(FrameworkElement), typeof(NixxisPanelSelectorItem), new FrameworkPropertyMetadata(ParentFormPropertyChanged));
        public FrameworkElement ParentForm
        {
            get { return (FrameworkElement)GetValue(ParentFormProperty); }
            set { SetValue(ParentFormProperty, value); }
        }
        public static void ParentFormPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty NixxisPanelKeyProperty = DependencyProperty.Register("NixxisPanelKey", typeof(string), typeof(NixxisPanelSelectorItem));
        public string NixxisPanelKey
        {
            get { return (string)GetValue(NixxisPanelKeyProperty); }
            set { SetValue(NixxisPanelKeyProperty, value); }
        }

        public static readonly DependencyProperty NixxisPanelDescriptionProperty = DependencyProperty.Register("NixxisPanelDescription", typeof(string), typeof(NixxisPanelSelectorItem));
        public string NixxisPanelDescription
        {
            get { return (string)GetValue(NixxisPanelDescriptionProperty); }
            set { SetValue(NixxisPanelDescriptionProperty, value); }
        }
        #endregion

        #region Events
        public event NixxisPanelSelectorEventHandler BringToFront;
        protected void OnBringToFront()
        {
            if (BringToFront != null)
                BringToFront(this, new NixxisPanelSelectorEventArgs());
        }

        public event NixxisPanelSelectorEventHandler RequestAction;
        protected void OnRequestAction()
        {
            if (RequestAction != null)
                RequestAction(this, new NixxisPanelSelectorEventArgs());
        }
        #endregion
    }

    public class NixxisPanelSelectorEventArgs
    {

    }

    public delegate void NixxisPanelSelectorEventHandler(object sender, NixxisPanelSelectorEventArgs e);
}
