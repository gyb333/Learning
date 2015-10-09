using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Mongodb
{
    public sealed class Customer
    {

        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Tel { get; set; }
        public DateTime Createdate { get; set; }
    }

    class Program
    {
       

        static void Main(string[] args)
        {
            //链接字符串
            string connectionString = "mongodb://localhost";

            //数据库名
            string databaseName = "myDatabase";

            //定义Mongo服务
            MongoServer server = MongoServer.Create(connectionString);

            //获取databaseName对应的数据库，不存在则自动创建
            MongoDatabase mongoDatabase = server.GetDatabase(databaseName) as MongoDatabase;

            MongoCollection<BsonDocument> books = mongoDatabase.GetCollection<BsonDocument>("books");


            //链接数据库
            server.Connect();
            try
            {
                BsonDocument book = new BsonDocument 
                {
                    { "author", "Ernest Hemingway" },
                    { "title", "For Whom the Bell Tolls" }
                };
                books.Insert(book);

                var query = new QueryDocument("author", "Ernest Hemingway");
                foreach (BsonDocument bookItem in books.Find(query))
                {
                    Console.WriteLine(book["author"]);
                }
            }
            finally
            {
                //关闭链接
                server.Disconnect();
            }
            Console.Read();
        }
    }
}
