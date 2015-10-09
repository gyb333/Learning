using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.OSS
{
    public interface ISettings
    {
        string EndPoint { get; }

        string AccessID { get; }

        string AccessKey { get; }

        string BucketName { get; }
    }
}
