using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursor_normal;
    public Texture2D cursor_OnButton;

    public Vector2 normalCursorHotSpot;
    public Vector2 buttonCursorHotSpot;

    void Start()
    {
        Cursor.SetCursor(cursor_normal, normalCursorHotSpot, CursorMode.Auto);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Cursor.SetCursor(cursor_normal, normalCursorHotSpot, CursorMode.Auto);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Cursor.SetCursor(cursor_OnButton, buttonCursorHotSpot, CursorMode.Auto);
        }
    }

    public void OnButtonCursorEnter()
    {
        Cursor.SetCursor(cursor_OnButton, buttonCursorHotSpot, CursorMode.Auto);

    }

    public void OnButtonCursorExit()
    {
        Cursor.SetCursor(cursor_normal, normalCursorHotSpot, CursorMode.Auto);

    }

}


