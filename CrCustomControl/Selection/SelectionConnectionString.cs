using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Nixxis.Windows.Controls
{
    public partial class SelectionConnectionString : UserControl
    {
        #region Enums
        public enum InputModeConnectionString
        {
            SingleBox,
            MultiBox,
        }
        #endregion

        #region Class data
        private bool _IsConnectionStringVisible = true;
        private bool _IsButtonSetVisible = true;
        private bool _IsUserIdVisible = true;
        private bool _IsPasswordVisible = true;
        private bool _IsInitCatVisible = true;
        private bool _IsDataSourceVisible = true;
        private bool _IsOptionVisible = true;
        private InputModeConnectionString _InputMode = InputModeConnectionString.MultiBox;
        private DatabasetringObject _ConnectionString = new DatabasetringObject();
        #endregion

        #region Constructors
        public SelectionConnectionString()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        public bool IsConnectionStringVisible
        {
            get { return _IsConnectionStringVisible; }
            set { _IsConnectionStringVisible = value; SetConnectionStringVisibility(); }
        }
        public bool IsButtonSetVisible
        {
            get { return _IsButtonSetVisible; }
            set { _IsButtonSetVisible = value; SetButtonSetVisibility(); }
        }
        public bool IsUserIdVisible
        {
            get { return _IsUserIdVisible; }
            set { _IsUserIdVisible = value; SetsUserIdVisibility(); }
        }
        public bool IsPasswordVisible
        {
            get { return _IsPasswordVisible; }
            set { _IsPasswordVisible = value; SetPasswordVisibility(); }
        }
        public bool IsInitialCatalogVisible
        {
            get { return _IsInitCatVisible; }
            set { _IsInitCatVisible = value; SetInitialCatalogVisibility(); }
        }
        public bool IsDataSourceVisible
        {
            get { return _IsDataSourceVisible; }
            set { _IsDataSourceVisible = value; SetDataSourceVisibility(); }
        }
        public bool IsOptionVisible
        {
            get { return _IsOptionVisible; }
            set { _IsOptionVisible = value; SetOptionVisibility(); }
        }
        public InputModeConnectionString InputMode
        {
            get { return _InputMode; }
            set { _InputMode = value; SetInputMode(); }
        }

        public string ConnectionString
        {
            get { return _ConnectionString.ConnectionString; }
            set { _ConnectionString = new DatabasetringObject(value); DisplayConnectionString(); }
        }
        public string ConnectionStringFull
        {
            get 
            {
                if (_InputMode == InputModeConnectionString.MultiBox)
                    return _ConnectionString.ConnectionStringFull;
                else
                    return _ConnectionString.ConnectionStringRaw;
            }
            set 
            {
                string tmp = value;
                if (tmp == null) tmp = "";
                _ConnectionString = new DatabasetringObject(tmp); 
                DisplayConnectionString(); 
            }
        }
        #endregion

        #region Members
        private void SetConnectionStringVisibility()
        {
            if (_IsConnectionStringVisible || _InputMode == InputModeConnectionString.SingleBox)
            {
                tableLayoutPanel1.RowStyles[5].Height = 20F;
                tableLayoutPanel1.RowStyles[6].SizeType = SizeType.Percent;
                tableLayoutPanel1.RowStyles[6].Height = 100F;
                tableLayoutPanel1.RowStyles[7].Height = 20F;
            }
            else
            {
                tableLayoutPanel1.RowStyles[5].Height = 0F;
                tableLayoutPanel1.RowStyles[6].SizeType = SizeType.Absolute;
                tableLayoutPanel1.RowStyles[6].Height = 0F;
                tableLayoutPanel1.RowStyles[7].Height = 0F;
            }
        }
        private void SetButtonSetVisibility()
        {
            if (_IsButtonSetVisible && _InputMode == InputModeConnectionString.MultiBox)
            {
                btnSet.Visible = true;
                tableLayoutPanel1.RowStyles[7].Height = 20F;
            }
            else
            {
                btnSet.Visible = false;
                tableLayoutPanel1.RowStyles[7].Height = 0F;
            }
        }
        private void SetsUserIdVisibility()
        {
            if (_IsUserIdVisible && _InputMode == InputModeConnectionString.MultiBox)
            {
                tableLayoutPanel1.RowStyles[0].Height = 20F;
                txtUserId.Visible = true;
            }
            else
            {
                tableLayoutPanel1.RowStyles[0].Height = 0F;
                txtUserId.Visible = false;
            }
        }
        private void SetPasswordVisibility()
        {
            if (_IsPasswordVisible && _InputMode == InputModeConnectionString.MultiBox)
            {
                tableLayoutPanel1.RowStyles[1].Height = 20F;
                txtPassword.Visible = true;
            }
            else
            {
                tableLayoutPanel1.RowStyles[1].Height = 0F;
                txtPassword.Visible = false;
            }
        }
        private void SetDataSourceVisibility()
        {
            if (_IsDataSourceVisible && _InputMode == InputModeConnectionString.MultiBox)
            {
                tableLayoutPanel1.RowStyles[2].Height = 20F;
                txtDataSource.Visible = true;
            }
            else
            {
                tableLayoutPanel1.RowStyles[2].Height = 0F;
                txtDataSource.Visible = false;
            }
        }
        private void SetInitialCatalogVisibility()
        {
            if (_IsInitCatVisible && _InputMode == InputModeConnectionString.MultiBox)
            {
                tableLayoutPanel1.RowStyles[3].Height = 20F;
                txtInitialCatalog.Visible = true;
            }
            else
            {
                tableLayoutPanel1.RowStyles[3].Height = 0F;
                txtInitialCatalog.Visible = false;
            }
        }
        private void SetOptionVisibility()
        {
            if (_IsOptionVisible && _InputMode == InputModeConnectionString.MultiBox)
            {
                tableLayoutPanel1.RowStyles[4].Height = 20F;
                txtOptions.Visible = true;
            }
            else
            {
                tableLayoutPanel1.RowStyles[4].Height = 0F;
                txtOptions.Visible = false;
            }
        }
        private void SetInputMode()
        {
            SetConnectionStringVisibility();
            SetButtonSetVisibility();
            SetsUserIdVisibility();
            SetPasswordVisibility();
            SetDataSourceVisibility();
            SetInitialCatalogVisibility();
            SetOptionVisibility();
        }
        private void DisplayConnectionString()
        {
            if (_InputMode == InputModeConnectionString.MultiBox)
                txtConnectionString.Text = _ConnectionString.ConnectionString;
            else
                txtConnectionString.Text = _ConnectionString.ConnectionStringRaw;

            txtUserId.Text = _ConnectionString.UserId;
            txtPassword.Text = _ConnectionString.Password;
            txtInitialCatalog.Text = _ConnectionString.InitialCatalog;
            txtDataSource.Text = _ConnectionString.DataSource;
        }
        #endregion

        private void btnSet_Click(object sender, EventArgs e)
        {
            _ConnectionString = new DatabasetringObject(txtConnectionString.Text);
            DisplayConnectionString();
        }

        private void txtUserId_TextChanged(object sender, EventArgs e)
        {
            _ConnectionString.UserId = txtUserId.Text;
            if(_InputMode == InputModeConnectionString.MultiBox)
                txtConnectionString.Text = _ConnectionString.ConnectionString;
            else
                txtConnectionString.Text = _ConnectionString.ConnectionStringRaw;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            _ConnectionString.Password = txtPassword.Text;
            if (_InputMode == InputModeConnectionString.MultiBox)
                txtConnectionString.Text = _ConnectionString.ConnectionString;
            else
                txtConnectionString.Text = _ConnectionString.ConnectionStringRaw;
        }

        private void txtDataSource_TextChanged(object sender, EventArgs e)
        {
            _ConnectionString.DataSource = txtDataSource.Text;
            if (_InputMode == InputModeConnectionString.MultiBox)
                txtConnectionString.Text = _ConnectionString.ConnectionString;
            else
                txtConnectionString.Text = _ConnectionString.ConnectionStringRaw;
        }

        private void txtInitialCatalog_TextChanged(object sender, EventArgs e)
        {
            _ConnectionString.InitialCatalog = txtInitialCatalog.Text;
            if (_InputMode == InputModeConnectionString.MultiBox)
                txtConnectionString.Text = _ConnectionString.ConnectionString;
            else
                txtConnectionString.Text = _ConnectionString.ConnectionStringRaw;
        }

        private void txtOptions_TextChanged(object sender, EventArgs e)
        {
            _ConnectionString.Options = txtOptions.Text;
            if (_InputMode == InputModeConnectionString.MultiBox)
                txtConnectionString.Text = _ConnectionString.ConnectionString;
            else
                txtConnectionString.Text = _ConnectionString.ConnectionStringRaw;
        }

        private void txtConnectionString_TextChanged(object sender, EventArgs e)
        {
            if(_InputMode == InputModeConnectionString.SingleBox)
                _ConnectionString.ConnectionStringRaw = txtConnectionString.Text;
        }
    }

    internal class DatabasetringObject
    {
        #region Class data
        private string _ConnectionString;

        private string _UserId;
        private string _Password;
        private string _InitialCatalog;
        private string _DataSource;
        private string _Options;
        private string _IntegratedSecurity;
        private string _PersistSecurityInfo;

        private const string _UserIdString = "USER ID";
        private const string _PasswordString = "PASSWORD";
        private const string _InitialCatalogString = "INITIAL CATALOG";
        private const string _DataSourceString = "DATA SOURCE";
        private const string _IntegratedSecurityString = "INTEGRATED SECURITY";
        private const string _PersistSecurityInfoString = "PERSIST SECURITY INFO";
        #endregion

        #region Constructors
        public DatabasetringObject()
        {
        }
        public DatabasetringObject(string connectionString)
        {
            _ConnectionString = connectionString;
            SetConnectionString(connectionString);
        }
        public DatabasetringObject(string userId, string password, string initialCatalog, string dataSource)
        {
            _UserId = userId;
            _Password = password;
            _InitialCatalog = initialCatalog;
            _DataSource = dataSource;
            _ConnectionString = GetConnectionString();
        }
        #endregion

        #region Properties
        public string ConnectionString
        {
            get { return GetConnectionString(); }
            set { _ConnectionString = value; SetConnectionString(value); }
        }
        public string ConnectionStringFull
        {
            get { return GetConnectionStringFull(); }
            set { _ConnectionString = value; SetConnectionString(value); }
        }
        public string ConnectionStringRaw
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; SetConnectionString(value); }
        }
        public string UserId
        {
            get { return _UserId; }
            set { _UserId = value; }
        }
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }
        public string InitialCatalog
        {
            get { return _InitialCatalog; }
            set { _InitialCatalog = value; }
        }
        public string DataSource
        {
            get { return _DataSource; }
            set { _DataSource = value; }
        }
        public string Options
        {
            get { return _Options; }
            set { _Options = value; }
        }
        #endregion

        #region Members
        public void SetConnectionString(string connectionstring)
        {
            string[] list = connectionstring.Split(new char[] { ';' });
            ClearVar();
            foreach (string item in list)
            {
                int pos = item.IndexOf('=');
                if (pos > 0)
                {
                    string property = item.Substring(0, pos);
                    switch (property.ToUpper())
                    {
                        case _UserIdString:
                            _UserId = item.Substring(pos + 1);
                            break;
                        case _PasswordString:
                            _Password = item.Substring(pos + 1);
                            break;
                        case _InitialCatalogString:
                            _InitialCatalog = item.Substring(pos + 1);
                            break;
                        case _DataSourceString:
                            _DataSource = item.Substring(pos + 1);
                            break;
                        case _IntegratedSecurityString:
                            _IntegratedSecurity = item.Substring(pos + 1);
                            break;
                        case _PersistSecurityInfoString:
                            _PersistSecurityInfo = item.Substring(pos + 1);
                            break;
                        default:
                            _Options += item.Substring(pos + 1);
                            break;
                    }
                }
            }
        }
        public string GetConnectionString()
        {
            string returnValue = _UserIdString + "=" + _UserId + ";" + _PasswordString + "=********;" + _DataSourceString + "=" + _DataSource + ";" + _InitialCatalogString + "=" + _InitialCatalog;
            if (!string.IsNullOrEmpty(_Options)) returnValue += ";" + _Options;

            return returnValue;
        }
        public string GetConnectionStringFull()
        {
            string returnValue = _UserIdString + "=" + _UserId + ";" + _PasswordString + "=" + _Password + ";" + _DataSourceString + "=" + _DataSource + ";" + _InitialCatalogString + "=" + _InitialCatalog;
            if (!string.IsNullOrEmpty(_Options)) returnValue += ";" + _Options;

            return returnValue;
        }

        private void ClearVar()
        {
            _UserId = "";
            _Password = "";
            _DataSource = "";
            _InitialCatalog = "";
            _Options = "";
            _IntegratedSecurity = "";
            _PersistSecurityInfo = "";
        }
        #endregion
    }
}
