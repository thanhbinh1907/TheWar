# Performance Guard

activation: always

Enforce performance rules for Unity 6 URP with Synty 3D assets.

## Auto-fix these patterns without asking

| Pattern | Fix |
|---------|-----|
| `GetComponent<T>()` in Update | Cache in Awake |
| `Instantiate` for enemy/projectile | Replace with ObjectPool.Get() |
| `Destroy` for enemy/projectile | Replace with ObjectPool.Release() |
| `animator.SetBool("name", ...)` | Use `Animator.StringToHash` cached int |
| `FindObjectOfType` anywhere | Use serialized reference or event |
| `string` comparison in hot path | Use int/enum instead |

## Always add these to new MonoBehaviours

```csharp
// Template for any component attached to pooled objects
public class NewComponent : MonoBehaviour, IPoolable {
    // Cache all GetComponent in Awake
    private Rigidbody _rb;
    private Animator _anim;
    private static readonly int _stateHash = Animator.StringToHash("State");

    void Awake() {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    // IPoolable reset when retrieved from pool
    public void OnGetFromPool() { }
    public void OnReturnToPool() { }
}
```

## GPU Instancing reminder

When creating or modifying materials for Synty assets: always verify `enableInstancing = true`.
If writing code that creates a material instance at runtime, warn the user this breaks instancing.
