using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Snake - Ground obstacle for Jungle world
    /// Moves on the ground like Scorpion
    /// </summary>
    public class Snake : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
