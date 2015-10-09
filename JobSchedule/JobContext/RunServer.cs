using Common.Logging;
using JobSchedule.Jobs;
using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobSchedule.JobContext
{
    public class RunServer : IRun
    {
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public virtual void Run()
        {
            ILog log = LogManager.GetLogger(typeof(HelloRun));

            log.Info("------- Initializing ----------------------");

            // First we must get a reference to a scheduler
            //ISchedulerFactory sf = new StdSchedulerFactory();
            //IScheduler sched = sf.GetScheduler();
            IQuartzServer server = QuartzServerFactory.CreateServer();
            server.Initialize();
            IScheduler sched = server.GetScheduler();

            log.Info("------- Initialization Complete -----------");


            // computer a time that is on the next round minute
            DateTimeOffset runTime = DateBuilder.EvenMinuteDate(DateTimeOffset.UtcNow);

            log.Info("------- Scheduling Job  -------------------");

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<HelloJob>()
                .WithIdentity("job1", "group1")
                .Build();

            // Trigger the job to run on the next round minute
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartAt(runTime)
                .Build();

            // Tell quartz to schedule the job using our trigger
            sched.ScheduleJob(job, trigger);
            log.Info(string.Format("{0} will run at: {1}", job.Key, runTime.ToString("r")));

            // Start up the scheduler (nothing can actually run until the 
            // scheduler has been started)
            sched.Start();
            log.Info("------- Started Scheduler -----------------");

            // wait long enough so that the scheduler as an opportunity to 
            // run the job!
            log.Info("------- Waiting 65 seconds... -------------");

            // wait 65 seconds to show jobs
            Thread.Sleep(TimeSpan.FromSeconds(65));

            // shut down the scheduler
            log.Info("------- Shutting Down ---------------------");
            sched.Shutdown(true);
            log.Info("------- Shutdown Complete -----------------");
        }

    }
}
