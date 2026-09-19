namespace TeaSpoons.GameReward
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A set of rewards with weights, to pick one at random: a loot table.
    /// </summary>
    public sealed class WeightedRewardTable
    {
        private readonly List<(Reward Reward, double Weight)> entries = new();

        public int Count => entries.Count;

        public double TotalWeight { get; private set; }

        /// <summary>
        /// Adds a reward. A reward with weight 3 is rolled three times as often as one with weight 1.
        /// </summary>
        public WeightedRewardTable Add(Reward reward, double weight)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            if (double.IsNaN(weight) || double.IsInfinity(weight) || weight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "The weight must be a number above zero.");
            }

            entries.Add((reward, weight));
            TotalWeight += weight;
            return this;
        }

        /// <summary>
        /// The chance (0 to 1) that <see cref="Roll"/> gives the reward at <paramref name="index"/>.
        /// </summary>
        public double GetChance(int index)
        {
            return entries[index].Weight / TotalWeight;
        }

        /// <summary>
        /// Picks a reward. Pass a seeded <see cref="Random"/> to get repeatable results.
        /// </summary>
        /// <exception cref="InvalidOperationException">The table is empty.</exception>
        public Reward Roll(Random random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (entries.Count == 0) throw new InvalidOperationException("The table has no rewards.");

            var roll = random.NextDouble() * TotalWeight;
            foreach (var (reward, weight) in entries)
            {
                roll -= weight;
                if (roll < 0)
                {
                    return reward;
                }
            }

            return entries[entries.Count - 1].Reward;
        }
    }
}
