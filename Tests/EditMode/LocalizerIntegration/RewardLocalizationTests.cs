namespace TeaSpoons.GameReward.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using TeaSpoons.Localizer;

    public class RewardLocalizationTests
    {
        private Translator translator;

        [SetUp]
        public void SetUp()
        {
            translator = new Translator("en");
            translator.AddTable("en", new Dictionary<string, string>
            {
                ["reward.gold"] = "Gold",
                ["reward.item.sword"] = "Sword",
                ["loot.gold"] = "Coins",
            });
            translator.AddTable("de", new Dictionary<string, string>
            {
                ["reward.gold"] = "Gold (de)",
                ["reward.item.sword"] = "Schwert",
            });
        }

        [Test]
        public void TheKeyHasTheTypeAndTheId()
        {
            Assert.AreEqual("reward.gold", new RewardItem("gold", 1).GetKey());
            Assert.AreEqual("reward.item.sword", new RewardItem("item", "sword", 1).GetKey());
            Assert.AreEqual("loot.gold", new RewardItem("gold", 1).GetKey("loot"));
        }

        [Test]
        public void TheNameFollowsTheLanguage()
        {
            var sword = new RewardItem("item", "sword", 1);

            Assert.AreEqual("Sword", sword.GetName(translator));

            translator.SetLanguage("de");
            Assert.AreEqual("Schwert", sword.GetName(translator));
        }

        [Test]
        public void AnItemWithoutATextIsNamedAfterItsIdOrType()
        {
            Assert.AreEqual("shield", new RewardItem("item", "shield", 1).GetName(translator));
            Assert.AreEqual("gems", new RewardItem("gems", 1).GetName(translator));
        }

        [Test]
        public void AnotherPrefixCanBeUsed()
        {
            Assert.AreEqual("Coins", new RewardItem("gold", 5).GetName(translator, "loot"));
        }

        [Test]
        public void DescribesAnItemWithItsAmount()
        {
            Assert.AreEqual("100x Gold", new RewardItem("gold", 100).Describe(translator));
        }

        [Test]
        public void DescribesAWholeReward()
        {
            var reward = new Reward(new RewardItem("gold", 100), new RewardItem("item", "sword", 1));

            Assert.AreEqual("100x Gold, 1x Sword", reward.Describe(translator));
        }

        [Test]
        public void UsesTheDefaultTranslatorWhenNoneIsGiven()
        {
            var original = Translator.Default;
            try
            {
                Translator.Default = translator;

                Assert.AreEqual("Gold", new RewardItem("gold", 1).GetName());
            }
            finally
            {
                Translator.Default = original;
            }
        }
    }
}
