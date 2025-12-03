using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class TabletTouchInjector : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster targetRaycaster;
    [SerializeField] private PlayerID deviceIndex = PlayerID.Player1;

    private GameObject currentPressedObject;
    private GameObject currentDraggingObject;
    private GameObject currentHoveredObject;
    private PointerEventData pointerData;
    private EventSystem eventSystem;
    private TabletDevice myDevice;

    void Start()
    {
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("シーンに EventSystem がありません。UIが動きません。");
            return;
        }

        pointerData = new PointerEventData(eventSystem);
        myDevice = deviceIndex == PlayerID.Player1 ? TabletDeviceDriver.Instance.DeviceP1 : TabletDeviceDriver.Instance.DeviceP2;
    }

    void Update()
    {
        if (myDevice == null) return;

        Vector2 rawPos = myDevice.touchPos.ReadValue();
        bool isPressed = myDevice.press.isPressed;

        Vector2 screenPos = rawPos;
        screenPos.x *= Screen.width;
        screenPos.y *= Screen.height;

        pointerData.delta = screenPos - pointerData.position;
        pointerData.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        targetRaycaster.Raycast(pointerData, results);

        if (results.Count > 0) pointerData.pointerCurrentRaycast = results[0];

        GameObject hitObjectRaw = results.Count > 0 ? results[0].gameObject : null;

        GameObject hitObject = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(hitObjectRaw);
        if (hitObject == null) hitObject = hitObjectRaw;

        if (hitObject != currentHoveredObject)
        {
            if (currentHoveredObject != null)
                ExecuteEvents.Execute(currentHoveredObject, pointerData, ExecuteEvents.pointerExitHandler);

            if (hitObject != null)
                ExecuteEvents.Execute(hitObject, pointerData, ExecuteEvents.pointerEnterHandler);

            currentHoveredObject = hitObject;
            pointerData.pointerEnter = hitObject;
        }

        if (isPressed)
        {
            PressProcess(hitObjectRaw);
        }
        else
        {
            ReleaseProcess(hitObjectRaw);
        }
    }

    private void PressProcess(GameObject hitObjectRaw)
    {
        if (currentPressedObject == null)
        {
            if (hitObjectRaw == null) return;

            GameObject downHandler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(hitObjectRaw);
            GameObject target = downHandler != null ? downHandler : hitObjectRaw;

            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);

            currentPressedObject = target;
            pointerData.pointerPress = target;

            GameObject dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(hitObjectRaw);

            if (dragHandler != null)
            {
                ExecuteEvents.Execute(dragHandler, pointerData, ExecuteEvents.beginDragHandler);
                currentDraggingObject = dragHandler;
            }
        }
        else
        {
            if (currentDraggingObject != null)
            {
                ExecuteEvents.Execute(currentDraggingObject, pointerData, ExecuteEvents.dragHandler);
            }
        }
    }

    private void ReleaseProcess(GameObject hitObjectRaw)
    {
        if (currentPressedObject != null)
        {
            ExecuteEvents.Execute(currentPressedObject, pointerData, ExecuteEvents.pointerUpHandler);

            GameObject clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObjectRaw);

            if (currentPressedObject == clickHandler)
            {
                ExecuteEvents.Execute(currentPressedObject, pointerData, ExecuteEvents.pointerClickHandler);
            }

            if (currentDraggingObject != null)
            {
                ExecuteEvents.Execute(currentDraggingObject, pointerData, ExecuteEvents.endDragHandler);
                currentDraggingObject = null;
            }

            pointerData.pointerPress = null;
            currentPressedObject = null;
        }
    }
}