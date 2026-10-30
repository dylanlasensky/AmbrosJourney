using System;
using TMPro;
using UnityEngine;

namespace AmbrosJourney.Journey
{
    public static class JourneyCertificateView
    {
        public static RectTransform Draw(JourneyUI ui,JourneyData data,float scale)
        {
            var page=new GameObject("Personalized certificate",typeof(RectTransform));page.transform.SetParent(ui.Root,false);
            var root=page.GetComponent<RectTransform>();root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(1000,700);root.localScale=Vector3.one*scale;
            var background=new GameObject("Approved certificate artwork",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));background.transform.SetParent(root,false);
            JourneyUI.Place(background.GetComponent<RectTransform>(),new Rect(-500,-350,1000,700));
            var image=background.GetComponent<UnityEngine.UI.RawImage>();image.texture=Resources.Load<Texture2D>("Journey/CertificateBackground");image.raycastTarget=false;
            if(!image.texture)throw new InvalidOperationException("Missing certificate artwork.");
            var name=ui.Text(root,"Certificate player name",data.playerName,new Rect(-330,-103,660,57),36,new Color(.153f,.243f,.212f));
            name.alignment=TextAlignmentOptions.Center;name.fontStyle=FontStyles.Bold;name.enableAutoSizing=true;name.fontSizeMin=14;name.fontSizeMax=36;name.textWrappingMode=TextWrappingModes.NoWrap;
            var date=ui.Text(root,"Certificate completion date",data.completedOn,new Rect(-332,204,218,25),12,new Color(.153f,.243f,.212f));
            date.alignment=TextAlignmentOptions.Center;date.enableAutoSizing=true;date.fontSizeMin=9;date.fontSizeMax=12;date.textWrappingMode=TextWrappingModes.NoWrap;
            return root;
        }
    }
}
