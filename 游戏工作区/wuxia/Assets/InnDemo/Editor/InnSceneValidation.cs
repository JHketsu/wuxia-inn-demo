using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class InnSceneValidation
{
    [MenuItem("Tools/Inn Demo/Validate Scene Scale And Camera")]
    public static void MenuRun(){Debug.Log(Run());}
    static Bounds Bounds(string name){
        var demo=UnityEngine.Object.FindFirstObjectByType<InnDemo>();
        if(demo&&demo.buildingAssembly){
            string path=name=="客栈完整屋顶"?"Architecture/Ceiling":name=="Upstairs room"?"Rooms/West_02/Floor":name=="Counter"?"Furniture/Counter":null;
            if(path!=null){var rs=demo.buildingAssembly.Find(path).GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
        }
        var g=GameObject.Find(name);if(g==null)throw new Exception("Missing scene object: "+name);return g.GetComponent<Renderer>().bounds;}
    public static string Run(){
        var demo=UnityEngine.Object.FindFirstObjectByType<InnDemo>();if(demo==null)throw new Exception("Enter Play mode first.");
        var failures=new List<string>();var roof=Bounds("客栈完整屋顶");var floor=Bounds("Upstairs room");
        if(roof.min.y-floor.max.y<4)failures.Add("Upper room clear height < 4m");
        if(Mathf.Abs(Bounds("Counter").max.y-1.05f)>.01f)failures.Add("Counter height");
        if(Mathf.Abs(Bounds("老板").size.y-1.75f)>.01f)failures.Add("Boss height");
        if(Mathf.Abs(Bounds("女主占位模型").size.y-1.6f)>.01f)failures.Add("Hero height");
        if(Mathf.Abs(GameObject.Find("同行者").transform.localScale.y*2-1.7f)>.01f)failures.Add("Companion height");
        if(Bounds("老板").max.y-Bounds("Counter").max.y<.69f)failures.Add("NPC hidden by counter");
        if(Mathf.Abs(Bounds("女主床").size.z-2.1f)>.01f)failures.Add("Bed length");
        var type=typeof(InnDemo);var flags=BindingFlags.NonPublic|BindingFlags.Instance;
        var player=(CharacterController)type.GetField("player",flags).GetValue(demo);var camera=(Camera)type.GetField("cam",flags).GetValue(demo);
        if(Mathf.Abs(player.bounds.size.y-1.6f)>.02f)failures.Add("Hero world collider height");
        var follower=GameObject.Find("同行者").GetComponent<CharacterController>();
        if(follower!=null&&follower.enabled&&Mathf.Abs(follower.bounds.size.y-1.7f)>.02f)failures.Add("Companion world collider height");
        var saved=player.transform.position;float oldYaw=(float)type.GetField("yaw",flags).GetValue(demo),oldPitch=(float)type.GetField("pitch",flags).GetValue(demo);int samples=0;
        try{
            foreach(var position in demo.buildingAssembly
                ?new[]{new Vector3(-4.5f,4.5f,12.3f),new Vector3(0,4.5f,13),new Vector3(-1.3f,4.5f,13)}
                :new[]{new Vector3(-5,4.5f,9.3f),new Vector3(1.25f,4.5f,10),new Vector3(-.1f,4.5f,9.4f)}){
                player.enabled=false;player.transform.position=position;player.enabled=true;Physics.SyncTransforms();
                foreach(float yaw in new[]{0f,45,90,135,180,225,270,315})foreach(float pitch in new[]{-15f,18,45,65}){
                    type.GetField("yaw",flags).SetValue(demo,yaw);type.GetField("pitch",flags).SetValue(demo,pitch);type.GetMethod("ResetCamera",flags).Invoke(demo,null);type.GetMethod("UpdateInteriorCamera",flags).Invoke(demo,null);
                    foreach(var collider in Physics.OverlapSphere(camera.transform.position,.18f))if((bool)type.GetMethod("CameraObstacle",flags).Invoke(demo,new object[]{collider.transform}))failures.Add("Camera overlap at "+position+" yaw="+yaw+" pitch="+pitch);
                    samples++;
                }
            }
        }finally{player.enabled=false;player.transform.position=saved;player.enabled=true;type.GetField("yaw",flags).SetValue(demo,oldYaw);type.GetField("pitch",flags).SetValue(demo,oldPitch);type.GetMethod("ResetCamera",flags).Invoke(demo,null);type.GetMethod("UpdateInteriorCamera",flags).Invoke(demo,null);}
        if(failures.Count>0)throw new Exception(string.Join("\n",failures));
        return "PASS: metre scale; clear ceiling="+(roof.min.y-floor.max.y).ToString("F2")+"m; counter/NPC/bed; camera collision samples="+samples;
    }
}
