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
        public Sprite pointer; 
        public Pos parent;
        public Node(int x, int y, Tile t, Sprite s)
        {
            parent.x = x;
            parent.y = y;
            cur = t;
            pointer = s;
        }
    }

    private Node[,] terrainmap;
    
    // map size 15 x 15 chosen from the unity editor
    public Vector3Int tmapsize;

    // the follwing elements exist for graphic ad will store sprites 
    //public Tilemap topMap;
    public Tilemap botMap;
    public Grid tileGrid;
    public Tile select;
    public Tile std;
    public Tile start;
    public Tile end;
    public Tile start_s;
    public Tile end_s;
    public Tile wall;

    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;
    public Sprite up_left;
    public Sprite up_right;
    public Sprite down_left;
    public Sprite down_right;
    public Sprite none;
    
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
        //int counter = 0;

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
                        endSelected = true;
                        gameStart = false;
                    }
                }
            }
        }
    }
    
    // Update is called once per frame
    void Start()
    {
        //Camera.main.orthograpthicSize = widthToBeSeen * Screen.height / Screen.width * 0.5;
        genMap();
    }
    void Update()
    {
        getStartandEnd();
        updateMap();
    }
}
