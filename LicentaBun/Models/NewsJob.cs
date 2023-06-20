using Quartz;
//using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace LicentaBun.Models
{
    public class NewsJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            //script:
            string pythonPath = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python311\\python.exe"; // Specifică calea către interpretorul Python instalat pe serverul tău 
            string scriptPath = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\PythonScripts\\NewsScript.py"; // Specifică calea către scriptul SearchScript.py

            // Inițializează un proces pentru a rula scriptul Python
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = pythonPath;
            psi.Arguments = $"{scriptPath}";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            Process pythonProcess = new Process();
            pythonProcess.StartInfo = psi;
            pythonProcess.Start();
            pythonProcess.WaitForExit();

            return Task.FromResult(true);
        }
    }
}
