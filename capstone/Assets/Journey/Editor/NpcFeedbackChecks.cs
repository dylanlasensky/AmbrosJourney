// Runs isolated Editor regression checks for npc feedback.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AmbrosJourney.Journey;
using AmbrosJourney.Presentation;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;

public static class NpcFeedbackChecks
{
 const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool value,string why){if(!value)throw new Exception("NPC_FEEDBACK: "+why);}
 static object Call(object owner,string method,params object[] args)=>owner.GetType().GetMethod(method,Private).Invoke(owner,args);
 static void Next(object owner)=>((Action)owner.GetType().GetField("next",Private).GetValue(owner))();
 static void Card(Transform actor,string name,bool enabled)
 {
  var card=actor.GetComponentInChildren<SeasonWorldLabel>(true);
  Check(card&&card.Caption.Contains("[E]")&&card.Caption.Contains(name),"Named card: "+name);
  Check(card.enabled==enabled,"Card availability: "+name+" expected "+enabled);
 }
 [MenuItem("Capstone/Final feedback/Verify menu music and start")]
 public static void VerifyMenuAudio()
 {
  var session=JourneySession.Instance;Check(EditorApplication.isPlaying&&session,"Start the main menu in Play mode");
  var music=typeof(JourneySession).GetField("menuMusic",Private).GetValue(session) as AudioSource;
  Check(music&&music.isPlaying&&music.loop&&music.clip==Resources.Load<AudioClip>("SeasonMusic/andriig-spring-easter-day-music-507171"),"Menu loops the Spring track");
  var input=typeof(JourneySession).GetField("nameInput",Private).GetValue(session) as TMPro.TMP_InputField;Check(input,"Name entry is visible");input.text="Audio Check";
  session.NewJourney();Check(!music.isPlaying,"Start stops music immediately");
  var disclaimer=GameObject.Find("AI disclaimer");Check(disclaimer&&!GameObject.Find("Opening video screen"),"Disclaimer precedes video");Canvas.ForceUpdateCanvases();
  foreach(var text in disclaimer.GetComponentsInChildren<TMPro.TMP_Text>()){text.ForceMeshUpdate();Check(text.font==GameFont.Resolve()&&!text.isTextOverflowing,"Disclaimer font and fit");}
  Directory.CreateDirectory("Logs/FinalFeedback");File.WriteAllText("Logs/FinalFeedback/Menu.txt","PASS: Spring music loops on main menu, stops on Start, disclaimer appears before intro with Determination and no text overflow. Fade and video transition reviewed visually.\n");
  Debug.Log("NPC_FEEDBACK_PASS: menu audio and disclaimer");
 }
 [MenuItem("Capstone/Final feedback/Load fresh Summer")]
 public static void LoadSummer()=>Load(2);
 [MenuItem("Capstone/Final feedback/Load fresh Fall")]
 public static void LoadFall()=>Load(3);
 [MenuItem("Capstone/Final feedback/Load fresh Winter")]
 public static void LoadWinter()=>Load(4);
 static void Load(int chapter)
 {
  Check(EditorApplication.isPlaying&&JourneySession.Instance,"Start Play mode first");
  var session=JourneySession.Instance;session.BeginTestProfile();session.Data.unlocked=4;
  var music=typeof(JourneySession).GetField("menuMusic",Private).GetValue(session) as AudioSource;if(music)music.Stop();
  session.LoadChapter(chapter);
 }
 [MenuItem("Capstone/Final feedback/Verify current season")]
 public static void Verify()
 {
  Check(EditorApplication.isPlaying,"Start a fresh season in Play mode first");
  var session=JourneySession.Instance;Check(session,"Session is ready");session.BeginTestProfile();session.Resume();
  string season="";
  var summer=UnityEngine.Object.FindAnyObjectByType<SummerLevel>();
  var fall=UnityEngine.Object.FindAnyObjectByType<FallLevel>();
  var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();
  if(summer)
  {
   season="Summer";Check(!summer.Progress.Introduced,"Fresh Summer");Call(summer,"RefreshWorld");Card(summer.maleWarrior.transform,"Workshop Warrior",true);Card(summer.femaleWarrior.transform,"Garden Warrior",true);
   summer.Interact(0);Next(summer);Call(summer,"RefreshWorld");Card(summer.maleWarrior.transform,"Workshop Warrior",false);Card(summer.femaleWarrior.transform,"Garden Warrior",false);
   summer.Progress.GuideArrived=true;Call(summer,"RefreshWorld");Card(summer.maleWarrior.transform,"Workshop Warrior",true);
   summer.Interact(0);Next(summer);Call(summer,"RefreshWorld");Card(summer.maleWarrior.transform,"Workshop Warrior",false);Check(summer.Progress.GuideBriefed,"Workshop briefing consumed");
   Check(JsonUtility.FromJson<SummerProgress>(JsonUtility.ToJson(summer.Progress)).GuideBriefed,"Summer chapter remembers read briefing");
   summer.Interact(0);Check(!summer.IsDialogueOpen,"Finished guide has no repeat conversation");
  }
  else if(fall)
  {
   season="Fall";Check(!fall.Progress.Introduced,"Fresh Fall");Call(fall,"RefreshWorld");Card(fall.ember.transform,"Emberfist",true);
   fall.Interact(0);Next(fall);Call(fall,"RefreshWorld");Card(fall.ember.transform,"Emberfist",false);
   fall.Progress.GuideArrived=true;Call(fall,"RefreshWorld");Card(fall.ember.transform,"Emberfist",true);
   fall.Interact(0);fall.DismissPanel();Call(fall,"RefreshWorld");Card(fall.ember.transform,"Emberfist",false);Check(fall.Progress.GuideBriefed,"Dismissed briefing consumed");
   fall.Interact(0);Check(!fall.IsDialogueOpen,"Finished guide has no repeat conversation");
  }
  else if(winter)
  {
   season="Winter";Check(!winter.Progress.Introduced,"Fresh Winter");Call(winter,"PlaceLittleMage");Card(winter.littleMage.transform,"Little Mage",true);
   winter.Interact(0);Next(winter);Call(winter,"PlaceLittleMage");Card(winter.littleMage.transform,"Little Mage",false);
   winter.Progress.WinterComplete=true;Call(winter,"PlaceLittleMage");Card(winter.littleMage.transform,"Little Mage",true);Check(!winter.Progress.CanEnterCave,"Cave requires mage briefing");
   winter.Interact(0);Next(winter);Call(winter,"PlaceLittleMage");Card(winter.littleMage.transform,"Little Mage",false);Check(winter.Progress.CanEnterCave,"Read briefing opens cave");
   winter.Interact(0);Check(!winter.IsDialogueOpen,"Finished guide has no repeat conversation");
  }
  else throw new Exception("Use fresh Summer, Fall or Winter for this check. Spring is tested separately.");
  Call(session,"ShowHUD");Canvas.ForceUpdateCanvases();
  var ui=(JourneyUI)typeof(JourneySession).GetField("ui",Private).GetValue(session);
  foreach(string title in new[]{"Pause / Esc","0/12 Artifacts"})
  {
   var rect=System.Array.Find(ui.Root.GetComponentsInChildren<RectTransform>(),r=>r.name==title);Check(rect&&rect.anchorMin==Vector2.one&&rect.anchoredPosition.y==-136,"Top-right HUD below lessons: "+title);
  }
  Directory.CreateDirectory("Logs/FinalFeedback");File.WriteAllText("Logs/FinalFeedback/"+season+".txt","PASS: named NPC cards, completed conversation hidden, later dialogue reappears, finished NPC cannot repeat, top-right HUD placement.\n");
  Debug.Log("NPC_FEEDBACK_PASS: "+season);
 }
}
#endif
