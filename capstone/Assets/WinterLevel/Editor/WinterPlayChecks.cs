#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using TMPro;
using AmbrosJourney.Presentation;
namespace AmbrosJourney.Winter.Editor
{
    public static class WinterPlayChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static WinterLevel level;static SeasonOverlay ui;public static int States;
        static void Check(bool value,string why){if(!value)throw new Exception("WINTER CHECK: "+why);}
        static void Call(string name)=>typeof(WinterLevel).GetMethod(name,Flags).Invoke(level,null);
        static void Inspect()
        {
            typeof(SeasonOverlay).GetMethod("LateUpdate",Flags).Invoke(ui,null);Canvas.ForceUpdateCanvases();
            foreach(var t in ui.GetComponentsInChildren<TextMeshProUGUI>()){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Text overflow "+t.name+" / "+ui.Heading.text);}
            States++;
        }
        static void Next(){Inspect();ui.Choices[0].onClick.Invoke();}
        static void Choice(int c){Inspect();ui.Choices[c].onClick.Invoke();}
        static void Move(Vector2 p){level.player.transform.position=p;level.player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();}
        static bool Route(Vector2 from,Vector2 to,bool cave)=>SeasonNavigation.FindPath(from,to,cave?WinterWorld.CaveMin:WinterWorld.Min,cave?WinterWorld.CaveMax:WinterWorld.Max).Count>0;
        public static void Run()
        {
            level=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(level,"Winter loaded");ui=level.GetComponentInChildren<SeasonOverlay>();States=0;
            Check(level.InputBlocked&&!level.Progress.Introduced,"Intro blocks movement");Next();Next();Next();Check(!level.InputBlocked,"Intro completes");
            foreach(var p in new[]{WinterWorld.MissionPositions[0],WinterWorld.MissionPositions[1],WinterWorld.Entrance})Check(Route(WinterLevel.Spawn,p,false),"Outdoor destination reachable: "+p);
            Move(WinterWorld.Entrance);level.Interact(6);Next();Check(!level.Progress.InCave,"Cave guard blocks early entry");
            for(int m=0;m<2;m++)Mission(m);
            Move(new Vector2(24,22));level.Interact(8);Next();
            for(int i=0;i<5;i++){int c=WinterContent.Boss[i].Correct;Choice(i<3?c:(c+1)%3);Next();}Check(!level.Progress.WinterComplete,"Winter 3/5 fails");Next();
            for(int i=0;i<5;i++){int c=WinterContent.Boss[i].Correct;Choice(i<4?c:(c+1)%3);Next();}Check(level.Progress.WinterComplete,"Winter 4/5 passes");Next();
            Check(level.littleMage.follow==level.ember.transform,"Little Mage recruited by Winter crest");
            Move(WinterWorld.Entrance);level.Interact(6);Next();Check(level.Progress.InCave,"Winter crest permits cave entry");
            Check(Route(WinterWorld.CaveSpawn,WinterWorld.MissionPositions[2],true),"Archive reachable");Check(!Route(WinterWorld.CaveSpawn,WinterWorld.BossPosition,true),"Final physically sealed before Archive");
            Mission(2);Call("RefreshWorld");Physics2D.SyncTransforms();Check(Route(WinterWorld.CaveSpawn,WinterWorld.BossPosition,true),"Final reachable after all seals");
            Move(WinterWorld.BossPosition+Vector2.down*2);level.Interact(4);Next();
            for(int i=0;i<8;i++){int c=WinterContent.Final[i].Correct;Choice(i<6?c:(c+1)%3);Next();}Check(!level.Progress.Complete,"Final 6/8 fails");Next();
            for(int i=0;i<8;i++){int c=WinterContent.Final[i].Correct;Choice(i<7?c:(c+1)%3);Next();}Check(level.Progress.Complete,"Final 7/8 passes");Inspect();Call("Close");
            Call("ToggleJournal");Inspect();Call("ToggleJournal");
            Move(level.caveExit.position);level.Interact(7);Next();Check(level.Progress.Complete&&level.Progress.RuneCount==3,"Leaving preserves completed season");
            Move(WinterWorld.Entrance);level.Interact(6);Next();Check(level.Progress.Complete,"Re-entry preserves completion");
            Move(WinterWorld.BossPosition+Vector2.down*3);Call("RefreshWorld");
            Debug.Log("WINTER_PLAY_PASS: "+States+" UI states, outdoor and cave routes, cave guard, return trips, Winter 3/5 retry and 4/5 pass, cumulative final 6/8 retry and 7/8 pass.");
        }
        static void Mission(int m)
        {
            Move(WinterWorld.MissionPositions[m]+Vector2.down);level.Interact(m+1);Next();
            foreach(var q in WinterContent.Missions[m]){Choice((q.Correct+1)%3);Next();Choice(q.Correct);Next();}
            Next();Check(level.Progress.MissionComplete(m),"Evidence station completed");
        }
    }
}
#endif
