using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Wonder.UWP.Util;

namespace Wonder.UWP.Extension
{
    public static class ColorExtensions
    {
        public static Color GetColor(this ColorScale scale, double position)
        {
            var color = scale.GetColor(position, ColorScaleInterpolationMode.RGB);
            return color;
        }

        public static ColorScale GetColorScale(this Color baseColorRGB)
        {
            var baseColorHSL = ColorUtils.RGBToHSL(baseColorRGB);
            var baseColorNormalized = new NormalizedRGB(baseColorRGB);

            var scaleColorLight = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            var scaleColorDark = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            var baseScale = new ColorScale(new[] { scaleColorLight, baseColorRGB, scaleColorDark });

            var clipLight = 0.185;
            var clipDark = 0.160;
            var trimmedScale = baseScale.Trim(clipLight, 1.0 - clipDark);
            var interpolationMode = ColorScaleInterpolationMode.RGB;
            var trimmedLight = new NormalizedRGB(trimmedScale.GetColor(0, interpolationMode));
            var trimmedDark = new NormalizedRGB(trimmedScale.GetColor(1, interpolationMode));

            var adjustedLight = trimmedLight;
            var adjustedDark = trimmedDark;

            var saturationAdjustmentCutoff = 0.05;
            if (baseColorHSL.S >= saturationAdjustmentCutoff)
            {
                var saturationLight = 0.35;
                var saturationDark = 1.25;
                adjustedLight = ColorBlending.SaturateViaLCH(adjustedLight, saturationLight);
                adjustedDark = ColorBlending.SaturateViaLCH(adjustedDark, saturationDark);
            }

            var multiplyLight = 0.0;
            if (multiplyLight != 0)
            {
                var multiply = ColorBlending.Blend(baseColorNormalized, adjustedLight, ColorBlendMode.Multiply);
                adjustedLight = ColorUtils.InterpolateColor(adjustedLight, multiply, multiplyLight, interpolationMode);
            }

            var multiplyDark = 0.0;
            if (multiplyDark != 0)
            {
                var multiply = ColorBlending.Blend(baseColorNormalized, adjustedDark, ColorBlendMode.Multiply);
                adjustedDark = ColorUtils.InterpolateColor(adjustedDark, multiply, multiplyDark, interpolationMode);
            }

            var overlayLight = 0.0;
            if (overlayLight != 0)
            {
                var overlay = ColorBlending.Blend(baseColorNormalized, adjustedLight, ColorBlendMode.Overlay);
                adjustedLight = ColorUtils.InterpolateColor(adjustedLight, overlay, overlayLight, interpolationMode);
            }

            var overlayDark = 0.25;
            if (overlayDark != 0)
            {
                var overlay = ColorBlending.Blend(baseColorNormalized, adjustedDark, ColorBlendMode.Overlay);
                adjustedDark = ColorUtils.InterpolateColor(adjustedDark, overlay, overlayDark, interpolationMode);
            }

            var finalScale = new ColorScale(new Color[] { adjustedLight.Denormalize(), baseColorRGB, adjustedDark.Denormalize() });
            return finalScale;
        }
    }
}
