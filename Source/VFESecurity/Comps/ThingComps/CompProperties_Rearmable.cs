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

    public class CompProperties_Rearmable : CompProperties
    {

        public CompProperties_Rearmable()
        {
            compClass = typeof(CompRearmable);
        }

        public bool initiallyArmed;
        public GraphicData unarmedGraphicData;
        public int workToRearm;
        public SoundDef rearmSound;

    }

}
