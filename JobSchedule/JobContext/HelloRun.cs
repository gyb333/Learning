#region License

/* 
 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved. 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */

#endregion

using System;
using System.Threading;

using Common.Logging;
#if !NET_20
#endif
using Quartz.Impl;
using JobSchedule.Jobs;
using Quartz;
using log4net;

namespace JobSchedule.JobContext
{
    /// <summary> 
    /// This Example will demonstrate how to start and shutdown the Quartz 
    /// scheduler and how to schedule a job to run in Quartz.
    /// </summary>
    /// <author>Bill Kratzer</author>
    /// <author>Marko Lahma (.NET)</author>
    public class HelloRun : IRun
    {
        public string Name
        {
            get { throw new NotImplementedException(); }
        }
   
        public virtual void Run()
        {
            //ILog log = LogManager.GetLogger(typeof (HelloRun));
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Info("------- Initializing ----------------------");

            // First we must get a reference to a scheduler
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();

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