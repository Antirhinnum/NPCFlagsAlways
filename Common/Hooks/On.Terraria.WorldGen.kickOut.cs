using NPCFlagsAlways.Common.Configs;
using NPCFlagsAlways.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace NPCFlagsAlways.Common.Hooks;

internal sealed partial class HookLoader : ILoadable
{
	private void WorldGen_kickOut(On_WorldGen.orig_kickOut orig, int n)
	{
		// Only allow NPCs to be evicted if:
		//	Flags should be drawn (housing menu open, proceed a normal), OR
		//	Eviction is always allowed and banners are being drawn.
		if (!OriginalFailCondition || (ModContent.GetInstance<EvictionConfig>().EvictionAllowed && Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility != BannerVisibility.Hidden))
		{
			orig(n);
		}
	}
}