using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

//此类正式写为渲染管线流程
public class Mypipeline : RenderPipeline
{

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

        //在命名空间UnityEngine.Rendering下，会创建一个缓存
        CommandBuffer buffer = new CommandBuffer();
        buffer.ClearRenderTarget(true, true, Color.clear);
        //我们可以使用ExecuteCommandBuffer方法命令上下文执行缓存。再说明一下，上下文不会立即执行命令，而是复制他们到上下文的内部缓存中。
        context.ExecuteCommandBuffer(buffer);
        //要记得把buffer释放掉 调用buffer.release
        buffer.Release();

        //用camera【0】来绘制天空盒
        context.DrawSkybox(camera);
    
        
      
        



        //执行每一帧的渲染
        context.Submit();
    }
}
