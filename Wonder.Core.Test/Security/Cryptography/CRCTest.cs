using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wonder.Core.Security.Cryptography;

namespace Wonder.Core.Test.Security.Cryptography
{
    [TestClass]
    public class CRCTest
    {
        [TestMethod]
        public void TestCalculate_CRC4ITU_123456789_Returns0x07()
        {
            TestCalculate(CRCModel.CRC_4_ITU, false, "123456789", 0x07);
        }

        [TestMethod]
        public void TestCalculate_CRC5EPC_123456789_Returns0x00()
        {
            TestCalculate(CRCModel.CRC_5_EPC, false, "123456789", 0x00);
        }

        [TestMethod]
        public void TestCalculate_CRC5ITU_123456789_Returns0x07()
        {
            TestCalculate(CRCModel.CRC_5_ITU, false, "123456789", 0x07);
        }

        [TestMethod]
        public void TestCalculate_CRC5USB_123456789_Returns0x19()
        {
            TestCalculate(CRCModel.CRC_5_USB, false, "123456789", 0x19);
        }

        [TestMethod]
        public void TestCalculate_CRC6ITU_123456789_Returns0x06()
        {
            TestCalculate(CRCModel.CRC_6_ITU, false, "123456789", 0x06);
        }

        [TestMethod]
        public void TestCalculate_CRC7MMC_123456789_Returns0x75()
        {
            TestCalculate(CRCModel.CRC_7_MMC, false, "123456789", 0x75);
        }

        [TestMethod]
        public void TestCalculate_CRC8_123456789_Returns0xF4()
        {
            TestCalculate(CRCModel.CRC_8, false, "123456789", 0xF4);
        }

        [TestMethod]
        public void TestCalculate_CRC8ITU_123456789_Returns0xA1()
        {
            TestCalculate(CRCModel.CRC_8_ITU, false, "123456789", 0xA1);
        }

        [TestMethod]
        public void TestCalculate_CRC8ROHC_123456789_Returns0xD0()
        {
            TestCalculate(CRCModel.CRC_8_ROHC, false, "123456789", 0xD0);
        }

        [TestMethod]
        public void TestCalculate_CRC8MAXIM_123456789_Returns0xA1()
        {
            TestCalculate(CRCModel.CRC_8_MAXIM, false, "123456789", 0xA1);
        }

        [TestMethod]
        public void TestCalculate_CRC16_123456789_Returns0xBB3D()
        {
            TestCalculate(CRCModel.CRC_16, false, "123456789", 0xBB3D);
        }

        [TestMethod]
        public void TestCalculate_CRC16MAXIM_123456789_Returns0x44C2()
        {
            TestCalculate(CRCModel.CRC_16_MAXIM, false, "123456789", 0x44C2);
        }

        [TestMethod]
        public void TestCalculate_CRC16USB_123456789_Returns0xB4C8()
        {
            TestCalculate(CRCModel.CRC_16_USB, false, "123456789", 0xB4C8);
        }

        [TestMethod]
        public void TestCalculate_CRC16MODBUS_123456789_Returns0x4B37()
        {
            TestCalculate(CRCModel.CRC_16_MODBUS, false, "123456789", 0x4B37);
        }

        [TestMethod]
        public void TestCalculate_CRC16CCITT_123456789_Returns0x2189()
        {
            TestCalculate(CRCModel.CRC_16_CCITT, false, "123456789", 0x2189);
        }

        [TestMethod]
        public void TestCalculate_CRC16CCITTFALSE_123456789_Returns0x29B1()
        {
            TestCalculate(CRCModel.CRC_16_CCITT_FALSE, false, "123456789", 0x29B1);
        }

        [TestMethod]
        public void TestCalculate_CRC16X25_123456789_Returns0x906E()
        {
            TestCalculate(CRCModel.CRC_16_X25, false, "123456789", 0x906E);
        }

        [TestMethod]
        public void TestCalculate_CRC16XMODEM_123456789_Returns0x31C3()
        {
            TestCalculate(CRCModel.CRC_16_XMODEM, false, "123456789", 0x31C3);
        }

        [TestMethod]
        public void TestCalculate_CRC16DNP_123456789_Returns0xEA82()
        {
            TestCalculate(CRCModel.CRC_16_DNP, false, "123456789", 0xEA82);
        }

        [TestMethod]
        public void TestCalculate_CRC32_123456789_Returns0xCBF43926()
        {
            TestCalculate(CRCModel.CRC_32, false, "123456789", 0xCBF43926);
        }

        [TestMethod]
        public void TestCalculate_CRC32MPEG2_123456789_Returns0x0376E6E7()
        {
            TestCalculate(CRCModel.CRC_32_MPEG_2, false, "123456789", 0x0376E6E7);
        }

        [TestMethod]
        public void TestVerity_CRC4ITU_True()
        {
            TestVerify(CRCModel.CRC_4_ITU, false, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 0x0B, true);
        }

        [TestMethod]
        public void TestVerity_CRC4ITU_False()
        {
            TestVerify(CRCModel.CRC_4_ITU, false, "ABCDEFGHIJKLMNOPQRSTUVWXYA", 0x0B, false);
        }

        private void TestCalculate(CRCModel model, bool mapping, string str, uint expected)
        {
            var crc = CRC.Create(model, mapping);
            var data = Encoding.ASCII.GetBytes(str);
            var actual = crc.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        private void TestVerify(CRCModel model, bool mapping, string str, uint crc, bool expected)
        {
            var data = Encoding.ASCII.GetBytes(str);
            var obj = CRC.Create(model, mapping);
            var actual = obj.Verify(data, crc);
            Assert.AreEqual(expected, actual);
        }
    }
}
