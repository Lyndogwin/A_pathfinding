using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class tileautomate : MonoBehaviour
{
    public struct Pos
    {
        public int x;
        public int y; 
    }
     
    public struct Node : IEquatable<Node>
    {
        // *** implement enumerated state or use integer representations
        
        public Tile cur; // will reprsent state
        public Tile pointer; 
        public Pos parent;
        public Pos myPos;
        public int f;
        public int g;
        public int h;
        public Node(int w, int r, Tile t, Tile s)
        {
            cur = t;
            pointer = s;
            myPos.x = w;
            myPos.y = r;
            parent.x = 0;
            parent.y = 0;
            f = 500;
            g = 0;
            h = 0;
        }
        public bool Equals(Node node)
            => node.myPos.x == myPos.x && node.myPos.y == myPos.y;
    }
    // create references 
    public class Ref<T> where T : struct
    {
        public T Value {get; set;}
    }

    private List<Ref<Node>> openList;
    private List<Ref<Node>> closedList;
    private Node[,] terrainmap;
    private Vector3Int startPos;
    private Vector3Int endPos;
    private Vector3Int curPos;
    
    // map size 15 x 15 chosen from the unity editor
    public Vector3Int tmapsize;
    
    public Tilemap botMap;
    public Tilemap topMap;
    public Grid tileGrid;

    // The following tiles will represent states
    public Tile select;
    public Tile std;
    public Tile start;
    public Tile end;
    public Tile start_s;
    public Tile end_s;
    public Tile wall;
    public Tile select_p;

    // The folling tiles will represent parent pointers
    public Tile up;
    public Tile down;
    public Tile left;
    public Tile right;
    public Tile up_left;
    public Tile up_right;
    public Tile down_left;
    public Tile down_right;
    public Tile none;
    
    //*** read more documentation of enum */
    //[Flags]
    public enum State
    {
        std = 0x01,
        select = 0x02,
        start = 0x04,
        end = 0x08,
        start_s = 0x10,
        end_s = 0x20,
        wall = 0x40
    }

    int width;
    int height;
    int numTiles;
    int numWallTiles;

    bool gameStart = true;
    bool startSelected = false;
    bool endSelected = false;
    bool pathFound = false;

    public void genMap()
    {
        width = tmapsize.x;
        height = tmapsize.y;
        numTiles = width * height;
        numWallTiles = (int)(Math.Ceiling(numTiles * .1));
        System.Random rnd = new System.Random();

        terrainmap = new Node[width,height]; // initalize mapsize
        openList = new List<Ref<Node>>();
        closedList = new List<Ref<Node>>();
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                terrainmap[i,j] = new Node(i,j,std,none);
            }
        }
        
        for(int i = 0; i < numWallTiles; i++)
        {
            int []selector = new int[2];
            selector[0] = rnd.Next(0,15);
            selector[1] = rnd.Next(0,15);

            if(terrainmap[selector[0],selector[1]].cur != wall)
            {
               terrainmap[selector[0],selector[1]].cur = wall;
            }
            else
            {
                --i;
            }
        }


        // check for proper dimensions and size at the console 
        if (terrainmap.Rank > 1) {
            for (int dimension = 1; dimension <= terrainmap.Rank; dimension++)
                Debug.Log("Dimension:" + dimension + " size: " +
                            terrainmap.GetUpperBound(dimension - 1));
        }
        
    }
    public void updateMap()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                botMap.SetTile(new Vector3Int(i,j, 0), terrainmap[i,j].cur);
                topMap.SetTile(new Vector3Int(i,j, 0), terrainmap[i,j].pointer);
                //Debug.Log(new Vector3Int(-i + width / 2, -j + height / 2, 0));
            }
        }
    }

    public void getStartandEnd()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int tilePos = tileGrid.WorldToCell(worldPos);
        //Debug.Log(tilePos);
        if(gameStart)
        {
            if(!startSelected)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    if(terrainmap[tilePos.x,tilePos.y].cur != wall)
                    {
                        terrainmap[tilePos.x,tilePos.y].cur = start;
                        startPos = tilePos;
                        curPos = tilePos;
                        startSelected = true;
                    }
                }
            }
            else if(!endSelected)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    if((terrainmap[tilePos.x,tilePos.y].cur != start) && (terrainmap[tilePos.x,tilePos.y].cur != wall))
                    {
                        terrainmap[tilePos.x,tilePos.y].cur = end;
                        endPos = tilePos;
                        endSelected = true;
                        gameStart = false;
                    }
                }
            }
        }
    }
    
    public void search()
    {
        int s_move = 10;
        int d_move = 14;
        int [,]moves = new int[8,2] {{curPos.x -1,curPos.y}, {curPos.x -1,curPos.y -1}, 
                        {curPos.x,curPos.y -1}, {curPos.x +1,curPos.y -1}, 
                        {curPos.x +1,curPos.y}, {curPos.x +1,curPos.y +1}, 
                        {curPos.x,curPos.y +1}, {curPos.x -1,curPos.y+1}};

        for(int k = 0; k < 8; k++)
        {
            int i = moves[k,0];
            int j = moves[k,1];
            bool diag_wall = false;
            if(i >= width || j >= height || i < 0 || j < 0){
                continue;
            }
            if(terrainmap[i,j].cur == wall || closedList.Exists(p => p.Value.Equals(terrainmap[i,j])))
            {
                continue;
            }
            
            switch(k)
            {
                case 0:
                    break;
                case 1:
                    if(terrainmap[moves[k-1,0],moves[k-1,1]].cur == wall || terrainmap[moves[k+1,0],moves[k+1,1]].cur == wall)
                    {
                        diag_wall = true;
                    }
                    break;
                case 2:
                    break;
                case 3:
                    if(terrainmap[moves[k-1,0],moves[k-1,1]].cur == wall || terrainmap[moves[k+1,0],moves[k+1,1]].cur == wall)
                    {
                        diag_wall = true;
                    }
                    break;
                case 4:
                    break;
                case 5:
                    if(terrainmap[moves[k-1,0],moves[k-1,1]].cur == wall || terrainmap[moves[k+1,0],moves[k+1,1]].cur == wall)
                    {
                        diag_wall = true;
                    }
                    break;
                case 6:
                    break;
                case 7:
                    if(terrainmap[moves[k-1,0],moves[k-1,1]].cur == wall || terrainmap[moves[0,0],moves[0,1]].cur == wall)
                    {
                        diag_wall = true;
                    }
                    break;
            }
            if(diag_wall)
            {
                continue;
            }
            
            //Debug.Log("<color=red>"+(!openList.Exists(p => p.Value.Equals(terrainmap[i,j])))+"</color>");
            if((terrainmap[curPos.x,curPos.y].myPos.x == startPos.x && terrainmap[curPos.x,curPos.y].myPos.y == startPos.y ) || 
                (!openList.Exists(p => p.Value.Equals(terrainmap[i,j]))) ||
                (terrainmap[curPos.x,curPos.y].g < terrainmap[terrainmap[i,j].parent.x, terrainmap[i,j].parent.y].g))
            {
                Debug.Log("<color=green> conditions met to change parent to "+curPos+"</color>");
                terrainmap[i,j].parent.x = curPos.x;
                terrainmap[i,j].parent.y = curPos.y;
                switch(k)
                {
                    case 0:
                        terrainmap[i,j].pointer = right;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + s_move;
                        break;
                    case 1:
                        terrainmap[i,j].pointer = up_right;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + d_move;
                        break;
                    case 2:
                        terrainmap[i,j].pointer = up;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + s_move;
                        break;
                    case 3:
                        terrainmap[i,j].pointer = up_left;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + d_move;
                        break;
                    case 4:
                        terrainmap[i,j].pointer = left;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + s_move;
                        break;
                    case 5:
                        terrainmap[i,j].pointer = down_left;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + d_move;
                        break;
                    case 6:
                        terrainmap[i,j].pointer = down;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + s_move;
                        break;
                    case 7:
                        terrainmap[i,j].pointer = down_right;
                        terrainmap[i,j].g = terrainmap[curPos.x,curPos.y].g + d_move;
                        break;
                }
                int e_dx = Math.Abs(endPos.x - i); 
                int e_dy = Math.Abs(endPos.y - j);
                terrainmap[i,j].h = (e_dx + e_dy)*10;
                terrainmap[i,j].f = terrainmap[i,j].g + terrainmap[i,j].h;
                /* 
                Debug.Log("current position:"+ curPos + i + " " + j );
                Debug.Log("h value is: " + terrainmap[i,j].h);
                Debug.Log("g value is: " + terrainmap[i,j].g);
                Debug.Log("f value is: " + terrainmap[i,j].f);
                Debug.Log("h copy value is: " + openList[openList.Count - 1].h);
                Debug.Log("g copy value is: " + openList[openList.Count - 1].g);
                Debug.Log("f copy value is: " + openList[openList.Count - 1].f);
                */
                if(!openList.Exists(p => p.Value.Equals(terrainmap[i,j])))
                {
                    Debug.Log("<color=yellow> Alert: </color>Adding a new node to openList");
                    openList.Add(new Ref<Node> {Value = terrainmap[i,j]}); // add if not in openList
                }
            }
        }
        if(curPos.x == startPos.x && curPos.y == startPos.y)
        {
            terrainmap[curPos.x,curPos.y].cur = start_s;
        }
        else if(curPos.x == endPos.x && curPos.y == endPos.y)
        {
            terrainmap[curPos.x,curPos.y].cur = end_s;
            pathFound = true;
        }
        else
        {
            terrainmap[curPos.x,curPos.y].cur = select;
        }

        
        closedList.Add(new Ref<Node> {Value = terrainmap[curPos.x,curPos.y]});
        
        foreach (Ref<Node> i in openList)
        {   
            //if(i.Value.myPos.x == curPos.x && i.Value.myPos.y == curPos.y )
            if(i.Value.Equals(terrainmap[curPos.x,curPos.y]))
            {    
                openList.Remove(i);
                //Debug.Log("<color=green>Success! </color>removed current node from open list");
            }
        }
       
        if(terrainmap[curPos.x,curPos.y].cur != end_s) //may be problematic
        {
            Ref<Node> lowest_f = openList[0];//new Node(500,500,none,none); this may be the problem
            
            // loop through the openList of structs "Node" and calculate the f value
            foreach (Ref<Node> i in openList)
            {
                /* 
                int e_dx = Math.Abs(endPos.x - i.Value.myPos.x); 
                int e_dy = Math.Abs(endPos.y - i.Value.myPos.y);
                terrainmap[i.Value.myPos.x,i.Value.myPos.y].h = (e_dx + e_dy)*10;
                terrainmap[i.Value.myPos.x,i.Value.myPos.y].f = terrainmap[i.Value.myPos.x,i.Value.myPos.y].g + terrainmap[i.Value.myPos.x,i.Value.myPos.y].h;
                */
                Debug.Log("<color=green>f here is "+ i.Value.f+"</color>"); //references are working if changes are noticable
                Debug.Log("position is "+ i.Value.myPos.x + "," + i.Value.myPos.y);
                Debug.Log("parent position is "+ i.Value.parent.x + "," + i.Value.parent.y);
                
                if(i.Value.f < lowest_f.Value.f)//this will greatly effect decision making
                {
                    lowest_f = i;
                }
            }
            //Debug.Log("lowest f is " + lowest_f.Value.f);
            Debug.Log("---------------------------");
            curPos.x = lowest_f.Value.myPos.x;
            curPos.y = lowest_f.Value.myPos.y;
        }
    }
    public void tracePath()
    {
        if(terrainmap[curPos.x,curPos.y].myPos.x != startPos.x && terrainmap[curPos.x,curPos.y].myPos.y != startPos.y)
        {
            if(terrainmap[curPos.x,curPos.y].myPos.x != endPos.x && terrainmap[curPos.x,curPos.y].myPos.y != endPos.y)
            {
                terrainmap[curPos.x,curPos.y].cur = select_p;
            }
            curPos.x = terrainmap[curPos.x,curPos.y].parent.x;
            curPos.y = terrainmap[curPos.x,curPos.y].parent.y;
        }
        else 
        {
            if(Input.GetMouseButtonDown(0))
            {
                gameStart = true;
                pathFound = false;
                endSelected = false;
                startSelected = false;
                genMap();
            }
        }
    }
    
    void Start()
    {
        genMap();
    }
    
    // Update is called once per frame
    void Update()
    {
        getStartandEnd();
        if(!gameStart && !pathFound)
        {
            search();
        }
        if(pathFound)
        {
            tracePath();
        }
        
        updateMap();
    }
}
