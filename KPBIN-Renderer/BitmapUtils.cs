using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPBIN_Renderer
{
    public static class BitmapUtils
    {
        public static Bitmap RGB5A3Decode(byte[] data, int width, int height)
        {
            Bitmap output = new Bitmap(width, height);

            int i = 0;
            for (int ytile = 0; ytile < height; ytile += 4)
            {
                for (int xtile = 0; xtile < width; xtile += 4)
                {
                    for (int ypixel = ytile; ypixel < ytile + 4; ypixel++)
                    {
                        for (int xpixel = xtile; xpixel < xtile + 4; xpixel++)
                        {
                            if (i >= data.Length) { continue; }
                            uint newpixel = ((uint)data[i] << 8) | (uint)data[i + 1];

                            uint red, green, blue, alpha;
                            if ((newpixel & 0x8000) > 0) // Check if it's RGB555
                            {
                                red = ((newpixel >> 10) & 0x1F) * 255 / 0x1F;
                                green = ((newpixel >> 5) & 0x1F) * 255 / 0x1F;
                                blue = (newpixel & 0x1F) * 255 / 0x1F;
                                alpha = 0xFF;
                            }
                            else // If not, it's RGB5A3
                            {
                                alpha = ((newpixel & 0x7000) >> 12) * 255 / 0x7;
                                red = ((newpixel & 0xF00) >> 8) * 255 / 0xF;
                                green = ((newpixel & 0xF0) >> 4) * 255 / 0xF;
                                blue = (newpixel & 0xF) * 255 / 0xF;
                            }

                            i += 2;

                            if (xpixel < width && ypixel < height)
                                output.SetPixel(xpixel, ypixel, Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue));
                        }
                    }
                }
            }

            return output;
        }

        public static Bitmap RGBA8Decode(byte[] data, int width, int height)
        {
            Bitmap output = new Bitmap(width, height);

            int i = 0;
            for (int ytile = 0; ytile < height; ytile += 4)
            {
                for (int xtile = 0; xtile < width; xtile += 4)
                {
                    List<int> A = new List<int>();
                    List<int> R = new List<int>();
                    List<int> G = new List<int>();
                    List<int> B = new List<int>();

                    if (i + 64 > data.Length) { continue; }

                    for (int AR = 0; AR < 16; AR++)
                    {
                        A.Add(data[i]);
                        R.Add(data[i + 1]);
                        i += 2;
                    }
                    for (int GB = 0; GB < 16; GB++)
                    {
                        G.Add(data[i]);
                        B.Add(data[i + 1]);
                        i += 2;
                    }


                    int j = 0;
                    for (int ypixel = ytile; ypixel < ytile + 4; ypixel++)
                    {
                        for (int xpixel = xtile; xpixel < xtile + 4; xpixel++)
                        {
                            if (xpixel < width && ypixel < height)
                                output.SetPixel(xpixel, ypixel, Color.FromArgb(A[j], R[j], G[j], B[j]));

                            j++;
                        }
                    }
                }
            }

            return output;
        }
    }


    public static class BitmapExtensions
    {
        public static void PlaceTile(this Bitmap fullpic, Bitmap tileset, int tileID, int x, int y)
        {
            int tileX = (tileID % 32) * 24;
            int tileY = (tileID / 32) * 24;

            fullpic.PlaceBitmapPieceAt(tileset, tileX, tileY, 24, 24, x, y);
        }

        public static void BlendBitmap(this Bitmap fullpic, Bitmap bitmap, int x, int y)
        {
            Graphics g = Graphics.FromImage(fullpic);
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(bitmap, new Point(x, y));
        }

        public static void PlaceBitmapPieceAt(this Bitmap fullpic, Bitmap bitmap, int placeX, int placeY, int sizeX, int sizeY, int atX, int atY)
        {
            for (int x = atX; x < atX + sizeX; x++)
            {
                for (int y = atY; y < atY + sizeY; y++)
                {
                    fullpic.SetPixel(x, y, bitmap.GetPixel(placeX + (x - atX), placeY + (y - atY)));
                }
            }
        }

        public static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        public static Bitmap RotateImage(Bitmap image, float angle) // https://stackoverflow.com/a/4320581/
        {
            const double pi2 = Math.PI / 2.0;


            double oldWidth = (double)image.Width;
            double oldHeight = (double)image.Height;

            // Convert degrees to radians
            double theta = ((double)angle) * Math.PI / 180.0;
            double locked_theta = theta;

            // Ensure theta is now [0, 2pi)
            while (locked_theta < 0.0)
                locked_theta += 2 * Math.PI;

            double newWidth, newHeight;
            int nWidth, nHeight; // The newWidth/newHeight expressed as ints



            double adjacentTop, oppositeTop;
            double adjacentBottom, oppositeBottom;


            if ((locked_theta >= 0.0 && locked_theta < pi2) ||
                (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2)))
            {
                adjacentTop = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
                oppositeTop = Math.Abs(Math.Sin(locked_theta)) * oldWidth;

                adjacentBottom = Math.Abs(Math.Cos(locked_theta)) * oldHeight;
                oppositeBottom = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
            }
            else
            {
                adjacentTop = Math.Abs(Math.Sin(locked_theta)) * oldHeight;
                oppositeTop = Math.Abs(Math.Cos(locked_theta)) * oldHeight;

                adjacentBottom = Math.Abs(Math.Sin(locked_theta)) * oldWidth;
                oppositeBottom = Math.Abs(Math.Cos(locked_theta)) * oldWidth;
            }

            newWidth = adjacentTop + oppositeBottom;
            newHeight = adjacentBottom + oppositeTop;

            nWidth = (int)Math.Ceiling(newWidth);
            nHeight = (int)Math.Ceiling(newHeight);

            Bitmap rotatedBmp = new Bitmap(nWidth, nHeight);

            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {

                Point[] points;

                if (locked_theta >= 0.0 && locked_theta < pi2)
                {
                    points = new Point[] {
                                             new Point( (int) oppositeBottom, 0 ),
                                             new Point( nWidth, (int) oppositeTop ),
                                             new Point( 0, (int) adjacentBottom )
                                         };

                }
                else if (locked_theta >= pi2 && locked_theta < Math.PI)
                {
                    points = new Point[] {
                                             new Point( nWidth, (int) oppositeTop ),
                                             new Point( (int) adjacentTop, nHeight ),
                                             new Point( (int) oppositeBottom, 0 )
                                         };
                }
                else if (locked_theta >= Math.PI && locked_theta < (Math.PI + pi2))
                {
                    points = new Point[] {
                                             new Point( (int) adjacentTop, nHeight ),
                                             new Point( 0, (int) adjacentBottom ),
                                             new Point( nWidth, (int) oppositeTop )
                                         };
                }
                else
                {
                    points = new Point[] {
                                             new Point( 0, (int) adjacentBottom ),
                                             new Point( (int) oppositeBottom, 0 ),
                                             new Point( (int) adjacentTop, nHeight )
                                         };
                }

                g.DrawImage(image, points);
            }

            return rotatedBmp;
        }

        public static Bitmap SetImageOpacity(Bitmap image, byte alpha) // https://stackoverflow.com/a/4779371/
        {
            float opacity = alpha / 255.0f;
            //create a Bitmap the size of the image provided  
            Bitmap bmp = new Bitmap(image.Width, image.Height);

            //create a graphics object from the image  
            using (Graphics gfx = Graphics.FromImage(bmp))
            {

                //create a color matrix object  
                ColorMatrix matrix = new ColorMatrix();

                //set the opacity  
                matrix.Matrix33 = opacity;

                //create image attributes  
                ImageAttributes attributes = new ImageAttributes();

                //set the color(opacity) of the image  
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                //now draw the image  
                gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        public static void DrawLineInt(this Bitmap bmp, Color color, Point p1, Point p2, int size)
        {
            Pen blackPen = new Pen(color, size);

            // Draw line to screen.
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawLine(blackPen, p1.X, p1.Y, p2.X, p2.Y);
            }
        }

        private static byte[,] ptShape =
        {
                {0, 0, 0, 0, 1, 0, 0, 0, 0 },
                {0, 0, 0, 1, 2, 1, 0, 0, 0 },
                {0, 0, 1, 2, 2, 2, 1, 0, 0 },
                {0, 1, 2, 2, 2, 2, 2, 1, 0 },
                {1, 2, 2, 2, 2, 2, 2, 2, 1 },
                {0, 1, 2, 2, 2, 2, 2, 1, 0 },
                {0, 0, 1, 2, 2, 2, 1, 0, 0 },
                {0, 0, 0, 1, 2, 1, 0, 0, 0 },
                {0, 0, 0, 0, 1, 0, 0, 0, 0 }
            };

        private static byte[,] sShape =
        {
                {0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
                {0, 0, 0, 1, 1, 2, 2, 2, 2, 2, 1, 1, 0, 0, 0 },
                {0, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0, 0 },
                {0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0 },
                {0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0 },
                {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1 },
                {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1 },
                {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1 },
                {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1 },
                {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1 },
                {0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0 },
                {0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0 },
                {0, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0, 0 },
                {0, 0, 0, 1, 1, 2, 2, 2, 2, 2, 1, 1, 0, 0, 0 },
                {0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 }
            };

        public static void DrawNode(this Bitmap bmp, Color color, Point p, byte[,] shape)
        {
            int x = p.X;
            int y = p.Y;

            for (int dx = 0; dx < shape.GetLength(0); dx++)
            {
                for (int dy = 0; dy < shape.GetLength(1); dy++)
                {
                    byte val = shape[dx, dy];
                    if (val > 0)
                    {
                        Color toUse = (val == 1) ? Color.Black : color;
                        bmp.SetPixel(x + (dx - ((shape.GetLength(0) - 1) / 2)), y + (dy - ((shape.GetLength(1) - 1) / 2)), toUse);
                    }
                }
            }
        }

        public static void DrawPTNode(this Bitmap bmp, Color color, Point p)
        {
            bmp.DrawNode(color, p, ptShape);
        }

        public static void DrawSNode(this Bitmap bmp, Color color, Point p)
        {
            bmp.DrawNode(color, p, sShape);
        }

        public static void WriteTextAt(this Bitmap bmp, Rectangle rect, string font, Color color, string txt)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (SolidBrush myBrush = new SolidBrush(color))
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawString(txt, new Font(font, 10, FontStyle.Bold), myBrush, rect, sf);
                }
            }
        }
    }
}
