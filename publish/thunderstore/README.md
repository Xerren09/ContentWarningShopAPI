Content Warning Shop API
===
Exposes an easy to use API to add custom items to the in-game shop. Loosely based on the now defunct [ShopUtils mod by hyydsz](https://github.com/hyydsz/ContentWarningShopUtils).

**If you are a developer, see the mod's [GitHub repository](https://github.com/Xerren09/ContentWarningShopAPI/#compatibility) for detailed setup instructions and more information.**

## Usage

Once added as a reference to your project, all classes are available under the `ContentWarningShop` namespace. 

### Registering items

Use the `RegisterItem` method to register an `Item` to the shop. Ideally the item instance is preconfigured and loaded from an AssetBundle, but you can also construct it during runtime.
Make sure to set the `persistentID`, `price`, `purchasable`, `Category`, and `icon` properties at the very least to ensure the item will show up correctly in the store.

> NOTE:
> Item prices are automatically synchronised between players on lobby join. The price set by the lobby's host will be used for the entire lobby. Once a lobby has been created, the price of a registered item can be updated via `UpdateItemPrice`.

You can check if a custom item has been already registered via the `IsItemRegistered` method. The list of **all** registered custom items is also available via the `CustomItems` property.

If your item uses custom `ItemDataEntry` types, call the `RegisterCustomDataEntries` method to fetch and register all custom types defined in your assembly. This will let the game automatically synchronise the items' custom state between players. (See [compatibility](#compatibility) if you run into issues)

### SynchronisedMetadata

The `SynchronisedMetadata<T>` class allows you to synchronise arbitrary settings between players through the use of [Steam Lobby Metadata](https://partner.steamgames.com/doc/features/multiplayer/matchmaking#6) keys. Simply create a new instance with a specific type and key and it will be automatically updated whenever the key's value is changed.

> TIP:
> Consider prepending your mod's GUID to the key to ensure it won't accidentally collide with a different mod. 

For example to synchronise a simple boolean setting with `false` as the initial value:
```csharp
public static readonly SynchronisedMetadata<bool> ExampleSetting = new("ExampleSetting", false);
```
`SynchronisedMetadata` instances remain valid between different lobbies, so once bound to a key they can be safely kept in a static property and used for the entire duration of the game.

To update the value, call `SetValue(T value)`. Only the lobby's host may update the value of a key, so the method returns a boolean indicating if the set was allowed. If it was rejected, the instance's value isn't updated. Use the `CanSet` method to check if the current player has permission to update the setting.

> IMPORTANT:
> Values are converted to strings when passed on to the steam lobby, so make sure your type can be cast to string and back.

When a key is successfully updated either locally or remotely, the `ValueChanged` event will be raised with the new value. Since instances are valid for the lifetime of the game, this event can also be safely used anywhere in your plugin.

> NOTE:
> When not currently in a lobby, setting the value is permitted as if the current player was the host, and the `ValueChanged` event will still be raised.

### Localisation

The game's built-in localisation implementation is not extendable, so a custom solution is included with the mod under the `ContentWarningShop.Localisation` namespace. This patches `Item.GetLocalizedDisplayName` and `Item.GetTootipData`.

Use the `ShopLocalisation` class to add localised strings to your items. Each string is represented as a key-value pair assigned to a specific locale. For built in strings such as display name and tooltips, the item's unity object name (filename) is used or prefixed. For example to localise the Item "Spookbox" the key would be simply also "Spookbox".

When adding locale strings, use the constants defined in the `LocaleKeys` static class to retreive a locale used by the game via the `ShopLocalisation.TryGetLocale` method:

```csharp
ShopLocalisation.TryGetLocale(LocaleKeys.English, out UnityEngine.Localization.Locale locale);
```

The returned standard Unity Locale object can then be used via the `AddLocaleString` extension method to register a key-value pair:

```csharp
locale?.AddLocaleString("Spookbox_ToolTips", $"{ShopLocalisation.UseGlyphString} Play;{ShopLocalisation.Use2GlyphString} Next Track");
```

Note that when localising item tooltips the key must be the item's name, suffixed with `_ToolTips` (`ShopLocalisation.TooltipsSuffix`), and the value must be a `;` separated list. To display action glyphs (such as right mouse button, etc) use the included constants in your strings, and the appropriate icon will be inserted into the tooltip by the game:

| Const | Glyph |
| -------- | ------- |
| UseGlyph | Left click |
| Use2Glyph | Right click |
| SelfieGlyph | R (Default) |
| ZoomGlyph | Scroll wheel |

> IMPORTANT:
> If you don't want to add localisation ( :( ), use the `SetDefaultTooltips` extension method on your `Item` to set default tooltips. 
> If you set tooltips in the editor, they won't work: this is a bug on Unity's end, not this mod. (those tooltips are serialised to null when you save them, even if they look right in the inspector)
> Setting a default is recommended in any case, but especially if you don't or only partially provide localisation.

## Compatibility

This mod patches `ItemInstanceData`'s `GetEntryIdentifier` and `GetEntryType` methods which may cause issues with other mods that do the same. The library exposes a property to help avoid these issues, which can be read without requiring this mod as a dependency. See the repository's [compatibility](https://github.com/Xerren09/ContentWarningShopAPI/#compatibility) section for more information.