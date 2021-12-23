using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.Xml;

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public float rot;
    public string type = "slot";
    public Vector2 stagger;
    public int player;
    public Vector3 pos;
}

public class BartokLayout : MonoBehaviour
{
    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;
    public List<SlotDef> slotDefs;
    public SlotDef discardPile;
    public SlotDef target;
    public SlotDef drawPile;

    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), CultureInfo.InvariantCulture);
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), CultureInfo.InvariantCulture);
        SlotDef tSD;
        if (Bartok.players_ == 2)
        {
            PT_XMLHashList slotsX = xml["_slot"];
            for (int i = 0; i < slotsX.Count; i++)
            {
                tSD = new SlotDef();
                if (slotsX[i].HasAtt("type"))
                    tSD.type = slotsX[i].att("type");
                else
                    tSD.type = "slot";
                tSD.x = float.Parse(slotsX[i].att("x"), CultureInfo.InvariantCulture);
                tSD.y = float.Parse(slotsX[i].att("y"), CultureInfo.InvariantCulture);
                tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);
                tSD.layerID = int.Parse(slotsX[i].att("layer"), CultureInfo.InvariantCulture);
                tSD.layerName = tSD.layerID.ToString();
                switch (tSD.type)
                {
                    case "slot":
                        break;
                    case "drawpile":
                        tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), CultureInfo.InvariantCulture);
                        drawPile = tSD;
                        break;
                    case "discardpile":
                        discardPile = tSD;
                        break;
                    case "target":
                        target = tSD;
                        break;
                    case "hand":
                        tSD.player = int.Parse(slotsX[i].att("player"), CultureInfo.InvariantCulture);
                        tSD.rot = float.Parse(slotsX[i].att("rot"), CultureInfo.InvariantCulture);
                        slotDefs.Add(tSD);
                        break;
                }
            }
        }
        if (Bartok.players_ == 3)
        {
            PT_XMLHashList slotsX = xml["slot_"];
            for (int i = 0; i < slotsX.Count; i++)
            {
                tSD = new SlotDef();
                if (slotsX[i].HasAtt("type"))
                    tSD.type = slotsX[i].att("type");
                else
                    tSD.type = "slot";
                tSD.x = float.Parse(slotsX[i].att("x"), CultureInfo.InvariantCulture);
                tSD.y = float.Parse(slotsX[i].att("y"), CultureInfo.InvariantCulture);
                tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);
                tSD.layerID = int.Parse(slotsX[i].att("layer"), CultureInfo.InvariantCulture);
                tSD.layerName = tSD.layerID.ToString();
                switch (tSD.type)
                {
                    case "slot":
                        break;
                    case "drawpile":
                        tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), CultureInfo.InvariantCulture);
                        drawPile = tSD;
                        break;
                    case "discardpile":
                        discardPile = tSD;
                        break;
                    case "target":
                        target = tSD;
                        break;
                    case "hand":
                        tSD.player = int.Parse(slotsX[i].att("player"), CultureInfo.InvariantCulture);
                        tSD.rot = float.Parse(slotsX[i].att("rot"), CultureInfo.InvariantCulture);
                        slotDefs.Add(tSD);
                        break;
                }
            }
        }
    }
}
