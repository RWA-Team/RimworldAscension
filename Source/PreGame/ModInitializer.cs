using System;
using UnityEngine;
using Verse;

namespace GlobalMod.PreGame
{
	public class ModInitializer : ITab
	{
		protected GameObject gameObject;

		public ModInitializer()
		{
			Log.Message("Initialized the EdB Prepare Carefully mod");
			this.gameObject = new GameObject("EdBPrepareCarefullyController");
			this.gameObject.AddComponent<ModController>();
			UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
		}

		protected override void FillTab()
		{
		}
	}
}
