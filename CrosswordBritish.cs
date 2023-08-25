using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BAK
{
    class CrosswordBritish : Crossword
    {
        WordComparer comparer = new WordComparer();
        double bestScore = 0.0;
        double minimumRequiredScore = 22;

        public CrosswordBritish(int x, int y) : base(x, y)
        {
            PrintClues();
        }

        public override void Generate()
        {
            string[,] cs = new string[this.width, this.height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    cs[j, i] = " ";
                }
            }

            while (bestScore < minimumRequiredScore)
            {
                GenerateCrossword(cs);
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
            Console.WriteLine("fixing crossing");
            PrintCs(cs);
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
            if (changed)
            {
                double score = GetScore(cs);
                Console.WriteLine(score);
                if (score < minimumRequiredScore && width * height > 9)
                {
                    (string[,], List<Word>) t = FixBigSpaces(cs, usedWords);
                    cs = t.Item1;
                    usedWords = t.Item2;
                    PrintCs(cs);
                    cs = GenerateCrossword(cs);
                }
            }
            return (cs, usedWords);
        }

        (string[,], List<Word>) FixBigSpaces(string[,] cs, List<Word> usedWords)
        {
            // Console.WriteLine("Big Spaces");
            //PrintCs(cs);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    (int, int, int) t = ContainsBigSpace(cs, x, y);
                    if (t.Item1 > -1)
                    {
                        x = t.Item1;
                        y = t.Item2;
                        int length = t.Item3;
                        if (length > 4)
                        {
                            Console.WriteLine(x + " " + y);
                            bool horizontalDirection = true;
                            string[] containedLetters = ContainedLetters(cs, x, y, horizontalDirection);
                            Word wordNew = dictionary.SelectWord(containedLetters, usedWords, length);
                            if (!wordNew.word.Equals("") && CanPlace(cs, wordNew, x, y, horizontalDirection))
                            {
                                WordWrite(cs, wordNew, x, y, horizontalDirection);
                                usedWords.Add(wordNew);
                                return (cs, usedWords);
                            }
                        }
                    }
                }
            }

            return (cs, usedWords);
        }


        (int, int, int) ContainsBigSpace(string[,] cs, int x, int y)
        {
            int bigSpaceX = 3;
            int bigSpaceY = 3;
            for (; y < height - bigSpaceY; y++)
            {
                for (; x < width - bigSpaceX; x++)
                {
                    if (cs[x, y] != " ") continue;
                    bool containsBigSpace = true;
                    for (int i = 1; i <= bigSpaceX; i++)
                    {
                        if (!(y < height - 2 && cs[x + i, y] == " " && cs[x + i, y + 1] == " " && cs[x + i, y + 2] == " "))
                        {
                            containsBigSpace = false;
                            break;
                        }
                    }
                    if (containsBigSpace)
                    {

                        return (x, y, BigSpaceLength(cs, x, y));
                    }
                }
            }
            return (-1, -1, -1);
        }


        int BigSpaceLength(string[,] cs, int x, int y)
        {
            int i = 0;
            while (x + i < height && cs[x + i, y] == " ")
            {
                i++;
            }
            return i;
        }

       
        public string[] ContainedLetters(string[,] cs, int x, int y, bool horintalDirection)
        {
            string[] pismena;
            int i = 0;
            Regex regex = new Regex(@"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]");
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
                    else if (cs[x + i, y].Contains("7") || cs[x + i, y].Contains("clue"))
                    {
                        break;
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
                    else if (cs[x, y + i].Contains("7") || cs[x, y + i].Contains("clue"))
                    {
                        break;
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

        public override void ClueWrite(Word word, int x, int y, bool horintalDirection)
        {
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


        public void PrintClues()
        {
            for (int i = 0; i < this.usedWords.Count; i++)
            {
                this.usedWords[i].Print();
            }
            

            var t = GetCluesInRightDirection();
            List<Word> clueHorizontal = t.Item1;
            List<Word> clueVertical = t.Item2;
            Console.WriteLine("Legenda");
            Console.WriteLine("Vodorovná: ");
            for (int i = 0; i < clueHorizontal.Count; i++)
            {
                Console.WriteLine(clueHorizontal[i].clue);
            }
            Console.WriteLine("Svislá: ");
            for (int i = 0; i < clueVertical.Count; i++)
            {
                Console.WriteLine(clueVertical[i].clue);
            }
        }

        public (List<Word>, List<Word>) GetCluesInRightDirection()
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

        public (int, int) CountWords()
        {
            bool horizontalDir = true;
            Word word;
            int countHorizontal = 0;
            int countVertical = 0;
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
                            countHorizontal++;
                        }
                        else
                        {
                            countVertical++;
                        }
                    }
                }
            }
            return (countHorizontal, countHorizontal);
        }



        public bool ContainsDira()
        {
            if (height < 10 || width < 10) return false;
            return (Dirka(1, 1) || Dirka(width - width / 3, height - width / 3) || Dirka(width / 2, height / 2) || Dirka(1, height - 6) || Dirka(width - 6, 1));
        }

        public bool Dirka(int x, int y)
        {
            for (int j = y; j < y + height / 3; j++)
            {
                for (int i = x; i < x + height / 3; i++)
                {
                    if (crossword[i, j] != " ")
                    {
                        Console.WriteLine(i + " " + j);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
