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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Nixxis.RfbServer
{
    /// <summary>
    /// A clone of Java's PixelGrabber class.
    /// </summary>
    public static unsafe class PixelGrabber
    {
        /// <summary>
        /// Creates a screen capture in a bitmap format. The currently used method in EncodedRectangleFactory.
        /// </summary>
        /// <param name="r">The rectangle from the screen that we should take a screenshot from.</param>
        /// <returns>A bitmap containing the image data of our screenshot. The return value is null only if a problem occured.</returns>
        public static Bitmap CreateScreenCapture(Rectangle r)
        {

            try
            {
                int width = r.Width;
                int height = r.Height;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(r.X, r.Y, 0, 0, new Size(width, height));

                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                System.Threading.Thread.Sleep(500);
                try
                {
                    int width = r.Width;
                    int height = r.Height;
                    Bitmap bitmap = new Bitmap(width, height);
                    Graphics g = Graphics.FromImage(bitmap);
                    g.CopyFromScreen(r.X, r.Y, 0, 0, new Size(width, height));
                }
                catch (Exception)
                {
                    return null;
                }
                return null;
            }
        }

        /// <summary>
        /// Extracts the pixels consisted in a bitmap from the specified rectangle into an integer array
        /// </summary>
        /// <param name="img">The bitmap whose pixels should be converted to an integer array</param>
        /// <param name="x">The X coordinate of the Rectangle</param>
        /// <param name="y">The Y coordinate of the Rectangle</param>
        /// <param name="w">The width of the Rectangle</param>
        /// <param name="h">The height of the Rectangle</param>
        /// <param name="pf">The pixel format that should be used</param>
        /// <returns></returns>
        public static int[]  GrabPixels(Bitmap img, int x, int y, int w, int h, PixelFormat pf)
        {
            int[] array = new int[w * h];
            BitmapData bmp = img.LockBits(new Rectangle(x, y, w, h), ImageLockMode.ReadOnly, pf);
            unsafe
            {
                int PixelSize = 4;

                for (int j = 0; j < h; j++)
                {
                    byte* row = (byte*)bmp.Scan0 + (j * bmp.Stride);
                    for (int i = 0; i < w; i++)
                    {
                        int a = Convert.ToInt32(row[(i * PixelSize) + 3]);
                        int r = Convert.ToInt32(row[(i * PixelSize) + 2]);
                        int g = Convert.ToInt32(row[(i * PixelSize) + 1]);
                        int b = Convert.ToInt32(row[(i * PixelSize)]);


                        array[j * w + i] = (int)((a & 255) << 24) + (int)((r & 255) << 16) + (int)((g & 255) << 8) + (int)(b & 255);
                    }
                }

            }
            img.UnlockBits(bmp);
            return array;
        }

    }
}
