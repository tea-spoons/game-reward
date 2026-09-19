namespace TeaSpoons.GameReward.Tests
{
    using System;
    using NUnit.Framework;

    public class WeightedRewardTableTests
    {
        private static Reward Gold(long amount) => new Reward(new RewardItem("gold", amount));

        [Test]
        public void ChancesFollowTheWeights()
        {
            var table = new WeightedRewardTable().Add(Gold(1), 1).Add(Gold(2), 3);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(4, table.TotalWeight, 1e-9);
            Assert.AreEqual(0.25, table.GetChance(0), 1e-9);
            Assert.AreEqual(0.75, table.GetChance(1), 1e-9);
        }

        [Test]
        public void RollsFollowTheWeightsOverManyDraws()
        {
            var table = new WeightedRewardTable().Add(Gold(1), 1).Add(Gold(2), 3);
            var random = new Random(1234);

            var common = 0;
            const int draws = 20000;
            for (var i = 0; i < draws; i++)
            {
                if (table.Roll(random).GetAmount("gold") == 2)
                {
                    common++;
                }
            }

            Assert.AreEqual(0.75, common / (double)draws, 0.02);
        }

        [Test]
        public void ASeededRandomGivesRepeatableRolls()
        {
            var table = new WeightedRewardTable().Add(Gold(1), 1).Add(Gold(2), 1).Add(Gold(3), 1);

            var first = new[] { table.Roll(new Random(7)), table.Roll(new Random(7)) };

            Assert.AreSame(first[0], first[1]);
        }

        [Test]
        public void AnEntryWithAllTheWeightIsAlwaysRolled()
        {
            var table = new WeightedRewardTable().Add(Gold(9), 1);
            var random = new Random(1);

            for (var i = 0; i < 100; i++)
            {
                Assert.AreEqual(9, table.Roll(random).GetAmount("gold"));
            }
        }

        [Test]
        public void ARollFromAnEmptyTableIsRefused()
        {
            Assert.Throws<InvalidOperationException>(() => new WeightedRewardTable().Roll(new Random(1)));
        }

        [Test]
        public void WeightsMustBeAboveZero()
        {
            var table = new WeightedRewardTable();

            Assert.Throws<ArgumentOutOfRangeException>(() => table.Add(Gold(1), 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => table.Add(Gold(1), -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => table.Add(Gold(1), double.NaN));
            Assert.Throws<ArgumentNullException>(() => table.Add(null, 1));
            Assert.AreEqual(0, table.Count);
        }
    }
}
