using System;
using System.Drawing.Imaging;
using System.IO;

namespace PhotoDemonTransform
{
    public class FilterSupport
    {
        private long m_FinalX;
        private long m_FinalY;
        private EDGE_OPERATOR m_EdgeMethod;
        private long m_ColorDepth;
        private long m_ColorDepthLoop;
        private bool m_Interpolate;
        private long m_DstX;
        private long m_DstY;

        public double Modulo(double Quotient, double Divisor)
        {
            double functionReturnValue = 0;
            functionReturnValue = Quotient - (Quotient / Divisor) * Divisor;
            if (functionReturnValue < 0)
                functionReturnValue = functionReturnValue + Divisor;
            return functionReturnValue;
        }

        public void setDistortParameters(long imgColorDepth, EDGE_OPERATOR edgeMethod, bool toInterpolate, long finalX, long finalY)
        {

            m_ColorDepth = imgColorDepth;
            m_ColorDepthLoop = m_ColorDepth - 1;
            m_EdgeMethod = edgeMethod;
            m_Interpolate = toInterpolate;
            m_FinalX = finalX;
            m_FinalY = finalY;

        }

        public bool FixDistortEdges(double srcX, double srcY)
        {
            bool functionReturnValue = false;

            switch (m_EdgeMethod)
            {

                //Clamp
                case EDGE_OPERATOR.EDGE_CLAMP:

                    if (srcX < 0)
                        srcX = 0;
                    if (srcY < 0)
                        srcY = 0;
                    if (srcX > m_FinalX)
                        srcX = m_FinalX;
                    if (srcY > m_FinalY)
                        srcY = m_FinalY;
                    break;

                //Reflect
                case EDGE_OPERATOR.EDGE_REFLECT:

                    if (srcX < 0)
                        srcX = Math.Abs(srcX);
                    if (srcY < 0)
                        srcY = Math.Abs(srcY);
                    if (srcX > m_FinalX)
                        srcX = m_FinalX - (srcX - m_FinalX);
                    if (srcY > m_FinalY)
                        srcY = m_FinalY - (srcY - m_FinalY);

                    //If the modified pixel STILL lies outside the image, use modulo to move it in-bounds
                    if (srcX < 0)
                        srcX = Modulo(srcX, (m_FinalX + 1));
                    if (srcY < 0)
                        srcY = Modulo(srcY, (m_FinalY + 1));
                    if (srcX > m_FinalX)
                        srcX = Modulo(srcX, (m_FinalX + 1));
                    if (srcY > m_FinalY)
                        srcY = Modulo(srcY, (m_FinalY + 1));
                    break;

                //Wrap
                case EDGE_OPERATOR.EDGE_WRAP:

                    if (srcX < 0)
                        srcX = Modulo(srcX, (m_FinalX + 1));
                    if (srcY < 0)
                        srcY = Modulo(srcY, (m_FinalY + 1));
                    if (srcX > m_FinalX)
                        srcX = Modulo(srcX, (m_FinalX + 1));
                    if (srcY > m_FinalY)
                        srcY = Modulo(srcY, (m_FinalY + 1));
                    break;

                //Erase
                case EDGE_OPERATOR.EDGE_ERASE:

                    if (srcX < 0)
                    {
                        functionReturnValue = true;
                        return functionReturnValue;
                    }

                    if (srcY < 0)
                    {
                        functionReturnValue = true;
                        return functionReturnValue;
                    }

                    if (srcX > m_FinalX)
                    {
                        functionReturnValue = true;
                        return functionReturnValue;
                    }

                    if (srcY > m_FinalY)
                    {
                        functionReturnValue = true;
                        return functionReturnValue;
                    }
                    break;

                case EDGE_OPERATOR.EDGE_ORIGINAL:
                    if (srcX < 0)
                    {
                        srcX = m_DstX;
                        srcY = m_DstY;
                    }

                    if (srcY < 0)
                    {
                        srcX = m_DstX;
                        srcY = m_DstY;
                    }

                    if (srcX > m_FinalX)
                    {
                        srcX = m_DstX;
                        srcY = m_DstY;
                    }

                    if (srcY > m_FinalY)
                    {
                        srcX = m_DstX;
                        srcY = m_DstY;
                    }
                    break;

            }

            functionReturnValue = false;
            return functionReturnValue;

        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, format);
                return ms.ToArray();
            }
        }

        public int[,] ImageToIntMatrix(System.Drawing.Image imageIn, ImageFormat format)
        {
            var array = imageToByteArray(imageIn, format);
            return To2dArray(array, imageIn.Height);
        }

        public byte[,] ImageToByteMatrix(System.Drawing.Image imageIn, ImageFormat format)
        {
            var array = imageToByteArray(imageIn, format);
            return To2dByteArray(array, imageIn.Height);
        }

        public int[,] ConvertIntArray(byte[] Input, int size)
        {
            int[,] Output = new int[(int)(Input.Length / size), size];
            //System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\OutFile.txt");
            for (int i = 0; i < Input.Length; i += size)
            {
                for (int j = 0; j < size; j++)
                {
                    Output[(int)(i / size), j] = Input[i + j];
                    //      sw.Write(Input[i + j]);
                }
                //                sw.WriteLine("");
            }
            //          sw.Close();
            return Output;
        }

        public byte[,] To2dByteArray(byte[] source, int width)
        {
            int height = source.Length / width;
            byte[,] result = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = source[i * width + j];
                }
            }
            return result;
        }

        public int[,] To2dArray(byte[] source, int width)
        {
            int height = source.Length / width;
            int[,] result = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = source[i * width + j];
                }
            }
            return result;
        }

        public byte[,] ConvertArray(byte[] Input, int size)
        {
            byte[,] Output = new byte[(int)(Input.Length / size), size];
            //System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\OutFile.txt");
            for (int i = 0; i < Input.Length; i += size)
            {
                for (int j = 0; j < size; j++)
                {
                    Output[(int)(i / size), j] = Input[i + j];
                    //      sw.Write(Input[i + j]);
                }
                //                sw.WriteLine("");
            }
            //          sw.Close();
            return Output;
        }

        public void getColorsFromSource(ref long r, ref long g, ref long b, ref long a, ref double srcX, ref double srcY, ref byte[,] srcData, ref long origX, ref long origY)
        {

            //Cache the original x/y values as necessary
            if (m_EdgeMethod == EDGE_OPERATOR.EDGE_ORIGINAL)
            {
                m_DstX = origX;
                m_DstY = origY;
            }

            //First, fix any coordinates that lie outside the image
            bool fixDistort = false;
            fixDistort = FixDistortEdges(srcX, srcY);

            //Now, interpolate as necessary
            long quickValX = 0;
            long quickValY = 0;

            //fixDistort will only be set to TRUE when the current pixel needs to be erased
            if (fixDistort)
            {
                r = 0;
                g = 0;
                b = 0;
                if (m_ColorDepth == 4)
                    a = 0;
            }
            else
            {

                //Interpolate a new pixel value
                if (m_Interpolate)
                {

                    r = pInterpolate(srcX, srcY, srcData, 2);
                    g = pInterpolate(srcX, srcY, srcData, 1);
                    b = pInterpolate(srcX, srcY, srcData, 0);
                    if (m_ColorDepth == 4)
                        a = pInterpolate(srcX, srcY, srcData, 3);

                    //Round to the nearest coordinate value
                }
                else
                {

                    quickValX = Convert.ToInt32(srcX) * m_ColorDepth;
                    quickValY = Convert.ToInt32(srcY);

                    r = srcData[quickValX + 2, quickValY];
                    g = srcData[quickValX + 1, quickValY];
                    b = srcData[quickValX, quickValY];
                    if (m_ColorDepth == 4)
                        a = srcData[quickValX + 3, quickValY];

                }

            }

        }

        private byte pInterpolate(double x1, double y1, byte[,] iData, long iOffset)
        {

            //Retrieve the four surrounding pixel values
            double topLeft = 0;
            double topRight = 0;
            double bottomLeft = 0;
            double bottomRight = 0;
            topLeft = iData[(int)(Math.Truncate(x1) * m_ColorDepth + iOffset), (int)Math.Truncate(y1)];
            double fixX = 0;
            double fixY = 0;

            //Pixels at the far edges of the image require special treatment during interpolation
            if (x1 < m_FinalX)
            {
                topRight = iData[(int)(Math.Truncate(x1 + 1) * m_ColorDepth + iOffset), (int)Math.Truncate(y1)];
            }
            else
            {
                fixX = x1 + 1;
                fixY = y1;
                if (FixDistortEdges(fixX, fixY))
                {
                    topRight = 0;
                }
                else
                {
                    topRight = iData[(int)(Math.Truncate(fixX) * m_ColorDepth + iOffset), (int)Math.Truncate(y1)];
                }
            }
            if (y1 < m_FinalY)
            {

                bottomLeft = iData[(int)(Math.Truncate(x1) * m_ColorDepth + iOffset), (int)Math.Truncate(y1 + 1)];
            }
            else
            {
                fixX = x1;
                fixY = y1 + 1;
                if (FixDistortEdges(fixX, fixY))
                {
                    bottomLeft = 0;
                }
                else
                {
                    bottomLeft = iData[(int)(Math.Truncate(x1) * m_ColorDepth + iOffset), (int)Math.Truncate(fixY)];
                }
            }
            if (x1 < m_FinalX)
            {
                if (y1 < m_FinalY)
                {
                    bottomRight = iData[(int)(Math.Truncate(x1 + 1) * m_ColorDepth + iOffset), (int)Math.Truncate(y1 + 1)];
                }
                else
                {
                    fixX = x1 + 1;
                    fixY = y1 + 1;
                    if (FixDistortEdges(fixX, fixY))
                    {
                        bottomRight = 0;
                    }
                    else
                    {
                        bottomRight = iData[(int)(Math.Truncate(x1 + 1) * m_ColorDepth + iOffset), (int)Math.Truncate(fixY)];
                    }
                }
            }
            else
            {
                fixX = x1 + 1;
                fixY = y1 + 1;
                if (FixDistortEdges(fixX, fixY))
                {
                    bottomRight = 0;
                }
                else
                {
                    if (y1 < m_FinalY)
                    {
                        bottomRight = iData[(int)(Math.Truncate(fixX) * m_ColorDepth + iOffset), (int)Math.Truncate(y1 + 1)];
                    }
                    else
                    {
                        bottomRight = iData[(int)(Math.Truncate(fixX) * m_ColorDepth + iOffset), (int)Math.Truncate(fixY)];
                    }
                }
            }

            //Calculate blend ratios
            double yBlend = 0;
            double xBlend = 0;
            double xBlendInv = 0;
            yBlend = y1 - Math.Truncate(y1);
            xBlend = x1 - Math.Truncate(x1);
            xBlendInv = 1 - xBlend;

            //Blend in the x-direction
            double topRowColor = 0;
            double bottomRowColor = 0;
            topRowColor = topRight * xBlend + topLeft * xBlendInv;
            bottomRowColor = bottomRight * xBlend + bottomLeft * xBlendInv;

            //Blend in the y-direction
            return (byte)Convert.ToInt32(bottomRowColor * yBlend + topRowColor * (1 - yBlend));

        }
    }

    public enum EDGE_OPERATOR
    {
        EDGE_CLAMP = 0,
        EDGE_REFLECT = 1,
        EDGE_WRAP = 2,
        EDGE_ERASE = 3,
        EDGE_ORIGINAL = 4
    }
}