using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BAK
{
    /* "7" je dočasná náhrada za LEGENDU */
    class Crossword
    {
        public string[,] crossword { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public Dictionary dictionary;
        public string language { get; set; }
        public int difficulty { get; set; }
        public Word[] clueHoritonzal;
        public Word[] clueVertical;
        public List<(string[] containedLetters, bool horizontalDirection)>[][] impossiblePathsList;


        public Crossword(int width, int height)   //pak přidat jazyk
        {
            this.width = width;
            this.height = height;
            dictionary = new Dictionary(Math.Min(width, height));

            impossiblePathsList = new List<(string[] containedLetters, bool horizontalDirection)>[width][];
            for (int i = 0; i < width; i++)
            {
                impossiblePathsList[i] = new List<(string[] containedLetters, bool horizontalDirection)>[height];
                for (int j = 0; j < height; j++)
                {
                    impossiblePathsList[i][j] = new List<(string[] containedLetters, bool horizontalDirection)>(); // můžete zde přidat data
                }
            }

            crossword = new string[this.width, this.height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    crossword[j, i] = " ";
                }
            }
            Generate();
            Print();
        }


        public virtual string[] ContainedLetters(int x, int y, bool horintalDirection, int maxLength)
        {
            string[] pismena = new string[maxLength];
            int i = 0;
            Regex regex = new Regex(@"[\p{Lu}\p{L}ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ]");
            if (horintalDirection)
            {
                while (i < maxLength)
                {
                    if (regex.IsMatch(crossword[x + i, y]))
                    {
                        pismena[i] = crossword[x + i, y];
                    }
                    else
                    {
                        pismena[i] = "_";
                    }
                    i += 1;
                }
            }
            else
            {
                while (i < maxLength)
                {
                    if (regex.IsMatch(crossword[x, y + i]))
                    {
                        pismena[i] = crossword[x, y + i];
                    }
                    else
                    {
                        pismena[i] = "_";
                    }
                    i += 1;
                }
            }
            return pismena;
        }

        public virtual int MaxLength(int x, int y, bool horintalDirection)
        {
            return -1;
        }



        public virtual void ClueWrite(Word word, int x, int y, bool horintalDirection)
        {

            if (crossword[x, y].Equals(" "))
            {
                crossword[x, y] = "7"; // slovo.legenda; PROZATÍM KVŮLI PŘEHLEDNOSTI V KONZOLE
            }
            else
            {
                if (horintalDirection)
                {
                    crossword[x, y] = "7/7";
                }
                else
                {
                    crossword[x, y] = "7/7";
                }
            }
        }

        public virtual void Generate()
        {
        }

        public void Print()
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < height; j += 1)
            {
                for (int i = 0; i < width; i += 1)
                {
                    Console.Write(crossword[i, j] + " | ");
                    sb.Append(crossword[i, j] + " | ");
                }
                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }

        public string[,] testing()
        {
            string input = @"";

            char[] pole = input.ToCharArray();

            int i = 0;
            int j = 0;
            for (int k = 0; k < pole.Length; k++)
            {
                if (pole[k].Equals(' ') && pole[k + 1].Equals(' ') && pole[k + 2].Equals(' '))
                {
                    crossword[i, j] = " ";
                }
                else if (pole[k].Equals('|'))
                {
                    i++;
                }
                else if (pole[k].Equals('\n'))
                {
                    j++;
                    i = 0;
                }
                else if (pole[k].Equals('7') || pole[k].Equals('/') || char.IsLetter(pole[k]))
                {
                    crossword[i, j] += pole[k];
                }

            }
            return crossword;
            ;
        }
    }
}
