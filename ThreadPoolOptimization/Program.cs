﻿#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion // Using

#pragma warning disable CS0162 // Unreachable code detected
namespace Bnaya.Samples
{
    enum Mode
    {
        ThreadPool,
        Task,
        FairTask
    }

    class Program
    {
        // TODO: Change the modes
        private const Mode MODE = Mode.ThreadPool;

        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            for (int i = 0; i < 2; i++)
            {
                int id = i;
                Invoke(() => DoComplexWork(id)); // have child work
            }
            Thread.Sleep(1);// ensure that the first 2 request served
            // total initial thread count = ProcessorCount + 2
            for (int i = 2; i < Environment.ProcessorCount + 2; i++)
            {
                int id = i;
                Invoke(() => DoWork("Parent " + i, 1000));
            }

            Console.ReadKey();
        }

        #region Invoke

        private static void Invoke(Action action)
        {
            switch (MODE)
            {
                case Mode.ThreadPool:
                    ThreadPool.QueueUserWorkItem(state => action());
                    break;
                case Mode.Task:
                    Task.Run(action);
                    break;
                case Mode.FairTask:
                    Task.Factory.StartNew(action, TaskCreationOptions.PreferFairness);
                    break;
            }
        }

        #endregion // Invoke

        #region DoComplexWork

        private static void DoComplexWork(int i)
        {
            DoWork("Parent " + i, 500);
            Invoke(() => DoWork("\tChild: " + i, 1000));
        }

        #endregion // DoComplexWork

        #region DoWork

        private static void DoWork(string title, int duration)
        {
            WriteInfo(title);
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < duration) ;
        }

        #endregion // DoWork

        #region WriteInfo

        private static void WriteInfo(string title)
        {
            var t = Thread.CurrentThread;
            Console.WriteLine("{0}, Thread ID = {1}", title, t.ManagedThreadId);
        }

        #endregion // WriteInfo
    }
}