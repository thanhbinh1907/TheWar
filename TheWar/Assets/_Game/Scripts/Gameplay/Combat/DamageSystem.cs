using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Gameplay.Combat
{
    public class DamageSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<DamageEvent>(OnDamageEvent);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageEvent>(OnDamageEvent);
        }

        private void OnDamageEvent(DamageEvent evt)
        {
            if (evt.target != null)
            {
                evt.target.TakeDamage(evt.amount);
            }
        }
    }
}
