﻿using System.Threading.Tasks;
using Lyn.Protocol.Bolt1.Messages;
using Lyn.Protocol.Common.Messages;

namespace Lyn.Protocol.Connection
{
    public interface IBoltMessageService<T> where T : MessagePayload
    {
        Task ProcessMessageAsync(PeerMessage<T> message);
    }
}