using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Security.Cryptography
{
    class CRC_32 : CRC
    {
        public override string Name => "CRC-32";

        public override int Width => 32;

        public override ulong Poly => 0x04C11DB7;

        public override ulong Init => 0xFFFFFFFF;

        public override bool RefIn => true;

        public override bool RefOut => true;

        public override ulong XorOut => 0xFFFFFFFF;
    }
}
