using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        rb.gravityScale = 12f;
    }

    // Update is called once per frame
    protected override void Update() // OVERRIDE sirve para ampliar o modificar la implementación abstracta o virtual de un método
    {
        base.Update(); // esto llamara a la funcion UPDATE de Enemy que es la clase base
        FollowPlayer();
    }

    protected override void Awake()
    {
        base.Awake(); // esto llamara a la funcion AWAKE de Enemy que es la clase base
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }

    public void FollowPlayer() // FUNCION PARA QUE SIGA AL JUGADOR CUANDO NO ESTE EN RECOIL
    {
        if (!isRecoiling) 
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(PlayerController.instance.transform.position.x, PlayerController.instance.transform.position.y), speed * Time.deltaTime);
        }
    }
}
