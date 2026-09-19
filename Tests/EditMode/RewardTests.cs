namespace TeaSpoons.GameReward.Tests
{
    using System;
    using NUnit.Framework;

    public class RewardTests
    {
        [Test]
        public void ItemsKeepTheirOrder()
        {
            var reward = new Reward(new RewardItem("gold", 100), new RewardItem("item", "sword", 1));

            Assert.AreEqual(2, reward.Count);
            Assert.AreEqual("gold", reward[0].Type);
            Assert.AreEqual("sword", reward[1].Id);
            Assert.IsFalse(reward.IsEmpty);
        }

        [Test]
        public void ItemsWithoutAmountAreDropped()
        {
            var reward = new Reward(new RewardItem("gold", 0), new RewardItem("gems", 3));

            Assert.AreEqual(1, reward.Count);
            Assert.AreEqual("gems", reward[0].Type);
        }

        [Test]
        public void AnItemNeedsATypeAndAPositiveAmount()
        {
            Assert.Throws<ArgumentException>(() => new Reward(new RewardItem("", 5)));
            Assert.Throws<ArgumentException>(() => new Reward(new RewardItem(null, 5)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Reward(new RewardItem("gold", -1)));
        }

        [Test]
        public void TheEmptyRewardHasNothing()
        {
            Assert.IsTrue(Reward.Empty.IsEmpty);
            Assert.AreEqual(0, Reward.Empty.Count);
            Assert.AreEqual("(empty)", Reward.Empty.ToString());
        }

        [Test]
        public void MergedCombinesTheSameKindAndKeepsTheFirstOrder()
        {
            var reward = new Reward(
                new RewardItem("gold", 10),
                new RewardItem("item", "sword", 1),
                new RewardItem("gold", 5),
                new RewardItem("item", "shield", 1),
                new RewardItem("item", "sword", 2)).Merged();

            Assert.AreEqual(3, reward.Count);
            Assert.AreEqual(new RewardItem("gold", 15), reward[0]);
            Assert.AreEqual(new RewardItem("item", "sword", 3), reward[1]);
            Assert.AreEqual(new RewardItem("item", "shield", 1), reward[2]);
        }

        [Test]
        public void PlusAddsTwoRewardsTogether()
        {
            var sum = new Reward(new RewardItem("gold", 10), new RewardItem("gems", 1))
                .Plus(new Reward(new RewardItem("gold", 5), new RewardItem("energy", 2)));

            Assert.AreEqual(15, sum.GetAmount("gold"));
            Assert.AreEqual(1, sum.GetAmount("gems"));
            Assert.AreEqual(2, sum.GetAmount("energy"));
            Assert.AreEqual(3, sum.Count);
        }

        [Test]
        public void MultiplyScalesEveryAmount()
        {
            var reward = new Reward(new RewardItem("gold", 10), new RewardItem("gems", 1)).Multiply(3);

            Assert.AreEqual(30, reward.GetAmount("gold"));
            Assert.AreEqual(3, reward.GetAmount("gems"));
        }

        [Test]
        public void MultiplyByZeroGivesNothingAndByANegativeIsRefused()
        {
            var reward = new Reward(new RewardItem("gold", 10));

            Assert.IsTrue(reward.Multiply(0).IsEmpty);
            Assert.Throws<ArgumentOutOfRangeException>(() => reward.Multiply(-1));
        }

        [Test]
        public void MultiplyThatOverflowsIsRefused()
        {
            var reward = new Reward(new RewardItem("gold", long.MaxValue / 2 + 1));

            Assert.Throws<OverflowException>(() => reward.Multiply(2));
        }

        [Test]
        public void ScaleRoundsAndDropsWhatRoundsToZero()
        {
            var reward = new Reward(new RewardItem("gold", 10), new RewardItem("gems", 1)).Scale(0.25);

            Assert.AreEqual(3, reward.GetAmount("gold"));    // 2.5 rounds away from zero
            Assert.IsFalse(reward.Contains("gems"));         // 0.25 rounds to 0
        }

        [Test]
        public void ScaleRoundingCanBeChosen()
        {
            var reward = new Reward(new RewardItem("gold", 10)).Scale(0.25, MidpointRounding.ToEven);

            Assert.AreEqual(2, reward.GetAmount("gold"));
        }

        [Test]
        public void ScaleRefusesBadFactors()
        {
            var reward = new Reward(new RewardItem("gold", 10));

            Assert.Throws<ArgumentOutOfRangeException>(() => reward.Scale(-0.5));
            Assert.Throws<ArgumentOutOfRangeException>(() => reward.Scale(double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => reward.Scale(double.PositiveInfinity));
        }

        [Test]
        public void GetAmountSumsTheItemsOfOneKind()
        {
            var reward = new Reward(new RewardItem("gold", 10), new RewardItem("gold", 5), new RewardItem("item", "sword", 1));

            Assert.AreEqual(15, reward.GetAmount("gold"));
            Assert.AreEqual(1, reward.GetAmount("item", "sword"));
            Assert.AreEqual(0, reward.GetAmount("item"), "An item with an id is another kind than one without.");
            Assert.IsTrue(reward.Contains("gold"));
            Assert.IsFalse(reward.Contains("gems"));
        }

        [Test]
        public void RewardsAreImmutable()
        {
            var items = new[] { new RewardItem("gold", 10) };
            var reward = new Reward(items);

            items[0] = new RewardItem("gold", 999);

            Assert.AreEqual(10, reward.GetAmount("gold"));
        }
    }
}
