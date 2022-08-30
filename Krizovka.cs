using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BAK
{
    /* '7' je prozatimní substituce za LEGENDU */
    class Krizovka
    {
        public string jazyk { get; set; }
        public int šířka { get; set; }
        public int výška { get; set; }
        public Slovnik slovnik { get; set; }
        Slovo[] slovnikPouzitychSlov = new Slovo[100];

        public char[,] krizovka { get; set; }                                                               //předělat na string[,]
        public Krizovka(int šířka, int výška)                                                                       //pak přidat jazyk
        {
            slovnik = new Slovnik();
            //slovnik.SeradPodleDelky(); teď nevyužívám
            this.šířka = šířka;                                                                            
            this.výška = výška;
            krizovka = new char[this.šířka, this.výška];
            slovnikPouzitychSlov[0] = new Slovo("", "");
        }

        public bool JePlna()
        {
            for (int i = 0; i < this.výška; i += 1)
            {
                for (int j = 0; j < this.šířka; j += 1)
                {
                    if (!Char.IsLetter(this.krizovka[j, i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool SlovoSeVleze(Slovo slovo, int x, int y, bool vodorovnySmer) 
        {
            int n = slovo.slovo.Length + 1;                                                                 //      1 je KVŮLI LEGENDĚ 
            if (vodorovnySmer)
            {
                if (šířka >= x + n) return true;
            }
            else
            {
                if (výška >= y + n) return true;
            }

            return false;
        }


        public (int x, int y) NajdiPrazdnePolicko() /* (int, int) souradnicePrazdne = NajdiPrazdnePolicko(); */
        {
            for (int i = 0; i < this.výška; i += 1)
            {
                for(int j = 0; j < this.šířka; j += 1)
                {
                    if (krizovka[j,i] == '\0')
                    {
                        return (j, i);
                    }
                }
            }
            return (-1, -1);
        }

        public Slovo SlovoVyber(int slovoDelka, string slovoObsahuje)
        {
            int slovnikDelka = slovnik.Delka();
            Console.WriteLine(slovoDelka.ToString() + slovoObsahuje);
            while (true)
            {
                Thread.Sleep(30);
                var r = new Random();
                int cisloNahodne = r.Next(0, slovnikDelka);
                Slovo slovoNahodne = slovnik[cisloNahodne];
                if ((slovoDelka == slovoNahodne.slovo.Length || slovoDelka - 2 > slovoNahodne.slovo.Length)         //aby tam nezůstávalo místo pro slova s délkou 1
                    && slovoNahodne.slovo.Contains(slovoObsahuje)){ // && SlovoNebyloPouzito(slovoNahodne)){
                    return slovoNahodne;
                }
            }
        }

        /*public bool SlovoNebyloPouzito(Slovo slovo)
        {
            int i = 0;
            while(!string.IsNullOrEmpty(slovnikPouzitychSlov[i].slovo))
            {
                if (!slovo.Rovnost(slovnikPouzitychSlov[i]))
                {
                    return false;
                }
            }
            return true;
        }*/

        public (int x, int y, bool vodorovnySmer) PolickoProNoveSlovo(int x, int y)  //bude potřeba upravit
        {
            if (x != 0) x -= 1;
            if (y != 0) y -= 1;

            for (int i = y; i < this.výška - 1; i += 1)
            {
                for (int j = y; j < this.šířka - 1; j += 1)
                {
                    if (krizovka[j, i] == '7')
                    {
                        if (krizovka[j + 1, i] == '\0' && krizovka[j, i + 1] == '\0') // pokud je volno napravo i dole, tak vybere jeden směr náhodně
                        {
                            var rand = new Random();
                            int r = rand.Next(2);
                            if (r == 0)
                            {
                                return (j + 1, i, false);
                            }
                            else
                            {
                                return (j, i + 1, true);
                            }
                        }
                        else if (krizovka[j + 1, i] == '\0')
                        {
                            return (j + 1, i, false);
                        }
                        else if (krizovka[j, i + 1] == '\0')
                        {
                            return (j, i + 1, true);
                        }
                    }
                    else if (krizovka[j, i] == '\0')
                    {
                        if (krizovka[j + 1, i] == '\0')
                        {
                            return (j, i, true);
                        }
                        else if (krizovka[j, i+1] == '\0')
                        {
                            return (j, i, false);
                        }
                    }
                }
            }
            return (-1, -1, false);
        }

        public int SlovoMaximalniDelka(int x, int y, bool vodorovnySmer)
        {
            int i = 0;
            if (vodorovnySmer)
            {
                while(x+i < this.šířka && krizovka[x + i, y] != '7')
                {
                    i += 1;
                }
            }
            else
            {
                while (y + i < this.výška && krizovka[x, y + i] != '7')
                {
                    i += 1;
                }
            }
            return i - 1; /* -1 protože ještě legenda*/
        }
        
        public string ObsazenaPismena(int x, int y, bool vodorovnySmer) /* NEFUNGUJE */
        {
            string pismena = "";
            int i = 0;
            if (vodorovnySmer)
            {
                while (x + i < šířka && krizovka[x + i, y] != '7' )
                {
                    if (char.IsLetter(krizovka[x + i, y]))
                    {
                        Console.WriteLine(krizovka[x + i, y]);
                        pismena = pismena + krizovka[x + i, y];
                    }
                    i += 1;
                }
            }
            else
            {
                while (y + i < výška && krizovka[x, y + i] != '7' )
                {
                    if (char.IsLetter(krizovka[x, y + i]))
                    {
                        pismena = pismena + krizovka[x, y + i];
                    }
                    i += 1;
                }
            }

            return pismena;
        }

        public void LegendaZapsat(Slovo slovo, int x, int y)
        {
            krizovka[x, y] = '7';
        }

        public void SlovoZapsat(Slovo slovo, int x, int y, bool vodorovnySmer)
        {
            int n = slovo.slovo.Length;
            if (vodorovnySmer)
            {
                x += 1; // legenda
                for (int i = 0; i < n; i += 1) 
                {
                    krizovka[x + i, y] = slovo.slovo[i];
                }
            }
            else
            {
                y += 1; //legenda
                for (int i = 0; i < n; i += 1)
                {
                    krizovka[x, y + i] = slovo.slovo[i];
                }
            }
        }

        public void Dopln()
        {
            bool krizovkaJePlna = false;
            int x = 0;
            int y = 0;
            int pocetSlovZapsanych = 0;
            bool vodorovnySmer;
            Slovo slovoNove;
            while (!krizovkaJePlna)
            {
                (int, int, bool) t = PolickoProNoveSlovo(x, y);
                x = t.Item1;
                if (x == -1) break;
                y = t.Item2;
                vodorovnySmer = t.Item3;
                
                int maxDelka = SlovoMaximalniDelka(x, y, vodorovnySmer);
                string obsazenaPismena = ObsazenaPismena(x, y, vodorovnySmer);
                Console.WriteLine("obsazena pismena: " + obsazenaPismena);
                slovoNove = SlovoVyber(maxDelka, obsazenaPismena);

                Console.WriteLine("slovo: " + slovoNove.slovo);

                LegendaZapsat(slovoNove, x, y);
                SlovoZapsat(slovoNove, x, y, vodorovnySmer);
        
                slovnikPouzitychSlov[pocetSlovZapsanych] = slovoNove;
                pocetSlovZapsanych += 1;
             
                krizovkaJePlna = JePlna();
            }
        }
        public void Vypsat()
        {
            for (int i = 0; i < výška; i +=1)
            {
                for (int j = 0; j < šířka; j +=1)
                {
                    Console.Write(krizovka[j, i] + " | ");
                }
                Console.WriteLine();
            }
        }

    }
}
