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

using System;
using RailgunNet.Logic;

namespace RailgunNet.Connection.Traffic
{
    public delegate void RailNetPeerEvent(IRailNetPeer peer, ArraySegment<byte> buffer);

    public interface IRailNetPeer
    {
        /// <summary>
        ///     Used for attaching arbitrary player data.
        /// </summary>
        object PlayerData { get; set; }

        float? Ping { get; }
        event RailNetPeerEvent PayloadReceived;

        void SendPayload(ArraySegment<byte> buffer);
    }
}
