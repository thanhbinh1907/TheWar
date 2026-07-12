using TowerDefense.Gameplay.Tower;

namespace TowerDefense.Enemy
{
    public interface IEnemyAttackBehavior
    {
        void ExecuteAttack(GuardUnit target, float damage);
    }
}
