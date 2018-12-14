using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialUnifiedTagStream : IUniversalTagStream
    {
        private ConnectionString connectionString;
        private SerialReader serialReader;

        public SerialUnifiedTagStream(ConnectionString cs)
        {
            connectionString = cs.Clone();
            serialReader = new SerialReader(new SerialConnectionString(connectionString).Connect());
        }

        public void Dispose()
        {
            serialReader?.DisposeSafe();
        }

        readonly Subject<Tag> tags = new Subject<Tag>();
        public IObservable<Tag> Tags => tags;
        readonly Subject<Exception> errors = new Subject<Exception>();
        public IObservable<Exception> Errors => errors;
        readonly BehaviorSubject<bool> connected = new BehaviorSubject<bool>(false);
        public IObservable<bool> Connected => connected;
        public Task Start()
        {
            throw new NotImplementedException();
        }

        public Task<int> QValue(int? newValue = null)
        {
            if (newValue != null) connectionString.QValue = newValue.Value;
            return Task.FromResult(connectionString.QValue);
        }

        public Task<int> Session(int? newValue = null)
        {
            if (newValue != null) connectionString.Session = newValue.Value;
            return Task.FromResult(connectionString.Session);
        }

        public async Task<int> RFPower(int? newValue = null)
        {
            if (newValue != null)
            {
                connectionString.RFPower = newValue.Value;
                await serialReader.SetRFPower((byte) newValue.Value);
            }

            var info = await serialReader.GetReaderInfo();
            return info.RFPower;
        }

        public async Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null)
        {
            if (newValue != null)
            {
                connectionString.AntennaConfiguration = newValue.Value;
                await serialReader.SetAntennaConfiguration((GenAntennaConfiguration) newValue.Value);
            }
            var info = await serialReader.GetReaderInfo();
            return (AntennaConfiguration)info.AntennaConfiguration;
        }
    }
}