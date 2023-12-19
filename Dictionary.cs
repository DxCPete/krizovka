using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace BAK
{
    class Dictionary
    {

        public List<Word> dictionary { get; set; } = new List<Word>();
        public bool languageCzech { get; set; } = true;
        public int difficulty { get; set; } = 1;
        private WordComparer comparer { get; } = new WordComparer();
        static string currentDirectory = System.Environment.CurrentDirectory;
        private string conStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + currentDirectory.Substring(0, currentDirectory.LastIndexOf("bin")) + "Directory.mdf\";Integrated Security=True"; //bude potřeba změnit, když přesunu soubor
        int limit = 50;
        Dictionary<char, int> indexes;

        public Dictionary(int maxLength)
        {
            setDictionary(maxLength); //přidat k tomu podmínku, že musí být menší jak min(x,y)-1 z rozměrů křížovky
        }

        public void setDictionary(int maxLength)
        {
            SqlConnection con = new SqlConnection(conStr);

            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                int language = languageCzech ? 1 : 0;
                string query = "SELECT * FROM dbo.Dictionary;";// WHERE czechLanguage = " + language + " AND difficulty = " + difficulty + ";";

                SqlCommand command = new SqlCommand(query, con);
                SqlDataReader reader = command.ExecuteReader();
                int n = 0;
                while (reader.Read())
                {
                    string w = reader["word"].ToString();
                    string c = reader["clue"].ToString();
                    if (w.Length >= maxLength || w.Contains(" ") || w.Contains("-") || w.Contains("/") || w.Contains("-")) continue;
                    dictionary.Add(new Word(w, c));
                    n++;
                }
                Console.WriteLine(n);
                reader.Close();
                con.Close();
            }
            else
            {
                Console.WriteLine("Chyba při připojování se k DB.");
                throw new Exception("Could not connect to DB");
            }

            Random rnd = new Random();
            dictionary = dictionary.GroupBy(w => w.word)
                .Select(s => s.First())
                .OrderBy(w => w.word)
                .ToList();


            indexes = dictionary.Select((word, index) => new { Word = word, Index = index })
                .GroupBy(item => item.Word.word[0])
                .ToDictionary(group => group.Key, group => group.First().Index);




        }



        public List<Word> SelectWords(List<Word> usedWords, string[] containedLetters) //ten hlavní
        {
            Word w = new Word(string.Concat(containedLetters), "");
            int startIndex = 0;
            if (Char.IsLetter(char.Parse(containedLetters[0])))
            {
                startIndex = indexes[char.Parse(containedLetters[0])];
            }
            else
            {
                startIndex = new Random().Next(startIndex, dictionary.Count - 1000);
            }
            List<Word> wordsFiltered = dictionary.AsParallel()
                .Skip(startIndex)
                .Except(usedWords.AsParallel())
                .Where(word => comparer.Equals(word, w))
                .Take(limit)
                .ToList();
            return wordsFiltered;
        }

        public List<Word> SelectWords(List<Word> usedWords, string[] wordContains, int length1, int length2)
        {
            Word w = new Word(string.Concat(wordContains), "");
            List<Word> wordsFiltered = dictionary.AsParallel()
                .Except(usedWords.AsParallel())
                .Where(word => comparer.Equals(word, w) && (word.word.Length == length1 || word.word.Length == length2))
                .Take(limit)
                .ToList();
            return wordsFiltered;
        }

        public Word GetRightClue(List<Word> usedWords, string[] wordContains)
        {
            Word word = new Word(string.Concat(wordContains), "");
            // Word rightWord = (Word)usedWords.Where(w => w.word.Equals(word.word));
            List<Word> list = usedWords.Where(w => w.word.Equals(word.word)).ToList();
            if (list.Count <= 0)
            {
                return new Word("", "");
            }
            return list[0];
        }

        public Word SelectWord(string[] wordContains, List<Word> usedWords)
        {
            return SelectWord(wordContains, usedWords, 100);
        }

        public Word SelectWord(string[] wordContains, List<Word> usedWords, int maxLength)
        {
            string lettersContained = string.Concat(wordContains);
            lettersContained = lettersContained.Substring(0, Math.Min(maxLength, lettersContained.Length));
            Word word = new Word(lettersContained, "");
            List<Word> wordsFiltered = dictionary.Except(usedWords)
                .AsParallel()
                .Where(w => comparer.Equals(w, word) && w.word.Length < maxLength && w.word.Length > 2).Take(20) //limit
                .ToList();
            if (wordsFiltered.Count == 0)
            {
                return new Word("", "");
            }

            Random rnd = new Random();
            return wordsFiltered[rnd.Next(wordsFiltered.Count)];
        }



        public Word SelectWord(List<Word> usedWords, string[] wordContains, /*jenom pro test*/string[,] crossword, int x, int y, bool horizontalDirection)
        {
            try
            {
                Word word = new Word(string.Concat(wordContains), "");
                Word selectedWord = (Word)usedWords.Where(w => comparer.Equals(w, word))
                    .First();
                return selectedWord;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Posralo se to pro: " + string.Concat(wordContains));
                Console.WriteLine(ex);
                throw ex;
            }
        }

        public bool ImpossibleToSelect(string containedLetters)
        {
            Word w = new Word(containedLetters, "");
            Random rnd = new Random();
            List<Word> wordsFiltered = dictionary.Where(word => comparer.StartsWith(word, w))
                .Take(1)
                .ToList();
            return (wordsFiltered.Count == 0);
        }


        public bool ImpossibleToSelect(string[] wordContains)
        {
            Word w = new Word(string.Concat(wordContains), "");
            Random rnd = new Random();
            List<Word> wordsFiltered = dictionary.Where(word => comparer.StartsWith(word, w))
                .Take(1)
                .ToList();
            return (wordsFiltered.Count == 0);
        }

        public bool ImpossibleToSelectEquals(string[] wordContains)
        {
            Word w = new Word(string.Concat(wordContains), "");
            Random rnd = new Random();
            List<Word> wordsFiltered = dictionary.Where(word => comparer.Equals(word, w))
                .Take(1)
                .ToList();
            return (wordsFiltered.Count == 0);
        }

        public void InsertDataToDB()
        { //C:\\Users\\petro\\OneDrive\\Desktop\\BAK - C#\\Directory.mdf
            SqlConnection con = new SqlConnection(conStr);

            con.Open();
            int n = 0;
            if (con.State == System.Data.ConnectionState.Open)
            {
                foreach (Word w in dictionary)
                {
                    if (w.word.Contains(" ") || w.word.Contains("-") || w.word.Contains(",") || w.word.Contains(".")) continue;
                    if (w.word.StartsWith("A ") || w.word.StartsWith("AN ") || w.word.StartsWith("THE "))
                    {
                        w.word = w.word.Substring(w.word.IndexOf(" ") + 1);
                    }
                    if (w.clue.Contains("'"))
                    {
                        w.clue = w.clue.Replace("\'", "");
                    }
                    if (w.word.Contains("'"))
                    {
                        w.word = w.word.Replace("\'", "");
                    }
                    string query = "INSERT INTO dbo.Dictionary (word, clue, czechLanguage, difficulty) VALUES ('" + w.word.ToUpper() + "', '" + w.clue.ToUpper() + "', 'true', 1);";


                    SqlCommand command = new SqlCommand(query, con);
                    command.ExecuteNonQuery();
                    n++;
                }
                Console.WriteLine("Nových slov: " + n);
            }
            else
            {
                Console.WriteLine("error");
            }
            con.Close();
        }

        void FileToDictionary()
        {
            string filePath = @"";

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                string word;
                string clue;
                while ((line = sr.ReadLine()) != null)
                {
                    word = line.Substring(0, line.IndexOf(" "));
                    clue = line.Substring(line.IndexOf(" "));
                    dictionary.Add(new Word(word, clue));
                }
            }
        }


        public void Remove(Word word)
        {
            dictionary.Remove(word);
        }

        public void Add(Word word)
        {
            dictionary.Add(word);
        }

        public int Length()
        {
            return dictionary.Count;
        }


        public void VypsatSlovnik()
        {
            foreach (Word s in dictionary)
            {
                s.Print();
            }
        }

        internal bool Any(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
