using System;
using maxbl4.RfidDotNet.AlienTech.TagStream;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class StringExtensions
    {
        public static string ToTagString(this string tagId)
        {
            return new Tag
            {
                TagId = tagId,
                DiscoveryTime = DateTimeOffset.Now,
                LastSeenTime = DateTimeOffset.Now,
                ReadCount = 1
            }.ToCustomFormatString();
        }
    }
}