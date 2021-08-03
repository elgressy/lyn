using System.Buffers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lyn.Protocol.Bolt1;
using Lyn.Protocol.Bolt1.Entities;
using Lyn.Protocol.Bolt2.ChannelEstablishment;
using Lyn.Protocol.Bolt2.ChannelEstablishment.Messages;
using Lyn.Protocol.Bolt3;
using Lyn.Protocol.Bolt9;
using Lyn.Protocol.Common;
using Lyn.Protocol.Common.Blockchain;
using Lyn.Protocol.Common.Messages;
using Lyn.Protocol.Connection;
using Lyn.Types;
using Lyn.Types.Bitcoin;
using Lyn.Types.Bolt;
using Lyn.Types.Fundamental;
using Lyn.Types.Serialization;
using Lyn.Types.Serialization.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lyn.Protocol.Tests.Bolt2
{
    public class AcceptChannelMessageServiceTests
    {
        private AcceptChannelMessageService _sut;

        private InMemoryChannelCandidateRepository _candidateRepository;
        private readonly Mock<ISecretStore> _store;

        private SerializationFactory serializationFactory;

        private InMemoryPeerRepository inMemoryPeerRepository;

        public AcceptChannelMessageServiceTests()
        {
            _candidateRepository = new InMemoryChannelCandidateRepository();

            _store = new Mock<ISecretStore>();

            var ci = new ServiceCollection().AddSerializationComponents().BuildServiceProvider();
            serializationFactory = new SerializationFactory(ci);

            inMemoryPeerRepository = new InMemoryPeerRepository();

            _sut = new AcceptChannelMessageService(new Logger<AcceptChannelMessageService>(new LoggerFactory()),
            new LightningTransactions(new Logger<LightningTransactions>(new LoggerFactory()), serializationFactory,
                new LightningScripts()),
            new TransactionHashCalculator(ci.GetService<IProtocolTypeSerializer<Transaction>>()),
            new LightningScripts(),
            new LightningKeyDerivation(),
            _candidateRepository,
            new ChainConfigProvider(),
            _store.Object,
            inMemoryPeerRepository);
        }

        [Fact]
        public void TestGeneratingFundingCreated()
        {
            var acceptChannel = new AcceptChannel
            {
                FundingPubkey = Hex.FromString("0x03100a0e62903ae4a67902241f41590bce4f925e3676a41667388c8b2da446b25f"),
                HtlcBasepoint = Hex.FromString("0x02615c1607d46592c11c3dc7e3215cf05fbf1c417702ae88be500c22a30c084de0"),
                MinimumDepth = 3,
                PaymentBasepoint =
                    Hex.FromString("0x024e50f1f547d9cb1150fd19b252e2bc56966f3000b331f6ff12c9736f041e9e02"),
                RevocationBasepoint =
                    Hex.FromString("0x02a6ea134b2fda74bdb23de5b1f060c96d8c6d0bf405a102ed65af5e854cda1879"),
                ChannelReserveSatoshis = 160000,
                DelayedPaymentBasepoint =
                    Hex.FromString("0x025b646fef398cffd0a2a4584ae604a788b14ff57350a0ce548d1825af3d762db7"),
                DustLimitSatoshis = 546,
                HtlcMinimumMsat = 1,
                MaxAcceptedHtlcs = 30,
                TemporaryChannelId =
                    new UInt256(Hex.FromString("431edbcc612dffd3ede22b84beca344a810cc360b94b22601d165f584159c085")),
                ToSelfDelay = 720,
                FirstPerCommitmentPoint =
                    Hex.FromString("0x03cf020f4341d3ef7af94b49f13d5d234ed82887529f122bb6d27d2ba645ac4340"),
                MaxHtlcValueInFlightMsat = 5000000000
            };

            var openChannel = new OpenChannel
            {
                ChainHash = new UInt256(
                    Hex.FromString("06226e46111a0b59caaf126043eb5bbf28c34f3a5e332a1fc7b2b73cf188910f")),
                ChannelFlags = 0,
                FundingPubkey = Hex.FromString("0x02f46a97ef60cf4f4e09882439683346f71b2edc07cbe64a5d57d0d727e3fed231"),
                FundingSatoshis = 16000000,
                HtlcBasepoint = Hex.FromString("0x033318fa8caf6f23df0524b5ceb45b4f364526dca74ab3a85cdd11554eae93ab86"),
                PaymentBasepoint =
                    Hex.FromString("0x03e23c94d3e266ccf10f930fea0e59c2267c649866c0ffd71dcd295f9135712064"),
                PushMsat = 0,
                RevocationBasepoint =
                    Hex.FromString("0x02be67385cac45c599e2ce7e6e59a518b01e36f1b58cab5410031ba00bed4bfb21"),
                ChannelReserveSatoshis = 160000,
                DelayedPaymentBasepoint =
                    Hex.FromString("0x02f4b187ffc1fd574493726193750f06adde9c9d0b2b31d9d006a5c9bcf33139d6"),
                DustLimitSatoshis = 100,
                FeeratePerKw = 1000,
                HtlcMinimumMsat = 30000,
                MaxAcceptedHtlcs = 100,
                TemporaryChannelId =
                    new UInt256(Hex.FromString("431edbcc612dffd3ede22b84beca344a810cc360b94b22601d165f584159c085")),
                ToSelfDelay = 2016,
                FirstPerCommitmentPoint =
                    Hex.FromString("0x03a25b530f11377cb644fab06bb363aaaa70e0f78a785ee057ac7c1519ae7dee41"),
                MaxHtlcValueInFlightMsat = 12000000
            };

            var seed = new Secret(Hex.FromString("0x179c322acdd402a29131762db49ff5011916a8e86b752dbae2a9a8d664cf6ce0"));

            //_sut.ProcessMessageAsync();
        }

        [Fact]
        public void TestFromSeed()
        {
            var remp = TransactionHelper.ParseToString(
                serializationFactory.Deserialize<Transaction>(
                    Hex.FromString("020000000001018701bec7eacc574c242235a66699361246b1f04ab0fabc79fdee339d21796d54000000000008e3eb8001c785010000000000160014c61c93543d40bfeeda5942d99b2e43a4d79dbf4c0400473044022047304402203cffd29f644771b14e6f66dc3aa98144df2a3717144024eaa536af02201333be68f5022040202def68a9bce2fbffda01cdbe0aaba4b452720ceab14c1901473044022021815276374b01276ad5b8b86df8238bb12d31c2001b4a5c034fca46e910454c02206dc835fb94dea3070a9bd777f25c99209b0527f4295e63a4a2b58ea03f71731c0147522102b085ac037bb3b3ab6de81abf620e42df8d2a51ce4de2905b83bcd514e39f290f21034199f4188b3ad214ccdd9be733703375bcb398d8abe7e169f57a017818ccdc1752aeeaa67720")));

            var locp = TransactionHelper.ParseToString(
                serializationFactory.Deserialize<Transaction>(
                    Hex.FromString("0x020000000001018701bec7eacc574c242235a66699361246b1f04ab0fabc79fdee339d21796d54000000000008e3eb8001c785010000000000160014c61c93543d40bfeeda5942d99b2e43a4d79dbf4c040047304402203cffd29f644771b14e6f66dc3aa98144df2a3717144024eaa536af1333be68f5022040202def68a9bce2fbffda01cdbe0aaba4b452720ceab14c19f27996440fe95d014a000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000047522102b085ac037bb3b3ab6de81abf620e42df8d2a51ce4de2905b83bcd514e39f290f21034199f4188b3ad214ccdd9be733703375bcb398d8abe7e169f57a017818ccdc1752aeeaa67720")));

            var seed = new Secret(Hex.FromString("0x5e46094b865e688419c3bec96de09da2f1e40fd71f79588c34502a12332ef074"));

            var nodeId = Hex.FromString("03702309a58b6067e51a93a213d72a9bdebf5f5f03960d9ff2bc311e301e4ba999");

            var rawAcceptChannel = "0xe4712a00c599e9be8dbc2f44e2624eb8a5ffd8b89eb1afe2d62e4d18adb4258a0000000000000222000000012a05f200000000000002710000000000000000010000000302d0001e03e795e84d991e34b591f1a7bf2fe7d0489557b3dcf18bd51cd05e4dbebe27bd3f02ba6c3df56bf7825d9d8386759bee9b85f1b1c600ca4c80bbc0d9f1baf236b7040388ba412e864141a791b465b45b3dea39d62f5c72978726d815d406007b35d84b03ff7cee4f9082560c3156a945c9e45e311537f8032b239ad03b76d91fc646db4702796917759bc5d00f29b5ebe0520278c96a1f6c0eb1ee3e13ea4cdd2495ececf8036170c4015d4614ad2117175498a91d59f919a514e4e9615353b1f0468695a171";

            var deserializedAcceptChannel = serializationFactory.Deserialize<AcceptChannel>(Hex.FromString(rawAcceptChannel));

            _store.Setup(_ => _.GetSeed())
                .Returns(seed);

            inMemoryPeerRepository.AddNewPeerAsync(new Peer
            {
                Featurs = 0,
                NodeId = nodeId
            }).GetAwaiter().GetResult();

            var generator = new Mock<IRandomNumberGenerator>();

            generator.Setup(_ => _.GetBytes(32))
                .Returns(deserializedAcceptChannel.TemporaryChannelId.GetBytes().ToArray());

            var startOpenChannelService = new StartOpenChannelService(
                new Logger<OpenChannelMessageService>(new LoggerFactory()),
                generator.Object,
                new LightningKeyDerivation(),
                _candidateRepository,
                inMemoryPeerRepository,
                new ChainConfigProvider(),
                new LynImplementedBoltFeatures(new ParseFeatureFlags()),
                new ParseFeatureFlags(),
                _store.Object);

            var openChannelResponse = startOpenChannelService.CreateOpenChannelAsync(new CreateOpenChannelIn(
                nodeId,
                new UInt256(Hex.FromString("0f9188f13cb7b2c71f2a335e3a4fc328bf5beb436012afca590b1a11466e2206")),
                16000000, 0, 1000, true))
                .GetAwaiter().GetResult();

            var expectedTransaction = Hex.FromString("020000000001017e3e2666942003b00119281be2665503f96543f099bb814a5454644796f08a02000000000035638880012c21f40000000000160014ee666c47268bf2571e1ffe51cd7c7a262f186ca6040047304402203a77357de8c239b83a6612a7cbcfb2d2041770a6ede0921761646bb057fe0e3d0220046662c9eb643c78b487e5b87014f3350a5f3cd09d718be5f9bd22d919f0e8130147304402207c45f65f5eb852f8e3861c2e490597a37497d1382b0455e5d5732ebb9513beba0220444849aa6e5d86b2d01f5e0303dfecdcb7708087f524db0db7ee8a6b631b9a520147522102b085ac037bb3b3ab6de81abf620e42df8d2a51ce4de2905b83bcd514e39f290f2103e795e84d991e34b591f1a7bf2fe7d0489557b3dcf18bd51cd05e4dbebe27bd3f52ae4616b020");

            //var expectedTransaction = Hex.FromString("020000000116d700d0d653aa659a018da8936f9f48307117777037b52455d433d8ccd8face00000000003896d48001c785010000000000160014b2e7cc254f13a72fc66916ff7dc14caac9ef8140f5703d20");

            var transaction = serializationFactory.Deserialize<Transaction>(expectedTransaction);

            var result = _sut.ProcessMessageAsync(new PeerMessage<AcceptChannel>(nodeId, new BoltMessage { Payload = deserializedAcceptChannel }))
                .GetAwaiter().GetResult();

            var candidate = _candidateRepository.ChannelStates.First().Value;

            var trx1 = TransactionHelper.ParseToString(transaction);
            var trx2 = TransactionHelper.ParseToString(candidate.CommitmentTransaction);

            candidate.CommitmentTransaction.Should()
                .BeEquivalentTo(transaction);
        }
    }
}