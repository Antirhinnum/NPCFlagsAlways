using NPCFlagsAlways.Common.Hooks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways
{
	public class NPCFlagsAlways : Mod
	{
		private const int LastGamepadPage = 4010;

		public NPCFlagsAlways()
		{
		}

		public override void Load()
		{
			if (!Main.dedServ)
			{
				HookLoader.Load(this);

				UILinkPage builderAccPage = UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs];
				UILinkPoint point = new UILinkPoint(LastGamepadPage, enabled: true, GamepadPointID.EndLeft, GamepadPointID.Inventory40, GamepadPointID.EndUp, GamepadPointID.EndDown);
				point.SetPage(GamepadPageID.BuilderAccs);
				builderAccPage.LinkMap.Add(LastGamepadPage, point);
				UILinkPointNavigator.Points.Add(LastGamepadPage, point);
			}
		}

		public override void Unload()
		{
			if (!Main.dedServ)
			{
				HookLoader.Unload();

				UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs].LinkMap.Remove(LastGamepadPage);
				UILinkPointNavigator.Points.Remove(LastGamepadPage);
			}
		}
	}
}