using Aliyun.OpenServices.OpenStorageService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.OSS
{
    class Program
    {
        static void Main(string[] args)
        {
            //string strSourFilePath =@"C:\Users\zhongduzhi\Desktop\阿里云\整体架构.png";

            //// 打开文件
            //FileStream fileStream = new FileStream(strSourFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //// 读取文件的 byte[]
            //byte[] bytes = new byte[fileStream.Length];
            //fileStream.Read(bytes, 0, bytes.Length);
            //fileStream.Close();
            //// 把 byte[] 转换成 Stream
            //Stream stream = new MemoryStream(bytes);


            //string firstName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            //string lastName = Path.GetExtension(strSourFilePath);
            //string fullName = firstName + lastName;
            //string strPath = "images/"+fullName;

            string path = "Mobile/20141225/wanying-1453438913.jpg";
            OSSHelper ossHelper =new OSSHelper();

            string URL = ossHelper.GetURL(path);
            OssObject ossObeject = new OSSHelper().GetObject(path);

            Stream stream = ossObeject.Content;



        }


    }
}
