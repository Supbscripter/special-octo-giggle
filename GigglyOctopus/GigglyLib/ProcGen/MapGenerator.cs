using System;
using System.Collections.Generic;
using System.IO;
using DefaultEcs;
using GigglyLib.Components;
using static GigglyLib.Game1;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GigglyLib.ProcGen
{
    public class MapGenerator
    {
        MetaballGenerator _metaballGen;
        CAGenerator _CAGen;
        RoomGenerator _RoomGen;

        public MapGenerator() { }

        public void Generate()
        {
            _metaballGen = new MetaballGenerator(30f, 0.90f, 2, 4, angleVariance: 3.141f / 2f, angleVarianceDeadzone: 1f);
            _CAGen = new CAGenerator();
            _RoomGen = new RoomGenerator();

            bool[,] tiles = null;
            List<Room> rooms = null;
            int startRoom=-1;
            int endRoom=-1;
            // actual map gen code
            for (int i = 0; i < 1; i++)
            {
                tiles = _metaballGen.Generate();
                tiles = _CAGen.DoSimulationStep(tiles, 5, 0);
                (rooms, startRoom, endRoom) = _RoomGen.Generate(tiles);
                tiles = _CAGen.DoSimulationStep(tiles, 1, 1);
                Game1.DebugOutput += DebugOutput(tiles);
            }

            // Start room
            {
                var room = rooms[startRoom];
                var region = room.Region;
                int x = Game1.GameStateRandom.Next(region.X, region.X + region.Width);
                int y = Game1.GameStateRandom.Next(region.Y, region.Y + region.Height);
                CreatePlayer(x, y);
            }

            // End room
            {
                var room = rooms[endRoom];
                var region = room.Region;
                int x = (region.X + (region.Width / 2)) - 2;
                int y = (region.Y + (region.Height / 2)) - 2;
                CreatePortal(x, y);
            }

            List<Entity> enemies = new List<Entity>();

            for (int i = 0; i < rooms.Count; i++)
            {
                if (i == startRoom)
                    continue;
                var room = rooms[i];
                var region = room.Region;

                int enemiesToSpawn = Game1.GameStateRandom.Next(1, 4) + 2 * stageCount;
                for (int j = 0; j < enemiesToSpawn; j++)
                {
                    int x = Game1.GameStateRandom.Next(region.X, region.X + region.Width);
                    int y = Game1.GameStateRandom.Next(region.Y, region.Y + region.Height);
                    enemies.Add(SpawnEnemy(x, y));
                }
            }
            int randomEnemy = Game1.GameStateRandom.Next(enemies.Count);

            var specialEnemy = enemies[randomEnemy];
            specialEnemy.Set(new CEnemy { HasPowerUp = true });
            specialEnemy.Set(new CParticleSpawner
            {
                Texture = PARTICLES[(int)specialEnemy.Get<CWeaponsArray>().Weapons[0].Colour],
                Impact = 1.0f
            });
            enemies.RemoveAt(randomEnemy);


            randomEnemy = Game1.GameStateRandom.Next(enemies.Count);

            specialEnemy = enemies[randomEnemy];
            specialEnemy.Set(new CEnemy { HasPowerUp = true });
            specialEnemy.Set(new CParticleSpawner
            {
                Texture = PARTICLES[(int)specialEnemy.Get<CWeaponsArray>().Weapons[0].Colour],
                Impact = 1.0f
            });
            enemies.RemoveAt(randomEnemy);

            CreateTiles(tiles);
        }

        private void CreatePortal(int x, int y)
        {
            var portal = Game1.world.CreateEntity();
            portal.Set(new CPortal());
            portal.Set(new CGridPosition { X = x, Y = y });
            portal.Set(new CSprite {
                Texture = "portal",
                Depth = 1f,
                X = x * Config.TileSize,
                Y = y * Config.TileSize,
            });
            portal.Set(new CParticleSpawner
            {
                Impact = 3.5f,
                RandomColours = true
            });
        }

        private void CreatePlayer(int x, int y)
        {
            var _player = Game1.world.CreateEntity();
            Game1.Player = _player;
            _player.Set(new CPlayer());
            _player.Set(new CGridPosition { X = x, Y = y });
            _player.Set(new CMovable());
            _player.Set(new CHealth
            {
                Max = 15
            });
            _player.Set(new CSprite
            {
                Texture = "player",
                Transparency = 0.1f,
                Depth = 0.5f,
                X = x * Config.TileSize,
                Y = y * Config.TileSize
            });
            _player.Set(new CParticleSpawner
            {
                Texture = "particles-rainbow",
                Impact = 1.0f
            });
            var weapons = new CWeaponsArray
            {
                Weapons = new CWeaponsArray(Game1.startingWeapons).Weapons
            };
            if (weapons.Weapons.Count == 0)
            {
                weapons.Weapons.Add(new CWeapon
                {
                    Damage = 5,
                    RangeFront = 7,
                    RangeLeft = 2,
                    RangeRight = 2,
                    RangeBack = -1,
                    CooldownMax = 0,
                    AttackPattern = new List<string>
                    {
                       "0"
                    },
                    Colour = (Colour)Game1.NonDeterministicRandom.Next(18),
                    RandomColours = true
                });
            }
            Game1.startingWeapons.Weapons = null;
            // FOR TESTING
            //weapons.Weapons.Add(Config.Weapons["Sukima"]);

            //////////////////////////////////////////////////
            //                  GODMODE +w+                 //
            //////////////////////////////////////////////////
            //foreach (var w in Config.Weapons.Values)      //
            //{                                             //
            //    var _w = w;                               //
            //    _w.Colour = (Colour)Config.RandInt(18);   //
            //    weapons.Weapons.Add(_w);                  //
            //}                                             //
            //////////////////////////////////////////////////
            _player.Set(weapons);
        }

        private void CreateTiles(bool[,] tiles)
        {
            Tiles = new HashSet<(int, int)>();
            CostGrid = new int[tiles.GetLength(0), tiles.GetLength(1)];
            for (int x = 0; x < tiles.GetLength(0); x++)
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (!tiles[x, y])
                    {
                        CostGrid[x, y] = 1;
                        continue;
                    }
                    Tiles.Add((x, y));
                    CostGrid[x, y] = int.MaxValue;
                    var tileEntity = Game1.world.CreateEntity();
                    tileEntity.Set(new CSprite { Texture = "asteroid", Depth = 0.49f, X = x * Config.TileSize, Y = y * Config.TileSize });
                    tileEntity.Set(new CGridPosition { X = x, Y = y });
                }
        }

        public Entity SpawnEnemy(int gX, int gY, Direction dir = Direction.WEST)
        {
            var e = Game1.world.CreateEntity();
            bool hasPowerUp = (float)Game1.GameStateRandom.NextDouble() < 0.05;
            e.Set(new CGridPosition { X = gX, Y = gY, Facing = dir });
            e.Set<CMovable>();
            e.Set(new CHealth { Max = 15 });
            var weapon = Config.Weapons[new List<string>(Config.Weapons.Keys)[Game1.GameStateRandom.Next(Config.Weapons.Count)]];
            weapon.Colour = (Colour)Game1.NonDeterministicRandom.Next(18);
            var weapons = new List<CWeapon>();
            weapons.Add(weapon);
            e.Set(new CWeaponsArray { Weapons = weapons });
            e.Set(new CEnemy());
            e.Set(new CSprite { 
                Texture = PARTICLES[(int)weapon.Colour].Replace("particles", "enemy"), 
                Depth = 0.25f, 
                X = gX * Config.TileSize, 
                Y = gY * Config.TileSize,
                Transparency = 0.05f
            });
            e.Set(new CScalable{ Scale = 1.5f });
            e.Set(new CSourceRectangle
            {
                Rectangle = new Rectangle
                {
                    X = Game1.GameStateRandom.Next(10) * Config.TileSize,
                    Y = Game1.GameStateRandom.Next(10) * Config.TileSize,
                    Height = Config.TileSize,
                    Width = Config.TileSize
                }
            });
            return e;
        }

        private string DebugOutput(bool[,] tileGrid, List<BSPSplit> BSPLeafs = null)
        {
            string output = "";
            for (int y = 0; y < tileGrid.GetLength(1); y++)
            {
                for (int x = 0; x < tileGrid.GetLength(0); x++)
                {
                    string leaf = "";
                    if (BSPLeafs != null)
                        for (int i = 0; i < BSPLeafs.Count; i++)
                            if (BSPLeafs[i].Region[x, y])
                            {
                                leaf = i.ToString();
                                if (leaf.Length == 1)
                                    leaf = "0" + leaf;
                            }
                    if (leaf != "")
                        output += leaf;
                    else if (tileGrid[x, y])
                        output += "##";
                    else
                        output += "  ";
                }
                output += "\n";
            }
            output += "\n\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n\n";
            return output;
        }
    }
}
