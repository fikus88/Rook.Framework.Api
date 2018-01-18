using System;
using System.Threading;
using MongoDB.Bson;

namespace Microlise.MicroService.Core.Api.Utils
{
    public sealed class DataWaitHandle : EventWaitHandle
    {
        public DataWaitHandle(bool initialState, EventResetMode mode,
            Func<string, bool> solutionMatchFunction) : base(initialState, mode)
        {
            SolutionMatchFunction = solutionMatchFunction ?? (s => true);
        }
        private DataWaitHandle(bool initialState, EventResetMode mode, string name) : base(initialState, mode, name) { }
        private DataWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew) : base(initialState, mode, name, out createdNew) { }
        public string Solution { get; set; }
        public string Errors { get; set; }

        public Func<string, bool> SolutionMatchFunction { get; } = s => true;

        public void Set(string solution, string errors)
        {
            if (SolutionMatchFunction(solution))
            {
                Solution = solution;
                Errors = errors;

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
}
