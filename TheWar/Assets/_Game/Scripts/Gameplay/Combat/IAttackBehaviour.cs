using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    public interface IAttackBehaviour
    {
        bool IsAttacking { get; }
        void StartAttack(Transform target);
        void CancelAttack();
    }
}
