

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("novy");

            new CrosswordSw(25,30);

            /*for (int i = 0; i < 10; i++)
            {
                new CrosswordSw(30, 30);
            }*/
        }

        static string GetShortestLettersCount(string[] containedLetters)
        {
            List<string[]> list = new List<string[]>();
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
            Console.WriteLine(string.Join("", minimalLetters));
            return "";
        }

        static void GenerateCombinations(string[] containedLetters, List<string[]> list, int index)
        {
            Dictionary dictionary = new Dictionary(100);
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
    }
}
