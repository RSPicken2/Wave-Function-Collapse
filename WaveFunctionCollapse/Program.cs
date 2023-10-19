using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using WaveFunctionCollapse;
using System.Diagnostics;

namespace WaveFunctionCollapse2D
{
    ////////////////////////////
    //how many tiles wide and tall the output should be
    const int waveWidth = 10;
    const int waveHeight = 10;
    //how wide and tall a single tile is in pixels
    const int tileWidth = 128;
    const int tileHeight = 128;
    /////////////////////////

    class Program
    {



        static readonly char[,] example =
        {
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'L', 'L' },
            {'L', 'L', 'L', 'L', 'B', 'L' },
            {'L', 'L', 'B', 'B', 'S', 'B' },
            {'L', 'B', 'S', 'S', 'S', 'S' },
            {'B', 'S', 'S', 'S', 'S', 'S' }
        };

        //static readonly char[,] example =
        //{
        //    {'A', 'B'},
        //    {'C', 'D'}

        //};


        static void Main(string[] args)
        {

            //get example image
            Bitmap input = new Bitmap("example.png");

            //convert the image into an array of integer IDs which the wave function can use and also get
            //the pixel data those Ids can point to later 
            (int[,] example, List<int[]> tileVals) =
                ImageHelper.GetTileIDs(input, tileHeight, tileWidth);
            input.Dispose();

            //start WFC
            WaveFunction waveFunction = new WaveFunction(waveWidth, waveHeight, example);
            int[,] collapsedWave = waveFunction.Generate();

            //generate an output image with our collapsed wave and the tile pixel data we got earlier
            Bitmap output =
                ImageHelper.GenerateOutputImage(collapsedWave, tileVals, tileHeight, tileWidth);

            output.Save("output.png", System.Drawing.Imaging.ImageFormat.Png);
            output.Dispose();



            



            //while (true)
            //{
            //    WaveFunction waveFunction = new WaveFunction(waveWidth, waveHeight, example);
            //    char[,] output = waveFunction.Generate();

            //    for (int y = 0; y < waveHeight; y++)
            //    {
            //        for (int x = 0; x < waveWidth; x++)
            //        {
            //            switch (output[y, x])
            //            {
            //                case 'L':
            //                    Console.ForegroundColor = ConsoleColor.Green;
            //                    break;
            //                case 'B':
            //                    Console.ForegroundColor = ConsoleColor.Yellow;
            //                    break;
            //                case 'S':
            //                    Console.ForegroundColor = ConsoleColor.Cyan;
            //                    break;

            //            }
            //            Console.Write(output[y, x]);
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.ForegroundColor = ConsoleColor.White;
            //    Console.ReadLine();
            //} 
        }
    }
}
