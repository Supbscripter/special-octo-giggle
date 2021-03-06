﻿using System;
using DefaultEcs;
using DefaultEcs.System;
using GigglyLib.Components;

namespace GigglyLib.Systems
{
    public class RoundPrepSys : AEntitySystem<float>
    {
        public RoundPrepSys()
            : base(Game1.world.GetEntities().WithEither<CPlayer>().Or<CEnemy>().AsSet())
        { }

        protected override void Update(float state, in Entity entity)
        {
            entity.Set<CSimTurn>();

            base.Update(state, entity);
        }
    }
}
