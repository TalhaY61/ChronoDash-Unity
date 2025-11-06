using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Monkey - Ground obstacle for Jungle world
    /// Moves on the ground
    /// </summary>
    public class Monkey : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
