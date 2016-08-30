using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace ContactRoute.ApplicationServer.Formatting
{
	internal class DataFormatter : Stream
	{
		private enum SerializationType : byte
		{
			Null,
			EmptyString,
			String,
			Formatter
		}

		public static int DefaultBlockSize = 256;

		private IFormatter m_Formatter;
		private bool m_ReadOnly;
		private byte[] m_Buffer;
		private byte[] m_OuterBuffer = null;
		private int m_BufferStart = 0;
		private int m_BufferPos = 0;
		private int m_BufferSize = 0;
		private int m_BlockSize = DefaultBlockSize;

		public DataFormatter()
		{
			m_Formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			m_Buffer = new byte[m_BlockSize];
		}

		public void Initialize(byte[] buffer, int offset, int count, bool copyBuffer)
		{
			Reset();

			m_OuterBuffer = null;

			if (copyBuffer)
			{
				m_ReadOnly = false;

				if (count > m_Buffer.Length)
				{
					byte[] NewBuffer = new byte[((count / m_BlockSize) + 1) * m_BlockSize];

					m_Buffer = NewBuffer;
				}

				Buffer.BlockCopy(buffer, offset, m_Buffer, 0, count);

				m_BufferStart = 0;
				m_BufferPos = 0;
				m_BufferSize = count;
			}
			else
			{
				m_ReadOnly = true;

				m_OuterBuffer = buffer;
				m_BufferStart = offset;

				m_BufferPos = 0;
				m_BufferSize = count;
			}
		}

		public void Serialize(object graph)
		{
			if (m_ReadOnly)
				throw new NotSupportedException("Stream is read-only.");

			if (graph == null)
			{
				WriteByte((byte)SerializationType.Null);
			}
			else if (graph is string)
			{
				string Text = (string)graph;
				int Length = Text.Length;

				if (Length == 0)
				{
					WriteByte((byte)SerializationType.EmptyString);
				}
				else
				{
					short Size = (short)Encoding.UTF8.GetByteCount(Text);

					WriteByte((byte)SerializationType.String);
					WriteByte((byte)(Size >> 8));
					WriteByte((byte)(Size & 0x00FF));

					int NewPos = m_BufferPos + Size;

					if (m_BufferStart + NewPos > m_Buffer.Length)
					{
						byte[] NewBuffer = new byte[(((m_BufferStart + NewPos) / m_BlockSize) + 1) * m_BlockSize];

						if (m_BufferPos > 0)
							Buffer.BlockCopy(m_Buffer, 0, NewBuffer, 0, m_BufferStart + m_BufferPos);

						m_Buffer = NewBuffer;
					}

					Encoding.UTF8.GetBytes(Text, 0, Length, m_Buffer, m_BufferStart + m_BufferPos);

					m_BufferPos = NewPos;

					if (m_BufferPos > m_BufferSize)
						m_BufferSize = m_BufferPos;
				}
			}
			else
			{
				WriteByte((byte)SerializationType.Formatter);
				m_Formatter.Serialize(this, graph);
			}
		}

		public object Deserialize()
		{
			object Value;
			SerializationType SType = (SerializationType)ReadByte();

			if (SType == SerializationType.Null)
			{
				Value = null;
			}
			else if (SType == SerializationType.EmptyString)
			{
				Value = string.Empty;
			}
			else if (SType == SerializationType.String)
			{
				short Size = (short)((ReadByte() << 8) | ReadByte());

				if (m_BufferPos + Size > m_BufferSize)
					throw new SerializationException("Reached end of stream");

				if (m_ReadOnly)
					Value = Encoding.UTF8.GetString(m_OuterBuffer, m_BufferStart + m_BufferPos, Size);
				else
					Value = Encoding.UTF8.GetString(m_Buffer, m_BufferStart + m_BufferPos, Size);

				m_BufferPos += Size;
			}
			else
			{
				Value = m_Formatter.Deserialize(this);
			}

			return Value;
		}

		public void Reset()
		{
			m_OuterBuffer = null;

			m_ReadOnly = false;
			m_BufferStart = 0;
			m_BufferPos = 0;
			m_BufferSize = 0;
		}

		#region IPooledObject Members

		public void Recycle()
		{
			Reset();
		}

		#endregion

		public byte[] UnderlyingBuffer
		{
			get
			{
				if (m_ReadOnly)
					throw new NotSupportedException("Stream is read-only.");

				return m_Buffer;
			}
		}

		#region Stream

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return !m_ReadOnly;
			}
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get
			{
				return (long)m_BufferSize;
			}
		}

		public override long Position
		{
			get
			{
				return (long)m_BufferPos;
			}
			set
			{
				if (value < 0 || value > m_BufferSize)
					throw new ArgumentOutOfRangeException("Cannot seek across stream boundaries.");

				m_BufferPos = (int)value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				int NewPos = m_BufferPos + count;

				if (NewPos > m_BufferSize)
				{
					count = m_BufferSize - m_BufferPos;
					NewPos = m_BufferSize;
				}

				if (count > 0)
				{
					if (m_ReadOnly)
						Buffer.BlockCopy(m_OuterBuffer, m_BufferStart + m_BufferPos, buffer, offset, count);
					else
						Buffer.BlockCopy(m_Buffer, m_BufferStart + m_BufferPos, buffer, offset, count);

					m_BufferPos = NewPos;
				}
			}

			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			int NewPos;

			switch (origin)
			{
				case SeekOrigin.Begin:
					NewPos = (int)offset;
					break;

				case SeekOrigin.End:
					NewPos = m_BufferSize + (int)offset;
					break;

				default:
					NewPos = m_BufferPos + (int)offset;
					break;
			}

			if (NewPos < 0 || NewPos > m_BufferSize)
				throw new ArgumentOutOfRangeException("Cannot seek across stream boundaries.");

			m_BufferPos = NewPos;

			return m_BufferPos;
		}

		public override void SetLength(long value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException("Cannot have a negative length.");

			if (value > m_BufferSize)
				throw new ArgumentOutOfRangeException("Cannot expand stream.");

			m_BufferSize = (int)value;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (m_ReadOnly)
				throw new NotSupportedException("Stream is read-only.");

			if (count > 0)
			{
				int NewPos = m_BufferPos + count;

				if (m_BufferStart + NewPos > m_Buffer.Length)
				{
					byte[] NewBuffer = new byte[(((m_BufferStart + NewPos) / m_BlockSize) + 1) * m_BlockSize];

					if (m_BufferPos > 0)
						Buffer.BlockCopy(m_Buffer, 0, NewBuffer, 0, m_BufferStart + m_BufferPos);

					m_Buffer = NewBuffer;
				}

				Buffer.BlockCopy(buffer, offset, m_Buffer, m_BufferStart + m_BufferPos, count);

				m_BufferPos = NewPos;

				if (m_BufferPos > m_BufferSize)
					m_BufferSize = m_BufferPos;
			}
		}

		public void Write(string value)
		{
			if (m_ReadOnly)
				throw new NotSupportedException("Stream is read-only.");

			if (!string.IsNullOrEmpty(value))
			{
				int Size = Encoding.UTF8.GetByteCount(value);
				int NewPos = m_BufferPos + Size;

				if (m_BufferStart + NewPos > m_Buffer.Length)
				{
					byte[] NewBuffer = new byte[(((m_BufferStart + NewPos) / m_BlockSize) + 1) * m_BlockSize];

					if (m_BufferPos > 0)
						Buffer.BlockCopy(m_Buffer, 0, NewBuffer, 0, m_BufferStart + m_BufferPos);

					m_Buffer = NewBuffer;
				}

				Encoding.UTF8.GetBytes(value, 0, value.Length, m_Buffer, m_BufferStart + m_BufferPos);

				m_BufferPos = NewPos;

				if (m_BufferPos > m_BufferSize)
					m_BufferSize = m_BufferPos;
			}
		}

		#endregion
	}

}
