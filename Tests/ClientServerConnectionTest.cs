﻿using System;
using System.Linq;
using Moq;
using RailgunNet;
using RailgunNet.Connection.Client;
using RailgunNet.Connection.Server;
using RailgunNet.Factory;
using RailgunNet.Logic;
using Xunit;
using static Tests.Example.Util;

namespace Tests.Example
{
    public class ClientServerConnectionTest
    {
        public ClientServerConnectionTest()
        {
            peerClientSide = new Mock<InMemoryNetPeerWrapper>
            {
                CallBase = true
            };
            peerServerSide = new Mock<InMemoryNetPeerWrapper>
            {
                CallBase = true
            };
            peerClientSide.Object.OnSendPayload += peerServerSide.Object.ReceivePayload;
            peerServerSide.Object.OnSendPayload += peerClientSide.Object.ReceivePayload;
        }

        private readonly RailClient client = new RailClient(Registry.Get(Component.Client));
        private readonly RailServer server = new RailServer(Registry.Get(Component.Server));

        private readonly Mock<InMemoryNetPeerWrapper> peerClientSide;
        private readonly Mock<InMemoryNetPeerWrapper> peerServerSide;

        private static class Registry
        {
            public static RailRegistry Get(Component eComponent)
            {
                RailRegistry registry = new RailRegistry(eComponent);
                switch (eComponent)
                {
                    case Component.Server:
                        registry.AddEntityType<EntityServer, EntityState>();
                        break;
                    case Component.Client:
                        registry.AddEntityType<EntityClient, EntityState>();
                        break;
                }

                registry.SetCommandType<Command>();
                return registry;
            }
        }

        private class EntityState : RailState
        {
            [Mutable] public int EntityId { get; set; }
            [Mutable] [Compressor(typeof(CoordinateCompressor))] public float PosX { get; set; }
            [Mutable] [Compressor(typeof(CoordinateCompressor))] public float PosY { get; set; }
        }

        private class Command : RailCommand
        {
            [CommandData] [Compressor(typeof(CoordinateCompressor))] public float PosX { get; set; }
            [CommandData] [Compressor(typeof(CoordinateCompressor))] public float PosY { get; set; }
        }

        private class EntityClient : RailEntityClient<EntityState>
        {
        }

        private class EntityServer : RailEntityServer<EntityState>
        {
        }

        [Fact]
        private void EntitiesAreSynchronized()
        {
            // Initialization
            RailClientRoom clientRoom = client.StartRoom();
            RailServerRoom serverRoom = server.StartRoom();
            EntityServer entityServerSide = serverRoom.AddNewEntity<EntityServer>();
            server.AddClient(peerServerSide.Object, "");
            client.SetPeer(peerClientSide.Object);

            // Nothing has been sent or received yet
            peerClientSide.Verify(
                c => c.SendPayload(It.IsAny<ArraySegment<byte>>()),
                Times.Never());
            peerServerSide.Verify(
                c => c.SendPayload(It.IsAny<ArraySegment<byte>>()),
                Times.Never());
            Assert.Empty(clientRoom.Entities);
            Assert.Single(serverRoom.Entities);

            // Let the server send its update to the client
            for (int i = 0; i < RailConfig.SERVER_SEND_RATE + RailConfig.CLIENT_SEND_RATE + 1; ++i)
            {
                peerServerSide.Object.PollEvents();
                server.Update();
                peerClientSide.Object.PollEvents();
                client.Update();
            }

            // The client has received the server entity.
            Assert.Single(clientRoom.Entities);
            Assert.Single(serverRoom.Entities);

            // Clients representation of the entity is identical to the server
            RailEntityBase entityProxy = clientRoom.Entities.First();
            Assert.IsType<EntityClient>(entityProxy);
            EntityClient entityClientSide = entityProxy as EntityClient;
            Assert.NotNull(entityClientSide);
            Assert.Equal(entityServerSide.Id, entityProxy.Id);
            Assert.Equal(entityClientSide.State.PosX, entityServerSide.State.PosX);
            Assert.Equal(entityClientSide.State.PosY, entityServerSide.State.PosY);

            // Change the entity on the server side and sync it to the client
            float fExpectedPosX = 42;
            float fExpectedPosY = 106;
            entityServerSide.State.PosX = fExpectedPosX;
            entityServerSide.State.PosY = fExpectedPosY;

            // Let the server detect the change and send the packet
            server.Update();
            bool bWasSendTick = serverRoom.Tick.IsSendTick(RailConfig.SERVER_SEND_RATE);
            while (!bWasSendTick)
            {
                server.Update();
                bWasSendTick = serverRoom.Tick.IsSendTick(RailConfig.SERVER_SEND_RATE);
            }

            // Let the client receive & process the packet. We need to bring the client up to the same tick as the server to see the result.
            while (clientRoom.Tick < serverRoom.Tick)
            {
                peerClientSide.Object.PollEvents();
                client.Update();
            }

            Assert.Equal(fExpectedPosX, entityClientSide.State.PosX);
            Assert.Equal(fExpectedPosY, entityClientSide.State.PosY);
            Assert.Equal(fExpectedPosX, entityServerSide.State.PosX);
            Assert.Equal(fExpectedPosY, entityServerSide.State.PosY);
        }
    }
}
