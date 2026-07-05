using UnityEngine;
using TowerDefense.Core.Events;

namespace TowerDefense.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game State config")]
        [SerializeField] private int _startingBaseHealth = 20;
        [SerializeField] private int _startingGold = 100;

        private int _currentBaseHealth;
        private int _currentGold;
        private bool _isGameOver;

        // Singleton instance
        public static GameManager Instance { get; private set; }

        public int CurrentBaseHealth => _currentBaseHealth;
        public int CurrentGold => _currentGold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _currentBaseHealth = _startingBaseHealth;
            _currentGold = _startingGold;
            _isGameOver = false;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GoalReachedEvent>(OnGoalReached);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<GameWinEvent>(OnGameWin);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoalReachedEvent>(OnGoalReached);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<GameWinEvent>(OnGameWin);
        }

        private void OnGoalReached(GoalReachedEvent evt)
        {
            if (_isGameOver) return;

            _currentBaseHealth -= evt.damageToBase;
            _currentBaseHealth = Mathf.Max(_currentBaseHealth, 0);

#if UNITY_EDITOR
            Debug.Log($"Base HP: {_currentBaseHealth}");
#endif

            if (_currentBaseHealth <= 0)
            {
                _isGameOver = true;
                EventBus.Publish(new GameOverEvent());
                
#if UNITY_EDITOR
                Debug.Log("GAME OVER");
#endif
            }
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (_isGameOver) return;

            _currentGold += evt.goldReward;

#if UNITY_EDITOR
            Debug.Log($"Gold: {_currentGold}");
#endif
        }

        private void OnGameWin(GameWinEvent evt)
        {
            if (_isGameOver) return;

            _isGameOver = true;

#if UNITY_EDITOR
            Debug.Log("YOU WIN");
#endif
        }

        public bool TrySpendGold(int amount)
        {
            if (_currentGold >= amount)
            {
                _currentGold -= amount;
#if UNITY_EDITOR
                Debug.Log($"Spent {amount} Gold. Remaining Gold: {_currentGold}");
#endif
                return true;
            }
            return false;
        }
    }
}
