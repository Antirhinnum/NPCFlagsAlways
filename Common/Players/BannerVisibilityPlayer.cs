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

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			SaveDataVersion version = SaveDataVersion.OnlyVisibility;
			tag.Add(nameof(version), (int)version);

			tag.Add(nameof(bannerVisibility), (int)bannerVisibility);
			return tag;
		}

		public override void Load(TagCompound tag)
		{
			SaveDataVersion version = (SaveDataVersion)tag.Get<int>(nameof(version));
			switch (version)
			{
				case SaveDataVersion.OnlyVisibility:
					bannerVisibility = (BannerVisibility)tag.Get<int>(nameof(bannerVisibility));
					break;

				default:
					mod.Logger.WarnFormat(Language.GetTextValue("Mods.NPCFlagsAlways.Common.UnknownDataFormat"), player.name, mod.DisplayName, (int)version);
					bannerVisibility = BannerVisibility.Normal;
					break;
			}
		}
	}
}