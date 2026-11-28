// Runs isolated Editor regression checks for winter play.
#if UNITY_EDITOR
using System;
using System.Collections;
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
        // Editor entry points keep preparation and checks outside normal player execution.
        static void Call(string name)=>typeof(WinterLevel).GetMethod(name,Flags).Invoke(level,null);
        static void Inspect()
        {
            typeof(SeasonOverlay).GetMethod("LateUpdate",Flags).Invoke(ui,null);Canvas.ForceUpdateCanvases();
            foreach(var t in ui.GetComponentsInChildren<TextMeshProUGUI>()){t.ForceMeshUpdate();Check(t.font==GameFont.Resolve(),"Determination font on "+t.name);Check(!t.isTextOverflowing,"Text overflow "+t.name+" / "+ui.Heading.text);}
            States++;
        }
        static void Next(){Inspect();ui.Choices[0].onClick.Invoke();}
        static void Choice(int c){Inspect();ui.Choices[c].onClick.Invoke();}
        static void Move(Vector2 p){level.player.transform.position=p;level.player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();}
        static bool Route(Vector2 from,Vector2 to,bool cave)=>SeasonNavigation.FindPath(from,to,cave?WinterWorld.CaveMin:WinterWorld.Min,cave?WinterWorld.CaveMax:WinterWorld.Max).Count>0;
        public static bool Running {get;private set;}
        public static void Run()=>UnityEngine.Object.FindAnyObjectByType<WinterLevel>().StartCoroutine(RunSequence());
        static IEnumerator RunSequence()
        {
            Running=true;
            level=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(level,"Winter loaded");ui=level.GetComponentInChildren<SeasonOverlay>();States=0;
            Check(!level.InputBlocked&&!level.Progress.Introduced,"Winter waits for E while permitting movement immediately");AmbrosJourney.Presentation.Editor.CompanionGuidanceChecks.Winter(level);
            Move(level.littleMage.transform.position);level.Interact(0);Check(level.IsDialogueOpen&&level.InputBlocked,"E opens Winter greeting");Next();Check(level.Progress.Introduced&&!level.InputBlocked,"Finishing greeting enables Winter lessons");level.Progress.CompassWarningShown=true;
            foreach(var map in level.World.Outdoors.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true))Check(!map.name.Contains("Frozen lake"),"Frozen lake surface removed");
            foreach(var label in level.World.Outdoors.GetComponentsInChildren<SeasonWorldLabel>(true))Check(!(label.Caption??"").Contains("THIN ICE"),"Ice warning removed with the lake");
            foreach(var block in level.World.Outdoors.GetComponentsInChildren<Collider2D>(true))Check(block.name!="Thin ice","No invisible ice blockers remain");
            var grove=level.World.Outdoors.transform.Find("Snowy woodland clearing");Check(grove&&grove.GetComponentsInChildren<SpriteRenderer>().Length>=4,"Former lake is an authored snow grove");
            foreach(var trunk in grove.GetComponentsInChildren<Collider2D>())Check(trunk.GetComponent<SpriteRenderer>()&&trunk.GetComponent<SpriteRenderer>().sprite,"Grove collision belongs to a visible tree");
            Check(!Physics2D.OverlapCircle(new Vector2(-2,0),.4f)&&Route(new Vector2(-8,0),new Vector2(5,0),false),"Snowy gap through the former lake is walkable");
            var border=level.World.Outdoors.transform.Find("Arrival fir border");Check(border&&border.GetComponentsInChildren<SpriteRenderer>().Length>=2,"Arrival is framed by a visible fir border");
            foreach(var p in new[]{WinterWorld.MissionPositions[0],WinterWorld.MissionPositions[1],WinterWorld.Entrance})Check(Route(WinterLevel.Spawn,p,false),"Outdoor destination reachable: "+p);
            Move(WinterWorld.Entrance);level.Interact(6);Next();Check(!level.Progress.InCave,"Cave guard blocks early entry");
            for(int m=0;m<2;m++)Mission(m);
            Move(WinterWorld.WardenPosition+Vector2.down);level.Interact(8);Next();
            for(int i=0;i<5;i++){int c=WinterContent.Boss[i].Correct;Choice(i<3?c:(c+1)%3);Next();}Check(!level.Progress.WinterComplete,"Winter 3/5 fails");Next();
            for(int i=0;i<5;i++){int c=WinterContent.Boss[i].Correct;Choice(i<4?c:(c+1)%3);Next();}Check(level.Progress.WinterComplete,"Winter 4/5 passes");Next();
            yield return null;
            while(AmbrosJourney.Journey.BossPortalSequence.Busy||!AmbrosJourney.Journey.BossPortalSequence.AllowsEntry(level.caveEntrance))yield return null;
            Check(!level.littleMage.follow&&(Vector2)level.littleMage.transform.position==WinterWorld.CaveGuide,"Little Mage waits beside cave after Winter crest");
            Check(!level.Progress.CaveGuideRead&&!level.Progress.CanEnterCave&&!level.IsDialogueOpen&&!level.InputBlocked,"Victory leaves a deliberate cave conversation available");
            Move(WinterWorld.Entrance);Call("LateUpdate");Check(!level.Progress.InCave,"Cave stays closed until the guide briefing is finished");
            Move(level.littleMage.transform.position);level.Interact(0);Check(level.IsDialogueOpen&&level.InputBlocked,"E opens cave briefing");Next();
            Check(level.Progress.CaveGuideRead&&level.Progress.CanEnterCave&&!level.Progress.InCave&&!level.InputBlocked,"Finishing briefing permits entry without teleporting the player");
            Move(WinterWorld.Entrance);Call("LateUpdate");Check(level.Progress.InCave,"Walking into cleared doorway automatically enters cave");
            Check(level.littleMage.follow==level.ember.transform,"Little Mage follows inside cave");
            Check(Route(WinterWorld.CaveSpawn,WinterWorld.MissionPositions[2],true),"Archive reachable");Check(!Route(WinterWorld.CaveSpawn,WinterWorld.BossPosition,true),"Final physically sealed before Archive");
            Mission(2);Call("RefreshWorld");Physics2D.SyncTransforms();Check(Route(WinterWorld.CaveSpawn,WinterWorld.BossPosition,true),"Final reachable after all seals");
            Move(WinterWorld.BossPosition+Vector2.down*2);level.Interact(4);Next();
            for(int i=0;i<8;i++){Activity(i,i<6);Next();}Check(!level.Progress.Complete,"Final 6/8 fails");Next();
            for(int i=0;i<8;i++){Activity(i,i<7);Next();}Check(level.Progress.Complete,"Final 7/8 passes");Inspect();Call("Close");
            while(level.Transitioning)yield return null;
            Call("ToggleJournal");Inspect();Call("ToggleJournal");
            Move(level.caveExit.position);level.Interact(7);Next();Check(level.Progress.Complete&&level.Progress.RuneCount==3,"Leaving preserves completed season");
            Move(WinterWorld.Entrance);Call("LateUpdate");Check(level.Progress.InCave&&level.Progress.Complete,"Automatic re-entry preserves completion");
            Move(WinterWorld.BossPosition+Vector2.down*3);Call("RefreshWorld");
            Running=false;
            Debug.Log("WINTER_PLAY_PASS: "+States+" UI states, outdoor and cave routes, cave guard, return trips, Winter 3/5 retry and 4/5 pass, cumulative final 6/8 retry and 7/8 pass.");
        }
        static void Activity(int index,bool correct)
        {
            var q=WinterContent.Final[index];var panel=level.GetComponent<FinalActivityPanel>();Check(panel&&panel.IsOpen,"Interactive final opens");Inspect();
            SeasonIntegrationChecks.Capture("FinalActivity"+index);
            int[] answer=(int[])q.Expected.Clone();
            if(!correct)
            {
                if(q.Kind==FinalActivityKind.SelectMany)answer=new[]{Array.FindIndex(q.Choices,x=>Array.IndexOf(q.Expected,Array.IndexOf(q.Choices,x))<0)};
                else if(q.Kind==FinalActivityKind.Arrange)Array.Reverse(answer);
                else answer[0]=(answer[0]+1)%(q.Kind==FinalActivityKind.Match?q.Categories.Length:q.Choices.Length);
            }
            if(q.Kind==FinalActivityKind.Arrange)
            {
                for(int slot=0;slot<answer.Length;slot++)
                {
                    var area=ui.Dialog.transform.Find("Final activity");var source=area.Find("Card "+answer[slot]).gameObject;var target=area.Find("Space "+slot).gameObject;
                    var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerDrag=source,delta=new Vector2(429,0)};
                    UnityEngine.EventSystems.ExecuteEvents.Execute(source,e,UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
                    UnityEngine.EventSystems.ExecuteEvents.Execute(source,e,UnityEngine.EventSystems.ExecuteEvents.dragHandler);
                    UnityEngine.EventSystems.ExecuteEvents.Execute(target,e,UnityEngine.EventSystems.ExecuteEvents.dropHandler);
                    UnityEngine.EventSystems.ExecuteEvents.Execute(source,e,UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
                }
            }
            else if(q.Kind==FinalActivityKind.Match){for(int i=0;i<answer.Length;i++)for(int n=0;n<=answer[i];n++)panel.Select(i);}
            else foreach(int selected in answer)panel.Select(selected);
            Inspect();int before=level.Progress.BossQuestion;panel.GetComponentsInChildren<UnityEngine.UI.Button>()[panel.GetComponentsInChildren<UnityEngine.UI.Button>().Length-1].onClick.Invoke();
            Check(level.Progress.BossQuestion==before+1,"One submission advances exactly one activity");Inspect();
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
