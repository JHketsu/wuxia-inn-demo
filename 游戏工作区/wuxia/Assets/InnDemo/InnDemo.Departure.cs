using UnityEngine;

public partial class InnDemo
{
    bool departing;
    void BeginDeparture(){departing=true;CloseDialog();Say("女主 · 心想\n等她走到门边，再一起出去。");}
    void TickDeparture(){
        if(!story.CanLeave){departing=false;return;}
        Vector3 playerGoal=new(-.65f,0,-11.5f);
        Vector3 delta=playerGoal-player.transform.position;delta.y=0;
        player.Move((Vector3.ClampMagnitude(delta,1)*2.3f+Vector3.down*5)*Time.deltaTime);
        if(Vector3.Distance(companion.position,new Vector3(0,CompanionCenterY,-9))>4.5f){TickFollower();return;}
        // The final few metres are an escort action, not ordinary following.
        // Stop-distance hysteresis must not prevent crossing the threshold.
        Vector3 companionGoal=new(.65f,CompanionCenterY,-11.4f);
        Vector3 companionDelta=companionGoal-companion.position;companionDelta.y=0;
        if(companionDelta.sqrMagnitude>.01f)companion.rotation=Quaternion.LookRotation(companionDelta);
        follower.Move((Vector3.ClampMagnitude(companionDelta,1)*2f+Vector3.down*5)*Time.deltaTime);
        if(CrossedExit(player.transform.position)&&CrossedExit(companion.position-Vector3.up*CompanionCenterY)){
            story.won=true;won=true;departing=false;story.Record("离店：P"+story.pact+" / S"+story.swordOutcome);SetCursor(true);
        }
    }
    bool CrossedExit(Vector3 feet)=>feet.z< -10.6f&&Mathf.Abs(feet.x)<1.8f&&Mathf.Abs(feet.y)<1f;
}
