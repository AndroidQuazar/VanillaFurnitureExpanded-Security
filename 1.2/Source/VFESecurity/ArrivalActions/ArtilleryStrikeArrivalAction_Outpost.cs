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

    public class ArtilleryStrikeArrivalAction_Outpost : ArtilleryStrikeArrivalAction_AIBase
    {

        public ArtilleryStrikeArrivalAction_Outpost()
        {
        }

        public ArtilleryStrikeArrivalAction_Outpost(WorldObject worldObject)
        {
            this.worldObject = worldObject;
        }

        protected Site Site => worldObject as Site;

        protected override bool CanDoArriveAction => Site != null && Site.Spawned;

        protected override int MapSize => CaravanArrivalAction_VisitSite.MapSize.x;

        protected override int BaseSize => 16;

        protected override float DestroyChancePerCellInRect => 0.02f; 

        protected override void StrikeAction(ActiveArtilleryStrike strike, CellRect mapRect, CellRect baseRect, ref bool destroyed)
        {
            Log.Message(Site.AllComps.ToStringSafeEnumerable());
            var radialCells = GenRadial.RadialCellsAround(mapRect.RandomCell, strike.shellDef.projectile.explosionRadius, true);
            int cellsInRect = radialCells.Count(c => baseRect.Contains(c));

            // Destroy outpost and give reward
            if (cellsInRect > 0 && Rand.Chance(cellsInRect * DestroyChancePerCellInRect))
            {
                QuestUtility.SendQuestTargetSignals(Site.questTags, QuestUtility.QuestTargetSignalPart_AllEnemiesDefeated, Site.Named("SUBJECT"));
                NonPublicFields.Site_allEnemiesDefeatedSignalSent.SetValue(Site, true);
                Find.WorldObjects.Remove(worldObject);
                destroyed = true;
            }
        }

    }

}
