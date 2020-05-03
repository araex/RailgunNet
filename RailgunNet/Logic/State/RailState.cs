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

using RailgunNet.Factory;
using RailgunNet.Logic.Wrappers;
using RailgunNet.System.Encoding;
using RailgunNet.Util;
using RailgunNet.Util.Pooling;

namespace RailgunNet.Logic.State
{
    /// <summary>
    ///     States are the fundamental data management class of Railgun. They
    ///     contain all of the synchronized information that an Entity needs to
    ///     function. States have multiple sub-fields that are sent at different
    ///     cadences, as follows:
    ///     Mutable Data:
    ///     Sent whenever the state differs from the client's view.
    ///     Delta-encoded against the client's view.
    ///     Controller Data:
    ///     Sent to the controller of the entity every update.
    ///     Not delta-encoded -- always sent full-encode.
    ///     Immutable Data:
    ///     Sent only once at creation. Can not be changed after.
    /// </summary>
    public abstract class RailState : IRailPoolable<RailState>
    {
        private const uint FLAGS_ALL = 0xFFFFFFFF; // All values different

        public int FactoryType { get; set; }
        public uint Flags { get; set; } // Synchronized
        public bool HasControllerData { get; set; } // Synchronized
        public bool HasImmutableData { get; set; } // Synchronized

        public RailState Clone(IRailStateConstruction stateCreator)
        {
            RailState clone = stateCreator.CreateState(FactoryType);
            clone.OverwriteFrom(this);
            return clone;
        }

        public void OverwriteFrom(RailState source)
        {
            Flags = source.Flags;
            ApplyMutableFrom(source, FLAGS_ALL);
            ApplyControllerFrom(source);
            ApplyImmutableFrom(source);
            HasControllerData = source.HasControllerData;
            HasImmutableData = source.HasImmutableData;
        }

        [OnlyIn(Component.Client)]
        public void ApplyDelta(RailStateDelta delta)
        {
            RailState deltaState = delta.State;
            ApplyMutableFrom(deltaState, deltaState.Flags);

            ResetControllerData();
            if (deltaState.HasControllerData) ApplyControllerFrom(deltaState);
            HasControllerData = delta.HasControllerData;

            HasImmutableData = delta.HasImmutableData || HasImmutableData;
            if (deltaState.HasImmutableData) ApplyImmutableFrom(deltaState);
        }

        #region Pooling
        IRailMemoryPool<RailState> IRailPoolable<RailState>.Pool { get; set; }

        void IRailPoolable<RailState>.Reset()
        {
            Flags = 0;
            HasControllerData = false;
            HasImmutableData = false;
            ResetAllData();
        }
        #endregion

        #region Interface
        public abstract int FlagBits { get; }
        public abstract void ResetAllData();
        public abstract void ResetControllerData();

        public abstract void ApplyMutableFrom(RailState source, uint flags);
        public abstract void ApplyControllerFrom(RailState source);
        public abstract void ApplyImmutableFrom(RailState source);

        public abstract uint CompareMutableData(RailState basis);
        public abstract bool IsControllerDataEqual(RailState basis);
        #endregion

        #region Encode & Decode
        public abstract void DecodeMutableData(RailBitBuffer buffer, uint flags);
        public abstract void DecodeControllerData(RailBitBuffer buffer);
        public abstract void DecodeImmutableData(RailBitBuffer buffer);
        public abstract void EncodeMutableData(RailBitBuffer buffer, uint flags);
        public abstract void EncodeControllerData(RailBitBuffer buffer);
        public abstract void EncodeImmutableData(RailBitBuffer buffer);
        #endregion
    }
}
