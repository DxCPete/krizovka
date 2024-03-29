using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BAK
{
    class WordComparer : IEqualityComparer<Word>
    {
        public WordComparer()
        {
        }

        public bool Equals(Word candidate, Word obsahuje)
        {
            if (candidate.word.Equals(obsahuje.word))
            {
                return true;
            }
            else
            {
                String[] slovoObsahuje = obsahuje.word.ToCharArray().Select(c => c.ToString()).ToArray(); //převede string na string[]
                return CanUseThisWord(candidate, slovoObsahuje);
            }
        }


        public bool StartsWith(Word candidate, Word contains)
        {
            return (candidate.word.StartsWith(contains.word));
        }

        public bool Contains(Word kandidat, string[] slovoObsahuje)
        {
            if (kandidat.word.Equals(string.Concat(slovoObsahuje)))
            {
                return true;
            }
            else
            {
                return CanUseThisWord(kandidat, slovoObsahuje);
            }
        }


        public bool EqualsNew(Word candidate, Word obsahuje)
        {
            if (candidate.word.Equals(obsahuje.word))
            {
                return true;
            }
            else
            {
                string[] slovoObsahuje = obsahuje.word.ToCharArray().Select(c => c.ToString()).ToArray(); //převede string na string[]
                return CanUseThisWordNew(candidate, slovoObsahuje);
            }
        }

        public bool CanUseThisWordNew(Word candidate, string[] wordContains)
        {
            int n = candidate.word.Length;
            int n2 = wordContains.Length;
            if (n != n2) return false;

            int i = 0;
            while (i < n)
            {
                // Console.WriteLine(candidate.word[i].ToString() + " " + wordContains[i]);
                if (candidate.word[i].ToString() != wordContains[i] && (wordContains[i] != "_" || wordContains[i] == " "))
                {
                    return false;
                }
                i++;
            }

            /* string[] newA = wordContains.Skip(i).ToArray();
             int errorCounter = Regex.Matches(string.Concat(newA), @"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]").Count;
             if (errorCounter > 0) return false;
             */
            return true;
        }


        public bool CanUseThisWord(Word candidate, string[] wordContains)
        {
            int n = candidate.word.Length;
            int n2 = wordContains.Length;
            if (n > n2 || n == n2 - 1) return false;

            int i = 0;
            //Console.WriteLine(candidate.word);
            while (i < n && char.IsLetter(candidate.word[i]))
            {
                // Console.WriteLine(candidate.word[i].ToString() + " " + wordContains[i]);
                if (candidate.word[i].ToString() != wordContains[i] && (wordContains[i] != "_" || wordContains[i] == " "))
                {
                    return false;
                }
                i++;
            }
            if (i < n2 && wordContains[i] != "_")
            {
                return false;
            }
            /* string[] newA = wordContains.Skip(i).ToArray();
             int errorCounter = Regex.Matches(string.Concat(newA), @"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]").Count;
             if (errorCounter > 0) return false;
             */
            return true;
        }

        public int GetHashCode(Word word)
        {
            if (Object.ReferenceEquals(word, null)) return 0;

            int hashWord = word.word == null ? 0 : word.word.GetHashCode();

            return hashWord;
        }

    }
}
