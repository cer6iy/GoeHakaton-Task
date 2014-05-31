using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using Finisar.SQLite;
using System.Threading;
using System.Globalization;

namespace Greenny
{
    class DButils
    {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();

        double eps = 0.007;
        int minPts = 10;
        List<List<Point>> clusters;

        public List<Point> points = new List<Point>();

        public void PrintClusters()
        {
            Console.Clear();
            // print points to console
            Console.WriteLine("The {0} points are :\n", points.Count);
            foreach (Point p in points) Console.Write(" {0} ", p);
            Console.WriteLine();
            // print clusters to console
            int total = 0;
            for (int i = 0; i < clusters.Count; i++)
            {
                int count = clusters[i].Count;
                total += count;
                string plural = (count != 1) ? "s" : "";
                Console.WriteLine("\nCluster {0} consists of the following {1} point{2} :\n", i + 1, count, plural);
                foreach (Point p in clusters[i]) Console.Write(" {0} ", p);
                Console.WriteLine();
            }
            // print any points which are NOISE
            total = points.Count - total;
            if (total > 0)
            {
                string plural = (total != 1) ? "s" : "";
                string verb = (total != 1) ? "are" : "is";
                Console.WriteLine("\nThe following {0} point{1} {2} NOISE :\n", total, plural, verb);
                foreach (Point p in points)
                {
                    if (p.ClusterId == Point.NOISE) Console.Write(" {0} ", p);
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("\nNo points are NOISE");
            }
            Console.ReadKey();
        }

        public void PrintPoints()
        {
            if (DT == null) return;
            Console.WriteLine();
            foreach (var column in DT.Columns)
            {
                DataColumn dataColumn = column as DataColumn;

                Console.Write("{0}", dataColumn.ColumnName + "\t");
            }
            Console.WriteLine();
            StringBuilder str = new StringBuilder();
            foreach (Point p in points)
            {
                str.AppendLine(String.Format("{0}\t{1}", p.X, p.Y));

                //Console.Write("{0} \t\t {1}", p.X, p.Y);
                //Console.WriteLine();
            }
            File.WriteAllText("points.txt", str.ToString());
        }

        public void ToList()
        {
            foreach (DataRow row in DT.Rows)
            {
                try
                {
                    points.Add(new Point(double.Parse(row.ItemArray[0].ToString().Replace(',', '.')), double.Parse(row.ItemArray[1].ToString().Replace(',', '.'))));

                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        public void GetData(int b_t = 0, int e_t = 0)
        {

            sql_con = new SQLiteConnection("Data Source=omsk.sqlite;Version=3;New=False;Compress=True;");
            sql_con.Open();

            sql_cmd = sql_con.CreateCommand();
            // подключение геобиблиотеки
            string query = "SELECT load_extension('libspatialite-2.dll')";
            SQLiteDataAdapter ret = new SQLiteDataAdapter(query, sql_con);

            string CommandText = "select * from building where Contains(geom, PointFromText(\"POINT(73.365707 54.91741)\"))";
            DB = new SQLiteDataAdapter(CommandText, sql_con);
            sql_con.Close();
            DS.Reset();
            DB.Fill(DS);
            DT = DS.Tables[0];
            this.ToList();
            //this.PrintPoints();
            //Console.ReadKey();
            clusters = DBSCAN.GetClusters(points, eps, minPts);
            this.PrintClusters();
            Console.WriteLine(clusters.Count);
            Console.ReadKey();
        }
    }
    class Program
    {
        public static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            DButils a = new DButils();
            a.GetData();
            Console.ReadKey();
        }
    }
}