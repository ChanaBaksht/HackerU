using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelLoops
{
    public class EX_PLINQ
    {
        public static int Sum(int num)
        {
            var sum = ParallelEnumerable.Range(1, 10).Aggregate(0,
            (partialSum, i) => partialSum += i,
            (totalSum, subTotal) => totalSum += subTotal,
            i => i);
            return sum;
        }

        public static int Factorial(int num)
        {
            var factorial = ParallelEnumerable.Range(1, num).Aggregate(1,
             (partialfactorial, i) => partialfactorial *= i,
             (totalfactorial, subTotalfactorial) => totalfactorial *= subTotalfactorial,
              i => i);
            return factorial;
        }

        public static int Average(int num)
        {
            var average = ParallelEnumerable.Range(1, num).Aggregate(0, (partialSum, i) => partialSum += i, (totalSum, subTotal) => totalSum += subTotal, result => result / num);
            return average;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Sum= {Sum(10)}"); //55
            Console.WriteLine($"Factorial= {Factorial(10)}"); //3628800
            Console.WriteLine($"Average= {Average(10)}"); //5.5
        }
    }
}
