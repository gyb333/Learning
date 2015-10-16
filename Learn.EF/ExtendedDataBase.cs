using EFCachingProvider;
using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;
using EFTracingProvider;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFTracingProvider;

namespace Learn.EF
{
    public class ExtendedDataBase:DbContext
    {
        private TextWriter logOutput;
        //private static ILog logger;
        public ExtendedDataBase()

        {
            //// 初始化Log4net,配置在独立的"log4net.config"中配置
            //log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            //// 初始化一个logger
            //logger = log4net.LogManager.GetLogger("EFLog4net");

           
            
            // 输出 TraceString (SQL文)
            //logger.Debug(Environment.NewLine + e.ToTraceString().TrimEnd());
 
        }
        public ExtendedDataBase(string connectionString)
            : base(EntityConnectionWrapperUtils.CreateEntityConnectionWithWrappers(
                    connectionString,
                    "EFTracingProvider",
                    "EFCachingProvider"
            ),false)
        {

        }



        #region Tracing Extensions

        private EFTracingConnection TracingConnection
        {
            get
            {
                IObjectContextAdapter  oca = this as IObjectContextAdapter;
                return oca.ObjectContext.Connection.UnwrapConnection<EFTracingConnection>();
  
            }
        }

        public event EventHandler<CommandExecutionEventArgs> CommandExecuting
        {
            add { this.TracingConnection.CommandExecuting += value; }
            remove { this.TracingConnection.CommandExecuting -= value; }
        }

        public event EventHandler<CommandExecutionEventArgs> CommandFinished
        {
            add { this.TracingConnection.CommandFinished += value; }
            remove { this.TracingConnection.CommandFinished -= value; }
        }

        public event EventHandler<CommandExecutionEventArgs> CommandFailed
        {
            add { this.TracingConnection.CommandFailed += value; }
            remove { this.TracingConnection.CommandFailed -= value; }
        }

        private void AppendToLog(object sender, CommandExecutionEventArgs e)
        {
            if (this.logOutput != null)
            {
                this.logOutput.WriteLine(e.ToTraceString().TrimEnd());
                this.logOutput.WriteLine();
            }
        }

        public TextWriter Log
        {
            get { return this.logOutput; }
            set
            {
                if ((this.logOutput != null) != (value != null))
                {
                    if (value == null)
                    {
                        CommandExecuting -= AppendToLog;
                    }
                    else
                    {
                        CommandExecuting += AppendToLog;
                    }
                }

                this.logOutput = value;
            }
        }


        #endregion

        #region Caching Extensions

        private EFCachingConnection CachingConnection
        {
            get
            {
                IObjectContextAdapter oca = this as IObjectContextAdapter;
                return oca.ObjectContext.Connection.UnwrapConnection<EFCachingConnection>();
            }
        }

        public ICache Cache
        {
            get { return CachingConnection.Cache; }
            set { CachingConnection.Cache = value; }
        }

        public CachingPolicy CachingPolicy
        {
            get { return CachingConnection.CachingPolicy; }
            set { CachingConnection.CachingPolicy = value; }
        }

        #endregion


    }
}
