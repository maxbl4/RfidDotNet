using System.Collections.Generic;
using System.Linq;

namespace maxbl4.RfidDotNet.Infrastructure
{
    public class Triple
    {
        public static readonly Triple Empty = new Triple(new []{0d});
        public Triple(IEnumerable<double> source)
        {
            Min = (int)source.Min();
            Avg = (int)source.Average();
            Max = (int)source.Max();
            Sum = (int)source.Sum();
        }

        public int Max { get; }
        public int Avg { get; }
        public int Min { get; }
        public int Sum { get; }

        public override string ToString()
        {
            return $"Min={Min}, Avg={Avg}, Max={Max}, Sum={Sum}";
        }
    }
}