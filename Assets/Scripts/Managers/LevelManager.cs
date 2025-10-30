using UnityEngine;

namespace ChronoDash.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int obstaclesPerLevel = 15; // Obstacles to pass per level
        [SerializeField] private int obstaclePassScore = 1; // Points per obstacle passed

        private int currentLevel;
        private int score;
        private int totalScore; // Total score across all levels
        private int obstaclesPassed; // Count obstacles for level progression
        private int requiredObstacles;
        private float scoreMultiplier = 1f; // For powerups like DoublePoints

        // Events
        public System.Action<int> OnLevelChanged;
        public System.Action<int> OnScoreChanged;

        // Properties
        public int CurrentLevel => currentLevel;
        public int Score => totalScore;
        public int RequiredObstacles => requiredObstacles;
        public int ObstaclesPassed => obstaclesPassed;

        private void Start()
        {
            Reset();
        }

        public void Reset()
        {
            currentLevel = startingLevel;
            score = 0;
            totalScore = 0;
            obstaclesPassed = 0;
            requiredObstacles = obstaclesPerLevel;
            
            OnLevelChanged?.Invoke(currentLevel);
            OnScoreChanged?.Invoke(totalScore);
        }

        public void OnObstaclePassed()
        {
            // Increment counters
            obstaclesPassed++;
            int pointsToAdd = Mathf.RoundToInt(obstaclePassScore * scoreMultiplier);
            score += pointsToAdd;
            totalScore += pointsToAdd;
            
            OnScoreChanged?.Invoke(totalScore);
            
            // Check for level up
            if (obstaclesPassed >= requiredObstacles)
            {
                AdvanceToNextLevel();
            }
        }

        public void AddScore(int points)
        {
            int pointsToAdd = Mathf.RoundToInt(points * scoreMultiplier);
            score += pointsToAdd;
            totalScore += pointsToAdd;
            OnScoreChanged?.Invoke(totalScore);
        }

        public void SetScoreMultiplier(float multiplier)
        {
            scoreMultiplier = multiplier;
            Debug.Log($"Score multiplier set to {multiplier}x");
        }

        public void LoseScore(int points)
        {
            score -= points;
            totalScore -= points;
            score = Mathf.Max(0, score);
            totalScore = Mathf.Max(0, totalScore);
            OnScoreChanged?.Invoke(totalScore);
        }

        private void AdvanceToNextLevel()
        {
            currentLevel++;
            
            // Reset level-specific counters
            obstaclesPassed = 0;
            
            // Increase difficulty: more obstacles needed per level (caps at 25)
            requiredObstacles = Mathf.Min(15 + (currentLevel * 2), 25);
            
            OnLevelChanged?.Invoke(currentLevel);
            
            Debug.Log($"🎉 Level Up! Now at Level {currentLevel} - Need {requiredObstacles} obstacles to advance!");
        }
    }
}
