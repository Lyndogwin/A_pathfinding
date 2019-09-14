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
        public object state = default(state);
        public Pos parent;
        public Node(int x, int y)
        {
            switch (state)
            {
                case state.end:
                    state = state.end;
            }
            parent.x = x;
            parent.y = y;
        }
    }
    //*** read more documentation of enum */
    public enum state
    {
        std,
        select,
        start,
        end,
        start_s,
        end_s,
        wall
    }

    private Node[,] terrainmap;
    
    // map size 15 x 15 chosen from the unity editor
    public Vector3Int tmapsize;

    public Tilemap topMap;
    public Tilemap botMap;
    public Tile select;
    public Tile std;
    public Tile start;
    public Tile end;
    public Tile start_s;
    public Tile end_s;
    public Tile wall;

    int width;
    int height;

    public void genMap()
    {
        // clearmap(false);
        width = tmapsize.x;
        height = tmapsize.y;

        terrainmap = new Node[width,height];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                terrainmap[i,j] = new Node();
            }
        }
        if (terrainmap.Rank > 1) {
            for (int dimension = 1; dimension <= terrainmap.Rank; dimension++)
                Debug.Log("Dimension:" + dimension + " size: " +
                            terrainmap.GetUpperBound(dimension - 1));
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        genMap();
    }
}
