using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public partial class InnDemo : MonoBehaviour
{
    CharacterController player; Camera cam; Transform body, companion, sword, boss; GameObject door;
    CharacterController follower;
    bool blood, medicine, awake, armed, opened, won; float yaw=90, pitch=18, vertical, messageUntil; string message="你在陌生客栈二楼醒来。下楼寻找同行者。";
    class Clue { public Transform target; public string name; public System.Action action; }
    readonly System.Collections.Generic.List<Clue> clues=new();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){ if(FindFirstObjectByType<InnDemo>()==null) new GameObject("Inn Demo").AddComponent<InnDemo>(); }
    Material Mat(Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard"));m.color=c;return m;}
    GameObject Box(string n,Vector3 p,Vector3 s,Color c, bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=Mat(c);if(!solid)Destroy(g.GetComponent<Collider>());return g;}
    Transform Person(string n, Vector3 p, Color c,float height,float width){var g=GameObject.CreatePrimitive(PrimitiveType.Capsule);g.name=n;g.transform.position=p;g.transform.localScale=new Vector3(width,height/2,width);g.GetComponent<Renderer>().sharedMaterial=Mat(c);return g.transform;}
    void Add(Transform t,string n,System.Action a){clues.Add(new Clue{target=t,name=n,action=a});}
    void Say(string s){message=s;messageUntil=Time.time+7;}
    void Start(){
        foreach(var c in FindObjectsByType<Camera>(FindObjectsSortMode.None)) c.gameObject.SetActive(false);
        foreach(var existingLight in FindObjectsByType<Light>(FindObjectsSortMode.None))existingLight.gameObject.SetActive(false);
        var wood=new Color(.25f,.13f,.07f);var wall=new Color(.55f,.43f,.29f);
        Box("Ground",new(0,-.2f,5),new(20,.4f,34),wood);
        Box("Back wall",new(0,RoofBottomY/2,20),new(20,RoofBottomY,.3f),wall);Box("West wall",new(-10,RoofBottomY/2,5),new(.3f,RoofBottomY,30),wall);Box("East wall",new(10,RoofBottomY/2,5),new(.3f,RoofBottomY,30),wall);
        Box("Front left",new(-5.7f,RoofBottomY/2,-10),new(8.6f,RoofBottomY,.3f),wall);Box("Front right",new(5.7f,RoofBottomY/2,-10),new(8.6f,RoofBottomY,.3f),wall);Box("Lintel",new(0,(2.6f+RoofBottomY)/2,-10),new(2.8f,RoofBottomY-2.6f,.3f),wall);
        door=Box("Exit door",new(0,1.3f,-10),new(2.8f,2.6f,.18f),wood);
        Box("Upstairs room",new(-5,UpperFloorY,10),new(9,.3f,19),wood);
        Bed("女主床",new(-7,UpperWalkY,10),new Color(.35f,.28f,.3f));
        // Broad shallow steps join the open upper landing to the hall.
        for(int i=0;i<24;i++)Box("Stair "+i,new(4,(i+.5f)*.175f,-4.4f+i*.42f),new(2.4f,.175f,.43f),wood);
        Box("Upper railing",new(-5,UpperWalkY+.6f,.5f),new(9,1.2f,.15f),wood);
        Box("Landing",new(2.5f,UpperFloorY,5.8f),new(6.4f,.3f,1.4f),wood);
        Box("一楼后侧隔墙",new(0,UpperFloorY/2,10),new(20,UpperFloorY,.25f),wall);
        Box("Counter",new(5,CounterHeight/2,6),new(6,CounterHeight,.95f),wood);
        boss=Person("老板",new(5,BossHeight/2,6.95f),new Color(.32f,.31f,.28f),BossHeight,.62f);
        sword=Box("老板的剑",new(4.8f,CounterHeight+.04f,6),new(.06f,.04f,.95f),Color.gray,false).transform;
        for(int i=0;i<3;i++){Table("大堂桌"+i,new(i==2?7:-6+i*5,0,-4));}
        companion=Person("同行者",new(-5,.29f,1),new Color(.2f,.38f,.45f),CompanionHeight,.56f);companion.rotation=Quaternion.Euler(0,0,90);
        var stain=Box("血迹",new(-3,.025f,0),new(2,.025f,1.2f),new Color(.35f,.015f,.01f),false);
        var remedy=Box("醒神药",new(7,1.85f,6),new(.3f,.35f,.3f),new Color(.3f,.7f,.6f));
        remedy.SetActive(false);
        var p=new GameObject("女主");p.transform.position=new(-5,UpperWalkY+.15f,9.3f);player=p.AddComponent<CharacterController>();player.height=HeroHeight;player.center=new(0,HeroHeight/2,0);player.radius=.27f;player.stepOffset=.35f;player.skinWidth=.03f;
        body=Person("女主占位模型",p.transform.position+Vector3.up*(HeroHeight/2),new Color(.62f,.16f,.22f),HeroHeight,.54f);Destroy(body.GetComponent<Collider>());body.SetParent(p.transform);
        cam=new GameObject("Follow camera").AddComponent<Camera>();cam.nearClipPlane=.08f;cam.gameObject.AddComponent<AudioListener>();
        RenderSettings.ambientLight=new Color(.65f,.58f,.48f);var light=new GameObject("Warm daylight").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(45,-30,0);
        BuildStory(stain.transform);
        FinishInterior();
        Cursor.lockState=CursorLockMode.Locked;messageUntil=12;
    }
    void Open(bool broken){ story.opened=true;story.broken=broken;opened=true;door.SetActive(false);Say(broken?"你撞断了旧门闩。老板看了一眼门：走吧，别在路上耽搁。":(story.pact>0?"门闩抬起，门外天色将明。照说好的，等她一起走。":"门闩抬起，门外天色将明。离开前，还有话要和她说。")); }
    Clue Nearest(){Clue best=null;float d=2.5f;foreach(var c in clues){if(!c.target.gameObject.activeInHierarchy)continue;var delta=c.target.position-(player.transform.position+Vector3.up);if(Mathf.Abs(delta.y)>1.9f)continue;float n=delta.magnitude;if(n<d && !Physics.Linecast(player.transform.position+Vector3.up*HeroEyeY,c.target.position,out var hit)){d=n;best=c;}}return best;}
    void Update(){if(player==null)return;var k=Keyboard.current;var m=Mouse.current;if(k==null)return;
        if(k.f5Key.wasPressedThisFrame)SaveStory();if(k.f9Key.wasPressedThisFrame){LoadStory();return;}
        if(k.rKey.wasPressedThisFrame && (won || k.leftCtrlKey.isPressed)){SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);return;}
        if(k.tabKey.wasPressedThisFrame)showObjectives=!showObjectives;
        if(k.jKey.wasPressedThisFrame){journal=!journal;dialogTitle=null;SetCursor(journal);return;}
        if(k.escapeKey.wasPressedThisFrame){if(departing){departing=false;Say("你停在门边，回头等她。");return;}if(dialogTitle!=null||journal){CloseDialog();journal=false;}else SetCursor(Cursor.lockState==CursorLockMode.Locked);return;}
        if(dialogTitle!=null && k.eKey.wasPressedThisFrame){CloseDialog();return;}
        if(won||dialogTitle!=null||journal)return;
        if(departing){TickDeparture();return;}
        if(k.qKey.wasPressedThisFrame)scanUntil=Time.time+4f;
        if(m!=null&&Cursor.lockState==CursorLockMode.Locked){yaw+=m.delta.ReadValue().x*.15f;pitch=Mathf.Clamp(pitch-m.delta.ReadValue().y*.12f,-15,65);}
        float cameraHorizontal=(k.rightArrowKey.isPressed?1f:0f)-(k.leftArrowKey.isPressed?1f:0f);
        float cameraVertical=(k.downArrowKey.isPressed?1f:0f)-(k.upArrowKey.isPressed?1f:0f);
        yaw+=cameraHorizontal*100f*Time.deltaTime;
        pitch=Mathf.Clamp(pitch+cameraVertical*70f*Time.deltaTime,-15f,65f);
        Vector2 axis=new((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));Vector3 move=Quaternion.Euler(0,yaw,0)*new Vector3(axis.x,0,axis.y).normalized;
        if(player.isGrounded&&vertical<0)vertical=-2;vertical+=Physics.gravity.y*Time.deltaTime;player.Move((move*(k.leftShiftKey.isPressed?5:3)+Vector3.up*vertical)*Time.deltaTime);if(move.sqrMagnitude>.01f)body.rotation=Quaternion.LookRotation(move);
        if(player.transform.position.y<-5){player.enabled=false;player.transform.position=new(-5,UpperWalkY+.15f,9.3f);player.enabled=true;}
        var clue=Nearest();if(clue!=null&&k.eKey.wasPressedThisFrame)clue.action();if(k.fKey.wasPressedThisFrame&&Vector3.Distance(player.transform.position,door.transform.position)<3){if(!opened){var before=Snapshot();Open(true);Audit(before,"撞开门闩");}}
        TickFollower();
        if(player.transform.position.z<-10.6f && Mathf.Abs(player.transform.position.x)<2){
            if(story.CanLeave)BeginDeparture();
            else {player.enabled=false;player.transform.position=new Vector3(0,0,-9.1f);player.enabled=true;ExitTalk();}
        }
        sword.Rotate(0,armed?0:Mathf.Sin(Time.time*4)*Time.deltaTime*12,0);
    }
    void LateUpdate(){UpdateInteriorCamera();}
    void OnGUI(){DrawStoryUI();}
}
