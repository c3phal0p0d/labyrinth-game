using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthCreator
{
    private int mazeWidth = 10;
    private int mazeHeight = 10;
    private float cellWidth = 1f;
    private float wallDepth = 0.1f;
    private float wallHeight = 1.5f;

    private float timer;

    public LabyrinthCreator(LabyrinthSize sizes)
    {
        this.mazeWidth = sizes.mazeWidth;
        this.mazeHeight = sizes.mazeHeight;
        this.cellWidth = sizes.cellWidth;
        this.wallHeight = sizes.wallHeight;
        this.wallDepth = sizes.wallDepth;
    }
    
    private void CreateMaze(Maze maze, GameObject mazeOrigin, LabyrinthParameters labyrinthParameters, MazeParameters mazeParameters)
    {   
        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.layer = LayerMask.NameToLayer("Ground");
        floor.name = "Floor";
        floor.transform.SetParent(mazeOrigin.transform);
        floor.transform.localScale = new Vector3(maze.Width * cellWidth + wallDepth, 0.5f, maze.Height * cellWidth + wallDepth);
        floor.transform.localPosition = new Vector3(maze.Width * cellWidth / 2, 0, maze.Height * cellWidth / 2 );
        floor.GetComponent<Renderer>().material = labyrinthParameters.brickMaterial;

        // Roof
        
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(mazeOrigin.transform);
        ceiling.transform.localScale = new Vector3(maze.Width * cellWidth + wallDepth, 0.5f, maze.Height * cellWidth + wallDepth);
        ceiling.transform.localPosition = new Vector3(maze.Width * cellWidth / 2, wallHeight, maze.Height * cellWidth / 2 );
        ceiling.GetComponent<Renderer>().material = labyrinthParameters.brickMaterial;
        
        
        // Starting position
        GameObject startPos = new GameObject();
        startPos.name = "StartPos";
        startPos.transform.SetParent(mazeOrigin.transform);
        startPos.transform.localScale = new Vector3(cellWidth, 3 * wallDepth, cellWidth);
        startPos.transform.localPosition = new Vector3(cellWidth * (maze.StartX + 0.5f), 0, cellWidth * (maze.StartY + 0.5f));

        int i = 0;
        foreach (Maze.Wall wall in maze.HorizontalWalls)
        {
            GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObj.name = "HorizontalWall" + i;
            wallObj.transform.SetParent(mazeOrigin.transform);
            wallObj.transform.localScale = new Vector3(cellWidth * wall.length + wallDepth, wallHeight, wallDepth);
            wallObj.transform.localPosition = new Vector3(cellWidth * (wall.x + 0.5f * wall.length), wallHeight / 2, cellWidth * (wall.y + 1));
            wallObj.GetComponent<Renderer>().material = labyrinthParameters.brickMaterial;
            wallObj.layer = LayerMask.NameToLayer("Wall");

            // Add torches to wall
            int torchRotation;
            Vector3 torchPositionOffset;

            // One side of wall
            torchRotation = 90;
            torchPositionOffset = new Vector3(0, 0, wallDepth/2);
            SpawnTorch(torchRotation, torchPositionOffset, wallObj, labyrinthParameters);

            // Opposite side of wall
            torchRotation = -90;
            torchPositionOffset = new Vector3(0, 0, -wallDepth/2);
            SpawnTorch(torchRotation, torchPositionOffset, wallObj, labyrinthParameters);

            i++;
        }

        i = 0;
        foreach (Maze.Wall wall in maze.VerticalWalls)
        {
            GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObj.name = "VerticalWall" + i;
            wallObj.transform.SetParent(mazeOrigin.transform);
            float posOffset = 0;
            float length = cellWidth * wall.length - wallDepth;
            if (!wall.hasWallBelow)
            {
                length += wallDepth;
                posOffset -= wallDepth / 2;
            }
            if (!wall.hasWallAbove) {
                length += wallDepth;
                posOffset += wallDepth / 2;
            }
            wallObj.transform.localScale = new Vector3(wallDepth, wallHeight, length);
            wallObj.transform.localPosition = new Vector3(cellWidth * (wall.x + 1), wallHeight / 2, cellWidth * (wall.y + 0.5f * wall.length) + posOffset);
            wallObj.GetComponent<Renderer>().material = labyrinthParameters.brickMaterial;
            wallObj.layer = LayerMask.NameToLayer("Wall");

            // Add torches to wall
            int torchRotation;
            Vector3 torchPositionOffset;

            // One side of wall
            torchRotation = 180;
            torchPositionOffset = new Vector3(wallDepth/2, 0, 0);
            SpawnTorch(torchRotation, torchPositionOffset, wallObj, labyrinthParameters);

            // Other side of wall
            torchRotation = 0;
            torchPositionOffset = new Vector3(-wallDepth/2, 0, 0);
            SpawnTorch(torchRotation, torchPositionOffset, wallObj, labyrinthParameters);

            i++;
        }
    }

    private void SpawnTorch(int torchRotation, Vector3 torchPositionOffset, GameObject wallObj, LabyrinthParameters parameters){
        GameObject wallTorch = GameObject.Instantiate(parameters.wallTorchPrefab, wallObj.transform.position + torchPositionOffset, Quaternion.identity);
        GameObject wallTorchObject = new GameObject();
        wallTorch.transform.rotation = Quaternion.AngleAxis(torchRotation, Vector3.up);
        wallTorchObject.transform.SetParent(wallObj.transform);
        wallTorch.transform.SetParent(wallTorchObject.transform);
    }

    private bool CheckOverlap(Vector3 overlapPosition, Vector3 overlapRadius){
        // Check if object overlaps with other objects
        Collider[] overlappingObjects = Physics.OverlapBox(overlapPosition, overlapRadius, Quaternion.identity, LayerMask.GetMask("Wall"));
        if (overlappingObjects.Length > 1){
            return true;
        }
        return false;
    }

    public void CreateLabyrinth(LabyrinthParameters parameters)
    {
        int x = 0;
        int y = 0;
        int dxPrev = 0;
        int dyPrev = 0;
        int dx = 0;
        int dy = 0;
        //int holePos;
        for (int i = 0; i < parameters.numSections; i++)
        {
            // Position of the maze
            GameObject mazeOrigin = new GameObject("MazeOrigin" + i);
            mazeOrigin.transform.SetParent(parameters.origin.transform);
            mazeOrigin.transform.localPosition = new Vector3(x * (mazeWidth * cellWidth + wallDepth), 0, y * (mazeHeight * cellWidth + wallDepth));

            (dx, dy) = NewDirection(dxPrev, dyPrev, parameters.random);
            x += dx;
            y += dy;

            // Outer Walls
            if (dxPrev != 1 && (i == parameters.numSections-1 || dx != -1))
                CreateOuterWall(0, false, mazeOrigin, "OuterWallLeft", parameters);
            if (dxPrev != -1 && (i == parameters.numSections - 1 || dx != 1))
                CreateOuterWall(1, false, mazeOrigin, "OuterWallRight", parameters);
            if (dyPrev != 1 && (i == parameters.numSections - 1 || dy != -1))
                CreateOuterWall(0, true, mazeOrigin, "OuterWallDown", parameters);
            if (dyPrev != -1 && (i == parameters.numSections - 1 || dy != 1))
                CreateOuterWall(1, true, mazeOrigin, "OuterWallUp", parameters);

            // Generate inner walls
            Maze maze = new Maze(mazeWidth, mazeHeight, new System.Tuple<int, int>(parameters.random.Next() % mazeWidth, parameters.random.Next() % mazeHeight), parameters.random);
            CreateMaze(maze, mazeOrigin, parameters, null);

            dxPrev = dx;
            dyPrev = dy;
        }

        Physics.SyncTransforms();

        // Check that all torches are spawned correctly and do not overlap with walls
        GameObject[] wallTorches = GameObject.FindGameObjectsWithTag("Torch");
        foreach (GameObject wallTorch in wallTorches){
            if (CheckOverlap(wallTorch.transform.position, new Vector3(0.0001f, 0.0001f, 0.0001f))){   // Check if wall torch has been spawned in a position where it overlaps with a wall, and if so destroy it
                GameObject.Destroy(wallTorch);
            }
        }
    }

    private (int, int) NewDirection(int dxPrev, int dyPrev, System.Random random)
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

    private void CreateOuterWall(int x, bool isHorizontal, GameObject mazeOrigin, string name, LabyrinthParameters labyrinthParameters)
    {
        GameObject outerWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        outerWall.name = name;
        outerWall.transform.SetParent(mazeOrigin.transform);
        outerWall.GetComponent<Renderer>().material = labyrinthParameters.brickMaterial;
        
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

        outerWall.layer = LayerMask.NameToLayer("Wall");
    }

    /*
    private GameObject CreateInnerWall(int i, Vector3 wallPosition, Vector3 wallScale, GameObject mazeOrigin, bool isHorizontal = false){
        GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallObj.name = "InnerWall" + i;
        wallObj.transform.SetParent(mazeOrigin.transform);
        wallObj.transform.localScale = wallScale;
        wallObj.transform.localPosition = wallPosition;
        wallObj.GetComponent<Renderer>().material = brickMaterial;

        // Add torches to wall
        int torchRotation;
        Vector3 torchPositionOffset;

        // One side of wall
        if (isHorizontal){
            torchRotation = 90;
            torchPositionOffset = new Vector3(0, 0, wallDepth/2);
        } else {
            torchRotation = 180;
            torchPositionOffset = new Vector3(wallDepth/2, 0, 0);
        }
        SpawnTorch(torchRotation, torchPositionOffset, wallObj);

        // Other side of wall
        if (isHorizontal){
            torchRotation = -90;
            torchPositionOffset = new Vector3(0, 0, -wallDepth/2);
        } else {
            torchRotation = 0;
            torchPositionOffset = new Vector3(-wallDepth/2, 0, 0);
        }
        SpawnTorch(torchRotation, torchPositionOffset, wallObj);
        
        return wallObj;
    }
    */

}
