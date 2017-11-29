using System;
using Microlise.MicroService.Core.Api.Utils;

namespace Microlise.MicroService.Core.Api
{
    public interface IRequestMatcher
    {
        void RegisterWaitHandle(Guid uuid, DataWaitHandle handle, ResponseStyle responseStyle);
        void RegisterMessageWrapper(Guid uuid, MessageWrapper wrapper);
    }
}