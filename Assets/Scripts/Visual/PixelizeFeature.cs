using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelizeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("픽셀화 기준 세로 해상도. 낮을수록 픽셀 굵어짐.")]
        public int targetHeight = 180;
    }

    public Settings settings = new();
    private PixelizePass _pass;

    public override void Create()
    {
        _pass = new PixelizePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }
}