using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Pvr_GrabRestore : MonoBehaviour
{
    public Transform defaultPos;

    public float waitTime = 1.5f;


    /// <summary>
    /// 获取控制点.
    /// </summary>
    /// <param name="startPos">起点.</param>
    /// <param name="endPos">终点.</param>
    /// <param name="offset">偏移量.</param>
    private Vector3 CalcControlPos(Vector3 startPos, Vector3 endPos, float offset)
    {
        //方向(由起始点指向终点)
        Vector3 dir = endPos - startPos;
        //取另外一个方向. 这里取向上.
        Vector3 otherDir = Vector3.up;

        //求平面法线.  注意otherDir与dir不能调换位置,平面的法线是有方向的,(调换位置会导致法线方向相反)
        //ps: 左手坐标系使用左手定则 右手坐标系使用右手定则 (具体什么是左右手坐标系这里不细说请Google)
        //unity中世界坐标使用的是左手坐标系,所以法线的方向应该用左手定则判断.
        Vector3 planeNormal = Vector3.Cross(otherDir, dir);

        //再求startPos与endPos的垂线. 其实就是再求一次叉乘.
        Vector3 vertical = Vector3.Cross(dir, planeNormal).normalized;
        //中点.
        Vector3 centerPos = (startPos + endPos) / 2f;
        //控制点.
        Vector3 controlPos = centerPos + vertical * offset;

        return controlPos;
    }


    /// <summary>
    /// 重新设置抓取位置
    /// </summary>
    public void ResetGrab()
    {
        StartCoroutine(ResetPos());
    }

    /// <summary>
    /// 还原位置
    /// </summary>
    private IEnumerator ResetPos()
    {
        yield return new WaitForSeconds(waitTime);
        GetComponentInChildren<Rigidbody>().isKinematic = true;
        var collider = GetComponentsInChildren<Collider>();
        for (int j = 0; j < collider.Length; j++)
        {
            collider[j].enabled = false;
        }

        while (Vector3.Distance(defaultPos.position, transform.position) > 0.1f)
        {
            var end = CalcControlPos(defaultPos.position, transform.position, 0.1f);
            transform.position = Vector3.Lerp(transform.position, end, Time.deltaTime);
            yield return null;
        }

        transform.position = defaultPos.position;
        transform.rotation = defaultPos.rotation;
        GetComponentInChildren<Rigidbody>().isKinematic = false;
        for (int j = 0; j < collider.Length; j++)
        {
            collider[j].enabled = true;
        }
    }
}
