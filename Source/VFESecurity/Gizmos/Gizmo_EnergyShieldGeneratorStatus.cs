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

namespace VFESecurity
{

    [StaticConstructorOnStartup]
    public class Gizmo_EnergyShieldGeneratorStatus : Gizmo
    {
        public Gizmo_EnergyShieldGeneratorStatus()
        {
            order = -100;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, shieldGenerator.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float displayEnergy = shieldGenerator.active ? shieldGenerator.Energy : 0;
                float fillPercent = displayEnergy / shieldGenerator.MaxEnergy;
                Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (displayEnergy * 100).ToString("F0") + " / " + (shieldGenerator.MaxEnergy * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        public Building_Shield shieldGenerator;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }

}
