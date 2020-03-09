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

    public class ListerThingsExtended : MapComponent
    {

        public ListerThingsExtended(Map map) : base(map)
        {
        }

        public List<Building_Shield> listerShieldGens = new List<Building_Shield>();
        public IEnumerable<Building_Shield> ListerShieldGensActive => listerShieldGens.Where(g => g.active && g.Energy > 0);

    }

}
