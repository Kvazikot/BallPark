﻿// Kvazikot 


using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public partial class MazeGenParallel : MonoBehaviour
{
    
    (float, float)[] vertexes_d;
    (int, int)[] edges_d;
    bool[] visibility_d;
    bool[] visited;
    Dictionary<string, int> edge_map;
    Dictionary<int, int> notCellNumbers;
    bool canDraw;
    /*
    static (float, float)[] vertexes_d = new (float, float)[] { (-30f, 30f), (0,30), (30,30), (-30,10),
                                              (0, 10), (30,10), (-30,-8), (0,-8), (30,-8)};

    static (int, int)[] edges_d = new (int, int)[] { (0,1), (1,2), (3,4), (4,5), (6,7), (7,8),
                                                     (0,3), (1,4), (2,5), (3,6), (4,7), (5,8)};

    static bool[] visibility_d = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true, true };

    static bool[] visited = new bool[] { false, false, false, false, false, false, false, false, false };
    // TODO: change this on Dictionary<int, List<int>> 
    static Dictionary<string, int> edge_map = new Dictionary<string, int>()
    {
        { "0-1", 0 }, {"1-2", 1}, {"3-4", 2}, {"4-5", 3}, {"6-7", 4}, {"7-8", 5},
         {"3-0", 6}, {"1-4", 7}, {"2-5", 8}, {"3-6", 9}, {"4-7",10}, {"5-8",11}
    };

    */

    public struct CreateMazeJob : IJob
    {
        //public NativeArray<(float,float)> vertexes;
        //public NativeArray<(int,int)> edges;
        //public NativeArray<bool> visibility;
        public int cellsX;
        public int cellsY;

        public void Execute()
        {
            //vertexes.CopyFrom(vertexes_d);
            //edges.CopyFrom(edges_d);
            //visibility.CopyFrom(visibility_d);
            //BuildMaze();
            /*
                        // Test AddNeibours
                        for (int i = 0; i < 8; i++)
                        {
                            AddEdgeNeibours(i, ref neibours);
                            string values = " ";
                            foreach (var item in neibours)
                                values += $"{item}, ";
                            Debug.Log($"{edges_d[i].Item1}-{edges_d[i].Item2} neibours: " + values);
                            neibours.Clear();
                        }
            */

}



    }
        
    public void CreateGrid()
    {

        float maze_size = 10 * transform.localScale.x;
        float grid_step = maze_size / cellsX;
        wall_width = grid_step * 0.3f;

        (float, float) v00 = (transform.position.x - maze_size/2, transform.position.y + maze_size/2);
        Vector3 v0 = new Vector3(v00.Item1, transform.position.y, v00.Item2);
        Vector3 v = new Vector3(v00.Item1, transform.position.y, v00.Item2);
        int counter = 0;
        int x_counter = 0;

        canDraw = false;
        int n_vert = (cellsX + 1) * (cellsY + 1);
        int n_edges = (cellsX ) * (cellsY + 1) * 2;
        //fill vertices
        int y_counter = 0;
        while (counter != n_vert )
        {
            vertexes_d[counter] = (v.x, v.z);
            v += new Vector3(grid_step, 0, 0);
            if (x_counter == cellsX)
            {
                v.x = v0.x;
                v.z -= grid_step;
                x_counter = 0;
                if(!notCellNumbers.ContainsKey(counter))
                    notCellNumbers.Add(counter, counter);
                counter++; y_counter++;
            }
            else
            {
                x_counter++;
                counter++;
            }
            if (y_counter == cellsY)
                if (!notCellNumbers.ContainsKey(counter))
                    notCellNumbers.Add(counter, counter);
            //Debug.Log("counter xcounter " + counter + " " + x_counter);
        }

        // horizontal edges
        int e_counter = 0;
        counter = 0;
        x_counter = 0;
        while (e_counter != n_edges / 2   )
        {
            if (x_counter == cellsX )
            {
                x_counter = 0;
                counter++;
            }
            else
            {
                x_counter++;
                edges_d[e_counter] = (counter, counter + 1);
                counter++; e_counter++;
            }
        }

        //vertical edges
        e_counter = n_edges / 2;
        counter = 0;
        x_counter = 0;
        while (e_counter  != n_edges)
        {
             x_counter++;
             edges_d[e_counter] = (counter, counter + cellsX + 1);
             counter++; e_counter++;
        }

        //create edge map
        n_edges = 0;
        foreach (var e in edges_d)
        {
            edge_map[$"{e.Item1}-{e.Item2}"] = n_edges;
            n_edges++;
        }

        canDraw = true;
        return;
      
    }
    /*
    Choose the initial cell, mark it as visited and push it to the stack
    While the stack is not empty
        Pop a cell from the stack and make it a current cell
            If the current cell has any neighbours which have not been visited
                Push the current cell to the stack
                Choose one of the unvisited neighbours
                Remove the wall between the current cell and the chosen cell
                Mark the chosen cell as visited and push it to the stack
                */
    public void BuildMaze()
    {
        List<int> cell_list = new List<int>();
        List<int> neibours = new List<int>();

       
        ResetMe();
        CreateGrid();
        var rand = new System.Random();
        int rand_idx = getRandomCell();      
        visited[rand_idx] = true;
        cell_list.Add(rand_idx);
        int iters = 0;
        int current_cell = 0;

        while (cell_list.Count != 0)
        {
            int n;
            //if ( n >= wall_list.Count) n--;
            current_cell = cell_list[cell_list.Count - 1];
            cell_list.RemoveAt(cell_list.Count - 1);
            GetUnvisitedNeibours(current_cell, ref neibours);
            if (neibours.Count != 0)
            {
                cell_list.Add(current_cell);
                n = rand.Next(0, neibours.Count);
                int cell_id = neibours[n];
                RemoveWall(current_cell, cell_id);
                visited[cell_id] = true;
                cell_list.Add(cell_id);
            }
            neibours.Clear();
            iters++;
            if (iters > 10000) break;
        }
        Debug.Log("iters " + iters);
    }

    int GetEdgeID(int vertex1, int vertex2)
    {
        int edge_id=-1;
        if (!edge_map.TryGetValue($"{vertex1}-{vertex2}", out edge_id))
            edge_map.TryGetValue($"{vertex2}-{vertex1}", out edge_id);
        return edge_id;
    }

    void RemoveWall(int cell1, int cell2)
    {
        int edge_id;
        int d = cell1 - cell2;
        if (Math.Abs(d) == (cellsX + 1))
        {
            int cell = Math.Max(cell1, cell2);
            if ((edge_id = GetEdgeID(cell, cell + 1))!=-1)
               visibility_d[edge_id] = false;
        }
        if (d == 1)
        {
            if ((edge_id = GetEdgeID(cell1, cell1 + cellsX + 1)) != -1)
                visibility_d[edge_id] = false;
        }
        if (d == -1)
        {
            if ((edge_id = GetEdgeID(cell2, cell2 + cellsX + 1)) != -1)
                visibility_d[edge_id] = false;
        }

    }

    bool validCellNumber(int n)
    {
       return (n >= 0) && (n < vertexes_d.Length) && (!notCellNumbers.ContainsKey(n));
    }
        
    void GetUnvisitedNeibours(int current_cell, ref List<int> neibours)
    {
        int n = (current_cell + 1);
        if(validCellNumber(n) && !visited[n] )
           neibours.Add(n);
        n = (current_cell - 1);
        if (validCellNumber(n) && !visited[n] )
            neibours.Add(n);
        n = (current_cell + cellsX + 1);
        if(validCellNumber(n) && !visited[n] )
            neibours.Add(n);
        n = (current_cell - cellsX - 1);
        if (validCellNumber(n) && !visited[n] )
            neibours.Add(n);
    }

    int getRandomCell()
    {
        var rand = new System.Random();
        int i = rand.Next(0, cellsX - 1);
        int j = rand.Next(0, cellsY - 1);
        return j * cellsX + i;
    }

    void ResetMe()
    {
        edge_map.Clear();
        for (int i = 0; i < vertexes_d.Length; i++)
            vertexes_d[i] = (0, 0);
        for (int i = 0; i < edges_d.Length; i++)
            edges_d[i] = (0,0);
        for (int i = 0; i < visibility_d.Length; i++)
            visibility_d[i] = true;
        for (int i = 0; i < visited.Length; i++)
            visited[i] = false;
    }
              
    public void DrawGrapth((float,float)[] vertexes, (int,int)[] edges, ref bool[] visibility)
    {
       
        int n = 0;
        foreach (var e in edges)
        {
              float x1 = vertexes[e.Item1].Item1;
              float y1 = vertexes[e.Item1].Item2;
              float x2 = vertexes[e.Item2].Item1;
              float y2 = vertexes[e.Item2].Item2;

              if (visibility[n])
                Gizmos.DrawLine(new Vector3(x1, 5, y1), new Vector3(x2, 5, y2));
            n++;
        }
    }

    void CreateWallPrimitive(Vector3 position, Vector3 localScale, string name, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = localScale;
        wall.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.RandomRange(0, 0.1F), Vector3.up);
        wall.transform.parent = transform;
        wall.transform.name = name;
        //var renderer = wall.GetComponent<MeshRenderer>();
        //renderer.material.SetColor("_Color", color);
        wall.layer = 8;
        wall.tag = "wall";
    }

    // параметрическое уравнение линии
    Vector2 line_eq(Vector2 A, Vector2 B, float t)
    {
        Vector2 C;

        C.x = A.x + t * (B.x - A.x);
        C.y = A.y + t * (B.y - A.y);

        return C;
    }

    public void DrawMaze()
    {

        int n = 0;
        foreach (var e in edges_d)
        {
            float x1 = vertexes_d[e.Item1].Item1;
            float y1 = vertexes_d[e.Item1].Item2;
            float x2 = vertexes_d[e.Item2].Item1;
            float y2 = vertexes_d[e.Item2].Item2;
            Vector2 v1 = new Vector2(x1, y1);
            Vector2 v2 = new Vector2(x2, y2);
            Vector2 center = line_eq(v1, v2, 0.5f);
            bool horizontal;
            if (x1 == x2)
                horizontal = true;
            else
                horizontal = false;
            if (visibility_d[n])
            {
                //Gizmos.DrawLine(new Vector3(x1, 5, y1), new Vector3(x2, 5, y2));
                if (horizontal)
                    CreateWallPrimitive(new Vector3(center.x, transform.position.y, center.y), 
                                        new Vector3(wall_width, 2, 
                                        (v2 - v1).magnitude + wall_width/2), 
                                        $"wall {n}", Color.green);
                else
                    CreateWallPrimitive(new Vector3(center.x, transform.position.y, center.y), 
                                        new Vector3((v2 - v1).magnitude + wall_width / 2, 
                                                    2, wall_width), $"wall {n}", Color.green);
            }
            n++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
      

    }

    public void RegenerateMaze()
    {
        int n_vert = (cellsX + 1) * (cellsY + 1);
        int n_edges = (cellsX) * (cellsY + 1) * 2;
        vertexes_d = new (float, float)[n_vert + 1];
        edges_d = new (int, int)[n_edges];
        visibility_d = new bool[n_edges];
        visited = new bool[n_edges];
        // creating vertex and edges arrays based on grid topology
        edge_map = new Dictionary<string, int>();
        notCellNumbers = new Dictionary<int, int>();
        //NativeArray<Cell1> result = new NativeArray<Cell1>((cellsX+2) * (cellsY+2) + 2, Allocator.TempJob);
         maze_job = new CreateMazeJob
        {
            //vertexes = new NativeArray<(float, float)>(vertexes_d.Length, Allocator.Persistent),
            //edges = new NativeArray<(int, int)>(edges_d.Length, Allocator.Persistent),
            //visibility = new NativeArray<bool>(visibility_d.Length, Allocator.Persistent),
            cellsX = cellsX,
            cellsY = cellsY
        };
        //JobHandle firstHandle = maze_job.Schedule();
        //firstHandle.Complete();
        //DrawCells1(result);
        BuildMaze();
        //maze_job.visibility.Dispose();
        DrawMaze();

        bMazeGenerated = true;


    }

    private void OnDrawGizmos1()
    {
        if (!canDraw) return;
        foreach (var v in vertexes_d)
            Gizmos.DrawSphere(new Vector3(v.Item1, 5, v.Item2), 1);

        DrawGrapth(vertexes_d, edges_d, ref visibility_d);
    }


    // Update is called once per frame
    void Update()
    {

        
    }

    public int cellsX;
    public int cellsY;
    CreateMazeJob maze_job;
    public float wall_width = 3f;
    float grid_step;
    float size;
    public bool bMazeGenerated = false;
    //This declared because of note in documentation on function CreatePrimitive 
    private MeshFilter mesh_filter;



}


