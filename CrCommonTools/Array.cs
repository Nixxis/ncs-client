using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
    public static class ArrayTools
    {
        /// <summary>
        /// Changes the bounderies of the array 
        /// but saves what is in the orignele array
        /// </summary>
        /// <param name="array">Original array</param>
        /// <param name="upperBound">New upper bound</param>
        /// <returns>Original array with new boundery</returns>
        public static Array ReDimPreserv(Array array, int upperBound)
        {
            int UpperBound;

            if (upperBound < array.Length)
                UpperBound = array.Length + 1;
            else
                UpperBound = upperBound;

            object[] tmpArray = new object[UpperBound];

            array.CopyTo(tmpArray, 0);
            return tmpArray;
        }
        /// <summary>
        /// Changes the bounderies of the array with 1
        /// but saves what is in the orignele array
        /// </summary>
        /// <param name="array">Original array</param>
        /// <returns>Original array with new boundery</returns>
        public static Array ReDimPreserv(Array array)
        {
            int UpperBound = array.Length + 1;

            object[] tmpArray = new object[UpperBound];

            array.CopyTo(tmpArray, 0);
            return tmpArray;
        }
        /// <summary>
        /// Changes the bounderies of the array
        /// but saves what is in the orignele array
        /// </summary>
        /// <param name="array">Original array</param>
        /// <param name="upperBound">New upper bound</param>
        /// <returns></returns>
        public static object[] ReDimPreserv(object[] array, int upperBound)
        {
            int UpperBound;

            if (upperBound < array.Length)
                UpperBound = array.Length + 1;
            else
                UpperBound = upperBound;

            object[] tmpArray = new object[UpperBound];

            array.CopyTo(tmpArray, 0);
            return tmpArray;
        }
        /// <summary>
        /// Changes the bounderies of the array with 1
        /// but saves what is in the orignele array
        /// </summary>
        /// <param name="array">Original array</param>
        /// <returns>Original array with new boundery</returns>
        public static object[] ReDimPreserv(object[] array)
        {
            int UpperBound = array.Length + 1;

            object[] tmpArray = new object[UpperBound];

            array.CopyTo(tmpArray, 0);
            return tmpArray;
        }
        /// <summary>
        /// Add 2 arrays together
        /// </summary>
        /// <param name="array1">First array</param>
        /// <param name="array2">Array to add</param>
        /// <returns>Array1 + Array2</returns>
        public static object[] AddArrayToArray(object[] array1, object[] array2)
        {
            if (array2 == null)
            {
                throw new ArgumentNullException("Array2 is null");
            }
            if (array2.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("Array2 needs to have a least 1 element");
            }

            object[] tmpArray;

            if (array1 == null)
            {
                tmpArray = array2;
                return tmpArray;
            }
            else
            {
                tmpArray = ReDimPreserv(array1, array1.Length + array2.Length);
            }

            array2.CopyTo(tmpArray, array1.Length);

            for (int i = array1.Length; i < (array1.Length + array2.Length - 1); i++)
            {
                tmpArray[i] = array2[i - array1.Length];
            }

            return tmpArray;
        }
        /// <summary>
        /// Add 2 arrays together
        /// </summary>
        /// <param name="array1">First array</param>
        /// <param name="array2">Array to add</param>
        /// <returns>Array1 + Array2</returns>
        public static string[] AddArrayToArray(string[] array1, string[] array2)
        {
            if (array2 == null)
            {
                throw new ArgumentNullException("Array2 is null");
            }
            if (array2.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("Array2 needs to have a least 1 element");
            }

            string[] tmpArray;

            if (array1 == null)
            {
                tmpArray = array2;
                return tmpArray;
            }
            else
            {
                tmpArray = ToStringArray(ReDimPreserv(array1, array1.Length + array2.Length));
            }

            array2.CopyTo(tmpArray, array1.Length);

            for (int i = array1.Length; i < (array1.Length + array2.Length - 1); i++)
            {
                tmpArray[i] = array2[i - array1.Length];
            }

            return tmpArray;
        }

        public static string[] ToStringArray(Array array)
        {
            string[] newArray = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == null)
                    newArray[i] = "";
                else
                    newArray[i] = (string)array.GetValue(i).ToString();
            }

            return newArray;
        }
        public static tpe[] CopyArrayToType<tpe>(Array array)
        {
            tpe[] newArray = new tpe[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == null)
                    newArray[i] = default(tpe);
                else
                    newArray[i] = (tpe)array.GetValue(i);
            }

            return newArray;
        }
    }
}
