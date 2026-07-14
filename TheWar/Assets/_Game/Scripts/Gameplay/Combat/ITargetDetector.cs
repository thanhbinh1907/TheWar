using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    public interface ITargetDetector
    {
        Transform CurrentTarget { get; }
        bool HasValidTarget { get; }
    }
}
