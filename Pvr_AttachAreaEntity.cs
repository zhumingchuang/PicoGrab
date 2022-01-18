using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
/// <summary>
/// 远程抓取
/// </summary>
public class Pvr_AttachAreaEntity : MonoBehaviour, IAttach
{
    [SerializeField]
    bool attachAuthority=true;
    public bool AttachAuthority 
    {
        get
        {
            return attachAuthority;
        }
        set
        {
            attachAuthority = value;
        }
    }
    public bool Authority { get; private set; }

    public bool OnRelease { get;private set; }

    public Transform ReleaseTarget { get; private set; }

    Ray ray;
    RaycastHit hit;
    IAttachArea attachArea;

    AttachSeting attachSeting;

    /// <summary>
    /// 放置区域
    /// </summary>
    public List<Transform> place;


    public Action<Transform> Release;

    [SerializeField]
    public UnityEvent OnAttachEnterEvent;
    public UnityEvent OnAttachExitEvent;
    public UnityEvent AttachEnterEvent;

    void Start()
    {
        attachSeting = GetComponentInChildren<IAttachSeting>() == null ? Pvr_AttachSeting.DefaultAttachSeting : GetComponentInChildren<IAttachSeting>().AttachSeting;
    }

    /// <summary>
    /// 抓取
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="attachData"></param>
    public void AttachEnter(object obj, AttachData attachData)
    {
        Debug.Log($"开始抓取物体{attachData.obj}");
        ray = new Ray();
        ray.origin = attachData.rayData.origin;
        ray.direction = attachData.rayData.direction;
        AttachEnterEvent?.Invoke();
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="attachData"></param>
    public void AttachExit(object obj, AttachData attachData)
    {
        Debug.Log($"释放抓取物体{attachData.obj}");
        ray.origin = attachData.obj.transform.position;
        ray.direction = attachData.rayData.direction;
        if (Physics.Raycast(ray, out hit) && (place.Contains(hit.transform)))
        {
            Debug.Log($"{attachData.obj}放在了指定位置");
            OnRelease = true;
            var attachArea = hit.transform.GetComponent<IAttachArea>();
            if (attachArea != null)
            {
                this.attachArea = attachArea;
                attachArea.Release(attachData.obj);
            }
            ReleaseTarget = hit.transform;
            if (Release!=null)
            {
                Release.Invoke(ReleaseTarget);
            }
            AttachAuthority = false;
        }
        else
        {
            OnRelease = false;
            ReleaseTarget = null;
            GetComponent<Pvr_GrabRestore>().ResetGrab();
        }
    }

    /// <summary>
    /// 抓取中
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="attachData"></param>
    public void AttachStay(object obj, AttachData attachData)
    {
        ray.origin = attachData.obj.transform.position;
        ray.direction = attachData.rayData.direction;
        Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        if (Physics.Raycast(ray, out hit) && (place.Contains(hit.transform)))
        {
            Debug.Log($"{attachData.obj}检测到投放区域");
            Authority = true;

            var attachArea = hit.transform.GetComponent<IAttachArea>();
            //if (this.attachArea != null && this.attachArea != attachArea)
            //{
            //    this.attachArea.Exit(attachData.obj);
            //}
            if (attachArea != null)
            {
                this.attachArea = attachArea;
                attachArea.Hit(attachData.obj);
            }
            //else
            //{
            if (Vector3.Distance(transform.position, ray.origin + ray.direction.normalized * hit.distance) > 0.1f)
                    transform.position = Vector3.Lerp(transform.position, ray.origin + ray.direction.normalized * hit.distance, Time.deltaTime * attachSeting.attachSpeed);
            //}
        }
        else
        {
            Debug.Log($"{attachData.obj}没有检测到投放区域");
            Authority = false;

            if (attachArea != null)
            {
                attachArea.Exit(attachData.obj);
            }
        }
    }

    /// <summary>
    /// 进入抓取范围
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="attachData"></param>
    public void OnAttachEnter(object obj, AttachData attachData)
    {
        Debug.Log(obj);
        Debug.Log(attachData.obj);

        //Debug.Log("可抓取物体");
        OnAttachEnterEvent?.Invoke();
    }

    /// <summary>
    /// 离开抓取范围
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="attachData"></param>
    public void OnAttachExit(object obj, AttachData attachData)
    {
        //Debug.Log("离开可抓取物体");
        OnAttachExitEvent?.Invoke();
    }
}
