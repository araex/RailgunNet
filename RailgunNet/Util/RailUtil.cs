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

namespace Railgun
{
    public static class RailUtil
    {
        // http://stackoverflow.com/questions/15967240/fastest-implementation-of-log2int-and-log2float
        private static readonly int[] DeBruijnLookup = new int[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        public static bool ValuesEqual<T>(T[] a, T[] b)
            where T : struct
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            for (int i = 0; i < a.Length; i++)
                if (a[i].Equals(b[i]) == false)
                    return false;
            return true;
        }

        public static int Log2(uint v)
        {
            v |= v >> 1; // Round down to one less than a power of 2 
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            return DeBruijnLookup[(v * 0x07C4ACDDU) >> 27];
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = b;
            b = a;
            a = temp;
        }

        public static bool GetFlag(byte field, byte flag)
        {
            return (field & flag) > 0;
        }

        public static byte SetFlag(byte field, byte flag, bool value)
        {
            if (value)
                return (byte) (field | flag);
            return (byte) (field & ~flag);
        }

        public static int Abs(int a)
        {
            if (a < 0)
                return -a;
            return a;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max) value = max;
            return value;
        }
    }
}