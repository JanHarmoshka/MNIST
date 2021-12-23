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
        public int allEror = 0;
        private List<int> all = new List<int>();
        public int nn = 0;
        public float n_blekc = 0;
        public float n_green = 0;
        double n_col = 0;
        string writePath = "";

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
            Harmoshka = new Harmoshka();
        }

        public static int ReverseBytes(int v)
        {
            byte[] intAsBytes = BitConverter.GetBytes(v);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }

        string[] setting = new string[2];
        bool session_flag = false;
        private void стартToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public Harmoshka Harmoshka;
        BackgroundWorker worker;
        bool get_files = false;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            worker = sender as BackgroundWorker;

            List<float> inter_result = new List<float>();
            int Index = 0;
            List<bool> scroll = new List<bool>();
            List<float> List_get_files_end = new List<float>();
            Counter counter = new Counter();
            List<float> InputData_ = new List<float>();

            List<float> InputData = new List<float>();
            List<byte> InputDataP = new List<byte>();
            List<byte> InputDataP2 = new List<byte>();
            List<byte> InputDataP3 = new List<byte>();
            List<byte> InputDataBuf = new List<byte>();
            byte[] IndexList = new byte[10];
            bool Eror_Bool;

            int X = 7;
            int Y = 7;
            int X_ = 7;
            int Y_ = 7;
            int Xb = 0;
            int Yb = 0;

            int bb = 0;
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            bool col;
            float semblance = 20;

            List<Point> pixels_way_List = new List<Point>();
            byte[,] pixels_way = new byte[28, 28];
            Point XY;
            Bitmap bitMap_way;

            sw.Start();
            // проход по файлам данных
            for (int l = 0; l < 100; l++)
            {

                FileStream ifsPixels = new FileStream(images, FileMode.Open);
                FileStream ifsLabels = new FileStream(labels, FileMode.Open);

                BinaryReader brImages = new BinaryReader(ifsPixels);
                BinaryReader brLabels = new BinaryReader(ifsLabels);

                int magic1 = brImages.ReadInt32();
                magic1 = ReverseBytes(magic1);

                int imageCount = brImages.ReadInt32();
                imageCount = ReverseBytes(imageCount);

                int numRows = brImages.ReadInt32();
                numRows = ReverseBytes(numRows);
                int numCols = brImages.ReadInt32();
                numCols = ReverseBytes(numCols);

                int magic2 = brLabels.ReadInt32();
                magic2 = ReverseBytes(magic2);

                int numLabels = brLabels.ReadInt32();
                numLabels = ReverseBytes(numLabels);

                Random rnd = new Random();
                Bitmap bitMap;
                Bitmap bitMap_;
                Bitmap bitMap4;
                Bitmap b2 = new Bitmap(10, 10);

                byte[,] pixelsBuf = new byte[28, 28];
                byte[,] arrayb2 = new byte[12, 12];
                byte[,] pixels = new byte[28, 28];

                int focus_scale = 6;// Размер зрачка

                PreparationInput preparation_input = new PreparationInput();

                if (session_flag)
                {
                    Harmoshka.SetTrigger(true);
                }
                else
                {
                    Harmoshka.SetTrigger(false);
                }

                //поиск файла индекса

                for (int di = 0; di < Harmoshka.GetFullError(); di++)
                {
                    button1.Invoke((MethodInvoker)delegate
                    {
                        if (button1.Enabled == false)
                        {
                            try
                            {
                                Harmoshka.SetFullError(int.Parse(textBox1.Text));

                                Harmoshka.SetSatiety(float.Parse(textBox3.Text));

                                Harmoshka.SetV1(float.Parse(textBox8.Text));

                                Harmoshka.SetV2(float.Parse(textBox7.Text));

                                Harmoshka.SetV5(float.Parse(textBox12.Text));

                                Harmoshka.SetV6(int.Parse(textBox9.Text));

                            }
                            catch (Exception ex)
                            {
                                e.Cancel = true;
                                MessageBox.Show(ex.Message);
                                return;
                            }
                        }
                        button1.Enabled = true;
                    });
                    nn++;
                    if (scroll.Count < Harmoshka.GetFullError())
                    {
                        scroll.Add(false);
                    }

                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        ifsPixels.Close();
                        ifsLabels.Close();
                        return;
                    }

                    if (di % 100 == 0) // Частота прорисовки графиков
                    {
                        worker.ReportProgress(0);
                    }

                    {//Подготовка изображения для загрузки в нейронную сеть
                        InputDataBuf.Clear();
                        byte b = 0;
                        for (int i = 0; i < 28; ++i)// Исходное изображение переписывается в массив с порогом яркости
                        {
                            for (int j = 0; j < 28; ++j)
                            {

                                b = brImages.ReadByte();
                                if (b > 190) // Порог яркости
                                {
                                    InputDataBuf.Add(1);
                                    pixels[i, j] = 1;
                                }
                                else
                                {
                                    InputDataBuf.Add(0);
                                    pixels[i, j] = 0;
                                }
                            }
                        }
                        b2 = MakeBitmap.Make_Bitmap(pixels, X_, Y_, false, 1, false); //Уменьшение исходного изображения для переферийного зрения
                        b2 = new Bitmap(b2, new Size(12, 12));
                        for (int i = 0; i < 12; ++i)
                        {
                            for (int j = 0; j < 12; ++j)
                            {
                                arrayb2[j, i] = b2.GetPixel(j, i).R;
                            }
                        }
                    }// Конец подготовки изображения

                    Index = (int)brLabels.ReadByte();// Индекс изображения для обучения

                    if (bb % 600f == 0)// Вывод статистики о времени работы и ошибке
                    {
                        Harmoshka.message += String.Format(">>>{0:000}% ", (bb / 600.001f)) + allEror;
                        Harmoshka.message += String.Format(" {00:00.00}", sw.ElapsedMilliseconds / 1000.0f) + "c." + "\r\n";
                        sw.Restart();
                    }

                    bb++;

                    allEror++;
                    all.Add(1);
                    Eror_Bool = true;


                    bool TabPagesBool = false;
                    tabControl1.Invoke((MethodInvoker)delegate
                    {
                        if (tabControl1.SelectedIndex == 1)
                        {
                            TabPagesBool = true;
                        }
                    });

                    int ll = 0;
                    counter.Assessment.Clear();
                    counter.room.Clear();
                    do //обработка одного изображения в цикле узнавания
                    {
                        col = true;
                        n_green++;
                        ;
                        preparation_input.PreparationInput_1(counter, col, semblance, Xb, Yb, reproduction, TabPagesBool);
                        col = preparation_input.col;
                        n_blekc += preparation_input.n_blekc;
                        n_green += preparation_input.n_green;
                        X = preparation_input.X;
                        Y = preparation_input.Y;


                        pictureBox3.Invoke((MethodInvoker)delegate
                        {
                            pictureBox3.Image = preparation_input.bitMap;
                        });


                        if (TabPagesBool)
                        {
                            bitMap = MakeBitmap.Make_Bitmap(pixels, X, Y, col);
                            pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                pictureBox1.Image = bitMap;
                            });
                        }

                        InputData.Clear();


                        byte[,] pixels_ = new byte[28, 28];

                        for (int i = 0; i < focus_scale; ++i)
                        {
                            for (int j = 0; j < focus_scale; ++j)
                            {
                                InputData.Add(pixels[i + Y - 3, j + X - 3]); // Запись во входящий вектор фокуса зрения
                                if (TabPagesBool)
                                {
                                    pixels_[i + 6, j + 6] = pixels[i + Y - 3, j + X - 3];
                                }
                            }
                        }

                        { //Дабавление перефирийное зрение к входящиму вектору
                            for (int i = 0; i < 12; ++i)
                            {
                                for (int j = 0; j < 12; ++j)
                                {

                                    if (arrayb2[j, i] > 200)
                                    {
                                        InputData.Add(0);
                                        InputData.Add(0);
                                    }
                                    else
                                    {
                                        InputData.Add(1);
                                        InputData.Add(1);
                                    }
                                }
                            }
                        }

                        if (TabPagesBool)
                        {
                            bitMap_ = MakeBitmap.Make_Bitmap(pixels_, 9, 9, col, 15, true);//прорисовка фокуса зрения
                            pictureBox2.Invoke((MethodInvoker)delegate
                            {
                                pictureBox2.Image = bitMap_;
                            });

                            b2 = new Bitmap(b2, new Size(280, 280)); //прорисовка переферийного зрения
                            pictureBox5.Invoke((MethodInvoker)delegate
                            {
                                pictureBox5.Image = b2;
                            });

                            pictureBox6.Invoke((MethodInvoker)delegate
                            {
                                pictureBox6.Image = preparation_input.bitMap_Draw;
                            });

                            label16.Invoke((MethodInvoker)delegate
                            {
                                label16.Text = "Повторение символа: " + ll.ToString();
                                label17.Text = "Множетель корекции входящего сигнала: " + semblance.ToString();
                            });
                        }


                        Xb = X;
                        Yb = Y;
                        for (int i = 0; i < preparation_input.InputData.Count; i++)
                        {
                            if (preparation_input.InputData[i] > 0)
                            {
                                InputData.Add(1);
                            }
                            else
                            {
                                InputData.Add(0);
                            }
                        }


                        for (int i = 0; i < InputData.Count; i++)
                        {
                            if (rnd.Next(0, 9) > 5)
                            {
                                InputData[i] = 0;
                            }
                            if (rnd.Next(0, 9) > 8)
                            {
                                InputData[i] = 1;
                            }

                        }


                        try
                        {
                            counter = Harmoshka.Assessment(14 * 14, InputData, semblance, Index);
                        }
                        catch (Exception ex)
                        {
                            //Выводим ошибку
                            MessageBox.Show(ex.ToString());
                        }

                        if (counter.str2 & Eror_Bool)
                        {
                            Eror_Bool = false;
                        }
                        IndexList[counter.Index]++;
                        ll++;
                    }
                    while ((ll < 25 | Eror_Bool) & ll < 50);//


                    if (TabPagesBool)
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
                                g.FillRectangle(sbIndex, 21 * i, 370 - (7 * IndexList[i] + 5), 20, (7 * IndexList[i] + 5));
                            }

                        });
                        pictureBox8.Invoke((MethodInvoker)delegate
                        {
                            Graphics g = pictureBox8.CreateGraphics();
                            g.Clear(Color.White);
                            Pen blackPen = new Pen(Color.Red, 1);
                            for (int i = 0; i < counter.Assessment.Count; i++)
                            {
                                blackPen.Color = Color.Black;
                                if (counter.room[i] == Index)
                                {
                                    blackPen.Color = Color.Red;
                                }
                                g.DrawLine(blackPen, i, 200, i, 200 - counter.Assessment[i] * 100);
                            }

                        });
                    }
                    int indexVar = IndexList[Index];
                    bool indexBool = false;
                    for (int i = 0; i < 10; i++)
                    {
                        if (IndexList[i] > indexVar)
                        {
                            indexBool = true;
                        }
                        IndexList[i] = 0;
                    }
                    if (!indexBool)
                    {
                        allEror--;
                        all[all.Count - 1] = 0;
                    }


                    if (Harmoshka.message != null)
                    {
                        worker.ReportProgress(di, Harmoshka.message);
                        Harmoshka.message = null;
                    }

                    if (l > 0)
                    {
                        allEror -= all[0];
                        all.RemoveAt(0);
                    }

                }
                if (Harmoshka.SleepStep < 200)
                {
                    Harmoshka.SleepStep += 5;
                }

                ifsPixels.Close();
                brImages.Close();
                ifsLabels.Close();
                brLabels.Close();
                InputData.Clear();
                InputData.TrimExcess();
            }
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
                double y1 = 0;
                y1 = allEror / 600f;

                if (n_blekc > 0)
                {
                    if (n_col == 0)
                    {
                        n_col = n_green / n_blekc;
                    }
                    else
                    {
                        n_col = (n_green / n_blekc + n_col) / 2f;
                    }

                }
                n_blekc = 0;
                n_green = 0;

                if (nn < 60000)
                {
                    y1 = 100 - (((nn - allEror) / 600f) / (nn / 600f)) * 100;
                }
                writePath = String.Format("{0:00.00}%", y1) + "\r\n" + writePath;
                label18.Text = writePath;

                if (series == null)
                {
                    series = new List<int>();
                }
                series.Add((int)y1);
                if ((sender as BackgroundWorker).CancellationPending != true)
                {
                    if (n_col > 0)
                    {
                        chart1.Series["Y"].Points.AddXY(series.Count - 2, y1);
                        chart2.Series["N"].Points.AddXY(series.Count - 2, n_col);
                    }
                }
                if (get_files == false)
                {
                    Consol.Text += "Файл изображения " + images + "\r\n";
                    Consol.Text += "Файл индексов " + labels + "\r\n";
                    get_files = true;
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

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
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
                List<string> session = new List<string>();
                session.Add(images);
                session.Add(labels);
                session.Add(copy);
                session.Add(textBox1.Text);
                session.Add(textBox3.Text);
                session.Add(textBox8.Text);
                session.Add(textBox7.Text);
                session.Add(textBox5.Text);
                session.Add(textBox12.Text);
                File.WriteAllLines(path, session.ToArray());
                MessageBox.Show("Сессия сохранена");
            }
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*.ini)|*.ini";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] text = File.ReadAllLines(openFileDialog1.FileName);
                    images = text[0];
                    labels = text[1];
                    //session_flag = true;

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

        private void изображенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*.idx3-ubyte)|*.idx3-ubyte";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                images = openFileDialog1.FileName;
            }
        }
        private void индексыToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*.idx1-ubyte)|*.idx1-ubyte";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                labels = openFileDialog1.FileName;
            }
        }

        private void экспортБазыToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void импортБазыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ini files (*ReverseMatte.dat)|*ReverseMatte.dat";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Harmoshka.LodeMatte(openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length - 16));
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
                button2.Enabled = false;
                button4.Text = "Стоп";
                session_flag = true;
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
                    button2.Enabled = true;
                    button4.Text = "Запись";
                    session_flag = false;
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
                button2.Text = "Стоп";
                session_flag = false;
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
                button2.Text = "Воспроизведение";
                session_flag = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

    }

}




