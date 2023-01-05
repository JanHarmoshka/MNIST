using System;
using System.Collections.Generic;
using System.Linq;

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
        public int mattepPositive;
        public int matteNegative;
        public Single summ;

        public Matte(List<float> InputData, ushort Room, float satiety = 0.4f, bool elect_ = false)
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
            mattepPositive = 0;
            matteNegative = 0;
            Contraction = false;
            Contraction_ = false;
            Single max = 0;
            Single max_0;
            float matteVar;

            for (int j = 0; j < mask.Count; j++)
            {
                if (max < mask[j])
                    max = mask[j];
            }
            max_0 = max;

            max /= 2.0f;
            summ = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                matteNegative++;
                matte[j] = mask[j] - max;
                if (matte[j] > 0)
                {
                    summ += matte[j];
                    mattepPositive++;
                    matteNegative--;
                }
            }

            Single summ_0 = 0;
            for (int j = 0; j < mask.Count; j++)
            {
                matteVar = matte[j] / summ;
                if (matteVar <= -0.01f || matteVar >= 0.01f)
                {
                    matte[j] = matteVar;
                    summ_0 += matte[j];
                }
                else
                {
                    matte[j] = 0;
                }
            }
            if (max_0 < 10)
            {
                if (summ_0 < 0.3f)
                {
                    if (Control_value > 0)//&& !elect
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
            this.Sleep();
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
            this.Sleep();
        }
    }

    /// Результирующая маска
    [Serializable]
    public class ReverseMatte
    {
        public bool room;
        public int Live;
        public float Control_value;
        public bool Contraction;
        public bool Contraction_;
        public float appeal_ = 0.1f;
        public float participation;
        readonly List<float> mask = new List<float>();
        public List<float> matte = new List<float>();
        public List<float> Refined = new List<float>();
        public List<float> Correct = new List<float>();
        private Single max;
        public int mattepPositive;
        public int matteNegative;
        public Single summ;
        public Single summCorrect;

        public ReverseMatte(List<float> InputData, int IndexData, List<float> inter_result, int nn, float satiety = 0.3f)
        {

            room = false;

            participation = 0;
            Live = nn;
            for (int i = 0; i < InputData.Count; i++)
            {
                mask.Add(InputData[i] * 0.1f);
                matte.Add(-0f);
                if (max < mask.Last())
                    max = mask.Last();
            }
            for (int i = 0; i < inter_result.Count; i++)
            {
                Refined.Add(inter_result[i] * 0.1f);
                Correct.Add(-0.5f);
            }
            Control_value = 100;
            this.Sleep();
            Contraction = true;
        }

        /// Обучение результирующей маски.
        /// Это ситуация дообучения результирующей маски, на текущем
        /// представлении о событии которое ассоциировано с группой.
        public void Lesson(List<Single> InputData, List<float> inter_result, bool leader)
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

            if (inter_result.Count > 0)
            {
                for (int j = 1; j < inter_result.Count; j++)
                {
                    if (inter_result[j] > 0.0f)
                    {
                        Refined[j] = Refined[j] + inter_result[j];
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
                        if (max < mask[j])
                            max = mask[j];
                    }
                }
            }
            this.Sleep();
        }
        public void Lesson(List<Single> InputData, List<float> inter_result)
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
            if (inter_result.Count > 0)
            {
                for (int j = 1; j < inter_result.Count; j++)
                {
                    if (inter_result[j] > 0.0f)
                    {
                        Refined[j] = Refined[j] + inter_result[j];
                    }
                }
            }

            this.Sleep();
        }
        public void Lesson(List<Single> InputData, List<float> inter_result, Single res)
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
            if (inter_result.Count > 0)
            {
                for (int j = 1; j < inter_result.Count; j++)
                {
                    if (inter_result[j] > 0.0f)
                    {
                        Refined[j] = Refined[j] + inter_result[j] * res;//
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
            float matteVar;
            float calculatedMax = 0;
            summ = 0;
            summCorrect = 0;
            mattepPositive = 0;
            matteNegative = 0;

            calculatedMax = max / 2.0f;
  
            for (int j = 0; j < mask.Count; j++)
            {
                matteNegative++;
                matte[j] = mask[j] - calculatedMax;
                if (matte[j] > 0)
                {
                    summ += matte[j];
                    matteNegative--;
                    mattepPositive++;
                }
            }

            for (int j = 0; j < mask.Count; j++)
            {
                matteVar = matte[j] / summ;
                if (matteVar <= -0.01f || matteVar >= 0.01f)
                {
                    matte[j] = matteVar;
                }
                else
                {
                    matte[j] = 0;
                }
            }

            calculatedMax = 0.01f;
            for (int j = 1; j < Refined.Count; j++)
            {
                if (Refined[j] > calculatedMax)
                    calculatedMax = Refined[j];
            }

            for (int j = 1; j < Refined.Count; j++)
            {
                if (Refined[j] != 0)
                {
                    Correct[j] = Refined[j] / calculatedMax;
                    summCorrect += Correct[j];
                }
                else
                {
                    Correct[j] = 0;
                }
            }

            if ((summCorrect / Correct.Count < 1.1f || summCorrect / Correct.Count > 1.1f) && Control_value > 0 && appeal_ < 0.99f)
            {
                Contraction_ = true;
            }
        }
    }

    public class Counter
    {
        public float str1;
        public bool pos;
        public List<byte> inter_Data_ = new List<byte>();
        public List<byte> inter_Data_Full = new List<byte>();
        public List<float> Assessment = new List<float>();
        public List<int> room = new List<int>();
        public List<int> room2 = new List<int>();
        public int summ2;
        public string message;
    }
}
