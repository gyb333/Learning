using Aliyun.OpenServices.OpenStorageService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.OSS
{
    public class OSSHelper
    {
        public void UpLoadFile(string strEndPoint, string strAccessID, string strAccessKey, string strBucketName, string strFullName, Stream stream, ObjectMetadata meta)
        {
            try
            {
                OssClient ossClient = new OssClient(strEndPoint, strAccessID, strAccessKey);
                PutObjectResult result = ossClient.PutObject(strBucketName, strFullName, stream, meta);//上传图片
            }
            catch 
            {
                throw;
            }
        }

        public void UpLoadFile( string strFullName, Stream stream, ObjectMetadata meta)
        {
            ISettings settings = SettingsFactory.GetSettings();
            UpLoadFile(settings.EndPoint, settings.AccessID, settings.AccessKey, settings.BucketName, strFullName, stream, meta);
        }

        public void UploadFile(string strFullName, Stream stream)
        {
             UpLoadFile(strFullName, stream,new ObjectMetadata());
        }

        public OssObject GetObject(string strEndPoint, string strAccessID, string strAccessKey, string strBucketName, string strFullName)
        {
            try
            {
                OssClient ossClient = new OssClient(strEndPoint, strAccessID, strAccessKey);
                OssObject ossObject = ossClient.GetObject(strBucketName, strFullName);
                return ossObject;
            }
            catch
            {
                throw;
            }

        }

        public OssObject GetObject(string strFullName)
        {
            try
            {
                ISettings settings = SettingsFactory.GetSettings();
                OssClient ossClient = new OssClient(settings.EndPoint, settings.AccessID, settings.AccessKey);
                OssObject ossObject = ossClient.GetObject(settings.BucketName, strFullName);

                return ossObject;
            }
            catch
            {
                throw;
            }

        }

        public string GetURL(string strEndPoint, string strAccessID, string strAccessKey, string strBucketName , string strFullName)
        {
            try
            {
                string strURL = string.Empty;
                OssClient ossClient = new OssClient(strEndPoint, strAccessID, strAccessKey);
                AccessControlList accs = ossClient.GetBucketAcl(strBucketName);
                if (!accs.Grants.Any())//判断是否有读取权限
                {
                    strURL = ossClient.GeneratePresignedUri(strBucketName, strFullName, DateTime.Now.AddMinutes(10)).AbsoluteUri; //生成一个签名的Uri 有效期5分钟
                }
                else
                {
                    strURL = string.Format("http://{0}.oss.aliyuncs.com/{1}", strBucketName, strFullName);
                }
                return strURL;
            }
            catch
            {
                throw;
            }
        }
        public string GetURL(string strFullName)
        {
            ISettings settings = SettingsFactory.GetSettings();
            return GetURL(settings.EndPoint, settings.AccessID, settings.AccessKey, settings.BucketName,strFullName);
        }
    }
    
}
