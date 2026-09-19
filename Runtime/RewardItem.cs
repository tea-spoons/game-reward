namespace TeaSpoons.GameReward
{
    using System;
    using UnityEngine;

    /// <summary>
    /// An amount of one kind of thing: 100 <c>gold</c>, 1 <c>item</c> with id <c>sword</c>, 3 <c>energy</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="Type"/> picks the <see cref="IRewardHandler"/> that grants it, <see cref="Id"/> tells things of one type
    /// apart (leave it empty when the type is enough).
    /// </remarks>
    [Serializable]
    public struct RewardItem : IEquatable<RewardItem>
    {
        [SerializeField]
        private string type;

        [SerializeField]
        private string id;

        [SerializeField]
        private long amount;

        public RewardItem(string type, string id, long amount)
        {
            this.type = type;
            this.id = id;
            this.amount = amount;
        }

        public RewardItem(string type, long amount) : this(type, string.Empty, amount)
        {
        }

        public string Type => type ?? string.Empty;

        public string Id => id ?? string.Empty;

        public long Amount => amount;

        /// <summary>
        /// Whether both items are the same thing, regardless of their amounts.
        /// </summary>
        public bool IsSameKind(RewardItem other)
        {
            return string.Equals(Type, other.Type, StringComparison.Ordinal) &&
                   string.Equals(Id, other.Id, StringComparison.Ordinal);
        }

        public RewardItem WithAmount(long newAmount)
        {
            return new RewardItem(Type, Id, newAmount);
        }

        public bool Equals(RewardItem other)
        {
            return IsSameKind(other) && amount == other.amount;
        }

        public override bool Equals(object obj)
        {
            return obj is RewardItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Id, amount);
        }

        public static bool operator ==(RewardItem left, RewardItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RewardItem left, RewardItem right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Id.Length > 0 ? $"{Type}:{Id} x{amount}" : $"{Type} x{amount}";
        }
    }
}
