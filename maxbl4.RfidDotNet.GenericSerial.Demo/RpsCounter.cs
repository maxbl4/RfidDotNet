using System.Collections.Generic;
using System.Linq;

namespace maxbl4.RfidDotNet.GenericSerial.Demo
{
    public class RpsCounter
    {
        public static RpsStats Count(IList<Tag> list, int samplingIntervalMs)
        {
                var diffs = new List<double>(list.Count);
                var dates = list.OrderBy(x => x.LastSeenTime).ToList();
                for (int i = 1; i < dates.Count; i++)
                {
                    var msDIff = (dates[i].LastSeenTime - dates[i - 1].LastSeenTime).TotalMilliseconds;
                    diffs.Add(msDIff);
                }
                var ord = diffs.OrderByDescending(x => x).ToList();
                var report = new List<double>();
                if (ord.Count > 1)
                {
                    report.Add(ord[0]);
                    report.Add(ord[ord.Count * 1 / 10]);
                    report.Add(ord[ord.Count * 2 / 10]);
                    report.Add(ord[ord.Count * 3 / 10]);
                    report.Add(ord[ord.Count * 4 / 10]);
                    report.Add(ord[ord.Count * 5 / 10]);
                    report.Add(ord[ord.Count * 6 / 10]);
                    report.Add(ord[ord.Count * 7 / 10]);
                    report.Add(ord[ord.Count * 8 / 10]);
                    report.Add(ord[ord.Count * 9 / 10]);
                    report.Add(ord.Last());
                }
            
                var aggTags = list.GroupBy(x => x.TagId)
                    .Select(x => new Tag{TagId = x.Key, ReadCount = x.Count()})
                    .OrderBy(x => x.TagId)
                    .ToList();

                var rps = new RpsStats {
                    Histogram = report, 
                    AggTags = aggTags,
                    Average = (diffs.Any() ? diffs.Average(x => x)*1000/ samplingIntervalMs : 0),
                    RPS = list.Sum(x => x.ReadCount)*1000/ samplingIntervalMs,
                    Reads = list.Sum(x => x.ReadCount),
                    TagIds = list.Select(x => x.TagId).Distinct().Count()
                };
            return rps;
        }
    }

    
    public class RpsStats
    {
        public List<double> Histogram { get; set; }
        public double Average { get; set; }
        public int RPS { get; set; }
        public int Reads { get; set; }
        public int TagIds { get; set; }
        public List<Tag> AggTags { get; set; }
    }
}