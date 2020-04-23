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

namespace Railgun
{
  public class RailConfig
  {
    /// <summary>
    /// Use this to control which entities update relative to one another.
    /// </summary>
    public enum RailUpdateOrder
    {
      Early,
      Normal,
      Late,
      VeryLate,
    }

    // Pre-cache the array for iterating over.
    internal static readonly RailUpdateOrder[] Orders = new[]
    {
      RailUpdateOrder.Early,
      RailUpdateOrder.Normal,
      RailUpdateOrder.Late,
      RailUpdateOrder.VeryLate,
    };

    public enum RailApplication
    {
        Client,
        Server
    }

    /// <summary>
    /// Network send rate in ticks/packet.
    /// </summary>
    public const int SERVER_SEND_RATE = 2;

    /// <summary>
    /// Network send rate in ticks/packet.
    /// </summary>
    public const int CLIENT_SEND_RATE = 2;

    /// <summary>
    /// Number of outgoing commands to send per packet.
    /// </summary>
    internal const int COMMAND_SEND_COUNT = 40;

    /// <summary>
    /// Number of commands to buffer for prediction.
    /// </summary>
    internal const int COMMAND_BUFFER_COUNT = 40;

    /// <summary>
    /// Number of entries to store in a dejitter buffer.
    /// </summary>
    internal const int DEJITTER_BUFFER_LENGTH = 50;

    /// <summary>
    /// Number of ticks we'll resend a view entry for without receiving
    /// an update on that entity.
    /// </summary>
    internal const int VIEW_TICKS = 100;

    /// <summary>
    /// How many chunks to keep in the history bit array. The resulting
    /// max history length will be EVENT_HISTORY_CHUNKS * 32.
    /// </summary>
    internal const int HISTORY_CHUNKS = 6;

    #region Message Sizes
    /// <summary>
    /// Data buffer size used for packet I/O. 
    /// Don't change this without a good reason.
    /// </summary>
    internal const int DATA_BUFFER_SIZE = 2048;

    /// <summary>
    /// The maximum message size that a packet can contain, based on known
    /// MTUs for internet traffic. Don't change this without a good reason.
    /// 
    /// If using MiniUDP, this should be equal to NetConfig.DATA_MAXIMUM
    /// </summary>
    internal const int PACKCAP_MESSAGE_TOTAL = 1200;

    /// <summary>
    /// The max byte size when doing a first pass on packing events.
    /// </summary>
    internal const int PACKCAP_EARLY_EVENTS = 370;

    /// <summary>
    /// The max byte size when packing commands. (Client-only.)
    /// </summary>
    internal const int PACKCAP_COMMANDS = 670;

    /// <summary>
    /// Maximum bytes for a single entity. Used when packing entity deltas.
    /// </summary>
    internal const int MAXSIZE_ENTITY = 100;

    /// <summary>
    /// Maximum bytes for a single event. 
    /// </summary>
    internal const int MAXSIZE_EVENT = 100;

    /// <summary>
    /// Maximum bytes for a single command update.
    /// </summary>
    internal const int MAXSIZE_COMMANDUPDATE = 335;

    /// <summary>
    /// Number of bits before doing VarInt fallback in compression.
    /// </summary>
    internal const int VARINT_FALLBACK_SIZE = 10;

    /// <summary>
    /// Maximum size for an encoded string.
    /// </summary>
    internal const int STRING_LENGTH_MAX = 63;
    #endregion
  }
}
