using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoDemonTransform
{
    public class ImageUtils
    {
        public static Bitmap CreateImageFromArrayFast(int height, int width, byte[,] array)
        {
            Bitmap bmp = new Bitmap(width, height);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                // Check this is not a null area
                
                // Go through the draw area and set the pixels as they should be
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // layer.GetBitmap().SetPixel(x, y, m_colour);
                        ptr[(x * 3) + y * stride] = array[x, y];
                        ptr[(x * 3) + y * stride + 1] = array[x + 1, y];
                        ptr[(x * 3) + y * stride + 2] = array[x + 2, y];
                    }
                }
                
            }
            bmp.UnlockBits(data);

            return bmp;
        }

        public static Bitmap CreateImageFromArray(int height, int width, byte[,] array)
        {
            //int height = array.Length / width;
            Bitmap bmp = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                int i = 0;
                int x = 0;
                while (i < width * 3)
                {
                    int B = array[y, i];
                    int G = array[y, i + 1];
                    int R = array[y, i + 2];

                    bmp.SetPixel(x, y, Color.FromArgb(R, G, B));

                    i = i + 3;
                    x++;
                }

                /*for (int x = 0; x < array.Length; x += width)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(array[i]));
                }*/
            }
            return bmp;
        }
        private byte GetBitsPerPixel(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return 24;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;
                    break;
                default:
                    throw new ArgumentException("Only 24 and 32 bit images are supported");

            }
        }

        /*Note unsafe keyword*/
        public unsafe byte[,] ThresholdUA(Bitmap image)
        {
            byte[,] matrixResult = new byte[image.Width * 3, image.Height];

            var result = new List<byte>();
            var matrix = new List<byte[]>();
            //Bitmap b = new Bitmap(_image);//note this has several overloads, including a path to an image
            int row = 0;
            int col = 0;

            try
            {
                BitmapData bData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

                byte bitsPerPixel = GetBitsPerPixel(bData.PixelFormat);

                /*This time we convert the IntPtr to a ptr*/
                byte* scan0 = (byte*)bData.Scan0.ToPointer();

                

                for (int i = 0; i < bData.Height; ++i)
                {                    
                    row = 0;
                    for (int j = 0; j < bData.Width; ++j)
                    {
                        byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
                        byte r = data[0];
                        byte g = data[1];
                        byte b = data[2];

                        //data is a pointer to the first byte of the 3-byte color data

                        if (j == 0 && i == 0)
                        {
                            matrixResult[row, col] = (byte)(r - 1);
                            matrixResult[row+1, col] = (byte)(g - 1);
                            matrixResult[row+2, col] = (byte)(b - 1);                            
                        }
                        else
                        {                            
                            matrixResult[row, col] = r;
                            matrixResult[row + 1, col] = g;
                            matrixResult[row + 2, col] = b;
                        }
                        row++;
                        
                    }
                    col++;
                    
                }

                image.UnlockBits(bData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());                
            }

            //return RotateMatrix(CreateRectangularArray(matrix), image.Width * 3, image.Height);
            return matrixResult;
        }
        public static byte[,] GetByteArray(Bitmap img)
        {
            var result = new List<byte>();
            var matrix = new List<byte[]>();
            
            int i = 0;
            int j = 0;
            try
            {
                for (i = 0; i < img.Height; i++) //rows
                {
                    result = new List<byte>();

                    for (j = 0; j < img.Width; j++) //cols
                    {
                        Color pixel = img.GetPixel(j, i);
                        if (j == 0 && i == 0)
                        {
                            result.Add((byte)(pixel.B - 1));
                            result.Add((byte)(pixel.G - 1));
                            result.Add((byte)(pixel.R - 1));
                        }
                        else
                        {
                            result.Add(pixel.B);
                            result.Add(pixel.G);
                            result.Add(pixel.R);
                        }

                    }
                    matrix.Add(result.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("x = {0}, y = {1}", i, j));
                Debug.WriteLine(ex.ToString());
            }

            return RotateMatrix(CreateRectangularArray(matrix), img.Width * 3, img.Height);
        }

        static T[,] CreateRectangularArray<T>(IList<T[]> arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int minorLength = arrays[0].Length;
            T[,] ret = new T[arrays.Count, minorLength];
            for (int i = 0; i < arrays.Count; i++)
            {
                var array = arrays[i];
                if (array.Length != minorLength)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int j = 0; j < minorLength; j++)
                {
                    ret[i, j] = array[j];
                }
            }
            return ret;
        }

        public static byte[,] RotateMatrix(byte[,] matrix, int cols, int rows)
        {
            byte[,] ret = new byte[cols, rows];

            for (int r = 0; r < rows; ++r)
            {
                for (int c = 0; c < cols; ++c)
                {
                    ret[c, r] = matrix[r, c];
                }
            }

            return ret;
        }
    }
}