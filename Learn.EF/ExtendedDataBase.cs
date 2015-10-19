using EFCachingProvider;
using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;
using EFTracingProvider;
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
using log4net;

namespace Learn.EF
{
    public class ExtendedDataBase:DbContext
    {

        private ILog logger;

    
        public ExtendedDataBase()

        {
    
 
        }
        public ExtendedDataBase(string connectionString)
            : base(EntityConnectionWrapperUtils.CreateEntityConnectionWithWrappers(
                    connectionString,
                    "EFTracingProvider",
                    "EFCachingProvider"
            ),false)
        {

        }

        private DbConnection Connection
        {
            get
            {
                IObjectContextAdapter oca = this as IObjectContextAdapter;
                if (oca != null && oca.ObjectContext != null)
                {
                    return oca.ObjectContext.Connection;
                }
                else
                {
                    return null;
                }
            }
        }


        #region Tracing Extensions

        private EFTracingConnection TracingConnection
        {
            get
            {
               
                return Connection.UnwrapConnection<EFTracingConnection>();
  
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
            if ( logger != null)
            {
                logger.Debug(e.ToTraceString().TrimEnd());                
            }
        }

        public  ILog Logger
        {
            get { return  logger; }
            set
            {
                if (( logger != null) != (value != null))
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

                logger = value;
            }
        }


        #endregion

        #region Caching Extensions

        private EFCachingConnection CachingConnection
        {
            get
            {
               return Connection.UnwrapConnection<EFCachingConnection>();
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
