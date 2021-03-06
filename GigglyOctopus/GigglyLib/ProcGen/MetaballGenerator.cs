﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GigglyLib.ProcGen
{
    internal struct Circle
    {
        public Circle(Vector2 pos, float radius, float spawnDir)
        {
            P = pos;
            R = radius;
            SpawnDir = spawnDir;
            Depth = 0;
        }
        public Circle(float x, float y, float radius, float spawnDir)
        {
            P = new Vector2(x, y);
            R = radius;
            SpawnDir = spawnDir;
            Depth = 0;
        }
        public Vector2 P;
        public float R;
        public float SpawnDir;
        public int Depth;
    }

    public class MetaballGenerator
    {
        List<Circle> _circles = new List<Circle>();
        float _rStart;
        float _rDecrease;
        int _maxDepth;
        int _minDepth;
        float _angleVariance;
        float _angleVarianceDeadzone;

        public MetaballGenerator(float startingR, float rDecrease, int minDepth, int maxDepth, float angleVariance, float angleVarianceDeadzone)
        {
            _rStart = startingR;
            _rDecrease = rDecrease;
            _maxDepth = maxDepth;
            _minDepth = minDepth;
            _angleVariance = angleVariance;
            _angleVarianceDeadzone = angleVarianceDeadzone;
        }

        public bool[,] Generate()
        {
            _circles = new List<Circle> {
                new Circle(0, 0, _rStart, 0f),
                new Circle(0, 0, _rStart, 0f),
                new Circle(0, 0, _rStart, 0f),
                new Circle(0, 0, _rStart, 0f),
                new Circle(0, 0, _rStart, 0f)
            };

            AddCircles(_circles);
            var tiles = GetOverlappingTiles();
            Console.WriteLine($"Metaball finished with {tiles.Count} overlapping tiles");
            return ConvertToGrid(tiles);
        }

        private void AddCircles(List<Circle> roots)
        {
            List<Circle> open = new List<Circle>(roots);

            while (open.Count > 0)
            {
                Circle circle = CreateCircleChild(open[0]);
                _circles.Add(circle);

                open.RemoveAt(0);
                if ((Game1.GameStateRandom.NextDouble() <= 0.8f || circle.Depth < _minDepth) && circle.Depth < _maxDepth)
                    open.Add(circle);
            }
        }

        private Circle CreateCircleChild(Circle parent)
        {
            float radius = parent.R * _rDecrease;
            float angle = 0f;
            if (parent.SpawnDir == 0f)
                angle = (float)Game1.GameStateRandom.NextDouble() * 6.282f;
            else
                angle = parent.SpawnDir + (float)Game1.GameStateRandom.Range(-_angleVariance, _angleVariance, _angleVarianceDeadzone);

            Vector2 offset = new Vector2(radius, 0f).RotateBy(angle);

            return new Circle(parent.P + offset, radius, angle) { Depth = parent.Depth + 1};
        }

        private List<(int x, int y)> GetOverlappingTiles()
        {
            var tiles = new List<(int x, int y)>();

            foreach (var circle in _circles)
            {
                int r = (int)Math.Round(Math.Abs(circle.R));
                int xMin = (int)(circle.P.X < 0 ? Math.Floor(circle.P.X) : Math.Ceiling(circle.P.X)) - r;
                int xMax = (int)(circle.P.X < 0 ? Math.Floor(circle.P.X) : Math.Ceiling(circle.P.X)) + r;
                int yMin = (int)(circle.P.Y < 0 ? Math.Floor(circle.P.Y) : Math.Ceiling(circle.P.Y)) - r;
                int yMax = (int)(circle.P.Y < 0 ? Math.Floor(circle.P.Y) : Math.Ceiling(circle.P.Y)) + r;

                for (int x = xMin; x <= xMax; x++)
                    for (int y = yMin; y <= yMax; y++)
                        if ((new Vector2(x, y) - circle.P).Length() < circle.R)
                        {
                            if (!tiles.Contains((x, y)))
                                tiles.Add((x, y));
                        }
            }

            return tiles;
        }

        private bool[,] ConvertToGrid(List<(int x, int y)> tilelist)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            for (int i = 0; i < tilelist.Count; i++)
            {
                var (x, y) = tilelist[i];
                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
                if (x < minX)
                    minX = x;
                if (y < minY)
                    minY = y;
            }

            int width = Math.Abs(minX) + maxX + 1;
            int height = Math.Abs(minY) + maxY + 1;
            bool[,] tileGrid = new bool[width, height];

            for (int i = 0; i < tilelist.Count; i++)
                tileGrid[tilelist[i].x + Math.Abs(minX), tilelist[i].y + Math.Abs(minY)] = true;
            return tileGrid;
        }
    }
}
