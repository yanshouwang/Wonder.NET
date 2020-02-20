using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Security.Cryptography
{
    class CRC_16_CCITT : CRC
    {
        public override string Name => "CRC-16/CCITT";

        public override int Width => 16;

        public override ulong Poly => 0x1021;

        public override ulong Init => 0x0000;

        public override bool RefIn => true;

        public override bool RefOut => true;

        public override ulong XorOut => 0x0000;
    }
}
