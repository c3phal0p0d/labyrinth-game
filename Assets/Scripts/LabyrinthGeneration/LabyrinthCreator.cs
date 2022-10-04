using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthCreator
{
    private int numSections = 4;
    private int mazeWidth = 10;
    private int mazeHeight = 10;
    private float cellWidth = 1f;
    private float wallDepth = 0.1f;
    private float wallHeight = 1.5f;
    private GameObject origin;
    private Material brickMaterial;
    private GameObject wallTorchPrefab;

    private System.Random random;

    public LabyrinthCreator(int numSections, int mazeWidth, int mazeHeight, float cellWidth, float wallHeight, float wallDepth, GameObject origin, Material brickMaterial, GameObject wallTorchPrefab, System.Random random)
    {
        this.numSections = numSections;
        this.mazeWidth = mazeWidth;
        this.mazeHeight = mazeHeight;
        this.cellWidth = cellWidth;
        this.wallHeight = wallHeight;
        this.wallDepth = wallDepth;
        this.origin = origin;
        this.random = random;
        this.brickMaterial = brickMaterial;
        this.wallTorchPrefab = wallTorchPrefab;

        CreateLabyrinth();
    }
    
    private void CreateMaze(Maze maze, GameObject mazeOrigin, Material brickMaterial, GameObject wallTorchPrefab)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(mazeOrigin.transform);
        floor.transform.localScale = new Vector3(maze.Width * cellWidth + wallDepth, 0.5f, maze.Height * cellWidth + wallDepth);
        floor.transform.localPosition = new Vector3(maze.Width * cellWidth / 2, 0, maze.Height * cellWidth / 2 );
        floor.GetComponent<Renderer>().material = brickMaterial;
        floor.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(12, 12));

        GameObject startPos = new GameObject();
        startPos.name = "StartPos";
        startPos.transform.SetParent(mazeOrigin.transform);
        startPos.transform.localScale = new Vector3(cellWidth, 3 * wallDepth, cellWidth);
        startPos.transform.localPosition = new Vector3(cellWidth * (maze.StartX + 0.5f), 0, cellWidth * (maze.StartY + 0.5f));

        int i = 0;
        foreach (Maze.Wall wall in maze.Walls)
        {
            if (wall.isHorizontal)
            {
                GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallObj.name = "InnerWall" + i;
                wallObj.transform.SetParent(mazeOrigin.transform);
                wallObj.transform.localScale= new Vector3(cellWidth + wallDepth, wallHeight, wallDepth);
                wallObj.transform.localPosition = new Vector3(cellWidth * (wall.x + 0.5f), wallHeight / 2, cellWidth * (wall.y + 1));
                wallObj.GetComponent<Renderer>().material = brickMaterial;

                // Add torch to wall
                int randomInt = Random.Range(1,3);
                if (randomInt%2==0){    // reduce number of torches spawned
                    if (i%2==0){    // alternate between sides of walls
                        GameObject wallTorch = GameObject.Instantiate(wallTorchPrefab, wallObj.transform.position + new Vector3(0, wallHeight/10, wallDepth/2), Quaternion.identity);
                        wallTorch.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
                    }
                    else {
                        GameObject wallTorch = GameObject.Instantiate(wallTorchPrefab, wallObj.transform.position + new Vector3(0, wallHeight/10, -wallDepth/2), Quaternion.identity);
                        wallTorch.transform.rotation = Quaternion.AngleAxis(-90, Vector3.up);
                    }
                }
            }
            else
            {
                GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallObj.name = "InnerWall" + i;
                wallObj.transform.SetParent(mazeOrigin.transform);
                wallObj.transform.localScale = new Vector3(wallDepth, wallHeight, cellWidth + wallDepth);
                wallObj.transform.localPosition = new Vector3(cellWidth * (wall.x + 1), wallHeight / 2, cellWidth * (wall.y + 0.5f));
                wallObj.GetComponent<Renderer>().material = brickMaterial;

                // Add torch to wall
                int randomInt = Random.Range(1,3);
                if (randomInt%2==0){     // reduce number of torches spawned
                    if (i%2==0){    // alternate between sides of walls
                        GameObject wallTorch = GameObject.Instantiate(wallTorchPrefab, wallObj.transform.position + new Vector3(wallDepth/2, wallHeight/10, 0), Quaternion.identity);
                        wallTorch.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
                    }
                    else {
                        GameObject wallTorch = GameObject.Instantiate(wallTorchPrefab, wallObj.transform.position + new Vector3(-wallDepth/2, wallHeight/10, 0), Quaternion.identity);
                    }
                }
            }

            i++;
        }
    }

    private void CreateLabyrinth()
    {
        int x = 0;
        int y = 0;
        int dxPrev = 0;
        int dyPrev = 0;
        int dx = 0;
        int dy = 0;
        //int holePos;
        for (int i = 0; i < numSections; i++)
        {
            // Position of the maze
            GameObject mazeOrigin = new GameObject("MazeOrigin" + i);
            mazeOrigin.transform.SetParent(origin.transform);
            mazeOrigin.transform.localPosition = new Vector3(x * (mazeWidth * cellWidth + wallDepth), 0, y * (mazeHeight * cellWidth + wallDepth));

            (dx, dy) = NewDirection(dxPrev, dyPrev);
            x += dx;
            y += dy;

            // Outer Walls
            if (dxPrev != 1 && (i == numSections-1 || dx != -1))
                CreateOuterWall(0, false, mazeOrigin, "OuterWallLeft");
            if (dxPrev != -1 && (i == numSections - 1 || dx != 1))
                CreateOuterWall(1, false, mazeOrigin, "OuterWallRight");
            if (dyPrev != 1 && (i == numSections - 1 || dy != -1))
                CreateOuterWall(0, true, mazeOrigin, "OuterWallDown");
            if (dyPrev != -1 && (i == numSections - 1 || dy != 1))
                CreateOuterWall(1, true, mazeOrigin, "OuterWallUp");

            // Generate inner walls
            Maze maze = new Maze(mazeWidth, mazeHeight, new System.Tuple<int, int>(random.Next() % mazeWidth, random.Next() % mazeHeight), random);
            CreateMaze(maze, mazeOrigin, brickMaterial, wallTorchPrefab);

            dxPrev = dx;
            dyPrev = dy;
        }
    }

    private (int, int) NewDirection(int dxPrev, int dyPrev)
    {
        if (dyPrev != 0)
        {
            int x = random.Next() % 2;
            return (x, (1-x) * dyPrev); // x=0 -> (0,dyPrev)   x=1 -> (1,0)
        }
        else
        {
            int x = random.Next() % 3;
            return (x % 2, 1 - x); // x=0 -> (0,1)   x=1 -> (1,0)   x=2 -> (0,-1)
        }
    }

    private void CreateOuterWall(int x, bool isHorizontal, GameObject mazeOrigin, string name)
    {
        GameObject outerWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        outerWall.name = name;
        outerWall.transform.SetParent(mazeOrigin.transform);
        outerWall.GetComponent<Renderer>().material = brickMaterial;
        outerWall.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(8, 1));
        
        if (isHorizontal)
        {
            outerWall.transform.localScale = new Vector3(mazeWidth * cellWidth + wallDepth, wallHeight, wallDepth);
            outerWall.transform.localPosition = new Vector3(mazeWidth * cellWidth / 2, wallHeight / 2, x * mazeHeight * cellWidth);
        }
        else
        {
            outerWall.transform.localScale = new Vector3(wallDepth, wallHeight, mazeHeight * cellWidth + wallDepth);
            outerWall.transform.localPosition = new Vector3(x * mazeWidth * cellWidth, wallHeight / 2, mazeHeight * cellWidth / 2);
        }
    }
}
