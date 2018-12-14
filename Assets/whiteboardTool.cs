using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class whiteboardTool : MonoBehaviour {

    private int textureSize = 2048;
    private int drawSize = 10;
    private Texture2D texture;
    private Color[] color;
    public bool isDrawing;

    private bool touchingLast;
    private float posX, posY;
    private float lastX, lastY;

    public void ToggleDrawing(bool isDrawing) {
        this.isDrawing = isDrawing;
    }

    public void SetDrawPos(float x, float y) {
        this.posX = x;
        this.posY = y;
    }

    public void SetColor(Color color) {
        this.color = Enumerable.Repeat<Color>(color, drawSize * drawSize).ToArray<Color>();
    }

	// Use this for initialization
	void Start () {
        Renderer renderer = GetComponent<Renderer>();
        this.texture = new Texture2D(textureSize, textureSize);
        renderer.material.mainTexture = this.texture;
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(posX + " , " + posY);
        int x = (int)(posX * textureSize - (drawSize / 2));
        int y = (int)(posY * textureSize - (drawSize / 2));
        if(touchingLast) {
            print("Drawing pixels.. at" + x  +", " + y);
            texture.SetPixels(x, y, drawSize, drawSize, color);
            texture.Apply();
        }
        this.lastX = (float)x;
        this.lastY = (float)y;
        this.touchingLast = isDrawing;
    }
}
