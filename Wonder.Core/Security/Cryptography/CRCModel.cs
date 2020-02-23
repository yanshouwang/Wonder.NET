using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Core.Security.Cryptography
{
    /// <summary>
    /// CRC 校验模型
    /// </summary>
#pragma warning disable CA1027 // Mark enums with FlagsAttribute
    public enum CRCModel
#pragma warning restore CA1027 // Mark enums with FlagsAttribute
    {
        /// <summary>
        /// CRC-4/ITU
        /// 多项式: 0x03 (x4 + x + 1)
        /// </summary>
        CRC_4_ITU,
        /// <summary>
        /// CRC-5/EPC
        /// 多项式: 0x09 (x5 + x3 + 1)
        /// </summary>
        CRC_5_EPC,
        /// <summary>
        /// CRC-5/ITU
        /// 多项式: 0x15 (x5 + x4 + x2 + 1)
        /// </summary>
        CRC_5_ITU,
        /// <summary>
        /// CRC-5/USB
        /// 多项式: 0x05 (x5 + x2 + 1)
        /// </summary>
        CRC_5_USB,
        /// <summary>
        /// CRC-6/ITU
        /// 多项式: 0x03 (x6 + x + 1)
        /// </summary>
        CRC_6_ITU,
        /// <summary>
        /// CRC-7/MMC
        /// 多项式: 0x09 (x7 + x3 + 1)
        /// </summary>
        CRC_7_MMC,
        /// <summary>
        /// CRC-8
        /// 多项式: 0x07 (x8 + x2 + x + 1)
        /// </summary>
        CRC_8,
        /// <summary>
        /// CRC-8/ITU
        /// 多项式: 0x07 (x8 + x2 + x + 1)
        /// </summary>
        CRC_8_ITU,
        /// <summary>
        /// CRC-8/ROHC
        /// 多项式: 0x07 (x8 + x2 + x + 1)
        /// </summary>
        CRC_8_ROHC,
        /// <summary>
        /// CRC-8/MAXIM
        /// 多项式: 0x31 (x8 + x5 + x4 + 1)
        /// </summary>
        CRC_8_MAXIM,
        /// <summary>
        /// DOW-CRC
        /// </summary>
        DOW_CRC = CRC_8_MAXIM,
        /// <summary>
        /// CRC-16
        /// 多项式: 0x8005 (x16 + x15 + x2 + 1)
        /// </summary>
        CRC_16,
        /// <summary>
        /// CRC-16/IBM
        /// </summary>
        CRC_16_IBM = CRC_16,
        /// <summary>
        /// CRC-16/ARC
        /// </summary>
        CRC_16_ARC = CRC_16,
        /// <summary>
        /// CRC-16/LHA
        /// </summary>
        CRC_16_LHA = CRC_16,
        /// <summary>
        /// CRC-16/MAXIM
        /// 多项式: 0x8005 (x16 + x15 + x2 + 1)
        /// </summary>
        CRC_16_MAXIM,
        /// <summary>
        /// CRC-16/USB
        /// 多项式: 0x8005 (x16 + x15 + x2 + 1)
        /// </summary>
        CRC_16_USB,
        /// <summary>
        /// CRC-16/MODBUS
        /// 多项式: 0x8005 (x16 + x15 + x2 + 1)
        /// </summary>
        CRC_16_MODBUS,
        /// <summary>
        /// CRC-16/CCITT
        /// 多项式: 0x1021 (x16 + x12 + x5 + 1)
        /// </summary>
        CRC_16_CCITT,
        /// <summary>
        /// CRC-CCITT
        /// </summary>
        CRC_CCITT = CRC_16_CCITT,
        /// <summary>
        /// CRC-16/CCITT-TRUE
        /// </summary>
        CRC_16_CCITT_TRUE = CRC_16_CCITT,
        /// <summary>
        /// CRC-16/KERMIT
        /// </summary>
        CRC_16_KERMIT = CRC_16_CCITT,
        /// <summary>
        /// CRC-16/CCITT-FALSE
        /// 多项式: 0x1021 (x16 + x12 + x5 + 1)
        /// </summary>
        CRC_16_CCITT_FALSE,
        /// <summary>
        /// CRC-16/X25
        /// 多项式: 0x1021 (x16 + x12 + x5 + 1)
        /// </summary>
        CRC_16_X25,
        /// <summary>
        /// CRC-16/XMODEM
        /// 多项式: 0x1021 (x16 + x12 + x5 + 1)
        /// </summary>
        CRC_16_XMODEM,
        /// <summary>
        /// CRC-16/ZMODEM
        /// </summary>
        CRC_16_ZMODEM = CRC_16_XMODEM,
        /// <summary>
        /// CRC-16/ACORN
        /// </summary>
        CRC_16_ACORN = CRC_16_XMODEM,
        /// <summary>
        /// CRC-16/DNP
        /// 多项式: 0x3D65 (x16 + x13 + x12 + x11 + x10 + x8 + x6 + x5 + x2 + 1)
        /// </summary>
        CRC_16_DNP,
        /// <summary>
        /// CRC-32
        /// 多项式: 0x04C11DB7 (x32 + x26 + x23 + x22 + x16 + x12 + x11 + x10 + x8 + x7 + x5 + x4 + x2 + x1 + 1)
        /// </summary>
        CRC_32,
        /// <summary>
        /// CRC-32/ADCCP
        /// </summary>
        CRC_32_ADCCP = CRC_32,
        /// <summary>
        /// CRC-32/MPEG-2
        /// 多项式: 0x04C11DB7 (x32 + x26 + x23 + x22 + x16 + x12 + x11 + x10 + x8 + x7 + x5 + x4 + x2 + x1 + 1)
        /// </summary>
        CRC_32_MPEG_2
    }
}
