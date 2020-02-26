using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wonder.Core.Security.Cryptography;

namespace Wonder.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region 字段
        private CRCModel mValue = (CRCModel)(-1);
        #endregion

        #region 属性
        private string mKey;
        public string Key
        {
            get { return mKey; }
            set
            {
                if (!SetProperty(ref mKey, value))
                    return;

                UpdateCRC();
            }
        }

        private void UpdateCRC()
        {
            var value = Models[Key];
            if (value == mValue)
                return;
            CRC = CRC.Create(value);
            mValue = value;
        }

        private CRC mCRC;
        public CRC CRC
        {
            get { return mCRC; }
            set { SetProperty(ref mCRC, value); }
        }

        private string mCRCStr;
        public string CRCStr
        {
            get { return mCRCStr; }
            set { SetProperty(ref mCRCStr, value); }
        }

        public IDictionary<string, CRCModel> Models { get; }
        #endregion

        public MainViewModel()
        {
            Models = CreateModels();
            Key = Models.Keys.First();
        }

        private IDictionary<string, CRCModel> CreateModels()
        {
            return new Dictionary<string, CRCModel>()
            {
                ["CRC-4/ITU"] = CRCModel.CRC_4_ITU,
                ["CRC-5/EPC"] = CRCModel.CRC_5_EPC,
                ["CRC-5/ITU"] = CRCModel.CRC_5_ITU,
                ["CRC-5/USB"] = CRCModel.CRC_5_USB,
                ["CRC-6/ITU"] = CRCModel.CRC_6_ITU,
                ["CRC-7/MMC"] = CRCModel.CRC_7_MMC,
                ["CRC-8"] = CRCModel.CRC_8,
                ["CRC-8/ITU"] = CRCModel.CRC_8_ITU,
                ["CRC-8/ROHC"] = CRCModel.CRC_8_ROHC,
                ["CRC-8/MAXIM"] = CRCModel.CRC_8_MAXIM,
                ["DOW-CRC"] = CRCModel.DOW_CRC,
                ["CRC-16"] = CRCModel.CRC_16,
                ["CRC-16/IBM"] = CRCModel.CRC_16_IBM,
                ["CRC-16/ARC"] = CRCModel.CRC_16_ARC,
                ["CRC-16/LHA"] = CRCModel.CRC_16_LHA,
                ["CRC-16/MAXIM"] = CRCModel.CRC_16_MAXIM,
                ["CRC-16/USB"] = CRCModel.CRC_16_USB,
                ["CRC-16/MODBUS"] = CRCModel.CRC_16_MODBUS,
                ["CRC-16/CCITT"] = CRCModel.CRC_16_CCITT,
                ["CRC-CCITT"] = CRCModel.CRC_CCITT,
                ["CRC-16/CCITT-TRUE"] = CRCModel.CRC_16_CCITT_TRUE,
                ["CRC-16/KERMIT"] = CRCModel.CRC_16_KERMIT,
                ["CRC-16/CCITT-FALSE"] = CRCModel.CRC_16_CCITT_FALSE,
                ["CRC-16/X25"] = CRCModel.CRC_16_X25,
                ["CRC-16/XMODEM"] = CRCModel.CRC_16_XMODEM,
                ["CRC-16/ZMODEM"] = CRCModel.CRC_16_ZMODEM,
                ["CRC-16/ACORN"] = CRCModel.CRC_16_ACORN,
                ["CRC-16/DNP"] = CRCModel.CRC_16_DNP,
                ["CRC-32"] = CRCModel.CRC_32,
                ["CRC-32/ADCCP"] = CRCModel.CRC_32_ADCCP,
                ["CRC-32/MPEG-2"] = CRCModel.CRC_32_MPEG_2,
            };
        }

        private DelegateCommand<byte[]> mCalculateCommand;
        public DelegateCommand<byte[]> CalculateCommand =>
            mCalculateCommand ?? (mCalculateCommand = new DelegateCommand<byte[]>(ExecuteCalculateCommand));

        void ExecuteCalculateCommand(byte[] data)
        {
            if (data == null)
                return;
            var crc = CRC.Calculate(data);
            var length = Math.Ceiling(CRC.Width / 8.0) * 2;
            CRCStr = $"0x{crc.ToString($"X{length}")}";
        }
    }
}
