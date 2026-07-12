# Antigravity Overrides
Antigravity-specific behaviour. Takes priority over AGENTS.md.

## Planning before coding

Before writing any script, state:
1. Which component(s) will be created
2. Which ScriptableObject data it needs
3. Which interfaces it implements
4. Which events it publishes / subscribes to

Wait for approval if the plan creates more than 2 new files.

## Preferred response style

- Vietnamese explanations, English code and comments
- When showing code, always show the full class (not snippets) for files under 100 lines
- For files over 100 lines, show only the changed method with context (10 lines above/below)
- Always mention which folder the file should be placed in

## What NOT to do

- Do not generate Unity DOTS/ECS code — this project uses classic MonoBehaviour
- Do not suggest NavMesh — this project uses custom A* for learning purposes
- Do not use `Resources.Load` — use direct ScriptableObject references
- Do not add third-party packages without asking first

## Component Setup Rule (UnitSetup)

Whenever you create a new script/component intended for a Unit (e.g., Tower Unit, Guard Unit, FreeDeploy Unit), you MUST automatically update the corresponding Setup script in `Assets\_Game\Scripts\Gameplay\Tower\UnitSetup\` by adding `[RequireComponent(typeof(NewScript))]`. This ensures the user can always drag 1 single Setup script to attach all dependencies.
