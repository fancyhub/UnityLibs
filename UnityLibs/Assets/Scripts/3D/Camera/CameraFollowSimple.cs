using UnityEngine;


public class CameralFollowSimple : MonoBehaviour
{
    // 限制上下角度，避免穿模或倒立
    private const float CPitchMin = -15f;
    private const float CPitchMax = 70f;

    [Header("Target to follow")]
    public Transform target;

    [Header("Camera distance")]
    public float distanceMin = 1.0f;
    public float distanceMax = 3.0f;

    [Space]
    public float OffsetYMin = 1.8f;
    public float offsetYMax = 1.2f;

    [Space]
    public float distance = 2.5f;       // 距离

    [Space]
    public float mouseZoomSensitivity = 2f;
    public float touchZoomSensitivity = 2f;

    [Header("Camera Rotation")]
    public float pitch = 31f;         // 上下角度
    public float yaw = 0f;            // 左右角度    

    public float mouseRotSensitivity = 10f;
    public float touchRotSensitivity = 10f;

    [Header("平滑参数")]
    public float localSmoothSpeed = 6f;
    public float worldSmoothSpeed = 6f;

    private float currentDistance;
    private Quaternion currentRot;
    private Vector3 targetWorldPos;

    private void Awake()
    {
        currentDistance = distance;
        currentRot = Quaternion.Euler(pitch, yaw, 0);
        if (target != null)
        {
            targetWorldPos = target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (GetInputDelta(mouseRotSensitivity, touchRotSensitivity, out var input))
        {
            yaw += input.x;            
            pitch -= input.y;
            pitch = Mathf.Clamp(pitch, CPitchMin, CPitchMax);
        }

        if (GetInputZoom(mouseZoomSensitivity, touchZoomSensitivity, out var zoom))
        {
            distance += zoom;
            distance = Mathf.Clamp(distance, distanceMin, distanceMax);
        }

        currentDistance = Mathf.Lerp(currentDistance, distance, localSmoothSpeed * Time.deltaTime);
        currentRot = Quaternion.Slerp(currentRot, Quaternion.Euler(pitch, yaw, 0), localSmoothSpeed * Time.deltaTime);


        Vector3 targetLocalOffset = Vector3.zero;
        {
            float distanceRange = Mathf.InverseLerp(distanceMin, distanceMax, currentDistance);
            float offsetY = Mathf.Lerp(OffsetYMin, offsetYMax, distanceRange);
            targetLocalOffset.y += offsetY;
        }
        Vector3 camLocalPos = targetLocalOffset - currentRot * Vector3.forward * currentDistance;


        Vector3 targetPos = target.position;
        targetWorldPos = Vector3.Lerp(targetWorldPos, targetPos, worldSmoothSpeed * Time.deltaTime);
        //targetWorldPos = target.position;// Vector3.SmoothDamp(targetWorldPos, targetPos, ref targetVelocity, smoothTime);

        // 平滑移动
        transform.position = camLocalPos + targetWorldPos;

        // 始终看向目标
        transform.LookAt(targetLocalOffset + targetPos);

    }

    private static bool GetInputDelta(float mouseRotSensitivity, float touchRotSensitivity, out Vector2 delta)
    {
        // 电脑：按住左键才返回位移
        if (Input.GetMouseButton(0))
        {
            const float Threshold = 3;
            delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            if (Mathf.Abs(delta.x) < Threshold && Mathf.Abs(delta.y) < Threshold)
            {
                delta = delta * mouseRotSensitivity;
                return true;
            }
        }

        // 手机：单指滑动返回位移
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                delta = touch.deltaPosition;
                delta = delta * touchRotSensitivity;
                return true;
            }
        }

        // 无操作返回 0
        delta = Vector2.zero;
        return false;
    }


    private static bool GetInputZoom(float zoomSensitivity, float touchZoomSensitivity, out float delta)
    {
        delta = 0;
        if (Input.mouseScrollDelta.y != 0)
        {
            delta -= Input.mouseScrollDelta.y * zoomSensitivity;
            return true;
        }

        // 手机：双指缩放
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float oldDist = Vector2.Distance(prev0, prev1);
            float newDist = Vector2.Distance(t0.position, t1.position);

            delta -= (newDist - oldDist) * touchZoomSensitivity;
            return true;
        }
        return false;
    }
}


