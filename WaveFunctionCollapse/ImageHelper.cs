using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WaveFunctionCollapse
{
    class ImageHelper
    {
        static public (int[,], List<int[]>) GetTileIDs(Bitmap image, int tileHeight, int tileWidth)
        {
            //iterates through the input image and determines all the different tiles and stores them with ids in tileIds
            //the ids will then point back to the tiles' pixel data stored as 1D arrays in tileValues

            int[,] tileIds = new int[image.Height / tileHeight, image.Width / tileWidth];
            List<int[]> tileValues = new List<int[]>();
            int nextFreeID = 0;

            //go through each section in the image split up into the tiles 
            for (int y = 0; y < tileIds.GetLength(0); y++)
            {
                for (int x = 0; x < tileIds.GetLength(1); x++)
                {
                    //store where in the image we are
                    int imageX = x * tileWidth;
                    int imageY = y * tileHeight;


                    int[] newTileVal = new int[tileWidth * tileHeight];
                    int counter = 0;
                    //convert the 2D pixel data for the current tile into a 1D array as its easier to work with
                    for (int yoff = 0; yoff < tileHeight; yoff++)
                    {
                        for (int xoff = 0; xoff < tileWidth; xoff++)
                        {
                            newTileVal[counter++] =
                                image.GetPixel(imageX + xoff, imageY + yoff).ToArgb();
                        }
                    }
                    //determine if the current tile is unique or if we've already seen it
                    bool unique = true;
                    int tileId = -1;
                    for (int i = 0; i < tileValues.Count; i++)
                    {

                        if (tileValues[i].SequenceEqual(newTileVal))
                        {
                            unique = false;
                            tileId = i;
                            break;
                        }
                    }
                    //if its unique, add it to our list of tileValues and also give it a unique id
                    //else give the current cell an id pointing at whatever cell it is that we've already seen
                    if (unique)
                    {
                        tileValues.Add(newTileVal);
                        tileIds[y, x] = nextFreeID++;
                    }
                    else
                    {
                        tileIds[y, x] = tileId;
                    }
                }
            }
            return (tileIds, tileValues);
        }

        static public Bitmap GenerateOutputImage(int[,] collapsedWave, List<int[]> tileVals, int tileHeight, int tileWidth)
        {
            //Takes a collapsed wave array where each value is an index pointing to the pixel data in tileVals and uses these to construct an output bitmap

            int waveHeight = collapsedWave.GetLength(0);
            int waveWidth = collapsedWave.GetLength(1);

            Bitmap output = new Bitmap(waveWidth * tileWidth, waveHeight * tileHeight);

            //wavey and x are used to iterate through the tiles containeed in the wave
            for (int wavey = 0; wavey < waveHeight; wavey++)
            {
                for (int wavex = 0; wavex < waveWidth; wavex++)
                {
                    //get the pixel data for this section of the collapsed wave
                    int[] tileVal = tileVals[collapsedWave[wavey, wavex]];
                    //counter for where in tileVal we are
                    int counter = 0;
                   
                    //iterate through each pixel in this section of the collasped wave and use the pixel data to set the pixels

                    //tiley and x represent where in the specific tile we are
                    //these dont really need to be here but are easier to read than just working with imagex and y
                    for (int tiley = 0; tiley < tileHeight; tiley++)
                    {
                        //imagey and x represent where in the image we are writing to
                        int imagey = wavey * tileHeight + tiley;
                        for (int tilex = 0; tilex < tileWidth; tilex++)
                        {
                            int imagex = wavex * tileWidth + tilex;                           
                            Color colour = Color.FromArgb(tileVal[counter++]);
                            output.SetPixel(imagex, imagey, colour);
                        }
                    }
                }
            }
            return output;
        }
    }
}
