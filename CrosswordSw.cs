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
        Stack<string[]> stack = new Stack<string[]>();

        int longestWordLength = 0;
        int pocetNesplnitelnychCest = 0;

        public CrosswordSw(int x, int y) : base(x, y)
        {
        }


        public override void Generate()
        {
            /*
             mám pocit, že to přepisuje písmena
             */
            cs = new string[width * height];
            do
            {
                InitCrosswordContraints();
            } while (!CrosswordContraintsComplied()); //hotova jen z casti
            longestWordLength = LongestPossibleWord();
            dictionary = new Dictionary(LongestPossibleWord());
            FillWithWords();
            To2DArray();
        }


        int counterTest = 0;
        public void FillWithWords()
        {
            string[] csClone = (string[])cs.Clone();
            //List<Word> usedWords = new List<Word>(); //probably useless
            stack.Push(csClone);
            int x;
            int y;
            bool horizontalDirection;
            while (stack.Count > 0)
            {
                foreach (Word word in dictionary.wordsList)
                {
                    counterTest++;
                    csClone = (string[])stack.Pop();
                    PrintCs(csClone);
                    if (DeadEnd(cs, 0, 0))
                    {
                        break;
                    }

                    (int, int, bool) coordinates = FindWordStart(csClone, word);
                    x = coordinates.Item1;
                    y = coordinates.Item2;
                    horizontalDirection = coordinates.Item3;
                    if (x == -1)
                    {
                        if (IsFinished(csClone))
                        {
                            cs = csClone;
                            return;
                        }
                        stack.Push(csClone);
                        continue;
                        // if x == -2.... křížovka je plná? 
                    }
                    WriteWord(csClone, word, x, y, horizontalDirection);
                    PrintCs(csClone);
                    stack.Push(csClone);
                }
            }
            FindDeadEnds(csClone);
            /*projít si všechna neobsažená místa a zkusit je dát do DeadEnd*/
        }

        bool IsFinished(string[] cs)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x * width + y] == emptyField)
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
                    cs[(x + i + 1) * width + y] = wordLetters[i];
                }
            }
            else
            {
                for (int i = 0; i < wordLetters.Length; i++)
                {
                    cs[x * width + y + i + 1] = wordLetters[i];
                }
            }

            return cs;
        }

        string[] WriteClue(string[] cs, Word word, int x, int y, bool horizontalDirection)
        {
            if (horizontalDirection)
            {
                if (cs[x * width + y].Contains("/"))
                {
                    cs[x * width + y] = cs[x * width + y].Replace(clueSymbol + "/", clue + "/");
                }
                else
                {
                    cs[x * width + y] = cs[x * width + y].Replace(clueSymbol, clue);
                }
            }
            else
            {
                cs[x * width + y] = cs[x * width + y].Replace("/" + clueSymbol, "/" + clue);
            }
            PrintCs(cs);
            Console.WriteLine(cs[x * width + y]);
            return cs;
        }


        (int, int, bool) FindWordStart(string[] cs, Word word)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * width + y].Contains(clueSymbol)) continue;
                    if (cs[x * width + y].Contains("/" + clueSymbol))
                    {
                        if (CanPlace(cs, word, x, y, false))
                        {
                            return (x, y, false);
                        }
                    }
                    else if (cs[x * width + y].Contains(clueSymbol))
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
            return DeadEndBorder(cs, x, y) && DeadEndInner(cs, x, y);
        }



        bool DeadEndBorder(string[] cs, int x, int y)
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

        bool DeadEndInner(string[] cs, int x, int y)
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

        void FindDeadEnds(string[] cs)
        {
            bool horizontalDirection;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!cs[x * width + y].Contains(clueSymbol)) continue;
                    if (cs[x * width + y].Contains("/" + clueSymbol))
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
                    if (cs[x * width + y] == clueSymbol || cs[x * width + y] == clueSymbol + "/" + clueSymbol)
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
                while (x + i < width && !IsClue(cs[(x + i) * width + y]))
                {
                    if (regex.IsMatch(cs[(x + i) * width + y]) && cs[(x + i) * width + y].Length <= 1)
                    {
                        //pismena.Append(cs[x + i, y]);
                        pismena[i] = cs[(x + i) * width + y];
                    }
                    else if (cs[(x + i) * width + y] == " ")
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
                while (y + i < height && !IsClue(cs[x * width + y + i]))
                {
                    if (regex.IsMatch(cs[x * width + y + i]) && cs[x * width + y + i].Length <= 1)
                    {
                        pismena[i] = cs[x * width + y + i];
                    }
                    else if (cs[x * width + y + i] == " ")
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
            int i = 0;
            if (horizontalDirection)
            {
                while (x + i < width && !IsClue(cs[(x + i) * width + y]))
                {
                    i++;
                }

            }
            else
            {
                while (y + i < height && !IsClue(cs[x * width + y + i]))
                {
                    i++;
                }
            }
            Console.WriteLine(i);
            return i;
        }

        bool CanPlace(string[] cs, Word word, int x, int y, bool horizontalDirection)
        {
            int i = 0;
            string[] wordLetters = word.word.Select(c => c.ToString()).ToArray();
            if (horizontalDirection)
            {
                while (i < wordLetters.Length && x + i + 1 < width && (wordLetters[i] == cs[(x + i + 1) * width + y] || cs[(x + i + 1) * width + y] == emptyField))
                {
                    i++;
                }
                if (x + i + 1 != width && !IsClue(cs[(x + i + 1) * width + y]))
                {
                    return false;
                }
            }
            else
            {
                while (i < wordLetters.Length && y + i + 1 < height && (wordLetters[i] == cs[x * width + y + i + 1] || cs[x * width + y + i + 1] == emptyField))
                {
                    i++;
                }
                if (y + i + 1 != height && !IsClue(cs[x * width + y + i + 1]))
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

        int LongestPossibleWord()
        {
            int max = -1;
            PrintMainCs();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x * width + y].Contains("/" + clueSymbol))
                    {
                        int i = 1;
                        while (y + i < height && !cs[x * width + y + i].Contains(clueSymbol))
                        {
                            i++;
                        }
                        i--;
                        if (i > max)
                        {
                            Console.WriteLine(x + " " + y + " " + false);
                            max = i;

                        }
                    }
                    else if (cs[x * width + y].Contains(clueSymbol))
                    {
                        int i = 1;
                        while (x + i < width && !cs[(x + i) * width + y].Contains(clueSymbol))
                        {
                            i++;
                        }
                        i--;
                        if (i > max)
                        {
                            Console.WriteLine(x + " " + y + " " + true);
                            max = i;

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
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (!cs[x * width + y].Contains(clueSymbol)) continue;
                    if (x + 2 < width && cs[(x + 2) * width + y].Contains(clueSymbol)) //existuje místo pro slovo délky 1
                    {
                        return false;
                    }
                    if (y + 2 < height && cs[x * width + y + 2].Contains(clueSymbol)) //existuje místo pro slovo délky 1
                    {
                        return false;
                    }
                    if ((x + 1 < width && cs[(x + 1) * width + y].Contains(clueSymbol)) &&
                        (y + 1 < height && cs[x * width + y + 1].Contains(clueSymbol)))
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
                    cs[x * width + y] = emptyField;
                }
            }

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
                    if (cs[x * width + y] != clueSymbol) continue;
                    if (x + 1 < width && y != height - 1 && cs[(x + 1) * width + y] == clueSymbol)
                    {
                        cs[x * width + y] = emptyField;
                    }
                    if (x != width - 1 && y + 1 < height && cs[x * width + y + 1] != clueSymbol)
                    {
                        cs[x * width + y] += "/" + clueSymbol;
                    }
                    if (x == width - 1)
                    {
                        cs[x * width + y] = "/" + clueSymbol;
                    }

                    if (x == width - 1 && y + 1 < height && cs[x * width + y + 1].Contains(clueSymbol))
                    {
                        if (cs[(x - 1) * width + y].Contains(clueSymbol))
                        {
                            cs[x * width + y + 1] = emptyField;
                        }
                        else
                        {
                            cs[x * width + y] = emptyField;
                        }
                    }
                    else if (y == height - 1 && x + 1 < width && cs[(x + 1) * width + y].Contains(clueSymbol))
                    {
                        if (cs[x * width + y - 1].Contains(clueSymbol))
                        {
                            cs[(x + 1) * width + y] = emptyField;
                        }
                        else
                        {
                            cs[x * width + y] = emptyField;
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
                    sb.Append(cs[x * width + y].Replace("7/7", "7").Replace("/7", "7") + " | ");
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
                    sb.Append(cs[x * width + y] + " | ");
                }

                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }

        void FirstLines(WeightedRNG rng)
        {
            for (int i = 1; i < width; i++)
            {
                //crossword[i, 0] = clueSymbol;
                cs[i * width + 0] = "/" + clueSymbol;
            }

            for (int i = 1; i < height; i++)
            {
                //crossword[0, i] = clueSymbol;
                cs[0 * width + i] = clueSymbol;

            }
            int number;
            int index = 1;
            while (index < width - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > width - 3 || number > 6 || index + number > width - 3) { }
                index += number;
                if (index > width - 3) break;
                //crossword[index, 0] = " ";
                cs[index * width + 0] = emptyField;
                //crossword[index, 1] = clueSymbol;
                cs[index * width + 1] = clueSymbol;
                //crossword[index, 2] = clueSymbol;
                cs[index * width + 2] = clueSymbol;
            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 3 || number > 6 || index + number > height - 3) { }
                if (index > height - 3) break;
                index += number;
                //crossword[0, index] = " ";
                cs[0 * width + index] = emptyField;
                //crossword[1, index] = clueSymbol;
                cs[1 * width + index] = clueSymbol;
                //crossword[2, index] = clueSymbol;
                cs[2 * width + index] = clueSymbol;

            }

        }

        void InnerPart(WeightedRNG rng)
        {
            for (int y = 2; y < height; y++)
            {
                for (int x = rng.GetRandomNumber(); x < width; x++)
                {
                    if (x != width - 2 && y != height - 2 && cs[x * width + y] != clueSymbol && (y - 2 >= 0 && cs[x * width + y - 2] != clueSymbol) &&
                        (x + 2 < width && cs[(x + 2) * width + y] != clueSymbol) && (x - 2 >= 0 && cs[(x - 2) * width + y] != clueSymbol))
                    {
                        cs[x * width + y] = clueSymbol;
                        x += rng.GetRandomNumber();
                    }
                }
            }

            for (int x = 3; x < width; x++)
            {
                int number = rng.GetRandomNumber();
                for (int i = 0; i < height; i++)
                {
                    int y = rng.GetRandomNumber();
                    if (y < height && cs[x * width + y] != clueSymbol && cs[(x - 1) * width + y] != clueSymbol && cs[(x - 2) * width + y] != clueSymbol &&
                       (x + 1 < width && cs[(x + 1) * width + y] != clueSymbol) && (x + 2 < width && cs[(x + 2) * width + y] != clueSymbol)
                        && cs[x * width + y - 1] != clueSymbol && cs[x * width + y - 2] != clueSymbol && (y + 1 < height && cs[x * width + y + 1] != clueSymbol)
                        && (y + 2 < height && cs[x * width + y + 2] != clueSymbol))
                    {
                        Console.WriteLine(x + " " + y);
                        cs[x * width + y] = clueSymbol;
                    }
                }


                bool hasClue = false;
                for (int y = 2; y < height; y++)
                {
                    if (cs[x * width + y] == clueSymbol)
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
                        Console.WriteLine(x);
                        if (y < height - 2 && (x - 2 < 0 || x - 2 >= 0 && cs[(x - 2) * width + y] != clueSymbol)
                            && (x + 2 >= width || (x + 2 < width && cs[(x + 2) * width + y] != clueSymbol)))
                        {
                            cs[x * width + y] = clueSymbol;
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
                cs[index * width + height - 2] = clueSymbol;
                //crossword[index, height - 2] = clueSymbol;
                //crossword[index, height - 1] = clueSymbol;
                cs[index * width + height - 1] = clueSymbol;

            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 2 || number > 6) { }
                index += number;
                if (index > height - 3) break;
                //crossword[width - 2, index] = clueSymbol;
                cs[(width - 2) * width + index] = clueSymbol;
                cs[(width - 1) * width + index] = clueSymbol;
                //crossword[width - 1, index] = clueSymbol;
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
            for (int i = 11; i < dictionary.GetLongestWord().Length; i++)
            {
                list.Add(new WeightedNumber(i, 1));

            }
            WeightedRNG rng = new WeightedRNG();
            for (int i = 0; i < Math.Max(width - 1, height - 1); i++)
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
                    crossword[x, y] = cs[x * width + y];
                }

            }
        }
    }
}
