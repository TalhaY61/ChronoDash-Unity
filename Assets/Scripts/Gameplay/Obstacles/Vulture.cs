using UnityEngine;

namespace ChronoDash.Obstacles
{
    /// <summary>
    /// Vulture - Flying obstacle for Desert world
    /// Flies at height determined by ObstaclesManager
    /// </summary>
    public class Vulture : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
