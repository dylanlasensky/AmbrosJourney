#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using AmbrosJourney.Presentation;

namespace AmbrosJourney.Journey.Editor
{
    public static class JourneyPerformanceChecks
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Check(bool ok,string message){if(!ok)throw new Exception("JOURNEY_PERFORMANCE: "+message);}
        public static void VerifyCurrentScene()
        {
            var session=JourneySession.Instance;var player=session.Player();
            Check(player&&player.gameObject.scene==SceneManager.GetActiveScene(),"Player cache belongs to current chapter after travel");
            for(int i=0;i<200;i++)Check(session.Player()==player,"Repeated lookups preserve the active player");
            var textures=new HashSet<Texture2D>();
            foreach(var trail in UnityEngine.Object.FindObjectsByType<NaturalTrail>(FindObjectsInactive.Include))
                foreach(var renderer in trail.GetComponentsInChildren<SpriteRenderer>(true))if(renderer.sprite)textures.Add(renderer.sprite.texture);
            Check(textures.Count>0,"Trail surfaces exist");
            foreach(var map in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include))
            {
                string name=map.name.ToLowerInvariant();if(!name.Contains("pond")&&!name.Contains("river")&&!name.Contains("lake"))continue;
                foreach(var cell in map.cellBounds.allPositionsWithin)if(map.GetTile(cell) is Tile tile&&tile.sprite)textures.Add(tile.sprite.texture);
            }
            long releasedBytes=0;
            foreach(var texture in textures){Check(!texture.isReadable,"Static terrain does not keep a duplicate CPU image");releasedBytes+=(long)texture.width*texture.height*4;}
            VerifyAtlasBounds(UnityEngine.Object.FindAnyObjectByType<WorldCohesion>());
            VerifyContextualLabels();VerifyLandmarkLayout();VerifyCameraResize();
            Debug.Log("JOURNEY_PERFORMANCE_SCENE_PASS: "+SceneManager.GetActiveScene().name+"; terrain CPU copies released "+releasedBytes+" bytes; atlas bounds unchanged.");
        }
        static void VerifyContextualLabels()
        {
            var scene=SceneManager.GetActiveScene();int count=0,bossCount=0;
            foreach(var marker in UnityEngine.Object.FindObjectsByType<SeasonWorldLabel>(FindObjectsInactive.Include))
            {
                if(marker.gameObject.scene!=scene)continue;count++;
                Check(marker.Caption.StartsWith("[E] "),"World cards describe interactions, not decorative location headers: "+marker.Caption);
                Check(marker.Anchored&&marker.RevealDistance>0&&marker.RevealDistance<=5.5f,"Prompt follows a real target and stays local: "+marker.Caption);
                var origin=marker.InteractionPosition;
                Check(marker.InRange(origin+Vector2.right*(marker.RevealDistance-.1f),8),"Prompt is available within its interaction range");
                Check(!marker.InRange(origin+Vector2.right*(marker.RevealDistance+.1f),8),"Prompt hides before promising an out-of-range interaction");
                if(marker.Caption.Contains("GUARDIAN")||marker.Caption.Contains("WARDEN")||marker.Caption.Contains("SENTINEL")||marker.Caption.Contains("ECHO KEEPER"))
                {
                    bossCount++;Check(marker.InRange(origin+Vector2.down*4.5f,8),"Boss prompt supports the larger battle radius");
                    Check(!marker.InRange(origin+Vector2.down*4.5f,3.8f),"Cave tunnel visibility still limits distant labels");
                }
            }
            Check(count>0&&bossCount>0,"Every chapter retains useful nearby prompts including its boss");
            var world=UnityEngine.Object.FindAnyObjectByType<JourneyWorld>();
            foreach(var artifact in world.Artifacts)
            {
                var marker=artifact.GetComponentInChildren<SeasonWorldLabel>(true);
                Check(marker&&marker.Caption=="[E] Collect artifact"&&Mathf.Approximately(marker.RevealDistance,2.3f),"Collectible has one compact E prompt at pickup range");
                Check(Vector2.Distance(marker.InteractionPosition,artifact.transform.position)<.001f,"Artifact range is measured from the item rather than its floating text");
            }
        }
        static void VerifyCameraResize()
        {
            var host=new GameObject("Temporary camera resize check");var target=new GameObject("Temporary camera target");
            var view=host.AddComponent<Camera>();view.enabled=false;view.orthographic=true;
            try
            {
                var min=new Vector2(-40,-34);var max=new Vector2(40,36);
                foreach(var corner in new[]{new Vector2(-1,-1),new Vector2(-1,1),new Vector2(1,-1),new Vector2(1,1)})
                {
                    view.orthographicSize=5;view.aspect=4f/3;
                    var position=new Vector3(corner.x<0?min.x+view.orthographicSize*view.aspect:max.x-view.orthographicSize*view.aspect,corner.y<0?min.y+5:max.y-5,-10);
                    view.transform.position=position;target.transform.position=new Vector3(position.x,position.y,0);
                    view.orthographicSize=7;view.aspect=16f/9;var velocity=new Vector3(corner.x*2,corner.y*2,0);
                    SeasonCameraMotion.Follow(view,target.transform,min,max,ref velocity);
                    var result=view.transform.position;float halfWidth=view.orthographicSize*view.aspect;
                    Check(result.x-halfWidth>=min.x-.001f&&result.x+halfWidth<=max.x+.001f&&result.y-7>=min.y-.001f&&result.y+7<=max.y+.001f,"First resize frame keeps every camera corner within the map");
                    Check(!float.IsNaN(result.x)&&!float.IsNaN(result.y),"Resized camera coordinates remain finite");
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(host);UnityEngine.Object.DestroyImmediate(target);}
        }
        static void VerifyLandmarkLayout()
        {
            var place=typeof(SeasonOverlay).GetMethod("TryPlaceLabel",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(var size in new[]{new Vector2(1280,720),new Vector2(1280,960),new Vector2(1280,1280),new Vector2(1680,720)})
            {
                var occupied=new List<Rect>{new Rect(16,16,306,99),new Rect(size.x-436,16,420,116),new Rect(16,size.y-70,796,54)};
                occupied.Add(new Rect(size.x*.5f-35,size.y*.4f-45,70,90));
                var requests=new[]{new Rect(size.x*.5f-80,size.y*.4f-20,160,44),new Rect(24,40,280,44),new Rect(size.x-420,60,310,44),new Rect(24,size.y-55,240,44),new Rect(-60,220,180,44),new Rect(size.x-70,220,180,44)};
                foreach(var desired in requests)
                {
                    var args=new object[]{desired,size,occupied,new Rect()};
                    Check((bool)place.Invoke(null,args),"Landmark card finds clear space beside fixed HUD");
                    var result=(Rect)args[3];
                    Check(result.xMin>=8&&result.yMin>=8&&result.xMax<=size.x-8&&result.yMax<=size.y-8,"Landmark card stays inside screen");
                    foreach(var other in occupied)Check(!result.Overlaps(other),"Landmark card avoids HUD and earlier cards");
                    occupied.Add(result);
                }
                var blocked=new object[]{new Rect(24,40,280,44),size,new List<Rect>{new Rect(Vector2.zero,size)},new Rect()};
                Check(!(bool)place.Invoke(null,blocked),"Crowded landmark card hides instead of covering required UI");
            }
            var overlay=UnityEngine.Object.FindAnyObjectByType<SeasonOverlay>();
            var layer=overlay.transform.Find("Landmark labels");
            Check(layer&&overlay.Arrow.GetSiblingIndex()>layer.GetSiblingIndex(),"Guide arrow remains above landmark labels");
            VerifyPlayerLabelClearance(overlay,layer);
        }
        static void VerifyPlayerLabelClearance(SeasonOverlay overlay,Transform layer)
        {
            var player=JourneySession.Instance.Player();
            Check(player&&player.gameObject.scene==SceneManager.GetActiveScene()&&player.GetComponent<Rigidbody2D>(),"Label check uses the current chapter player, not a temporary transform");
            var visual=player.GetComponent<SpriteRenderer>();var view=Camera.main;
            Check(visual&&visual.sprite&&view,"Visible player has sprite bounds for label placement");
            var host=new GameObject("Player visibility regression label");host.transform.position=visual.bounds.center;
            var marker=host.AddComponent<SeasonWorldLabel>();marker.Caption="Player visibility regression label";marker.Width=160;
            var refresh=typeof(SeasonOverlay).GetMethod("UpdateLabels",Private);
            try
            {
                Canvas.ForceUpdateCanvases();refresh.Invoke(overlay,null);Canvas.ForceUpdateCanvases();
                var sample=layer.Find(marker.Caption);Check(sample,"Nearby regression label is registered");
                var root=overlay.GetComponent<RectTransform>();var point=view.WorldToViewportPoint(marker.Position);
                Check(point.z>0&&point.x>=0&&point.x<=1&&point.y>=0&&point.y<=1&&!overlay.Dialog.activeSelf&&!overlay.Journal.activeSelf,"Live label clearance is checked on screen during exploration");
                if(!sample.gameObject.activeInHierarchy)
                {
                    // Real guide cards take priority. The extra test card may hide only when
                    // the bounded layout cannot place it clear of the player and existing UI.
                    var occupied=(List<Rect>)typeof(SeasonOverlay).GetField("labelAreas",Private).GetValue(overlay);
                    var desired=new Rect(point.x*root.rect.width-marker.Width/2,(1-point.y)*root.rect.height,marker.Width,44);
                    var args=new object[]{desired,root.rect.size,occupied,new Rect()};
                    var place=typeof(SeasonOverlay).GetMethod("TryPlaceLabel",BindingFlags.Static|BindingFlags.NonPublic);
                    Check(!(bool)place.Invoke(null,args),"Extra crowded card hides only when no clear label slot remains");
                }
                var lower=view.WorldToScreenPoint(visual.bounds.min);var upper=view.WorldToScreenPoint(visual.bounds.max);
                var silhouette=Rect.MinMaxRect(Mathf.Min(lower.x,upper.x),Mathf.Min(lower.y,upper.y),Mathf.Max(lower.x,upper.x),Mathf.Max(lower.y,upper.y));
                Camera uiCamera=overlay.Canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:overlay.Canvas.worldCamera;
                var corners=new Vector3[4];
                foreach(RectTransform card in layer)
                {
                    if(!card.gameObject.activeInHierarchy)continue;card.GetWorldCorners(corners);
                    var first=RectTransformUtility.WorldToScreenPoint(uiCamera,corners[0]);var last=RectTransformUtility.WorldToScreenPoint(uiCamera,corners[2]);
                    var area=Rect.MinMaxRect(Mathf.Min(first.x,last.x),Mathf.Min(first.y,last.y),Mathf.Max(first.x,last.x),Mathf.Max(first.y,last.y));
                    Check(!area.Overlaps(silhouette),"World name card leaves Ambros visible: "+card.name);
                }
                VerifyArrowLabelClearance(overlay,layer,refresh,marker);
                host.transform.position=player.position+Vector3.right*(marker.RevealDistance+.2f);
                refresh.Invoke(overlay,null);Check(!sample.gameObject.activeSelf,"Walking out of interaction range hides the rendered prompt");
                host.transform.position=player.position;
                marker.enabled=false;refresh.Invoke(overlay,null);Check(!sample.gameObject.activeSelf,"Unavailable or completed interactions do not keep a name card");
                marker.enabled=true;refresh.Invoke(overlay,null);
                bool dialogOpen=overlay.Dialog.activeSelf;
                try{overlay.Dialog.SetActive(true);refresh.Invoke(overlay,null);foreach(Transform card in layer)Check(!card.gameObject.activeSelf,"Conversation keeps world prompts out of the reading area");}
                finally{overlay.Dialog.SetActive(dialogOpen);refresh.Invoke(overlay,null);}
            }
            finally
            {
                host.SetActive(false);refresh.Invoke(overlay,null);UnityEngine.Object.DestroyImmediate(host);refresh.Invoke(overlay,null);Canvas.ForceUpdateCanvases();
            }
        }
        static void VerifyArrowLabelClearance(SeasonOverlay overlay,Transform layer,MethodInfo refresh,SeasonWorldLabel marker)
        {
            var arrow=overlay.Arrow;bool active=arrow.gameObject.activeSelf;
            var position=arrow.localPosition;var rotation=arrow.localRotation;var scale=arrow.localScale;
            var view=Camera.main;var cameraPosition=view.transform.position;var markerPosition=marker.transform.position;
            var otherMarkers=new List<SeasonWorldLabel>(SeasonWorldLabel.Active);
            Camera uiCamera=overlay.Canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:overlay.Canvas.worldCamera;
            var corners=new Vector3[4];
            Rect ScreenBounds(RectTransform target)
            {
                target.GetWorldCorners(corners);var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                foreach(var corner in corners){var point=RectTransformUtility.WorldToScreenPoint(uiCamera,corner);low=Vector2.Min(low,point);high=Vector2.Max(high,point);}
                return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
            }
            try
            {
                // Isolate this collision from a crowded arrival view. The preceding check still
                // inspects real chapter cards; here the same renderer must keep both targets visible.
                var player=JourneySession.Instance.Player();view.transform.position=new Vector3(player.position.x,player.position.y,cameraPosition.z);
                marker.transform.position=player.position+Vector3.right*2;
                foreach(var other in otherMarkers)if(other&&other!=marker)other.enabled=false;
                foreach(float angle in new[]{0f,37f,90f,225f})foreach(float pulse in new[]{1f,1.05f})
                {
                    arrow.gameObject.SetActive(false);refresh.Invoke(overlay,null);Canvas.ForceUpdateCanvases();
                    var covered=layer.Find(marker.Caption) as RectTransform;
                    Check(covered&&covered.gameObject.activeInHierarchy,"Controlled nearby prompt is visible before every arrow collision");
                    arrow.gameObject.SetActive(true);arrow.position=covered.TransformPoint(covered.rect.center);
                    arrow.localRotation=Quaternion.Euler(0,0,angle);arrow.localScale=Vector3.one*pulse;
                    Canvas.ForceUpdateCanvases();Check(ScreenBounds(arrow).Overlaps(ScreenBounds(covered)),"Regression begins with arrow overlapping the prompt");
                    refresh.Invoke(overlay,null);Canvas.ForceUpdateCanvases();var arrowArea=ScreenBounds(arrow);
                    Check(covered.gameObject.activeInHierarchy,"Collision moves the useful E card instead of removing it in a clear view");
                    foreach(RectTransform card in layer)if(card.gameObject.activeInHierarchy)
                        Check(!ScreenBounds(card).Overlaps(arrowArea),"Nearby E prompt stays clear of rotated and pulsing arrow: "+card.name);
                }
            }
            finally
            {
                view.transform.position=cameraPosition;marker.transform.position=markerPosition;
                foreach(var other in otherMarkers)if(other&&other!=marker)other.enabled=true;
                arrow.localPosition=position;arrow.localRotation=rotation;arrow.localScale=scale;arrow.gameObject.SetActive(active);
                refresh.Invoke(overlay,null);Canvas.ForceUpdateCanvases();
            }
        }
        static void VerifyAtlasBounds(WorldCohesion cohesion)
        {
            Check(cohesion,"World art initialized");
            // The journal can ask for any season after initialization, so include those late requests.
            for(int id=0;id<12;id++)cohesion.ArtifactIcon(id);
            var sprites=(Dictionary<string,Sprite>)typeof(WorldCohesion).GetField("sprites",Private).GetValue(cohesion);
            var pixels=new Dictionary<Texture2D,Color32[]>();
            foreach(var sprite in sprites.Values)
            {
                string[] name=sprite.name.Split(new[]{" / "},StringSplitOptions.None);string atlas=name[0];int index=int.Parse(name[1]);
                bool worldcraft=atlas=="SeasonalRelics"||atlas=="SeasonalDetails";int columns=worldcraft||atlas=="VillageDetails"?3:2;
                float[] xs=worldcraft?new[]{0f,1f/3,2f/3,1f}:columns==3?new[]{0f,.35f,.64f,1f}:atlas=="DiscoveryDetails"?new[]{0f,.55f,1f}:new[]{0f,.5f,1f};
                float[] ys=atlas=="SeasonalRelics"?new[]{0f,.25f,.495f,.735f,1f}:worldcraft?new[]{0f,.25f,.5f,.75f,1f}:atlas=="WinterNature"?new[]{0f,.66f,1f}:columns==3?new[]{0f,.39f,.665f,1f}:atlas=="DiscoveryDetails"?new[]{0f,.55f,1f}:new[]{0f,.5f,1f};
                var texture=sprite.texture;if(!pixels.TryGetValue(texture,out var colors)){colors=texture.GetPixels32();pixels[texture]=colors;}
                int column=index%columns,row=index/columns;
                int left=Mathf.RoundToInt(xs[column]*texture.width),right=Mathf.RoundToInt(xs[column+1]*texture.width);
                int bottom=Mathf.RoundToInt((1-ys[row+1])*texture.height),top=Mathf.RoundToInt((1-ys[row])*texture.height);
                int x0=right,y0=top,x1=left,y1=bottom;
                for(int y=bottom;y<top;y++)for(int x=left;x<right;x++)if(colors[y*texture.width+x].a>100){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);}
                Check(x0<=x1&&y0<=y1,"Nonempty atlas cell: "+sprite.name);
                Check(sprite.rect==new Rect(x0,y0,x1-x0+1,y1-y0+1),"Cached bounds equal original per-cell crop: "+sprite.name);
                Check(sprite.pivot.y==0,"Art remains grounded at its original pivot");
            }
        }
        public static void VerifyKeyboardNavigation()
        {
            var priorKeyboard=Keyboard.current;var eventSystem=EventSystem.current;Check(eventSystem,"Keyboard test has event system");
            var priorSelection=eventSystem.currentSelectedGameObject;var host=new GameObject("Temporary keyboard performance checks");var ui=host.AddComponent<JourneyUI>();
            var keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            try
            {
                ui.Initialize();
                var first=ui.Button(ui.Root,"First",new Rect(0,0,50,40),()=>{});
                var disabled=ui.Button(ui.Root,"Disabled",new Rect(0,50,50,40),()=>{});disabled.interactable=false;
                var last=ui.Button(ui.Root,"Last",new Rect(0,100,50,40),()=>{});
                var tick=(Action)Delegate.CreateDelegate(typeof(Action),ui,typeof(JourneyUI).GetMethod("LateUpdate",Private));
                eventSystem.SetSelectedGameObject(null);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftShift,Key.Tab));InputSystem.Update();tick();
                Check(eventSystem.currentSelectedGameObject==last.gameObject,"Shift+Tab with no focus selects final usable control");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();tick();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));InputSystem.Update();tick();
                Check(eventSystem.currentSelectedGameObject==first.gameObject,"Tab wraps past disabled controls");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();tick();
                long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)tick();long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
                Check(allocated==0,"Unchanged menu focus does not allocate arrays each frame ("+allocated+" bytes)");
                first.interactable=false;tick();Check(eventSystem.currentSelectedGameObject==last.gameObject,"Focus recovers when selected control becomes unavailable");
                last.gameObject.SetActive(false);tick();
                Debug.Log("JOURNEY_PERFORMANCE_KEYBOARD_PASS: Tab/Shift+Tab, disabled and hidden controls, 100 idle focus frames allocate zero bytes.");
            }
            finally
            {
                eventSystem.SetSelectedGameObject(priorSelection);InputSystem.RemoveDevice(keyboard);if(priorKeyboard!=null)priorKeyboard.MakeCurrent();UnityEngine.Object.DestroyImmediate(host);
            }
        }
    }
}
#endif
