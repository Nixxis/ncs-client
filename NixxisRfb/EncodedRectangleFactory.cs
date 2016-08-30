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
using System.Drawing;
using Nixxis.RfbServer.Encodings;

namespace Nixxis.RfbServer
{
    public class EncodedRectangleFactory
    {
        RfbProtocol rfb;
        Framebuffer framebuffer;
        public EncodedRectangleFactory(RfbProtocol rfb, Framebuffer framebuffer)
        {
            System.Diagnostics.Debug.Assert(rfb != null, "RfbProtocol object must be non-null");
            System.Diagnostics.Debug.Assert(framebuffer != null, "Framebuffer object must be non-null");

            this.rfb = rfb;
            this.framebuffer = framebuffer;
        }

        public EncodedRectangle Build(Rectangle rectangle, RfbProtocol.Encoding encoding)
        {
            EncodedRectangle e = null;
            Bitmap bmp = PixelGrabber.CreateScreenCapture(rectangle);
            int[] pixels = PixelGrabber.GrabPixels(bmp, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, bmp.PixelFormat);

            switch (encoding)
            {
                case RfbProtocol.Encoding.ZRLE_ENCODING:
                    e = new ZrleRectangle(rfb, framebuffer, pixels, rectangle);				
                    break;
                default:
                    throw new FormatException ("Unsupported Encoding: " + encoding.ToString() + ".");
            }
            return e;
        }
        public EncodedRectangle Build(Bitmap bmp,int x, int y, RfbProtocol.Encoding encoding)
        {
            EncodedRectangle e = null;
            int[] pixels = PixelGrabber.GrabPixels(bmp, 0, 0, bmp.Width, bmp.Height, bmp.PixelFormat);
            Rectangle rect = new Rectangle(x, y, bmp.Width, bmp.Height);

            switch (encoding)
            {
                case RfbProtocol.Encoding.ZRLE_ENCODING:
                    e = new ZrleRectangle(rfb, framebuffer, pixels, rect);				
                    break;
                default:
                    throw new FormatException("Unsupported Encoding: " + encoding.ToString() + ".");
            }
            return e;
        }
    }
}
