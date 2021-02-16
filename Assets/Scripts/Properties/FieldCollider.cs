using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(EventTrigger))]
public class FieldCollider : MonoBehaviour
{
    private UnityAction<int, int, BaseEventData> callback = null;

    public void SetPhysicsRaycastHitCallback(UnityAction<int, int, BaseEventData> physicsRaycastHitCallback)
    {
        this.callback = physicsRaycastHitCallback;
    }

    public void Init(float width, float height, int floor)
    {
        this.transform.localScale = new Vector3(width, height, 1.0f);
        this.transform.position = new Vector3(width * 0.5f, floor, -height * 0.5f);
        this.transform.rotation = MapLoader.SurfaceUp;

        EventTrigger trigger = this.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(this.OnPhysicsRaycasterHit);

        trigger.triggers.Add(entry);
    }

    private void OnPhysicsRaycasterHit(BaseEventData data)
    {
        Vector3 hitPos = ((PointerEventData)data).pointerCurrentRaycast.worldPosition;
        int x = (int)Mathf.Floor(hitPos.x / MapLoader.FieldUnit.x);
        int y = (int)-Mathf.Floor(hitPos.z / MapLoader.FieldUnit.y);
        if (hitPos.x < 0.0f)
            x -= 1;
        if (hitPos.z < 0.0f)
            y -= 1;
        this.callback?.Invoke(x, y, data);
    }
}
