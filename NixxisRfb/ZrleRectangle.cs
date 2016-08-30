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
using System.Collections.Generic;
using System.Drawing;

namespace Nixxis.RfbServer.Encodings
{
    /// <summary>
    /// Implementation of ZRLE encoding, as well as drawing support. See RFB Protocol document v. 3.8 section 6.6.5.
    /// </summary>
    public sealed class ZrleRectangle : EncodedRectangle
    {
        private const int TILE_WIDTH = 64;
        private const int TILE_HEIGHT = 64;
        private int[] pixels;
        public ZrleRectangle(RfbProtocol rfb, Framebuffer framebuffer, int[] pixels, Rectangle rectangle)
            : base(rfb, framebuffer, rectangle, RfbProtocol.Encoding.ZRLE_ENCODING)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            int x = rectangle.X;
            int y = rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            
            int rawDataSize = w * h * (this.framebuffer.BitsPerPixel / 8);
            byte[] data = new byte[rawDataSize];
            int currentX, currentY;
            int tileW, tileH;

            //Bitmap bmp = PixelGrabber.GrabImage(rectangle.Width, rectangle.Height, pixels);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (currentY = y; currentY < y + h; currentY += TILE_HEIGHT)
                {
                    tileH = TILE_HEIGHT;
                    tileH = Math.Min(tileH, y + h - currentY);
                    for (currentX = x; currentX < x + w; currentX += TILE_WIDTH)
                    {
                        tileW = TILE_WIDTH;
                        tileW = Math.Min(tileW, x + w - currentX);

                        byte subencoding = 0;
                        ms.WriteByte(subencoding);

                        int[] pixelz = CopyPixels(pixels, w, currentX - x, currentY - y, tileW, tileH);

                        //PixelGrabber.GrabPixels(pixels, new Rectangle(currentX, currentY, tileW, tileH), this.framebuffer);

                        for (int i = 0; i < pixelz.Length; ++i)
                        {
                            int bb = 0;

                            //The CPixel structure (Compressed Pixel) has 3 bytes, opposed to the normal pixel which has 4.
                            byte[] bytes = new byte[3];
                            int pixel = pixelz[i];

                            bytes[bb++] = (byte)(pixel & 0xFF);
                            bytes[bb++] = (byte)((pixel >> 8) & 0xFF);
                            bytes[bb++] = (byte)((pixel >> 16) & 0xFF);
                            //bytes[b++] = (byte)((pixel >> 24) & 0xFF);

                            ms.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                byte[] uncompressed = ms.ToArray();
                this.bytes = uncompressed;
            }
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUint32(Convert.ToUInt32(RfbProtocol.Encoding.ZRLE_ENCODING));

            //ZrleRectangle exclusively uses a ZlibWriter to compress the bytes
            rfb.ZlibWriter.Write(this.bytes);
            rfb.ZlibWriter.Flush();
        }

        public override int[] GetCopy()
        {
            int[] retVal = new int[pixels.Length];
            Array.Copy(pixels, retVal, retVal.Length);
            return retVal;
        }

        public override void Diff(int[] backup)
        {


            int minX = int.MaxValue;
            int maxX = -1;
            int minY = int.MaxValue;
            int maxY = -1;

            if (backup.Length != pixels.Length)
            {
                return;
            }



            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    int progress = y * rectangle.Width + x;
                    if (backup[progress] != pixels[progress])
                    {
                        if (x < minX)
                            minX = x;
                        if (x > maxX)
                            maxX = x;
                        if (y < minY)
                            minY = y;
                        if (y > maxY)
                        {
                            maxY = y;
                            if (x == minX)
                            {
                                x += (maxX - minX) + 1;
                            }
                        }
                    }
                }
            }


            if (minX == int.MaxValue)
                minX = 0;
            if (minY == int.MaxValue)
                minY = 0;

            rectangle.X = minX;
            rectangle.Y = minY;
            int backupWidth = rectangle.Width;
            rectangle.Width = maxX - minX + 1;
            rectangle.Height = maxY - minY + 1;
            int[] result = new int[rectangle.Width * rectangle.Height];
            int counter = 0;
            for (int y = minY; y < maxY + 1; y++)
            {
                for (int x = minX; x < maxX + 1; x++)
                {
                    result[counter] = pixels[y * backupWidth + x];
                    counter++;
                }
            }


            pixels = result;
        }

        public override EncodedRectangle[] ComputeChanges(int[] backup)
        {
            List<EncodedRectangle> returnVal = new List<EncodedRectangle>();
            List<Rectangle> result = Test(backup);
            foreach(Rectangle rect in result)
            {
                int[] pix = new int[rect.Width*rect.Height];

                for (int x = 0; x < rect.Width; x++)
                    for (int y = 0; y < rect.Height; y++)
                        pix[x + y * rect.Width] = pixels[x + rect.Left + (y + rect.Top) * rectangle.Width];

                returnVal.Add(new ZrleRectangle(rfb, framebuffer, pix, rect));
            }

            return returnVal.ToArray();
        }


        public List<Rectangle> differenceImage(int[] screenShot)
        {
            int xRover = 0;
            int yRover = 0;
            int index = 0;
            int limit = 0;
            int rover = 0;

            bool isRectChanged = false;
            bool shouldSkip = false;

            List<Rectangle> rectangles = new List<Rectangle>();
            Rectangle rect = Rectangle.Empty;


            // xRover - Rovers over the height of 2D Array
            // yRover - Rovers over the width of 2D Array
            int verticalLimit = rectangle.Height;
            int horizontalLimit =  rectangle.Width;

            for (xRover = 0; xRover < verticalLimit; xRover += 1)
            {
                for (yRover = 0; yRover < horizontalLimit; yRover += 1)
                {

                    if (pixels[xRover * rectangle.Height + yRover] != screenShot[xRover * rectangle.Height + yRover])
                    {
                        // Skip over the already processed Rectangles
                        foreach (Rectangle itrRect in rectangles)
                        {
                            if (((xRover < itrRect.X + itrRect.Height) && (xRover >= itrRect.X)) && ((yRover < itrRect.Y + itrRect.Width) && (yRover >= itrRect.Y)))
                            {
                                shouldSkip = true;
                                yRover = itrRect.Y + itrRect.Width - 1;
                                break;
                            }
                        }

                        if (shouldSkip)
                        {
                            shouldSkip = false;
                            // Need to come out of the if condition as below that is why "continue" has been provided
                            // if(( (xRover <= (itrRect.x + itrRect.h)) && (xRover >= itrRect.x) ) && ( (yRover <= (itrRect.y + itrRect.w)) && (yRover >= itrRect.y) ))
                            continue;
                        }

                        rect = new Rectangle();

                        rect.X = ((xRover - 1) < 0) ? 0 : (xRover - 1);
                        rect.Y = ((yRover - 1) < 0) ? 0 : (yRover - 1);
                        rect.Width = 2;
                        rect.Height = 2;

                        /* Boolean variable used to re-scan the currently found rectangle
                         for any change due to previous scanning of boundaries */
                        isRectChanged = true;

                        while (isRectChanged)
                        {
                            isRectChanged = false;
                            index = 0;


                            /*      I      */
                            /* Scanning of left-side boundary of rectangle */
                            index = rect.X;
                            limit = rect.X + rect.Height;
                            while (index < limit && rect.Y != 0)
                            {
                                if (pixels[index * rectangle.Height + rect.Y] != screenShot[index * rectangle.Height + rect.Y])
                                {
                                    isRectChanged = true;
                                    rect.Y = rect.Y - 1;
                                    rect.Width = rect.Width + 1;
                                    index = rect.X;
                                    continue;
                                }

                                index = index + 1; ;
                            }


                            /*      II      */
                            /* Scanning of bottom boundary of rectangle */
                            index = rect.Y;
                            limit = rect.Y + rect.Width;
                            while ((index < limit) && (rect.X + rect.Height != verticalLimit))
                            {
                                rover = rect.X + rect.Height - 1;
                                if (pixels[rover * rectangle.Height + index] != screenShot[rover * rectangle.Height + index])
                                {
                                    isRectChanged = true;
                                    rect.Height = rect.Height + 1;
                                    index = rect.Y;
                                    continue;
                                }

                                index = index + 1;
                            }


                            /*      III      */
                            /* Scanning of right-side boundary of rectangle */
                            index = rect.X;
                            limit = rect.X + rect.Height;
                            while ((index < limit) && (rect.Y + rect.Width != horizontalLimit))
                            {
                                rover = rect.Y + rect.Width - 1;
                                if (pixels[index * rectangle.Height + rover] != screenShot[index * rectangle.Height + rover])
                                {
                                    isRectChanged = true;
                                    rect.Width = rect.Width + 1;
                                    index = rect.X;
                                    continue;
                                }

                                index = index + 1;
                            }

                        }


                        // Remove those rectangles that come inside "rect" rectangle.
                        int idx = 0;
                        while (idx < rectangles.Count)
                        {
                            Rectangle r = rectangles[idx];
                            if (((rect.X <= r.X) && (rect.X + rect.Height >= r.X + r.Height)) && ((rect.Y <= r.Y) && (rect.Y + rect.Width >= r.Y + r.Width)))
                            {
                                rectangles.Remove(r);
                            }
                            else
                            {
                                idx += 1;
                            }
                        }

                        // Giving a head start to the yRover when a rectangle is found
                        rectangles.Insert(0, rect);

                        yRover = rect.Y + rect.Width - 1;
                        rect = Rectangle.Empty;

                    }
                }
            }

            return rectangles;
        }

        public List<Rectangle> Test(int[] backup)
        {
            List<Rectangle> rects = new List<Rectangle>();
            for (int y = 0; y < rectangle.Height; y+= TILE_HEIGHT)
            {
                for (int x = 0; x < rectangle.Width; x += TILE_WIDTH)
                {
                    bool rectAdded = false;
                    for (int suby = 0; suby < TILE_HEIGHT &&( y + suby) < rectangle.Height; suby++)
                    {
                        for (int subx = 0; subx < TILE_WIDTH && (x + subx) < rectangle.Width ; subx++)
                        {
                            if(pixels[x + subx + (y + suby)*rectangle.Width]!=backup[x + subx + (y + suby)*rectangle.Width])
                            {
                                rects.Add(new Rectangle(x, y, (x + TILE_WIDTH) >= rectangle.Width ? rectangle.Width - x : TILE_WIDTH, (y + TILE_HEIGHT) >= rectangle.Height ? rectangle.Height - y : TILE_HEIGHT));
                                rectAdded = true;
                                break;
                            }
                        }
                        if (rectAdded)
                            break;
                    }
                }
            }

            if (rects.Count == 0)
                rects.Add(new Rectangle(0, 0, 0, 0));
            else
            {
                bool somethingChanged = true;
                while (somethingChanged)
                {
                    somethingChanged = false;
                    for(int i=0; i< rects.Count; i++)
                    {
                        Rectangle current = rects[i];
                        for(int j=i+1; j<rects.Count; j++)
                        {
                            if(rects[j].Top == current.Top && rects[j].Height == current.Height && rects[j].Left == current.Right)
                            {
                                current.Width += rects[j].Width;
                                rects[i] = current;
                                rects.RemoveAt(j);
                                j--;
                                somethingChanged = true;
                            }
                            if(j>=0 && rects[j].Left == current.Left && rects[j].Width == current.Width && rects[j].Top == current.Bottom)
                            {
                                
                                current.Height += rects[j].Height;
                                rects[i] = current;
                                rects.RemoveAt(j);
                                j--;
                                somethingChanged = true;
                            }
                        }
                    }
                }
            }

            return rects;
        }
    }
}