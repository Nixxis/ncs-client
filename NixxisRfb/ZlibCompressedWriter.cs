// NVNC - .NET VNC Server Library
// Copyright (C) 2014 T!T@N
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;
using System.IO.Compression;

namespace Nixxis.RfbServer
{
    /// <summary>
    /// A BinaryWriter that uses the Zlib algorithm to write compressed data to a Stream.
    /// I have overrided only the necessary methods used by the ZRLE and Zlib encoding.
    /// </summary>
    public sealed class ZlibCompressedWriter : BinaryWriter
    {
        /// <summary>
        /// A temporary MemoryStream to hold the compressed data.
        /// </summary>
        private MemoryStream zMemoryStream;



        /// <summary>
        /// CompressedWriter is used to write the compressed bytes from zMemoryStream to uncompressedStream.
        /// </summary>
        private BinaryWriter compressedWriter;
        /// <summary>
        /// BigWriter is used to write the number of bytes in a BigEndian format.
        /// </summary>
        private BigEndianBinaryWriter bigWriter;
        
        private long oldPos;

        /// <summary>
        /// Writes compressed data to the given stream.
        /// </summary>
        /// <param name="uncompressedStream">A stream where the compressed data should be written.</param>
        /// <param name="level">The Zlib compression level that should be used. Default is Z_BEST_COMPRESSION = 9.</param>
        public ZlibCompressedWriter(Stream uncompressedStream, int level=9)
            : base(uncompressedStream)
        {
            /* Since we need to write the number of compressed bytes that we are going to send first,
             * We cannot directly write to the uncompressedStream.
             * We first write the compressed data to zMemoryStream, and after that we write the data from it to the uncompressedStream
             * using CompressedWriter.
             */ 

            zMemoryStream = new MemoryStream();

            
           
            //The VNC Protocol uses Z_SYNC_FLUSH as a Flush Mode
            //zCompressStream.FlushMode = zlibConst.Z_SYNC_FLUSH;
            
            compressedWriter = new BinaryWriter(uncompressedStream);
            bigWriter = new BigEndianBinaryWriter(uncompressedStream);
            
            oldPos=0;
        }
        public override void Write(byte[] buffer, int index, int count)
        {

            using(GZipStream compressor = new GZipStream(zMemoryStream, CompressionMode.Compress, true))
                compressor.Write(buffer, index, count);

            long cPos = zMemoryStream.Position;
            int len = Convert.ToInt32(cPos - oldPos);
            long nPos = cPos - len;

            bigWriter.Write(len);

            int pos = 0;
            zMemoryStream.Position = nPos;

            using (MemoryStream tmp = new MemoryStream())
            {
                while (pos++ < len)
                {
                    try
                    {
                        int bData = zMemoryStream.ReadByte();
                        tmp.WriteByte((byte)bData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                compressedWriter.Write(tmp.ToArray());
            }
            
            
            oldPos = cPos;
        }
        public override void Write(byte[] buffer)
        {
            this.Write(buffer, 0, buffer.Length);
        }
        public override void Write(byte value)
        {
            byte[] b = new byte[1];
            b[0] = value;
            this.Write(b, 0, 1);
        }
    }
}
