﻿using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.RfidDotNet;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class FakeUniversalTagStream : IUniversalTagStream
    {
        public virtual void Dispose()
        {
        }

        public virtual Task Start()
        {
            return Task.CompletedTask;
        }

        public virtual Task<int> QValue(int? newValue = null)
        {
            return Task.FromResult(0);
        }

        public virtual Task<int> Session(int? newValue = null)
        {
            return Task.FromResult(0);
        }

        public virtual Task<int> RFPower(int? newValue = null)
        {
            return Task.FromResult(0);
        }

        public virtual Task<AntennaConfiguration> AntennaConfiguration(AntennaConfiguration? newValue = null)
        {
            return Task.FromResult(RfidDotNet.AntennaConfiguration.Antenna1);
        }
        
        public Subject<Tag> TagsSubject { get; set; } = new Subject<Tag>();
        public Subject<Exception> ErrorsSubject { get; set; } = new Subject<Exception>();
        public Subject<bool> ConnectedSubject { get; set; } = new Subject<bool>();

        public virtual IObservable<Tag> Tags => TagsSubject;
        public virtual IObservable<Exception> Errors => ErrorsSubject;
        public virtual IObservable<bool> Connected => ConnectedSubject;
    }
}