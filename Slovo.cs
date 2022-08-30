using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAK
{
    class Slovo
    {
        public string slovo { get; set; }
        public string legenda { get; set; }
        public Slovo(string slovo1, string legenda1)
        {
            this.slovo = slovo1;
            this.legenda = legenda1;
        }

        public bool Rovnost(Slovo slovo2)
        {
            if ((string.Equals(this.slovo, slovo2.slovo)) || (string.Equals(this.legenda, slovo2.legenda))) {
                return true;
            }
            return false;
        }

        public void Vypsat()
        {
            Console.WriteLine("Slovo {0} s legendou {1}", slovo, legenda);
        }
    }
}
