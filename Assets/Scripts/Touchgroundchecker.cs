using UnityEngine;



    public class Touchgroundchecker : MonoBehaviour
{
        public bool istouchgroundchecker = false;
    private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals("Ground"))
            {
                istouchgroundchecker = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag.Equals("Ground"))
            {
                istouchgroundchecker = false;
            }
        }


    }
