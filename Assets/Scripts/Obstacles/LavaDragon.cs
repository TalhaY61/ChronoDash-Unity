using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Lava Dragon - Ground obstacle for Lava world
    /// Moves on the ground
    /// </summary>
    public class LavaDragon : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
