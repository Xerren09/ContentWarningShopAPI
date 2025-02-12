Content Warning Shop API
===
Exposes an easy-to-use API to add custom items to the in-game shop in Content Warning.

See the mod's [GitHub repository](https://github.com/Xerren09/ContentWarningShopAPI/) for detailed setup instructions and usage information.

## Features

* **Custom items:** Opens up the in-game shop to allow custom items to be added. This includes automatically synchronising item prices between players, and registering custom `ItemDataEntry` types defined in your mod to interface with the game's built-in item synchroniser.
* **Synchronisation:** Easily synchronise any arbitrary settings between players in a lobby using [Steam Lobby Metadata](https://partner.steamgames.com/doc/features/multiplayer/matchmaking#6) keys via the `SynchronisedMetadata<T>` class.
* **Localisation:** Extends and opens up the game's localisation system to allow items to be translated to the supported locales by patching item and shop related localised methods.

## Integration

**DO NOT** bundle the mod's DLL with your own. Ensure that no `ShopAPI.dll` is included with your build.

Depending on your publishing target, you should instead require it as a dependency on your publishing platform:

### Steam Workshop

When publishing on the Steam Workshop, add [this Workshop Item ](https://steamcommunity.com/sharedfiles/filedetails/?id=3408837293) as your item's dependency via the "Add/Remove Required Items" option (on your mod's page right hand side panel). 

Steam will ensure that the dependency will load before your mod when the game is launched.

### Thunderstore (BepInEx)

When building a BepInEx plugin, add this mod as a dependency to your plugin's main file:

```csharp
[BepInDependency(ShopApiPlugin.MOD_GUID)]
public class YourCustomPlugin : BaseUnityPlugin
{
    // ...
}
```

If a specific version is needed, pass `ShopApiPlugin.MOD_VER` after the GUID.

When publishing on Thunderstore, add the mod's [Dependency String](https://thunderstore.io/c/content-warning/p/Xerren/ShopAPI/) to the [manifest.json](https://thunderstore.io/c/content-warning/create/docs/) file, so mod managers can automatically fetch it.

## Usage

Once added as a reference to your project, all classes are available under the `ContentWarningShop` namespace. 

### Creating items

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

> **NOTE:**
> Item prices are automatically synchronised between players on lobby join. The price set by the lobby's host will be used for the entire lobby.

You can check if a custom item has been already registered via the `IsItemRegistered` method. The list of **all** registered *custom* items is also available via the `CustomItems` property.

If your item uses custom `ItemDataEntry` types, call the `RegisterCustomDataEntries` method to fetch and register all custom types defined in your assembly. This will let the game automatically synchronise the items' custom state between players. (See [compatibility](#compatibility) if you run into issues) 
It is enough call this method once per assembly / mod.

### Updating item prices

If you need to update an item's price at any point after a lobby has been started, use the `UpdateItemPrice` method:

```csharp
var success = Shop.UpdateItemPrice(yourCustomItem, price);
```

This will re-synchronise the item's price to every player. Note that only the lobby's host can update an item's price, so the method returns a boolean indicating if it was successful (if the local player had permissions).

### Synchronising Settings

The `SynchronisedMetadata<T>` class allows arbitrary settings to be synchronised between players through the use of [Steam Lobby Metadata](https://partner.steamgames.com/doc/features/multiplayer/matchmaking#6) keys. Simply create a new instance with a specific type and key and it will be automatically updated whenever the key's value is changed.

> **TIP:**
> Consider prepending your mod's GUID to the key to ensure it won't accidentally collide with a different mod. 

For example to synchronise a simple boolean setting with `false` as the initial value:
```csharp
public static readonly SynchronisedMetadata<bool> ExampleSetting = new("ExampleSetting", false);
```
An instance bound to a key will remain valid even if the player changes lobbies, so they can be kept for the entire run-time of the game.

To update a setting's value, call `SetValue(T value)`. Only the lobby's host may update the value of a key, so the method returns a boolean indicating if the set was allowed. If it was rejected, the instance's value isn't updated. Use `CanSet` method to check if the current player has permission to update the setting.

The `ValueChanged` event will be raised with the new (current) value when a key is updated either locally or remotely.

> **IMPORTANT:**
> Values are converted to strings when passed on to the steam lobby, so make sure your type can be cast to string and back.

> **NOTE:**
> When not currently in a lobby, setting the value is permitted as if the current player was the host, and the `ValueChanged` event will still be raised.

In the scenario that you have separate player and lobby settings, and you want to apply the local player's settings when they host a new lobby, make sure to subscribe to the `LobbyHosted` event and overwrite the current value. This ensures that any values set by a previous lobby will be replaced with your player's settings:

```csharp
ExampleSetting.LobbyHosted += () => {
    ExampleSetting.SetValue(SomeContentWarningSetting.Value);
};
```

#### Other members

##### Properties

* `IsHost`: Returns true if the local player is the lobby host.
* `InLobby`: Returns true if the local player is in a lobby.
* `Key`: The key this instance is bound to.
* `Value`: The current value of the key.
* `IsSynced`: Returns true if the key is currently synched with a lobby.
* `IsConnected`: Returns true if this instance is connected to the Steamworks API and will receive events. Will only return false if `Dispose()` was called.

##### Events
* `ValueChanged`: Invoked when the instance's value is sucessfully updated either locally or remotely.
* `LobbyHosted`: Invoked when the current (local) player has sucessfully created a new lobby.

#### Disposing

`SynchronisedMetadata` implements `IDisposable`, so if for some reason an instance is no longer needed, call `Dispose()`. This will cause it to no longer receive any events from the Steamworks API, and any subsequent instance method calls will throw `ObjectDisposedException`.

Check the `IsConnected` property to see if an instance was disposed.

### Localisation

The game's built-in localisation implementation is not extendable, so a custom solution is included with the mod under the `ContentWarningShop.Localisation` namespace. This patches the `Item.GetLocalizedDisplayName` and `Item.GetTootipData` methods, and `ShopItem`'s constructor.

Use the `ShopLocalisation` class to add localised strings to your items. Each string is represented as a key-value pair assigned to a specific locale. For built-in strings such as display name and tooltips, the item's Unity `Object.name` (filename) is used or prefixed. For example, to localise an item with the object name "Spookbox", the key would also be simply "Spookbox".

When adding localised strings to a locale, use the constants defined in the `LocaleKeys` static class to retrieve a locale supported by the game via the `ShopLocalisation.TryGetLocale` method. These locales are guaranteed to be available:

```csharp
ShopLocalisation.TryGetLocale(LocaleKeys.English, out UnityEngine.Localization.Locale locale);
```

The returned standard Unity Locale object can then be used via the `AddLocaleString` extension method to register a key-value pair:

```csharp
locale?.AddLocaleString("Spookbox_ToolTips", $"{ShopLocalisation.UseGlyphString} Play;{ShopLocalisation.Use2GlyphString} Next Track");
```

When localising item tooltips, the key must be the item's name suffixed with `_ToolTips` (`ShopLocalisation.TooltipsSuffix`), and the value must be a `;` delimited list. To display action glyphs (for example "[Left Click] Toggle", etc) use the glyph strings defined on the `ShopLocalisation` class in your strings, and the appropriate icon will be inserted into the tooltip by the game:

| Const | Glyph |
| -------- | ------- |
| ShopLocalisation.UseGlyph | Left click |
| ShopLocalisation.Use2Glyph | Right click |
| ShopLocalisation.SelfieGlyph | R (Default) |
| ShopLocalisation.ZoomGlyph | Scroll wheel |

> **IMPORTANT:**
> If you don't want to add full localisation ( :( ), use the `SetDefaultTooltips` extension method on your `Item` to set default tooltips. 
> If you set tooltips in the editor, they won't work: this is a bug on Unity's end, not this mod. (those tooltips are serialised to null when you save them, even if they look right in the inspector)
> Setting a default is recommended in any case, but especially if you don't- or only partially provide localisation.

## Compatibility

This mod patches `ItemInstanceData`'s `GetEntryIdentifier` and `GetEntryType` methods which are used by the game to serialise and deserialise items when synchronising state between players. Unfortunately, the IDs are hardcoded and there is no way to "reserve" one for a specific type, which means two mods patching these same methods can interpret the same values as their own entries incorrectly. To avoid most (hopefully all) collisions like this, entry IDs used by this mod are counted backwards, from `byte.MaxValue`. However, just to be safe the `Shop` class exposes a `MaxUsedEntryID` property that returns the lowest entry ID it uses, above which all other IDs are reserved.

> **WARNING:**
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
        Debug.Log($"No ShopAPI is loaded; assuming unaltered ItemInstanceData entry registry.");
    }
    return 0;
}
```

## Reporting Issues

Encountered a bug or issue? Please let me know by opening a [new Issue over on GitHub](https://github.com/Xerren09/ContentWarningSpookbox/issues): briefly describe the issue, include any potential error messages, and any relevant context or (un)expected behavior.