using OpenTK;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Vitaru.Objects
{
    public class Bullet : VitaruHitObject, IHasCurve
    {
        public override HitObjectType Type => HitObjectType.Bullet;

        public bool DummyMode { get; set; }

        public float BulletDamage { get; set; } = 10;
        public float BulletSpeed { get; set; } = 1f;
        public float BulletDiameter { get; set; } = 16f;
        public double BulletAngle { get; set; }
        public bool DynamicBulletVelocity { get; set; }
        public bool Piercing { get; set; }
        public int Team { get; set; } = -1;
        public bool ShootPlayer { get; set; }
        public bool ObeyBoundries { get; } = true;
        public bool Ghost { get; set; }
        public SliderType SliderType
        {
            get { return sliderType; }
            set
            {
                sliderType = value;
                switch (value)
                {
                    case SliderType.Straight:
                        Curve = new SliderCurve()
                        {
                            CurveType = CurveType.Linear,
                            ControlPoints = new List<Vector2>
                            {
                                Position,
                                new Vector2((float)Math.Cos(BulletAngle) * 1000 + Position.X, (float)Math.Sin(BulletAngle) * 1000 + Position.Y)
                            },
                            Distance = 1000,
                        };
                        break;
                }
                EndTime = StartTime + Curve.Distance / Velocity;

            }
        }

        private SliderType sliderType;

        public List<List<SampleInfo>> RepeatSamples { get; set; } = new List<List<SampleInfo>>();
        public bool IsSlider { get; set; } = false;
        private const float base_scoring_distance = 100;
        public double Duration => EndTime - StartTime;
        public SliderCurve Curve { get; private set; } = new SliderCurve();
        public double Velocity => BulletSpeed;
        public double SpanDuration => Duration / this.SpanCount();
        public int RepeatCount { get; set; }

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public CurveType CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public double Distance
        {
            get { return Curve.Distance; }
            set { Curve.Distance = value; }
        }

        public override Vector2 EndPosition => this.CurvePositionAt(1);
        public Vector2 PositionAt(double t) => this.CurvePositionAt(t);

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            EndTime = StartTime + Curve.Distance / Velocity;
            SliderType = SliderType.Straight;
        }
    }

    public enum SliderType
    {
        Straight,
        Curve,
    }
}
