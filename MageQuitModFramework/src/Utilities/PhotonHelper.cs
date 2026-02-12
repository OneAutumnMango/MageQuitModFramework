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
        {
            SendRpc(rpcName, PhotonTargets.All, args);
        }

        /// <summary>
        /// Send an RPC to specified targets.
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="target">The PhotonTargets to send to (All, Others, MasterClient, etc.)</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpc(string rpcName, PhotonTargets target, params object[] args)
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

            if (!photonView.isMine)
            {
                Debug.LogWarning($"[PhotonRpcManager] Cannot send RPC '{rpcName}' - PhotonView is not owned by this client");
                return;
            }

            photonView.RPC("HandleRpc", target, rpcName, args);
        }

        /// <summary>
        /// Send an RPC using RPCLocal (executes locally immediately, then sends to network).
        /// </summary>
        /// <param name="rpcName">The name of the RPC channel</param>
        /// <param name="target">The PhotonTargets to send to</param>
        /// <param name="args">Arguments to pass to the RPC handlers</param>
        public void SendRpcLocal(string rpcName, PhotonTargets target, params object[] args)
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

            if (!photonView.isMine)
            {
                Debug.LogWarning($"[PhotonRpcManager] Cannot send RPC '{rpcName}' - PhotonView is not owned by this client");
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
            if (_rpcHandlers.ContainsKey(rpcName))
            {
                try
                {
                    _rpcHandlers[rpcName]?.Invoke(args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PhotonRpcManager] Error executing RPC handler '{rpcName}': {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning($"[PhotonRpcManager] No handler registered for RPC '{rpcName}'");
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
            if (manager == null)
            {
                manager = gameObject.AddComponent<PhotonRpcManager>();
            }

            // Ensure PhotonView exists
            PhotonView pv = gameObject.GetPhotonView();
            if (pv == null)
            {
                pv = gameObject.AddComponent<PhotonView>();
            }

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
            return obj != null ? obj.GetComponent<PhotonRpcManager>() : null;
        }
    }

    /// <summary>
    /// Static helper utilities for common Photon networking operations.
    /// </summary>
    public static class PhotonHelper
    {
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
            return photonView != null ? photonView.gameObject : null;
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
}
