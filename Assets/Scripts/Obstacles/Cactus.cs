using UnityEngine;

namespace ChronoDash.Obstacles
{
    public class Cactus : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
