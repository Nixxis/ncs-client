using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Specialized;

namespace ContactRoute.Config.Common
{
    public static class GuiHelp
    {
        /// <summary>
        /// Based on the tag of the control a value of the data object while be displayed
        /// </summary>
        /// <param name="data">Object that contains the value</param>
        /// <param name="control">The control to display the value(s) on</param>
        /// <param name="tagIndicator">Only check the tags that start with this character</param>
        /// <param name="checkChilderen">Also check the childeren of the control</param>
        public static void SetDataToControl(object data, Control control, string tagIndicator, bool checkChilderen)
        {
            if (control == null) return;
            if (data == null) return;
            if (string.IsNullOrEmpty(tagIndicator)) tagIndicator = "#";

            try
            {
                foreach (Control item in control.Controls)
                {
                    string tag = Convert.ToString(item.Tag);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (tag.StartsWith(tagIndicator))
                        {
                            string[] part = tag.Split(tagIndicator.ToCharArray());
                            string parameterName = null;
                            //Option list
                            bool LogicNOT = false;

                            if (part.Length > 2)
                            {
                                string[] option = part[1].Split(',');

                                foreach (string op in option)
                                {
                                    if (op.ToLower() == "not")
                                    {
                                        LogicNOT = true;
                                    }
                                }
                                parameterName = part[2];
                            }
                            else
                            {
                                parameterName = tag.Substring(1);
                            }

                            bool found = false;

                            foreach (PropertyInfo property in data.GetType().GetProperties())
                            {
                                foreach (ConfigFileValueFieldAttribute attr in property.GetCustomAttributes(typeof(ConfigFileValueFieldAttribute), true))
                                {
                                    string name = string.Empty;
                                    if (string.IsNullOrEmpty(attr.Name))
                                        name = property.Name;
                                    else
                                        name = attr.Name;

                                    if (name.Trim() == parameterName.Trim())
                                    {
                                        if (item.GetType() == typeof(TextBox))
                                        {
                                            item.Text = property.GetValue(data, null).ToString();
                                            found = true;
                                        }
                                        else if (item.GetType() == typeof(CheckBox))
                                        {
                                            if (LogicNOT)
                                                ((CheckBox)item).Checked = !(Boolean)property.GetValue(data, null);
                                            else
                                                ((CheckBox)item).Checked = (Boolean)property.GetValue(data, null);
                                            found = true;
                                        }
                                        else if (item.GetType() == typeof(ComboBox))
                                        {
                                            SetEnumToControl(item, property.GetValue(data, null), true);
                                            found = true;
                                        }
                                        else
                                        {
                                            item.Text = property.GetValue(data, null).ToString();
                                            found = true;
                                        }
                                    }
                                    if (found) break;
                                }
                                if (found) break;
                            }
                        }
                    }
                    if (item.Controls.Count > 0 && checkChilderen) SetDataToControl(data, item, tagIndicator, checkChilderen);
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.WriteLine("ContactRoute.Config.Common.GuiHelp. Error: " + error.ToString());
            }
        }
        /// <summary>
        /// Based on the tag of the control the value of the control will be put in the data object
        /// </summary>
        /// <param name="data">Object that will contains the values to return</param>
        /// <param name="control">The control to read the value(s) from</param>
        /// <param name="tagIndicator">Only check the tags that start with this character</param>
        /// <param name="checkChilderen">Also check the childeren of the control</param>
        /// <returns>The object containing the data</returns>
        public static object GetDataFromControl(object data, Control control, string tagIndicator, bool checkChilderen)
        {
            if (control == null) return null;
            if (data == null) return null;
            if (string.IsNullOrEmpty(tagIndicator)) tagIndicator = "#";

            try
            {
                foreach (Control item in control.Controls)
                {
                    string tag = Convert.ToString(item.Tag);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (tag.StartsWith(tagIndicator))
                        {
                            string[] part = tag.Split(tagIndicator.ToCharArray());
                            string parameterName = null;
                            //Option list
                            bool LogicNOT = false;

                            if (part.Length > 2)
                            {
                                string[] option = part[1].Split(',');

                                foreach (string op in option)
                                {
                                    if (op.ToLower() == "not")
                                    {
                                        LogicNOT = true;
                                    }
                                }
                                parameterName = part[2];
                            }
                            else
                            {
                                parameterName = tag.Substring(1);
                            }


                            bool found = false;

                            foreach (PropertyInfo property in data.GetType().GetProperties())
                            {
                                foreach (ConfigFileValueFieldAttribute attr in property.GetCustomAttributes(typeof(ConfigFileValueFieldAttribute), true))
                                {
                                    string name = string.Empty;
                                    if (string.IsNullOrEmpty(attr.Name))
                                        name = property.Name;
                                    else
                                        name = attr.Name;

                                    if (name.Trim() == parameterName.Trim())
                                    {
                                        //
                                        //Getting the value depping on the type of control
                                        //
                                        object value = null;
                                        if (item.GetType() == typeof(TextBox))
                                        {
                                            value = item.Text;
                                        }
                                        else if (item.GetType() == typeof(CheckBox))
                                        {
                                            if (LogicNOT)
                                                value = !((CheckBox)item).Checked;
                                            else
                                                value = ((CheckBox)item).Checked;
                                        }
                                        else
                                        {
                                            value = item.Text;
                                        }
                                        //
                                        //Saving the value into the data object depping on the type of the property
                                        //
                                        if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(bool))
                                        {
                                            property.SetValue(data, (bool)value, null);
                                        }
                                        else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                                        {
                                            property.SetValue(data, value.ToString(), null);
                                        }
                                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64))
                                        {
                                            property.SetValue(data, System.Convert.ToInt32(value), null);
                                        }
                                        else if (property.PropertyType == typeof(string[]))
                                        {
                                            property.SetValue(data, value, null);
                                        }
                                        else if (property.PropertyType == typeof(StringCollection))
                                        {
                                            property.SetValue(data, value, null);
                                        }
                                        else if (property.PropertyType.BaseType == typeof(System.Enum))
                                        {
                                            property.SetValue(data, Enum.Parse(property.PropertyType, value.ToString()), null);
                                        }
                                        else
                                        {
                                            property.SetValue(data, value.ToString(), null);
                                        }
                                        found = true;
                                    }
                                    if (found) break;
                                }
                                if (found) break;
                            }
                        }
                    }
                    if (item.Controls.Count > 0 && checkChilderen) data = GetDataFromControl(data, item, tagIndicator, checkChilderen);
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.WriteLine("Configurator.GetProfileData. Error: " + error.ToString());
            }
            return data;
        }
        
        /// <summary>
        /// To dispaly a enum value into a supported control (Combobox,DataGridViewColumn)
        /// </summary>
        /// <param name="control">The control to display the value(s) on</param>
        /// <param name="item">The item to display</param>
        /// <param name="selectItem">Select the item when this is possible</param>
        public static void SetEnumToControl(Control control, object itemData, bool selectItem)
        {
            //For now only combobox is supported
            if (control.GetType() == typeof(ComboBox))
            {
                ComboBox cbo = (ComboBox)control;
                cbo.Items.Clear();

                if (itemData.GetType().BaseType == typeof(Enum))
                {
                    Array list = Enum.GetValues(itemData.GetType());

                    for (int i = 0; i < list.Length; i++)
                    {
                        cbo.Items.Add(list.GetValue(i));
                        if (selectItem)
                        {
                            if (list.GetValue(i).Equals(itemData))
                            {
                                cbo.SelectedItem = itemData;
                            }
                        }
                    }
                }
                else
                {
                    cbo.Items.Add(itemData);
                    if (selectItem)
                    {
                        cbo.SelectedItem = itemData;
                    }
                }
            }           
        }
        /// <summary>
        /// To dispaly a enum value into a supported control (Combobox, DataGridViewColumn)
        /// </summary>
        /// <param name="control">The control to display the value(s) on</param>
        /// <param name="item">The item to display</param>
        public static void SetEnumToControl(DataGridViewColumn control, object itemData)
        {
            //For now only combobox is supported
            if (control.GetType() == typeof(DataGridViewComboBoxColumn))
            {
                DataGridViewComboBoxColumn cbo = (DataGridViewComboBoxColumn)control;
                cbo.Items.Clear();

                if (itemData.GetType().BaseType == typeof(Enum))
                {
                    Array list = Enum.GetValues(itemData.GetType());

                    for (int i = 0; i < list.Length; i++)
                    {
                        cbo.Items.Add(list.GetValue(i).ToString());
                    }
                }
                else
                {
                    cbo.Items.Add(itemData);
                }
            }
        }
    }
}
