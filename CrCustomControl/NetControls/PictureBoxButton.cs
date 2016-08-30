using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Nixxis.Windows.Controls
{
    public partial class PictureBoxButton : PictureBox
    {
        #region Class data
        private bool _Focused = true;
        private Image _Hover = null;
        private Image _Focus = null;
        private Image _NoFocus = null;
        #endregion

        #region Events
        protected override void OnClick(EventArgs e)
        {
            this.Image = _Focus;
            _Focused = true;
            base.OnClick(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            if (!_Focused) this.Image = _Hover;
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_Focused) this.Image = _NoFocus;
            base.OnMouseLeave(e);
        }
        #endregion

        #region Properties
        [Category("Behavior")]
        [Localizable(false)]
        public bool Active
        {
            get { return _Focused; }
            set { _Focused = value; SetFocus(value); }
        }
        [Category("Appearance")]
        [Localizable(false)]
        public Image HoverImage
        {
            get { return _Hover; }
            set { _Hover = value; }
        }
        [Category("Appearance")]
        [Localizable(false)]
        public Image FocusImage
        {
            get { return _Focus; }
            set { _Focus = value; }
        }
        [Category("Appearance")]
        [Localizable(false)]
        public Image NoFocusImage
        {
            get { return _NoFocus; }
            set { _NoFocus = value; }
        }
        #endregion

        #region Members
        public void SetFocus(bool value)
        {
            _Focused = value;
            if (_Focused)
                this.Image = _Focus;
            else
                this.Image = _NoFocus;
        }
        public void ToggleFocus()
        {
            this.SetFocus(!_Focused);
        }
        private void OnMouseEnter_Event(object sender, EventArgs e)
        {
        }
        private void OnMouseLeave_Event(object sender, EventArgs e)
        {
        }
        private void OnMouseClick_Event(object sender, EventArgs e)
        {
        }
        #endregion
    }
}
