using JobSchedule.JobContext;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JobSchedule
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);


                HelloRun instance = new HelloRun();
                instance.Run();
                //Application.Run(new Form1());
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running example: " + ex.Message);
                Console.WriteLine(ex.ToString());

            }

        }
    }

    public class TypeNameComparer : IComparer<Type>
    {
        public int Compare(Type t1, Type t2)
        {
            if (t1.Namespace.Length > t2.Namespace.Length)
            {
                return 1;
            }

            if (t1.Namespace.Length < t2.Namespace.Length)
            {
                return -1;
            }

            return t1.Namespace.CompareTo(t2.Namespace);
        }
    }
}
