using MantisLODEditor.ndmf;
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(MantisLODEditorNDMF))]

namespace MantisLODEditor.ndmf
{
    public class MantisLODEditorNDMF : Plugin<MantisLODEditorNDMF>
    {
        public override string QualifiedName => "MantisLODEditor.ndmf";

        public override string DisplayName => "Decimate Polygons by MantisLODEditor";

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Optimizing);
            seq.BeforePlugin("com.anatawa12.avatar-optimizer");
            seq.Run("Decimate Polygons by MantisLODEditor", ctx =>
            {
                var ndmfMantises = ctx.AvatarRootObject.GetComponentsInChildren<NDMFMantisLODEditor>(true);
                foreach (var ndmfMantis in ndmfMantises)
                {
                    ndmfMantis.Apply();
                    Object.DestroyImmediate(ndmfMantis);
                }
            });
        }
    }
}
