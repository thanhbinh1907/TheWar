using TowerDefense.Shared;

namespace TowerDefense.Core
{
    public struct DamageEvent
    {
        public IDamageable target;
        public float amount;

        public DamageEvent(IDamageable target, float amount)
        {
            this.target = target;
            this.amount = amount;
        }
    }
}
