using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Penguin - Ground obstacle for Ice world
    /// Moves on the ground
    /// </summary>
    public class Penguin : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
