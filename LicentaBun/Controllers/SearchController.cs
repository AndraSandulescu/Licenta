using LicentaBun.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System;

namespace LicentaBun.Controllers
{
    [Authorize]
    [Route("api/Search")]
    [ApiController]
    public class SearchController : Controller
    {
        private string model = "ModelTextblob";
        //private static string searchedFileCsv = "";
        
        //dictionar id-fisier
        private static Dictionary<int, SearchFile> searchFileDictionary = new Dictionary<int, SearchFile>();
        private static int searchFileIndex = 0;

        //dictionar id-comparePoliticiansCsv
        public static Dictionary<int, string> CsvFiles = new Dictionary<int, string>
        {
            {0, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\AlexandriaOcasioCortez_output.csv"},
            {1, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\BarackObama_output.csv"},
            {2, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\BillClinton_output.csv"},
            {3, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\ChuckSchumer_output.csv"},
            {4, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\CoryBooker_output.csv"},
            {5, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\ElizabethWarren_output.csv"},
            {6, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\HillaryClinton_output.csv"},
            {7, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\IlhanOmar_output.csv"},
            {8, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\JoeBiden_output.csv"},
            {9, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\KamalaHarris_output.csv"},
            {10, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\LindseyGraham_output.csv"},
            {11, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\MarcoRubio_output.csv"},
            {12, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\MichelleObama_output.csv"},
            {13, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\MitchMcConnell_output.csv"},
            {14, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\NancyPelosi_output.csv"},
            {15, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\RandPaul_output.csv"},
            {16, "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\ComparePoliticiansCsv\\TedCruz_output.csv"},
        };

        [AllowAnonymous]
        [HttpGet]
        [Route("SearchScript")]
        public IActionResult SearchScript([FromQuery] SearchRequest search, int userID)
        {
            string text = search.text;
            string username = search.username;
            string since = search.since;
            string until = search.until;
            string retweet = search.retweet;
            string replies = search.replies;
            int count = search.count ?? 0;

            string pythonPath = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python311\\python.exe"; // Specifică calea către interpretorul Python instalat pe serverul tău 
            string scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\SearchScript.py"; // Specifică calea către scriptul SearchScript.py
           


            // Inițializează un proces pentru a rula scriptul Python
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = pythonPath;
            psi.Arguments = $"{scriptPath} \"{text}\" \"{username}\" \"{since}\" \"{until}\" \"{retweet}\" \"{replies}\" {count}";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            Process pythonProcess = new Process();
            pythonProcess.StartInfo = psi;
            pythonProcess.Start();

            // Citește și afișează rezultatul scriptului Python
            string path = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\Csv\\";

            string outputFile = pythonProcess.StandardOutput.ReadToEnd();

            
            if(string.IsNullOrEmpty(outputFile))
            {
                return Ok("Nu s-au găsit Tweet-uri pentru parametrii introduși");
            }

            string output = path+ outputFile;
            output=output.Substring(0, output.Length - 2);

            pythonProcess.WaitForExit();

            //adaugat la dictionar
            SearchFile searchFile = new SearchFile
            {
                Filename = output,
                UserID = userID, // Înlocuiți "NumeUtilizator" cu utilizatorul care a făcut căutarea
                SearchInput = search
            };

            searchFileDictionary.Add(searchFileIndex, searchFile);


            

            searchFileIndex++;

            //ia din csv si pune in clasa -> json
            List<SearchCSV> results = new List<SearchCSV>();
            //string output = "__JoeBiden_climate change.csv";
            var reader = new StreamReader(output);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<SearchCSV>();
            results = records.ToList(); //lista SearchCSV
            //inchidere reader
            reader.Close();
            reader.Dispose();

            var response = new SearchResponse
            {
                Results = results,
                SearchIndex = searchFileIndex - 1
            };


            // Set multiple CORS headers
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            return Ok(response); // Returnează rezultatul scriptului Python ca răspuns HTTP 200
        }



        //script model
        [AllowAnonymous]
        [HttpGet]
        [Route("ModelScript")]
        public IActionResult ModelScript(int index, bool fromComparePoliticians)
        {
            SearchFile searchFile = new SearchFile();
            string inputFilename = "";






            if (searchFileDictionary.TryGetValue(index, out searchFile))
            {
                inputFilename = searchFile.Filename; //fisierul CSV gasit

                string pythonPath = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python311\\python.exe"; // Specifică calea către interpretorul Python instalat pe serverul tău

                //script default TextBlob
                string scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\ModelTextblob.py"; ;

                if (model == "ModelScript")
                {
                    scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\ModelScript.py"; //calea către script
                }
                else if (model == "ModelVader")
                {
                    scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\ModelVader.py"; //calea către script
                }
                else if (model == "ModelTextblob")
                {
                    scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\ModelTextblob.py"; //calea către script
                }

                // Inițializează un proces pentru a rula scriptul Python
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = pythonPath;
                psi.Arguments = $"{scriptPath} \"{inputFilename}\"";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;

                Process pythonProcess = new Process();
                pythonProcess.StartInfo = psi;
                pythonProcess.Start();

                // Citește și afișează rezultatul scriptului Python
                string output = pythonProcess.StandardOutput.ReadToEnd(); //deja are path ul complet
                pythonProcess.WaitForExit();

                if (string.IsNullOrEmpty(output))
                {
                    return Ok(new List<SentimentCSV>()); // Returnați un array gol în loc de un mesaj de eroare
                                                         //nu s-a gasit fisierul
                }

                output = output.Substring(0, output.Length - 2);
                //return Ok(output);

                //ia din csv si pune in clasa -> json; lista de linii de csv
                List<SentimentCSV> results = new List<SentimentCSV>();

                StreamReader reader = null;

                try
                {
                    reader = new StreamReader(output);
                    var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    var records = csv.GetRecords<SentimentCSV>();
                    results = records.ToList();

                    //procent poz-neg/total
                    int numPositive = results.Count(x => x.Sentiment == "positive");
                    int numTotal = results.Count;
                    float sum = results.Sum(x => x.Value);

                    //double average = records.Average(x => x.Value); //medie facuta altfel
                    float sentMed = sum / numTotal; //valoare medie a sentimentului identificat

                    //dictionar sentiment/luna
                    Dictionary<string, double> sentimentCountPerMonth = new Dictionary<string, double>();
                    //dictionar nr tweeturi/luna
                    Dictionary<string, int> tweetCountPerMonth = new Dictionary<string, int>();

                    foreach (var result in results)
                    {
                        DateTime dateTime = DateTime.Parse(result.searchCsv.DateTime);
                        string monthYear = dateTime.ToString("MMM yyyy");

                        // Contorizează sentimentul pe lună
                        if (!sentimentCountPerMonth.ContainsKey(monthYear))
                        {
                            sentimentCountPerMonth[monthYear] = 0;
                            tweetCountPerMonth[monthYear] = 0;
                        }

                        sentimentCountPerMonth[monthYear] += result.Value;
                        tweetCountPerMonth[monthYear]++;
                    }

                    // Calculează media sentimentului pe lună
                    foreach (var month in sentimentCountPerMonth.Keys.ToList())
                    {
                        sentimentCountPerMonth[month] /= tweetCountPerMonth[month];
                    }

                    reader.Close();
                    reader.Dispose();

                    ModelResponse response = new ModelResponse
                    {
                        Results = results,
                        SearchIndex = index,
                        PosTweets = numPositive,
                        TotalTweets = numTotal,
                        SentimentMediu = sentMed,
                        SentimentPerMonth = sentimentCountPerMonth,
                        TweetsPerMonth = tweetCountPerMonth
                    };

                    // Set multiple CORS headers
                    Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
                    Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                    Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                    return Ok(response); // Returnează rezultatul scriptului Python ca răspuns HTTP 200
                }
                finally
                {
                    reader?.Close();
                    reader?.Dispose();
                }
            }
            else
            {
                // Indexul nu a fost găsit în dicționar
                return NotFound();
            }
        }

    }

}
