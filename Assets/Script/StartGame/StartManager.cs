using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using HeoWeb.Fusion;

public class StartManager : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner; 
    public static StartManager instance; 
    public GameObject GameManager; 

    public bool readyPlayer1 = false;
    public bool readyPlayer2 = false;

    private float gameStartTime = 0f;
    private bool gameStarted = false;

    public GameObject playerObject_1;
    public GameObject playerObject_2;
    public PlayerStats playerStats_1;
    public PlayerStats playerStats_2;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            runner = FindObjectOfType<NetworkRunner>();
        }
        else if (instance != this) 
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // ������ ���۵��� �ʾҰ�, �÷��̾� 1�� 2�� ��� �غ�Ǿ��ٸ�
        if (!gameStarted && readyPlayer1 && readyPlayer2)
        {
            // gameStartTime�� 0�̸� ���� �ð����� ����
            if (gameStartTime == 0f)
            {
                gameStartTime = Time.time;
            }
            // 3�ʰ� �����ٸ�
            else if (Time.time - gameStartTime >= 3f)
            {
                FusionConnection.instance.playerObject_1 = playerObject_1;
                FusionConnection.instance.playerObject_2 = playerObject_2;
                FusionConnection.instance.playerStats_1 = playerStats_1;
                FusionConnection.instance.playerStats_2 = playerStats_2;

                FusionConnection.instance.FirstStart();

                gameStarted = true;

                Destroy(gameObject);


                // NetworkObject gameManager = runner.Spawn(GameManager, Vector3.zero);
                // GameManager game = gameManager.GetComponent<GameManager>();

                //game.playerObject_1 = playerObject_1;
                //game.playerObject_2 = playerObject_2;
                //game.playerStats_1 = playerStats_1;
                //game.playerStats_2 = playerStats_2;
            }
        }
        // �÷��̾� 1 �Ǵ� �÷��̾� 2�� �غ���� ���� ��� gameStartTime �ʱ�ȭ
        else
        {
            gameStartTime = 0f;
        }
    }
}
