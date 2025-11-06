using UnityEngine;

namespace ChronoDash.Obstacles
{
    public class Scorpion : Obstacle
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = "Obstacle";
        }
    }
}
