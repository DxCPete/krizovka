

using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class Program
    {

        static void Main(string[] args)
        {
            new CrosswordSwedish(10,10);
            System.Console.WriteLine(Match("ko__a"));  // false
            System.Console.WriteLine(FindShortestPossibleMatch("oke_a"));  // Vrátí "k____" protože odpovídá "krysa".

        }

        static public string FindShortestPossibleMatch(string pattern)
        {
            if (Match(pattern))
            {
                return pattern;
            }

            for (int len = pattern.Length - 1; len > 0; len--)
            {
                StringBuilder sb = new StringBuilder(pattern);
                for (int i = len; i < pattern.Length; i++)
                {
                    sb[i] = '_';
                    string newPattern = sb.ToString();
                    if (Match(newPattern))
                    {
                        return newPattern;
                    }
                }
            }
            return null;  // Nenalezli jsme žádný odpovídající vzor
        }
        static public bool Match(string pattern)
        {
            string dict = "krysa,kniha,auto,traktor,pes,omega";

            // Nahrazení '_' znaku ve vzoru za jakékoliv písmeno
            string regexPattern = pattern.Replace("_", ".");

            // Přidání hranic slov pro přesné vyhledání slova
            regexPattern = @"\b" + regexPattern + @"\b";

            return Regex.IsMatch(dict, regexPattern);
        }
    }
}
