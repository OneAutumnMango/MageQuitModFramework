using HarmonyLib;
using System;

namespace MageQuitModFramework.Data
{
    /// <summary>
    /// Observes and provides events for battle and round lifecycle in MageQuit.
    /// Subscribe to events using the Subscribe methods to avoid direct event access.
    /// </summary>
    [HarmonyPatch]
    public static class GameEventsObserver
    {
        // Game Data Loaded
        private static event Action _onGameDataLoaded;

        /// <summary>
        /// Whether game data has been loaded and is available for use.
        /// </summary>
        public static bool IsGameDataLoaded { get; private set; } = false;

        // Round Init
        private static event Action _onRoundInit;

        /// <summary>
        /// Whether the round init event has fired for the current round.
        /// </summary>
        public static bool IsRoundInit { get; private set; } = false;

        // Round Start
        private static event Action _onRoundStart;

        /// <summary>
        /// Whether the round start event has fired for the current round.
        /// </summary>
        public static bool IsRoundStarted { get; private set; } = false;

        // Round End
        private static event Action _onRoundEnd;

        /// <summary>
        /// Whether the round end event has fired for the current round.
        /// </summary>
        public static bool IsRoundEnded { get; private set; } = false;

        /// <summary>
        /// Subscribe to game data loaded event (SpellManager.Awake).
        /// Fires when the SpellManager initializes and spell data becomes available.
        /// Use this to safely access spell data that isn't available at plugin startup.
        /// </summary>
        public static void SubscribeToGameDataLoaded(Action callback)
        {
            if (callback != null)
                _onGameDataLoaded += callback;
        }

        /// <summary>
        /// Unsubscribe from game data loaded event.
        /// </summary>
        public static void UnsubscribeFromGameDataLoaded(Action callback)
        {
            if (callback != null)
                _onGameDataLoaded -= callback;
        }

        /// <summary>
        /// Subscribe to round initialization event (BattleManager.StartBattle).
        /// Fires before the round actually starts, when players aren't yet initialized.
        /// Use for setup that doesn't require active players or wizard controllers.
        /// </summary>
        public static void SubscribeToRoundInit(Action callback)
        {
            if (callback != null)
                _onRoundInit += callback;
        }

        /// <summary>
        /// Unsubscribe from round initialization event.
        /// </summary>
        public static void UnsubscribeFromRoundInit(Action callback)
        {
            if (callback != null)
                _onRoundInit -= callback;
        }

        /// <summary>
        /// Subscribe to round start event (BattleManager.StartBattle2).
        /// Fires when players are spawned and UI is set up.
        /// Note: Wizard controllers may need approximately 2 seconds after this event to fully initialize.
        /// Use for per-round setup that requires active players.
        /// </summary>
        public static void SubscribeToRoundStart(Action callback)
        {
            if (callback != null)
                _onRoundStart += callback;
        }

        /// <summary>
        /// Unsubscribe from round start event.
        /// </summary>
        public static void UnsubscribeFromRoundStart(Action callback)
        {
            if (callback != null)
                _onRoundStart -= callback;
        }

        /// <summary>
        /// Subscribe to round end event (BattleManager.EndBattle).
        /// Fires when a round concludes.
        /// Use for cleanup, resetting round-specific state, and reverting temporary changes.
        /// </summary>
        public static void SubscribeToRoundEnd(Action callback)
        {
            if (callback != null)
                _onRoundEnd += callback;
        }

        /// <summary>
        /// Unsubscribe from round end event.
        /// </summary>
        public static void UnsubscribeFromRoundEnd(Action callback)
        {
            if (callback != null)
                _onRoundEnd -= callback;
        }
        [HarmonyPatch(typeof(SpellManager), "Awake")]
        [HarmonyPostfix]
        static void OnGameDataLoaded()
        {
            IsGameDataLoaded = true;
            FrameworkPlugin.Log?.LogInfo("Game data loaded, firing OnGameDataLoaded event");
            _onGameDataLoaded?.Invoke();
        }
        [HarmonyPatch(typeof(BattleManager), nameof(BattleManager.StartBattle))]
        [HarmonyPostfix]
        static void OnRoundInit()
        {
            IsRoundInit = true;
            IsRoundStarted = false;
            IsRoundEnded = false;
            FrameworkPlugin.Log?.LogInfo("Round initializing, firing OnRoundInit event");
            _onRoundInit?.Invoke();
        }

        [HarmonyPatch(typeof(BattleManager), nameof(BattleManager.StartBattle2))]
        [HarmonyPostfix]
        static void OnRoundStart()
        {
            IsRoundStarted = true;
            FrameworkPlugin.Log?.LogInfo("Round started, firing OnRoundStart event");
            _onRoundStart?.Invoke();
        }

        [HarmonyPatch(typeof(BattleManager), nameof(BattleManager.EndBattle))]
        [HarmonyPostfix]
        static void OnRoundEnd()
        {
            IsRoundInit = false;
            IsRoundStarted = false;
            IsRoundEnded = true;
            FrameworkPlugin.Log?.LogInfo("Round ended, firing OnRoundEnd event");
            _onRoundEnd?.Invoke();
        }
    }
}
