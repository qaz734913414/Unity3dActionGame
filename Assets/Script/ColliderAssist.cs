using UnityEngine;
using System.Collections;
/// <summary>
/// 挂在碰撞检测辅助物体对象上的脚本
/// </summary>
public class ColliderAssist : MonoBehaviour {

    PlayerControl player;//玩家的脚本
    string abilityName;
	void Start () {
        GetComponent<Collider2D>().enabled = false;//初始化让他false····就不用手动调了
        player = transform.parent.GetComponent<PlayerControl>();//玩家的脚本
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    /*协同
yield return null;        //让出时间片，让父进程先执行
yield return new WaitForSeconds(1);            //在程序执行到1秒时才执行接下来的代码
yield return new WaitForFixedUpdate();        //在FixedUpdate调用之后才执行接下来的代码
yield return new WaitForEndOfFrame();        //在这一帧结束之后才执行接下来的代码
yield return new StartCoroutine("xxx");        //在xxx协同执行完之后才执行接下来的代码
    */
    void OnTriggerEnter2D(Collider2D hit)//碰撞打算用这个。多个物体可以正常接受
    {
        if(player.triggerAbility.ContainsKey(abilityName))//先看看玩家有没有这个技能
        {
            player.triggerAbility[abilityName](hit.transform);//将碰到的人传进指定的技能类的函数里
        }
    }
    void OnTriggerStay2D(Collider2D hit)//在这里关闭碰撞。每帧发送。在所有的OnTriggerEnter2D后再才会处理这个。
    {
        GetComponent<Collider2D>().enabled = false;//关闭
    }
    public void TriggerCollider(string _abilityName)
    {
        abilityName = _abilityName;//记录触发技能名称
        GetComponent<Collider2D>().enabled = true;//开启碰撞
        StartCoroutine(CloseCollider());
    }
    IEnumerator CloseCollider()
    {
        yield return new WaitForEndOfFrame();//等待当前帧执行完毕
        GetComponent<Collider2D>().enabled = false;//协作程序，功效是这帧完成后关闭碰撞激活。
    }
}
