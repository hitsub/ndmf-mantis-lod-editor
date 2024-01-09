using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MantisLODEditor.ndmf
{
    [CustomEditor(typeof(NDMFMantisLODEditor))]
    public class NDMFMantisLODCustomEditor : Editor
    {
        private bool m_isPreview;
        private bool m_applied;
        private Dictionary<Component, Mesh> m_originalMeshes;
        private (int, int) m_triangles;

        private SerializedProperty m_protectBoundary;
        private SerializedProperty m_protectDetail;
        private SerializedProperty m_protectSymmetry;
        private SerializedProperty m_protectNormal;
        private SerializedProperty m_protectShape;
        private SerializedProperty m_useDetailMap;
        private SerializedProperty m_detailBoost;
        private SerializedProperty m_quality;

        private void OnEnable()
        {
            m_protectBoundary = serializedObject.FindProperty("protect_boundary");
            m_protectDetail = serializedObject.FindProperty("protect_detail");
            m_protectSymmetry = serializedObject.FindProperty("protect_symmetry");
            m_protectNormal = serializedObject.FindProperty("protect_normal");
            m_protectShape = serializedObject.FindProperty("protect_shape");
            m_useDetailMap = serializedObject.FindProperty("use_detail_map");
            m_detailBoost = serializedObject.FindProperty("detail_boost");
            m_quality = serializedObject.FindProperty("quality");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(m_protectBoundary, new GUIContent("Protect Boundary"));
            EditorGUILayout.PropertyField(m_protectDetail, new GUIContent("More Details"));
            EditorGUILayout.PropertyField(m_protectSymmetry, new GUIContent("Protect Symmetry"));
            EditorGUILayout.PropertyField(m_protectNormal, new GUIContent("Protect Hard Edge"));
            EditorGUILayout.PropertyField(m_protectShape, new GUIContent("Beautiful Triangles"));
            EditorGUILayout.PropertyField(m_useDetailMap, new GUIContent("Use Detail Map"));
            EditorGUILayout.PropertyField(m_detailBoost, new GUIContent("Detail Boost"));
            EditorGUILayout.PropertyField(m_quality, new GUIContent("Quality"));
            if (serializedObject.hasModifiedProperties)
            {
                m_applied = false;
            }
            serializedObject.ApplyModifiedProperties();
            
            var mantis = target as NDMFMantisLODEditor;
            m_originalMeshes = m_originalMeshes ?? mantis.GetMesh();
            
            if (GUILayout.Button(m_isPreview ? "Stop Preview" : "Preview"))
            {
                m_isPreview = !m_isPreview;
                m_applied = false;
                if (!m_isPreview)
                {
                    Revert();
                }
            }

            if (m_isPreview)
            {
                if (mantis != null && !m_applied)
                {
                    m_triangles = mantis.Apply(m_originalMeshes);
                    m_applied = true;
                }
                EditorGUILayout.LabelField($"Triangles", $"{m_triangles.Item2}/{m_triangles.Item1}");
            }
        }

        private void OnDisable()
        {
            Revert();
        }

        private void Revert()
        {
            m_triangles = default;
            foreach (var pair in m_originalMeshes)
            {
                switch (pair.Key)
                {
                    case MeshFilter meshFilter:
                        meshFilter.sharedMesh = pair.Value;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        skinnedMeshRenderer.sharedMesh = pair.Value;
                        break;
                }
            }
        }
    }
}