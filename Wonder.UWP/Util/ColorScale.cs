// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace Wonder.UWP.Util
{
    public enum ColorScaleInterpolationMode { RGB, LAB, XYZ };

    public readonly struct ColorScaleStop
    {
        public ColorScaleStop(Color color, double position)
        {
            Color = color;
            Position = position;
        }

        public ColorScaleStop(ColorScaleStop source)
        {
            Color = source.Color;
            Position = source.Position;
        }

        public readonly Color Color;
        public readonly double Position;
    }

    public class ColorScale
    {
        // Evenly distributes the colors provided between 0 and 1
        public ColorScale(IEnumerable<Color> colors)
        {
            if (colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            int count = colors.Count();
            mStops = new ColorScaleStop[count];
            int index = 0;
            foreach (Color color in colors)
            {
                // Clean up floating point jaggies
                if (index == 0)
                {
                    mStops[index] = new ColorScaleStop(color, 0);
                }
                else if (index == count - 1)
                {
                    mStops[index] = new ColorScaleStop(color, 1);
                }
                else
                {
                    mStops[index] = new ColorScaleStop(color, (double)index * (1.0 / (double)(count - 1)));
                }
                index++;
            }
        }

        public ColorScale(IEnumerable<ColorScaleStop> stops)
        {
            if (stops == null)
            {
                throw new ArgumentNullException("stops");
            }

            int count = stops.Count();
            mStops = new ColorScaleStop[count];
            int index = 0;
            foreach (ColorScaleStop stop in stops)
            {
                mStops[index] = new ColorScaleStop(stop);
                index++;
            }
        }

        public ColorScale(ColorScale source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            mStops = new ColorScaleStop[source.mStops.Length];
            for (int i = 0; i < mStops.Length; i++)
            {
                mStops[i] = new ColorScaleStop(source.mStops[i]);
            }
        }

        private readonly ColorScaleStop[] mStops;

        public Color GetColor(double position, ColorScaleInterpolationMode mode = ColorScaleInterpolationMode.RGB)
        {
            if (mStops.Length == 1)
            {
                return mStops[0].Color;
            }
            if (position <= 0)
            {
                return mStops[0].Color;
            }
            else if (position >= 1)
            {
                return mStops[mStops.Length - 1].Color;
            }
            int lowerIndex = 0;
            for (int i = 0; i < mStops.Length; i++)
            {
                if (mStops[i].Position <= position)
                {
                    lowerIndex = i;
                }
            }
            int upperIndex = lowerIndex + 1;
            if (upperIndex >= mStops.Length)
            {
                upperIndex = mStops.Length - 1;
            }
            double scalePosition = (position - mStops[lowerIndex].Position) * (1.0 / (mStops[upperIndex].Position - mStops[lowerIndex].Position));

            switch (mode)
            {
                case ColorScaleInterpolationMode.LAB:
                    LAB leftLAB = ColorUtils.RGBToLAB(mStops[lowerIndex].Color, false);
                    LAB rightLAB = ColorUtils.RGBToLAB(mStops[upperIndex].Color, false);
                    LAB targetLAB = ColorUtils.InterpolateLAB(leftLAB, rightLAB, scalePosition);
                    return ColorUtils.LABToRGB(targetLAB, false).Denormalize();
                case ColorScaleInterpolationMode.XYZ:
                    XYZ leftXYZ = ColorUtils.RGBToXYZ(mStops[lowerIndex].Color, false);
                    XYZ rightXYZ = ColorUtils.RGBToXYZ(mStops[upperIndex].Color, false);
                    XYZ targetXYZ = ColorUtils.InterpolateXYZ(leftXYZ, rightXYZ, scalePosition);
                    return ColorUtils.XYZToRGB(targetXYZ, false).Denormalize();
                default:
                    return ColorUtils.InterpolateRGB(mStops[lowerIndex].Color, mStops[upperIndex].Color, scalePosition);
            }
        }

        public ColorScale Trim(double lowerBound, double upperBound, ColorScaleInterpolationMode mode = ColorScaleInterpolationMode.RGB)
        {
            if (lowerBound < 0 || upperBound > 1 || upperBound < lowerBound)
            {
                throw new ArgumentException("Invalid bounds");
            }
            if (lowerBound == upperBound)
            {
                return new ColorScale(new Color[] { GetColor(lowerBound, mode) });
            }
            List<ColorScaleStop> containedStops = new List<ColorScaleStop>(mStops.Length);

            for (int i = 0; i < mStops.Length; i++)
            {
                if (mStops[i].Position >= lowerBound && mStops[i].Position <= upperBound)
                {
                    containedStops.Add(mStops[i]);
                }
            }

            if (containedStops.Count == 0)
            {
                return new ColorScale(new Color[] { GetColor(lowerBound, mode), GetColor(upperBound, mode) });
            }

            if (containedStops.First().Position != lowerBound)
            {
                containedStops.Insert(0, new ColorScaleStop(GetColor(lowerBound, mode), lowerBound));
            }
            if (containedStops.Last().Position != upperBound)
            {
                containedStops.Add(new ColorScaleStop(GetColor(upperBound, mode), upperBound));
            }

            double range = upperBound - lowerBound;
            ColorScaleStop[] finalStops = new ColorScaleStop[containedStops.Count];
            for (int i = 0; i < finalStops.Length; i++)
            {
                double adjustedPosition = (containedStops[i].Position - lowerBound) / range;
                finalStops[i] = new ColorScaleStop(containedStops[i].Color, adjustedPosition);
            }
            return new ColorScale(finalStops);
        }

        public double FindNextColor(double position, double contrast, bool searchDown = false, ColorScaleInterpolationMode mode = ColorScaleInterpolationMode.RGB, double contrastErrorMargin = 0.005, int maxSearchIterations = 32)
        {
            if (position >= 1)
            {
                return 1;
            }
            if (position < 0)
            {
                position = 0;
            }
            Color startingColor = GetColor(position, mode);
            double finalPosition = 0.0;
            if (!searchDown)
            {
                finalPosition = 1.0;
            }
            Color finalColor = GetColor(finalPosition, mode);
            double finalContrast = ColorUtils.ContrastRatio(startingColor, finalColor, false);
            if (finalContrast <= contrast)
            {
                return finalPosition;
            }

            double testRangeMin, testRangeMax;
            if (searchDown)
            {
                testRangeMin = 0.0;
                testRangeMax = position;
            }
            else
            {
                testRangeMin = position;
                testRangeMax = 1.0;
            }
            double mid = finalPosition;
            int iterations = 0;
            while (iterations <= maxSearchIterations)
            {
                mid = Math.Abs(testRangeMax - testRangeMin) / 2.0 + testRangeMin;
                Color midColor = GetColor(mid, mode);
                double midContrast = ColorUtils.ContrastRatio(startingColor, midColor);

                if (Math.Abs(midContrast - contrast) <= contrastErrorMargin)
                {
                    return mid;
                }
                else if (midContrast > contrast)
                {
                    if(searchDown)
                    {
                        testRangeMin = mid;
                    }
                    else
                    {
                        testRangeMax = mid;
                    }
                }
                else
                {
                    if (searchDown)
                    {
                        testRangeMax = mid;
                    }
                    else
                    {
                        testRangeMin = mid;
                    }
                }

                iterations++;
            }

            return mid;
        }
    }
}
