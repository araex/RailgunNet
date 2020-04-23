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
using System.Collections.Generic;

namespace Railgun
{
    public abstract class RailRoom
    {
#if SERVER
        /// <summary>
        /// Fired when a controller has been added (i.e. player join).
        /// The controller has control of no entities at this point.
        /// </summary>
        public event Action<RailController> ClientJoined;

        /// <summary>
        /// Fired when a controller has been removed (i.e. player leave).
        /// This event fires before the controller has control of its entities
        /// revoked (this is done immediately afterwards).
        /// </summary>
        public event Action<RailController> ClientLeft;
#endif

        /// <summary>
        /// Fired before all entities have updated, for updating global logic.
        /// </summary>
        public event Action<Tick> PreRoomUpdate;

        /// <summary>
        /// Fired after all entities have updated, for updating global logic.
        /// </summary>
        public event Action<Tick> PostRoomUpdate;

        /// <summary>
        /// Notifies that we removed an entity.
        /// </summary>
        public event Action<IRailEntity> EntityRemoved;

        public object UserData { get; set; }

        /// <summary>
        /// The current synchronized tick. On clients this will be the predicted
        /// server tick. On the server this will be the authoritative tick.
        /// </summary>
        public Tick Tick { get; protected set; }

        /// <summary>
        /// All of the entities currently added to this room.
        /// </summary>
        public Dictionary<EntityId, IRailEntity>.ValueCollection Entities
        {
            get { return this.entities.Values; }
        }

        protected List<RailEntity> toUpdate; // Pre-allocated update list
        protected List<RailEntity> toRemove; // Pre-allocated removal list
        protected virtual void HandleRemovedEntity(EntityId entityId) { }

        public readonly RailResource resource;
        private readonly RailConnection connection;
        private readonly Dictionary<EntityId, IRailEntity> entities;

        public bool TryGet(EntityId id, out IRailEntity value)
        {
            return this.entities.TryGetValue(id, out value);
        }

        public TEvent CreateEvent<TEvent>()
          where TEvent : RailEvent, new()
        {
            return RailEvent.Create<TEvent>(this.resource);
        }

#if CLIENT
        /// <summary>
        /// Raises an event to be sent to the server.
        /// Caller should call Free() on the event when done sending.
        /// </summary>
        public abstract void RaiseEvent(
          RailEvent evnt,
          ushort attempts = 3,
          bool freeWhenDone = true);
#endif

#if SERVER
        /// <summary>
        /// Queues an event to broadcast to all present clients.
        /// </summary>
        public abstract void BroadcastEvent(
          RailEvent evnt,
          ushort attempts = 3,
          bool freeWhenDone = true);

        public abstract T AddNewEntity<T>() where T : RailEntity;
        public abstract void MarkForRemoval(IRailEntity entity);
#endif

        public RailRoom(RailResource resource, RailConnection connection)
        {
            this.resource = resource;
            this.connection = connection;
            this.entities =
              new Dictionary<EntityId, IRailEntity>(
                EntityId.CreateEqualityComparer());
            this.Tick = Tick.INVALID;

            this.toUpdate = new List<RailEntity>();
            this.toRemove = new List<RailEntity>();
        }

        public void Initialize(Tick tick)
        {
            this.Tick = tick;
        }

        protected void OnPreRoomUpdate(Tick tick)
        {
            this.PreRoomUpdate?.Invoke(tick);
        }

        protected void OnPostRoomUpdate(Tick tick)
        {
            this.PostRoomUpdate?.Invoke(tick);
        }

        protected IEnumerable<RailEntity> GetAllEntities()
        {
            // TODO: This makes multiple full passes, could probably optimize
            foreach (RailConfig.RailUpdateOrder order in RailConfig.Orders)
                foreach (RailEntity entity in this.entities.Values)
                    if (entity.UpdateOrder == order)
                        yield return entity;
        }

        protected void RegisterEntity(RailEntity entity)
        {
            this.entities.Add(entity.Id, entity);
            entity.Room = this;
        }

        protected void RemoveEntity(RailEntity entity)
        {
            if (this.entities.ContainsKey(entity.Id))
            {
                this.entities.Remove(entity.Id);
                entity.Shutdown();
                entity.Room = null;
                // TODO: Pooling entities?

                this.HandleRemovedEntity(entity.Id);
                this.EntityRemoved?.Invoke(entity);
            }
        }

#if CLIENT
        public abstract void RequestControlUpdate(
          RailEntity entity,
          RailStateDelta delta);
#endif

#if SERVER
        protected void OnClientJoined(RailController client)
        {
            this.ClientJoined?.Invoke(client);
        }

        protected void OnClientLeft(RailController client)
        {
            this.ClientLeft?.Invoke(client);
        }
#endif
    }
}
