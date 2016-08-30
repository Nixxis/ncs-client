using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Nixxis.Admin
{
    public class CsvReader : IDisposable
    {
        private Stream m_Input;
        private TextReader m_LineReader;
        private char m_FieldSep = ',';
        private char m_TextDelimiter = '\"';
        private string m_DecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private string m_LineDelimiter = "\r\n";
        private int m_DisposeCount = 0;
        private Encoding m_Encoding = Encoding.Default;
        private System.Globalization.CultureInfo m_CultureInfo = System.Globalization.CultureInfo.CurrentCulture;

        public CsvReader(Stream input)
        {
            m_Input = input;
        }

        public CsvReader(Stream input, Encoding encoding)
            : this(input)
        {
            m_Encoding = encoding;
        }

        public CsvReader(Stream input, Encoding encoding, char fieldSeparator)
            : this(input, encoding)
        {
            m_FieldSep = fieldSeparator;
        }

        public CsvReader(Stream input, Encoding encoding, char fieldSeparator, char textDelimiter)
            : this(input, encoding)
        {
            m_FieldSep = fieldSeparator;
            m_TextDelimiter = textDelimiter;
        }
        public CsvReader(Stream input, Encoding encoding, char fieldSeparator, char textDelimiter, string decimalSeparator)
            : this(input, encoding, fieldSeparator, textDelimiter)
        {
            m_DecimalSeparator = decimalSeparator;
            m_CultureInfo = new System.Globalization.CultureInfo(string.Empty);
            m_CultureInfo.NumberFormat.NumberDecimalSeparator = m_DecimalSeparator;
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

        private char[] m_Buffer = new char[1024];
        private long m_GlobalPosition = -1;
        private int m_Position = -1;
        private int m_BufferLength = -1;

        public long GlobalPosition
        {
            get
            {
                return m_GlobalPosition;
            }
        }

        public char GetCurrentChar()
        {
            return m_Buffer[m_Position];
        }
        public char PeakNext()
        {
            if (m_Position < m_BufferLength - 1)
            {
                return m_Buffer[m_Position + 1];
            }
            else
            {
                char backup = GetCurrentChar();

                m_BufferLength = m_LineReader.Read(m_Buffer, 1, 1023) + 1;
                m_Buffer[0] = backup;
                m_Position = 0;

                if (m_Position < m_BufferLength - 1)
                {
                    return m_Buffer[m_Position + 1];
                }
            }
            return (char)0;
        }
        public bool MoveToNextChar()
        {
            if (m_LineReader == null)
                m_LineReader = new StreamReader(m_Input, m_Encoding);

            if (m_Position < m_BufferLength - 1)
            {
                m_Position++;
                m_GlobalPosition++;
            }
            else
            {
                m_BufferLength = m_LineReader.Read(m_Buffer, 0, 1024);
                m_Position = 0;
                m_GlobalPosition++;
                if (m_BufferLength == 0)
                    return false;
            }
            return true;
        }

        public string[] ReadLine()
        {
            bool InText = false;
            List<string> Output = new List<string>();
            StringBuilder SB = new StringBuilder(1024);

            while (MoveToNextChar())
            {
                if (SB.Length > 100000)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("Stopping processing due to field containing more than {0} charcters!", SB.Length));
                    return null;
                }

                char current = GetCurrentChar();

                if (current == m_TextDelimiter)
                {
                    if (InText)
                    {
                        if (PeakNext() == m_TextDelimiter)
                        {
                            SB.Append(current);
                            MoveToNextChar();
                        }
                        else
                        {
                            InText = false;
                        }
                    }
                    else
                    {
                        InText = true;
                    }
                }
                else if (current == m_FieldSep)
                {
                    if (InText)
                    {

                        SB.Append(current);
                    }
                    else
                    {
                        Output.Add(SB.ToString());
                        SB.Remove(0, SB.Length);
                    }
                }
                else if (current == m_LineDelimiter[0] && PeakNext() == m_LineDelimiter[1])
                {
                    if (InText)
                    {
                        SB.Append(current);
                        MoveToNextChar();
                        SB.Append(current);
                    }
                    else
                    {
                        Output.Add(SB.ToString());
                        MoveToNextChar();
                        return Output.ToArray();
                    }
                }
                else if (current == m_LineDelimiter[1] || current == m_LineDelimiter[0])
                {
                    if (InText)
                    {
                        SB.Append(current);
                    }
                    else
                    {
                        Output.Add(SB.ToString());
                        return Output.ToArray();
                    }
                }
                else
                {
                    SB.Append(current);
                }
            }

            if (SB.Length > 0)
            {
                Output.Add(SB.ToString());
                return Output.ToArray();
            }

            return null;
        }

        public string[] ReadLineOld()
        {
            if (m_LineReader == null)
                m_LineReader = new StreamReader(m_Input, m_Encoding);

            if (m_Input.CanRead)
            {
                string Line = m_LineReader.ReadLine();

                if (Line != null)
                {
                    List<string> Output = new List<string>();
                    StringBuilder SB = new StringBuilder(Line.Length);
                    bool InText = false;

                    for (int i = 0; i < Line.Length; i++)
                    {
                        if (Line[i] == m_TextDelimiter)
                        {
                            if (InText)
                            {
                                if (i < (Line.Length - 1) && Line[i + 1] == m_TextDelimiter)
                                {
                                    SB.Append(Line[i]);
                                    i++;
                                }
                                else
                                {
                                    InText = false;
                                }
                            }
                            else
                            {
                                InText = true;
                            }
                        }
                        else if (Line[i] == m_FieldSep)
                        {
                            if (InText)
                            {
                                SB.Append(Line[i]);
                            }
                            else
                            {
                                Output.Add(SB.ToString());
                                SB.Remove(0, SB.Length);
                            }
                        }
                        else
                        {
                            SB.Append(Line[i]);
                        }
                    }

                    Output.Add(SB.ToString());

                    return Output.ToArray();
                }
            }

            return null;
        }

        public void Reset()
        {
            if (m_LineReader != null)
            {
                m_LineReader = null;
            }

            if (m_Input != null && m_Input.CanSeek)
                m_Input.Seek(0, SeekOrigin.Begin);
        }

        public DataRow ReadDataRow(DataTable table)
        {
            string[] Cols = ReadLine();

            if (Cols == null)
                return null;

            DataRow Row = table.NewRow();

            for (int i = 0; i < Cols.Length && i < table.Columns.Count; i++)
            {
                try
                {
                    if (!table.Columns[i].ReadOnly)
                    {
                        Type ColType = table.Columns[i].DataType;

                        if (string.IsNullOrEmpty(Cols[i]))
                        {
                            if (ColType.Equals(typeof(string)))
                            {
                                Row[i] = string.Empty;
                            }
                            else
                            {
                                Row[i] = DBNull.Value;
                            }
                        }
                        else
                        {
                            if (ColType.Equals(typeof(bool)))
                            {
                                bool Value;

                                if (!bool.TryParse(Cols[i], out Value))
                                {
                                    int IntVal;

                                    if (!int.TryParse(Cols[i], out IntVal))
                                    {
                                        throw new InvalidCastException(string.Concat("Invalid bool value: ", Cols[i]));
                                    }

                                    Value = (IntVal != 0);
                                }

                                Row[i] = Value;
                            }
                            else if (ColType.Equals(typeof(DateTime)))
                            {
                                Row[i] = Cols[i];
                            }
                            else if (ColType.Equals(typeof(double)))
                            {
                                try
                                {
                                    Row[i] = double.Parse(Cols[i], m_CultureInfo);
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                Row[i] = Cols[i];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }

            return Row;
        }

        public long GetLineCount()
        {
            long pos = m_Input.Position;

            m_Input.Seek(0, SeekOrigin.Begin);

            if (m_LineReader == null)
                m_LineReader = new StreamReader(m_Input, m_Encoding);

            long lineCount = 0;
            while (m_LineReader.ReadLine() != null)
            {
                lineCount++;
            }
            m_Input.Seek(pos, SeekOrigin.Begin);

            return lineCount;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref m_DisposeCount, 1) == 0)
            {
                if (m_LineReader != null)
                    m_LineReader.Dispose();
                if (m_Input != null)
                    m_Input.Dispose();

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }

    public class CsvWriter : IDisposable
    {
        private Stream m_Output;
        private TextWriter m_LineWriter;
        private char m_FieldSep = ',';
        private char m_TextDelimiter = '\"';
        private string m_DecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private System.Globalization.CultureInfo m_CultureInfo = System.Globalization.CultureInfo.CurrentCulture;
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


        public CsvWriter(Stream input, Encoding encoding, char fieldSeparator, char textDelimiter, string decimalSeparator)
            : this(input, encoding, fieldSeparator, textDelimiter)
        {
            m_DecimalSeparator = decimalSeparator;
            m_CultureInfo = new System.Globalization.CultureInfo(string.Empty);
            m_CultureInfo.NumberFormat.NumberDecimalSeparator = m_DecimalSeparator;
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
                    if (towrite[i] is string)
                    {
                        string str = towrite[i] as string;
                        m_LineWriter.Write(m_TextDelimiter);
                        string delim = new string(m_TextDelimiter, 1);
                        m_LineWriter.Write(str.Replace(delim, string.Concat(delim, delim)).Replace('\n', '|'));
                        m_LineWriter.Write(m_TextDelimiter);
                    }
                    else if (towrite[i] != null && (towrite[i] is double || towrite[i] is float))
                    {
                        string str = ((double)(towrite[i])).ToString(m_CultureInfo);
                        if (str.IndexOf(m_FieldSep) >= 0)
                        {
                            m_LineWriter.Write(m_TextDelimiter);
                            string delim = new string(m_TextDelimiter, 1);
                            m_LineWriter.Write(str.Replace(delim, string.Concat(delim, delim)).Replace('\n', '|'));
                            m_LineWriter.Write(m_TextDelimiter);
                        }
                        else
                        {
                            m_LineWriter.Write(str);
                        }
                    }
                    else
                    {
                        if (towrite[i] == null)
                        {
                            m_LineWriter.Write(towrite[i]);
                        }
                        else
                        {
                            string str = towrite[i].ToString();
                            if (str.IndexOf(m_FieldSep) >= 0)
                            {
                                m_LineWriter.Write(m_TextDelimiter);
                                string delim = new string(m_TextDelimiter, 1);
                                m_LineWriter.Write(str.Replace(delim, string.Concat(delim, delim)).Replace('\n', '|'));
                                m_LineWriter.Write(m_TextDelimiter);
                            }
                            else
                            {
                                m_LineWriter.Write(str);
                            }
                        }
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
