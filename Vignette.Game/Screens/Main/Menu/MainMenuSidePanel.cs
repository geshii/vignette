// Copyright 2020 - 2021 Vignette Project
// Licensed under NPOSLv3. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using Vignette.Game.Graphics.Typesets;
using Vignette.Game.Graphics.UserInterface;
using Vignette.Game.Screens.Main.Menu.Home;
using Vignette.Game.Screens.Main.Menu.Settings;

namespace Vignette.Game.Screens.Main.Menu
{
    public class MainMenuSidePanel : Container, IStateful<NavigationPanelState>
    {
        public Action OnScene = null;

        public MenuPage SelectedTab { get; private set; }

        public Action<MenuPage> OnTabSelect;

        private readonly MainMenuNavigationView mainNavigation;

        private readonly MainMenuNavigationView bottomNavigation;

        private IEnumerable<MenuPage> tabs => mainNavigation.Items.Concat(bottomNavigation.Items);

        public event Action<NavigationPanelState> StateChanged;

        private NavigationPanelState state = NavigationPanelState.Expanded;

        public NavigationPanelState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;

                Schedule(() => this.ResizeWidthTo(State == NavigationPanelState.Contracted ? 44 : 200, 200, Easing.OutQuint));
                StateChanged?.Invoke(State);
            }
        }

        public MainMenuSidePanel()
        {
            Width = 200;
            RelativeSizeAxes = Axes.Y;
            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                },
                Content = new Drawable[][]
                {
                    new Drawable[]
                    {
                        new FluentButton
                        {
                            Size = new Vector2(44),
                            Icon = FluentSystemIcons.EyeShow24,
                            Style = ButtonStyle.Text,
                            Action = () => OnScene?.Invoke(),
                        },
                    },
                    new Drawable[]
                    {
                        new FluentButton
                        {
                            Size = new Vector2(44),
                            Icon = FluentSystemIcons.Navigation20,
                            Style = ButtonStyle.Text,
                            Action = Toggle,
                        },
                    },
                    new Drawable[]
                    {
                        mainNavigation = new MainMenuNavigationView
                        {
                            Items = new MenuPage[]
                            {
                                new HomePage(),
                                new SceneSettings(),
                                new TrackerSettings(),
                                new KeybindSettings(),
                            },
                        }
                    },
                    new Drawable[]
                    {
                        bottomNavigation = new MainMenuNavigationView
                        {
                            SelectFirstTabByDefault = false,
                            Items = new MenuPage[]
                            {
                                new GameSettings(),
                            },
                        }
                    },
                },
            });

            mainNavigation.Current.ValueChanged += handleTabSelection;
            bottomNavigation.Current.ValueChanged += handleTabSelection;
        }

        /// <summary>
        /// Selects a tab.
        /// </summary>
        /// <param name="menuScreenType">The <see cref="MenuPage"/> as a type to select.</param>
        /// <returns>Whether the tab was successfully selected or not.</returns>
        public bool SelectTab<T>()
            where T : MenuPage
        {
            var target = tabs.FirstOrDefault(s => s?.GetType() == typeof(T));

            if (target == null)
                return false;

            if (bottomNavigation.Items.Contains(target))
                bottomNavigation.Current.Value = target;
            else
                mainNavigation.Current.Value = target;

            return true;
        }

        private void handleTabSelection(ValueChangedEvent<MenuPage> e)
        {
            if (e.NewValue == null)
                return;

            bool fromBottom = bottomNavigation.Items.Contains(e.NewValue);

            if (fromBottom)
                mainNavigation.Current.Value = null;
            else
                bottomNavigation.Current.Value = null;

            SelectedTab = e.NewValue;
            OnTabSelect?.Invoke(SelectedTab);
        }

        /// <summary>
        /// Toggles the side panel.
        /// </summary>
        public void Toggle()
            => State = State == NavigationPanelState.Contracted ? NavigationPanelState.Expanded : NavigationPanelState.Contracted;

        /// <summary>
        /// Expands the side panel.
        /// </summary>
        public void Expand()
            => State = NavigationPanelState.Expanded;

        /// <summary>
        /// Contracts the side panel.
        /// </summary>
        public void Contract()
            => State = NavigationPanelState.Contracted;
    }

    public enum NavigationPanelState
    {
        Contracted,

        Expanded,
    }
}