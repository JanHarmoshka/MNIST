using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MNIST
{
    public partial class Form1 : Form
    {
        public List<int> series = null;
        public List<int> series2 = null;
        string images, labels;
        public bool reproduction;
        public int allError = 0;
        readonly List<int> all = new List<int>();
        public int nn = 0;
        public float BlackCount = 0;
        public float GreenCount = 0;
        double greenToBlackRatio = 0;

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(
            backgroundWorker1_ProgressChanged);
            chart1.Series.Add(new Series("Y")
            {
                ChartType = SeriesChartType.Spline
            });
            chart2.Series.Add(new Series("N")
            {
                ChartType = SeriesChartType.Spline
            });
            chart1.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;
            chart2.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;
            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Consolas", 6);
            chart2.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Consolas", 10);
            chart1.Series["Y"].BorderWidth = 2;
            chart2.Series["N"].BorderWidth = 2;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Minimum = 0;

            string path = Directory.GetCurrentDirectory() + "\\settings.ini";
            Harmoshka = Harmoshka.Instance;
        }

        public static int ReverseBytes(int v)
        {
            byte[] intAsBytes = BitConverter.GetBytes(v);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }

        //bool sessionFlag = false;
        private void стартToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private Harmoshka Harmoshka;
        BackgroundWorker worker;
        byte minWoll = 0;
        int WollWin = 0;
        int gamesPlayed = 0;
        int bb = 0;
        private object RichTextBox1;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            worker = sender as BackgroundWorker;

            bool TabPages4BoolStop = false;
            bool TabPages4Bool = false;

            List<float> inter_result = new List<float>();
            int Index = 0;
            List<bool> scroll = new List<bool>();
            Counter counter = new Counter();

            List<float> InputData = new List<float>();
            List<byte> InputDataBuf = new List<byte>();
            byte[] IndexList = new byte[14];
            // bool Eror_Bool;
            int NumberGames = 0;

            int X = 7;
            int Y = 7;
            int X_ = 7;
            int Y_ = 7;
            int Xb = 0;
            int Yb = 0;

            float summ = 0;
            float summ_0 = 0;

            Stopwatch sw = new Stopwatch();
            bool col;
            float semblance = 50f;

            int baseСoordinate = 5;
            int baseСoordinateBuf = -1;

            sw.Start();

            Bitmap bitMap;
            Bitmap bitMap_;
            Bitmap b2 = new Bitmap(10, 10);
            PreparationInput preparation_input = PreparationInput.Instance;
            BreakOut game = new BreakOut();
            try
            {
                // Счётчик запусков основного алгоритмс
                for (int l = 0; l < 100; l++)
                {
                    Random rnd = new Random();

                    byte[,] pixelsBuf = new byte[28, 28];
                    byte[,] arrayb2 = new byte[12, 12];
                    byte[,] pixels = new byte[28, 28];

                    int focusSize = 6;// Размер зрачка

                    //Harmoshka.LessonTrigger = sessionFlag;

                    //основной алгоритм программы

                    for (int di = 0; di < Harmoshka.ErrorCount; di++)
                    {
                        button1.Invoke((MethodInvoker)delegate
                        {
                            if (button1.Enabled == false)
                            {
                                InitializeHarmoshka();
                            }
                            button1.Enabled = true;
                        });
                        nn++;
                        if (scroll.Count < Harmoshka.ErrorCount)
                        {
                            scroll.Add(false);
                        }

                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (di % 500 == 0) // Частота прорисовки графиков
                        {
                            worker.ReportProgress(0);
                        }

                        {//Подготовка изображения для загрузки в нейронную сеть

                            if (game.gameOver || game.gameWin)
                            {
                                NumberGames++;
                                gamesPlayed++;
                                game.gameOver = false;
                                game.gameWin = false;
                            }
                            pixels = game.MoveGame(baseСoordinate);


                            b2 = BitmapMaker.MakeBitmap(pixels, X_, Y_, false, 1, false); //Уменьшение исходного изображения для переферийного зрения
                            b2 = new Bitmap(b2, new Size(12, 12));
                            BackgroundWorkerHelper.PixelsFromImage(b2, arrayb2);
                        }// Конец подготовки изображения


                        Index = game.bollСoordinate;
                        if (game.woll == 0)
                        {
                            minWoll = game.minWoll;
                        }

                        //if (summ == 0)
                        //{
                        //    summ = counter.summ2;
                        //}
                        //else if (counter.summ2 > 0)
                        //{
                        //    summ = ((counter.summ2 + summ) / 2f);
                        //}

                        //if (counter.summ2 == 0)
                        //{
                        //    summ_0++;
                        //}


                        if (bb % 600f == 0)// Вывод статистики о времени работы и ошибке
                        {
                            Harmoshka.Message += String.Format(">>{0:000}% ", (bb / 600.001f)) + "Сыграно:" + NumberGames;//+ " " + String.Format("{0:000} ", summ) + " " + summ_0
                            Harmoshka.Message += String.Format(" {00:00.0}", sw.ElapsedMilliseconds / 1000.0f) + "c" + "\r\n";
                            sw.Restart();
                            NumberGames = 0;
                            summ_0 = 0;
                        }

                        bb++;

                        allError++;
                        all.Add(1);
                        //Eror_Bool = true;


                        bool TabPagesBool = false;
                        tabControl1.Invoke((MethodInvoker)delegate
                        {
                            TabPagesBool = tabControl1.SelectedIndex == 1;
                        });

                        tabControl1.Invoke((MethodInvoker)delegate
                        {
                            TabPages4Bool = tabControl1.SelectedIndex == 3;
                        });
                        if (!TabPages4Bool)
                        {
                            TabPages4BoolStop = false;
                        }

                        int ll = 0;
                        counter.Assessment.Clear();
                        counter.room.Clear();
                        do //обработка одного изображения в цикле узнавания
                        {
                            col = true;
                            GreenCount++;
                            preparation_input.PrepareInput(counter, semblance, Xb, Yb, reproduction, TabPagesBool);

                            col = preparation_input.IsColoured;
                            BlackCount += preparation_input.BlackCount;
                            GreenCount += preparation_input.GreenCount;
                            X = preparation_input.X;
                            Y = preparation_input.Y;
                            //semblance = preparation_input.Semblance;



                            if (TabPagesBool)
                            {
                                DrawImage(pictureBox3, preparation_input.bitMap);
                                bitMap = BitmapMaker.MakeBitmap(pixels, X + 3, Y + 3, col);
                                DrawImage(pictureBox1, bitMap);
                            }

                            BackgroundWorkerHelper.FillInputData(InputData, arrayb2, pixels, focusSize, X, Y);

                            if (TabPagesBool)
                            {
                                byte[,] pixels_ = BackgroundWorkerHelper.CreateFocusArray(X, Y, pixels, focusSize);
                                bitMap_ = BitmapMaker.MakeBitmap(pixels_, 9, 9, col, 15, true);//прорисовка фокуса зрения
                                DrawImage(pictureBox2, bitMap_);

                                b2 = new Bitmap(b2, new Size(280, 280)); //прорисовка переферийного зрения
                                DrawImage(pictureBox5, b2);
                                // DrawImage(pictureBox6, preparation_input.bitMap_Draw);
                                DrawImage(pictureBox4, preparation_input.way_Draw);

                                label16.Invoke((MethodInvoker)delegate
                                {
                                    label16.Text = "Повторение символа: " + ll.ToString();
                                });
                            }
                            if (preparation_input.DrawFocusField)
                            {
                                DrawImage(pictureBox9, preparation_input.way_Draw);
                            }

                            Xb = X;
                            Yb = Y;

                            //for (int i = 0; i < InputData.Count; i++)
                            //{
                            //    if (rnd.Next(0, 9) > 8)
                            //    {
                            //        InputData[i] = 0;
                            //    }
                            //    if (rnd.Next(0, 9) > 8)
                            //    {
                            //        InputData[i] = 1;
                            //    }
                            //}
                            for (int i = 0; i < preparation_input.InputData.Count; i++) // К входящиму в нейросеть сигналу добавляется информация о движении
                            {
                                InputData.Add(preparation_input.InputData[i] > 0 ? 1 : 0);
                            }
                            counter = Harmoshka.Assessment(784, InputData, semblance); // вход в нейронную сеть

                            if (TabPages4Bool && !TabPages4BoolStop)
                            {
                                richTextBox1.Invoke((MethodInvoker)delegate
                                {
                                    string Text = "";
                                    foreach (Matte item in Harmoshka.Mattes)
                                    {
                                        //richTextBox1.Text += item.room + " | " + item.appeal + " | " + item.Control_value + " | " + item.mattepPositive + " | " + item.Control_value + "\r\n";
                                        Text += String.Format("{00:00000} ", (item.room)) + String.Format("|{00:00.0000} ", (item.appeal)) + String.Format("|{00:000} ", (item.Control_value)) +
                                             String.Format("|{00:000} ", (item.mattepPositive)) + String.Format("|{00:000} ", (item.matteNegative)) + String.Format("|{00:0000} ", (item.summ)) + "\r\n";
                                    }
                                    Text += "____________________________" + "\r\n";
                                    foreach (ReverseMatte item in Harmoshka.ReverseMattes)
                                    {
                                        //richTextBox1.Text += item.room + " | " + item.appeal + " | " + item.Control_value + " | " + item.mattepPositive + " | " + item.Control_value + "\r\n";
                                        Text += String.Format("{00:00000} ", (item.Live)) + String.Format("|{00:00.0000} ", (item.appeal_)) + String.Format("|{00:000.000} ", (item.Control_value)) +
                                            String.Format("|{00:000} ", (item.mattepPositive)) + String.Format("|{00:000} ", (item.matteNegative)) + String.Format("|{00:0000} ", (item.summ)) +
                                            String.Format("|{00:0000} ", (item.room)) + String.Format("|{00:0000} ", (item.participation)) + String.Format("|{00:0.000} ", (item.summCorrect)) + "\r\n";
                                    }
                                    richTextBox1.Text += Text + "____________________________" + "\r\n";
                                });
                                TabPages4BoolStop = true;
                            }

                            //if (counter.str2 && Eror_Bool)
                            //{
                            //    Eror_Bool = false;
                            //}
                            int p = (int)(Math.Round(Y / 2.4f) - 1);
                            if (p < 0)
                            {
                                p = 0;
                            }
                            if (p > 10)
                            {
                                p = 10;
                            }
                            IndexList[p]++;
                            ll++;
                        }
                        while (ll < 3);


                        if (TabPagesBool)
                        {
                            DrawNumberHistogram(Index, IndexList);
                            DrawActivityHistogram(Index, counter);
                        }

                        int IndexLisBuf = 0;//поиск самой активной группы, для определения координаты платформы
                        for (int i = 0; i < 9; i++)
                        {
                            if (IndexList[i] > IndexLisBuf)
                            {
                                IndexLisBuf = IndexList[i];
                                baseСoordinate = i;//результат поиска
                            }

                        }

                        baseСoordinateBuf = baseСoordinate;

                        int indexVar = IndexList[Index];
                        int sunset = 0;
                        bool indexBool = false;
                        for (int i = 0; i < 10; i++)
                        {
                            if (IndexList[i] > indexVar)
                            {
                                indexBool = true;
                                sunset++;
                            }
                            IndexList[i] = 0;
                        }
                        if ((!indexBool || sunset < 2) & indexVar != 0)
                        {
                            allError--;
                            all[all.Count - 1] = 0;
                        }


                        if (Harmoshka.Message != null)
                        {
                            worker.ReportProgress(di, Harmoshka.Message);
                            Harmoshka.Message = null;
                        }

                        if (l > 0)
                        {
                            allError -= all[0];
                            all.RemoveAt(0);
                        }

                        if (!game.gameOver && game.gameWin)
                        {
                            WollWin++;
                        }
                    }

                    InputData.Clear();
                    InputData.TrimExcess();
                }
            }
            catch (Exception ex)
            {
                //Выводим ошибку
                MessageBox.Show(ex.ToString());
            }

        }

        private void DrawActivityHistogram(int Index, Counter counter)
        {
            pictureBox8.Invoke((MethodInvoker)delegate
            {
                Graphics g = pictureBox8.CreateGraphics();
                g.Clear(Color.White);
                Pen blackPen = new Pen(Color.Sienna, 1);
                for (int i = 0; i < counter.room.Count; i++)
                {
                    g.DrawLine(blackPen, i, 200, i, 200 - counter.room[i]);
                }
                g.DrawString("Активность нейронов второго слоя", new Font("Arial", 14), Brushes.Green, 2, 2);
            });
            pictureBox10.Invoke((MethodInvoker)delegate
            {
                Graphics g = pictureBox10.CreateGraphics();
                g.Clear(Color.White);
                Pen blackPen = new Pen(Color.Sienna, 1);
                for (int i = 0; i < counter.room2.Count; i++)
                {
                    g.DrawLine(blackPen, i, 200, i, 200 - counter.room2[i]);
                }
                g.DrawString("Активность нейронов первого слоя", new Font("Arial", 14), Brushes.Green, 2, 2);

            });
        }

        private void DrawNumberHistogram(int Index, byte[] IndexList)
        {
            pictureBox7.Invoke((MethodInvoker)delegate
            {
                Graphics g = pictureBox7.CreateGraphics();
                g.Clear(Color.White);
                SolidBrush sbIndex = new SolidBrush(Color.Red);
                for (int i = 0; i < 10; i++)
                {
                    sbIndex.Color = Color.Black;
                    if (i == Index)
                    {
                        sbIndex.Color = Color.Red;
                    }
                    g.FillRectangle(sbIndex, 21 * i, 210 - (7 * IndexList[i] + 5), 20, (7 * IndexList[i] + 5));
                }

            });
        }

        private void DrawImage(PictureBox pictureBox, Bitmap bitmap)
        {
            pictureBox.Invoke((MethodInvoker)delegate
            {
                pictureBox.Image = bitmap;
            });
        }

        private void InitializeHarmoshka()
        {
            Harmoshka.ErrorCount = int.Parse(textBox1.Text);

            Harmoshka.Satiety = float.Parse(textBox3.Text);

            Harmoshka.CorrectionThreshold = float.Parse(textBox8.Text);

            //Harmoshka.V2 = float.Parse(textBox7.Text);

            Harmoshka.V5 = float.Parse(textBox12.Text);

            Harmoshka.MemoryDuration = int.Parse(textBox9.Text);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Consol.Text += "\r\n Canceled\r\n";
            }
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                backgroundWorker1.CancelAsync();
                foreach (Control obj in this.Controls)
                {
                    if (obj is MenuStrip)
                    {
                        (obj as MenuStrip).Items[0].Enabled = true;
                        (obj as MenuStrip).Items[1].Enabled = true;
                    }
                    if (obj is TextBox) obj.Enabled = true;
                }
                Consol.Text += "\r\n Done...\r\n";
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                double y1 = (double)(WollWin);
                label18.Text = y1.ToString() + "/" + gamesPlayed;
                if (gamesPlayed > 0)
                {
                    y1 = (double)WollWin / (gamesPlayed);
                }
                minWoll = 0;

                if (BlackCount > 0)
                {
                    if (greenToBlackRatio == 0)
                    {
                        greenToBlackRatio = GreenCount / BlackCount;
                    }
                    else
                    {
                        greenToBlackRatio = (GreenCount / BlackCount + greenToBlackRatio) / 2f;
                    }

                }
                BlackCount = 0;
                GreenCount = 0;

                if (series == null)
                {
                    series = new List<int>();
                }
                series.Add((int)y1);
                if ((sender as BackgroundWorker).CancellationPending != true)
                {
                    //if (greenToBlackRatio > 0)
                    //{
                    chart2.Series["N"].Points.AddXY(series.Count - 2, greenToBlackRatio); //график участия нейронной сети в управлении фокусом
                    chart1.Series["Y"].Points.AddXY(series.Count - 2, y1);//график прироста побед
                    //}
                }
                return;
            }
            if ((sender as BackgroundWorker).CancellationPending != true)
            {
                Consol.Text += (String)e.UserState;
                Consol.Focus();
                Consol.Select(Consol.Text.Length, 0);
            }

        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (worker != null)
            {
                if (worker.WorkerSupportsCancellation == true)
                {
                    worker.CancelAsync();
                }
                while (true)
                {
                    if (worker.CancellationPending == true)
                    {
                        return;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (images == null)
            {
                MessageBox.Show("Задайте путь к папке изображений"); return;
            }
            else if (labels == null)
            {
                MessageBox.Show("Задайте путь к папке индексов"); return;
            }

            if (Harmoshka == null)
            {
                MessageBox.Show("Обьект Harmoshka не инициализирован"); return;
            }
            folderBrowserDialog1.SelectedPath = Directory.GetCurrentDirectory();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = DateTime.Now.ToString();
                path = path.Replace('.', '-');
                path = path.Replace(' ', '-');
                path = path.Replace(':', '-');
                path = folderBrowserDialog1.SelectedPath + "\\" + path;

                BinaryFormatter formatter = new BinaryFormatter();
                string copy = path + ".dat";
                using (FileStream fs = new FileStream(copy, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, Harmoshka);
                }

                int width = panel1.Size.Width;
                int height = panel1.Size.Height;

                Bitmap bm = new Bitmap(width, height);
                panel1.DrawToBitmap(bm, new Rectangle(0, 0, width, height));

                bm.Save(path + ".png", ImageFormat.Png);

                path = path + "_session.ini";
                File.Create(path).Close();
                List<string> session = new List<string>
                {
                    images,
                    labels,
                    copy,
                    textBox1.Text,
                    textBox3.Text,
                    textBox8.Text,
                    textBox7.Text,
                    textBox5.Text,
                    textBox12.Text
                };
                File.WriteAllLines(path, session.ToArray());
                MessageBox.Show("Сессия сохранена");
            }
        }
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*.ini)||*.ini";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] text = File.ReadAllLines(openFileDialog1.FileName);
                    images = text[0];
                    labels = text[1];

                    textBox1.Text = text[2];
                    textBox3.Text = text[3];
                    textBox8.Text = text[4];
                    textBox7.Text = text[5];
                    textBox5.Text = text[6];
                    textBox12.Text = text[7];


                    BinaryFormatter formatter = new BinaryFormatter();
                    using (FileStream fs = new FileStream(text[2], FileMode.OpenOrCreate))
                    {
                        Harmoshka = (Harmoshka)formatter.Deserialize(fs);
                        MessageBox.Show("Сессия загружина");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*.idx3-ubyte)|*.idx3-ubyte";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                images = openFileDialog1.FileName;
            }
        }
        private void IndicesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*.idx1-ubyte)|*.idx1-ubyte";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                labels = openFileDialog1.FileName;
            }
        }

        private void ExportBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Directory.GetCurrentDirectory();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {


                string path = DateTime.Now.ToString();
                path = path.Replace('.', '-');
                path = path.Replace(' ', '-');
                path = path.Replace(':', '-');
                path = folderBrowserDialog1.SelectedPath + "\\" + path;
                Harmoshka.SaveMatte(path);

                MessageBox.Show("Экспорт оуществлён");


            }
        }

        private void ImportBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*ReverseMatte.dat)|*ReverseMatte.dat";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Harmoshka.LoadMatte(openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length - 16));
                    MessageBox.Show("Импорт осуществлён");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + "\\settings.ini";
            reproduction = true;

            if (backgroundWorker1.IsBusy != true)
            {

                foreach (Control obj in this.Controls)
                {
                    if (obj is MenuStrip)
                    {
                        (obj as MenuStrip).Items[0].Enabled = false;
                        (obj as MenuStrip).Items[1].Enabled = false;
                    }
                    if (obj is TextBox) obj.Enabled = false;
                }
                if (series != null) series.Clear();
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                backgroundWorker1.RunWorkerAsync();
                button4.Text = "Стоп";
            }
            else
            {
                if (backgroundWorker1.WorkerSupportsCancellation == true)
                {

                    backgroundWorker1.CancelAsync();


                    foreach (Control obj in this.Controls)
                    {
                        if (obj is MenuStrip)
                        {
                            (obj as MenuStrip).Items[0].Enabled = true;
                            (obj as MenuStrip).Items[1].Enabled = true;
                        }
                        if (obj is TextBox) obj.Enabled = true;
                    }
                    button4.Text = "Запись";
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + "\\settings.ini";

            reproduction = false;

            if (backgroundWorker1.IsBusy != true)
            {

                foreach (Control obj in this.Controls)
                {
                    if (obj is MenuStrip)
                    {
                        (obj as MenuStrip).Items[0].Enabled = false;
                        (obj as MenuStrip).Items[1].Enabled = false;
                    }
                    if (obj is TextBox) obj.Enabled = false;
                }
                if (series != null) series.Clear();
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                backgroundWorker1.RunWorkerAsync(); //запуск потока
                button4.Enabled = false;
            }
            else if (backgroundWorker1.WorkerSupportsCancellation == true)
            {

                backgroundWorker1.CancelAsync();


                foreach (Control obj in this.Controls)
                {
                    if (obj is MenuStrip)
                    {
                        (obj as MenuStrip).Items[0].Enabled = true;
                        (obj as MenuStrip).Items[1].Enabled = true;
                    }
                    if (obj is TextBox) obj.Enabled = true;
                }
                button4.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

    }

}



