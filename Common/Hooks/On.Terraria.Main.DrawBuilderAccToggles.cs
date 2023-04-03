using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NPCFlagsAlways.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways.Common.Hooks;

internal sealed partial class HookLoader : ILoadable
{
	/// <summary>
	/// Draws the new toggle button.
	/// </summary>
	private void Main_DrawBuilderAccToggles(On_Main.orig_DrawBuilderAccToggles orig, Main self, Vector2 start)
	{
		orig(self, start);

		if (!Main.playerInventory || Main.LocalPlayer.sign >= 0)
		{
			return;
		}

		object[] args = new object[] { Main.LocalPlayer, 0, 0, 0 }; // plr, out int blockReplaceIcons, out int torchGodIcons, out int totalDrawnIcons
		_GetBuilderAccsCountToShow_Cached.Invoke(null, args);
		bool pushSideToolsUp = (int)args[3] >= 10; // totalDrawnIcons >= 10

		int drawPositionY = 92;
		if (pushSideToolsUp)
		{
			drawPositionY = 71;
		}

		if (!Main.LocalPlayer.unlockedBiomeTorches)
		{
			drawPositionY -= 24;
		}

		ref BannerVisibility visibility = ref Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility;
		Color disabledColor = new(127, 127, 127);

		Color drawColor = visibility switch
		{
			BannerVisibility.Normal => disabledColor,
			BannerVisibility.Faded => disabledColor * 0.66f,
			_ => Color.White
		};

		Texture2D texture = _bannerIconAsset.Value;
		Rectangle frame = texture.Frame(3, 1, frameX: visibility == BannerVisibility.Hidden ? 1 : 0);

		bool mouseHovering = false;
		if (Main.mouseX > 0 && Main.mouseX < frame.Width && Main.mouseY > drawPositionY && Main.mouseY < drawPositionY + frame.Height)
		{
			mouseHovering = true;
			Main.LocalPlayer.mouseInterface = true;

			string bannerVisibility = Language.GetTextValue($"GameUI.{visibility}");
			self.MouseText($"{Language.GetTextValue("Mods.NPCFlagsAlways.Common.NPCBanner")}: {bannerVisibility}", 0, 0);
			Main.mouseText = true;

			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				SoundEngine.PlaySound(SoundID.Unlock);
				visibility = visibility.NextEnum();
			}
		}

		Vector2 drawPosition = new(0, drawPositionY);
		Main.spriteBatch.Draw(texture, drawPosition, frame, drawColor, 0f, default, 0.9f, SpriteEffects.None, 0f);
		if (mouseHovering)
		{
			Main.spriteBatch.Draw(texture, drawPosition, texture.Frame(3, 1, frameX: 2), Main.OurFavoriteColor, 0f, default, 0.9f, SpriteEffects.None, 0f);
		}

		UILinkPointNavigator.SetPosition(GamepadPointID.BuilderAccs + 2, drawPosition + (frame.Size() * 0.65f));
		UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT += 1;
	}
}