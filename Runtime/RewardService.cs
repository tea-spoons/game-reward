namespace TeaSpoons.GameReward
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Grants <see cref="Reward"/>s through the handlers registered for the types of their items.
    /// </summary>
    /// <remarks>
    /// A reward is all or nothing as far as the service can tell: it first asks the handler of every item whether it
    /// can grant it, and grants nothing if any of them cannot. If a handler still fails while granting, the items granted
    /// before it are revoked, as far as their handlers implement <see cref="IRevocableRewardHandler"/>.
    /// </remarks>
    public sealed class RewardService
    {
        private readonly Dictionary<string, IRewardHandler> handlers = new(StringComparer.Ordinal);

        /// <summary>
        /// Raised after a whole reward was granted.
        /// </summary>
        public event Action<Reward> Granted;

        /// <summary>
        /// Registers the handler for items of <paramref name="type"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">There already is a handler for the type.</exception>
        public void Register(string type, IRewardHandler handler)
        {
            if (string.IsNullOrEmpty(type)) throw new ArgumentException("A type is required.", nameof(type));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!handlers.TryAdd(type, handler))
            {
                throw new InvalidOperationException($"A handler is already registered for reward type '{type}'.");
            }
        }

        /// <summary>
        /// Registers a handler made of delegates.
        /// </summary>
        /// <param name="grant">Adds the item to the game.</param>
        /// <param name="canGrant">Whether the item can be added now. Without it every item can.</param>
        /// <param name="revoke">Takes the item back. Without it a reward that fails half way cannot undo this type.</param>
        public void Register(string type, Action<RewardItem> grant, Func<RewardItem, bool> canGrant = null, Action<RewardItem> revoke = null)
        {
            if (grant == null) throw new ArgumentNullException(nameof(grant));

            Register(type, revoke == null
                ? new DelegateHandler(grant, canGrant)
                : new RevocableDelegateHandler(grant, canGrant, revoke));
        }

        public bool Unregister(string type)
        {
            return type != null && handlers.Remove(type);
        }

        public bool IsRegistered(string type)
        {
            return type != null && handlers.ContainsKey(type);
        }

        /// <summary>
        /// Checks whether <paramref name="reward"/> could be granted now, without granting anything.
        /// </summary>
        public RewardResult Validate(Reward reward)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));

            var failures = new List<RewardFailure>();
            foreach (var item in reward)
            {
                if (!handlers.TryGetValue(item.Type, out var handler))
                {
                    failures.Add(new RewardFailure(item, RewardFailureReason.NoHandler, $"No handler is registered for reward type '{item.Type}'."));
                    continue;
                }

                try
                {
                    if (!handler.CanGrant(item))
                    {
                        failures.Add(new RewardFailure(item, RewardFailureReason.Rejected, $"The handler of '{item.Type}' cannot grant this now."));
                    }
                }
                catch (Exception exception)
                {
                    failures.Add(new RewardFailure(item, RewardFailureReason.Failed, exception.Message));
                }
            }

            return new RewardResult(Array.Empty<RewardItem>(), failures);
        }

        /// <summary>
        /// Grants <paramref name="reward"/>. Never throws for a failing handler, look at the result instead.
        /// </summary>
        public RewardResult Grant(Reward reward)
        {
            var validation = Validate(reward);
            if (!validation.Success)
            {
                return validation;
            }

            var granted = new List<RewardItem>(reward.Count);
            foreach (var item in reward)
            {
                try
                {
                    handlers[item.Type].Grant(item);
                    granted.Add(item);
                }
                catch (Exception exception)
                {
                    var failures = new List<RewardFailure> { new RewardFailure(item, RewardFailureReason.Failed, exception.Message) };
                    Rollback(granted);
                    return new RewardResult(granted, failures);
                }
            }

            Granted?.Invoke(reward);
            return new RewardResult(granted, Array.Empty<RewardFailure>());
        }

        public bool TryGrant(Reward reward, out RewardResult result)
        {
            result = Grant(reward);
            return result.Success;
        }

        // Takes back what it can, latest first. What is left in the list could not be taken back.
        private void Rollback(List<RewardItem> granted)
        {
            for (var i = granted.Count - 1; i >= 0; i--)
            {
                if (handlers[granted[i].Type] is not IRevocableRewardHandler revocable)
                {
                    continue;
                }

                try
                {
                    revocable.Revoke(granted[i]);
                    granted.RemoveAt(i);
                }
                catch (Exception)
                {
                    // Stays in the list: it is still granted.
                }
            }
        }

        private class DelegateHandler : IRewardHandler
        {
            private readonly Action<RewardItem> grant;
            private readonly Func<RewardItem, bool> canGrant;

            public DelegateHandler(Action<RewardItem> grant, Func<RewardItem, bool> canGrant)
            {
                this.grant = grant;
                this.canGrant = canGrant;
            }

            public bool CanGrant(RewardItem item) => canGrant == null || canGrant(item);

            public void Grant(RewardItem item) => grant(item);
        }

        private sealed class RevocableDelegateHandler : DelegateHandler, IRevocableRewardHandler
        {
            private readonly Action<RewardItem> revoke;

            public RevocableDelegateHandler(Action<RewardItem> grant, Func<RewardItem, bool> canGrant, Action<RewardItem> revoke)
                : base(grant, canGrant)
            {
                this.revoke = revoke;
            }

            public void Revoke(RewardItem item) => revoke(item);
        }
    }
}
