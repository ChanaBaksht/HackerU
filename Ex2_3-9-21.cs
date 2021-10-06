using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordination
{
    class Ex2_3_9_21
    {
        public static int SumRangeNumbers(int n1, int n2)
        {
            int sum = 0;
            for (int i = n1; i < n2 + 1; i++)
            {
                sum += i;
            }
            return sum;
        }

        static void Main(string[] args)
        {
            Console.WriteLine();

            int longRange = 100, totalSum = 0;
            List<Task> tasks = new List<Task>();
            ConcurrentDictionary<int, int> sumSubRangeD = new ConcurrentDictionary<int, int>();
            CountdownEvent cde = new CountdownEvent(10);
            int k = 0;
            object locker = new object();

            for (int i = 0, j = 0; i < longRange; i += 10)
            {
                Task<int> task = new Task<int>(() =>
                {
                    lock (locker)
                    {
                        totalSum = SumRangeNumbers(k, k + 10);
                        sumSubRangeD.TryAdd((k + 10), totalSum);
                        k += 10;
                        return k;
                    }
                });

                var printRecordTask = task.ContinueWith(t =>
                {
                    lock (locker)
                    {
                        Console.WriteLine($"subSumRange from {t.Result - 10} to {t.Result} is: {sumSubRangeD[t.Result]}");
                        cde.Signal();
                    }
                });

                tasks.Add(task);
                tasks[j++].Start();
            }
            cde.Wait();
            Console.WriteLine();
            Console.WriteLine($"Total Sum of Range 0 to {longRange} is: {SumRangeNumbers(0, longRange)}");

            Console.ReadLine();
        }
    }

}
