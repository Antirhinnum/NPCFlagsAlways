using Microsoft.Xna.Framework;
using MonoMod.Cil;
using NPCFlagsAlways.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader : ILoadable
	{
		/// <summary>
		/// Replaces the two Lighting.GetColor calls in Main.DrawNPCHousesInWorld() with a custom color function.
		/// </summary>
		private void Main_DrawNPCHousesInWorld(ILContext il)
		{
			ILCursor cursor = new(il);

			// Replace both calls: one for the banner, one for the NPC head.
			for (int i = 0; i < 2; i++)
			{
				///	Match:
				///		... Lighting.GetColor(...) ...
				///	Change to:
				///		... GetNPCBannerColor(...) ...

				if (!cursor.TryGotoNext(MoveType.Before,
					i => i.MatchCall<Lighting>(nameof(Lighting.GetColor))))
				{
					LogHookFailed(il);
					return;
				}

				cursor.Remove();
				cursor.EmitDelegate(GetNPCBannerColor);
			}
		}

		private Color GetNPCBannerColor(int x, int y)
		{
			BannerVisibility visibility = Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility;
			Color baseColor = Lighting.GetColor(x, y);
			return visibility switch
			{
				BannerVisibility.Bright => Color.White,
				BannerVisibility.Faded => baseColor * 0.5f,
				BannerVisibility.Hidden => OriginalFailCondition ? Color.Transparent : baseColor,
				_ => baseColor,
			};
		}
	}
}