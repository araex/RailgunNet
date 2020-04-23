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

using System.Collections.Generic;

namespace Railgun
{
#if CLIENT
    public interface IRailServerPacket : IRailPacket
    {
        Tick ServerTick { get; }
        IEnumerable<RailStateDelta> Deltas { get; }
    }
#endif

    /// <summary>
    ///     Packet sent from server to client.
    /// </summary>
    public class RailServerPacket
        : RailPacket
#if CLIENT
    , IRailServerPacket
#endif
    {
        #region Interface

#if CLIENT
        Tick IRailServerPacket.ServerTick { get { return this.SenderTick; } }
        IEnumerable<RailStateDelta> IRailServerPacket.Deltas { get { return this.deltas.Received; } }
#endif

        #endregion

#if CLIENT
        public IEnumerable<RailStateDelta> Deltas { get { return this.deltas.Received; } }
#endif
#if SERVER
        public IEnumerable<RailStateDelta> Sent => deltas.Sent;
#endif

        private readonly RailPackedListS2C<RailStateDelta> deltas;

        public RailServerPacket()
        {
            deltas = new RailPackedListS2C<RailStateDelta>();
        }

        public override void Reset()
        {
            base.Reset();

            deltas.Clear();
        }

#if SERVER
        public void Populate(
            IEnumerable<RailStateDelta> activeDeltas,
            IEnumerable<RailStateDelta> frozenDeltas,
            IEnumerable<RailStateDelta> removedDeltas)
        {
            deltas.AddPending(removedDeltas);
            deltas.AddPending(frozenDeltas);
            deltas.AddPending(activeDeltas);
        }
#endif

        #region Encode/Decode

        protected override void EncodePayload(
            RailResource resource,
            RailBitBuffer buffer,
            Tick localTick,
            int reservedBytes)
        {
#if SERVER
            // Write: [Deltas]
            EncodeDeltas(resource, buffer, reservedBytes);
        }

        private void EncodeDeltas(
            RailResource resource,
            RailBitBuffer buffer,
            int reservedBytes)
        {
            deltas.Encode(
                buffer,
                RailConfig.PACKCAP_MESSAGE_TOTAL - reservedBytes,
                RailConfig.MAXSIZE_ENTITY,
                delta => RailState.EncodeDelta(resource, buffer, delta));
#endif
        }

        protected override void DecodePayload(
            RailResource resource,
            RailBitBuffer buffer)
        {
#if CLIENT
            // Read: [Deltas]
            this.DecodeDeltas(resource, buffer);
        }

        private void DecodeDeltas(
          RailResource resource,
          RailBitBuffer buffer)
        {
            this.deltas.Decode(
              buffer,
              () => RailState.DecodeDelta(resource, buffer, this.SenderTick));
#endif
        }

        #endregion
    }
}