using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FollowMouse : MonoBehaviour
{
	public RectTransform myTrans;
	// Start is called before the first frame update
	private void Start()
	{
		Cursor.visible = false;
	}
	// Update is called once per frame
	void Update()
    {
		myTrans.position = Input.mousePosition;
    }
}
