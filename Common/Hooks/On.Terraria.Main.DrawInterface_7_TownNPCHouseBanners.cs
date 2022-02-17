using NPCFlagsAlways.Common.Players;
using System;
using Terraria;
using Terraria.ModLoader;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader : ILoadable
	{
		private void Main_DrawInterface_7_TownNPCHouseBanners(On.Terraria.Main.orig_DrawInterface_7_TownNPCHouseBanners orig, Main self)
		{
			orig(self);

			// Only force drawing if the condition in DrawInterface_7 fails.
			if (OriginalFailCondition && Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility != BannerVisibility.Hidden)
			{
				_DrawNPCHousesInWorld_Cached.Invoke(Main.instance, Array.Empty<object>());
			}
		}
	}
}