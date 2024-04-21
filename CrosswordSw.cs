using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class CrosswordSw : Crossword
    {
        protected string[] cs;
        protected Stack<(string[], List<Word>)> stack = new Stack<(string[], List<Word>)>();
        protected List<string> secrets = new List<string>();
        protected (int, int, bool) lastDeadEndedCoordinates = (0, 0, false);
        protected string clueSymbol = "7";
        protected string clue = "clue";
        protected string placeholderSecretSymbol = "8";

        protected int longestWordLength = 0;
        protected int pocetNesplnitelnychCest = 0;
        protected int pocetPouzitychSlov = 0;

        protected bool isFinished = false;
        private string secretPart1;
        private string secretPart2;

        public CrosswordSw(int x, int y, bool isCzechLanguage) : base(x, y,  isCzechLanguage)
        {
        }


        public override void Generate()
        {
            cs = new string[width * height];
            SetSecrets();
            do
            {
                InitCrosswordContraints();
                longestWordLength = LongestPossibleWord();
            } while (!CrosswordContraintsComplied() || longestWordLength >= 9);
            InsertSecrets();
            dictionary = new Dictionary(longestWordLength, isCzechLanguage, difficulty);
            FillWithWords();
            To2DArray();
            if (!isFinished)
            {
                Crossword csNew = new CrosswordSw(width, height, isCzechLanguage);
                this.crossword = csNew.crossword;
                this.usedWords = csNew.usedWords;
            }
        }


        int counterTest = 0;
        public int ukoncenoNaDeadEnd = 0;
        void FillWithWords()
        {
            int x;
            int y;
            bool horizontalDirection;
            int maxSide = Math.Max(width, height);
            while (stack.Count > 0)
            {
                if (counterTest > maxSide * 2000)
                {
                    return;
                }
                (string[], List<Word>) st = stack.Pop();
                string[] currentCs = st.Item1;
                List<Word> usedWords = st.Item2;
                (int, int, bool) coordinates = NextCoordinates(currentCs, usedWords.Count);
                x = coordinates.Item1;
                if (x == -1)
                {
                    AverageWordLength(usedWords);
                    cs = currentCs;
                    this.usedWords = usedWords;
                    isFinished = true;
                    return;
                }
                y = coordinates.Item2;
                horizontalDirection = coordinates.Item3;
                if (DeadEnd(currentCs))
                {
                    ukoncenoNaDeadEnd++;
                    continue;
                }
                string[] containedLetters = ContainedLetters(currentCs, x, y, horizontalDirection);
                List<Word> possibleWords = dictionary.SelectWordsLengthSensitive(usedWords, containedLetters);
                foreach (Word word in possibleWords)
                {
                    pocetPouzitychSlov++;
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
                        impossiblePathsList[x][y].Add((containedLetters, horizontalDirection));
                    }
                    lastDeadEndedCoordinates = (x, y, horizontalDirection);
                }
            }
        }

        (int, int, bool) NextCoordinates(string[] cs, int number)
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

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
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
                if (Char.IsLetter(char.Parse(containedLetters[i])) || containedLetters[i] == chPlaceholder)
                {
                    count++;
                }
            }
            return count;
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
                    cs[x * height + y] = cs[x * height + y].Replace(clueSymbol + "/", word.clue + "/");
                }
                else
                {
                    cs[x * height + y] = cs[x * height + y].Replace(clueSymbol, word.clue);
                }
            }
            else
            {
                cs[x * height + y] = cs[x * height + y].Replace("/" + clueSymbol, "/" + word.clue);
            }
            PrintCs(cs);
            return cs;
        }

        bool DeadEnd(string[] cs)
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
                for (int i = impossiblePathsList[0][j].Count - 1; i >= 0; i--)
                {
                    containedLetters = impossiblePathsList[0][j][i].containedLetters;
                    if (containedLetters.SequenceEqual(currentContainedLetters))
                    {
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
                for (int j = impossiblePathsList[i][0].Count - 1; j >= 0; j--)
                {
                    containedLetters = impossiblePathsList[i][0][j].containedLetters;
                    if (containedLetters.SequenceEqual(currentContainedLetters))
                    {
                        lastDeadEndedCoordinates = (i, 0, false);
                        return true;
                    }
                }
            }
            return false;
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
                            lastDeadEndedCoordinates = (i, j, horizontalDirection);
                            return true;
                        }

                        else if (!horizontalDirection && Match(currentContainedLettersVer, containedLetters))
                        {
                            lastDeadEndedCoordinates = (i, j, horizontalDirection);
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        public bool Match(string[] currentContainedLetters, string[] containedLetters)
        {
            string ccl = string.Join("", currentContainedLetters);
            string cl = string.Join("", containedLetters);

            ccl = ccl.Replace("_", ".");
            cl = cl.Replace("_", ".");
            return Regex.IsMatch(ccl, cl);
        }


        bool DeadEndAlreadyFound(string[] cs, int x, int y, bool horizontalDirection)
        {
            string[] currentContainedLetters = ContainedLetters(cs, x, y, horizontalDirection);
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
            int i = 0;
            Regex regex = new Regex(regexString);
            if (horintalDirection)
            {
                x++;
                pismena = new string[GetMaxWordLength(cs, x, y, horintalDirection)];
                while (x + i < width && !IsClue(cs[(x + i) * height + y]))
                {
                    if (regex.IsMatch(cs[(x + i) * height + y]) && cs[(x + i) * height + y].Length <= 1)
                    {
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

        int LongestPossibleWord()
        {
            int max = -1;
            PrintMainCs();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if (cs[x * height + y].Contains("/" + clueSymbol))
                    {
                        int i = 1;
                        while (y + i < height && !IsClue(cs[x * height + y + i]) && !IsClueForSecret(cs[x * height + y + i])) //.Contains(clueSymbol) || cs[x * height + y + i].Length > 2))
                        {
                            i++;
                        }
                        if (i > max)
                        {
                            max = i;
                        }
                        if (i == 1)
                        {
                            return 999;
                        }
                    }
                    if (cs[x * height + y] == clueSymbol || cs[x * height + y].Contains(clueSymbol + "/"))
                    {
                        int i = 1;
                        while (x + i < width && !cs[(x + i) * height + y].Contains(clueSymbol) && !IsClueForSecret(cs[(x + i) * height + y]))
                        {
                            i++;
                        }
                        if (i > max)
                        {
                            max = i;

                        }
                        if (i == 1)
                        {
                            return 999;
                        }
                    }
                }
            }
            return max;
        }

        private void InsertSecrets()
        {
            int n;
            List<(int, int, bool, string)> coords = new List<(int, int, bool, string)>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsClueForSecret(cs[x * height + y]))
                    {
                        n = 0;
                        if (y + 1 < height && cs[x * height + y + 1].Contains(placeholderSecretSymbol))
                        {
                            while (y + n + 1 < height && cs[x * height + y + n + 1] == placeholderSecretSymbol)
                            {
                                n++;
                            }
                            coords.Add((x, y, false, cs[x * height + y]));
                        }
                        else
                        {
                            while (x + n + 1 < width && cs[(x + n + 1) * height + y] == placeholderSecretSymbol)
                            {
                                n++;
                            }
                            coords.Add((x, y, true, cs[x * height + y]));
                        }
                    }
                }
            }
            PrintCs(cs);
            if (coords[0].Item4.Contains("2"))
            {
                var temp = coords[0];
                coords[0] = coords[1];
                coords[1] = temp;
            }

            foreach (string secret in secrets)
            {
                string[] csClone = (string[])cs.Clone();
                Word word = new Word(secret.Substring(0, secret.Length / 2), secretPart1);
                WriteWord(csClone, word, coords[0].Item1, coords[0].Item2, coords[0].Item3);
                word = new Word(secret.Substring(secret.Length / 2), secretPart2);
                WriteWord(csClone, word, coords[1].Item1, coords[1].Item2, coords[1].Item3);
                PrintCs(csClone);
                stack.Push((csClone, new List<Word>()));
            }
        }

        bool IsClueForSecret(string field)
        {
            return field.Contains(secretPart1) || field.Contains(secretPart2);
        }

        bool IsClue(string field)
        {
            return field.Contains(clueSymbol) || field.Contains(clue) || field.Length > 2;
        }



        bool CrosswordContraintsComplied()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!cs[x * height + y].Contains(clueSymbol)) continue;
                    if ((y != 0 && x + 2 < width && IsClue(cs[(x + 2) * height + y]))
                        || (x == width - 2 && !IsClue(cs[(x + 1) * height + y]))) //existuje místo pro slovo délky 1
                    {
                        return false;
                    }
                    if ((x != 0 && y + 2 < height && IsClue(cs[x * height + y + 2]))
                        || (y == height - 2 && !IsClue(cs[x * height + y + 1]))) //existuje místo pro slovo délky 1
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
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cs[x * height + y] = emptyField;
                }
            }
            AddSecrets();
            PrintMainCs();
            WeightedRNG rng = InitRNG();
            Console.WriteLine("FirstLines");
            FirstLines(rng);
            LastLines(rng);
            InnerPart(rng);
            PrintMainCs();
            FinishingTouches();
        }

        private void AddSecrets()
        {
            bool directionHorizontal = width >= height;
            int x = 0;
            int y = 0;
            string secret = secrets[0];
            Random random = new Random();
            int length = secret.Substring(0, secret.Length / 2).Length;
            if (directionHorizontal)
            {
                y = random.Next(3, 6);
                Word word = new Word(CreateSecretPlaceholder(length), secretPart1);
                if (word.word.Length == width - 3)
                {
                    x = width - length - 1;
                }
                if (x == 2)
                {
                    cs[(x - 1) * height + y] = clueSymbol;
                }
                cs[x * height + y] = word.clue;
                if (x != 0) cs[x * height + y] += "/" + clueSymbol;
                WriteWord(cs, word, x, y, directionHorizontal);
                if (x + word.word.Length + 1 < width)
                {
                    cs[(x + word.word.Length + 1) * height + y] = clueSymbol;
                }
                //druhá část
                length = secret.Substring(secret.Length / 2).Length;
                x = random.Next(0, width - length - 1);
                if (x + word.word.Length + 1 == width - 3)
                {
                    x = width - length - 1;
                }
                y = random.Next(7, width - 3);
                if (x == 2)
                {
                    cs[(x - 1) * height + y] = clueSymbol;
                }
                word = new Word(CreateSecretPlaceholder(length), secretPart2);
                cs[x * height + y] = word.clue;
                if (x != 0) cs[x * height + y] += "/" + clueSymbol;
                WriteWord(cs, word, x, y, directionHorizontal);
                if (x + word.word.Length + 1 < width)
                {
                    cs[(x + word.word.Length + 1) * height + y] = clueSymbol;
                }
            }
            else
            {
                x = random.Next(3, 6);
                Word word = new Word(CreateSecretPlaceholder(length), secretPart1);
                if (word.word.Length == height - 3)
                {
                    y = Math.Max(width, height) - length - 1;
                }
                if (x == 2)
                {
                    cs[x * height + y - 1] = clueSymbol;
                }
                cs[x * height + y] = "/" + word.clue;
                if (y != 0) cs[x * height + y] = clueSymbol + "/" + word.clue;
                WriteWord(cs, word, x, y, directionHorizontal);
                if (y + word.word.Length + 1 < height)
                {
                    cs[x * height + y + word.word.Length + 1] = clueSymbol;
                }
                //druhá část
                length = secret.Substring(secret.Length / 2).Length;
                y = random.Next(0, height - length - 1);
                if (y + word.word.Length + 1 == height - 3)
                {
                    y = height - length - 1;
                }
                if (x == 2)
                {
                    cs[x * height + y - 1] = clueSymbol;
                }
                x = random.Next(7, width - 3);
                word = new Word(CreateSecretPlaceholder(length), secretPart2);
                cs[x * height + y] = "/" + word.clue;
                if (y != 0) cs[x * height + y] = clueSymbol + "/" + word.clue;
                WriteWord(cs, word, x, y, directionHorizontal);
                if (y + word.word.Length + 1 < height)
                {
                    cs[x * height + y + word.word.Length + 1] = clueSymbol;
                }
            }
            PrintCs(cs);
        }

        private string CreateSecretPlaceholder(int length)
        {
            string placeholder = "";
            for (int i = 0; i < length; i++)
            {
                placeholder += placeholderSecretSymbol;
            }
            return placeholder;
        }

        private void SetSecrets()
        {
            int side = Math.Max(width, height) - 1;
            int secretLength = new Random().Next(side + side / 2, (side * 2 - 4));
            if (secretLength == side - 2) secretLength++;
            foreach (string sec in dictionary.secretsList)
            {
                string temp = sec.Replace(" ", "").Replace(",", "");
                if (temp.Length == secretLength)
                {
                    secrets.Add(temp);
                }
            }
            if (secrets.Count == 0) SetSecrets();
            if (isCzechLanguage)
            {
                secretPart1 = "TAJENKA 1";
                secretPart2 = "TAJENKA 2";
            }
            else
            {
                secretPart1 = "SECRET 1";
                secretPart2 = "SECRET 2";
            }
        }


        public void AddSecret()
        {
            List<string> secret = GetSecret();
            Random random = new Random();
            int x = 0;
            int y = 3;
            Word word = new Word(secret[0], secretPart1);
            cs[x * height + y] = clueSymbol;
            WriteWord(cs, word, x, y, true);
            if (x + word.word.Length + 1 < width)
            {
                cs[(x + word.word.Length + 1) * height + y] = clueSymbol;
            }
            PrintMainCs();
            word = new Word(secret[1], secretPart2);
            if (secret[1].Length < height - y)
            {
                x = random.Next(3, width - 2);
                y += 1;
                cs[x * height + y] = clueSymbol + "/" + clueSymbol;
                WriteWord(cs, word, x, y, false);
                if (y + word.word.Length + 1 < height)
                {
                    cs[x * height + y + word.word.Length + 1] = clueSymbol;
                }
            }
            else
            {
                while (true)
                {
                    y = random.Next(3, height - 2);
                    if (cs[x * height + y - 1] != emptyField) y += 1;
                    else if (cs[x * height + y] != emptyField) y += 2;
                    else if (cs[x * height + y + 1] != emptyField) y += 3;
                    else break;
                }
                cs[x * height + y] = clueSymbol;
                WriteWord(cs, word, x, y, true);
                if (x + word.word.Length + 1 < width)
                {
                    cs[(x + word.word.Length + 1) * height + y] = clueSymbol;
                }
            }
            PrintMainCs();

            if (secret.Count < 3) return;
            PrintMainCs();

        }

        public List<string> GetSecret()
        {
            int maxPartLength = Math.Max(width, height) - 1;
            string secret = "";
            if (secret == "")
            {
                throw new Exception("Secret not found for this grid");
            }
            char[] separators = { ' ', ',' };
            string[] parts = secret.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            List<string> results = new List<string>();
            string temp = "";

            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(temp) && temp.Length + part.Length + 1 > maxPartLength)
                {
                    results.Add(temp);
                    temp = part;
                }
                else
                {
                    if (string.IsNullOrEmpty(temp))
                    {
                        temp = part;
                    }
                    else
                    {
                        temp += " " + part;
                    }
                }
                if (temp.Length >= maxPartLength / 2 && temp.Length <= maxPartLength && temp.Length != maxPartLength - 3)
                {
                    results.Add(temp.Replace(" ", "").Replace(",", ""));
                    temp = "";
                }
            }
            if (!string.IsNullOrEmpty(temp))
            {
                results.Add(temp);
            }

            if (!results[results.Count - 1].Contains(parts[parts.Length - 1]) || results.Count > 3) return GetSecret();
            return results;
        }


        void FinishingTouches()
        {
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (cs[x * height + y] != clueSymbol) continue;
                    if (x + 1 < width && y != height - 1 && IsClue(cs[(x + 1) * height + y]))
                    {
                        if (IsClueForSecret(cs[x * height + y]))
                        {
                            cs[x * height + y] = cs[x * height + y].Replace(clueSymbol, "");
                        }
                        else if (IsClueForSecret(cs[(x + 1) * height + y]))
                        {
                            cs[x * height + y] = emptyField;
                        }
                        else if (cs[(x - 1) * height + y] != emptyField)
                        {
                            cs[(x + 1) * height + y] = emptyField;
                        }
                        else if (cs[x * height + y - 1] == placeholderSecretSymbol && cs[x * height + y - 2] == placeholderSecretSymbol)
                        {

                        }
                        else
                        {
                            cs[x * height + y] = emptyField;
                        }
                    }

                    if (x != width - 1 && y + 1 < height && !IsClue(cs[x * height + y + 1]))
                    {
                        cs[x * height + y] += "/" + clueSymbol;
                    }
                    if (x == width - 1)
                    {
                        cs[x * height + y] = "/" + clueSymbol;
                    }

                    if (x == width - 1 && y + 1 < height && cs[x * height + y + 1].Contains(clueSymbol)
                        && cs[(x - 1) * height + y] == emptyField)
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
                    if (y == height - 1 && x + 1 < width && cs[(x + 1) * height + y].Contains(clueSymbol)
                         && cs[x * height + y - 1] == emptyField)
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
                    else if (cs[x * height + y] == clueSymbol + "/" + clue && IsClue(cs[(x + 1) * height + y]))
                    {
                        cs[x * height + y] = "/" + clue;
                    }
                    if (y > 0 && x != 1 && x + 1 < width && cs[x * height + y - 1].Contains("/" + clueSymbol) && IsClue(cs[x * height + y])
                        && !IsClueForSecret(cs[(x + 1) * height + y - 1]))
                    {
                        cs[x * height + y - 1] = cs[x * height + y - 1].Replace("/" + clueSymbol, "");
                    }
                    if (IsClueForSecret(cs[x * height + y]) && y < 0 && y < height - 1 &&
                        (cs[x * height + y + 2].Contains("/" + clueSymbol) || cs[x * height + y + 1].Contains("/" + clueSymbol)))
                    {
                        cs[x * height + y] = cs[x * height + y].Replace("/" + clueSymbol, "");
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
                      sb.Append(cs[x * height + y]+ " | ");
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
                if (cs[i * height + 0] == emptyField && cs[i * height + 1] == emptyField)
                {
                    cs[i * height + 0] = "/" + clueSymbol;
                }
            }

            for (int i = 1; i < height; i++)
            {
                if (cs[0 * height + i] == emptyField && cs[1 * height + i] == emptyField)
                {
                    cs[0 * height + i] = clueSymbol;
                }
            }
            int number;
            int index = 1;
            while (index < width - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > width - 3 || number > 6 || index + number > width - 3) { }
                index += number;
                if (cs[index * height + 1] != emptyField) continue;
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
                if (cs[1 * height + index] != emptyField) continue;
                cs[0 * height + index] = emptyField;
                cs[1 * height + index] = clueSymbol;
                cs[2 * height + index] = clueSymbol;

            }

        }

        void InnerPart(WeightedRNG rng)
        {
            Regex regex = new Regex(regexString);
            for (int y = 3; y < height - 2; y++)
            {
                for (int x = Math.Max(rng.GetRandomNumber(), 3); x < width - 2; x++)
                {

                    if (cs[x * height + y] == emptyField && !IsClue(cs[x * height + y - 2]) && !IsClue(cs[x * height + y + 2])
                         && !IsClue(cs[(x + 2) * height + y]) && !IsClue(cs[(x - 2) * height + y]))
                    {
                        cs[x * height + y] = clueSymbol;
                        x += rng.GetRandomNumber();
                    }
                }
            }
            for (int x = 3; x < width - 2; x++)
            {
                for (int y = Math.Max(rng.GetRandomNumber(), 3); y < height - 2; y++)
                {
                    if (cs[x * height + y] == emptyField && !IsClue(cs[x * height + y - 2]) && !IsClue(cs[x * height + y + 2])
                         && !IsClue(cs[(x + 2) * height + y]) && !IsClue(cs[(x - 2) * height + y]))
                    {
                        cs[x * height + y] = clueSymbol;
                        y += rng.GetRandomNumber();
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
                        if (cs[x * height + y].Contains(placeholderSecretSymbol) && cs[x * height + y - 1].Contains(placeholderSecretSymbol))
                        {
                            break;
                        }
                        if (cs[x * height + y] == emptyField && y < height - 2 && (x - 2 < 0 || x - 2 >= 0 && cs[(x - 2) * height + y] != clueSymbol)
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
            if (width < 6)
            {
                return;
            }
            int number;
            int index = 1;
            while (index < width - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > width - 2 || number > 6) { }
                if (cs[(index + number) * height + height - 1] != emptyField) continue;
                index += number;
                if (index > width - 3) break;
                cs[index * height + height - 2] = clueSymbol;
                cs[index * height + height - 1] = clueSymbol;

            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 2 || number > 6) { }
                index += number;
                if (index > height - 3) break;
                if (cs[(width - 1) * height + index] == emptyField)
                {
                    cs[(width - 2) * height + index] = clueSymbol;
                }
                cs[(width - 1) * height + index] = clueSymbol;
            }
        }

        public WeightedRNG InitRNG()
        {
            List<WeightedNumber> list = new List<WeightedNumber>();
            list.Add(new WeightedNumber(3, 50));
            list.Add(new WeightedNumber(4, 70));
            list.Add(new WeightedNumber(5, 70));
            list.Add(new WeightedNumber(6, 70));
            list.Add(new WeightedNumber(7, 40));
            list.Add(new WeightedNumber(8, 30));
            list.Add(new WeightedNumber(9, 10));
            list.Add(new WeightedNumber(10, 3));
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
            for (int x = 0; x < width; x += 1)
            {
                for (int y = 0; y < height; y += 1)
                {
                    crossword[x, y] = cs[x * height + y];
                }

            }
        }

        double AverageWordLength(List<Word> usedWords)
        {
            double count = 0.0;
            foreach (Word w in usedWords)
            {
                count += w.word.Length;
            }

            double average = count / usedWords.Count;
            Console.WriteLine("Average: " + average);
            return average;
        }

    }
}
