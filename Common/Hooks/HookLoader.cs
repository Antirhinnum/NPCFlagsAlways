using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways.Common.Hooks;

internal sealed partial class HookLoader : ILoadable
{
	private const string ILFailMessage = "{0} could not patch {1}.";
	private const string BannerIconAssetPath = "NPCFlagsAlways/Assets/Textures/BannerIcon";
	private static Mod _mod;

	private MethodInfo _DrawNPCHousesInWorld_Cached;
	private MethodInfo _GetBuilderAccsCountToShow_Cached;
	private Asset<Texture2D> _bannerIconAsset;

	/// <summary>
	/// The original condition used to determine whether NPC housing flags should be drawn. Used for brevity.
	/// </summary>
	private static bool OriginalFailCondition => Main.EquipPage != 1 && (!UILinkPointNavigator.Shortcuts.NPCS_IconsDisplay || !PlayerInput.UsingGamepad);

	public void Load(Mod mod)
	{
		_mod = mod;

		_bannerIconAsset = ModContent.Request<Texture2D>(BannerIconAssetPath);
		_DrawNPCHousesInWorld_Cached = typeof(Main).GetMethod("DrawNPCHousesInWorld", BindingFlags.Instance | BindingFlags.NonPublic);
		_GetBuilderAccsCountToShow_Cached = typeof(Main).GetMethod("GetBuilderAccsCountToShow", BindingFlags.Static | BindingFlags.NonPublic);

		IL_Main.DrawNPCHousesInWorld += Main_DrawNPCHousesInWorld;
		On_Main.DrawBuilderAccToggles += Main_DrawBuilderAccToggles;
		On_Main.DrawInterface_7_TownNPCHouseBanners += Main_DrawInterface_7_TownNPCHouseBanners;
		On_Main.GetBuilderAccsCountToShow += Main_GetBuilderAccsCountToShow;
		On_WorldGen.kickOut += WorldGen_kickOut;
	}

	public void Unload()
	{
		_mod = null;

		_bannerIconAsset = null;
		_DrawNPCHousesInWorld_Cached = null;
		_GetBuilderAccsCountToShow_Cached = null;

		IL_Main.DrawNPCHousesInWorld -= Main_DrawNPCHousesInWorld;
		On_Main.DrawBuilderAccToggles -= Main_DrawBuilderAccToggles;
		On_Main.DrawInterface_7_TownNPCHouseBanners -= Main_DrawInterface_7_TownNPCHouseBanners;
		On_Main.GetBuilderAccsCountToShow -= Main_GetBuilderAccsCountToShow;
		On_WorldGen.kickOut -= WorldGen_kickOut;
	}

	private static void LogHookFailed(ILContext failingIL)
	{
		_mod.Logger.ErrorFormat(ILFailMessage, failingIL.Method.FullName);
		throw new Exception(string.Format(ILFailMessage, _mod.Name, failingIL.Method.FullName));
	}
}