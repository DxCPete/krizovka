using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BAK
{
    class WordComparer : IEqualityComparer<Word>
    {
        /* podle mě jsem to jenom zapomněl smazat dřív
         * private string value;


        public WordComparer(string value)
        {
            this.value = value;
        }*/
        public WordComparer() {
        }
        
        public bool Equals(Word kandidat, Word obsahuje/*, bool a*/)
        {   
            if (kandidat.word.Equals(obsahuje.word))
            {
                return true;
            }
            else
            {
                String[] slovoObsahuje = obsahuje.word.ToCharArray().Select(c => c.ToString()).ToArray(); //převede string na string[]
                return PouzitelneSlovo(kandidat, slovoObsahuje);
            }
        }


        public bool StartsWith(Word kandidat, Word contains)
        {
            return (kandidat.word.StartsWith(contains.word));            
        }

        public bool Contains(Word kandidat, string[] slovoObsahuje)
        {
            if (kandidat.word.Equals(string.Concat(slovoObsahuje)))
            {
                return true;
            }
            else
            {
                return PouzitelneSlovo(kandidat, slovoObsahuje);
            }
        }

        public bool PouzitelneSlovo(Word kandidat, string[] slovoObsahuje)
        {
            int n = kandidat.word.Length;
            int n2 = slovoObsahuje.Length;
            if (n > n2) return false;

            int i = 0;
            while (i < n && i < n2 && char.IsLetter(kandidat.word[i]))
            {
                //Console.WriteLine(kandidat.slovo[i].ToString() + " " + slovoObsahuje[i]);
                if (kandidat.word[i].ToString() != slovoObsahuje[i] && slovoObsahuje[i] != "_" )
                {
                    return false;
                }

                i++;
            }
            string[] newA = slovoObsahuje.Skip(i).ToArray();
            int errorCounter = Regex.Matches(string.Concat(newA), @"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]").Count;
            if (errorCounter > 0) return false;
            return true;
        }

       /* public bool Equals(Word x, Word y)
        {
            if (y.clue.Equals(""))
            {
                return Equals(x, y, true);
            }

            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.word == y.word;
        }*/

        public int GetHashCode(Word word)
        {
            if (Object.ReferenceEquals(word, null)) return 0;

            int hashWord = word.word == null ? 0 : word.word.GetHashCode();

            return hashWord;
        }

    }
}
