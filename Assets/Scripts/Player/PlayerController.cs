using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings:")]
    [SerializeField] private float walkSpeed = 1.0f; //SerializeField se usa para poder modificar una variable desde el editor aun siendo esta privada
    
    [Space(5)]

    [Header("Vertical Movemetn Setting:")]
    [SerializeField] private float jumpForce = 45.0f;
    private int jumpBufferCounter = 0; //los buffers sirven para que el jugador pueda poner la accion de saltar a la cola antes de que haya acabado la accion anterio de saltar
    [SerializeField] private int jumpBufferFrames;
    private float coyoteTimeCounter = 0; //el coyoteTime sirve para que el jugador tenga un tiempo limitado para saltar cuando no esta Grounded
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    
    [Space(5)]

    [Header("GroundCheck Settings:")]
    [SerializeField] Transform groundCheck;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] LayerMask whatIsGround;
    
    [Space(5)]

    [Header("Dash Settings:")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] GameObject dashEffect;
    private float gravity;
    private bool canDash = true;
    private bool dashed; // para solo poder dashear una vez en el aire

    [Space(5)]

    [Header("Attack Settings:")]
    [SerializeField] Transform sideAttackTransform;
    [SerializeField] Transform upAttackTransform;
    [SerializeField] Transform downAttackTransform;
    [SerializeField] Vector2 sideAttackArea;
    [SerializeField] Vector2 upAttackArea;
    [SerializeField] Vector2 downAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [SerializeField] GameObject slashEffect;
    bool attack = false;
    float attackCooldown, timeSinceAttack;

    [Space(5)]

    [Header("Recoil Settings:")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;

    [Space(5)]

    [Header("Health Settings:")]
    public int health;
    public int maxHealth;

    [Space(5)]

    private float xAxis;
    private float yAxis;
    private Rigidbody2D rb;
    Animator anim;
    public static PlayerController instance;
    public PlayerStateList pState;


    private void Awake()
    {
        health = maxHealth;

        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos() //SIRVE PARA VER LAS AREAS DE ATAQUE CLARAMENTE EN LA ESCENA
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sideAttackTransform.position, sideAttackArea);
        Gizmos.DrawWireCube(upAttackTransform.position, upAttackArea);
        Gizmos.DrawWireCube(downAttackTransform.position, downAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();

        if(pState.dashing) return;// esto se hace para que el dash corte todo movimiento

        Move();
        Jump();
        Flip();
        StartDash();
        Attack();
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Fire1");
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    void Move()
    {
        rb.velocity = new Vector2 (walkSpeed *  xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if(Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);

        if (Grounded()) Instantiate(dashEffect, transform); // agregamos el efecto de dash

        yield return new WaitForSeconds(dashTime);
        
        rb.gravityScale = gravity;
        pState.dashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        for(int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }

    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }
        if (pState.recoilingY)
        {
            rb.gravityScale = 0;

            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }

            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //parar el recoil en el eje X
        if(pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        //parar el recoil en el eje Y
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(attack && timeSinceAttack >= attackCooldown)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
            
            if(yAxis == 0 || yAxis < 0 && Grounded()) //atacar hacia delante o atras
            {
                Hit(sideAttackTransform, sideAttackArea, ref pState.recoilingX, recoilXSpeed);
                Instantiate(slashEffect, sideAttackTransform);
            }
            else if(yAxis > 0) //atacar hacia arriba
            {
                Hit(upAttackTransform, upAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 80, upAttackTransform);
            }
            else if (yAxis < 0 && !Grounded()) //atacar hacia abajo
            {
                Hit(downAttackTransform, downAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -100, downAttackTransform);
            }
        }
    }


    public bool Grounded()
    {
        //Raycast se usa para "lanzar" un rayo que detecta varias cosas (posicion desde donde se lanza (origen), hacia donde (direccion), cuanto grande es el rayo (cuanto viaja), lo que tiene que detectar)
        if (Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckY, whatIsGround)  
            || Physics2D.Raycast(groundCheck.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheck.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0) //Esto permitira que el jugador cancele el salto cuando suelte el boton de saltar
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0) // se usa el jumpbuffer y el coyote time para ofrecer una mejor jugabilidad
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);

                pState.jumping = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;

                airJumpCounter++;

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }

    void ClampHealth() // funcion para que la vida este entre el maximo y el minimo
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void TakeDamage(float _damage) // funcion para recibir daño
    {
        health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage() // corrutina para que el enemigo no haga daño infinito
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        ClampHealth();
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }
}
