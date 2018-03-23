using osu.Framework.Graphics;
using OpenTK;
using System;
using osu.Game.Rulesets.Vitaru.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Vitaru.Judgements;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.UI;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using Symcol.Core.GameObjects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableBullet : DrawableVitaruHitObject
    { 
        public static int BulletCount;

        private readonly ScoringMetric currentScoringMetric = VitaruSettings.VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric);
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);

        //Used like a multiple (useful for spells in multiplayer)
        public static float BulletSpeedModifier = 1;

        //Playfield size + Margin of 10 on each side
        public Vector4 BulletBounds = new Vector4(-10, -10, 520, 830);

        //Set to "true" when a judgement should be returned
        private bool returnJudgement;

        public bool ReturnGreat = false;

        //Can be set for the Graze ScoringMetric
        public int ScoreZone;

        //Should be set to true when a character is hit
        public bool Hit;

        //Incase we want to be deleted in the near future
        public double BulletDeleteTime = -1;

        private readonly DrawablePattern drawablePattern;
        public readonly Bullet Bullet;

        public Action OnHit;

        public SymcolHitbox Hitbox;

        private BulletPiece bulletPiece;

        private bool started;
        private bool loaded;

        public DrawableBullet(Bullet bullet, DrawablePattern drawablePattern, VitaruPlayfield playfield) : base(bullet, playfield)
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;

            BulletCount++;

            Bullet = bullet;
            this.drawablePattern = drawablePattern;

            if (currentGameMode == VitaruGamemode.Dodge)
                BulletBounds = new Vector4(-10, -10, 522, 394);

            load();
        }

        public DrawableBullet(Bullet bullet, VitaruPlayfield playfield) : base(bullet, playfield)
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;

            BulletCount++;

            Bullet = bullet;

            if (currentGameMode == VitaruGamemode.Dodge)
                BulletBounds = new Vector4(-10, -10, 522, 394);

            load();
        }

        private void load()
        {
            Size = new Vector2(Bullet.BulletDiameter);
            Scale = new Vector2(0.1f);

            Children = new Drawable[]
            {
                bulletPiece = new BulletPiece(this),
                Hitbox = new SymcolHitbox(new Vector2(Bullet.BulletDiameter), Shape.Circle)
                {
                    Team = Bullet.Team,
                    HitDetection = false
                }
            };
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            base.CheckForJudgements(userTriggered, timeOffset);

            if (returnJudgement)
            {
                if (currentScoringMetric == ScoringMetric.ScoreZones)
                {
                    switch (VitaruPlayfield.VitaruPlayer.ScoreZone)
                    {
                        case 0:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                            break;
                        case 100:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Ok });
                            break;
                        case 200:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Good });
                            break;
                        case 300:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                    }
                }
                else if (currentScoringMetric == ScoringMetric.InverseCatch)
                {
                    switch (VitaruPlayfield.VitaruPlayer.ScoreZone)
                    {
                        case 0:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                            break;
                        case 100:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                        case 200:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                        case 300:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                    }
                }
                else if (currentScoringMetric == ScoringMetric.Graze)
                {
                    switch (ScoreZone)
                    {
                        case 0:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Miss });
                            break;
                        case 50:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Meh });
                            break;
                        case 100:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Ok });
                            break;
                        case 200:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Good });
                            break;
                        case 300:
                            AddJudgement(new VitaruJudgement { Result = HitResult.Great });
                            break;
                    }
                }
            }

            else if (Hit)
                AddJudgement(new VitaruJudgement { Result = HitResult.Miss });

            else if (ReturnGreat)
                AddJudgement(new VitaruJudgement { Result = HitResult.Great });
        }

        protected override void Dispose(bool isDisposing)
        {
            BulletCount--;
            base.Dispose(isDisposing);
        }

        protected override void Update()
        {
            base.Update();

            if (OnHit != null && Hit)
            {
                OnHit();
                OnHit = null;
            }

            if (Time.Current >= Bullet.StartTime)
            {
                double completionProgress = MathHelper.Clamp((Time.Current - Bullet.StartTime) / Bullet.Duration, 0, 1);

                Position = Bullet.PositionAt(completionProgress);

                if (Bullet.ObeyBoundries && Position.Y < BulletBounds.Y | Position.X < BulletBounds.X | Position.Y > BulletBounds.W | Position.X > BulletBounds.Z && !returnJudgement)
                    returnJudgement = true;
            }
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            Position = Bullet.Position;
            Hitbox.HitDetection = true;
            started = true;
            this.FadeInFromZero(100)
                .ScaleTo(Vector2.One, 100);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            if (!Bullet.DummyMode)
                switch (state)
                {
                    case ArmedState.Idle:
                        this.Delay(HitObject.TimePreempt).FadeOut(500);

                        Expire(true);

                        // override lifetime end as FadeIn may have been changed externally, causing out expiration to be too early.
                        LifetimeEnd = double.MaxValue;
                        break;
                    case ArmedState.Miss:
                        LifetimeEnd = Time.Current + HitObject.TimePreempt / 6;
                        this.FadeOutFromOne(HitObject.TimePreempt / 6);
                        Expire();
                        break;
                    case ArmedState.Hit:
                        LifetimeEnd = Time.Current + HitObject.TimePreempt / 6;
                        this.FadeOutFromOne(HitObject.TimePreempt / 6);
                        Expire();
                        break;
                }
        }
    }
}
