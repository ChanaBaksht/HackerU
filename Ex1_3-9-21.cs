using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordination
{
    class Ex1_3_10_21
    {
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

            public Bank(string name = "Bank's Name")
            {
                Name = name;
                AccountsD = new ConcurrentDictionary<int, BankAccount>();
                for (int i = 0; i < 10; i++)
                {
                    AccountsD.TryAdd(i, new BankAccount(i + 1, 100));
                }
            }
        }

        private static Random rnd = new Random();

        public static void ChildTasks()
        {
            Bank bank = new Bank();
            var mainTasks = new List<Task>();

            //Iterations of actions in accounts:
            for (int i = 0; i < 20; i++)
            {
                //Every account start a task of deposite and withdraw actionts:
                mainTasks.Add(Task.Factory.StartNew(() =>
                {
                    //Go over all bank account:
                    for (int j = 0; j < 10; j++)
                    {
                        Console.WriteLine($"[{Task.CurrentId}] Running iteration {j}.");

                        //Deposite for all accounts:
                        Task depositeTask = Task.Factory.StartNew(() =>
                        {
                            for (int k = 0; k < 10; k++)
                            {
                                BankAccount ba;
                                bank.AccountsD.TryGetValue(k, out ba);
                                ba.Deposit(10);
                                bool success = bank.AccountsD.TryUpdate(k, ba, ba);
                                if (!success)
                                    Console.WriteLine($"[{Task.CurrentId}] Update Failed for account id {ba.Id}.");
                                Thread.Sleep(rnd.Next(100));
                            }
                        }, TaskCreationOptions.AttachedToParent);

                        //Withdraw for all accounts:
                        Task withdrawTask = Task.Factory.StartNew(() =>
                        {
                            for (int k = 0; k < 10; k++)
                            {
                                BankAccount ba;
                                bank.AccountsD.TryGetValue(k, out ba);
                                ba.Withdraw(10);
                                bool success = bank.AccountsD.TryUpdate(k, ba, ba);
                                if (!success)
                                    Console.WriteLine($"[{Task.CurrentId}] Update Failed for account id {ba.Id}.");
                                Thread.Sleep(rnd.Next(100));
                            }
                        }, TaskCreationOptions.AttachedToParent);

                    }
                }));
            }

            try
            {
                Task.WaitAll(mainTasks.ToArray());
                //Print account's details:
                for (int i = 0; i < 10; i++)
                {
                    BankAccount ba;
                    bank.AccountsD.TryGetValue(i, out ba);
                    Console.WriteLine($"Final balance is {ba.Balance}.");
                }
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => true);
            }
        }

        public static void ContinuationTask()
        {
            Bank bank = new Bank();
            var mainTasks = new List<Task>();

            for (int i = 0; i < 20; i++)
            {
                Task t = new Task(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Console.WriteLine($"[{Task.CurrentId}] Running iteration {j}.");

                    //Deposite for all accounts:
                    Task depositeTask = Task.Factory.StartNew(() =>
                        {
                            for (int k = 0; k < 10; k++)
                            {
                                BankAccount ba;
                                bank.AccountsD.TryGetValue(k, out ba);
                                ba.Deposit(10);
                                bool success = bank.AccountsD.TryUpdate(k, ba, ba);
                                if (!success)
                                    Console.WriteLine($"[{Task.CurrentId}] Update Failed for account id {ba.Id}.");
                                Thread.Sleep(rnd.Next(100));
                            }
                        }, TaskCreationOptions.AttachedToParent);

                    //Withdraw for all accounts:
                    Task withdrawTask = Task.Factory.StartNew(() =>
                        {
                            for (int k = 0; k < 10; k++)
                            {
                                BankAccount ba;
                                bank.AccountsD.TryGetValue(k, out ba);
                                ba.Withdraw(10);
                                bool success = bank.AccountsD.TryUpdate(k, ba, ba);
                                if (!success)
                                    Console.WriteLine($"[{Task.CurrentId}] Update Failed for account id {ba.Id}.");
                                Thread.Sleep(rnd.Next(100));
                            }
                        }, TaskCreationOptions.AttachedToParent);
                    }
                });

                Task failHandlerChild1 = t.ContinueWith(t =>
                {
                    Console.WriteLine($"Unfortunately, task {t.Id}'s state is {t.Status}:(  Error Message is: {t.Exception.Message}");
                }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnFaulted);
                
                Task completionHandlerChild1 = t.ContinueWith(t =>
                {
                    Console.WriteLine($"Hooray, task {t.Id}'s state is {t.Status}:)");
                    for (int i = 0; i < 10; i++)
                    {
                        BankAccount ba;
                        bank.AccountsD.TryGetValue(i, out ba);
                        Console.WriteLine($"Final balance of baId {i} is: {ba.Balance}.");
                    }
                }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion);

                mainTasks.Add(t);
                mainTasks[i].Start();
            }

            try
            {
                Task.WaitAll(mainTasks.ToArray());
                Console.WriteLine("END!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => true);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("ChildTasks function start:");
            ChildTasks();
            
            Console.WriteLine("ContinuationTask function start:");
            ContinuationTask();

            Console.WriteLine("End Program, press any key...");
            Console.ReadKey();

        }
    }
}
