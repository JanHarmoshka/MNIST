using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MNIST
{

    /// Анализ активности и обучение масок.
    [Serializable]
    public class Harmoshka
    {

        public string message = null;
        public List<Matte> ListMatte = new List<Matte>();
        public List<ReverseMatte> ListReverseMatte = new List<ReverseMatte>();

        private List<int> all = new List<int>();

        private int allEror { get; set; }
        public void SetAllError(int val) { allEror = val; }
        public int GetAllError() { return allEror; }


        private int fulEror = 60000;//Количество элементов
        public void SetFullError(int val) { fulEror = val; }
        public int GetFullError() { return fulEror; }

        private int nn = 0;
        public void SetNN(int val) { nn = val; }
        public int GetNN() { return nn; }

        private bool Lesson_trigger = false;
        public void SetTrigger(bool val) { Lesson_trigger = val; }
        public bool GetTrigger() { return Lesson_trigger; }

        private float satiety = 0.3f; //Порог жизни маски
        public void SetSatiety(float val) { satiety = val; }
        public float GetSatiety() { return satiety; }

        private float ReverseSatiety = 0.0f;//Порог разрушения группы
        public void SetReverseSatiety(float val) { ReverseSatiety = val; }
        public float GetReverseSatiety() { return ReverseSatiety; }

        private float V1 = 0.8f;//Порог коррекции входа
        public void SetV1(float val) { V1 = val; }
        public float GetV1() { return V1; }

        private float V2 = 0.7f;//Мах возраст участия
        public void SetV2(float val) { V2 = val; }
        public float GetV2() { return V2; }

        private float V3 = 0.4f;//Порог влияния на вход
        public void SetV3(float val) { V3 = val; }
        public float GetV3() { return V3; }

        private float V4 = 0.5f;//Min возраст участия
        public void SetV4(float val) { V4 = val; }
        public float GetV4() { return V4; }

        private float V5 = 0.2f;//Мах активность группы
        public void SetV5(float val) { V5 = val; }
        public float GetV5() { return V5; }

        private int V6 = 1000;//Длительность памяти
        public void SetV6(int val) { V6 = val; }
        public int GetV6() { return V6; }


        private List<float> inter_Data = new List<float>();
        List<float> assessment = new List<float>();
        System.Diagnostics.Stopwatch sw = new Stopwatch();


        public Counter Assessment(List<float> InputData, int IndexData = -1)
        {

            List<float> inter_result = new List<float>();
            List<int> ContractionInputData = new List<int>();
            List<int> ContractionInterResult = new List<int>();
            bool Correct_trigger = true;
            bool Pass = true;
            float ReverseSatiety = 0.0f;
            int fulEror100 = (int)(fulEror / 100f);
            Counter counter = new Counter();

            float assessment_;
            float appeal_;
            float inter_Data_;

            if (nn == 0)
            {
                sw.Start();
            }
            nn++;



            // Чистка
            if (nn % V6 == 0)
            {
                List<int> Empty = new List<int>();
                for (int j = 0; j < ListMatte.Count; j++)
                {
                    if (ListMatte[j].appeal == 0.3f & ListMatte[j].Control_value <= 0)
                    {
                        //message += "-";
                        Empty.Add(j);
                    }
                }

                if (Empty.Count > 0)
                {
                    int e = 0;
                    for (int j = 0; j < Empty.Count; j++)
                    {
                        e = Empty[j] - j;
                        ListMatte.RemoveAt(e);
                        for (int i = 0; i < ListReverseMatte.Count; i++)
                        {
                            if (ListReverseMatte[i].Correct.Count > e)
                            {
                                ListReverseMatte[i].Correct.RemoveAt(e);
                                ListReverseMatte[i].Refined.RemoveAt(e);
                            }
                        }
                    }
                }
                Empty.Clear();

                for (int j = 0; j < ListReverseMatte.Count; j++)
                {
                    if (ListReverseMatte[j].appeal_ <= 0.0f & ListReverseMatte[j].Control_value <= 0f)
                    {
                        ListReverseMatte.RemoveAt(j);
                    }
                    else
                    {
                        if (ListReverseMatte[j].Correct.Count < ListMatte.Count * 0.6f)
                        {
                            ListReverseMatte[j].Control_value = ListReverseMatte[j].Control_value - 0.1f;
                        }
                        if (ListReverseMatte[j].Correct.Count < ListMatte.Count * 0.95f & ListReverseMatte[j].appeal_ > 0.01f)
                        {
                            ListReverseMatte[j].appeal_ = ListReverseMatte[j].appeal_ - 0.0005f;

                        }
                    }
                }
            }//Конец чистки


            inter_Data.Clear();
            inter_Data = InputData.GetRange(0, InputData.Count);

            //Коррекция сигнала от сенсоров (новый вариант)
            if (inter_Data.Count > 0 & ListReverseMatte.Count > 0 & assessment.Count > 0)
            {
                for (int j = 0; j < ListReverseMatte.Count; j++)
                {
                    if (ListReverseMatte[j].Control_value > 0f & assessment[j] > 0.0f)
                    {
                        assessment_ = assessment[j];
                        appeal_ = ListReverseMatte[j].appeal_;

                        for (int i = 0; i < inter_Data.Count; i++)
                        {
                            inter_Data_ = InputData[i] + ListReverseMatte[j].matte[i] * assessment_ * appeal_ * 10f;
                            if (inter_Data[i] < inter_Data_)
                            {
                                inter_Data[i] = inter_Data_;
                            }

                        }
                    }
                }
            }//Конец коррекции

            // Порог минимального значения после коррекции
            for (int i = 0; i < inter_Data.Count; i++)
            {
                if (inter_Data[i] <= V1)
                {
                    inter_Data[i] = 0;
                }
                else
                {
                    ContractionInputData.Add(i);//Для ускорения расчётов
                }
            }

            assessment.Clear();


            //Инициализация масок
            if (ListMatte.Count == 0)
            {
                Matte matte = new Matte(InputData, 0, satiety);
                ListMatte.Add(matte);
            }

            //Расчёт активности масок - поиск совпадений
            float Activ = 0;
            float Activ_ = -1;
            int n;
            int Index = 0;

            for (int i = 0; i < ListMatte.Count; i++)
            {
                Activ = 0;
                if (ListMatte[i].appeal == 0.3f & ListMatte[i].Control_value <= 0) { }
                else
                {
                    for (int j = 0; j < ContractionInputData.Count; j++)
                    {
                        n = ContractionInputData[j];
                        Activ += ListMatte[i].matte[n] * inter_Data[n];
                    }
                }
                if (Activ > Activ_)
                {
                    Activ_ = Activ;
                    Index = i;
                }
                if (Activ > -0.0f)
                {
                    inter_result.Add(Activ);
                    ContractionInterResult.Add(inter_result.Count - 1);
                }
                else
                {
                    inter_result.Add(0);
                }
            } //Расчёт активности масок - поиск совпадений


            //Обучение масок
            if (Activ_ > ListMatte[Index].appeal + 0.1f)//
            {
                ListMatte[Index].Lesson(InputData);
                if (Activ_ > ListMatte[Index].appeal & ListMatte[Index].appeal < 0.9f)
                {
                    ListMatte[Index].appeal = ListMatte[Index].appeal + 0.001f;
                }
            }
            //else
            //{
            if (Activ_ <= satiety + 0.1f)//
            {
                Matte matte = new Matte(InputData, (ushort)(ListMatte.Count), satiety);
                ListMatte.Add(matte);
            }
            //}
            //Конец обучения масок

            //Расчёт активности результирующих масок 
            var Ind = -1;
            Activ = -1;
            for (int i = 0; i < ListReverseMatte.Count; i++)
            {
                if (assessment.Count != ListReverseMatte.Count)
                {
                    assessment.Add(0);
                }
                if (ListReverseMatte[i].appeal_ <= 0.0f & ListReverseMatte[i].Control_value <= 0f) { }
                else
                {
                    for (int j = 0; j < ContractionInterResult.Count; j++)
                    {
                        n = ContractionInterResult[j];
                        if (ListReverseMatte[i].Correct.Count > n)
                        {
                            if (ListReverseMatte[i].Correct[n] > 0)
                            {
                                assessment[i] = assessment[i] + inter_result[n] * ListReverseMatte[i].Correct[n];
                            }
                        }
                    }
                }

                if (Activ < assessment[i] & assessment[i] > 0)
                {
                    Activ = assessment[i];
                    Ind = i;
                }
            }//Конец расчёт активности результирующих масок 
            counter.assessment_.Clear();

            //Подсчет ошибки
            if (ListReverseMatte.Count < Ind | Ind == -1)
            {
                all.Add(1);
                allEror++;
                counter.str2 = false;
            }
            else
            {
                if (ListReverseMatte[Ind].room != IndexData | IndexData == -1)
                {
                    all.Add(1);
                    allEror++;
                    counter.str2 = false;
                }
                else
                {
                    all.Add(0);
                    counter.str2 = true;
                }
            }

            if (all.Count > fulEror)
            {
                allEror -= all[0];
                all.RemoveAt(0);
            }


            if (nn % (fulEror100 * 1) == 0)//Вывод ошибки
            {
                message = "\r\n";
                message += String.Format("{0:0000}", (double)nn / fulEror100) + "%"; // Сколько элементов просмотрено, %

                if (nn < fulEror)
                {
                    // message += String.Format(" {0:00.00}", (double)(nn - allEror) / fulEror100); // Какой процент без ошибки среди просмотренных, %
                    if (nn != 0)
                    {
                        message += " ошибка:" + String.Format("{0:00.00}", (100f - (((nn - allEror) / (float)fulEror100) / (float)(nn / (float)fulEror100)) * 100f)) + "%"; // Предполагаемая ошибка
                    }
                }
                else
                {
                    message += " ошибка:" + String.Format("{0:00.00}", (double)allEror / fulEror100) + "%"; // Какой процент ошибки среди просмотренных, %		
                }
                message += " нейронов:" + ListMatte.Count.ToString() + " групп:" + ListReverseMatte.Count.ToString();
            }
            //Конец подсчёта ошибок	

            // Формирование результирующей маски
            if (ListReverseMatte.Count == 0)
            {
                ReverseMatte reverseMatte = new ReverseMatte(InputData, IndexData, inter_result, ReverseSatiety);
                ListReverseMatte.Add(reverseMatte);
            }
            else
            {
                if (Ind == -1)
                {
                    if (Activ_ > 0)
                    {
                        for (int i = 0; i < assessment.Count; i++)
                        {
                            assessment[i] = 0;
                        }
                        ReverseMatte reverseMatte = new ReverseMatte(InputData, IndexData, inter_result, ReverseSatiety);
                        ListReverseMatte.Add(reverseMatte);
                        Ind = ListReverseMatte.Count - 1;
                        assessment.Add(1);
                        ListReverseMatte[Ind].Sleep();
                        Pass = false;
                    }
                }
                else
                {
                    if (ListReverseMatte[Ind].appeal_ > V2 & Activ < 1f - ListReverseMatte[Ind].appeal_ & ListReverseMatte.Count < 1000)//
                    {
                        for (int i = 0; i < assessment.Count; i++)
                        {
                            assessment[i] = 0;
                        }
                        ReverseMatte reverseMatte = new ReverseMatte(InputData, ListReverseMatte[Ind].room, inter_result, ReverseSatiety);
                        ListReverseMatte.Add(reverseMatte);
                        Ind = ListReverseMatte.Count - 1;
                        assessment.Add(1);
                        ListReverseMatte[Ind].Sleep();
                        Pass = false;
                    }

                }
            }

            counter.str1 = Activ;
            // Конец формирования результирующей маски		


            //Начало обучения результирующей маски
            float RoomValue = -1000;
            int RoomIndex = -1;
            if (Ind > -1 & Lesson_trigger & Pass)// Принудительное обучение на индексированных данных
            {
                if (ListReverseMatte[Ind].room != IndexData)
                {
                    for (int i = 0; i < assessment.Count; i++)
                    {
                        if (i != Ind & ListReverseMatte[i].room == IndexData & assessment[i] > RoomValue)
                        {
                            RoomValue = assessment[i];
                            RoomIndex = i;
                        }
                    }
                    if (RoomIndex > -1 & RoomValue > -1000)
                    {
                        assessment[Ind] = 0;
                        Correct_trigger = false;
                        Ind = RoomIndex;
                    }
                }
            }// Конец принудительного обучения
            if (Activ > 1)
            {
                for (int i = 0; i < ListReverseMatte.Count; i++)
                {
                    if (assessment[i] > 0)
                    {
                        assessment[i] = (float)assessment[i] / Activ;
                    }
                }
            }
            if (Ind > -1 & Pass)
            {
                Activ = assessment[Ind];
                if (ListReverseMatte[Ind].room == IndexData | !Lesson_trigger)//Основной цикл обучения
                {
                    if (!Correct_trigger & ListReverseMatte[Ind].appeal_ < V3)
                    {
                        Correct_trigger = false;
                    }
                    if (assessment[Ind] > ListReverseMatte[Ind].appeal_ | Lesson_trigger)
                    {
                        ListReverseMatte[Ind].Lesson(InputData, inter_result, 1, Correct_trigger);
                        if (assessment[Ind] > ListReverseMatte[Ind].appeal_ & ListReverseMatte[Ind].appeal_ < 0.95f)
                        {
                            ListReverseMatte[Ind].appeal_ += 0.01f;
                        }
                    }
                }//Конец основного цикла обучения

                for (int i = 0; i < ListReverseMatte.Count; i++)//Формирование обстракций
                {
                    if (ListReverseMatte[i].appeal_ > V4 & ListReverseMatte[i].appeal_ < V2)
                    {
                        if (Ind != i & assessment[i] > V5 & assessment[i] < V2)
                        {
                            ListReverseMatte[i].Lesson(InputData, inter_result, (assessment[i] - (V5 + 0.01f)), false);
                            ListReverseMatte[i].Control_value++;
                            if (ListReverseMatte[i].appeal_ < 0.90f)
                            {
                                ListReverseMatte[i].appeal_ += 0.001f;
                            }
                        }
                    }
                }//Конец формирования абстракций
            }
            //Конец обучения результирующей маски

            //Начало фиксации обучения
            if (nn % 10 == 0)
            {
                for (int j = 0; j < ListMatte.Count; j++)
                {
                    if (ListMatte[j].Contraction)
                    {
                        ListMatte[j].Sleep();
                    }
                    else
                    {
                        if (ListMatte[j].Contraction_)
                        {
                            ListMatte[j].Control_value--;
                        }
                    }
                }
                for (int i = 0; i < ListReverseMatte.Count; i++)
                {
                    if (ListReverseMatte[i].Contraction)
                    {
                        ListReverseMatte[i].Sleep();
                    }
                    else
                    {
                        if (ListReverseMatte[i].Contraction_ & Pass)
                        {
                            ListReverseMatte[i].Control_value = ListReverseMatte[i].Control_value - 0.1f;
                        }
                    }

                }
                if (nn % (fulEror100 * 1) == 0)
                {
                    message += String.Format(" {00:00.00}", sw.ElapsedMilliseconds / 1000.0) + "c.";
                    sw.Restart();
                }


            }
            //Конец фиксации обучения	
            counter.str1 = allEror;
            return counter;
        }

        // Сохранение масок
        public void SaveMatte(string path)
        {
            BinaryFormatter Serial = new BinaryFormatter();

            using (FileStream fs = new FileStream(path + "Matte.dat", FileMode.OpenOrCreate))
            {
                Serial.Serialize(fs, ListMatte);
            }
            using (FileStream fs = new FileStream(path + "ReverseMatte.dat", FileMode.OpenOrCreate))
            {
                Serial.Serialize(fs, ListReverseMatte);
            }
            message += "Объект экспортирован";
        }
        public void LodeMatte(string path)
        {
            BinaryFormatter Serial = new BinaryFormatter();

            using (FileStream fs = new FileStream(path + "Matte.dat", FileMode.OpenOrCreate))
            {
                ListMatte = (List<Matte>)Serial.Deserialize(fs);
            }

            using (FileStream fs = new FileStream(path + "ReverseMatte.dat", FileMode.OpenOrCreate))
            {
                ListReverseMatte = (List<ReverseMatte>)Serial.Deserialize(fs);
            }
            message += "Объект импортирован";
        }

        public void ClearMatte()
        {
            ListMatte.Clear();
            ListReverseMatte.Clear();
        }
    }


    /// Маска 
    [Serializable]
    public class Matte
    {
        public int room;
        public float appeal;
        public bool Contraction;
        public bool Contraction_;
        public int Control_value;
        private List<float> mask = new List<float>();
        public List<float> matte = new List<float>();


        public Matte()
        {

        }

        public Matte(List<float> InputData, ushort Room, float satiety = 0.4f)
        {
            room = Room;
            mask = InputData.GetRange(0, InputData.Count);
            for (int i = 0; i < mask.Count; i++)
            {
                matte.Add(0);
            }
            appeal = satiety;
            Control_value = 500;
            this.Sleep();
            Contraction = true;
        }

        /// Сон маски. 
        /// Это ситуация повторного обучения маски, после обновления
        /// представления об ассоциированном событии.
        public void Sleep()
        {
            Contraction = false;
            Contraction_ = false;
            Single max = 0;
            Single max_0 = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                if (max < mask[j])
                    max = mask[j];
            }
            max_0 = max;

            max = (float)max / 2;
            Single summ = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                matte[j] = mask[j] - max;
                if (matte[j] > 0)
                    summ = summ + matte[j];
            }

            Single summ_0 = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                if (Math.Abs(matte[j] / summ) >= 0.01f)
                {
                    matte[j] = (float)(matte[j] / summ);
                }
                else
                {
                    matte[j] = 0;
                }
                summ_0 = summ_0 + matte[j];
            }
            if (max_0 < 2.0f)
            {
                if (summ_0 < 0.1f)
                {
                    if (Control_value > 0)
                    {
                        Control_value--;
                        Contraction_ = true;
                    }
                }
                else
                {
                    Control_value = 1000;
                }

            }
        }

        /// Обучение маски.
        /// Это ситуация дообучения маски, на текущем представлении о событии.
        public void Lesson(List<float> InputData, float res)
        {
            Contraction = true;
            for (int j = 1; j < mask.Count; j++)
            {
                if (InputData[j] >= 1)
                {
                    mask[j] = (mask[j] + (InputData[j] * res * 10f));
                }
            }
        }

        /// Обучение маски.
        /// Это ситуация обучения, на выборке относящейся к искомому событию.
        public void Lesson(List<float> InputData)
        {
            Contraction = true;
            for (int j = 1; j < mask.Count; j++)
            {
                if (InputData[j] >= 1)
                {
                    mask[j]++;
                }
            }
        }

        public void Add(List<float> InputData)
        {
            for (int i = mask.Count; i < InputData.Count; i++)
            {
                mask.Add(InputData[i]);
                matte.Add(0);
            }
        }
    }

    /// Результирующая маска
    [Serializable]
    public class ReverseMatte
    {
        public int room;
        public float Control_value;
        public bool Contraction;
        public bool Contraction_;
        private float appeal;
        public float appeal_;
        private List<float> mask = new List<float>();
        public List<float> matte = new List<float>();
        public List<Single> Refined = new List<Single>();
        public List<Single> Correct = new List<Single>();
        public List<Single> Appeal = new List<Single>();

        public ReverseMatte()
        {

        }

        public ReverseMatte(List<float> InputData, int IndexData, List<float> inter_result, float satiety = 0.3f)
        {
            appeal = satiety;
            appeal_ = 0.0f;
            room = IndexData;
            mask = InputData.GetRange(0, InputData.Count);
            Refined = inter_result.GetRange(0, inter_result.Count);
            for (int i = 0; i < mask.Count; i++)
            {
                matte.Add(0);
            }
            for (int i = 0; i < Refined.Count; i++)
            {
                Correct.Add(0);
                Appeal.Add(appeal);
            }
            Control_value = 100;
            this.Sleep();
            Contraction = true;
        }

        /// Обучение результирующей маски.
        /// Это ситуация дообучения результирующей маски, на текущем
        /// представлении о событии которое ассоциировано с группой.
        public void Lesson(List<Single> InputData, List<float> inter_result, Single res, bool leader)
        {
            Contraction = true;

            if (matte.Count != InputData.Count)
                for (int i = matte.Count; i < InputData.Count; i++)
                {
                    matte.Add(0);
                }
            if (Correct.Count < inter_result.Count)
            {
                for (int i = Correct.Count; i < inter_result.Count; i++)
                {
                    Correct.Add(0);
                    Refined.Add(0);
                }
            }
            if (Appeal.Count < Correct.Count)
            {
                for (int i = Appeal.Count; i < Correct.Count; i++)
                {
                    Appeal.Add(appeal);
                }
            }

            for (int j = 1; j < inter_result.Count; j++)
            {
                if (inter_result[j] > 0.0f)
                {
                    if (Correct[j] * 10f >= Appeal[j] | Correct[j] < 0.00001f)
                    {
                        Refined[j] = (float)(Refined[j] + inter_result[j] * res);
                        if (Appeal[j] < 0.3f)
                        {
                            Appeal[j] = Appeal[j] + 0.0001f;
                        }
                    }
                }
            }

            if (leader)
            {
                for (int j = 1; j < InputData.Count; j++)
                {
                    if (InputData[j] == 1)
                    {
                        mask[j]++;
                    }
                }
            }
            this.Sleep();
        }

        /// Сон результирующей маски.
        /// Это ситуация повторного обучения результирующей маски, после обновления
        /// представления об ассоциированном с группой событии.
        public void Sleep()
        {
            Contraction = false;
            Contraction_ = false;
            Single max = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                if (max < mask[j])
                    max = mask[j];
            }

            max = (float)max / 2;
            Single summ = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                matte[j] = mask[j] - max;
                if (matte[j] > 0)
                    summ = summ + matte[j] * 1f;
            }

            for (int j = 0; j < mask.Count; j++)
            {
                if (Math.Abs(matte[j] / summ) >= 0.01)
                {
                    matte[j] = (float)(matte[j] / summ);
                }
                else
                {
                    matte[j] = 0;
                }
            }

            max = 0.01f;
            for (int j = 1; j < Refined.Count; j++)
            {
                if (Refined[j] > max)
                    max = Refined[j];
            }

            Single summ_0 = 0;
            for (int j = 1; j < Refined.Count; j++)
            {
                Correct[j] = Refined[j] / max;
                summ_0 = summ_0 + Correct[j];
            }
            summ_0 = summ_0 / Correct.Count;

            if (Math.Abs(summ_0) < 1.1f)
            {
                if (Control_value > 0)
                {
                    Control_value--;
                    Contraction_ = true;
                }
            }
            else
            {
                Control_value = 100;
            }
        }

        public void Add(List<float> InputData)
        {
            for (int i = mask.Count; i < InputData.Count; i++)
            {
                mask.Add(InputData[i]);
                matte.Add(0);
            }
        }
    }
    public class Counter
    {
        public float str1;
        public bool str2;
        public List<float> assessment_ = new List<float>();
    }
}
