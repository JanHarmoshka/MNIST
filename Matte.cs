using System;
using System.Collections.Generic;

namespace MNIST
{
    /// Маска 
    [Serializable]
    public class Matte
    {
        public int room;
        public float appeal;
        public bool Contraction;
        public bool Contraction_;
        public int Control_value;
        public int SleepStep;
        readonly List<float> mask = new List<float>();
        public List<float> matte = new List<float>();

        public Matte(List<float> InputData, ushort Room, float satiety = 0.4f)
        {
            room = Room;
            mask = InputData.GetRange(0, InputData.Count);
            for (int i = 0; i < mask.Count; i++)
            {
                matte.Add(0);
            }
            appeal = satiety;
            Control_value = 250;
            this.Sleep();
            Contraction = true;
            SleepStep = 10;
        }

        /// Сон маски. 
        /// Это ситуация повторного обучения маски, после обновления
        /// представления об ассоциированном событии.
        public void Sleep()
        {
            Contraction = false;
            Contraction_ = false;
            Single max = 0;
            Single max_0;
            for (int j = 0; j < mask.Count; j++)
            {
                if (max < mask[j])
                    max = mask[j];
            }
            max_0 = max;

            max /= 1.85f;//2
            Single summ = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                matte[j] = mask[j] - max;
                if (matte[j] > 0)
                    summ += matte[j];
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
                summ_0 += matte[j];
            }
            if (max_0 < 10)// 50.0f SleepStep
            {
                if (summ_0 < 0.3f)//0.1
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
            //if (SleepStep < 300)
            //{
            //    SleepStep++;
            //}

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
    }

    /// Результирующая маска
    [Serializable]
    public class ReverseMatte
    {
        public int room;
        public int Live;
        public float Control_value;
        public bool Contraction;
        public bool Contraction_;
        readonly float appeal;
        public float appeal_;
        readonly List<float> mask = new List<float>();
        public List<float> matte = new List<float>();
        public List<float> Refined = new List<float>();
        public List<float> Correct = new List<float>();
        public List<float> Appeal = new List<float>();

        public ReverseMatte(List<float> InputData, int IndexData, List<float> inter_result, int nn, float satiety = 0.3f)
        {
            appeal = satiety;
            appeal_ = 0.0f;
            room = IndexData;
            //mask = InputData.GetRange(0, InputData.Count);
            //Refined = inter_result.GetRange(0, inter_result.Count);
            Live = nn;
            for (int i = 0; i < InputData.Count; i++)
            {
                mask.Add(InputData[i] * 0.1f);
                matte.Add(-0f);
            }
            for (int i = 0; i < inter_result.Count; i++)
            {
                Refined.Add(inter_result[i] * 0.1f);
                Correct.Add(-0.5f);
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
            if (inter_result.Count > 0)
            {
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

            max /= 3f;
            Single summ = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                matte[j] = mask[j] - max;
                if (matte[j] > 0)
                    summ += matte[j] * 1f;
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
                summ_0 += Correct[j];
            }
            summ_0 /= Correct.Count;

            if (Math.Abs(summ_0) < 1.1f)
            {
                if (Control_value > 0 & appeal_ < 0.95f)//0,8
                {
                    Control_value--;
                    Contraction_ = true;
                }
            }
            else
            {
                Control_value = 101;
            }
        }
    }

    public class Counter
    {
        public float str1;
        public bool pos;
        public bool str2;
        public List<byte> inter_Data_ = new List<byte>();
        public List<byte> inter_Data_Full = new List<byte>();
        public List<float> Assessment = new List<float>();
        public List<int> room = new List<int>();
        public int summ;
        public int summ2;
        public int Index;
    }
}
