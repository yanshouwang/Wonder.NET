using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wonder.Security.Cryptography
{
    [TestClass]
    public class CRCTest
    {
        public CRC CRC8 { get; }
        public CRC CRC16 { get; }
        public CRC CRC32 { get; }

        public CRCTest()
        {
            CRC8 = CRC.Create(CRCModel.CRC_8);
            CRC16 = CRC.Create(CRCModel.CRC_16_CCITT);
            CRC32 = CRC.Create(CRCModel.CRC_32);
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
        public void TestCalculate_CRC16_123456789_Returns0xF4()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0x2189U;
            var actual = CRC16.Calculate(data);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCalculate_CRC32_123456789_Returns0xF4()
        {
            var data = Encoding.ASCII.GetBytes("123456789");
            var expected = 0xCBF43926U;
            var actual = CRC32.Calculate(data);
            Assert.AreEqual(expected, actual);
        }
    }
}
