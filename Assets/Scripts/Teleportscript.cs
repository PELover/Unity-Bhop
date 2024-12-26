using UnityEngine;

public class Teleportscript : MonoBehaviour
{
    [SerializeField] GameObject tele;
    [SerializeField] GameObject player;
    [SerializeField] float timecount = 0f;
    [SerializeField] bool isTouchground = false;
    [SerializeField] float settime = 0f; // bhop platform = 0.1f , end = 0f;

   private void Update()
    {
        if(isTouchground && timecount == 0)
        {
            timecount = Time.time;
        }
        if (Time.time - settime >= timecount && timecount != 0)
        {            
            player.transform.position = tele.transform.position;
        }
        if (!isTouchground)
        {
            timecount = 0;
        }  
    }
    private void OnCollisionEnter(Collision collision)
    {       
        if(collision.gameObject.tag.Equals("Player"))
        {
            isTouchground = true;          
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            isTouchground = false;

        }
    }
}