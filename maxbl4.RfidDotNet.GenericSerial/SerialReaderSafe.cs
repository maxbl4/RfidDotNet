using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.RfidDotNet.GenericSerial.Model;

namespace maxbl4.RfidDotNet.GenericSerial
{
    public class SerialReaderSafe: IDisposable
    {
        private volatile bool disposed = false;
        private readonly IObserver<bool> connected;
        private readonly IObserver<Exception> errors;
        private ConnectionString connectionString;
        private readonly SemaphoreSlim actionSemaphore = new(1);
        private SerialReader serialReader;

        public SerialReaderSafe(ConnectionString connectionString, IObserver<bool> connected, IObserver<Exception> errors)
        {
            this.connected = connected;
            this.errors = errors;
            this.connectionString = connectionString.Clone();
        }
        
        public async Task<T> Do<T>(Func<SerialReader, Task<T>> action)
        {
            using var lck = actionSemaphore.UseOnce();
            try
            {
                return await action(await GetReader());
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            return default;
        }
        
        public async Task Do(Action<SerialReader> action)
        {
            using var lck = actionSemaphore.UseOnce();
            try
            {
                action(await GetReader());
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void UpdateConnectionString(ConnectionString newConnectionString)
        {
            using var lck = actionSemaphore.UseOnce();
            connectionString = newConnectionString.Clone();
            serialReader.DisposeSafe();
            serialReader = null;
        }

        private void HandleError(Exception exception)
        {
            try
            {
                serialReader.DisposeSafe();
                serialReader = null;
                errors.OnNext(exception);
            }catch {}
        }

        private async Task<SerialReader> GetReader()
        {
            if (disposed)
                throw new ObjectDisposedException("SerialReaderSafe");
            if (serialReader != null)
                return serialReader;
            serialReader = new SerialReader(new SerialConnectionString(connectionString).Connect())
            {
                ThrowOnIllegalCommandError = false
            };
            await serialReader.ActivateOnDemandInventoryMode(true);
            await serialReader.SetAntennaConfiguration((GenAntennaConfiguration) connectionString.AntennaConfiguration);
            await serialReader.SetRFPower((byte)connectionString.RFPower);
            await serialReader.SetInventoryScanInterval(TimeSpan.FromMilliseconds(connectionString.InventoryDuration));
            serialReader.Errors.Subscribe(e =>
            {
                connected.OnNext(false);
                errors.OnNext(e);
            });
            return serialReader;
        }

        public void Dispose()
        {
            disposed = true;
            serialReader.DisposeSafe();
            actionSemaphore.DisposeSafe();
        }
    }
}