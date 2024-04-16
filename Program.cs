

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
      

            new CrosswordSw(20,21);
            /*CrosswordSw cs;
            int celkovyPocetDeadEnd = 0;
            int celkovyPocetImpossiblePaths = 0;
            int celkovyPocetSlov = 0;
            int celkovyPocetPouzitychSlov = 0;
            int n = 1;
            for (int i = 0; i < n; i++)
            {
                cs = new CrosswordSw(25,25);
                celkovyPocetDeadEnd += cs.ukoncenoNaDeadEnd;
                celkovyPocetImpossiblePaths += cs.pocetNesplnitelnychCest;
                celkovyPocetSlov += cs.pocetPouzitychSlov;
                celkovyPocetPouzitychSlov += cs.usedWords.Count;
            }

            Console.WriteLine("DeadEnd: " + celkovyPocetDeadEnd/n);
            Console.WriteLine("ImpossiblePaths: " + celkovyPocetImpossiblePaths / n);
            Console.WriteLine("PocetSlov: " + celkovyPocetSlov / n);
            Console.WriteLine("PocetUzitychSlov: " + celkovyPocetPouzitychSlov / n);
            */
        }
    }
}
