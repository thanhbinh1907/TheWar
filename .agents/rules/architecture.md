# Architecture Guard

activation: always

Enforce composition-over-inheritance architecture for this Unity tower defense project.

## Red flags — always flag these to the user

1. **Class inheriting MonoBehaviour more than 1 level deep**
   - `FlyingEnemy : Enemy : MonoBehaviour` → ❌ STOP, suggest composition
   - `Enemy : MonoBehaviour` → ✅ OK

2. **Hardcoded stats in MonoBehaviour**
   - `float damage = 25f;` directly in class → ❌ move to ScriptableObject
   - `[SerializeField] EnemyDataSO _data;` → ✅ OK

3. **GetComponent inside Update/FixedUpdate/LateUpdate**
   - Always flag, always refactor to Awake cache

4. **Instantiate or Destroy for poolable objects**
   - Enemy, projectile, VFX → must use ObjectPool
   - One-time UI popups → Instantiate is OK

5. **Direct reference between Tower and Enemy classes**
   - Tower should communicate via events or interfaces, not direct class reference
   - `IDamageable` interface is the bridge

## When asked to create a new enemy or tower type

Always follow the checklist in AGENTS.md "When Adding New Enemy Type" section.
Never create a subclass. Always compose with components and a new ScriptableObject.
