using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NPCFlagsAlways.Common.Configs;
using NPCFlagsAlways.Common.Players;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCFlagsAlways.Common.Systems
{
	public class NPCHousingBannerSystem : ModSystem
	{
		private MethodInfo _DrawNPCHousesInWorld_Cached;
		private MethodInfo _GetBuilderAccsCountToShow_Cached;
		private Asset<Texture2D> _bannerIconAsset;

		/// <summary>
		/// The original condition used to determine whether NPC housing flags should be drawn. Used for brevity.
		/// </summary>
		private static bool OriginalFailCondition => Main.EquipPage != 1 && (!UILinkPointNavigator.Shortcuts.NPCS_IconsDisplay || !PlayerInput.UsingGamepad);


		public override void Load()
		{
			_bannerIconAsset = ModContent.Request<Texture2D>("NPCFlagsAlways/Assets/Textures/BannerIcon");
			_DrawNPCHousesInWorld_Cached = typeof(Main).GetMethod("DrawNPCHousesInWorld", BindingFlags.Instance | BindingFlags.NonPublic);
			_GetBuilderAccsCountToShow_Cached = typeof(Main).GetMethod("GetBuilderAccsCountToShow", BindingFlags.Static | BindingFlags.NonPublic);

			On.Terraria.Main.DrawBuilderAccToggles += DrawBannerVisibilityIcon;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners += ForceDrawHouseBanners;
			On.Terraria.Main.GetBuilderAccsCountToShow += MakeSpaceForBannerVisibilityIcon;
			On.Terraria.WorldGen.kickOut += PreventUnintentionalEvictions;

			UILinkPage builderAccPage = UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs];
			UILinkPoint point = new(6012, enabled: true, -3, 40, -1, -2);
			point.SetPage(GamepadPageID.BuilderAccs);
			builderAccPage.LinkMap.Add(6012, point);
			UILinkPointNavigator.Points.Add(6012, point);
		}

		public override void Unload()
		{
			On.Terraria.WorldGen.kickOut -= PreventUnintentionalEvictions;
			On.Terraria.Main.GetBuilderAccsCountToShow -= MakeSpaceForBannerVisibilityIcon;
			On.Terraria.Main.DrawInterface_7_TownNPCHouseBanners -= ForceDrawHouseBanners;
			On.Terraria.Main.DrawBuilderAccToggles -= DrawBannerVisibilityIcon;
			_GetBuilderAccsCountToShow_Cached = null;
			_DrawNPCHousesInWorld_Cached = null;
			_bannerIconAsset = null;

			UILinkPointNavigator.Pages[GamepadPageID.BuilderAccs].LinkMap.Remove(6012);
			UILinkPointNavigator.Points.Remove(6012);
		}

		private void DrawBannerVisibilityIcon(On.Terraria.Main.orig_DrawBuilderAccToggles orig, Main self, Vector2 start)
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

			ref BannerVisibility visibility = ref Main.LocalPlayer.GetModPlayer<BannerVisibilityPlayer>().bannerVisibility;
			Color disabledColor = new(127, 127, 127);

			Color drawColor = visibility switch
			{
				BannerVisibility.Bright => Color.White,
				BannerVisibility.Normal => disabledColor,
				BannerVisibility.Faded => disabledColor * 0.66f,
				BannerVisibility.Hidden => disabledColor * 0.33f,
				_ => Color.White
			};

			Texture2D texture = _bannerIconAsset.Value;
			Rectangle frame = texture.Frame(2, 1, frameX: 0);

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
					SoundEngine.PlaySound(22);
					visibility += 1;
					if ((int)visibility >= (int)BannerVisibility.Hidden)
					{
						visibility = BannerVisibility.Bright;
					}
				}
			}

			Vector2 drawPosition = new(0, drawPositionY);
			Main.spriteBatch.Draw(texture, drawPosition, frame, drawColor, 0f, default, 0.9f, SpriteEffects.None, 0f);
			if (mouseHovering)
			{
				Main.spriteBatch.Draw(texture, drawPosition, texture.Frame(2, 1, frameX: 1), Main.OurFavoriteColor, 0f, default, 0.9f, SpriteEffects.None, 0f);
			}

			UILinkPointNavigator.SetPosition(GamepadPointID.BuilderAccs + 2, drawPosition + (frame.Size() * 0.65f));
			UILinkPointNavigator.Shortcuts.BUILDERACCCOUNT += 1;
		}

		private void ForceDrawHouseBanners(On.Terraria.Main.orig_DrawInterface_7_TownNPCHouseBanners orig, Main self)
		{
			orig(self);

			// Only force drawing if the condition in DrawInterface_7 fails.
			if (OriginalFailCondition)
			{
				_DrawNPCHousesInWorld_Cached.Invoke(Main.instance, Array.Empty<object>());
			}
		}

		private void MakeSpaceForBannerVisibilityIcon(On.Terraria.Main.orig_GetBuilderAccsCountToShow orig, Player plr, out int blockReplaceIcons, out int torchGodIcons, out int totalDrawnIcons)
		{
			// Increase torchGodIcons so that the banner toggle goes above wiring toggles.
			orig(plr, out blockReplaceIcons, out torchGodIcons, out totalDrawnIcons);
			torchGodIcons += 1;
			//totalDrawnIcons += 1;
		}

		private void PreventUnintentionalEvictions(On.Terraria.WorldGen.orig_kickOut orig, int n)
		{
			// Only allow NPCs to be evicted if:
			//	You are the server (has no UI to click, unaffected by this mod), OR
			//	Flags should be drawn (housing menu open, proceed a normal), OR
			//	Eviction is always allowed
			if (Main.netMode == NetmodeID.Server || !OriginalFailCondition || ModContent.GetInstance<EvictionConfig>().EvictionAllowed)
			{
				orig(n);
			}
		}
	}
}
