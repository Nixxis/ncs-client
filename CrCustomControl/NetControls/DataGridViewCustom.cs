using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace Nixxis.Windows.Controls
{
    public class DataGridViewCustom : DataGridView
    {
        private object _RowFixBottomTagId = null;

        [Category("Behavior")]
        [Localizable(false)]
        public object RowFixBottomTagId
        {
            get { return _RowFixBottomTagId; }
            set { _RowFixBottomTagId = value; }
        }

        public override void Sort(DataGridViewColumn dataGridViewColumn, System.ComponentModel.ListSortDirection direction)
        {
            DataGridViewRow backup = null; //= this.Rows[this.Rows.Count - 1];
            
            if (_RowFixBottomTagId != null)
            {
                foreach (DataGridViewRow item in Rows)
                {
                    if (item.Tag.Equals(_RowFixBottomTagId))
                    {
                        backup = item;
                        break;
                    }
                }
            }
            if (backup != null) this.Rows.Remove(backup);
            
            //this.Rows.RemoveAt(6);

            base.Sort(dataGridViewColumn, direction);

            if (backup != null) this.Rows.Add(backup);
        }
    }
}
