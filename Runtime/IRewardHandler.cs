namespace TeaSpoons.GameReward
{
    /// <summary>
    /// Grants one type of <see cref="RewardItem"/> to the game: adds the gold, puts the item into the inventory.
    /// Register it on a <see cref="RewardService"/>.
    /// </summary>
    public interface IRewardHandler
    {
        /// <summary>
        /// Whether <paramref name="item"/> can be granted right now, for example because the inventory has room.
        /// </summary>
        /// <remarks>
        /// The service asks this for every item of a reward before it grants any of them.
        /// </remarks>
        bool CanGrant(RewardItem item);

        void Grant(RewardItem item);
    }

    /// <summary>
    /// A handler that can take back what it granted. The service uses it to undo a reward when a later item of the
    /// same reward fails.
    /// </summary>
    public interface IRevocableRewardHandler : IRewardHandler
    {
        void Revoke(RewardItem item);
    }
}
