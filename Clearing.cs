using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MNIST
{
    class Clearing
    {
        public Clearing(List<Matte> mattes, List<ReverseMatte> reverseMattes, float satiety, List<bool> ReverseMattes_List)
        {
            List<int> empty = new List<int>();
            for (int j = 0; j < mattes.Count; j++)
            {
                if ((mattes[j].Control_value <= 0) )
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
                if (reverseMattes[j].Control_value <= 0 )
                {
                    empty.Add(j);
                    ReverseMattes_List[reverseMattes[j].Live] = false;
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

    /// <summary>
    /// коррекция входящего сигнала
    /// </summary>
    class Correction
    {
        public float participation;
        public Correction(int inputDataCount, List<ReverseMatte> reverseMattes, List<float> firstAssessment, List<float> secondAssessment, float[] interData, float semblance, List<int> indexData_)
        {
            participation = 40;
            if (secondAssessment.Count < firstAssessment.Count)
            {
                for (int i = secondAssessment.Count; i < firstAssessment.Count; i++)
                {
                    secondAssessment.Add(0);
                }
            }
            bool roomContr = false;
            for (int i = 0; i < reverseMattes.Count; i++)
            {
                if (reverseMattes[i].participation > 5)
                {
                    roomContr = true;
                    break;
                }
            }
            for (int j = 0; j < reverseMattes.Count; j++)
            {
                if (reverseMattes[j].Control_value > 0f)
                {
                    float VFirst_ = (secondAssessment[j] * firstAssessment[j]) * reverseMattes[j].appeal_;//
                    if (VFirst_ != 0.0f)
                    {
                        for (int i = 0; i < inputDataCount; i++)
                        {
                            interData[i] += reverseMattes[j].matte[i] * VFirst_;
                        }
                    }
                    if (firstAssessment[j] > 0.01f)
                    {
                        if (reverseMattes[j].participation < participation + 1)
                        {
                            reverseMattes[j].participation++;
                            if (roomContr && reverseMattes[j].participation > 10)
                            {
                                reverseMattes[j].room = true;
                            }
                        }

                        if (participation < reverseMattes[j].participation && reverseMattes[j].appeal_ > 0.5f && indexData_.Count < 3)
                        {
                            indexData_.Add(j);
                        }
                    }
                    else
                    {
                        reverseMattes[j].participation = 0;
                        reverseMattes[j].room = false;
                    }
                }
            }
            if (indexData_.Count == 0)
            {
                indexData_.Add(-1);
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
        private float maxAxtiv = -1;
        private float ActivSecond;
        public int Ind { get; private set; }
        private readonly int InputDataCount;
        private readonly int Dispenser;
        private readonly List<Matte> mattes;
        private readonly List<int> contractionInputData;
        private readonly float[] interData;

        private readonly List<float> interResult;
        public IEnumerable<float> InterResult { get => interResult; }
        private readonly List<int> firstContractionInterResult;
        public IEnumerable<int> FirstContractionInterResult { get => firstContractionInterResult; }
        private readonly List<int> secondContractionInterResult;
        public IEnumerable<int> SecondContractionInterResult { get => secondContractionInterResult; }
        public Task Task { get; private set; }

        public Activity(ActivityArgs listArgs, InternalAcitivityArgs internalArgs, int inputDataCount, int dispenser, int taskIdx, int taskCount = 4, float activ = 0, float activeSecond = 0, int ind = 0)
        {
            mattes = listArgs.Mattes;
            contractionInputData = listArgs.ContractionInputData;
            interData = listArgs.InterData;

            var len = mattes.Count;
            interResult = internalArgs.interResult;
            firstContractionInterResult = internalArgs.firstContractionInterResult;
            secondContractionInterResult = internalArgs.secondContractionInterResult;
            interResult.Clear();
            firstContractionInterResult.Clear();
            secondContractionInterResult.Clear();

            Activ = activ;
            ActivSecond = activeSecond;
            Ind = ind;
            InputDataCount = inputDataCount;
            Dispenser = dispenser;

            Task = new Task(GenerateActivityFunction(taskIdx, taskCount));
        }

        public Action GenerateActivityFunction(int taskIdx, int taskCount)
        {

            void ActivityFor()
            {
                for (int i = mattes.Count / taskCount * taskIdx; i < mattes.Count / taskCount * (taskIdx + 1); i++)
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

    struct InternalActivityMasksArgs
    {
        public List<List<float>> interResults;
        public List<List<int>> firstContractionInterResults;
        public List<List<int>> secondContractionInterResults;
    }

    struct InternalAcitivityArgs
    {
        public List<float> interResult;
        public List<int> firstContractionInterResult;
        public List<int> secondContractionInterResult;
    }

    struct ActivityMasksArgs
    {
        public List<Matte> mattes;
        public List<int> contractionInputData;
        public float[] interData;
        public List<float> interResult;
        public List<int> firstContractionInterResult;
        public List<int> secondContractionInterResult;
    }

    class ActivityMasks
    {
        public float Activ_;
        public int Index;

        private const int TaskCount = 4;
        public ActivityMasks(ActivityMasksArgs args, InternalActivityMasksArgs internalArgs, int inputDataCount, int dispenser)
        {
            Activ_ = -1;
            Index = 0;

            var listArgs = new ActivityArgs()
            {
                ContractionInputData = args.contractionInputData,
                InterData = args.interData,
                Mattes = args.mattes
            };
            var activities = new List<Activity>(TaskCount);

            if (args.mattes.Count < 500)
            {
                var internals = new InternalAcitivityArgs()
                {
                    interResult = internalArgs.interResults[0],
                    firstContractionInterResult = internalArgs.firstContractionInterResults[0],
                    secondContractionInterResult = internalArgs.secondContractionInterResults[0]
                };
                var activity = new Activity(listArgs, internals, inputDataCount, dispenser, 0, 1);
                activity.Task.RunSynchronously();
                UpdateArraysAndActiv(args.interResult, args.firstContractionInterResult, args.secondContractionInterResult, activity);
            }
            else
            {
                for (int i = 0; i < TaskCount; i++)
                {
                    var internals = new InternalAcitivityArgs()
                    {
                        interResult = internalArgs.interResults[i],
                        firstContractionInterResult = internalArgs.firstContractionInterResults[i],
                        secondContractionInterResult = internalArgs.secondContractionInterResults[i]
                    };
                    var activity = new Activity(listArgs, internals, inputDataCount, dispenser, i);
                    activities.Add(activity);
                    activity.Task.Start();
                }
            }
            foreach (var activity in activities)
            {
                activity.Task.Wait();
                UpdateArraysAndActiv(args.interResult, args.firstContractionInterResult, args.secondContractionInterResult, activity);
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
    class ActivityReverseMasks
    {
        public List<float> FirstAssessment = new List<float>();
        public List<float> SecondAssessment = new List<float>();
        public float Activ;
        public float secondActiv;
        public int Ind;
        readonly int defenseLearning;
        readonly List<ReverseMatte> reverseMattes;
        readonly List<int> firstContractionInterResult;
        readonly List<int> secondContractionInterResult;
        readonly List<float> interResult;

        private const int TaskCount = 8;

        public ActivityReverseMasks(List<ReverseMatte> vReverseMattes, List<int> vFirstContractionInterResult, List<int> vSecondContractionInterResult, List<float> vInterResult)
        {
            defenseLearning = 20;

            Activ = 0;
            secondActiv = -1;
            Ind = -1;

            reverseMattes = new List<ReverseMatte>(vReverseMattes);

            for (int i = 0; i < reverseMattes.Count; i++)
            {
                FirstAssessment.Add(0);
            }
            SecondAssessment.AddRange(FirstAssessment);

            firstContractionInterResult = new List<int>(vFirstContractionInterResult);
            secondContractionInterResult = new List<int>(vSecondContractionInterResult);

            if (reverseMattes.Count < 200)
            {
                interResult = new List<float>(vInterResult);

                ActivityFor();
            }
            else
            {
                var activities = new List<ReverseActivity>(TaskCount);
                for (int i = 0; i < TaskCount; i++)
                {
                    var listArgs = new ReverseActivityArgs()
                    {
                        FirstContractionInterResult = vFirstContractionInterResult,
                        SecondContractionInterResult = vSecondContractionInterResult,
                        InterResult = vInterResult,
                        ReverseMattes = vReverseMattes
                    };

                    var activity = new ReverseActivity(listArgs, i, defenseLearning);

                    activity.SecondAssessment.AddRange(FirstAssessment);
                    activity.FirstAssessment.AddRange(FirstAssessment);
                    activities.Add(activity);
                    if (i != 0) activity.Task.Start();
                }

                activities[0].Task.RunSynchronously();
                for (int i = 1; i < TaskCount; i++)
                {
                    activities[i].Task.Wait();
                    if (Activ < activities[i].Activ)
                    {
                        Activ = activities[i].Activ;
                        Ind = activities[i].Ind;
                    }
                    if (secondActiv < activities[i].SecondActiv) secondActiv = activities[i].SecondActiv;
                }
                for (int i = 0; i < FirstAssessment.Count; i++)
                {
                    FirstAssessment[i] = 0;
                    SecondAssessment[i] = 0;
                }

                for (int j = 0; j < activities.Count; j++)
                {
                    for (int i = 0; i < activities[j].CounterFirstAssessment.Count; i++)
                    {
                        int n = activities[j].CounterFirstAssessment[i];
                        FirstAssessment[n] += activities[j].FirstAssessment[n];
                    }

                    for (int i = 0; i < activities[j].CounterSecondAssessment.Count; i++)
                    {
                        int n = activities[j].CounterSecondAssessment[i];
                        SecondAssessment[n] += activities[j].SecondAssessment[n];
                    }

                }

            }
            if (Activ > 1)
            {
                Parallel.For(0, reverseMattes.Count, i =>
                {
                    FirstAssessment[i] = FirstAssessment[i] / Activ;
                });
            }

            if (secondActiv > 1)
            {
                Parallel.For(0, reverseMattes.Count, i =>
                {
                    SecondAssessment[i] = (float)SecondAssessment[i] / secondActiv;
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
                if (reverseMattes[i].Control_value > 0f && arrayCorrectLength > 0)
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

        private class ReverseActivity
        {
            public List<float> FirstAssessment = new List<float>();
            public List<int> CounterFirstAssessment = new List<int>();

            public List<float> SecondAssessment = new List<float>();
            public List<int> CounterSecondAssessment = new List<int>();

            public int Ind { get; private set; }
            public float Activ { get; private set; }
            public float SecondActiv { get; private set; }
            public Task Task { get; private set; }
            readonly List<ReverseMatte> reverseMattes;
            readonly List<int> firstContractionInterResult;
            readonly List<int> secondContractionInterResult;
            readonly List<float> interResult;

            public ReverseActivity(ReverseActivityArgs listArgs, int num, int defenseLearning, int ind = -1, float activ = 0, float secondActiv = 0)
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
                void ActivityFor()
                {
                    int n1;
                    int n2;
                    Activ = 0;
                    SecondActiv = 0;
                    Ind = -1;

                    int firstContractionInterResultCount = firstContractionInterResult.Count;
                    int secondContractionInterResultCount = secondContractionInterResult.Count;

                    if (firstContractionInterResultCount > 0 || secondContractionInterResultCount > 0)
                    {
                        for (int i = reverseMattes.Count / TaskCount * num; i < reverseMattes.Count / TaskCount * (num + 1); i++)
                        {
                            bool CounterFirst = false;
                            bool CounterSecond = false;

                            int arrayCorrectLength = reverseMattes[i].Correct.Count;
                            if (arrayCorrectLength > 0 && reverseMattes[i].Control_value > 0f)
                            {
                                float[] BuffCorrekt = reverseMattes[i].Correct.ToArray();

                                for (int j = 0; j < firstContractionInterResultCount; j++)
                                {
                                    n1 = firstContractionInterResult[j];
                                    if (arrayCorrectLength > n1)
                                    {
                                        if (BuffCorrekt[n1] != 0)// Исключаю операции с 0
                                        {
                                            FirstAssessment[i] += BuffCorrekt[n1] * interResult[n1];
                                            CounterFirst = true;
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
                                        if (BuffCorrekt[n2] != 0)// Исключаю операции с 0
                                        {
                                            SecondAssessment[i] += BuffCorrekt[n2] * interResult[n2];
                                            CounterSecond = true;
                                        }

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            if (CounterFirst && FirstAssessment[i] > 0)
                            {
                                CounterFirstAssessment.Add(i);
                            }
                            if (CounterSecond && SecondAssessment[i] > 0)
                            {
                                CounterSecondAssessment.Add(i);
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

                }
                return ActivityFor;
            }
        }
    }

}
