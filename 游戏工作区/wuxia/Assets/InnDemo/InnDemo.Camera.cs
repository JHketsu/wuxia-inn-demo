using UnityEngine;

public partial class InnDemo
{
    const float CameraDistance=3.4f, CameraRadius=.22f;
    float cameraDistance=CameraDistance, cameraDistanceVelocity;
    bool cameraInitialized;
    Renderer heroRenderer;
    bool heroHidden;
    void ResetCamera(){cameraInitialized=false;cameraDistance=CameraDistance;cameraDistanceVelocity=0;}
    bool CameraObstacle(Transform target){
        if(target==null)return false;
        return !target.IsChildOf(player.transform)&&target!=companion&&!target.IsChildOf(companion)&&target!=boss&&!target.IsChildOf(boss);
    }
    float SafeCameraDistance(Vector3 origin,Vector3 direction,float maxDistance){
        float available=maxDistance;
        foreach(var hit in Physics.SphereCastAll(origin,CameraRadius,direction,maxDistance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
            if(CameraObstacle(hit.transform))available=Mathf.Min(available,Mathf.Max(.12f,hit.distance-.08f));
        return available;
    }
    void UpdateInteriorCamera(){
        if(cam==null||player==null)return;
        var focus=player.transform.position+Vector3.up*CameraFocusY;
        var offset=Quaternion.Euler(pitch,yaw,0)*new Vector3(.65f,0,-CameraDistance);
        float desired=SafeCameraDistance(focus,offset.normalized,offset.magnitude);
        if(!cameraInitialized){cameraDistance=desired;cameraInitialized=true;}
        else if(desired<cameraDistance){cameraDistance=desired;cameraDistanceVelocity=0;}
        else cameraDistance=Mathf.SmoothDamp(cameraDistance,desired,ref cameraDistanceVelocity,.28f,10,Time.deltaTime);
        cameraDistance=Mathf.Min(cameraDistance,desired);
        cam.transform.position=focus+offset.normalized*cameraDistance;
        cam.transform.rotation=Quaternion.Euler(pitch,yaw,0);
        if(heroRenderer==null)heroRenderer=body.GetComponent<Renderer>();
        // At extremely close distances hide only the heroine's placeholder mesh,
        // not the walls or ceiling; collision and movement remain active.
        if(!heroHidden&&cameraDistance<1.05f)heroHidden=true;else if(heroHidden&&cameraDistance>1.25f)heroHidden=false;
        heroRenderer.enabled=!heroHidden;
    }
}
