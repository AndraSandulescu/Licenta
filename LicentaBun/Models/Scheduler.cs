using Quartz.Impl;
using Quartz;

namespace LicentaBun.Models
{
    public class Scheduler
    {
        public static async Task Start()
        {
            // Obțineți un scheduler
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            // Creați un job și un trigger
            IJobDetail job = JobBuilder.Create<NewsJob>()
                .WithIdentity("newsScrapingJob", "newsScrapingGroup")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("newsScrapingJob", "newsScrapingGroup")
                .WithCronSchedule("0 0 9 * * ?")
                .Build();

            // Programați job-ul cu trigger-ul
            await scheduler.ScheduleJob(job, trigger);

            // Porniți scheduler-ul
            await scheduler.Start();
        }
    }
}
