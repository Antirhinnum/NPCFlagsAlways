using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader
	{
		private const string ILFailMessage = "NPCFlagsAlways could not patch {0}.";
		private static Mod _mod;

		private static Texture2D _bannerIconAsset;
		private static MethodInfo _DrawNPCHouse_Cached;

		/// <summary>
		/// The original condition used to determine whether NPC housing flags should be drawn. Used for brevity.
		/// </summary>
		private static bool OriginalFailCondition => Main.EquipPage != 1 && !UILinkPointNavigator.Shortcuts.NPCS_IconsDisplay;

		public static void Load(Mod mod)
		{
			_mod = mod;
			_bannerIconAsset = ModContent.GetTexture("NPCFlagsAlways/Assets/Textures/BannerIcon");
			_DrawNPCHouse_Cached = typeof(Main).GetMethod("DrawNPCHouse", BindingFlags.Instance | BindingFlags.NonPublic);

			On.Terraria.Main.DrawBuilderAccToggles += Main_DrawBuilderAccToggles;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners += Main_DrawInterface_7_TownNPCHouseBanners;
			On.Terraria.WorldGen.kickOut += WorldGen_kickOut;
			IL.Terraria.Main.DrawNPCHouse += Main_DrawNPCHouse;
		}

		public static void Unload()
		{
			_mod = null;
			_bannerIconAsset = null;
			_DrawNPCHouse_Cached = null;

			On.Terraria.Main.DrawBuilderAccToggles -= Main_DrawBuilderAccToggles;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners -= Main_DrawInterface_7_TownNPCHouseBanners;
			On.Terraria.WorldGen.kickOut -= WorldGen_kickOut;
			IL.Terraria.Main.DrawNPCHouse -= Main_DrawNPCHouse;
		}

		private static void LogHookFailed(ILContext failingIL)
		{
			_mod.Logger.ErrorFormat(ILFailMessage, failingIL.Method.FullName);
			throw new Exception(string.Format(ILFailMessage, failingIL.Method.FullName));
		}
	}
}