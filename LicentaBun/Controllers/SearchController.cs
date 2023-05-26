using LicentaBun.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LicentaBun.Controllers
{
    [Authorize]
    [Route("api/Search")]
    [ApiController]
    public class SearchController : Controller
    {
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
            string output = pythonProcess.StandardOutput.ReadToEnd();
            pythonProcess.WaitForExit();

            return Ok(output); // Returnează rezultatul scriptului Python ca răspuns HTTP 200
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SearchScriptNew")]
        public IActionResult SearchScriptNew([FromQuery] SearchRequest search)
        {
            string text = search.text;
            string username = search.username;
            string since = search.since;
            string until = search.until;
            string retweet = search.retweet;
            string replies = search.replies;
            int count = search.count ?? 0;

            string pythonPath = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python311\\python.exe"; // Specifică calea către interpretorul Python instalat pe serverul tău
            string scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\SearchScriptNew.py"; // Specifică calea către scriptul SearchScript.py

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
            string output = pythonProcess.StandardOutput.ReadToEnd();
            pythonProcess.WaitForExit();

            return Ok(output); // Returnează rezultatul scriptului Python ca răspuns HTTP 200
        }


    }
}
