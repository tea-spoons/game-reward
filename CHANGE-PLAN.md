# Change plan

> Draft. This file tracks what I plan to change next. Edit freely.

## Origin

Written from scratch by me (Muhammad Tarek Abdou) in 2026 and released under the MIT license. It is not derived from
any employer's or other project's code.

## Planned changes

- [x] Tag and publish `v0.1.0` with the Release workflow.
- [ ] Reward conditions and multipliers (for example a double-rewards event) as a wrapper around `Reward.Multiply`.
- [ ] A drawer that shows a `RewardItem` on one line in the Inspector.
- [ ] Async handlers (for rewards that need a server round trip) with the same all-or-nothing behaviour.
- [ ] Try older Unity versions than 6000.3 (only 6000.3 is tested).
<!-- review-items:start -->
- [ ] **P1** Add a `WeightedRewardTableAsset` ScriptableObject that references `RewardDefinition`s with weights.
- [ ] **P1** Add `Roll(count, withoutReplacement)` to the table.
- [ ] **P1** Run the tests in CI. The kit's `run-tests` needs a Unity project, so this waits for package-mode support in `unity-ci-kit` (planned there; GameCI's test runner has a `packageMode` for the same reason).
- [ ] **P2** Add a guaranteed/pity option (a table that forces a rare drop after N misses), with a serializable counter.
- [ ] **P2** Add amount ranges to reward items (a minimum and maximum rolled with the seeded `Random`).
- [ ] **P2** Offer a clamping variant of the arithmetic next to the throwing one.
<!-- review-items:end -->

<!-- review:start -->
## Review (September 2026)

Reviewed as a senior Unity engineer would: I read the code and compared the package with similar open-source projects (September 2026). Those projects are listed for ideas only. Nothing was copied from them, and their licenses are noted in case code is ever reused. Priorities: **P0** correctness bug or broken metadata, **P1** should be done soon, **P2** nice to have.

### Compared with

| Project | License | Worth noting |
|---|---|---|
| [Kellojo/Unity-Simple-Loot-Table](https://github.com/Kellojo/Unity-Simple-Loot-Table) | not checked | A loot table where you bring your own item classes; tables are managed in an editor UI. |
| [Balastrong/random-loot-table-video](https://github.com/Balastrong/random-loot-table-video) | not checked | Scripts for a weighted loot table. |

### Findings from reading the code

- **[Gap]** `WeightedRewardTable` can only be built in code. `RewardDefinition` is an asset, but there is no asset for a table, so designers cannot author loot tables.
- **[Gap]** A table rolls one reward with replacement. There is no "roll N without repeats", no guaranteed or pity drop, and no amount ranges.
- **[Overflow]** `Reward.Merged`, `Plus`, `Multiply` and `Scale` use `checked` arithmetic, so an overflow throws `OverflowException` (tested). Decide whether a game wants a throw or a clamp.
<!-- review:end -->

## Notes and ideas

_Add your own here._
