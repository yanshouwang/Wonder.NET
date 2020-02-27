using System;

namespace Wonder.Core.Security.Cryptography
{
    class SimpleCRC : CRC
    {
        public SimpleCRC(string name, int width, uint poly, uint init, bool refIn, bool refOut, uint xorOut)
            : base(name, width, poly, init, refIn, refOut, xorOut)
        {

        }

        public override uint Calculate(byte[] data)
        {
            var crc = Init << Math.Max(8 - Width, 0);
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if (RefIn)
                {
                    // 反转每个字节
                    item.Invert();
                }
                crc ^= Convert.ToUInt32(item) << Math.Max(Width - 8, 0);
                var expected = 0x80U << Math.Max(Width - 8, 0);
                var poly = Poly << Math.Max(8 - Width, 0);
                for (int j = 0; j < 8; j++)
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
            }
            crc >>= Math.Max(8 - Width, 0);
            if (RefOut)
            {
                // 反转校验码
                crc.Invert();
                crc >>= 32 - Width;
            }
            crc ^= XorOut;
            var mask = uint.MaxValue >> 32 - Width;
            crc &= mask;
            return crc;
        }
    }
}
