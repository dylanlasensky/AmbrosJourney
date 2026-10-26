using UnityEngine;
namespace AmbrosJourney.Presentation
{
    public static class FieldCardSkin
    {
        public static Sprite Create()
        {
            var texture=Resources.Load<Texture2D>("Journey/FieldCard");if(!texture)return null;
            var rect=new Rect(texture.width*.018f,texture.height*.035f,texture.width*.964f,texture.height*.93f);
            float edge=texture.height*.085f;
            return Sprite.Create(texture,rect,Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));
        }
    }
}
