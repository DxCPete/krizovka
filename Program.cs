

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace BAK
{
    class Program
    {

        static void Main(string[] args)
        {
            //new CrosswordSw(15,15);


            CrosswordSw cs;
            int celkovyPocetDeadEnd = 0;
            int celkovyPocetImpossiblePaths = 0;
            int celkovyPocetSlov = 0;
            int celkovyPocetPouzitychSlov = 0;
            int n = 5;
            for (int i = 0; i < n; i++)
            {
                cs = new CrosswordSw(30,30);
                celkovyPocetDeadEnd += cs.ukoncenoNaDeadEnd;
                celkovyPocetImpossiblePaths += cs.pocetNesplnitelnychCest;
                celkovyPocetSlov += cs.pocetPouzitychSlov;
                celkovyPocetPouzitychSlov += cs.usedWords.Count;
                Console.WriteLine(i);
            }

            Console.WriteLine("DeadEnd: " + celkovyPocetDeadEnd/n);
            Console.WriteLine("ImpossiblePaths: " + celkovyPocetImpossiblePaths / n);
            Console.WriteLine("PocetSlov: " + celkovyPocetSlov / n);
            Console.WriteLine("PocetUzitychSlov: " + celkovyPocetPouzitychSlov / n);
            
        }
    }
}
