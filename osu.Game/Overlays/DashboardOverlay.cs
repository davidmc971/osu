﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Overlays
{
    public class DashboardOverlay : FullscreenOverlay
    {
        private CancellationTokenSource cancellationToken;

        private readonly Box background;
        private readonly Container content;
        private readonly DashboardOverlayHeader header;
        private readonly LoadingLayer loading;
        private readonly OverlayScrollContainer scrollFlow;

        public DashboardOverlay()
            : base(OverlayColourScheme.Purple)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                scrollFlow = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new DashboardOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue
                            },
                            content = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                },
                loading = new LoadingLayer(content),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            header.Current.BindValueChanged(onTabChanged);
        }

        private bool displayUpdateRequired = true;

        protected override void PopIn()
        {
            base.PopIn();

            // We don't want to create new display on every call, only when exiting from fully closed state.
            if (displayUpdateRequired)
            {
                header.Current.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            loadDisplay(Empty());
            displayUpdateRequired = true;
        }

        private void loadDisplay(Drawable display)
        {
            scrollFlow.ScrollToStart();

            LoadComponentAsync(display, loaded =>
            {
                loading.Hide();
                content.Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private void onTabChanged(ValueChangedEvent<DashboardOverlayTabs> tab)
        {
            cancellationToken?.Cancel();

            loading.Show();

            switch (tab.NewValue)
            {
                case DashboardOverlayTabs.Friends:
                    loadDisplay(new FriendDisplay());
                    break;

                default:
                    throw new NotImplementedException($"Display for {tab.NewValue} tab is not implemented");
            }
        }

        public override void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    // Will force to create a display based on visibility state
                    displayUpdateRequired = true;
                    State.TriggerChange();
                    return;

                default:
                    content.Clear();
                    loading.Show();
                    return;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
