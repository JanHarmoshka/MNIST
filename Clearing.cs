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

    class ActivityMasks
    {
        public float Activ_;
        public int Index;
        public ActivityMasks(List<Matte> ListMatte, List<int> ContractionInputData, float[] inter_Data,
            int InputDataCount, int Dispenser, List<float> inter_result, List<int> ContractionInterResultFirst, List<int> ContractionInterResultSecond)
        {
            float Activ;
            Activ_ = -1;
            float ActivSecond;
            int n;
            Index = 0;

            for (int i = 0; i < ListMatte.Count; i++)
            {
                Activ = 0;
                ActivSecond = 0;
                if (ListMatte[i].Control_value > 0)
                {
                    for (int j = 0; j < ContractionInputData.Count; j++)
                    {
                        n = ContractionInputData[j];
                        if (n <= InputDataCount - Dispenser)
                        {
                            Activ += ListMatte[i].matte[n] * inter_Data[n];
                        }
                        else
                        {
                            ActivSecond += ListMatte[i].matte[n] * inter_Data[n];
                        }
                    }
                }
                if (Activ >= Activ_)
                {
                    Activ_ = Activ;
                    Index = i;
                }

                if (Activ > -0.0f)
                {
                    inter_result.Add(Activ);
                    ContractionInterResultFirst.Add(inter_result.Count - 1);
                    //if (ActivSecond > -0.0f)
                    //{
                    //    ContractionInterResultSecond.Add(inter_result.Count - 1);
                    //}
                }
                else if (ActivSecond > -0.0f)
                {
                    inter_result.Add(ActivSecond);
                    ContractionInterResultSecond.Add(inter_result.Count - 1);
                }
                else
                {
                    inter_result.Add(0);
                }

            }
        }
    }
    class ActivityReverseMasks //TODO: всюду задаться вопросами нейминга. 
    {
        public List<float> AssessmentFirst = new List<float>();
        public List<float> AssessmentSecond = new List<float>();
        public float Activ;
        private float SecondActiv;
        public int Ind;
        readonly int DefenseLearning;
        readonly List<ReverseMatte> ListReverseMatte;
        readonly List<int> ContractionInterResultFirst;
        readonly List<int> ContractionInterResultSecond;
        readonly List<float> inter_result;


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
                ListReverseMatte1 = new List<ReverseMatte>(vListReverseMatte);//TODO: здесь и ниже переделать в массивы/списки. Роман: этот зверинец для инкапсуляции в отдельных потоках. Если вычисления ActivityReverseMasks можно распоралелить другим способом, то можно всё это убрать.
                ListReverseMatte2 = new List<ReverseMatte>(vListReverseMatte);
                ListReverseMatte3 = new List<ReverseMatte>(vListReverseMatte);
                ListReverseMatte4 = new List<ReverseMatte>(vListReverseMatte);

                ContractionInterResultFirst1 = new List<int>(vContractionInterResultFirst);
                ContractionInterResultSecond1 = new List<int>(vContractionInterResultSecond);
                ContractionInterResultFirst2 = new List<int>(vContractionInterResultFirst);
                ContractionInterResultSecond2 = new List<int>(vContractionInterResultSecond);
                ContractionInterResultFirst3 = new List<int>(vContractionInterResultFirst);
                ContractionInterResultSecond3 = new List<int>(vContractionInterResultSecond);
                ContractionInterResultFirst4 = new List<int>(vContractionInterResultFirst);
                ContractionInterResultSecond4 = new List<int>(vContractionInterResultSecond);

                inter_result1 = new List<float>(vinter_result);
                inter_result2 = new List<float>(vinter_result);
                inter_result3 = new List<float>(vinter_result);
                inter_result4 = new List<float>(vinter_result);

                AssessmentSecond1.AddRange(AssessmentFirst);
                AssessmentSecond2.AddRange(AssessmentFirst);
                AssessmentSecond3.AddRange(AssessmentFirst);
                AssessmentSecond4.AddRange(AssessmentFirst);
                AssessmentFirst1.AddRange(AssessmentFirst);
                AssessmentFirst2.AddRange(AssessmentFirst);
                AssessmentFirst3.AddRange(AssessmentFirst);
                AssessmentFirst4.AddRange(AssessmentFirst);

                Ind1 = -1;
                Ind2 = -1;
                Ind3 = -1;
                Ind4 = -1;

                Task task = new Task(() => ActivityFor4());
                task.Start();

                Task task2 = new Task(() => ActivityFor2());
                task2.Start();

                Task task3 = new Task(() => ActivityFor3());
                task3.Start();

                ActivityFor1();

                task.Wait();
                task2.Wait();
                task3.Wait();

                Parallel.For(0, AssessmentFirst.Count, i =>
                {
                    AssessmentFirst[i] = AssessmentFirst1[i] + AssessmentFirst2[i] + AssessmentFirst3[i] + AssessmentFirst4[i];
                    AssessmentSecond[i] = AssessmentSecond1[i] + AssessmentSecond2[i] + AssessmentSecond3[i] + AssessmentSecond4[i];
                    if (AssessmentFirst[i] > 0)
                    {
                        ListReverseMatte[i].ActivityFrequency++;
                    }
                    else
                    {
                        ListReverseMatte[i].ActivityFrequency = 0;
                    }
                });

                Activ = Activ1;
                Ind = Ind1;
                if (Activ < Activ2)
                {
                    Activ = Activ2;
                    Ind = Ind2;
                }
                if (Activ < Activ3)
                {
                    Activ = Activ3;
                    Ind = Ind3;
                }
                if (Activ < Activ4)
                {
                    Activ = Activ4;
                    Ind = Ind4;
                }

                SecondActiv = SecondActiv1;
                if (SecondActiv < SecondActiv2)
                {
                    SecondActiv = SecondActiv2;
                }
                if (SecondActiv < SecondActiv3)
                {
                    SecondActiv = SecondActiv3;
                }
                if (SecondActiv < SecondActiv4)
                {
                    SecondActiv = SecondActiv4;
                }

            }
            if (Activ > 1)
            {
                Parallel.For(0, ListReverseMatte.Count, i =>
                {
                    AssessmentFirst[i] = (float)AssessmentFirst[i] / Activ;
                });
            }

            if (SecondActiv > 1)
            {
                Parallel.For(0, ListReverseMatte.Count, i =>
                {
                    AssessmentSecond[i] = (float)AssessmentSecond[i] / SecondActiv;
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

        public List<float> AssessmentFirst1 = new List<float>();
        public List<float> AssessmentSecond1 = new List<float>();
        private float Activ1;
        private float SecondActiv1;
        private int Ind1;
        readonly List<ReverseMatte> ListReverseMatte1;
        readonly List<int> ContractionInterResultFirst1;
        readonly List<int> ContractionInterResultSecond1;
        readonly List<float> inter_result1;

        public void ActivityFor1()
        {
            int n1;
            int n2;
            Activ1 = 0;
            SecondActiv1 = 0;
            Ind1 = -1;

            int ContractionInterResultFirstCount1 = ContractionInterResultFirst1.Count;
            int ContractionInterResultSecondCount1 = ContractionInterResultSecond1.Count;

            for (int i = 0; i < ListReverseMatte1.Count / 4; i++)
            {
                int arrayCorrectLength = ListReverseMatte1[i].Correct.Count;
                if (ListReverseMatte1[i].appeal_ >= 0 & ListReverseMatte1[i].Control_value > 0f & arrayCorrectLength > 0)
                {
                    for (int j = 0; j < ContractionInterResultFirstCount1; j++)
                    {
                        n1 = ContractionInterResultFirst1[j];
                        if (arrayCorrectLength > n1)
                        {
                            AssessmentFirst1[i] += ListReverseMatte1[i].Correct[n1] * inter_result1[n1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int j = 0; j < ContractionInterResultSecondCount1; j++)
                    {
                        n2 = ContractionInterResultSecond1[j];
                        if (arrayCorrectLength > n2)
                        {
                            AssessmentSecond1[i] += ListReverseMatte1[i].Correct[n2] * inter_result1[n2];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < ListReverseMatte1.Count; i++)
            {
                if (AssessmentSecond1[i] >= SecondActiv1)
                {
                    SecondActiv1 = AssessmentSecond1[i];
                }
                if (Activ1 < AssessmentFirst1[i] & AssessmentFirst1[i] > 0 & i < ListReverseMatte1.Count - DefenseLearning)
                {
                    Activ1 = AssessmentFirst1[i];
                    Ind1 = i;
                }
            }
        }

        public List<float> AssessmentFirst2 = new List<float>();
        public List<float> AssessmentSecond2 = new List<float>();
        float Activ2;
        float SecondActiv2;
        int Ind2;
        readonly List<ReverseMatte> ListReverseMatte2;
        readonly List<int> ContractionInterResultFirst2;
        readonly List<int> ContractionInterResultSecond2;
        readonly List<float> inter_result2;

        public void ActivityFor2()//TODO: 2, 3 и 4 методы идентичны, изменить на один или генераторную функцию с каррированием.  
        {
            int n1;
            int n2;
            Activ2 = 0;
            SecondActiv2 = 0;
            Ind2 = -1;

            int ContractionInterResultFirstCount2 = ContractionInterResultFirst2.Count;
            int ContractionInterResultSecondCount2 = ContractionInterResultSecond2.Count;

            for (int i = ListReverseMatte2.Count / 4 + 1; i < ListReverseMatte2.Count / 4 * 2; i++)
            {
                int arrayCorrectLength = ListReverseMatte2[i].Correct.Count;
                if (ListReverseMatte2[i].appeal_ >= 0 & ListReverseMatte2[i].Control_value > 0f & arrayCorrectLength > 0)
                {
                    for (int j = 0; j < ContractionInterResultFirstCount2; j++)
                    {
                        n1 = ContractionInterResultFirst2[j];
                        if (arrayCorrectLength > n1)
                        {
                            AssessmentFirst2[i] += ListReverseMatte2[i].Correct[n1] * inter_result2[n1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int j = 0; j < ContractionInterResultSecondCount2; j++)
                    {
                        n2 = ContractionInterResultSecond2[j];
                        if (arrayCorrectLength > n2)
                        {
                            AssessmentSecond2[i] += ListReverseMatte2[i].Correct[n2] * inter_result2[n2];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < ListReverseMatte2.Count; i++)
            {
                if (AssessmentSecond2[i] >= SecondActiv2)
                {
                    SecondActiv1 = AssessmentSecond2[i];
                }
                if (Activ2 < AssessmentFirst2[i] & AssessmentFirst2[i] > 0 & i < ListReverseMatte2.Count - DefenseLearning)
                {
                    Activ2 = AssessmentFirst2[i];
                    Ind2 = i;
                }
            }
        }

        public List<float> AssessmentFirst3 = new List<float>();
        public List<float> AssessmentSecond3 = new List<float>();
        private float Activ3;
        private float SecondActiv3;
        private int Ind3;
        readonly List<ReverseMatte> ListReverseMatte3;
        readonly List<int> ContractionInterResultFirst3;
        readonly List<int> ContractionInterResultSecond3;
        readonly List<float> inter_result3;

        public void ActivityFor3()
        {
            int n1;
            int n2;
            Activ3 = 0;
            SecondActiv3 = 0;
            Ind3 = -1;

            int ContractionInterResultFirstCount3 = ContractionInterResultFirst1.Count;
            int ContractionInterResultSecondCount3 = ContractionInterResultSecond1.Count;

            for (int i = ListReverseMatte3.Count / 4 * 2 + 1; i < ListReverseMatte3.Count / 4 * 3; i++)
            {
                int arrayCorrectLength = ListReverseMatte3[i].Correct.Count;
                if (ListReverseMatte3[i].appeal_ >= 0 & ListReverseMatte3[i].Control_value > 0f & arrayCorrectLength > 0)
                {
                    for (int j = 0; j < ContractionInterResultFirstCount3; j++)
                    {
                        n1 = ContractionInterResultFirst3[j];
                        if (arrayCorrectLength > n1)
                        {
                            AssessmentFirst3[i] += ListReverseMatte3[i].Correct[n1] * inter_result3[n1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int j = 0; j < ContractionInterResultSecondCount3; j++)
                    {
                        n2 = ContractionInterResultSecond3[j];
                        if (arrayCorrectLength > n2)
                        {
                            AssessmentSecond3[i] += ListReverseMatte3[i].Correct[n2] * inter_result3[n2];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < ListReverseMatte3.Count; i++)
            {
                if (AssessmentSecond3[i] >= SecondActiv3)
                {
                    SecondActiv3 = AssessmentSecond3[i];
                }
                if (Activ3 < AssessmentFirst3[i] & AssessmentFirst3[i] > 0 & i < ListReverseMatte3.Count - DefenseLearning)
                {
                    Activ3 = AssessmentFirst3[i];
                    Ind3 = i;
                }
            }
        }

        public List<float> AssessmentFirst4 = new List<float>();
        public List<float> AssessmentSecond4 = new List<float>();
        private float Activ4;
        private float SecondActiv4;
        private int Ind4;
        readonly List<ReverseMatte> ListReverseMatte4;
        readonly List<int> ContractionInterResultFirst4;
        readonly List<int> ContractionInterResultSecond4;
        readonly List<float> inter_result4;

        public void ActivityFor4()
        {
            int n1;
            int n2;
            Activ4 = 0;
            SecondActiv4 = 0;
            Ind4 = -1;

            int ContractionInterResultFirstCount4 = ContractionInterResultFirst1.Count;
            int ContractionInterResultSecondCount4 = ContractionInterResultSecond1.Count;

            for (int i = ListReverseMatte4.Count / 4 * 3 + 1; i < ListReverseMatte4.Count; i++)
            {
                int arrayCorrectLength = ListReverseMatte4[i].Correct.Count;
                if (ListReverseMatte4[i].appeal_ >= 0 & ListReverseMatte4[i].Control_value > 0f & arrayCorrectLength > 0)
                {
                    for (int j = 0; j < ContractionInterResultFirstCount4; j++)
                    {
                        n1 = ContractionInterResultFirst4[j];
                        if (arrayCorrectLength > n1)
                        {
                            AssessmentFirst4[i] += ListReverseMatte4[i].Correct[n1] * inter_result4[n1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    for (int j = 0; j < ContractionInterResultSecondCount4; j++)
                    {
                        n2 = ContractionInterResultSecond4[j];
                        if (arrayCorrectLength > n2)
                        {
                            AssessmentSecond4[i] += ListReverseMatte4[i].Correct[n2] * inter_result4[n2];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < ListReverseMatte4.Count; i++)
            {
                if (AssessmentSecond4[i] >= SecondActiv4)
                {
                    SecondActiv4 = AssessmentSecond4[i];
                }
                if (Activ4 < AssessmentFirst4[i] & AssessmentFirst4[i] > 0 & i < ListReverseMatte4.Count - DefenseLearning)
                {
                    Activ4 = AssessmentFirst4[i];
                    Ind4 = i;
                }
            }
        }
    }

}
