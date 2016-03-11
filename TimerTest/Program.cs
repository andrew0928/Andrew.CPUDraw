using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimerTest
{
    class Program
    {
        static bool stop = false;

        static void Main(string[] args)
        {
            // noise: background threads
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread t = new Thread(Foo);
                    //t.Priority = ThreadPriority.BelowNormal;
                    t.Start();
                }

            }


            Stopwatch timer = new Stopwatch();
            TimeSpan idle = TimeSpan.FromMilliseconds(10);

            // test Thread.Sleep()
            for(int i = 0; i < 10; i++)
            {
                timer.Restart();
                Thread.Sleep(idle);
                Console.WriteLine("Sleep(10): take {0} ms", timer.ElapsedMilliseconds);
            }

            // test SpinWait.SpinUntil()
            for(int i = 0; i<10; i++)
            {
                timer.Restart();
                SpinWait.SpinUntil(() => { return false; }, idle);
                Console.WriteLine("SpinUntil(10): take {0} ms", timer.ElapsedMilliseconds);
            }

            stop = true;
        }

        static Random _rnd = new Random();

        static void Foo()
        {
            while (stop == false) ; if (_rnd.Next() % 5 == 0) Thread.Sleep(0);
        }
    }
}
