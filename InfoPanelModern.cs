using System;
using System.Collections.Generic;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("InfoPanelModern", "Fadir Stave", "3.0.0")]
    [Description("Smooth parchment info panel with clean layout.")]
    public class InfoPanelModern : RustPlugin
    {
        const string UI = "InfoPanelModernUI";

        HashSet<ulong> open = new HashSet<ulong>();
        PluginConfig config;

        class PluginConfig
        {
            public string Title = "HEAR YE, HEAR YE";

            public string Welcome =
                "Welcome to The Commonwealth!\nA PVE server run by an active admin and father of three!";

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
                "/help or /info – Open this panel",
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
            config = Config.ReadObject<PluginConfig>();
        }

        protected override void SaveConfig() => Config.WriteObject(config, true);

        [ChatCommand("info")]
        void CmdInfo(BasePlayer player)
        {
            if (open.Contains(player.userID))
            {
                Close(player);
                return;
            }

            Open(player);
        }

        void Open(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI);
            var c = new CuiElementContainer();

            c.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.6" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                CursorEnabled = true
            }, "Overlay", UI);

            c.Add(new CuiPanel
            {
                Image = { Color = Hex("#E6D3A3", .98f) },
                RectTransform = { AnchorMin = "0.32 0.05", AnchorMax = "0.68 0.95" }
            }, UI, UI + ".panel");

            // HEADER
            c.Add(new CuiPanel
            {
                Image = { Color = Hex("#9A6A55", .95f) },
                RectTransform = { AnchorMin = "0.06 0.93", AnchorMax = "0.94 0.98" }
            }, UI + ".panel");

            c.Add(new CuiLabel
            {
                Text =
                {
                    Text = config.Title,
                    FontSize = 24,
                    Align = TextAnchor.MiddleCenter,
                    Color = Hex("#2E2E2E",1)
                },
                RectTransform = { AnchorMin = "0.1 0.93", AnchorMax = "0.9 0.98" }
            }, UI + ".panel");

            c.Add(new CuiButton
            {
                Button = { Command = "infopanelmodern.close", Color = Hex("#8D614F",1) },
                RectTransform = { AnchorMin = "0.9 0.935", AnchorMax = "0.94 0.975" },
                Text = { Text = "X", FontSize = 16, Align = TextAnchor.MiddleCenter }
            }, UI + ".panel");

            // WELCOME
            c.Add(new CuiLabel
            {
                Text =
                {
                    Text = config.Welcome,
                    FontSize = 14,
                    Align = TextAnchor.UpperLeft,
                    Color = Hex("#2E2E2E",1)
                },
                RectTransform = { AnchorMin = "0.1 0.86", AnchorMax = "0.9 0.91" }
            }, UI + ".panel");

            string scroll = UI + ".scroll";

            c.Add(new CuiElement
            {
                Name = scroll,
                Parent = UI + ".panel",
                Components =
                {
                    new CuiScrollViewComponent
                    {
                        Vertical = true,
                        ScrollSensitivity = 35f,
                        ContentTransform = new CuiRectTransform
                        {
                            AnchorMin="0 0",
                            AnchorMax="1 3"
                        }
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin="0.08 0.08",
                        AnchorMax="0.92 0.83"
                    }
                }
            });

            float y = 0.95f;

            AddSection(c, scroll, ref y, "WHAT WE OFFER", config.WhatWeOffer);
            AddSection(c, scroll, ref y, "LAWS OF THE LAND", config.Laws);
            AddSection(c, scroll, ref y, "DISCORD", config.Discord);
            AddSection(c, scroll, ref y, "WIPE SCHEDULE", config.Wipe);
            AddSection(c, scroll, ref y, "COMMANDS", config.Commands);
            AddSection(c, scroll, ref y, "MORE COMMANDS", config.More);

            CuiHelper.AddUi(player, c);
            open.Add(player.userID);
        }

        void AddSection(CuiElementContainer c, string parent, ref float y, string title, List<string> lines)
        {
            float headerHeight = 0.045f;

            c.Add(new CuiPanel
            {
                Image = { Color = Hex("#9A6A55", .9f) },
                RectTransform = { AnchorMin = $"0.05 {y-headerHeight}", AnchorMax = $"0.95 {y}" }
            }, parent);

            c.Add(new CuiLabel
            {
                Text = { Text = title, FontSize = 18, Align = TextAnchor.MiddleLeft, Color = Hex("#2E2E2E",1) },
                RectTransform = { AnchorMin = $"0.08 {y-headerHeight}", AnchorMax = $"0.92 {y}" }
            }, parent);

            y -= headerHeight + 0.01f;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    y -= 0.02f;
                    continue;
                }

                c.Add(new CuiLabel
                {
                    Text =
                    {
                        Text = "• " + line,
                        FontSize = 14,
                        Align = TextAnchor.MiddleLeft,
                        Color = Hex("#2E2E2E",1)
                    },
                    RectTransform =
                    {
                        AnchorMin = $"0.1 {y-0.025f}",
                        AnchorMax = $"0.9 {y}"
                    }
                }, parent);

                y -= 0.028f;
            }

            y -= 0.04f;
        }

        void Close(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI);
            open.Remove(player.userID);
        }

        string Hex(string hex, float alpha)
        {
            Color c;
            ColorUtility.TryParseHtmlString(hex, out c);
            return $"{c.r} {c.g} {c.b} {alpha}";
        }
    }
}