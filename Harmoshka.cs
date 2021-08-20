using System;
using System.Collections.Generic;
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
        List<float> AssessmentFirst = new List<float>();
        List<float> AssessmentSecond = new List<float>();


        public Counter Assessment(int Dispenser, List<float> InputData, float semblance, int IndexData = -1)
        {
            List<float> inter_result = new List<float>();
            List<int> ContractionInputData = new List<int>();
            List<int> ContractionInterResultFirst = new List<int>();
            List<int> ContractionInterResultSecond = new List<int>();
            bool Correct_trigger = true;
            bool Pass = true;
            float ReverseSatiety = 0.0f;
            int fulEror100 = (int)(fulEror / 100f);
            Counter counter = new Counter();
            float AssessmentFirst_;
            float AssessmentSecond_;
            float appeal_;
            float inter_Data_;

            nn++;

            // Чистка
            if (nn % V6 == 0)
            {
                List<int> Empty = new List<int>();
                for (int j = 0; j < ListMatte.Count; j++)
                {
                    // message += "app " + ListMatte[j].appeal.ToString() + " C_v " + ListMatte[j].Control_value.ToString() + "\r\n";
                    if ((ListMatte[j].appeal == satiety & ListMatte[j].Control_value <= 0) | (ListMatte[j].Control_value <= -30) | (ListMatte[j].appeal == satiety + 0.05f & ListMatte[j].Control_value < 200))//&  ListMatte[j].appeal < 0.31f
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
                    //message += "Rapp " + ListReverseMatte[j].appeal_.ToString() + " R C_v " + ListReverseMatte[j].Control_value.ToString() + "\r\n";
                    if ((ListReverseMatte[j].appeal_ <= 0.0f & ListReverseMatte[j].Control_value <= 0f) | ListReverseMatte[j].Control_value <= -0.2f | (ListReverseMatte[j].appeal_ < 0.011f & ListReverseMatte[j].Control_value < 90f))
                    {
                        ListReverseMatte.RemoveAt(j);
                        //AssessmentSecond.RemoveAt(j);
                        //AssessmentFirst.RemoveAt(j);
                    }
                    else
                    {
                        if (ListReverseMatte[j].Correct.Count < ListMatte.Count * 0.8f & ListReverseMatte[j].appeal_ <= 0.0f)
                        {
                            ListReverseMatte[j].Control_value = ListReverseMatte[j].Control_value - 0.1f;
                        }
                        if (ListReverseMatte[j].Correct.Count < ListMatte.Count * 0.95f & ListReverseMatte[j].appeal_ > 0.01f)//
                        {
                            ListReverseMatte[j].appeal_ = ListReverseMatte[j].appeal_ - 0.0005f;
                        }
                    }
                }
            }//Конец чистки

            inter_Data.Clear();
            inter_Data = InputData.GetRange(0, InputData.Count);

            //Коррекция сигнала от сенсоров (новый вариант)
            if (inter_Data.Count > 0 & ListReverseMatte.Count > 0 & AssessmentFirst.Count > 0)
            {

                if (AssessmentSecond.Count < AssessmentFirst.Count)
                {
                    for (int i = AssessmentSecond.Count; i < AssessmentFirst.Count; i++)
                    {
                        AssessmentSecond.Add(0);
                    }
                }

                for (int j = 0; j < ListReverseMatte.Count; j++)
                {
                    AssessmentFirst_ = AssessmentFirst[j];
                    AssessmentSecond_ = AssessmentSecond[j];
                    if (ListReverseMatte[j].Control_value > 0f)
                    {
                        appeal_ = ListReverseMatte[j].appeal_;
                        for (int i = 0; i < inter_Data.Count; i++)
                        {
                            inter_Data_ = InputData[i] + ListReverseMatte[j].matte[i] * AssessmentFirst_ * appeal_ * semblance;//* 10f
                            inter_Data_ += ListReverseMatte[j].matte[i] * AssessmentSecond_ * appeal_ * semblance;//* 10f* 20
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
                if (inter_Data[i] <= V1 | inter_Data[i] > 1 + V1)
                {
                    inter_Data[i] = 0;
                }
                else
                {
                    ContractionInputData.Add(i);//Для ускорения расчётов
                }
            }

            counter.inter_Data_.Clear();

            for (int i = inter_Data.Count - 28; i < inter_Data.Count; i++)
            {
                if (inter_Data[i] > 1)//& inter_Data[i] < 2.5f
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
            for (int i = inter_Data.Count - 29; i < inter_Data.Count; i++)
            {
                if (inter_Data[i] > 1.0f)// & inter_Data[i] < 2.5f
                {
                    if (l < 14)
                    {
                        counter.summ++;
                    }
                    else
                    {
                        counter.summ2++;
                    }
                    //break;
                }
                l++;
            }
            counter.inter_Data_Full.Clear();
            for (int i = 0; i < inter_Data.Count; i++)
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

            AssessmentFirst.Clear();
            AssessmentSecond.Clear();

            //Инициализация масок
            if (ListMatte.Count == 0)
            {
                Matte matte = new Matte(InputData, 0, satiety);
                ListMatte.Add(matte);
            }

            //Расчёт активности масок - поиск совпадений
            float Activ = 0;
            float ActivSecond = 0;
            float Activ_ = -1;
            int n;
            int Index = 0;

            for (int i = 0; i < ListMatte.Count; i++)
            {
                Activ = 0;
                ActivSecond = 0;
                if (ListMatte[i].appeal == 0.3f & ListMatte[i].Control_value <= 0) { }
                else
                {
                    for (int j = 0; j < ContractionInputData.Count; j++)
                    {
                        n = ContractionInputData[j];
                        if (n <= inter_Data.Count - (Dispenser ))//+ 14 * 14
                        {
                            Activ += ListMatte[i].matte[n] * inter_Data[n];
                        }
                        else
                        {
                            ActivSecond += ListMatte[i].matte[n] * inter_Data[n];
                        }
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
                    ContractionInterResultFirst.Add(inter_result.Count - 1);
                }
                else
                {
                    if (ActivSecond > -0.0f)
                    {
                        inter_result.Add(ActivSecond);
                        //ContractionInterResultFirst.Add(inter_result.Count - 1);
                        ContractionInterResultSecond.Add(inter_result.Count - 1);
                    }
                    else
                    {
                        inter_result.Add(0);
                    }
                }


            } //Конец расчёта активности масок 

            //Обучение масок
            if (Activ_ > ListMatte[Index].appeal - 0.1f)//0.1
            {
                ListMatte[Index].Lesson(InputData);
                if (Activ_ > ListMatte[Index].appeal & ListMatte[Index].appeal < 0.9f)
                {
                    ListMatte[Index].appeal = ListMatte[Index].appeal + 0.001f;
                }
            }

            if (Activ_ < satiety)//+ 0.1f =Activ_ > 0.0001f & - 0.2f
            {
                Matte matte = new Matte(InputData, (ushort)(ListMatte.Count), satiety);
                ListMatte.Add(matte);
            }
            //Конец обучения масок

            //Расчёт активности результирующих масок отвечающих за зрение
            var Ind = -1;
            Activ = -1;
            for (int i = 0; i < ListReverseMatte.Count; i++)
            {
                if (AssessmentFirst.Count != ListReverseMatte.Count)
                {
                    AssessmentFirst.Add(0);
                }
                if (ListReverseMatte[i].appeal_ <= 0.0f & ListReverseMatte[i].Control_value <= 0f) { }
                else
                {
                    for (int j = 0; j < ContractionInterResultFirst.Count; j++)
                    {
                        n = ContractionInterResultFirst[j];
                        if (ListReverseMatte[i].Correct.Count > n)
                        {
                            AssessmentFirst[i] = AssessmentFirst[i] + inter_result[n] * ListReverseMatte[i].Correct[n];
                        }
                    }
                }

                if (Activ < AssessmentFirst[i] & AssessmentFirst[i] > 0)
                {
                    Activ = AssessmentFirst[i];
                    Ind = i;
                }
            }

            //Расчёт активности результирующих масок отвечающих за движение
            float SecondActiv = 0;
            for (int i = 0; i < ListReverseMatte.Count; i++)
            {
                if (AssessmentSecond.Count < ListReverseMatte.Count)
                {
                    AssessmentSecond.Add(0);
                }
                if (ListReverseMatte[i].appeal_ <= 0.0f & ListReverseMatte[i].Control_value <= 0f) { }
                else
                {
                    for (int j = 0; j < ContractionInterResultSecond.Count; j++)
                    {
                        n = ContractionInterResultSecond[j];
                        if (ListReverseMatte[i].Correct.Count > n)
                        {
                            AssessmentSecond[i] = (AssessmentSecond[i] + inter_result[n] * ListReverseMatte[i].Correct[n]);

                        }
                    }
                }
                if (AssessmentSecond[i] > SecondActiv)
                {
                    SecondActiv = AssessmentSecond[i];
                }
            }
            if (SecondActiv > 1)//!= 0
            {
                for (int i = 0; i < ListReverseMatte.Count; i++)
                {
                    AssessmentSecond[i] = (float)AssessmentSecond[i] / SecondActiv;
                }
            }
            //Конец расчёт активности результирующих масок  

            //Подсчет ошибки
            counter.str2 = false;
            if (ListReverseMatte.Count >= Ind & Ind != -1)
            {
                if (ListReverseMatte[Ind].room == IndexData & IndexData != -1)
                {
                    counter.str2 = true;
                }
            }

            if (nn % 1000f == 0)//Вывод ошибки
            {
                message += " нейр:" + ListMatte.Count.ToString() + " гр:" + ListReverseMatte.Count.ToString() + "\r\n";
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
                    if (Activ_ > 0.7f)//0.5
                    {
                        for (int i = 0; i < AssessmentFirst.Count; i++)
                        {
                            AssessmentFirst[i] = 0;
                        }
                        ReverseMatte reverseMatte = new ReverseMatte(InputData, IndexData, inter_result, ReverseSatiety);
                        ListReverseMatte.Add(reverseMatte);
                        Ind = ListReverseMatte.Count - 1;
                        AssessmentFirst.Add(1);
                        ListReverseMatte[Ind].Sleep();
                        Pass = false;
                    }
                }
                else
                {
                    if (ListReverseMatte[Ind].appeal_ > V2 & Activ < 1f - ListReverseMatte[Ind].appeal_)
                    {
                        for (int i = 0; i < AssessmentFirst.Count; i++)
                        {
                            AssessmentFirst[i] = 0;
                        }
                        ReverseMatte reverseMatte = new ReverseMatte(InputData, ListReverseMatte[Ind].room, inter_result, ReverseSatiety);
                        ListReverseMatte.Add(reverseMatte);
                        Ind = ListReverseMatte.Count - 1;
                        AssessmentFirst.Add(1);
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
                    for (int i = 0; i < AssessmentFirst.Count; i++)
                    {
                        if (i != Ind & ListReverseMatte[i].room == IndexData & AssessmentFirst[i] > RoomValue)
                        {
                            RoomValue = AssessmentFirst[i];
                            RoomIndex = i;
                        }
                    }
                    if (RoomIndex > -1 & RoomValue > -1000)
                    {
                        AssessmentFirst[Ind] = 0;
                        Correct_trigger = false;
                        Ind = RoomIndex;
                    }
                }
            }// Конец принудительного обучения

            if (Activ > 1)
            {
                for (int i = 0; i < ListReverseMatte.Count; i++)
                {
                    AssessmentFirst[i] = (float)AssessmentFirst[i] / Activ;
                }
            }

            if (Ind > -1 & Pass)
            {
                float appeal = 0;
                Activ = AssessmentFirst[Ind];
                if (ListReverseMatte[Ind].room == IndexData | !Lesson_trigger)//Основной цикл обучения
                {
                    appeal = ListReverseMatte[Ind].appeal_;
                    if (Activ > appeal | Lesson_trigger)
                    {
                        ListReverseMatte[Ind].Lesson(InputData, inter_result, 1, Correct_trigger);
                        if (Activ > appeal & appeal < 0.95f)
                        {
                            ListReverseMatte[Ind].appeal_ += 0.01f;
                        }
                        //if (RoomIndex > -1)
                        //{
                        //    ListReverseMatte[Ind].Lesson(InputData, inter_result, 1, Correct_trigger);
                        //    if (Activ > appeal & appeal < 0.95f)
                        //    {
                        //        ListReverseMatte[Ind].appeal_ += 0.01f;
                        //    }
                        //}

                    }
                }//Конец основного цикла обучения

                for (int i = 0; i < ListReverseMatte.Count; i++)//Формирование обстракций
                {
                    appeal = ListReverseMatte[i].appeal_;
                    if (appeal > V4 & appeal < V2)
                    {
                        if (Ind != i & AssessmentFirst[i] > V5 & AssessmentFirst[i] < V2)
                        {
                            ListReverseMatte[i].Lesson(InputData, inter_result, (AssessmentFirst[i] - (V5 + 0.01f)), false);
                            ListReverseMatte[i].Control_value++;
                            if (appeal < 0.90f)
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
                        if (ListReverseMatte[i].Contraction_ & Pass & ListReverseMatte[i].appeal_ <= 0.0f)
                        {
                            ListReverseMatte[i].Control_value = ListReverseMatte[i].Control_value - 0.1f;
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
