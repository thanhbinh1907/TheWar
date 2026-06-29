# Code Style

activation: glob
glob: **/*.cs

Enforce C# style for this Unity project.

## Auto-apply these rules to all generated C#

- Private fields: `_camelCase` with underscore prefix
- SerializeField always private: `[SerializeField] private Type _field;`
- No `public` fields — always property or SerializeField private
- Interfaces prefix `I`: `IDamageable`, `IPoolable`, `IWaveTrigger`
- ScriptableObjects suffix `SO`: `EnemyDataSO`, `TowerDataSO`
- Events prefix `On`: `OnEnemyDied`, `OnWaveComplete`
- Constants: `UPPER_SNAKE_CASE`
- No `Debug.Log` outside `#if UNITY_EDITOR` blocks

## Header template for new scripts

```csharp
using UnityEngine;

namespace TowerDefense.{Folder}
{
    /// <summary>
    /// One-line description of what this component does.
    /// </summary>
    public class ClassName : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private DataSO _data;

        [Header("References")]
        [SerializeField] private Transform _target;

        // Runtime state
        private bool _isActive;

        private void Awake() { }
        private void OnEnable() { }
        private void Start() { }
        private void Update() { }
        private void OnDisable() { }
    }
}
```

## Max size limits

- Method: 50 lines. If longer, extract helper methods.
- Class: 200 lines. If longer, split into separate components.
- File: one public class per file, filename matches class name exactly.
