using Microsoft.Xna.Framework;
using MonoMod.Cil;
using NPCFlagsAlways.Common.Players;
using System;
using Terraria;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader
	{
		/// <summary>
		/// Replaces the two Lighting.GetColor calls in Main.DrawNPCHousesInWorld() with a custom color function.
		/// </summary>
		private static void Main_DrawNPCHouse(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			// Replace both calls: one for the banner, one for the NPC head.
			for (int k = 0; k < 2; k++)
			{
				if (!cursor.TryGotoNext(MoveType.Before,
					i => i.MatchCall<Lighting>(nameof(Lighting.GetColor))))
				{
					LogHookFailed(il);
					return;
				}

				cursor.Remove();
				cursor.EmitDelegate<Func<int, int, Color>>(GetNPCBannerColor);
			}
		}

		private static Color GetNPCBannerColor(int x, int y)
		{
			BannerVisibility visibility = Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility;
			Color baseColor = Lighting.GetColor(x, y);
			switch (visibility)
			{
				case BannerVisibility.Bright:
					return Color.White;

				case BannerVisibility.Faded:
					return baseColor * 0.5f;

				case BannerVisibility.Hidden:
					return OriginalFailCondition ? Color.Transparent : baseColor;

				default:
					return baseColor;
			}
		}
	}
}