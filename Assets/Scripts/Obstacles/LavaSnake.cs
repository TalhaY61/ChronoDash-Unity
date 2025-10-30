using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Lava Snake - Ground obstacle for Lava world
    /// Moves on the ground
    /// </summary>
    public class LavaSnake : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
