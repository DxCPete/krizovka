﻿using System;
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
        public bool languageCzech { get; set; } = true;
        public int difficulty { get; set; } = 1;
        private WordComparer comparer { get; } = new WordComparer();
        static string currentDirectory = System.Environment.CurrentDirectory;
        private string conStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + currentDirectory.Substring(0, currentDirectory.LastIndexOf("bin")) + "Directory.mdf\";Integrated Security=True"; //bude potřeba změnit, když přesunu soubor
        int limit = 150;

        private string longestWord { get; set; } = "";


        public Dictionary(int maxLength)
        {
            SetDictionary(maxLength);
            SetSecrets();
        }

        public string vymazCarky(string word)
        {
            return word.Replace("Á", "A").Replace("É", "E").Replace("Í", "I")
                .Replace("Ó", "O").Replace("Ú", "U").Replace("Ů", "U").Replace("Ý", "Y");
        }

        public void SetSecrets()
        {
            Random random = new Random();
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                int language = languageCzech ? 1 : 0;
                //todo přidat jazyk a obtížnost
                string query = "SELECT * FROM dbo.Secrets;"; // ORDER BY NEWID()";

                SqlCommand command = new SqlCommand(query, con);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string secr = reader["secret"].ToString();
                    secr = secr.Replace("'", "");
                    secretsList.Add(secr);
                    /*if (secr.Length >= maxLength * 1.5 && secr.Length < maxLength * 2.5)
                    {
                        return (secr);
                    }*/
                }
            }
        }

        public void VepsatTajenky()
        {
            int n = 0;
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            if (con.State == System.Data.ConnectionState.Open)
            {
                string filePath = @"C:\Users\Pete\OneDrive\Desktop\todo\tajenkaENG.txt";
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string tajenka;
                    while ((tajenka = reader.ReadLine()) != null)
                    {
                        tajenka = vymazCarky(tajenka.ToUpper());
                        Console.WriteLine(tajenka);
                        string query = "INSERT INTO dbo.Secrets (secret, czechLanguage, difficulty) VALUES ('" + tajenka + "',  'false', 1);";
                        SqlCommand command = new SqlCommand(query, con);
                        command.ExecuteNonQuery();
                    }
                    reader.Close();
                }
            }
            con.Close();
            Console.WriteLine(n);
        }


        public void SetDictionary(int maxLength)
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
            //wordsList = wordsList náhodně zamíchat
            wordsList = wordsList.GroupBy(w => w.word)
                .Select(s => s.First())
                .OrderBy(x => rnd.Next())
                .ToList();
        }



        public List<Word> SelectWords(List<Word> usedWords, string[] containedLetters) //ten hlavní CHYB V EQUALS
        {
            Word w = new Word(string.Concat(containedLetters), "");
            List<Word> wordsFiltered = wordsList.AsParallel()
                .Except(usedWords.AsParallel())
                .Where(word => comparer.Equals(word, w))
                .Take(limit)
                .ToList();
            return wordsFiltered;
        }
        public List<Word> SelectWordsNew(List<Word> usedWords, string[] containedLetters) //ten hlavní pro novou Sw
        {
            Word w = new Word(string.Concat(containedLetters), "");
            List<Word> wordsFiltered = wordsList.AsParallel()
                .Except(usedWords.AsParallel())
                .Where(word => comparer.EqualsNew(word, w))
                .Take(limit)
                .ToList();
            return wordsFiltered;
        }
        public List<Word> SelectWords(List<Word> usedWords, string[] wordContains, int length1, int length2)
        {
            Word w = new Word(string.Concat(wordContains), "");
            List<Word> wordsFiltered = wordsList.AsParallel()
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

        public bool ImpossibleToSelect(string containedLetters)//update kvůli CrosswordSw
        {
            Word w = new Word(containedLetters, "");
            Random rnd = new Random();
            List<Word> wordsFiltered = wordsList.Where(word => comparer.EqualsNew(word, w))
                .Take(1)
                .ToList();
            return (wordsFiltered.Count == 0);
        }


        /*  public bool ImpossibleToSelect(string[] wordContains) 
          {
              Word w = new Word(string.Concat(wordContains), "");
              Random rnd = new Random();
              List<Word> wordsFiltered = wordsList.Where(word => comparer.Contains(word, wordContains))
                  .Take(1)
                  .ToList();
              return (wordsFiltered.Count == 0);
          }*/

        public bool ImpossibleToSelectEquals(string[] wordContains)
        {
            Word w = new Word(string.Concat(wordContains), "");
            Random rnd = new Random();
            List<Word> wordsFiltered = wordsList.Where(word => comparer.Equals(word, w))
                .Take(1)
                .ToList();
            return (wordsFiltered.Count == 0);
        }

        public void InsertDataToDB()
        {
            SqlConnection con = new SqlConnection(conStr);

            con.Open();
            int n = 0;
            if (con.State == System.Data.ConnectionState.Open)
            {
                foreach (Word w in wordsList)
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
                    wordsList.Add(new Word(word, clue));
                }
            }
        }

        public Word TestGetWord(string clue)
        {
            return (Word)wordsList.Where(w => w.clue.Equals(clue))
                .First();
        }


        public void Remove(Word word)
        {
            wordsList.Remove(word);
        }

        public void Add(Word word)
        {
            wordsList.Add(word);
        }

        public int Length()
        {
            return wordsList.Count;
        }

        public string GetLongestWord()
        {
            return longestWord;
        }

        public void VypsatSlovnik()
        {
            foreach (Word s in wordsList)
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
