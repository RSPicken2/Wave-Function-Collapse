using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace WaveFunctionCollapse2D
{
    public class WaveFunction
    {
    
        //arrays storing vectors we can move, these get used when propegating collapses
        //and for storing compatabilites
        //in these y comes before x 
        static readonly int[] up = { -1, 0 },
             down = { 1, 0 },
             right = { 0, 1 },
             left = { 0, -1 };
        //array storing all these direction vectors
        static readonly int[][] directions = { up, down, right, left };

        //used for debug
        //static readonly string[] directionNames = { "up", "down", "right", "left" };

        //how many tiles wide the wave is
        int waveWidth;
        //how many tiles tall the wave is
        int waveHeight;
        //example to find our compatabilities from
        int[,] example;

        public WaveFunction(int waveWidth, int waveHeight, int[,] example)
        {
            this.waveWidth = waveWidth;
            this.waveHeight = waveHeight;
            this.example = example;
        }
        public int[,] Generate()
        {
            //returns a collapsed wave based on the example input, this can then be connverted to an image outside


            List<int> tiles; //list storing the different tile types
            List<int> tileWeights; //list storing how often those tiles appear in the example input
            GetTileFreq(example, out tiles, out tileWeights);

            //find which tiles are compatable with each other and in what direction
            List<int[]> compatabilities = GetCompatabilities(tiles);
            //used for debug
            //foreach (int[] i in compatabilities)
            //{
            //    Console.WriteLine("{0} {1} {2}", tiles[i[0]], tiles[i[1]], directionNames[i[2]]);
            //}
#if DEBUG
            Console.WriteLine("DEBUG: Compatabilities calculated");
#endif 

            //initialise wave
            //the first 2 dimentions here are coordinates (y,x), then the 3rd is used to show which tiles are still in superposition here
            bool[,,] wave = new bool[waveHeight, waveWidth, tiles.Count];
            Fill(ref wave, true);



            //at the start, any cell could be any pattern so they'll all have the same entropy
            //so we can just calculate it for one then fill the whole array with that value
            double startEntropy = CalcCellEntropy(0, 0, wave, tileWeights);
            double[,] entropies = new double[waveHeight, waveWidth];
            Fill(ref entropies, startEntropy);

            //collapse the wave
            //repeat until there isnt an exception (indicating a contradiction)
            bool success = false;      
            do
            {
                try
                {
                    wave = CollapseWave(wave, entropies, tileWeights, compatabilities);
                    success = true;
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e.Message);
#endif
                    Fill(ref wave, true);
                    Fill(ref entropies, startEntropy);
                }
            } while (!success);
#if DEBUG
            Console.WriteLine("DEBUG: Wave Collapsed");
#endif

            return GenerateOutput(wave, tiles);           
        }

        private int[,] GenerateOutput(bool[,,] wave, List<int> tiles)
        {
            //take the wave and convert it into an array storing whichever values the cells collapsed to 

            int[,] output = new int[wave.GetLength(0), wave.GetLength(1)];
            for (int y = 0; y < wave.GetLength(0); y++)
            {
                for (int x = 0; x < wave.GetLength(1); x++)
                {
                    for (int i = 0; i < wave.GetLength(2); i++)
                    {
                        if (wave[y, x, i])
                        {
                            output[y, x] = tiles[i];
                            break;
                        }
                    }
                }
            }
            return output;
        }

        private bool[,,] CollapseWave(bool[,,] wave, double[,] entropies, List<int> tileWeights, List<int[]> compatabilities)
        {
            //takes the superposed wave and attempts to collapse it down into a low entropy state
            do
            {
               //find which cell has the lowest entropy
                int[] L = GetLowestEntropyPos(entropies);
                int y = L[0], x = L[1];

                //find the weights of all the potential tiles the current cell could be
                //- if a tile isnt in superposition here, its weight is 0
                List<int> currentTileWeights = new List<int>();
                for (int i = 0; i < wave.GetLength(2); i++)
                {
                    if (wave[y, x, i])
                    {
                        currentTileWeights.Add(tileWeights[i]);
                    }
                    else
                    {
                        currentTileWeights.Add(0);
                    }
                }
                //pick a weighted random tile to collapse the current cell to
                int collapsedTile = PickWeightedRandomTile(currentTileWeights);
                for (int i = 0; i < wave.GetLength(2); i++)
                {
                    wave[y,x, i] = false;
                }
                wave[y, x, collapsedTile] = true;
                entropies[y, x] = 0;

                //propegate the collapse to the surrounding cells in the wave
                PropegateCollapse(ref wave, ref entropies, compatabilities, tileWeights, y, x);
#if DEBUG
                Console.WriteLine("DEBUG: Collapse Propegated");
#endif
                //loop while there are still cells with non 0 entropies
            } while (!IsFullyCollapsed(entropies));
            return wave;
        }

        private void PropegateCollapse(ref bool[,,] wave, ref double[,] entropies, List<int[]> compatabilities, List<int> tileWeights, int y, int x)
        {
            //propegates out the collapse of a tile recursively by removing tiles from the surrounding cell's superposition if the collapse has now made any of those
            //adjacencies invalid

#if DEBUG
            #region debugOutput
            //for debugging
            //for (int i = 0; i < waveHeight; i++)
            //{
            //    for (int j = 0; j < waveWidth; j++)
            //    {
            //        if (i == y && j == x)
            //        {
            //            Console.BackgroundColor = ConsoleColor.DarkGray;
            //        }
            //        else
            //        {
            //            Console.BackgroundColor = ConsoleColor.Black;
            //        }
            //        for (int k = 0; k < wave.GetLength(2); k++)
            //        {                      
            //            if (wave[i, j, k])
            //            {
            //                Console.Write(k);
            //            }
            //            else
            //            {
            //                Console.Write(" ");
            //            }
            //        }
            //        Console.BackgroundColor = ConsoleColor.Black;
            //        Console.Write(" ");
            //    }
            //    Console.WriteLine();
            //}
            //Console.WriteLine("\n");
            #endregion
#endif

            //get all the tiles the current tile could be (including if thats just 1 tile)
            List<int> potenTiles = new List<int>();
            for (int i = 0; i < wave.GetLength(2); i++)
            {
                if (wave[y, x, i])
                {
                    potenTiles.Add(i);
                }
            }

            //for each direction we can move in (not off the side of the wave)
            foreach (int[] direc in GetValidDirections(wave, y, x))
            {
                //direc[0] = y, direc[1] = x

                //get all the cells in the adjacent cells superposition
                    List<int> adjPotenTiles = new List<int>();
                    for (int i = 0; i < wave.GetLength(2); i++)
                    {
                        if (wave[y + direc[0], x + direc[1], i])
                        {
                            adjPotenTiles.Add(i);
                        }
                    }

                    //stores if any of the adjacent cells potential tiles now become invalid 
                    bool isUpdated = false;

                    //for each potential tile the adjacent cell could become
                    foreach (int adjTile in adjPotenTiles)
                    {
                        bool isValid = false;
                        //for each potential tile in the current tile
                        foreach (int currentTile in potenTiles)
                        {
                            //stores the compatability needed for these 2 tiles being next to each other being valid
                            int[] neededCompat = new int[3];
                            neededCompat[0] = currentTile;
                            neededCompat[1] = adjTile;
                            neededCompat[2] = Array.IndexOf(directions, direc);

                            //try and find the needed compatability 
                            for (int i = 0; i < compatabilities.Count; i++)
                            {
                                if (compatabilities[i].SequenceEqual(neededCompat))
                                {
                                    isValid = true;
                                    break;
                                }
                            }
                            if (isValid) break;
                        }
                        //has the compatability needed  been found?
                        if (!isValid)
                        {
                            isUpdated = true;
                            //if not then that tile in the adjacent cell has to be removed from the superposition
                            wave[y + direc[0], x + direc[1], adjTile] = false;
                        }
                    }
                    //the change to the adjacent cell's superposition may affect the cells around it so we need to recurse here
                    //we also calculate the new entropy of the adjacent cell here
                    if (isUpdated)
                    {
                        entropies[y + direc[0], x + direc[1]] = CalcCellEntropy(y + direc[0], x + direc[1], wave, tileWeights);
                        PropegateCollapse(ref wave, ref entropies, compatabilities, tileWeights, y + direc[0], x + direc[1]);
                    }
                  
            }
        }

        private bool IsFullyCollapsed(double[,] entropies)
        {
            //returns if there are no values > 0 in entropies

            for (int i = 0; i < entropies.GetLength(0); i++)
            {
                for (int j = 0; j < entropies.GetLength(1); j++)
                {
                    if (entropies[i, j] > 0) return false;
                }
            }
            return true;
        }

        private void Fill<T>(ref T[,,] array, T value)
        {
            //fills a 3d array with a value
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        array[i, j, k] = value;
                    }
                }
            }
        }
        private void Fill<T>(ref T[,] array, T value)
        {
            //fills a 2d array with a value
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = value;
                }
            }
        }

        private void GetTileFreq(int[,] example, out List<int> tiles, out List<int> tileFreqs)
        {
            //searches through the example array and finds all the different tiles in it and how often they appear
            tiles = new List<int>();
            tileFreqs = new List<int>();

            for (int y = 0; y < example.GetLength(0); y++)
            {
                for (int x = 0; x < example.GetLength(1); x++)
                {
                    int currentTile = example[y, x];
                    int index = tiles.IndexOf(currentTile);
                    if (index == -1)
                    {
                        tiles.Add(currentTile);
                        tileFreqs.Add(1);
                    }
                    else
                    {
                        tileFreqs[index]++;
                    }
                }
            }
        }
        private List<int[]> GetCompatabilities(List<int> tiles)
        {
            //gets a list of all the compatabiliteis between tiles shown in the original example
            //a compatability is stored as an int array (tile1 id, tile2 id, direction from tile1 to tile2 id)
            List<int[]> compatabilities = new List<int[]>();
            for (int y = 0; y < example.GetLength(0); y++)
            {
                for (int x = 0; x < example.GetLength(1); x++)
                {
                    foreach (int[] direc in GetValidDirections(example, y, x))
                    {
                        int[] compat = new int[3];
                        compat[0] = tiles.IndexOf(example[y, x]);
                        compat[1] = tiles.IndexOf(example[y + direc[0], x + direc[1]]);
                        compat[2] = Array.IndexOf(directions, direc);

                        //try and find this compatability in our list
                        bool isUnique = true;
                        foreach (int[] c in compatabilities)
                        {
                            if (c.SequenceEqual(compat)) isUnique = false;
                        }
                        //if its not in the list, add it
                        if (isUnique) compatabilities.Add(compat);
                    }
                }
            }
            return compatabilities;
        }

        private List<int[]> GetValidDirections(int[,] example, int y, int x)
        {
            //returns which directions can be checked in the example when finding compatabilities
            //so we dont ever try to check outside the bounds of the example
            var valid = new List<int[]>();
            if (y > 0) valid.Add(up);
            if (y < example.GetLength(0) - 1) valid.Add(down);
            if (x > 0) valid.Add(left);
            if (x < example.GetLength(1) - 1) valid.Add(right);
            return valid;
        }
        private List<int[]> GetValidDirections(bool[,,] wave, int y, int x)
        {
            //returns which directions can be checked in the wave when propegating collapses
            //so we dont ever try to check outside the bounds of the example
            var valid = new List<int[]>();
            if (y > 0) valid.Add(up);
            if (y < wave.GetLength(0) - 1) valid.Add(down);
            if (x > 0) valid.Add(left);
            if (x < wave.GetLength(1) - 1) valid.Add(right);
            return valid;
        }

        private double CalcCellEntropy(int y, int x, bool[,,] wave, List<int> tileWeights)
        {
            //calculates the shannon entropy of a single cell on the wave
            //will return 0 if the cell is collapsed 
            //if there has been a contradiction, this method will throw an exception to be handled
            double sumWeights = 0d;
            double sumWeightsLogWeights = 0d;
            for (int i = 0; i < wave.GetLength(2); i++)
            {
                if (wave[y, x, i])
                {
                    sumWeights += tileWeights[i];
                    sumWeightsLogWeights += tileWeights[i] * Math.Log(tileWeights[i]);
                }
            }

            double entropy = Math.Log(sumWeights) - (sumWeightsLogWeights / sumWeights);
            if (entropy == double.NaN)
            {
                throw new Exception("Contradiction found");
            }
            return entropy;
        }

        private int PickWeightedRandomTile(List<int> currentTileWeights)
        {
            //returns the index of a tile randomly based on its weight 
            var rand = new Random();

            int sumWeight = 0;
            for (int i = 0; i < currentTileWeights.Count; i++)
            {
                sumWeight += currentTileWeights[i];
            }
            //the min of 1 is needed because if a 0 is generated, the first tile can be selected even if its weight is 0
            int randVal = rand.Next(1,sumWeight);
            int currentweight = 0;
            for (int i = 0; i < currentTileWeights.Count; i++)
            {
                currentweight += currentTileWeights[i];
               
                if (randVal <= currentweight) return i;
            }
            throw new Exception("random number error");
        }

        private int[] GetLowestEntropyPos(double[,] entropies)
        {
            //find the cell with the lowest entropy in the wave
            int[] lowestPos = new int[2];
            double lowest = double.MaxValue;

            for (int y = 0; y < entropies.GetLength(0); y++)
            {
                for (int x = 0; x < entropies.GetLength(1); x++)
                {
                    if (entropies[y,x] < lowest && entropies[y, x] > 0)
                    {
                        lowest = entropies[y, x];
                        lowestPos[0] = y;
                        lowestPos[1] = x;
                    }
                }
            }
            return lowestPos;
        } 
    }
}
