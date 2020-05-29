using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

//此类正式写为渲染管线流程
public class Mypipeline : RenderPipeline
{

    /**
     * Unity会根据激活的Camera调用Render管线
     */
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);
        renderContext.DrawSkybox(cameras[0]);
        renderContext.Submit();
    }
}
