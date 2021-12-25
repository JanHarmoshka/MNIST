using System;
using System.Collections.Generic;
using System.Drawing;

namespace MNIST
{
    class PreparationInput
    {
        public bool col;
        int prevX1;
        int prevY1;
        int prevX2 = 0;
        int prevY2 = 0;

        public int bufX;
        public int bufY;
        byte[] XYBuf = { 7, 7 };

        public float n_blekc;
        public float n_green;
        public Bitmap bitMap;
        public Bitmap bitMap_Draw;

        public int X;
        public int Y;

        public List<byte> InputData = new List<byte>();

        public void PreparationInput_1(Counter counter, bool col, float semblance, int Xb, int Yb, bool reproduction, bool TabPagesBool)
        {

            double SumX = 0;
            double SumY = 0;
            int num = 0;
            n_green = 0;
            n_blekc = 0;
            Random rnd = new Random();

            InputData.Clear();

            if (counter.inter_Data_.Count >= 14 * 14)
            {
                InputData.AddRange(counter.inter_Data_);
                int n = 0;
                for (int j = 0; j < 14; ++j)
                {
                    for (int i = 0; i < 14; ++i)
                    {
                        if (InputData[n] > 0)
                        {
                            SumX += i;
                            SumY += j;
                            num += 1;
                        }
                        n++;
                    }
                }
                if (num > 0)
                {
                    X = (int)Math.Ceiling(SumX / num);
                    Y = (int)Math.Ceiling(SumY / num);
                }
            }

            X += X;//координаты зрачка
            Y += Y;

            if (prevX2 > 0 & prevY2 > 0)
            {
                if ((prevX2 == X & prevY2 == Y) | (prevX2 == Xb & prevY2 == Yb))
                {

                    if (semblance > 1)
                    {
                        semblance -= 3f;
                    }
                }
                else
                {
                    semblance = 11;
                }
            }
            else
            {
                semblance = 11;
            }

            prevX1 = prevX2;
            prevY1 = prevY2;
            if (counter.summ < 1 & counter.summ2 > 0)
            {
                X = prevX2;
                counter.summ = 1;
            }
            if (counter.summ2 < 1 & counter.summ > 0)
            {
                Y = prevY2;
                counter.summ2 = 1;
            }
            prevX2 = X;
            prevY2 = Y;

            if (counter.summ < 1 & counter.summ2 < 1 | counter.inter_Data_.Count < 1 | X > 21 | Y > 21 | X < 4 | Y < 4 | (X == Xb & Y == Yb))//counter.summ > 20 | counter.summ2 > 20 |
            {

                InputData.Clear();
                col = false;//bleck
                n_blekc++;
                n_green--;


                if (X == Xb & Y == Yb & counter.summ > 0 & counter.summ2 > 0)
                {
                    col = true;//green
                    n_blekc--;
                    n_green++;
                }
                do
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 1; ++j)
                        {
                            if (rnd.Next(0, 100) > 50)
                            {
                                if (XYBuf[i] >= 0 & XYBuf[i] < 23)
                                {
                                    XYBuf[i]++;
                                }
                            }
                            else
                            {
                                if (XYBuf[i] > 0 & XYBuf[i] <= 23)
                                {
                                    XYBuf[i]--;
                                }
                            }
                        }
                    }
                }
                while ((X == XYBuf[0] | Y == XYBuf[1]));
                X = XYBuf[0];
                Y = XYBuf[1];

                byte[,] Coordinate = new byte[14, 14];
                //byte[,] Coordinate_ = new byte[28, 28];

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        // if (i % 2 == 0 & j % 2 == 0)
                        {
                            if (i + X > 0 & i + X < 28 & j + Y > 0 & j + Y < 28)
                            {
                                Coordinate[i + Y / 2, j + X / 2] = 1;
                                //Coordinate_[i + Y, j + X] = 1;
                            }
                        }
                    }
                }
                //bitMap_Draw = MakeBitmap.Make_Bitmap(Coordinate_, 7, 7, col, 20);

                for (int i = 0; i < 14; i++)
                {
                    for (int j = 0; j < 14; j++)
                    {
                        if (Coordinate[i, j] == 1)
                        {
                            InputData.Add(1);
                        }
                        else
                        {
                            InputData.Add(0);
                        }
                    }
                }
                counter.inter_Data_.Clear();
                counter.inter_Data_.AddRange(InputData);
            }

            if ((counter.inter_Data_Full.Count > 0 & !reproduction) | (counter.inter_Data_Full.Count > 0) & TabPagesBool)//Рисовать фокус внимания 
            {
                byte[,] pixels = new byte[28, 28];
                int n = 0;
                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        pixels[i, j] = 0;
                        if (counter.inter_Data_Full[n] > 0)
                        {
                            pixels[i, j] = 1;
                        }
                        n++;
                    }
                }
                //for (int i = 0; i < 28; i++)
                //{
                //    for (int j = 12; j < 26; j++)
                //    {
                //        pixels[i, j] = 0;

                //        if (counter.inter_Data_Full[n] > 0)
                //        {
                //            pixels[i, j] = 1;

                //        }
                //        n++;
                //        if (counter.inter_Data_Full.Count - 28 < n)
                //        {
                //            break;
                //        }
                //    }
                //}

                bitMap = MakeBitmap.Make_Bitmap(pixels, 6, 6, col, 10, false);
            }// Конец прорисовки фокуса внимания

            InputData.Clear();

            if (counter.inter_Data_.Count >= 100)//14*14
            {
                SumX = 0;
                SumY = 0;
                num = 0;
                InputData.AddRange(counter.inter_Data_);
                int n = 0;
                for (int j = 0; j < 14; ++j)
                {
                    for (int i = 0; i < 14; ++i)
                    {
                        if (InputData[n] > 0)
                        {
                            //Coordinate[i, j] = 1;
                            SumX += i;
                            SumY += j;
                            num += 1;
                        }
                        n++;
                    }
                }
                X = (int)Math.Ceiling(SumX / num);
                Y = (int)Math.Ceiling(SumY / num);
            }

            X += X;//координаты зрачка
            Y += Y;

            if (X < 4 | X > 21)
            {
                X = 14;
                XYBuf[0] = 14;
            }

            if (Y < 4 | Y > 21)
            {
                Y = 14;
                XYBuf[1] = 14;
            }

            if (bufX != 0 & bufX != X)
            {
                if (bufX > X)
                {
                    X = bufX - 1;
                }
                else
                {
                    X = bufX + 1;
                }
            }
            bufX = X;

            if (bufY != 0 & bufY != Y)
            {

                if (bufY > Y)
                {
                    Y = bufY - 1;
                }
                else
                {
                    Y = bufY + 1;
                }
            }
            bufY = Y;

            //X = 21;
            //Y = X;

            {//Пустые скобки
                InputData.Clear();
                byte[,] Coordinate = new byte[14, 14];
                byte[,] Coordinate_ = new byte[28, 28];

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        //if (i % 2 == 0 & j % 2 == 0)
                        {
                            if (i + X > 0 & i + X < 28 & j + Y > 0 & j + Y < 28)
                            {
                                Coordinate[i + Y / 2, j + X / 2] = 1;
                                Coordinate_[i + Y - 2, j + X - 2] = 1;
                            }
                        }
                    }
                }

                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        //if (rnd.Next(0, 9) > 5)
                        //{
                        //    Coordinate[i / 2, j / 2] = 0;
                        //    Coordinate_[i, j] = 0;
                        //}
                        if (rnd.Next(0, 100) < 10)
                        {
                            Coordinate[i / 2, j / 2] = 1;
                            Coordinate_[i, j] = 1;
                        }
                    }
                }

                bitMap_Draw = MakeBitmap.Make_Bitmap(Coordinate_, X, Y, col, 10, true);

                for (int i = 0; i < 14; i++)
                {
                    for (int j = 0; j < 14; j++)
                    {
                        if (Coordinate[i, j] == 1)
                        {
                            InputData.Add(1);
                        }
                        else
                        {
                            InputData.Add(0);
                        }
                    }
                }
                counter.inter_Data_.Clear();
                //counter.inter_Data_.AddRange(InputData);
            }
        }
    }
    class MakeBitmap
    {
        public static Bitmap MakeBitmapLine(List<byte> InputData1, List<byte> InputData2, List<byte> InputData3, int mag = 20)
        {
            int width = 28 * mag;
            int height = 3 * mag;
            Bitmap result = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(result);
            SolidBrush sb = new SolidBrush(Color.FromArgb(0, 0, 0));
            for (int i = 0; i < 28; ++i)
            {
                if (InputData1[i] > 0)
                {
                    gr.FillRectangle(sb, i * mag, 0 * mag, mag - 1, mag - 1);
                }
                if (InputData2[i] > 0)
                {
                    gr.FillRectangle(sb, i * mag, 1 * mag, mag - 1, mag - 1);
                }
                if (InputData3[i] > 0)
                {
                    gr.FillRectangle(sb, i * mag, 2 * mag, mag - 1, mag - 1);
                }
            }
            return result;
        }
        public static Bitmap Make_Bitmap(byte[,] pixels, int X, int Y, bool Col, int mag = 10, bool DrawR = true)
        {
            int width = 28 * mag;
            int height = 28 * mag;
            Bitmap result = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(result);
            gr.Clear(Color.FromArgb(255, 255, 255));
            SolidBrush sb = new SolidBrush(Color.FromArgb(0, 0, 0));
            for (int i = 0; i < 28; ++i)
            {
                for (int j = 0; j < 28; ++j)
                {
                    if (pixels[i, j] > 0)
                    {
                        gr.FillRectangle(sb, j * mag, i * mag, mag, mag);
                    }
                }
            }
            if (DrawR)
            {
                Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                if (Col)
                {
                    blackPen = new Pen(Color.FromArgb(255, 0, 200, 50), 5);
                }
                gr.DrawRectangle(blackPen, (X - 3) * mag, (Y - 3) * mag, 6 * mag, 6 * mag);
            }
            return result;
        }
    }
}



