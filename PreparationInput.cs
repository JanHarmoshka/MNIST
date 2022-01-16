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

        public int bufX = 14;
        public int bufY = 14;
        readonly byte[] XYBuf = { 14, 14 };

        public float n_blekc;
        public float n_green;
        public Bitmap bitMap;
        public Bitmap bitMap_Draw;
        public Bitmap way_Draw;

        public int X;
        public int Y;
        int way_max = 256;

        public List<byte> InputData = new List<byte>();
        readonly int[,] way = new int[28, 28];
        readonly byte[,] way_byte = new byte[28, 28];

        readonly int lower_limit = 2;
        readonly int Upper_limit = 22;
        readonly int proportions = 6;
        public float semblance;

        public int meter = 100;


        public void PreparationInput_1(Counter counter, float vsemblance, int Xb, int Yb, bool reproduction, bool TabPagesBool)
        {
            semblance = vsemblance;
            col = true;
            double SumX = 0;
            double SumY = 0;
            int num = 0;
            n_green = 0;
            n_blekc = 0;
            Random rnd = new Random();

            InputData.Clear();

            if (counter.inter_Data_.Count >= 28 * 28)
            {
                InputData.AddRange(counter.inter_Data_);
                int n = 0;
                for (int j = 0; j < 28; ++j)
                {
                    for (int i = 0; i < 28; ++i)
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

            X -= 3;//координаты зрачка
            Y -= 3;

            if (prevX2 > 0 & prevY2 > 0)
            {
                if ((prevX2 == X & prevY2 == Y) | (prevX2 == Xb & prevY2 == Yb))
                {

                    if (semblance > 1)
                    {
                        semblance -= 7f;
                    }
                }
                else
                {
                    semblance = 22;
                }
            }
            else
            {
                semblance = 22;
            }

            prevX1 = prevX2;
            prevY1 = prevY2;
            //if (counter.summ < 1 & counter.summ2 > 0)
            //{
            //    X = prevX2;
            //    counter.summ = 1;
            //}
            //if (counter.summ2 < 1 & counter.summ > 0)
            //{
            //    Y = prevY2;
            //    counter.summ2 = 1;
            //}
            prevX2 = X;
            prevY2 = Y;

            if ((counter.summ < 1 & counter.summ2 < 1) | counter.inter_Data_.Count < 1 | X > Upper_limit | Y > Upper_limit | X < lower_limit | Y < lower_limit | (X == Xb & Y == Yb))//counter.summ > 20 | counter.summ2 > 20 |
            {

                InputData.Clear();
                col = false;//bleck
                n_blekc++;
                n_green--;


                //if (X == Xb & Y == Yb & counter.summ > 0 & counter.summ2 > 0)
                //{
                //    col = true;//green
                //    n_blekc--;
                //    n_green++;
                //}

                do
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (rnd.Next(1, 100) > 50)
                            if (rnd.Next(1, 100) > 50)
                            {
                                if (XYBuf[i] >= 0 & XYBuf[i] < Upper_limit)
                                {
                                    XYBuf[i]++;
                                }
                            }
                            else
                            {
                                if (XYBuf[i] > 0 & XYBuf[i] <= Upper_limit)
                                {
                                    XYBuf[i]--;
                                }
                            }
                    }
                    //XYBuf[0] = (byte)rnd.Next(lower_limit + 1, Upper_limit - 1);
                    //XYBuf[1] = (byte)rnd.Next(lower_limit + 1, Upper_limit - 1);
                }
                while ((X == XYBuf[0] | Y == XYBuf[1]));
                X = XYBuf[0];
                Y = XYBuf[1];

                byte[,] Coordinate = new byte[28, 28];

                for (int i = 0; i < proportions; i++)
                {
                    for (int j = 0; j < proportions; j++)
                    {
                        // if (i % 2 == 0 & j % 2 == 0)
                        {
                            if (i + Y + proportions / 2 > 0 & i + Y < 28 & j + X + proportions / 2 > 0 & j + X < 28)
                            {
                                Coordinate[i + Y, j + X] = 1;//                                
                            }
                        }
                    }
                }
                //bitMap_Draw = MakeBitmap.Make_Bitmap(Coordinate_, 7, 7, col, 20);

                counter.inter_Data_.Clear();
                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        if (Coordinate[i, j] == 1)
                        {
                            counter.inter_Data_.Add(1);
                        }
                        else
                        {
                            counter.inter_Data_.Add(0);
                        }
                    }
                }
            }

            if ((counter.inter_Data_Full.Count > 0 & !reproduction) | (counter.inter_Data_Full.Count > 0) & TabPagesBool)//Рисовать фокус внимания 
            {

                byte[,] pixels = new byte[28, 28];
                int n = 0;
                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        pixels[i, j] = 0;
                        if (counter.inter_Data_[n] > 0)//counter.inter_Data_Full[n + counter.inter_Data_Full.Count - 28 * 28] > 0
                        {
                            pixels[i, j] = 1;
                        }
                        n++;
                    }
                }
                bitMap = MakeBitmap.Make_Bitmap(pixels, X + 3, Y + 3, col, 10, true);
            }// Конец прорисовки фокуса внимания

            InputData.Clear();

            if (counter.inter_Data_.Count >= 28 * 28)//14*14
            {
                SumX = 0;
                SumY = 0;
                num = 0;
                InputData.AddRange(counter.inter_Data_);
                int n = 0;
                for (int j = 0; j < 28; ++j)
                {
                    for (int i = 0; i < 28; ++i)
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
                X = (int)Math.Ceiling(SumX / num) - 3;
                Y = (int)Math.Ceiling(SumY / num) - 3;
            }

            //X -= 3;
            //X += X;//координаты зрачка
            //Y += Y;             

            if (X < lower_limit | X > Upper_limit)//
            {
                X = rnd.Next(lower_limit, Upper_limit);//rnd.Next(lower_limit, Upper_limit)
                XYBuf[0] = (byte)X;
            }

            if (Y < lower_limit | Y > Upper_limit)//
            {
                Y = rnd.Next(lower_limit, Upper_limit);//rnd.Next(lower_limit, Upper_limit)
                XYBuf[1] = (byte)Y;
            }


            if (bufX != X)//& col
            {
                if (bufX > X)
                {
                    X = bufX - 1;
                }
                else
                {
                    X = bufX + 1;
                }
                bufX = X;
            }


            if (bufY != Y)//& col
            {

                if (bufY > Y)
                {
                    Y = bufY - 1;
                }
                else
                {
                    Y = bufY + 1;
                }
                bufY = Y;
            }


            //X = 3;
            //Y = X;

            // if (X != 4 & Y != 4)
            {
                way[X + 2, Y + 2] += 1;
                if (way_max < way[X + 2, Y + 2])
                {
                    way_max = way[X + 2, Y + 2];
                }
            }

            meter--;
            if (TabPagesBool | meter == 0)
            {
                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        way_byte[i, j] = (byte)(256 - (255f * way[i, j] / way_max)); //чем темнее тем сильнее
                        if (way[i, j] > 0)
                        {
                            way[i, j] = 256 - way_byte[i, j];
                        }

                    }
                }
                if (meter == 0)
                {
                    way_Draw = MakeBitmap.Make_Bitmap_grey(way_byte, X + 3, Y + 3, false);
                }
                else
                {
                    way_Draw = MakeBitmap.Make_Bitmap_grey(way_byte, X + 3, Y + 3, true);
                }
                way_max = 256;
                meter = 100;
            }

            {//Пустые скобки
                InputData.Clear();
                byte[,] Coordinate = new byte[28, 28];

                for (int i = 0; i < proportions; i++)
                {
                    for (int j = 0; j < proportions; j++)
                    {
                        if (i + Y + proportions / 2 > 0 & i + Y < 28 & j + X + proportions / 2 > 0 & j + X < 28)
                        {
                            Coordinate[i + Y, j + X] = 1;
                        }

                    }
                }

                //for (int i = 0; i < 28; i++)
                //{
                //    for (int j = 0; j < 28; j++)
                //    {
                //        if (rnd.Next(0, 100) < 1)
                //        {
                //            Coordinate[i, j] = 1;
                //        }
                //    }
                //}

                bitMap_Draw = MakeBitmap.Make_Bitmap(Coordinate, X + 3, Y + 3, col, 10, false);

                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
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
        public static Bitmap Make_Bitmap_grey(byte[,] pixels, int X, int Y, bool DrawR = true)
        {
            int mag = 10;
            int width = 28 * mag;
            int height = 28 * mag;
            byte Color_grey;
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
                        Color_grey = pixels[i, j];
                        sb.Color = Color.FromArgb(Color_grey, Color_grey, Color_grey);
                        gr.FillRectangle(sb, j * mag, i * mag, mag, mag);
                    }
                }
            }
            if (DrawR)
            {
                Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                gr.DrawRectangle(blackPen, (X - 3) * mag, (Y - 3) * mag, 6 * mag, 6 * mag);
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



