using UnityEngine;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    public delegate void TouchEventHandler(Vector2 swipe);

    public static event TouchEventHandler SwipeEvent;
    public static event TouchEventHandler SwipeEndEvent;

    private Vector2 touchMovement;

    private int minSwipeDistance = 20;

    private void OnSwipe()
    {
        if (SwipeEvent != null)
            SwipeEvent(touchMovement);
    }

    private void OnSwipeEnd()
    {
        if (SwipeEndEvent != null)
            SwipeEndEvent(touchMovement);
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            if (touch.phase == TouchPhase.Began)
            {
                touchMovement = Vector2.zero;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                touchMovement += touch.deltaPosition;

                if (touchMovement.magnitude > minSwipeDistance)
                    OnSwipe();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                OnSwipeEnd();
            }
        }
    }
}
