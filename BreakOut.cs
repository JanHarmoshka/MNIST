﻿using System;
using System.Collections.Generic;

namespace MNIST
{
    class BreakOut
    {
        private readonly int[] boll = { 5, 1 };
        public byte bollСoordinate = 5;
        private byte bollСoordinateBuf = 5;
        private readonly List<int[]> trail = new List<int[]>();
        private readonly int[] ballСhange = { 0, 1 };
        private readonly int[,] Word = new int[11, 11];
        private byte bas = 5;
        private byte basBuf = 5;
        private byte woll = 0;

        private System.Random rnd = new Random();

        public byte[,] MoveGame(int X)
        {
            byte[,] pixels = new byte[28, 28];
            if (woll == 0)//Начало новой игры
            {
                bas = 5;
                woll = 22;
                trail.Clear(); //стираю след мяча
                boll[0] = 5;
                boll[1] = 1;
                ballСhange[0] = 0;
                ballСhange[1] = 1;
                for (int i = 0; i < 11; i++)
                {
                    Word[i, 6] = 1;
                    Word[i, 9] = 1;
                }
                for (int i = 0; i < 3; i++)
                {
                    Word[i * 3 + 2, 7] = 1;
                    Word[i * 3 + 1, 8] = 1;
                }
            }
            else
            {
                bas = (byte)X;
                if (bas < 2 || bas > 8)
                {
                    bas = basBuf;
                }
                basBuf = bas;

                if (boll[0] + ballСhange[0] > 10 || boll[0] + ballСhange[0] < 0)//касание стенок 
                {
                    ballСhange[0] *= -1;
                }
                if (boll[1] + ballСhange[1] > 10 || boll[1] + ballСhange[1] < 0 || Word[boll[0], boll[1]] == 1)//касание крыши
                {
                    ballСhange[1] *= -1;
                }

                if (Word[boll[0], boll[1]] == 1) // контакт с блокоми
                {
                    Word[boll[0], boll[1]] = 0;
                    woll--;
                }

                boll[0] = ballСhange[0] + boll[0];//смена траектории от касания
                boll[1] = ballСhange[1] + boll[1];
                trail.Add((int[])boll.Clone());
                if (trail.Count > 15)
                {
                    trail.RemoveAt(0);
                }

                if (boll[1] == 0)//промах
                {
                    woll = 0;
                }
                if (boll[1] == 1)// контакт с батутом
                {
                    int Сhange = boll[0] - bas;
                    if (Сhange == -1 || Сhange == 1)
                    {
                        if (ballСhange[0] > 0)
                        {
                            ballСhange[0] = 1;
                        }
                        else
                        {
                            ballСhange[0] = -1;
                        }

                        ballСhange[1] = 1;
                    }
                    else if (Сhange == 0)
                    {
                        if (ballСhange[0] > 0)
                        {
                            ballСhange[0] = 1;
                        }
                        else
                        {
                            ballСhange[0] = -1;
                        };
                        ballСhange[1] = 1;
                    }
                }
            }

            bollСoordinate = (byte)boll[0];
            if (bollСoordinate < 2 || bollСoordinate > 8)
            {
                bollСoordinate = bollСoordinateBuf;
            }
            bollСoordinateBuf = bollСoordinate;



            byte a = (byte)rnd.Next(2);
            byte b = (byte)rnd.Next(2);

            // Вывод ситуации в игре
            for (int i = 0; i < 11; i++)
            {
                for (int j = 6; j < 11; j++)
                {
                    if (Word[i, j] == 1)
                    {
                        //pixels[i * 2 + 3, j * 2 + 3] = 1;
                        pixels[i * 2 + 3 + a, j * 2 + 4 + b] = 1;
                        pixels[i * 2 + 4 + a, j * 2 + 3 + b] = 1;
                        //pixels[i * 2 + 4, j * 2 + 4] = 1;
                    }
                }
            }

            for (int i = 0; i < trail.Count; i++) // траектория мяча
            {
                pixels[trail[i][0] * 2 + 3 + a, trail[i][1] * 2 + 3 + b] = 1;//мячь
                //pixels[trail[i][0] * 2 + 3 + a, trail[i][1] * 2 + 4 + b] = 1;
                //pixels[trail[i][0] * 2 + 4 + a, trail[i][1] * 2 + 3 + b] = 1;
                pixels[trail[i][0] * 2 + 4 + a, trail[i][1] * 2 + 4 + b] = 1;
            }

            pixels[bas * 2 + 3 + a, 2 + b] = 1;//платформа
            pixels[bas * 2 + 3 + a, 4 + b] = 1;
            pixels[bas * 2 + 4 + a, 2 + b] = 1;
            pixels[bas * 2 + 4 + a, 4 + b] = 1;
            pixels[(bas - 1) * 2 + 3 + a, 2 + b] = 1;
            pixels[(bas - 1) * 2 + 3 + a, 4 + b] = 1;
            pixels[(bas - 1) * 2 + 4 + a, 2 + b] = 1;
            pixels[(bas - 1) * 2 + 4 + a, 4 + b] = 1;
            pixels[(bas + 1) * 2 + 3 + a, 2 + b] = 1;
            pixels[(bas + 1) * 2 + 3 + a, 4 + b] = 1;
            pixels[(bas + 1) * 2 + 4 + a, 2 + b] = 1;
            pixels[(bas + 1) * 2 + 4 + a, 4 + b] = 1;

            return pixels;
        }
    }
}