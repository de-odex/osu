﻿using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Vitaru.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Audio;
using System.Linq;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Framework.Configuration;

namespace osu.Game.Rulesets.Vitaru.Beatmaps
{
    internal class VitaruBeatmapConverter : BeatmapConverter<VitaruHitObject>
    {
        private readonly VitaruGamemode currentGameMode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);
        private readonly bool multiplayer = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.ShittyMultiplayer);
        private readonly int enemyPlayerCount = VitaruSettings.VitaruConfigManager.GetBindable<int>(VitaruSetting.EnemyPlayerCount);

        public static List<HitObject> HitObjectList = new List<HitObject>();

        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasPosition) };

        private float ar;
        private float cs;

        protected override IEnumerable<VitaruHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            var endTimeData = original as IHasEndTime;
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;

            List<SampleInfo> samples = original.Samples;

            double complexity = 1;
            if (currentGameMode == VitaruGamemode.Dodge)
                complexity = 0.5f;

            ar = calculateAr(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate);
            cs = calculateCs(beatmap.BeatmapInfo.BaseDifficulty.CircleSize);

            bool isWhistle = samples.Any(s => s.Name == SampleInfo.HIT_WHISTLE);
            bool isFinish = samples.Any(s => s.Name == SampleInfo.HIT_FINISH);
            bool isClap = samples.Any(s => s.Name == SampleInfo.HIT_CLAP);

            Pattern p = new Pattern
            {
                Ar = ar,
                Cs = cs,
                StartTime = original.StartTime,
                Position = positionData?.Position ?? Vector2.Zero,
                Samples = original.Samples,
                PatternComplexity = complexity,
                PatternTeam = 1,
                NewCombo = comboData?.NewCombo ?? false,
            };

            if (original is IHasCurve curveData)
            {
                p.IsSlider = true;
                p.ControlPoints = curveData.ControlPoints;
                p.CurveType = curveData.CurveType;
                p.Distance = curveData.Distance;
                p.RepeatSamples = curveData.RepeatSamples;
                p.RepeatCount = curveData.RepeatCount;

                p.EnemyHealth = 60;

                if (isWhistle)
                {
                    p.PatternSpeed = 0.4f;
                    p.PatternID = 3;
                }
                else if (isFinish)
                {
                    p.PatternID = 4;
                }
                else if (isClap)
                {
                    p.PatternID = 5;
                }
                else
                {
                    p.PatternID = 1;
                }
            }
            else if (endTimeData != null)
            {
                p.IsSpinner = true;
                p.PatternSpeed = 0.3f;
                p.EnemyHealth = 180;
                p.PatternDamage = 5;
                p.PatternID = 6;
                p.EndTime = endTimeData.EndTime;
            }
            else
            {
                if (isWhistle)
                {
                    p.PatternSpeed = 0.5f;
                    p.PatternID = 3;
                }
                else if (isFinish)
                {
                    p.PatternID = 4;
                }
                else if (isClap)
                {
                    p.PatternID = 5;
                }
                else
                {
                    p.PatternID = 1;
                }
            }

            if (multiplayer && enemyPlayerCount > 0)
                HitObjectList.Add(p);

            yield return p;
        }

        private float calculateAr(float ar)
        {
            if (ar >= 5)
            {
                this.ar = 1200 - ((ar - 5) * 150);
                return this.ar;
            }
            else
            {
                this.ar = 1800 - (ar * 120);
                return this.ar;
            }
        }

        private float calculateCs(float cs)
        {
            this.cs = cs / 4;
            return this.cs;
        }
    }
}
