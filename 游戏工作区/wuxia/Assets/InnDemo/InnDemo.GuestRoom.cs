using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public partial class InnDemo
{
    void BuildUpstairsRooms(){
        Color wall=new(.48f,.38f,.27f),wood=new(.28f,.16f,.08f);
        Box("二楼宽走廊",new(1.25f,UpperFloorY,10),new(3.5f,.3f,19),wood);
        Box("东侧背景客房地板",new(6.25f,UpperFloorY,14),new(6.5f,.3f,12),wood);
        Box("西侧客房分隔墙一",new(-5,UpperWallCenter,6.5f),new(9,UpperWallHeight,.18f),wall);
        Box("西侧客房分隔墙二",new(-5,UpperWallCenter,12.5f),new(9,UpperWallHeight,.18f),wall);
        RoomFront("你们的双床房",-.5f,6.5f,12.5f,9.4f,false,new Color(.5f,.35f,.2f));
        RoomFront("昨夜客人的单床房",-.5f,.5f,6.5f,4,false,new Color(.3f,.4f,.45f));
        RoomFront("西侧其他住客房",-.5f,12.5f,19.5f,16,true,wood);
        RoomFront("东侧其他住客房甲",3,8,13.5f,10.8f,true,wood);
        RoomFront("东侧其他住客房乙",3,13.5f,19.5f,16.5f,true,wood);
        Box("东侧背景房前墙",new(6.25f,UpperWallCenter,8),new(6.5f,UpperWallHeight,.18f),wall);
        Box("东侧背景房分隔墙",new(6.25f,UpperWallCenter,13.5f),new(6.5f,UpperWallHeight,.18f),wall);
        Box("二楼走廊北端",new(1.25f,UpperWallCenter,19.5f),new(3.5f,UpperWallHeight,.18f),wall);
        Box("走廊南端护栏",new(1.25f,UpperWalkY+.6f,.5f),new(3.5f,1.2f,.15f),wood);
        Box("走廊临空护栏",new(3,UpperWalkY+.6f,2.7f),new(.15f,1.2f,4.1f),wood);
        Bed("同行者床",new(-3.2f,UpperWalkY,10),new Color(.25f,.4f,.43f));
        Hotspot("hotspot_empty_bed",new(-3.2f,UpperWalkY+.9f,9.1f),()=>{
            if(story.Has("wound"))Observe(Copy("exploration_buildupstairsrooms_01"),Copy("exploration_buildupstairsrooms_02"),Copy("exploration_buildupstairsrooms_03"),"emptyBed");
            else Observe(Copy("exploration_buildupstairsrooms_04"),Copy("exploration_buildupstairsrooms_05"),Copy("exploration_buildupstairsrooms_06"),"emptyBed");
        });
        Hotspot("hotspot_own_room_sign",new(-.15f,UpperWalkY+1.3f,9.4f),()=>Observe(Copy("exploration_buildupstairsrooms_07"),Copy("exploration_buildupstairsrooms_08"),Copy("exploration_buildupstairsrooms_09")));
        Hotspot("hotspot_guest_room_sign",new(-.15f,UpperWalkY+1.3f,4),()=>Observe(Copy("exploration_buildupstairsrooms_10"),Copy("exploration_buildupstairsrooms_11"),Copy("exploration_buildupstairsrooms_12")));
    }
    void RoomFront(string name,float x,float minZ,float maxZ,float doorZ,bool closed,Color doorColor){
        Color wall=new(.48f,.38f,.27f);float low=doorZ-.8f,high=doorZ+.8f;
        Box(name+"门侧墙一",new(x,UpperWallCenter,(minZ+low)/2),new(.18f,UpperWallHeight,low-minZ),wall);
        Box(name+"门侧墙二",new(x,UpperWallCenter,(high+maxZ)/2),new(.18f,UpperWallHeight,maxZ-high),wall);
        Box(name+"门楣",new(x,(UpperWalkY+2.5f+RoofBottomY)/2,doorZ),new(.24f,RoofBottomY-UpperWalkY-2.5f,1.6f),wall);
        Box(name+"房牌",new(x+(x<0?.14f:-.14f),UpperWalkY+1.9f,doorZ+1.05f),new(.1f,.35f,.5f),doorColor,false);
        if(closed){Box(name+"关着的门",new(x,UpperWalkY+1.25f,doorZ),new(.22f,2.5f,1.6f),doorColor);Box(name+"门把",new(x+(x<0?.2f:-.2f),UpperWalkY+1.05f,doorZ-.5f),new(.12f,.12f,.25f),Color.gray,false);}
    }
    void BuildGuestRoom(){
        Color wood=new(.3f,.19f,.1f), cloth=new(.35f,.38f,.39f);
        Bed("客人单床",new(-8.4f,UpperWalkY,3),cloth);
        Table("客房食桌",new(-5.6f,UpperWalkY,2.7f),1.2f,.8f);
        Box("食桌凳",new(-4.5f,UpperWalkY+.225f,2.7f),new(.4f,.45f,.4f),wood);
        Box("残食碗",new(-5.8f,UpperWalkY+.8f,2.7f),new(.18f,.08f,.18f),new Color(.65f,.58f,.4f));
        Box("撕开的药包",new(-5.3f,UpperWalkY+.78f,2.7f),new(.14f,.03f,.18f),new Color(.8f,.73f,.52f));
        Box("开着的箱子",new(-3.3f,UpperWalkY+.25f,2.6f),new(.8f,.5f,.5f),wood);
        Box("箱盖",new(-3.3f,UpperWalkY+.55f,2.3f),new(.8f,.6f,.08f),wood);
        Box("留在箱中的布料",new(-3.3f,UpperWalkY+.51f,2.6f),new(.6f,.08f,.35f),cloth);
        Hotspot("hotspot_guest_luggage",new(-3,UpperWalkY+.85f,3.6f),()=>Observe(Copy("exploration_buildguestroom_01"),Copy("exploration_buildguestroom_02"),Copy("exploration_buildguestroom_03"),"guestRoomSearched"));
    }
    Dictionary<string,string> Snapshot(){var values=new Dictionary<string,string>();foreach(var field in typeof(InnStoryState).GetFields(BindingFlags.Public|BindingFlags.Instance))if(field.FieldType==typeof(bool)||field.FieldType==typeof(int))values[field.Name]=field.GetValue(story).ToString();return values;}
    void Audit(Dictionary<string,string> before,string source){foreach(var entry in Snapshot())if(before.TryGetValue(entry.Key,out var old)&&old!=entry.Value)story.Record(source+" | "+entry.Key+": "+old+" → "+entry.Value);}
}
