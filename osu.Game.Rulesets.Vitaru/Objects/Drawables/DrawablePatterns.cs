﻿using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using System;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawablePattern : DrawableVitaruHitObject
    {
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        public static int PatternCount;
        private readonly Pattern pattern;
        private Vector2 patternStartPosition;
        private Container energyCircle;

        private bool loaded;
        private bool started;
        private bool done;

        private readonly double endTime;
        
        private int currentRepeat;

        private Enemy enemy;

        private bool prepedToPop;
        private bool popped;

        public DrawablePattern(Container parent, Pattern pattern) : base(pattern, parent)
        {
            AlwaysPresent = true;

            this.pattern = pattern;

            if (!pattern.IsSlider && !pattern.IsSpinner)
            {
                endTime = this.pattern.StartTime + TIME_FADEOUT;
                this.pattern.EndTime = endTime;
            }
            else if (pattern.IsSlider)
                endTime = this.pattern.EndTime + TIME_FADEOUT;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PatternCount++;

            LifetimeStart = pattern.StartTime - (TIME_PREEMPT + 1000f);
        }

        //Should be called when a DrawablePattern is getting ready to become visable as to save on resources before hand
        private void load()
        {
            if (!loaded)
            {
                if (currentGameMode != VitaruGamemode.Dodge)
                {
                    //load the enemy
                    ParentContainer.Add(enemy = new Enemy(ParentContainer, pattern, this)
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Depth = 5,
                        MaxHealth = pattern.EnemyHealth,
                        Team = 1
                    });

                    Child = energyCircle = new Container
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
                    };
                    enemy.FadeInFromZero(TIME_FADEIN);
                    enemy.Position = getPatternStartPosition();
                    enemy.MoveTo(pattern.Position, TIME_PREEMPT);
                }
                else
                {
                    Child = energyCircle = new CircularContainer
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
                    };
                }

                Position = getPatternStartPosition();
                this.MoveTo(pattern.Position, TIME_PREEMPT);

                if (NestedHitObjects != null)
                    foreach (var o in NestedHitObjects)
                    {
                        var b = (DrawableBullet)o;
                        ParentContainer.Remove(b);
                        b.Dispose();
                    }

                //Load the bullets
                foreach (var o in pattern.NestedHitObjects)
                {
                    var b = (Bullet)o;
                    DrawableBullet drawableBullet = new DrawableBullet(ParentContainer, b, this);
                    ParentContainer.Add(drawableBullet);
                    AddNested(drawableBullet);
                }

                loaded = true;
            }
        }

        private void unload()
        {
            if (loaded)
            {
                if (currentGameMode != VitaruGamemode.Dodge)
                {
                    ParentContainer.Remove(enemy);
                    enemy.Dispose();
                }

                loaded = false;
                started = false;
                done = false;
            }
        }

        private Vector2 getPatternStartPosition()
        {
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

        protected override void Update()
        {
            base.Update();

            //Used just to keep this Update(); function clean looking
            generalUpdateLogic();

            if (!pattern.IsSlider && !pattern.IsSpinner && loaded)
                hitcircleUpdate();

            if (pattern.IsSlider && loaded)
                sliderUpdate();

            if (pattern.IsSpinner && loaded)
                spinnerUpdate();
        }

        private void generalUpdateLogic()
        {
            if (HitObject.StartTime - TIME_PREEMPT <= Time.Current && Time.Current < pattern.EndTime + TIME_FADEOUT)
                load();

            else
                unload();

            if (currentGameMode != VitaruGamemode.Dodge && prepedToPop && HitObject.StartTime <= Time.Current)
                pop();
        }

        /// <summary>
        /// Will leave and hide
        /// </summary>
        private void end()
        {
            if (energyCircle.Alpha <= 0)
            {
                if (currentGameMode != VitaruGamemode.Dodge)
                    enemy.MoveTo(patternStartPosition, TIME_FADEOUT, Easing.InQuint);
                this.MoveTo(patternStartPosition, TIME_FADEOUT, Easing.InQuint);
                enemy.ScaleTo(new Vector2(0.5f), TIME_FADEOUT, Easing.InQuint);
                enemy.FadeOut(TIME_FADEOUT, Easing.InQuint);
            }
            else
            {
                energyCircle.FadeOut(TIME_FADEOUT / 4);
                energyCircle.ScaleTo(new Vector2(0.1f), TIME_FADEOUT / 4);
            }
        }

        public void PrepPop()
        {
            if (!prepedToPop && !done)
            {
                double time = pattern.StartTime - Time.Current;

                if (time < 0)
                    time = 0;

                energyCircle.FadeInFromZero(time);
                energyCircle.ScaleTo(Vector2.One, time);
                prepedToPop = true;
            }
        }

        private void pop()
        {
            if (!popped)
            {
                enemy.FadeOut(100);
                enemy.ScaleTo(new Vector2(1.2f), 100);
                popped = true;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
                PatternCount--;
        }

        private void throwBullets()
        {
            PlaySamples();
            foreach (var o in NestedHitObjects)
                if (o is DrawableBullet b && b.Bullet.StartTime <= Time.Current)
                {
                    b.Position = Position;

                    if (b.Bullet.ShootPlayer)
                        b.Bullet.BulletAngle += pattern.PlayerRelativePositionAngle(VitaruPlayfield.VitaruPlayer.Position, b.Position) - Math.PI / 2;
                }
        }

        /// <summary>
        /// All the hitcircle stuff
        /// </summary>
        #region Hitcircle Stuff
        private void hitcircleUpdate()
        {
            if (HitObject.StartTime <= Time.Current && !started)
            {
                started = true;
                done = true;

                throwBullets();
                end();
            }
        }
        #endregion

        /// <summary>
        /// All The Slider Stuff
        /// </summary>
        #region Slider Stuff
        private void sliderUpdate()
        {
            double completionProgress = MathHelper.Clamp((Time.Current - pattern.StartTime) / pattern.Duration, 0, 1);
            int repeat = pattern.RepeatAt(completionProgress);

            if (HitObject.StartTime <= Time.Current && !started)
            {
                throwBullets();
                started = true;
            }

            if (!done && started)
            {
                Position = pattern.PositionAt(completionProgress);
                if (currentGameMode != VitaruGamemode.Dodge)
                    enemy.Position = pattern.PositionAt(completionProgress);
            }

            if (repeat > currentRepeat)
            {
                if (repeat < pattern.RepeatCount)
                    throwBullets();
                currentRepeat = repeat;
            }

            if (pattern.EndTime <= Time.Current && started && !done)
            {
                end();
                throwBullets();
                done = true;
            }
        }
        #endregion

        /// <summary>
        /// All the spinner stuff
        /// </summary>
        #region Spinner Stuff
        private void spinnerUpdate()
        {
            if (pattern.StartTime <= Time.Current && !started)
            {
                throwBullets();
                started = true;
            }
            if (pattern.EndTime <= Time.Current)
            {
                done = true;
                end();
            }
        }
        #endregion
    }
}
