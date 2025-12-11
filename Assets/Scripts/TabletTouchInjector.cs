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

    // 前フレームで押していたかを判定するフラグ
    private bool wasPressed = false;

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

        // 1. 座標の計算
        Vector2 normalizedPosition = myDevice.normalizedTouchPos.ReadValue();
        Vector2 screenPos = new Vector2(
            normalizedPosition.x * Screen.width,
            normalizedPosition.y * Screen.height
        );

        bool isPressed = myDevice.press.isPressed;

        // 2. Delta（移動量）の計算修正
        // タッチした瞬間(isPressed == true && wasPressed == false)は
        // 前回位置が遠くにある可能性があるため、Deltaを0にする
        if (isPressed)
        {
            if (!wasPressed)
            {
                // 押し始め：移動量はゼロ
                pointerData.delta = Vector2.zero;
            }
            else
            {
                // ドラッグ中：現在地 - 前回地
                pointerData.delta = screenPos - pointerData.position;
            }
            // 位置を更新
            pointerData.position = screenPos;
        }
        else
        {
            // 押していない時も位置は追跡しておくが、Deltaは発生させない
            pointerData.delta = Vector2.zero;
            pointerData.position = screenPos;
        }

        // 3. Raycast実行
        List<RaycastResult> results = new List<RaycastResult>();
        targetRaycaster.Raycast(pointerData, results);

        // Raycast結果を更新
        if (results.Count > 0)
        {
            pointerData.pointerCurrentRaycast = results[0];
        }
        else
        {
            // 何も当たってない場合はクリアしておく
            pointerData.pointerCurrentRaycast = new RaycastResult();
        }

        GameObject hitObjectRaw = results.Count > 0 ? results[0].gameObject : null;

        // ホバー処理（Enter/Exit）
        HandleHover(hitObjectRaw);

        // 4. クリック・ドラッグ処理
        if (isPressed)
        {
            PressProcess(hitObjectRaw);
        }
        else
        {
            ReleaseProcess(hitObjectRaw);
        }

        // 最後にフラグを更新
        wasPressed = isPressed;
    }

    private void HandleHover(GameObject hitObjectRaw)
    {
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
    }

    private void PressProcess(GameObject hitObjectRaw)
    {
        // まだ何も押していない（押し始め）
        if (currentPressedObject == null)
        {
            if (hitObjectRaw == null) return;

            // 【重要】スライダー等のために、押した瞬間のRaycast情報をPressRaycastに保存する
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;

            GameObject downHandler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(hitObjectRaw);
            GameObject target = downHandler != null ? downHandler : hitObjectRaw;

            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);

            currentPressedObject = target;
            pointerData.pointerPress = target;

            // ドラッグ開始判定
            GameObject dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(hitObjectRaw);
            if (dragHandler != null)
            {
                ExecuteEvents.Execute(dragHandler, pointerData, ExecuteEvents.beginDragHandler);
                currentDraggingObject = dragHandler;
            }
        }
        else
        {
            // 既に何かを押している（ドラッグ中）
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

            // 同じオブジェクト上で離されたらクリックとみなす
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
            // pointerPressRaycastはクリアしなくてよい（次のPressで上書きされるため）
        }
    }
}