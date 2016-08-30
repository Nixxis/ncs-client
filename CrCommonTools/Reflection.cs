using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
    public static class Reflection
    {
        /// <summary>
        /// Gets the value of a property or sub-property of a class
        /// </summary>
        /// <param name="item">Value holder</param>
        /// <param name="property">Value to get (Realtim, Realtime.Pause, ...)</param>
        /// <returns>The reqeusted value as an object</returns>
        public static object GetValueFromClassProperty(object item, string property)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException("Item is null");
            }
            if (property == null)
            {
                throw new System.ArgumentNullException("Property is null");
            }
            if (property == "" || property == string.Empty)
            {
                throw new System.ArgumentException("Property can't be empty");
            }

            object obj = item;

            foreach (string s in property.Split(new char[] { '.' }))
            {
                obj = GetValueFromObject(obj, s);
            }

            return obj;
        }
        /// <summary>
        /// Gets the value of a porerty based on the field propertie name
        /// </summary>
        /// <param name="item">Value holder</param>
        /// <param name="field">Propertie name</param>
        /// <returns>The reqeusted value as an object</returns>
        public static object GetValueFromObject(object item, string field)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException("Item is null");
            }
            if (field == null)
            {
                throw new System.ArgumentNullException("Field is null");
            }
            if (field == "" || field == string.Empty)
            {
                throw new System.ArgumentException("Field can't be empty");
            }

            foreach (System.Reflection.PropertyInfo propInfo in item.GetType().GetProperties())
            {
                if (propInfo.Name == field)
                {
                    return propInfo.GetValue(item, new object[] { });
                }
            }

            return null;
        }
        /// <summary>
        /// Sets the value of a property or sub-property of a class
        /// </summary>
        /// <param name="item">Value holder</param>
        /// <param name="property">Value to get (Realtim, Realtime.Pause, ...)</param>
        public static void SetValueFromClassProperty(object item, string property, object value)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException("Item is null");
            }
            if (property == null)
            {
                throw new System.ArgumentNullException("Property is null");
            }
            if (property == "" || property == string.Empty)
            {
                throw new System.ArgumentException("Property can't be empty");
            }

            object obj = item;
            string field = "";
            string[] m_ClassProperties = property.Split(new char[] { '.' });

            if (m_ClassProperties == null) return;
            if (m_ClassProperties.Length == 0) return;

            for (int i = 0; i < m_ClassProperties.Length - 1; i++)
            {
                field = m_ClassProperties[i];
                obj = GetValueFromObject(obj, field);
            }
            SetValueFromObject(obj, m_ClassProperties[m_ClassProperties.Length - 1], value); 
        }
        /// <summary>
        /// Sets the value of a porerty based on the field propertie name
        /// </summary>
        /// <param name="item">Value holder</param>
        /// <param name="field">Propertie name</param>
        public static void SetValueFromObject(object item, string field, object value)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException("Item is null");
            }
            if (field == null)
            {
                throw new System.ArgumentNullException("Field is null");
            }
            if (field == "" || field == string.Empty)
            {
                throw new System.ArgumentException("Field can't be empty");
            }

            foreach (System.Reflection.PropertyInfo propInfo in item.GetType().GetProperties())
            {
                if (propInfo.Name == field)
                {
                    propInfo.SetValue(item, value, null);
                }
            }
        }
    }
}
