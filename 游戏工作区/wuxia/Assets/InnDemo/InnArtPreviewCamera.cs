using UnityEngine;
using UnityEngine.InputSystem;

// Isolated art viewer; does not participate in the InnDemo game controller.
public class InnArtPreviewCamera : MonoBehaviour
{
    public Transform assembly;
    float yaw, pitch;
    public void SetView(int view)
    {
        var pos = view == 2 ? new Vector3(0, 2.2f, -8.8f) : view == 3 ? new Vector3(0, 5.95f, 4.2f) : view == 4 ? new Vector3(23, 29, -20) : new Vector3(31, 22, -39);
        var target = view == 2 ? new Vector3(0, 4.3f, 6) : view == 3 ? new Vector3(0, 5.95f, 17) : new Vector3(0, 4.8f, 5);
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(target - pos);
        yaw = transform.eulerAngles.y; pitch = transform.eulerAngles.x;
        if (pitch > 180) pitch -= 360;
        if (assembly)
        {
            var roof = assembly.Find("Architecture/Roof");
            var ceiling = assembly.Find("Architecture/Ceiling");
            if (roof) roof.gameObject.SetActive(view != 4);
            if (ceiling) ceiling.gameObject.SetActive(view != 4);
        }
    }
    void Start()
    {
        // The editor may keep the old game scene open additively for comparison.
        foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (camera.gameObject.scene != gameObject.scene) camera.enabled = false;
        foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.gameObject.scene != gameObject.scene) light.enabled = false;
        SetView(1);
    }
    void Update()
    {
        var k = Keyboard.current; var m = Mouse.current;
        if (k == null) return;
        if (k.digit1Key.wasPressedThisFrame) SetView(1);
        if (k.digit2Key.wasPressedThisFrame) SetView(2);
        if (k.digit3Key.wasPressedThisFrame) SetView(3);
        if (k.digit4Key.wasPressedThisFrame) SetView(4);
        if (m == null || !m.rightButton.isPressed) return;
        yaw += m.delta.ReadValue().x * .12f; pitch = Mathf.Clamp(pitch - m.delta.ReadValue().y * .12f, -85, 85);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        var v = new Vector3((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.eKey.isPressed ? 1 : 0) - (k.qKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
        transform.position += transform.rotation * Vector3.ClampMagnitude(v, 1) * (k.leftShiftKey.isPressed ? 12 : 4) * Time.unscaledDeltaTime;
    }
    void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 640, 54), "客栈 v2 · 美术预览\n1 外观 / 2 大堂 / 3 二楼 / 4 剖切；按住右键 + WASD，Q/E 升降，Shift 加速");
    }
}
