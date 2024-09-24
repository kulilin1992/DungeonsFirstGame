using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;

    [SerializeField] private Transform cursorTarget;

    private void Awake()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
    }
    // Start is called before the first frame update
    void Start()
    {
        SetCinemachineTargetGroup();
    }

    private void SetCinemachineTargetGroup()
    {
        CinemachineTargetGroup.Target target_player = new CinemachineTargetGroup.Target { weight = 1, radius = 2.5f,
            target = GameManager.Instance.GetPlayer().transform };

        CinemachineTargetGroup.Target target_cursor = new CinemachineTargetGroup.Target { weight = 1, radius = 1f,
            target = cursorTarget};

        CinemachineTargetGroup.Target[] cinemachineTargets = new CinemachineTargetGroup.Target[] { target_player, target_cursor};

        targetGroup.m_Targets = cinemachineTargets;
    }

    private void Update()
    {
        if (cursorTarget != null)
        {
            cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
        }
    }
}
