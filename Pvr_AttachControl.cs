using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct AttachData
{
    public GameObject obj;
    public Transform parent;
    public RayData rayData;
    public IAttach attach;
}

[Serializable]
public class AttachSeting
{
    public float distance = 0.2f;

    /// <summary>
    /// 抓取时速度
    /// </summary>
    public float attachSpeed = 15;

    /// <summary>
    /// 投掷速度
    /// </summary>
    public float throwSpeed = 5;
}

public static class Pvr_AttachSeting
{
    public static AttachSeting DefaultAttachSeting = new AttachSeting();

}

public class Pvr_AttachControl : MonoBehaviour
{
    public Pvr_ControllerRay pvr_ControllerRay;
    public LayerMask AttachLayer = ~(0 << 1);

    /// <summary>
    /// 可抓取物体
    /// </summary>
    public Action<object, AttachData> OnAttachEnter;

    /// <summary>
    /// 离开可抓取
    /// </summary>
    public Action<object, AttachData> OnAttachExit;

    /// <summary>
    /// 抓取
    /// </summary>
    public Action<object, AttachData> AttachEnter;

    /// <summary>
    /// 释放
    /// </summary>
    public Action<object, AttachData> AttachExit;

    /// <summary>
    /// 抓取中
    /// </summary>
    public Action<object, AttachData> AttachStay;

    public AttachData attachData;
    public object obj;
    public IAttach attachEntity;

    /// <summary>
    /// 抓取参数
    /// </summary>
    AttachSeting attachSeting;

    GameObject controller0;
    GameObject controller1;
    Transform node0;
    Transform node1;
    Transform currentNode;

    Vector3 angularVelocity;
    Vector3 linearVelocity;
    Vector3 angularVelocityGetKey;
    Vector3 angularVelocityAverage;

    bool isAttach;

    float lastTime;
    /// <summary>
    /// 当前是否可以抓取
    /// </summary>
    public bool onAttach { get; private set; }

    /// <summary>
    /// 抓取状态
    /// </summary>
    public bool attachState { get; private set; }

    public void Start()
    {
        attachData = new AttachData();
        controller0 = Pvr_UnitySDKManager.SDK.GetComponentInChildren<Pvr_Controller>().controller0;
        controller1 = Pvr_UnitySDKManager.SDK.GetComponentInChildren<Pvr_Controller>().controller1;

        node0 = controller0.transform.Find("node0");
        if (node0 == null)
        {
            GameObject node0 = new GameObject("node0");
            node0.transform.SetParent(controller0.transform);
            node0.transform.localRotation = Quaternion.identity;
            node0.transform.localScale = Vector3.one;
            this.node0 = node0.transform;
        }

        node1 = controller1.transform.Find("node1");
        if (node1 == null)
        {
            GameObject node1 = new GameObject("node1");
            node1.transform.SetParent(controller1.transform);
            node1.transform.localRotation = Quaternion.identity;
            node1.transform.localScale = Vector3.one;
            this.node1 = node1.transform;
        }
    }

    public void OnEnable()
    {
        pvr_ControllerRay.ControllerRayEnter += ControllerRayEnter;
        pvr_ControllerRay.ControllerRayExit += ControllerRayExit;
        pvr_ControllerRay.ControllerRayDown += ControllerRayDown;
        pvr_ControllerRay.ControllerRayHoldDown += ControllerRayHoldDown;
    }

    public void OnDisable()
    {
        pvr_ControllerRay.ControllerRayEnter -= ControllerRayEnter;
        pvr_ControllerRay.ControllerRayExit -= ControllerRayExit;
        pvr_ControllerRay.ControllerRayDown -= ControllerRayDown;
        pvr_ControllerRay.ControllerRayHoldDown -= ControllerRayHoldDown;
    }

    public void ControllerRayEnter(object obj, RayData raydata)
    {
        if (!attachState)
        {
            int index = 1 << raydata.obj.layer;
            if ((AttachLayer & index) == index)
            {
                var attach = raydata.obj.GetComponentInChildren<IAttach>();
                if (attach != null)
                {
                    var attachSeting = raydata.obj.GetComponentInChildren<IAttachSeting>();

                    this.attachSeting = attachSeting == null ? Pvr_AttachSeting.DefaultAttachSeting : attachSeting.AttachSeting;


                    var direction0 = controller0.transform.forward - controller0.transform.up * 0.25f;
                    var origin0 = controller0.transform.Find("start").position;
                    node0.transform.position = origin0 + direction0.normalized * this.attachSeting.distance;

                    var direction = controller1.transform.forward - controller1.transform.up * 0.25f;
                    var origin = controller1.transform.Find("start").position;
                    node1.transform.position = origin + direction.normalized * this.attachSeting.distance;

                    this.obj = obj;
                    attachData.obj = raydata.obj;
                    attachData.parent = raydata.obj.transform.parent;
                    attachData.rayData = raydata;
                    attachData.attach = attach;
                    onAttach = attach.AttachAuthority;
                    attachEntity = attach;
                    attach.OnAttachEnter(obj, attachData);
                    OnAttachEnter?.Invoke(obj, attachData);
                }
            }
        }
    }

    public void ControllerRayDown(object obj, RayData raydata)
    {
        //Debug.Log(raydata.obj + "按下");
    }

    public void ControllerRayHoldDown(object obj, RayData raydata)
    {
        //Debug.Log(raydata.obj + "按住");
    }

    public void ControllerRayExit(object obj, RayData raydata)
    {
        if (!attachState)
        {
            int index = 1 << raydata.obj.layer;
            if ((AttachLayer & index) == index)
            {
                var attach = raydata.obj.GetComponentInChildren<IAttach>();
                if (attach != null)
                {
                    attachData.obj = raydata.obj;
                    attachData.rayData = raydata;
                    attachData.attach = attachEntity;
                    onAttach = false;
                    attach.OnAttachExit(obj, attachData);
                    OnAttachExit?.Invoke(obj, attachData);
                }
            }
        }
    }

    public void Update()
    {
        if (attachEntity == null) return;
        onAttach = attachEntity.AttachAuthority;
        if (onAttach)
        {
            float time = Time.time;
            if (time > lastTime)
                if (Input.GetKey(KeyCode.Space) || Pvr_UnitySDKAPI.Controller.UPvr_GetKey(pvr_ControllerRay.ControllerIndex, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER))
                {
                    if (pvr_ControllerRay.ControllerIndex == 0)
                    {
                        currentNode = node0;
                    }

                    if (pvr_ControllerRay.ControllerIndex == 1)
                    {
                        currentNode = node1;
                    }
                    attachState = true;
                    attachData.rayData = pvr_ControllerRay.RayData;

                    if (!attachEntity.Authority)
                    {
                        attachData.obj.transform.position = Vector3.Lerp(attachData.obj.transform.position, currentNode.position, Time.deltaTime * attachSeting.attachSpeed);
                        attachData.obj.transform.rotation = Quaternion.Lerp(attachData.obj.transform.rotation, currentNode.rotation, Time.deltaTime * attachSeting.attachSpeed);
                        attachData.obj.transform.SetParent(currentNode);
                        attachData.obj.GetComponent<Rigidbody>().isKinematic = true;
                    }

                    angularVelocityGetKey = Pvr_UnitySDKAPI.Controller.UPvr_GetAngularVelocity(pvr_ControllerRay.ControllerIndex);

                    if (!isAttach)
                    {
                        isAttach = true;
                        attachEntity.AttachEnter(obj, attachData);
                        AttachEnter?.Invoke(obj, attachData);
                    }
                    attachEntity.AttachStay(obj, attachData);
                    AttachStay?.Invoke(obj, attachData);
                }
        }

        if (Input.GetKeyUp(KeyCode.Space) || Pvr_UnitySDKAPI.Controller.UPvr_GetKeyUp(pvr_ControllerRay.ControllerIndex, Pvr_UnitySDKAPI.Pvr_KeyCode.TRIGGER))
        {
            if (attachState)
            {
                isAttach = false;
                attachData.obj.transform.SetParent(attachData.parent);
                attachData.obj.GetComponent<Rigidbody>().isKinematic = false;

                angularVelocity = Pvr_UnitySDKAPI.Controller.UPvr_GetAngularVelocity(pvr_ControllerRay.ControllerIndex);
                angularVelocityAverage = (angularVelocityGetKey + angularVelocity) / 2;
                linearVelocity = Pvr_UnitySDKAPI.Controller.UPvr_GetVelocity(pvr_ControllerRay.ControllerIndex);

                attachData.obj.GetComponent<Rigidbody>().angularVelocity = angularVelocityAverage * 0.0001f * attachSeting.throwSpeed;
                attachData.obj.GetComponent<Rigidbody>().velocity = linearVelocity * 0.0001f * attachSeting.throwSpeed;

                attachState = false;
                attachData.rayData = pvr_ControllerRay.RayData;
                attachEntity.AttachExit(obj, attachData);
                AttachExit?.Invoke(obj, attachData);

                lastTime = Time.time + Time.deltaTime * 4;
            }
        }
    }
}
