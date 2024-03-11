using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if !UNITY_2020_1_OR_NEWER
using AnimationModeDriver = UnityEngine.Object;
#endif

namespace MantisLODEditor.ndmf
{
    [CustomEditor(typeof(NDMFMantisLODEditor))]
    public class NDMFMantisLODCustomEditor : Editor
    {
        private bool m_isPreview;
        private bool m_applied;
        private Dictionary<Component, Mesh> m_originalMeshes;
        private (int, int) m_triangles;
        private AnimationModeDriver m_animationModeDriver;

        //Mantis Editor Parameters
        private SerializedProperty m_protectBoundary;
        private SerializedProperty m_protectDetail;
        private SerializedProperty m_protectSymmetry;
        private SerializedProperty m_protectNormal;
        private SerializedProperty m_protectShape;
        private SerializedProperty m_useDetailMap;
        private SerializedProperty m_detailBoost;
        private SerializedProperty m_quality;
        
        //NDMF Mantis Parameters
        private SerializedProperty m_removeVertexColor;

        private AnimationModeDriver AnimationModeDriver => m_animationModeDriver
            ? m_animationModeDriver
#if UNITY_2020_1_OR_NEWER
            : m_animationModeDriver = CreateInstance<AnimationModeDriver>();
#else
            : m_animationModeDriver = ScriptableObject.CreateInstance(typeof(AnimationMode).Assembly.GetType("UnityEditor.AnimationModeDriver"));
#endif

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
            m_removeVertexColor = serializedObject.FindProperty("remove_vertex_color");
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
            EditorGUILayout.PropertyField(m_removeVertexColor, new GUIContent("Remove Vertex Color After Optimize"));
            EditorGUILayout.PropertyField(m_quality, new GUIContent("Quality"));
            if (serializedObject.hasModifiedProperties)
            {
                m_applied = false;
            }
            serializedObject.ApplyModifiedProperties();
            
            var mantis = target as NDMFMantisLODEditor;
            m_originalMeshes = m_originalMeshes ?? mantis.GetMesh();


            if (!m_isPreview && AnimationMode.InAnimationMode())
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    GUILayout.Button("Preview (Maybe other preview is working)");
                }
            }
            else
            {
                if (GUILayout.Button(m_isPreview ? "Stop Preview" : "Preview"))
                {
                    m_isPreview = !m_isPreview;
                    m_applied = false;
                    if (m_isPreview)
                    {
                        ReadyPreview();
                    }
                    else
                    {
                        StopPreview();
                    }
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
            else
            {
                EditorGUILayout.LabelField($"Triangles", $"- / - (works during only Preview)");
            }
        }

        private void ReadyPreview()
        {
#if UNITY_2020_1_OR_NEWER
            AnimationMode.StartAnimationMode(AnimationModeDriver);
#else
            AnimationMode.StartAnimationMode();
#endif
            try
            {
                AnimationMode.BeginSampling();

                foreach (var originalMeshPair in m_originalMeshes)
                {
                    AnimationMode.AddPropertyModification(
                        EditorCurveBinding.PPtrCurve("", typeof(SkinnedMeshRenderer), "m_Mesh"),
                        new PropertyModification
                        {
                            target = originalMeshPair.Key,
                            propertyPath = "m_Mesh",
                            objectReference = originalMeshPair.Value,
                        }, 
                        true);
                }
            }
            finally
            {
                AnimationMode.EndSampling();   
            }
        }
        
        private void StopPreview()
        {
#if UNITY_2020_1_OR_NEWER
            AnimationMode.StopAnimationMode(AnimationModeDriver);
#else
            AnimationMode.StopAnimationMode();
#endif
        }

        private void OnDisable()
        {
            StopPreview();
        }
    }
}