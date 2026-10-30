using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
namespace AmbrosJourney.Journey
{
    public static class JourneyCertificate
    {
        static bool exporting;
        public static string OutputDirectoryOverride;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]static extern void AmbrosDownloadPdf(byte[] bytes,int size,string filename);
#endif
        public static void Export(JourneyData data,JourneyUI parent)
        {
            if(!data.finished||data.certificatePending||exporting)return;parent.StartCoroutine(Render(data,parent));
        }
        public static string LastExportPath {get;private set;}
        static IEnumerator Render(JourneyData data,JourneyUI parent)
        {
            exporting=true;
            var host=new GameObject("Certificate print surface");var ui=host.AddComponent<JourneyUI>();ui.Initialize();host.layer=31;
            var cameraObject=new GameObject("Certificate print camera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=5;camera.transform.position=new Vector3(10000,10000,-10);camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=JourneyUI.Paper;
            var target=new RenderTexture(2400,1680,24);camera.targetTexture=target;ui.Canvas.renderMode=RenderMode.ScreenSpaceCamera;ui.Canvas.worldCamera=camera;ui.Canvas.planeDistance=10;
            var scaler=host.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.referenceResolution=new Vector2(1000,700);
            JourneyCertificateView.Draw(ui,data,1);
            foreach(var t in host.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            Canvas.ForceUpdateCanvases();yield return null;Canvas.ForceUpdateCanvases();
            Texture2D pixels=null;
            try
            {
                foreach(var text in host.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();if(text.isTextOverflowing)throw new InvalidOperationException("Certificate text does not fit: "+text.name);}
                camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
                try{pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();}finally{RenderTexture.active=previous;}
                byte[] bytes=ImagePdf(pixels.EncodeToJPG(95),target.width,target.height,data.playerName);
#if UNITY_WEBGL && !UNITY_EDITOR
                AmbrosDownloadPdf(bytes,bytes.Length,"AmbrosJourney-Certificate.pdf");
#else
                string downloads=OutputDirectoryOverride??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");
                if(!Directory.Exists(downloads))downloads=Application.persistentDataPath;
                string path=Path.Combine(downloads,"AmbrosJourney-Certificate-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".pdf");
                File.WriteAllBytes(path,bytes);LastExportPath=path;Debug.Log("JOURNEY_CERTIFICATE_SAVED: "+path);
                if(!JourneySession.TestMode)Application.OpenURL(new Uri(path).AbsoluteUri);
#endif
            }
            catch(Exception error){Debug.LogError("Certificate export failed: "+error.Message);parent.Text(parent.Root,"Export error","Could not save the PDF. Check available disk space, then try Download again.",new Rect(-550,305,1100,45),17,JourneyUI.Paper);}
            finally{if(pixels)UnityEngine.Object.Destroy(pixels);camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(cameraObject);UnityEngine.Object.Destroy(host);exporting=false;}
        }
        static TextMeshProUGUI Center(JourneyUI ui,string value,float y,int size,Color color,float height=62)
        {var text=ui.Text(ui.Root,value,value,new Rect(-530,y,1060,height),size,color);text.alignment=TextAlignmentOptions.Center;return text;}
        // A single high-resolution page preserves TMP's name glyphs without external PDF packages or network access.
        public static byte[] ImagePdf(byte[] jpeg,int width,int height,string name)
        {
            using(var stream=new MemoryStream())
            {
                var offsets=new List<long>{0};Action<string> write=s=>{var bytes=Encoding.ASCII.GetBytes(s);stream.Write(bytes,0,bytes.Length);};
                Action<int,string> obj=(id,value)=>{offsets.Add(stream.Position);write(id+" 0 obj\n"+value+"\nendobj\n");};
                write("%PDF-1.4\n");obj(1,"<< /Type /Catalog /Pages 2 0 R >>");obj(2,"<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
                obj(3,"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 900 630] /Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >>");
                offsets.Add(stream.Position);write("4 0 obj\n<< /Type /XObject /Subtype /Image /Width "+width+" /Height "+height+" /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length "+jpeg.Length+" >>\nstream\n");stream.Write(jpeg,0,jpeg.Length);write("\nendstream\nendobj\n");
                string content="q 900 0 0 630 0 0 cm /Im0 Do Q\n";obj(5,"<< /Length "+content.Length+" >>\nstream\n"+content+"endstream");
                string title="FEFF"+BitConverter.ToString(Encoding.BigEndianUnicode.GetBytes("AI Learning Certificate - "+name)).Replace("-","");obj(6,"<< /Title <"+title+"> /Creator (Ambros Journey) >>");
                long xref=stream.Position;write("xref\n0 7\n0000000000 65535 f \n");for(int i=1;i<offsets.Count;i++)write(offsets[i].ToString("D10")+" 00000 n \n");write("trailer\n<< /Size 7 /Root 1 0 R /Info 6 0 R >>\nstartxref\n"+xref+"\n%%EOF\n");return stream.ToArray();
            }
        }
    }
}
