﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BAK
{
    class CrosswordBritish : Crossword
    {
        protected double bestScore = 0.0;
        protected double lastScore = 0.0;
        protected double minimumRequiredScore;



        public string[] cluesHorizontal;
        public string[] cluesVertical;

        public CrosswordBritish(int x, int y, bool isCzechLanguage) : base(x, y, isCzechLanguage)
        {
            SetClues();
        }

        public override void Generate()
        {
            SetMinimumRequiredScore();
            string[,] cs = new string[this.width, this.height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    cs[j, i] = " ";
                }
            }
            int index = 0;
            while (bestScore < minimumRequiredScore && index < 10)
            {
                GenerateCrossword(cs);
                index++;
            }

            Console.WriteLine("nejlepší výsledek: " + bestScore);
            Print();
        }

        public string[,] GenerateCrossword(string[,] cs)
        {
            List<Word> usedWords = new List<Word>();
            bool horizontalDirection;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        horizontalDirection = k == 0;
                        Word wordNew = dictionary.SelectWord(ContainedLetters(cs, x, y, horizontalDirection), usedWords);
                        if (!wordNew.word.Equals("") && CanPlace(cs, wordNew, x, y, horizontalDirection))
                        {
                            WordWrite(cs, wordNew, x, y, horizontalDirection);
                            usedWords.Add(wordNew);
                        }
                    }
                }
            }
            (string[,], List<Word>) t = FixCrossing(cs, usedWords);
            cs = t.Item1;
            usedWords = t.Item2;
            double score = GetScore(cs);
            if (score > bestScore)
            {
                bestScore = score;
                this.usedWords = usedWords;
                crossword = (string[,])cs.Clone();
            }
            else if (score < lastScore + 0.5)
            {
                return cs;
            }
            lastScore = score;
            Console.WriteLine(score);
            PrintCs(cs);
            return cs;
        }

        public double GetScore(string[,] cs)
        {
            double sizeRatio = width / height;
            if (height > width)
            {
                sizeRatio = height / width;
            }
            double filled = 0.0;
            double empty = 0.0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x, y] == " ")
                    {
                        empty++;
                    }
                    else
                    {
                        filled++;
                    }
                }
            }
            double filledRatio = filled / empty;
            return 10 * sizeRatio + filledRatio * 20;
        }

        public (string[,], List<Word>) FixCrossing(string[,] cs, List<Word> usedWords)
        {
            //PrintCs(cs);
            bool changed = false;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x, y] != " ")
                    {
                        if (x > 0 && cs[x - 1, y] != " ")
                        {
                            continue;
                        }
                        else if (y > 0 && cs[x, y - 1] != " ")
                        {
                            continue;
                        }
                        bool horizontalDirection = false;
                        if (x < width - 1 && cs[x + 1, y] != " ")
                        {
                            horizontalDirection = true;
                        }
                        if (!IsCrossed(cs, x, y, horizontalDirection))
                        {
                            (string[,], List<Word>) t = DeleteWord(cs, x, y, horizontalDirection, usedWords);
                            cs = t.Item1;
                            usedWords = t.Item2;
                            changed = true;
                        }
                    }
                }
            }
           // PrintCs(cs);
            if (changed)
            {
                double score = GetScore(cs);
                Console.WriteLine(score);
                if (score < minimumRequiredScore && score < lastScore - 0.5)
                {
                    return (cs, usedWords);
                }
                if (score < minimumRequiredScore && width * height > 9)
                {
                    cs = GenerateCrossword(cs);
                }
            }
            return (cs, usedWords);
        }

        public string[] ContainedLetters(string[,] cs, int x, int y, bool horintalDirection)
        {
            string[] pismena;
            int i = 0;
            Regex regex = new Regex(regexString);
            if (horintalDirection)
            {
                pismena = new string[width - x];
                while (x + i < width)
                {
                    if (regex.IsMatch(cs[x + i, y]) && cs[x + i, y].Length <= 1)
                    {
                        pismena[i] = cs[x + i, y];
                    }
                    else if (cs[x + i, y] == " ")
                    {
                        pismena[i] = "_";
                    }
                    else
                    {
                        break;
                    }
                    i += 1;
                }
            }
            else
            {
                pismena = new string[height - y];
                while (y + i < height)
                {
                    if (regex.IsMatch(cs[x, y + i]) && cs[x, y + i].Length <= 1)
                    {
                        pismena[i] = cs[x, y + i];
                    }
                    else if (cs[x, y + i] == " ")
                    {
                        pismena[i] = "_";
                    }
                    else
                    {
                        break;
                    }
                    i += 1;
                }
            }
            return pismena;
        }

        public void WordWrite(string[,] cs, Word word, int x, int y, bool horintalDirection)
        {
            ClueWrite(word, x, y, horintalDirection);
            int n = word.word.Length;
            int i;
            if (horintalDirection)
            {
                for (i = 0; i < n; i += 1)
                {
                    cs[x + i, y] = word.word[i].ToString();
                }
            }
            else
            {
                for (i = 0; i < n; i += 1)
                {
                    cs[x, y + i] = word.word[i].ToString();
                }
            }
        }


        public bool CanPlace(string[,] cs, Word word, int x, int y, bool horintalDirection)
        {
            int n = word.word.Length;
            if (horintalDirection)
            {
                if (x > 0 && cs[x - 1, y] != " ") return false;
                if (x + n < width && cs[x + n, y] != " ") return false;
            }
            else
            {
                if (y > 0 && cs[x, y - 1] != " ") return false;
                if (y + n < height && cs[x, y + n] != " ") return false;
            }

            for (int i = 0; i < n; i++)
            {
                if (horintalDirection)
                {
                    if (cs[x + i, y] != " ") continue;
                    if (y > 0 && cs[x + i, y - 1] != " " && cs[x + i, y] == " ")
                    {
                        return false;
                    }
                    if (y + 1 < height && cs[x + i, y + 1] != " " && cs[x + i, y] == " ")
                    {
                        return false;
                    }
                }
                else
                {
                    if (cs[x, y + i] != " ") continue;
                    if (x > 0 && cs[x - 1, y + i] != " " && cs[x, y + i] == " ")
                    {
                        return false;
                    }
                    if (x + 1 < width && cs[x + 1, y + i] != " " && cs[x, y + i] == " ")
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public bool IsCrossed(string[,] cs, int x, int y, bool horizontalDirection)
        {
            int i = 0;
            if (horizontalDirection)
            {
                while (x + i < width && cs[x + i, y] != " ")
                {
                    if (y > 0 && cs[x + i, y - 1] != " ")
                    {
                        return true;
                    }
                    if (y + 1 < height && cs[x + i, y + 1] != " ")
                    {
                        return true;
                    }
                    i++;
                }

                i = 0;
                while (x - i >= 0 && cs[x - i, y] != " ")
                {
                    if (y > 0 && cs[x - i, y - 1] != " ")
                    {
                        return true;
                    }
                    if (y + 1 < height && cs[x - i, y + 1] != " ")
                    {
                        return true;
                    }
                    i++;
                }
            }
            else
            {
                i = 0;
                while (y + i < height && cs[x, y + i] != " ")
                {
                    if (x > 0 && cs[x - 1, y + i] != " ")
                    {
                        return true;
                    }
                    if (x + 1 < width && cs[x + 1, y + i] != " ")
                    {
                        return true;
                    }
                    i++;
                }
                i = 0;
                while (y - i >= 0 && cs[x, y - i] != " ")
                {
                    if (x > 0 && cs[x - 1, y - i] != " ")
                    {
                        return true;
                    }
                    if (x + 1 < width && cs[x + 1, y - i] != " ")
                    {
                        return true;
                    }
                    i++;
                }
            }
            return false;
        }

        public (string[,], List<Word>) DeleteWord(string[,] cs, int x, int y, bool horizontalDirection, List<Word> usedWords)
        {
            int i = 0;
            Word word = FindWord(x, y, horizontalDirection);
            if (horizontalDirection)
            {
                while (x + i < width && cs[x + i, y] != " ")
                {
                    cs[x + i, y] = " ";
                    i++;
                }
            }
            else
            {
                while (y + i < height && cs[x, y + i] != " ")
                {
                    cs[x, y + i] = " ";
                    i++;
                }
            }
            if (!word.word.Equals(""))
            {
                usedWords.Remove(word);
            }
            return (cs, usedWords);
        }

        void SetMinimumRequiredScore()
        {
            if (width * height < 100)
            {
                minimumRequiredScore = 35;
            }
            else if (width * height < 300)
            {
                minimumRequiredScore = 29;
            }
            else
            {
                minimumRequiredScore = 25;
            }
        }

        void SetClues()
        {
            (List<Word>, List<Word>) t = GetCluesInRightDirectionAndOrder();
            List<Word> clueHorizontal = t.Item1;
            List<Word> clueVertical = t.Item2;
            Console.WriteLine("Legenda");
            Console.WriteLine("Vodorovná: ");
            cluesHorizontal = new string[clueHorizontal.Count];
            for (int i = 0; i < clueHorizontal.Count; i++)
            {
                cluesHorizontal[i] = clueHorizontal[i].clue;
                Console.WriteLine(cluesHorizontal[i]);
            }
            Console.WriteLine("Svislá: ");
            cluesVertical = new string[clueVertical.Count];
            for (int i = 0; i < clueVertical.Count; i++)
            {
                cluesVertical[i] = clueVertical[i].clue;
                Console.WriteLine(cluesVertical[i]);
            }
        }

        public (List<Word>, List<Word>) GetCluesInRightDirectionAndOrder()
        {
            List<Word> cluesHorizontal = new List<Word>();
            List<Word> cluesVertical = new List<Word>();
            bool horizontalDir = true;
            Word word;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int d = 0; d < 2; d++)
                    {
                        horizontalDir = !horizontalDir;
                        word = FindWord(i, j, horizontalDir);
                        if (word.word.Equals("")) continue;
                        if (horizontalDir)
                        {
                            cluesHorizontal.Add(word);
                        }
                        else
                        {
                            cluesVertical.Add(word);
                        }
                    }
                }
            }
            return (cluesHorizontal, cluesVertical);
        }

        public Word FindWord(int x, int y, bool horizontalDir)
        {
            if (crossword[x, y] == " ") return new Word("", "");
            string letters = "";
            if (horizontalDir)
            {
                while (x < width && crossword[x, y] != " ")
                {
                    letters += crossword[x, y];
                    x++;
                }
            }
            else
            {
                while (y < height && crossword[x, y] != " ")
                {
                    letters += crossword[x, y];
                    y++;
                }

            }
            Word word = dictionary.GetRightClue(this.usedWords, letters.ToCharArray().Select(c => c.ToString()).ToArray());
            return word;
        }
    }
}
