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
using HarmonyLib;

namespace VFESecurity
{

    public abstract class ArtilleryStrikeArrivalAction : IExposable
    {

        public abstract void Arrived(List<ActiveArtilleryStrike> artilleryStrikes, int tile);

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref source, "source");
        }

        protected CompLongRangeArtillery ArtilleryComp => source.TryGetComp<CompLongRangeArtillery>();

        public Thing source;

    }

}
