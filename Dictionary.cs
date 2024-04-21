using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace BAK
{
    class Dictionary
    {

        public List<Word> wordsList { get; set; } = new List<Word>();
        public List<string> secretsList { get; set; } = new List<string>();
        public bool isCzechLanguage { get; set; } = true;
        public int difficulty { get; set; } = 1;
        private WordComparer comparer { get; } = new WordComparer();
        static string currentDirectory = System.Environment.CurrentDirectory;
        static string basePath = AppDomain.CurrentDomain.BaseDirectory + "Models\\";
        private string conStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + currentDirectory.Substring(0, currentDirectory.LastIndexOf("bin")) + "Directory.mdf\";Integrated Security=True"; //bude potřeba změnit, když přesunu soubor
        int limit = 150;

        private string longestWord { get; set; } = "";


        public Dictionary(int maxLength, bool isCzechLanguage, int difficulty)
        {
            this.isCzechLanguage = isCzechLanguage;
            this.difficulty = difficulty;
            SetDictionary(maxLength);
            SetSecrets();
        }

        public void SetSecrets()
        {
            Random random = new Random();
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                string query = "SELECT * FROM dbo.Secrets;"; // WHERE czechLanguage = " + isCzechLanguage + " AND difficulty = " + difficulty + ";";

                SqlCommand command = new SqlCommand(query, con);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string secr = reader["secret"].ToString();
                    secr = secr.Replace("’", "");
                    secretsList.Add(secr);
                }
            }
        }

        public void SetDictionary(int maxLength)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                int language = isCzechLanguage ? 1 : 0;
                string query = "SELECT * FROM dbo.Dictionary;"; // WHERE czechLanguage = " + isCzechLanguage + " AND difficulty = " + difficulty + ";";

                SqlCommand command = new SqlCommand(query, con);
                SqlDataReader reader = command.ExecuteReader();
                int n = 0;
                while (reader.Read())
                {
                    string w = reader["word"].ToString();
                    if (w.Length > longestWord.Length)
                    {
                        longestWord = w;
                    }
                    string c = reader["clue"].ToString();
                    if (w.Length > maxLength || w.Contains(" ") || w.Contains("-") || w.Contains("/") || w.Contains("-") || w.Contains("+") ||
                        w.Contains("5") || w.Contains("&"))
                    {
                        // Console.WriteLine(w);
                        continue;
                    };//todo smazat tyto znaky z db
                    w = vymazCarky(w);
                    if (isCzechLanguage && w.Contains("CH"))
                    {
                        w = w.Replace("CH", "6");
                    }
                    wordsList.Add(new Word(w, c));
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
            wordsList = wordsList.GroupBy(w => w.word)
                .Select(s => s.First())
                .OrderBy(x => rnd.Next())
                .ToList();
        }

        public List<Word> SelectWordsLengthSensitive(List<Word> usedWords, string[] containedLetters) 
        {
            Word w = new Word(string.Concat(containedLetters), "");
            List<Word> wordsFiltered = wordsList.AsParallel()
                .Except(usedWords.AsParallel())
                .Where(word => comparer.EqualsLengthSensitive(word, w))
                .Take(limit)
                .ToList();
            return wordsFiltered;
        }

        public Word GetRightClue(List<Word> usedWords, string[] wordContains)
        {
            Word word = new Word(string.Concat(wordContains), "");
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
            List<Word> wordsFiltered = wordsList.Except(usedWords)
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

        public bool ImpossibleToSelect(string containedLetters)
        {
            Word w = new Word(containedLetters, "");
            Random rnd = new Random();
            List<Word> wordsFiltered = wordsList.Where(word => comparer.EqualsLengthSensitive(word, w))
                .Take(1)
                .ToList();
            return (wordsFiltered.Count == 0);
        }
        public string vymazCarky(string word)
        {
            return word.Replace("Á", "A").Replace("É", "E").Replace("Í", "I")
                .Replace("Ó", "O").Replace("Ú", "U").Replace("Ů", "U").Replace("Ý", "Y");
        }
    }
}
