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
        private List<bool> ReverseMattes_List = new List<bool>();

        public int ErrorCount { get; set; } = 60000;//Количество элементов. 

        public float Satiety { get; set; } = 0.15f; //Порог жизни маски

        public float CorrectionThreshold { get; set; } = 0.8f;

        public float V5 { get; set; } = 0.2f;//Мах активность группы

        public int MemoryDuration { get; set; } = 1000;//Длительность памяти

        public int SleepStep = 22;

        private readonly List<float> firstAssessment = new List<float>();
        private readonly List<float> secondAssessment = new List<float>();

        private int assessmentCounter = 0;
        private int reverseMatteCounter;

        private readonly List<float> interResult = new List<float>();
        private readonly List<int> contractionInputData = new List<int>();
        private readonly List<int> firstContractionInterResult = new List<int>();
        private readonly List<int> secondContractionInterResult = new List<int>();

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
                _ = new Clearing(Mattes, ReverseMattes, Satiety, ReverseMattes_List);// Matteselect,
            }

            float[] interData = inputData.ToArray();
            int inputDataCount = interData.Length;

            //Коррекция входящего сигнала 
            if (inputDataCount > 0 && ReverseMattes.Count > 0 && firstAssessment.Count > 0)
            {
                _ = new Correction(inputDataCount, ReverseMattes, firstAssessment, secondAssessment, interData, semblance, indexData_);
                indexData = indexData_[0];// Индекс узнанного нейросетью события (вспомогательная величина соотносящаяся с реальным событием)
            }

            for (int i = 0; i < inputDataCount; i++)// Порог минимального значения входящих данных после коррекции     
            {
                if (interData[i] <= CorrectionThreshold)
                {
                    interData[i] = 0;
                }
                else
                {
                    contractionInputData.Add(i);//Для ускорения расчётов
                }
            }

            counter.inter_Data_.Clear();
            counter.summ2 = 0;
            for (int i = inputDataCount - dispenser; i < inputDataCount; i++)
            {
                counter.inter_Data_.Add(interData[i] > 1 ? (byte)1 : (byte)0); //Список активных моторных нейронов
                if (interData[i] > 1)
                    counter.summ2++;// сведения о наличии активности моторных нейронов 
            }

            counter.inter_Data_Full.Clear();
            for (int i = 0; i < inputDataCount; i++)
                counter.inter_Data_Full.Add(interData[i] > 1 ? (byte)1 : (byte)0);// Список активных зрительных и моторных нейронов. Передаётся в обработчик движения Preparationinput 


            if (Mattes.Count == 0) //Инициализация нейронов первого слоя
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
            ActivityMasks activityMasks = new ActivityMasks(activityMasksArgs, internalActivityMasksArgs, inputDataCount, dispenser); //Расчёт активности первого слоя
            float Activ_ = activityMasks.Activ_; //Значение активнсти самого возбуждённого нейрона первого слоя
            int Index = activityMasks.Index; //Самый возбуждённый нейрон первого слоя
            for (int i = 0; i < firstContractionInterResult.Count; i++) //Фиксируется обращение к нейроны первого слоя
            {
                Mattes[i].Contraction_ = false;
            }

            for (int i = 0; i < interResult.Count; i++) counter.room2.Add((int)Math.Truncate(interResult[i] * 100));// Выыод данных для графтка активности первого слоя

            TeachMatte(inputData, Activ_, Index, Mattes);//формирование и обучение первого слоя            

            ActivityReverseMasks activityReverseMasks = CalculateReverseMasksActivity(interResult, firstContractionInterResult, secondContractionInterResult, counter, firstAssessment, secondAssessment); //Расчёт активности второго слоя
            int maxActivityIndex = activityReverseMasks.Ind; //Самый возбуждённый нейрон второго слоя
            float firstActiv = activityReverseMasks.Activ; // Значение максимального возбуждения среди зрительных нейронов
            float secondActiv = activityReverseMasks.secondActiv;//Значение максимального возбуждения среди моторных нейронов
            for (int i = 0; i < firstAssessment.Count; i++) //Фиксируется обращение к нейроны второго слоя
            {
                if (firstAssessment[i] > 0)
                {
                    ReverseMattes[i].Contraction_ = false;
                }
            }

            if (assessmentCounter % 4000 == 0)//Вывод в консоль кратких сведеней о нейросети            
                Message += "нейр:" + Mattes.Count.ToString() + " гр:" + ReverseMattes.Count.ToString() + "\r\n";

            if (ReverseMattes.Count == 0)// Формирование второго слоя
            {
                reverseMatteCounter++; //Здесь и ниже, счётчик фактического порядкого номера нейрона второго слоя
                CreateReverseMatte(inputData, indexData, interResult, reverseSatiety, reverseMatteCounter);
            }
            else
            {
                bool IndVarMax = maxActivityIndex == -1 && firstActiv < 0.1f; // Если нет активных нейронов во втором слое или если возбуждение нейрона второго слоя не значительно.|| ( && firstActiv > 0.0f)

                if (firstAssessment.Count > 0 && (IndVarMax))
                {
                    if ((maxActivityIndex == -1)) // Если во втором слое нет ни одного нейрона удовлетворяющего условиям возбуждённого состояния. Активность второго слоя обнуляется.
                    {
                        for (int i = 0; i < firstAssessment.Count; i++)
                        {
                            firstAssessment[i] = 0;
                        }
                        pass = false;
                    }
                    CreateReverseMatte(inputData, indexData, interResult, reverseSatiety, reverseMatteCounter); // формирование нейронов второго слоя
                    maxActivityIndex = ReverseMattes.Count - 1; //Вновь сформированный нейрон назначается самым активным
                    firstAssessment.Add(1);

                    reverseMatteCounter++;
                }
            }//Конец формирования второго слоя

            TeachReverseMatte(inputData, Activ_, interResult, ref correctTrigger, pass, counter, activityReverseMasks, ref maxActivityIndex); //Обучение второго слоя


            if (assessmentCounter % 10 == 0) FixLesson(pass, SleepStep, Mattes, ReverseMattes); //Фиксация обоих слоёв

            return counter;
        }



        private void TeachReverseMatte(List<float> inputData, float Activ_, List<float> interResult, ref bool correctTrigger, bool pass, Counter counter, ActivityReverseMasks activityReverseMasks, ref int index)
        {
            float activ = activityReverseMasks.Activ;
            counter.str1 = activ;

            if (activ > 1)
            {
                for (int i = 0; i < ReverseMattes.Count; i++)
                {
                    firstAssessment[i] = (float)firstAssessment[i] / activ;// Приведение значения активности результирующих масок к еденице. Так становится понятно где максимум.
                }
            }


            if (index > -1 && pass) //Если есть активный нейрон первого слоя, давно создан 
            {
                activ = firstAssessment[index];
                if (ReverseMattes[index].room && activ >= 0.3f) //Основной цикл обучения самого возбуждённого нейрона второго слоя начинается только для тех нейронов, которые имеют приемственность в передаче сигнала и высокую активность.
                {
                    if (activ >= 0.7f) // Если нейрон достаточно активен и участвует в цепочке возбуждения.
                    {
                        ReverseMattes[index].Lesson(inputData, interResult, correctTrigger); // Нейрон обучается как на входящей в слой информации, так и на информации поступившей в нейронную сеть. Последнее необходимо для формирования исходящего сигнала передаваемого в обработчик движения Preparationinput.
                    }
                    else
                    {
                        ReverseMattes[index].Lesson(inputData, interResult);
                    }
                    if (ReverseMattes[index].appeal_ < 2)
                        ReverseMattes[index].appeal_ += 0.001f;
                }//Конец основного цикла обучения

                for (int i = 0; i < ReverseMattes.Count; i++)// Формирование обстракций
                {
                    if (index != i && firstAssessment[i] > 0 && ReverseMattes[i].appeal_ < 0.5f)//
                    {
                        ReverseMattes[i].Lesson(inputData, interResult, firstAssessment[i] * 0.0001f);
                        //if (ReverseMattes[i].appeal_ < 2)
                        //    ReverseMattes[i].appeal_ += firstAssessment[i] * 0.001f;
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
                    if (mattes[j].Contraction_)
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
                    if (reverseMattes[i].Contraction_ && pass)
                    {
                        reverseMattes[i].Control_value -= 0.01f;
                    }
                }
            }
        }

        private void CreateReverseMatte(List<float> inputData, int dataIndex, List<float> interResult, float reverseSatiety, int reverseMatteCounter)
        {
            ReverseMatte reverseMatte = new ReverseMatte(inputData, dataIndex, interResult, reverseMatteCounter, reverseSatiety);
            ReverseMattes.Add(reverseMatte);
            ReverseMattes_List.Add(true);
        }

        private ActivityReverseMasks CalculateReverseMasksActivity(List<float> interResult, List<int> firstContractionInterResult, List<int> secondContractionInterResult,
                                                                   Counter counter, List<float> firstAssessment, List<float> secondAssessment)
        {
            firstAssessment.Clear();
            secondAssessment.Clear();
            ActivityReverseMasks activityReverseMasks = new ActivityReverseMasks(ReverseMattes, firstContractionInterResult, secondContractionInterResult, interResult);
            firstAssessment.AddRange(activityReverseMasks.FirstAssessment);
            secondAssessment.AddRange(activityReverseMasks.SecondAssessment);
            for (int i = 0; i < firstAssessment.Count; i++)
            {
                counter.Assessment.Add(firstAssessment[i] + secondAssessment[i]);
                counter.room.Add((int)Math.Truncate(firstAssessment[i] * 100));
            }

            return activityReverseMasks;
        }

        private void TeachMatte(List<float> inputData, float Activ_, int index, List<Matte> mattes)
        {
            if (Activ_ <= 0.1f) //Дублирование самого возбуждённого нейрона первого слоя, если его активность не превышает порог, или создание нового нейрона.
            {
                Matte matte = new Matte(inputData, (ushort)(mattes[mattes.Count - 1].room + 1), Satiety);
                mattes.Add(matte);
            }
            else
            {
                if (Activ_ > 0.8f && mattes[index].appeal > 0.5f)// Если в первом слое есть активный нейрон mattes[index].appeal
                {
                    mattes[index].Lesson(inputData);
                }
                if (mattes[index].appeal < 1.0f)
                {
                    mattes[index].appeal += 0.01f;
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
