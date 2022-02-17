using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MNIST
{
    class Clearing
    {
        //public string message;
        public Clearing(List<Matte> ListMatte, List<ReverseMatte> ListReverseMatte, float satiety)
        {
            List<int> Empty = new List<int>();
            for (int j = 0; j < ListMatte.Count; j++)
            {
                //message += "app " + ListMatte[j].appeal.ToString() + " C_v " + ListMatte[j].Control_value.ToString() + " >> " + ListMatte[j].room.ToString() + "\r\n";
                //TODO: числа с плавающей запятой сравниваются на точное равенство, скорее всего, нужно заменить на сравнение с точностью до машинного эпсилон. 
                if ((ListMatte[j].appeal == satiety & ListMatte[j].Control_value <= 0) | (ListMatte[j].Control_value < 0) | (ListMatte[j].appeal < satiety & ListMatte[j].Control_value < 200))
                {
                    Empty.Add(j);
                }
            }

            if (Empty.Count > 0)
            {
                int e;
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
                //message += "ap " + ListReverseMatte[j].appeal_.ToString() + " C " + ListReverseMatte[j].Control_value.ToString() + " r " + ListReverseMatte[j].room.ToString() + "\r\n";
                if ((ListReverseMatte[j].appeal_ <= 0 & ListReverseMatte[j].Control_value <= 0f) | ListReverseMatte[j].Control_value <= 0 | (ListReverseMatte[j].appeal_ <= 0.10f & ListReverseMatte[j].Control_value <= 97.0f)
                    | ListReverseMatte[j].Correct.Count < ListMatte.Count * 0.1f | ListReverseMatte[j].ActivityFrequency > 1000)
                {
                    Empty.Add(j);
                }
            }
            if (Empty.Count > 0)
            {
                for (int j = Empty.Count - 1; j >= 0; j--)
                {
                    ListReverseMatte.RemoveAt(Empty[j]);
                }
            }

            Empty.Clear();
        }
    }

    class Correction
    {
        public Correction(int InputDataCount, List<ReverseMatte> ListReverseMatte, List<float> AssessmentFirst, List<float> AssessmentSecond, float[] inter_Data, float semblance)
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
                if (ListReverseMatte[j].Control_value > 0f)
                {
                    float VFirst_ = (AssessmentSecond[j] * AssessmentFirst[j]) * ListReverseMatte[j].appeal_ * semblance;
                    if (VFirst_ != 0)
                    {
                        for (int i = 0; i < InputDataCount; i++)
                        {
                            inter_Data[i] += ListReverseMatte[j].matte[i] * VFirst_;
                        }
                    }

                }
            }
        }
    }

    struct ActivityArgs
    {
        public List<Matte> ListMatte;
        public List<int> ContractionInputData;
        public float[] InterData;
    }

    class Activity
    {
        public float Activ { get; private set; }
        private float maxAxtiv = -1; //TODO: возможно, я хочу наоборот сделать maxActiv публичным свойством - это название лучше отражает смысл переменной. 
        private float ActivSecond;
        public int Ind { get; private set; }
        private int InputDataCount;
        private int Dispenser;
        private List<Matte> ListMatte;
        private List<int> ContractionInputData;
        private float[] InterData;

        //Нужно для инкансуляции коллекции, чтобы лишние её методы не торчали наружу и не смущали.
        //Внутри класса можно использовать приватное поле, снаружи - только свойство, которое является типом интерфейса и не позволяет себя менять. 
        //Если это перестанет быть желаемым поведением, можно просто поменять типы свойств с IEnumerable на List.
        private readonly List<float> interResult;
        public IEnumerable<float> InterResult { get => interResult; }
        private List<int> contractionInterResultFirst;
        public IEnumerable<int> ContractionInterResultFirst { get => contractionInterResultFirst; }
        private List<int> contractionInterResultSecond;
        public IEnumerable<int> ContractionInterResultSecond { get => contractionInterResultSecond; }
        public Task Task { get; private set; }

        public Activity(ActivityArgs listArgs, int inputDataCount, int dispenser, int taskIdx, int taskCount = 4, float activ = 0, float activeSecond = 0, int ind = 0)
        {
            ListMatte = listArgs.ListMatte;
            ContractionInputData = listArgs.ContractionInputData;
            InterData = listArgs.InterData;

            var len = ListMatte.Count;
            interResult = new List<float>(len); //Здесь и ниже длина этих трёх списков не больше ListMatte.Count.
            contractionInterResultFirst = new List<int>(len);
            contractionInterResultSecond = new List<int>(len);

            Activ = activ;
            ActivSecond = activeSecond;
            Ind = ind;
            InputDataCount = inputDataCount;
            Dispenser = dispenser;

            Task = new Task(GenerateActivityFunction(taskIdx, taskCount));
        }

        //TODO: исправить позже на более изящное решение, это почти наверняка временное. 
        //Скорее всего, нужно будет проверять размер массив ListMatte прямо здесь и в зависимости от него выполнять ActivityFor
        //синхронно или асинхронно. Тогда функционал класса ActivityMasks будет продублирован, значит, нужно будет перетащить 
        //этот код в ActivityMasks. Это замечание справедливо также и для ActivityReverseMasks. 
        //Также TODO: привести нейминг здесь и в ReverseMasks к одному виду. 
        public Action GenerateActivityFunction(int taskIdx, int taskCount)
        {
            //TODO: временное название, чтобы выдержать целостность терминологии аналогично с ReverseActivity. Не забыть потом изменить название и здесь, и там. 
            void ActivityFor() //TODO: сделать что-то подобное ActivityReverseMasks - длинные списки дробить и обрабатывать асинхронно. Done. 
            {
                for (int i = ListMatte.Count / taskCount * taskIdx; i < ListMatte.Count / taskCount * (taskIdx + 1); i++) //TODO: проверить, вроде бы здесь ошибка с определением границ цикла. 
                {
                    Activ = 0;
                    ActivSecond = 0;
                    if (ListMatte[i].Control_value > 0)
                    {
                        for (int j = 0; j < ContractionInputData.Count; j++)
                        {
                            int n = ContractionInputData[j];
                            float matte = ListMatte[i].matte[n];
                            if (matte != 0)// Исключаю операции с 0
                            {
                                if (n <= InputDataCount - Dispenser)
                                {
                                    Activ += matte * InterData[n];
                                }
                                else
                                {
                                    ActivSecond += matte * InterData[n];
                                }

                            }
                        }
                    }

                    if (Activ >= maxAxtiv)
                    {
                        maxAxtiv = Activ;
                        Ind = i;
                    }
                    if (Activ > -0.0f)
                    {
                        interResult.Add(Activ);
                        contractionInterResultFirst.Add(i); //Единый порядок поэтому i
                    }
                    else if (ActivSecond > -0.0f)
                    {
                        interResult.Add(ActivSecond);
                        contractionInterResultSecond.Add(i);
                    }
                    else
                    {
                        interResult.Add(0);
                    }
                }
                Activ = maxAxtiv;
            }
            return ActivityFor;
        }
    }

    class ActivityMasks
    {
        public float Activ_;
        public int Index;

        private const int TaskCount = 4;
        public ActivityMasks(List<Matte> listMatte, List<int> contractionInputData, float[] interData,
            int inputDataCount, int dispenser, List<float> interResult, List<int> contractionInterResultFirst, List<int> contractionInterResultSecond)
        {
            Activ_ = -1;
            Index = 0;

            var listArgs = new ActivityArgs()
            {
                ContractionInputData = contractionInputData, //Компилятор умный, компилятор и так поймёт. 
                InterData = interData, //А я - нет, так что TODO: изменить названия аргументов конструктора, чтобы начинались с маленькой буквы. Done. 
                ListMatte = listMatte
            };
            var activityList = new List<Activity>(TaskCount);
            //Если этот if запихнуть в ActivityFor, то необходимость в классе Activity в основном пропадает, достаточно будет перетащить
            //его функционал сюда. См. также замечание в самом классе Activity. 
            if (listMatte.Count < 500) //TODO: заменить магическую константу на именованную, то же самое и в ReverseMasks. 
            {
                //Здесь должна быть функция вычисления активностей Activ и SecondActiv. Done.
                var activity = new Activity(listArgs, inputDataCount, dispenser, 0, 1);
                activity.Task.RunSynchronously();
                UpdateArraysAndActiv(interResult, contractionInterResultFirst, contractionInterResultSecond, activity);
            }
            else
            {
                //А здесь она же, но применённая асинхронно к ListMatte, разбитому на куски. Done.
                for (int i = 0; i < TaskCount; i++)
                {
                    var activity = new Activity(listArgs, inputDataCount, dispenser, i);
                    activityList.Add(activity);
                    activity.Task.Start();
                }
            }
            foreach (var activity in activityList)
            {
                activity.Task.Wait();
                UpdateArraysAndActiv(interResult, contractionInterResultFirst, contractionInterResultSecond, activity);
            }
        }

        private void UpdateArraysAndActiv(List<float> interResult, List<int> contractionInterResultFirst, List<int> contractionInterResultSecond, Activity activity)
        {
            interResult.AddRange(activity.InterResult);
            contractionInterResultFirst.AddRange(activity.ContractionInterResultFirst);
            contractionInterResultSecond.AddRange(activity.ContractionInterResultSecond);
            if (activity.Activ > Activ_)
            {
                Activ_ = activity.Activ;
                Index = activity.Ind;
            }
        }
    }
    class ActivityReverseMasks //TODO: всюду задаться вопросами нейминга. 
    {
        public List<float> AssessmentFirst = new List<float>();
        public List<float> AssessmentSecond = new List<float>();
        public float Activ { get; private set; } // Было приватным полем, но вызывалось извне. Возможно, просто ошибка текущего изменения, но на всякий случай добавлю комментарий. 
        private float SecondActiv;
        public int Ind;
        readonly int DefenseLearning;
        readonly List<ReverseMatte> ListReverseMatte;
        readonly List<int> ContractionInterResultFirst;
        readonly List<int> ContractionInterResultSecond;
        readonly List<float> inter_result;

        private const int TaskCount = 8;

        public ActivityReverseMasks(List<ReverseMatte> vListReverseMatte, List<int> vContractionInterResultFirst, List<int> vContractionInterResultSecond, List<float> vinter_result)
        {
            DefenseLearning = 20;//20

            Activ = 0;
            SecondActiv = -1;
            Ind = -1;

            ListReverseMatte = new List<ReverseMatte>(vListReverseMatte);

            for (int i = 0; i < ListReverseMatte.Count; i++)
            {
                AssessmentFirst.Add(0);
            }
            AssessmentSecond.AddRange(AssessmentFirst);

            if (ListReverseMatte.Count < 200)
            {
                ContractionInterResultFirst = new List<int>(vContractionInterResultFirst);
                ContractionInterResultSecond = new List<int>(vContractionInterResultSecond);

                inter_result = new List<float>(vinter_result);

                ActivityFor();
            }
            else
            {
                var activityList = new List<ReverseActivity>(TaskCount);
                for (int i = 0; i < TaskCount; i++)
                {
                    //var listArgs = new ActivityArgs()
                    //{
                    //    ContractionInterResultFirst = new List<int>(vContractionInterResultFirst), 
                    //    ContractionInterResultSecond = new List<int>(vContractionInterResultSecond),
                    //    InterResult = new List<float>(vinter_result),
                    //    ListReverseMatte = new List<ReverseMatte>(vListReverseMatte) //TODO: исправить, на каждом шаге используется только ListReverseMatteCount/TaskCount элементов, так что нет необходимости копировать весь список. 
                    //}; //TODO: а точно ли вообще нужно копировать списки? 

                    var listArgs = new ReverseActivityArgs()
                    {
                        ContractionInterResultFirst = vContractionInterResultFirst,
                        ContractionInterResultSecond = vContractionInterResultSecond,
                        InterResult = vinter_result,
                        ListReverseMatte = vListReverseMatte
                    };
                    var activity = new ReverseActivity(listArgs,
                                                i,
                                                DefenseLearning);
                    activity.AssessmentSecond.AddRange(AssessmentFirst); //TODO: уточнить, не опечатка ли? 
                    activity.AssessmentFirst.AddRange(AssessmentFirst);
                    activityList.Add(activity);
                    if (i != 0) activity.Task.Start(); //TODO: не забыть уточнить, почему первый из методов ActivityFor запускается в главном потоке. 
                }

                activityList[0].Task.RunSynchronously();
                for (int i = 1; i < TaskCount; i++) //Не опечатка, цикл должен начинаться с единицы. См. также комментарий выше про ActivityFor. 
                {
                    activityList[i].Task.Wait();
                    if (Activ < activityList[i].Activ)
                    {
                        Activ = activityList[i].Activ;
                        Ind = activityList[i].Ind;
                    }
                    if (SecondActiv < activityList[i].SecondActiv) SecondActiv = activityList[i].SecondActiv;
                }

                Parallel.For(0, AssessmentFirst.Count, i =>
                {
                    //Можно было сделать через LINQ и сэкономить пару строчек, но я решил выдерживать более явный стиль кода.
                    AssessmentFirst[i] = 0;
                    AssessmentSecond[i] = 0;
                    foreach (var activity in activityList)
                    {
                        AssessmentFirst[i] += activity.AssessmentFirst[i];
                        AssessmentSecond[i] += activity.AssessmentSecond[i];
                    }
                    if (AssessmentFirst[i] > 0)
                    {
                        ListReverseMatte[i].ActivityFrequency++;
                    }
                    else
                    {
                        ListReverseMatte[i].ActivityFrequency = 0;
                    }
                });
            }
            if (Activ > 1)
            {
                Parallel.For(0, ListReverseMatte.Count, i =>
                {
                    AssessmentFirst[i] = AssessmentFirst[i] / Activ; //TODO: убрать явное приведение к float. Done. 
                });
            }

            if (SecondActiv > 1)
            {
                Parallel.For(0, ListReverseMatte.Count, i =>
                {
                    AssessmentSecond[i] = (float)AssessmentSecond[i] / SecondActiv; //TODO: то же самое. 
                });
            }
        }
        public void ActivityFor()
        {
            int n1;
            int n2;
            Activ = 0;
            SecondActiv = 0;
            Ind = -1;

            int ContractionInterResultFirstCount = ContractionInterResultFirst.Count;
            int ContractionInterResultSecondCount = ContractionInterResultSecond.Count;

            for (int i = 0; i < ListReverseMatte.Count; i++)
            {
                int arrayCorrectLength = ListReverseMatte[i].Correct.Count;
                if (ListReverseMatte[i].appeal_ >= 0 & ListReverseMatte[i].Control_value > 0f & arrayCorrectLength > 0)
                {
                    for (int j = 0; j < ContractionInterResultFirstCount; j++)
                    {
                        n1 = ContractionInterResultFirst[j];
                        if (arrayCorrectLength > n1)
                        {
                            AssessmentFirst[i] += ListReverseMatte[i].Correct[n1] * inter_result[n1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int j = 0; j < ContractionInterResultSecondCount; j++)
                    {
                        n2 = ContractionInterResultSecond[j];
                        if (arrayCorrectLength > n2)
                        {
                            AssessmentSecond[i] += ListReverseMatte[i].Correct[n2] * inter_result[n2];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < ListReverseMatte.Count; i++)
            {
                if (AssessmentSecond[i] >= SecondActiv)
                {
                    SecondActiv = AssessmentSecond[i];
                }
                if (Activ < AssessmentFirst[i] & AssessmentFirst[i] > 0 & i < ListReverseMatte.Count - DefenseLearning)
                {
                    Activ = AssessmentFirst[i];
                    Ind = i;
                }
            }
        }

        private struct ReverseActivityArgs
        {
            public List<ReverseMatte> ListReverseMatte;
            public List<int> ContractionInterResultFirst;
            public List<int> ContractionInterResultSecond;
            public List<float> InterResult;
        }
        //TODO: перетащить таск в этот класс. Done. 
        private class ReverseActivity
        {
            public List<float> AssessmentFirst = new List<float>();
            public List<float> AssessmentSecond = new List<float>();

            public int Ind { get; private set; }
            public float Activ { get; private set; }
            public float SecondActiv { get; private set; }
            public Task Task { get; private set; } //TODO: возможно, лучше переименовать, чтобы имя переменной не совпадало с именем класса. 
            readonly List<ReverseMatte> ListReverseMatte;
            readonly List<int> ContractionInterResultFirst;
            readonly List<int> ContractionInterResultSecond;
            readonly List<float> inter_result;

            public ReverseActivity(ReverseActivityArgs listArgs,
                            int num,
                            int defenseLearning,
                            int ind = -1,
                            float activ = 0,
                            float secondActiv = 0) //TODO: может, я хочу структуру с аргументами вместо запихивания шести аргументов в конструктор? Done для аргументов-списков. 
            {
                Activ = activ;
                SecondActiv = secondActiv;
                ListReverseMatte = listArgs.ListReverseMatte;
                ContractionInterResultFirst = listArgs.ContractionInterResultFirst;
                ContractionInterResultSecond = listArgs.ContractionInterResultSecond;
                this.inter_result = listArgs.InterResult;
                Ind = ind;
                Task = new Task(GenerateActivityFunction(defenseLearning, num));
            }

            public Action GenerateActivityFunction(int DefenseLearning, int num)
            {
                void ActivityFor()//TODO: 2, 3 и 4 методы идентичны, изменить на один или генераторную функцию с каррированием. Done, также и ActivityFor1 укладывается сюда же. 
                {
                    int n1;
                    int n2;
                    Activ = 0;
                    SecondActiv = 0;
                    Ind = -1;

                    int ContractionInterResultFirstCount = ContractionInterResultFirst.Count;
                    int ContractionInterResultSecondCount = ContractionInterResultSecond.Count;

                    //TODO: возможно, ошибка с индексами в первоначальном коде, исправить, но не забыть уточнить. Done. 
                    //TODO: также не забыть уточнить, всегда ли ListReverseMatte.Count кратно четырём. 
                    for (int i = ListReverseMatte.Count / TaskCount * num; i < ListReverseMatte.Count / TaskCount * (num + 1); i++)
                    {
                        int arrayCorrectLength = ListReverseMatte[i].Correct.Count;
                        if (ListReverseMatte[i].appeal_ >= 0 & ListReverseMatte[i].Control_value > 0f & arrayCorrectLength > 0)
                        {
                            for (int j = 0; j < ContractionInterResultFirstCount; j++)
                            {
                                n1 = ContractionInterResultFirst[j];
                                if (arrayCorrectLength > n1)
                                {
                                    if (ListReverseMatte[i].Correct[n1] != 0)// Исключаю операции с 0
                                    {
                                        AssessmentFirst[i] += ListReverseMatte[i].Correct[n1] * inter_result[n1];
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                            for (int j = 0; j < ContractionInterResultSecondCount; j++)
                            {
                                n2 = ContractionInterResultSecond[j];
                                if (arrayCorrectLength > n2)
                                {
                                    if (ListReverseMatte[i].Correct[n2] != 0)// Исключаю операции с 0
                                    {
                                        AssessmentSecond[i] += ListReverseMatte[i].Correct[n2] * inter_result[n2];
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < ListReverseMatte.Count; i++)
                    {
                        if (AssessmentSecond[i] >= SecondActiv)
                        {
                            SecondActiv = AssessmentSecond[i];
                        }
                        if (Activ < AssessmentFirst[i] & AssessmentFirst[i] > 0 & i < ListReverseMatte.Count - DefenseLearning)
                        {
                            Activ = AssessmentFirst[i];
                            Ind = i;
                        }
                    }
                }
                return ActivityFor;
            }
        }
    }

}
