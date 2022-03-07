using System.Collections.Generic;
using System.Drawing;

//Возможно, временное название, но лучше пока не могу придумать. 
public static class BackgroundWorkerHelper
{
    public static void FillInputData(List<float> InputData, byte[,] peripheralArray, byte[,] pixels, int focusSize, int X, int Y)
    {
        InputData.Clear();

        for (int i = 0; i < focusSize; ++i)
        {
            for (int j = 0; j < focusSize; ++j)
            {
                InputData.Add(pixels[i + Y, j + X]); // Запись во входящий вектор фокуса зрения                                                                    
            }
        }

        { //Дабавление перефирийное зрение к входящиму вектору
            for (int i = 0; i < 12; ++i)
            {
                for (int j = 0; j < 12; ++j)
                {
                    InputData.Add(peripheralArray[j, i] > 200 ? 0 : 1);
                }
            }
        }
    }

    public static byte[,] CreateFocusArray(int X, int Y, byte[,] pixels, int focusSize)
    {
        byte[,] pixels_ = new byte[28, 28];
        for (int i = 0; i <= focusSize; i++)
        {
            for (int j = 0; j <= focusSize; j++)
            {
                pixels_[i + 6, j + 6] = pixels[i + Y, j + X];
            }
        }
        return pixels_;
    }

    //TODO: отразить в нейминге различия с методом ниже (сейчас здесь и там используется слово Image). Done, изменил 
    //LimitImageByThreshold на FormArrayByThreshold ниже, но, возможно, нужно будет переделать ещё раз. Done дважды,
    //изменил всюду Array на Pixels как более описательный термин. 
    public static void PixelsFromImage(Bitmap b2, byte[,] pixels)
    {
        for (int i = 0; i < 12; ++i)
        {
            for (int j = 0; j < 12; ++j)
            {
                pixels[j, i] = b2.GetPixel(j, i).R;
            }
        }
    }

    //TODO: механически выделил, изменить аргументы после. Done. 
    //TODO: возможно, заменить BinaryReader в аргументе на массив байтов с исходным изображением через ReadBytes() снаружи. Done. 
    public static void FormPixelsByThreshold(List<byte> InputDataBuf, byte[,] pixels, byte[] sourceImage, int threshold)
    {
        InputDataBuf.Clear();
        for (int i = 0; i < 28; ++i)// Исходное изображение переписывается в массив с порогом яркости
        {
            for (int j = 0; j < 28; ++j)
            {
                byte b = sourceImage[28 * i + j];
                //byte b = sourceImage.ReadByte();
                pixels[i, j] = b > threshold ? (byte)1 : (byte)0;
                InputDataBuf.Add(b > threshold ? (byte)1 : (byte)0); //TODO: уточнить, зачем нужен InputDataBuf. Done. 
            }
        }
    }
}
