using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttachAreaEntity : MonoBehaviour, IAttachArea
{
    //public GameObject CloneObject { get; set;}

    public UnityEvent HitEvent;
    public UnityEvent ExitEvent;
    public UnityEvent ReleaseEvent;
    /// <summary>
    /// 射线命中效果
    /// </summary>
    /// <param name="gameObject"></param>
    public void Hit(GameObject gameObject)
    {
        //if (CloneObject == null)
        //{
        //    var obj = gameObject.GetComponentsInChildren<Renderer>();
        //    GameObject game = new GameObject(gameObject.name + "Clone");
        //    for (int i = 0; i < obj.Length; i++)
        //    {
        //        if (obj[i] as SkinnedMeshRenderer)
        //        {
        //            var skinnedMeshRenderer = (SkinnedMeshRenderer)obj[i];
        //            GameObject skin = new GameObject(obj[i].name + "Clone");
        //            skin.AddComponent<MeshFilter>().mesh = skinnedMeshRenderer.sharedMesh;
        //            skin.AddComponent<MeshRenderer>().materials = skinnedMeshRenderer.materials;
        //            skin.transform.SetParent(game.transform);
        //            skin.transform.localScale = obj[i].transform.lossyScale;

        //        }
        //        else
        //        {
        //            var cloneObj = GameObject.Instantiate(obj[i].gameObject, game.transform);
        //            cloneObj.transform.localScale = obj[i].transform.lossyScale;
        //        }
        //    }
        //    game.transform.SetParent(transform);
        //    CloneObject = game;

        //    gameObject.SetActive(false);
        //}
        HitEvent?.Invoke();
    }

    public void Exit(GameObject gameObject)
    {
        ExitEvent?.Invoke();
    }

    public Transform ReleasePosition;

    /// <summary>
    /// 释放
    /// </summary>
    public void Release(GameObject gameObject)
    {
        gameObject.transform.position = ReleasePosition.transform.position;
        gameObject.transform.rotation = ReleasePosition.transform.rotation;
        GetComponent<Collider>().enabled = false;
        ReleaseEvent?.Invoke();
    }
}
