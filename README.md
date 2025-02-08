Content Warning Shop API
===
[![Steam Downloads](https://img.shields.io/steam/downloads/3408837293?style=flat-square&logo=steam&label=Downloads)](https://steamcommunity.com/sharedfiles/filedetails/?id=3408837293)
[![Steam Subscriptions](https://img.shields.io/steam/subscriptions/3408837293?style=flat-square&logo=steam&label=Subscriptions)](https://steamcommunity.com/sharedfiles/filedetails/?id=3408837293)
[![Steam Last Update Date](https://img.shields.io/steam/update-date/3408837293?style=flat-square&logo=steam&label=Updated)](https://steamcommunity.com/sharedfiles/filedetails/?id=3408837293)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Xerren/ShopAPI?style=flat-square&logo=thunderstore&label=Downloads)](https://thunderstore.io/c/content-warning/p/Xerren/ShopAPI/)
[![Thunderstore Likes](https://img.shields.io/thunderstore/likes/Xerren/ShopAPI?style=flat-square&logo=thunderstore&label=Likes)](https://thunderstore.io/c/content-warning/p/Xerren/ShopAPI/)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Xerren/ShopAPI?style=flat-square&logo=thunderstore&label=Version)](https://thunderstore.io/c/content-warning/p/Xerren/ShopAPI/)

Exposes an easy-to-use API to add custom items to the in-game shop. Loosely based on the now defunct [ShopUtils mod by hyydsz](https://github.com/hyydsz/ContentWarningShopUtils).

## Setup

Download one of the DLLs from the [releases page](https://github.com/Xerren09/ContentWarningShopAPI/releases) or use the one you already have if the mod is installed and add a reference to it in your project. If you are referencing a local DLL, be sure to set "Copy Local" to `No` to avoid distributing it with your mod, as this will cause issues.

> [!IMPORTANT]
> The DLLs available on the releases page are built for use with different mod loaders. **For development, use the one without a suffix**, regardless of if you are building your mod for the Steam Workshop (vanilla mod loader) or Thunderstore (BepInEx).
>
> The version suffixed with `.bepinex` expects BepInEx as a mod loader and will not work without it. It is there for manual installations but should not be used otherwise.
>
> In all releases of this mod the assembly's name will be without a suffix, so which version is loaded at runtime should not matter.

If you are publishing on the Steam Workshop (using the game's built-in mod loader) add [this workshop item ](https://steamcommunity.com/sharedfiles/filedetails/?id=3408837293) as your item's dependency via the "Add/Remove Required Items" option.

If you are using BepInEx as your modloader, make sure to add the `[BepInDependency("xerren.cwshopapi")]` attribute to your plugin's main class to ensure it is ready by the time your plugin is loaded. Also add the mod's [Dependency String](https://thunderstore.io/c/content-warning/p/Xerren/ShopAPI/) to your [`manifest.json`](https://thunderstore.io/c/content-warning/create/docs/) file if you are using Thunderstore so the mod manager can automatically fetch the mod.

## Usage

Once added as a reference to your project, all classes are available under the `ContentWarningShop` namespace. 

#### Creating items

Ideally `Item`s should be preconfigured and packed into an [`AssetBundle`](https://docs.unity3d.com/Manual/AssetBundlesIntro.html), but you can also construct them at runtime if its easier.

Custom `Item`s should have their `persistentID`, `price`, `purchasable`, `Category`, and `icon` properties set to work.

If you would like your item to have a chance to be randomly spawned in the Old World like other items, set `spawnable` to `true`, and `itemType` to `Item.ItemType.Tool`.

### Registering items

Items can be registered via the `RegisterItem` method:

```csharp
//using ContentWarningShop;

var yourCustomItem = yourAssetBundle.LoadAsset<Item>("yourItem");

Shop.RegisterItem(yourCustomItem);
Shop.RegisterCustomDataEntries();
```

> [!NOTE]
> Item prices are automatically synchronised between players on lobby join. The price set by the lobby's host will be used for the entire lobby. Once a lobby has been created, the price of a registered item can be updated via `UpdateItemPrice`.

You can check if a custom item has been already registered via the `IsItemRegistered` method. The list of **all** registered custom items is also available via the `CustomItems` property.

If your item uses custom `ItemDataEntry` types, call the `RegisterCustomDataEntries` method to fetch and register all custom types defined in your assembly. This will let the game automatically synchronise the items' custom state between players. (See [compatibility](#compatibility) if you run into issues)

### SynchronisedMetadata

The `SynchronisedMetadata<T>` class allows you to synchronise arbitrary settings between players through the use of [Steam Lobby Metadata](https://partner.steamgames.com/doc/features/multiplayer/matchmaking#6) keys. Simply create a new instance with a specific type and key and it will be automatically updated whenever the key's value is changed.

> [!TIP]
> Consider prepending your mod's GUID to the key to ensure it won't accidentally collide with a different mod. 

For example to synchronise a simple boolean setting with `false` as the initial value:
```csharp
public static readonly SynchronisedMetadata<bool> ExampleSetting = new("ExampleSetting", false);
```
Instances will remain valid between different lobbies, so once bound to a key they can be safely kept in a static property and used for the entire duration of the game. 

To update the value, call `SetValue(T value)`. Only the lobby's host may update the value of a key, so the method returns a boolean indicating if the set was allowed. If it was rejected, the instance's value isn't updated. The `CanSet` method can be used to check if the current player has permission to update the setting.

> [!IMPORTANT]  
> Values are converted to strings when passed on to the steam lobby, so make sure your type can be cast to string and back.

When a key is successfully updated either locally or remotely, the `ValueChanged` event will be raised with the new value. Since instances are valid for the lifetime of the game, this event can also be safely used anywhere in your plugin.

> [!NOTE]
> When not currently in a lobby, setting the value is permitted as if the current player was the host, and the `ValueChanged` event will still be raised.

In the scenario that you have separate player and lobby settings, and you want to apply the local player's settings when they host a new lobby, make sure to subscribe to the `LobbyHosted` event and overwrite the current value. This ensures that any values set by a previous lobby will be replaced with your player's settings:

```csharp
ExampleSetting.LobbyHosted += () => {
    ExampleSetting.SetValue(SomeContentWarningSetting.Value);
};
```

### Localisation

The game's built-in localisation implementation is not extendable, so a custom solution is included with the mod under the `ContentWarningShop.Localisation` namespace. This patches `Item.GetLocalizedDisplayName`, `Item.GetTootipData`, and the `ShopItem`'s constructor.

Use the `ShopLocalisation` class to add localised strings to your items. Each string is represented as a key-value pair assigned to a specific locale. For built in strings such as display name and tooltips, the item's unity object name (filename) is used or prefixed. For example, to localise an `Item` with the object name "Spookbox", the key would also be simply "Spookbox".

When adding locale strings, use the constants defined in the `LocaleKeys` static class to retrieve a locale used by the game via the `ShopLocalisation.TryGetLocale` method. These locales are guaranteed to be available:

```csharp
ShopLocalisation.TryGetLocale(LocaleKeys.English, out UnityEngine.Localization.Locale locale);
```

The returned standard Unity Locale object can then be used via the `AddLocaleString` extension method to register a key-value pair:

```csharp
locale?.AddLocaleString("Spookbox_ToolTips", $"{ShopLocalisation.UseGlyphString} Play;{ShopLocalisation.Use2GlyphString} Next Track");
```

Note that when localising item tooltips, the key must be the item's name suffixed with `_ToolTips` (`ShopLocalisation.TooltipsSuffix`), and the value must be a `;` separated list. To display action glyphs (for example "[Left Click] Toggle", etc) use the glyph strings defined on the `ShopLocalisation` class in your strings, and the appropriate icon will be inserted into the tooltip by the game:

| Const | Glyph |
| -------- | ------- |
| UseGlyph | Left click |
| Use2Glyph | Right click |
| SelfieGlyph | R (Default) |
| ZoomGlyph | Scroll wheel |

> [!IMPORTANT]
> If you don't want to add full localisation ( :( ), use the `SetDefaultTooltips` extension method on your `Item` to set default tooltips. 
> If you set tooltips in the editor, they won't work: this is a bug on Unity's end, not this mod. (those tooltips are serialised to null when you save them, even if they look right in the inspector)
> Setting a default is recommended in any case, but especially if you don't- or only partially provide localisation.

## Compatibility

This mod patches `ItemInstanceData`'s `GetEntryIdentifier` and `GetEntryType` methods which are used by the game to serialise and deserialise items when synchronising state between players. Unfortunately, the IDs are hardcoded and there is no way to "reserve" one for a specific type, which means two mods patching these same methods can interpret the same values as their own entries incorrectly. To avoid most (hopefully all) collisions like this, entry IDs used by this mod are counted backwards, from `byte.MaxValue`. However, just to be safe the `Shop` class exposes a `MaxUsedEntryID` property that returns the lowest entry ID it uses, above which all other IDs are reserved.

> [!WARNING]  
> `MaxUsedEntryID` will not be accurate until all other mods have initialised and registered their items. Try to check for this value as late as possible, after all other mods are loaded.

Obviously if you aren't already using this mod, you don't want to require it just for this; use this snippet to check if this mod is in use and attempt to fetch this value as a "soft" dependency:

```csharp
using System.Reflection;

private static byte GetMaxShopReservedID() 
{
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    var target = Array.Find(assemblies, a => a.GetName().Name == "ShopAPI");
    if (target != null)
    {
        var t = target.GetType("ContentWarningShop.Shop");
        var prop = t.GetProperty("MaxUsedEntryID", BindingFlags.Public | BindingFlags.Static);
        var val = prop.GetValue(null);
        if (val != null)
        {
            return (byte)val;
        }
    }
    else
    {
        Debug.Log($"Neither the steam or modman version of ShopAPI is loaded; assuming unaltered ItemInstanceData entry registry.");
    }
    return 0;
}
```
