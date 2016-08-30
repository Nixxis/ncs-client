using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using System.IO;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Admin
{
    public class ExcelHandler
    {

        public delegate void progressReportDelegate(int percent, string message, string progressMessage);

        public class ExcelSheet
        {
            private int m_Id;
            private string m_Name;
            private ExcelHandler m_Handler;
            private bool m_FirstLineContainsName = true;

            internal ExcelSheet(ExcelHandler handler, int id, string name)
            {
                m_Handler = handler;
                m_Id = id;
                m_Name = name;
            }

            internal int Id
            {
                get
                {
                    return m_Id;
                }
            }

            public string Name
            {
                get
                {
                    return m_Name;
                }
            }
            public bool FirstLineContainsName
            {
                get
                {
                    return m_FirstLineContainsName;
                }
                set
                {
                    m_FirstLineContainsName = value;
                }
            }

            public DataSet DataSet
            {
                get
                {
                    return m_Handler.GetDataSet(this);
                }
                set
                {
                    m_Handler.SetDataSet(this, value, null);
                }
            }

            public void SetDataSet(DataSet ds, progressReportDelegate progress)
            {
                m_Handler.SetDataSet(this, ds, progress);
            }

            public void SaveFromCsv(string file, List<Type> types)
            {
                m_Handler.SaveFromCsv(this, file, types);
            }

            public void SaveAsCsv(string fileName)
            {
                m_Handler.SaveAsCsv(this, fileName);
            }


        }


        private string m_File;

        public ExcelHandler(string file)
        {
            m_File = file;
        }


        public IList<ExcelSheet> Sheets
        {
            get
            {
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(m_File, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                List<ExcelSheet> temp = new List<ExcelSheet>();
                for (int i = 1; i <= xlWorkBook.Worksheets.Count; i++)
                {
                    xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(i);
                    temp.Add(new ExcelSheet(this, i, xlWorkSheet.Name));
                    releaseObject(xlWorkSheet);
                }

                xlWorkBook.Close(true, misValue, misValue);
                xlApp.Quit();

                releaseObject(xlWorkBook);
                releaseObject(xlApp);
                return temp;
            }
        }

        public ExcelSheet CreateNewSheet()
        {
            return CreateNewSheet("Data");
        }

        public ExcelSheet CreateNewSheet(string name)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Excel.Application();

            xlApp.DisplayAlerts = false;
            xlApp.ScreenUpdating = false;
            xlApp.Visible = false;
            xlApp.UserControl = false;
            xlApp.Interactive = false;


            xlWorkBook = xlApp.Workbooks.Add();

            xlWorkSheet = xlWorkBook.Worksheets.Add(misValue, misValue, misValue, misValue);

            xlWorkSheet.Name = name;


            string extension = Path.GetExtension(m_File);
            if (extension == ".xls")
            {
                xlWorkBook.SaveAs(m_File, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel8, misValue, misValue, misValue, misValue,
                    Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    misValue, misValue, misValue, misValue, misValue);
            }
            else if (extension == ".xlsx")
            {
                xlWorkBook.SaveAs(m_File, misValue, misValue, misValue, misValue, misValue,
                    Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    misValue, misValue, misValue, misValue, misValue);
            }


            xlWorkBook.Close(misValue, misValue, misValue);

            xlApp.Quit();

            ExcelSheet temp = new ExcelSheet(this, 1, name);
            
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);
            return temp;
        }

        internal DataSet GetDataSet(ExcelSheet sheet)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(m_File, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(sheet.Id);

            Excel.Range excelRange = xlWorkSheet.UsedRange;


            object raw = excelRange.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);
            object[,] valueArray = null;

            DataSet ds = new System.Data.DataSet();

            DataTable dt = new DataTable();

            ds.Tables.Add(dt);

            if (raw == null)
                return ds;

            if (raw.GetType().IsArray)
                valueArray = (object[,])raw;
            else
                return ds;

            for (int i = 1; i <= valueArray.GetLength(1); i++)
            {
                object obj = valueArray[1, i];
                if (obj != null && sheet.FirstLineContainsName)
                    if(!dt.Columns.Contains(obj.ToString()))
                        dt.Columns.Add(obj.ToString(), typeof(object));
                    else
                        dt.Columns.Add(string.Empty, typeof(object));
                else
                    dt.Columns.Add(string.Empty, typeof(object));
            }

            int startProcessing = sheet.FirstLineContainsName ? 2 : 1;


            DataRow r;
            object[] temp;

            for (int j = startProcessing; j <= valueArray.GetLength(0); j++)
            {
                r = dt.NewRow();
                temp = new object[valueArray.GetLength(1)];

                for (int i = 1; i <= valueArray.GetLength(1); i++)
                {
                    temp[i - 1] = valueArray[j, i];
                }

                r.ItemArray = temp;
                dt.Rows.Add(r);
            }

            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return ds;
        }

        internal void SaveFromCsv(ExcelSheet sheet, string file, List<Type> types)
        {

            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Excel.Application();

            xlApp.DisplayAlerts = false;
            xlApp.ScreenUpdating = false;
            xlApp.Visible = false;
            xlApp.UserControl = false;
            xlApp.Interactive = false;

            xlWorkBook = xlApp.Workbooks.Open(m_File, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(sheet.Id);

            Excel.QueryTable qt = xlWorkSheet.QueryTables.Add("TEXT;" + file, xlWorkSheet.get_Range("$A$1"));
            qt.Name = "ccc";
            qt.FieldNames = true;
            qt.RowNumbers = false;
            qt.FillAdjacentFormulas = false;
            qt.PreserveFormatting = true;
            qt.RefreshOnFileOpen = false;
            qt.RefreshStyle = Excel.XlCellInsertionMode.xlInsertDeleteCells;
            qt.SavePassword = false;
            qt.SaveData = true;
            qt.AdjustColumnWidth = true;
            qt.RefreshPeriod = 0;
            qt.TextFilePromptOnRefresh = false;
            qt.TextFilePlatform = 1252;
            qt.TextFileStartRow = 1;
            qt.TextFileParseType = Excel.XlTextParsingType.xlDelimited; 
            qt.TextFileTextQualifier = Excel.XlTextQualifier.xlTextQualifierDoubleQuote;
            qt.TextFileConsecutiveDelimiter = false;
            qt.TextFileTabDelimiter = false;
            qt.TextFileSemicolonDelimiter = true;
            qt.TextFileCommaDelimiter = false;
            qt.TextFileSpaceDelimiter = false;
            List<Excel.XlColumnDataType> tempTypes = new List<Excel.XlColumnDataType>();
            foreach (Type t in types)
            {
                if (t == typeof(string))
                {
                    tempTypes.Add(Excel.XlColumnDataType.xlTextFormat);
                }
                else if (t == typeof(DateTime))
                {
                    tempTypes.Add(Excel.XlColumnDataType.xlYMDFormat);
                }
                else
                {
                    tempTypes.Add(Excel.XlColumnDataType.xlGeneralFormat);
                }
            }
            qt.TextFileColumnDataTypes = tempTypes.ToArray<Excel.XlColumnDataType>();
            qt.TextFileTrailingMinusNumbers = true;
            qt.Refresh(false);


            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(qt);
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);
        }

        internal void SaveAsCsv(ExcelSheet sheet, string file)
        {
            DataSet ds = GetDataSet(sheet);

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                Directory.CreateDirectory(Path.GetDirectoryName(file));

            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (CsvWriter R = new CsvWriter(fs, Encoding.UTF8, ';', '"'))
                {

                    string[] strs = new string[ds.Tables[0].Columns.Count];
                    int i = 0;
                    foreach (DataColumn col in ds.Tables[0].Columns)
                    {
                        strs[i] = col.ColumnName;
                        i++;
                    }
                    R.WriteLine(strs);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        R.WriteDataRow(dr);
                    }
                }
            }
        }

        internal void SetDataSet(ExcelSheet sheet, DataSet dataset, progressReportDelegate progress)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Excel.Application();

            xlApp.DisplayAlerts = false;
            xlApp.ScreenUpdating = false;
            xlApp.Visible = false;
            xlApp.UserControl = false;
            xlApp.Interactive = false;

            xlWorkBook = xlApp.Workbooks.Open(m_File, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(sheet.Id);

            int startRow = 1;
            List<int> columnsToType = new List<int>();
            List<string> columnNames = new List<string>();

            if (sheet.FirstLineContainsName)
            {
                startRow = 2;
                foreach (DataColumn col in dataset.Tables[0].Columns)
                {
                    xlWorkSheet.Cells[1, col.Ordinal + 1] = col.ColumnName;
                    columnsToType.Add(col.Ordinal);
                }
            }
            else
            {
                foreach (DataColumn col in dataset.Tables[0].Columns)
                {
                    columnsToType.Add(col.Ordinal);
                }

            }

            int lastprogress = 0;
            if (progress != null)
            {
                progress(0, TranslationContext.Default.Translate("Generating file..."), string.Empty);
            }


            string strFormat = string.Format("{0}{1}:{2}{1}", GetA1Notation(1), "{0}", GetA1Notation(dataset.Tables[0].Columns.Count));
            for (int i = 0; i < dataset.Tables[0].Rows.Count; i++)
            {
                object[] objArr = dataset.Tables[0].Rows[i].ItemArray;

                if (columnsToType.Count > 0)
                {
                    for (int j = 0; j < columnsToType.Count; j++)
                    {
                        if (objArr[j] != null)
                        {
                            Type tpe = objArr[columnsToType[j]].GetType();
                            Excel.Range tempange = xlWorkSheet.get_Range(string.Format("{0}1", GetA1Notation(columnsToType[j] + 1))).EntireColumn;
                            if (tpe == typeof(string))
                                tempange.NumberFormat = "@";

                            columnsToType.RemoveAt(j);
                            j--;

                        }
                    }
                }

                Excel.Range excelRange = xlWorkSheet.get_Range(string.Format(strFormat, i + startRow));
                excelRange.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, objArr);

                if (progress != null && dataset.Tables[0].Rows.Count > 0)
                {
                    int newprogress = i * 100 / dataset.Tables[0].Rows.Count;
                    if (lastprogress != newprogress)
                    {
                        lastprogress = newprogress;
                        progress(lastprogress, TranslationContext.Default.Translate("Generating file..."), string.Format(TranslationContext.Default.Translate("{0}/{1} rows"), i, dataset.Tables[0].Rows.Count));
                    }
                }

            }


            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

        }

        private string GetA1Notation(int column)
        {
            // 1-> A
            // 2-> B
            // ...
            // 26 -> Z
            // 27 -> AA

            if (column == 0)
            {
                return string.Empty;
            }
            else if (column <= 26)
            {
                return new string(Convert.ToChar(64 + column), 1);
            }
            else
            {
                int modulo = (column) % 26;
                column = (column) / 26;

                return string.Concat(GetA1Notation(column), GetA1Notation(modulo));
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                System.Diagnostics.Trace.WriteLine("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }


    public class CsvWriter : IDisposable
    {
        private Stream m_Output;
        private TextWriter m_LineWriter;
        private char m_FieldSep = ';';
        private char m_TextDelimiter = '\"';
        private int m_DisposeCount = 0;
        private Encoding m_Encoding = Encoding.Default;

        public CsvWriter(Stream output)
        {
            m_Output = output;
        }

        public CsvWriter(Stream output, Encoding encoding)
            : this(output)
        {
            m_Encoding = encoding;
        }

        public CsvWriter(Stream output, Encoding encoding, char fieldSeparator)
            : this(output, encoding)
        {
            m_FieldSep = fieldSeparator;
        }

        public CsvWriter(Stream input, Encoding encoding, char fieldSeparator, char textDelimiter)
            : this(input, encoding)
        {
            m_FieldSep = fieldSeparator;
            m_TextDelimiter = textDelimiter;
        }

        public char FieldSeparator
        {
            get
            {
                return m_FieldSep;
            }
            set
            {
                m_FieldSep = value;
            }
        }

        public char TextDelimiter
        {
            get
            {
                return m_TextDelimiter;
            }
            set
            {
                m_TextDelimiter = value;
            }
        }

        public void WriteLine(object[] towrite)
        {
            if (m_LineWriter == null)
                m_LineWriter = new StreamWriter(m_Output, m_Encoding);

            if (m_Output.CanWrite)
            {
                for (int i = 0; i < towrite.Length; i++)
                {
                    if (towrite[i] is string || (towrite[i] != null && towrite[i].ToString().IndexOf(m_FieldSep) >= 0) )
                    {
                        string str = null;
                        if (towrite[i] is string)
                            str = towrite[i] as string;
                        else
                            str = towrite[i].ToString();
                        m_LineWriter.Write(m_TextDelimiter);
                        string delim = new string(m_TextDelimiter, 1);
                        m_LineWriter.Write(str.Replace(delim, string.Concat(delim, delim)).Replace('\n', '|'));
                        m_LineWriter.Write(m_TextDelimiter);
                    }
                    else
                    {
                        m_LineWriter.Write(towrite[i]);
                    }
                    if (i != towrite.Length - 1)
                    {
                        m_LineWriter.Write(m_FieldSep);
                    }
                }
                m_LineWriter.WriteLine(string.Empty);
            }
        }

        public void Reset()
        {
            if (m_LineWriter != null)
            {
                m_LineWriter = null;
            }

            if (m_Output != null && m_Output.CanSeek)
                m_Output.Seek(0, SeekOrigin.Begin);
        }

        public void WriteDataRow(DataRow row)
        {
            WriteLine(row.ItemArray);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref m_DisposeCount, 1) == 0)
            {
                if (m_LineWriter != null)
                    m_LineWriter.Dispose();
                if (m_Output != null)
                    m_Output.Dispose();

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }


}
