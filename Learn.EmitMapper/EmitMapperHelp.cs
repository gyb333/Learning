using EmitMapper;
using EmitMapper.MappingConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.EmitMapper
{
    public class Sourse
    {
        public int A;
        public decimal? B;
        public string C;
        public Inner D;
        public string E;
    }

    public class Dest
    {
        public int? A;
        public decimal B;
        public DateTime C;
        public Inner2 D;
        public string F;
    }

    public class Inner
    {
        public long D1;
        public Guid D2;
    }

    public class Inner2
    {
        public long D12;
        public Guid D22;
    }

    public class EmitMapperHelp
    {


        private void Use()
        {
            Sourse src = new Sourse
     {
         A = 1,
         B = 0M,
         C = "2011/9/21 0:00:00",
         D = new Inner
         {
             D2 = Guid.NewGuid()
         },
         E = "test"
     };

            ObjectsMapper<Sourse, Dest> mapper1 =
         new ObjectMapperManager().GetMapper<Sourse, Dest>(
             new DefaultMapConfig()
                //忽略
             .IgnoreMembers<Sourse, Dest>(new string[] { "A" })
                //为空赋值
             .NullSubstitution<decimal?, decimal>((value) => -1M)
                //两个实体之间名字不一致，需要映射
              .MatchMembers((x, y) =>
              {
                  if (x == "address" && y == "userAddress")
                  {
                      return true;
                  }
                  return x == y;
              })

                //有外键关联，需要映射出外键所带名字
             .ConvertUsing<Inner, Inner2>(value => new Inner2 { D12 = value.D1, D22 = value.D2 })
                //需要对某一个字段进行特殊处理
             .PostProcess<Dest>((value, state) =>
             {
                 value.F = "nothing";
                 return value;
             })
             );

            Dest dst = mapper1.Map(src);
        }

    }
}
