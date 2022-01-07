using System;
using System.Collections.Generic; // only necessary for List<double> _lstMilliseconds
using System.Linq; // only necessary for PrintSortedTotals()


// HOW TO GENERATE A 64-BIT RANDOM INTEGER USING 6 DIFFERENT METHODS AND COMPARING THE TIME LAPSES FOR EACH.

namespace RandomU64
{
    class Program
    {
        static private Random _random = new Random();

        private const int ITERATIONS = 100000;

        private struct TotalsStructure
        {
            public int MethodNumber;
            public double TotalMilliseconds;
        }

        private static List<TotalsStructure> millisecondTotalsList = new List<TotalsStructure>();


        static void Main(string[] args)
        {
            Func<ulong>[] arr_Methods = { Method1, Method2, Method3, Method4, Method5, Method6 };

            Console.WriteLine(string.Format("{0} iterations for each method.", ITERATIONS));

            foreach(Func<ulong> methodX in arr_Methods)
            {
                string MethodName = methodX.Method.Name;
                double totalMilliseconds = RunMethod(methodX);

                PrintHeader(MethodName);
                PrintFooter(totalMilliseconds);
                AddToStatsList(MethodName, totalMilliseconds);
            }    

            PrintStats();
            Console.ReadLine();
        }

        static double RunMethod(Func<ulong> MethodX)
        {
            DateTime startTime = DateTime.Now;

            for (int count = 1; count <= ITERATIONS; count++)
                _ = MethodX.Invoke();

            return (DateTime.Now - startTime).TotalMilliseconds;
        }


        static void PrintHeader(string methodName)
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine(string.Format("Running {0}()", methodName));
        }

        static void PrintFooter(double totalMilliseconds) =>
            Console.WriteLine(string.Format("Elapsed time: {0} milliseconds", totalMilliseconds));

        static void AddToStatsList(string methodName, double totalMilliseconds)
        {
            int methodNumber = Convert.ToInt32(methodName.Substring(methodName.Length - 1, 1));
            millisecondTotalsList.Add(new TotalsStructure { MethodNumber = methodNumber, TotalMilliseconds = totalMilliseconds });
        }

        static void PrintStats()
        {
            int[] lengths = millisecondTotalsList.OrderBy(s => s.TotalMilliseconds).Select(n => n.MethodNumber).ToArray();
            int limit = lengths.GetUpperBound(0);

            Console.WriteLine();
            Console.Write("Methods in order of speed (fastest -> slowest): ");
            
            for (int index = 0; index <= limit; index++)
            {
                Console.Write(lengths[index]);
                if (index < limit)
                    Console.Write(", ");
                else
                    Console.WriteLine();
            }

        }

        // *********************************************************************************************************************************************************************************************************************************************************

        static ulong Method1()
        {
            ulong result = 0;

            for (int index = 0; index < 32; index++)
                result += Convert.ToUInt64((uint)_random.Next(int.MinValue, int.MaxValue));

            return result;
        }

        static ulong Method2()
        {
            /*
             * 1) Get signed 32-bit random integer x1 between -(2^31 + 1) and +(2^31 - 1)
             * 2) Get signed 32-bit random integer x2 between -(2^31 + 1) and +(2^31 - 1)
             * 3) Dynamically convert x1 to unsigned 64-bit and set y = x1;
             * 4) Shift y 32 bits to left
             * 5) y = y OR x2
            */

            int x1 = _random.Next(int.MinValue, int.MaxValue);
            int x2 = _random.Next(int.MinValue, int.MaxValue);
            ulong y;

            // lines must be separated or result won't go past 2^32
            y = (uint)x1;
            y = y << 32;
            y = y | (uint)x2;

            return y;
        }

        static ulong Method3()
        {
            /*
             * 1) result = 0
             * 2) Loop power 0->63
             * 3)   random = random floating point value between 0->1
             * 4)   if random < 0.5
             * 5)       dValue = 2 ^ power
             * 6)       ulValue = dValue converted to unsigned 64-bit
             * 7)       result = result OR ulValue
             * 8)  End Loop
            */
  
            ulong result = 0;

            for(int power = 0; power < 64; power++)
            {
                double random = _random.NextDouble();
                if(random > 0.5)
                {
                    double dValue = Math.Pow(2, power);
                    ulong ulValue = Convert.ToUInt64(dValue);
                    result = result | ulValue;
                }
            }

            return result;
        }

        static ulong Method4()  // only difference between #4 and #3 is that this one (#4) uses .Next() instead of .NextDouble()
        {
            /*
             * 1) result = 0
             * 2) Loop power 0->63
             * 3)   if random 0 or 1 = 1
             * 4)       result = result OR ConvertTo64bit(2 ^ power)
             * 5) End Loop
            */

            ulong result = 0;

            for (int power = 0; power < 64; power++)
                if (_random.Next(0, 1) == 1)
                    result = result | Convert.ToUInt64(Math.Pow(2, power));

            return result;
        }

        static ulong Method5()
        {
            /*
             * 1) Create 8-element byte[] array (each element is 8 bits)
             * 2) Fill each element with random number between 0-255 (0 -> 2^8 - 1)
             * 3) Merge byte[] array into unsigned 64-bit integer
            */
            
            byte[] arr_bt = new byte[8];
            ulong result;

            _random.NextBytes(arr_bt);
            result = BitConverter.ToUInt64(arr_bt, 0);
            return result;
        }

        // Next method courtesy of https://stackoverflow.com/questions/14708778/how-to-convert-unsigned-integer-to-signed-integer-without-overflowexception/39107847
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        struct EvilUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public int Int32;
            [System.Runtime.InteropServices.FieldOffset(0)] public uint UInt32;
        }

        static ulong Method6()
        {
            /*
             * evil.Int32 and evil.UInt32 occupy same space in memory (aka unions). So, any change to one immediately changes the other
             * 
             * 1) Get signed 32-bit random integer evil.Int32 between -(2^31 + 1) and +(2^31 - 1)
             * 2) result = evil.UInt32
             * 3) Shift result value 32 bits to left
             * 4) Get new signed 32-bit random integer value into evil.Int32 between -(2^31 + 1) and +(2^31 - 1)
             * 5) result = result | evil.UInt32
            */
            var evil = new EvilUnion();
            ulong result = 0;

            evil.Int32 = _random.Next(int.MinValue, int.MaxValue);
            result = evil.UInt32;
            result = result << 32;
            evil.Int32 = _random.Next(int.MinValue, int.MaxValue);
            result = result | evil.UInt32;

            return result;
        }
    }
}
