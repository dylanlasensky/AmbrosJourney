using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SpringValidation
{
    static double started;
    static SpringValidation(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("SpringValidationPending",false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}};}
    [MenuItem("Capstone/Validate Spring Progression")]
    static void Run()
    {
        if(EditorApplication.isPlaying)return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");SessionState.SetBool("SpringValidationPending",true);
        EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup-started<3)return;
        var g=UnityEngine.Object.FindAnyObjectByType<SpringGuide>();if(g==null)return;
        EditorApplication.update-=Tick;SessionState.SetBool("SpringValidationPending",false);
        try
        {
            Require(Time.frameCount>1,"Player loop advances");Require(g.gateSprite!=null,"Gate art assigned");Require(g.batSheet!=null,"Bat art assigned");Require(g.guardianSheet!=null,"Distinct guardian art assigned");Require(UnityEngine.Object.FindAnyObjectByType<SpringCamera>().worldMax.x==32,"Expanded valley bounds");Require(!g.SpringComplete,"Gate initially locked");
            Set(g,"target",-1);for(int i=0;i<5;i++)Call(g,"Continue");Require(g.LessonComplete,"Mage tutorial completes");
            for(int i=0;i<3;i++){Set(g,"target",5+i);Call(g,"Continue");}
            Set(g,"target",0);Call(g,"Answer",1);Require(g.MissionsComplete==0,"Wrong myth answer does not clear mission");Call(g,"Continue");
            int[][] rounds={new[]{0,1,0},new[]{1,1,0},new[]{1,0,0}};
            for(int i=0;i<3;i++){Set(g,"target",i);foreach(int a in rounds[i]){Call(g,"Answer",a);Call(g,"Continue");}Require(g.MissionsComplete==i+1,"Three trials complete one landmark");}
            Set(g,"target",3);Set(g,"bossQuestion",0);Set(g,"feedback","");Set(g,"bossResult",false);
            int[] fail={0,0,1,1,0};foreach(var a in fail){Call(g,"Answer",a);Call(g,"Continue");}Require(g.BossScore==3 && !g.SpringComplete,"60 percent keeps gate locked");
            Call(g,"Continue");int[] pass={0,0,1,0,0};foreach(var a in pass){Call(g,"Answer",a);Call(g,"Continue");}Require(g.BossScore==4 && g.SpringComplete,"80 percent unlocks Summer");
            var boss=GameObject.Find("Thornhide Guardian");Require(boss==null,"Boss disappears after passing");
            Debug.Log("SPRING_VALIDATION_PASSED: tutorial, field notes, wrong-answer retry, nine challenges, failed boss, retry and 80 percent gate.");
        }
        catch(Exception e){Debug.LogError("SPRING_VALIDATION_FAILED: "+e);}
        EditorApplication.isPlaying=false;
    }
    static void Set(object o,string n,object v)=>o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,v);
    static void Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,a);
    static void Require(bool test,string message){if(!test)throw new Exception(message);}
}
