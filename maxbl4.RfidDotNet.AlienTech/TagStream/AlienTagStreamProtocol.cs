using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Net;
using Serilog;

namespace maxbl4.RfidDotNet.AlienTech.TagStream
{
    public class AlienTagStreamProtocol : DuplexProtocol
    {
        static readonly ILogger Logger = Log.ForContext<AlienTagStreamProtocol>();
        private readonly IObserver<Tag> tags;
        private readonly IObserver<string> unparsedMessages;
        readonly TagStreamParser parser = new TagStreamParser();
        
        public override string IncomingMessageTerminators => "\r\n\0";

        public AlienTagStreamProtocol(IObserver<Tag> tags, IObserver<string> unparsedMessages) : base(int.MaxValue)
        {
            this.tags = tags;
            this.unparsedMessages = unparsedMessages;
        }

        public void Accept(Socket client)
        {
            if (client?.Connected != true)
                throw new ArgumentException("Socket should be connected", nameof(client));
            Connect(client);
            new Task(ReceiveLoop, TaskCreationOptions.LongRunning).Start();
        }

        private async void ReceiveLoop()
        {
            try
            {
                while (IsConnected)
                {
                    var msgs = await Receive();
                    Logger.Debug("Received {Count} messages", msgs.Count);
                    foreach (var msg in msgs)
                    {
                        switch (parser.Parse(msg))
                        {
                            case TagStreamParserReponse.ParsedTag:
                                Logger.Debug("Parsed tag");
                                tags.OnNext(parser.Tag);
                                break;
                            case TagStreamParserReponse.Failed:
                                Logger.Debug("Parsed failure");
                                unparsedMessages.OnNext(msg);
                                break;
                            case TagStreamParserReponse.ParsedReader:
                                Logger.Debug("Parsed reader");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("{ex}", ex);
            }
        }
    }
}