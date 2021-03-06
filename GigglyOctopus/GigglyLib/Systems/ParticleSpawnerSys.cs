﻿using System;
using DefaultEcs;
using DefaultEcs.System;
using GigglyLib.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GigglyLib.Systems
{
    public class ParticleSpawnerSys : AEntitySystem<float>
    {
        public ParticleSpawnerSys()
            : base(Game1.world.GetEntities().With<CParticleSpawner>().With<CSprite>().AsSet())
        { }

        protected override void Update(float state, in Entity entity)
        {
            ref var sprite = ref entity.Get<CSprite>();

            float impact = entity.Get<CParticleSpawner>().Impact;

            float x = sprite.X;
            float y = sprite.Y;

            x += (Game1.NonDeterministicRandom.NextFloat() - 0.5f) * 0.2f * Config.TileSize * impact;
            y += (Game1.NonDeterministicRandom.NextFloat() - 0.5f) * 0.2f * Config.TileSize * impact;

            ref var spawner = ref entity.Get<CParticleSpawner>();
            var texture = spawner.Texture;
            float depth = 0;

            if (entity.Has<CPlayer>())
            {
                var health = entity.Get<CHealth>();
                if (Game1.NonDeterministicRandom.NextFloat() < (float) health.Damage / (float) health.Max)
                {
                    switch (Game1.NonDeterministicRandom.Next(10)) {
                        case 0:
                            texture = Game1.PARTICLES[0];
                            break;
                        case 1:
                            texture = Game1.PARTICLES[1];
                            break;
                        case 2:
                            texture = Game1.PARTICLES[2];
                            break;
                        default:
                            texture = "particles-smoke";
                            depth = 0.1f;
                            break;
                    }
                }
            }

            ParticleManager.CreateParticle(
                x: x,
                y: y,
                texture: spawner.RandomColours ? Game1.PARTICLES[Game1.NonDeterministicRandom.Next(18)] : texture,
                deltaRotation: Game1.NonDeterministicRandom.NextFloat() * 0.05f,
                velocity: Game1.NonDeterministicRandom.NextFloat() * 0.02f * impact,
                scale: (Game1.NonDeterministicRandom.NextFloat() * 0.4f) + 0.3f,
                depth: depth,
                transparency: (Game1.NonDeterministicRandom.NextFloat() * 0.2f) + 0.12f,
                rotation: Game1.NonDeterministicRandom.NextFloat() * 2 * (float)Math.PI
                );

            base.Update(state, entity);
        }
    }
}
