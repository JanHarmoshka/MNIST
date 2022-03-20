using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MNIST
{
    class Clearing
    {
        //public string message;
        public Clearing(List<Matte> mattes, List<ReverseMatte> reverseMattes, float satiety)
        {
            List<int> empty = new List<int>();
            for (int j = 0; j < mattes.Count; j++)
            {
                //message += "app " + ListMatte[j].appeal.ToString() + " C_v " + ListMatte[j].Control_value.ToString() + " >> " + ListMatte[j].room.ToString() + "\r\n";
                //TODO: числа с плавающей запятой сравниваются на точное равенство, скорее всего, нужно заменить на сравнение с точностью до машинного эпсилон. 
                if ((mattes[j].appeal == satiety && mattes[j].Control_value <= 0) || (mattes[j].Control_value < 0) || (mattes[j].appeal < satiety && mattes[j].Control_value < 200))
                {
                    empty.Add(j);
                }
            }

            if (empty.Count > 0)
            {
                int e;
                for (int j = 0; j < empty.Count; j++)
                {
                    e = empty[j] - j;
                    mattes.RemoveAt(e);
                    for (int i = 0; i < reverseMattes.Count; i++)
                    {
                        if (reverseMattes[i].Correct.Count > e)
                        {
                            reverseMattes[i].Correct.RemoveAt(e);
                            reverseMattes[i].Refined.RemoveAt(e);
                        }
                    }
                }
            }
            empty.Clear();

            for (int j = 0; j < reverseMattes.Count; j++)
            {
                //message += "ap " + ListReverseMatte[j].appeal_.ToString() + " C " + ListReverseMatte[j].Control_value.ToString() + " r " + ListReverseMatte[j].room.ToString() + "\r\n";
                if ((reverseMattes[j].appeal_ <= 0 && reverseMattes[j].Control_value <= 0f) || reverseMattes[j].Control_value <= 0 || (reverseMattes[j].appeal_ <= 0.10f && reverseMattes[j].Control_value <= 97.0f)
                    || reverseMattes[j].Correct.Count < mattes.Count * 0.1f || reverseMattes[j].ActivityFrequency > 1000)
                {
                    empty.Add(j);
                }
            }
            if (empty.Count > 0)
            {
                for (int j = empty.Count - 1; j >= 0; j--)
                {
                    reverseMattes.RemoveAt(empty[j]);
                }
            }

            empty.Clear();
        }
    }

    class Correction
    {
        public Correction(int inputDataCount, List<ReverseMatte> reverseMattes, List<float> firstAssessment, List<float> secondAssessment, float[] interData, float semblance)
        {
            if (secondAssessment.Count < firstAssessment.Count)
            {
                for (int i = secondAssessment.Count; i < firstAssessment.Count; i++)
                {
                    secondAssessment.Add(0);
                }
            }
            for (int j = 0; j < reverseMattes.Count; j++)
            {
                if (reverseMattes[j].Control_value > 0f)
                {
                    float VFirst_ = (secondAssessment[j] * firstAssessment[j]) * reverseMattes[j].appeal_ * semblance;
                    if (VFirst_ != 0)
                    {
                        for (int i = 0; i < inputDataCount; i++)
                        {
                            interData[i] += reverseMattes[j].matte[i] * VFirst_;
                        }
                    }

                }
            }
        }
    }

    struct ActivityArgs
    {
        public List<Matte> Mattes;
        public List<int> ContractionInputData;
        public float[] InterData;
    }

    class Activity
    {
        public float Activ { get; private set; }
        private float maxAxtiv = -1; //TODO: возможно, я хочу наоборот сделать maxActiv публичным свойством - это название лучше отражает смысл переменной. 
        private float ActivSecond;
        public int Ind { get; private set; } //TODO: Возможно, заменить на MaxActivityIndex или как-то так. 
        private int InputDataCount;
        private int Dispenser;
        private List<Matte> mattes;
        private List<int> contractionInputData;
        private float[] interData;

        //Нужно для инкансуляции коллекции, чтобы лишние её методы не торчали наружу и не смущали.
        //Внутри класса можно использовать приватное поле, снаружи - только свойство, которое является типом интерфейса и не позволяет себя менять. 
        //Если это перестанет быть желаемым поведением, можно просто поменять типы свойств с IEnumerable на List.
        private readonly List<float> interResult;
        public IEnumerable<float> InterResult { get => interResult; }
        private List<int> firstContractionInterResult;
        public IEnumerable<int> FirstContractionInterResult { get => firstContractionInterResult; }
        private List<int> secondContractionInterResult;
        public IEnumerable<int> SecondContractionInterResult { get => secondContractionInterResult; }
        public Task Task { get; private set; }

        public Activity(ActivityArgs listArgs, int inputDataCount, int dispenser, int taskIdx, int taskCount = 4, float activ = 0, float activeSecond = 0, int ind = 0)
        {
            mattes = listArgs.Mattes;
            contractionInputData = listArgs.ContractionInputData;
            interData = listArgs.InterData;

            var len = mattes.Count;
            interResult = new List<float>(len); //Здесь и ниже длина этих трёх списков не больше ListMatte.Count.
            firstContractionInterResult = new List<int>(len);
            secondContractionInterResult = new List<int>(len);

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
                for (int i = mattes.Count / taskCount * taskIdx; i < mattes.Count / taskCount * (taskIdx + 1); i++) //TODO: проверить, вроде бы здесь ошибка с определением границ цикла. 
                {
                    Activ = 0;
                    ActivSecond = 0;
                    if (mattes[i].Control_value > 0)
                    {
                        for (int j = 0; j < contractionInputData.Count; j++)
                        {
                            int n = contractionInputData[j];
                            float matte = mattes[i].matte[n];
                            if (matte != 0)// Исключаю операции с 0
                            {
                                if (n <= InputDataCount - Dispenser)
                                {
                                    Activ += matte * interData[n];
                                }
                                else
                                {
                                    ActivSecond += matte * interData[n];
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
                        firstContractionInterResult.Add(i); //Единый порядок поэтому i
                    }
                    else if (ActivSecond > -0.0f)
                    {
                        interResult.Add(ActivSecond);
                        secondContractionInterResult.Add(i);
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
        public ActivityMasks(List<Matte> mattes, List<int> contractionInputData, float[] interData,
            int inputDataCount, int dispenser, List<float> interResult, List<int> firstContractionInterResult, List<int> secondContractionInterResult)
        {
            Activ_ = -1;
            Index = 0;

            var listArgs = new ActivityArgs()
            {
                ContractionInputData = contractionInputData, //Компилятор умный, компилятор и так поймёт. 
                InterData = interData, //А я - нет, так что TODO: изменить названия аргументов конструктора, чтобы начинались с маленькой буквы. Done. 
                Mattes = mattes
            };
            var activities = new List<Activity>(TaskCount);
            //Если этот if запихнуть в ActivityFor, то необходимость в классе Activity в основном пропадает, достаточно будет перетащить
            //его функционал сюда. См. также замечание в самом классе Activity. 
            if (mattes.Count < 500) //TODO: заменить магическую константу на именованную, то же самое и в ReverseMasks. 
            {
                //Здесь должна быть функция вычисления активностей Activ и SecondActiv. Done.
                var activity = new Activity(listArgs, inputDataCount, dispenser, 0, 1);
                activity.Task.RunSynchronously();
                UpdateArraysAndActiv(interResult, firstContractionInterResult, secondContractionInterResult, activity);
            }
            else
            {
                //А здесь она же, но применённая асинхронно к ListMatte, разбитому на куски. Done.
                for (int i = 0; i < TaskCount; i++)
                {
                    var activity = new Activity(listArgs, inputDataCount, dispenser, i);
                    activities.Add(activity);
                    activity.Task.Start();
                }
            }
            foreach (var activity in activities)
            {
                activity.Task.Wait();
                UpdateArraysAndActiv(interResult, firstContractionInterResult, secondContractionInterResult, activity);
            }
        }

        private void UpdateArraysAndActiv(List<float> interResult, List<int> contractionInterResultFirst, List<int> contractionInterResultSecond, Activity activity)
        {
            interResult.AddRange(activity.InterResult);
            contractionInterResultFirst.AddRange(activity.FirstContractionInterResult);
            contractionInterResultSecond.AddRange(activity.SecondContractionInterResult);
            if (activity.Activ > Activ_)
            {
                Activ_ = activity.Activ;
                Index = activity.Ind;
            }
        }
    }
    class ActivityReverseMasks //TODO: всюду задаться вопросами нейминга. 
    {
        public List<float> FirstAssessment = new List<float>();
        public List<float> SecondAssessment = new List<float>();
        public float Activ { get; private set; } // Было приватным полем, но вызывалось извне. Возможно, просто ошибка текущего изменения, но на всякий случай добавлю комментарий. 
        private float secondActiv;
        public int Ind;
        readonly int defenseLearning;
        readonly List<ReverseMatte> reverseMattes;
        readonly List<int> firstContractionInterResult;
        readonly List<int> secondContractionInterResult;
        readonly List<float> interResult;

        private const int TaskCount = 8;

        public ActivityReverseMasks(List<ReverseMatte> vReverseMattes, List<int> vFirstContractionInterResult, List<int> vSecondContractionInterResult, List<float> vInterResult)
        {
            defenseLearning = 20;//20

            Activ = 0;
            secondActiv = -1;
            Ind = -1;

            reverseMattes = new List<ReverseMatte>(vReverseMattes);

            for (int i = 0; i < reverseMattes.Count; i++)
            {
                FirstAssessment.Add(0);
            }
            SecondAssessment.AddRange(FirstAssessment);

            if (reverseMattes.Count < 200)
            {
                firstContractionInterResult = new List<int>(vFirstContractionInterResult);
                secondContractionInterResult = new List<int>(vSecondContractionInterResult);

                interResult = new List<float>(vInterResult);

                ActivityFor();
            }
            else
            {
                var activities = new List<ReverseActivity>(TaskCount);
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
                        FirstContractionInterResult = vFirstContractionInterResult,
                        SecondContractionInterResult = vSecondContractionInterResult,
                        InterResult = vInterResult,
                        ReverseMattes = vReverseMattes
                    };
                    var activity = new ReverseActivity(listArgs,
                                                i,
                                                defenseLearning);
                    activity.SecondAssessment.AddRange(FirstAssessment); //TODO: уточнить, не опечатка ли? 
                    activity.FirstAssessment.AddRange(FirstAssessment);
                    activities.Add(activity);
                    if (i != 0) activity.Task.Start(); //TODO: не забыть уточнить, почему первый из методов ActivityFor запускается в главном потоке. 
                }

                activities[0].Task.RunSynchronously();
                for (int i = 1; i < TaskCount; i++) //Не опечатка, цикл должен начинаться с единицы. См. также комментарий выше про ActivityFor. 
                {
                    activities[i].Task.Wait();
                    if (Activ < activities[i].Activ)
                    {
                        Activ = activities[i].Activ;
                        Ind = activities[i].Ind;
                    }
                    if (secondActiv < activities[i].SecondActiv) secondActiv = activities[i].SecondActiv;
                }

                Parallel.For(0, FirstAssessment.Count, i =>
                {
                    //Можно было сделать через LINQ и сэкономить пару строчек, но я решил выдерживать более явный стиль кода.
                    FirstAssessment[i] = 0;
                    SecondAssessment[i] = 0;
                    foreach (var activity in activities)
                    {
                        FirstAssessment[i] += activity.FirstAssessment[i];
                        SecondAssessment[i] += activity.SecondAssessment[i];
                    }
                    if (FirstAssessment[i] > 0)
                    {
                        reverseMattes[i].ActivityFrequency++;
                    }
                    else
                    {
                        reverseMattes[i].ActivityFrequency = 0;
                    }
                });
            }
            if (Activ > 1)
            {
                Parallel.For(0, reverseMattes.Count, i =>
                {
                    FirstAssessment[i] = FirstAssessment[i] / Activ; //TODO: убрать явное приведение к float. Done. 
                });
            }

            if (secondActiv > 1)
            {
                Parallel.For(0, reverseMattes.Count, i =>
                {
                    SecondAssessment[i] = (float)SecondAssessment[i] / secondActiv; //TODO: то же самое. 
                });
            }
        }
        public void ActivityFor()
        {
            int n1;
            int n2;
            Activ = 0;
            secondActiv = 0;
            Ind = -1;

            int firstContractionInterResultCount = firstContractionInterResult.Count;
            int secondContractionInterResultCount = secondContractionInterResult.Count;

            for (int i = 0; i < reverseMattes.Count; i++)
            {
                int arrayCorrectLength = reverseMattes[i].Correct.Count;
                if (reverseMattes[i].appeal_ >= 0 && reverseMattes[i].Control_value > 0f && arrayCorrectLength > 0)
                {
                    for (int j = 0; j < firstContractionInterResultCount; j++)
                    {
                        n1 = firstContractionInterResult[j];
                        if (arrayCorrectLength > n1)
                        {
                            FirstAssessment[i] += reverseMattes[i].Correct[n1] * interResult[n1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int j = 0; j < secondContractionInterResultCount; j++)
                    {
                        n2 = secondContractionInterResult[j];
                        if (arrayCorrectLength > n2)
                        {
                            SecondAssessment[i] += reverseMattes[i].Correct[n2] * interResult[n2];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < reverseMattes.Count; i++)
            {
                if (SecondAssessment[i] >= secondActiv)
                {
                    secondActiv = SecondAssessment[i];
                }
                if (Activ < FirstAssessment[i] && FirstAssessment[i] > 0 && i < reverseMattes.Count - defenseLearning)
                {
                    Activ = FirstAssessment[i];
                    Ind = i;
                }
            }
        }

        private struct ReverseActivityArgs
        {
            public List<ReverseMatte> ReverseMattes;
            public List<int> FirstContractionInterResult;
            public List<int> SecondContractionInterResult;
            public List<float> InterResult;
        }
        //TODO: перетащить таск в этот класс. Done. 
        private class ReverseActivity
        {
            public List<float> FirstAssessment = new List<float>();
            public List<float> SecondAssessment = new List<float>();

            public int Ind { get; private set; } //TODO: см. замечание к аналогичному полю в классе Activity. 
            public float Activ { get; private set; }
            public float SecondActiv { get; private set; }
            public Task Task { get; private set; } //TODO: возможно, лучше переименовать, чтобы имя переменной не совпадало с именем класса. 
            readonly List<ReverseMatte> reverseMattes;
            readonly List<int> firstContractionInterResult;
            readonly List<int> secondContractionInterResult;
            readonly List<float> interResult;

            public ReverseActivity(ReverseActivityArgs listArgs,
                            int num,
                            int defenseLearning,
                            int ind = -1,
                            float activ = 0,
                            float secondActiv = 0) //TODO: может, я хочу структуру с аргументами вместо запихивания шести аргументов в конструктор? Done для аргументов-списков. 
            {
                Activ = activ;
                SecondActiv = secondActiv;
                reverseMattes = listArgs.ReverseMattes;
                firstContractionInterResult = listArgs.FirstContractionInterResult;
                secondContractionInterResult = listArgs.SecondContractionInterResult;
                interResult = listArgs.InterResult;
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

                    int firstContractionInterResultCount = firstContractionInterResult.Count;
                    int secondContractionInterResultCount = secondContractionInterResult.Count;

                    //TODO: возможно, ошибка с индексами в первоначальном коде, исправить, но не забыть уточнить. Done. 
                    //TODO: также не забыть уточнить, всегда ли ListReverseMatte.Count кратно четырём. 
                    for (int i = reverseMattes.Count / TaskCount * num; i < reverseMattes.Count / TaskCount * (num + 1); i++)
                    {
                        int arrayCorrectLength = reverseMattes[i].Correct.Count;
                        if (reverseMattes[i].appeal_ >= 0 && reverseMattes[i].Control_value > 0f && arrayCorrectLength > 0)
                        {
                            for (int j = 0; j < firstContractionInterResultCount; j++)
                            {
                                n1 = firstContractionInterResult[j];
                                if (arrayCorrectLength > n1)
                                {
                                    if (reverseMattes[i].Correct[n1] != 0)// Исключаю операции с 0
                                    {
                                        FirstAssessment[i] += reverseMattes[i].Correct[n1] * interResult[n1];
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                            for (int j = 0; j < secondContractionInterResultCount; j++)
                            {
                                n2 = secondContractionInterResult[j];
                                if (arrayCorrectLength > n2)
                                {
                                    if (reverseMattes[i].Correct[n2] != 0)// Исключаю операции с 0
                                    {
                                        SecondAssessment[i] += reverseMattes[i].Correct[n2] * interResult[n2];
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < reverseMattes.Count; i++)
                    {
                        if (SecondAssessment[i] >= SecondActiv)
                        {
                            SecondActiv = SecondAssessment[i];
                        }
                        if (Activ < FirstAssessment[i] && FirstAssessment[i] > 0 && i < reverseMattes.Count - DefenseLearning)
                        {
                            Activ = FirstAssessment[i];
                            Ind = i;
                        }
                    }
                }
                return ActivityFor;
            }
        }
    }

}
