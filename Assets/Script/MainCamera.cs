using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{

    public Transform player;//玩家角色
    public float xOffset = 0, yOffset = 0;//镜头位置和玩家位置的偏移
    public float xMaxOffset = 0f, yMaxOffset = 0f;//超过偏移镜头才会自动跟踪
    public float xMin = 0, xMax = 0, yMin = 0, yMax = 0;//限制镜头X轴的最大最小值

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
            ResetCamera();//重设镜头
        else Debug.Log("找不到要跟随的角色");
    }
    void LateUpdate()
    {
        //if (player != null)
        //    ResetCamera();//重设镜头
        //else Debug.Log("找不到要跟随的角色");
    }

    /// <summary>
    /// 重置镜头
    /// </summary>
    void ResetCamera()//设置镜头的XY跟角色对齐
    {
        Vector3 camPos = transform.position;//镜头当前位置
        Vector3 plyPos = player.position;//玩家位置


        camPos = plyPos;
        camPos = camPos - new Vector3(xOffset, yOffset, 0);//加上和玩家的偏移量
        camPos.z = transform.position.z;
        transform.position = camPos;
    }
}
