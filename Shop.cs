using UnityEngine;
using System.Reflection;
using Zorro.Core;
using System.Collections.ObjectModel;

namespace ContentWarningShop;

public static class Shop
{
    /// <summary>
    /// The number of <see cref="ItemDataEntry"/> types in the base game. Also the maximum vanilla ID. 
    /// </summary>
    internal static byte _vanillaEntryCount = 0;
    /// <summary>
    /// The list of registered custom entry types deriving from <see cref="ItemDataEntry"/>.
    /// </summary>
    internal static List<Type> _customEntries = new();
    /// <summary>
    /// The list of registered custom items. Use <see cref="CustomItems"/> for a readonly wrapper.
    /// </summary>
    internal static List<Item> _items = new();
    /// <summary>
    /// The list of registered custom items.
    /// </summary>
    public static ReadOnlyCollection<Item> CustomItems => _items.AsReadOnly();
    /// <summary>
    /// Gets the entry ID above which all other identifiers are reserved by this mod. Exposed for potential compatibility with other mods that wish to also register entries separately from this mod.
    /// </summary>
    /// <remarks>
    /// Try to get this value as late as possible, preferably sometime after it is guaranteed that all mods have been loaded and initialised,
    /// otherwise it may not be accurate.
    /// </remarks>
    public static byte MaxUsedEntryID => (byte)(byte.MaxValue - (byte)_customEntries.Count);

    static Shop()
    {
        GetVanillaItemDataEntryCount();
        PriceSynchroniser.RegisterCallbacks();
    }

    /// <summary>
    /// Gets if a custom item was already registered in the api.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool IsItemRegistered(Item item)
    {
        return _items.Contains(item);
    }

    /// <summary>
    /// Registers an item if it wasn't already registered.
    /// </summary>
    /// <remarks>
    /// For items that define and use their own custom <see cref="ItemDataEntry"/> types, 
    /// call <see cref="RegisterCustomDataEntries()"/> during initialisation to ensure they are also registered.
    /// </remarks>
    /// <param name="item"></param>
    public static void RegisterItem(Item item)
    {
        if (IsItemRegistered(item) == true)
        {
            Debug.LogWarning($"Item {item.displayName} ({item.persistentID}) already registered.");
            return;
        }
        if (item.Category == ShopItemCategory.Invalid)
        {
            throw new Exception($"Item {item.displayName} ({item.persistentID}) shop category is set to {nameof(ShopItemCategory.Invalid)}.");
        }
        _items.Add(item);
        SingletonAsset<ItemDatabase>.Instance.AddRuntimeEntry(item);
        Debug.Log($"Registered custom item: {item.displayName} ({item.persistentID}) [{Assembly.GetCallingAssembly().GetSimpleName()}].");
    }

    /// <summary>
    /// Finds and registers all types deriving from <see cref="ItemDataEntry"/> in the current mod.
    /// </summary>
    /// <remarks>
    /// The current mod is based on <see cref="Assembly.GetCallingAssembly()"/>'s result.
    /// <para>
    /// Note that other mods that interact with this system might break AND break this mod as well, 
    /// as there is no way to specifically assign an ID to a given entry type. 
    /// See <see href="https://github.com/Xerren09/ContentWarningShopAPI/#compatibility"/> for more information.
    /// </para>
    /// </remarks>
    public static void RegisterCustomDataEntries()
    {
        var assembly = Assembly.GetCallingAssembly();
        var entries = GetItemDataEntries(assembly);
        foreach (var item in entries)
        {
            if (_customEntries.Contains(item))
            {
                continue;
            }
            if (_customEntries.Count >= (byte.MaxValue - GetVanillaItemDataEntryCount()))
            {
                throw new Exception($"Custom ItemDataEntry {item.Name} from {assembly.GetSimpleName()} can not be registered, as its index would overlap with vanilla entries. This is not necessarily an issue with your mod, but the player may have too many custom items installed. The game only supports {byte.MaxValue} entries total, out of which {GetVanillaItemDataEntryCount()} are reserved.");
            }
            _customEntries.Add(item);
            Debug.Log($"Added custom entry type: {item.Name} [{assembly.GetSimpleName()}] - idx: {byte.MaxValue - (_customEntries.Count-1)}");
        }
    }

    /// <summary>
    /// Updates the given item's price and synchronises it with other players.
    /// </summary>
    /// <remarks>
    /// Setting lobby metadata is only allowed if the current player is the lobby's owner. Use the return value to determine if the set was possible.
    /// </remarks>
    /// <param name="item"></param>
    /// <param name="price"></param>
    /// <returns>
    /// <see langword="true"/> if updating the price was successful (player is not in a lobby OR the owner of the current lobby).
    /// <see langword="false"/> if the player is in a lobby but is not the lobby's host.
    /// </returns>
    public static bool UpdateItemPrice(Item item, int price)
    {
        if (IsItemRegistered(item) == false)
        {
            Debug.LogWarning($"Item {item.name} ({item.persistentID}) is not registered with {ShopApiPlugin.MOD_NAME}");
            return false;
        }
        if (SteamLobbyMetadataHandler.IsHost == false && SteamLobbyMetadataHandler.InLobby)
        {
            Debug.LogError($"Tried updating item price when not the lobby host; this is not allowed.");
            return false;
        }
        item.price = price;
        PriceSynchroniser.SyncPrice(item);
        return true;
    }

    /// <summary>
    /// Returns the list of item data data entries derived from <see cref="ItemDataEntry"/> in the given assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    internal static Type[] GetItemDataEntries(Assembly assembly)
    {
        List<Type> ret = new();
        Type[] types = Array.Empty<Type>();
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Debug.LogError(ex.Message);
            return Array.Empty<Type>();
        }
        var sourceType = typeof(ItemDataEntry);
        foreach (Type type in types)
        {
            if (type.IsSubclassOf(sourceType))
            {
                if (ret.Contains(type) == false)
                {
                    ret.Add(type);
                }
            }
        }
        return ret.ToArray();
    }

    /// <summary>
    /// Gets the number of types deriving from <see cref="ItemDataEntry"/> in the base game.
    /// </summary>
    /// <remarks>
    /// The value returned by this method is cached after the first run.
    /// </remarks>
    /// <returns></returns>
    internal static byte GetVanillaItemDataEntryCount()
    {
        if (_vanillaEntryCount == 0)
        {
            var types = GetItemDataEntries(typeof(ItemDataEntry).Assembly);
            _vanillaEntryCount = (byte)types.Length;
            Debug.Log($"Vanilla data entries found: {_vanillaEntryCount} -> max vanilla ID: {_vanillaEntryCount}");
        }
        return _vanillaEntryCount;
    }
}
