using LicentaBun.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using System.IO;

namespace LicentaBun.Controllers
{
    [Authorize]
    [Route("api/Search")]
    [ApiController]
    public class SearchController : Controller
    {
        //private static string searchedFileCsv = "";
        
        //dictionar id-fisier
        private static Dictionary<int, string> searchFileDictionary = new Dictionary<int, string>();
        private static int searchFileIndex = 0;

        //private string modelFileCsv;

        [AllowAnonymous]
        [HttpGet]
        [Route("SearchScript")]
        public IActionResult SearchScript([FromQuery] SearchRequest search)
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

            //searchedFileCsv = output;
            searchFileDictionary.Add(searchFileIndex, output);
            searchFileIndex++;

            //ia din csv si pune in clasa -> json
            List<SearchCSV> results = new List<SearchCSV>();
            //string output = "__JoeBiden_climate change.csv";
            var reader = new StreamReader(output);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<SearchCSV>();
            results = records.ToList(); //lista SearchCSV

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
        public IActionResult ModelScript(int index)
        {

            string inputFilename = string.Empty;
            if (searchFileDictionary.TryGetValue(index, out inputFilename))
            {
                string pythonPath = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python311\\python.exe"; // Specifică calea către interpretorul Python instalat pe serverul tău
                string scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\ModelScript.py"; // Specifică calea către scriptul SearchScript.py

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
                    return Ok("Nu s-a găsit fisierul cautat");
                }
                output = output.Substring(0, output.Length - 2);
                //return Ok(output);

                //ia din csv si pune in clasa -> json
                List<SentimentCSV> results = new List<SentimentCSV>();
                var reader = new StreamReader(output);
                var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<SentimentCSV>();
                results = records.ToList(); //lista SearchCSV

                // Set multiple CORS headers
                Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                return Ok(results); // Returnează rezultatul scriptului Python ca răspuns HTTP 200
            }
            else
            {
                // Indexul nu a fost găsit în dicționar
                return NotFound();
            }
        }
    }
}
