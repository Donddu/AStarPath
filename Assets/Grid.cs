using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    //[SerializeField] public Transform player;
    [SerializeField]public bool onlyDisplayPathGizmos;
    [SerializeField] public LayerMask unwalkableMask;
    [SerializeField] public Vector2 gridWorldSize;
    [SerializeField] public float nodeRadius;
    //Get node class and make two dimensional array
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start() 
    {
        nodeDiameter = nodeRadius*2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
        CreateGrid();
    }

    public int MaxSize 
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        //Get left edge of world
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //Get every point and make square in grid
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                //Collision check for every point
                //Physics.CheckSphere returns true if there is collision
                //Return walkable not true if there is collision
                //World point and node radius checks every square we make in grid
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                //Populate grid with nodes and check if it's walkable or not
                // in Node(walkable, worldPoint, x, y) x and y is keeping track of position
                grid[x,y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        //Search 3x3 block around the node
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //Skip center node
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                //Add nodes to the neighbour list check grid boundaries
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    //Find node where player is
    public Node NodeFromWorldPoint(Vector3 worldPosition) 
    {
        //Convert percentage for how far a long grid it is in x/y coordinates
        //far left percentage 0, middle percentage 0.5 and far right percentage 1
        //examble position calculation: (0 + 15) / 30 = 0.5 and (-15 + 15) / 30 = 0 etc.
        float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
        //Clamp01 means clamps value between 0 and 1 and returns value
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //Get x and y values from grid array
        //examble grid position calculation: (30-1) * 0.5 = 14.5
        //Cals give character pos in grid and returns value in array
        //Round to int so it is grid value
        int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
        return grid[x,y];
    }

    public List<Node> path;
    void OnDrawGizmos() 
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,1,gridWorldSize.y));

        //Hide grid gizmos
        if (onlyDisplayPathGizmos)
        {
            if (path != null)
            {
                foreach (Node n in path)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
                }
            }
        }
        else
        {
            if (grid != null)
            {
                //Node playerNode = NodeFromWorldPoint(player.position);
                //Draw cubes in grid
                foreach (Node n in grid)
                {
                    //? returns value one of the two expressions
                    //if not collision white and collision red
                    Gizmos.color = (n.walkable)?Color.white:Color.red;
                    /*if (playerNode == n) 
                    {
                    Gizmos.color = Color.cyan; 
                    }*/
                    if (path != null)
                    {
                        if (path.Contains(n))
                        {
                            Gizmos.color = Color.black;
                        }
                    }
                    //one means giving one, one, one in each of axis
                    //nodeDiameter-.1f -.1f means outline
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
                }
            }
        }
    }
}
