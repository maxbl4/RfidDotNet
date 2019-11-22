using System;
using maxbl4.RfidDotNet.AlienTech.TagStream;

namespace maxbl4.RfidDotNet.AlienTech.Extensions.TagFormatting
{
    public static class StringExtensions
    {
        public static string ToTagString(this string tagId)
        {
            return new Tag
            {
                TagId = tagId,
                DiscoveryTime = DateTime.UtcNow,
                LastSeenTime = DateTime.UtcNow,
                ReadCount = 1
            }.ToCustomFormatString();
        }
    }
}