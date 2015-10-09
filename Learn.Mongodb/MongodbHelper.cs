using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Learn.Mongodb
{
    public class MongodbHelper
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        private const string conn = "mongodb://127.0.0.1:27017";
        /// <summary>
        /// 指定的数据库
        /// </summary>
        private const string dbName = "dbName";
        /// <summary>
        /// 指定的表
        /// </summary>
        private const string tbName = "tbName";

        #region Server
        public MongoServer GetMongoServer()
        {
            return GetMongoServer(conn);
        }

        public MongoServer GetMongoServer(string conn)
        {
            //创建数据连接
            MongoServer server = MongoServer.Create(conn);
            return server;
        }

        #endregion

        #region Table
        public MongoCollection  GetMongoCollection (MongoServer server, string tbName)
        {
            //获取指定数据库
            MongoDatabase db = server.GetDatabase(dbName);
            //获取表
            return db.GetCollection(tbName);
        }

        public MongoCollection GetMongoCollection()
        {
            return GetMongoCollection(GetMongoServer(), tbName);
        }

        public MongoCollection<T> GetMongoCollection<T>(MongoServer server, string tbName)
        {
            //获取指定数据库
            MongoDatabase db = server.GetDatabase(dbName);
            //获取表
            return db.GetCollection<T>(tbName);
        }

        public MongoCollection<T> GetMongoCollection<T>()
        {
            return GetMongoCollection<T>(GetMongoServer(), tbName);
        }
        #endregion


        public  void Add<T>(T t)
        {
            //获取表
            MongoCollection<T> col = GetMongoCollection<T>();
            //插入
            col.Insert(t);

        }

        public  void Delete<T>(string strId)
        {
            MongoCollection<T> col = GetMongoCollection<T>();

            IMongoQuery query = Query.EQ("_id", new ObjectId(strId));

            col.Remove(query);
        }


        public void Update<T>(T t,string strId)
        {
            MongoCollection<T> col = GetMongoCollection<T>();

            BsonDocument bd = BsonExtensionMethods.ToBsonDocument(t);

            IMongoQuery query = Query.EQ("_id", new ObjectId(strId));

            col.Update(query, new UpdateDocument(bd));

        }


        public  List<T> SelectAll<T>()
        {
            List<T> list = new List<T>();

            MongoCollection<T> col = GetMongoCollection<T>();
            //查询全部
            list.AddRange(col.FindAll());
            return list;
            
        }

        /// <summary>
        /// 根据ObjectID 查询
        /// </summary>
        public  T SelectOne<T>(string objId)
        {
            MongoCollection<T> col = GetMongoCollection<T>();
            //条件查询            
            return col.FindOne(Query.EQ("_id", new ObjectId(objId)));
        }
    }
}
