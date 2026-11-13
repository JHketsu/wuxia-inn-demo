using UnityEngine;
using System.Collections.Generic;

public partial class InnDemo
{
    [Tooltip("Integrated scene's editable architecture instance; leave empty for the original graybox.")]
    public Transform buildingAssembly;
    readonly List<GameObject> generatedBoxes = new();
    Vector3 HeroSpawn => buildingAssembly ? new(-4.5f,4.5f,12.3f) : new(-5,4.5f,9.3f);
    Vector3 SwordRest => new(4.8f,CounterHeight+.04f,buildingAssembly?7:6);
    Vector3 CompanionRest(bool standing) => new(buildingAssembly?-3.4f:-5,standing?CompanionCenterY:.29f,1);
    int LayoutVersion => buildingAssembly ? 5 : CurrentLayoutVersion;

    void IntegrateBuilding()
    {
        // The imported assembly supplies all walls, floors, stairs and furniture.
        // Keep story props independent so replacing an art asset never removes a clue.
        string[] own={"女主床","同行者床","床边小几","行李包袱","床边药渍杯","床边药笺"};
        string[] guest={"客人单床","客房食桌","食桌凳","残食碗","撕开的药包","开着的箱子","箱盖","留在箱中的布料"};
        string[] counter={"柜台药箱","柜台血滴","柜台侧边书桌","账本","老板的剑"};
        foreach(var g in generatedBoxes)
        {
            string n=g.name;
            if(n=="Ground"){g.GetComponent<Renderer>().enabled=false;continue;}
            if(n=="Exit door"||n=="血迹"||n=="醒神药")continue;
            if(Starts(n,own)){
                g.transform.position+=Vector3.forward*3;
                if(n.StartsWith("女主床"))g.transform.position+=new Vector3(-1,0,.7f);
                else if(n.StartsWith("同行者床"))g.transform.position+=new Vector3(-2.8f,0,.7f);
                else if(n.StartsWith("床边小几")||n=="床边药渍杯")g.transform.position+=new Vector3(-.9f,0,-.6f);
                else if(n=="行李包袱")g.transform.position=new(-7,4.53f,11.4f);
                else if(n=="床边药笺")g.transform.position=new(-7.15f,5.115f,11.9f);
                continue;
            }
            if(Starts(n,guest)){g.transform.position+=Vector3.forward*5;continue;}
            if(Starts(n,counter)){g.transform.position+=Vector3.forward;continue;}
            // The imported dining table at (-6,-1) carries the loose-board clue.
            if(n.StartsWith("藏令桌"))continue;
            g.SetActive(false);
        }
        // Avoid a second table at the evidence site; use the modeled dining set.
        foreach(var g in generatedBoxes)if(g.name.StartsWith("藏令桌"))g.SetActive(false);
        foreach(var mesh in buildingAssembly.GetComponentsInChildren<MeshFilter>())
        {
            if(!mesh.sharedMesh||mesh.GetComponent<Collider>())continue;
            if(mesh.name=="Wear"||mesh.name=="Banner")continue;
            mesh.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;
        }
        player.enabled=false;player.transform.position=HeroSpawn;player.enabled=true;
        boss.position+=Vector3.forward;
        // Keep her beside the evidence table, clear of its benches and stools.
        companion.position=new(-3.4f,.29f,1);
        foreach(var marker in markers)
        {
            var t=marker.anchor;string label=marker.label;
            if(label=="hotspot_luggage_and_cup")t.position=new(-7,5.25f,11.6f);
            else if(label=="hotspot_medicine_note")t.position=new(-7.15f,5.25f,11.9f);
            else if(label=="hotspot_empty_bed")t.position=new(-6,5.25f,12.8f);
            else if(label=="hotspot_guest_meal"||label=="hotspot_guest_luggage")t.position+=Vector3.forward*5;
            else if(label=="hotspot_own_room_sign")t.position=new(-1.45f,5.65f,13);
            else if(label=="hotspot_guest_room_sign")t.position=new(-1.45f,5.65f,8.33f);
            else if(label=="hotspot_innkeeper"||label=="hotspot_medicine_chest"||label=="hotspot_ledger"||label=="hotspot_counter_blood")t.position+=Vector3.forward;
            else if(label=="hotspot_companion")t.position=new(-3.4f,1.1f,1);
            else if(label=="hotspot_table_blood")t.position=new(-4.5f,1,-.4f);
            else if(label=="hotspot_hidden_order")t.position=new(-4.9f,.65f,-1);
        }
        Box("桌沿血痕",new(-5.35f,.768f,-.65f),new(.45f,.012f,.23f),new Color(.35f,.015f,.01f),false);
        Box("证据松木条",new(-5.32f,.48f,-1.55f),new(.65f,.08f,.08f),new Color(.42f,.28f,.13f),false);
        Box("木条后纸角",new(-5.35f,.47f,-1.53f),new(.24f,.015f,.16f),new Color(.8f,.73f,.55f),false);
        foreach(var z in new[]{-.3f,.1f,.5f})Box("桌边滴落血点",new(-4.25f,.018f,z),new(.16f,.012f,.12f),new Color(.35f,.015f,.01f),false);
        Lamp("整合双床房灯",new(-5,7.5f,13),9,3);
        Lamp("整合客房灯",new(-5,7.5f,8.3f),9,3);
        Lamp("整合大堂灯",new(0,3.6f,-3),14,3.5f);
        Lamp("整合柜台灯",new(5,3.3f,7.5f),10,3.5f);
        Lamp("整合走廊灯",new(0,7.5f,13),12,3);
        var sun=GameObject.Find("Warm daylight");
        sun.transform.rotation=Quaternion.LookRotation(new Vector3(-.883f,-.342f,.321f));
        sun.GetComponent<Light>().color=new Color(1,.91f,.78f);
        RenderSettings.ambientLight=new Color(.46f,.43f,.38f);
        ResetCamera();Physics.SyncTransforms();
    }
    static bool Starts(string name,string[] prefixes){foreach(var p in prefixes)if(name.StartsWith(p))return true;return false;}
}
