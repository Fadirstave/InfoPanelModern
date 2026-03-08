using System;
using System.Collections.Generic;
using System.Globalization;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("InfoPanelModern", "Fadir Stave", "3.4.0")]
    [Description("Smooth info panel with polished scrolling and configurable background image.")]
    public class InfoPanelModern : RustPlugin
    {
        private const string UI = "InfoPanelModernUI";
        private const string Panel = UI + ".panel";
        private const string ScrollRegion = UI + ".scroll.region";
        private const string Scroll = UI + ".scroll";

        private readonly HashSet<ulong> open = new HashSet<ulong>();
        private HashSet<ulong> shownPlayers = new HashSet<ulong>();
        private PluginConfig config;

        private class SectionData
        {
            public string Title;
            public List<string> Lines;

            public SectionData(string title, List<string> lines)
            {
                Title = title;
                Lines = lines ?? new List<string>();
            }
        }

        private class PluginConfig
        {
            public string Title = "HEAR YE, HEAR YE";

            public string Welcome =
                "Welcome to The Commonwealth!\nA PVE server run by an active admin and father of three!";

            public string BackgroundImageUrl = "";
            public float BackgroundImageAlpha = 1f;
            public string BackgroundTintColor = "0.12 0.10 0.08 0.58";
            public string TitleTextColor = "0.18 0.18 0.18 1";
            public string WelcomeTextColor = "0.18 0.18 0.18 1";
            public string SectionHeaderTextColor = "0.18 0.18 0.18 1";
            public string MenuTextColor = "0.18 0.18 0.18 1";

            public List<string> WhatWeOffer = new List<string>
            {
                "QUESTING! Make your way to the Commonwealth Outpost and seek the Herald within the Admin Shop.",
                "Handcrafted 4K medieval map",
                "Admin shop & custom vendors",
                "No upkeep / No decay",
                "Easter eggs around the land and hidden rare loot"
            };

            public List<string> Laws = new List<string>
            {
                "Be respectful – no toxicity or harassment",
                "Turrets & traps must stay within your base",
                "No griefing or blocking access",
                "Roleplay is optional but encouraged"
            };

            public List<string> Discord = new List<string>
            {
                "Join the Commonwealth Discord",
                "discord.gg/XrCh3KVvmM"
            };

            public List<string> Wipe = new List<string>
            {
                "Force wipe on the first Thursday of each month",
                "New handcrafted map every wipe",
                "Blueprints do not wipe unless forced by Facepunch"
            };

            public List<string> Commands = new List<string>
            {
                "/help or /Info – Open this panel",
                "!info !help !pop !ping – Legacy commands",
                "/quest – Track quests",
                "/title – Manage earned titles",
                "/hud – Toggle the top-right HUD",
                "/remove – Pick up items you placed"
            };

            public List<string> More = new List<string>
            {
                "/pve toggle – Share deployables",
                "/horse unclaim – Release your horse"
            };
        }

        protected override void LoadDefaultConfig()
        {
            config = new PluginConfig();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<PluginConfig>() ?? new PluginConfig();
            EnsureConfigDefaults();
        }

        protected override void SaveConfig() => Config.WriteObject(config, true);

        private void EnsureConfigDefaults()
        {
            if (config.WhatWeOffer == null) config.WhatWeOffer = new List<string>();
            if (config.Laws == null) config.Laws = new List<string>();
            if (config.Discord == null) config.Discord = new List<string>();
            if (config.Wipe == null) config.Wipe = new List<string>();
            if (config.Commands == null) config.Commands = new List<string>();
            if (config.More == null) config.More = new List<string>();

            if (config.BackgroundTintColor == null)
                config.BackgroundTintColor = "0.12 0.10 0.08 0.58";
            if (config.BackgroundImageUrl == null)
                config.BackgroundImageUrl = "";
            if (string.IsNullOrWhiteSpace(config.TitleTextColor))
                config.TitleTextColor = "0.18 0.18 0.18 1";
            if (string.IsNullOrWhiteSpace(config.WelcomeTextColor))
                config.WelcomeTextColor = "0.18 0.18 0.18 1";
            if (string.IsNullOrWhiteSpace(config.SectionHeaderTextColor))
                config.SectionHeaderTextColor = "0.18 0.18 0.18 1";
            if (string.IsNullOrWhiteSpace(config.MenuTextColor))
                config.MenuTextColor = "0.18 0.18 0.18 1";

            config.BackgroundImageAlpha = Mathf.Clamp01(config.BackgroundImageAlpha);
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || shownPlayers.Contains(player.userID))
                return;

            shownPlayers.Add(player.userID);

            timer.Once(3f, () =>
            {
                if (player != null && player.IsConnected)
                    Draw(player);
            });
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player != null)
                shownPlayers.Remove(player.userID);
        }

        [ChatCommand("Info")]
        private void CmdInfo(BasePlayer player)
        {
            if (open.Contains(player.userID))
            {
                Close(player);
                return;
            }

            Draw(player);
        }

        [ConsoleCommand("infopanelmodern.close")]
        private void CmdClose(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null)
                return;

            Close(player);
        }

        private void Draw(BasePlayer player)
        {
            Open(player);
        }

        private void Open(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI);

            var sections = BuildSections();
            float contentHeight = CalculateRequiredContentHeightPx(sections);
            var c = new CuiElementContainer();

            c.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.62" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                CursorEnabled = true
            }, "Overlay", UI);

            c.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0" },
                RectTransform = { AnchorMin = "0.29 0.05", AnchorMax = "0.71 0.95" }
            }, UI, Panel);

            if (!string.IsNullOrWhiteSpace(config.BackgroundImageUrl))
            {
                c.Add(new CuiElement
                {
                    Parent = Panel,
                    Components =
                    {
                        new CuiRawImageComponent
                        {
                            Url = config.BackgroundImageUrl,
                            Color = $"1 1 1 {config.BackgroundImageAlpha}"
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        }
                    }
                });
            }

            c.Add(new CuiPanel
            {
                Image = { Color = config.BackgroundTintColor },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, Panel);

            c.Add(new CuiPanel
            {
                Image = { Color = Hex("#9A6A55", 0.95f) },
                RectTransform = { AnchorMin = "0.06 0.93", AnchorMax = "0.94 0.98" }
            }, Panel);

            c.Add(new CuiLabel
            {
                Text =
                {
                    Text = config.Title,
                    FontSize = 22,
                    Align = TextAnchor.MiddleCenter,
                    Color = config.TitleTextColor
                },
                RectTransform = { AnchorMin = "0.1 0.93", AnchorMax = "0.9 0.98" }
            }, Panel);

            c.Add(new CuiButton
            {
                Button = { Command = "infopanelmodern.close", Color = Hex("#6E4839", 1f), Close = UI },
                RectTransform = { AnchorMin = "0.875 0.935", AnchorMax = "0.925 0.975" },
                Text = { Text = "✕", FontSize = 17, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, Panel);

            c.Add(new CuiLabel
            {
                Text =
                {
                    Text = config.Welcome,
                    FontSize = 14,
                    Align = TextAnchor.MiddleCenter,
                    Color = config.WelcomeTextColor
                },
                RectTransform = { AnchorMin = "0.09 0.85", AnchorMax = "0.91 0.91" }
            }, Panel);

            c.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.10" },
                RectTransform = { AnchorMin = "0.08 0.07", AnchorMax = "0.93 0.83" }
            }, Panel, ScrollRegion);

            c.Add(new CuiElement
            {
                Name = Scroll,
                Parent = ScrollRegion,
                Components =
                {
                    new CuiImageComponent
                    {
                        Color = "1 1 1 0.01"
                    },
                    new CuiScrollViewComponent
                    {
                        Vertical = true,
                        Horizontal = false,
                        Inertia = false,
                        Elasticity = 0f,
                        ScrollSensitivity = 30f,
                        ContentTransform = new CuiRectTransform
                        {
                            AnchorMin = "0 1",
                            AnchorMax = "1 1",
                            OffsetMin = $"0 -{F(contentHeight)}",
                            OffsetMax = "0 0"
                        },
                        VerticalScrollbar = new CuiScrollbar
                        {
                            AutoHide = false,
                            Size = 7f,
                            TrackColor = "0.2 0.17 0.13 0.25",
                            HandleColor = "0.54 0.35 0.24 0.95"
                        }
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = "0 0",
                        AnchorMax = "1 1"
                    }
                }
            });

            float top = 8f;
            foreach (var section in sections)
                AddSection(c, Scroll, ref top, section.Title, section.Lines);

            CuiHelper.AddUi(player, c);
            open.Add(player.userID);
        }

        private List<SectionData> BuildSections()
        {
            return new List<SectionData>
            {
                new SectionData("WHAT WE OFFER", config.WhatWeOffer),
                new SectionData("LAWS OF THE LAND", config.Laws),
                new SectionData("DISCORD", config.Discord),
                new SectionData("WIPE SCHEDULE", config.Wipe),
                new SectionData("COMMANDS", config.Commands),
                new SectionData("MORE COMMANDS", config.More)
            };
        }

        private void AddSection(CuiElementContainer c, string parent, ref float top, string title, List<string> lines)
        {
            const float headerHeight = 38f;
            const float headerSpacing = 8f;
            const float emptyLineHeight = 14f;
            const float sectionBottomGap = 14f;
            const float lineSpacing = 3f;

            AddTopPanel(c, parent, top, headerHeight, 14f, 14f, Hex("#9A6A55", 0.9f));
            AddTopLabel(c, parent, title, top, headerHeight, 24f, 24f, 17, config.SectionHeaderTextColor, TextAnchor.MiddleCenter);
            top += headerHeight + headerSpacing;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    top += emptyLineHeight;
                    continue;
                }

                int rows;
                var wrapped = WrapLineForDisplay(line, out rows);
                float lineHeight = EstimateWrappedLineHeight(rows);

                AddTopLabel(c, parent, "• " + wrapped, top, lineHeight, 32f, 26f, 13, config.MenuTextColor, TextAnchor.UpperLeft);
                top += lineHeight + lineSpacing;
            }

            top += sectionBottomGap;
        }

        private void AddTopPanel(CuiElementContainer c, string parent, float top, float height, float left, float right, string color)
        {
            c.Add(new CuiPanel
            {
                Image = { Color = color },
                RectTransform =
                {
                    AnchorMin = "0 1",
                    AnchorMax = "1 1",
                    OffsetMin = $"{F(left)} {F(-(top + height))}",
                    OffsetMax = $"-{F(right)} {F(-top)}"
                }
            }, parent);
        }

        private void AddTopLabel(CuiElementContainer c, string parent, string text, float top, float height, float left, float right, int fontSize, string color, TextAnchor align)
        {
            c.Add(new CuiLabel
            {
                Text =
                {
                    Text = text,
                    FontSize = fontSize,
                    Align = align,
                    Color = color
                },
                RectTransform =
                {
                    AnchorMin = "0 1",
                    AnchorMax = "1 1",
                    OffsetMin = $"{F(left)} {F(-(top + height))}",
                    OffsetMax = $"-{F(right)} {F(-top)}"
                }
            }, parent);
        }

        private string WrapLineForDisplay(string line, out int rows)
        {
            const int approximateCharsPerRow = 62;

            if (string.IsNullOrEmpty(line) || line.Length <= approximateCharsPerRow)
            {
                rows = 1;
                return line;
            }

            var words = line.Split(' ');
            var current = string.Empty;
            var wrapped = new List<string>();

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(current))
                {
                    current = word;
                    continue;
                }

                if ((current.Length + 1 + word.Length) <= approximateCharsPerRow)
                {
                    current += " " + word;
                    continue;
                }

                wrapped.Add(current);
                current = word;
            }

            if (!string.IsNullOrEmpty(current))
                wrapped.Add(current);

            rows = Mathf.Max(1, wrapped.Count);
            return string.Join("\n", wrapped.ToArray());
        }

        private float EstimateWrappedLineHeight(int wrappedRows)
        {
            const float singleRowHeight = 20f;
            const float extraRowHeight = 16f;
            return singleRowHeight + ((Mathf.Max(1, wrappedRows) - 1) * extraRowHeight);
        }

        private float CalculateRequiredContentHeightPx(List<SectionData> sections)
        {
            const float startTopPadding = 8f;
            const float headerAndSpacing = 46f;
            const float emptyLineHeight = 14f;
            const float sectionBottomGap = 14f;
            const float lineSpacing = 3f;
            const float bottomPadding = 8f;

            float top = startTopPadding;

            foreach (var section in sections)
            {
                top += headerAndSpacing;

                foreach (var line in section.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        top += emptyLineHeight;
                        continue;
                    }

                    int rows;
                    WrapLineForDisplay(line, out rows);
                    top += EstimateWrappedLineHeight(rows) + lineSpacing;
                }

                top += sectionBottomGap;
            }

            return top + bottomPadding;
        }

        private void Close(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI);
            open.Remove(player.userID);
        }

        private string Hex(string hex, float alpha)
        {
            Color c;
            ColorUtility.TryParseHtmlString(hex, out c);
            return $"{c.r} {c.g} {c.b} {alpha}";
        }

        private string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}