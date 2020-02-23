using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Core
{
    public static class Extensions
    {
        public static void Invert(this ref byte value)
        {
            // 神奇算法
            var v1 = value * 0x0202020202UL;
            var v2 = v1 & 0x010884422010UL;
            var v3 = v2 % 1023;
            value = (byte)v3;
        }

        public static void Invert(this ref ushort value)
        {
            // 交换相邻的两个字节
            value = (ushort)((value >> 8) | (value << 8));
            // 交换每八位中的前四位和后四位
            value = (ushort)(((value & 0xF0F0) >> 4) | ((value & 0x0F0F) << 4));
            // 交换每四位中的前两位和后两位
            value = (ushort)(((value & 0xCCCC) >> 2) | ((value & 0x3333) << 2));
            // 交换每两位
            value = (ushort)(((value & 0xAAAA) >> 1) | ((value & 0x5555) << 1));
        }

        public static void Invert(this ref uint value)
        {
            // 交换前后两个双字节
            value = (value >> 16) | (value << 16);
            // 交换相邻的两个字节
            value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
            // 交换每八位中的前四位和后四位
            value = ((value & 0xF0F0F0F0) >> 4) | ((value & 0x0F0F0F0F) << 4);
            // 交换每四位中的前两位和后两位
            value = ((value & 0xCCCCCCCC) >> 2) | ((value & 0x33333333) << 2);
            // 交换每两位
            value = ((value & 0xAAAAAAAA) >> 1) | ((value & 0x55555555) << 1);
        }

        public static void Invert(this ref ulong value)
        {
            // 交换前后两个四字节
            value = (value >> 32) | (value << 32);
            // 交换前后两个双字节
            value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
            // 交换相邻的两个字节
            value = ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
            // 交换每八位中的前四位和后四位
            value = ((value & 0xF0F0F0F0F0F0F0F0) >> 4) | ((value & 0x0F0F0F0F0F0F0F0F) << 4);
            // 交换每四位中的前两位和后两位
            value = ((value & 0xCCCCCCCCCCCCCCCC) >> 2) | ((value & 0x3333333333333333) << 2);
            // 交换每两位
            value = ((value & 0xAAAAAAAAAAAAAAAA) >> 1) | ((value & 0x5555555555555555) << 1);
        }
    }
}
