using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Lulz : MonoBehaviour
{
    public Transform player;
    public RectTransform obj, btn;
    bool btnTrigger = false;
    private void OnTriggerEnter(Collider other) {
        if(other.tag == gameObject.tag){
            LeanTween.moveY(obj,-830,8).setEaseInOutBounce();
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(player.position.y < -2 && !btnTrigger){
            btnTrigger = true;
            LeanTween.rotate(btn,720,2);
            LeanTween.moveY(btn,0,2);
        }
    }

    public void Restart(){
        SceneManager.LoadScene(0);
    }
}
