using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NPCFlagsAlways.Common.Configs
{
	public class EvictionConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(false)]
		[DisplayName("Eviction Always Allowed")]
		[Tooltip("If true, then right-clicking an NPC banner will evict them even if the housing menu is closed.")]
		public bool EvictionAllowed { get; set; }
	}
}