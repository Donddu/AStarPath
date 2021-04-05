using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker, target;
    Grid grid;

    void Update() 
    {
        if (Input.GetButtonDown("Jump"))
        {
            FindPath(seeker.position, target.position);
        }
    }

    void Awake() {
        grid = GetComponent<Grid>();
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //Test performance
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        //With heap system
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        //No heap system
        //List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            //With heap system
            Node currentNode = openSet.RemoveFirst();
            //No Heap system
            /*Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);*/
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                //Performance test
                sw.Stop();
                print ("Path found: " + sw.ElapsedMilliseconds + " ms");

                //Calculate new path before exiting loop
                RetracePath(startNode, targetNode);
                return;
            }

            //Skip to the next neighbour if grid is not walkable or already in list (aka Closed)
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                //Calculate new movement to neighbour
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                //Check neighbours gCost values if they are lower after move
                //or not in the list already
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    //Calculate new fCost aka gCost and hCost
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    //Set parent node to current node
                    neighbour.parent = currentNode;

                    //Add neighbour in open set if it's not in there
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    //Retrace distance
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    //Calculating closest node to end point (distance)
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        //Formula for calculating closest path
        //Idea is that if y is less than x you go corner to corner till you are in same row 
        //and after that you take straight path in x axis
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }



}
