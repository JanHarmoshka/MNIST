using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MNIST
{

    /// Анализ активности и обучение масок.
    [Serializable]
    public class Harmoshka //TODO: сделать синглтоном тоже? Done. 
    {
        private static Harmoshka instance = null;
        public static Harmoshka Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Harmoshka();
                }
                return instance;
            }
        }

        private Harmoshka() { }

        public string Message = null;
        public List<Matte> Mattes = new List<Matte>();
        public List<ReverseMatte> ReverseMattes = new List<ReverseMatte>();
        readonly List<float> inter_result_ = new List<float>(); //TODO: уточнить, что это именно, и переименовать соответствующе. 


        public int ErrorCount { get; set; } = 60000;//Количество элементов. 

        public bool LessonTrigger { get; set; } = false;

        public float Satiety { get; set; } = 0.15f; //Порог жизни маски

        public float CorrectionThreshold { get; set; } = 0.5f; 

        public float V2 { get; set; } = 0.7f;//Мах возраст участия

        public float V5 { get; set; } = 0.2f;//Мах активность группы

        public int MemoryDuration { get; set; } = 5000;//Длительность памяти

        public int SleepStep = 22;
        private int Matteselect = 0;
        private int reverseMatteselect = 0;

        private readonly List<float> firstAssessment = new List<float>();
        private readonly List<float> secondAssessment = new List<float>();

        private int assessmentCounter = 0;
        private int reverseMatteCounter;

        private List<float> interResult = new List<float>();
        private List<int> contractionInputData = new List<int>();
        private List<int> firstContractionInterResult = new List<int>();
        private List<int> secondContractionInterResult = new List<int>();

        private float Reverse_appeal_max = 0.65f;
        private float Reverse_Control_value_max = 43.0f;
        private float Mattes_Activ_max = 1.01f;
        private float Mattes_appeal_max = 0.7009f;
        private int Mattes_Control_value_max = 243;

        private float Reverse_appeal_max_ = 0;
        private float Reverse_Control_value_max_ = 0;
        private float Mattes_Activ_max_ = 0;
        private float Mattes_appeal_max_ = 0;
        private int Mattes_Control_value_max_ = 0;

        private float Reverse_appeal_max__ = 0;
        private float Reverse_Control_value_max__ = 0;
        private float Mattes_Activ_max__ = 0;
        private float Mattes_appeal_max__ = 0;
        private int Mattes_Control_value_max__ = 0;

        private InternalActivityMasksArgs internalActivityMasksArgs = new InternalActivityMasksArgs()
        {
            interResults = new List<List<float>>()
            { new List<float>(1000), new List<float>(1000), new List<float>(1000), new List<float>(1000) },
            firstContractionInterResults = new List<List<int>>()
            { new List<int>(1000), new List<int>(1000), new List<int>(1000), new List<int>(1000) },
            secondContractionInterResults = new List<List<int>>()
            { new List<int>(1000), new List<int>(1000), new List<int>(1000), new List<int>(1000) }
        };
        public Counter Assessment(int dispenser, List<float> inputData, float semblance, int indexData = -1)
        {
            interResult.Clear();
            contractionInputData.Clear();
            firstContractionInterResult.Clear();
            secondContractionInterResult.Clear();
            bool correctTrigger = true;
            bool pass = true;
            float reverseSatiety = 0.0f;
            Counter counter = new Counter();
            List<int> indexData_ = new List<int>();


            assessmentCounter++;


            if (assessmentCounter % MemoryDuration == 0)
            {

                _ = new Clearing(Mattes, ReverseMattes, Satiety, Matteselect);
            }

            float[] interData = inputData.ToArray();
            int inputDataCount = interData.Length;

            //Коррекция входящего сигнала 
            if (inputDataCount > 0 && ReverseMattes.Count > 0 && firstAssessment.Count > 0)
            {

                _ = new Correction(inputDataCount, ReverseMattes, firstAssessment, secondAssessment, interData, semblance, indexData_);
                indexData = indexData_[0];
            }
            // Порог минимального значения после коррекции
            ResetByThreshold(contractionInputData, interData, CorrectionThreshold);

            counter.inter_Data_.Clear();

            for (int i = inputDataCount - dispenser; i < inputDataCount; i++)
            {
                counter.inter_Data_.Add(interData[i] > 1.0f ? (byte)1 : (byte)0);
            }

            counter.summ = 0;
            counter.summ2 = 0;
            int l = 0;
            for (int i = inputDataCount - dispenser + 1; i < inputDataCount; i++)
            {
                if (interData[i] > 1.0f) // сведения о наличии активности зрительных и моторных нейронов по отдельности
                {
                    if (l < dispenser)
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
            for (int i = 0; i < inputDataCount; i++)// сведения о активности зрительных и моторных нейронов по отдельности
            {
                counter.inter_Data_Full.Add(interData[i] > 1 ? (byte)1 : (byte)0);
            }

            //Инициализация масок
            if (Mattes.Count == 0)
            {
                Matte matte = new Matte(inputData, 0, Satiety);
                Mattes.Add(matte);
            }

            var activityMasksArgs = new ActivityMasksArgs()
            {
                contractionInputData = contractionInputData,
                firstContractionInterResult = firstContractionInterResult,
                interData = interData,
                interResult = interResult,
                mattes = Mattes,
                secondContractionInterResult = secondContractionInterResult
            };
            ActivityMasks activityMasks = new ActivityMasks(activityMasksArgs, internalActivityMasksArgs, inputDataCount, dispenser);
            float Activ_ = activityMasks.Activ_;

            int Index = activityMasks.Index;

            TeachMatte(inputData, Activ_, Index, Mattes);//Обучение масок            

            //Расчёт активности результирующих масок отвечающих за зрение
            ActivityReverseMasks activityReverseMasks = CalculateReverseMasksActivity(interResult, firstContractionInterResult,
                                                                                      secondContractionInterResult, counter,
                                                                                      firstAssessment, secondAssessment);

            //Подсчет ошибки
            counter.str2 = false; //TODO: уточнить, что за str2. 

            int maxActivityIndex = activityReverseMasks.Ind;
            if (ReverseMattes.Count >= maxActivityIndex && maxActivityIndex != -1)
            {
                if (ReverseMattes[maxActivityIndex].room == indexData && indexData != -1)
                {
                    counter.str2 = true;
                }
                counter.Index = ReverseMattes[maxActivityIndex].room;
            }

            if (assessmentCounter % 4000 == 0)//Вывод ошибки
            {
                Message += "нейр:" + Mattes.Count.ToString() + "/" + Matteselect.ToString() + " гр:" + ReverseMattes.Count.ToString() + "/" + reverseMatteselect.ToString() + "\r\n";
                if (Reverse_appeal_max_ > 0 && Mattes_appeal_max_ > 0)
                {
                    if (Reverse_appeal_max__ == Reverse_appeal_max_ && Reverse_Control_value_max__ == Reverse_Control_value_max_)
                    {
                        Reverse_appeal_max = Reverse_appeal_max_ - 0.001f;
                    }
                    if (Mattes_Activ_max__ == Mattes_Activ_max_ && Mattes_appeal_max__ == Mattes_appeal_max_ &&
                        Mattes_Control_value_max__ == Mattes_Control_value_max_ && reverseMatteselect > 10)
                    {
                        Mattes_Activ_max = Mattes_Activ_max_;
                        Mattes_appeal_max = Mattes_appeal_max_ - 0.01f;
                    }
                    Reverse_appeal_max__ = Reverse_appeal_max_;
                    Reverse_Control_value_max__ = Reverse_Control_value_max_;
                    Mattes_Activ_max__ = Mattes_Activ_max_;
                    Mattes_appeal_max__ = Mattes_appeal_max_;
                    Mattes_Control_value_max__ = Mattes_Control_value_max_;
                }

            }
            //Конец подсчёта ошибок	

            // Формирование результирующей маски
            if (ReverseMattes.Count == 0)
            {
                reverseMatteCounter++;
                CreateReverseMatte(inputData, indexData, interResult, reverseSatiety, reverseMatteCounter, false);
            }
            else
            {
                bool IndVarMin = (maxActivityIndex == -1) ||
                    (ReverseMattes[maxActivityIndex].appeal_ < 0.2f && ReverseMattes[maxActivityIndex].appeal_ > 0.001f && Activ_ < 0.9f);
                bool IndVarMax = (maxActivityIndex == -1) ||
                    (ReverseMattes[maxActivityIndex].appeal_ < 0.95f && ReverseMattes[maxActivityIndex].appeal_ > 0.1f && Activ_ < 0.9f);
                bool IndVarActiv = false;
                if (ReverseMattes.Count > maxActivityIndex && maxActivityIndex > -1)
                {
                    if (ReverseMattes[maxActivityIndex].appeal_ >= Reverse_appeal_max_)
                    {
                        if (ReverseMattes[maxActivityIndex].Control_value < 100)
                        {
                            Reverse_Control_value_max_ = ReverseMattes[maxActivityIndex].Control_value;
                        }
                        Reverse_appeal_max_ = ReverseMattes[maxActivityIndex].appeal_;

                    }

                    IndVarActiv = (ReverseMattes[maxActivityIndex].appeal_ >= Reverse_appeal_max && ReverseMattes[maxActivityIndex].Control_value >= Reverse_Control_value_max && !ReverseMattes[maxActivityIndex].elect);
                    if (IndVarActiv)
                    {
                        reverseMatteselect++;
                    }

                }

                if (firstAssessment.Count > 0 && (IndVarMin || IndVarMax || IndVarActiv))
                {
                    for (int i = 0; i < firstAssessment.Count; i++)
                    {
                        firstAssessment[i] = 0;
                    }
                    CreateReverseMatte(inputData, indexData, interResult, reverseSatiety, reverseMatteCounter, IndVarActiv);
                    maxActivityIndex = ReverseMattes.Count - 1;
                    firstAssessment.Add(1);
                    pass = false;
                    reverseMatteCounter++;
                }
            }
            //Конец формирования результирующей маски

            //Начало обучения результирующей маски
            if (indexData_.Count == 1)
            {
                TeachReverseMatte(inputData, indexData, interResult, ref correctTrigger, pass, counter, activityReverseMasks, ref maxActivityIndex);
            }
            else
            {
                for (int i = 0; i < indexData_.Count; i++)
                {
                    indexData = indexData_[i];
                    TeachReverseMatte(inputData, indexData, interResult, ref correctTrigger, pass, counter, activityReverseMasks, ref maxActivityIndex);
                }
            }
            //Конец обучения результирующей маски

            //Фиксация обучения
            if (assessmentCounter % 10 == 0)
            {
                FixLesson(pass, SleepStep, Mattes, ReverseMattes);
            }
            return counter;
        }

        private void TeachReverseMatte(List<float> inputData,
                                       int dataIndex,
                                       List<float> interResult,
                                       ref bool correctTrigger,
                                       bool pass,
                                       Counter counter,
                                       ActivityReverseMasks activityReverseMasks,
                                       ref int index)
        {
            float activ = activityReverseMasks.Activ;
            counter.str1 = activ;
            float roomValue = -1000;
            int roomIndex = -1;
            float roomAppeal = 0.9f;
            int roomLive = 0;
            if (index > -1 && LessonTrigger && pass)// Принудительное исправление активного индекса
            {
                if (ReverseMattes[index].room != dataIndex && firstAssessment[index] > 0)
                {
                    for (int i = 0; i < firstAssessment.Count; i++)
                    {
                        if (i != index && firstAssessment[i] >= roomValue && ReverseMattes[i].appeal_ <= roomAppeal && ReverseMattes[i].Live > roomLive && ReverseMattes[i].room == dataIndex)
                        {
                            roomValue = firstAssessment[i];
                            roomIndex = i;
                            roomAppeal = ReverseMattes[i].appeal_;
                            roomLive = ReverseMattes[i].Live;
                        }
                    }
                    if (roomIndex > -1 && roomValue > -1000)
                    {
                        firstAssessment[index] = 0;
                        correctTrigger = false;
                        index = roomIndex;
                        activ = firstAssessment[index];
                    }
                }
            }// Конец принудительного исправления индекса

            if (activ > 1)
            {
                for (int i = 0; i < ReverseMattes.Count; i++)
                {
                    firstAssessment[i] = (float)firstAssessment[i] / activ;// Приведение значения активности результирующих масок к еденице
                }
            }

            if (index > -1 && pass)
            {
                float appeal;
                activ = firstAssessment[index];
                if (ReverseMattes[index].room == dataIndex || !LessonTrigger)//Основной цикл обучения 
                {
                    appeal = ReverseMattes[index].appeal_;
                    if ((activ > appeal || LessonTrigger) && !ReverseMattes[index].elect)
                    {
                        if (appeal < 0.8f)
                        {
                            ReverseMattes[index].Lesson(inputData, interResult, 1, correctTrigger);
                        }
                        else
                        {
                            ReverseMattes[index].Lesson(inputData, inter_result_, 1, correctTrigger);
                        }
                        if (activ > appeal && appeal < 0.97f)
                        {
                            ReverseMattes[index].appeal_ += 0.01f;
                        }
                    }
                }//Конец основного цикла обучения
                if (dataIndex == ReverseMattes[index].room || dataIndex == -1)//Формирование обстракций
                {
                    for (int i = 0; i < ReverseMattes.Count; i++)
                    {
                        appeal = ReverseMattes[i].appeal_;
                        if (appeal > 0.3f && appeal < V2)
                        {
                            if (index != i && firstAssessment[i] > V5 && firstAssessment[i] < V2)
                            {
                                ReverseMattes[i].Lesson(inputData, interResult, firstAssessment[i] - (V5 + 0.01f), false);
                                ReverseMattes[i].Control_value++;
                                ReverseMattes[i].appeal_ += 0.001f;
                            }
                        }
                    }
                }//Конец формирования абстракций
            }
        }

        private void FixLesson(bool pass, int sleepStep, List<Matte> mattes, List<ReverseMatte> reverseMattes)
        {
            for (int j = 0; j < mattes.Count; j++)
            {
                if (mattes[j].Contraction)
                {
                    mattes[j].Sleep();
                    mattes[j].SleepStep = sleepStep;
                }
                else
                {
                    if (mattes[j].Contraction_ && !mattes[j].elect)
                    {
                        mattes[j].Control_value--;
                    }
                }
            }
            for (int i = 0; i < reverseMattes.Count; i++)
            {
                if (reverseMattes[i].Contraction)
                {
                    reverseMattes[i].Sleep();
                }
                else
                {
                    if (reverseMattes[i].Contraction_ && pass && reverseMattes[i].appeal_ <= 0.2f && !reverseMattes[i].elect)
                    {
                        reverseMattes[i].Control_value -= 0.01f;  
                    }
                }
            }
        }

        private void CreateReverseMatte(List<float> inputData, int dataIndex, List<float> interResult, float reverseSatiety, int reverseMatteCounter, bool IndVarActiv)
        {
                ReverseMatte reverseMatte = new ReverseMatte(inputData, dataIndex, interResult, reverseMatteCounter, reverseSatiety, IndVarActiv);
                ReverseMattes.Add(reverseMatte);
        }

        private ActivityReverseMasks CalculateReverseMasksActivity(List<float> interResult,
                                                                   List<int> firstContractionInterResult,
                                                                   List<int> secondContractionInterResult,
                                                                   Counter counter,
                                                                   List<float> firstAssessment,
                                                                   List<float> secondAssessment)
        {
            firstAssessment.Clear();
            secondAssessment.Clear();
            ActivityReverseMasks activityReverseMasks = new ActivityReverseMasks(ReverseMattes, firstContractionInterResult, secondContractionInterResult, interResult);
            firstAssessment.AddRange(activityReverseMasks.FirstAssessment);
            secondAssessment.AddRange(activityReverseMasks.SecondAssessment);
            for (int i = 0; i < firstAssessment.Count; i++)
            {
                counter.Assessment.Add(firstAssessment[i] + secondAssessment[i]);
                counter.room.Add(ReverseMattes[i].room);
            }

            return activityReverseMasks;
        }

        private void TeachMatte(List<float> inputData, float Activ_, int index, List<Matte> mattes) 
        {
            try
            {
                if (Activ_ > mattes[index].appeal)// Отслежывается активность масок
                {
                    mattes[index].Lesson(inputData);
                    if (mattes[index].appeal < 0.7f)  
                    {
                        mattes[index].appeal += 0.001f;  
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(index.ToString());
                MessageBox.Show(ex.ToString());
            }

            //Формирование масок
            if (Activ_ <= Satiety + 0.2f)
            {
                Matte matte = new Matte(inputData, (ushort)(mattes.Count), Satiety);
                mattes.Add(matte);
            }

            if (Activ_ > Mattes_Activ_max && mattes[index].appeal > Mattes_appeal_max && !mattes[index].elect && mattes[index].Control_value > Mattes_Control_value_max)
            {
                if (Matteselect * 0.01f < reverseMatteselect)
                {
                    Mattes_Activ_max = Activ_;
                    Mattes_appeal_max = mattes[index].appeal;
                    Matte matte = new Matte(inputData, (ushort)(mattes.Count), Satiety, true);
                    mattes.Add(matte);
                    Matteselect++;
                }
            }

            if (Mattes_appeal_max < mattes[index].appeal && Mattes_Activ_max_ < Activ_ && Matteselect * 0.01f < reverseMatteselect)
            {

                Mattes_Activ_max_ = Mattes_Activ_max;
                Mattes_appeal_max_ = Mattes_appeal_max;
                Mattes_Control_value_max_ = mattes[index].Control_value;
            }
        }

        private void ResetByThreshold(List<int> contractionInputData, float[] array, float threshold)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] <= threshold)
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
            BinaryFormatter serial = new BinaryFormatter();

            using (FileStream fs = new FileStream(path + "Matte.dat", FileMode.OpenOrCreate))
            {
                serial.Serialize(fs, Mattes);
            }
            using (FileStream fs = new FileStream(path + "ReverseMatte.dat", FileMode.OpenOrCreate))
            {
                serial.Serialize(fs, ReverseMattes);
            }
            Message += "Объект экспортирован";
        }
        public void LoadMatte(string path)
        {
            BinaryFormatter serial = new BinaryFormatter();

            using (FileStream fs = new FileStream(path + "Matte.dat", FileMode.OpenOrCreate))
            {
                Mattes = (List<Matte>)serial.Deserialize(fs);
            }

            using (FileStream fs = new FileStream(path + "ReverseMatte.dat", FileMode.OpenOrCreate))
            {
                ReverseMattes = (List<ReverseMatte>)serial.Deserialize(fs);
            }
            Message += "Объект импортирован";
        }
    }
}
