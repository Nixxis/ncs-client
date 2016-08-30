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
using System.Windows.Threading;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for SkillsControl.xaml
    /// </summary>
    public partial class PromptsControl : UserControl
    {
        private static TranslationContext TranslationContext = new TranslationContext("PromptsControl");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }


        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(PromptsControl));

        public void SetSelectedWithContext(AdminObject selected, AdminObject context, object ExtraInformation)
        {
            if (selected != null)
            {
                SelectedItem = ((Prompt)selected).Links[0];
            }
        }

        public void SetSelected(AdminObject selected)
        {
            if (selected != null)
            {
                if (selected is PromptLink)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() => { MainGrid.SelectedItem = selected; }));
                }
                else if (selected is Prompt)
                {
                    Prompt pr = (Prompt)selected;
                    if (pr.Links != null && pr.Links.Count > 0)
                    {
                        PromptLink pl = (PromptLink)pr.Links[0];
                        SetSelected(pl);
                    }

                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        public PromptsControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(PromptsControl_IsVisibleChanged);

            InitializeComponent();
            ((CollectionViewSource)(FindResource("List"))).Filter += new FilterEventHandler(List_Filter);
        }

        private bool BaseFilter(AdminObject obj, string groupkey)
        {
            return (!obj.IsSystem && !obj.IsDummy && !obj.IsPartial) && (groupkey == null || groupkey.Equals(obj.GroupKey));
        }


        void List_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = BaseFilter(e.Item as AdminObject, null);
        }


        void PromptsControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)sender).IsVisible)
            {
                NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.CanExecute(nep))
                        Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.Execute(nep);
                }
                nep = FindResource("DetailsPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.CanExecute(nep))
                        Nixxis.Client.Controls.NixxisGrid.SetPanelCommand.Execute(nep);
                }

            }
            else
            {
                NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                        NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("DetailsPanel") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                        NixxisGrid.RemovePanelCommand.Execute(nep);
                }

            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                DlgAddGlobalPrompt dlg = new DlgAddGlobalPrompt();

                dlg.Owner = Application.Current.MainWindow;
                dlg.DataContext = DataContext;

                if (MainGrid.SelectedItem != null && TabPrompts.IsSelected)
                {
                    dlg.Prompt = (MainGrid.SelectedItem as PromptLink).Prompt;
                    dlg.PromptUnder = true;
                    dlg.WizControl.CurrentStep = "StartWithChoice";
                    dlg.WizControl.Context = SelectedItem;
                }
                else
                {
                    dlg.WizControl.CurrentStep = "Start";
                }

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    SetSelected(dlg.Created);
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                if ( TabPrompts.IsSelected && DGCampRelatedPrompts.SelectedItem!=null )
                {
                    DeleteTargetConfirmationDialog dlg = new DeleteTargetConfirmationDialog();

                    dlg.ItemsSource = new List<string>() { 
                            string.Format("Selected prompt related to {0}", ((PromptLink)MainGrid.SelectedItem).Prompt.DisplayText),
                            ((PromptLink)MainGrid.SelectedItem).Prompt.TypedDisplayText
                            };


                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        PromptLink pr = (PromptLink)MainGrid.SelectedItem;

                        if (dlg.SelectedIndex == 0)
                        {
                            Prompt prr = (Prompt)DGCampRelatedPrompts.SelectedItem;
                            (DataContext as AdminCore).Delete(prr);
                        }
                        else
                        {
                            (DataContext as AdminCore).Delete(pr.Prompt);
                        }
                    }
                }
                else
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.MessageText = string.Format(Translate("Are you sure you want to delete the prompt \"{0}\"?"), ((PromptLink)MainGrid.SelectedItem).Prompt.DisplayText);
                    dlg.Owner = Application.Current.MainWindow;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        PromptLink pr = MainGrid.SelectedItem as PromptLink;
                        (DataContext as AdminCore).Delete(pr.Prompt);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectDuplicateOperation)
            {
            }
            else if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);

                    afs.ShowObjectWithContext(toShow, (AdminObject)MainGrid.SelectedItem);

                }
            }
            
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.ShowObject)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    AdminObject toShow = AdminFrameSet.GetContextMenuObject(cm);

                    AdminFrameSet afs = Helpers.FindParent<AdminFrameSet>(this);


                    e.CanExecute = afs.CanShowObject(toShow);
                    e.Handled = true;

                }
            }
            else if (e.Command == AdminFrameSet.AdminObjectAddOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == AdminFrameSet.AdminObjectDeleteOperation)
            {
                e.CanExecute = (MainGrid.SelectedItem != null) && (!((AdminObject)MainGrid.SelectedItem).IsSystemWithValidOwner) && (((AdminObject)MainGrid.SelectedItem).IsDeletable);
            }
            else
            {
                e.CanExecute = (MainGrid.SelectedItem != null);
            }
            e.Handled = true;
        }

        private string m_BackupId;

        public void BackupSelection()
        {
            AdminObject agt = MainGrid.SelectedItem as AdminObject;
            if (agt != null)
                m_BackupId = agt.Id;
            else
                m_BackupId = null;
        }

        public void RestoreSelection()
        {
            if (m_BackupId != null)
            {
                SetSelected((DataContext as AdminCore).GetAdminObject(m_BackupId));
            }
        }
    }
}
