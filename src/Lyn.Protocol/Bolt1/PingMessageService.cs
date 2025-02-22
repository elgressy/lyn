using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Lyn.Protocol.Bolt1.Messages;
using Lyn.Protocol.Common;
using Lyn.Protocol.Common.Messages;
using Lyn.Protocol.Connection;
using Lyn.Types.Fundamental;
using Microsoft.Extensions.Logging;

namespace Lyn.Protocol.Bolt1
{
    public class PingMessageService : IBoltMessageService<PingMessage>, IPingMessageAction
    {
        private readonly ILogger<PingMessageService> _logger;

        private const int PING_INTERVAL_SECS = 30;
      
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRandomNumberGenerator _numberGenerator;
        
        private DateTime? _lastPingReceivedDateTime; // the service lifetime will be associated with a node so no need to store in repo
        private readonly IPingPongMessageRepository _messageRepository;


        public PingMessageService(ILogger<PingMessageService> logger, IDateTimeProvider dateTimeProvider, 
            IRandomNumberGenerator numberGenerator, IPingPongMessageRepository messageRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider;
            _numberGenerator = numberGenerator;
            _messageRepository = messageRepository;
        }

        public async Task<MessageProcessingOutput> ProcessMessageAsync(PeerMessage<PingMessage> request)
        {
            var utcNow = _dateTimeProvider.GetUtcNow();
            
            if (_lastPingReceivedDateTime > utcNow.AddSeconds(-PING_INTERVAL_SECS))
                throw new ProtocolViolationException( //TODO David this case requires failing all the channels with the node
                    $"Ping message can only be received every {PING_INTERVAL_SECS} seconds");

            if (request.MessagePayload.NumPongBytes > PingMessage.MAX_BYTES_LEN)
                return new MessageProcessingOutput();

            _lastPingReceivedDateTime = utcNow;

            _logger.LogDebug($"Send pong to with length {request.MessagePayload.NumPongBytes}");

            var pong = new PongMessage
            {
                BytesLen = request.MessagePayload.NumPongBytes,
                Ignored = new byte[request.MessagePayload.NumPongBytes]
            };

            return new MessageProcessingOutput
            {
                Success = true,
                ResponseMessages = new[] {new BoltMessage {Payload = pong}}
            };
        }

        public int ActionTimeIntervalSeconds()
        {
            return PING_INTERVAL_SECS;
        }

        public async Task<MessageProcessingOutput> GeneratePingMessageAsync(PublicKey nodeId, CancellationToken token)
        {
            var bytesLength = _numberGenerator.GetUint16() % PingMessage.MAX_BYTES_LEN;
         
            while(await _messageRepository.PendingPingExistsForIdAsync(nodeId,(ushort) bytesLength)) //TODO David need to get all pending from repo and check locally *but chances of more than one attempt are very small
                bytesLength = _numberGenerator.GetUint16() % PingMessage.MAX_BYTES_LEN;
         
            var pingMessage = new PingMessage((ushort)bytesLength);

            await _messageRepository.AddPingMessageAsync(nodeId, _dateTimeProvider.GetUtcNow(),pingMessage);

            _logger.LogDebug($"Ping generated ,pong length {pingMessage.NumPongBytes}");
            
            return new MessageProcessingOutput
            {
                Success = true,
                ResponseMessages = new []{new BoltMessage {Payload = pingMessage}}
            };
        }
    }
}