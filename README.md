# Game Reward

Rewards as data. A reward is a bundle of typed items ("100 gold", "1 sword"). You register a small handler per item type
that knows how to add it to *your* game, and a service grants whole rewards through those handlers, all or nothing.

## What's in it

- **`RewardItem`** is a type, an optional id and an amount. **`Reward`** is an immutable bundle of items with `Merged`,
  `Plus`, `Multiply` and `Scale`.
- **`RewardService`** grants rewards through the handlers you register per item type. It checks every item first,
  grants nothing if one cannot be granted, and takes back what it already granted if a handler fails half way.
- **`IRewardHandler`** / **`IRevocableRewardHandler`**: implement them, or register plain delegates.
- **`WeightedRewardTable`** picks one reward at random by weight (loot tables), with a seedable `Random`.
- **`RewardDefinition`** is a reward as an asset (**Assets > Create > TeaSpoons > Game Reward > Reward**).

## Example

```csharp
using TeaSpoons.GameReward;

var service = new RewardService();

// One registration per item type. `revoke` is optional; it lets a failed reward be undone.
service.Register("gold",
    grant:  item => wallet.Add(item.Amount),
    revoke: item => wallet.Remove(item.Amount));

service.Register("item",
    grant:    item => inventory.Add(item.Id, (int)item.Amount),
    canGrant: item => inventory.HasRoomFor(item.Id, (int)item.Amount));

var reward = new Reward(new RewardItem("gold", 100), new RewardItem("item", "sword", 1));

if (!service.TryGrant(reward, out var result))
{
    foreach (var failure in result.Failures)
    {
        Debug.LogWarning(failure);    // "item:sword x1: Rejected (The handler of 'item' cannot grant this now.)"
    }
}
```

### How a grant works

1. Every item must have a handler and that handler must say `CanGrant`. If not, **nothing is granted** and the result lists
   every failing item (`NoHandler`, `Rejected` or `Failed`).
2. Items are granted in order. If a handler throws, the items granted before it are revoked, latest first, as far as their
   handlers are `IRevocableRewardHandler`. What could not be revoked stays in `result.Granted` and `result.IsPartial` is true.
3. After a complete success `RewardService.Granted` is raised.

`Grant` never throws because of a failing handler, you always get a `RewardResult`.

### Loot tables

```csharp
var table = new WeightedRewardTable()
    .Add(new Reward(new RewardItem("gold", 50)), weight: 70)
    .Add(new Reward(new RewardItem("gold", 500)), weight: 25)
    .Add(new Reward(new RewardItem("item", "crown", 1)), weight: 5);

Reward drop = table.Roll(new System.Random(seed));
```

## Requirements

- Unity 6000.3 (developed and tested there; older versions were not tested)
- No dependencies.

## Installation

In Unity: **Window > Package Manager > + > Add package from git URL**, then enter:

```
https://github.com/tea-spoons/game-reward.git
```

Pin a release by appending a tag, for example `#v0.1.0`.

## Works with

These packages are optional. When your project has them, they get extra features (Unity detects them automatically):

| Package | Adds |
|---|---|
| [Localizer](https://github.com/tea-spoons/localizer) (0.1.0+) | `GetName` and `Describe` for items and rewards: the name of an item is the text under `reward.{type}.{id}` (or `reward.{type}`), so `Describe` gives "100x Gold, 1x Sword" in the current language. |
| [Game Task](https://github.com/tea-spoons/game-task) | Tasks can name a reward that is granted when the task is claimed. |

## Change plan

See [CHANGE-PLAN.md](CHANGE-PLAN.md) for what is planned next.

## License

[MIT](LICENSE.md), © Muhammad Tarek Abdou.
