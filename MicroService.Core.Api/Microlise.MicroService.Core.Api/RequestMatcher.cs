using System;
using System.Linq;
using Microlise.MicroService.Core.Api.Utils;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api
{
    internal class RequestMatcher : IRequestMatcher
    {
        private readonly AutoDictionary<Guid, RequestData> requests = new AutoDictionary<Guid, RequestData>();

        public void RegisterWaitHandle(Guid uuid, DataWaitHandle handle)
        {
            lock (requests)
            {
                if (requests.ContainsKey(uuid))
                    requests[uuid].Handle = handle;
                else
                    requests[uuid] = new RequestData(uuid, handle);
                ProcessRemovals();
            }
        }

        public void RegisterMessageWrapper(Guid uuid, MessageWrapper wrapper)
        {
            lock (requests)
            {
                if (requests.ContainsKey(uuid))
                    requests[uuid].Wrapper = wrapper;
                else
                    requests[uuid] = new RequestData(uuid, wrapper);
                ProcessRemovals();
            }
        }

        private void ProcessRemovals()
        {
            Guid[] removals = requests.Where(kvp => kvp.Value.Set || kvp.Value.Expired).Select(kvp => kvp.Key).ToArray();
            foreach (Guid guid in removals)
                requests.Remove(guid);
        }

        internal class RequestData
        {
            internal Guid Uuid;
            private readonly DateTime createdAt = DateTime.UtcNow;
            internal bool Set;
            internal bool Expired => DateTime.UtcNow - createdAt > TimeSpan.FromMinutes(1);

            private DataWaitHandle handle;
            private MessageWrapper wrapper;

            internal DataWaitHandle Handle
            {
                set
                {
                    handle = value;
                    if (wrapper == null) return;

                    SetHandle();
                }
            }

            internal MessageWrapper Wrapper
            {
                set
                {
                    wrapper = value;
                    if (handle == null) return;

                    SetHandle();
                }
            }

            private void SetHandle()
            {
                handle.Set(wrapper.SolutionJson, wrapper.ErrorsJson);
                Set = true;
            }

            public RequestData(Guid uuid, DataWaitHandle handle)
            {
                Uuid = uuid;
                Handle = handle;
            }

            public RequestData(Guid uuid, MessageWrapper wrapper)
            {
                Uuid = uuid;
                Wrapper = wrapper;
            }
        }
    }
}