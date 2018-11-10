using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class TimeoutAction
    {
        public static IDisposable Set(int timeout, Action onExipiration)
        {
            if (timeout == int.MaxValue || timeout <= 0) return Disposable.Empty;
            return Observable.Timer(DateTime.Now.AddMilliseconds(timeout))
                .Subscribe(x => onExipiration());
        }
    }
}