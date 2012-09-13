﻿using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using System.Data;
namespace SqlMapper
{
    [ServiceStack.DataAnnotations.Alias("Posts")]
	[Soma.Core.Table(Name = "Posts")]
    class Post
    {
		[Soma.Core.Id(Soma.Core.IdKind.Identity)]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastChangeDate { get; set; }
        public int? Counter1 { get; set; }
        public int? Counter2 { get; set; }
        public int? Counter3 { get; set; }
        public int? Counter4 { get; set; }
        public int? Counter5 { get; set; }
        public int? Counter6 { get; set; }
        public int? Counter7 { get; set; }
        public int? Counter8 { get; set; }
        public int? Counter9 { get; set; }

    }

    class Program
    {

        //public static readonly string connectionString = "Data Source=.;Initial Catalog=tempdb;Integrated Security=True";
        public static readonly string connectionString = "Server=127.0.0.1;Uid=root;Pwd=asdf;Database=sample;";

        public static IDbConnection GetOpenConnection()
        {
            //var connection = new SqlConnection(connectionString);
            var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        static void RunPerformanceTests()
        {
            var test = new PerformanceTests();
            const int iterations = 500;
            Console.WriteLine("Running {0} iterations that load up a post entity", iterations);
            test.Run(iterations);
        }

        static void Main()
        {

#if DEBUG
            RunTests();
#else 
            EnsureDBSetup();
            RunPerformanceTests();
#endif
            Console.WriteLine("(end of tests; press any key)");

            Console.ReadKey();
        }

        private static void EnsureDBSetup()
        {
            using (var cnn = GetOpenConnection())
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = @"
if (OBJECT_ID('Posts') is null)
begin
	create table Posts
	(
		Id int identity primary key, 
		[Text] varchar(max) not null, 
		CreationDate datetime not null, 
		LastChangeDate datetime not null,
		Counter1 int,
		Counter2 int,
		Counter3 int,
		Counter4 int,
		Counter5 int,
		Counter6 int,
		Counter7 int,
		Counter8 int,
		Counter9 int
	)
	   
	set nocount on 

	declare @i int
	declare @c int

	declare @id int

	set @i = 0

	while @i <= 5001
	begin 
		
		insert Posts ([Text],CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE())
		set @id = @@IDENTITY
		
		set @i = @i + 1
	end
end
";
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
            }
        }

        private static void RunTests()
        {
            var tester = new Tests();
            int fail = 0;
            MethodInfo[] methods = typeof(Tests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var activeTests = methods.Where(m => Attribute.IsDefined(m, typeof(ActiveTestAttribute))).ToArray();
            if (activeTests.Length != 0) methods = activeTests;
            foreach (var method in methods)
            {
                Console.Write("Running " + method.Name);
                try
                {
                    method.Invoke(tester, null);
                    Console.WriteLine(" - OK!");
                } catch (Exception ex)
                {
                    fail++;
                    if (ex.InnerException == null)
                        Console.WriteLine(" - " + ex.Message);
                    else
                        Console.WriteLine(" - " + ex.InnerException.Message);
                }
            }
            Console.WriteLine();
            if(fail == 0)
            {
                Console.WriteLine("(all tests successful)");
            }
            else
            {
                Console.WriteLine("#### FAILED: {0} / {1}", fail, methods.Length);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ActiveTestAttribute : Attribute {}

}
