using System.Linq;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Controller : MonoBehaviour
	{
		public Window currentLayer;
		public bool gameplay;

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
                return InstalledModLister.AllInstalledMods.Any(mod => mod.Name == "Rimworld Ascension" && mod.Active);
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
            }
            if (level == 1)
            {
                this.gameplay = true;
                base.enabled = false;

                if (ModEnabled)
                {
                    RootMap component = GameObject.Find("GameCoreDummy").GetComponent<RootMap>();
                    component.enabled = false;
                    DestroyImmediate(component);
                    GameObject.Find("GameCoreDummy").AddComponent<RA_RootMap>();
                }
            }
        }

        public virtual void Update()
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

        public virtual void MenusUpdate()
        {
            if (ModEnabled)
            {
                if (TopLayer != null && "RimWorld.Page_CharMaker".Equals(TopLayer.GetType().FullName))
                {
                    Find.WindowStack.TryRemove(TopLayer, true);
                    Find.WindowStack.Add(new Page_CharMaker(true));
                }
            }
        }

		public virtual void GameplayUpdate()
		{
		}
	}
}
