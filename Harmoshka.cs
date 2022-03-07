using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MNIST
{

    /// Анализ активности и обучение масок.
    [Serializable]
    public class Harmoshka //TODO: сделать синглтоном тоже? 
    {
        public string message = null;
        public List<Matte> ListMatte = new List<Matte>();
        public List<ReverseMatte> ListReverseMatte = new List<ReverseMatte>();
        readonly List<float> inter_result_ = new List<float>();

        //TODO: здесь и ниже переделать в свойства. Done. 
        public int FullError { get; set; } = 60000;//Количество элементов. TODO: Сменить название. Done, но, возможно, нужно будет поменять ещё раз, пока просто исправил орфографию. 

        public bool LessonTrigger { get; set; } = false; //TODO: Переименовать. Done. 

        public float Satiety { get; set; } = 0.1f; //Порог жизни маски

        public float V1 { get; set; } = 0.5f;//Порог коррекции входа. TODO: возможно, сменить название здесь и ниже для прочих V. 

        public float V2 { get; set; } = 0.7f;//Мах возраст участия

        public float V5 { get; set; } = 0.2f;//Мах активность группы

        public int V6 { get; set; } = 5000;//Длительность памяти

        private readonly List<float> AssessmentFirst = new List<float>();
        private readonly List<float> AssessmentSecond = new List<float>();

        private int nn = 0; //TODO: Уточнить, зачем нужны переменные nn, nnn и nnnbuf, и переименовать соответственно. 
        private int nnn;
        private int nnnbuf = 0;
        public int SleepStep = 22;

        //TODO: длинновато, декомпозировать. Done. 
        public Counter Assessment(int Dispenser, List<float> InputData, float semblance, int IndexData = -1)
        {
            //TODO: функция вызывается внутри тройного цикла, возможно, нужно убрать создание списков наружу и переиспользовать заранее созданные. 
            //также TODO: привести нейминг списков к единообразному виду. Done. 
            List<float> interResult = new List<float>();
            List<int> contractionInputData = new List<int>();
            List<int> contractionInterResultFirst = new List<int>();
            List<int> contractionInterResultSecond = new List<int>();
            bool Correct_trigger = true;
            bool Pass = true;
            float ReverseSatiety = 0.0f;
            Counter counter = new Counter();


            nn++;


            if (nn % V6 == 0)
            {
                // Чистка
                _ = new Clearing(ListMatte, ListReverseMatte, Satiety); //TODO: заменить на символ удаления. Done. 
                //message = сlearing.message;
            }

            float[] inter_Data = InputData.ToArray();
            int InputDataCount = inter_Data.Length;

            //Коррекция сигнала от сенсоров 
            if (InputDataCount > 0 & ListReverseMatte.Count > 0 & AssessmentFirst.Count > 0)
            {
                _ = new Correction(InputDataCount, ListReverseMatte, AssessmentFirst, AssessmentSecond, inter_Data, semblance); //TODO: заменить на символ удаления. Done. 
            }
            // Порог минимального значения после коррекции
            ResetByThreshold(contractionInputData, inter_Data, V1);

            counter.inter_Data_.Clear();

            for (int i = InputDataCount - Dispenser; i < InputDataCount; i++)
            {
                //TODO: заменить на тетрарный оператор. Done. 
                counter.inter_Data_.Add(inter_Data[i] > 1.0f ? (byte)1 : (byte)0); //TODO: уточнить, почему сравнивается с единицей. 
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
                        counter.summ++; //TODO: здесь должно быть что-то вроде counter.summ = 2 * Dispencer - InputDataCount - 1, но с учётом inter_Data[i] > 1.0f.
                    }
                    else
                    {
                        counter.summ2++; //TODO: а здесь - что-то вроде counter.summ2 = 2 * (InputDataCount - Dispencer) + 1 с учётом того же условия. 
                    }
                }
                l++;
            }
            counter.inter_Data_Full.Clear();
            for (int i = 0; i < InputDataCount; i++)
            {
                //TODO: заменить на тетрарный оператор. Done. 
                counter.inter_Data_Full.Add(inter_Data[i] > 1 ? (byte)1 : (byte)0);
            }

            //Инициализация масок
            if (ListMatte.Count == 0)
            {
                Matte matte = new Matte(InputData, 0, Satiety);
                ListMatte.Add(matte);
            }

            ActivityMasks activityMasks = new ActivityMasks(ListMatte, contractionInputData, inter_Data, InputDataCount, Dispenser, interResult, contractionInterResultFirst, contractionInterResultSecond);
            float Activ_ = activityMasks.Activ_;
            //int n;
            int Index = activityMasks.Index;

            //Обучение масок
            TeachMatte(InputData, Activ_, Index, ListMatte);
            //Конец обучения масок

            //Расчёт активности результирующих масок отвечающих за зрение
            ActivityReverseMasks activityReverseMasks = CalculateReverseMasksActivity(interResult, contractionInterResultFirst,
                                                                                      contractionInterResultSecond, counter,
                                                                                      AssessmentFirst, AssessmentSecond);
            //Конец расчёт активности результирующих масок  

            //Подсчет ошибки
            counter.str2 = false; //TODO: уточнить, что за str2. 

            int Ind = activityReverseMasks.Ind;
            if (ListReverseMatte.Count >= Ind & Ind != -1)
            {
                if (ListReverseMatte[Ind].room == IndexData & IndexData != -1)
                {
                    counter.str2 = true;
                }
                counter.Index = ListReverseMatte[Ind].room;
            }

            //TODO: заменить на nn % 2000 == 0? 0_o
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
                CreateReverseMatte(InputData, IndexData, interResult, ReverseSatiety, nnn);
            }
            else
            {
                //TODO: свернуть условия if'ов в саму булеву переменную. Done. 
                bool IndVar = (Ind == -1) ||
                    (ListReverseMatte[Ind].appeal_ < 0.15f & ListReverseMatte[Ind].appeal_ > 0.001f & Activ_ < 0.9f);

                if (AssessmentFirst.Count > 0 & IndVar)//(Activ_ < 0.7f | Ind == -1)
                {
                    for (int i = 0; i < AssessmentFirst.Count; i++)
                    {
                        AssessmentFirst[i] = 0;//.001f
                    }
                    CreateReverseMatte(InputData, IndexData, interResult, ReverseSatiety, nnn);
                    Ind = ListReverseMatte.Count - 1;
                    AssessmentFirst.Add(1);
                    //ListReverseMatte[Ind].Sleep();
                    Pass = false;
                    nnn++;
                }
            }
            //Конец формирования результирующей маски

            //Начало обучения результирующей маски
            TeachReverseMatte(InputData, IndexData, interResult, ref Correct_trigger, Pass, counter, activityReverseMasks, ref Ind);
            //Конец обучения результирующей маски

            //Начало фиксации обучения
            if (nn % 10 == 0)
            {
                FixLesson(Pass, SleepStep, ListMatte, ListReverseMatte);
            }
            return counter;
        }

        private void TeachReverseMatte(List<float> InputData, //Эта функция извлечена чисто механически и, возможно, сама по себе требует рефакторинга. 
                                       int IndexData,
                                       List<float> interResult,
                                       ref bool Correct_trigger,
                                       bool Pass,
                                       Counter counter,
                                       ActivityReverseMasks activityReverseMasks,
                                       ref int Ind)
        {
            float Activ = activityReverseMasks.Activ;
            counter.str1 = Activ;
            float RoomValue = -1000;
            int RoomIndex = -1;
            float RoomAppeal = 0.9f;
            int RoomLive = 0;
            if (Ind > -1 & LessonTrigger & Pass)// Принудительное обучение на индексированных данных
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
                    //if (AssessmentFirst[i] > 1 & ListReverseMatte[i].room != IndexData)
                    //{
                    //    AssessmentFirst[i] = 0.1f;
                    //}
                }
            }

            if (Ind > -1 & Pass)
            {
                float appeal;
                Activ = AssessmentFirst[Ind];
                if (ListReverseMatte[Ind].room == IndexData | !LessonTrigger)//Основной цикл обучения
                {
                    appeal = ListReverseMatte[Ind].appeal_;
                    if (Activ > appeal | LessonTrigger)//
                    {
                        if (appeal < 0.8f)
                        {
                            ListReverseMatte[Ind].Lesson(InputData, interResult, 1, Correct_trigger);
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
                                ListReverseMatte[i].Lesson(InputData, interResult, AssessmentFirst[i] - (V5 + 0.01f), false);
                                ListReverseMatte[i].Control_value++;
                                ListReverseMatte[i].appeal_ += 0.001f;
                            }
                        }
                    }
                }//Конец формирования абстракций
            }
        }

        private void FixLesson(bool pass, int sleepStep, List<Matte> matteList, List<ReverseMatte> reverseMatteList)
        {
            for (int j = 0; j < matteList.Count; j++)
            {
                if (matteList[j].Contraction)
                {
                    matteList[j].Sleep();
                    matteList[j].SleepStep = sleepStep;
                }
                else
                {
                    if (matteList[j].Contraction_)
                    {
                        matteList[j].Control_value--;
                    }
                }
            }
            for (int i = 0; i < reverseMatteList.Count; i++)
            {
                if (reverseMatteList[i].Contraction)
                {
                    reverseMatteList[i].Sleep();
                }
                else
                {
                    if (reverseMatteList[i].Contraction_ & pass & reverseMatteList[i].appeal_ <= 0.2f)
                    {
                        reverseMatteList[i].Control_value -= 0.01f; //TODO: заменить на -=. Done. 
                    }
                }
            }
        }

        private void CreateReverseMatte(List<float> InputData, int IndexData, List<float> interResult, float ReverseSatiety, int nnn)
        {
            try
            {
                ReverseMatte reverseMatte = new ReverseMatte(InputData, IndexData, interResult, nnn, ReverseSatiety);
                ListReverseMatte.Add(reverseMatte);

            }
            //Все знают, что ловить базовый класс Exception нехорошо, но все так делают. TODO: если будет время, переделать.
            catch (Exception ex) 
            {
                //Выводим ошибку
                MessageBox.Show(ex.ToString());
            }
        }

        private ActivityReverseMasks CalculateReverseMasksActivity(List<float> interResult,
                                                                   List<int> contractionInterResultFirst,
                                                                   List<int> contractionInterResultSecond,
                                                                   Counter counter,
                                                                   List<float> assessmentFirst,
                                                                   List<float> assessmentSecond)
        {
            assessmentFirst.Clear();
            assessmentSecond.Clear();
            ActivityReverseMasks activityReverseMasks = new ActivityReverseMasks(ListReverseMatte, contractionInterResultFirst, contractionInterResultSecond, interResult);
            assessmentFirst.AddRange(activityReverseMasks.AssessmentFirst);
            assessmentSecond.AddRange(activityReverseMasks.AssessmentSecond);
            for (int i = 0; i < assessmentFirst.Count; i++)
            {
                counter.Assessment.Add(assessmentFirst[i] + assessmentSecond[i]);
                counter.room.Add(ListReverseMatte[i].room);
            }

            return activityReverseMasks;
        }

        private void TeachMatte(List<float> InputData, float Activ_, int Index, List<Matte> matteList)
        {
            try
            {
                if (Activ_ > matteList[Index].appeal)
                {
                    matteList[Index].Lesson(InputData);
                    if (matteList[Index].appeal < 0.7f) //TODO: часть условия повторяется с внешним if, можно упростить. Done. 
                    {
                        matteList[Index].appeal += 0.001f; //TODO: заменить на +=. Done. 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Index.ToString());
                MessageBox.Show(ex.ToString());
            }

            if (Activ_ <= Satiety)
            {
                Matte matte = new Matte(InputData, (ushort)(matteList.Count), Satiety);
                matteList.Add(matte);
            }
        }

        private void ResetByThreshold(List<int> contractionInputData, float[] array, float limit)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] <= limit)
                {
                    array[i] = 0;
                }
                else
                {
                    contractionInputData.Add(i);//Для ускорения расчётов
                }
            }
        }

        // Сохранение масок
        //TODO: какой-то косяк с сохранением, не забыть посмотреть. Вроде бы здесь всё хорошо, нужно искать дальше. 
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
        public void LoadMatte(string path)//TODO: исправить название. Done.
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
