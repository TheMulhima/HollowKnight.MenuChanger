﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;

namespace MenuChanger
{
    /// <summary>
    /// Static class which manages the MenuChanger mode menu.
    /// </summary>
    public static class ModeMenu
    {
        /// <summary>
        /// The main entry point for adding to the mode menu. Stores the menu constructor to be invoked when the mode menu is built.
        /// </summary>
        public static void AddMode(ModeMenuConstructor constructor)
        {
            constructors.Add(constructor);
        }


        private static MenuPage ModePage;
        private static MultiGridItemPanel ModeButtonPanel;

        private static List<ModeMenuConstructor> constructors = new()
        {
            new DefaultModeConstructor(Mode.Classic),
            new DefaultModeConstructor(Mode.Steel),
            new DefaultModeConstructor(Mode.Godmaster),
        };

        internal static void OnEnterMainMenu()
        {
            MenuChangerMod.instance.Log("Constructing mode menu...");
            ModePage = new MenuPage("Mode Page");
            List<BigButton> buttons = new();

            for (int i = 0; i < constructors.Count; i++)
            {
                try
                {
                    constructors[i].OnEnterMainMenu(ModePage);
                    if (constructors[i].TryGetModeButton(ModePage, out BigButton button))
                    {
                        buttons.Add(button);
                    }
                    MenuChangerMod.instance.Log($"  Constructed menu from {constructors[i].GetType().FullName}");
                }
                catch (Exception e)
                {
                    MenuChangerMod.instance.LogError($"Error during OnEnterMainMenu for constructor of type {constructors[i].GetType().FullName}:\n{e}");
                    constructors.RemoveAt(i--);
                    continue;
                }
            }
            ModeButtonPanel = new MultiGridItemPanel(ModePage, 5, 3, 150f, 650f, new Vector2(0, 300), buttons.ToArray());
        }

        internal static void Show()
        {
            ModePage.Show();
        }

        internal static void OnExitMainMenu()
        {
            ModePage = null;
            ModeButtonPanel = null;
            foreach (var c in constructors)
            {
                try
                {
                    c.OnExitMainMenu();
                }
                catch (Exception e)
                {
                    MenuChangerMod.instance.LogError($"Error during OnEnterMainMenu for constructor of type {c.GetType().Name}:\n{e}");
                } 
            }
        }

        private class DefaultModeConstructor : ModeMenuConstructor
        {
            readonly Mode mode;
            public DefaultModeConstructor(Mode mode)
            {
                this.mode = mode;
            }

            private BigButton button;
            private bool wasUnlocked;

            public override void OnEnterMainMenu(MenuPage modeMenu) 
            {
                wasUnlocked = IsUnlocked;
                if (!wasUnlocked) return;

                button = new BigButton(modeMenu, mode);
                button.OnClick += () =>
                {
                    MenuChangerMod.HideAllMenuPages();
                    UIManager.instance.StartNewGame(permaDeath: mode == Mode.Steel, bossRush: mode == Mode.Godmaster);
                };
            }
            public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
            {
                if (wasUnlocked)
                {
                    button = this.button;
                    return true;
                }
                else
                {
                    button = null;
                    return false;
                }
            }
            public override void OnExitMainMenu()
            {
                button = null;
                wasUnlocked = false;
            }

            public bool IsUnlocked => mode switch
            {
                Mode.Classic => true,
                Mode.Steel => GameManager.instance.GetStatusRecordInt("RecPermadeathMode") == 1,
                Mode.Godmaster => GameManager.instance.GetStatusRecordInt("RecBossRushMode") == 1,
                _ => true,
            };
        }
    }
}
