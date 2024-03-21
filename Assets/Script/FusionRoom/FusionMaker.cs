using HeoWeb.Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionMaker : MonoBehaviour
{
    public GameObject FusionManagerPrefab;

    private void Start()
    {
        // FusionConnection ��ũ��Ʈ�� ã���ϴ�.
        FusionConnection fusionConnection = FindObjectOfType<FusionConnection>();

        // FusionConnection�� ���ٸ� FusionManager�� �����մϴ�.
        if (fusionConnection == null)
        {
            CreateFusionManager();
        }
    }

    // FusionManager�� �����ϴ� �Լ�
    private void CreateFusionManager()
    {
        Debug.Log("ǻ�� �Ŵ��� ����");
        // FusionManagerPrefab�� �̿��Ͽ� FusionManager ������Ʈ�� �����մϴ�.
        GameObject fusionManager = Instantiate(FusionManagerPrefab);
    }
}
