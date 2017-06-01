// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseMedalOverlay : TestCase
    {
        public override string Description => @"medal get!";

        public override void Reset()
        {
            base.Reset();

            MedalOverlay overlay;
            Add(overlay = new MedalOverlay(new Medal
            {
                Name = @"Animations",
                Description = @"More complex than you think.",
            }));

            AddStep(@"show", overlay.Show);
        }
    }
}
