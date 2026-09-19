namespace TeaSpoons.GameReward.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public class RewardServiceTests
    {
        private RewardService service;
        private Dictionary<string, long> wallet;
        private List<string> log;

        [SetUp]
        public void SetUp()
        {
            service = new RewardService();
            wallet = new Dictionary<string, long>();
            log = new List<string>();

            // A currency handler that can be undone.
            service.Register("gold",
                grant: item => { wallet["gold"] = wallet.GetValueOrDefault("gold") + item.Amount; log.Add("grant " + item); },
                revoke: item => { wallet["gold"] -= item.Amount; log.Add("revoke " + item); });
        }

        [Test]
        public void GrantsAReward()
        {
            var result = service.Grant(new Reward(new RewardItem("gold", 100)));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(100, wallet["gold"]);
            Assert.AreEqual(1, result.Granted.Count);
            Assert.IsEmpty(result.Failures);
            Assert.IsFalse(result.IsPartial);
        }

        [Test]
        public void GrantsEveryItemInOrder()
        {
            service.Register("gems", item => log.Add("grant " + item));

            service.Grant(new Reward(new RewardItem("gold", 1), new RewardItem("gems", 2), new RewardItem("gold", 3)));

            CollectionAssert.AreEqual(new[] { "grant gold x1", "grant gems x2", "grant gold x3" }, log);
        }

        [Test]
        public void RaisesGrantedAfterAWholeReward()
        {
            var grantedRewards = new List<Reward>();
            service.Granted += grantedRewards.Add;
            var reward = new Reward(new RewardItem("gold", 5));

            service.Grant(reward);

            Assert.AreEqual(1, grantedRewards.Count);
            Assert.AreSame(reward, grantedRewards[0]);
        }

        [Test]
        public void AnItemWithoutAHandlerGrantsNothing()
        {
            var result = service.Grant(new Reward(new RewardItem("gold", 100), new RewardItem("mystery", 1)));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(RewardFailureReason.NoHandler, result.Failures.Single().Reason);
            Assert.AreEqual("mystery", result.Failures.Single().Item.Type);
            Assert.IsEmpty(wallet, "Nothing may be granted when one item cannot be.");
            Assert.IsEmpty(log);
        }

        [Test]
        public void AHandlerThatRejectsAnItemGrantsNothing()
        {
            var inventoryFull = true;
            service.Register("item", item => log.Add("grant " + item), canGrant: _ => !inventoryFull);

            var result = service.Grant(new Reward(new RewardItem("gold", 100), new RewardItem("item", "sword", 1)));

            Assert.AreEqual(RewardFailureReason.Rejected, result.Failures.Single().Reason);
            Assert.IsEmpty(log);

            inventoryFull = false;
            Assert.IsTrue(service.Grant(new Reward(new RewardItem("gold", 100), new RewardItem("item", "sword", 1))).Success);
        }

        [Test]
        public void AllFailuresAreReported()
        {
            var result = service.Grant(new Reward(new RewardItem("a", 1), new RewardItem("gold", 1), new RewardItem("b", 1)));

            CollectionAssert.AreEqual(new[] { "a", "b" }, result.Failures.Select(f => f.Item.Type));
        }

        [Test]
        public void AHandlerThatFailsWhileGrantingIsRolledBack()
        {
            service.Register("boom", _ => throw new InvalidOperationException("no space"));

            var result = service.Grant(new Reward(new RewardItem("gold", 100), new RewardItem("gold", 50), new RewardItem("boom", 1)));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(RewardFailureReason.Failed, result.Failures.Single().Reason);
            Assert.AreEqual("no space", result.Failures.Single().Message);
            Assert.AreEqual(0, wallet["gold"], "The gold that was already granted has to be taken back.");
            Assert.IsEmpty(result.Granted);
            CollectionAssert.AreEqual(new[] { "grant gold x100", "grant gold x50", "revoke gold x50", "revoke gold x100" }, log);
        }

        [Test]
        public void ItemsThatCannotBeTakenBackStayGranted()
        {
            service.Register("gems", item => log.Add("grant " + item));   // no revoke
            service.Register("boom", _ => throw new InvalidOperationException());

            var result = service.Grant(new Reward(new RewardItem("gems", 3), new RewardItem("gold", 10), new RewardItem("boom", 1)));

            Assert.IsTrue(result.IsPartial);
            Assert.AreEqual(1, result.Granted.Count);
            Assert.AreEqual("gems", result.Granted[0].Type);
            Assert.AreEqual(0, wallet["gold"]);
        }

        [Test]
        public void ARevokeThatFailsLeavesTheItemGranted()
        {
            service.Register("cursed", item => log.Add("grant " + item), revoke: _ => throw new InvalidOperationException());
            service.Register("boom", _ => throw new InvalidOperationException());

            var result = service.Grant(new Reward(new RewardItem("cursed", 1), new RewardItem("boom", 1)));

            Assert.IsTrue(result.IsPartial);
            Assert.AreEqual("cursed", result.Granted.Single().Type);
        }

        [Test]
        public void AFailedRewardDoesNotRaiseGranted()
        {
            var raised = 0;
            service.Granted += _ => raised++;

            service.Grant(new Reward(new RewardItem("nope", 1)));

            Assert.AreEqual(0, raised);
        }

        [Test]
        public void ValidateChecksWithoutGranting()
        {
            var ok = service.Validate(new Reward(new RewardItem("gold", 5)));
            var notOk = service.Validate(new Reward(new RewardItem("nope", 5)));

            Assert.IsTrue(ok.Success);
            Assert.IsFalse(notOk.Success);
            Assert.IsEmpty(wallet);
        }

        [Test]
        public void ACanGrantThatThrowsIsAFailureNotAnException()
        {
            service.Register("bad", _ => { }, canGrant: _ => throw new InvalidOperationException("broken"));

            var result = service.Validate(new Reward(new RewardItem("bad", 1)));

            Assert.AreEqual(RewardFailureReason.Failed, result.Failures.Single().Reason);
        }

        [Test]
        public void TryGrantReturnsTheResult()
        {
            Assert.IsTrue(service.TryGrant(new Reward(new RewardItem("gold", 1)), out var ok));
            Assert.IsTrue(ok.Success);

            Assert.IsFalse(service.TryGrant(new Reward(new RewardItem("nope", 1)), out var failed));
            Assert.IsFalse(failed.Success);
        }

        [Test]
        public void AnEmptyRewardSucceeds()
        {
            Assert.IsTrue(service.Grant(Reward.Empty).Success);
        }

        [Test]
        public void ATypeCanOnlyBeRegisteredOnce()
        {
            Assert.Throws<InvalidOperationException>(() => service.Register("gold", _ => { }));
            Assert.IsTrue(service.IsRegistered("gold"));

            Assert.IsTrue(service.Unregister("gold"));
            Assert.IsFalse(service.IsRegistered("gold"));
            Assert.IsFalse(service.Unregister("gold"));
            Assert.DoesNotThrow(() => service.Register("gold", _ => { }));
        }

        [Test]
        public void RegisteringNeedsATypeAndAHandler()
        {
            Assert.Throws<ArgumentException>(() => service.Register("", _ => { }));
            Assert.Throws<ArgumentNullException>(() => service.Register("x", (Action<RewardItem>)null));
            Assert.Throws<ArgumentNullException>(() => service.Register("x", (IRewardHandler)null));
        }

        [Test]
        public void ARewardIsRequired()
        {
            Assert.Throws<ArgumentNullException>(() => service.Grant(null));
            Assert.Throws<ArgumentNullException>(() => service.Validate(null));
        }

        [Test]
        public void HandlersCanBeWrittenAsClasses()
        {
            var handler = new EnergyHandler { Capacity = 5 };
            service.Register("energy", handler);

            Assert.IsTrue(service.Grant(new Reward(new RewardItem("energy", 3))).Success);
            Assert.AreEqual(3, handler.Energy);

            var overflow = service.Grant(new Reward(new RewardItem("energy", 3)));
            Assert.AreEqual(RewardFailureReason.Rejected, overflow.Failures.Single().Reason);
            Assert.AreEqual(3, handler.Energy);
        }

        private class EnergyHandler : IRewardHandler
        {
            public long Energy;
            public long Capacity;

            public bool CanGrant(RewardItem item) => Energy + item.Amount <= Capacity;

            public void Grant(RewardItem item) => Energy += item.Amount;
        }
    }
}
