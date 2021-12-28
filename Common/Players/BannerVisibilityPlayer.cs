using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NPCFlagsAlways.Common.Players
{
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

	public class BannerVisibilityPlayer : ModPlayer
	{
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
					Mod.Logger.WarnFormat(Language.GetTextValue("Mods.NPCFlagsAlways.Common.UnknownDataFormat"), Player.name, Mod.DisplayName, (int)version);
					bannerVisibility = BannerVisibility.Normal;
					break;
			}
		}
	}
}
