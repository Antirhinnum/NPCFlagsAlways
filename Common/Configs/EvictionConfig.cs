using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NPCFlagsAlways.Common.Configs;

public sealed class EvictionConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[DefaultValue(false)]
	[Label("$Mods.NPCFlagsAlways.Config.EvictionAllowed.Label")]
	[Tooltip("$Mods.NPCFlagsAlways.Config.EvictionAllowed.Tooltip")]
	public bool EvictionAllowed { get; set; }
}