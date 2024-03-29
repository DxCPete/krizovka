using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BAK
{
    class CrosswordSw : Crossword
    {
        string clueSymbol = "7";
        string clue = "clue";
        string[] cs;
        Stack<(string[], List<Word>)> stack = new Stack<(string[], List<Word>)>();

        int longestWordLength = 0;
        int pocetNesplnitelnychCest = 0;

        (int, int, bool) lastDeadEndedCoordinates = (0, 0, false);

        public CrosswordSw(int x, int y) : base(x, y)
        {
        }


        public override void Generate()
        {
            cs = new string[width * height];
            do
            {
                InitCrosswordContraints();
                longestWordLength = LongestPossibleWord();
            } while (!CrosswordContraintsComplied() || longestWordLength > 8);

            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y += 1)
            {
                for (int x = 0; x < width; x += 1)
                {
                    if (cs[x * height + y].Contains("7"))
                    {
                        sb.Append(clueSymbol + " | ");

                    }
                    else
                    {
                        sb.Append(cs[x * height + y] + " | ");
                    }
                }

                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
            Console.WriteLine(LongestPossibleWord());
            dictionary = new Dictionary(longestWordLength);
            FillWithWords();
            To2DArray();
        }


        int counterTest = 0;
        int ukoncenoNaDeadEnd = 0;
        void FillWithWords()
        {
            stack.Push((cs, new List<Word>()));
            int x;
            int y;
            bool horizontalDirection;

            while (stack.Count > 0)
            {
                (string[], List<Word>) st = stack.Pop();
                string[] currentCs = st.Item1;
                PrintCs(currentCs);
                List<Word> usedWords = st.Item2;
                (int, int, bool) coordinates = NextCoordinates(currentCs, usedWords.Count);
                x = coordinates.Item1;
                if (x == -1)
                {
                    PismenaSedi(currentCs, usedWords);
                    cs = currentCs;
                    return;
                }
                y = coordinates.Item2;
                horizontalDirection = coordinates.Item3;
                if (DeadEnd(currentCs, x, y))
                {
                    ukoncenoNaDeadEnd++;
                    continue;
                }
                string[] containedLetters = ContainedLetters(currentCs, x, y, horizontalDirection);
                if(containedLetters.Length > longestWordLength)//TEST ONLY
                {

                }
                List<Word> possibleWords = dictionary.SelectWordsNew(usedWords, containedLetters);
                foreach (Word word in possibleWords)
                {
                    string[] csClone = (string[])currentCs.Clone();
                    List<Word> usedWordsClone = usedWords.ToList();
                    counterTest++;
                    WriteWord(csClone, word, x, y, horizontalDirection);
                    usedWordsClone.Add(word);
                    PrintCs(csClone);
                    stack.Push((csClone, usedWordsClone));
                }
                if (possibleWords.Count == 0)
                {
                    if (!DeadEndAlreadyFound(currentCs, x, y, horizontalDirection))
                    {
                        pocetNesplnitelnychCest++;
                        containedLetters = GetMinimalImposibilePath(containedLetters);
                        impossiblePathsList[x][y].Add((containedLetters, horizontalDirection));

                    }
                }
            }
            Console.WriteLine("Fuck");
        }

        (int, int, bool) NextCoordinates(string[] cs, int number) //most contrained 
        {
            int bestX = -1;
            int bestY = -1;
            bool horDir = false;
            int mostConstrains = -1;
            bool horizontalDirection;
            int count;
            int lastDeadEndIndex = lastDeadEndedCoordinates.Item1 * height + lastDeadEndedCoordinates.Item2;
            if (cs[lastDeadEndIndex].Contains(clueSymbol))
            {
                if (!lastDeadEndedCoordinates.Item3 && cs[lastDeadEndIndex].Contains("/" + clueSymbol))
                {
                    (int, int, bool) temp = (lastDeadEndedCoordinates.Item1, lastDeadEndedCoordinates.Item2, lastDeadEndedCoordinates.Item3);
                    // lastDeadEndedCoordinates = (0, 0, false);
                    return temp;
                }
                if (lastDeadEndedCoordinates.Item3 && cs[lastDeadEndIndex].Contains(clueSymbol + "/") || cs[lastDeadEndIndex] == clueSymbol)
                {
                    (int, int, bool) temp = (lastDeadEndedCoordinates.Item1, lastDeadEndedCoordinates.Item2, lastDeadEndedCoordinates.Item3);
                    //lastDeadEndedCoordinates = (0, 0, false);
                    return temp;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if (cs[x * height + y].Contains("/" + clueSymbol))
                    {
                        horizontalDirection = false;
                        count = ConstraintsCount(cs, x, y, horizontalDirection);
                        if (count > mostConstrains)
                        {
                            bestX = x; bestY = y;
                            horDir = horizontalDirection;
                            mostConstrains = count;
                        }
                    }
                    if (cs[x * height + y] == clueSymbol || cs[x * height + y].Contains(clueSymbol + "/"))
                    {
                        horizontalDirection = true;
                        count = ConstraintsCount(cs, x, y, horizontalDirection);
                        if (count > mostConstrains)
                        {
                            bestX = x; bestY = y;
                            horDir = horizontalDirection;
                            mostConstrains = count;
                        }
                    }
                }
            }
            return (bestX, bestY, horDir);
        }

        int ConstraintsCount(string[] cs, int x, int y, bool horizontalDirection)
        {
            string[] containedLetters = ContainedLetters(cs, x, y, horizontalDirection);
            int count = 0;
            for (int i = 0; i < containedLetters.Length; i++)
            {
                if (Char.IsLetter(char.Parse(containedLetters[i])))
                {
                    count++;
                }
            }
            return count;
        }


        bool IsFinished(string[] cs)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x * height + y] == emptyField || cs[x * height + y].Contains(clueSymbol))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        string[] WriteWord(string[] cs, Word word, int x, int y, bool horizontalDirection)
        {
            string[] wordLetters = word.word.Select(c => c.ToString()).ToArray();
            WriteClue(cs, word, x, y, horizontalDirection);
            if (horizontalDirection)
            {
                for (int i = 0; i < wordLetters.Length; i++)
                {
                    cs[(x + i + 1) * height + y] = wordLetters[i];
                }
            }
            else
            {
                for (int i = 0; i < wordLetters.Length; i++)
                {
                    cs[x * height + y + i + 1] = wordLetters[i];
                }
            }

            return cs;
        }

        string[] WriteClue(string[] cs, Word word, int x, int y, bool horizontalDirection)
        {
            if (horizontalDirection)
            {
                if (cs[x * height + y].Contains("/"))
                {
                    cs[x * height + y] = cs[x * height + y].Replace(clueSymbol + "/", clue + "/");
                }
                else
                {
                    cs[x * height + y] = cs[x * height + y].Replace(clueSymbol, clue);
                }
            }
            else
            {
                cs[x * height + y] = cs[x * height + y].Replace("/" + clueSymbol, "/" + clue);
            }
            PrintCs(cs);
            return cs;
        }


        (int, int, bool) FindWordStart(string[] cs, Word word)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if (cs[x * height + y].Contains("/" + clueSymbol))
                    {
                        if (CanPlace(cs, word, x, y, false))
                        {
                            return (x, y, false);
                        }
                    }
                    else if (cs[x * height + y].Contains(clueSymbol))
                    {
                        if (CanPlace(cs, word, x, y, true))
                        {
                            return (x, y, true);
                        }
                    }
                }
            }

            return (-1, -1, false);
        }
        bool DeadEnd(string[] cs, int x, int y)
        {
            return lastDeadEnded(cs) || DeadEndBorder(cs) || DeadEndInner(cs);
        }

        bool lastDeadEnded(string[] cs)
        {
            int x = lastDeadEndedCoordinates.Item1;
            int y = lastDeadEndedCoordinates.Item2;
            bool horizontalDirection = lastDeadEndedCoordinates.Item3;
            string[] containedLetters;
            string[] currentContainedLetters = ContainedLetters(cs, x, y, horizontalDirection);

            foreach ((string[] containedLetters, bool horizontalDirection) t in impossiblePathsList[x][y])
            {
                containedLetters = t.Item1;
                horizontalDirection = t.Item2;
                if (Match(currentContainedLetters, containedLetters))
                {
                    Console.WriteLine(string.Join("", currentContainedLetters) + " " + string.Join("", containedLetters));
                    Console.WriteLine("DeadEnd: " + x + " " + y);
                    return true;
                }
            }
            return false;
        }



        bool DeadEndBorder(string[] cs)
        {
            string[] containedLetters;
            string[] currentContainedLetters;
            for (int j = 1; j < height; j++)
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
                        Console.WriteLine("DeadEnd: " + 0 + " " + j);
                        lastDeadEndedCoordinates = (0, j, true);
                        return true;
                    }
                }

            }
            for (int i = 1; i < width; i++)
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
                        Console.WriteLine("DeadEnd: " + i + " " + 0);
                        lastDeadEndedCoordinates = (i, 0, false);
                        return true;
                    }
                }
            }
            return false;
        }


        string[] GetMinimalImposibilePath(string[] containedLetters)
        {
            return containedLetters;
            /* List<string[]> list = new List<string[]>();
             GenerateCombinations(containedLetters, list, 0);
             int min = 999;
             string[] minimalLetters = containedLetters;
             int n = containedLetters.Length;
             foreach (string[] letters in list)
             {
                 int count = 0;
                 Console.WriteLine(string.Join("", letters));
                 for (int i = 0; i < n; i++)
                 {
                     if (Char.IsLetter(char.Parse(letters[i])))
                     {
                         count++;
                     }
                 }
                 if (count < min && count > 0)
                 {
                     min = count;
                     minimalLetters = letters;
                 }
             }
             return minimalLetters;*/
        }

        void GenerateCombinations(string[] containedLetters, List<string[]> list, int index)
        {
            if (!dictionary.ImpossibleToSelect(string.Join("", containedLetters)))
            {
                return;
            }
            if (index == containedLetters.Length)
            {
                list.Add((string[])containedLetters.Clone());
                return;
            }

            if (containedLetters[index] == "_")
            {
                GenerateCombinations(containedLetters, list, index + 1);
            }
            else
            {
                GenerateCombinations(containedLetters, list, index + 1);

                string original = containedLetters[index];
                containedLetters[index] = "_";
                GenerateCombinations(containedLetters, list, index + 1);
                containedLetters[index] = original;
            }
        }


        /*  string[] GetMinimalImposibilePath(string[] containedLetters)
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
        */
        bool DeadEndInner(string[] cs)
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
                    currentContainedLettersHor = ContainedLetters(cs, i, j, true);

                    currentContainedLettersVer = ContainedLetters(cs, i, j, false);
                    foreach ((string[] containedLetters, bool horizontalDirection) t in impossiblePathsList[i][j])
                    {
                        containedLetters = t.Item1;
                        horizontalDirection = t.Item2;
                        if (horizontalDirection && Match(currentContainedLettersHor, containedLetters))
                        {
                            Console.WriteLine(string.Join("", currentContainedLettersHor) + " " + string.Join("", containedLetters));
                            Console.WriteLine("DeadEnd: " + i + " " + j);
                            lastDeadEndedCoordinates = (i, j, horizontalDirection);
                            return true;
                        }

                        else if (!horizontalDirection && Match(currentContainedLettersVer, containedLetters))
                        {
                            Console.WriteLine(string.Join("", currentContainedLettersVer) + " " + string.Join("", containedLetters));
                            Console.WriteLine("DeadEnd: " + i + " " + j);
                            lastDeadEndedCoordinates = (i, j, horizontalDirection);
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

        void FindDeadEnds(string[] cs)
        {
            bool horizontalDirection;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if (cs[x * height + y].Contains("/" + clueSymbol))
                    {
                        if (!DeadEndAlreadyFound(cs, x, y, false)) //kontrola, že to ještě není uložené na impossiblePathsList
                        {
                            pocetNesplnitelnychCest++;
                            horizontalDirection = false;
                            string[] containedLetters = ContainedLetters(cs, x, y, horizontalDirection);
                            containedLetters = GetMinimalImposibilePath(containedLetters);
                            impossiblePathsList[x][y].Add((containedLetters, horizontalDirection));
                        }
                    }
                    if (cs[x * height + y] == clueSymbol || cs[x * height + y] == clueSymbol + "/" + clueSymbol)
                    {
                        if (!DeadEndAlreadyFound(cs, x, y, false)) //kontrola, že to ještě není uložené na impossiblePathsList
                        {
                            pocetNesplnitelnychCest++;
                            horizontalDirection = true;
                            string[] containedLetters = ContainedLetters(cs, x, y, horizontalDirection);
                            containedLetters = GetMinimalImposibilePath(containedLetters);
                            impossiblePathsList[x][y].Add((containedLetters, horizontalDirection));
                        }
                    }
                }
            }
        }


        bool DeadEndAlreadyFound(string[] cs, int x, int y, bool horizontalDirection)
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

        public string[] ContainedLetters(string[] cs, int x, int y, bool horintalDirection)
        {
            string[] pismena;
            //StringBuilder pismena = new StringBuilder();
            int i = 0;
            Regex regex = new Regex(@"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]");
            if (horintalDirection)
            {
                x++;
                pismena = new string[GetMaxWordLength(cs, x, y, horintalDirection)];
                while (x + i < width && !IsClue(cs[(x + i) * height + y]))
                {
                    if (regex.IsMatch(cs[(x + i) * height + y]) && cs[(x + i) * height + y].Length <= 1)
                    {
                        //pismena.Append(cs[x + i, y]);
                        pismena[i] = cs[(x + i) * height + y];
                    }
                    else if (cs[(x + i) * height + y] == " ")
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
                while (y + i < height && !IsClue(cs[x * height + y + i]))
                {
                    if (regex.IsMatch(cs[x * height + y + i]) && cs[x * height + y + i].Length <= 1)
                    {
                        pismena[i] = cs[x * height + y + i];
                    }
                    else if (cs[x * height + y + i] == " ")
                    {
                        pismena[i] = "_";
                    }
                    i += 1;
                }
            }
            return pismena;
        }
        public int GetMaxWordLength(string[] cs, int x, int y, bool horizontalDirection)
        {
            int i = 1;
            if (horizontalDirection)
            {
                while (x + i < width && !IsClue(cs[(x + i) * height + y]))
                {
                    i++;
                }

            }
            else
            {
                while (y + i < height && !IsClue(cs[x * height + y + i]))
                {
                    i++;
                }
            }
            return i;
        }

        bool CanPlace(string[] cs, Word word, int x, int y, bool horizontalDirection)
        {
            int i = 0;
            string[] wordLetters = word.word.Select(c => c.ToString()).ToArray();
            if (horizontalDirection)
            {
                while (i < wordLetters.Length && x + i + 1 < width && (wordLetters[i] == cs[(x + i + 1) * height + y] || cs[(x + i + 1) * height + y] == emptyField))
                {
                    i++;
                }
                if (x + i + 1 != width && !IsClue(cs[(x + i + 1) * height + y]))
                {
                    return false;
                }
            }
            else
            {
                while (i < wordLetters.Length && y + i + 1 < height && (wordLetters[i] == cs[x * height + y + i + 1] || cs[x * height + y + i + 1] == emptyField))
                {
                    i++;
                }
                if (y + i + 1 != height && !IsClue(cs[x * height + y + i + 1]))
                {
                    return false;
                }
            }
            //i--;
            if (i == wordLetters.Length)
            {
                return true;
            }

            return false;
        }

        int LongestPossibleWord() //nakonec se toho můžu klidně i zbavit a jet jenom podle rozměrů křížovky
        {
            int max = -1;
            PrintMainCs();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if (cs[x * height + y].Contains("/" + clueSymbol))
                    {
                        int i = 1;
                        while (y + i < height && !cs[x * height + y + i].Contains(clueSymbol))
                        {
                            i++;
                        }
                        if (i > max)
                        {
                            max = i;
                        }
                        Console.WriteLine(x + " " + y + " " + false + " " + i);
                        if (i == 1)
                        {
                            Console.WriteLine(x + " " + y); return 999;
                        }
                    }
                    else if (cs[x * height + y] == clueSymbol || cs[x * height + y].Contains(clueSymbol + "/"))
                    {
                        int i = 1;
                        while (x + i < width && !cs[(x + i) * height + y].Contains(clueSymbol))
                        {
                            i++;
                        }
                        if (i > max)
                        {
                            max = i;

                        }
                        Console.WriteLine(x + " " + y + " " + true + " " + i);
                        if (i == 1)
                        {
                            Console.WriteLine(x + " " + y); return 999;
                        }
                    }
                }
            }
            return max;
        }

        bool IsClue(string field)
        {
            return field.Contains(clueSymbol) || field.Contains(clue);
        }

        bool CrosswordContraintsComplied()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if (y != 0 && x + 2 < width && cs[(x + 2) * height + y].Contains(clueSymbol)) //existuje místo pro slovo délky 1
                    {
                        return false;
                    }
                    if (x != 0 && y + 2 < height && cs[x * height + y + 2].Contains(clueSymbol)) //existuje místo pro slovo délky 1
                    {
                        return false;
                    }
                    if ((x + 1 < width && cs[(x + 1) * height + y].Contains(clueSymbol)) &&
                        (y + 1 < height && cs[x * height + y + 1].Contains(clueSymbol)))
                    {
                        return false;
                    }
                }
            }
            //nesmí existovat slova délky 1 a legendy, ze kterých nemůže vést žádné slovo
            return true;
        }

        public void InitCrosswordContraints()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cs[x * height + y] = emptyField;
                }
            }

            cs[0] = "#"; //napověda
            WeightedRNG rng = InitRNG();
            Console.WriteLine("FirstLines");
            FirstLines(rng);
            PrintMainCs();
            if (width > 6)
            {
                Console.WriteLine("Last Lines");
                LastLines(rng);
            }
            PrintMainCs();
            Console.WriteLine("InnerPart");
            InnerPart(rng);
            PrintMainCs();
            Console.WriteLine("Finishing Touches");
            FinishingTouches();
            PrintMainCs();

        }

        void FinishingTouches()
        {
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (cs[x * height + y] != clueSymbol) continue;
                    if (x + 1 < width && y != height - 1 && cs[(x + 1) * height + y] == clueSymbol)
                    {
                        cs[x * height + y] = emptyField;
                    }
                    if (x != width - 1 && y + 1 < height && cs[x * height + y + 1] != clueSymbol)
                    {
                        cs[x * height + y] += "/" + clueSymbol;
                    }
                    if (x == width - 1)
                    {
                        cs[x * height + y] = "/" + clueSymbol;
                    }

                    if (x == width - 1 && y + 1 < height && cs[x * height + y + 1].Contains(clueSymbol))
                    {
                        if (cs[(x - 1) * height + y].Contains(clueSymbol))
                        {
                            cs[x * height + y + 1] = emptyField;
                        }
                        else
                        {
                            cs[x * height + y] = emptyField;
                        }
                    }
                    else if (y == height - 1 && x + 1 < width && cs[(x + 1) * height + y].Contains(clueSymbol))
                    {
                        if (cs[x * height + y - 1].Contains(clueSymbol))
                        {
                            cs[(x + 1) * height + y] = emptyField;
                        }
                        else
                        {
                            cs[x * height + y] = emptyField;
                        }
                    }
                }
            }
        }

        void PrintCs(string[] cs)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y += 1)
            {
                for (int x = 0; x < width; x += 1)
                {
                    sb.Append(cs[x * height + y]/*.Replace("7/7", "7").Replace("/7", "7")*/ + " | ");
                }

                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }

        public void PrintMainCs()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y += 1)
            {
                for (int x = 0; x < width; x += 1)
                {
                    sb.Append(cs[x * height + y] + " | ");
                }

                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }

        void FirstLines(WeightedRNG rng)
        {
            for (int i = 1; i < width; i++)
            {
                cs[i * height + 0] = "/" + clueSymbol;
            }

            for (int i = 1; i < height; i++)
            {
                cs[0 * height + i] = clueSymbol;

            }
            int number;
            int index = 1;
            while (index < width - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > width - 3 || number > 6 || index + number > width - 3) { }
                index += number;
                if (index > height - 3) break;
                cs[index * height + 0] = emptyField;
                cs[index * height + 1] = clueSymbol;
                cs[index * height + 2] = clueSymbol;
            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 3 || number > 6 || index + number > height - 3) { }
                if (index > height - 3) break;
                index += number;
                cs[0 * height + index] = emptyField;
                cs[1 * height + index] = clueSymbol;
                cs[2 * height + index] = clueSymbol;

            }

        }

        void InnerPart(WeightedRNG rng) //todo zasahuje do  2. řádku, což nesmí
        {
            for (int y = 3; y < height - 2; y++)
            {
                for (int x = rng.GetRandomNumber(); x < width - 2; x++)
                {
                    if (x != width - 2 && y != height - 2 && cs[x * height + y] != clueSymbol && (y - 2 >= 0 && cs[x * height + y - 2] != clueSymbol) &&
                        (x + 2 < width && cs[(x + 2) * height + y] != clueSymbol) && (x - 2 >= 0 && cs[(x - 2) * height + y] != clueSymbol))
                    {
                        cs[x * height + y] = clueSymbol;
                        x += rng.GetRandomNumber();
                    }
                }
            }

            for (int x = 3; x < width - 2; x++)
            {
                int number = rng.GetRandomNumber();
                for (int i = 0; i < height; i++)
                {
                    int y = rng.GetRandomNumber();
                    if (y < height && cs[x * height + y] != clueSymbol && cs[(x - 1) * height + y] != clueSymbol && cs[(x - 2) * height + y] != clueSymbol &&
                       (x + 1 < width && cs[(x + 1) * height + y] != clueSymbol) && (x + 2 < width && cs[(x + 2) * height + y] != clueSymbol)
                        && cs[x * height + y - 1] != clueSymbol && cs[x * height + y - 2] != clueSymbol && (y + 1 < height && cs[x * height + y + 1] != clueSymbol)
                        && (y + 2 < height && cs[x * height + y + 2] != clueSymbol))
                    {
                        cs[x * height + y] = clueSymbol;
                    }
                }


                bool hasClue = false;
                for (int y = 2; y < height; y++)
                {
                    if (cs[x * height + y] == clueSymbol)
                    {
                        hasClue = true;
                        break;
                    }
                }
                if (!hasClue)
                {
                    while (true)
                    {
                        int y = rng.GetRandomNumber();
                        if (y < height - 2 && (x - 2 < 0 || x - 2 >= 0 && cs[(x - 2) * height + y] != clueSymbol)
                            && (x + 2 >= width || (x + 2 < width && cs[(x + 2) * height + y] != clueSymbol)))
                        {
                            cs[x * height + y] = clueSymbol;
                            break;
                        }
                    }
                }
            }
        }

        void LastLines(WeightedRNG rng)
        {
            int number;
            int index = 1;
            while (index < width - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > width - 2 || number > 6) { }
                index += number;
                if (index > width - 3) break;
                cs[index * height + height - 2] = clueSymbol;
                //crossword[index, height - 2] = clueSymbol;
                //crossword[index, height - 1] = clueSymbol;
                cs[index * height + height - 1] = clueSymbol;

            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 2 || number > 6) { }
                index += number;
                if (index > height - 3) break;
                //crossword[height - 2, index] = clueSymbol;
                cs[(width - 2) * height + index] = clueSymbol;
                cs[(width - 1) * height + index] = clueSymbol;
                //crossword[height - 1, index] = clueSymbol;
            }
        }

        public WeightedRNG InitRNG()
        {
            List<WeightedNumber> list = new List<WeightedNumber>();
            //list.Add(new WeightedNumber(2, 30));
            list.Add(new WeightedNumber(3, 50));
            list.Add(new WeightedNumber(4, 70));
            list.Add(new WeightedNumber(5, 70));
            list.Add(new WeightedNumber(6, 70));
            list.Add(new WeightedNumber(7, 40));
            list.Add(new WeightedNumber(8, 30));
            list.Add(new WeightedNumber(9, 10));
            list.Add(new WeightedNumber(10, 3));
            /* int longestWordLength = dictionary.GetLongestWord().Length;
             for (int i = 11; i < longestWordLength; i++)
             {
                 list.Add(new WeightedNumber(i, 1));
             }*/
            WeightedRNG rng = new WeightedRNG();
            int max = Math.Min(Math.Max(width - 1, height - 1), list.Count);
            for (int i = 0; i < max; i++)
            {
                rng.AddNumber(list[i].Number, list[i].Weight);
            }
            return rng;
        }

        public void To2DArray()
        {
            for (int y = 0; y < height; y += 1)
            {
                for (int x = 0; x < width; x += 1)
                {
                    crossword[x, y] = cs[x * height + y];
                }

            }
        }

        void PismenaSedi(string[] cs, List<Word> usedWords) //JENOM NA TEST
        {
            int count = 0;
            string[] containedLetters;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * height + y].Contains("clue")) continue;
                    if (cs[x * height + y].Contains("/"))
                    {
                        containedLetters = ContainedLetters(cs, x, y, false);
                        if (ExistujeSlovo(usedWords, containedLetters))
                        {
                            count++;
                        }
                        else
                        {

                        }
                    }
                    if (cs[x * height + y] == "clue" || cs[x * height + y] == "clue/clue")
                    {
                        containedLetters = ContainedLetters(cs, x, y, true);
                        if (ExistujeSlovo(usedWords, containedLetters))
                        {
                            count++;
                        }
                        else
                        {

                        }
                    }
                }
            }
            if (count == usedWords.Count)
            {
                Console.WriteLine("joooooooooooo");
            }
            else
            {
                Console.WriteLine("neeeeeeeeee");
            }
        }

        bool ExistujeSlovo(List<Word> usedWords, string[] containedLetters)
        {
            string sl = string.Join("", containedLetters);
            foreach (Word w in usedWords)
            {
                if (w.word == sl || w.word.Equals(sl))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
