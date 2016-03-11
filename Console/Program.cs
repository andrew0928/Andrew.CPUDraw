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
            int total_threads = 8;
            for (int i = 0; i < total_threads; i++)
            {
                Thread t = new Thread(Program.DrawBitmap);
                t.Start();
            }
        }


        public static string[] bitmap = new string[]
        {
            @"              XXXX          X      X          XXXX              ",
            @"             X   X          XX    XX          X   X             ",
            @"            X    X         X  XXXX  X         X    X            ",
            @"           X      XXXX     X        X     XXXX      X           ",
            @"          X           XXXXX          XXXXX           X          ",
            @"         X                                            X         ",
            @"        X                                              X        ",
            @"       X                                                X       ",
            @"      X                                                  X      ",
            @"     X                                                    X     "
        };
        //public static string[] bitmap = new string[]
        //{
        //    @"                 XXXX                ",
        //    @"                 X   X               ",
        //    @"                X    X               ",
        //    @"      XXXXXXXXXX     XXXXXXX         ",
        //    @"      X     X X             X        ",
        //    @"      X     X X              X       ",
        //    @"      X     X X              X       ",
        //    @"      X     X X              X       ",
        //    @"      X     X X              X       ",
        //    @"      XXXXXXX XXXXXXXXXXXXXXXX       "
        //};




        public static long[] GetDataFromBitmap(long period, long unit)
        {
            int max_x = bitmap[0].Length;
            int max_y = bitmap.Length;
            long steps = period / unit;

            long[] data = new long[steps];

            for (int s = 0; s < steps; s++)
            {
                int x = (int)(s * max_x / steps);
                int value = 0;
                for (int y = 0; y < bitmap.Length; y++)
                {
                    value = y;
                    if (bitmap[y][x] == 'X') break;
                }
                data[s] = value * unit / max_y;
            }

            return data;
        }

        /// <summary>
        /// 產生對照表，每個時段對應的附載數值。
        /// </summary>
        /// <param name="period">週期，圖形多久循環一次 (ms)?</param>
        /// <param name="unit">每個時段的單位長度 (ms)</param>
        /// <returns></returns>
        public static long[] GetDataFromSineWave(long period, long unit)
        {
            long steps = period / unit;
            long[] data = new long[steps];

            for (int s = 0; s < steps; s++)
            {
                //long degree = s * 360 / steps
                data[s] = (long)(unit - (Math.Sin(Math.PI * s * 360 / steps / 180.0) / 2 + 0.5) * unit);
            }

            return data;
        }

        /// <summary>
        /// 按照對照表的數據，將圖形用 CPU utilization 繪製出來
        /// </summary>
        public static void DrawBitmap()
        {
            long unit = 100; // ms
            long period = 60 * 1000; // msec, full image time period

            // get drawing data
            long[] data = GetDataFromBitmap(period, unit);
            //long[] data = GetDataFromSineWave(period, unit);

            Stopwatch timer = new Stopwatch();
            timer.Restart();
            while (true)
            {
                long step = (timer.ElapsedMilliseconds / unit) % (period / unit);
                long offset = (long)(timer.ElapsedMilliseconds % unit);
                long v = data[step];
                long idle_until = timer.ElapsedMilliseconds - offset + v;
                long busy_until = timer.ElapsedMilliseconds - offset + unit;

                // idle part
                SpinWait.SpinUntil(() => { return (timer.ElapsedMilliseconds > idle_until); });

                // busy part
                while (timer.ElapsedMilliseconds < busy_until) ;
            }
        }


        public static void DrawSinWave()
        {
            long unit = 100;
            Stopwatch timer = new Stopwatch();
            timer.Restart();
            while (true)
            {
                long degree = (timer.ElapsedMilliseconds / unit) % 360;
                long offset = (long)(timer.ElapsedMilliseconds % unit);
                long v = (long)(unit - (Math.Sin(Math.PI * degree / 180.0) / 2 + 0.5) * unit);
                long idle_until = timer.ElapsedMilliseconds - offset + v;
                long busy_until = timer.ElapsedMilliseconds - offset + unit;

                SpinWait.SpinUntil(() => { return (timer.ElapsedMilliseconds > idle_until); });
                while (timer.ElapsedMilliseconds < busy_until) ;
            }
        }
    }
}
