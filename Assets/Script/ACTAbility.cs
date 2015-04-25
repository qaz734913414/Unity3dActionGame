using UnityEngine;
using System.Collections;
/// <summary>
/// 所有按键动作技能的基类
/// </summary>
public abstract class ACTAbility : MonoBehaviour {
    //---------public----------//

    //------HideInInspector------//
    /// <summary>
    /// 技能名称
    /// </summary>
    public abstract string AbilityName { get; }
    [HideInInspector]
    /// <summary>
    /// 控制玩家的脚本
    /// </summary>
    public PlayerControl player;
    

    //----------protected---------//
    /// <summary>
    /// 玩家的动画控制器
    /// </summary>
    protected Animator anim;
    protected virtual void TriggerAbility(Transform hit) { }//技能碰撞的接口。以后可以直接使用了
    /// <summary>
    /// 继承他的子类应该使用Init函数初始化，这个函数需要初始化整体
    /// </summary>
    protected virtual void Init() { }

    //-----------private-----------//

    /// <summary>
    /// 这个是Start
    /// </summary>

	void Start () {
        anim = GetComponent<Animator>();//初始化动画
        player = GetComponent<PlayerControl>();//初始化玩家
        if (player.triggerAbility.ContainsKey(AbilityName))//如果玩家类的触发技能里没有我的技能
        {
            player.triggerAbility.Add(AbilityName, TriggerAbility);//给玩家添加技能接口
        }
        Init();//最后调用好点
	}
    /// <summary>
    /// //返回角色是否正在播放某个动画
    /// </summary>
    /// <param name="name">检测是否正在播放的动画名称</param>
    /// <returns></returns>
    protected bool IsName(string name)
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(name);
    }
    /// <summary>
    /// 返回当前动画播放的时间比例
    /// </summary>
    /// <returns></returns>
    protected float GetAnimRate//返回当前动画播放的时间比例
    {
        get
        {
            return anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
        }
    }
    /// <summary>
    /// 返回动画播放时间总长
    /// </summary>
    /// <returns></returns>
    protected float GetAnimLength//返回动画播放时间总长
    {
        get
        {
            return anim.GetCurrentAnimatorStateInfo(0).length;
        }
    }
    /// <summary>
    /// 返回动画还有多少秒播放完。不包括循环
    /// </summary>
    /// <returns></returns>
    protected float GetAnimEndTime//返回动画还有多少秒播放完。不包括循环
    {
        get
        {
            return GetAnimLength - GetAnimLength * GetAnimRate;
        }
    }
}
