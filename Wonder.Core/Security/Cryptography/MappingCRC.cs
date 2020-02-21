using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Security.Cryptography
{
    /// <summary>
    /// 单字节查表法 CRC
    /// </summary>
    class MappingCRC : CRC
    {
        private readonly Lazy<IDictionary<byte, uint>> mLazyCRCs;

        public MappingCRC(string name, int width, uint poly, uint init, bool refIn, bool refOut, uint xorOut)
            : base(name, width, poly, init, refIn, refOut, xorOut)
        {
            mLazyCRCs = new Lazy<IDictionary<byte, uint>>(() => CreateCRCs(), true);
        }

        private IDictionary<byte, uint> CreateCRCs()
        {
            var mapping = new Dictionary<byte, uint>(256);
            for (var i = 0; i <= 0xFF; i++)
            {
                var key = (byte)i;
                var value = Calculate(key);
                mapping.Add(key, value);
            }
            return mapping;
        }

        private uint Calculate(byte value)
        {
            // 1) 将 Mx^r 的前 r 位放入一个长度为 r 的寄存器
            // 2) 如果寄存器的首位为 1，将寄存器左移 1 位(将 Mx^r 剩下部分的 MSB 移入寄存器的 LSB)，再与 G 的后 r 位异或，否则仅将寄存器左移 1 位(将 Mx^r 剩下部分的 MSB 移入寄存器的 LSB)
            // 3) 重复第 2 步，直到 M 全部 Mx^r 移入寄存器
            // 4) 寄存器中的值则为校验码
            var crc = Convert.ToUInt32(value) << Math.Max(Width - 8, 0);
            var expected = 0x80U << Math.Max(Width - 8, 0);
            var poly = Poly << Math.Max(8 - Width, 0);
            for (int i = 0; i < 8; i++)
            {
                var actual = crc & expected;
                if (actual == expected)
                {
                    crc = (crc << 1) ^ poly;
                }
                else
                {
                    crc <<= 1;
                }
            }
            return crc;
        }

        public override uint Calculate(byte[] data)
        {
            var crcs = mLazyCRCs.Value;
            var crc = Init << Math.Max(8 - Width, 0);
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if (RefIn)
                {
                    // 反转每个字节
                    item.Invert();
                }
                // 字节算法: 本字节的 CRC, 等于上一字节的 CRC 左移八位, 与上一字节的 CRC 高八位同本字节异或后对应 CRC 的异或值
                var key = (byte)((crc >> Math.Max(Width - 8, 0)) ^ item);
                var value = crcs[key];
                crc = (crc << 8) ^ value;
            }
            crc >>= Math.Max(8 - Width, 0);
            if (RefOut)
            {
                // 反转校验码
                crc.Invert();
                crc >>= 64 - Width;
            }
            crc ^= XorOut;
            var mask = uint.MaxValue >> 32 - Width;
            crc &= mask;
            return crc;
        }

        public override bool Verify(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
