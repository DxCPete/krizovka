using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BAK
{
    class CrosswordSw : Crossword
    {
        string clueSymbol = "7";
        string[] cs;
        public CrosswordSw(int x, int y) : base(x, y)
        {
        }


        public override void Generate()
        {
            cs = new string[width * height];
            InitCrosswordContraints();
            FillWithWords();

        }

        public void FillWithWords()
        {
            (int, int, bool) t = FindWordStart();
        }

        (int, int, bool) FindWordStart()
        {
            return (0, 0, false);
        }

        int LongestPossibleWord()
        {
            int max = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cs[x * width + y].Contains("/" + clueSymbol))
                    {
                        while (y < height) ;
                    }
                }
            }
            return max;
        }

        public void InitCrosswordContraints()
        {
            WeightedRNG rng = InitRNG();
            Console.WriteLine("FirstLines");
            FirstLines(rng);
            Print();
            if (width > 6)
            {
                Console.WriteLine("Last Lines");
                LastLines(rng);
            }
            Print();
            Console.WriteLine("InnerPart");
            InnerPart(rng);
            Print();
            Console.WriteLine("Finishing Touches");
            FinishingTouches();
        }

        void FinishingTouches()
        {
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    if (cs[x * width + y] != clueSymbol) continue;
                    if (x + 1 < width && y != height - 1 && crossword[x + 1, y] == clueSymbol)
                    {
                        crossword[x, y] = emptyField;
                    }
                    if (x != width - 1 && y + 1 < height && crossword[x, y + 1] != clueSymbol)
                    {
                        crossword[x, y] += "/" + clueSymbol;
                    }
                    if (x == width - 1)
                    {
                        crossword[x, y] = "/" + clueSymbol;
                    }

                    if (x == width - 1 && y + 1 < height && crossword[x, y + 1].Contains(clueSymbol))
                    {
                        if (crossword[x - 1, y].Contains(clueSymbol))
                        {
                            crossword[x, y + 1] = emptyField;
                        }
                        else
                        {
                            crossword[x, y] = emptyField;
                        }
                    }
                    else if (y == height - 1 && x + 1 < width && crossword[x + 1, y].Contains(clueSymbol))
                    {
                        if (crossword[x, y - 1].Contains(clueSymbol))
                        {
                            crossword[x + 1, y] = emptyField;
                        }
                        else
                        {
                            crossword[x, y] = emptyField;
                        }
                    }
                }
            }
        }


        void FirstLines(WeightedRNG rng)
        {
            for (int i = 1; i < width; i++)
            {
                crossword[i, 0] = clueSymbol;
            }

            for (int i = 1; i < height; i++)
            {
                crossword[0, i] = clueSymbol;
            }
            int number;
            int index = 1;
            while (index < width - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > width - 3 || number > 6 || index + number > width - 3) { }
                index += number;
                if (index > width - 3) break;
                crossword[index, 0] = " ";
                crossword[index, 1] = clueSymbol;
                crossword[index, 2] = clueSymbol;
            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 3 || number > 6 || index + number > height - 3) { }
                if (index > height - 3) break;
                index += number;
                crossword[0, index] = " ";
                crossword[1, index] = clueSymbol;
                crossword[2, index] = clueSymbol;

            }

        }

        void InnerPart(WeightedRNG rng)
        {
            for (int y = 2; y < height; y++)
            {
                for (int x = rng.GetRandomNumber(); x < width; x++)
                {
                    if (x != width - 2 && y != height - 2 && crossword[x, y] != clueSymbol && (y - 2 >= 0 && crossword[x, y - 2] != clueSymbol) &&
                        (x + 2 < width && crossword[x + 2, y] != clueSymbol) && (x - 2 >= 0 && crossword[x - 2, y] != clueSymbol))
                    {
                        crossword[x, y] = clueSymbol;
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
                    if (y < height && crossword[x, y] != clueSymbol && crossword[x - 1, y] != clueSymbol && crossword[x - 2, y] != clueSymbol &&
                       (x + 1 < width && crossword[x + 1, y] != clueSymbol) && (x + 2 < width && crossword[x + 2, y] != clueSymbol)
                        && crossword[x, y - 1] != clueSymbol && crossword[x, y - 2] != clueSymbol && (y + 1 < height && crossword[x, y + 1] != clueSymbol)
                        && (y + 2 < height && crossword[x, y + 2] != clueSymbol))
                    {
                        Console.WriteLine(x + " " + y);
                        crossword[x, y] = clueSymbol;
                    }
                }


                bool hasClue = false;
                for (int y = 2; y < height; y++)
                {
                    if (crossword[x, y] == clueSymbol)
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
                        if (y < height - 2 && (x - 2 < 0 || x - 2 >= 0 && crossword[x - 2, y] != clueSymbol)
                            && (x + 2 >= width || (x + 2 < width && crossword[x + 2, y] != clueSymbol)))
                        {
                            crossword[x, y] = clueSymbol;
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
                crossword[index, height - 2] = clueSymbol;
                crossword[index, height - 1] = clueSymbol;
            }
            index = 1;
            while (index < height - Math.Max(rng.GetRandomNumber(), 6))
            {
                while ((number = rng.GetRandomNumber()) > height - 2 || number > 6) { }
                index += number;
                if (index > height - 3) break;
                crossword[width - 2, index] = clueSymbol;
                crossword[width - 1, index] = clueSymbol;
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
    }
}
