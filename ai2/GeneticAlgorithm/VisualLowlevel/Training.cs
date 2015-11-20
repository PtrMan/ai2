using System;
using System.Collections.Generic;

using System.Drawing;

using Misc;

namespace GeneticAlgorithm.VisualLowlevel
{
    class Training
    {
        public static void learn()
        {
            string pathToImages = "C:\\Users\\r0b3\\temp\\aiExperiment0\\references\\";
            string pathToOutputImages = "C:\\Users\\r0b3\\temp\\aiExperiment0\\output\\";

            ComputationBackend.cs.OperatorColorTransformToHsl toHsl;
            ComputationBackend.cs.OperatorColorTransformFromHsl toRgb;

            toHsl = new ComputationBackend.cs.OperatorColorTransformToHsl();
            toRgb = new ComputationBackend.cs.OperatorColorTransformFromHsl();


            ComputationBackend.cs.OperatorColorTransform colorTransformForYellowBlue;
            ComputationBackend.cs.OperatorColorTransform colorTransformForRedGreen;

            colorTransformForYellowBlue = new ComputationBackend.cs.OperatorColorTransform();
            colorTransformForYellowBlue.colorForZero = new ColorRgb(0.5f, 0.5f, 0.0f);
            colorTransformForYellowBlue.colorForOne = new ColorRgb(0.0f, 0.0f, 1.0f);

            colorTransformForRedGreen = new ComputationBackend.cs.OperatorColorTransform();
            colorTransformForRedGreen.colorForZero = new ColorRgb(0.0f, 1.0f, 0.0f);
            colorTransformForRedGreen.colorForOne = new ColorRgb(1.0f, 0.0f, 0.0f);



            Image readImage = Image.FromFile(pathToImages + "StarCitizen0.jpg");
            

            // convert it to white/black image



            Bitmap workingBitmap = new Bitmap(readImage);


            Map2d<ColorRgb> readMap = new Map2d<ColorRgb>((uint)workingBitmap.Width, (uint)workingBitmap.Height);
            Map2d<ColorHsl> temporaryHsl = new Map2d<ColorHsl>((uint)workingBitmap.Width, (uint)workingBitmap.Height);
            Map2d<ColorRgb> convertedMap = new Map2d<ColorRgb>((uint)workingBitmap.Width, (uint)workingBitmap.Height);


            int x, y;

            
            for( y = 0; y < workingBitmap.Height; y++ )
            {
                for( x = 0; x < workingBitmap.Width; x++ )
                {
                    Color readColor = workingBitmap.GetPixel(x, y);

                    float r, g, b;

                    r = (float)readColor.R / 255.0f;
                    g = (float)readColor.G / 255.0f;
                    b = (float)readColor.B / 255.0f;

                    ColorRgb color = new ColorRgb(r, g, b);

                    readMap.writeAt(x, y, color);

                }
            }


            toHsl.inputRgb = readMap;
            toHsl.resultMap = temporaryHsl;

            toRgb.inputHsl = temporaryHsl;
            toRgb.resultMap = convertedMap;

            toHsl.calculate(null);


            for( y = 0; y < workingBitmap.Height; y++ )
            {
                for( x = 0; x < workingBitmap.Width; x++ )
                {
                    ColorHsl hsl = temporaryHsl.readAt(x, y);
                    hsl.l = 0.5f; // we don't want only the color without lighting information
                    hsl.s = 1.0f; // same
                    temporaryHsl.writeAt(x, y, hsl);
                }
            }

            toRgb.calculate(null);

            // convert to two channel representation

            Map2d<float> yellowBlueChannel = new Map2d<float>((uint)workingBitmap.Width, (uint)workingBitmap.Height);
            Map2d<float> redGreenChannel = new Map2d<float>((uint)workingBitmap.Width, (uint)workingBitmap.Height);

            colorTransformForYellowBlue.inputRgb = convertedMap;
            colorTransformForYellowBlue.resultMap = yellowBlueChannel;

            colorTransformForYellowBlue.calculate(null);

            colorTransformForRedGreen.inputRgb = convertedMap;
            colorTransformForRedGreen.resultMap = redGreenChannel;

            colorTransformForRedGreen.calculate(null);


            GeneticAlgorithm.VisualLowlevel.VisualLowLevel visualLowLevel;

            visualLowLevel = new GeneticAlgorithm.VisualLowlevel.VisualLowLevel();
            visualLowLevel.patchWidth = 32;

            // TODO< read all tiles from two channels >

            int tileX, tileY;

            for( tileY = 0; tileY < workingBitmap.Height / 32 - 1; tileY++ )
            {
                for (tileX = 0; tileX < workingBitmap.Height / 32 - 1; tileX++)
                {
                    GeneticAlgorithm.VisualLowlevel.VisualLowLevel.PatchSample patchYellowBlue;
                    GeneticAlgorithm.VisualLowlevel.VisualLowLevel.PatchSample patchRedGreen;

                    patchYellowBlue = new VisualLowLevel.PatchSample();
                    patchYellowBlue.values = new float[32 * 32];

                    patchRedGreen = new VisualLowLevel.PatchSample();
                    patchRedGreen.values = new float[32 * 32];

                    for( y = 0; y < 32; y++ )
                    {
                        for (x = 0; x < 32; x++)
                        {
                            int absoluteX, absoluteY;

                            absoluteX = x + 32 * tileX;
                            absoluteY = y + 32 * tileY;

                            patchYellowBlue.values[x + y * 32] = yellowBlueChannel.readAt(absoluteX, absoluteY);
                            patchRedGreen.values[x + y * 32] = redGreenChannel.readAt(absoluteX, absoluteY);
                        }
                    }

                    visualLowLevel.patchSamples.Add(patchYellowBlue);
                    visualLowLevel.patchSamples.Add(patchRedGreen);
                }
            }


            visualLowLevel.work(50000);

            List<float[]> templatesResult;

            templatesResult = new List<float[]>();

            visualLowLevel.getBestTemplates(templatesResult);



            // store template as image
            {
                Bitmap outputBitmap = new Bitmap(32, 32);

                int xp;
                int yp;

                for (xp = 0; xp < 32; xp++)
                {
                    for (yp = 0; yp < 32; yp++)
                    {
                        Color color;

                        float valueFloat;
                        int valueInt;

                        valueFloat = templatesResult[0][xp + yp * 32];

                        valueFloat *= 100.0f;
                        valueFloat = System.Math.Min(1.0f, valueFloat);

                        valueInt = (int)(valueFloat * 255.0f);

                        color = Color.FromArgb(valueInt, valueInt, valueInt);

                        outputBitmap.SetPixel(xp, yp, color);
                    }
                }

                outputBitmap.Save(pathToOutputImages + "template" + "0" + ".png");
            }

        }
    }
}
