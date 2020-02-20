using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Security.Cryptography
{
    /// <summary>
    /// CRC-8, 多项式: x^8 + x^2 + 1
    /// </summary>
    internal class CRC_8 : CRC
    {
        public override string Name => "CRC-8";

        public override int Width => 8;

        public override ulong Poly => 0x07;

        public override ulong Init => 0x00;

        public override bool RefIn => false;

        public override bool RefOut => false;

        public override ulong XorOut => 0x00;
    }
}
