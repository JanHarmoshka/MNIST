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
        string ReportText;
        public bool reproduction_ = true;
        public int allEror = 0;
        private List<int> all = new List<int>();
        public int nn = 0;
        public float n_blekc = 0;
        public float n_green = 0;
        double n_col = 0;
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

        public static Bitmap MakeBitmap(byte[,] pixels, int X, int Y, bool Col, int mag = 10, bool DrawR = true)
        {
            int width = 28 * mag;
            int height = 28 * mag;
            Bitmap result = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(result);
            for (int i = 0; i < 28; ++i)
            {
                for (int j = 0; j < 28; ++j)
                {

                    int pixelColor = 255;
                    if (pixels[i, j] > 0)
                    {
                        pixelColor = 0;
                    }
                    // Черные цифры
                    Color c = Color.FromArgb(pixelColor, pixelColor, pixelColor);
                    SolidBrush sb = new SolidBrush(c);
                    gr.FillRectangle(sb, j * mag, i * mag, mag, mag);
                }
                if (DrawR)
                {
                    Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                    if (Col)
                    {
                        blackPen = new Pen(Color.FromArgb(255, 0, 200, 50), 5);
                    }

                    gr.DrawRectangle(blackPen, (X - 6) * mag, (Y - 6) * mag, 12 * mag, 12 * mag);
                }

            }
            return result;
        }
        public static Bitmap MakeBitmapLine(List<byte> InputData1, List<byte> InputData2, List<byte> InputData3, int mag = 20)
        {
            int width = 28 * mag;
            int height = 3 * mag;
            Bitmap result = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(result);
            for (int i = 0; i < 28; ++i)
            {
                int pixelColor = 255;
                if (InputData1[i] > 0)
                {
                    pixelColor = 0;
                }
                Color c = Color.FromArgb(pixelColor, pixelColor, pixelColor);
                SolidBrush sb = new SolidBrush(c);
                gr.FillRectangle(sb, i * mag, 0 * mag, mag - 1, mag - 1);

                pixelColor = 255;
                if (InputData2[i] > 0)
                {
                    pixelColor = 0;
                }
                c = Color.FromArgb(pixelColor, pixelColor, pixelColor);
                sb = new SolidBrush(c);
                gr.FillRectangle(sb, i * mag, 1 * mag, mag - 1, mag - 1);

                pixelColor = 255;
                if (InputData3[i] > 0)
                {
                    pixelColor = 0;
                }
                c = Color.FromArgb(pixelColor, pixelColor, pixelColor);
                sb = new SolidBrush(c);
                gr.FillRectangle(sb, i * mag, 2 * mag, mag - 1, mag - 1);
            }
            return result;
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
            string writePath = @"E:\hta.txt";
            List<float> InputData = new List<float>();
            List<byte> InputDataP = new List<byte>();
            List<byte> InputDataP2 = new List<byte>();
            List<byte> InputDataP3 = new List<byte>();
            List<float> InputDataBuf = new List<float>();

            byte[] XYBuf = { 7, 7 };
            int X = 7;
            int Y = 7;
            int X_ = 7;
            int Y_ = 7;
            int Xb = 0;
            int Yb = 0;
            int prevX1 = 0;
            int prevY1 = 0;
            int prevX2 = 0;
            int prevY2 = 0;
            //bool prev = false;
            int bb = 0;
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            bool col;
            float semblance;

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

                byte[,] pixelsBuf = new byte[28, 28];

                if (button1.Enabled == false)
                {
                    try
                    {
                        Harmoshka.SetFullError(int.Parse(textBox1.Text));

                        Harmoshka.SetSatiety(float.Parse(textBox3.Text));

                        Harmoshka.SetReverseSatiety(float.Parse(textBox4.Text));

                        Harmoshka.SetV1(float.Parse(textBox8.Text));

                        Harmoshka.SetV2(float.Parse(textBox7.Text));

                        Harmoshka.SetV3(float.Parse(textBox6.Text));

                        Harmoshka.SetV4(float.Parse(textBox5.Text));

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

                button1.Invoke((MethodInvoker)delegate
                {
                    button1.Enabled = true;
                });

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

                    if (di % 500 == 0)
                    {
                        worker.ReportProgress(0);
                    }

                    InputDataBuf.Clear();
                    for (int i = 0; i < 784; i++)
                    {
                        byte b = brImages.ReadByte();
                        if (b > 190)
                        {
                            InputDataBuf.Add(1);
                        }
                        else
                        {
                            InputDataBuf.Add(0);
                        }
                    }

                    Index = (int)brLabels.ReadByte();

                    if (bb % 600f == 0)
                    {
                        Harmoshka.message += String.Format(">>>{0:000}% ", (bb / 600.001f)) + allEror;
                        Harmoshka.message += String.Format(" {00:00.00}", sw.ElapsedMilliseconds / 1000.0f) + "c." + "\r\n";
                        sw.Restart();
                    }

                    bb++;

                    allEror++;
                    all.Add(1);

                    for (int ll = 0; ll < 30; ll++)
                    {
                        InputDataP.Clear();
                        X = 0;
                        Y = 0;
                        col = true;

                        if (counter.inter_Data_.Count >= 28)
                        {
                            InputDataP = counter.inter_Data_.GetRange(0, counter.inter_Data_.Count);
                            for (int j = 0; j < 28; ++j)
                            {
                                if (counter.inter_Data_[j] > 0)
                                {
                                    if (j < 14)
                                    {
                                        X = j - 1;
                                    }
                                    else
                                    {
                                        Y = j - 15;
                                    }
                                }
                            }
                        }
                        //prev = false;
                        semblance = 20;
                        if (prevX2 > 0 & prevY2 > 0)
                            if ((prevX2 == X | prevY2 == Y))//(prevX1 == X & prevY1 == Y) |
                            {
                                // prev = true;
                                semblance = 0;
                            }
                        prevX1 = prevX2;
                        prevY1 = prevY2;
                        prevX2 = X;
                        prevY2 = Y;

                        if ((counter.summ < 1 | counter.summ > 3) | (counter.summ2 < 1 | counter.summ2 > 3) | counter.inter_Data_.Count < 28 | X > 10 | Y > 10 | X < 3 | Y < 3 | (X == Xb & Y == Yb))// reproduction_ || (X * 2 == X_ | Y * 2 == Y_)
                        {
                            InputDataP.Clear();
                            do
                            {
                                col = false;//bleck
                                for (int i = 0; i < 2; i++)
                                {
                                    for (int j = 0; j < 1; ++j)
                                    {
                                        //byte rnd_buf = 0;
                                        //rnd_buf = Convert.ToByte(rnd.Next(1, 3));

                                        if (XYBuf[i] >= 0 & XYBuf[i] < 14)//- rnd_buf
                                        {
                                            if (rnd.Next(0, 9) > 3)
                                            {
                                                XYBuf[i]++;//+= rnd_buf
                                                continue;
                                            }
                                        }
                                        if (XYBuf[i] > 0 & XYBuf[i] <= 14)//rnd_buf
                                        {
                                            if (rnd.Next(0, 9) > 3)
                                            {
                                                XYBuf[i]--;//-= rnd_buf
                                            }
                                        }
                                    }

                                    for (int j = 0; j < 14; ++j)
                                    {
                                        if (XYBuf[i] == j | (XYBuf[i] == j - 1 & j > 1))
                                        {
                                            InputDataP.Add(1);
                                        }
                                        else
                                        {
                                            InputDataP.Add(0);
                                        }
                                    }
                                }
                                counter.inter_Data_ = InputDataP.GetRange(0, InputDataP.Count);
                            }
                            while ((X == XYBuf[0] & Y == XYBuf[1]));

                            //if ((X == XYBuf[0] & Y == XYBuf[1]) | (X * 2 == X_ & Y * 2 == Y_))
                            //{
                            //    //Harmoshka.message += X + " " + Xb + " " + Y + " " + Yb + "\r\n";
                            //Harmoshka.message += X_ + " " + Y_ + "/ " + XYBuf[0] * 2 + " " + XYBuf[1] * 2 + "/ " + X * 2 + " " + Y * 2 + "\r\n";// + "\r\n"
                            //}
                        }

                        if (col)
                        {
                            n_green++;
                        }
                        else
                        {
                            n_blekc++;
                        }

                        bool TabPagesBool = false;
                        tabControl1.Invoke((MethodInvoker)delegate
                         {
                             if (tabControl1.SelectedIndex == 1)
                             {
                                 TabPagesBool = true;
                             }
                         });

                        if ((counter.inter_Data_Full.Count > 0 & !reproduction_) | (counter.inter_Data_Full.Count > 0) & TabPagesBool)//Рисовать фокус внимания 
                        {
                            byte[,] pixels__ = new byte[28, 28];
                            int n = 0;
                            for (int i = 0; i < 28; i++)
                            {
                                for (int j = 0; j < 12; j++)
                                {
                                    pixels__[i, j] = 0;

                                    if (counter.inter_Data_Full[n] > 0)
                                    {
                                        pixels__[i, j] = 1;

                                    }
                                    n++;
                                }
                            }
                            for (int i = 0; i < 28; i++)
                            {
                                for (int j = 12; j < 26; j++)
                                {
                                    pixels__[i, j] = 0;

                                    if (counter.inter_Data_Full[n] > 0)
                                    {
                                        pixels__[i, j] = 1;

                                    }
                                    n++;
                                    if (counter.inter_Data_Full.Count - 28 < n)
                                    {
                                        break;
                                    }
                                }
                            }

                            bitMap_ = MakeBitmap(pixels__, 6, 6, col, 10);
                            pictureBox3.Invoke((MethodInvoker)delegate
                            {
                                pictureBox3.Image = bitMap_;
                            });

                            //InputDataP2.Clear();
                            //for (int i = counter.inter_Data_Full.Count - 28; i < counter.inter_Data_Full.Count; i++)
                            //{
                            //    InputDataP2.Add(counter.inter_Data_Full[i]);
                            //}

                            //bitMap4 = MakeBitmapLine(InputDataP2, InputDataP);
                            //pictureBox4.Invoke((MethodInvoker)delegate
                            //{
                            //    pictureBox4.Image = bitMap4;
                            //});
                        }// Конец прорисовки фокуса внимания

                        InputDataP.Clear();
                        InputDataP3.Clear();
                        InputDataP = counter.inter_Data_.GetRange(0, counter.inter_Data_.Count);
                        InputDataP3 = counter.inter_Data_.GetRange(0, counter.inter_Data_.Count);
                        for (int j = 0; j < 28; ++j)
                        {
                            if (counter.inter_Data_[j] > 0)
                            {
                                if (j < 14)
                                {
                                    X = j - 1;//
                                }
                                else
                                {
                                    Y = j - 15;
                                }
                            }
                        }
                        //if (!col)
                        //{
                        //    Harmoshka.message += Xb + " " + Yb + "/ " + XYBuf[0] + " " + XYBuf[1] + "/ " + X + " " + Y + "\r\n";
                        //}

                        byte[,] pixels = new byte[28, 28];

                        //if ((X * 2 == X_ & Y * 2 == Y_))
                        //if (!col)
                        //{
                        //    Harmoshka.message += X * 2 + " " + X_ + " " + XYBuf[0] * 2 + " " + Y * 2 + " " + Y_ + " " + XYBuf[1] * 2 + "\r\n";// + "\r\n"
                        //}

                        X_ = X * 2;//координаты зрачка
                        Y_ = Y * 2;

                        if (X_ < 6 | X_ > 20)
                        {
                            X_ = 14;
                            XYBuf[0] = 7;
                            X = 7;
                        }

                        if (Y_ < 6 | Y_ > 20)
                        {
                            Y_ = 14;
                            XYBuf[1] = 7;
                            Y = 7;
                        }



                        int Lp = 0;
                        for (int i = 0; i < 28; ++i)
                        {
                            for (int j = 0; j < 28; ++j)
                            {
                                pixels[i, j] = (byte)InputDataBuf[Lp];
                                Lp++;
                            }
                        }

                        Bitmap b2 = MakeBitmap(pixels, X_, Y_, col, 1, false);
                        b2 = new Bitmap(b2, new Size(12, 12));

                        if (TabPagesBool)
                        {
                            bitMap = MakeBitmap(pixels, X_, Y_, col);
                            pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                pictureBox1.Image = bitMap;
                            });
                        }

                        InputData.Clear();
                        byte[,] pixels_ = new byte[28, 28];
                        for (int i = 0; i < 12; ++i)
                        {
                            for (int j = 0; j < 12; ++j)
                            {
                                InputData.Add(pixels[i + Y_ - 6, j + X_ - 6]);
                                if (TabPagesBool)
                                {
                                    pixels_[i + 8, j + 8] = pixels[i + Y_ - 6, j + X_ - 6];
                                }

                            }
                        }

                        for (int i = 0; i < 12; ++i)
                        {
                            for (int j = 0; j < 12; ++j)
                            {
                                Color pixelsCol;
                                pixelsCol = b2.GetPixel(j, i);
                                if (pixelsCol.R > 200)
                                {
                                    InputData.Add(0);
                                }
                                else
                                {
                                    InputData.Add(1);
                                }
                            }
                        }
                        b2 = new Bitmap(b2, new Size(280, 280));

                        if (TabPagesBool)
                        {
                            bitMap_ = MakeBitmap(pixels_, 14, 14, col, 10, false);
                            pictureBox2.Invoke((MethodInvoker)delegate
                            {
                                pictureBox2.Image = bitMap_;
                            });

                            pictureBox5.Invoke((MethodInvoker)delegate
                            {
                                pictureBox5.Image = b2;
                            });
                        }
                        //



                        if (InputData_.Count == 0)
                            InputData_ = InputData.GetRange(0, InputData.Count);

                        InputDataP.Clear();

                        for (int j = 0; j < 14; ++j)
                        {
                            if (X == j | (X == j - 1 & j > 1))
                            {
                                InputDataP.Add(1);
                            }
                            else
                            {
                                InputDataP.Add(0);
                            }
                        }

                        for (int j = 0; j < 14; ++j)
                        {
                            if (Y == j | (Y == j - 1 & j > 1))
                            {
                                InputDataP.Add(1);
                            }
                            else
                            {
                                InputDataP.Add(0);
                            }
                        }



                        XY = new Point(Y, X);//Запись пройденного пути
                        pixels_way_List.Add(XY);
                        pixels_way[pixels_way_List[pixels_way_List.Count - 1].X, pixels_way_List[pixels_way_List.Count - 1].Y] = 1;
                        if (pixels_way_List.Count > 30)
                        {
                            pixels_way[pixels_way_List[0].X, pixels_way_List[0].Y] = 0;
                            pixels_way_List.RemoveAt(0);
                        }
                        if (TabPagesBool)
                        {
                            bitMap_way = MakeBitmap(pixels_way, pixels_way_List[pixels_way_List.Count - 1].X * 2, pixels_way_List[pixels_way_List.Count - 1].Y * 2, false, 20, false);
                            pictureBox6.Invoke((MethodInvoker)delegate
                            {
                                pictureBox6.Image = bitMap_way;
                            });
                        }
                        for (int i = 0; i < 14; ++i)
                        {
                            for (int j = 0; j < 14; ++j)
                            {
                                if (pixels_way[i, j] > 0)
                                {
                                    InputData.Add(1);
                                }
                                else
                                {
                                    InputData.Add(0);
                                }
                            }
                        }//конец записи пройденного пути

                        if (counter.inter_Data_Full.Count > 0 & TabPagesBool)
                        {
                            InputDataP2.Clear();
                            for (int i = counter.inter_Data_Full.Count - 28; i < counter.inter_Data_Full.Count; i++)
                            {
                                InputDataP2.Add(counter.inter_Data_Full[i]);
                            }

                            bitMap4 = MakeBitmapLine(InputDataP2, InputDataP3, InputDataP);
                            pictureBox4.Invoke((MethodInvoker)delegate
                            {
                                pictureBox4.Image = bitMap4;
                            });
                        }

                        if (TabPagesBool)
                        {
                            label16.Invoke((MethodInvoker)delegate
                            {
                                label16.Text = ll.ToString();
                                label13.Text = Xb.ToString() + " / " + Yb.ToString();
                                label14.Text = X.ToString() + " / " + Y.ToString();
                            });
                            Thread.Sleep(200);
                        }
                        Xb = X;
                        Yb = Y;
                        //if (ll == 10)
                        //{
                        //    Harmoshka.message += X + " " + Y + "\r\n";
                        //}

                        for (int i = 0; i < 28; ++i)
                        {
                            InputData.Add(InputDataP[i]);

                        }

                        counter = Harmoshka.Assessment(28, InputData, semblance, Index);
                        if (counter.str2)
                        {
                            allEror--;
                            all[all.Count - 1] = 0;
                            break;
                        }

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

                label15.Text = y1.ToString();

                if (series == null)
                {
                    series = new List<int>();
                }
                series.Add((int)y1);
                if ((sender as BackgroundWorker).CancellationPending != true)
                {
                    chart1.Series["Y"].Points.AddXY(series.Count - 2, y1);
                    if (n_col > 0)
                    {
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
                session.Add(textBox4.Text);
                session.Add(textBox8.Text);
                session.Add(textBox7.Text);
                session.Add(textBox6.Text);
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

                    textBox1.Text = text[3];
                    textBox3.Text = text[4];
                    textBox4.Text = text[5];

                    textBox8.Text = text[6];
                    textBox7.Text = text[7];
                    textBox6.Text = text[8];
                    textBox5.Text = text[9];
                    textBox12.Text = text[10];


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
            reproduction_ = true;


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
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
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
                    checkBox1.Enabled = true;
                    checkBox2.Enabled = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + "\\settings.ini";
            reproduction_ = false;

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
                button4.Enabled = false;
                button2.Text = "Стоп";
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                //label13.Visible = false;
                //label14.Visible = false;
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
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                //label13.Visible = true;
                //label14.Visible = true;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

    }

}




