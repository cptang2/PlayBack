using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PlayBack
{
    //Code modified from http://www.codeproject.com/Tips/240428/Work-with-bitmap-faster-with-Csharp
    class ConvertBitmap
    {
        public static byte[] getBytes(Bitmap source)
        {
            try
            {
                // Get width and height of bitmap
                int Width = source.Width;
                int Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                int Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                BitmapData bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                byte[] Pixels = new byte[PixelCount * step];
                IntPtr Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);

                source.UnlockBits(bitmapData);
                return Pixels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
