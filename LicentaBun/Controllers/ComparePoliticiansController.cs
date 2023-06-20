using Microsoft.AspNetCore.Mvc;
using LicentaBun.Models;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace LicentaBun.Controllers
{
    [Authorize]
    [Route("api/ComparePoliticians")]
    [ApiController]

    public class ComparePoliticiansController : Controller
    {
        //dictionar id-comparePoliticiansCsv
        public static Dictionary<int, string> ComparePoliticiansCsv = new Dictionary<int, string>
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

       
        [HttpGet]
        [Route("Compare")]
        public IActionResult Compare([FromQuery] CompareRequest compare)
        {

            string text = compare.text;

            string since = compare.formattedSince;
            string until = compare.formattedUntil;

            int politician1 = compare.politician1 ?? -1;
            int politician2 = compare.politician2 ?? -1;


            if (!ComparePoliticiansCsv.ContainsKey(politician1) && !ComparePoliticiansCsv.ContainsKey(politician2))
            {
                return Ok("Nu ați selectat niciun politician pentru a realiza comparația");
            }
            List<SentimentCSV> results = new List<SentimentCSV>();
            ModelResponse response1 = new ModelResponse();
            ModelResponse response2 = new ModelResponse();

            string filePolitician1 = "";
            string filePolitician2 = "";
            if (politician1!=-1 && ComparePoliticiansCsv.TryGetValue(politician1, out filePolitician1))
            {
                results = SearchEntriesInCsv(filePolitician1, since, until, text);
                response1 = CreateModelResponse(results, politician1);
            }

            if (politician2 != -1 && ComparePoliticiansCsv.TryGetValue(politician2, out filePolitician2))
            {
                results = SearchEntriesInCsv(filePolitician2, since, until, text);
                response2 = CreateModelResponse(results, politician2);
            }
            CompareResponse compareResponse = new CompareResponse();
            compareResponse.Response1 = response1;
            compareResponse.Response2 = response2;

            // Set multiple CORS headers
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            return Ok(compareResponse);

            // Returnați ce este necesar în cazul în care nu se găsește niciun fișier
        }

        private List<SentimentCSV> SearchEntriesInCsv(string csvFilePath, string since, string until, string text)
        {

            //trateaza momentele cand since si until sunt egale cu null

            //trateaza moemnt cand text e null

            List<SentimentCSV> matchingEntries = new List<SentimentCSV>();

            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                matchingEntries = csv.GetRecords<SentimentCSV>()
                    .Where(entry =>
                    {
                        DateTime entryDate = DateTime.Parse(entry.searchCsv.DateTime);
                        DateTime sinceDate = DateTime.Parse(since);
                        DateTime untilDate = DateTime.Parse(until);
                        return entryDate >= sinceDate && entryDate <= untilDate && entry.searchCsv.Text.Contains(text);
                    })
                    .ToList();
            }

            return matchingEntries;
        }

        private ModelResponse CreateModelResponse(List<SentimentCSV> results, int searchIndex)
        {
            int numPositive = results.Count(x => x.Sentiment == "positive");
            int numTotal = results.Count;
            float sum = results.Sum(x => x.Value);
            float sentMed = 0;

            if (float.IsFinite(sum))
            {
                sentMed = sum / numTotal;
            }

            if (float.IsNaN(sentMed))
            {
                sentMed = 0;
            }

            Dictionary<string, double> sentimentCountPerMonth = new Dictionary<string, double>();
            Dictionary<string, int> tweetCountPerMonth = new Dictionary<string, int>();

            foreach (var result in results)
            {
                DateTime dateTime = DateTime.Parse(result.searchCsv.DateTime);
                string monthYear = dateTime.ToString("MMM yyyy");

                if (!sentimentCountPerMonth.ContainsKey(monthYear))
                {
                    sentimentCountPerMonth[monthYear] = 0;
                    tweetCountPerMonth[monthYear] = 0;
                }

                sentimentCountPerMonth[monthYear] += result.Value;
                tweetCountPerMonth[monthYear]++;
            }

            foreach (var month in sentimentCountPerMonth.Keys.ToList())
            {
                sentimentCountPerMonth[month] /= tweetCountPerMonth[month];
            }

            if (float.IsNegativeInfinity(sentMed))
            {
                sentMed = 0;
            }

            if (float.IsPositiveInfinity(sentMed))
            {
                sentMed = 1;
            }

            return new ModelResponse
            {
                Results = results,
                SearchIndex = searchIndex,
                PosTweets = numPositive,
                TotalTweets = numTotal,
                SentimentMediu = sentMed,
                SentimentPerMonth = sentimentCountPerMonth,
                TweetsPerMonth = tweetCountPerMonth
            };
        }
    }
}