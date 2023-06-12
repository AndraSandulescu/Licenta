using Microsoft.AspNetCore.Mvc;
using LicentaBun.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace LicentaBun.Controllers
{
    [Authorize]
    [Route("api/News")]
    [ApiController]

    public class NewsController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("NewsFeed")]
        public IActionResult LoadNews()
        {
            try
            {
                
                string path = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\NewsCsv\\news.csv"; //calea news.csv

                var reader = new StreamReader(path);
                var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<NewsItem>();
                var news = records.ToList(); // Lista NewsResponse

                reader.Close();
                reader.Dispose();



                DateTime lastWeek = DateTime.Now.AddDays(-7);
                var filteredResults = news.Where(r => DateTime.Parse(r.DateTime) >= lastWeek);

                // Căutare și numărare subiecte
                Dictionary<string, int> subjectCounts = new Dictionary<string, int>();

                foreach (var result in filteredResults)
                {
                    foreach (var subject in GetSubjectsList())
                    {
                        if (result.Text.Contains(subject, StringComparison.OrdinalIgnoreCase))
                        {
                            if (subjectCounts.ContainsKey(subject))
                                subjectCounts[subject]++;
                            else
                                subjectCounts[subject] = 1;
                        }
                    }
                }

                // Sortare după numărul de apariții și selectarea celor mai frecvente 3 subiecte
                var topSubjects = subjectCounts.OrderByDescending(kv => kv.Value).Take(3);

                NewsResponse response = new NewsResponse
                {
                    Results = news,
                    TopSubjects = topSubjects.Select(kv => kv.Key).ToList()
                };







                return Ok(response); // Returnează rezultatul ca răspuns HTTP 200
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // În caz de eroare, returnează un cod de eroare HTTP 500 și mesajul de eroare
            }
        }

        private List<string> GetSubjectsList()
        {
            return new List<string>()
            {
                "Corruption",
                "Transparency",
                "Arms Control",
                "War",
                "Nonproliferation",
                "Gun",
                "Climate",
                "Environment",
                "Terrorism",
                "COVID-19",
                "Energy",
                "Health"
            };
        }


    }
}