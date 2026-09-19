namespace TeaSpoons.GameReward.Tests
{
    using NUnit.Framework;

    public class RewardItemTests
    {
        [Test]
        public void ItemsAreEqualWhenTypeIdAndAmountAre()
        {
            Assert.AreEqual(new RewardItem("gold", 5), new RewardItem("gold", 5));
            Assert.AreEqual(new RewardItem("item", "sword", 1), new RewardItem("item", "sword", 1));
            Assert.IsTrue(new RewardItem("gold", 5) == new RewardItem("gold", string.Empty, 5));
            Assert.IsTrue(new RewardItem("gold", 5) != new RewardItem("gold", 6));
            Assert.AreNotEqual(new RewardItem("item", "sword", 1), new RewardItem("item", "shield", 1));
            Assert.AreEqual(new RewardItem("gold", 5).GetHashCode(), new RewardItem("gold", 5).GetHashCode());
        }

        [Test]
        public void ANullTypeAndIdReadAsEmpty()
        {
            var item = new RewardItem(null, null, 3);

            Assert.AreEqual(string.Empty, item.Type);
            Assert.AreEqual(string.Empty, item.Id);
        }

        [Test]
        public void IsSameKindIgnoresTheAmount()
        {
            Assert.IsTrue(new RewardItem("gold", 5).IsSameKind(new RewardItem("gold", 500)));
            Assert.IsFalse(new RewardItem("item", "sword", 1).IsSameKind(new RewardItem("item", "shield", 1)));
            Assert.IsFalse(new RewardItem("gold", 5).IsSameKind(new RewardItem("Gold", 5)), "Types are case-sensitive.");
        }

        [Test]
        public void WithAmountKeepsTheKind()
        {
            var item = new RewardItem("item", "sword", 1).WithAmount(4);

            Assert.AreEqual("sword", item.Id);
            Assert.AreEqual(4, item.Amount);
        }

        [Test]
        public void ToStringShowsTheKindAndAmount()
        {
            Assert.AreEqual("gold x5", new RewardItem("gold", 5).ToString());
            Assert.AreEqual("item:sword x1", new RewardItem("item", "sword", 1).ToString());
        }
    }
}
