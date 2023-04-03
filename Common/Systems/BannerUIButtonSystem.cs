using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways.Common.Systems;

/// <summary>
/// Sets up the gamepad UI points for the added Banner Visibility button.
/// </summary>
public sealed class BannerUIButtonSystem : ModSystem
{
	private const int FirstUnusedVanillaBuilderAccsID = 6012;

	public override void Load()
	{
		UILinkPage builderAccPage = UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs];
		UILinkPoint point = new(FirstUnusedVanillaBuilderAccsID, enabled: true, GamepadPointID.EndLeft, GamepadPointID.Inventory40, GamepadPointID.EndUp, GamepadPointID.EndDown);
		point.SetPage(GamepadPageID.BuilderAccs);
		builderAccPage.LinkMap.Add(FirstUnusedVanillaBuilderAccsID, point);
		UILinkPointNavigator.Points.Add(FirstUnusedVanillaBuilderAccsID, point);
	}

	public override void Unload()
	{
		UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs].LinkMap.Remove(FirstUnusedVanillaBuilderAccsID);
		UILinkPointNavigator.Points.Remove(FirstUnusedVanillaBuilderAccsID);
	}
}