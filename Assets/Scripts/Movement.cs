using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Movement : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject cam;
    //[SerializeField] Text showvelospeed;
    [SerializeField] GameObject groundcheckobj;
    GameObject touchchecker;
    Touchgroundchecker thistouchchecker;
    

    [Header("Character State")]
    [SerializeField] string positionstate = "";
    [SerializeField] bool isGrounded = false;
    [SerializeField] bool isDuck = false;
    [SerializeField] bool isLadder = false;

    [Header("In Air Controls")]
    [SerializeField] float MaxAirSpeed = 3.2f;
    [SerializeField] float AirStrafeForce = 20f;
    [SerializeField]float jumpforce = 7.15f;
    Vector3 wishdir = Vector3.zero;
    float ADkey;
    float WSkey;

    [Header("On Ground Control")]
    [SerializeField] bool frictioninclude = false;
    [SerializeField] float MaxGroundSpeed = 11.46f;
    [SerializeField] float friction = 12f;

    [Header("On Ladder Controls")]
    [SerializeField] float climbforce = 10f;
    float turn;
    private Collision ladderCollider;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {     
        GetDuck();
        feetscheck();
        GetJump();
        Vector3 vel = new Vector3(rb.linearVelocity.x,0f,rb.linearVelocity.z);
      //  showvelospeed.text = vel.magnitude.ToString("0.0");
        Friction();        
    }

    void FixedUpdate()
    {
        if(isLadder)
        {
            positionstate = "Ladder";
        }
        if (isGrounded)
        {
            if (isLadder)
            {
                positionstate = "Ladder";
            }
            if (!isLadder)
            {
                positionstate = "Ground";
            }        
        }
        else if (!isGrounded)
        {
            if (isLadder)
            {
                positionstate = "Ladder";
            }
            if (!isLadder)
            { 
                positionstate = "Air"; 
            }          
        }
        this.transform.rotation = new Quaternion(0f, cam.transform.rotation.y, 0f, cam.transform.rotation.w);               
        ADkey = Input.GetAxis("Horizontal");
        WSkey = Input.GetAxis("Vertical");
        wishdir = transform.right * ADkey + transform.forward * WSkey; // นำค่าไปเก็บที่แกน X แกน Z ตามการหันหน้าของกล้อง , align Wish move direction and View direction
        switch (positionstate)
        {
            case "Ground":
                GroundAccelerate(wishdir);
                break;
            case "Air": 
                Airacceletrate(wishdir.normalized);
                break;
            case "Ladder":
                LadderMove();
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag.Equals("Ladder"))
        {
            isLadder = true;
            rb.useGravity= false;
            rb.linearVelocity = Vector3.zero;
            ladderCollider = collision;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag.Equals("Ladder"))
        {
            isLadder = false;
            rb.useGravity = true;
            ladderCollider = null;
        }
    }

    void Friction()
    {
        if (rb.linearVelocity.y < 0.15f)
        {
            frictioninclude = true;
        }
        if(rb.linearVelocity.y > 0.2f)
        {
            frictioninclude = false;
        }
    }

    void feetscheck()
    {
        if (thistouchchecker == null)
        {
            thistouchchecker = gameObject.AddComponent<Touchgroundchecker>();
        }
        isGrounded = thistouchchecker.istouchgroundchecker;
    }

    void GetJump()
    {
        if (isGrounded && (Input.GetButton("Jump") || Input.GetAxis("Mouse ScrollWheel") < 0f))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpforce, rb.linearVelocity.z);          
        }
    }

    void GetDuck()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isDuck = true;
            this.transform.localScale = new Vector3(1.37f, 1.03f, 1.37f);       
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isDuck = false;
            this.transform.localScale = new Vector3(1.37f, 1.37f, 1.37f);          
        }
    }

    void Airacceletrate(Vector3 wishdir)
    {
        rb.linearDamping = 0f;
        /*                      
         view
         ^
         |           v
         |          /|
         |         / |
         |        /  |
         |       /   |
         |      /    |
         |     /     |
         |    /      |
         |   /       |
         |  /        |
         | /       __|
         |/___>___|__|________> wishdir
         |---projv---|
        */

        Vector3 callinearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 projv = Vector3.Project(callinearVelocity, wishdir);

        /*                      
                                view
                                ^
                                |           v
                                |          /|
                                |         / |
                                |        /  |
                                |       /   |
                                |      /    |
                                |     /     |
                                |    /      |
                                |   /       |
                                |  /        |
                                | /       __|
            wishdir <___________|/__>____|__|
                                |---projv---|
       */

        bool isaway = Vector3.Dot(wishdir, projv) <= 0f; // ตรวจทิศทางที่ไปตรงหรือกำลังห่างออกไปจาก projection , check if it moving away or moving toward from the projection vel
        // เพิ่มความเร็วให้ถ้า length ของ projection ยังน้อยกว่า maxspeed หรือ หันออกจาก projection
        if (isaway || projv.magnitude < MaxAirSpeed)
        {
            Vector3 applyforce = wishdir * AirStrafeForce; //คูณทิศทางที่ต้องการไปกับ Force , scale dir with airstrafeforce

            // จำกัดการเร่งไม่ให้เร่งเกิน maxspeed
            if (!isaway)
            {
                applyforce = Vector3.ClampMagnitude(applyforce, MaxAirSpeed - projv.magnitude);
            }
            else
            {
                applyforce = Vector3.ClampMagnitude(applyforce, MaxAirSpeed + projv.magnitude);
            }
            rb.AddForce(applyforce, ForceMode.VelocityChange);
        }
    }

    void GroundAccelerate(Vector3 wishdir)
    {
        if (!isDuck)
        {
            rb.AddForce(wishdir * MaxGroundSpeed * 10f, ForceMode.Force);
        }
        if(isDuck)
        {
            rb.AddForce(wishdir * (MaxGroundSpeed*50/100) * 10f, ForceMode.Force);
        }
        if (frictioninclude)
        {
            rb.linearDamping = friction;
        }
        else
        { 
            rb.linearDamping = 0f;
        }
    }
    
    void LadderMove()
    {
        rb.linearVelocity = Vector3.zero;
        Vector3 forward = cam.transform.forward;
        forward.z = 0f;
        forward.Normalize();
        rb.AddForce(forward * WSkey * climbforce, ForceMode.VelocityChange);
        Vector3 sideway = this.transform.right;
        sideway.z = 0f;
        sideway.Normalize();
        Vector3 ladderDirection = ladderCollider.transform.forward;
        ladderDirection *= 180f;
        if(cam.transform.rotation.eulerAngles.y >= 180f)
        {
            turn = -1f;
        }
        if (cam.transform.rotation.eulerAngles.y < 180f)
        {
            turn = 1f;
        }
        if (Mathf.Abs(ladderDirection.z)  !<= 165f)
        {
            rb.AddForce(this.transform.up * ADkey * climbforce * turn, ForceMode.VelocityChange);
        }
        if(Mathf.Abs(ladderDirection.z) > 165f)
        {
            rb.AddForce(sideway * ADkey * climbforce, ForceMode.VelocityChange);            
        }
    }
}