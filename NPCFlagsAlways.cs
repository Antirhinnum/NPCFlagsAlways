using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using NPCFlagsAlways.Common.Configs;
using NPCFlagsAlways.Common.Players;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways
{
	public class NPCFlagsAlways : Mod
	{
		private MethodInfo _DrawNPCHouse_Cached;

		private Texture2D _bannerIconAsset;

		/// <summary>
		/// The original condition used to determine whether NPC housing flags should be drawn. Used for brevity.
		/// </summary>
		private static bool OriginalFailCondition => Main.EquipPage != 1 && !UILinkPointNavigator.Shortcuts.NPCS_IconsDisplay;

		public NPCFlagsAlways()
		{
		}

		public override void Load()
		{
			_bannerIconAsset = ModContent.GetTexture("NPCFlagsAlways/Assets/Textures/BannerIcon");
			_DrawNPCHouse_Cached = typeof(Main).GetMethod("DrawNPCHouse", BindingFlags.Instance | BindingFlags.NonPublic);

			On.Terraria.Main.DrawBuilderAccToggles += DrawBannerVisibilityIcon;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners += ForceDrawHouseBanners;
			On.Terraria.WorldGen.kickOut += PreventUnintentionalEvictions;
			IL.Terraria.Main.DrawNPCHouse += NPCBannerDrawColor;

			UILinkPage builderAccPage = UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs];
			UILinkPoint point = new UILinkPoint(4010, enabled: true, -3, 40, -1, -2);
			point.SetPage(GamepadPageID.BuilderAccs);
			builderAccPage.LinkMap.Add(4010, point);
			UILinkPointNavigator.Points.Add(4010, point);
		}

		public override void Unload()
		{
			IL.Terraria.Main.DrawNPCHouse -= NPCBannerDrawColor;
			On.Terraria.WorldGen.kickOut -= PreventUnintentionalEvictions;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners -= ForceDrawHouseBanners;
			On.Terraria.Main.DrawBuilderAccToggles -= DrawBannerVisibilityIcon;
			_DrawNPCHouse_Cached = null;
			_bannerIconAsset = null;

			UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs].LinkMap.Remove(4010);
			UILinkPointNavigator.Points.Remove(4010);
		}

		private void DrawBannerVisibilityIcon(On.Terraria.Main.orig_DrawBuilderAccToggles orig, Main self, Vector2 start)
		{
			orig(self, start);

			if (!Main.playerInventory || Main.LocalPlayer.sign >= 0)
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
			if (Main.mouseX > drawPosition.X && Main.mouseX < drawPosition.X + frame.Width && Main.mouseY > drawPosition.Y && Main.mouseY < drawPosition.Y + frame.Height)
			{
				mouseHovering = true;
				Main.LocalPlayer.mouseInterface = true;

				string bannerVisibility = Language.GetTextValue($"GameUI.{visibility}");
				self.MouseText($"{Language.GetTextValue("Mods.NPCFlagsAlways.Common.NPCBanner")}: {bannerVisibility}", 0, 0);
				Main.mouseText = true;

				if (Main.mouseLeft && Main.mouseLeftRelease)
				{
					Main.PlaySound(SoundID.MenuTick);
					visibility += 1;
					if ((int)visibility > (int)BannerVisibility.Hidden)
					{
						visibility = BannerVisibility.Bright;
					}
				}
			}

			Main.spriteBatch.Draw(texture, drawPosition, frame, drawColor, 0f, frame.Size() / 2f, 0.9f, SpriteEffects.None, 0f);
			if (mouseHovering)
			{
				Main.spriteBatch.Draw(texture, drawPosition, texture.Frame(3, 1, frameX: 2), Main.OurFavoriteColor, 0f, frame.Size() / 2f, 0.9f, SpriteEffects.None, 0f);
			}

			UILinkPointNavigator.SetPosition(GamepadPointID.BuilderAccs + UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT, drawPosition + (frame.Size() * 0.65f));
			UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT += 1;
		}

		private void ForceDrawHouseBanners(On.Terraria.Main.orig_DrawInterface_7_TownNPCHouseBanners orig, Main self)
		{
			orig(self);

			// Only force drawing if the condition in DrawInterface_7 fails and banners aren't hidden.
			if (OriginalFailCondition && Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility != BannerVisibility.Hidden)
			{
				_DrawNPCHouse_Cached.Invoke(Main.instance, new object[] { });
			}
		}

		private void PreventUnintentionalEvictions(On.Terraria.WorldGen.orig_kickOut orig, int n)
		{
			// Only allow NPCs to be evicted if:
			//	You are the server (has no UI to click, unaffected by this mod), OR
			//	Flags should be drawn (housing menu open, proceed a normal), OR
			//	Eviction is always allowed and banners are being drawn.
			if (Main.netMode == NetmodeID.Server || !OriginalFailCondition || (ModContent.GetInstance<EvictionConfig>().EvictionAllowed && Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility != BannerVisibility.Hidden))
			{
				orig(n);
			}
		}

		/// <summary>
		/// Replaces the two Lighting.GetColor calls in Main.DrawNPCHousesInWorld() with a custom color function.
		/// </summary>
		/// <param name="il"></param>
		private void NPCBannerDrawColor(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			// Replace both calls: one for the banner, one for the NPC head.
			for (int k = 0; k < 2; k++)
			{
				if (!cursor.TryGotoNext(MoveType.Before,
					i => i.MatchCall<Lighting>(nameof(Lighting.GetColor))))
				{
					throw new Exception(Language.GetText("Mods.NPCFlagsAlways.Common.ILPatchFail").FormatWith(Language.GetTextValue("Mods.NPCFlagsAlways.Common.LightingILFail") + $" {k + 1}"));
				}

				cursor.Remove();
				cursor.EmitDelegate<Func<int, int, Color>>(GetNPCBannerColor);
			}
		}

		private Color GetNPCBannerColor(int x, int y)
		{
			BannerVisibility visibility = Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility;
			Color baseColor = Lighting.GetColor(x, y);
			switch (visibility)
			{
				case BannerVisibility.Bright:
					return Color.White;

				case BannerVisibility.Faded:
					return baseColor * 0.5f;

				case BannerVisibility.Hidden:
					return OriginalFailCondition ? Color.Transparent : baseColor;

				default:
					return baseColor;
			}
		}
	}
}