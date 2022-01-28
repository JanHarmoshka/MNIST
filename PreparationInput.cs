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

        public bool IsColoured { get; private set; } //TODO: Переименовать. Done. // если управляет зрачком нейронная сеть - true
        //int prevX1; //TODO: Закомментировать. Done. 
        //int prevY1;
        private int prevX2 = 0;
        private int prevY2 = 0;

        private int bufX = 14; //TODO: нет внешних ссылок. Заменить уровень доступа? Done.
        private int bufY = 14;
        private readonly int[] XYBuf = { 14, 14 };

        public float n_black;
        public float n_green;
        public Bitmap bitMap;
        public Bitmap bitMap_Draw;
        public Bitmap way_Draw;

        public int X { get; private set; }
        public int Y { get; private set; }
        private int way_max = 256;

        public List<byte> InputData = new List<byte>();
        private const int focusFieldSize = 28; //Размер поля. 
        private readonly int[,] way = new int[focusFieldSize, focusFieldSize]; //TODO: Здесь и всюду дальше заменить 28 на именованную константу. Done, но см. замечание выше. 
        private readonly byte[,] way_byte = new byte[focusFieldSize, focusFieldSize];

        private readonly int lower_limit = 2;
        private readonly int Upper_limit = 22;
        private readonly int proportions = 6;

        private Random rnd = new Random();
        public float semblance { get; private set; }
        private int meter { get; set; } = 100; //TODO: одна внешняя ссылка, в которой поле используется в условии. Изменить поле на приватное и сделать публичное булево свойство. Done. => Это счётчик вывода картинки ка форму с графиками.
        public bool DrawFocusField { get { return meter == 100; } } //TODO: Уточнить название. 
        private PreparationInput() { }
        //TODO: Переименовать в соответствии с выполняемыми действиями. Done.
        /// <summary>
        /// Этот метод инициализирует поля класса. 
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="vsemblance"></param>
        /// <param name="Xb"></param>
        /// <param name="Yb"></param>
        /// <param name="reproduction"></param>
        /// <param name="TabPagesBool"></param>
        public void PrepareInput(Counter counter, float vsemblance, int Xb, int Yb, bool reproduction, bool TabPagesBool)
        {
            semblance = vsemblance;
            IsColoured = true; // Считаем что всегда управлять фокусом будет нейронная сеть. Цвет рамки зелёный

            n_green = 0;
            n_black = 0;
           

            InputData.Clear();

            //TODO: Здесь и ниже декомпозировать. 
            var XY = CalculateXY(counter, X, Y);
            X = XY.Item1;
            Y = XY.Item2;

            semblance = CalculateSemblance(Xb, Yb, semblance);

            prevX2 = X;
            prevY2 = Y;

            //TODO: Выделить в булеву переменную? На Ваше усмотрение
            if ((counter.summ < 1 & counter.summ2 < 1) |
                counter.inter_Data_.Count < 1 |
                X > Upper_limit | Y > Upper_limit | X < lower_limit | Y < lower_limit | (X == Xb & Y == Yb))//Условия для запуска генератора движения
            {

                InputData.Clear();
                IsColoured = false;//Если фокусом управляет гератор движения. Цвет рамки чёрный
                n_black++;
                n_green--;

                ShakeXYBuf(rnd, XYBuf);

                X = XYBuf[0];
                Y = XYBuf[1];

                //TODO: Отсюда и ниже выделить в метод UpdateCoordinate или как-то так. Done, но ещё нужно будет подумать над названием. 
                byte[,] Coordinates = new byte[focusFieldSize, focusFieldSize]; //TODO: Переименовать переменную на CoordinateFlags. Возможно, изменить тип на булевый массив. Done тоже. Вернул, как было, потому что ниже по коду требуется именно byte. 

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

            if ((counter.inter_Data_Full.Count > 0 & !reproduction) | (counter.inter_Data_Full.Count > 0) & TabPagesBool)//Рисовать фокус внимания 
            {
                DrawFocus(counter, X, Y); // Если нет данных от нейронной сети, создаёт рисунок с данными от генератора.
            }// Конец прорисовки фокуса внимания

            InputData.Clear();

            XY = CalculateXY(counter, X, Y);
            X = XY.Item1;
            Y = XY.Item2;

            BackXYBufInLimits(rnd, XYBuf);

            UpdateXY();


            {// Прорисовка того, как зрачёк движится по полю (в каких точках он бывает чаще). Чем меньше пятно в центре и чем сельнее контраст между центром и перефериеей, тем лучше обучена нейронная сеть. Идеально - красный квадрат в центре белого поля.
                way[X + 2, Y + 2] += 1;
                if (way_max < way[X + 2, Y + 2])
                {
                    way_max = way[X + 2, Y + 2];
                }


                meter--;
                if (TabPagesBool | meter == 0)
                {
                    for (int i = 0; i < focusFieldSize; i++)
                    {
                        for (int j = 0; j < focusFieldSize; j++)
                        {
                            way_byte[i, j] = (byte)(256 - (255f * way[i, j] / way_max)); //чем темнее тем сильнее (при прорисовке раскрашивается)
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
            }
            if (IsColoured)
            {
                XYBuf[0] = X;
                XYBuf[1] = Y;
            }

            {
                InputData.Clear();
                byte[,] Coordinates = new byte[focusFieldSize, focusFieldSize];
                InitializeCoordinates(Coordinates, X, Y); //На окончательных координатах фокуса, после всех коррекций

                bitMap_Draw = MakeBitmap.Make_Bitmap(Coordinates, X + 3, Y + 3, IsColoured, 10, false); //Выводит рисунок фокуса на поле, в том виде, в котором эта информация будет передана в нейронную сеть 

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
        private void UpdateXY() //Не уверен в выделении этого метода, но пусть пока так. 
        {
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
                bufX = X;
            }

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
                bufY = Y;
            }
        }

        /// <summary>
        /// Эта функция возвращает зрачёк если он оказывается за границами поля. 
        /// </summary>
        private void BackXYBufInLimits(Random rnd, int[] XYBuf)
        {
            if (X < lower_limit | X > Upper_limit)
            {
                X = rnd.Next(lower_limit, Upper_limit);
                XYBuf[0] = (byte)X;
            }
            if (Y < lower_limit | Y > Upper_limit)
            {
                Y = rnd.Next(lower_limit, Upper_limit);
                XYBuf[1] = (byte)Y;
            }
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
            bitMap = MakeBitmap.Make_Bitmap(pixels, X + 3, Y + 3, IsColoured, 10, true);
        }

        /// <summary>
        /// Эта функция задаёт случайное значение координат фокуса с некоторой вероятностью. 
        /// </summary>
        /// <param name="rnd"></param>
        private void ShakeXYBuf(Random rnd, int[] XYBuf)
        {
            if (bufX == XYBuf[0] & bufY == XYBuf[1])
            {
                XYBuf[0] = rnd.Next(lower_limit, Upper_limit);
                XYBuf[1] = rnd.Next(lower_limit, Upper_limit);
            }
        }

        /// <summary>
        /// Эта функция препятствует "зависанию" зрачка на одном и томже месте, влияя на нейронную сеть. 
        /// </summary>
        private float CalculateSemblance(int Xb, int Yb, float semblance)
        {
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
            return semblance;
        }

        /// <summary>
        /// Эта функция создаёт пятно фокуса на поле. 
        /// </summary>
        private void InitializeCoordinates(byte[,] Coordinates, int X, int Y)
        {
            for (int i = 0; i < proportions; i++)
            {
                for (int j = 0; j < proportions; j++)
                {
                    bool cond = i + Y + proportions / 2 > 0 & i + Y < focusFieldSize &
                        j + X + proportions / 2 > 0 & j + X < focusFieldSize;
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
                    X = (int)Math.Ceiling(SumX / num) - 3;
                    Y = (int)Math.Ceiling(SumY / num) - 3;
                }
            }
            return Tuple.Create(X, Y);
        }
    }

    /// <summary>
    ///  Эта функция рисует как зрачёк заполняет поле. 
    /// </summary>
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
                        sb.Color = Color.FromArgb(256 - Color_grey, 100, Color_grey);//красное чаще, синие реже
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

        /// <summary>
        /// Эта функция рисует поле, со зрачком.
        /// </summary>
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


