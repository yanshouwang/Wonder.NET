using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Security.Cryptography
{
    [TestClass]
    public class CRCTest
    {
        #region CRC 实例
        public CRC CRC4ITU { get; }
        public CRC CRC5EPC { get; }
        public CRC CRC5ITU { get; }
        public CRC CRC5USB { get; }
        public CRC CRC6ITU { get; }
        public CRC CRC7MMC { get; }
        public CRC CRC8 { get; }
        public CRC CRC8ITU { get; }
        public CRC CRC8ROHC { get; }
        public CRC CRC8MAXIM { get; }
        public CRC CRC16 { get; }
        public CRC CRC16MAXIM { get; }
        public CRC CRC16USB { get; }
        public CRC CRC16MODBUS { get; }
        public CRC CRC16CCITT { get; }
        public CRC CRC16CCITTFALSE { get; }
        public CRC CRC16X25 { get; }
        public CRC CRC16XMODEM { get; }
        public CRC CRC16DNP { get; }
        public CRC CRC32 { get; }
        public CRC CRC32MPEG2 { get; }
        #endregion

        public CRCTest()
        {
            CRC4ITU = CRC.Create(CRCModel.CRC_4_ITU);
            CRC5EPC = CRC.Create(CRCModel.CRC_5_EPC);
            CRC5ITU = CRC.Create(CRCModel.CRC_5_ITU);
            CRC5USB = CRC.Create(CRCModel.CRC_5_USB);
            CRC6ITU = CRC.Create(CRCModel.CRC_6_ITU);
            CRC7MMC = CRC.Create(CRCModel.CRC_7_MMC);
            CRC8 = CRC.Create(CRCModel.CRC_8);
            CRC8ITU = CRC.Create(CRCModel.CRC_8_ITU);
            CRC8ROHC = CRC.Create(CRCModel.CRC_8_ROHC);
            CRC8MAXIM = CRC.Create(CRCModel.CRC_8_MAXIM);
            CRC16 = CRC.Create(CRCModel.CRC_16);
            CRC16MAXIM = CRC.Create(CRCModel.CRC_16_MAXIM);
            CRC16USB = CRC.Create(CRCModel.CRC_16_USB);
            CRC16MODBUS = CRC.Create(CRCModel.CRC_16_MODBUS);
            CRC16CCITT = CRC.Create(CRCModel.CRC_16_CCITT);
            CRC16CCITTFALSE = CRC.Create(CRCModel.CRC_16_CCITT_FALSE);
            CRC16X25 = CRC.Create(CRCModel.CRC_16_X25);
            CRC16XMODEM = CRC.Create(CRCModel.CRC_16_XMODEM);
            CRC16DNP = CRC.Create(CRCModel.CRC_16_DNP);
            CRC32 = CRC.Create(CRCModel.CRC_32);
            CRC32MPEG2 = CRC.Create(CRCModel.CRC_32_MPEG_2);
        }

        [TestMethod]
        public void TestCalculate_CRC4ITU_123456789_Returns0x07()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x07U;
            var actual = CRC4ITU.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC5EPC_123456789_Returns0x00()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x00U;
            var actual = CRC5EPC.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC5ITU_123456789_Returns0x07()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x07U;
            var actual = CRC5ITU.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC5USB_123456789_Returns0x19()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x19U;
            var actual = CRC5USB.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC6ITU_123456789_Returns0x06()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x06U;
            var actual = CRC6ITU.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC7MMC_123456789_Returns0x75()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x75U;
            var actual = CRC7MMC.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC8_123456789_Returns0xF4()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            //var expected = new byte[data.Length + 1];
            //Array.Copy(data, 0, expected, 0, data.Length);
            //expected[data.Length] = 0x9F;
            var expected = 0xF4U;
            var actual = CRC8.Calculate(data);
            //CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC8ITU_123456789_Returns0xA1()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            //var expected = new byte[data.Length + 1];
            //Array.Copy(data, 0, expected, 0, data.Length);
            //expected[data.Length] = 0x9F;
            var expected = 0xA1U;
            var actual = CRC8ITU.Calculate(data);
            //CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC8ROHC_123456789_Returns0xD0()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            //var expected = new byte[data.Length + 1];
            //Array.Copy(data, 0, expected, 0, data.Length);
            //expected[data.Length] = 0x9F;
            var expected = 0xD0U;
            var actual = CRC8ROHC.Calculate(data);
            //CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC8MAXIM_123456789_Returns0xA1()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            //var expected = new byte[data.Length + 1];
            //Array.Copy(data, 0, expected, 0, data.Length);
            //expected[data.Length] = 0x9F;
            var expected = 0xA1U;
            var actual = CRC8MAXIM.Calculate(data);
            //CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16_123456789_Returns0xBB3D()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            //var expected = new byte[data.Length + 1];
            //Array.Copy(data, 0, expected, 0, data.Length);
            //expected[data.Length] = 0x9F;
            var expected = 0xBB3DU;
            var actual = CRC16.Calculate(data);
            //CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16MAXIM_123456789_Returns0x44C2()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x44C2U;
            var actual = CRC16MAXIM.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16USB_123456789_Returns0xB4C8()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0xB4C8U;
            var actual = CRC16USB.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16MODBUS_123456789_Returns0x4B37()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x4B37U;
            var actual = CRC16MODBUS.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16CCITT_123456789_Returns0x2189()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x2189U;
            var actual = CRC16CCITT.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16CCITTFALSE_123456789_Returns0x29B1()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x29B1U;
            var actual = CRC16CCITTFALSE.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16X25_123456789_Returns0x906E()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x906EU;
            var actual = CRC16X25.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16XMODEM_123456789_Returns0x31C3()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x31C3U;
            var actual = CRC16XMODEM.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC16DNP_123456789_Returns0xEA82()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0xEA82U;
            var actual = CRC16DNP.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC32_123456789_Returns0xCBF43926()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0xCBF43926U;
            var actual = CRC32.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC32MPEG2_123456789_Returns0x0376E6E7()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x0376E6E7U;
            var actual = CRC32MPEG2.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestVerity_CRC4ITU_True()
        {
            var expected = true;
            var list = new List<byte>();
            var data = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            var ccr = BitConverter.GetBytes(0x0BU);
            list.AddRange(data);
            list.AddRange(ccr);
            data = list.ToArray();
            var actual = CRC4ITU.Verify(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestVerity_CRC4ITU_False()
        {
            var expected = true;
            var list = new List<byte>();
            var data = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYA");
            var ccr = BitConverter.GetBytes(0x0BU);
            list.AddRange(data);
            list.AddRange(ccr);
            data = list.ToArray();
            var actual = CRC4ITU.Verify(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestVerity_CRC5EPC_True()
        {
            var expected = true;
            var list = new List<byte>();
            var data = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            var ccr = BitConverter.GetBytes(0x08U);
            list.AddRange(data);
            list.AddRange(ccr);
            data = list.ToArray();
            var actual = CRC5EPC.Verify(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestVerity_CRC32_True()
        {
            var expected = true;
            var list = new List<byte>();
            var data = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            var ccr = BitConverter.GetBytes(0xABF77822U);
            list.AddRange(data);
            list.AddRange(ccr);
            data = list.ToArray();
            var actual = CRC32.Verify(data);
            Assert.AreEqual(expected, actual);
        }
    }
}
