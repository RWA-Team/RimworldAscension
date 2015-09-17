using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace GlobalMod.PreGame
{
	internal class ModController : MonoBehaviour
	{
		public static readonly string ModName = "Global Mod";

		private Window currentLayer;

		private bool gameplay;

		private RootMap replacementRootMap;

		public Window TopLayer
		{
			get
			{
				foreach (Window current in Find.WindowStack.Windows)
				{
					if (current.GetType().FullName != "Verse.EditWindow_Log")
					{
						return current;
					}
				}
				return null;
			}
		}

		public bool ModEnabled
		{
			get
			{
				InstalledMod installedMod = InstalledModLister.AllInstalledMods.First((InstalledMod m) => m.Name.Equals(ModController.ModName));
				return installedMod != null && installedMod.Active;
			}
		}

		public virtual void Start()
		{
			base.enabled = true;
		}

		public void OnLevelWasLoaded(int level)
		{
			if (level == 0)
			{
				this.gameplay = false;
				base.enabled = true;
				return;
			}
			if (level == 1)
			{
				this.gameplay = true;
				base.enabled = false;
				if (this.ModEnabled)// && PrepareCarefully.Instance.Active && !ScenarioController.Instance.IsInScenario())
				{
					try
					{
						RootMap component = GameObject.Find("GameCoreDummy").GetComponent<RootMap>();
						component.enabled = false;
						UnityEngine.Object.DestroyImmediate(component);
						this.replacementRootMap = GameObject.Find("GameCoreDummy").AddComponent<RootMap>();
						Log.Message("Replaced original RootMap with EdB Prepare Carefully RootMap");
					}
					catch (Exception ex)
					{
						Log.Error("Failed to start the game with the EdB Prepare Carefully mod");
						Log.Error(ex.ToString());
						throw ex;
					}
				}
			}
		}

		public virtual void Update()
		{
			try
			{
				if (!this.gameplay)
				{
					this.MenusUpdate();
				}
				else
				{
					this.GameplayUpdate();
				}
			}
			catch (Exception ex)
			{
				base.enabled = false;
				Log.Error(ex.ToString());
			}
		}

		public virtual void MenusUpdate()
		{
			bool flag = false;
			Window topLayer = this.TopLayer;
			if (topLayer != this.currentLayer)
			{
				this.currentLayer = topLayer;
				flag = true;
			}
			if (topLayer != null && "RimWorld.Page_CharMaker".Equals(topLayer.GetType().FullName))
			{
				if (this.ModEnabled)
				{
					this.ResetTextures();
					Find.WindowStack.TryRemove(topLayer, true);
                    Find.WindowStack.Add(new Page_CharMaker(true));
					Log.Message("Swapped in EdB Prepare Carefully Character Creation Page");
					return;
				}
				if (flag)
				{
					Log.Message("EdB Prepare Carefully not enabled.  Did not replace Character Creation Page");
				}
			}
		}

		public virtual void GameplayUpdate()
		{
		}

		public void ResetTextures()
		{
		}
	}
}
