using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class CrosswordSwedish : Crossword
    {
        bool isFinished = false;
        string randomChar = "#"; //místo toho bude nakonec nápověda
        string clueSymbol = "7";


        public CrosswordSwedish(int x, int y) : base(x, y)
        {
        }

        public override void Generate()
        {
            Tajenka();
            // PrepareCrossword();
            Print();
            crossword[0, 0] = randomChar;
            FillBorder(crossword, new List<Word>());

            //zapisVysledek(crossword);
            Print();
        }

        public void Tajenka()
        {
            if (width > 10)
            {
                WordWrite(crossword, new Word("TAJENKA", ""), 0, 3, true);
            }
        }


        int deadEnded = 0;
        int pocetNesplnitelnychCest = 0; //test only
        void FillBorder(string[,] currentCs, List<Word> currentUsedWords)
        {
            PrintCs(currentCs);
            if (isFinished) return;
            //if (OneLetterWordInStart(currentCs)) return;
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
            if (DeadEndBorder(currentCs, x, y) || DeadEndInner(currentCs, x, y))
            {
                deadEnded++;
                PrintCs(currentCs);
                return;
            }

            string[] containedLetters = ContainedLetters(currentCs, x, y, horizontalDirection);
            if (containedLetters.Length < 2)
            {
                if (currentCs[x - 1, y] == clueSymbol && currentCs[x, y - 1] == clueSymbol)
                {
                    currentCs[x, y] = clueSymbol;
                    FillBorder(currentCs, currentUsedWords);
                }
                return;
            }

            List<Word> possibleWords;
            if (y == 2 || x == 2)
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
                if (!CanPlace(csClone, word, x, y, horizontalDirection)) continue;

                csClone = WordWrite(csClone, word, x, y, horizontalDirection);
                usedWordsClone.Add(word);

                if (x == width - 2 || y == height - 2)
                {
                    PreventOneLetterWordsNearEnd(csClone, horizontalDirection);
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
            PrintCs(currentCs);
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
                zapisVysledek(currentCs);
                return;
            }
            currentCs = (string[,])GetCsReady(currentCs).Clone();
            PrintCs(currentCs);
            if (!WordCanGoFromHere(currentCs)) return;

            if (DeadEndInner(currentCs, x, y))
            {
                deadEnded++;
                return;
            }
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


        public (int x, int y, bool horizontalDirection) StartingCellBorder(string[,] cs)
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
                for (int i = min; i < height; i++)
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
                    if (cs[i, j].Contains(clueSymbol))
                    {
                        if (cs[i, j] == clueSymbol + "/" + clueSymbol) return (i, j, true);
                        if (cs[i, j].Contains("/" + clueSymbol)) return (i, j, false);
                        if (cs[i, j].Contains(clueSymbol)) return (i, j, true);
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
                while (x + i < width && !cs[x + i, y].Contains(clueSymbol) && !cs[x + i, y].Contains("clue"))
                {
                    i += 1;
                }
            }
            else
            {
                while (y + i < height && !cs[x, y + i].Contains(clueSymbol) && !cs[x, y + i].Contains("clue"))
                {
                    i += 1;
                }
            }
            return i - 1;
        }

        public string[] ContainedLetters(string[,] cs, int x, int y, bool horintalDirection)
        {
            string[] pismena;
            //StringBuilder pismena = new StringBuilder();
            int i = 0;
            Regex regex = new Regex(@"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]");
            if (horintalDirection)
            {
                x++;
                pismena = new string[GetMaxWordLength(cs, x, y, horintalDirection)];
                while (x + i < width && !IsClue(cs[x + i, y]))
                {
                    if (regex.IsMatch(cs[x + i, y]) && cs[x + i, y].Length <= 1)
                    {
                        //pismena.Append(cs[x + i, y]);
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
                pismena = new string[GetMaxWordLength(cs, x, y, horintalDirection)];
                while (y + i < height && !IsClue(cs[x, y + i]))
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

        public int GetMaxWordLength(string[,] cs, int x, int y, bool horizontalDirection)
        {
            int i = 0;
            if (horizontalDirection)
            {
                while (x + i < width && !IsClue(cs[x + i, y]))
                {
                    i++;
                }

            }
            else
            {
                while (y + i < height && !IsClue(cs[x, y + i]))
                {
                    i++;
                }
            }
            Console.WriteLine(i);
            return i;
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
                    cs[x + i, y] = clueSymbol;
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
                    cs[x, y + i] = clueSymbol;
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
                    cs[x, y] = clueSymbol;
                }
                else if (x == 1 && cs[0, y] != clueSymbol)
                {
                    cs[x, y] = clueSymbol;
                }
                else if (y == 1 && cs[x, 0] != clueSymbol)
                {
                    cs[x, y] = clueSymbol;
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
            else if (cs[x, y] == clueSymbol && x != 0 && y != 0)
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
                    cs[x, y] = clueSymbol; 
                }
                else if (x < width - 1 && !cs[x + 1, y].Contains(clueSymbol) && y < height - 1 && !cs[x, y + 1].Contains(clueSymbol))
                {
                    cs[x, y] = clueSymbol;
                }
                else cs[x, y] = clueSymbol;
            }
            else if (cs[x, y].Equals(clueSymbol + "/" + clueSymbol)) //může to vůbec nastat?
            {
                if (horizontalDirection)
                {
                    cs[x, y] = "clue/" + clueSymbol;
                }
                else
                {
                    cs[x, y] = clueSymbol + "/clue";
                }
            }
            else if (cs[x, y].Equals(clueSymbol))
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
            else if (cs[x, y].Equals(clueSymbol + "/" + clueSymbol))
            {
                if (horizontalDirection)
                {
                    cs[x, y] = "clue/" + clueSymbol;
                }
                else
                {
                    cs[x, y] = clueSymbol + "/clue";
                }
            }
            else if (cs[x, y].Contains(clueSymbol))
            {
                cs[x, y] = cs[x, y].Replace(clueSymbol, "clue");
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
                                cs[x, y] = clueSymbol + "/clue";
                            }
                            else
                            {
                                cs[x, y] = "/clue";
                            }
                        }
                        else if ((y == 1 || y == 2) && !IsClue(cs[x, y + 1]))
                        {
                            cs[x, y] += "/" + clueSymbol;
                        }

                    }
                    else if (cs[x, y] == clueSymbol)
                    {
                        if (x == width - 1)
                        {
                            cs[x, y] = "/" + clueSymbol;
                        }
                        else if (y < height - 1 && x < width - 1 && !IsClue(cs[x, y + 1]) && !IsClue(cs[x + 1, y]))
                        {
                            cs[x, y] = clueSymbol + "/" + clueSymbol;
                        }
                        else if (y < height - 1 && x < width - 1 && IsClue(cs[x + 1, y]) && !IsClue(cs[x, y + 1]))
                        {
                            cs[x, y] = "/" + clueSymbol;
                        }
                    }
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



        bool DeadEndBorder(string[,] cs, int x, int y)
        {
            string[] containedLetters;
            string[] currentContainedLetters;
            for (int j = Math.Max(1, y); j < height; j++)
            {
                if (impossiblePathsList[0][j].Count == 0) continue;
                currentContainedLetters = ContainedLetters(cs, 0, j, true);
                if (currentContainedLetters.Length == 0) continue;
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
                if (impossiblePathsList[i][0].Count == 0) continue;
                currentContainedLetters = ContainedLetters(cs, i, 0, false);
                if (currentContainedLetters.Length == 0) continue;
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
                    if (impossiblePathsList[i][j].Count == 0) continue;
                    if (x > i && y > j) continue;
                    currentContainedLettersHor = ContainedLetters(cs, i, j, true);

                    currentContainedLettersVer = ContainedLetters(cs, i, j, false);
                    foreach ((string[] containedLetters, bool horizontalDirection) t in impossiblePathsList[i][j])
                    {
                        containedLetters = t.Item1;
                        horizontalDirection = t.Item2;
                        if (horizontalDirection && Match(currentContainedLettersHor, containedLetters))
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
            shortestMatches.Add(string.Join("", containedLetters));//todo

            for (int len = pattern.Length - 1; len > 0; len--)
            {
                StringBuilder sb = new StringBuilder(pattern);
                for (int i = len; i < pattern.Length; i++)
                {
                    sb[i] = '_';
                    string newPattern = sb.ToString();

                    if (!Match(newPattern))
                    {
                        Console.WriteLine("Patter: " + newPattern);
                        shortestMatches.Add(newPattern);
                    }
                    else
                    {
                        System.Console.WriteLine("Počet podpaternů: " + shortestMatches.Count);
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

            return dictionary.wordsList.AsParallel().Any(w => Regex.IsMatch(w.word, regexPattern));
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

        bool WordCanGoFromHere(string[,] cs) // neexistuje místo pro slova o délce 1 || je trojuhelnik legend
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
                    if (!CanPlace(cs, word, x, y, horizontalDirection)) continue;
                    if (cs[x + word.word.Length, y - 1].Contains(clueSymbol)) continue;
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
                    if (!CanPlace(cs, word, x, y, horizontalDirection)) continue;
                    if (cs[x - 1, y + word.word.Length].Contains(clueSymbol)) continue;
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
                for (int i = 1; i < width; i++) //najdi si clue v tomto sloupci
                {
                    if (cs[i, y].Contains(clueSymbol))
                    {
                        return (i, y);
                    }
                }
            }
            else
            {
                for (int i = 1; i < height; i++)
                {
                    if (cs[x, i].Contains(clueSymbol))
                    {
                        return (x, i);
                    }
                }
            }
            return (-1, -1);
        }

        bool CanPlace(string[,] cs, Word word, int x, int y, bool horizontalDirection)
        {
            int length = word.word.Length;
            if ((horizontalDirection && length == width - 3) || ((!horizontalDirection && length == height - 3))) return false; //nesmí na konci řádku/sloupku vznikat místo pro další slovo délky 1
            //if (1 + length < width && y > 1 && IsClue(cs[length + 1, y - 1]) && IsClue(cs[length + 1, y - 2])) return false; //todo chci je tady nebo ne?  //nesmí být 3 legendy v pod sebou
            //if (length + 1 < height && x > 1 && IsClue(cs[x - 1, length + 1]) && IsClue(cs[x - 2, length + 1])) return false; // nesmí být 3 legendy vedle sebe
            if (y > 1 && length + 1 == width - 1 && IsClue(cs[width - 1, y - 1])) return false; // nesmí být 2 legendy nad sebou u pravého kraje
            if (x > 1 && length + 1 == height - 1 && IsClue(cs[x - 1, height - 1])) return false; // nesmí být 2 legendy u dolního hraje vedle sebe
            if (IsClue(cs[width - 1, height - 1]) || IsClue(cs[width - 1, height - 2]) || IsClue(cs[width - 2, height - 1])) return false;
            /*
            if (horizontalDirection && cs[1 + word.word.Length, y] == clueSymbol && cs[1 + word.word.Length, y - 1] == clueSymbol) return false;
            if (!horizontalDirection && cs[x, 1 + word.word.Length] == clueSymbol && cs[x - 1, 1 + word.word.Length] == clueSymbol) return false;
            */
            return true;
        }

        void PreventOneLetterWordsNearEnd(string[,] cs, bool horizontalDirection)
        {
            if (horizontalDirection)
            {
                for (int i = 1; i < width; i++)
                {
                    if (IsClue(cs[i, height - 2]))
                    {
                        cs[i, height - 1] = clueSymbol;
                    }
                }

            }
            else
            {
                for (int i = 1; i < height; i++)
                {
                    if (IsClue(cs[width - 2, i]))
                    {
                        cs[width - 1, i] = clueSymbol;
                    }
                }
            }

        }

        bool EndsWith1LetterWord(string[,] cs)
        {
            for (int i = 1; i < width; i++)
            {
                if (cs[i, height - 2] == clueSymbol && cs[i, height - 1] != clueSymbol) return true; // nebo 7/7?
            }
            for (int i = 1; i < height; i++)
            {
                if (cs[width - 2, i] == clueSymbol && cs[width - 1, i] != clueSymbol) return true;
            }
            if (cs[width - 1, height - 1].Contains(clueSymbol)) return true;
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
                    if (i > 0 && j > 0 && cs[i, j].Contains(clueSymbol))
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
                    if (cs[x, y].Contains("clue") || cs[x, y].Contains(clueSymbol))
                    {
                        if (cs[x, y].Contains("/clue"))
                        {
                            containedLetters = ContainedLetters(cs, x, y, false);
                            word = dictionary.GetRightClue(usedWords, containedLetters);
                            if (word.word.Equals(""))
                            {
                                word = dictionary.SelectWord(usedWords, containedLetters, cs, x, y, false);
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
                                word = dictionary.SelectWord(usedWords, containedLetters, cs, x, y, true);
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
            return potentionalClue.Length > 1 || potentionalClue.Contains("clue") || potentionalClue.Contains(clueSymbol) || potentionalClue.Contains("/");
        }

        bool IsEverythingAlright(string[,] cs)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((x * y != 0 && cs[x, y] == " ") || cs[x, y].Contains(clueSymbol) || cs[x, y].Contains("clue"))
                    {
                        Console.WriteLine(x + " " + y);
                        return false;
                    }
                }
            }
            return true;
        }

        public void zapisVysledek(string[,] cs) //test 
        {

            string dir = System.Environment.CurrentDirectory;
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

        }

    }
}
