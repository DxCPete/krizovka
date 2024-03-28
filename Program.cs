

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class Program
    {

        static void Main(string[] args)
        {
            string[] a = { "A", "_", "_", "_", "_", "_", "_" };
            GetMinimalImposibilePath(a);
           // new CrosswordSw(15,15);

        }

       static string[] GetMinimalImposibilePath(string[] containedLetters)
        {
            Dictionary dictionary = new Dictionary(11);
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
            Console.WriteLine(letters);
            return letters.Select(c => c.ToString()).ToArray();
        }
    }
}
