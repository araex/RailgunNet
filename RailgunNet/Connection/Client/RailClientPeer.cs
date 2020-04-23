﻿/*
 *  RailgunNet - A Client/Server Network State-Synchronization Layer for Games
 *  Copyright (c) 2016-2018 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
 */

#if CLIENT
using System;
using System.Collections.Generic;

namespace Railgun
{
    /// <summary>
    /// The peer created by the client representing the server.
    /// </summary>
    public class RailClientPeer
      : RailPeer<RailServerPacket, RailClientPacket>
    {
        public event Action<IRailServerPacket> PacketReceived;

        private readonly RailView localView;
        private readonly Comparer<Tick> tickComparer;

        private List<IRailEntity> sortingList;

        public RailClientPeer(
          RailResource resource,
          IRailNetPeer netPeer,
          RailInterpreter interpreter)
          : base(
              resource,
              netPeer,
              RailConfig.SERVER_SEND_RATE,
              interpreter)
        {
            this.localView = new RailView();
            this.tickComparer = Tick.CreateComparer();
            this.sortingList = new List<IRailEntity>();
        }

        public void SendPacket(
          Tick localTick,
          IEnumerable<IRailEntity> controlledEntities)
        {
            // TODO: Sort controlledEntities by most recently sent

            RailClientPacket packet = base.PrepareSend<RailClientPacket>(localTick);
            packet.Populate(
              this.ProduceCommandUpdates(controlledEntities),
              this.localView);

            // Send the packet
            base.SendPacket(packet);

            foreach (RailCommandUpdate commandUpdate in packet.Sent)
                commandUpdate.Entity.AsBase.LastSentCommandTick = localTick;
        }

        protected override void ProcessPacket(
          RailPacket packet,
          Tick localTick)
        {
            base.ProcessPacket(packet, localTick);

            RailServerPacket serverPacket = (RailServerPacket)packet;
            foreach (RailStateDelta delta in serverPacket.Deltas)
                this.localView.RecordUpdate(
                  delta.EntityId,
                  packet.SenderTick,
                  localTick,
                  delta.IsFrozen);
            this.PacketReceived?.Invoke(serverPacket);
        }

        private IEnumerable<RailCommandUpdate> ProduceCommandUpdates(
          IEnumerable<IRailEntity> entities)
        {
            // If we have too many entities to fit commands for in a packet,
            // we want to round-robin sort them to avoid starvation
            this.sortingList.Clear();
            this.sortingList.AddRange(entities);
            this.sortingList.Sort(
              (x, y) => this.tickComparer.Compare(
                x.AsBase.LastSentCommandTick,
                y.AsBase.LastSentCommandTick));

            foreach (IRailEntity entity in sortingList)
            {
                RailCommandUpdate commandUpdate =
                  RailCommandUpdate.Create(
                    this.resource,
                    entity.Id,
                    entity.AsBase.OutgoingCommands);
                commandUpdate.Entity = entity;
                yield return commandUpdate;
            }
        }
    }
}
#endif