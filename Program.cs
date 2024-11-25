using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net;

using Isac;
using Isac.Isql;
using Isac.Isql.Collections;
using Isac.Isql.QueryCommand;
using Isac.Isql.Logistics;
using System.IO;

namespace ISql
{ 
    class Program
    {
        public static string Client()
        {
            return "Client Method";
        }
        static void Main(string[] args)
        {

            ObjectLoader.MyLoader(typeof(Program), As.MethodObject);
            //LogicExpressionEngine lexp = new LogicExpressionEngine("a + b - 3 ^ 5");

            //lexp.ChangeValues(new Dictionary<string, object>
            //{
            //    ["a"] = 3,
            //    ["b"] = 9
            //});

            //lexp.Solve();
            //Console.WriteLine(lexp.RawResult);


            Connection con = new Connection("DATABASE", "main", "pwd");
            ISqlConnection.Add("con1", con);
            ISqlConnection.Use("con1");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Query q = new Query();
            //q.Insert("_name", new object[] {"Mustapha Abdulazeez" }).Into("workers");
            q.Select("_id, _name").From("workers")
                .Where("_name == 'Mustapha Abdulazeez'");
            watch.Stop();
            var dt = q.GetData();
            Console.WriteLine(string.Join("\t", dt.ColumnNames()));
            foreach (Row row in dt)
                Console.WriteLine(string.Join("\t", row.GetCurrentValues()));


            Console.WriteLine(watch.Elapsed);
        }
    }
}
