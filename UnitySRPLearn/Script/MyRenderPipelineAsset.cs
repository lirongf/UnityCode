using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;


[CreateAssetMenu(menuName = "Rendering/My PipeRenderLine")]/*此特性使得AssetCreate的列表中新增这个Asset功能*/
public class MyRenderPipelineAsset : RenderPipelineAsset
{
    //返回render Object，我们写好的渲染管线在这里传送给unity
    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new Mypipeline();
    }
}
