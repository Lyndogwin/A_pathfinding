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
     
    public struct Node
    {
        // *** implement enumerated state or use integer representations
        
        public Tile cur; // will reprsent state
        public Tile pointer; 
        public Pos parent;
        public Node(int x, int y, Tile t, Tile s)
        {
            parent.x = x;
            parent.y = y;
            cur = t;
            pointer = s;
        }
    }

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

    public void genMap()
    {
        // clearmap(false);
        width = tmapsize.x;
        height = tmapsize.y;
        numTiles = width * height;
        numWallTiles = (int)(Math.Ceiling(numTiles * .1));
        System.Random rnd = new System.Random();

        terrainmap = new Node[width,height]; // initalize mapsize
        
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                terrainmap[i,j] = new Node(0,0,std,none);
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
        Debug.Log(tilePos);
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
        int [,]moves = new int[8,2] {{curPos.x -1,curPos.y}, {curPos.x -1,curPos.y -1}, 
                        {curPos.x,curPos.y -1}, {curPos.x +1,curPos.y -1}, 
                        {curPos.x +1,curPos.y}, {curPos.x +1,curPos.y +1}, 
                        {curPos.x,curPos.y +1}, {curPos.x -1,curPos.y+1}};
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < 8; k++)
                {
                    if((i == moves[k,0]) && (j == moves[k,1]))  
                    {
                        terrainmap[i,j].parent.x = curPos.x;
                        terrainmap[i,j].parent.y = curPos.y;
                        switch(k)
                        {
                            case 0:
                                terrainmap[i,j].pointer = right;
                                break;
                            case 1:
                                terrainmap[i,j].pointer = up_right;
                                break;
                            case 2:
                                terrainmap[i,j].pointer = up;
                                break;
                            case 3:
                                terrainmap[i,j].pointer = up_left;
                                break;
                            case 4:
                                terrainmap[i,j].pointer = left;
                                break;
                            case 5:
                                terrainmap[i,j].pointer = down_left;
                                break;
                            case 6:
                                terrainmap[i,j].pointer = down;
                                break;
                            case 7:
                                terrainmap[i,j].pointer = down_right;
                                break;
                        }
                        int e_dx = Math.Abs(endPos.x - curPos.x); 
                        int e_dy = Math.Abs(endPos.y - curPos.y);
                        int h = e_dx + e_dy;
                        Debug.Log("current position:"+ curPos + i + " " + j );
                        Debug.Log("h value is: " + h);
                    }
                }
            }
        }
    }
    
    void Start()
    {
        //Camera.main.orthograpthicSize = widthToBeSeen * Screen.height / Screen.width * 0.5;
        genMap();
    }
    
    // Update is called once per frame
    void Update()
    {
        getStartandEnd();
        if(!gameStart)
        {
            search();
        }
        updateMap();
    }
}
