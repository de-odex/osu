using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using System;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawablePattern : DrawableVitaruHitObject
    {
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        public static int PatternCount;
        private readonly Pattern pattern;
        private Container energyCircle;

        private bool loaded;
        private bool started;
        private bool done;

        private readonly double endTime;
        
        private int currentRepeat;

        private Enemy enemy;

        private bool prepedToPop;
        private bool popped;

        public DrawablePattern(Pattern pattern, VitaruPlayfield playfield) : base(pattern, playfield)
        {
            AlwaysPresent = true;

            this.pattern = pattern;

            if (!pattern.IsSlider && !pattern.IsSpinner)
            {
                endTime = this.pattern.StartTime + HitObject.TimePreempt * 2 - HitObject.TimeFadein;
                this.pattern.EndTime = endTime;
            }
            else if (pattern.IsSlider)
                endTime = this.pattern.EndTime + HitObject.TimePreempt * 2 - HitObject.TimeFadein;

            PatternCount++;

            LifetimeStart = pattern.StartTime - (HitObject.TimePreempt);
            LifetimeEnd = pattern.EndTime + HitObject.TimePreempt * 2 - HitObject.TimeFadein;

            load();
        }

        //Should be called when a DrawablePattern is getting ready to become visable as to save on resources before hand
        private void load()
        {
            if (!loaded)
            {
                if (currentGameMode != VitaruGamemode.Dodge)
                {
                    //load the enemy
                    VitaruPlayfield.CharacterField.Add(enemy = new Enemy(VitaruPlayfield, pattern, this)
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Depth = 5,
                        MaxHealth = pattern.EnemyHealth,
                        Team = 1,
                    });

                    VitaruPlayfield.CharacterField.Add(energyCircle = new Container
                    {
                        Alpha = 0,
                        Masking = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(30),
                        CornerRadius = 30f / 2,
                        BorderThickness = 10,
                        BorderColour = AccentColour,

                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        },
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = AccentColour.Opacity(0.5f),
                            Radius = Width / 2,
                        }
                    });

                    enemy.Position = getPatternStartPosition();
                    energyCircle.Position = enemy.Position;
                }
                else
                {
                    VitaruPlayfield.CharacterField.Add(energyCircle = new CircularContainer
                    {
                        Masking = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(20),
                        BorderThickness = 6,
                        BorderColour = AccentColour,

                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        },

                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = AccentColour.Opacity(0.5f),
                            Radius = Width / 2,
                        }
                    });
                    energyCircle.Position = getPatternStartPosition();
                }

                Position = pattern.Position;
                Size = new Vector2(64);

                //Load the bullets
                foreach (var o in pattern.NestedHitObjects)
                {
                    Bullet b = (Bullet)o;
                    DrawableBullet drawableBullet = new DrawableBullet(b, this, VitaruPlayfield);
                    VitaruPlayfield.BulletField.Add(drawableBullet);
                    AddNested(drawableBullet);
                }

                loaded = true;
            }
        }

        private Vector2 getPatternStartPosition()
        {
            Vector2 patternStartPosition;

            if (pattern.Position.X <= 384f / 2 && pattern.Position.Y <= 512f / 2)
                patternStartPosition = pattern.Position - new Vector2(384f / 2, 512f / 2);
            else if (pattern.Position.X > 384f / 2 && pattern.Position.Y <= 512f / 2)
                patternStartPosition = new Vector2(pattern.Position.X + 384f / 2, pattern.Position.Y - 512f / 2);
            else if (pattern.Position.X > 384f / 2 && pattern.Position.Y > 512f / 2)
                patternStartPosition = pattern.Position + new Vector2(384f / 2, 512f / 2);
            else
                patternStartPosition = new Vector2(pattern.Position.X - 384f / 2, pattern.Position.Y + 512f / 2);

            return patternStartPosition;
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            enemy.FadeIn(Math.Min(HitObject.TimeFadein * 2, HitObject.TimePreempt))
                .MoveTo(pattern.Position, HitObject.TimePreempt);

            energyCircle.FadeIn(Math.Min(HitObject.TimeFadein * 2, HitObject.TimePreempt))
                .MoveTo(pattern.Position, HitObject.TimePreempt);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            if (HitObject.StartTime <= Time.Current && !started)
            {
                started = true;
                done = true;

                PlaySamples();
                end();
            }
        }

        private void end()
        {
            if (currentGameMode != VitaruGamemode.Dodge)
                enemy.MoveTo(getPatternStartPosition(), HitObject.TimePreempt * 2, Easing.InQuint)
                    .Delay(HitObject.TimePreempt * 2 - HitObject.TimeFadein)
                    .ScaleTo(new Vector2(0.5f), HitObject.TimeFadein, Easing.InQuint)
                    .FadeOut(HitObject.TimeFadein, Easing.InQuint)
                    .Expire();

            this.MoveTo(getPatternStartPosition(), HitObject.TimePreempt * 2, Easing.InQuint)
                .Expire();

            energyCircle.FadeOut(HitObject.TimePreempt / 2)
                .ScaleTo(new Vector2(0.1f), HitObject.TimePreempt / 2)
                .Expire();
        }

        protected override void Dispose(bool isDisposing)
        {
            PatternCount--;
            base.Dispose(isDisposing);
        }
    }
}
