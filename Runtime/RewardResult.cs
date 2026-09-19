namespace TeaSpoons.GameReward
{
    using System.Collections.Generic;

    /// <summary>
    /// Why a <see cref="RewardItem"/> was not granted.
    /// </summary>
    public enum RewardFailureReason
    {
        /// <summary>No handler is registered for the type of the item.</summary>
        NoHandler,

        /// <summary>The handler said it cannot grant the item right now.</summary>
        Rejected,

        /// <summary>The handler threw an exception.</summary>
        Failed,
    }

    public readonly struct RewardFailure
    {
        public RewardFailure(RewardItem item, RewardFailureReason reason, string message)
        {
            Item = item;
            Reason = reason;
            Message = message;
        }

        public RewardItem Item { get; }

        public RewardFailureReason Reason { get; }

        public string Message { get; }

        public override string ToString()
        {
            return $"{Item}: {Reason} ({Message})";
        }
    }

    /// <summary>
    /// What <see cref="RewardService"/> did with a reward.
    /// </summary>
    public sealed class RewardResult
    {
        internal RewardResult(IReadOnlyList<RewardItem> granted, IReadOnlyList<RewardFailure> failures)
        {
            Granted = granted;
            Failures = failures;
        }

        /// <summary>
        /// Whether every item was granted.
        /// </summary>
        public bool Success => Failures.Count == 0;

        /// <summary>
        /// The items that were granted and are still granted. Empty when a failed reward was rolled back completely.
        /// </summary>
        public IReadOnlyList<RewardItem> Granted { get; }

        public IReadOnlyList<RewardFailure> Failures { get; }

        /// <summary>
        /// A failed reward of which some items could not be taken back.
        /// </summary>
        public bool IsPartial => !Success && Granted.Count > 0;
    }
}
