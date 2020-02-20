using System;
using System.Collections.Generic;

namespace Wonder.Security.Cryptography
{
    /// <summary>
    /// 循环冗余校验 (Cyclic Redundancy Check, CRC)
    /// </summary>
    public abstract class CRC
    {
        public abstract string Name { get; }
        public abstract int Width { get; }
        public abstract ulong Poly { get; }
        public abstract ulong Init { get; }
        public abstract bool RefIn { get; }
        public abstract bool RefOut { get; }
        public abstract ulong XorOut { get; }

        public Lazy<IDictionary<byte, ulong>> LazyMapping { get; }
        public ulong Mask { get; set; }

        protected CRC()
        {
            LazyMapping = new Lazy<IDictionary<byte, ulong>>(() => this.CreateMapping(), true);
            Mask = ulong.MaxValue >> (64 - Width);
        }

        private IDictionary<byte, ulong> CreateMapping()
        {
            var mapping = new Dictionary<byte, ulong>(256);
            for (var i = 0; i < 0xFF; i++)
            {
                var key = (byte)i;
                var value = MapValue(key);
                mapping.Add(key, value);
            }
            return mapping;
        }

        private ulong MapValue(byte key)
        {
            // 1) 将 Mx^r 的前 r 位放入一个长度为 r 的寄存器
            // 2) 如果寄存器的首位为 1，将寄存器左移 1 位(将 Mx^r 剩下部分的 MSB 移入寄存器的 LSB)，再与 G 的后 r 位异或，否则仅将寄存器左移 1 位(将 Mx^r 剩下部分的 MSB 移入寄存器的 LSB)
            // 3) 重复第 2 步，直到M全部 Mx^r 移入寄存器
            // 4) 寄存器中的值则为校验码
            var value = (ulong)key;
            value <<= Width - 8;
            for (int i = 0; i < 8; i++)
            {
                var expected = 1UL << Width - 1;
                var actual = value & expected;
                if (actual == expected)
                {
                    value = (value << 1) ^ Poly;
                }
                else
                {
                    value <<= 1;
                }
            }
            return value & Mask;
        }

        public static CRC Create(CRCModel model)
        {
            return model switch
            {
                CRCModel.CRC_8 => new CRC_8(),
                CRCModel.CRC_16_CCITT => new CRC_16_CCITT(),
                CRCModel.CRC_32 => new CRC_32(),
                _ => throw new ArgumentException("不支持的计算模型", nameof(model)),
            };
        }

        public ulong Calculate(byte[] data)
        {
            var crc = Init;
            var mapping = LazyMapping.Value;
            for (int i = 0; i < data.Length; i++)
            {
                var byteValue = data[i];
                if (RefIn)
                {
                    // 反转每个字节
                    byteValue = byteValue.Invert();
                }
                var key = (byte)((crc >> (Width - 8)) ^ byteValue);
                var value = mapping[key];
                crc = (crc << 8) ^ value;
            }
            if (RefOut)
            {
                // 反转校验码
                crc = crc.Invert();
                crc >>= 64 - Width;
            }
            crc ^= XorOut;
            crc &= Mask;
            return crc;
        }

        public bool Verify(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
