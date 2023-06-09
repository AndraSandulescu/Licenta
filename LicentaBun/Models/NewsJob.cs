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


            //var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ////var filePath = "D:\\UPB\\Licenta\\CronJob1\\CronJob1\\Models\\cron[{currentTime:HHmmss}].txt";
            //string fileName = Path.Combine(@"D:\UPB\Licenta\CronJob1\CronJob1\Models\", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt");
            //using (StreamWriter writer = new StreamWriter(fileName))
            //{
            //    writer.WriteLine($"Ora exactă a creării: {currentTime}");
            //}


            return Task.FromResult(true);
        }
    }
}
