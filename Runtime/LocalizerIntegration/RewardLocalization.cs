namespace TeaSpoons.GameReward
{
    using System.Linq;
    using TeaSpoons.Localizer;

    /// <summary>
    /// Names and describes rewards in the current language. Only compiled when the Localizer package is in the project.
    /// </summary>
    /// <remarks>
    /// The name of an item is the text under the key <c>reward.{type}.{id}</c>, or <c>reward.{type}</c> for an item
    /// without an id. The prefix (<c>reward</c>) can be changed. An item without a text is named after its id, or its type.
    /// </remarks>
    public static class RewardLocalization
    {
        public const string DefaultKeyPrefix = "reward";

        /// <summary>
        /// The key of the name of the item.
        /// </summary>
        public static string GetKey(this RewardItem item, string keyPrefix = DefaultKeyPrefix)
        {
            return item.Id.Length > 0
                ? keyPrefix + "." + item.Type + "." + item.Id
                : keyPrefix + "." + item.Type;
        }

        /// <summary>
        /// The name of the item, for example "Gold".
        /// </summary>
        public static string GetName(this RewardItem item, Translator translator = null, string keyPrefix = DefaultKeyPrefix)
        {
            translator ??= Translator.Default;

            if (translator.TryGet(item.GetKey(keyPrefix), out var name))
            {
                return name;
            }

            return item.Id.Length > 0 ? item.Id : item.Type;
        }

        /// <summary>
        /// The amount and name of the item, for example "100x Gold".
        /// </summary>
        public static string Describe(this RewardItem item, Translator translator = null, string keyPrefix = DefaultKeyPrefix)
        {
            return item.Amount + "x " + item.GetName(translator, keyPrefix);
        }

        /// <summary>
        /// The items of the reward described one after the other, for example "100x Gold, 1x Sword".
        /// </summary>
        public static string Describe(this Reward reward, Translator translator = null, string keyPrefix = DefaultKeyPrefix)
        {
            return string.Join(", ", reward.Select(item => item.Describe(translator, keyPrefix)));
        }
    }
}
