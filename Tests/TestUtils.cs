﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using RailgunNet.Connection;
using RailgunNet.Factory;
using RailgunNet.Logic;
using RailgunNet.Logic.Wrappers;
using RailgunNet.System.Encoding;
using RailgunNet.System.Encoding.Compressors;
using RailgunNet.System.Types;

namespace Tests
{
    public class TestUtils
    {
        public static Mock<IRailCommandConstruction> CommandConstructionMock()
        {
            Mock<IRailCommandConstruction> mock = new Mock<IRailCommandConstruction>();
            mock.Setup(f => f.CreateCommand()).Returns(new Command());
            mock.Setup(f => f.CreateCommandUpdate()).Returns(new RailCommandUpdate());
            return mock;
        }

        public static Mock<IRailEventConstruction> EventConstructionMock()
        {
            Mock<IRailEventConstruction> mock = new Mock<IRailEventConstruction>();
            mock.Setup(f => f.CreateEvent(0)).Returns(new Event());
            mock.Setup(f => f.EventTypeCompressor).Returns(new RailIntCompressor(0, 2));
            return mock;
        }

        public static Mock<IRailStateConstruction> StateConstructionMock()
        {
            Mock<IRailStateConstruction> mock = new Mock<IRailStateConstruction>();
            mock.Setup(f => f.CreateState(0)).Returns(new State());
            mock.Setup(f => f.CreateDelta()).Returns(new RailStateDelta());
            mock.Setup(f => f.CreateRecord()).Returns(new RailStateRecord());
            mock.Setup(f => f.EntityTypeCompressor).Returns(new RailIntCompressor(0, 2));
            return mock;
        }

        public static Tick CreateTick(uint uiValue)
        {
            // Ticks can, by design, not be created from a raw value. We can work around this since
            // we know that ticks are serialized as uint.
            RailBitBuffer bitBuffer = new RailBitBuffer(2);
            bitBuffer.WriteUInt(uiValue);
            return Tick.Read(bitBuffer);
        }

        public static SequenceId CreateSequenceId(int iValue)
        {
            return SequenceId.Start + iValue;
        }

        public class Command : RailCommand
        {
            public Command() : this(0)
            {
            }

            public Command(int i)
            {
                Data = i;
            }

            [CommandData] public int Data { get; set; }
        }

        public class Event : RailEvent
        {
            protected override void Execute(RailRoom room, RailController sender)
            {
            }
        }

        public class State : RailState
        {
        }

        public class RailPacketComparer : IEqualityComparer<RailPacketBase>
        {
            public bool Equals([AllowNull] RailPacketBase x, [AllowNull] RailPacketBase y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                bool bSameSenderTick = x.SenderTick == y.SenderTick;
                bool bSameAckTick = x.LastAckTick == y.LastAckTick;
                bool bSameAckEventId = x.LastAckEventId == y.LastAckEventId;

                return bSameSenderTick && bSameAckTick && bSameAckEventId;
            }

            public int GetHashCode([DisallowNull] RailPacketBase packet)
            {
                return packet.SenderTick.GetHashCode() ^
                       packet.LastAckTick.GetHashCode() ^
                       packet.LastAckEventId.GetHashCode();
            }
        }
    }
}
