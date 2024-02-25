using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;

class BetterFullScreenRenderPass : ScriptableRenderPass
{
    private Material m_Material;
    private int m_PassIndex;
    private bool m_CopyActiveColor;
    private bool m_BindDepthStencilAttachment;
    private RTHandle m_CopiedColor;
    private RTHandle m_MaskColor;
    private Material m_DefaultMaterial;

    private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();
    private static FieldInfo s_CommandBufferInfo = typeof(RenderingData)
        .GetField("commandBuffer", BindingFlags.NonPublic | BindingFlags.Instance);

    public BetterFullScreenRenderPass(string passName)
    {
        profilingSampler = new ProfilingSampler(passName);
        m_DefaultMaterial = CoreUtils.CreateEngineMaterial("Unlit/Texture");
    }

    public void SetupMembers(Material material, int passIndex, bool copyActiveColor, bool bindDepthStencilAttachment, RTHandle maskColor = null)
    {
        if (material != null)
            m_Material = material;
        else
            m_Material = m_DefaultMaterial;
        m_PassIndex = passIndex;
        m_CopyActiveColor = copyActiveColor;
        m_BindDepthStencilAttachment = bindDepthStencilAttachment;
        m_MaskColor = maskColor;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // FullScreenPass manages its own RenderTarget.
        // ResetTarget here so that ScriptableRenderer's active attachement can be invalidated when processing this ScriptableRenderPass.
        ResetTarget();

        if (m_CopyActiveColor)
            ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
    }

    internal void ReAllocate(RenderTextureDescriptor desc)
    {
        desc.msaaSamples = 1;
        desc.depthBufferBits = (int)DepthBits.None;
        RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, desc, name: "_FullscreenPassColorCopy");
    }

    public void Dispose()
    {
        m_CopiedColor?.Release();
        CoreUtils.Destroy(m_DefaultMaterial);
    }

    private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture)
    {
        Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
    }

    private static void ExecuteMainPass(CommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex, RTHandle maskTexture = null)
    {
        s_SharedPropertyBlock.Clear();
        if (sourceTexture != null)
            s_SharedPropertyBlock.SetTexture("_BlitTexture", sourceTexture);

        if (maskTexture != null && maskTexture.rt != null)
            s_SharedPropertyBlock.SetTexture("_MaskTexture", maskTexture);

        // We need to set the "_BlitScaleBias" uniform for user materials with shaders relying on core Blit.hlsl to work
        s_SharedPropertyBlock.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));

        cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        //var cmd = renderingData.commandBuffer;
        var cmd = (CommandBuffer)s_CommandBufferInfo.GetValue(renderingData);

        using (new ProfilingScope(cmd, profilingSampler))
        {
            if (m_CopyActiveColor)
            {
                CoreUtils.SetRenderTarget(cmd, m_CopiedColor);
                ExecuteCopyColorPass(cmd, cameraData.renderer.cameraColorTargetHandle);
            }

            if (m_BindDepthStencilAttachment)
                CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle, cameraData.renderer.cameraDepthTargetHandle);
            else
                CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);

            ExecuteMainPass(cmd, m_CopyActiveColor ? m_CopiedColor : null, m_Material, m_PassIndex, m_MaskColor);
        }
    }
}