using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;

    [Header("移动参数")]
    public float speed = 8f;
    //下蹲的移动速率
    public float crouchSpeedDivisor = 3f;

    [Header("跳跃参数")]
    public float jumpForce = 6.3f;
    public float jumpHoldForce = 1.9f;//长按跳跃加成的力
    public float jumpHoldDuration = 0.1f;//长按跳跃的时间
    public float crouchJumpBoost = 2.5f;//下蹲跳跃的额外加成
    private float jumpTime;//跳跃时间

    [Header("状态")]
    public bool isCrouch;
    public bool isOnGround;
    public bool isJump;
    public bool isHeadBlocked;

    [Header("环境监测")]
    public float footOffset = 0.4f;
    public float headClearance = 0.5f;//头顶检测距离
    public float groundDistance = 0.2f;//与地面之间的距离
    public LayerMask groundLayer;

    //x轴力的方向
    private float xVelocity;

    //按键设置
    private bool jumpPressed;//单次跳跃
    private bool jumpHeld;//长按跳跃
    private bool crouchHeld;//长按下蹲




    //碰撞体尺寸
    private Vector2 colliderStandSize;
    private Vector2 colliderStandOffset;
    private Vector2 colliderCrouchSize;
    private Vector2 colliderCrouchOffset;





    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        colliderStandSize = coll.size;
        colliderStandOffset = coll.offset;
        colliderCrouchSize = new Vector2(coll.size.x, coll.size.y / 2f);
        colliderCrouchOffset = new Vector2(coll.offset.x, coll.offset.y / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
        crouchHeld = Input.GetButton("Crouch");
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        PhysicsCheck();
        GroundMovement();
        MidAirMovement();
    }

    //物理的环境判断
    void PhysicsCheck()
    {
        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, 0f), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, groundDistance, groundLayer);
        isOnGround = leftCheck || rightCheck ? true : false;

        RaycastHit2D headCheck = Raycast(new Vector2(0f, coll.size.y), Vector2.up, headClearance, groundLayer);
        isHeadBlocked = headCheck ? true : false;
    }

    void GroundMovement()
    {
        if (crouchHeld && !isCrouch && isOnGround)
            Crouch();
        else if (!crouchHeld && isCrouch && !isHeadBlocked)
            StanUp();
        else if (!isOnGround && isCrouch)
            StanUp();

        // -1~1之间的浮点型
        xVelocity = Input.GetAxis("Horizontal");

        if (isCrouch)
            xVelocity /= crouchSpeedDivisor;

        rb.velocity = new Vector2(xVelocity * speed, rb.velocity.y);
        FilpDirction();
    }

    //跳跃
    void MidAirMovement()
    {
        if (jumpPressed && isOnGround && !isJump)
        {
            if (isCrouch && isOnGround)
            {
                StanUp();
                rb.AddForce(new Vector2(0f, crouchJumpBoost), ForceMode2D.Impulse);
            }

            isOnGround = false;
            isJump = true;
            jumpTime = Time.time + jumpHoldDuration;
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        else if (isJump)
        {
            if (jumpHeld)
                rb.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);
            if (jumpTime < Time.time)
                isJump = false;
        }
    }

    //角色翻转
    void FilpDirction()
    {
        if (xVelocity < 0)
            transform.localScale = new Vector2(-1, 1);
        else if (xVelocity > 0)
            transform.localScale = new Vector2(1, 1);
    }

    //下蹲
    void Crouch()
    {
        isCrouch = true;
        coll.size = colliderCrouchSize;
        coll.offset = colliderCrouchOffset;
    }

    //站立
    void StanUp()
    {
        isCrouch = false;
        coll.size = colliderStandSize;
        coll.offset = colliderStandOffset;
    }

    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDiraction, float length, LayerMask layer)
    {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDiraction, length, layer);

        Color color = hit ? Color.red : Color.green;

        Debug.DrawRay(pos + offset, rayDiraction * length, color);
        return hit;
    }
}
