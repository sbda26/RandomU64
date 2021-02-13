using System;
using System.Collections.Generic; // only necessary for List<double> _lstMilliseconds
using System.Linq; // only necessary for PrintSortedTotals()

namespace RandomU64
{
    class Program
    {
        static private Random _clsRandom = new Random();

        private const int _ciIterations = 100000;

        private struct TotalsStructure
        {
            public int MethodNumber;
            public double TotalMilliseconds;
        }

        private static List<TotalsStructure> _lstMillisecondTotals = new List<TotalsStructure>();


        static void Main(string[] args)
        {
            Func<ulong>[] arr_Methods = { Method1, Method2, Method3, Method4, Method5 };

            Console.WriteLine(string.Format("{0} iterations for each method.", _ciIterations));

            foreach(Func<ulong> methodX in arr_Methods)
            {
                string sMethodName = methodX.Method.Name;
                double dTotalMilliseconds = RunMethod(methodX);

                PrintHeader(sMethodName);
                PrintFooter(dTotalMilliseconds);
                AddToStatsList(sMethodName, dTotalMilliseconds);
            }    

            PrintStats();
            Console.ReadLine();
        }

        static double RunMethod(Func<ulong> MethodX)
        {
            ulong ulResult;
            DateTime dtStart = DateTime.Now;

            for (int iCount = 1; iCount <= _ciIterations; iCount++)
                ulResult = MethodX.Invoke();

            return (DateTime.Now - dtStart).TotalMilliseconds;
        }


        static void PrintHeader(string sMethodName)
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine(string.Format("Running {0}()", sMethodName));
        }

        static void PrintFooter(double dTotalMilliseconds)
        {
            Console.WriteLine(string.Format("Elapsed time: {0} milliseconds", dTotalMilliseconds));
        }

        static void AddToStatsList(string sMethodName, double dTotalMilliseconds)
        {
            int iMethodNumber = Convert.ToInt32(sMethodName.Substring(sMethodName.Length - 1, 1));
            _lstMillisecondTotals.Add(new TotalsStructure { MethodNumber = iMethodNumber, TotalMilliseconds = dTotalMilliseconds });
        }

        static void PrintStats()
        {
            int[] arr_iLengths = _lstMillisecondTotals.OrderBy(s => s.TotalMilliseconds).Select(n => n.MethodNumber).ToArray();
            int iLimit = arr_iLengths.GetUpperBound(0);

            Console.WriteLine();
            Console.Write("Methods in order of speed (fastest -> slowest): ");
            
            for (int iIndex = 0; iIndex <= iLimit; iIndex++)
            {
                Console.Write(arr_iLengths[iIndex]);
                if (iIndex < iLimit)
                    Console.Write(", ");
                else
                    Console.WriteLine();
            }

        }

        // *********************************************************************************************************************************************************************************************************************************************************
      
        static ulong Method1()
        {
            /*
             * 1) Get signed 32-bit random integer x1 between -(2^31 + 1) and +(2^31 - 1)
             * 2) Get signed 32-bit random integer x2 between -(2^31 + 1) and +(2^31 - 1)
             * 3) Dynamically convert x1 to unsigned 64-bit and set y = x1;
             * 4) Shift y 32 bits to left
             * 5) y = y OR x2
            */

            int x1 = _clsRandom.Next(int.MinValue, int.MaxValue);
            int x2 = _clsRandom.Next(int.MinValue, int.MaxValue);
            ulong y;

            // lines must be separated or result won't go past 2^32
            y = (uint)x1;
            y = y << 32;
            y = y | (uint)x2;

            return y;
        }

        static ulong Method2()
        {
            /*
             * 1) ulResult = 0
             * 2) Loop power 0->63
             * 3)   dRandom = random floating point value between 0->1
             * 4)   if dRandom < 0.5
             * 5)       dValue = 2 ^ power
             * 6)       ulValue = dValue converted to unsigned 64-bit
             * 7)       ulResult = ulResult OR ulValue
             * 8)  End Loop
            */
  
            ulong ulResult = 0;

            for(int iPower = 0; iPower < 64; iPower++)
            {
                double dRandom = _clsRandom.NextDouble();
                if(dRandom > 0.5)
                {
                    double dValue = Math.Pow(2, iPower);
                    ulong ulValue = Convert.ToUInt64(dValue);
                    ulResult = ulResult | ulValue;
                }
            }

            return ulResult;
        }

        static ulong Method3()  // only difference between #3 and #2 is that this one (#3) uses .Next() instead of .NextDouble()
        {
            /*
             * 1) ulResult = 0
             * 2) Loop power 0->63
             * 3)   if random 0 or 1 = 1
             * 4)       ulResult = ulResult OR ConvertTo64bit(2 ^ power)
             * 5) End Loop
            */

            ulong ulResult = 0;

            for (int iPower = 0; iPower < 64; iPower++)
                if (_clsRandom.Next(0, 1) == 1)
                    ulResult = ulResult | Convert.ToUInt64(Math.Pow(2, iPower));

            return ulResult;
        }

        static ulong Method4()
        {
            /*
             * 1) Create 8-element byte[] array (each element is 8 bits)
             * 2) Fill each element with random number between 0-255 (0 -> 2^8 - 1)
             * 3) Merge byte[] array into unsigned 64-bit integer
            */
            
            byte[] arr_bt = new byte[8];
            ulong ulResult;

            _clsRandom.NextBytes(arr_bt);
            ulResult = BitConverter.ToUInt64(arr_bt, 0);
            return ulResult;
        }

        // Next method courtesy of https://stackoverflow.com/questions/14708778/how-to-convert-unsigned-integer-to-signed-integer-without-overflowexception/39107847
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        struct EvilUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public int Int32;
            [System.Runtime.InteropServices.FieldOffset(0)] public uint UInt32;
        }

        static ulong Method5()
        {
            /*
             * evil.Int32 and evil.UInt32 occupy same space in memory (aka unions). So, any change to one immediately changes the other
             * 
             * 1) Get signed 32-bit random integer evil.Int32 between -(2^31 + 1) and +(2^31 - 1)
             * 2) ulResult = evil.UInt32
             * 3) Shift ulResult value 32 bits to left
             * 4) Get new signed 32-bit random integer value into evil.Int32 between -(2^31 + 1) and +(2^31 - 1)
             * 5) ulResult = ulResult | evil.UInt32
            */
            var evil = new EvilUnion();
            ulong ulResult = 0;

            evil.Int32 = _clsRandom.Next(int.MinValue, int.MaxValue);
            ulResult = evil.UInt32;
            ulResult = ulResult << 32;
            evil.Int32 = _clsRandom.Next(int.MinValue, int.MaxValue);
            ulResult = ulResult | evil.UInt32;

            return ulResult;
        }
    }
}
