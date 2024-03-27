using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{ 
    private bool bTrigger = false;

    private void Awake()
    {
        Destroy(gameObject,0.05f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        // �浹�� ��ü�� "Player" �±׸� ������ �ִ��� Ȯ��
        if (other.CompareTag("Player"))
        {
            
            // �浹�� ��ü�� PlayerMove ��ũ��Ʈ ������Ʈ ��������
            PlayerMove playerMove = other.GetComponent<PlayerMove>();

            // PlayerMove ��ũ��Ʈ�� �����Ѵٸ� TakeDamage �Լ� ȣ��
            if (playerMove != null || !bTrigger)
            {
                bTrigger = true;
                playerMove.TakeDamage(1);
            }
        }
    }
}
