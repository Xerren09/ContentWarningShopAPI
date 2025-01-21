# Changelog

All notable changes will be documented in this file.
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.3
- Minor patch that mainly affects the internals. The only outward change is removal of an unintended and difficult to access static property from SynchronisedMetadata class:
```csharp
public static bool InLobby -> public bool InLobby
public static bool IsHost -> public bool IsHost
```
  - These are still available as instance fields, but have been removed from the static class.

## 1.0.2
- Fix tooltips not being parsed correctly when using the fallback

## 1.0.1
- Fix `GetTootipData` patch exception when null tooltips are set on the Item, and no localised strings were provided.
  - This is unity's fault. Tooltips should be serialised correctly on Item resources, but they aren't loaded, so the count is correct but the text is null.
- Added `SetDefaultTooltips` extension method to set fallback tooltips.

## 1.0.0
- Initial Release