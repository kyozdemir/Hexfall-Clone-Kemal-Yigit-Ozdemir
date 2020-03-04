using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public int x;
    public int y;
    public Color color;
    public BoardManager board;
    public Vector2 lerpPosition;
    public bool lerp;
    // Start is called before the first frame update
    void Start()
    {
        board = BoardManager.instance;
        lerp = false;
    }
    // For neighbor grid coordinates
    public struct NeighborHexes
    {
        public Vector2 up;
        public Vector2 upLeft;
        public Vector2 upRight;
        public Vector2 down;
        public Vector2 downLeft;
        public Vector2 downRight;
    }
    /* Builds a struct from grid position of neighbour hexagons and returns it */
    public NeighborHexes GetNeighbors()
    {
        NeighborHexes neighbours;
        

        //0,0 gibi özel yerlerde olan altıgenler için koşullar da eklendi
        neighbours.down = new Vector2(x, y-1);
        neighbours.up = new Vector2(x, y+1);
        neighbours.upLeft = new Vector2(x-1,y+1);
        neighbours.upRight = new Vector2(x == board.rows ? x : x+1, y == 0 ? y : y+1);
        neighbours.downLeft = new Vector2(x-1,y);
        neighbours.downRight = new Vector2(x+1,y);


        return neighbours;
    }
    // Update is called once per frame
    void Update()
    {
        if (lerp)
        {
            float newX = Mathf.Lerp(transform.position.x, lerpPosition.x, Time.deltaTime * 9);
            float newY = Mathf.Lerp(transform.position.y, lerpPosition.y, Time.deltaTime * 9);
            transform.position = new Vector2(newX, newY);


            if (Vector3.Distance(transform.position, lerpPosition) < 0.05f)
            {
                transform.position = lerpPosition;
                lerp = false;
            }
        }
    }
    // Rotating the hexagons
    public void Rotate(int newX, int newY, Vector2 newPos)
    {
        lerpPosition = newPos;
        SetX(newX);
        SetY(newY);
        lerp = true;
    }
    public void Exploded()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }
    public void ChangeWorldPosition(Vector2 newPosition)
    {
        lerpPosition = newPosition;
        lerp = true;
    }



    /* Set new grid position for hexagon */
    public void ChangeGridPosition(Vector2 newPosition)
    {
        x = (int)newPosition.x;
        y = (int)newPosition.y;
    }

    /* Setters & Getters */
    public void SetX(int value) { x = value; }
    public void SetY(int value) { y = value; }
    public void SetColor(Color newColor) { GetComponent<SpriteRenderer>().color = newColor; color = newColor; }
    

    public int GetX() { return x; }
    public int GetY() { return y; }
    public Color GetColor() { return GetComponent<SpriteRenderer>().color; }
    
}
