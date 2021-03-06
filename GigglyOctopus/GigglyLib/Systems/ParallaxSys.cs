﻿using System;
using DefaultEcs;
using DefaultEcs.System;
using GigglyLib.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GigglyLib.Systems
{
    public class ParallaxSys : AEntitySystem<float>
    {
        public ParallaxSys()
            : base(Game1.world.GetEntities().With<CParallaxBackground>().With<CSprite>().AsSet())
        {
        }

        protected override void Update(float state, in Entity entity)
        {
            ref var pos = ref Game1.Player.Get<CGridPosition>();
            ref var playerSprite = ref Game1.Player.Get<CSprite>();
            ref var sprite = ref entity.Get<CSprite>();
            ref var parallax = ref entity.Get<CParallaxBackground>();

            parallax.OffsetX +=
                pos.Facing == Direction.WEST ? parallax.ScrollVelocity :
                pos.Facing == Direction.EAST ? -parallax.ScrollVelocity :
                0;

            parallax.OffsetY +=
                pos.Facing == Direction.NORTH ? parallax.ScrollVelocity :
                pos.Facing == Direction.SOUTH ? -parallax.ScrollVelocity :
                0;

            float width = Config.Textures[sprite.Texture].Width;
            float height = Config.Textures[sprite.Texture].Height;

            if (parallax.OffsetX < -width/2)
                parallax.OffsetX += width;
            if (parallax.OffsetX >= width/2)
                parallax.OffsetX -= width;
            if (parallax.OffsetY < -height/2)
                parallax.OffsetY += height;
            if (parallax.OffsetY >= height/2)
                parallax.OffsetY -= height;

            sprite.X = playerSprite.X + parallax.OffsetX;
            sprite.Y = playerSprite.Y + parallax.OffsetY;

            base.Update(state, entity);
        }
    }
}
