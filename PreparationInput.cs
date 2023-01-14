using System;
using System.Collections.Generic;
using System.Drawing;

namespace MNIST
{
    class PreparationInput //TODO: Сделать синглтоном. Done. 
    {
        private static PreparationInput instance = null;
        public static PreparationInput Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PreparationInput();
                }
                return instance;
            }
        }

        public bool IsColoured { get; private set; }
        private int prevX2 = 0;
        private int prevY2 = 0;
        private int waiting = 0;

        private int bufX = 14;
        private int bufY = 14;
        private readonly int[] XYBuf = { 14, 14 };

        public float BlackCount;
        public float GreenCount;
        public Bitmap bitMap;
        //public Bitmap bitMap_Draw;
        public Bitmap way_Draw;

        public int X { get; private set; }
        public int Y { get; private set; }
        private int wayMax = 256;

        public List<byte> InputData = new List<byte>();
        private const int focusFieldSize = 28; //Размер поля. 
        private readonly int[,] way = new int[focusFieldSize, focusFieldSize];
        private readonly byte[,] wayByte = new byte[focusFieldSize, focusFieldSize];

        private readonly int lowerLimit = 0;
        private readonly int upperLimit = 26;
        private readonly int proportions = 7;

        private readonly Random rnd = new Random();
        public float Semblance { get; private set; }
        private int meter = 100;
        public bool DrawFocusField { get { return meter == 100; } }
        private PreparationInput() { }

        /// <summary>
        /// Этот метод инициализирует поля класса. 
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="vsemblance"></param>
        /// <param name="Xb"></param>
        /// <param name="Yb"></param>
        /// <param name="reproduction"></param>
        /// <param name="isVisualSelected"></param>
        public void PrepareInput(Counter counter, float vsemblance, int Xb, int Yb, bool reproduction, bool isVisualSelected)
        {
            Semblance = vsemblance;
            IsColoured = true; // Считаем что всегда управлять фокусом будет нейронная сеть. Цвет рамки зелёный

            GreenCount = 0;
            BlackCount = 0;


            InputData.Clear();


            var XY = CalculateXY(counter, X, Y);
            X = XY.Item1;
            Y = XY.Item2;

            prevX2 = X;
            prevY2 = Y;

            if ((counter.summ2 < 5 || counter.summ2 > 65) || X > upperLimit || Y > upperLimit || X < lowerLimit || Y < lowerLimit || (X == Xb && Y == Yb))//Условия для запуска генератора движения counter.summ < 1 && counter.inter_Data_.Count < 1 || 
            {

                InputData.Clear();
                IsColoured = false;//Если фокусом управляет генератор движения. Цвет рамки чёрный
                BlackCount++;
                GreenCount--;

                ShakeXYBuf(XYBuf);

                X = XYBuf[0];
                Y = XYBuf[1];


                byte[,] Coordinates = new byte[focusFieldSize, focusFieldSize];

                InitializeCoordinates(Coordinates, X, Y); //На сгенерированных координатах фокуса    

                counter.inter_Data_.Clear();
                for (int i = 0; i < focusFieldSize; i++)
                {
                    for (int j = 0; j < focusFieldSize; j++)
                    {
                        counter.inter_Data_.Add(Coordinates[i, j]);// Имитирует данные о пятне фокуса (подменяет данные из нейронной сети, сгенерированными данными)
                    }
                }
            }
            else
            {
                //
            }

            if ((counter.inter_Data_Full.Count > 0 && !reproduction) || (counter.inter_Data_Full.Count > 0) && isVisualSelected)//Рисовать фокус внимания 
            {
                DrawFocus(counter, X, Y); // Если нет данных от нейронной сети, создаёт рисунок с данными от генератора.
            }// Конец прорисовки фокуса внимания

            InputData.Clear();

            XY = CalculateXY(counter, X, Y);
            X = XY.Item1;
            Y = XY.Item2;

            UpdateXY();


            {// Прорисовка того, как зрачёк движится по полю (в каких точках он бывает чаще).
                way[Y, X] += 1;
                if (wayMax < way[Y, X])
                {
                    wayMax = way[Y, X] + 1;
                }


                meter--;
                if (isVisualSelected || meter == 0)
                {
                    for (int i = 0; i < focusFieldSize; i++)
                    {
                        for (int j = 0; j < focusFieldSize; j++)
                        {
                            wayByte[i, j] = (byte)(256 - (255f * way[i, j] / wayMax)); //чем темнее тем сильнее (при прорисовке раскрашивается)
                            if (way[i, j] > 0)
                            {
                                way[i, j] = 256 - wayByte[i, j];
                            }
                        }
                    }
                    if (meter == 0)
                    {
                        way_Draw = BitmapMaker.MakeGreyBitmap(wayByte, X + 3, Y + 3, false);// wayMin + 10,
                    }
                    else
                    {
                        way_Draw = BitmapMaker.MakeGreyBitmap(wayByte, X + 3, Y + 3, true);//wayMin + 10,
                    }
                    wayMax = 256;
                    meter = 100;
                }
            }

            {
                InputData.Clear();
                byte[,] Coordinates = new byte[focusFieldSize, focusFieldSize];

                if (IsColoured)
                {
                    InitializeCoordinates(Coordinates, prevX2, prevY2); //Повторяется положение вспомненного фокуса для укрепления памяти//rnd,

                }
                else
                {
                    InitializeCoordinates(Coordinates, X, Y); //На окончательных координатах фокуса, после всех коррекций//rnd,
                }
                //if (isVisualSelected)
                //{
                //    bitMap_Draw = BitmapMaker.MakeBitmap(Coordinates, X + 3, Y + 3, IsColoured, 10, false); //Выводит рисунок фокуса на поле, в том виде, в котором эта информация будет передана в нейронную сеть 
                //}

                for (int i = 0; i < focusFieldSize; i++)
                {
                    for (int j = 0; j < focusFieldSize; j++)
                    {
                        InputData.Add(Coordinates[i, j]);//Запись данных о фокусе для дальнейшей передачи в нейронную сеть
                    }
                }
                counter.inter_Data_.Clear();
            }
        }

        /// <summary>
        /// Этот метод располагает координаты X и Y "по соседству" с их предыдущими значениями, если текущие и предыдущие значения не совпадают. 
        /// </summary>
        private void UpdateXY()
        {
            //if (Math.Abs(bufX - X) > 1)
            if (bufX != X)
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

            //  if (Math.Abs(bufY - Y) > 1)
            if (bufY != Y)
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
        }

        /// <summary>
        /// Эта функция выводит рисунок пятна фокуса на поле. 
        /// </summary>
        private void DrawFocus(Counter counter, int X, int Y)
        {
            byte[,] pixels = new byte[focusFieldSize, focusFieldSize];
            int n = 0;
            for (int i = 0; i < focusFieldSize; i++)
            {
                for (int j = 0; j < focusFieldSize; j++)
                {
                    pixels[i, j] = 0;
                    if (counter.inter_Data_[n] > 0)
                    {
                        pixels[i, j] = 1;
                    }
                    n++;
                }
            }
            bitMap = BitmapMaker.MakeBitmap(pixels, X + 3, Y + 3, IsColoured, 10, true);
        }

        /// <summary>
        /// Эта функция задаёт случайное значение координат фокуса с некоторой вероятностью. 
        /// </summary>
        /// <param name="rnd"></param>
        private void ShakeXYBuf(int[] XYBuf)//Random rnd,
        {
            Random rnd = new Random();
            if ((bufX == XYBuf[0] && bufY == XYBuf[1]) || waiting == 20)
            {
                waiting = 0;
                XYBuf[0] = rnd.Next(lowerLimit, upperLimit - 5);//upperLimit -
                XYBuf[1] = rnd.Next(lowerLimit, upperLimit - 2);//upperLimit -
            }
            waiting++;
        }

        /// <summary>
        /// Эта функция создаёт пятно фокуса на поле. 
        /// </summary>
        private void InitializeCoordinates(byte[,] Coordinates, int X, int Y)//Random rnd,
        {
            //Random rnd = new Random();
            //for (int i = 0; i < focusFieldSize; i++)
            //{
            //    for (int j = 0; j < focusFieldSize; j++)
            //    {
            //        if (rnd.Next(100) == 1)
            //        {
            //            Coordinates[i, j] = 1;
            //        }
            //    }
            //}
            for (int i = 0; i < proportions; i++)
            {
                for (int j = 0; j < proportions; j++)
                {
                    bool cond = i + Y + proportions / 2 > 0 && i + Y < focusFieldSize && j + X + proportions / 2 > 0 && j + X < focusFieldSize;
                    if (cond) Coordinates[i + Y, j + X] = 1;

                }
            }
        }

        /// <summary>
        /// Эта функция получает данные о пятне фокуса из нейронной сети. 
        /// </summary>
        private Tuple<int, int> CalculateXY(Counter counter, int X, int Y)
        {
            int num = 0;
            double SumX = 0;
            double SumY = 0;
            if (counter.inter_Data_.Count >= focusFieldSize * focusFieldSize)
            {
                InputData.AddRange(counter.inter_Data_);
                int n = 0;
                for (int j = 0; j < focusFieldSize; ++j)
                {
                    for (int i = 0; i < focusFieldSize; ++i)
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
                    X = (int)Math.Ceiling(SumX / num);//- 2
                    Y = (int)Math.Ceiling(SumY / num);//- 2
                }
            }
            return Tuple.Create(X, Y);
        }
    }

    /// <summary>
    ///  Эта функция рисует как зрачёк заполняет поле. 
    /// </summary>
    class BitmapMaker
    {
        public static Bitmap MakeGreyBitmap(byte[,] pixels, int X, int Y, bool isRectDrawn = true)//int wayMin,
        {
            int mag = 10;
            int width = 28 * mag;
            int height = 28 * mag;
            byte Color_grey;
            Bitmap result = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(result);
            SolidBrush sb = new SolidBrush(Color.FromArgb(0, 0, 0));
            for (int i = 0; i < 28; ++i)
            {
                for (int j = 0; j < 28; ++j)
                {
                    if (pixels[i, j] > 0 && pixels[i, j] < 240)
                    {
                        Color_grey = pixels[i, j];
                        sb.Color = Color.FromArgb(255 - Color_grey, 100, Color_grey);//красное чаще, синие реже
                        gr.FillRectangle(sb, j * mag, i * mag, mag, mag);
                    }
                }
            }
            if (isRectDrawn)
            {
                Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                gr.DrawRectangle(blackPen, (X - 3) * mag, (Y - 3) * mag, 6 * mag, 6 * mag);
            }
            return result;
        }

        /// <summary>
        /// Эта функция рисует поле, со зрачком.
        /// </summary>
        public static Bitmap MakeBitmap(byte[,] pixels, int X, int Y, bool isColoured, int mag = 10, bool isRectDrawn = true)
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
            if (isRectDrawn)
            {
                Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                if (isColoured)
                {
                    blackPen = new Pen(Color.FromArgb(255, 0, 200, 50), 5);
                }
                gr.DrawRectangle(blackPen, (X - 3) * mag, (Y - 3) * mag, 6 * mag, 6 * mag);
            }
            return result;
        }
    }
}


