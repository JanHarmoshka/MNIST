using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MNIST
{

    /// Анализ активности и обучение масок.
    [Serializable]
    public class Harmoshka
    {
        public string message = null;
        public List<Matte> ListMatte = new List<Matte>();
        public List<ReverseMatte> ListReverseMatte = new List<ReverseMatte>();
        List<float> inter_result_ = new List<float>();

        private int fulEror = 60000;//Количество элементов
        public void SetFullError(int val) { fulEror = val; }
        public int GetFullError() { return fulEror; }

        private int nn = 0;
        public void SetNN(int val) { nn = val; }
        public int GetNN() { return nn; }

        private bool Lesson_trigger = false;
        public void SetTrigger(bool val) { Lesson_trigger = val; }
        public bool GetTrigger() { return Lesson_trigger; }

        private float satiety = 0.1f; //Порог жизни маски
        public void SetSatiety(float val) { satiety = val; }
        public float GetSatiety() { return satiety; }

        private float V1 = 0.5f;//Порог коррекции входа
        public void SetV1(float val) { V1 = val; }
        public float GetV1() { return V1; }

        private float V2 = 0.7f;//Мах возраст участия
        public void SetV2(float val) { V2 = val; }
        public float GetV2() { return V2; }

        private float V5 = 0.2f;//Мах активность группы
        public void SetV5(float val) { V5 = val; }
        public float GetV5() { return V5; }

        private int V6 = 5000;//Длительность памяти
        public void SetV6(int val) { V6 = val; }
        public int GetV6() { return V6; }

        List<float> AssessmentFirst = new List<float>();
        List<float> AssessmentSecond = new List<float>();
        int nnn;
        int nnnbuf = 0;
        public int SleepStep = 20;

        public Counter Assessment(int Dispenser, List<float> InputData, float semblance, int IndexData = -1)
        {
            List<float> inter_result = new List<float>();
            List<int> ContractionInputData = new List<int>();
            List<int> ContractionInterResultFirst = new List<int>();
            List<int> ContractionInterResultSecond = new List<int>();
            bool Correct_trigger = true;
            bool Pass = true;
            float ReverseSatiety = 0.0f;
            Counter counter = new Counter();


            nn++;


            if (nn % V6 == 0)
            {
                // Чистка
                Clearing сlearing = new Clearing(ListMatte, ListReverseMatte, satiety);
                //message = сlearing.message;
                сlearing = null;
            }

            float[] inter_Data = InputData.ToArray();
            int InputDataCount = inter_Data.Length;

            //Коррекция сигнала от сенсоров 
            if (InputDataCount > 0 & ListReverseMatte.Count > 0 & AssessmentFirst.Count > 0)
            {
                Correction correction = new Correction(InputDataCount, ListReverseMatte, AssessmentFirst, AssessmentSecond, inter_Data, semblance);
            }
            // Порог минимального значения после коррекции
            for (int i = 0; i < InputDataCount; i++)
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

            counter.inter_Data_.Clear();

            for (int i = InputDataCount - Dispenser; i < InputDataCount; i++)
            {
                if (inter_Data[i] > 1.0f)
                {
                    counter.inter_Data_.Add(1);
                }
                else
                {
                    counter.inter_Data_.Add(0);
                }
            }

            counter.summ = 0;
            counter.summ2 = 0;
            int l = 0;
            for (int i = InputDataCount - Dispenser + 1; i < InputDataCount; i++)
            {
                if (inter_Data[i] > 1.0f)
                {
                    if (l < Dispenser)
                    {
                        counter.summ++;
                    }
                    else
                    {
                        counter.summ2++;
                    }
                }
                l++;
            }
            counter.inter_Data_Full.Clear();
            for (int i = 0; i < InputDataCount; i++)
            {
                if (inter_Data[i] > 1)
                {
                    counter.inter_Data_Full.Add(1);
                }
                else
                {
                    counter.inter_Data_Full.Add(0);
                }
            }

            //Инициализация масок
            if (ListMatte.Count == 0)
            {
                Matte matte = new Matte(InputData, 0, satiety);
                ListMatte.Add(matte);
            }

            //Расчёт активности масок - поиск совпадений
            float Activ;
            float Activ_ = -1;
            int n;
            int Index;

            ActivityMasks activityMasks = new ActivityMasks(ListMatte, ContractionInputData, inter_Data, InputDataCount, Dispenser, inter_result, ContractionInterResultFirst, ContractionInterResultSecond);
            Activ_ = activityMasks.Activ_;
            Index = activityMasks.Index;

            //Обучение масок
            try
            {
                if (Activ_ > ListMatte[Index].appeal)
                {
                    ListMatte[Index].Lesson(InputData);
                    if (Activ_ > ListMatte[Index].appeal & ListMatte[Index].appeal < 0.9f)
                    {
                        ListMatte[Index].appeal = ListMatte[Index].appeal + 0.001f;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Index.ToString());
                MessageBox.Show(ex.ToString());
            }

            if (Activ_ <= satiety)
            {
                Matte matte = new Matte(InputData, (ushort)(ListMatte.Count), satiety);
                ListMatte.Add(matte);
            }
            //Конец обучения масок

            //Расчёт активности результирующих масок отвечающих за зрение
            int Ind;
            AssessmentFirst.Clear();
            AssessmentSecond.Clear();
            ActivityReverseMasks activityReverseMasks = new ActivityReverseMasks(ListReverseMatte, ContractionInterResultFirst, ContractionInterResultSecond, inter_result);
            AssessmentFirst.AddRange(activityReverseMasks.AssessmentFirst);
            AssessmentSecond.AddRange(activityReverseMasks.AssessmentSecond);
            for (int i = 0; i < AssessmentFirst.Count; i++)
            {
                counter.Assessment.Add(AssessmentFirst[i] + AssessmentSecond[i]);
                counter.room.Add(ListReverseMatte[counter.room.Count].room);
            }
            Activ = activityReverseMasks.Activ;
            Ind = activityReverseMasks.Ind;
            //Конец расчёт активности результирующих масок  

            //Подсчет ошибки
            counter.str2 = false;
            if (ListReverseMatte.Count >= Ind & Ind != -1)
            {
                if (ListReverseMatte[Ind].room == IndexData & IndexData != -1)
                {
                    counter.str2 = true;
                }
                counter.Index = ListReverseMatte[Ind].room;
            }

            if (nn / 2000f == Math.Round((double)(nn / 2000)))//Вывод ошибки
            {
                message += "нейр:" + ListMatte.Count.ToString() + " гр:" + ListReverseMatte.Count.ToString() + " ";
                if (ListReverseMatte.Count > 1)
                {
                    message += ListReverseMatte[0].Live.ToString() + "  " + (nnn - nnnbuf) + "\r\n";
                }
                nnnbuf = nnn;
            }
            //Конец подсчёта ошибок	

            // Формирование результирующей маски
            if (ListReverseMatte.Count == 0)
            {
                nnn++;
                try
                {
                    ReverseMatte reverseMatte = new ReverseMatte(InputData, IndexData, inter_result, nnn, ReverseSatiety);
                    ListReverseMatte.Add(reverseMatte);

                }
                catch (Exception ex)
                {
                    //Выводим ошибку
                    MessageBox.Show(ex.ToString());
                }

            }
            else
            {
                bool IndVar = false;
                if (Ind == -1)
                {
                    IndVar = true;
                }
                else
                {
                    if (ListReverseMatte[Ind].appeal_ < 0.15f & ListReverseMatte[Ind].appeal_ > 0.001f & Activ_ < 0.9f)//
                    {
                        IndVar = true;
                    }
                }

                if (AssessmentFirst.Count > 0 & IndVar)//(Activ_ < 0.7f | Ind == -1)
                {
                    for (int i = 0; i < AssessmentFirst.Count; i++)
                    {
                        AssessmentFirst[i] = 0.0f;
                    }
                    ReverseMatte reverseMatte = new ReverseMatte(InputData, IndexData, inter_result, nnn, ReverseSatiety);
                    ListReverseMatte.Add(reverseMatte);
                    Ind = ListReverseMatte.Count - 1;
                    AssessmentFirst.Add(1);
                    ListReverseMatte[Ind].Sleep();
                    Pass = false;
                    nnn++;
                }
            }
            counter.str1 = Activ;
            //Конец формирования результирующей маски

            //Начало обучения результирующей маски
            float RoomValue = -1000;
            int RoomIndex = -1;
            float RoomAppeal = 0.9f;
            int RoomLive = 0;
            if (Ind > -1 & Lesson_trigger & Pass)// Принудительное обучение на индексированных данных
            {
                if (ListReverseMatte[Ind].room != IndexData)
                {
                    for (int i = 0; i < AssessmentFirst.Count; i++)
                    {
                        if (i != Ind & ListReverseMatte[i].room == IndexData & AssessmentFirst[i] >= RoomValue & ListReverseMatte[i].appeal_ <= RoomAppeal & ListReverseMatte[i].Live > RoomLive)//
                        {
                            RoomValue = AssessmentFirst[i];
                            RoomIndex = i;
                            RoomAppeal = ListReverseMatte[i].appeal_;
                            RoomLive = ListReverseMatte[i].Live;
                        }
                    }
                    if (RoomIndex > -1 & RoomValue > -1000)
                    {
                        AssessmentFirst[Ind] = 0;
                        Correct_trigger = false;
                        Ind = RoomIndex;
                        Activ = AssessmentFirst[Ind];
                    }
                }
            }// Конец принудительного обучения

            if (Activ > 1)
            {
                for (int i = 0; i < ListReverseMatte.Count; i++)
                {
                    AssessmentFirst[i] = (float)AssessmentFirst[i] / Activ;
                    if (AssessmentFirst[i] > 1 & ListReverseMatte[i].room != IndexData)
                    {
                        AssessmentFirst[i] = 0.1f;
                    }
                }
            }

            if (Ind > -1 & Pass)
            {
                float appeal;
                Activ = AssessmentFirst[Ind];
                if (ListReverseMatte[Ind].room == IndexData | !Lesson_trigger)//Основной цикл обучения
                {
                    appeal = ListReverseMatte[Ind].appeal_;
                    if (Activ > appeal | Lesson_trigger)//
                    {
                        if (appeal < 0.8f)
                        {
                            ListReverseMatte[Ind].Lesson(InputData, inter_result, 1, Correct_trigger);
                        }
                        else
                        {
                            ListReverseMatte[Ind].Lesson(InputData, inter_result_, 1, Correct_trigger);
                        }
                        if (Activ > appeal & appeal < 0.97f)
                        {
                            ListReverseMatte[Ind].appeal_ += 0.01f;
                        }
                    }
                }//Конец основного цикла обучения
                if (IndexData == ListReverseMatte[Ind].room | IndexData == -1)
                {
                    for (int i = 0; i < ListReverseMatte.Count; i++)//Формирование обстракций
                    {
                        appeal = ListReverseMatte[i].appeal_;
                        if (appeal > 0.3f & appeal < V2)
                        {
                            if (Ind != i & AssessmentFirst[i] > V5 & AssessmentFirst[i] < V2)
                            {
                                ListReverseMatte[i].Lesson(InputData, inter_result, (AssessmentFirst[i] - (V5 + 0.01f)), false);
                                ListReverseMatte[i].Control_value++;
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
                        ListMatte[j].SleepStep = SleepStep;
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
                        if (ListReverseMatte[i].Contraction_ & Pass & ListReverseMatte[i].appeal_ <= 0.2f)
                        {
                            ListReverseMatte[i].Control_value = ListReverseMatte[i].Control_value - 0.01f;
                        }
                    }
                }
            }
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
    }
}
