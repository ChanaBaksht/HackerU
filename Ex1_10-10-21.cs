using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLoops
{
    class Ex1_10_10_21
    {
        public static Random rnd = new Random();
        class BankAccount
        {
            public object padlock = new();
            public int Balance { get; private set; }
            public int Id { get; private set; }

            public BankAccount(int id, int balance)
            {
                Id = id;
                Balance = balance;
            }
            public void Deposit(int amount)
            {
                lock (padlock)
                {
                    Balance += amount;
                }
            }

            public void Withdraw(int amount)
            {
                lock (padlock)
                {
                    Balance -= amount;
                }
            }

        }
        class Bank
        {
            public string Name { get; set; }
            public ConcurrentDictionary<int, BankAccount> AccountsD;

            public Bank(int size, string name = "Bank's Name")
            {
                Name = name;
                AccountsD = new ConcurrentDictionary<int, BankAccount>();
                for (int i = 0; i < size; i++)
                {
                    AccountsD.TryAdd(i, new BankAccount(i + 1, 100));
                }
            }
        }

        public static void IncreaseAccountsBalance()
        {
            Bank bank = new Bank(1000);
            int totalIncrease = 0, totalBalance = 0, increaseIn;
            Mutex mutex = new Mutex();

            Parallel.ForEach(bank.AccountsD,
                () => 0,
                (currentBA, state, PartialIncreaseIn) =>
                {
                    BankAccount ba;
                    bank.AccountsD.TryGetValue(currentBA.Key, out ba);

                    mutex.WaitOne();
                    increaseIn = (int)(ba.Balance * 0.25) - rnd.Next(10);
                    ba.Deposit(increaseIn);
                    PartialIncreaseIn += increaseIn;
                    totalBalance += ba.Balance;
                    mutex.ReleaseMutex();

                    Console.WriteLine($"[{Task.CurrentId}] Bank account {currentBA.Key}: balance = {currentBA.Value.Balance} (IncreaseIn {increaseIn})");
                    return PartialIncreaseIn;
                }, sumPartialIncrease =>
                {
                    Interlocked.Add(ref totalIncrease, sumPartialIncrease);
                }
                );

            Console.WriteLine($"\nTotalIncrease: {totalIncrease}");
            Console.WriteLine($"TotalBalance: {totalBalance}\n");

        }

        static void Main(string[] args)
        {
            IncreaseAccountsBalance();
        }
    }
}