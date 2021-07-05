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

    public class ArtilleryStrikeArrivalAction_Settlement : ArtilleryStrikeArrivalAction_AIBase
    {

        public ArtilleryStrikeArrivalAction_Settlement()
        {
        }

        public ArtilleryStrikeArrivalAction_Settlement(WorldObject worldObject)
        {
            this.worldObject = worldObject;
        }

        protected Settlement Settlement => worldObject as Settlement;

        protected override bool CanDoArriveAction => Settlement != null && Settlement.Spawned && Settlement.Faction != Faction.OfPlayer;

        protected override int MapSize => GameInitData.DefaultMapSize;

        protected override int BaseSize => 36;

        protected override float DestroyChancePerCellInRect => 0.01f;

        protected override void PreStrikeAction()
        {
            Settlement.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -99999, reason: "VFESecurity.GoodwillChangedReason_ArtilleryStrike".Translate());
        }

        protected override void StrikeAction(ActiveArtilleryStrike strike, CellRect mapRect, CellRect baseRect, ref bool destroyed)
        {
            var radialCells = GenRadial.RadialCellsAround(mapRect.RandomCell, strike.shellDef.projectile.explosionRadius, true);
            int cellsInRect = radialCells.Count(c => baseRect.Contains(c));

            // Destroy settlement
            if (cellsInRect > 0 && Rand.Chance(cellsInRect * DestroyChancePerCellInRect))
            {
                Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "VFESecurity.LetterFactionBaseDefeatedStrike".Translate(Settlement.Label), LetterDefOf.PositiveEvent,
                    new GlobalTargetInfo(Settlement.Tile), Settlement.Faction, null);
                var destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(RimWorld.WorldObjectDefOf.DestroyedSettlement);
                destroyedSettlement.Tile = Settlement.Tile;
                Find.WorldObjects.Add(destroyedSettlement);
                Find.WorldObjects.Remove(Settlement);
                destroyed = true;
            }
        }

        protected override void PostStrikeAction(bool destroyed)
        {
            if (!destroyed)
            {
                // Otherwise artillery retaliation
                var artilleryComp = Settlement.GetComponent<ArtilleryComp>();
                if (artilleryComp != null)
                    artilleryComp.TryStartBombardment();
            }
        }

    }

}
