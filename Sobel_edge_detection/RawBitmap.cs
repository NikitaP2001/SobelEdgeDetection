using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;

namespace Sobel
{
    [Serializable]
    class RawBitmap
    {
        public RawBitmap(Bitmap source)
        {
            m_width = source.Width;
            m_height = source.Height;
            BitmapData srcData = source.LockBits(new Rectangle(0, 0, m_width, m_height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            m_stride = srcData.Stride;
            int count = m_stride * srcData.Height;
            m_data = new byte[count];

            IntPtr srcScan0 = srcData.Scan0;
            Marshal.Copy(srcScan0, m_data, 0, count);
            source.UnlockBits(srcData);
        }

        public RawBitmap(RawBitmap source, int parts, int idx)
        {
            if (idx >= parts || idx < 0)
                throw new ArgumentOutOfRangeException("Wrong slice index");
            int partHeight = source.m_height / parts;
            m_height = partHeight;
            if (idx + 1 == parts)
                m_height += source.m_height % parts;

            m_stride = source.m_stride;
            m_width = source.m_width;
            m_data = new byte[m_stride * m_height];
            int sInd = partHeight * m_stride * idx;
            Array.Copy(source.m_data, sInd, m_data, 0, m_data.Length);
        }

        public void writePart(RawBitmap part, int parts, int idx)
        {
            if (idx >= parts || idx < 0)
                throw new ArgumentOutOfRangeException("Wrong slice index");
            int partHeight = m_height / parts;
            int sInd = partHeight * m_stride * idx;
            Array.Copy(part.m_data, 0, m_data, sInd, part.m_data.Length);
        }

        public Bitmap ToBitmap()
        {
            Bitmap result = new Bitmap(m_width, m_height);
            BitmapData resultData = result.LockBits(new Rectangle(0, 0, m_width, m_height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(m_data, 0, resultData.Scan0, m_data.Length);
            result.UnlockBits(resultData);
            return result;
        }

        public void edgeDetect()
        {
            double xg = 0.0;
            double yg = 0.0;
            double gt = 0.0;
            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

            byte[] resultBuffer = new byte[m_data.Length];

            //Start with the pixel that is offset 1 from top and 1 from the left side
            //this is so entire kernel is on our image
            for (int offsetY = filterOffset; offsetY < m_height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < m_width - filterOffset; offsetX++)
                {
                    xg = yg = 0;
                    gt = 0.0;
                    /* position of the kernel center pixel */
                    byteOffset = offsetY * m_stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * m_stride;
                            xg += (double)(m_data[calcOffset + 1]) * m_skXKernel[filterY
                                + filterOffset, filterX + filterOffset];
                            yg += (double)(m_data[calcOffset + 1]) * m_skYKernel[filterY
                                + filterOffset, filterX + filterOffset];
                        }
                    }
                    /* total rgb values for this pixel */
                    gt = Math.Sqrt((xg * xg) + (yg * yg));
                    if (gt > 255)
                        gt = 255;
                    else if (gt < 0)
                        gt = 0;

                    /* set new data in the other byte array for output image data */
                    resultBuffer[byteOffset] = (byte)(gt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(gt);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }
            m_data = resultBuffer;
        }

        public void grayScale()
        {
            float rgb = 0;
            for (int i = 0; i < m_data.Length; i += 4)
            {
                rgb = m_data[i] * .21f;
                rgb += m_data[i + 1] * .72f;
                rgb += m_data[i + 2] * .071f;

                m_data[i] = (byte)rgb;
                m_data[i + 1] = m_data[i];
                m_data[i + 2] = m_data[i];
                m_data[i + 3] = 255;
            }
        }

        private static readonly int[,] m_skXKernel = {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

        private static readonly int[,] m_skYKernel = {
            { 1, 2, 1 },
            { 0, 0, 0 },
            { -1, -2, -1 }
        };

        byte[] m_data;
        int m_stride;
        int m_width;
        int m_height;
    }
}
