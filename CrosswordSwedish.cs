using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BAK
{
    class CrosswordSwedish : Crossword
    {
        bool isFinished = false;
        Semaphore sema = new Semaphore(1, 1);

        public CrosswordSwedish(int x, int y) : base(x, y)
        {
        }

        public override void Generate()
        {
            //Tajenka();
            crossword[0, 0] = "X";
            FillBorder(crossword, new List<Word>());

            zapisVysledek(crossword);
            Print();
        }

        public void Tajenka()
        {
            if (width > 10)
            {
                //WordWrite(crossword, new Word("TAJENKABE", ""), 0, 3, true);
            }
        }


        int test = 0;
        int pocetNesplnitelnychCest = 0;
        void FillBorder(string[,] currentCs, List<Word> currentUsedWords)
        {
            PrintCs(currentCs);
            if (isFinished) return;
            if (OneLetterWordInStart(currentCs)) return;
            if (!WordCanGoFromHere(currentCs)) return;
            if (BorderIsFull(currentCs))
            {
                if (EndsWith1LetterWord(currentCs)) return;
                PrintCs(currentCs);
                FillInside(currentCs, currentUsedWords, 1, 1);
                return;
            }

            (int, int, bool) t = StartingCellBorder(currentCs);
            int x = t.Item1;
            int y = t.Item2;
            bool horizontalDirection = t.Item3;
            if (DeadEnd(currentCs, x, y) || DeadEndInner(currentCs, x, y))
            {
                test++;
                Console.WriteLine(test);
                PrintCs(currentCs);
                return;
            }

            string[] containedLetters = ContainedLetters(currentCs, x, y, horizontalDirection);
            if (containedLetters.Length < 2)
            {
                if (currentCs[x - 1, y] == "7" && currentCs[x, y - 1] == "7")
                {
                    currentCs[x, y] = "7";
                    FillBorder(currentCs, currentUsedWords);
                }
                return;
            }

            List<Word> possibleWords;
            if (x == 1 || y == 1)
            {
                possibleWords = dictionary.SelectWords(currentUsedWords, containedLetters);
            }
            else if (y == height - 1 || x == width - 1)
            {
                possibleWords = dictionary.SelectWords(currentUsedWords, containedLetters);//, MaxLength(currentCs, x, y, horizontalDirection) /*- 1*/);
            }
            else if (y == 2 || x == 2)
            {
                int length;
                int length1stWord;
                if (horizontalDirection)
                {
                    length = width - 1;
                    length1stWord = MaxLength(currentCs, x, y - 1, horizontalDirection);
                }
                else
                {
                    length = height - 1;
                    length1stWord = MaxLength(currentCs, x - 1, y, horizontalDirection);
                }
                possibleWords = dictionary.SelectWords(currentUsedWords, containedLetters, length, length1stWord);
            }
            else
            {
                possibleWords = dictionary.SelectWords(currentUsedWords, containedLetters);
            }

            foreach (Word word in possibleWords)
            {
                string[,] csClone = (string[,])currentCs.Clone();
                List<Word> usedWordsClone = currentUsedWords.ToList();
                if (!CanPlaceBorder(csClone, word, x, y, horizontalDirection)) continue;

                csClone = WordWrite(csClone, word, x, y, horizontalDirection);
                usedWordsClone.Add(word);

                if (x == width - 2 || y == height - 2)
                {
                    Prevent1LetterWordsNearEnd(csClone, horizontalDirection);
                }
                if (x == 1 || x == 2 || y == 1 || y == 2)
                {
                    (string[,], List<Word>) tuple = PutWordInside(csClone, usedWordsClone, x, y);
                    csClone = tuple.Item1;
                    usedWordsClone = tuple.Item2;
                }
                PrintCs(csClone);
                FillBorder(csClone, usedWordsClone);

            }
            if (possibleWords.Count == 0)
            {
                if (!DeadEndAlreadyFound(currentCs, x, y, horizontalDirection)) //kontrola, že to ještě není uložené na impossiblePathsList
                {
                    pocetNesplnitelnychCest++;
                    containedLetters = GetMinimalImposibilePath(containedLetters);
                    impossiblePathsList[x][y].Add((containedLetters, horizontalDirection));
                }
            }
        }

        void FillInside(string[,] currentCs, List<Word> currentUsedWords, int x, int y)
        {
            if (isFinished) return;
            if (IsFinished(currentCs))
            {
                PrintCs(currentCs);
                currentCs = CompleteCsClues(currentCs, currentUsedWords);
                PrintCs(currentCs);
                if (!IsEverythingAlright(currentCs))
                {
                    return;
                }

                isFinished = true;
                Console.WriteLine(currentCs[0, 4].Length);
                crossword = (string[,])currentCs.Clone();

                return;
            }
            currentCs = (string[,])GetCsReady(currentCs).Clone();
            PrintCs(currentCs);
            if (!WordCanGoFromHere(currentCs)) return;

            if (DeadEndInner(currentCs, x, y)) return;
            (int, int, bool) t = StartingCellInside(currentCs, x, y);
            x = t.Item1;
            y = t.Item2;
            if (x <= 0) //vlastně by to nikdy nemělo nastat
            {
            }
            bool horizontalDirection = t.Item3;
            string[] containedLetters = ContainedLetters(currentCs, x, y, horizontalDirection);
            List<Word> possibleWords = dictionary.SelectWords(currentUsedWords, containedLetters);
            foreach (Word word in possibleWords)
            {
                string[,] csClone = (string[,])currentCs.Clone();
                List<Word> usedWordsClone = currentUsedWords.ToList();
                WordWrite(csClone, word, x, y, horizontalDirection);
                usedWordsClone.Add(word);
                //word.Print();
                FillInside(csClone, usedWordsClone, x, y);
            }

            if (possibleWords.Count == 0)
            {
                if (!DeadEndAlreadyFoundInner(currentCs, x, y, horizontalDirection))
                {
                    PrintCs(currentCs);
                    //containedLetters = GetMinimumImposibile(containedLetters);
                    List<string> impossiblePatterns = FindShortestImpossibleMatches(containedLetters);
                    foreach (string pattern in impossiblePatterns)
                    {
                        impossiblePathsList[x][y].Add((pattern.ToCharArray().Select(c => c.ToString()).ToArray(), horizontalDirection));
                        pocetNesplnitelnychCest++;
                    }
                }
                return;
            }
        }

        public (int x, int y, bool horizontalDirection) StartingCellBorder(string[,] cs) //není trochu divná?
        {
            int min = Math.Min(width, height);
            for (int i = 1; i < min; i++)
            {
                if (cs[i, 0] == " " && !IsClue(cs[i, 1])) return (i, 0, false);
                if (cs[0, i] == " " && !IsClue(cs[1, i])) return (0, i, true);
            }
            if (width >= height)
            {
                for (int i = min; i < width; i++)
                {
                    if (cs[i, 0] == " " && !IsClue(cs[i, 1])) return (i, 0, false);

                }
            }
            else
            {
                for (int i = min; i < width; i++)
                {
                    if (cs[0, i] == " " && !IsClue(cs[1, i])) return (0, i, true);
                }
            }
            return (-1, -1, false);
        }

        public bool BorderIsFull(string[,] cs)
        {
            return (IsClue(cs[width - 1, 0]) || IsClue(cs[width - 1, 1])) && (IsClue(cs[0, height - 1]) || IsClue(cs[1, height - 1])) &&
                (IsClue(cs[width - 2, 0]) || IsClue(cs[width - 2, 1])) && (IsClue(cs[0, height - 2]) || IsClue(cs[1, height - 2]));
        }

        public (int x, int y, bool horintalDirection) StartingCellInside(string[,] cs, int x, int y)
        {
            for (int j = y; j < height; j++)
            {
                for (int i = (j == y ? x : 1); i < width; i++)
                {
                    if (cs[i, j].Contains("7"))
                    {
                        if (cs[i, j] == "7/7") return (i, j, true);
                        if (cs[i, j].Contains("/7")) return (i, j, false);
                        if (cs[i, j].Contains("7")) return (i, j, true);
                    }
                }
            }
            return (-1, -1, false);
        }


        public int MaxLength(string[,] cs, int x, int y, bool horintalDirection)
        {
            int i = 1;

            if (horintalDirection)
            {
                while (x + i < width && !cs[x + i, y].Contains("7") && !cs[x + i, y].Contains("clue"))
                {
                    i += 1;
                }
            }
            else
            {
                while (y + i < height && !cs[x, y + i].Contains("7") && !cs[x, y + i].Contains("clue"))
                {
                    i += 1;
                }
            }
            return i - 1;
        }

        public string[] ContainedLetters(string[,] cs, int x, int y, bool horintalDirection)
        {
            string[] pismena;
            int i = 0;
            Regex regex = new Regex(@"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]");
            if (horintalDirection)
            {
                x++;
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
                    i += 1;
                }
            }
            else
            {
                y++;
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
                    i += 1;
                }
            }
            return pismena;
        }


        public string[,] WordWrite(string[,] cs, Word word, int x, int y, bool horizontalDirection)
        {
            if (!BorderIsFull(cs))
            {
                ClueWriteBorder(cs, word, x, y, horizontalDirection);
            }
            else
            {
                ClueWriteInner(cs, word, x, y, horizontalDirection);
            }
            int n = word.word.Length;
            int i;
            if (horizontalDirection)
            {
                x += 1; // legenda
                for (i = 0; i < n; i += 1)
                {
                    cs[x + i, y] = word.word[i].ToString();
                }
                if (x + i < width && cs[x + i, y] == " ")
                {
                    cs[x + i, y] = "7";
                }
            }
            else
            {
                y += 1; //legenda
                for (i = 0; i < n; i += 1)
                {
                    cs[x, y + i] = word.word[i].ToString();
                }
                if (y + i < height && cs[x, y + i] == " ")
                {
                    cs[x, y + i] = "7";
                }
            }
            return cs;
        }

        public void ClueWriteBorder(string[,] cs, Word slovo, int x, int y, bool horizontalDirection)
        {

            if (cs[x, y].Equals(" "))
            {
                if (x == 0 || y == 0)
                {
                    cs[x, y] = "7";
                }
                else if (x == 1 && cs[0, y] != "7")
                {
                    cs[x, y] = "7";
                }
                else if (y == 1 && cs[x, 0] != "7")
                {
                    cs[x, y] = "7";
                }
                else if (x != 0 && y != 0)
                {
                    cs[x, y] = "clue";
                }
                else
                {
                    PrintCs(cs);
                }
            }
            else if (cs[x, y] == "7" && x != 0 && y != 0)
            {
                cs[x, y] = "clue";
            }
            else
            {
                PrintCs(cs);
            }


            /*if (cs[x, y].Equals(" "))
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    cs[x, y] = "7"; 
                }
                else if (x < width - 1 && !cs[x + 1, y].Contains("7") && y < height - 1 && !cs[x, y + 1].Contains("7"))
                {
                    cs[x, y] = "7";
                }
                else cs[x, y] = "7";
            }
            else if (cs[x, y].Equals("7/7")) //může to vůbec nastat?
            {
                if (horizontalDirection)
                {
                    cs[x, y] = "clue/7";
                }
                else
                {
                    cs[x, y] = "7/clue";
                }
            }
            else if (cs[x, y].Equals("7"))
            {
                cs[x, y] = "clue";
            }
            else
            {
            }*/
        }

        public void ClueWriteInner(string[,] cs, Word slovo, int x, int y, bool horizontalDirection)
        {
            if (cs[x, y].Equals(" "))
            {
                cs[x, y] = "clue";

            }
            else if (cs[x, y].Equals("7/7"))
            {
                if (horizontalDirection)
                {
                    cs[x, y] = "clue/7";
                }
                else
                {
                    cs[x, y] = "7/clue";
                }
            }
            else if (cs[x, y].Contains("7"))
            {
                cs[x, y] = cs[x, y].Replace("7", "clue");
            }
            else
            {
            }

        }



        string[,] GetCsReady(string[,] cs)
        {
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (!IsClue(cs[x, y]) || cs[x, y].Contains("clue/clue")) continue;

                    if (cs[x, y].Contains("clue"))
                    {
                        if (x == 1 || x == 2)
                        {
                            if (!IsClue(cs[x + 1, y]))
                            {
                                cs[x, y] = "7/clue";
                            }
                            else
                            {
                                cs[x, y] = "/clue";
                            }
                        }
                        else if ((y == 1 || y == 2) && !IsClue(cs[x, y + 1]) && !cs[x, y].Contains("7"))
                        {
                            cs[x, y] += "/7";
                        }

                    }
                    else if (cs[x, y] == "7")
                    {
                        if (x == width - 1)
                        {
                            cs[x, y] = "/7";
                        }
                        else if (y < height - 1 && x < width - 1 && !IsClue(cs[x, y + 1]) && !IsClue(cs[x + 1, y]))
                        {
                            cs[x, y] += "/7";
                        }
                        else if (y < height - 1 && x < width - 1 && IsClue(cs[x + 1, y]) && !IsClue(cs[x, y + 1]))
                        {
                            cs[x, y] = "/7";
                        }
                    }
                    /*else if (cs[x, y] == "7/7")
                    {
                        if (x + 1 == width || (x + 1 < width && IsClue(cs[x + 1, y])))
                        {
                            cs[x, y] = "/7";
                        }
                        else if (x + 1 == width || y < height - 1 && !IsClue(cs[x, y + 1]))
                        {
                            cs[x, y] = "7";
                        }
                        else
                        {

                        }
                    }*/
                }
            }
            return cs;
        }


        string[] GetMinimalImposibilePath(string[] containedLetters)
        {
            int i = 0;
            string letters = "";
            while (i < containedLetters.Length && containedLetters[i] != "_")
            {
                letters += containedLetters[i];
                i++;
            }
            while (letters.Length > 1 && dictionary.ImpossibleToSelect(letters.Substring(0, letters.Length - 1)))
            {
                if (letters.Length < 2)
                {
                    break;
                }
                letters = letters.Substring(0, letters.Length - 1);
            }
            return letters.Select(c => c.ToString()).ToArray();
        }



        bool DeadEnd(string[,] cs, int x, int y)
        {
            string[] containedLetters;
            string[] currentContainedLetters;
            for (int j = Math.Max(1, y); j < height; j++)
            {
                currentContainedLetters = ContainedLetters(cs, 0, j, true);
                if (currentContainedLetters[0] == "_") break;
                currentContainedLetters = GetMinimalImposibilePath(currentContainedLetters);
                for (int i = impossiblePathsList[0][j].Count - 1; i >= 0; i--)
                {
                    containedLetters = impossiblePathsList[0][j][i].containedLetters;
                    if (containedLetters.SequenceEqual(currentContainedLetters))
                    {
                        return true;
                    }
                }

            }
            for (int i = Math.Max(1, x); i < width; i++)
            {
                currentContainedLetters = ContainedLetters(cs, i, 0, false);
                if (currentContainedLetters[0] == "_") break;
                currentContainedLetters = GetMinimalImposibilePath(currentContainedLetters);
                for (int j = impossiblePathsList[i][0].Count - 1; j >= 0; j--)
                {
                    containedLetters = impossiblePathsList[i][0][j].containedLetters;
                    if (containedLetters.SequenceEqual(currentContainedLetters))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool DeadEndInner(string[,] cs, int x, int y) //NENÍ OTESTOVANÉ POŘÁDNĚ
        {
            string[] containedLetters;
            string[] currentContainedLettersHor;
            string[] currentContainedLettersVer;
            bool horizontalDirection;

            for (int j = 1; j < height; j++)
            {
                for (int i = 1; i < width; i++)
                {
                    if (x > i && y > j) continue;
                    currentContainedLettersHor = ContainedLetters(cs, i, j, true);

                    currentContainedLettersVer = ContainedLetters(cs, i, j, false);
                    foreach ((string[] containedLetters, bool horizontalDirection) t in impossiblePathsList[i][j])
                    {
                        containedLetters = t.Item1;
                        horizontalDirection = t.Item2;
                        if (horizontalDirection && Match(currentContainedLettersHor, containedLetters)) //tyhle podmínky vyměnit za to, jestli jsou containedletters podmnožina currectcontainedLetters
                        {
                            Console.WriteLine(string.Join("", currentContainedLettersHor) + " " + string.Join("", containedLetters));
                            return true;
                        }

                        else if (!horizontalDirection && Match(currentContainedLettersVer, containedLetters))
                        {
                            Console.WriteLine(string.Join("", currentContainedLettersVer) + " " + string.Join("", containedLetters));
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        public List<string> FindShortestImpossibleMatches(string[] containedLetters)
        {
            string pattern = string.Join("", containedLetters);
            if (Match(pattern))
            {
                return new List<string>();
            }
            List<string> shortestMatches = new List<string>();
            for (int len = pattern.Length - 1; len > 0; len--)
            {
                StringBuilder sb = new StringBuilder(pattern);
                for (int i = len; i < pattern.Length; i++)
                {
                    sb[i] = '_';
                    string newPattern = sb.ToString();

                    if (!Match(newPattern))
                    {
                        shortestMatches.Add(newPattern);
                    }
                    else
                    {
                        
                        System.Console.WriteLine("Pattern = " + pattern + " a jeho počet vyskytů: " + shortestMatches.Count);
                        foreach (string pat in shortestMatches)
                        {
                            System.Console.WriteLine(pat);
                        }
                        return shortestMatches;
                    }
                }
            }
            return shortestMatches;  // Nenalezli jsme žádný odpovídající vzor
        }
        public bool Match(string containedLetters)
        {
            string regexPattern = containedLetters.Replace("_", ".");

            // Přidání hranic slov pro přesné vyhledání slova
            regexPattern = @"\b" + regexPattern + @"\b";

            return dictionary.dictionary.AsParallel().Any(w => Regex.IsMatch(w.word, regexPattern));
        }

        public bool Match(string[] currentContainedLetters, string[] containedLetters)
        {
            string ccl = string.Join("", currentContainedLetters);
            string cl = string.Join("", containedLetters);

            ccl = ccl.Replace("_", ".");
            cl = cl.Replace("_", ".");
            return Regex.IsMatch(ccl, cl);
        }



        bool DeadEndAlreadyFound(string[,] cs, int x, int y, bool horizontalDirection)
        {
            string[] currentContainedLetters = ContainedLetters(cs, x, y, horizontalDirection);
            currentContainedLetters = GetMinimalImposibilePath(currentContainedLetters);

            foreach ((string[] containedLetters, bool horizontalDirection) t in impossiblePathsList[x][y])
            {
                if (t.Item2 != horizontalDirection)
                {
                    continue;
                }
                string[] containedLetters = t.Item1;
                if (containedLetters.SequenceEqual(currentContainedLetters))
                {
                    return true;
                }
            }
            return false;
        }

        bool DeadEndAlreadyFoundInner(string[,] cs, int x, int y, bool horizontalDirection)
        {
            string[] currentContainedLetters = ContainedLetters(cs, x, y, horizontalDirection);
            currentContainedLetters = GetMinimalImposibilePath(currentContainedLetters);

            foreach ((string[] containedLetters, bool horizontalDirection) t in impossiblePathsList[x][y])
            {
                if (t.Item2 != horizontalDirection)
                {
                    continue;
                }
                string[] containedLetters = t.Item1;

                if (Match(currentContainedLetters, containedLetters))
                {
                    return true;
                }
            }
            return false;
        }

        bool WordCanGoFromHere(string[,] cs) // existuje místo pro slova o délce 1 || je trojuhelnik legend
        {
            if (IsClue(cs[width - 1, height - 1])) return false;
            
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (!IsClue(cs[x, y])) continue;

                    if (x < width - 2 && IsClue(cs[x + 2, y])) return false;
                    if (y < height - 2 && IsClue(cs[x, y + 2])) return false;
                    if (x < width - 1 && y < height - 1 && IsClue(cs[x + 1, y]) && IsClue(cs[x, y + 1])) return false;
                    if (x == width - 1 && y + 1 < height && IsClue(cs[x, y + 1])) return false;
                    if (y == height - 1 && x + 1 < width && IsClue(cs[x + 1, y])) return false;

                }
            }
            return true;
        }

        (string[,], List<Word>) PutWordInside(string[,] cs, List<Word> usedWords, int x, int y)
        {
            bool horizontalDirection = x == 0;
            (x, y) = FindClue(cs, x, y);
            if (x == -1) return (cs, usedWords);
            if (horizontalDirection)
            {
                string[] containedLetters = ContainedLetters(cs, x, y, horizontalDirection);
                int length = width - x - 1;
                int length2 = length;
                if (y == 2)
                {
                    length2 = width - FindClue(cs, x, y - 1).x - 1;
                }
                List<Word> possibleWords = dictionary.SelectWords(usedWords, containedLetters, length, length2);
                foreach (Word word in possibleWords)
                {
                    if (!CanPlaceBorder(cs, word, x, y, horizontalDirection)) continue;
                    if (cs[x + word.word.Length, y - 1].Contains("7")) continue;
                    WordWrite(cs, word, x, y, horizontalDirection);
                    usedWords.Add(word);
                    return (cs, usedWords);
                }
            }
            else
            {
                string[] containedLetters = ContainedLetters(cs, x, y, horizontalDirection);
                int length = height - y - 1;
                int length2 = length;
                if (x == 2)
                {
                    length2 = height - FindClue(cs, x, y - 1).y - 1;
                }
                List<Word> possibleWords = dictionary.SelectWords(usedWords, containedLetters, length, length2);
                foreach (Word word in possibleWords)
                {
                    if (!CanPlaceBorder(cs, word, x, y, horizontalDirection)) continue;
                    if (cs[x - 1, y + word.word.Length].Contains("7")) continue;
                    WordWrite(cs, word, x, y, horizontalDirection);
                    usedWords.Add(word);
                    return (cs, usedWords);
                }
            }
            return (cs, usedWords);
        }

        (int x, int y) FindClue(string[,] cs, int x, int y)
        {
            if (x == 0)
            {
                for (int i = 1; i < width; i++) //najdi si clue v tomto sloupku
                {
                    if (cs[i, y].Contains("7"))
                    {
                        return (i, y);
                    }
                }
            }
            else
            {
                for (int i = 1; i < height; i++)
                {
                    if (cs[x, i].Contains("7"))
                    {
                        return (x, i);
                    }
                }
            }
            return (-1, -1);
        }

        bool CanPlaceBorder(string[,] cs, Word word, int x, int y, bool horizontalDirection)
        {
            int length = word.word.Length;
            if ((horizontalDirection && length == width - 1) || (!horizontalDirection && length == height - 1)) return true;

            if ((horizontalDirection && length == width - 3) || ((!horizontalDirection && length == height - 3))) return false; //nesmí na konci řádku/sloupku vznikat místo pro další slovo délky 1
            if (1 + length < width && y > 1 && IsClue(cs[length + 1, y - 1]) && IsClue(cs[length + 1, y - 2])) return false; //nesmí být 3 legendy v pod sebou
            if (length + 1 < height && x > 1 && IsClue(cs[x - 1, length + 1]) && IsClue(cs[x - 2, length + 1])) return false; // nesmí být 3 legendy vedle sebe
            if (y > 1 && length + 1 == width - 1 && IsClue(cs[width - 1, y - 1])) return false; // nesmí být 2 legendy nad sebou u pravého kraje
            if (x > 1 && length + 1 == height - 1 && IsClue(cs[x - 1, height - 1])) return false; // nesmí být 2 legendy u dolního hraje vedle sebe
            if (IsClue(cs[width - 1, height - 1]) || IsClue(cs[width - 1, height - 2]) || IsClue(cs[width - 2, height - 1])) return false;
            /*
            if (horizontalDirection && cs[1 + word.word.Length, y] == "7" && cs[1 + word.word.Length, y - 1] == "7") return false;
            if (!horizontalDirection && cs[x, 1 + word.word.Length] == "7" && cs[x - 1, 1 + word.word.Length] == "7") return false;
            */
            return true;
        }

        void Prevent1LetterWordsNearEnd(string[,] cs, bool horizontalDirection)
        {
            if (horizontalDirection)
            {
                for (int i = 1; i < width; i++)
                {
                    if (IsClue(cs[i, height - 2]))
                    {
                        cs[i, height - 1] = "7";
                    }
                }

            }
            else
            {
                for (int i = 1; i < height; i++)
                {
                    if (IsClue(cs[width - 2, i]))
                    {
                        cs[width - 1, i] = "7";
                    }
                }
            }

        }

        bool OneLetterWordInStart(string[,] cs)
        {
            for (int i = 1; i < width; i++)
            {
                if ((IsClue(cs[i, 2]) && !IsClue(cs[i, 1]))) return true;
            }
            for (int i = 1; i < height; i++)
            {
                if (IsClue(cs[2, i]) && !IsClue(cs[1, i])) return true;
            }
            return false;
        }

        bool EndsWith1LetterWord(string[,] cs)
        {
            for (int i = 1; i < width; i++)
            {
                if (cs[i, height - 2] == "7" && cs[i, height - 1] != "7") return true; // nebo 7/7?
            }
            for (int i = 1; i < height; i++)
            {
                if (cs[width - 2, i] == "7" && cs[width - 1, i] != "7") return true;
            }
            if (cs[width - 1, height - 1].Contains("7")) return true;
            return false;
        }

        public bool IsFinished(string[,] cs)
        {
            for (int j = this.height - 1; j > 0; j--)
            {
                for (int i = this.width - 1; i > 0; i--)
                {
                    if (cs[i, j] == " ")
                    {
                        return false;
                    }
                    if (i > 3 && j > 3 && cs[i, j].Contains("7"))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string[,] CompleteCsClues(string[,] cs, List<Word> usedWords)
        {
            cs = CompleteCsCluesBorder(cs, usedWords);
            if (cs == null) return null;
            cs = CompleteCsCluesInside(cs, usedWords);
            return cs;
        }

        public string[,] CompleteCsCluesBorder(string[,] cs, List<Word> usedWords)
        {
            string[] containedLetters;
            Word word;
            for (int i = 1; i < width; i++)
            {

                if (IsClue(cs[i, 0]))
                {
                    containedLetters = ContainedLetters(cs, i, 0, false);
                    word = dictionary.GetRightClue(usedWords, containedLetters);
                    if (word == null) return null;
                    cs[i, 0] = word.clue;
                }

            }
            for (int i = 1; i < height; i++)
            {
                if (IsClue(cs[0, i]))
                {
                    containedLetters = ContainedLetters(cs, 0, i, true);
                    word = dictionary.GetRightClue(usedWords, containedLetters);
                    if (word == null) return null;
                    cs[0, i] = word.clue;
                }
            }
            return cs;
        }

        public string[,] CompleteCsCluesInside(string[,] cs, List<Word> usedWords)
        {
            string[] containedLetters;
            Word word;
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (IsClue(cs[x, y]))
                    {
                        if (cs[x, y].Contains("/clue"))
                        {
                            containedLetters = ContainedLetters(cs, x, y, false);
                            word = dictionary.GetRightClue(usedWords, containedLetters);
                            if (word.word.Equals(""))
                            {
                                word = dictionary.SelectWord(usedWords, containedLetters);
                                if (word == null) return null;
                            }
                            cs[x, y] = cs[x, y].Replace("/clue", "/" + word.clue);
                            x--;
                            PrintCs(cs);
                        }
                        else
                        {
                            containedLetters = ContainedLetters(cs, x, y, true);
                            word = dictionary.GetRightClue(usedWords, containedLetters);
                            if (word.word.Equals(""))
                            {
                                word = dictionary.SelectWord(usedWords, containedLetters);
                                if (word == null) return null;
                            }
                            cs[x, y] = cs[x, y].Replace("clue", word.clue);
                            PrintCs(cs);
                        }
                    }
                }
            }
            return cs;
        }

        public bool IsClue(string potentionalClue)
        {
            return potentionalClue.Contains("clue") || potentionalClue.Contains("7") /*|| potentionalClue.Length > 1*/;
        }

        bool IsEverythingAlright(string[,] cs)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x, y] == " " || IsClue(cs[x, y]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void zapisVysledek(string[,] cs)
        {

            string dir = System.Environment.CurrentDirectory;

            sema.WaitOne();
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(dir, "output.txt")))
            {

                for (int j = 0; j < height; j += 1)
                {
                    for (int i = 0; i < width; i += 1)
                    {
                        outputFile.Write(cs[i, j] + " | ");
                    }
                    outputFile.WriteLine();
                }
            }
            sema.Release();
        }

    }
}
