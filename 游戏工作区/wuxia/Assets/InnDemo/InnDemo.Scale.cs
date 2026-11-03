using UnityEngine;
using UnityEngine.Rendering;

public partial class InnDemo
{
    // Unity units are metres. Keep collision, interaction and camera clearances consistent.
    const float UpperFloorY=4.2f, UpperWalkY=4.35f, RoofBottomY=8.5f;
    const float UpperWallHeight=RoofBottomY-UpperWalkY, UpperWallCenter=(RoofBottomY+UpperWalkY)/2;
    const float HeroHeight=1.6f, CompanionHeight=1.7f, BossHeight=1.75f;
    const float HeroEyeY=HeroHeight-.12f, CameraFocusY=1.3f;
    const float CompanionCenterY=CompanionHeight/2, CounterHeight=1.05f;
    const int CurrentLayoutVersion=4;
    void Bed(string name,Vector3 feet,Color cover){
        Box(name,feet+Vector3.up*.275f,new(1.1f,.55f,2.1f),new Color(.28f,.16f,.08f));
        Box(name+"床褥",feet+new Vector3(0,.6f,.15f),new(1.06f,.1f,1.65f),cover);
        Box(name+"枕头",feet+new Vector3(0,.7f,.77f),new(.7f,.12f,.35f),new Color(.62f,.58f,.49f));
    }
    void Table(string name,Vector3 feet,float width=1.4f,float depth=1.1f){
        var wood=new Color(.28f,.16f,.08f);
        Box(name,feet+Vector3.up*.71f,new(width,.1f,depth),wood);
        foreach(float x in new[]{-.4f,.4f})foreach(float z in new[]{-.4f,.4f})Box(name+"桌腿",feet+new Vector3(x*width,.33f,z*depth),new(.08f,.66f,.08f),wood);
    }
    void FinishInterior(){
        Box("客栈完整屋顶",new(0,RoofBottomY+.15f,5),new(20.3f,.3f,30.3f),new Color(.22f,.17f,.12f));
        Table("床边小几",new(-6.1f,UpperWalkY,9.6f),.55f,.5f);
        Box("行李包袱",new(-5.7f,UpperWalkY+.18f,10.6f),new(.5f,.36f,.35f),new Color(.25f,.29f,.24f));
        Box("床边药渍杯",new(-6.1f,UpperWalkY+.82f,9.6f),new(.09f,.12f,.09f),new Color(.63f,.58f,.45f));
        Box("床边药笺",new(-7.45f,UpperWalkY+.665f,9.2f),new(.22f,.01f,.3f),new Color(.76f,.69f,.52f),false);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.38f,.34f,.29f);
        Lamp("大堂灯一",new(-4,3.1f,-2),12,3.5f);Lamp("大堂灯二",new(5,3.1f,5),12,4);
        Lamp("双床房灯",new(-5,7.5f,9.5f),9,3);Lamp("单床房灯",new(-5,7.5f,3.5f),9,3);
        Lamp("走廊灯一",new(1.25f,7.5f,7),10,2.5f);Lamp("走廊灯二",new(1.25f,7.5f,16),9,2.5f);
        foreach(float x in new[]{2.7f,5.3f}){
            Vector3 a=new(x,1,-4.6f),b=new(x,5.3f,5.1f);var rail=Box("楼梯扶手",(a+b)/2,new(.08f,.08f,Vector3.Distance(a,b)),new Color(.28f,.16f,.08f));rail.transform.rotation=Quaternion.LookRotation(b-a);
        }
    }
    void Lamp(string name,Vector3 position,float range,float intensity){
        var light=new GameObject(name).AddComponent<Light>();light.type=LightType.Point;light.transform.position=position;light.color=new Color(1,.79f,.55f);light.range=range;light.intensity=intensity;light.shadows=LightShadows.None;
        Box(name+"灯罩",position+Vector3.up*.25f,new(.3f,.25f,.3f),new Color(.75f,.58f,.32f),false);
        float top=position.x<0&&position.z>0&&position.y<4?UpperFloorY-.15f:RoofBottomY;
        float length=top-position.y-.375f;
        if(length>0)Box(name+"吊杆",new(position.x,position.y+.375f+length/2,position.z),new(.025f,length,.025f),new Color(.25f,.2f,.13f),false);
    }
}
