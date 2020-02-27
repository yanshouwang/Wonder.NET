using System;

namespace Wonder.Core.Security.Cryptography
{
    /// <summary>
    /// 循环冗余校验 (Cyclic Redundancy Check, CRC)
    /// </summary>
    public abstract class CRC
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// 多项式
        /// </summary>
        public uint Poly { get; }
        /// <summary>
        /// 初始值
        /// </summary>
        public uint Init { get; }
        /// <summary>
        /// 输入反转
        /// </summary>
        public bool RefIn { get; }
        /// <summary>
        /// 输出反转
        /// </summary>
        public bool RefOut { get; }
        /// <summary>
        /// 输出异或值
        /// </summary>
        public uint XorOut { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="width">宽度</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="refIn">输入反转</param>
        /// <param name="refOut">输出反转</param>
        /// <param name="xorOut">输出异或值</param>
        protected CRC(string name, int width, uint poly, uint init, bool refIn, bool refOut, uint xorOut)
        {
            if (width > 32)
            {
                throw new ArgumentException("CRC 宽度超出范围", nameof(width));
            }

            Name = name;
            Width = width;
            Poly = poly;
            Init = init;
            RefIn = refIn;
            RefOut = refOut;
            XorOut = xorOut;
        }

        /// <summary>
        /// 创建指定校验模式的 CRC 实例
        /// </summary>
        /// <param name="model">校验模式</param>
        /// <param name="mapping">是否查表</param>
        /// <returns>指定校验模式的 CRC 实例</returns>
        public static CRC Create(CRCModel model, bool mapping = true)
        {
            switch (model)
            {
                case CRCModel.CRC_4_ITU:
                    return Create("CRC-4/ITU", 4, 0x03, 0x00, true, true, 0x00, mapping);
                case CRCModel.CRC_5_EPC:
                    return Create("CRC-5/EPC", 5, 0x09, 0x09, false, false, 0x00, mapping);
                case CRCModel.CRC_5_ITU:
                    return Create("CRC-5/ITU", 5, 0x15, 0x00, true, true, 0x00, mapping);
                case CRCModel.CRC_5_USB:
                    return Create("CRC-5/USB", 5, 0x05, 0x1F, true, true, 0x1F, mapping);
                case CRCModel.CRC_6_ITU:
                    return Create("CRC-6/ITU", 6, 0x03, 0x00, true, true, 0x00, mapping);
                case CRCModel.CRC_7_MMC:
                    return Create("CRC-7/MMC", 7, 0x09, 0x00, false, false, 0x00, mapping);
                case CRCModel.CRC_8:
                    return Create("CRC-8", 8, 0x07, 0x00, false, false, 0x00, mapping);
                case CRCModel.CRC_8_ITU:
                    return Create("CRC-8/ITU", 8, 0x07, 0x00, false, false, 0x55, mapping);
                case CRCModel.CRC_8_MAXIM:
                    return Create("CRC-8/MAXIM", 8, 0x31, 0x00, true, true, 0x00, mapping);
                case CRCModel.CRC_8_ROHC:
                    return Create("CRC-8/ROHC", 8, 0x07, 0xFF, true, true, 0x00, mapping);
                case CRCModel.CRC_16:
                    return Create("CRC-16", 16, 0x8005, 0x0000, true, true, 0x0000, mapping);
                case CRCModel.CRC_16_CCITT:
                    return Create("CRC-16/CCITT", 16, 0x1021, 0x0000, true, true, 0x0000, mapping);
                case CRCModel.CRC_16_CCITT_FALSE:
                    return Create("CRC-16/CCITT-FALSE", 16, 0x1021, 0xFFFF, false, false, 0x0000, mapping);
                case CRCModel.CRC_16_DNP:
                    return Create("CRC-16/DNP", 16, 0x3D65, 0x0000, true, true, 0xFFFF, mapping);
                case CRCModel.CRC_16_MAXIM:
                    return Create("CRC-16/MAXIM", 16, 0x8005, 0x0000, true, true, 0xFFFF, mapping);
                case CRCModel.CRC_16_MODBUS:
                    return Create("CRC-16/MODBUS", 16, 0x8005, 0xFFFF, true, true, 0x0000, mapping);
                case CRCModel.CRC_16_USB:
                    return Create("CRC-16/USB", 16, 0x8005, 0xFFFF, true, true, 0xFFFF, mapping);
                case CRCModel.CRC_16_X25:
                    return Create("CRC-16/X25", 16, 0x1021, 0xFFFF, true, true, 0xFFFF, mapping);
                case CRCModel.CRC_16_XMODEM:
                    return Create("CRC-16/XMODEM", 16, 0x1021, 0x0000, false, false, 0x0000, mapping);
                case CRCModel.CRC_32:
                    return Create("CRC-32", 32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0xFFFFFFFF, mapping);
                case CRCModel.CRC_32_MPEG_2:
                    return Create("CRC-32/MPEG-2", 32, 0x04C11DB7, 0xFFFFFFFF, false, false, 0x00000000, mapping);
                default:
                    throw new ArgumentException("未定义的校验模型", nameof(model));
            }
        }

        /// <summary>
        /// 创建自定义 CRC 实例
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="width">宽度 (0 ~ 32)</param>
        /// <param name="poly">多项式</param>
        /// <param name="init">初始值</param>
        /// <param name="refIn">输入反转</param>
        /// <param name="refOut">输出反转</param>
        /// <param name="xorOut">输出异或值</param>
        /// <param name="mapping">是否查表</param>
        /// <returns>自定义 CRC 实例</returns>
        public static CRC Create(string name, int width, uint poly, uint init, bool refIn, bool refOut, uint xorOut, bool mapping = true)
        {
            if (mapping)
            {
                return new MappingCRC(name, width, poly, init, refIn, refOut, xorOut);
            }
            else
            {
                return new SimpleCRC(name, width, poly, init, refIn, refOut, xorOut);
            }
        }

        /// <summary>
        /// 校验数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="crc">校验码</param>
        /// <returns>校验结果</returns>
        public bool Verify(byte[] data, uint crc)
        {
            var expected = Calculate(data);
            return crc == expected;
        }

        /// <summary>
        /// 计算数据的校验码
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>校验码</returns>
        public abstract uint Calculate(byte[] data);
    }
}
