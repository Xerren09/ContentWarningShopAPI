# Changelog

All notable changes will be documented in this file.
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.2.0
- DLLs are now correctly versioned
- Added default value constructor to SynchronisedMetadata
- SynchronisedMetadata now implements IDisposable
  - Disconnect() is now obsolete but calls Dispose()
- Added LobbyHosted event to SynchronisedMetadata
  - Invoked when the local player successfully creates a new lobby
  - Use this to overwrite any remnant settings from a previous lobby
- `ShopItem` now correctly uses localised item names whenever available
- Items registered to the shop via the mod will now be considered when spawning random items in the Old World
  - For an item to be eligible, `spawnable` must be true and `itemType` must be `Item.ItemType.Tool`

## 1.1.0
- Fixed an issue where all synchronisation broke after leaving a lobby and joining a new one in the same session.

- Added `UpdateItemPrice` method. Use this to update the price of items anytime during a game, and they'll be synchronised between players.
	- This will also reload the store, so the current cart gets reset.

- Removed mistakenly marked `static` fields from `SynchronisedMetadata`. These are now instance fields:
```csharp
public static bool InLobby -> public bool InLobby
public static bool IsHost -> public bool IsHost
```

- Reworked `SynchronisedMetadata` internals

## 1.0.2
- Fix tooltips not being parsed correctly when using the fallback

## 1.0.1
- Fix `GetTootipData` patch exception when null tooltips are set on the Item, and no localised strings were provided.
  - This is unity's fault. Tooltips should be serialised correctly on Item resources, but they aren't loaded, so the count is correct but the text is null.
- Added `SetDefaultTooltips` extension method to set fallback tooltips.

## 1.0.0
- Initial Release