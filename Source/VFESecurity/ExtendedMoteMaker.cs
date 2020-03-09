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
using HarmonyLib;

namespace VFESecurity
{

    public static class ExtendedMoteMaker
    {

        public static void SearchlightEffect(Vector3 loc, Map map, float size, float lifespan)
        {
            if (!loc.ShouldSpawnMotesAt(map))
                return;

            var mote = (MoteSpotLight)ThingMaker.MakeThing(ThingDefOf.VFES_Mote_SpotLight);
            mote.lifespan = lifespan;
            mote.Scale = 1.9f * size;
            mote.exactPosition = loc;
            GenSpawn.Spawn(mote, loc.ToIntVec3(), map);
        }

    }

}
