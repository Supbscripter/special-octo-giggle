﻿using System;
namespace GigglyLib.ProcGen
{
    public class CAGenerator
    {
        public CAGenerator() { }

        public bool[,] DoSimulationStep(bool[,] map, int iterations, int logicVersion)
        {
            int mapWidth = map.GetLength(0);
            int mapHeight = map.GetLength(1);
            for (int i = 0; i < iterations; i++)
            {
                bool[,] newMap = new bool[mapWidth, mapHeight];
                for (int y = 0, x = 0; x < mapWidth; x++)
                {
                    for (y = 0; y < mapHeight; y++)
                    {
                        newMap[x, y] = DoWallLogic(map, x, y, logicVersion);
                    }
                }
                map = newMap;
            }
            Console.WriteLine($"CA finished running after {iterations} cycles");
            return map;
        }

        private bool DoWallLogic(bool[,] map, int x, int y, int version)
        {
            int amount = GetAdjacentTiles(map, x, y);

            if (version == 0)
            {
                if (amount == 1)
                    return false;
                if (amount == 2)
                {
                    if (RandomPercent(20))
                        return true;
                    return false;
                }
                if (amount == 3 && RandomPercent(50))
                    return true;
                if (amount == 4 && RandomPercent(90))
                    return true;
                return map[x, y];
            }
            else if (version == 1)
            {
                if (amount == 2)
                    return RandomPercent(80);
                if (amount == 3)
                    return RandomPercent(60);
                return map[x, y];
            }
            throw (new Exception("wrong version passed"));
        }

        private int GetAdjacentTiles(bool[,] map, int x, int y)
        {
            int walls = (IsWall(map, x+1, y) ? 1 : 0) +
                        (IsWall(map, x-1, y) ? 1 : 0) +
                        (IsWall(map, x, y+1) ? 1 : 0) +
                        (IsWall(map, x, y-1) ? 1 : 0);
            return walls;
        }

        private bool IsWall(bool[,] map, int x, int y)
        {
            if (x >= map.GetLength(0) || x < 0 || y >= map.GetLength(1) || y < 0)
            { return false; }
            return map[x, y];
        }

        private bool RandomPercent(int percent)
        {
            int randomInt = Game1.GameStateRandom.Next(0, 100);
            if (randomInt < percent) { return true; }
            return false;
        }
    }
}
