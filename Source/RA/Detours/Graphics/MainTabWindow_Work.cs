using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class MainTabWindow_Work : MainTabWindow_PawnList
    {
        private const float TopAreaHeight = 40f;

        protected const float LabelRowHeight = 50f;

        private float workColumnSpacing = -1f;

        private static List<WorkTypeDef> visibleWorkTypesInPriorityOrder;

        private static DefMap<WorkTypeDef, Vector2> cachedLabelSizes = new DefMap<WorkTypeDef, Vector2>();

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(1010f, 90f + PawnsCount * 30f + 65f);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            visibleWorkTypesInPriorityOrder = (from def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                                                                  where def.visible
                                                                  select def).ToList();
            foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
            {
                cachedLabelSizes[current] = Text.CalcSize(current.labelShort);
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);
            if (Event.current.type == EventType.Layout)
            {
                return;
            }
            Rect position = new Rect(0f, 0f, rect.width, 40f);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(5f, 5f, 140f, 30f);
            bool useWorkPriorities = Current.Game.playSettings.useWorkPriorities;
            Widgets.CheckboxLabeled(rect2, "ManualPriorities".Translate(), ref Current.Game.playSettings.useWorkPriorities, false);
            if (useWorkPriorities != Current.Game.playSettings.useWorkPriorities)
            {
                foreach (Pawn current in Find.MapPawns.FreeColonists)
                {
                    current.workSettings.Notify_UseWorkPrioritiesChanged();
                }
            }
            float num = position.width / 3f;
            float num2 = position.width * 2f / 3f;
            Rect rect3 = new Rect(num - 50f, 5f, 160f, 30f);
            Rect rect4 = new Rect(num2 - 50f, 5f, 160f, 30f);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, "<= " + "HigherPriority".Translate());
            Widgets.Label(rect4, "LowerPriority".Translate() + " =>");
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Rect position2 = new Rect(0f, 40f, rect.width, rect.height - 40f);
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position2.width, position2.height - 50f);
            workColumnSpacing = (position2.width - 16f - 175f) / visibleWorkTypesInPriorityOrder.Count;
            float num3 = 175f;
            for (int i = 0; i < visibleWorkTypesInPriorityOrder.Count; i++)
            {
                WorkTypeDef wdef = visibleWorkTypesInPriorityOrder[i];
                Vector2 vector = cachedLabelSizes[wdef];
                float num4 = num3 + 15f;
                Rect rect5 = new Rect(num4 - vector.x / 2f, 0f, vector.x, vector.y);
                if (i % 2 == 1)
                {
                    rect5.y += 20f;
                }
                if (Mouse.IsOver(rect5))
                {
                    Widgets.DrawHighlight(rect5);
                }
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect5, wdef.labelShort);
                TooltipHandler.TipRegion(rect5, new TipSignal(() => string.Concat(wdef.gerundLabel, "\n\n", wdef.description, "\n\n", SpecificWorkListString(wdef)), wdef.GetHashCode()));
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
                Widgets.DrawLineVertical(num4, rect5.yMax - 3f, 50f - rect5.yMax + 3f);
                Widgets.DrawLineVertical(num4 + 1f, rect5.yMax - 3f, 50f - rect5.yMax + 3f);
                GUI.color = Color.white;
                num3 += workColumnSpacing;
            }
            DrawRows(outRect);
            GUI.EndGroup();
        }

        private static string SpecificWorkListString(WorkTypeDef def)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < def.workGiversByPriority.Count; i++)
            {
                stringBuilder.Append(def.workGiversByPriority[i].LabelCap);
                if (def.workGiversByPriority[i].emergency)
                {
                    stringBuilder.Append(" (" + "EmergencyWorkMarker".Translate() + ")");
                }
                if (i < def.workGiversByPriority.Count - 1)
                {
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }

        protected override void DrawPawnRow(Rect rect, Pawn p)
        {
            float num = 175f;
            Text.Font = GameFont.Medium;
            for (int i = 0; i < visibleWorkTypesInPriorityOrder.Count; i++)
            {
                WorkTypeDef wdef = visibleWorkTypesInPriorityOrder[i];
                Vector2 topLeft = new Vector2(num, rect.y + 2.5f);
                bool incapable = IsIncapableOfWholeWorkType(p, visibleWorkTypesInPriorityOrder[i]);
                WidgetsWork.DrawWorkBoxFor(topLeft, p, wdef, incapable);
                Rect rect2 = new Rect(topLeft.x, topLeft.y, 25f, 25f);
                TooltipHandler.TipRegion(rect2, () => WidgetsWork.TipForPawnWorker(p, wdef, incapable), p.thingIDNumber ^ wdef.GetHashCode());
                num += workColumnSpacing;
            }
        }

        private bool IsIncapableOfWholeWorkType(Pawn p, WorkTypeDef work)
        {
            for (int i = 0; i < work.workGiversByPriority.Count; i++)
            {
                bool flag = true;
                for (int j = 0; j < work.workGiversByPriority[i].requiredCapacities.Count; j++)
                {
                    PawnCapacityDef activity = work.workGiversByPriority[i].requiredCapacities[j];
                    if (!p.health.capacities.CapableOf(activity))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
