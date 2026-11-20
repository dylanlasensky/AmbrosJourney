using System;
using TMPro;
using UnityEngine;

namespace AmbrosJourney.Journey
{
    // Pixel key diagrams are live uGUI artwork, using the same font and book as Pause.
    public static class JourneyWelcome
    {
        static readonly Color Ink=new Color(.22f,.26f,.20f),Muted=new Color(.43f,.40f,.30f);
        static readonly Color Edge=new Color(.30f,.28f,.20f),Light=new Color(1,.97f,.82f),Face=new Color(.89f,.82f,.63f),Shadow=new Color(.64f,.55f,.36f);
        public static void Draw(JourneyUI ui,Action close)
        {
            JourneyPauseJournal.Book(ui,JourneySession.Instance?JourneySession.Instance.Data:new JourneyData());
            Text(ui,"Welcome eyebrow","YOUR FIRST FIELD NOTES",new Rect(-510,-253,440,24),15,Muted);
            Text(ui,"Welcome title","Welcome to Spring",new Rect(-510,-219,440,49),32);
            Text(ui,"Movement heading","MOVE & EXPLORE",new Rect(-510,-152,440,25),17,Muted);
            Cluster(ui,-484,-101,false);Cluster(ui,-270,-101,true);
            Text(ui,"Movement caption","WASD",new Rect(-484,16,160,28),20);
            Text(ui,"Arrows caption","ARROW KEYS",new Rect(-270,16,184,28),20);
            Text(ui,"Movement description","Use either set of keys to walk around Spring.",new Rect(-510,65,440,61),21);
            Text(ui,"Compass heading","FOLLOW THE COMPASS",new Rect(-510,145,440,27),18,Muted);
            Arrow(ui,-500,194,5,3);
            Text(ui,"Compass explanation","Look for the arrow beside your character. It points toward your next stop.",new Rect(-445,185,375,88),21);
            Text(ui,"Controls eyebrow","KEEP THIS GUIDE HANDY",new Rect(58,-253,440,24),15,Muted);
            Text(ui,"Controls title","Your controls",new Rect(58,-219,440,49),34);
            Row(ui,"E","Talk, read & collect",-144);
            Row(ui,"1  2","Choose an answer",-80,90);
            Row(ui,"J","Open your field notes",-16);
            Row(ui,"I","View your artifacts",48);
            Row(ui,"Esc","Go back / pause",112,90);
            Text(ui,"First destination","FIRST STOP: ANCIENT MAGE",new Rect(58,185,440,27),16,Muted);
            Text(ui,"First destination detail","Close this guide, then follow the arrow to his cottage.",new Rect(58,213,440,47),20);
            JourneyPauseJournal.Button(ui,"Let's explore  [ E / Enter ]",new Rect(-210,266,420,37),close,true);
        }
        static void Row(JourneyUI ui,string key,string description,float y,float width=58)
        {
            Key(ui,key,58,y,width,48);
            Text(ui,"Control "+key,description,new Rect(167,y+6,335,38),21);
        }
        static void Cluster(JourneyUI ui,float x,float y,bool arrows)
        {
            Key(ui,arrows?"":"W",x+54,y,48,48,arrows?0:-1);
            Key(ui,arrows?"":"A",x,y+54,48,48,arrows?1:-1);
            Key(ui,arrows?"":"S",x+54,y+54,48,48,arrows?2:-1);
            Key(ui,arrows?"":"D",x+108,y+54,48,48,arrows?3:-1);
        }
        static void Key(JourneyUI ui,string label,float x,float y,float w,float h,int arrow=-1)
        {
            // Stepped corners, a raised face and a thick lower edge form a pixel keycap.
            Paint(ui,"Key shadow "+label,new Rect(x+3,y+5,w,h),Shadow);
            Paint(ui,"Key outline "+label,new Rect(x+3,y,w-6,h),Edge);
            Paint(ui,"Key outline sides "+label,new Rect(x,y+3,w,h-6),Edge);
            Paint(ui,"Key face "+label,new Rect(x+4,y+4,w-8,h-12),Face);
            Paint(ui,"Key top highlight "+label,new Rect(x+4,y+4,w-8,3),Light);
            Paint(ui,"Key left highlight "+label,new Rect(x+4,y+4,3,h-12),Light);
            if(arrow>=0)Arrow(ui,x+w/2-10,y+12,4,arrow);
            else {var text=Text(ui,"Key label "+label,label,new Rect(x+6,y+4,w-12,h-12),21);text.alignment=TextAlignmentOptions.Center;}
        }
        static void Arrow(JourneyUI ui,float x,float y,float pixel,int direction)
        {
            string[] shape={"00100","01110","11111","00100","00100"};
            for(int row=0;row<5;row++)for(int col=0;col<5;col++)if(shape[row][col]=='1')
            {
                int cx=col,cy=row;
                if(direction==1){cx=row;cy=4-col;}else if(direction==2){cx=4-col;cy=4-row;}else if(direction==3){cx=4-row;cy=col;}
                Paint(ui,"Arrow pixel",new Rect(x+cx*pixel,y+cy*pixel,pixel,pixel),Ink);
            }
        }
        static TextMeshProUGUI Text(JourneyUI ui,string name,string value,Rect bounds,float size,Color? color=null)
        {
            var text=ui.Text(ui.Root,name,value,bounds,size,color??Ink);text.enableAutoSizing=true;text.fontSizeMin=size-2;text.fontSizeMax=size;return text;
        }
        static void Paint(JourneyUI ui,string name,Rect bounds,Color color)=>ui.Panel(ui.Root,name,bounds,color).GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
    }
}
