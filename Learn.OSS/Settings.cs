using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.OSS
{
    public class Settings:ISettings
    {
        public string EndPoint
        {
            get { return ConfigurationManager.AppSettings["EndPoint"]; }
        }

        public string AccessID
        {
            get { return ConfigurationManager.AppSettings["AccessID"]; }
        }

        public string AccessKey
        {
            get { return ConfigurationManager.AppSettings["AccessKey"]; }
        }

        public string BucketName
        {
            get { return ConfigurationManager.AppSettings["BucketName"]; }
        }
    }
}
