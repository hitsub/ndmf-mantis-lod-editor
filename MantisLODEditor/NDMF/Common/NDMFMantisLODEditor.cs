using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace MantisLODEditor.ndmf
{
    public class NDMFMantisLODEditor : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        private bool protect_boundary = true;
        
        [SerializeField]
        private bool protect_detail = false;
        
        [SerializeField]
        private bool protect_symmetry = false;
        
        [SerializeField]
        private bool protect_normal = false;
        
        [SerializeField]
        private bool protect_shape = true;
        
        [SerializeField]
        private bool use_detail_map = false;
        
        [SerializeField]
        private int detail_boost = 10;
        
        [SerializeField][Range(0, 100)]
        private float quality = 100.0f;
        
        [SerializeField]
        private bool remove_vertex_color = false;

        /// <summary>
        /// メッシュをまとめて投げてMantisLODEditorにデシメートしてもらう
        /// Through meshes and make MantisLODEditor decimate them
        /// </summary>
        /// <param name="_meshes">MeshComponents and meshes</param>
        /// <returns>triangles</returns>
        public (int, int) Apply(Dictionary<Component, Mesh> _meshes = null)
        {
            var meshes = _meshes ?? GetMesh();
            if (meshes == null)
            {
                Debug.LogError("No mesh found!");
                return default;
            }

            var originalTriangles = 0;
            var modifiedTriangles = 0; 
            foreach (var meshPair in meshes)
            {
                var mantisMeshArray = new[] { new Mantis_Mesh { mesh = Instantiate(meshPair.Value) } };
                originalTriangles += MantisLODEditorUtility.PrepareSimplify(mantisMeshArray);
                MantisLODEditorUtility.Simplify(mantisMeshArray, protect_boundary, protect_detail, protect_symmetry, protect_normal, protect_shape, use_detail_map, detail_boost);
                modifiedTriangles += MantisLODEditorUtility.SetQuality(mantisMeshArray, quality);

                var mesh = mantisMeshArray[0].mesh;
                mesh.name = $"NDMFMantisMesh{mesh.name}";

                if (remove_vertex_color)
                {
                    mesh.colors32 = null;
                }

                switch (meshPair.Key)
                {
                    case MeshFilter meshFilter:
                        meshFilter.sharedMesh = mesh;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        skinnedMeshRenderer.sharedMesh = mesh;
                        break;
                    default:
                        Debug.LogError("Unknown mesh type!");
                        break;
                }
            }

            return (originalTriangles, modifiedTriangles);
        }

        public Dictionary<Component, Mesh> GetMesh()
        {
            var staticMeshes = gameObject.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshes = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            var meshes = new Dictionary<Component, Mesh>();
            
            if (staticMeshes.Length <= 0 && skinnedMeshes.Length <= 0)
            {
                return null;
            }

            foreach (var meshFilter in staticMeshes)
            {
                if (meshFilter.sharedMesh == null)
                {
                    continue;
                }
                meshes.Add(meshFilter, meshFilter.sharedMesh);
            }

            foreach (var skinnedMesh in skinnedMeshes)
            {
                if (skinnedMesh.sharedMesh == null)
                {
                    continue;
                }
                meshes.Add(skinnedMesh, skinnedMesh.sharedMesh);
            }

            return meshes;
        }
    }
}