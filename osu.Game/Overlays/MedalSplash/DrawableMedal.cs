// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.MedalSplash
{
    public class DrawableMedal : Container
    {
        private const float scale_when_unlocked = 0.76f;
        private const float scale_when_full = 0.6f;

        private readonly Container medal;
        private readonly Sprite medalGlow;
        private readonly OsuSpriteText unlocked, name;
        private readonly Paragraph description;
        private readonly FillFlowContainer infoFlow;
        private readonly IEnumerable<SpriteText> descriptionSprites;

        public DrawableMedal(Medal medal)
        {
            Position = new Vector2(0f, MedalOverlay.DISC_SIZE / 2);

            Children = new Drawable[]
            {
                this.medal = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Alpha = 0f,
                    Children = new[]
                    {
                        medalGlow = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                },
                unlocked = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Medal Unlocked".ToUpper(),
                    TextSize = 24,
                    Font = @"Exo2.0-Light",
                    Alpha = 0f,
                    Scale = new Vector2(1 / scale_when_unlocked),
                },
                infoFlow = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.6f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 5f),
                    Children = new Drawable[]
                    {
                        name = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = medal.Name,
                            TextSize = 20,
                            Font = @"Exo2.0-Bold",
                            Alpha = 0f,
                            Scale = new Vector2(1 / scale_when_full),
                        },
                        description = new Paragraph
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0f,
                            Scale = new Vector2(1 / scale_when_full),
                        },
                    },
                },
            };

            descriptionSprites = description.AddText(medal.Description, s =>
            {
                s.Anchor = Anchor.TopCentre;
                s.Origin = Anchor.TopCentre;
                s.TextSize = 16;
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            medalGlow.Texture = textures.Get(@"MedalSplash/medal-glow");

            foreach (var s in descriptionSprites)
                s.Colour = colours.BlueLight;

            var pos = new Vector2(0f, medal.Size.Y / 2 + 10);
            unlocked.Position = pos;
            pos.Y += 90;
            infoFlow.Position = pos;
        }

        public void ChangeState(DisplayState newState, double duration)
        {
            switch (newState)
            {
                case DisplayState.Icon:
                    medal.Scale = Vector2.Zero;
                    medal.ScaleTo(1, duration, EasingTypes.OutElastic);
                    medal.FadeInFromZero(duration);
                    break;
                case DisplayState.MedalUnlocked:
                    ScaleTo(scale_when_unlocked, duration, EasingTypes.OutExpo);
                    MoveToY(MedalOverlay.DISC_SIZE / 2 - 30, duration, EasingTypes.OutExpo);
                    unlocked.FadeInFromZero(duration);
                    break;
                case DisplayState.Full:
                    ScaleTo(scale_when_full, duration, EasingTypes.OutExpo);
                    MoveToY(MedalOverlay.DISC_SIZE / 2 - 60, duration, EasingTypes.OutExpo);
                    name.FadeInFromZero(duration);
                    description.FadeInFromZero(duration * 2);
                    break;
            }
        }

        public enum DisplayState
        {
            Icon,
            MedalUnlocked,
            Full,
        }
    }
}
