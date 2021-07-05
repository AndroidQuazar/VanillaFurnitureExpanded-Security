using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFESecurity
{
    [StaticConstructorOnStartup]
    public static class ArtillerySearchGroup
    {
        private static readonly Dictionary<ThingDef, ThingRequestGroup> registeredArtillery = new Dictionary<ThingDef, ThingRequestGroup>();

        static ArtillerySearchGroup()
        {
            RegisterArtillery(ThingDefOf.VFES_Turret_Catapult, ThingRequestGroup.Chunk);
        }

        public static bool RegisterArtillery(ThingDef def, ThingRequestGroup ammoGroup)
        {
            if (!registeredArtillery.ContainsKey(def))
            {
                registeredArtillery.Add(def, ammoGroup);
                return true;
            }
            return false;
        }

        public static bool TryGetArtillery(this Thing thing, out ThingRequestGroup group) => registeredArtillery.TryGetValue(thing.def, out group);
    }
}
