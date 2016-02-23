using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RA
{
	public class Page_CharMaker : Window
	{
		public const float TopAreaHeight = 80f;

		public static readonly Vector2 WinSize = new Vector2(1020f, 764f);

		public Pawn curPawn;

		public override Vector2 InitialWindowSize
		{
			get
			{
				return Page_CharMaker.WinSize;
			}
		}

		public Page_CharMaker(bool generateColonists)
		{
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.forcePause = true;
			this.soundAppear = null;
			if (generateColonists || MapInitData.colonists.Count == 0)
			{
                MapInitData.GenerateDefaultColonistsWithFaction();
			}
			this.curPawn = MapInitData.colonists[0];
		}

		public AcceptanceReport CanStart()
		{
			foreach (Pawn current in MapInitData.colonists)
			{
				if (!current.Name.IsValid)
				{
					return new AcceptanceReport(Translator.Translate("EveryoneNeedsValidName"));
				}
			}
			return AcceptanceReport.WasAccepted;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, 300f, 300f), Translator.Translate("CreateCharacters"));
            Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 80f, inRect.width, inRect.height - 38f - 80f);
			Widgets.DrawMenuSection(rect, true);
			TabDrawer.DrawTabs(rect, from c in MapInitData.colonists
			select new TabRecord(c.LabelCap, delegate
			{
				this.SelectPawn(c);
			}, c == this.curPawn));
			Rect rect2 = GenUI.ContractedBy(rect, 17f);
			Rect rect3 = rect2;
			rect3.width = rect2.width / 2f;
			CharacterCardUtility.DrawCharacterCard(rect3, this.curPawn, new Action(this.RandomizeCurChar));
			DialogUtility.DoNextBackButtons(inRect, Translator.Translate("Start"), new Action(this.TryStartGame), delegate
			{
				Find.WindowStack.Add(new Page_SelectLandingSite());
				this.Close(true);
			});
		}

		public void RandomizeCurChar()
		{
			do
			{
				this.curPawn = MapInitData.RegenerateStartingColonist(this.curPawn);
			}
			while (!MapInitData.AnyoneCanDoRequiredWorks());
		}

		public void SelectPawn(Pawn c)
		{
			if (c != this.curPawn)
			{
				this.curPawn = c;
			}
		}

		public void TryStartGame()
		{
			AcceptanceReport acceptanceReport = this.CanStart();
			if (!acceptanceReport.Accepted)
			{
				Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
				return;
			}
			Action action = delegate
			{
				MapInitData.colonyFaction.homeSquare = MapInitData.landingCoords;
				Find.FactionManager.Add(MapInitData.colonyFaction);
				FactionGenerator.EnsureRequiredEnemies(MapInitData.colonyFaction);
				MapInitData.colonyFaction = null;
				MapInitData.startedFromEntry = true;
				Application.LoadLevel("Gameplay");
			};
			LongEventHandler.QueueLongEvent(action, Translator.Translate("GeneratingMap"));
			this.Close(true);
		}
	}
}
