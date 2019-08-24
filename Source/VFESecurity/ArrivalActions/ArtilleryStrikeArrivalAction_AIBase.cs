using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace VFESecurity
{

    public abstract class ArtilleryStrikeArrivalAction_AIBase : ArtilleryStrikeArrivalAction
    {

        protected abstract bool CanDoArriveAction
        {
            get;
        }

        protected abstract int MapSize
        {
            get;
        }

        protected abstract int BaseSize
        {
            get;
        }

        protected abstract float DestroyChancePerCellInRect
        {
            get;
        }

        public override void Arrived(List<ActiveArtilleryStrike> artilleryStrikes, int tile)
        {
            // Boom
            if (CanDoArriveAction)
            {
                var harmfulStrikes = artilleryStrikes.Where(s => s.shellDef.projectile.damageDef.harmsHealth);
                if (harmfulStrikes.Any())
                {
                    PreStrikeAction();
                    bool destroyed = false;
                    var mapRect = new CellRect(0, 0, MapSize, MapSize);
                    var baseRect = new CellRect(GenMath.RoundRandom(mapRect.Width / 2f) - GenMath.RoundRandom(BaseSize / 2f), GenMath.RoundRandom(mapRect.Height / 2f) - GenMath.RoundRandom(BaseSize / 2f), BaseSize, BaseSize);
                    foreach (var strike in harmfulStrikes)
                        for (int i = 0; i < strike.shellCount; i++)
                            StrikeAction(strike, mapRect, baseRect, ref destroyed);
                    PostStrikeAction(destroyed);
                }
            }
        }

        protected virtual void PreStrikeAction()
        {
        }

        protected virtual void StrikeAction(ActiveArtilleryStrike strike, CellRect mapRect, CellRect baseRect, ref bool destroyed)
        {
        }

        protected virtual void PostStrikeAction(bool destroyed)
        {
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref worldObject, "worldObject");
            Scribe_References.Look(ref sourceMap, "sourceMap");
            base.ExposeData();
        }

        protected WorldObject worldObject;
        protected Map sourceMap;

    }

}
