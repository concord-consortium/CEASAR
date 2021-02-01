using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AvatarController : MonoBehaviour
{
    [SerializeField] float avatarScale = 1.0f;
    [SerializeField] Vector3 horizonSceneAvatarOffset;

    void Start()
    {
        this.transform.localScale = Vector3.one * avatarScale;
        if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_HORIZON)
        {
            this.transform.localPosition = Vector3.zero + horizonSceneAvatarOffset;
        }
    }

}
