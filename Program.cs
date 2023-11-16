

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class Program
    {

        static void Main(string[] args)
        {
            new CrosswordSwedish(9,9);
            // System.Console.WriteLine(Match("ko__a"));  // false
            // System.Console.WriteLine(FindShortestPossibleMatch("o_d_a"));  // Vrátí "k____" protože odpovídá "krysa".

            // System.Console.WriteLine(Match("gd_z","g__z"));



        }

        static public string FindShortestPossibleMatch(string pattern)
        {
            if (Match(pattern))
            {
                return pattern;
            }
            List<string> patterns = new List<string>();
            for (int len = pattern.Length - 1; len > 0; len--)
            {
                StringBuilder sb = new StringBuilder(pattern);
                for (int i = len; i < pattern.Length; i++)
                {
                    sb[i] = '_';
                    string newPattern = sb.ToString();

                    if (!Match(newPattern))
                    {
                        if (i == len)
                        {
                            patterns.Clear();
                        }
                        patterns.Add(newPattern);
                        System.Console.WriteLine("Toto ještě pořád nesplňuje podmínky: " + newPattern);
                        //return newPattern;
                    }
                    else
                    {
                        System.Console.WriteLine("Patterny: " + patterns.Count);
                        foreach(string pat in patterns)
                        {
                            System.Console.WriteLine(pat);
                        }
                        //přidat na impossiblePathList všechny prvky z List patterns
                        return newPattern;
                    }
                }
            }
            return null;  // Nenalezli jsme žádný odpovídající vzor
        }
        static public bool Match(string pattern)
        {
            string[] dictionary = { "krysa", "kniha", "auto", "traktor", "pes", "omega"};

            // Nahrazení '_' znaku ve vzoru za jakékoliv písmeno
            string regexPattern = pattern.Replace("_", ".");

            // Přidání hranic slov pro přesné vyhledání slova
            regexPattern = @"\b" + regexPattern + @"\b";

            return dictionary.Any(word => Regex.IsMatch(word, regexPattern));
        }

        static public bool Match(string containedLetters, string currenctContainedLetters)
        {
            containedLetters = containedLetters.Replace("_", ".");
            currenctContainedLetters = currenctContainedLetters.Replace("_", ".");


            // Přidání hranic slov pro přesné vyhledání slova

            return Regex.IsMatch(containedLetters, currenctContainedLetters);
        }
    }
}
