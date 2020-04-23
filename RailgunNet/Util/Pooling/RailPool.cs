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
  internal interface IRailPool<T>
  {
    T Allocate();
    void Deallocate(T obj);
    IRailPool<T> Clone();
  }

  internal static class RailPool
  {
    public static void Free<T>(T obj)
      where T : IRailPoolable<T>
    {
      obj.Pool.Deallocate(obj);
    }

    public static void SafeReplace<T>(ref T destination, T obj)
      where T : IRailPoolable<T>
    {
      if (destination != null)
        RailPool.Free(destination);
      destination = obj;
    }

    public static void DrainQueue<T>(Queue<T> queue)
      where T : IRailPoolable<T>
    {
      while (queue.Count > 0)
        RailPool.Free(queue.Dequeue());
    }
  }

  internal abstract class RailPoolBase<T> : IRailPool<T>
    where T : IRailPoolable<T>
  {
    private readonly Stack<T> freeList;
    protected readonly IRailFactory<T> factory;

    public abstract IRailPool<T> Clone();

    public RailPoolBase(IRailFactory<T> factory)
    {
        this.factory = factory;
        this.freeList = new Stack<T>();
    }

    public T Allocate()
    {
      T obj = this.freeList.Count > 0 ? this.freeList.Pop() : factory.Create();
      obj.Pool = this;
      obj.Reset();
      return obj;
    }

    public void Deallocate(T obj)
    {
      RailDebug.Assert(obj.Pool == this);

      obj.Reset();
      obj.Pool = null; // Prevent multiple frees
      this.freeList.Push(obj);
    }
  }

  internal class RailPool<T> : RailPoolBase<T>
    where T : IRailPoolable<T>
  {
      public RailPool(IRailFactory<T> factory) : base(factory)
      {
      }
    public override IRailPool<T> Clone()
    {
      return new RailPool<T>(factory);
    }
  }
}
