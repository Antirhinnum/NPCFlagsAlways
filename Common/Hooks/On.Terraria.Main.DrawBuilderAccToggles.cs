using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NPCFlagsAlways.Common.Players;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways.Common.Hooks
{
	internal partial class HookLoader
	{
		private static void Main_DrawBuilderAccToggles(On.Terraria.Main.orig_DrawBuilderAccToggles orig, Main self, Vector2 start)
		{
			orig(self, start);

			if ((!Main.playerInventory && !string.IsNullOrEmpty(Main.npcChatText)) || Main.player[Main.myPlayer].sign >= 0)
			{
				return;
			}

			Vector2 drawPosition = start + new Vector2(0f, UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT * 24);
			if (UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT > 8)
			{
				drawPosition.Y -= 44f;
			}

			ref BannerVisibility visibility = ref Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility;
			Color disabledColor = new Color(127, 127, 127);

			Color drawColor = Color.White;
			switch (visibility)
			{
				case BannerVisibility.Normal:
					drawColor = disabledColor;
					break;

				case BannerVisibility.Faded:
					drawColor = disabledColor * 0.66f;
					break;
			}

			Texture2D texture = _bannerIconAsset;
			Rectangle frame = texture.Frame(3, 1, frameX: visibility == BannerVisibility.Hidden ? 1 : 0);

			bool mouseHovering = false;
			if (Utils.CenteredRectangle(drawPosition, frame.Size()).Contains(Main.MouseScreen.ToPoint()) && !PlayerInput.IgnoreMouseInterface)
			{
				mouseHovering = true;
				Main.LocalPlayer.mouseInterface = true;

				string bannerVisibility = Language.GetTextValue($"GameUI.{visibility}");
				self.MouseText($"{Language.GetTextValue("Mods.NPCFlagsAlways.Common.NPCBanner")}: {bannerVisibility}", 0, 0);
				Main.mouseText = true;

				if (Main.mouseLeft && Main.mouseLeftRelease)
				{
					Main.PlaySound(SoundID.MenuTick);
					visibility = visibility.NextEnum();
				}
			}

			Main.spriteBatch.Draw(texture, drawPosition, frame, drawColor, 0f, frame.Size() / 2f, 1f, SpriteEffects.None, 0f);
			if (mouseHovering)
			{
				Main.spriteBatch.Draw(texture, drawPosition, texture.Frame(3, 1, frameX: 2), Main.OurFavoriteColor, 0f, frame.Size() / 2f, 1f, SpriteEffects.None, 0f);
			}

			UILinkPointNavigator.SetPosition(GamepadPointID.BuilderAccs + UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT, drawPosition + (frame.Size() * 0.3f));
			UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT += 1;
		}
	}
}