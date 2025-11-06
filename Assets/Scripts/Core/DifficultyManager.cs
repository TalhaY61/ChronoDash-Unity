using UnityEngine;

namespace ChronoDash.Managers
{
    /// <summary>
    /// Manages difficulty scaling based on player score instead of discrete levels.
    /// Difficulty increases gradually as score increases.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        [Header("Score Thresholds")]
        [SerializeField] private int[] scoreThresholds = { 0, 50, 150, 300, 500, 800, 1200, 1700 };
        
        [Header("Difficulty Scaling")]
        [SerializeField] private float baseObstacleSpeed = 400f;
        [SerializeField] private float maxObstacleSpeed = 900f;
        [SerializeField] private float baseObstacleGap = 600f;
        [SerializeField] private float minObstacleGap = 250f;
        
        private int currentScore = 0;
        private int currentDifficultyTier = 0;
        private float scoreMultiplier = 1f; // For combo system
        
        // Events
        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnDifficultyTierChanged; // Tier 0-7
        public System.Action<float> OnScoreMultiplierChanged;
        
        // Properties
        public int CurrentScore => currentScore;
        public int CurrentDifficultyTier => currentDifficultyTier;
        public float ScoreMultiplier => scoreMultiplier;
        public float CurrentObstacleSpeed { get; private set; }
        public float CurrentObstacleGap { get; private set; }
        
        private void Start()
        {
            Reset();
        }
        
        public void Reset()
        {
            currentScore = 0;
            currentDifficultyTier = 0;
            scoreMultiplier = 1f;
            UpdateDifficulty();
            
            OnScoreChanged?.Invoke(currentScore);
            OnDifficultyTierChanged?.Invoke(currentDifficultyTier);
            OnScoreMultiplierChanged?.Invoke(scoreMultiplier);
        }
        
        /// <summary>
        /// Add score and update difficulty if threshold crossed
        /// </summary>
        public void AddScore(int points)
        {
            int scoreBefore = currentScore;
            currentScore += Mathf.RoundToInt(points * scoreMultiplier);
            
            OnScoreChanged?.Invoke(currentScore);
            
            // Check if we crossed a difficulty threshold
            int newTier = CalculateDifficultyTier(currentScore);
            if (newTier != currentDifficultyTier)
            {
                currentDifficultyTier = newTier;
                UpdateDifficulty();
                OnDifficultyTierChanged?.Invoke(currentDifficultyTier);
            }
        }
        
        /// <summary>
        /// Set the score multiplier from combo system
        /// </summary>
        public void SetScoreMultiplier(float multiplier)
        {
            scoreMultiplier = multiplier;
            OnScoreMultiplierChanged?.Invoke(scoreMultiplier);
        }
    
        
        private int CalculateDifficultyTier(int score)
        {
            for (int i = scoreThresholds.Length - 1; i >= 0; i--)
            {
                if (score >= scoreThresholds[i])
                {
                    return i;
                }
            }
            return 0;
        }
        
        private void UpdateDifficulty()
        {
            // Calculate difficulty progression (0.0 to 1.0)
            float progress = currentDifficultyTier / (float)(scoreThresholds.Length - 1);
            
            // Interpolate obstacle speed
            CurrentObstacleSpeed = Mathf.Lerp(baseObstacleSpeed, maxObstacleSpeed, progress);
            
            // Interpolate obstacle gap (inverse - smaller gaps = harder)
            CurrentObstacleGap = Mathf.Lerp(baseObstacleGap, minObstacleGap, progress);
            
        }
        
        /// <summary>
        /// Get difficulty info for UI display
        /// </summary>
        public string GetDifficultyLabel()
        {
            switch (currentDifficultyTier)
            {
                case 0: return "Easy";
                case 1: return "Normal";
                case 2: return "Hard";
                case 3: return "Very Hard";
                case 4: return "Extreme";
                case 5: return "Insane";
                case 6: return "Nightmare";
                case 7: return "IMPOSSIBLE";
                default: return "Unknown";
            }
        }
        
        /// <summary>
        /// Get next threshold for UI
        /// </summary>
        public int GetNextThreshold()
        {
            if (currentDifficultyTier >= scoreThresholds.Length - 1)
                return -1; // Max difficulty
            
            return scoreThresholds[currentDifficultyTier + 1];
        }
    }
}
