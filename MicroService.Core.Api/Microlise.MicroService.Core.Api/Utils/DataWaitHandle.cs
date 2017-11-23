using System;
using System.Threading;

namespace Microlise.MicroService.Core.Api.Utils
{
    public sealed class DataWaitHandle : EventWaitHandle
    {
        public DataWaitHandle(bool initialState, EventResetMode mode) : base(initialState, mode) { }
        private DataWaitHandle(bool initialState, EventResetMode mode, string name) : base(initialState, mode, name) { }
        private DataWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew) : base(initialState, mode, name, out createdNew) { }
        public string Data { get; set; }

        public void Set(string data)
        {
            Data = data;

            try
            {
                Set();
            }
            catch (ObjectDisposedException)
            {
                //EventWaitHandle has already timed out so cannot Set() it
            }
        }
    }
}
