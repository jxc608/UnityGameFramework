using UnityEngine;
using System.Collections;

public class InputUtils : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Input.multiTouchEnabled = false;
    }

    public static bool MouseHeld;
	public static Vector3 MouseLastPosition;
	public static Vector3 MouseDeltaPosition;

    private static bool FakeOnPress;
    private static Vector3 FakeMousePos;

	void Update()
	{
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
				if (OnPressed()) {
					MouseHeld = true;
					MouseDeltaPosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
					MouseLastPosition = Input.mousePosition;
				} else if (OnHeld()) {
					MouseHeld = true;
					MouseDeltaPosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
				} else if (OnReleased()) {
					MouseHeld = false;
				}
				break;
		}
        if (FakeOnPress)
        {
            StartCoroutine(SetFakeOnPressFalse());
        }
	}

    IEnumerator SetFakeOnPressFalse()
    {
        yield return new WaitForEndOfFrame();
        FakeOnPress = false;
    }

    public static void SetFakeOnPress(Vector3 pos)
    {
        FakeOnPress = true;
        FakeMousePos = pos;
    }

	public static bool OnHeld(int index = 0)
	{
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
				return Input.GetMouseButton(index);
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				return Input.touchCount > index && (Input.GetTouch(index).phase == TouchPhase.Moved || Input.GetTouch(index).phase == TouchPhase.Stationary);
		}

		return false;
	}

	public static bool OnPressed(int index = 0)
	{
        if (FakeOnPress)
            return true;
        switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
				return Input.GetMouseButtonDown(index);
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				return Input.touchCount > index && Input.GetTouch(index).phase == TouchPhase.Began;
		}

		return false;
	}

	public static bool OnReleased(int index = 0)
	{
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
				return Input.GetMouseButtonUp(index);
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				return Input.touchCount > index && (Input.GetTouch(index).phase == TouchPhase.Ended || Input.GetTouch(index).phase == TouchPhase.Canceled);
		}

		return false;
	}

	public static Vector2 GetTouchPosition(int index = 0)
	{
        if (FakeOnPress)
            return FakeMousePos;
        
        switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
				return Input.mousePosition;
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				if (index < Input.touchCount)
					return Input.GetTouch(index).position;
				break;
		}

		return Vector2.zero;
	}

	public static Vector2 GetTouchDeltaPosition(int index = 0)
	{
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
				if (MouseHeld)
					return MouseDeltaPosition;
				return Vector2.zero;
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
				if (index < Input.touchCount)
					return Input.GetTouch(index).deltaPosition;
				return Vector2.zero;
		}

		return Vector2.zero;
	}
}
