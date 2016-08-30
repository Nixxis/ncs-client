using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections;
using System.ComponentModel;
using ContactRoute;

namespace Nixxis.Client.Controls
{
    public enum NixxisDataGridEditMode
    {
        Standard,
        SingleClick,
        SingleClickWhenSelected
    }

    public class NixxisDataGridRow : DataGridRow
    {
        public static readonly DependencyProperty AlertLevelProperty = DependencyProperty.Register("AlertLevel", typeof(int), typeof(NixxisDataGridRow), new PropertyMetadata(-1, new PropertyChangedCallback(AlertLevel_Changed), new CoerceValueCallback(AlertLevel_Coerce)));

        private static object AlertLevel_Coerce(DependencyObject d, object baseValue)
        {
            // A trick to prevent generating an alarm cleared event (causing a initial flash)
            NixxisDataGridRow ndr = d as NixxisDataGridRow;
            if (ndr.AlertLevel < 0 && ((int)baseValue) == 0)
                return -1;
            return baseValue;
        }

        private static void AlertLevel_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public int AlertLevel
        {
            get
            {
                return (int)GetValue(AlertLevelProperty);
            }
            set
            {
                SetValue(AlertLevelProperty, value);
            }
        }
    }

    public class NixxisDataGrid : DataGrid
    {
        private const string ScrollViewerNameInTemplate = "DG_ScrollViewer";
        private const string VerticalScrollBarNameInTemplate = "PART_VerticalScrollBar";
        public static List<DataGridRow> GlobalRows = new List<DataGridRow>();

        public bool IsEditing { get; set; }
        public bool IsDragging { get; set; }
        public object DraggedItem { get; set; }

        public Popup DragDropPopup { get; set; }



        public static readonly DependencyProperty ClickedDataProperty = DependencyProperty.RegisterAttached("ClickedData", typeof(object), typeof(NixxisDataGrid));

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register("EditMode", typeof(NixxisDataGridEditMode), typeof(NixxisDataGrid), new PropertyMetadata(NixxisDataGridEditMode.SingleClickWhenSelected));
        public static readonly DependencyProperty ColumnsSelectorProperty = DependencyProperty.Register("ColumnsSelector", typeof(ListBox), typeof(NixxisDataGrid), new PropertyMetadata(null, new PropertyChangedCallback(ColumnsSelectorChanged)));
        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register("Footer", typeof(NixxisDataGridFooter), typeof(NixxisDataGrid), new PropertyMetadata(null, new PropertyChangedCallback(FooterChanged)));
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register("IsSelectable", typeof(bool), typeof(NixxisDataGrid), new PropertyMetadata(true, new PropertyChangedCallback(IsSelectableChanged)));

        public static readonly DependencyProperty DefaultSortOrderProperty = DependencyProperty.Register("DefaultSortOrder", typeof(string), typeof(NixxisDataGrid));

        public static readonly DependencyProperty IsSelectedActiveProperty = DependencyProperty.RegisterAttached("IsSelectedActive", typeof(bool), typeof(NixxisDataGrid), new PropertyMetadata(false));

        public static readonly DependencyProperty AutoUncheckProperty = DependencyProperty.RegisterAttached("AutoUncheck", typeof(bool), typeof(NixxisDataGrid), new PropertyMetadata(true));



        public static readonly DependencyProperty IsDisabledProperty = DependencyProperty.Register("IsDisabled", typeof(bool), typeof(NixxisDataGrid), new PropertyMetadata(false, new PropertyChangedCallback(IsDisabledChanged)));

        public static void ColumnsSelectorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                NixxisDataGrid mainGrid = (NixxisDataGrid)obj;
                ListBox selector = (ListBox)args.NewValue;
                if (selector != null)
                {
                    Style stl = new Style(typeof(ListBoxItem));

                    stl.Setters.Add(new Setter(TemplateProperty, (ControlTemplate)(selector.FindResource("ColumnsSelectorItemCtrlTemplate"))));

                    selector.Resources.Add(typeof(ListBoxItem), stl);
                    selector.ItemTemplate = (DataTemplate)(selector.FindResource("ColumnsSelectorItemTemplate"));
                    selector.ItemsSource = mainGrid.Columns;
                    selector.SourceUpdated += new EventHandler<DataTransferEventArgs>(mainGrid.selector_TargetUpdated);
                }
            }
        }

        void selector_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            CheckBox cb = e.TargetObject as CheckBox;
            DataGridColumn dgc = (cb.Tag as DataGridColumn);

            if (ColumnVisibilityChanged != null)
                ColumnVisibilityChanged(dgc, null);
        }

        public static void FooterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                NixxisDataGrid mainGrid = (NixxisDataGrid)obj;
                NixxisDataGridFooter footer = (NixxisDataGridFooter)args.NewValue;

                if (mainGrid != null && footer != null)
                {
                    footer.RelatedGrid = mainGrid;

                    mainGrid.BorderThickness = new Thickness(mainGrid.BorderThickness.Left, mainGrid.BorderThickness.Top, mainGrid.BorderThickness.Right, 0);

                    // Needed to ensure it is working!!!
                    footer.ApplyTemplate();

                    ScrollViewer sv = (ScrollViewer)footer.Template.FindName(ScrollViewerNameInTemplate, footer);

                    // Needed to ensure it is working!!!
                    sv.ApplyTemplate();
                    ((ScrollBar)sv.Template.FindName(VerticalScrollBarNameInTemplate, sv)).Template = new ControlTemplate();

                    // Needed to ensure it is working!!!
                    mainGrid.ApplyTemplate();

                    ScrollViewer sv2 = (ScrollViewer)mainGrid.Template.FindName(ScrollViewerNameInTemplate, mainGrid);

                    sv.SetBinding(ScrollViewer.VerticalScrollBarVisibilityProperty, new Binding() { Source = sv2, Mode = BindingMode.OneWay, Path = new PropertyPath(ScrollViewer.ComputedVerticalScrollBarVisibilityProperty), Converter = new ScrollBarVisibilityConverter() });

                    footer.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(Footer_ScrollChanged));

                    SynchronizeVerticalDataGrid(mainGrid, footer);
                }
            }
        }
        public static void IsSelectableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (((bool)args.NewValue))
            {
            }
            else
            {
                ((NixxisDataGrid)obj).SelectionChanged += (o, x) => ((NixxisDataGrid)obj).Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ((NixxisDataGrid)obj).UnselectAll()));
            }
        }


        public static void IsDisabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((NixxisDataGrid)obj).IsReadOnly = (bool)(args.NewValue);
        }

        public static void SetClickedData(UIElement element, object value)
        {            
            element.SetValue(ClickedDataProperty, value);
        }
        public static object GetClickedData(UIElement element)
        {
            return (element.GetValue(ClickedDataProperty));
        }
        public static void SetOnlySelectedActive(DataGridRow row)
        {            
            {
                if (!NixxisDataGrid.GlobalRows.Contains(row))
                    NixxisDataGrid.GlobalRows.Add(row);
                foreach (UIElement uie in NixxisDataGrid.GlobalRows)
                {
                    SetIsSelectedActive(uie, uie == row);
                }
            }
        }

        public static void SetIsSelectedActive(UIElement element, bool value)
        {
            element.SetValue(IsSelectedActiveProperty, value);
        }
        public static bool GetIsSelectedActive(UIElement element)
        {
            return (bool)(element.GetValue(IsSelectedActiveProperty));
        }

        public static void SetAutoUncheck(UIElement element, bool value)
        {
            element.SetValue(AutoUncheckProperty, value);
        }
        public static bool GetAutoUncheckProperty(UIElement element)
        {
            return (bool)(element.GetValue(AutoUncheckProperty));
        }


        public bool IsDisabled
        {
            get
            {
                return (bool)GetValue(IsDisabledProperty);
            }
            set
            {
                SetValue(IsDisabledProperty, value);
            }
        }

        public NixxisDataGridEditMode EditMode
        {
            get
            {
                return (NixxisDataGridEditMode)GetValue(EditModeProperty);
            }
            set
            {
                SetValue(EditModeProperty, value);
            }
        }
        public ListBox ColumnsSelector
        {
            get
            {
                return (ListBox)GetValue(ColumnsSelectorProperty);
            }
            set
            {
                SetValue(ColumnsSelectorProperty, value);
            }
        }
        public NixxisDataGridFooter Footer
        {
            get
            {
                return (NixxisDataGridFooter)GetValue(FooterProperty);
            }
            set
            {
                SetValue(FooterProperty, value);
            }
        }
        public bool IsSelectable
        {
            get
            {
                return (bool)GetValue(IsSelectableProperty);
            }
            set
            {
                SetValue(IsSelectableProperty, value);
            }
        }
        public string DefaultSortOrder
        {
            get
            {
                return (string)GetValue(DefaultSortOrderProperty);
            }
            set
            {
                SetValue(DefaultSortOrderProperty, value);
            }
        }

        public event EventHandler ColumnVisibilityChanged;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        static NixxisDataGrid()
        {
            EventManager.RegisterClassHandler(typeof(DataGridCell), DataGridCell.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(DataGridCell_PreviewMouseLeftButtonDown));
            EventManager.RegisterClassHandler(typeof(DataGridCell), DataGridCell.GotFocusEvent, new RoutedEventHandler(DataGridCell_GotFocus) );
        }

        public NixxisDataGrid()
        {
            AddHandler(DataGrid.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
            AddHandler(DataGrid.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
            AddHandler(DataGrid.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));
            AddHandler(DataGrid.LoadedEvent, new RoutedEventHandler(OnLoad));
        }

        private void OnLoad(object sender, RoutedEventArgs args)
        {
            SelectedIndex = -1;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new NixxisDataGridRow();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            try
            {
                base.OnSelectionChanged(e);

                if (SelectedItem != null)
                    this.ScrollIntoView(SelectedItem);
            }
            catch
            {
            }
        }

        private static void SynchronizeVerticalDataGrid(DataGrid source, DataGrid associated)
        {
            associated.HeadersVisibility = source.HeadersVisibility & (DataGridHeadersVisibility.Row);

            source.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

            int sourceColIndex = 0;

            for (int associatedColIndex = 0; associatedColIndex < associated.Columns.Count; associatedColIndex++)
            {
                var colAssociated = associated.Columns[associatedColIndex];

                if (sourceColIndex >= source.Columns.Count)
                    break;

                var colSource = source.Columns[sourceColIndex];
                Binding binding = new Binding();
                binding.Mode = BindingMode.TwoWay;
                binding.Source = colSource;
                binding.Path = new PropertyPath(DataGridColumn.WidthProperty);
                BindingOperations.SetBinding(colAssociated, DataGridColumn.WidthProperty, binding);

                binding = new Binding();
                binding.Mode = BindingMode.TwoWay;
                binding.Source = colSource;
                binding.Path = new PropertyPath(DataGridColumn.VisibilityProperty);
                BindingOperations.SetBinding(colAssociated, DataGridColumn.VisibilityProperty, binding);


                sourceColIndex++;

            }
        }

        protected override void OnColumnDisplayIndexChanged(DataGridColumnEventArgs e)
        {
            base.OnColumnDisplayIndexChanged(e);

            if (Footer != null)
            {
                Footer.Columns[this.Columns.IndexOf(e.Column)].DisplayIndex = e.Column.DisplayIndex;
            }
        }

        private static void Footer_ScrollChanged(object sender, RoutedEventArgs eBase)
        {
            ScrollChangedEventArgs e = (ScrollChangedEventArgs)eBase;

            ScrollViewer sourceScrollViewer = (ScrollViewer)e.OriginalSource;

            SynchronizeScrollHorizontalOffset(((NixxisDataGridFooter)sender).RelatedGrid, sourceScrollViewer);
        }
        private static void SynchronizeScrollHorizontalOffset(DataGrid associatedDataGrid, ScrollViewer sourceScrollViewer)
        {
            if (associatedDataGrid != null)
            {
                ScrollViewer associatedScrollViewer = (ScrollViewer)associatedDataGrid.Template.FindName(ScrollViewerNameInTemplate, associatedDataGrid);
                associatedScrollViewer.ScrollToHorizontalOffset(sourceScrollViewer.HorizontalOffset);
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            if (this.ContextMenu != null)
            {
                Focus();
                SetClickedData(this.ContextMenu, (e.OriginalSource as FrameworkElement).DataContext);
            }

        }

        private static void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            if (cell != null)
            {

                DataGridRow dgr = Helpers.FindVisualParent<DataGridRow>(cell);
                if (dgr != null)
                {
                    NixxisDataGrid ndg =  Helpers.FindVisualParent<DataGrid>(dgr) as NixxisDataGrid;

                    if (dgr.IsSelected && dgr.DetailsVisibility == Visibility.Visible)
                    {                                                                       
                        Helpers.ApplyToVisualChildren<DataGrid>(dgr, DataGrid.SelectedItemProperty, null);
                    }

                    if(ndg!=null)
                    {
                        ndg.OnSelectedCellsChanged(null);
                    }
                }

            }
        }

        private static void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            // TODO: probably a few stuff to review here


            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                DependencyObject obj = Helpers.FindFirstControlInChildren(cell, "CheckBox");
                if (obj != null)
                {
                    System.Windows.Controls.CheckBox cb = (System.Windows.Controls.CheckBox)obj;
                    if (!cb.IsVisible)
                        return;
                    if (!cb.IsFocused )
                    {
                        if (cell.IsFocused && cell.IsSelected)
                            return;

                        if (cb.IsEnabled)
                            cb.Focus();
                        else
                        {
                            cell.Focus();
                            cell.IsSelected = true;
                            cell.IsEditing = true;
                            if (!cb.IsChecked.GetValueOrDefault())
                            {
                                if (!(e.OriginalSource is TextBox) && !(e.OriginalSource is TextBlock))
                                {
                                    if (cb.IsEnabled)
                                    {
                                        if (cb.IsThreeState)
                                        {
                                            if (!cb.IsChecked.HasValue)
                                                cb.IsChecked = false;
                                            else if (cb.IsChecked.Value)
                                            {
                                                cb.IsChecked = null;
                                            }
                                            else
                                            {
                                                cb.IsChecked = true;
                                            }

                                        }
                                        else
                                        {
                                            if (!cb.IsChecked.GetValueOrDefault())
                                                cb.IsChecked = !cb.IsChecked;
                                        }
                                    }
                                }
                            }
                            return;
                        }


                        if( !(e.OriginalSource is TextBox) && !(e.OriginalSource is TextBlock) )
                        {
                            if (Helpers.FindFirstControlInChildren(cell, "TextBlock") != null)
                            {
                                if (cb.IsEnabled)
                                {
                                    if (cb.IsThreeState)
                                    {
                                        if (!cb.IsChecked.HasValue)
                                            cb.IsChecked = false;
                                        else if (cb.IsChecked.Value)
                                        {
                                            cb.IsChecked = null;
                                        }
                                        else
                                        {
                                            cb.IsChecked = true;
                                        }
                                    }
                                    else
                                    {
                                        if (!cb.IsChecked.GetValueOrDefault())
                                            cb.IsChecked = !cb.IsChecked;
                                    }
                                }
                            }
                            else
                            {
                                if (cb.IsEnabled)
                                {
                                    if (cb.IsThreeState)
                                    {
                                        if (!cb.IsChecked.HasValue)
                                            cb.IsChecked = false;
                                        else if (cb.IsChecked.Value)
                                        {
                                            cb.IsChecked = null;
                                        }
                                        else
                                        {
                                            cb.IsChecked = true;
                                        }
                                    }
                                    else
                                    {
                                        cb.IsChecked = !cb.IsChecked;
                                    }
                                }
                            }
                        }
                    }
                }
                obj = Helpers.FindFirstControlInChildren(cell, "Slider");
                if (obj != null)
                {
                    cell.IsEditing = true;
                    cell.Focus();
                }

            }
            return;

            
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                NixxisDataGrid ndg = Helpers.FindVisualParent<NixxisDataGrid>(cell);
                if (ndg == null)
                    return;
                if (ndg.EditMode == NixxisDataGridEditMode.Standard)
                    return;

                DataGridRow row = Helpers.FindVisualParent<DataGridRow>(cell);

                if (ndg.EditMode == NixxisDataGridEditMode.SingleClickWhenSelected)
                {
                    if (row == null || !row.IsSelected)
                        return;
                }

                if (!cell.IsFocused)
                {
                    try
                    {
                        cell.Focus();
                    }
                    catch
                    {
                    }
                }
                DataGrid dataGrid = Helpers.FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                        {
                            cell.IsSelected = true;
                        }
                    }
                    else
                    {
                        if (ndg.EditMode != NixxisDataGridEditMode.SingleClickWhenSelected)
                        {
                            row = Helpers.FindVisualParent<DataGridRow>(cell);
                            if (row != null && !row.IsSelected)
                            {
                                row.IsSelected = true;
                            }
                        }
                    }
                }
            }
        }
       


        protected override void OnBeginningEdit(DataGridBeginningEditEventArgs e)
        {
            base.OnBeginningEdit(e);
            IsEditing = true;
        }

        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            base.OnCellEditEnding(e);
            IsEditing = false;
        }

        protected void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (IsEditing || DragDropPopup == null) return;

            var row = Helpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(this));
            if (row == null || row.IsEditing) return;

            //set flag that indicates we're capturing mouse movements
            IsDragging = true;
            DraggedItem = row.Item;

        }

        protected void OnMouseMove(object sender, MouseEventArgs e)
        {

            if (!IsDragging || e.LeftButton != MouseButtonState.Pressed || DragDropPopup == null) return;

            //display the popup if it hasn't been opened yet
            if (!DragDropPopup.IsOpen)
            {
                //switch to read-only mode
                this.IsReadOnly = true;

                //make sure the popup is visible
                DragDropPopup.IsHitTestVisible = false;
                DragDropPopup.Focusable = false;

                DragDropPopup.Placement = PlacementMode.RelativePoint;
                DragDropPopup.DataContext = this;

                DragDropPopup.AddHandler(Popup.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
                DragDropPopup.AddHandler(Popup.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));

                DragDropPopup.PlacementTarget = this;
                DragDropPopup.IsOpen = true;
            }


            Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
            DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

            //make sure the row under the grid is being selected
            Point position = e.GetPosition(this);
            var row = Helpers.TryFindFromPoint<DataGridRow>(this, position);
            if (row != null)
            {
                SelectedItem = row.Item;
            }
            else
            {
                SelectedItem = null;
            }
        }

        protected void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (!IsDragging || IsEditing || DragDropPopup == null)
                return;

            //get the target item
            object targetItem = SelectedItem;


            if (targetItem!=null && !ReferenceEquals(DraggedItem, targetItem))
            {
                IList internalList = this.ItemsSource as IList;

                internalList.Remove(DraggedItem);

                var targetIndex = Items.IndexOf(targetItem);

                internalList.Insert(targetIndex, DraggedItem);

                SelectedItem = DraggedItem;
            }

            ResetDragDrop();
        }

        private void ResetDragDrop()
        {
            IsDragging = false;
            DragDropPopup.IsOpen = false;
            this.IsReadOnly = false;
        }


        public bool DataGridRollbackOnUnfocused
        {
            get
            {
                return (bool)GetValue(DataGridRollbackOnUnfocusedProperty);
            }
            set
            {
                SetValue(DataGridRollbackOnUnfocusedProperty, value);
            }
        }

        public static readonly DependencyProperty DataGridRollbackOnUnfocusedProperty =  DependencyProperty.Register("DataGridRollbackOnUnfocused", typeof(bool), typeof(NixxisDataGrid), new UIPropertyMetadata(false, OnDataGridRollbackOnUnfocusedChanged));

        static void OnDataGridRollbackOnUnfocusedChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            DataGrid datagrid = depObj as DataGrid;
            if (datagrid == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                datagrid.LostKeyboardFocus += RollbackDataGridOnLostFocus;
                datagrid.DataContextChanged += RollbackDataGridOnDataContextChanged;
            }
            else
            {
                datagrid.LostKeyboardFocus -= RollbackDataGridOnLostFocus;
                datagrid.DataContextChanged -= RollbackDataGridOnDataContextChanged;
            }
        }

        static void RollbackDataGridOnLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DataGrid senderDatagrid = sender as DataGrid;

            if (senderDatagrid == null)
                return;

            UIElement focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement == null)
                return;

            DataGrid focusedDatagrid = Helpers.FindVisualParent<DataGrid>(focusedElement); //let's see if the new focused element is inside a datagrid
            if (focusedDatagrid == senderDatagrid)
            {
                return;
                //if the new focused element is inside the same datagrid, then we don't need to do anything;
                //this happens, for instance, when we enter in edit-mode: the DataGrid element loses keyboard-focus, which passes to the selected DataGridCell child
            }

            //otherwise, the focus went outside the datagrid; in order to avoid exceptions like ("DeferRefresh' is not allowed during an AddNew or EditItem transaction")
            //or ("CommitNew is not allowed for this view"), we undo the possible pending changes, if any
            IEditableCollectionView collection = senderDatagrid.Items as IEditableCollectionView;

            if (collection.IsEditingItem)
            {
                if (collection.CanCancelEdit)
                    collection.CancelEdit();
                else
                    collection.CommitEdit();
            }
            else if (collection.IsAddingNew)
            {
                collection.CancelNew();
            }
        }


        static void RollbackDataGridOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataGrid senderDatagrid = sender as DataGrid;

            if (senderDatagrid == null)
                return;

            IEditableCollectionView collection = senderDatagrid.Items as IEditableCollectionView;

            if (collection.IsEditingItem)
            {
                collection.CancelEdit();
            }
            else if (collection.IsAddingNew)
            {
                collection.CancelNew();
            }
        }

        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            base.OnPreparingCellForEdit(e);
            System.Diagnostics.Trace.WriteLine("OnPreparingCellForEdit");
        }



        protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
        {
            try
            {
                base.OnSelectedCellsChanged(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }


            // this is needed!!!! Workaround to "ensure" that the DataGridRow could be found
            Helpers.WaitForPriority();

            if (SelectedCells.Count > 0)
            {
                DataGridCell dgc = GetCell(SelectedCells[0]);
                if (dgc != null)
                {

                    DataGridRow dgr = Helpers.FindVisualParent<DataGridRow>(dgc);
                    if (dgr != null)
                    {
                        NixxisDataGrid.SetOnlySelectedActive(dgr);
                    }
                }
            }

        }

        public static DataGridCell GetCell(DataGridCellInfo dataGridCellInfo)
        {
            if (!dataGridCellInfo.IsValid)
            {
                return null;
            }

            var cellContent = dataGridCellInfo.Column.GetCellContent(dataGridCellInfo.Item);
            if (cellContent != null)
            {
                return (DataGridCell)cellContent.Parent;
            }
            else
            {
                return null;
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            try
            {
                // seems to be a problem when selection is outside the collection bounds
                base.OnItemsSourceChanged(oldValue, newValue);
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty( DefaultSortOrder))
            {
                foreach (DataGridColumn col in Columns)
                {
                    if (col is DataGridBoundColumn && ((DataGridBoundColumn)col).Binding is Binding &&  ((Binding)((DataGridBoundColumn)col).Binding).Path.Path.Equals(DefaultSortOrder) )
                    {
                        col.SortDirection = ListSortDirection.Ascending;
                        break;
                    }
                }
                if (ItemsSource == null)
                    return;

                ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
                if (view.SortDescriptions != null)
                {
                    foreach (SortDescription sd in view.SortDescriptions)
                    {
                        if (sd.PropertyName.Equals(DefaultSortOrder) && sd.Direction == ListSortDirection.Ascending)
                            return;
                    }
                    view.SortDescriptions.Add(new SortDescription(DefaultSortOrder, ListSortDirection.Ascending));
                }
            }
        }


    }

    public class NixxisDataGridFooter : NixxisDataGrid
    {
        public static readonly DependencyProperty RelatedGridProperty = DependencyProperty.Register("RelatedGrid", typeof(NixxisDataGrid), typeof(NixxisDataGridFooter));

        public NixxisDataGrid RelatedGrid
        {
            get
            {
                return (NixxisDataGrid)GetValue(RelatedGridProperty);
            }
            set
            {
                SetValue(RelatedGridProperty, value);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            IsSelectable = false;

            BorderThickness = new Thickness(BorderThickness.Left, 0, BorderThickness.Right, BorderThickness.Bottom);

        }
    }


    public class NixxisDataGridToggleDetailColumn : DataGridColumn
    {
        public static readonly DependencyProperty SingleDetailProperty = DependencyProperty.Register("SingleDetail", typeof(bool), typeof(NixxisDataGridToggleDetailColumn), new PropertyMetadata(false));
        public static readonly DependencyProperty ToggleDetailTemplateProperty = DependencyProperty.Register("ToggleDetailTemplate", typeof(ControlTemplate), typeof(NixxisDataGridToggleDetailColumn));

        public bool SingleDetail
        {
            get
            {
                return (bool)GetValue(SingleDetailProperty);
            }
            set
            {
                SetValue(SingleDetailProperty, value);
            }
        }
        public ControlTemplate ToggleDetailTemplate
        {
            get
            {
                return (ControlTemplate)GetValue(ToggleDetailTemplateProperty);
            }
            set
            {
                SetValue(ToggleDetailTemplateProperty, value);
            }
        }

        List<ToggleButton> m_CreatedToggleButtons = new List<ToggleButton>();

        public NixxisDataGridToggleDetailColumn()
        {
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            ToggleButton tbn = new ToggleButton();
            tbn.Name = "DetailsVisibilityToggle";
            if (ToggleDetailTemplate != null)
                tbn.Template = ToggleDetailTemplate;

            tbn.Checked += new RoutedEventHandler(tbn_Checked);
            tbn.Unchecked += new RoutedEventHandler(tbn_Unchecked);

            DependencyObject dep = cell;

            // iteratively traverse the visual tree upwards looking for 
            // the clicked row. 
            while ((dep != null) && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            m_CreatedToggleButtons.Add(tbn);

            return tbn;
        }

        void tbn_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO: check this try catch... Sometimes, an exception is generated from here but I don't know the real reason..
            try
            {
                Helpers.FindVisualParent<DataGridRow>(sender as UIElement).DetailsVisibility = System.Windows.Visibility.Collapsed;
            }
            catch
            {
            }
        }


        void tbn_Checked(object sender, RoutedEventArgs e)
        {

            Helpers.FindVisualParent<DataGridRow>(sender as UIElement).DetailsVisibility = System.Windows.Visibility.Visible;


            if (SingleDetail)
            {
                foreach (ToggleButton tbn in m_CreatedToggleButtons)
                {
                    if (tbn != sender)
                    {
                        tbn.IsChecked = false;
                    }
                }
            }
        }

        private class BoolToDetailsVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if ((bool)value)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class DataGridRowHeaderCommand : System.Windows.Input.ICommand
    {
        DataGridRow _prevRow;
        /// <summary>
        /// Collapse/Expands the selected DataGridRow.
        /// </summary>
        /// <param name="parameter">The DataGridRowHeader</param>
        public void Execute(object parameter)
        {
            var rowHeader = parameter as DataGridRowHeader;
            var row = Helpers.FindTemplatedParent<DataGridRow>(rowHeader) as DataGridRow;
            if (_prevRow is DataGridRow && _prevRow != row  && Helpers.FindVisualParent<DataGrid>(_prevRow) == Helpers.FindVisualParent<DataGrid>(rowHeader))
            {	
                _prevRow.DetailsVisibility = Visibility.Collapsed;
            }
            if (row.DetailsVisibility == Visibility.Visible)
                row.DetailsVisibility = Visibility.Collapsed;
            else
                row.DetailsVisibility = Visibility.Visible;

            _prevRow = row;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    public class NixxisDataGridPasswordColumn : DataGridBoundColumn
    {
        static NixxisDataGridPasswordColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridPasswordColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridPasswordColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
        }

        #region Styles

        /// <summary>
        ///     The default value of the ElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(TextBlock));

                    // When not in edit mode, the end-user should not be able to toggle the state
                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
                    style.Setters.Add(new Setter(UIElement.FocusableProperty, false));

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        /// <summary>
        ///     The default value of the EditingElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(PasswordBox));

                    style.Seal();
                    _defaultEditingElementStyle = style;
                }

                return _defaultEditingElementStyle;
            }
        }

        #endregion


        public RoutedEventHandler PasswordChanged { get; set; }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            PasswordBox cb = new PasswordBox();

            ApplyStyle(true, /* defaultToElementStyle = */ true, cb);

            cb.PasswordChanged += new RoutedEventHandler(cb_PasswordChanged);
            return cb;
        }

        void cb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordChanged != null)
                PasswordChanged(sender, e);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            TextBlock cb = new TextBlock();

            ApplyStyle(false, /* defaultToElementStyle = */ true, cb);

            cb.Text = "****";
            return cb;
        }

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing, bool defaultToElementStyle)
        {
            Style style = isEditing ? EditingElementStyle : ElementStyle;
            if (isEditing && defaultToElementStyle && (style == null))
            {
                style = ElementStyle;
            }

            return style;
        }


        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            editingElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        #region Data

        private static Style _defaultElementStyle;
        private static Style _defaultEditingElementStyle;

        #endregion
    }


    public class NixxisDataGridSliderColumn : DataGridBoundColumn
    {
        public BindingBase EnabledBinding { get; set; }

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SliderWidth { get; set; }
        public AutoToolTipPlacement AutoToolTipPlacement { get; set; }

        static NixxisDataGridSliderColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridSliderColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridSliderColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
        }

        #region Styles

        /// <summary>
        ///     The default value of the ElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(Slider));

                    // When not in edit mode, the end-user should not be able to toggle the state
                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
                    style.Setters.Add(new Setter(UIElement.FocusableProperty, false));

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        /// <summary>
        ///     The default value of the EditingElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(Slider));

                    style.Seal();
                    _defaultEditingElementStyle = style;
                }

                return _defaultEditingElementStyle;
            }
        }

        #endregion

        #region Element Generation

        /// <summary>
        ///     Creates the visual tree for boolean based cells.
        /// </summary>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return GenerateSlider(/* isEditing = */ false, cell);
        }

        /// <summary>
        ///     Creates the visual tree for boolean based cells.
        /// </summary>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateSlider(/* isEditing = */ true, cell);
        }

        private Slider GenerateSlider(bool isEditing, DataGridCell cell)
        {
            Slider slider = (cell != null) ? (cell.Content as Slider) : null;
            if (slider == null)
            {
                slider = new Slider();
                slider.AutoToolTipPlacement = AutoToolTipPlacement;
                slider.Width = SliderWidth;
                slider.Minimum = Minimum;
                slider.Maximum = Maximum;
                slider.IsSnapToTickEnabled = true;
                slider.TickFrequency = 1;
            }

            ApplyStyle(isEditing, /* defaultToElementStyle = */ true, slider);
            ApplyBinding(Binding, slider, Slider.ValueProperty);
            ApplyBinding(EnabledBinding, slider, Slider.IsEnabledProperty);

            return slider;
        }

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing, bool defaultToElementStyle)
        {
            Style style = isEditing ? EditingElementStyle : ElementStyle;
            if (isEditing && defaultToElementStyle && (style == null))
            {
                style = ElementStyle;
            }

            return style;
        }


        internal void ApplyBinding(BindingBase binding,  DependencyObject target, DependencyProperty property)
        {
            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);
                
            }
            else
            {
                BindingOperations.ClearBinding(target, property);
            }

        }



        #endregion

        #region Editing



        /// <summary>
        ///     Called when a cell has just switched to edit mode.
        /// </summary>
        /// <param name="editingElement">A reference to element returned by GenerateEditingElement.</param>
        /// <param name="editingEventArgs">The event args of the input event that caused the cell to go into edit mode. May be null.</param>
        /// <returns>The unedited value of the cell.</returns>
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {

            Slider slider = editingElement as Slider;
            if (slider != null)
            {
                slider.Focus();
                double uneditedValue = slider.Value;

                // If a click or a space key invoked the begin edit, then do an immediate toggle
                if (IsMouseLeftButtonDown(editingEventArgs) && IsMouseOver(slider, editingEventArgs))
                {
                    slider.ApplyTemplate();
                    Thumb thumb = (slider.Template.FindName("PART_Track", slider) as Track).Thumb;
                    MouseButtonEventArgs args = editingEventArgs as MouseButtonEventArgs;
                    thumb.RaiseEvent(args);
                }
                return uneditedValue;
            }

            return 0;
        }

        /// <summary>
        ///     Called when a cell's value is to be committed, just before it exits edit mode.
        /// </summary>
        /// <param name="editingElement">A reference to element returned by GenerateEditingElement.</param>
        /// <returns>false if there is a validation error. true otherwise.</returns>
        protected override bool CommitCellEdit(FrameworkElement editingElement)
        {
            Slider slider = editingElement as Slider;
            if (slider != null)
            {
                UpdateSource(slider, Slider.ValueProperty);
                return !Validation.GetHasError(slider);
            }

            return true;
        }

        /// <summary>
        ///     Called when a cell's value is to be cancelled, just before it exits edit mode.
        /// </summary>
        /// <param name="editingElement">A reference to element returned by GenerateEditingElement.</param>
        /// <param name="uneditedValue">UneditedValue</param>
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            Slider slider = editingElement as Slider;
            if (slider != null)
            {
                UpdateTarget(slider, Slider.ValueProperty);
            }
        }

        internal static void UpdateSource(FrameworkElement element, DependencyProperty dp)
        {
            BindingExpression binding = GetBindingExpression(element, dp);
            if (binding != null)
            {
                binding.UpdateSource();
            }
        }

        internal static void UpdateTarget(FrameworkElement element, DependencyProperty dp)
        {
            BindingExpression binding = GetBindingExpression(element, dp);
            if (binding != null)
            {
                binding.UpdateTarget();
            }
        }

        internal static BindingExpression GetBindingExpression(FrameworkElement element, DependencyProperty dp)
        {
            if (element != null)
            {
                return element.GetBindingExpression(dp);
            }

            return null;
        }


        private static bool IsMouseLeftButtonDown(RoutedEventArgs e)
        {
            MouseButtonEventArgs mouseArgs = e as MouseButtonEventArgs;
            return (mouseArgs != null) &&
                   (mouseArgs.ChangedButton == MouseButton.Left) &&
                   (mouseArgs.ButtonState == MouseButtonState.Pressed);
        }

        private static bool IsMouseOver(Slider slider, RoutedEventArgs e)
        {
            // This element is new, so the IsMouseOver property will not have been updated
            // yet, but there is enough information to do a hit-test.
            return slider.InputHitTest(((MouseButtonEventArgs)e).GetPosition(slider)) != null;
        }

        private static bool IsSpaceKeyDown(RoutedEventArgs e)
        {
            KeyEventArgs keyArgs = e as KeyEventArgs;
            return (keyArgs != null) &&
                   ((keyArgs.KeyStates & KeyStates.Down) == KeyStates.Down) &&
                   (keyArgs.Key == Key.Space);
        }

        #endregion

        #region Data

        private static Style _defaultElementStyle;
        private static Style _defaultEditingElementStyle;

        #endregion
    }

    public class NixxisDataGridComboBoxColumn : DataGridComboBoxColumn
    {
        public BindingBase EnabledBinding { get; set; }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            FrameworkElement fe = base.GenerateElement(cell, dataItem);
            if(EnabledBinding!=null && BindingOperations.GetBinding(fe, FrameworkElement.IsEnabledProperty)==null)
                BindingOperations.SetBinding(fe, FrameworkElement.IsEnabledProperty, EnabledBinding);
            return fe;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            FrameworkElement fe = base.GenerateEditingElement(cell, dataItem);

            if (EnabledBinding != null && BindingOperations.GetBinding(fe, FrameworkElement.IsEnabledProperty) == null)
                BindingOperations.SetBinding(fe, FrameworkElement.IsEnabledProperty, EnabledBinding);
            return fe;
        }

        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            ComboBox cbo = editingElement as ComboBox;
            if (cbo != null && IsMouseLeftButtonDown(editingEventArgs) && IsMouseOver(cbo, editingEventArgs))
                cbo.IsDropDownOpen = true;
            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        private static bool IsMouseLeftButtonDown(RoutedEventArgs e)
        {
            MouseButtonEventArgs mouseArgs = e as MouseButtonEventArgs;
            return (mouseArgs != null) &&
                   (mouseArgs.ChangedButton == MouseButton.Left) &&
                   (mouseArgs.ButtonState == MouseButtonState.Pressed);
        }

        private static bool IsMouseOver(ComboBox cbo, RoutedEventArgs e)
        {
            // This element is new, so the IsMouseOver property will not have been updated
            // yet, but there is enough information to do a hit-test.
            return cbo.InputHitTest(((MouseButtonEventArgs)e).GetPosition(cbo)) != null;
        }
    }

    public class NixxisDataGridCheckBoxColumn : DataGridCheckBoxColumn
    {
        public BindingBase EnabledBinding { get; set; }

        public BindingBase VisibilityBinding { get; set; }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            FrameworkElement fe = base.GenerateElement(cell, dataItem);
            if (EnabledBinding != null && BindingOperations.GetBinding(fe, FrameworkElement.IsEnabledProperty) == null)
                BindingOperations.SetBinding(fe, FrameworkElement.IsEnabledProperty, EnabledBinding);

            if (VisibilityBinding != null && BindingOperations.GetBinding(fe, FrameworkElement.IsVisibleProperty) == null)
                BindingOperations.SetBinding(fe, FrameworkElement.VisibilityProperty, VisibilityBinding);

            return fe;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            FrameworkElement fe = base.GenerateEditingElement(cell, dataItem);            
            if (EnabledBinding != null && BindingOperations.GetBinding(fe, FrameworkElement.IsEnabledProperty) == null)
                BindingOperations.SetBinding(fe, FrameworkElement.IsEnabledProperty, EnabledBinding);
            if (VisibilityBinding != null && BindingOperations.GetBinding(fe, FrameworkElement.VisibilityProperty) == null)
                BindingOperations.SetBinding(fe, FrameworkElement.VisibilityProperty, VisibilityBinding);
            return fe;
        }
    }

    public class NixxisDataGridTextColumn : DataGridBoundColumn
    {
        public static readonly DependencyProperty InternalUseAlternateBindingProperty = DependencyProperty.RegisterAttached("InternalUseAlternateBinding", typeof(bool), typeof(NixxisDataGridTextColumn), new PropertyMetadata(false, new PropertyChangedCallback(InternalUseAlternateBindingChanged)));

        public static void InternalUseAlternateBindingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BindingBase bnd = null;
            BindingBase altbnd = null;


            if(!(bool)args.OldValue)
            {
                if (obj is TextBox)
                {
                    TextBox tb = (TextBox)obj;

                    bnd = BindingOperations.GetBindingBase(tb, TextBox.TextProperty);
                    altbnd = BindingOperations.GetBindingBase(tb, TextBox.TagProperty);
                }
                else if(obj is TextBlock)
                {
                    TextBlock tb = (TextBlock)obj;

                    bnd = BindingOperations.GetBindingBase(tb, TextBlock.TextProperty);
                    altbnd = BindingOperations.GetBindingBase(tb, TextBlock.TagProperty);
                }
            }
            else
            {
                if (obj is TextBox)
                {
                    TextBox tb = (TextBox)obj;

                    altbnd = BindingOperations.GetBindingBase(tb, TextBox.TextProperty);
                    bnd = BindingOperations.GetBindingBase(tb, TextBox.TagProperty);
                }
                else if (obj is TextBlock)
                {
                    TextBlock tb = (TextBlock)obj;

                    altbnd = BindingOperations.GetBindingBase(tb, TextBlock.TextProperty);
                    bnd = BindingOperations.GetBindingBase(tb, TextBlock.TagProperty);
                }
            }

            if ((bool)args.NewValue)
            {
                if (obj is TextBox)
                {
                    TextBox tb = (TextBox)obj;

                    BindingOperations.SetBinding(tb, TextBox.TextProperty, altbnd);
                    BindingOperations.SetBinding(tb, TextBox.TagProperty, bnd);
                }
                else if(obj is TextBlock)
                {
                    TextBlock tb = (TextBlock)obj;

                    BindingOperations.SetBinding(tb, TextBlock.TextProperty, altbnd);
                    BindingOperations.SetBinding(tb, TextBlock.TagProperty, bnd);
                }
            }
            else
            {
                if (obj is TextBox)
                {
                    TextBox tb = (TextBox)obj;

                    BindingOperations.SetBinding(tb, TextBox.TextProperty, bnd);
                    BindingOperations.SetBinding(tb, TextBox.TagProperty, altbnd);
                }
                else if (obj is TextBlock)
                {
                    TextBlock tb = (TextBlock)obj;

                    BindingOperations.SetBinding(tb, TextBlock.TextProperty, bnd);
                    BindingOperations.SetBinding(tb, TextBlock.TagProperty, altbnd);
                }
            }
        }

        public static void SetInternalUseAlternateBinding(UIElement element, object value)
        {
            element.SetValue(InternalUseAlternateBindingProperty, value);
        }
        public static object GetInternalUseAlternateBinding(UIElement element)
        {
            return (element.GetValue(InternalUseAlternateBindingProperty));
        }


        private BindingBase m_AlternateBinding;
        private BindingBase m_UseAlternateBinding;
        private BindingBase m_IsNotEditableBinding;

        public BindingBase IsNotEditableBinding
        {
            get
            {
                return m_IsNotEditableBinding;
            }
            set
            {
                m_IsNotEditableBinding = value;
            }
        }
        public BindingBase AlternateBinding
        {
            get
            {
                return m_AlternateBinding;
            }
            set
            {
                m_AlternateBinding = value;
            }
        }
        public BindingBase UseAlternateBinding
        {
            get
            {
                return m_UseAlternateBinding;
            }
            set
            {
                m_UseAlternateBinding = value;
            }
        }

        static NixxisDataGridTextColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridTextColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridTextColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
        }

        #region Styles

        /// <summary>
        ///     The default value of the ElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(TextBlock));

                    // When not in edit mode, the end-user should not be able to toggle the state
                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
                    style.Setters.Add(new Setter(UIElement.FocusableProperty, false));

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        /// <summary>
        ///     The default value of the EditingElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(TextBox));
                    style.Seal();
                    _defaultEditingElementStyle = style;
                }

                return _defaultEditingElementStyle;
            }
        }

        #endregion


        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            TextBox cb = new TextBox();

            ApplyStyle(true, /* defaultToElementStyle = */ true, cb);
            BindingOperations.SetBinding(cb, NixxisDataGridTextColumn.InternalUseAlternateBindingProperty, m_UseAlternateBinding);
            BindingOperations.SetBinding(cb, TextBox.TextProperty, Binding);
            BindingOperations.SetBinding(cb, TextBox.TagProperty, AlternateBinding);
            BindingOperations.SetBinding(cb, TextBox.IsReadOnlyProperty, m_IsNotEditableBinding);
            return cb;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            TextBlock cb = new TextBlock();

            ApplyStyle(false, /* defaultToElementStyle = */ true, cb);
            BindingOperations.SetBinding(cb, NixxisDataGridTextColumn.InternalUseAlternateBindingProperty, m_UseAlternateBinding);
            BindingOperations.SetBinding(cb, TextBlock.TextProperty, Binding);
            BindingOperations.SetBinding(cb, TextBlock.TagProperty, AlternateBinding);
            return cb;
        }

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing, bool defaultToElementStyle)
        {
            Style style = isEditing ? EditingElementStyle : ElementStyle;
            if (isEditing && defaultToElementStyle && (style == null))
            {
                style = ElementStyle;
            }

            return style;
        }

        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {                        
            editingElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        #region Data

        private static Style _defaultElementStyle;
        private static Style _defaultEditingElementStyle;

        #endregion
    }

    public class NixxisDataGridDoubleSliderColumn : DataGridBoundColumn
    {
        public BindingBase EnabledBinding { get; set; }

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double SliderWidth { get; set; }

        static NixxisDataGridDoubleSliderColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridDoubleSliderColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(NixxisDataGridDoubleSliderColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
        }

        #region Styles

        /// <summary>
        ///     The default value of the ElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(NixxisDoubleSlider));

                    // When not in edit mode, the end-user should not be able to toggle the state
                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
                    style.Setters.Add(new Setter(UIElement.FocusableProperty, false));

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        /// <summary>
        ///     The default value of the EditingElementStyle property.
        ///     This value can be used as the BasedOn for new styles.
        /// </summary>
        public static Style DefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(NixxisDoubleSlider));

                    style.Seal();
                    _defaultEditingElementStyle = style;
                }

                return _defaultEditingElementStyle;
            }
        }

        #endregion

        #region Element Generation

        /// <summary>
        ///     Creates the visual tree for boolean based cells.
        /// </summary>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return GenerateSlider(/* isEditing = */ false, cell);
        }

        /// <summary>
        ///     Creates the visual tree for boolean based cells.
        /// </summary>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateSlider(/* isEditing = */ true, cell);
        }

        private NixxisDoubleSlider GenerateSlider(bool isEditing, DataGridCell cell)
        {
            NixxisDoubleSlider slider = (cell != null) ? (cell.Content as NixxisDoubleSlider) : null;
            if (slider == null)
            {
                slider = new NixxisDoubleSlider();
                slider.Width = SliderWidth;
                slider.Minimum = Minimum;
                slider.Maximum = Maximum;
            }


            ApplyStyle(isEditing, /* defaultToElementStyle = */ true, slider);
            ApplyBinding(Binding, slider, NixxisDoubleSlider.ValueProperty);
            ApplyBinding(EnabledBinding, slider, NixxisDoubleSlider.IsEnabledProperty);

            return slider;
        }

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing, bool defaultToElementStyle)
        {
            if (isEditing)
            {
                return DefaultEditingElementStyle;
            }
            else
            {
                return DefaultElementStyle;
            }

        }


        internal void ApplyBinding(BindingBase binding, DependencyObject target, DependencyProperty property)
        {
            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);

            }
            else
            {
                BindingOperations.ClearBinding(target, property);
            }

        }



        #endregion

        #region Editing



        /// <summary>
        ///     Called when a cell has just switched to edit mode.
        /// </summary>
        /// <param name="editingElement">A reference to element returned by GenerateEditingElement.</param>
        /// <param name="editingEventArgs">The event args of the input event that caused the cell to go into edit mode. May be null.</param>
        /// <returns>The unedited value of the cell.</returns>
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {

            NixxisDoubleSlider slider = editingElement as NixxisDoubleSlider;
            if (slider != null)
            {
                slider.Focus();
                DoubleRange uneditedValue = slider.Value;

                // If a click or a space key invoked the begin edit, then do an immediate toggle
                if (IsMouseLeftButtonDown(editingEventArgs) && IsMouseOver(slider.UpperSlider, editingEventArgs))
                {
                    slider.ApplyTemplate();
                    slider.UpperSlider.ApplyTemplate();

                    Thumb thumb = (slider.UpperSlider.Template.FindName("PART_Track", slider.UpperSlider) as Track).Thumb;
                    MouseButtonEventArgs args = editingEventArgs as MouseButtonEventArgs;
                    thumb.RaiseEvent(args);
                }
                else if (IsMouseLeftButtonDown(editingEventArgs) && IsMouseOver(slider.LowerSlider, editingEventArgs))
                {
                    slider.ApplyTemplate();
                    slider.LowerSlider.ApplyTemplate();

                    Thumb thumb = (slider.LowerSlider.Template.FindName("PART_Track", slider.LowerSlider) as Track).Thumb;
                    MouseButtonEventArgs args = editingEventArgs as MouseButtonEventArgs;
                    thumb.RaiseEvent(args);
                }
                else
                return uneditedValue;
            }

            return new DoubleRange() ;
        }

        /// <summary>
        ///     Called when a cell's value is to be committed, just before it exits edit mode.
        /// </summary>
        /// <param name="editingElement">A reference to element returned by GenerateEditingElement.</param>
        /// <returns>false if there is a validation error. true otherwise.</returns>
        protected override bool CommitCellEdit(FrameworkElement editingElement)
        {
            NixxisDoubleSlider slider = editingElement as NixxisDoubleSlider;
            if (slider != null)
            {
                UpdateSource(slider, NixxisDoubleSlider.ValueProperty);
                return !Validation.GetHasError(slider);
            }

            return true;
        }

        /// <summary>
        ///     Called when a cell's value is to be cancelled, just before it exits edit mode.
        /// </summary>
        /// <param name="editingElement">A reference to element returned by GenerateEditingElement.</param>
        /// <param name="uneditedValue">UneditedValue</param>
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            NixxisDoubleSlider slider = editingElement as NixxisDoubleSlider;
            if (slider != null)
            {
                UpdateTarget(slider, NixxisDoubleSlider.ValueProperty);
            }
        }

        internal static void UpdateSource(FrameworkElement element, DependencyProperty dp)
        {
            BindingExpression binding = GetBindingExpression(element, dp);
            if (binding != null)
            {
                binding.UpdateSource();
            }
        }

        internal static void UpdateTarget(FrameworkElement element, DependencyProperty dp)
        {
            BindingExpression binding = GetBindingExpression(element, dp);
            if (binding != null)
            {
                binding.UpdateTarget();
            }
        }

        internal static BindingExpression GetBindingExpression(FrameworkElement element, DependencyProperty dp)
        {
            if (element != null)
            {
                return element.GetBindingExpression(dp);
            }

            return null;
        }


        
        private static bool IsMouseLeftButtonDown(RoutedEventArgs e)
        {
            MouseButtonEventArgs mouseArgs = e as MouseButtonEventArgs;
            return (mouseArgs != null) &&
                   (mouseArgs.ChangedButton == MouseButton.Left) &&
                   (mouseArgs.ButtonState == MouseButtonState.Pressed);
        }

        private static bool IsMouseOver(Slider slider, RoutedEventArgs e)
        {
            // This element is new, so the IsMouseOver property will not have been updated
            // yet, but there is enough information to do a hit-test.
            return slider.InputHitTest(((MouseButtonEventArgs)e).GetPosition(slider)) != null;
        }

        private static bool IsSpaceKeyDown(RoutedEventArgs e)
        {
            KeyEventArgs keyArgs = e as KeyEventArgs;
            return (keyArgs != null) &&
                   ((keyArgs.KeyStates & KeyStates.Down) == KeyStates.Down) &&
                   (keyArgs.Key == Key.Space);
        }

        #endregion

        #region Data

        private static Style _defaultElementStyle;
        private static Style _defaultEditingElementStyle;

        #endregion
    }
}

