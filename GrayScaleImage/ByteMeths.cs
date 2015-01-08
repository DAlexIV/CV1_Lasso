using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LassoTool
{
    class ByteMeths
    {
        //Converts linear array with image into matrix
        public static int[,] LinArrToMatrix(int[] img, int H, int W)
        {
            int[,] ret = new int[H + 2, W + 2];

            for (int i = 0; i < img.Length; ++i)
            {
                ret[i / W + 1, i % W + 1] = img[i];
            }
            return ret;
        }
        //Converts matrix with image into linear array
        public static int[] MatrixToLinArr(int[,] m)
        {
            int[] ret = new int[(m.GetLength(0) - 2) * (m.GetLength(1) - 2)];
            int last = 0;
            for (int i = 1; i < m.GetLength(0) - 1; ++i)
                for (int k = 1; k < m.GetLength(1) - 1; ++k, last++)
                {
                    ret[last] = m[i, k];
                }
            return ret;
        }

        //Converts byte array into int array
        public static int[] BToInt(byte[] arr)
        {
            int[] ret = new int[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
                ret[i] = arr[i];
            return ret;
        }

        //Converts int array into byte array
        public static byte[] IntToB(int[] arr)
        {
            byte[] ret = new byte[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
                ret[i] = checkPixelValue(arr[i]);
            return ret;
        }
        
        //Converts pixel value into byte
        public static byte checkPixelValue(int pixel)
        {
            if (pixel > 255)
                return 255;
            else if (pixel < 0)
                return 0;
            else
                return (byte)pixel;
        }
    }
}
