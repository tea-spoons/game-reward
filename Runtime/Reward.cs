namespace TeaSpoons.GameReward
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An immutable bundle of <see cref="RewardItem"/>s.
    /// </summary>
    /// <remarks>
    /// Items with an amount of zero are dropped. Items need a type and must not be negative.
    /// The same kind of item may appear more than once; <see cref="Merged"/> combines them.
    /// </remarks>
    public sealed class Reward : IReadOnlyList<RewardItem>
    {
        private readonly RewardItem[] items;

        public Reward(IEnumerable<RewardItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var list = new List<RewardItem>();
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.Type))
                {
                    throw new ArgumentException("A reward item needs a type.", nameof(items));
                }

                if (item.Amount < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(items), item.ToString(), "A reward item cannot have a negative amount.");
                }

                if (item.Amount > 0)
                {
                    list.Add(item);
                }
            }

            this.items = list.ToArray();
        }

        public Reward(params RewardItem[] items) : this((IEnumerable<RewardItem>)items)
        {
        }

        public static Reward Empty { get; } = new Reward(Array.Empty<RewardItem>());

        public int Count => items.Length;

        public bool IsEmpty => items.Length == 0;

        public RewardItem this[int index] => items[index];

        /// <summary>
        /// The same reward with items of the same kind combined into one, in the order they first appear.
        /// </summary>
        public Reward Merged()
        {
            var merged = new List<RewardItem>();
            var indices = new Dictionary<(string, string), int>();

            foreach (var item in items)
            {
                var key = (item.Type, item.Id);
                if (indices.TryGetValue(key, out var index))
                {
                    merged[index] = merged[index].WithAmount(checked(merged[index].Amount + item.Amount));
                }
                else
                {
                    indices.Add(key, merged.Count);
                    merged.Add(item);
                }
            }

            return new Reward(merged);
        }

        /// <summary>
        /// This reward and <paramref name="other"/> together, with items of the same kind combined.
        /// </summary>
        public Reward Plus(Reward other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return new Reward(items.Concat(other.items)).Merged();
        }

        /// <summary>
        /// Every amount times <paramref name="factor"/>.
        /// </summary>
        public Reward Multiply(long factor)
        {
            if (factor < 0) throw new ArgumentOutOfRangeException(nameof(factor), factor, "The factor cannot be negative.");

            return new Reward(items.Select(item => item.WithAmount(checked(item.Amount * factor))));
        }

        /// <summary>
        /// Every amount times <paramref name="factor"/>, rounded. Items that round to zero are dropped.
        /// </summary>
        public Reward Scale(double factor, MidpointRounding rounding = MidpointRounding.AwayFromZero)
        {
            if (double.IsNaN(factor) || double.IsInfinity(factor) || factor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(factor), factor, "The factor must be a number of zero or more.");
            }

            return new Reward(items.Select(item => item.WithAmount(checked((long)Math.Round(item.Amount * factor, rounding)))));
        }

        /// <summary>
        /// The total amount of a kind of item in the reward.
        /// </summary>
        public long GetAmount(string type, string id = "")
        {
            var probe = new RewardItem(type, id, 0);

            long total = 0;
            foreach (var item in items)
            {
                if (item.IsSameKind(probe))
                {
                    total = checked(total + item.Amount);
                }
            }

            return total;
        }

        public bool Contains(string type, string id = "")
        {
            return GetAmount(type, id) > 0;
        }

        public IEnumerator<RewardItem> GetEnumerator()
        {
            return ((IEnumerable<RewardItem>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return items.Length == 0 ? "(empty)" : string.Join(", ", items);
        }
    }
}
