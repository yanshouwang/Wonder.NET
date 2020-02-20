using Wonder.UWP.ViewModels;

namespace Wonder.UWP
{
    internal static class Extensions
    {
        public static string ToValue(this EndSymbol symbol)
        {
            var value = string.Empty;
            switch (symbol)
            {
                case EndSymbol.Retrun:
                    value = "\r";
                    break;
                case EndSymbol.NewLine:
                    value = "\n";
                    break;
                case EndSymbol.ReturnNewLine:
                    value = "\r\n";
                    break;
                case EndSymbol.Zero:
                    value = "\0";
                    break;
            }
            return value;
        }

        public static string ToDisplay(this EndSymbol symbol)
        {
            var str = string.Empty;
            switch (symbol)
            {
                case EndSymbol.None:
                    str = "None";
                    break;
                case EndSymbol.Retrun:
                    str = @"\r";
                    break;
                case EndSymbol.NewLine:
                    str = @"\n";
                    break;
                case EndSymbol.ReturnNewLine:
                    str = @"\r\n";
                    break;
                case EndSymbol.Zero:
                    str = @"\0";
                    break;
            }
            return str;
        }
    }
}
