using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using Uaaa.Data.Mapper;
using static Uaaa.Data.Sql.Query;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const int maxIterations = 10000;
            long average = 0;
            Console.WriteLine($"Calculating average ({maxIterations} iterations)...");
            average = GetAverage(10, () => BenchmarkInsertQuery(maxIterations));
            Console.WriteLine($"InsertQuery: {average}ms");

            average = GetAverage(10, () => BenchmarkUpdateQuery(maxIterations));
            Console.WriteLine($"UpdateQuery: {average}ms");

            average = GetAverage(10, () => BenchmarkSelectQuery(maxIterations));
            Console.WriteLine($"SelectQuery: {average}ms");

            average = GetAverage(10, () => BenchmarkMappingSchema(maxIterations));
            Console.WriteLine($"MappingSchema: {average}ms");
        }

        private static long GetAverage(int repeatTimes, Func<long> benchmark)
        {
            if (repeatTimes <= 1)
                throw new ArgumentOutOfRangeException(nameof(repeatTimes));
            long totalTime = 0;
            for (int index = 0; index < repeatTimes; index++)
            {
                totalTime += benchmark();
            }
            return totalTime / repeatTimes;
        }

        private static long BenchmarkInsertQuery(int maxIterations)
        {
            const string table = "Table1";
            List<MySample> samples = GenerateSamples(maxIterations);
            Stopwatch watch = Stopwatch.StartNew();
            SqlCommand command = Insert(samples).Into(table).ResolveKeys();
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        private static long BenchmarkUpdateQuery(int maxIterations)
        {
            const string table = "Table1";
            List<MySample> samples = GenerateSamples(maxIterations);
            Stopwatch watch = Stopwatch.StartNew();
            SqlCommand command = Update(samples).In(table);
            //foreach (MySample mySample in samples)
            //{
            //    SqlCommand command = Update(mySample).In(table);
            //}

            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        private static long BenchmarkSelectQuery(int maxIterations)
        {
            const string table = "Table1";
            Stopwatch watch = Stopwatch.StartNew();
            for (int index = 0; index < maxIterations; index++)
            {
                SqlCommand command = Select<MySample>().From(table);
            }
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        private static long BenchmarkMappingSchema(int maxIterations)
        {
            var samples = GenerateSamples(maxIterations);
            var watch = Stopwatch.StartNew();
            foreach (MySample mySample in samples)
            {
                var schema = MappingSchema.Get<MySample>();
            }
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        private static List<MySample> GenerateSamples(int maxIteration)
        {
            var samples = new List<MySample>();
            for (int index = 0; index < maxIteration; index++)
            {
                samples.Add(new MySample { Id = index, Label = $"Label{index + 1}", Value = index + 1 });
            }
            return samples;
        }

        public class MySample
        {
            public int Id { get; set; }
            public string Label { get; set; }
            public int Value { get; set; }
            public DateTime DateTime { get; set; } = DateTime.UtcNow;
        }
    }
}
