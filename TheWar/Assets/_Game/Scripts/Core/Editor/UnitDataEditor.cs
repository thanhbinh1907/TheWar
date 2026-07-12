using UnityEditor;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.EditorScripts
{
    [CustomEditor(typeof(UnitData))]
    public class UnitDataEditor : Editor
    {
        private SerializedProperty _unitName;
        private SerializedProperty _unitClass;
        private SerializedProperty _faction;
        private SerializedProperty _deployMode;

        private SerializedProperty _maxHealth;
        private SerializedProperty _damage;
        private SerializedProperty _attackRange;
        private SerializedProperty _attackSpeed;
        private SerializedProperty _moveSpeed;
        
        private SerializedProperty _projectilePrefab;
        private SerializedProperty _deployPrefab;
        private SerializedProperty _spawnedGuardPrefab;

        private SerializedProperty _unitColor;

        private SerializedProperty _idleClip;
        private SerializedProperty _runClip;
        private SerializedProperty _attackClip;
        private SerializedProperty _victoryClip;

        private void OnEnable()
        {
            _unitName = serializedObject.FindProperty("_unitName");
            _unitClass = serializedObject.FindProperty("_unitClass");
            _faction = serializedObject.FindProperty("_faction");
            _deployMode = serializedObject.FindProperty("_deployMode");

            _maxHealth = serializedObject.FindProperty("_maxHealth");
            _damage = serializedObject.FindProperty("_damage");
            _attackRange = serializedObject.FindProperty("_attackRange");
            _attackSpeed = serializedObject.FindProperty("_attackSpeed");
            _moveSpeed = serializedObject.FindProperty("_moveSpeed");
            
            _projectilePrefab = serializedObject.FindProperty("_projectilePrefab");
            _deployPrefab = serializedObject.FindProperty("_deployPrefab");
            _spawnedGuardPrefab = serializedObject.FindProperty("_spawnedGuardPrefab");

            _unitColor = serializedObject.FindProperty("_unitColor");

            _idleClip = serializedObject.FindProperty("_idleClip");
            _runClip = serializedObject.FindProperty("_runClip");
            _attackClip = serializedObject.FindProperty("_attackClip");
            _victoryClip = serializedObject.FindProperty("_victoryClip");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_unitName);
            EditorGUILayout.PropertyField(_unitClass);
            EditorGUILayout.PropertyField(_faction);
            EditorGUILayout.PropertyField(_deployMode);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_maxHealth);
            EditorGUILayout.PropertyField(_damage);
            EditorGUILayout.PropertyField(_attackRange);
            EditorGUILayout.PropertyField(_attackSpeed);
            EditorGUILayout.PropertyField(_moveSpeed);

            // Xử lý ẩn/hiện dựa trên DeployMode
            DeployMode currentMode = (DeployMode)_deployMode.enumValueIndex;
            
            if (currentMode == DeployMode.SocketRanged)
            {
                EditorGUILayout.PropertyField(_projectilePrefab);
            }
            
            EditorGUILayout.PropertyField(_deployPrefab, new GUIContent("Deploy Prefab (Base)"));

            if (currentMode == DeployMode.SocketSpawner)
            {
                EditorGUILayout.PropertyField(_spawnedGuardPrefab, new GUIContent("Spawned Guard Prefab"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_unitColor);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_idleClip);
            EditorGUILayout.PropertyField(_runClip);
            EditorGUILayout.PropertyField(_attackClip);
            EditorGUILayout.PropertyField(_victoryClip);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
