using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NPCFlagsAlways.Common.Players;

public enum BannerVisibility
{
	Bright,
	Normal,
	Faded,
	Hidden
}

internal enum SaveDataVersion
{
	OnlyVisibility
}

public sealed class BannerVisibilityPlayer : ModPlayer
{
	private const string UnknownDataFormatWarning = "Player {0} tried to load data for {1}, but the format wasn't recognized. Format ID: {2}";

	public BannerVisibility bannerVisibility;

	public override void Initialize()
	{
		bannerVisibility = BannerVisibility.Normal;
	}

	public override void SaveData(TagCompound tag)
	{
		SaveDataVersion version = SaveDataVersion.OnlyVisibility;
		tag.Add(nameof(version), (int)version);

		tag.Add(nameof(bannerVisibility), (int)bannerVisibility);
	}

	public override void LoadData(TagCompound tag)
	{
		SaveDataVersion version = (SaveDataVersion)tag.Get<int>(nameof(version));
		switch (version)
		{
			case SaveDataVersion.OnlyVisibility:
				bannerVisibility = (BannerVisibility)tag.Get<int>(nameof(bannerVisibility));
				break;

			default:
				Mod.Logger.WarnFormat(UnknownDataFormatWarning, Player.name, Mod.DisplayName, (int)version);
				bannerVisibility = BannerVisibility.Normal;
				break;
		}
	}
}