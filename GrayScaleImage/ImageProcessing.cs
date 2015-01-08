using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LassoTool
{
    class ImageProcessing
    {
        //Applying convolution matrix to image
        public int[] MatrixSv(byte[] originalImage, int height, int width, double[,] ma)
        {
            double[,] kern = ma; //Convolution kernel
            int[,] rf = ByteMeths.LinArrToMatrix(ByteMeths.BToInt(originalImage), height, width); //Reference image
            int[,] proc = new int[height + 2, width + 2]; //Processed image

            //Fills processed image
            for (int i = 1; i < height + 1; ++i)
            {
                for (int k = 1; k < width + 1; ++k)
                {
                    int tmp = 0;
                    for (int j = -1; j < 2; ++j)
                        for (int l = -1; l < 2; ++l)
                            tmp += (int)rf[i + j, k + l] * (int)kern[j + 1, l + 1];
                    proc[i, k] = tmp;
                }
            }

            return ByteMeths.MatrixToLinArr(proc);
        }

        //sets Grayscale
        public byte[] Grsc(byte[] orig)
        {
            byte[] tmp = new byte[orig.Length / 4];
            for (int i = 0; i < tmp.Length; ++i)
            {
                byte tmps = (byte)(0.299 * orig[4 * i + 2] + 0.587 * orig[4 * i + 1] + 0.114 * orig[4 * i]);
                tmp[i] = tmps;
            }
            return tmp;
        }

        // sets Magic
        public byte[] setMagic(byte[] originalImage, int H, int W, out ImGraph imcur)
        {
            imcur = new ImGraph(setSobel(originalImage, H, W), H, W); //Creates image graph instance 
            return originalImage;
        }

        // shows only red channel
        public byte[] setSobel(byte[] originalImage, int H, int W)
        {
            byte[] gs = Grsc(originalImage); //Converts image to graycale

            //Sets convolution masks for x and y
            double[,] ykern = new double[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            double[,] xkern = new double[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };

            //Uses them to fill array
            int[] xk = MatrixSv(gs, H, W, xkern);
            int[] yk = MatrixSv(gs, H, W, ykern);

            byte[] retm = new byte[originalImage.Length / 4]; // Creating merged array

            //Merging them int one image
            for (int i = 0; i < xk.Length; ++i)
                retm[i] = ByteMeths.checkPixelValue((int)Math.Sqrt((int)xk[i] * (int)xk[i] + (int)yk[i] * (int)yk[i]));
            return retm;
        }
    }
}
