using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            TextWriter t1 = new StringWriter();
            TextWriter t2 = new StringWriter();

            t1.WriteLine("noise,sleep,spin,adv_sleep,adv_spin,");
            t2.WriteLine("noise,sleep,spin,adv_sleep,adv_spin,");
            for (int i = 0; i < 20; i++) RunEval(i, t1, t2);
            //Console.WriteLine(t1.ToString());

            File.WriteAllText(@"D:\Accu.csv", t1.ToString());
            File.WriteAllText(@"D:\SD.csv", t2.ToString());
        }

        static void RunEval(int noise_count, TextWriter accu_out, TextWriter sd_out)
        { 
            Stopwatch timer = new Stopwatch();
            TimeSpan idle = TimeSpan.FromMilliseconds(10);
            TimeSpan temp = TimeSpan.Zero;
            int testrun_count = 50;
            StatisticHelper stat = new StatisticHelper(idle);
            List<Thread> noise_threads = new List<Thread>();


            // noise: background threads
            stop = false;
            for (int i = 0; i < noise_count; i++)
            {
                Thread t = new Thread(Foo);
                //t.Priority = ThreadPriority.BelowNormal;
                t.Start();
                noise_threads.Add(t);
            }
            accu_out.Write("{0},", noise_count);
            sd_out.Write("{0},", noise_count);


            // test Thread.Sleep()
            stat.Reset();
            for (int i = 0; i < testrun_count; i++)
            {
                timer.Restart();
                Thread.Sleep(idle);
                temp = timer.Elapsed;
                Console.WriteLine("- Sleep(): take {0} ms", temp.TotalMilliseconds);
                stat.Add(temp);
            }
            Console.WriteLine("- Accuracy Rate: {0}%, Standard Div: {1}", stat.AccuracyRate, stat.StdDiv);
            Console.WriteLine();
            accu_out.Write("{0},", stat.AccuracyRate);
            sd_out.Write("{0},", stat.StdDiv);

            // test SpinWait.SpinUntil()
            stat.Reset();
            for (int i = 0; i < testrun_count; i++)
            {
                timer.Restart();
                SpinWait.SpinUntil(() => { return false; }, idle);
                temp = timer.Elapsed;
                Console.WriteLine("- SpinUntil(): take {0} ms", temp.TotalMilliseconds);
                stat.Add(temp);
            }
            Console.WriteLine("- Accuracy Rate: {0}%, Standard Div: {1}", stat.AccuracyRate, stat.StdDiv);
            Console.WriteLine();
            accu_out.Write("{0},", stat.AccuracyRate);
            sd_out.Write("{0},", stat.StdDiv);

            // test advanced Thread.Sleep()
            stat.Reset();
            for (int i = 0; i < testrun_count; i++)
            {
                timer.Restart();
                while (timer.Elapsed < idle) Thread.Sleep(0);
                temp = timer.Elapsed;
                Console.WriteLine("- Advanced Sleep(): take {0} ms", temp.TotalMilliseconds);
                stat.Add(temp);
            }
            Console.WriteLine("- Accuracy Rate: {0}%, Standard Div: {1}", stat.AccuracyRate, stat.StdDiv);
            Console.WriteLine();
            accu_out.Write("{0},", stat.AccuracyRate);
            sd_out.Write("{0},", stat.StdDiv);

            // test advanced SpinWait.SpinUntil()
            stat.Reset();
            for (int i = 0; i < testrun_count; i++)
            {
                timer.Restart();
                SpinWait.SpinUntil(() => { return timer.Elapsed > idle; });
                temp = timer.Elapsed;
                Console.WriteLine("- Advanced SpinUntil(): take {0} ms", temp.TotalMilliseconds);
                stat.Add(temp);
            }
            Console.WriteLine("- Accuracy Rate: {0}%, Standard Div: {1}", stat.AccuracyRate, stat.StdDiv);
            Console.WriteLine();
            accu_out.Write("{0},", stat.AccuracyRate);
            sd_out.Write("{0},", stat.StdDiv);

            accu_out.WriteLine();
            sd_out.WriteLine();
            stop = true;
            foreach (Thread t in noise_threads) t.Join();
        }

        static Random _rnd = new Random();

        static void Foo()
        {
            while (stop == false) ; //if (_rnd.Next() % 5 == 0) Thread.Sleep(0);
        }
    }

    public class StatisticHelper
    {
        public StatisticHelper(TimeSpan expected)
        {
            this.expectedValue = expected;
        }

        private TimeSpan expectedValue = TimeSpan.Zero;
        private List<TimeSpan> records = new List<TimeSpan>();

        public void Reset()
        {
            records.Clear();
        }

        public void Add(TimeSpan value)
        {
            records.Add(value);
        }

        public double AccuracyRate { get
            {
                TimeSpan total = TimeSpan.Zero;
                foreach (TimeSpan e in this.records) total += e;
                return total.Ticks * 100 / this.expectedValue.Ticks / this.records.Count;
            }
        }

        public double StdDiv
        {
            get
            {
                TimeSpan total = TimeSpan.Zero;
                foreach (TimeSpan e in this.records) total += e;

                double avg_ticks = (double)total.Ticks / this.records.Count;
                double total_sqrt = 0;
                foreach (TimeSpan e in this.records) total_sqrt += 1000000 * (e.Ticks - avg_ticks) * (e.Ticks - avg_ticks) / this.expectedValue.Ticks / this.expectedValue.Ticks;

                return total_sqrt / records.Count;
            }
        }
    }
}
