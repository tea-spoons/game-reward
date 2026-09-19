namespace TeaSpoons.GameReward
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A reward authored as an asset: a list of items to edit in the Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "TeaSpoons/Game Reward/Reward", fileName = "Reward")]
    public class RewardDefinition : ScriptableObject
    {
        [SerializeField]
        private List<RewardItem> items = new();

        public IReadOnlyList<RewardItem> Items => items;

        /// <summary>
        /// Replaces the items. Useful when a definition is built in code.
        /// </summary>
        public void SetItems(IEnumerable<RewardItem> newItems)
        {
            items = new List<RewardItem>(newItems);
        }

        /// <summary>
        /// The reward the asset describes.
        /// </summary>
        /// <exception cref="System.ArgumentException">An item has no type.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">An item has a negative amount.</exception>
        public Reward ToReward()
        {
            return new Reward(items);
        }
    }
}
