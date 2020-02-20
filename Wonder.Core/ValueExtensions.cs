using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder
{
    public static class ValueExtensions
    {
        public static byte Invert(this byte value)
        {
            var v1 = value * 0x0202020202UL;
            var v2 = v1 & 0x010884422010UL;
            var v3 = v2 % 1023;
            return (byte)v3;
        }

        public static ushort Invert(this ushort value)
        {
            // 交换每两位
            value = (ushort)(((value >> 1) & 0x5555) | ((value & 0x5555) << 1));
            // 交换每四位中的前两位和后两位
            value = (ushort)(((value >> 2) & 0x3333) | ((value & 0x3333) << 2));
            // 交换每八位中的前四位和后四位
            value = (ushort)(((value >> 4) & 0x0F0F) | ((value & 0x0F0F) << 4));
            // 交换相邻的两个字节
            value = (ushort)((value >> 8) | (value << 8));
            return value;
        }

        public static uint Invert(this uint value)
        {
            // 交换每两位
            value = ((value >> 1) & 0x55555555) | ((value & 0x55555555) << 1);
            // 交换每四位中的前两位和后两位
            value = ((value >> 2) & 0x33333333) | ((value & 0x33333333) << 2);
            // 交换每八位中的前四位和后四位
            value = ((value >> 4) & 0x0F0F0F0F) | ((value & 0x0F0F0F0F) << 4);
            // 交换相邻的两个字节
            value = ((value >> 8) & 0x00FF00FF) | ((value & 0x00FF00FF) << 8);
            // 交换前后两个双字节
            value = (value >> 16) | (value << 16);
            return value;
        }

        public static ulong Invert(this ulong value)
        {
            // 交换每两位
            value = ((value >> 1) & 0x5555555555555555) | ((value & 0x5555555555555555) << 1);
            // 交换每四位中的前两位和后两位
            value = ((value >> 2) & 0x3333333333333333) | ((value & 0x3333333333333333) << 2);
            // 交换每八位中的前四位和后四位
            value = ((value >> 4) & 0x0F0F0F0F0F0F0F0F) | ((value & 0x0F0F0F0F0F0F0F0F) << 4);
            // 交换相邻的两个字节
            value = ((value >> 8) & 0x00FF00FF00FF00FF) | ((value & 0x00FF00FF00FF00FF) << 8);
            // 交换前后两个双字节
            value = ((value >> 16) & 0x0000FFFF0000FFFF) | ((value & 0x0000FFFF0000FFFF) << 16);
            // 交换前后两个四字节
            value = (value >> 32) | (value << 32);
            return value;
        }
    }
}
