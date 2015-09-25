using System;
using UnityEngine;
using Verse;

namespace RimworldAscension.PreGame
{
	public class ModInitializer : ITab
	{
		protected GameObject gameObject;

		public ModInitializer()
		{
			this.gameObject = new GameObject("RimworldAscensionController");
			this.gameObject.AddComponent<ModController>();
			UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
		}

		protected override void FillTab()
		{
		}
	}
}
