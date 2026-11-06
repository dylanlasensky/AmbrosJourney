using System;
using TMPro;
using UnityEngine;

namespace AmbrosJourney.Journey
{
    // Pause-menu presentation only. State changes and callbacks remain in JourneySession.
    public static class JourneyPauseJournal
    {
        static readonly Color Ink=new Color(.22f,.26f,.20f),Muted=new Color(.43f,.40f,.30f),Rule=new Color(.57f,.43f,.25f,.34f);
        static readonly Color[] Seasons={new Color(.71f,.43f,.52f),new Color(.43f,.54f,.30f),new Color(.70f,.38f,.22f),new Color(.38f,.58f,.65f)};
        public static void Open(JourneyUI ui,JourneyData data,string notice)
        {
            ui.Clear();
            var dim=ui.Panel(ui.Root,"Journal dimmer",new Rect(-640,-360,1280,720),new Color(.025f,.045f,.035f,.76f));
            dim.anchorMin=Vector2.zero;dim.anchorMax=Vector2.one;dim.offsetMin=dim.offsetMax=Vector2.zero;
            Paint(ui,"Book shadow",new Rect(-566,-296,1146,620),new Color(0,0,0,.35f));
            Paint(ui,"Leather cover",new Rect(-578,-314,1156,626),new Color(.24f,.17f,.11f));
            Paint(ui,"Cover tooling",new Rect(-573,-309,1146,616),new Color(.49f,.34f,.20f));
            Paint(ui,"Cover inset",new Rect(-568,-304,1136,606),new Color(.30f,.23f,.14f));
            Paint(ui,"Left page stack",new Rect(-560,-295,550,594),new Color(.77f,.69f,.51f));
            Paint(ui,"Right page stack",new Rect(10,-295,550,594),new Color(.77f,.69f,.51f));
            Page(ui,"Left parchment page",new Rect(-556,-298,550,588));Page(ui,"Right parchment page",new Rect(6,-298,550,588));
            // Fine stepped page edges, a dark gutter, and short stitches form the spine.
            Paint(ui,"Spine shadow",new Rect(-9,-300,18,594),new Color(.32f,.25f,.16f));
            Paint(ui,"Left page fold",new Rect(-18,-295,9,585),new Color(.76f,.66f,.45f,.55f));
            Paint(ui,"Right page fold",new Rect(9,-295,9,585),new Color(.65f,.53f,.34f,.38f));
            for(int i=0;i<18;i++)Paint(ui,"Binding stitch "+i,new Rect(-5,-272+i*30,10,2),new Color(.81f,.68f,.43f));
            Paint(ui,"Green ribbon bookmark",new Rect(-505,-315,26,41),new Color(.23f,.36f,.27f));
            Paint(ui,"Bookmark tip",new Rect(-500,-274,16,6),new Color(.23f,.36f,.27f));
            for(int i=0;i<4;i++)
            {
                var color=Seasons[i];if(i>=data.unlocked)color=Color.Lerp(color,new Color(.40f,.34f,.25f),.65f);
                Paint(ui,"Season tab "+i,new Rect(557,-221+i*67,25,48),color);
                var tab=Text(ui,"Season tab number "+i,(i+1).ToString(),new Rect(559,-211+i*67,20,26),17,new Color(1,.95f,.80f));tab.alignment=TextAlignmentOptions.Center;
            }
            Text(ui,"Journal eyebrow","AMBROS' JOURNEY",new Rect(-510,-252,440,24),15,Muted);
            Text(ui,"Journal title","Field journal",new Rect(-510,-221,440,48),38,Ink);
            Text(ui,"Explorer name",data.playerName,new Rect(-510,-168,440,32),23,Muted);
            Paint(ui,"Left heading rule",new Rect(-510,-122,440,2),Rule);
            Text(ui,"Pause state","JOURNEY PAUSED",new Rect(-510,-109,440,23),13,Muted);
            Text(ui,"Travel eyebrow","CHAPTER MAP",new Rect(58,-252,440,24),15,Muted);
            Text(ui,"Travel heading","The four seasons",new Rect(58,-221,442,48),35,Ink);
            var explanation=Text(ui,"Travel explanation","Each season remembers your location and lessons. Return for missed artifacts whenever you like.",new Rect(58,-165,440,63),18,Muted);explanation.textWrappingMode=TextWrappingModes.Normal;
            Paint(ui,"Right heading rule",new Rect(58,-90,442,2),Rule);
            for(int i=data.unlocked;i<4;i++)
            {
                float y=-70+i*68;Text(ui,"Locked season "+i,(i+1)+"  "+JourneySession.ChapterNames[i],new Rect(76,y+4,390,27),21,new Color(.58f,.53f,.42f));
                Text(ui,"Locked season note "+i,"Not yet unlocked",new Rect(109,y+31,330,20),13,new Color(.58f,.53f,.42f));
                Paint(ui,"Locked season rule "+i,new Rect(58,y+59,442,1),new Color(.57f,.43f,.25f,.16f));
            }
            Paint(ui,"Reset separator",new Rect(-510,218,440,1),Rule);
            Text(ui,"Save note",string.IsNullOrEmpty(notice)?"Your field notes are saved on this device.":notice,new Rect(58,223,442,43),14,Muted).textWrappingMode=TextWrappingModes.Normal;
            Text(ui,"Left page number","1  /  FIELD NOTES",new Rect(-510,273,440,18),10,Muted);
            var pageNumber=Text(ui,"Right page number","CHAPTER MAP  /  2",new Rect(58,273,442,18),10,Muted);pageNumber.alignment=TextAlignmentOptions.Right;
            for(int i=0;i<4;i++)Paint(ui,"Season seal "+i,new Rect(-282+i*23,197,9,9),Seasons[i]);
            // A small folded paper corner, constructed on the same pixel grid as the game UI.
            for(int i=0;i<3;i++)Paint(ui,"Page corner fold "+i,new Rect(534+i*6,272+i*6,18-i*6,6),new Color(.81f,.72f,.53f));
        }
        public static UnityEngine.UI.Button Button(JourneyUI ui,string label,Rect bounds,Action action,bool primary=false,bool quiet=false)
        {
            var button=ui.Button(ui.Root,label,bounds,action);var face=button.GetComponent<UnityEngine.UI.Image>();face.sprite=null;face.type=UnityEngine.UI.Image.Type.Simple;face.color=Color.white;
            var outline=button.GetComponent<UnityEngine.UI.Outline>();if(outline)outline.enabled=false;
            var colors=button.colors;colors.normalColor=primary?new Color(.22f,.34f,.25f):new Color(1,1,1,0);colors.highlightedColor=primary?new Color(.31f,.44f,.30f):new Color(.66f,.51f,.28f,.16f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=primary?new Color(.17f,.26f,.19f):new Color(.66f,.51f,.28f,.28f);colors.fadeDuration=.08f;button.colors=colors;face.canvasRenderer.SetColor(colors.normalColor);
            var text=button.GetComponentInChildren<TextMeshProUGUI>();JourneyUI.Place(text.rectTransform,new Rect(-bounds.width/2+20,-bounds.height/2+3,bounds.width-40,bounds.height-6));
            text.color=primary?new Color(1,.96f,.83f):quiet?new Color(.48f,.30f,.22f):Ink;text.alignment=TextAlignmentOptions.MidlineLeft;text.fontSize=21;text.enableAutoSizing=true;text.fontSizeMin=15;text.fontSizeMax=21;text.textWrappingMode=TextWrappingModes.NoWrap;
            if(primary)Paint(ui,"Resume ribbon edge",new Rect(bounds.x,bounds.y,bounds.width,2),new Color(.76f,.63f,.35f));
            return button;
        }
        public static void Chapter(JourneyUI ui,int chapter,string label,Action action,bool current)
        {
            float y=-70+chapter*68;var bounds=new Rect(58,y,442,57);var button=Button(ui,label,bounds,action);
            var labelText=button.GetComponentInChildren<TextMeshProUGUI>();JourneyUI.Place(labelText.rectTransform,new Rect(-221+59,-28.5f+4,274,49));labelText.fontSizeMax=22;
            Paint(ui,"Chapter seal "+chapter,new Rect(72,y+13,29,29),Seasons[chapter]);
            var number=Text(ui,"Chapter number "+chapter,(chapter+1).ToString(),new Rect(73,y+14,27,27),18,new Color(1,.96f,.83f));number.alignment=TextAlignmentOptions.Center;
            if(current){var tag=Text(ui,"Current chapter","HERE",new Rect(431,y+20,57,20),12,Muted);tag.alignment=TextAlignmentOptions.Right;}
            Paint(ui,"Chapter rule "+chapter,new Rect(58,y+60,442,1),new Color(.57f,.43f,.25f,.24f));
        }
        static TextMeshProUGUI Text(JourneyUI ui,string name,string value,Rect rect,float size,Color color)
        {
            var text=ui.Text(ui.Root,name,value,rect,size,color);text.enableAutoSizing=true;text.fontSizeMin=size*.65f;text.fontSizeMax=size;text.textWrappingMode=TextWrappingModes.NoWrap;return text;
        }
        static void Paint(JourneyUI ui,string name,Rect rect,Color color)=>ui.Panel(ui.Root,name,rect,color).GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        static void Page(JourneyUI ui,string name,Rect bounds)
        {
            var host=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.RawImage));host.transform.SetParent(ui.Root,false);JourneyUI.Place(host.GetComponent<RectTransform>(),bounds);
            var image=host.GetComponent<UnityEngine.UI.RawImage>();image.texture=Resources.Load<Texture2D>("Journey/FieldCard");image.uvRect=new Rect(.11f,.13f,.78f,.74f);image.raycastTarget=false;
        }
    }
}
