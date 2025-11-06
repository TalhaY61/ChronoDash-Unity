using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Owl - Flying obstacle for Jungle world
    /// Flies at height determined by ObstaclesManager
    /// </summary>
    public class Owl : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
