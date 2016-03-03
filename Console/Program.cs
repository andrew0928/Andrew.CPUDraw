using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            int total_threads = 10;
            for (int i = 0; i < total_threads; i++)
            {
                Thread t = new Thread(Program.DrawSinWave);
                t.Start();
            }

            //DrawSinWave();
        }

        public static void DrawSinWave()
        {
            long unit = 100;


            Stopwatch timer = new Stopwatch();
            timer.Restart();
            while (true)
            {
                //SpinWait.SpinUntil(
                //    () => { return false; },
                //    (int)(unit - timer.ElapsedMilliseconds % unit));
                //Thread.Sleep((int)(unit - timer.ElapsedMilliseconds % unit));




                long degree = (timer.ElapsedMilliseconds / unit) % 360;

                long offset = (long)(timer.ElapsedMilliseconds % unit);

                long v = (long)(unit - (Math.Sin(Math.PI * degree / 180.0) / 2 + 0.5) * unit);
                long idle_until = timer.ElapsedMilliseconds - offset + v;
                long busy_until = timer.ElapsedMilliseconds - offset + unit;

                //long offset = (timer.ElapsedMilliseconds % unit);
                //Console.WriteLine("{0},{1}", degree, v);

                SpinWait.SpinUntil(() => { return (timer.ElapsedMilliseconds > idle_until); });
                while (timer.ElapsedMilliseconds < busy_until) ;
            }
        }
    }
}
