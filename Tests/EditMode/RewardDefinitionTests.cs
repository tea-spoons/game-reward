namespace TeaSpoons.GameReward.Tests
{
    using System;
    using NUnit.Framework;
    using UnityEngine;

    public class RewardDefinitionTests
    {
        private RewardDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<RewardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test]
        public void BecomesAReward()
        {
            definition.SetItems(new[] { new RewardItem("gold", 100), new RewardItem("item", "sword", 1) });

            var reward = definition.ToReward();

            Assert.AreEqual(2, reward.Count);
            Assert.AreEqual(100, reward.GetAmount("gold"));
            Assert.AreEqual(2, definition.Items.Count);
        }

        [Test]
        public void ANewDefinitionIsAnEmptyReward()
        {
            Assert.IsTrue(definition.ToReward().IsEmpty);
        }

        [Test]
        public void ABadItemIsReportedWhenTheRewardIsBuilt()
        {
            definition.SetItems(new[] { new RewardItem("", 5) });

            Assert.Throws<ArgumentException>(() => definition.ToReward());
        }

        [Test]
        public void ItemsAreStoredAsTypeIdAndAmount()
        {
            definition.SetItems(new[] { new RewardItem("gold", 100) });

            var serialized = new UnityEditor.SerializedObject(definition);

            Assert.AreEqual("gold", serialized.FindProperty("items.Array.data[0].type").stringValue);
            Assert.AreEqual("", serialized.FindProperty("items.Array.data[0].id").stringValue);
            Assert.AreEqual(100, serialized.FindProperty("items.Array.data[0].amount").longValue);
        }
    }
}
