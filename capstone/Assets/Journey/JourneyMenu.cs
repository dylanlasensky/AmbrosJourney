using System;
using TMPro;
using UnityEngine;

namespace AmbrosJourney.Journey
{
    // The painting is artwork only. Every label and action remains accessible, live UI.
    public static class JourneyMenu
    {
        public static readonly Color Cream = new Color(1f, .96f, .84f);
        public static readonly Color Gold = new Color(.94f, .72f, .34f);
        public static readonly Color Muted = new Color(.81f, .80f, .71f);

        public static void Background(JourneyUI ui)
        {
            ui.Clear();
            var image = new GameObject("Blossom valley painting", typeof(RectTransform), typeof(UnityEngine.UI.RawImage), typeof(JourneyMenuBackdrop));
            image.transform.SetParent(ui.Root, false);
            var rect = image.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var raw = image.GetComponent<UnityEngine.UI.RawImage>();
            raw.texture = Resources.Load<Texture2D>("Journey/MenuLandscape"); raw.raycastTarget = false;
            image.GetComponent<JourneyMenuBackdrop>().Refresh();
            // Keep type legible on bright displays without flattening the illustration.
            var shade = ui.Panel(ui.Root, "Menu shade", new Rect(-640,-360,500,720), new Color(.015f,.025f,.02f,.12f));
            shade.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            Text(ui,"Ambros title","AMBROS'",new Rect(-565,-307,435,100),82);
            Text(ui,"Journey title","JOURNEY",new Rect(-565,-229,435,100),82);
            Text(ui,"Subtitle","Four Seasons of Discovery",new Rect(-554,-128,420,39),25,Gold);
            Rule(ui,-553,-86,352);
            Rule(ui,-562,275,108);
            Text(ui,"Autosave footer","Your journey is saved automatically.",new Rect(-562,290,440,35),18,Muted);
        }

        public static TextMeshProUGUI Text(JourneyUI ui,string name,string value,Rect bounds,float size,Color? color=null)
        {
            var text=ui.Text(ui.Root,name,value,bounds,size,color??Cream);
            text.font=ui.MenuFont; text.textWrappingMode=TextWrappingModes.NoWrap;
            text.enableAutoSizing=true; text.fontSizeMin=size*.80f; text.fontSizeMax=size;
            return text;
        }

        public static UnityEngine.UI.Button Button(JourneyUI ui,string label,float y,Action action,bool primary=false,float height=51)
        {
            const float width=340;
            var panel=ui.Panel(ui.Root,label,new Rect(-562,y,width,height),primary?new Color(.025f,.065f,.05f,.85f):new Color(0,0,0,0));
            var button=panel.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(()=>action());
            var text=ui.Text(panel,"Label",label,new Rect(-width/2+28,-height/2+1,width-40,height-2),Mathf.Min(primary?31:28,height*.65f),Cream);
            text.font=ui.MenuFont; text.alignment=TextAlignmentOptions.MidlineLeft;
            text.enableAutoSizing=true;text.fontSizeMin=14;text.fontSizeMax=text.fontSize;
            button.targetGraphic=text;
            var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=Gold; colors.selectedColor=Gold; colors.pressedColor=new Color(.80f,.53f,.20f); colors.fadeDuration=.12f; button.colors=colors;
            if(primary)
            {
                Frame(ui,new Rect(-562,y,width,height));
                Diamond(ui,-562,y+height/2,8);
                // A second inset line evokes the reference's fine gilded frame.
                Line(ui,new Rect(-558,y+4,width-8,1),new Color(Gold.r,Gold.g,Gold.b,.45f));
                Line(ui,new Rect(-558,y+height-5,width-8,1),new Color(Gold.r,Gold.g,Gold.b,.45f));
            }
            return button;
        }

        public static void Frame(JourneyUI ui,Rect bounds)
        {
            Line(ui,new Rect(bounds.x,bounds.y,bounds.width,1.5f),Gold);
            Line(ui,new Rect(bounds.x,bounds.yMax-1.5f,bounds.width,1.5f),Gold);
            Line(ui,new Rect(bounds.x,bounds.y,1.5f,bounds.height),Gold);
            Line(ui,new Rect(bounds.xMax-1.5f,bounds.y,1.5f,bounds.height),Gold);
        }

        static void Line(JourneyUI ui,Rect rect,Color color)
        {
            ui.Panel(ui.Root,"Gold filigree",rect,color).GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        }
        static void Diamond(JourneyUI ui,float x,float y,float size)
        {
            var rect=ui.Panel(ui.Root,"Compass diamond",new Rect(x,y,size,size),Gold);
            rect.pivot=new Vector2(.5f,.5f);rect.localRotation=Quaternion.Euler(0,0,45);
            rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var inset=ui.Panel(rect,"Inset",new Rect(-size*.27f,-size*.27f,size*.54f,size*.54f),new Color(.06f,.09f,.06f));
            inset.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        }
        static void Rule(JourneyUI ui,float x,float y,float width)
        {
            Line(ui,new Rect(x,y,width*.44f,1),Gold); Line(ui,new Rect(x+width*.56f,y,width*.44f,1),Gold);
            Diamond(ui,x+width*.5f,y,5);
        }
    }

    // Crop instead of stretching. Left alignment preserves the tree behind the menu at 4:3.
    public sealed class JourneyMenuBackdrop : MonoBehaviour
    {
        void OnRectTransformDimensionsChange()=>Refresh();
        public void Refresh()
        {
            var raw=GetComponent<UnityEngine.UI.RawImage>(); if(!raw||!raw.texture)return;
            var size=((RectTransform)transform).rect.size; if(size.x<=0||size.y<=0)return;
            float ratio=(size.x/size.y)/((float)raw.texture.width/raw.texture.height);
            raw.uvRect=ratio<1?new Rect(0,0,ratio,1):new Rect(0,(1-1/ratio)*.5f,1,1/ratio);
        }
    }
}
