#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
using AmbrosJourney.Fall;
using AmbrosJourney.Summer;

namespace AmbrosJourney.Presentation.Editor
{
    public static class PresentationPlayChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly List<string> inspected=new List<string>();
        static readonly string Output=Path.GetFullPath("Temp/PresentationChecks")+Path.DirectorySeparatorChar;
        static MonoBehaviour level;
        static SeasonOverlay ui;
        static void Check(bool value,string message){PresentationTools.Require(value,message);}
        static void Inspect()
        {
            typeof(SeasonOverlay).GetMethod("LateUpdate",Flags).Invoke(ui,null);Canvas.ForceUpdateCanvases();
            foreach(var t in ui.GetComponentsInChildren<TextMeshProUGUI>())
            {
                t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Text overflow: "+t.name+" / "+ui.Heading.text);
                var r=t.rectTransform.rect;Check(r.width>0&&r.height>0,"Text has positive dimensions");
            }
            inspected.Add(ui.Heading.text);
        }
        static void Next(){Inspect();var callback=(Action)level.GetType().GetField("next",Flags).GetValue(level);Check(callback!=null,"Continue available");callback();}
        static void Choice(int index){Inspect();ui.Choices[index].onClick.Invoke();}
        static void Common()
        {
            PresentationTools.ValidateMath();Check(UnityEngine.EventSystems.EventSystem.current,"EventSystem exists");
            Check(ui.GetComponent<UnityEngine.UI.GraphicRaycaster>(),"Canvas receives clicks");
            var origin=Vector2.zero;foreach(var d in new[]{Vector2.right,Vector2.up,Vector2.left,Vector2.down}){ui.SetCompass(origin,d*10,d*40);Check(Mathf.Abs(Mathf.DeltaAngle(ui.Arrow.localEulerAngles.z,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg))<.01f,"Gold arrow cardinal direction");Check(ui.ObjectiveDistance==40,"Compass shows objective distance, not lookahead distance");}
        }
        [MenuItem("Capstone/Presentation/Validate Fall Interface (Play Mode)")]
        public static void Fall()
        {
            try
            {
                var l=UnityEngine.Object.FindAnyObjectByType<FallLevel>();Check(l,"Run Fall first");level=l;ui=l.GetComponentInChildren<SeasonOverlay>();Check(!l.Progress.Introduced,"Start a fresh Fall session for UI regression");inspected.Clear();Common();
                Next();Next();Next();Check(!l.InputBlocked,"Introduction unlocks movement");
                for(int m=0;m<3;m++)
                {
                    l.player.transform.position=l.stations[m].position+Vector3.down;l.player.GetComponent<Rigidbody2D>().position=l.player.transform.position;l.Interact(m+1);Next();
                    for(int q=0;q<FallContent.Missions[m].Length;q++){int answer=FallContent.Missions[m][q].Correct;if(q==0){Choice((answer+1)%3);Next();}Choice(answer);Next();}
                    Next();Check(l.Progress.MissionComplete(m),"Mission completed through visible buttons");
                }
                l.Interact(4);Next();for(int q=0;q<5;q++){int c=FallContent.Boss[q].Correct;Choice(q<3?c:(c+1)%3);Next();}Check(!l.Progress.Complete,"3/5 retries");Next();
                for(int q=0;q<5;q++){int c=FallContent.Boss[q].Correct;Choice(q<4?c:(c+1)%3);Next();}Check(l.Progress.Complete,"4/5 passes");Next();
                level.GetType().GetMethod("ToggleJournal",Flags).Invoke(level,null);Inspect();level.GetType().GetMethod("ToggleJournal",Flags).Invoke(level,null);
                Check(l.ember.follow==l.femaleWarrior.transform,"Emberfist recruited");
                foreach(var actor in new[]{l.mage,l.maleWarrior,l.femaleWarrior,l.ember})Check(actor.walk!=null&&actor.walk.Length>0,"All companions have walk frames");
                Save("Fall",true,"Visible button flow, all three-choice layouts, feedback, journal, guardian retry, shared compass cardinal directions and trail math.");
                l.Interact(1);Inspect();
            }
            catch(Exception e){Save("Fall",false,e.ToString());Debug.LogException(e);}
        }
        [MenuItem("Capstone/Presentation/Validate Summer Interface (Play Mode)")]
        public static void Summer()
        {
            try
            {
                var l=UnityEngine.Object.FindAnyObjectByType<SummerLevel>();Check(l,"Run Summer first");level=l;ui=l.GetComponentInChildren<SeasonOverlay>();Check(!l.Progress.Introduced,"Start a fresh Summer session for UI regression");inspected.Clear();Common();
                Next();Next();Next();l.Interact(1);Next();Choice(0);Next();Choice(1);Next();Next();
                l.Interact(2);Choice(0);Next();l.Interact(6);Next();l.Interact(8);Next();l.Interact(7);Next();Choice(0);Next();Next();
                l.Interact(3);Choice(0);Choice(1);Choice(2);Next();Choice(1);Next();Next();
                l.Interact(4);Next();for(int q=0;q<5;q++){Choice(SummerContent.Boss[q].Correct);Next();}Next();
                level.GetType().GetMethod("ToggleJournal",Flags).Invoke(level,null);Inspect();level.GetType().GetMethod("ToggleJournal",Flags).Invoke(level,null);
                Check(l.Progress.Complete,"Summer progression preserved");Save("Summer",true,"Visible button flow, three-option privacy question, journal, shared compass and existing progression.");l.Interact(5);Inspect();
            }
            catch(Exception e){Save("Summer",false,e.ToString());Debug.LogException(e);}
        }
        static void Save(string season,bool passed,string detail)
        {
            Directory.CreateDirectory(Output);File.WriteAllText(Output+season+"PresentationChecks.txt",(passed?"PASS":"FAIL")+"\n"+detail+"\nStates inspected: "+inspected.Count+"\n");Debug.Log((passed?"PRESENTATION_PASS: ":"PRESENTATION_FAIL: ")+season+" / "+detail);
        }
    }
}
#endif
