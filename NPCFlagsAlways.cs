using NPCFlagsAlways.Common.Configs;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways
{
	public class NPCFlagsAlways : Mod
	{
		private MethodInfo _drawNPCHouse_Cached;

		/// <summary>
		/// The original condition used to determine whether NPC housing flags should be drawn. Used for brevity.
		/// </summary>
		private static bool OriginalFailCondition => Main.EquipPage != 1 && (!UILinkPointNavigator.Shortcuts.NPCS_IconsDisplay || !PlayerInput.UsingGamepad);

		public NPCFlagsAlways()
		{
		}

		public override void Load()
		{
			_drawNPCHouse_Cached = typeof(Main).GetMethod("DrawNPCHousesInWorld", BindingFlags.Instance | BindingFlags.NonPublic);
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners += ForceDrawHouseBanners;
			On.Terraria.WorldGen.kickOut += PreventUnintentionalEvictions;
		}

		public override void Unload()
		{
			_drawNPCHouse_Cached = null;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners -= ForceDrawHouseBanners;
			On.Terraria.WorldGen.kickOut -= PreventUnintentionalEvictions;
		}

		private void ForceDrawHouseBanners(On.Terraria.Main.orig_DrawInterface_7_TownNPCHouseBanners orig, Main self)
		{
			orig(self);

			// Only force drawing if the condition in DrawInterface_7 fails.
			if (OriginalFailCondition)
			{
				_drawNPCHouse_Cached.Invoke(Main.instance, new object[] { });
			}
		}

		private void PreventUnintentionalEvictions(On.Terraria.WorldGen.orig_kickOut orig, int n)
		{
			// Only allow NPCs to be evicted if:
			//	You are the server (has no UI to click, unaffected by this mod), OR
			//	Flags should be drawn (housing menu open, proceed a normal), OR
			//	Eviction is always allowed
			if (Main.netMode == NetmodeID.Server || !OriginalFailCondition || ModContent.GetInstance<EvictionConfig>().EvictionAllowed)
			{
				orig(n);
			}
		}
	}
}