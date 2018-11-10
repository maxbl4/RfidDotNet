namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public class TagStreamParser
    {
        public ReaderInfo Reader { get; } = new ReaderInfo();
        public Tag Tag { get; private set; }

        public TagStreamParserReponse Parse(string msg)
        {
            if (msg.StartsWith("#"))
            {
                if (Reader.ParseLine(msg))
                    return TagStreamParserReponse.ParsedReader;
                return TagStreamParserReponse.Failed;
            }

            if (Tag.TryParse(msg, out var tag))
            {
                tag.Reader = Reader;
                Tag = tag;
                return TagStreamParserReponse.ParsedTag;
            }

            Tag = null;
            return TagStreamParserReponse.Failed;
        }
    }

    public enum TagStreamParserReponse
    {
        ParsedReader,
        ParsedTag,
        Failed
    }
}