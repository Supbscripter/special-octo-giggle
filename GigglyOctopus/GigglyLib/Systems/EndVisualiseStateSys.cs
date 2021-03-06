﻿using System;
using DefaultEcs;
using DefaultEcs.System;
using GigglyLib.Components;

namespace GigglyLib.Systems
{
    public class EndVisualiseStateSys : ISystem<float>
    {
        EntitySet movingSet;
        EntitySet set;
        public EndVisualiseStateSys()
        {
            movingSet = Game1.world.GetEntities().With<CMoving>().AsSet();
            set = Game1.world.GetEntities().With<CSprite>().With<CGridPosition>().AsSet();
        }

        public bool IsEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public void Dispose() { }

        public void Update(float state)
        {
            if (movingSet.Count == 0)
            {
                Game1.currentRoundState++;
                foreach (var entity in set.GetEntities())
                {
                    ref var sprite = ref entity.Get<CSprite>();
                    ref var pos = ref entity.Get<CGridPosition>();
                    sprite.X = pos.X * Config.TileSize;
                    sprite.Y = pos.Y * Config.TileSize;
                }
            }
        }
    }
}
