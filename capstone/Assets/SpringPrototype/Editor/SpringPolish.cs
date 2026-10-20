using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SpringPolish
{
    [MenuItem("Capstone/Polish Cottage and Learning Trail")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)return;
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");
        var cottage=GameObject.Find("The Mage's cottage");cottage.transform.localScale=Vector3.one*1.45f;
        var floor=GameObject.Find("Hand-laid winding trails").GetComponent<Tilemap>();
        var path=AssetDatabase.LoadAssetAtPath<Tile>("Assets/SpringPrototype/Tiles/Path.asset");
        for(int x=-23;x<=-17;x++)for(int y=9;y<=11;y++)floor.SetTile(new Vector3Int(x,y,0),path);
        for(int x=-23;x<=-21;x++)floor.SetTile(new Vector3Int(x,12,0),path);
        foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)){
            if(r.gameObject==cottage)continue;
            if(r.name=="Blossom canopy"||r.name=="Mossy boulder"||r.name=="Wildflower bed"){
                var p=r.transform.position;
                if(p.x> -29&&p.x< -16&&p.y>8&&p.y<21)Object.DestroyImmediate(r.gameObject);
            }
        }
        var g=Object.FindAnyObjectByType<SpringGuide>();g.StartPosition=new Vector2(-15,18);g.player.position=g.StartPosition;g.player.GetComponent<SpringPlayer>().spawnPoint=g.StartPosition;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());Debug.Log("COTTAGE_POLISHED: 45 percent larger, connected approach, clear entrance.");
    }
}
