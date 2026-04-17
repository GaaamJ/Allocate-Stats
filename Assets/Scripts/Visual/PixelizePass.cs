using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelizePass : ScriptableRenderPass
{
    private readonly PixelizeFeature.Settings _settings;
    private static readonly int LowResTexID = Shader.PropertyToID("_LowResTex");

    private class PassData
    {
        public TextureHandle src;
    }

    public PixelizePass(PixelizeFeature.Settings settings)
    {
        _settings = settings;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer) return;

        var srcDesc = cameraData.cameraTargetDescriptor;
        float aspect = (float)srcDesc.width / srcDesc.height;
        int w = Mathf.RoundToInt(_settings.targetHeight * aspect);
        int h = _settings.targetHeight;

        // 저해상도 임시 RT (Point 필터로 픽셀 유지)
        var lowResDesc = new RenderTextureDescriptor(w, h, srcDesc.colorFormat, 0);
        TextureHandle lowResHandle = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, lowResDesc, "_LowResTex", false, FilterMode.Point);

        // Pass 1 — 원본 → 저해상도
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelize Downsample", out var data))
        {
            data.src = resourceData.activeColorTexture;
            builder.UseTexture(data.src, AccessFlags.Read);
            builder.SetRenderAttachment(lowResHandle, 0, AccessFlags.Write);
            builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                Blitter.BlitTexture(ctx.cmd, d.src, new Vector4(1, 1, 0, 0), 0, false));
        }

        // Pass 2 — 저해상도 → 화면 (Point 업스케일)
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelize Upsample", out var data))
        {
            data.src = lowResHandle;
            builder.UseTexture(data.src, AccessFlags.Read);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                Blitter.BlitTexture(ctx.cmd, d.src, new Vector4(1, 1, 0, 0), 0, false));
        }
    }
}