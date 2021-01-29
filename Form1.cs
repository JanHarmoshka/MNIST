using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        string images, labels, index_di;
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
            chart1.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;
            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Consolas", 6);
            chart1.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Consolas", 10);
            chart1.Series["Y"].BorderWidth = 2;
            chart1.ChartAreas[0].AxisX.Minimum = 0;

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

                List<float> InputData = new List<float>();

                //поиск файла индекса

                for (int di = 0; di < Harmoshka.GetFullError(); di++)
                {
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

                    InputData.Clear();
                    for (int i = 0; i < 784; i++)
                    {
                        byte b = brImages.ReadByte();
                        if (b > 200)
                        {
                           InputData.Add(1);
                        }
                        else
                        {
                            InputData.Add(0);
                        }
                    }
                    Index = (int)brLabels.ReadByte();
                    counter = Harmoshka.Assessment(InputData, Index);

                    if (Harmoshka.message != null)
                    {
                        worker.ReportProgress(di, Harmoshka.message);
                        Harmoshka.message = null;
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
                Consol.Text += "Canceled\r\n";
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
                Consol.Text += "Done...\r\n";
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                double y = 100;
                if (Harmoshka.GetNN() < Harmoshka.GetFullError())
                {
                    y = (float)(100 - (((Harmoshka.GetNN() - Harmoshka.GetAllError()) / (float)(Harmoshka.GetFullError() / 100f)) / (Harmoshka.GetNN() / (float)(Harmoshka.GetFullError() / 100f))) * 100);
                }
                else
                {
                    y = (float)(Harmoshka.GetAllError() / (float)(Harmoshka.GetFullError() / 100f));
                }
                if (series == null)
                {
                    series = new List<int>();
                }
                series.Add((int)y);
                if ((sender as BackgroundWorker).CancellationPending != true)
                {
                    chart1.Series["Y"].Points.AddXY(series.Count - 2, y);
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
                backgroundWorker1.RunWorkerAsync();
                button4.Enabled = false;
                button2.Text = "Стоп";

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
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

    }

}




