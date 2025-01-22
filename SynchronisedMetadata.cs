using Steamworks;
using System.Globalization;
using UnityEngine;

namespace ContentWarningShop
{
    /// <summary>
    /// Handles synchronisation of a Steam Lobby metadata entry. When a lobby metadata key has been updated,
    /// <see cref="Value"/> will automatically be updated with the new value.
    /// </summary>
    /// <remarks>
    /// Setting lobby metadata is only allowed if the current player is the lobby's owner. 
    /// Use <see cref="CanSet"/> to check if assigning a new value is possible from this client.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class SynchronisedMetadata<T> where T : IConvertible, IComparable
    {
        /// <summary>
        /// Get if the player is the host of the current lobby. Will be <see langword="false"/> if <see cref="InLobby"/> is.
        /// </summary>
        public bool IsHost => SteamLobbyMetadataHandler.IsHost;
        /// <summary>
        /// Get if the player is in a Steam Lobby.
        /// </summary>
        public bool InLobby => SteamLobbyMetadataHandler.InLobby;
        /// <summary>
        /// The Steam Lobby Metadata key this instance is bound to.
        /// </summary>
        public string Key { get; protected set; } = string.Empty;
        private T _value = default;
        /// <summary>
        /// The current value of this entry.
        /// </summary>
        /// <remarks>
        /// Check <see cref="IsSynced"/> to see if the value is being actively synced with a lobby.
        /// </remarks>
        public T Value
        {
            get => _value;
        }
        /// <summary>
        /// Checks if the entry is being actively synced with a lobby.
        /// </summary>
        public bool IsSynced
        {
            get
            {
                if (IsConnected == false)
                {
                    return false;
                }
                return SteamLobbyMetadataHandler.InLobby;
            }
        }
        /// <summary>
        /// Whether this instance will automatically synchronise its key's value or not. 
        /// Once <see cref="Disconnect"/> was called, the instance will no longer update its value, and a new instance must be created to reconnect.
        /// </summary>
        public bool IsConnected { get; protected set; } = true;

        /// <summary>
        /// Event raised when <see cref="Value"/> is updated, either locally or remotely.
        /// </summary>
        public event Action<T>? ValueChanged;

        /// <param name="key">The Steam Lobby Metadata key this instance will synchronise with.</param>
        /// <param name="initialValue">
        /// The initial value that should be used before the first sync. 
        /// If the instance is created while in a lobby, it will first attempt to fetch the current lobby value.
        /// If the key doesn't exist yet, it will attempt to create it with this value (or the first time a lobby is joined).
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public SynchronisedMetadata(string key, T initialValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (key.Length > Steamworks.Constants.k_nMaxLobbyKeyLength)
            {
                throw new ArgumentException($"Key length must not exceed {nameof(Steamworks.Constants.k_nMaxLobbyKeyLength)} ({Steamworks.Constants.k_nMaxLobbyKeyLength}); was {key.Length}", nameof(key));
            }
            SteamLobbyMetadataHandler.OnLobbyDataUpdate += OnLobbyUpdate;
            SteamLobbyMetadataHandler.OnLobbyJoined += OnLobbyJoin;
            Key = key;
            _value = initialValue;
            // If we are creating the instance late, check if the key already has a registered value or not. If not, create it, if yes, fetch it.
            if (SteamLobbyMetadataHandler.InLobby)
            {
                var valStr = SteamMatchmaking.GetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, Key);
                if (string.IsNullOrEmpty(valStr))
                {
                    SetValue(_value);
                }
                else
                {
                    FetchValue();
                }
            }
            Debug.Log($"{nameof(SynchronisedMetadata<T>)} instance bound to lobby key: {Key}");
        }

        /// <summary>
        /// Checks if assigning a new value is possible from this client.
        /// </summary>
        /// <remarks>
        /// If the client is not in a lobby, returns true, otherwise checks if the player is the lobby's host.
        /// </remarks>
        /// <returns></returns>
        public bool CanSet()
        {
            return IsConnected == true && (InLobby == false || IsHost);
        }

        /// <summary>
        /// Sets the entry to a new value.
        /// </summary>
        /// <remarks>
        /// Setting a new value is only possible by the host of the lobby, or if the player in not in a lobby.
        /// </remarks>
        /// <param name="value"></param>
        /// <returns>
        /// <see langword="true"/> if assigning the new value from this client was possible, <see langword="false"/> if not.
        /// </returns>
        public bool SetValue(T value)
        {
            if (IsConnected == false)
            {
                return false;
            }
            if (InLobby == false)
            {
                _value = value;
                ValueChanged?.Invoke(_value);
                return true;
            }
            var canSet = CanSet();
            if (canSet)
            {
                SteamMatchmaking.SetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, Key, ValToString(value));
            }
            return canSet;
        }

        /// <summary>
        /// Disconnects this instance from updates, preventing it from synchronising. 
        /// This renders this instance useless; create a new instance to reconnect.
        /// </summary>
        public void Disconnect()
        {
            IsConnected = false;
            SteamLobbyMetadataHandler.OnLobbyDataUpdate -= OnLobbyUpdate;
            SteamLobbyMetadataHandler.OnLobbyJoined -= OnLobbyJoin;
        }

        private void OnLobbyJoin()
        {
            if (IsHost)
            {
                if (SteamMatchmaking.SetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, Key, ValToString(_value)))
                {
                    Debug.Log($"Set join lobby metadata {Key} to {_value} as host.");
                }
            }
            else
            {
                FetchValue();
            }
        }

        private void OnLobbyUpdate()
        {
            FetchValue();
        }

        /// <summary>
        /// Fetches the most up-to-date value from the lobby if possible.
        /// </summary>
        protected void FetchValue()
        {
            if (InLobby == false)
            {
                return;
            }
            var valStr = SteamMatchmaking.GetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, Key);
            if (string.IsNullOrEmpty(valStr))
            {
                return;
            }
            var val = StringToVal<T>(valStr);
            if (val != null && val.Equals(_value) == false)
            {
                Debug.Log($"Synced from lobby metadata {Key}: {_value} -> {val}");
                _value = val;
                ValueChanged?.Invoke(_value);
            }
        }

        private static string ValToString(object value)
        {
            return (string)Convert.ChangeType(value, typeof(string), CultureInfo.InvariantCulture);
        }

        private static TRes StringToVal<TRes>(string value)
        {
            return (TRes)Convert.ChangeType(value, typeof(TRes), CultureInfo.InvariantCulture);
        }
    }
}
