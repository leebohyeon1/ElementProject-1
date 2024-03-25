using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum projectile
{
    Catapulit
}

public class Projectile : NetworkBehaviour
{
    public PlayerMove playerMove;
    Rigidbody rb;
    NetworkObject trGameObject;
    [SerializeField]projectile projectile;
    public Vector3 Target;
    public float Speed;
    public float etc;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Target = playerMove.SimpleAttackPosition.position - playerMove.transform.position;
        switch (projectile)
        {
            case projectile.Catapulit:
                rb.AddForce(Target.normalized * Speed , ForceMode.Impulse);
                break;
        }

        
    }

    // Update is called once per frame
    void Update()
    {
      
    }
    private void OnCollisionEnter(Collision collision)
    {
        switch (projectile)
        {
            case projectile.Catapulit:
                if(collision.gameObject.CompareTag("Player"))
                {
                    Destroy(gameObject);
                    PlayerMove playerMove = collision.gameObject.GetComponent<PlayerMove>();
                    if (playerMove != null )
                    {
                        playerMove.TakeDamage();
                    }
                }
                if (etc != 0)
                {
                    ContactPoint cp = collision.GetContact(0);
                    Vector3 dir = transform.position - cp.point;
                    rb.AddForce((dir).normalized * Speed, ForceMode.Impulse);
                    etc--;
                }
                else
                {
                   Destroy(gameObject);
                }
                break;
        }
    }
}
