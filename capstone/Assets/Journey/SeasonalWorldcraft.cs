using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Journey
{
    public sealed partial class WorldCohesion
    {
        public readonly List<SpriteRenderer> SeasonalDetails=new List<SpriteRenderer>();
        public readonly List<SpriteRenderer> RidgePieces=new List<SpriteRenderer>();

        // Authored little places, rather than random scattering across the playable lawn.
        void ComposeSeasonalDetails(WinterLevel winter)
        {
            if(season==1)
            {
                Detail("Cottage blossom ferns",1,-24,7,1.4f);
                Detail("Cottage fern border",1,-26,6,1.8f);
                Detail("Lakeside fallen blossom log",0,-13,2,2.4f);
                Detail("Lakeside fern bank",1,-12,-1,1.7f);
                Detail("Reflection clearing log",0,7,-9,2.3f);
                Detail("Reflection clearing flowers",1,9,-8,1.6f);
                Detail("Southern woodland log",0,24,-21,2.3f);
                Detail("Southern woodland ferns",1,25,-18,1.7f);
            }
            else if(season==2)
            {
                Detail("Garden watering supplies",4,-26,-10,1.65f);
                Detail("Garden pollinator hive",3,-24,-16,1.7f);
                Detail("Second garden hive",3,-27,-15.5f,1.5f);
                Detail("Wellside herb pots",4,24,18,1.5f);
                Detail("Pond west reeds",5,-2,3,1.6f);
                Detail("Pond southern reeds",5,3,1,1.8f);
                Detail("Pond eastern reeds",5,7,5,1.65f);
                Detail("Workshop fern border",5,-26,9,1.4f);
            }
            else if(season==3)
            {
                Detail("Orchard sorting basket",6,-36,3,1.7f);
                Detail("Orchard woodland stump",7,-37,7,2.1f);
                Detail("Orchard harvest basket",6,-33,11,1.4f);
                Detail("Overlook woodland stump",7,-45,-25,1.9f);
                Detail("Overlook mushroom hollow",7,-43,-28,1.7f);
                Detail("Archive mushroom hollow",7,34,29,1.8f);
                Detail("Kiln fuel and kettle",8,34,-24,1.8f);
                Detail("Kiln woodland mushrooms",7,36,-20,1.8f);
                Detail("North bridge woodland",7,-9,22,1.8f);
            }
            else
            {
                Detail("Ledger snow-covered rocks",9,-33,-5,2.0f);
                Detail("Ledger snowberry border",10,-34,-8,1.8f);
                Detail("Ledger windblown stones",9,-31,-11,2.1f);
                Detail("Observatory expedition sled",11,24.8f,10.8f,1.8f);
                Detail("Observatory snowberry border",10,26,13,1.8f);
                Detail("Observatory windblown stones",9,30,12,2.1f);
                Detail("Ridge approach snowberry",10,16,21,1.7f);
                Detail("Ridge approach stones",9,13,21,2.1f);
                ComposeRidge(winter);
            }
        }

        void Detail(string label,int index,float x,float y,float width)
        {
            var sprite=Art("SeasonalDetails",index,width);
            var trails=FindObjectsByType<NaturalTrail>();
            var anchor=new Vector2(x,y);
            // Search only within the authored cluster. Do not drift into an unrelated landmark.
            foreach(var offset in new[]{Vector2.zero,new Vector2(-1.2f,0),new Vector2(1.2f,0),new Vector2(0,-1.2f),new Vector2(0,1.2f)})
            {
                var p=anchor+offset;
                if(JourneyWorld.ArtifactPositions.Any(a=>Vector2.Distance(a,p)<2.0f))continue;
                if(trails.Any(t=>t.Near(p,width*.5f+.3f)))continue;
                var bounds=new Bounds((Vector3)p+sprite.bounds.center,sprite.bounds.size);bounds.Expand(.2f);
                if(propFootprints.Any(b=>b.Intersects(bounds)))continue;
                if(original.Any(r=>r&&r.gameObject.activeInHierarchy&&r.sprite&&(r.sprite.name.StartsWith("VillageDetails")||r.sprite.name.StartsWith("UtilityDetails"))&&r.bounds.Intersects(bounds)))continue;
                // Check the whole base, not merely its centre; otherwise broad props straddle paths/walls.
                var collisions=Physics2D.OverlapBoxAll(p+Vector2.up*.25f,new Vector2(width+.2f,.7f),0);
                if(collisions.Any(c=>!c.TryGetComponent<SpriteRenderer>(out var r)||!Tree(r)))continue;
                // A clear trunk footprint is not enough: a southern canopy can hide the entire prop.
                foreach(var r in original)if(r&&r.gameObject.activeInHierarchy&&Tree(r)&&
                    (r.bounds.Intersects(bounds)&&r.sortingOrder>100-Mathf.RoundToInt(p.y*10)||collisions.Any(c=>c.gameObject==r.gameObject)))Remove(r.gameObject);
                // Remove low loose clutter beneath the new authored vignette, leaving trees and missions intact.
                foreach(var r in original)if(r&&Loose(r)&&!Tree(r)&&bounds.Contains(r.transform.position))Remove(r.gameObject);
                var obj=new GameObject(label,typeof(SpriteRenderer));obj.transform.SetParent(outside,false);obj.transform.position=p;
                var renderer=obj.GetComponent<SpriteRenderer>();Skin(renderer,"SeasonalDetails",index,width);
                SeasonalDetails.Add(renderer);propFootprints.Add(bounds);Physics2D.SyncTransforms();return;
            }
            Debug.Log("WORLDCRAFT: omitted crowded decoration: "+label);
        }

        public void FrameRelic(SpriteRenderer relic)
        {
            foreach(var tree in original)if(tree&&tree.gameObject.activeInHierarchy&&Tree(tree)&&
                tree.sortingOrder>relic.sortingOrder&&tree.bounds.Intersects(relic.bounds))Remove(tree.gameObject);
        }

        public readonly List<Vector2> PortalThresholds=new List<Vector2>();
        void AnchorPortals()
        {
            foreach(var portal in original.Where(r=>r&&r.sprite&&r.sprite.texture.name=="SeasonPortals"))
            {
                if(portal.name=="Daylight exit")continue; // Interior uses its stone floor, not an outdoor dirt trail.
                Vector2 previous=portal.transform.position,position=previous,connection=previous+Vector2.up*3;
                switch(portal.name)
                {
                    case "Spring arrival portal":position=new Vector2(0,23);connection=new Vector2(0,22);break;
                    case "Summer arrival portal":position=new Vector2(-38,36);connection=new Vector2(-38,34);break;
                    case "Autumn arrival":position=new Vector2(-30,-23);connection=WinterLevel.Spawn;break;
                    case "Fall portal":position=new Vector2(.5f,-30);connection=new Vector2(.5f,-27);break;
                    case "Winter portal":position=new Vector2(.5f,-46);connection=new Vector2(.5f,-43);break;
                }
                portal.transform.position=position;portal.sortingOrder=100-Mathf.RoundToInt(position.y*10);
                ClearRect(new Rect(position.x-2.3f,position.y-1,4.6f,5.5f));
                ClearSegment(position,connection);
                NaturalTrail.Approach(portal.transform.parent,season,1.65f,position,connection);
                PortalThresholds.Add(position);
                foreach(var label in FindObjectsByType<SeasonWorldLabel>())
                {
                    if(Vector2.Distance(label.transform.position,previous+Vector2.up*3)<2&&
                       !string.IsNullOrEmpty(label.Caption)&&(label.Caption.Contains("ARRIVAL")||label.Caption.Contains(" / ")))
                        label.transform.position=position+Vector2.up*(portal.bounds.size.y+.5f);
                }
            }
        }

        void ClearLandmarkSightlines()
        {
            var landmarks=original.Where(r=>r&&r.gameObject.activeInHierarchy&&r.sprite&&
                (r.sprite.name.StartsWith("VillageDetails")||r.sprite.name.StartsWith("UtilityDetails")||r.sprite.name.StartsWith("SeasonCottages")||r.sprite.name.StartsWith("SeasonPortals"))).ToArray();
            foreach(var tree in original)if(tree&&tree.gameObject.activeInHierarchy&&Tree(tree)&&
                landmarks.Any(r=>tree.sortingOrder>r.sortingOrder&&tree.bounds.Intersects(r.bounds)))Remove(tree.gameObject);
            Physics2D.SyncTransforms();
        }

        void ComposeRidge(WinterLevel winter)
        {
            foreach(var r in original)if(r&&r.name.Contains("mountain ridge"))Remove(r.gameObject);
            // All stone lies over the existing blocked mountain shoulders. The five-unit doorway stays open.
            Ridge("Snowbound western ridge",1,new Vector2(14.8f,28),13);
            Ridge("Snowbound rear ridge",1,new Vector2(24.5f,28.8f),15);
            Ridge("Snowbound eastern ridge",1,new Vector2(34.5f,28.5f),13).flipX=true;
            Ridge("Western foothill",2,new Vector2(14,26.7f),10);
            Ridge("Eastern foothill",2,new Vector2(34,26.5f),10);
            var apron=Ridge("Cave snow transition",3,new Vector2(24,24.8f),13);apron.sortingOrder=-985;
            Ridge("Echo Cavern / natural entrance",0,new Vector2(24,26.4f),9);
            // Replace a ruler-straight row with small mineral veins beside the archive wall.
            var crystals=original.Where(r=>r&&r.name=="Archive crystal").ToArray();
            var points=new[]{new Vector2(97.8f,14.8f),new Vector2(99.2f,15.5f),new Vector2(100,14.3f),new Vector2(110,15.5f),new Vector2(111,14.4f)};
            for(int i=0;i<crystals.Length;i++)
            {
                var r=crystals[i];r.transform.position=points[i%points.Length];
                r.transform.localScale=Vector3.one*(i%2==0?.65f:.85f);r.sortingOrder=100-Mathf.RoundToInt(r.transform.position.y*10);
            }
        }

        SpriteRenderer Ridge(string name,int index,Vector2 position,float width)
        {
            var obj=new GameObject(name,typeof(SpriteRenderer));obj.transform.SetParent(outside,false);obj.transform.position=position;
            var renderer=obj.GetComponent<SpriteRenderer>();Skin(renderer,"WinterRidge",index,width);RidgePieces.Add(renderer);return renderer;
        }
    }
}
