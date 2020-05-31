using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

//此类正式写为渲染管线流程
public class Mypipeline : RenderPipeline
{
    CullResults cull = new CullResults();
    CommandBuffer buffer = new CommandBuffer {
        name = "Render Camera"
    };
    //创建一个错误材质
    Material errorMaterial;
    /**
     * Unity会根据激活的Camera调用Render管线
     */
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        //不绘制任何东西，但是会检查管线对象是否有效用于渲染
        //我们重载这个方法来保证检测
        base.Render(renderContext, cameras);

        foreach (var camera in cameras) {
            Render(renderContext,camera);
        }
    }

    public void Render(ScriptableRenderContext context,Camera camera) {
        //设置相机的global shaderDefine。例如unity_MatrixVP
        context.SetupCameraProperties(camera);

        //为了内存优化  移除到了外面
        //在命名空间UnityEngine.Rendering下，会创建一个缓存
        //CommandBuffer buffer = new CommandBuffer();
        //给我们的缓存buffer命名，会在frameDebugger中看到相应的名字buffer
        //buffer.name = camera.name;
        /*来一种object initializer 的写法
         var buffer = new CommandBuffer{
            name = camera.name; 
         }
         */
        //用来调整Frambuffer里面的层次结构，层次名字为Render Camera
        buffer.BeginSample("Render Camera");
        //获得摄像机的clearFlags
        CameraClearFlags clearFlags = camera.clearFlags;
        buffer.ClearRenderTarget((clearFlags&CameraClearFlags.Depth)!=0,(clearFlags&CameraClearFlags.Color)!=0,camera.backgroundColor);
        
        //我们可以使用ExecuteCommandBuffer方法命令上下文执行缓存。再说明一下，上下文不会立即执行命令，而是复制他们到上下文的内部缓存中。
        context.ExecuteCommandBuffer(buffer);
        //要记得把buffer释放掉 调用buffer.release
        //buffer.Release();
        //不重新new的话只需要clear就好，要重复利用的话
        buffer.Clear();


        //这个是用来裁剪的信息，视锥体信息等
        ScriptableCullingParameters cullingParameters;
        //裁剪信息从camera中拿去，从下面这个静态方法中得到
        if (!CullResults.GetCullingParameters(camera, out cullingParameters)) {
            return;
        }

        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }

        //获得的裁剪参数用来做裁剪
        CullResults.Cull(ref cullingParameters, context,ref cull);
        //camera参数设置排序和剔除图层，pass参数控制那些shader pass用于渲染
        DrawRendererSettings drawSettings = new DrawRendererSettings(camera,new ShaderPassName("SRPDefaultUnlit"));
        //设置绘制非透明物体的顺序 是从前到后
        drawSettings.sorting.flags = SortFlags.CommonOpaque;
        //初始参数设置为true 表示所有都渲染
        FilterRenderersSettings filterSettings = new FilterRenderersSettings(true);
        //只渲染非透明物体
        filterSettings.renderQueueRange = RenderQueueRange.opaque;
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

        //用camera来绘制天空盒
        context.DrawSkybox(camera);
        //设置绘制透明物体的顺序，是从后往前
        drawSettings.sorting.flags = SortFlags.CommonTransparent;
        //再渲染透明物体
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

        //添加默认渲染管线，如果shaderpass名字不对的时候，会出现不正常的洋红色
        DrawDefaultPipeline(context,camera);
        
        
        //结束层次
        buffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();



        //执行每一帧的渲染
        context.Submit();
    }

    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera) {
        ////Unity的默认surface shader有一个ForwardBase pass，它用来作为第一个前向渲染pass。
        if (errorMaterial == null) {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader);
            errorMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        DrawRendererSettings drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
        //还有一些其他材质也不可渲染
        drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
        drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
        //覆盖材质的某个pass
        drawSettings.SetOverrideMaterial(errorMaterial,0);
        FilterRenderersSettings filterSetting = new FilterRenderersSettings(true);
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSetting);
    }
}
