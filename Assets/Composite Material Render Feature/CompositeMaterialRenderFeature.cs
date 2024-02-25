using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CompositeMaterialRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public struct PassSettings
    {
        public Material Material;
        public LayerMask LayerMask;
        public RenderQueueType RenderQueueType;
        public bool WriteDepth;
        public CompareFunction DepthCompareFunction;
        public string[] PassNames;
    }

    [SerializeField]
    private RenderPassEvent _Event = RenderPassEvent.BeforeRenderingPostProcessing;
    public RenderPassEvent Event
    {
        get => _Event;
        set => _Event = value;
    }

    [SerializeField]
    private PassSettings _SubjectSettings = new()
    {
        LayerMask = new LayerMask() { value = 0x7FFFFFFF }, // Everything
        PassNames = { },
        RenderQueueType = RenderQueueType.Transparent,
        WriteDepth = false,
        DepthCompareFunction = CompareFunction.LessEqual
    };
    public PassSettings SubjectSettings
    {
        get => _SubjectSettings;
        set => _SubjectSettings = value;
    }

    [SerializeField]
    private PassSettings _OpaqueSettings = new()
    {
        LayerMask = new LayerMask() { value = 0x7FFFFFFF }, // Everything
        PassNames = { },
        RenderQueueType = RenderQueueType.Opaque,
        WriteDepth = true,
        DepthCompareFunction = CompareFunction.LessEqual
    };

    public PassSettings OpaqueSettings
    {
        get => _OpaqueSettings;
        set => _OpaqueSettings = value;
    }

    [SerializeField]
    private PassSettings _TransparentSettings = new()
    {
        LayerMask = new LayerMask() { value = 0x7FFFFFFF }, // Everything
        PassNames = { },
        RenderQueueType = RenderQueueType.Transparent,
        WriteDepth = true,
        DepthCompareFunction = CompareFunction.LessEqual
    };

    public PassSettings TransparentSettings
    {
        get => _TransparentSettings;
        set => _TransparentSettings = value;
    }

    [SerializeField]
    private Material _CompositeMaterial;
    public Material CompositeMaterial
    {
        get => _CompositeMaterial;
        set => _CompositeMaterial = value;
    }

    [SerializeField]
    private ScriptableRenderPassInput _CompositeRequirements = ScriptableRenderPassInput.None;
    public ScriptableRenderPassInput CompositeRequirements
    {
        get => _CompositeRequirements;
        set => _CompositeRequirements = value;
    }

    private RTHandle _MaskColor;
    private RTHandle _MaskDepth;
    private RenderObjectsPass _SubjectPass;
    private RenderObjectsPass _OpaquePass;
    private RenderObjectsPass _TransparentPass;
    private BetterFullScreenRenderPass _CompositeMaterialPass;
    private RenderObjects.CustomCameraSettings _DefaultCameraSettings = new() { overrideCamera = false };

    private RenderObjectsPass CreateRenderObjectsPass(string suffix, PassSettings settings)
    {
        return new RenderObjectsPass(
            name + suffix,
            _Event,
            settings.PassNames,
            settings.RenderQueueType,
            settings.LayerMask,
            _DefaultCameraSettings);
    }

    public override void Create()
    {
        _OpaquePass = CreateRenderObjectsPass("_OpaqueRender", _OpaqueSettings);
        _TransparentPass = CreateRenderObjectsPass("_TransparentRender", _TransparentSettings);
        _SubjectPass = CreateRenderObjectsPass("_MaskRender", _SubjectSettings);
        _CompositeMaterialPass = new BetterFullScreenRenderPass(name + "_Composite")
        {
            renderPassEvent = _Event
        };
    }

    protected override void Dispose(bool disposing)
    {
        _CompositeMaterialPass?.Dispose();
        _MaskColor?.Release();
        _MaskDepth?.Release();
    }

    public static bool IsExcluded(in RenderingData renderingData)
    {
        var camera_type = renderingData.cameraData.cameraType;
        return camera_type == CameraType.Preview || camera_type == CameraType.Reflection;
    }

    private void SetupRenderObjectsPass(RenderObjectsPass pass, PassSettings settings)
    {
        pass.overrideMaterial = settings.Material;
        pass.ConfigureTarget(_MaskColor, _MaskDepth);
        pass.SetDetphState(settings.WriteDepth, settings.DepthCompareFunction);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (IsExcluded(renderingData))
            return;
        var color_descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default);
        var depth_descriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Depth, 32);
        RenderingUtils.ReAllocateIfNeeded(ref _MaskColor, color_descriptor, name: "MaskColor");
        RenderingUtils.ReAllocateIfNeeded(ref _MaskDepth, depth_descriptor, name: "MaskDepth");
        _SubjectPass.ConfigureClear(ClearFlag.All, Color.black);
        SetupRenderObjectsPass(_SubjectPass, _SubjectSettings);
        SetupRenderObjectsPass(_OpaquePass, _OpaqueSettings);
        SetupRenderObjectsPass(_TransparentPass, _TransparentSettings);
        _CompositeMaterialPass.ConfigureInput(_CompositeRequirements);
        _CompositeMaterialPass.SetupMembers(_CompositeMaterial, 0, true, false, _MaskColor);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (IsExcluded(renderingData))
            return;
        renderer.EnqueuePass(_SubjectPass);
        renderer.EnqueuePass(_OpaquePass);
        renderer.EnqueuePass(_TransparentPass);
        renderer.EnqueuePass(_CompositeMaterialPass);
    }
}
