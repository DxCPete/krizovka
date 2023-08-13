using System;
using System.Collections.Generic;
using System.Linq;

namespace BAK
{
    class CrosswordBritish : Crossword
    {
        public List<Word> wordsWritten { get; set; } = new List<Word>();
        int countHorizontal = 0;
        int countVertical = 0;
        public WordComparer comparer { get; } = new WordComparer();


        public CrosswordBritish(int x, int y) : base(x, y)
        {
            clueHoritonzal = new Word[y * 3];
            clueVertical = new Word[x * 3];
            PrintClues();
        }

        public override int MaxLength(int x, int y, bool horintalDirection)
        {
            int i = 0;
            if (horintalDirection)
            {
                while (x + i < width /*&& crossword[x + i, y].Length <= 1*/)
                {
                    i += 1;
                }
            }
            else
            {
                while (y + i < height/* && crossword[x, y + i].Length <= 1*/)
                {
                    i += 1;
                }
            }
            return i-1;
        }

        public override string[] ContainedLetters(int x, int y, bool horintalDirection, int maxDelka)
        {
            return base.ContainedLetters(x, y, horintalDirection, maxDelka);
        }

        public void WordWrite(Word word, int x, int y, bool horintalDirection)
        {
            ClueWrite(word, x, y, horintalDirection);
            int n = word.word.Length;
            int i;
            if (horintalDirection)
            {
                for (i = 0; i < n; i += 1)
                {
                    crossword[x + i, y] = word.word[i].ToString();
                }
            }
            else
            {
                for (i = 0; i < n; i += 1)
                {
                    crossword[x, y + i] = word.word[i].ToString();
                }
            }
            dictionary.Remove(word);
        }

        public override void ClueWrite(Word word, int x, int y, bool horintalDirection)
        {
            wordsWritten.Add(word);
        }

        public bool CanPlace(Word word, int x, int y, bool horintalDirection)
        {
            int n = word.word.Length;
            if (horintalDirection)
            {
                if (x > 0 && crossword[x - 1, y] != " ") return false;
                if (x + n < width && crossword[x + n, y] != " ") return false;

            }
            else
            {
                if (y > 0 && crossword[x, y - 1] != " ") return false;
                if (y + n < height && crossword[x, y + n] != " ") return false;
            }

            for (int i = 0; i < n; i++)//tady i = 0 bylo
            {
                if (horintalDirection)
                {
                    if (crossword[x + i, y] != " ") continue;
                    if (y > 0 && crossword[x + i, y - 1] != " " && crossword[x + i, y] == " ")
                    {
                        return false;
                    }
                    if (y + 1 < height && crossword[x + i, y + 1] != " " && crossword[x + i, y] == " ")
                    {
                        return false;
                    }


                }
                else
                {
                    if (crossword[x, y + i] != " ") continue;
                    if (x > 0 && crossword[x - 1, y + i] != " " && crossword[x, y + i] == " ")
                    {
                        return false;
                    }
                    if (x + 1 < width && crossword[x + 1, y + i] != " " && crossword[x, y + i] == " ")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        int index = 0;
        public override void Generate()
        {
            Console.WriteLine(dictionary.Length());
            Random r = new Random();
            bool horintalDirection = r.Next() % 2 == 0;
            int maxLength;
            Word wordNew;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        horintalDirection = !horintalDirection;
                        maxLength = MaxLength(x, y, horintalDirection);
                        wordNew = dictionary.SelectWord(maxLength, ContainedLetters(x, y, horintalDirection, maxLength)); //SelectWord(maxLength, ContainedLetters(x, y, horintalDirection, maxLength));
                        if (!wordNew.word.Equals("") && CanPlace(wordNew, x, y, horintalDirection))
                        {
                            WordWrite(wordNew, x, y, horintalDirection);
                        }
                    }
                }
            }
            Print();
            Console.WriteLine("Průchod: " + index++);
            Fix();
        }

        public void Fix()
        {
            bool horintalDirection = true;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        horintalDirection = !horintalDirection;
                        if (crossword[x, y] != " ")
                        {
                            /*if (horintalDirection && x + 1 < width && crossword[x + 1, y] == " ") continue;
                            if (!horintalDirection && y + 1 < height && crossword[x, y + 1] == " ") continue;
                            */
                            if (!IsCrossed(x, y, horintalDirection))
                            {
                                DeleteWord(x, y, horintalDirection); //tady byla deleteword
                               /* if (!AddWord(x, y))
                                {
                                   
                                }*/
                            }
                        }
                    }

                }
            }
            (int, int) t = CountWords();
            //if (t.Item1 < height * 0.6 || t.Item2 < width * 0.6 /*|| ContainsDira()|| changed*/) Generate();
        }

        public bool AddWord(int x, int y)
        {

            bool horizontalDirection = (x + 1 < width && crossword[x + 1, y] != " ");
            if (horizontalDirection)
            {
                int i = 0;
                bool dir = !horizontalDirection;
                while (x + i < width && crossword[x + i, y] != " ")
                {
                    int maxLength = MaxLength(x, y, dir);
                    Word slovoNove = dictionary.SelectWord(maxLength, ContainedLetters(x, y, horizontalDirection, maxLength)); ;
                    if (CanPlace(slovoNove, x, y, dir))
                    {
                        WordWrite(slovoNove, x + i, y, dir);
                        return true;
                    }
                    i++;
                }
            }
            else
            {
                int i = 0;
                bool dir = !horizontalDirection;
                while (y + i < height && crossword[x, y + i] != " ")
                {
                    int maxLength = MaxLength(x, y, dir);
                    Word slovoNove = dictionary.SelectWord(maxLength, ContainedLetters(x, y, horizontalDirection, maxLength));//SelectWord(maxDelka, ContainedLetters(x, y, dir, maxDelka));
                    if (CanPlace(slovoNove, x, y, dir))
                    {
                        WordWrite(slovoNove, x, y + i, dir);
                        return true;
                    }
                    i++;

                }
            }
            return false;
        }


        public bool IsCrossed(int x, int y, bool horintalDirection)
        {
            int i = 0;
            if (horintalDirection)
            {
                while (x + i < width && crossword[x + i, y] != " ")
                {
                    if (y > 0 && crossword[x + i, y - 1] != " ")
                    {
                        return true;
                    }
                    if (y + 1 < height && crossword[x + i, y + 1] != " ")
                    {
                        return true;
                    }
                    i++;
                }
                i = 0;
                while (x - i >= 0 && crossword[x - i, y] != " ")
                {
                    if (y > 0 && crossword[x - i, y - 1] != " ")
                    {
                        return true;
                    }
                    if (y + 1 < height && crossword[x - i, y + 1] != " ")
                    {
                        return true;
                    }
                    i++;
                }

            }
            else
            {
                while (y + i < height && crossword[x, y + i] != " ")
                {
                    if (x > 0 && crossword[x - 1, y + i] != " ")
                    {
                        return true;
                    }
                    if (x + 1 < width && crossword[x + 1, y + i] != " ")
                    {
                        return true;
                    }
                    i++;
                }
                i = 0;
                while (y - i >= 0 && crossword[x, y - i] != " ")
                {
                    if (x > 0 && crossword[x - 1, y - i] != " ")
                    {
                        return true;
                    }
                    if (x + 1 < width && crossword[x + 1, y - i] != " ")
                    {
                        return true;
                    }
                    i++;
                }
            }
            return false;
        }

        public void DeleteWord(int x, int y, bool horintalDirection)
        {
            int i = 0;
            Word word = FindWord(x, y, horintalDirection);
            if (horintalDirection)
            {
                while (x + i < width && crossword[x + i, y] != " ")
                {
                    crossword[x + i, y] = " ";
                    i++;
                }
            }
            else
            {
                while (y + i < height && crossword[x, y + i] != " ")
                {
                    crossword[x, y + i] = " ";
                    i++;
                }
            }
            if (!word.word.Equals(""))
            {
                dictionary.Add(word);
                wordsWritten.Remove(word);
            }
        }


        public void PrintClues()
        {
            CheckClues();
            Console.WriteLine("Legenda");
            Console.WriteLine("Vodorovná: ");
            for (int i = 0; i < countHorizontal; i++)
            {
                Console.WriteLine(clueHoritonzal[i].clue);
            }
            Console.WriteLine("Svislá: ");
            for (int i = 0; i < countVertical; i++)
            {
                Console.WriteLine(clueVertical[i].clue);
            }

        }

        public void CheckClues()
        {
            Console.WriteLine();
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
                            clueHoritonzal[countHorizontal] = word;
                            countHorizontal++;
                        }
                        else
                        {
                            clueVertical[countVertical] = word;
                            countVertical++;
                        }
                    }
                }
            }
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
            Word r = new Word(letters, "");
            List<Word> wordsFilltered = wordsWritten.Where(s => comparer.Equals(s, r)).ToList();
            if (wordsFilltered.Count == 0)
            {
                return new Word("", "");
            }
            return wordsFilltered[0];
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
            for (int j = y ; j < y + height/3; j++)
            {
                for (int i = x; i < x + height/3; i++)
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
