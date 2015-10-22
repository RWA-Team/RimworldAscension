using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace RA
{
    public class MainTabWindow_Architect : MainTabWindow
    {
        public const float WinWidth = 200f;

        public const float ButHeight = 32f;

        public List<ArchitectCategoryTab> desPanelsCached;

        public ArchitectCategoryTab selectedDesPanel;

        public float WinHeight
        {
            get
            {
                if (this.desPanelsCached == null)
                {
                    this.CacheDesPanels();
                }
                return (float)Mathf.CeilToInt((float)this.desPanelsCached.Count / 2f) * 32f;
            }
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(200f, this.WinHeight);
            }
        }

        protected override float WindowPadding
        {
            get
            {
                return 0f;
            }
        }

        public MainTabWindow_Architect()
        {

            this.CacheDesPanels();
        }

        public override void ExtraOnGUI()
        {
            base.ExtraOnGUI();
            if (this.selectedDesPanel != null)
            {
                this.selectedDesPanel.DesignationTabOnGUI();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Text.Font = GameFont.Small;
            float num = inRect.width / 2f;
            float num2 = 0f;
            float num3 = 0f;
            for (int i = 0; i < this.desPanelsCached.Count; i++)
            {
                Rect rect = new Rect(num2 * num, num3 * 32f, num, 32f);
                rect.height += 1f;
                if (num2 == 0f)
                {
                    rect.width += 1f;
                }
                if (WidgetsSubtle.ButtonSubtle(rect, this.desPanelsCached[i].def.LabelCap, 0f, 8f, SoundDefOf.MouseoverButtonCategory))
                {
                    this.ClickedCategory(this.desPanelsCached[i]);
                }
                num2 += 1f;
                if (num2 > 1f)
                {
                    num2 = 0f;
                    num3 += 1f;
                }
            }
        }

        public void CacheDesPanels()
        {
            this.desPanelsCached = new List<ArchitectCategoryTab>();
            foreach (DesignationCategoryDef current in from dc in DefDatabase<DesignationCategoryDef>.AllDefs
                                                       orderby dc.order descending
                                                       select dc)
            {
                this.desPanelsCached.Add(new ArchitectCategoryTab(current));
            }
        }

        public void ClickedCategory(ArchitectCategoryTab Pan)
        {
            if (this.selectedDesPanel == Pan)
            {
                this.selectedDesPanel = null;
            }
            else
            {
                this.selectedDesPanel = Pan;
            }
            SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
        }
    }
}
