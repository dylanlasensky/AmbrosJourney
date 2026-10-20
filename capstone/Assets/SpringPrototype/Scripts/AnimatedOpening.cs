using UnityEngine;

// Original twenty-second cutscene: articulated character, moving portal and particles.
// Rendered at 24 fps for Opening.mp4. No input is read here.
public class AnimatedOpening : MonoBehaviour
{
    public Texture2D tree,hero,cabin;
    public float Clock {get;private set;}
    public bool manualClock;
    public void SetTime(float t){Clock=t;}
    void Update(){if(!manualClock)Clock+=Time.deltaTime;}
    static Color C(string h){ColorUtility.TryParseHtmlString("#"+h,out var c);return c.linear;}
    void Box(float x,float y,float w,float h,string color){GUI.color=C(color);GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);}
    void Line(Vector2 a,Vector2 b,float width,string color){var m=GUI.matrix;GUI.matrix=m*Matrix4x4.TRS(a,Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);Box(0,0,Vector2.Distance(a,b),width,color);GUI.matrix=m;}
    void Label(string t,float x,float y,float w,int size,string color){GUI.color=Color.white;var s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};s.normal.textColor=C(color);GUI.Label(new Rect(x,y,w,size+10),t,s);}
    void Sprite(Texture2D t,Rect r){if(t==null)return;GUI.color=Color.white;GUI.DrawTexture(r,t,ScaleMode.ScaleToFit);}
    void Ring(Vector2 p,float radius,float phase,string color,int count=70){for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count+phase;float sz=2+(i%3);Box(p.x+Mathf.Cos(a)*radius,p.y+Mathf.Sin(a)*radius*.78f,sz,sz,color);}}
    void Boy(Vector2 p,float size,float rotation,float hands){var m=GUI.matrix;GUI.matrix=m*Matrix4x4.TRS(p,Quaternion.Euler(0,0,rotation),Vector3.one*size);
        Box(-11,10,12,26,"283650");Box(4,10,12,25,"283650");Box(-14,32,16,7,"151f35");Box(4,32,17,7,"151f35");
        Box(-16,-22,32,37,"447a85");Box(-12,-18,24,9,"528f92");Box(-8,-26,15,9,"e5a775");
        Box(-16,-52,31,28,"efb985");Box(-20,-57,35,13,"463333");Box(-20,-48,8,17,"463333");Box(10,-49,8,10,"463333");Box(7,-40,4,4,"273246");Box(9,-29,8,3,"ce8b69");
        Line(new Vector2(-12,-15),new Vector2(11+hands,-4),8,"518995");Line(new Vector2(11+hands,-4),new Vector2(27+hands,-10),6,"efb985");
        GUI.matrix=m;
    }
    void Room(float t){
        Box(0,0,640,360,"27374c");for(int y=0;y<242;y+=12)Box(0,y,640,1,"2d3e52");Box(0,242,640,118,"61473f");
        for(int y=248;y<360;y+=22){Box(0,y,640,2,"493b39");for(int x=(y%44==0?0:40);x<640;x+=85)Box(x,y,2,21,"513f3b");}
        Box(0,235,640,8,"a17d5c");
        // Night window, blinking stars, curtains.
        Box(63,34,136,145,"b08763");Box(70,41,122,130,"142b48");Box(150,52,18,18,"f9df9b");Box(156,48,17,18,"142b48");
        for(int i=0;i<18;i++){float a=.5f+.5f*Mathf.Sin(t*2+i);GUI.color=new Color(.8f,.9f,1,a);GUI.DrawTexture(new Rect(76+i*37%106,48+i*23%110,2,2),Texture2D.whiteTexture);}
        Box(126,40,5,132,"b08763");Box(70,104,122,5,"b08763");Box(51,27,15,162,"6a667f");Box(196,27,15,162,"6a667f");Box(46,23,170,7,"d0ad80");
        // Bookshelf and illustrated wall poster.
        Box(441,34,129,91,"b48a62");Box(447,40,117,79,"2f6571");Box(460,87,91,25,"4e7d63");for(int i=0;i<7;i++)Box(461+i*13,71-i%3*8,9,33,"82a277");Label("THE FOUR SEASONS",448,43,114,8,"e9d8aa");
        Box(26,202,121,13,"9a6948");Box(33,215,9,46,"624632");Box(130,215,9,46,"624632");for(int i=0;i<8;i++){Box(38+i*11,164+i%3*6,9,38-i%3*6,i%2==0?"c78b66":"6e8798");Box(40+i*11,173+i%3*6,5,2,"e7c795");}
        // Bed, rug and desk.
        Box(447,215,167,69,"6b4d41");Box(450,203,157,53,"a0b7b0");Box(450,222,157,35,"557d87");for(int x=460;x<600;x+=24)Box(x,224,3,28,"72999e");Box(461,200,47,22,"e1d8bc");Box(448,258,9,44,"8b6246");Box(600,258,9,44,"8b6246");
        Box(214,286,204,45,"a56c62");Box(222,293,188,31,"cc9a78");Box(230,299,172,19,"795d61");
        Box(278,204,126,12,"bc8b5d");Box(285,216,8,67,"855e43");Box(389,216,8,67,"855e43");Box(266,239,35,10,"986b4f");Box(270,249,6,31,"654938");
        // Laptop: living game on screen, keyboard flicker.
        Box(332,165,58,40,"152638");Box(336,169,50,31,"46806a");Box(350,183,5,13,"a7c179");Box(347,179,11,7,"a7c179");Box(365+Mathf.Sin(t)*8,185,5,7,"f7d99b");Box(325,205,70,4,"a4b2bd");
        Box(403,174,14,30,"c2946b");Box(400,166,20,12,"e9c88b");
        // Animated typing, then lifted from chair and drawn toward the screen.
        float suction=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-8)/3.5f));
        Vector2 p=Vector2.Lerp(new Vector2(299,205),new Vector2(361,184),suction);p.y-=Mathf.Sin(suction*Mathf.PI)*58;
        Boy(p,Mathf.Lerp(.85f,.02f,suction),suction*340,Mathf.Sin(t*14)*2);
        if(t>4.5f){float r=Mathf.Lerp(4,63,Mathf.Clamp01((t-4.5f)/4));Ring(new Vector2(360,184),r,t*1.6f,"8ce9cf");Ring(new Vector2(360,184),r*.8f,-t*2,"d4f4b2");for(int i=0;i<60;i++){float q=Mathf.Repeat(i*.117f-t*.22f,1);float a=i*2.4f+t*.5f;Box(360+Mathf.Cos(a)*q*190,184+Mathf.Sin(a)*q*120,2,2,"baf4c7");}}
        if(t<3)Label("One ordinary evening...",180,324,280,13,"f4e3bd");
    }
    void Tunnel(float t){Box(0,0,640,360,"172b40");for(int j=0;j<13;j++){float q=Mathf.Repeat(j/13f+t*.35f,1);Ring(new Vector2(320,180),q*q*440,t+j*.3f,j%2==0?"6fc9b8":"9da4dd",80);}Boy(new Vector2(320+Mathf.Sin(t*3)*15,180),.65f+Mathf.Sin(t)*.15f,t*100,5);}
    void Forest(float t){Box(0,0,640,360,"81b6a1");Box(0,100,640,260,"789c50");for(int i=0;i<70;i++)Box(i*97%640,130+i*37%220,3,2,i%2==0?"a7ba6c":"5a864b");
        Box(0,265,640,55,"b7a677");for(int i=0;i<8;i++)Sprite(tree,new Rect(-50+i*100,28-i%2*16,145,200));
        Sprite(cabin,new Rect(440,122,145,150));
        float fall=Mathf.Clamp01(t/1.5f);float y=Mathf.Lerp(-90,213,1-Mathf.Pow(1-fall,2));Sprite(hero,new Rect(270,y,100,100));
        if(t<2){Ring(new Vector2(320,240),Mathf.Lerp(65,0,t/2),t*3,"e4f4b9");}
        for(int i=0;i<40;i++)Box(Mathf.Repeat(i*79+t*15,640),Mathf.Repeat(i*37+t*9,360),3,2,"f1c7bc");
        if(t>1.7f){Box(70,38,500,65,"233e3d");Label("AMBROS' JOURNEY",70,45,500,29,"fff2c5");Label("Beat the seasons. Find your way home.",110,82,420,13,"f4eccf");Label("CHAPTER 01   /   SPRING",145,321,350,12,"fff2c5");}
    }
    void OnGUI(){GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/640f,Screen.height/360f,1));float t=Clock;if(t<11.5f)Room(t);else if(t<14.5f)Tunnel(t-11.5f);else Forest(t-14.5f);GUI.color=Color.white;
        float alpha=t<.7f?1-t/.7f:t>19.4f?(t-19.4f)/.6f:0;if(alpha>0){GUI.color=new Color(0,0,0,alpha);GUI.DrawTexture(new Rect(0,0,640,360),Texture2D.whiteTexture);}GUI.color=Color.white;}
}
