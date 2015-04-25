using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// 可以考虑平时只能walk。场景有怪的时候就变成run
public class PlayerControl : MonoBehaviour {
    //--------publuc--------//
    public float xWalkSpeed = 0.05f, yWalkSpeed = 0.03f, xRunSpeed = 0.5f, yRunSpeed = 0.35f;//玩家走跑速度
    public float jumpVelocity = 5f, jumpXquiken = 0.1f, jumpYquiken = -0.25f;//跳跃时设置的力。跳跃时按↔↕的移动的移动速度。
    public float countHP = 50f, currentHP = 50f;//主角的血量
    public float runKeyInterval = 0.3f;//跑按键间隔
    public Transform colliderAssist, groundCheck,body;//攻击辅助碰撞脚本对象，地面检测
    //-------HideInInspector------//
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public delegate void TriggerAbility(Transform hit);//定义一个委托，用于触发技能。主要给下面的Dictionary用
    [HideInInspector]
    public Dictionary<string, TriggerAbility> triggerAbility = new Dictionary<string, TriggerAbility>();//定义一个Dictionary(Mapping)。映射相应的技能
    [HideInInspector]//隐藏在InInspector面板显示
    public bool isRightSide = true;//控制角色的面相,.默认是右边
    [HideInInspector]
    public bool isGround;//是否在地面···
    [HideInInspector]
    public bool isGroundNormalAction = false, isSkyNormalAction = false;//是否能在地面、空中上正常行动。玩家需要在地面上，不被攻击
    [HideInInspector]
    public bool isDeath = false;//是否死亡
    [HideInInspector]
    public float unmatchedTime = 0.0f;//无敌时间
    //-------protected-----------//

    //-------private------------//
    bool isLeft, isRight, isDown, isUp;//按下上下左右
    bool isIdle;//是否休闲状态
    Transform root;
    Rigidbody2D rigid;
    private string deathName = "death", walkName = "walk", runName = "run", idleName = "idle", jumpName = "jump";//死亡、走、跑的动画名称
    private string deathVar = "death", walkVar = "walk", runVar = "run", idleVar = "idle", jumpVar = "jump";//控制死亡、走、跑的变量名称
    private string changeColorTimeVarName = "changeColorTime";//改变颜色变量名的值
    private KeyCode jumpKey = KeyCode.Space;//设置跳跃键
    KeyCode upKey = KeyCode.UpArrow, downKey = KeyCode.DownArrow, leftKey = KeyCode.LeftArrow, rightKey = KeyCode.RightArrow;//设置上下左右键
    private float lastDownTime;//第一次按下左右的时间
    private bool lastRightSide;//按下左右时的面相
    private bool isRunJump;//记录跳跃前是行走还是跑
	void Start () {
        //一些初始化
        anim = GetComponent<Animator>();
        root = transform.root;
        rigid = root.GetComponent<Rigidbody2D>();
        //添加技能脚本组件
        gameObject.AddComponent<Attack1>();
	}
	
    void Update()
    {
        CheckKey();//检测按键
        if (isLeft && isRightSide) Flip();//如果玩家按下右方向，并且面相不是右边。调用反转函数
        else if (isRight && !isRightSide) { Flip(); }//否则判断如果玩家按下←方向，并且面相是右边。调用反转函数
        if (Input.GetKeyDown(leftKey) || Input.GetKeyDown(rightKey) && isGroundNormalAction)//跑
        {
            //bool downRight = Input.GetKeyDown(rightKey), downLeft = Input.GetKeyDown(leftKey);//重写获取是否按下左右键
            //if (downRight && !isRightSide) Flip();//如果玩家按下右方向，并且面相不是右边。调用反转函数
            //if (downLeft && isRightSide) Flip();////否则判断如果玩家按下←方向，并且面相是右边。调用反转函数
            CheckKey();//检测按键
            if (isLeft && isRightSide) Flip();//如果玩家按下右方向，并且面相不是右边。调用反转函数
            else if (isRight && !isRightSide) { Flip(); }//否则判断如果玩家按下←方向，并且面相是右边。调用反转函数
            if (Time.time - lastDownTime <= runKeyInterval && lastRightSide == isRightSide)//如果距离上一次按下的时间小于0.5秒。以及面向跟上一次的一样
            {
                anim.SetBool(runVar, true);//设置跑
            }
            lastDownTime = Time.time;//上一次按下的时间
            lastRightSide = isRightSide;//上一次按下时的面向
        }
        if (isIdle && !anim.GetBool(runVar)) anim.SetBool(walkVar, isRight || isLeft || isUp || isDown);//判断玩家是否在地面正常行动。按下左右任意一个都true
        if (isIdle && anim.GetBool(runVar)) anim.SetBool(runVar, isRight || isLeft || isUp || isDown);
    }
    void FixedUpdate()
    {
        if (currentHP <= 0)
        {
            PlayerDeath();//角色死亡
        }
        CheckUnmatched();//检测无敌
        isGround = Physics2D.Linecast(groundCheck.position, root.position, 1 << LayerMask.NameToLayer("ground"));//检测是否在地面
        CheckIsSkyNormalAction();//检测是否能在空中正常行动
        CheckIsGroundNormalAction();//检测是否能在地面正常行动
        LRwalk();//角色左右走
        UDwalk();//角色上下走
        LRrun();//角色左右跑
        UDrun();//角色上下跑
        SkyLRwalk();//空中左右行走
        SkyUDwalk();//空中上下行走
        SkyLRrun();//空中左右跑
        SkyUDrun();//空中上下跑
        CheckJump();//检查跳跃
        CheckIdle();//检测休闲状态
        //遍历动画函数
        //Attack2();//技能
        //检测无敌··
    }
    void CheckIdle()//检测休闲状态
    {
        if(isGroundNormalAction&&!isLeft&&!isRight&&!isDown&&!isUp)
        {
            isIdle = true;
        }
        else isIdle=false;
        anim.SetBool(idleName, isIdle);//设置播放这个动画
    }
    void CheckJump()
    {
        if (Input.GetKeyDown(jumpKey) && !IsName(jumpName))//按下跳跃键以及不是正在播放跳跃动画
        {
            if (isGroundNormalAction)//如果能在地面正常活动
            {
                anim.SetBool(jumpVar, true);//设置跳跃变量为true
                //rigid.AddForce(new Vector2(0, 200f));
                rigid.velocity = new Vector2(rigid.velocity.x, jumpVelocity);//直接设置速度
                isRunJump = false;//记录跳跃前是行走还是跑
                if (IsName(runName))//如果是跑动画
                {
                    isRunJump = true;
                }
            }
        }
        else
        {
            anim.SetBool(jumpVar, false);//如果没按下jump键就让他成为false
        }
        //       if (Input.GetKeyDown(jumpKey) && isGround && !IsName(jumpName))
    }
    /// <summary>
    /// 检测按键上下左右
    /// </summary>
    void CheckKey()
    {
        isLeft = isRight = isDown = isUp = false;//默认是false··无视这点效率了
        if (isLeft = Input.GetKey(leftKey)) { }
        else
        {
            isRight = Input.GetKey(rightKey);
        }
        if (isDown = Input.GetKey(downKey)) { }
        else isUp = Input.GetKey(upKey);
    }
    void Flip()//面向反转函数
    {
        if (isGroundNormalAction || isSkyNormalAction)//如果是在地面能正常活动状态。才能反转
        {
            Vector3 vt3 = body.localScale;
            vt3.x *= -1;
            body.localScale = vt3;//修改父物体x缩放为反方向
            isRightSide = !isRightSide;//设置角色面相相反
        }
    }
    bool IsName(string name)//判断当前播放的是否某个动画名称
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(name);
    }
    void LRwalk()//左右行走
    {
        if (IsName(walkName) && isGroundNormalAction)//如果播放的是行走状态 并且是在地面
        {
            if (isLeft)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x - xWalkSpeed, v.y, v.z);//左右移动是改变根的位置
                //rigid.AddForce(new Vector2(-moveForce, 0));//给角色添加力
            }
            else if (isRight)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x + xWalkSpeed, v.y, v.z);//左右移动是改变根的位置
            }
        }
    }
    void SkyLRwalk()//空中左右行走
    {
        if (isSkyNormalAction && !isRunJump)// && IsName(jumpName))//在空中以及不是跑着跳
        {
            if (isLeft)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x - xWalkSpeed * (1 + jumpXquiken), v.y, v.z);
            }
            else if (isRight)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x + xWalkSpeed * (1 + jumpXquiken), v.y, v.z);
            }
        }
    }
    void UDwalk()//上下行走
    {
        if (IsName(walkName) && isGroundNormalAction)//如果播放的是行走状态 并且是在地面
        {
            if (isUp)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y + yWalkSpeed, v.z);//上下移动是改变自身的位置
                //rigid.AddForce(new Vector2(-moveForce, 0));//给角色添加力
            }
            else if (isDown)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y - yWalkSpeed, v.z);//上下移动是改变自身的位置
            }
        }
    }
    void SkyUDwalk()//空中上下行走
    {
        if (isSkyNormalAction && !isRunJump)//&& IsName(jumpName))//空中正常活动，以及是跑的时候跳的
        {
            if (isUp)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y + yWalkSpeed * (1 + jumpYquiken), v.z);
            }
            else if (isDown)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y - yWalkSpeed * (1 + jumpYquiken), v.z);
            }
        }
    }
    void LRrun()//左右跑
    {
        if (IsName(runName) && isGroundNormalAction)//如果播放的是run状态 并且是在地面
        {
            if (isLeft)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x - xRunSpeed, v.y, v.z);//左右移动是改变根的位置
                //rigid.AddForce(new Vector2(-moveForce, 0));//给角色添加力
            }
            else if (isRight)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x + xRunSpeed, v.y, v.z);//左右移动是改变根的位置
            }
        }
    }
    void SkyLRrun()//空中左右跑
    {
        if (isSkyNormalAction && isRunJump)//&& IsName(jumpName))//以及跑着跳的
        {
            if (isLeft)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x - xRunSpeed * (1 + jumpXquiken), v.y, v.z);//跳跃加速
            }
            else if (isRight)
            {
                Vector3 v = root.position;
                root.position = new Vector3(v.x + xRunSpeed * (1 + jumpXquiken), v.y, v.z);
            }
        }
    }
    void UDrun()//上下跑
    {
        if (IsName(runName) && isGroundNormalAction)//如果播放的是行走状态 并且是在地面
        {
            if (isUp)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y + yRunSpeed, v.z);//上下移动是改变自身的位置
                //rigid.AddForce(new Vector2(-moveForce, 0));//给角色添加力
            }
            else if (isDown)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y - yRunSpeed, v.z);//上下移动是改变自身的位置
            }
        }
    }
    void SkyUDrun()//空中上下跑
    {
        if (isSkyNormalAction && isRunJump)// && IsName(jumpName))//如果空中活动以及跑着跳的
        {
            if (isUp)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y + yRunSpeed * (1 + jumpYquiken), v.z);
            }
            else if (isDown)
            {
                Vector3 v = transform.position;
                transform.position = new Vector3(v.x, v.y - yRunSpeed * (1 + jumpYquiken), v.z);
            }
        }
    }

    void CheckIsSkyNormalAction()//检测是否能在空中正常行动
    {
        isSkyNormalAction = false;
        if (!isGround && !isDeath)//如果不是在地面和不是死亡
        {
            isSkyNormalAction = true;
        }
    }
    void CheckIsGroundNormalAction()//检测是否能在地面正常行动
    {
        isGroundNormalAction = false;
        if (isGround && !isDeath && (IsName(idleName) || IsName(walkName) || IsName(runName)))//如果是在地面和不是死亡。以及走、跑、站立任意一种状态
        {
            isGroundNormalAction = true;
        }
    }
    void PlayerDeath()//角色死亡
    {
        if (!isDeath)//不是死亡的
        {
            isDeath = true;//设成死亡
            anim.Play(deathName);//播放死亡动画
            if (isRightSide)
                rigid.AddForce(new Vector2(-300f, 220f));
            else rigid.AddForce(new Vector2(300f, 220f));
            //enabled = false;//脚本不再触发
        }
    }
    void CheckUnmatched()//检测无敌··
    {
        if (unmatchedTime > 0.0f)
        {
            anim.SetFloat(changeColorTimeVarName, unmatchedTime);//设置改变颜色变量名的值
            unmatchedTime -= Time.deltaTime;
            //playState.unmatchedTime -= Time.deltaTime;
            //float color = transform.GetComponent<SpriteRenderer>().color.r;//获取其中一个颜色
            //color=color > 50 ? (color-35) :255;//三目运算。小于150就255.不然就-2
            //Debug.Log(transform.GetComponent<SpriteRenderer>().color);
            //transform.GetComponent<SpriteRenderer>().color = new Color(color, color, color,255);//设置玩家颜色
        }
        unmatchedTime = Mathf.Max(unmatchedTime, 0);//尽量不然无敌时间小于0
    }
    void AbilityTrigger(string abilityName)//帧触发接口/
    {
        colliderAssist.GetComponent<ColliderAssist>().TriggerCollider(abilityName);//开启碰撞，碰到的人将传进abilityName对应的类的函数里
    }
}
