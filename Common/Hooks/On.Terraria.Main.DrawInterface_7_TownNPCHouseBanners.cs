using NPCFlagsAlways.Common.Players;
using Terraria;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader
	{
		private static void Main_DrawInterface_7_TownNPCHouseBanners(On.Terraria.Main.orig_DrawInterface_7_TownNPCHouseBanners orig, Main self)
		{
			orig(self);

			// Only force drawing if the condition in DrawInterface_7 fails and banners aren't hidden.
			if (OriginalFailCondition && Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility != BannerVisibility.Hidden)
			{
				_DrawNPCHouse_Cached.Invoke(Main.instance, new object[] { });
			}
		}
	}
}