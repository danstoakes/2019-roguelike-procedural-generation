/*
 * Author: Dan Stoakes
 * Purpose: Class for calculating paths using A*.
 * Date: 17/11/2019
 */

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AStar : MonoBehaviour
{
	private Position current = null;
	
	/* Generates the path to draw a corridor or the AI will follow
	 * @param startX - the starting x coordinate
	 * @param startY - the starting y coordinate
	 * @param targetX - the end x coordinate
	 * @param targetY - the end y coordinate
	 * @param ignore - distinguishes between corridor generating and AI pathfinding
	 * @param tile - the tile to be drawn in
	 * @param draw - whether to draw after generation or not
	 * @return Void
	 */
	public void drawAStarCorridor(int startX, int startY, int targetX, int targetY, bool ignore, Tile tile, bool draw)
    {
        Position start = new Position(startX, startY);
        Position target = new Position(targetX, targetY);

        List<Position> openList = new List<Position>();
        List<Position> closedList = new List<Position>();

        int g = 0;

		// add original position to the open list
        openList.Add(start);

        while (openList.Count > 0)
        {
			// get the tile with the lowest F score in the open list
            int lowest = getLowestF(openList);
            current = getLowestFPosition(openList, lowest);

			// swap the current tile from the closed list to the open list
            closedList.Add(current);
            openList.Remove(current);

			// if the target tile was added, a valid path is found so end loop
            if (closedList.FirstOrDefault(l => l.getX() == target.getX() && l.getY() == target.getY()) != null)
                break;
			
			// get a list of all possible adjacent tiles
            List<Position> adjacentSquares = getValidAdjacentSquares(current.getX(), current.getY(), target, ignore, draw);
            g++;

            foreach (Position adjacentSquare in adjacentSquares)
            {
				// if this adjacent tile is already in the closed list, ignore it
                if (closedList.FirstOrDefault(l => l.getX() == adjacentSquare.getX() && l.getY() == adjacentSquare.getY()) != null)
                    continue;
				
				// if the adjacent tile is in the open list
                if (openList.FirstOrDefault(l => l.getX() == adjacentSquare.getX() && l.getY() == adjacentSquare.getY()) != null)
                {
					// test if using the current G score makes the adjacent tile's F score lower
					// if yes, update the parent because it's a better path
                    if (g + adjacentSquare.getH() < adjacentSquare.getF())
                    {
                        adjacentSquare.setG(g);
                        adjacentSquare.setF(adjacentSquare.getG() + adjacentSquare.getH());
                        adjacentSquare.setParent(current);
                    }
                } else
                {
					// adjacent tile is not in the open list so compute the score and set the parent
                    adjacentSquare.setG(g);
                    adjacentSquare.setH(calculateManhattan(adjacentSquare, target));
                    adjacentSquare.setF(adjacentSquare.getG() + adjacentSquare.getH());
                    adjacentSquare.setParent(current);

					// add the adjacent tile to the open list
                    openList.Insert(0, adjacentSquare);
                }
            }
        }
		
		// if using the A* for generating corridors
		if (draw)
		{
			Tilemap tilemap = GetComponent<Tilemap>();
			
			// loop through each tile and draw it before retrieving the parent
			while (current != null)
			{
				tilemap.SetTile(tilemap.WorldToCell(new Vector3(current.getX(), current.getY(), 0)), tile);
				current = current.getParent();
			}
		}
    }
	
	/* Gets the lowest value for F there is amongst tiles
	 * @param openList - the list of tiles to consider
	 * @return Int
	 */
    private int getLowestF(List<Position> openList)
    {
        int lowest = openList[0].getF();
		
		// perform a simple linear search
        foreach (Position position in openList)
        {
            if (position.getF() < lowest)
                lowest = position.getF();
        }
        return lowest;
    }
	
	/* Gets the index for the lowest value of F there is amongst tiles
	 * @param openList - the list of tiles to consider
	 * @param lowest - the lowest value to match for
	 * @return Position
	 */
    private Position getLowestFPosition(List<Position> openList, int lowest)
    {
		// perform a search to find the position for
		// the lowest F value
        foreach (Position position in openList)
        {
            if (position.getF() == lowest)
                return position;
        }
        return null;
    }
	
	/* Gets all valid squares that the algorithm will be able to move to
	 * @param x - the x coordinate
	 * @param y - the y coordinate
	 * @param target - the position to move towards
	 * @param ignore - distinguishes between corridor generating and AI pathfinding
	 * @param draw - whether to draw after generation or not
	 * @return List<Position>
	 */
    private List<Position> getValidAdjacentSquares(int x, int y, Position target, bool ignore, bool draw)
    {
        Tilemap tilemap = GetComponent<Tilemap>();

		// all possible directions that the AI can move in
        List<Position> possiblePositions = new List<Position>()
        {
            new Position (x, y - 1),
            new Position (x, y + 1),
            new Position (x - 1, y),
            new Position (x + 1, y),
        };

        List<Position> validPositions = new List<Position>();

        Vector3Int tilePosition;
		// for each possible position of movement
        foreach (Position position in possiblePositions)
        {
			// get the new position if movement succeeds
			tilePosition = tilemap.WorldToCell(new Vector3(position.getX(), position.getY(), 0));
			
			// separate between the algorithm used for player movement, and corridor generation
            if (ignore)
            {
				// if the projected tile is not empty
                if (tilemap.GetTile(tilePosition) != null)
                {
					// check if a floor tile, which is valid for AI movement
                    if (tilemap.GetTile(tilePosition).ToString().Split(' ')[0] != "floor")
                    {
                        validPositions.Add(position);
                    } else
                    {
						// check if the tile is the end point
                        if (position.getX() == target.getX() && position.getY() == target.getY())
                            validPositions.Add(position);
                    }
                } else
                {
                    validPositions.Add(position);
                }
            } else
            {
				// only for corridor generation, eg draw corridor as the loop progresses
				if (draw)
				{
					// empty tile, so corridor can be generated here
					if (!tilemap.GetTile(tilePosition))
					{
						validPositions.Add(position);
					} else
					{
						// check if the tile is the end point
						if (position.getX() == target.getX() && position.getY() == target.getY())
							validPositions.Add(position);
					}
				} else 
				{
					TileBase tile = tilemap.GetTile(tilePosition);
					// check if a wall, as this is not valid for AI movement
					if (tile == null || tile.ToString().Split(new char[]{' '})[0] != "wall")
					{
						validPositions.Add(position);
					} else
					{
						// check if the tile is the end point
						if (position.getX() == target.getX() && position.getY() == target.getY())
						{
							validPositions.Add(position);
						}
					}
				}
            }
        }
        return validPositions;
    }
	
	/* Calculates the distance between two points in only two axes
	 * @param position - the start position
	 * @param target - the end position
	 * @return Int
	 */
    private int calculateManhattan(Position position, Position target)
    {
		// return the distance between two points measured along axes at right angles
        return Mathf.Abs(target.getX() - position.getX()) + Mathf.Abs(target.getY() - position.getY());
    }
}