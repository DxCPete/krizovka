using System;

namespace BAK
{
    class Word
    {
        public string word { get; set; }
        public string clue { get; set; }

        public Word(string word, string clue)
        {
            this.word = word;
            this.clue = clue;
        }

        public void Print()
        {
            Console.WriteLine("Slovo {0} s legendou {1}", word, clue);
        }

        public Word Clone()
        {
            Word clone = new Word(this.word, this.clue);
            return clone;
        }
    }
}
