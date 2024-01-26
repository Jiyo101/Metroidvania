using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // esto coge la longitud del primer clip de la animacion y destruye el GameObject tras la animacion
        Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);   
    }
}
