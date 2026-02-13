using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

namespace MageQuitModFramework.Utilities
{
    /// <summary>
    /// A manager class that provides a simplified interface for handling Photon RPCs through named channels.
    /// Allows registering callback handlers for RPC events, eliminating the need to write individual RPC methods.
    /// </summary>
    public class PhotonRpcManager : Photon.MonoBehaviour
    {
        private Dictionary<string, Action<object[]>> _rpcHandlers = new Dictionary<string, Action<object[]>>();

        /// <summary>
        /// Register a handler for a named RPC channel.
        /// Multiple handlers can be registered to the same channel name.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="callback">The callback to invoke when the RPC is received. Parameters are passed as object array.</param>
        public void RegisterHandler(string rpcName, Action<object[]> callback)
        {
            if (string.IsNullOrEmpty(rpcName))
            {
                Debug.LogError("[PhotonRpcManager] Cannot register handler with null or empty RPC name");
                return;
            }

            if (callback == null)
            {
                Debug.LogError("[PhotonRpcManager] Cannot register null callback");
                return;
            }

            if (_rpcHandlers.ContainsKey(rpcName))
                _rpcHandlers[rpcName] += callback;
            else
                _rpcHandlers[rpcName] = callback;
        }

        /// <summary>
        /// Unregister a specific handler for a named RPC channel.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="callback">The callback to remove</param>
        public void UnregisterHandler(string rpcName, Action<object[]> callback)
        {
            if (_rpcHandlers.ContainsKey(rpcName))
            {
                _rpcHandlers[rpcName] -= callback;
                if (_rpcHandlers[rpcName] == null)
                    _rpcHandlers.Remove(rpcName);
            }
        }

        /// <summary>
        /// Clear all handlers for a specific RPC channel.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel to clear</param>
        public void ClearHandlers(string rpcName)
        {
            if (_rpcHandlers.ContainsKey(rpcName))
                _rpcHandlers.Remove(rpcName);
        }

        /// <summary>
        /// Clear all registered RPC handlers.
        /// </summary>
        public void ClearAllHandlers()
        {
            _rpcHandlers.Clear();
        }

        /// <summary>
        /// Send an RPC to all clients.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpc(string rpcName, params object[] args)
            => SendRpc(rpcName, PhotonTargets.All, args);

        /// <summary>
        /// Send an RPC to specified targets.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="target">The PhotonTargets to send to (All, Others, MasterClient, etc.)</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpc(string rpcName, PhotonTargets target, params object[] args)
            => SendRpc(rpcName, target, false, args);


        /// <summary>
        /// Send an RPC to specified targets with ownership control.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="target">The PhotonTargets to send to (All, Others, MasterClient, etc.)</param>
        /// <param name="ownerOnly">If true, only the PhotonView owner can send</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpc(string rpcName, PhotonTargets target, bool ownerOnly, params object[] args)
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogWarning($"[PhotonRpcManager] Cannot send RPC '{rpcName}' - not connected to Photon network");
                return;
            }

            if (photonView == null)
            {
                Debug.LogError("[PhotonRpcManager] PhotonView is missing! Cannot send RPC.");
                return;
            }

            if (ownerOnly && !photonView.isMine)
            {
                Debug.LogWarning($"[PhotonRpcManager] Cannot send RPC '{rpcName}' - Only owner can send this RPC");
                return;
            }

            photonView.RPC("HandleRpc", target, rpcName, args);
        }

        /// <summary>
        /// Send an RPC with offline fallback. If connected to Photon, sends the RPC to network targets.
        /// If offline, executes the handler locally only.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="target">The PhotonTargets to send to when online</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpcLocal(string rpcName, PhotonTargets target, params object[] args)
            => SendRpcLocal(rpcName, target, false, args);

        /// <summary>
        /// Send an RPC with offline fallback and ownership control. If connected to Photon, sends the RPC to network targets.
        /// If offline, executes the handler locally only.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="target">The PhotonTargets to send to when online</param>
        /// <param name="ownerOnly">If true, only the PhotonView owner can send</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpcLocal(string rpcName, PhotonTargets target, bool ownerOnly, params object[] args)
        {
            if (!PhotonNetwork.connected)
            {
                HandleRpc(rpcName, args);
                return;
            }

            if (photonView == null)
            {
                Debug.LogError("[PhotonRpcManager] PhotonView is missing! Cannot send RPC.");
                return;
            }

            if (ownerOnly && !photonView.isMine)
            {
                Debug.LogWarning($"[PhotonRpcManager] Cannot send RPC '{rpcName}' - Only owner can send this RPC");
                return;
            }

            photonView.RPC(nameof(HandleRpc), target, rpcName, args);
        }

        /// <summary>
        /// Internal RPC handler that dispatches to registered callbacks.
        /// This method is called by Photon when an RPC is received.
        /// </summary>
        [PunRPC]
        private void HandleRpc(string rpcName, object[] args)
        {
            if (!_rpcHandlers.ContainsKey(rpcName))
            {
                Debug.LogWarning($"[PhotonRpcManager] No handler registered for RPC '{rpcName}'");
                return;
            }

            try
            {
                _rpcHandlers[rpcName]?.Invoke(args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhotonRpcManager] Error executing RPC handler '{rpcName}': {ex.Message}\n{ex.StackTrace}");
            }
        }


        /// <summary>
        /// Creates a persistent PhotonRpcManager GameObject that survives scene changes.
        /// </summary>
        /// <param name="name">Optional name for the GameObject (default: "PhotonRpcManager")</param>
        /// <param name="viewID">Optional PhotonView ID to assign. If not provided, a view ID must be allocated separately.</param>
        /// <returns>The created PhotonRpcManager instance</returns>
        public static PhotonRpcManager CreatePersistent(string name = "PhotonRpcManager", int? viewID = null)
        {
            GameObject obj = new GameObject(name);
            PhotonRpcManager manager = obj.AddComponent<PhotonRpcManager>();
            DontDestroyOnLoad(obj);

            PhotonView pv = obj.GetComponent<PhotonView>();
            pv ??= obj.AddComponent<PhotonView>();

            if (viewID.HasValue)
            {
                pv.viewID = viewID.Value;
            }

            return manager;
        }

        /// <summary>
        /// Creates a PhotonRpcManager attached to an existing GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to attach the manager to</param>
        /// <returns>The created PhotonRpcManager instance</returns>
        public static PhotonRpcManager CreateOn(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogError("[PhotonRpcManager] Cannot create manager on null GameObject");
                return null;
            }

            PhotonRpcManager manager = gameObject.GetComponent<PhotonRpcManager>();
            manager ??= gameObject.AddComponent<PhotonRpcManager>();

            // Ensure PhotonView exists
            PhotonView pv = gameObject.GetPhotonView();
            pv ??= gameObject.AddComponent<PhotonView>();

            return manager;
        }

        /// <summary>
        /// Find an existing PhotonRpcManager by GameObject name.
        /// </summary>
        /// <param name="name">The name of the GameObject to search for</param>
        /// <returns>The PhotonRpcManager instance if found, null otherwise</returns>
        public static PhotonRpcManager Find(string name)
        {
            GameObject obj = GameObject.Find(name);
            return obj?.GetComponent<PhotonRpcManager>();
        }
    }

    /// <summary>
    /// Static helper utilities for common Photon networking operations.
    /// </summary>
    public static class PhotonHelper
    {
        private static Dictionary<byte, Action<object[]>> _eventHandlers = new Dictionary<byte, Action<object[]>>();
        private static PhotonEventListener _globalEventListener;

        /// <summary>
        /// Initialize the global Photon event listening system.
        /// Safe to call multiple times—only initializes once.
        /// </summary>
        public static void InitializeEventSystem()
        {
            if (_globalEventListener != null)
            {
                Debug.Log("[PhotonHelper] Event system already initialized");
                return;
            }

            GameObject listenerObj = new GameObject("PhotonEventListener");
            _globalEventListener = listenerObj.AddComponent<PhotonEventListener>();
            UnityEngine.Object.DontDestroyOnLoad(listenerObj);
            Debug.Log("[PhotonHelper] Global event system initialized");
        }

        /// <summary>
        /// Register a handler for a custom Photon event.
        /// </summary>
        /// <param name="eventCode">The event code (0-199 reserved for Photon, 200+ for custom)</param>
        /// <param name="callback">The callback to invoke when the event is received</param>
        public static void RegisterEventHandler(byte eventCode, Action<object[]> callback)
        {
            if (callback == null)
            {
                Debug.LogError($"[PhotonHelper] Cannot register null callback for event {eventCode}");
                return;
            }

            if (!_eventHandlers.ContainsKey(eventCode))
            {
                _eventHandlers[eventCode] = callback;
                Debug.Log($"[PhotonHelper] Registered event handler for event code {eventCode} (total handlers: {_eventHandlers.Count})");
            }
            else
            {
                _eventHandlers[eventCode] += callback;
                Debug.Log($"[PhotonHelper] Added additional handler for event code {eventCode}");
            }
        }

        /// <summary>
        /// Unregister a handler for a custom Photon event.
        /// </summary>
        /// <param name="eventCode">The event code</param>
        /// <param name="callback">The callback to remove</param>
        public static void UnregisterEventHandler(byte eventCode, Action<object[]> callback)
        {
            if (_eventHandlers.ContainsKey(eventCode))
            {
                _eventHandlers[eventCode] -= callback;
                if (_eventHandlers[eventCode] == null)
                    _eventHandlers.Remove(eventCode);
            }
        }

        /// <summary>
        /// Raise a custom Photon event to all clients or specific targets.
        /// </summary>
        /// <param name="eventCode">The event code (use 200+ for custom events)</param>
        /// <param name="data">Event data parameters</param>
        /// <param name="targets">Which clients to send to (defaults to All)</param>
        public static void RaiseEvent(byte eventCode, object[] data, ReceiverGroup targets = ReceiverGroup.All)
        {
            Debug.Log($"[PhotonHelper] RaiseEvent called with eventCode={eventCode}, targets={targets}");

            if (!PhotonNetwork.connected)
            {
                Debug.LogWarning($"[PhotonHelper] Cannot raise event {eventCode} - not connected to Photon network");
                // Still invoke locally so offline testing works
                if (_eventHandlers.TryGetValue(eventCode, out Action<object[]> action))
                {
                    action?.Invoke(data);
                }
                return;
            }

            Debug.Log($"[PhotonHelper] About to call PhotonNetwork.RaiseEvent with eventCode={eventCode}, receivers={targets}");
            PhotonNetwork.RaiseEvent(eventCode, data, true, new RaiseEventOptions { Receivers = targets });
            Debug.Log($"[PhotonHelper] Event {eventCode} raised to {targets}");
        }

        /// <summary>
        /// Internal handler for all custom Photon events. Attach this to a PhotonBehaviour.
        /// </summary>
        public static void OnEvent(byte eventCode, object content, int playerNr)
        {
            Debug.Log($"[PhotonHelper] OnEvent called: eventCode={eventCode}, playerNr={playerNr}, hasHandler={_eventHandlers.ContainsKey(eventCode)}");

            if (_eventHandlers.TryGetValue(eventCode, out Action<object[]> action))
            {
                try
                {
                    var args = content is object[] arr ? arr : [content];
                    Debug.Log($"[PhotonHelper] Invoking handler for event {eventCode} with {args.Length} args");
                    action?.Invoke(args);
                    Debug.Log($"[PhotonHelper] Handler for event {eventCode} completed");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PhotonHelper] Error executing handler for event {eventCode}: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning($"[PhotonHelper] No handler registered for event {eventCode}. Available handlers: {string.Join(", ", _eventHandlers.Keys)}");
            }
        }

        /// <summary>
        /// Checks if we're currently connected to the Photon network.
        /// </summary>
        public static bool IsOnline()
        {
            return PhotonNetwork.connected;
        }

        /// <summary>
        /// Checks if we're the master client.
        /// </summary>
        public static bool IsMasterClient()
        {
            return PhotonNetwork.isMasterClient;
        }

        /// <summary>
        /// Gets the number of players currently in the room.
        /// </summary>
        public static int GetPlayerCount()
        {
            return PhotonNetwork.room != null ? PhotonNetwork.room.PlayerCount : 0;
        }

        /// <summary>
        /// Gets the PhotonView ID of a GameObject safely.
        /// </summary>
        public static int GetPhotonViewID(GameObject gameObject)
        {
            if (gameObject == null) return -1;
            var photonView = gameObject.GetPhotonView();
            return photonView != null ? photonView.viewID : -1;
        }

        /// <summary>
        /// Finds a GameObject by its PhotonView ID.
        /// </summary>
        public static GameObject FindByPhotonViewID(int viewID)
        {
            var photonView = PhotonView.Find(viewID);
            return photonView?.gameObject;
        }

        /// <summary>
        /// Checks if the local client owns the specified GameObject's PhotonView.
        /// </summary>
        public static bool IsOwner(GameObject gameObject)
        {
            if (gameObject == null) return false;
            var photonView = gameObject.GetPhotonView();
            return photonView != null && photonView.isMine;
        }

        /// <summary>
        /// Transfers ownership of a GameObject's PhotonView to the local client.
        /// </summary>
        public static bool TransferOwnership(GameObject gameObject)
        {
            if (gameObject == null) return false;
            var photonView = gameObject.GetPhotonView();
            if (photonView == null || photonView.isMine) return false;

            photonView.TransferOwnership(PhotonNetwork.player);
            return true;
        }
    }

    /// <summary>
    /// Internal MonoBehaviour that listens for Photon events and routes them to PhotonHelper handlers.
    /// Created automatically by PhotonHelper.InitializeEventSystem().
    /// </summary>
    public class PhotonEventListener : Photon.MonoBehaviour
    {
        private void OnEnable()
        {
            PhotonNetwork.OnEventCall += HandlePhotonEvent;
            Debug.Log("[PhotonEventListener] OnEnable: subscribed to PhotonNetwork.OnEventCall");
        }

        private void OnDisable()
        {
            PhotonNetwork.OnEventCall -= HandlePhotonEvent;
            Debug.Log("[PhotonEventListener] OnDisable: unsubscribed from PhotonNetwork.OnEventCall");
        }

        private void HandlePhotonEvent(byte eventCode, object content, int senderID)
        {
            Debug.Log($"[PhotonEventListener] HandlePhotonEvent: eventCode={eventCode}, senderID={senderID}");
            PhotonHelper.OnEvent(eventCode, content, senderID);
        }
    }
}
