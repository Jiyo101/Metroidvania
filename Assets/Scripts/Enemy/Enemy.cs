using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;

    // Start is called before the first frame update
    protected virtual void Start() // VIRTUAL se usa para modificar la declaracion de un metodo y permitir que se invalide en una clase derivada
    {
        
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.instance;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }

        if(isRecoiling )
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if(!isRecoiling )
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    protected virtual void Attack() // funcion para que el player reciba daño por el ataque del enemigo
    {
        PlayerController.instance.TakeDamage(damage);
    }

    protected void OnTriggerStay2D(Collider2D _other) // si el enemigo tiene contacto con el collider del jugador se activara lo que haya dentro
    {
        if (_other.CompareTag("Player") && !PlayerController.instance.pState.invincible)
        {
            Attack();
        }
    }
}
