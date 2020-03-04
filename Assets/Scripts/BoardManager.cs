
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*Anlamak için baştan yazmam gerekiyordu kodu. Artık match 3 işine baya hakimim. Üzerine düşeceğim. */
public class BoardManager : Constants
{
    public static BoardManager instance = null;

    [SerializeField]
    Text scoreText, moveText;
    

    public GameObject hexagonPrefab;
    public List<Color> colorList;
    public List<List<Hexagon>> grid;
    public Transform hexPool;
    
    List<Hexagon> selectedGroup;
    Hexagon selectedHex; 
    public int rows;
    public int cols;
    Vector2 selectedPosition;
    Vector2 startPos;
    int neighborSelection;
    int selectionCount = 6;
    private bool hexGenerating;
    private bool isExploding;
    private bool isRotating;
    private bool gameOver;

    int blownHex;
    int moves;
    //Singleton. Every new call means a new instance.
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        blownHex = 0;
        moves = 0;
        scoreText.text = "0";
        moveText.text = "0";
        gameOver = false;
        isRotating = false;
        hexGenerating = false;
        selectedGroup = new List<Hexagon>();
        grid = new List<List<Hexagon>>();
    }

    private void InitializeGrid()
    {
        List<int> emptyCells = new List<int>();
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                emptyCells.Add(i);
            }
            grid.Add(new List<Hexagon>());
        }
        StartCoroutine(GenerateHexagons(emptyCells, ColoringTheBoard()));
    }
    IEnumerator GenerateHexagons(List<int> columns, List<List<Color>> colorSeed = null)
    {
        Vector3 startPosition;
        float positionX, positionY;
        float startX = GetGridStartCoordinateX();
        foreach (int i in columns)
        {
            positionX = startX + ((HEX_DISTANCE_HORIZONTAL) * i);
            positionY = ((HEX_DISTANCE_VERTICAL) * grid[i].Count * 2) + GRID_VERTICAL_OFFSET + (i % 2 == 0 ? HEX_DISTANCE_VERTICAL : ZERO);
            startPosition = new Vector3(positionX, positionY, ZERO);

            GameObject newHex = Instantiate(hexagonPrefab, HEX_START_POSITION, Quaternion.identity, hexPool);
            Hexagon hex = newHex.GetComponent<Hexagon>();

            if (colorSeed == null)
                hex.SetColor(colorList[(int)(Random.value * RANDOM_SEED) % colorList.Count]);
            else
                hex.SetColor(colorSeed[i][grid[i].Count]);

            yield return new WaitForSeconds(DELAY_TO_PRODUCE_HEXAGON);
            hex.ChangeGridPosition(new Vector2(i, grid[i].Count));
            hex.ChangeWorldPosition(startPosition);
            grid[i].Add(hex);
        }

    }
    private List<List<Color>> ColoringTheBoard()
    {
        List<List<Color>> returnValue = new List<List<Color>>();
        List<Color> checkList = new List<Color>();
        bool exit = true;


        /* Creating a color list without ready to explode neighbours */
        for (int i = 0; i < cols; ++i)
        {
            returnValue.Add(new List<Color>());
            for (int j = 0; j < rows; ++j)
            {
                returnValue[i].Add(colorList[(int)(Random.value * RANDOM_SEED) % colorList.Count]);
                do
                {
                    exit = true;
                    returnValue[i][j] = colorList[(int)(Random.value * RANDOM_SEED) % colorList.Count];
                    if (i - 1 >= 0 && j - 1 >= 0)
                    {
                        if (returnValue[i][j - 1] == returnValue[i][j] || returnValue[i - 1][j] == returnValue[i][j])
                            exit = false;
                    }
                } while (!exit);
            }
        }


        return returnValue;
    }
    //Hexagonları düzgün biçimde sıralıyorum iç içe
    /*private Vector3 CalculateWorldPos(Vector2 gridPos)
    {
        float offset = 0f;
        if (gridPos.x % 2 != 0)
        {
            offset = hexagonPrefab.transform.localScale.x / 2;
        }
        float y = startPos.y + gridPos.y * hexagonPrefab.transform.localScale.x + offset;
        float x = -(startPos.x - gridPos.x * hexagonPrefab.transform.localScale.y * 0.75f);
        return new Vector2(x, y);
    }*/

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(Collider2D collider)
    {
        if (selectedHex == null || !selectedHex.GetComponent<Collider2D>().Equals(collider))
        {
            selectedHex = collider.gameObject.GetComponent<Hexagon>();
            selectedPosition.x = selectedHex.GetX();
            selectedPosition.y = selectedHex.GetY();
            neighborSelection = 0;
        }
        else
        {
            neighborSelection = (++neighborSelection) % selectionCount;
        }
        FindHexagonGroup();
        //Shader Programlamayı denedim ama tam beceremedim. Üzerine düşeceğim.
    }
    private void FindHexagonGroup()
    {
        List<Hexagon> returnValue = new List<Hexagon>();
        Vector2 firstPos, secondPos;

        /* Finding 2 other required hexagon coordinates on grid */
        selectedHex = grid[(int)selectedPosition.x][(int)selectedPosition.y];
        FindOtherHexagons(out firstPos, out secondPos);
        selectedGroup.Clear();
        selectedGroup.Add(selectedHex);
        selectedGroup.Add(grid[(int)firstPos.x][(int)firstPos.y].GetComponent<Hexagon>());
        selectedGroup.Add(grid[(int)secondPos.x][(int)secondPos.y].GetComponent<Hexagon>());
        foreach (var item in selectedGroup)
        {
            item.GetComponent<SpriteOutline>().isOutlineActive = true;
            item.GetComponent<SpriteOutline>().UpdateOutline(true);
        }
    }
    private void FindOtherHexagons(out Vector2 first, out Vector2 second)
    {
        Hexagon.NeighborHexes neighbours = selectedHex.GetNeighbors();
        bool breakLoop = false;


        /* Picking correct neighbour according to selection position */
        do
        {
            switch (neighborSelection)
            {
                case 0: first = neighbours.up; second = neighbours.upRight; break;
                case 1: first = neighbours.upRight; second = neighbours.downRight; break;
                case 2: first = neighbours.downRight; second = neighbours.down; break;
                case 3: first = neighbours.down; second = neighbours.downLeft; break;
                case 4: first = neighbours.downLeft; second = neighbours.upLeft; break;
                case 5: first = neighbours.upLeft; second = neighbours.up; break;
                default: first = Vector2.zero; second = Vector2.zero; break;
            }

            /* Loop until two neighbours with valid positions are found */
            if (first.x < 0 || first.x >= cols || first.y < 0 || first.y >= rows || second.x < 0 || second.x >= cols || second.y < 0 || second.y >= rows)
            {
                neighborSelection = (++neighborSelection) % 6;
            }
            else
            {
                breakLoop = true;
            }
        } while (!breakLoop);
    }
    public void Rotate(bool clockWise)
    {
        /* Specifying that rotation started and destroying outliner*/
        
        StartCoroutine(RotationCheckCoroutine(clockWise));
    }
    private IEnumerator RotationCheckCoroutine(bool clockWise)
    {
        List<Hexagon> explosiveHexagons = null;
        bool flag = true;


        /* Rotate selected group until an explosive hexagon found or maximum rotation reached */
       isRotating = true;
        for (int i = 0; i < selectedGroup.Count; ++i)
        {
            /* Swap hexagons and wait until they are completed rotation */
            SwapHexagons(clockWise);
            yield return new WaitForSeconds(0.3f);

            /* Check if there is any explosion available, break loop if it is */
           explosiveHexagons = CheckExplosion(grid);
            if (explosiveHexagons.Count > 0)
            {
                break;
            }
        }


        /* Indicate that rotation has ended and explosion starts */
        isExploding = true;
        isRotating = false;


        /* Explode the hexagons until no explosive hexagons are available */
        while (explosiveHexagons.Count > 0)
        {
            if (flag)
            {
               hexGenerating = true;
                ExplodeHex(explosiveHexagons);
                StartCoroutine(GenerateHexagons(ExplodeHex(explosiveHexagons)));
                flag = false;
            }

            else if (!hexGenerating)
            {
                explosiveHexagons = CheckExplosion(grid);
                flag = true;
            }

            yield return new WaitForSeconds(0.3f);
        }

        isExploding = false;
        
        
        FindHexagonGroup();
    }
    private void SwapHexagons(bool clockWise)
    {
        int x1, x2, x3, y1, y2, y3;
        Vector2 pos1, pos2, pos3;
        Hexagon first, second, third;



        /* Taking each position to local variables to prevent data loss during rotation */
        first = selectedGroup[0];
        second = selectedGroup[1];
        third = selectedGroup[2];



        x1 = first.GetX();
        x2 = second.GetX();
        x3 = third.GetX();

        y1 = first.GetY();
        y2 = second.GetY();
        y3 = third.GetY();

        pos1 = first.transform.position;
        pos2 = second.transform.position;
        pos3 = third.transform.position;


        /* If rotation is clokwise, rotate to the position of element on next index, else rotate to previous index */
        if (clockWise)
        {
            first.Rotate(x2, y2, pos2);
            grid[x2][y2] = first;

            second.Rotate(x3, y3, pos3);
            grid[x3][y3] = second;

            third.Rotate(x1, y1, pos1);
            grid[x1][y1] = third;
        }
        else
        {
            first.Rotate(x3, y3, pos3);
            grid[x3][y3] = first;

            second.Rotate(x1, y1, pos1);
            grid[x1][y1] = second;

            third.Rotate(x2, y2, pos2);
            grid[x2][y2] = third;
        }
    }

    private List<Hexagon> CheckExplosion(List<List<Hexagon>> grid)
    {
        List<Hexagon> neighborList = new List<Hexagon>();
        List<Hexagon> explosiveHex = new List<Hexagon>();
        Hexagon currentHex;
        Hexagon.NeighborHexes neighbors;
        Color currentHexColor;

        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                currentHex = grid[i][j];
                currentHexColor = currentHex.GetColor();
                neighbors = currentHex.GetNeighbors();
                if (IsValid(neighbors.up))
                {
                    neighborList.Add(grid[(int)neighbors.up.x][(int)neighbors.up.y]);
                }
                else
                {
                    neighborList.Add(null);
                }
                if (IsValid(neighbors.upRight))
                {
                    neighborList.Add(grid[(int)neighbors.upRight.x][(int)neighbors.upRight.y]);
                }
                else
                {
                    neighborList.Add(null);
                }
                if (IsValid(neighbors.downRight))
                {
                    neighborList.Add(grid[(int)neighbors.downRight.x][(int)neighbors.downRight.y]);
                }
                else
                {
                    neighborList.Add(null);
                }
                Debug.Log("neighborlist: "+ neighborList.Count);
                for (int a = 0; a < neighborList.Count-1; a++)
                {
                    if (neighborList[a] != null && neighborList[a+1] != null)
                    {
                        if (neighborList[a].GetColor() == currentHexColor && neighborList[a + 1].GetColor() == currentHexColor)
                        {
                            if (!explosiveHex.Contains(neighborList[a]))
                                explosiveHex.Add(neighborList[a]);
                            if (!explosiveHex.Contains(neighborList[a + 1]))
                                explosiveHex.Add(neighborList[a + 1]);
                            if (!explosiveHex.Contains(currentHex))
                                explosiveHex.Add(currentHex);
                        }
                    }
                }
                neighborList.Clear();
            }
        }
        Debug.Log("heyaaaa");
        Debug.Log("explosive List : "+explosiveHex.Count);
        return explosiveHex;
    }
    private List<int> ExplodeHex(List<Hexagon> list)
    {
        List<int> emptyCols = new List<int>();
        float posX, posY;
        blownHex += list.Count;
        scoreText.text = (blownHex * SCORE_CONSTANT).ToString();
        moves++;
        moveText.text = moves.ToString(); 
        foreach (Hexagon hex in list)
        {
            grid[hex.GetX()].Remove(hex);
            emptyCols.Add(hex.GetX());
            Destroy(hex.gameObject);
        }
        foreach (int i in emptyCols)
        {
            for (int j = 0; j < grid[i].Count; ++j)
            {
                posX = GetGridStartCoordinateX() + (HEX_DISTANCE_HORIZONTAL * i);
                posY = (HEX_DISTANCE_VERTICAL * j * 2) + GRID_VERTICAL_OFFSET + (j % 2 == 0 ? HEX_DISTANCE_VERTICAL : ZERO);
                grid[i][j].SetY(j);
                grid[i][j].SetX(i);
                grid[i][j].ChangeWorldPosition(new Vector3(posX, posY, 0));
            }
        }

        /* Indicate the end of process and return the missing column list */
        isExploding = false;
        
        return emptyCols;
    }
    private bool IsValid(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < cols && pos.y >= 0 && pos.y < rows;
    }
    private float GetGridStartCoordinateX()
    {
        return cols / 2 * -HEX_DISTANCE_HORIZONTAL;
    }

    public Hexagon GetSelectedHexagon() { return selectedHex; }
}
