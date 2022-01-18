using Pvr_UnitySDKAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RayData
{
    public GameObject obj;
    public Vector3 origin;
    public Vector3 direction;
}
public class Pvr_ControllerRay : MonoBehaviour
{
    GameObject controller0;
    GameObject controller1;
    GameObject currentController;

    GameObject dot;
    GameObject rayLine;
    GameObject referenceObj;
    GameObject currentStart;
    GameObject currentDot;


    bool isHasController;
    bool noClick;
    float disX, disY, disZ;

    Transform dragObj;


    //Ray
    Ray ray;
    RaycastHit hit;
    Transform currentHit;
    Transform lastHit;
    RayData rayData;

    //Public

    public RayData RayData
    {
        get
        {
            return rayData;
        }
    }

    /// <summary>
    /// 射线按键
    /// </summary>
    public Pvr_KeyCode pvr_RayKey = Pvr_KeyCode.TRIGGER;

    /// <summary>
    /// 忽略检测
    /// </summary>
    public LayerMask IgnoreLayer;

    /// <summary>
    /// 检测层级
    /// </summary>
    public LayerMask CheckLayer = ~(0 << 1);

    /// <summary>
    /// 射线默认长度
    /// </summary>
    public float rayDefaultLength = 4;

    /// <summary>
    /// 射线进入
    /// </summary>
    public Action<object, RayData> ControllerRayEnter;

    /// <summary>
    /// 射线离开
    /// </summary>
    public Action<object, RayData> ControllerRayExit;

    /// <summary>
    /// 按下
    /// </summary>
    public Action<object, RayData> ControllerRayDown;

    /// <summary>
    /// 按住
    /// </summary>
    public Action<object, RayData> ControllerRayHoldDown;

    /// <summary>
    /// 手柄
    /// </summary>
    public int ControllerIndex { get; private set; }


    private void Start()
    {
        ray = new Ray();
        hit = new RaycastHit();
        rayData = new RayData();
        if (Pvr_UnitySDKManager.SDK.isHasController)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerStateChangedEvent += ControllerStateListener;
            isHasController = true;
            controller0 = Pvr_UnitySDKManager.SDK.GetComponentInChildren<Pvr_Controller>().controller0;
            controller1 = Pvr_UnitySDKManager.SDK.GetComponentInChildren<Pvr_Controller>().controller1;
#if UNITY_EDITOR
            currentController = controller1;
            ControllerIndex = 1;
            currentStart = currentController.transform.Find("start").gameObject;
            currentDot = currentController.transform.Find("dot").gameObject;
            dot = controller1.transform.Find("dot").gameObject;
            dot.SetActive(true);
            rayLine = controller1.transform.Find("ray_LengthAdaptive").gameObject;
            rayLine.SetActive(true);
#endif
        }
        referenceObj = new GameObject("ReferenceObj");

    }

    private void OnDestroy()
    {
        if (isHasController)
        {
            Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
            Pvr_ControllerManager.SetControllerStateChangedEvent -= ControllerStateListener;
        }
    }

    private void Update()
    {
        if (currentController != null)
        {
            ray.direction = currentController.transform.forward - currentController.transform.up * 0.25f;
            ray.origin = currentStart.transform.position;
            rayData.origin = ray.origin;
            rayData.direction = ray.direction;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~IgnoreLayer))
            {
                currentHit = hit.transform;
                int index = 1 << hit.transform.gameObject.layer;
                if ((CheckLayer & index) == index)
                {
                    //进入
                    if (currentHit != lastHit && ControllerRayEnter != null)
                    {

                        if(lastHit!=null&& ControllerRayExit!=null)
                        {
                            rayData.obj = lastHit.gameObject;
                            ControllerRayExit.Invoke(currentController, rayData);
                        }

                        rayData.obj = hit.transform.gameObject;
                        ControllerRayEnter.Invoke(currentController, rayData);
                    }

                    if (!noClick)
                    {

                    }

                    if (Controller.UPvr_GetKeyDown(0, pvr_RayKey) || Controller.UPvr_GetKeyDown(1, pvr_RayKey) || Input.GetMouseButtonDown(0))
                    {
                        referenceObj.transform.position = hit.point;

                        disX = hit.transform.position.x - referenceObj.transform.position.x;
                        disY = hit.transform.position.y - referenceObj.transform.position.y;
                        dragObj = hit.transform;
                    }

                    //按住
                    if (Controller.UPvr_GetKey(0, pvr_RayKey) || Controller.UPvr_GetKey(1, pvr_RayKey) || Input.GetMouseButton(0))
                    {
                        if(dragObj!=null)
                        {
                            if (hit.transform == dragObj.transform)
                            {
                                referenceObj.transform.position = new Vector3(hit.point.x, hit.point.y, hit.transform.position.z);
                                //dragObj.position = new Vector3(referenceObj.transform.position.x + disX, referenceObj.transform.position.y + disY, hit.transform.position.z);
                            }
                        }

                        if (ControllerRayHoldDown != null)
                        {
                            rayData.obj = hit.transform.gameObject;
                            ControllerRayHoldDown.Invoke(currentController, rayData);
                        }
                    }
                }
                lastHit = hit.transform;
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, Color.red);
#endif
                currentDot.transform.position = hit.point;
                if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                {
                    float scale = 0.178f * currentDot.transform.localPosition.z / 3.3f;
                    Mathf.Clamp(scale, 0.05f, 0.178f);
                    currentDot.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
            else
            {
                //离开
                if (lastHit != null)
                {
                    int index = 1 << lastHit.transform.gameObject.layer;
                    if (lastHit != null && (CheckLayer & index) == index)
                    {
                        if (currentHit != null && ControllerRayExit != null)
                        {
                            rayData.obj = lastHit.gameObject;
                            ControllerRayExit.Invoke(currentController, rayData);
                        }
                    }
                }
                currentHit = null;
                lastHit = null;
                noClick = false;

                currentDot.transform.position = ray.origin + ray.direction.normalized*(0.5f+ rayDefaultLength);
                if (Pvr_ControllerManager.Instance.LengthAdaptiveRay)
                {
                    currentDot.transform.localScale = new Vector3(0.178f, 0.178f, 1);
                }
            }
#if UNITY_EDITOR
            rayLine.GetComponent<LineRenderer>().SetPosition(0, currentController.transform.TransformPoint(0, 0, 0.072f));
            rayLine.GetComponent<LineRenderer>().SetPosition(1, dot.transform.position);
#endif
        }
        //按下
        if (Controller.UPvr_GetKeyDown(0, pvr_RayKey) || Controller.UPvr_GetKeyDown(1, pvr_RayKey) || Input.GetMouseButtonDown(0))
        {
            if (lastHit != null)
            {
                int index = 1 << lastHit.transform.gameObject.layer;
                if (lastHit != null && (CheckLayer & index) == index && currentHit != null)
                {
                    noClick = true;
                    if (ControllerRayDown != null)
                    {
                        rayData.obj = hit.transform.gameObject;
                        ControllerRayDown.Invoke(currentController, rayData);
                    }
                }
            }
        }
    }


    /// <summary>
    /// 服务启动成功
    /// </summary>
    private void ServiceStartSuccess()
    {
        ControllerState();
    }

    /// <summary>
    /// 控制器状态监听
    /// </summary>
    private void ControllerStateListener(string data)
    {
        ControllerState();
    }

    private void ControllerState()
    {
        if (Controller.UPvr_GetMainHandNess() == 0)
        {
            currentController = controller0;
            currentStart = currentController.transform.Find("start").gameObject;
            currentDot = currentController.transform.Find("dot").gameObject;
            ControllerIndex = 0;
        }
        if (Controller.UPvr_GetMainHandNess() == 1)
        {
            currentController = controller1;
            currentStart = currentController.transform.Find("start").gameObject;
            currentDot = currentController.transform.Find("dot").gameObject;
            ControllerIndex = 1;
        }
    }
}
