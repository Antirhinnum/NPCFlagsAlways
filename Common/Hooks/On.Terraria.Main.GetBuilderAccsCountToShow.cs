using Terraria;
using Terraria.ModLoader;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader : ILoadable
	{
		private void Main_GetBuilderAccsCountToShow(On.Terraria.Main.orig_GetBuilderAccsCountToShow orig, Player plr, out int blockReplaceIcons, out int torchGodIcons, out int totalDrawnIcons)
		{
			// Increase torchGodIcons so that the banner toggle goes above wiring toggles.
			orig(plr, out blockReplaceIcons, out torchGodIcons, out totalDrawnIcons);
			torchGodIcons += 1;
			totalDrawnIcons += 1;
		}
	}
}