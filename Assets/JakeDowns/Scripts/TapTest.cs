using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
   
    public class TapTest : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    int tap;
    float interval = 0.35f;
    bool readyForDoubleTap;
    bool _didDoubleTap = false;
    
    public GameObject receiver;

    /*public class Payload {
        public string objectName;
        public int tapCount;
        public Payload(string objectName, int tapCount)
        {
            this.objectName = objectName;
        }
    }*/
    public void OnPointerClick(PointerEventData eventData)
    {
        tap ++;

        Debug.Log($"onPointerClick {tap}");

        if (tap == 1)
        {
            _didDoubleTap = false;
            StartCoroutine(DoubleTapInterval() );
            receiver.SendMessage("OnSingleTap", gameObject.name);
        }
 
        else if (tap>1 && readyForDoubleTap)
        {
            _didDoubleTap = true;
            receiver.SendMessage("OnDoubleTap", gameObject.name);

            tap = 0;
            readyForDoubleTap = false;
        }
    }
 
    IEnumerator DoubleTapInterval()
    {  
        yield return new WaitForSeconds(interval);
        //Debug.Log($"did double tap? {_didDoubleTap}");
        readyForDoubleTap = true;
        tap = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("onPointerDown");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("onPointerUp");
    }
}