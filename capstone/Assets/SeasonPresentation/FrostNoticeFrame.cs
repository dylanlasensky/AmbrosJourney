using UnityEngine;

namespace AmbrosJourney.Presentation
{
    // Small pixel-shaped ice facets, drawn directly in the existing uGUI panel.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FrostNoticeFrame : UnityEngine.UI.Graphic
    {
        static readonly Color32 Ice=new Color32(106,178,206,255),Light=new Color32(218,247,255,255),Shade=new Color32(75,135,171,255);
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;
            Quad(mesh,r.xMin-6,r.yMin-4,6,r.height+16,Ice);
            Quad(mesh,r.xMax,r.yMin-4,6,r.height+16,Shade);
            Quad(mesh,r.xMin-6,r.yMin-4,r.width+12,5,Ice);
            Quad(mesh,r.xMin-6,r.yMax+4,r.width+12,10,Ice);
            Quad(mesh,r.xMin-6,r.yMax+10,r.width+12,4,Light);
            int i=0;
            for(float x=r.xMin+12;x<r.xMax-8;x+=36,i++)
                Icicle(mesh,x,r.yMax+14,8+(i%3)*4,12+(i*13%5)*4);
            i=0;
            for(float x=r.xMin+18;x<r.xMax-8;x+=48,i++)
                Icicle(mesh,x,r.yMin+2,12+(i%3)*4,24+(i*11%5)*4);
        }
        static void Icicle(UnityEngine.UI.VertexHelper mesh,float x,float top,int width,int length)
        {
            for(int down=0;down<length;down+=4)
            {
                float half=Mathf.Max(1,Mathf.Floor(width*(1-down/(float)length)/4)*2);
                Quad(mesh,x-half,top-down-4,half,4,Ice);
                Quad(mesh,x,top-down-4,half,4,Light);
            }
        }
        static void Quad(UnityEngine.UI.VertexHelper mesh,float x,float y,float width,float height,Color32 color)
        {
            int start=mesh.currentVertCount;var v=UnityEngine.UIVertex.simpleVert;v.color=color;
            v.position=new Vector3(x,y);mesh.AddVert(v);v.position=new Vector3(x,y+height);mesh.AddVert(v);
            v.position=new Vector3(x+width,y+height);mesh.AddVert(v);v.position=new Vector3(x+width,y);mesh.AddVert(v);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
    }
}
