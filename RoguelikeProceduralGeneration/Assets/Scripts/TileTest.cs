/*
 * Author: Dan Stoakes
 * Purpose: Class for handling tile generation
 * Date: 15/11/2019
 */
 
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TileTest : MonoBehaviour
{
    // public so they can be seen in the Unity window
    public Tile floor;
    public Tile wall;

    private List<Room> rooms = new List<Room>();
    private List<Room> roomPairs = new List<Room>();
	
	private AStar pathFinder;
	
	private int roomX;
	private int roomY;
	private int roomEndX;
	private int roomEndY;
	private int nextRoomX;
	private int nextRoomY;
	private int nextRoomEndX;
	private int nextRoomEndY;

	// is executed once before the first frame update
    private void Start()
    {
		// set up the tilemap and pathFinder
        Tilemap tilemap = GetComponent<Tilemap>();
		pathFinder = FindObjectOfType<AStar>();

        int gridSquare = 50;

		// loop through in x and y, drawing a pair of rooms for each interval of 50
		// and sort the rooms by their x coordinate, before drawing the corridor between
        for (int y = 0; y < 100; y += gridSquare)
        {
            for (int x = 0; x < 100; x += gridSquare)
            {
                drawRooms(x, y);
                rooms = quickSort(rooms, 0, rooms.Count, 'x');
                drawCorridors();
            }
        }
		
		// loop through the pairs of rooms per gridSquare and connect to the next pair of rooms
		// using the A* algorithm to correctly join the last room of the first pair to the first
		// room of the last pair
        for (int i = 1; i < roomPairs.Count - 1; i += 2)
        {
			// joining from right side of left room and connecting to the left side of right room
            roomEndX = roomPairs[i].getX() + roomPairs[i].getWidth() - 1;
            roomY = Random.Range(roomPairs[i].getY() + 1, roomPairs[i].getY() + roomPairs[i].getHeight() - 2);
            nextRoomY = Random.Range(roomPairs[i + 1].getY() + 1, roomPairs[i + 1].getY() + roomPairs[i + 1].getHeight() - 2);
			
			// if the connecting room in the first pair is further right (eg, ends further)
			// than the start of the room that it is supposed to be connecting to, skip
            if (roomEndX > roomPairs[i + 1].getX())
                continue;
		   
		   // draw the outer wall, inner path and then outer wall using A*
		   pathFinder.drawAStarCorridor(roomEndX, roomY, roomPairs[i + 1].getX(), nextRoomY, false, floor, true);
           pathFinder.drawAStarCorridor(roomEndX, roomY - 1, roomPairs[i + 1].getX(), nextRoomY - 1, true, wall, true);
           pathFinder.drawAStarCorridor(roomEndX, roomY + 1, roomPairs[i + 1].getX(), nextRoomY + 1, true, wall, true);
        }
		
		// loop through the room pairs again, this time aiming to produce vertical connections
		// between the room pairs, eg first and last room in lower row of rooms and first and last room
		// in the higher row of rooms
		int additionRoomX = 0;
		int additionNextRoomX = 0;
		for (int i = 0; i < roomPairs.Count - 4; i += 3)
        {
			// to accomodate for the fact that the first connection starts from the left side and ends on
			// the left, while the second condition starts and ends on the right side
			if (i > 0)
            {
				additionRoomX = roomPairs[i].getWidth() - 1;
				additionNextRoomX = roomPairs[i + 4].getWidth() - 1;
			}
			
			// connect from left side for first instance, and right side for second instance
            roomEndX = roomPairs[i].getX() + additionRoomX;
			nextRoomX = roomPairs[i + 4].getX() + additionNextRoomX;
			roomEndY = Random.Range(roomPairs[i].getY() + 1, roomPairs[i].getY() + roomPairs[i].getHeight() - 1);
			nextRoomY = Random.Range(roomPairs[i + 4].getY() + 1, roomPairs[i + 4].getY() + roomPairs[i + 4].getHeight() - 1);
			
			// draw the outer wall, inner path and then outer wall using A*
			pathFinder.drawAStarCorridor(roomEndX, roomEndY + 1, nextRoomX, nextRoomY + 1, true, wall, true);
			pathFinder.drawAStarCorridor(roomEndX, roomEndY, nextRoomX, nextRoomY, false, floor, true);
			pathFinder.drawAStarCorridor(roomEndX, roomEndY - 1, nextRoomX, nextRoomY + 1, true, wall, true);
        }
    }
	
	/* Checks if a new room would collide with any existing room
	 * @param tilemap - the tilemap which tiles are written to
	 * @param x - the x coordinate to start
	 * @param y - the y coordinate to start
	 * @param width - the width to check
	 * @param height - the height to check
	 * @return Bool
	 */
    private bool checkForRoomCollision(Tilemap tilemap, int x, int y, int width, int height)
    {
		// loop through the entire possible mapping of the room
		// and return true if any collisions are found
        Vector3Int targetTilePosition;
        for (int row = x; row < (x + width); row++)
        {
            for (int col = y; col < (y + height); col++)
            {
                targetTilePosition = tilemap.WorldToCell(new Vector3(row, col, 0));

				// collision found, so return true
                if (tilemap.GetTile(targetTilePosition))
                    return true;
            }
        }
		// no collision found, so all clear
        return false;
    }
	
	/* Checks if a new room would collide with any existing room
	 * @param gridRangeX - the range in x for drawing
	 * @param gridRangeY - the range in y for drawing
	 * @return Void
	 */
    private void drawRooms(int gridRangeX, int gridRangeY)
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        Tile tile;

        Room room = null;
        Room nextRoom = null;
		
		// loop until the number of rooms drawn is two,
		// as the aim is to produce a pair of rooms
        int drawnRooms = 0;
        while (drawnRooms < 2)
        {
			// assign random start coordinates, width and height
            int width = Random.Range(10, 30);
            int height = Random.Range(10, 30);

            int x = Random.Range(gridRangeX, (gridRangeX - width) + 50);
            int y = Random.Range(gridRangeY, (gridRangeY - height) + 50);

			// if the placement of the room is collision free
            if (!checkForRoomCollision(tilemap, x, y, width, height))
            {
				// draw the room
                for (int row = x; row < (x + width); row++)
                {
                    for (int col = y; col < (y + height); col++)
                    {
						tile = floor;
						// if on the perimeter of the room, draw a wall instead
                        if (row == x || row == (x + width) - 1 || col == y || col == (y + height) - 1)
                            tile = wall;

                        tilemap.SetTile(tilemap.WorldToCell(new Vector3(row, col, 0)), tile);
                    }
                }
				
				// first room in the pair
                if (drawnRooms == 0)
                {
                    room = new Room(x, y, width, height, false);
                } else
                {
					// second room in the pair
                    nextRoom = new Room(x, y, width, height, false);
                }
                drawnRooms++;
            }
        }
		
		// generate the roomPair, which is important for corridor generation
		// and order in x
        List<Room> roomPair = new List<Room>() {room, nextRoom};
        roomPair = quickSort(roomPair, 0, roomPair.Count, 'x');

        rooms.Add(room);
        rooms.Add(nextRoom);
        roomPairs.Add(roomPair[0]);
        roomPairs.Add(roomPair[1]);
    }
	
	/* Draws a vertical path
	 * @param tilemap - the tilemap which tiles are written to
	 * @param corridorStartX - the x coordinate to start the corridor
	 * @param corridorStartY - the y coordinate to start the corridor
	 * @param corridorEndStartX - the x corridinate to end the corridor at
	 * @param corridorEndStartY - the y coordinate to end the corridor at
	 * @param count - the starting count
	 * @param type - the type of path to be drawn
	 * @return Void
	 */
    private void drawVerticalPath(Tilemap tilemap, int corridorStartX, int corridorStartY, int corridorEndStartX, int corridorEndStartY, int count, string type)
    {
        Tile corridorTile = null;

        int originalCount = count;
		
		// start from left of middle path and end to right of middle path
		// allowing for walls to be drawn for the corridor
        for (int x = corridorStartX - 1; x <= corridorStartX + 1; x++)
        {
			corridorTile = floor;
			// if on the outer perimeter of the corridor
			if (x == corridorStartX - 1 || x == corridorStartX + 1)
                corridorTile = wall;
			
			// determine if the connection starts higher or lower for the value being drawn for x
			// eg, which type of corner connection is being made
			if (type == "down")
			{
				for (int y = corridorStartY + count; y <= corridorEndStartY; y++)
					tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
			} else
            {
				for (int y = corridorStartY; y <= corridorEndStartY + count; y++)
					tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
			}
			
			// this allows for the generation of "pointed" corners
			// which connect easily to one another with no overlap
            if (originalCount > 0)
            {
                count--;
            } else if (originalCount < 0)
            {
                count++;
            }
        }
    }
	
	/* Draws a horizontal path
	 * @param tilemap - the tilemap which tiles are written to
	 * @param corridorStartX - the x coordinate to start the corridor
	 * @param corridorStartY - the y coordinate to start the corridor
	 * @param corridorEndStartX - the x corridinate to end the corridor at
	 * @param corridorEndStartY - the y coordinate to end the corridor at
	 * @param count - the starting count
	 * @param type - the type of path to be drawn
	 * @return Void
	 */
    private void drawHorizontalPath(Tilemap tilemap, int corridorStartX, int corridorStartY, int corridorEndStartX, int corridorEndStartY, int count, string type)
    {
        Tile corridorTile = null;

        int originalCount = count;
		
		// start from bottom of middle path and end to top of middle path
		// allowing for walls to be drawn for the corridor
        for (int y = corridorEndStartY - 1; y <= corridorEndStartY + 1; y++)
        {
			corridorTile = floor;
			// if on the outer perimeter of the corridor
            if (y == corridorEndStartY - 1 || y == corridorEndStartY + 1)
                corridorTile = wall;

			// determine if the connection starts more left or right for the value being drawn for y
			// eg, which type of corner connection is being made
            if (type == "left")
            {
                for (int x = corridorStartX + count; x <= corridorEndStartX; x++)
                    tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
            } else
            {
                for (int x = corridorEndStartX; x <= corridorStartX + count; x++)
                    tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
            }
			
			// this allows for the generation of "pointed" corners
			// which connect easily to one another with no overlap
            if (originalCount > 0)
            {
                count--;
            } else if (originalCount < 0)
            {
                count++;
            }
        }
    }
	
	/* Draws a horizontal path
	 * @param tilemap - the tilemap which tiles are written to
	 * @param x1 - the x coordinate to start the corridor
	 * @param y1 - the y coordinate to start the corridor
	 * @param x2 - the x corridinate to end the corridor at
	 * @param y2 - the y coordinate to end the corridor at
	 * @param length - the length of the path
	 * @param type - the type of path to be drawn
	 * @param mode - the mode of drawing
	 * @return Void
	 */
	private void drawHorizontalPathUpDownConnection (Tilemap tilemap, int x1, int y1, int x2, int y2, int length, string type, int mode)
	{
		int increase = 0;
		
		Tile corridorTile = null;
												
		for (int y = y1 - 1; y <= y1 + 1; y++)
		{
			corridorTile = wall;
			if (y == y1 - 1)
			{
				if (type == "up")
					increase = 1;
			} else if (y == y1 + 1)
			{
				if (type == "down")
					increase = 1;
			} else
			{
				corridorTile = floor;
			}
													
			if (mode == 0)
			{
				for (int x = x1; x < x1 + length + increase; x++)
					tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
			} else if (mode == 1)
			{
				for (int x = x2; x > x2 - length - increase; x--)
					tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
			} else if (mode == 2)
			{
				for (int x = x2; x >= x1 + increase; x--)
					tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
			}
			increase = 0;
		}
	}
	
	/* Gets the order in which rooms are in y
	 * @param room - the first room
	 * @param room - the second room
	 * @return Room[]
	 */
    private Room[] getRoomElevationOrder(Room room, Room nextRoom)
    {
        Room[] rooms = new Room[2];
		
		rooms[0] = room;
        rooms[1] = nextRoom;

        int roomEndY = (room.getY() + room.getHeight() - 1);
        int nextRoomEndY = (nextRoom.getY() + nextRoom.getHeight() - 1);

		// if the top of the first room is less than the bottom of the second room
        if (roomEndY < nextRoomEndY)
        {
            rooms[0] = nextRoom;
            rooms[1] = room;
        }
		// return the elevation order in y for the rooms
        return rooms;
    }
	
	/* Gets the clearance type for rooms which are in y
	 * @param room - the first room
	 * @param room - the second room
	 * @return String
	 */
    private string getClearanceTypeY(Room room, Room nextRoom)
    {
		// determine where the top of each room is
        int endY = (room.getY() + room.getHeight() - 1);
        int nextEndY = (nextRoom.getY() + nextRoom.getHeight() - 1);

        string clearanceType = "some";
		
		// if the rooms are not overlapping in y at all
        if (room.getY() < nextRoom.getY() && endY > nextEndY || room.getY() > nextRoom.getY() && endY < nextEndY)
        {
            clearanceType = "full";
        } else if (room.getY() > nextEndY || nextRoom.getY() > endY)
        {
			// completely overlapping in y, eg one could fit inside
            clearanceType = "none";
        }
        return clearanceType;
    }
	
	/* Draws the corridors between the rooms
	 * @return Void
	 */
    private void drawCorridors()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        int pathWidth = 3;
		
		// for each room, all connections are made from left to right
		// eg from room to nextRoom (see below)
        for (int r = 0; r < rooms.Count; r++)
        {
            Room room = rooms[r];
			
			// if the room does not have a valid connection to its pair
            if (room.getConnected() == false)
            {
                roomEndX = (room.getX() + room.getWidth() - 1);
                roomEndY = (room.getY() + room.getHeight() - 1);
				
				// loop through the second "pair" room which will be connected to via a corridor
                foreach (Room nextRoom in rooms)
                {
					// ensure that the room to be connected to is not the original room
                    if (room != nextRoom)
                    {
                        nextRoomEndX = (nextRoom.getX() + nextRoom.getWidth() - 1);
                        nextRoomEndY = (nextRoom.getY() + nextRoom.getHeight() - 1);
						
						// get the elevation order in y of the rooms
                        Room[] roomElevationOrder = getRoomElevationOrder(room, nextRoom);
						
                        bool stacked = false;
                        bool overlapY = false;
						
                        string overlapCondition = null;
						
						// ensure both rooms are not connected to any other
                        if (!room.getConnected() && !nextRoom.getConnected())
                        {
							// if room is further left than nextRoom
                            if (roomEndX < nextRoom.getX())
                            {
                                string clearanceType = getClearanceTypeY(room, nextRoom);
								
								// get the higher and lower room
                                Room higherRoom = roomElevationOrder[0];
                                Room lowerRoom = roomElevationOrder[1];
								
                                if (higherRoom == null && lowerRoom == null)
                                {
									// first room becomes the higher room and vice versa
                                    higherRoom = room;
                                    lowerRoom = nextRoom;
                                }
								
								// calculate the room boundary coordinates, which are vital for corridor connection
                                int higherRoomEndX = (higherRoom.getX() + higherRoom.getWidth() - 1);
                                int higherRoomEndY = (higherRoom.getY() + higherRoom.getHeight() - 1);
                                int lowerRoomEndX = (lowerRoom.getX() + lowerRoom.getWidth() - 1);
                                int lowerRoomEndY = (lowerRoom.getY() + lowerRoom.getHeight() - 1);
								
								// if the second room ends higher or equal to the bottom of
								// the first room in y
                                if (room == higherRoom && nextRoomEndY >= room.getY())
                                {
                                    if (nextRoomEndY - pathWidth < room.getY() - 1)
                                    {
										// there is some overlap towards the top
                                        overlapCondition = "top";
                                        overlapY = true;
                                    }

                                } else if (room == lowerRoom && roomEndY >= nextRoom.getY())
                                {
                                    if (roomEndY - pathWidth < nextRoom.getY() - 1)
                                    {
										// there is some overlap towards the bottom
                                        overlapCondition = "bottom";
                                        overlapY = true;
                                    }

                                } // this if, else if determines whether there are two rooms which are not overlapping in x
								  // but are only slightly overlapped in y, such that a corridor would not be able to be drawn
								  // in a simple straight line, as the overlap in y is less than the pathWidth (3)

                                int corridorStartX = roomEndX;
                                int corridorStartY = 0;
                                int corridorEndStartX = nextRoom.getX();
                                int corridorEndStartY = 0;

                                int drawingMode = 0;

                                Tile corridorTile;
								
								// if any overlap in y was found
                                if (overlapY)
                                {
                                    if (overlapCondition == "top")
                                    {
										// connect corridor from random point on right side of higherRoom to bottom of
										// lowerRoom in the form of a right angle connection
                                        corridorStartX = higherRoomEndX;
                                        corridorStartY = Random.Range(higherRoom.getY() + 1, higherRoomEndY - 1);

                                        corridorEndStartX = Random.Range(lowerRoom.getX() + 1, lowerRoomEndX - 1);
                                        corridorEndStartY = lowerRoomEndY;
										
										// draw the paths
										drawHorizontalPath(tilemap, corridorStartX, 0, corridorEndStartX, corridorStartY, 0, "left");
										drawVerticalPath(tilemap, corridorEndStartX, corridorEndStartY, 0, corridorStartY, -1, "up");
                                    } else if (overlapCondition == "bottom")
                                    {
										// connect corridor from random point on right side of lowerRoom to bottom of
										// higherRoom in the form of a right angle connection
                                        corridorStartX = lowerRoomEndX;
                                        corridorStartY = Random.Range(lowerRoom.getY() + 1, lowerRoomEndY - 1);

                                        corridorEndStartX = Random.Range(higherRoom.getX() + 1, higherRoomEndX - 1);
                                        corridorEndStartY = higherRoom.getY();
										
										// draw the paths
										drawHorizontalPath(tilemap, corridorStartX, 0, corridorEndStartX, corridorStartY, 0, "left");
										drawVerticalPath(tilemap, corridorEndStartX, corridorStartY, 0, corridorEndStartY, 1, "down");
                                    }
                                } else
                                {
									// no troublesome overlap in y, so need to determine whether a simple straight corridor
									// from room to room is possible, or whether a more advanced solution is required
                                    switch (clearanceType)
                                    {
                                        case "full":
                                            {
												// no overlap at all in Y, will need special measures
                                                drawingMode = 0;
                                                corridorStartY = Random.Range(lowerRoom.getY() + 1, lowerRoomEndY - 1);
                                                corridorEndStartY = corridorStartY;
                                                break;
                                            }
                                        case "some":
                                            {
												// will need special measures because the corridor will not fit in the overlap size
                                                if (nextRoom.getX() - roomEndX > pathWidth)
                                                {
                                                    drawingMode = 1;
                                                    if (room == lowerRoom)
                                                    {
                                                        corridorStartY = Random.Range(nextRoom.getY() + 1, roomEndY - 1);
                                                        corridorEndStartY = Random.Range(nextRoom.getY() + 1, roomEndY - 1);
                                                    } else if (room == higherRoom)
                                                    {
                                                        corridorStartY = Random.Range(room.getY() + 1, nextRoomEndY - 1);
                                                        corridorEndStartY = Random.Range(room.getY() + 1, nextRoomEndY - 1);
                                                    }
                                                } else
                                                {
													// there is a degree of overlap in y, which exceeds the pathWidth
													// allowing for a simple connection
                                                    drawingMode = 0;
                                                    corridorStartY = Random.Range(higherRoom.getY() + 1, lowerRoomEndY - 1);
                                                    corridorEndStartY = corridorStartY;
                                                }
                                                break;
                                            }
                                        case "none":
                                            {
												// one of the rooms is inside the other in terms of y
												// (remembering) that there is no collision in x
                                                if (nextRoom.getX() - roomEndX > pathWidth)
                                                {
                                                    drawingMode = 1;
                                                    corridorStartY = Random.Range(room.getY() + 1, roomEndY - 1);
                                                    corridorEndStartY = Random.Range(nextRoom.getY() + 1, nextRoomEndY - 1);
                                                } else
                                                {
                                                    if (room == lowerRoom)
                                                    {
														// will be connecting from the top of room and joining to leftside of nextRoom
                                                        drawingMode = 2;
                                                        corridorStartX = Random.Range(room.getX() + 1, roomEndX - 1);
                                                        corridorStartY = roomEndY;
                                                        corridorEndStartX = nextRoom.getX();
                                                        corridorEndStartY = Random.Range(nextRoom.getY() + 1, nextRoomEndY - 1);
                                                    } else
                                                    {
														// will be connecting from the bottom of room and joining to leftside of nextRoom
                                                        drawingMode = 3;
                                                        corridorStartX = Random.Range(room.getX() + 1, roomEndX - 1);
                                                        corridorStartY = room.getY();
                                                        corridorEndStartX = nextRoom.getX();
                                                        corridorEndStartY = Random.Range(nextRoom.getY() + 1, nextRoomEndY - 1);
                                                    }
                                                }
                                                break;
                                            }
                                    }
									
                                    if (drawingMode == 0)
                                    {
										// draw a simple connection from one room to the other
                                        drawHorizontalPath(tilemap, corridorStartX, corridorStartY, corridorEndStartX, corridorEndStartY, 0, "left");
                                    } else if (drawingMode == 1)
                                    {
										// this method works by drawing little corridors from the right side of room and the left
										// side of nextRoom, and then connecting with a horizontal line at the point they meet
                                        int distanceApartX = corridorEndStartX - corridorStartX;
                                        int corridorLength = Random.Range(pathWidth, distanceApartX - 2);
                                        int corridorEndLength = distanceApartX - corridorLength;
										
										// if the corridor starts below where it ends in Y
                                        if (corridorStartY < corridorEndStartY)
                                        {				
											// draw two horizontal paths from each room
											drawHorizontalPathUpDownConnection(tilemap, corridorStartX, corridorStartY, corridorStartX, corridorStartY, corridorLength, "up", 0);
											drawHorizontalPathUpDownConnection(tilemap, corridorEndStartX, corridorEndStartY, corridorEndStartX, corridorEndStartY, corridorEndLength, "down", 1);
											
											// connect the horizontal paths from the rooms with a single vertical in y
                                            for (int x = (corridorStartX + corridorLength) - 1; x <= (corridorStartX + corridorLength) + 1; x++)
                                            {
												corridorTile = floor;
                                                if (x == (corridorStartX + corridorLength) - 1 || x == (corridorStartX + corridorLength) + 1)
                                                    corridorTile = wall;

												// draw the corridor
                                                for (int y = corridorStartY; y <= corridorEndStartY; y++)
                                                    tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y - (x - (corridorStartX + corridorLength)), 0)), corridorTile);
                                            }

                                        } else if (corridorStartY > corridorEndStartY)
                                        {
											// corridor starts higher than it ends, so drawing the other way around
											drawHorizontalPathUpDownConnection(tilemap, corridorStartX, corridorStartY, corridorStartX, corridorStartY, corridorLength, "down", 0);
											drawHorizontalPathUpDownConnection(tilemap, corridorEndStartX, corridorEndStartY, corridorEndStartX, corridorEndStartY, corridorEndLength, "up", 1);
											
											// connect the horizontal paths with a vertical path, this time drawn from bottom to top
                                            for (int x = (corridorStartX + corridorLength) - 1; x <= (corridorStartX + corridorLength) + 1; x++)
                                            {
												corridorTile = floor;
                                                if (x == (corridorStartX + corridorLength) - 1 || x == (corridorStartX + corridorLength) + 1)
                                                    corridorTile = wall;

												// draw the corridor
                                                for (int y = corridorStartY; y >= corridorEndStartY; y--)
                                                    tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y + (x - (corridorStartX + corridorLength)), 0)), corridorTile);
                                            }
                                        } else
                                        {
											// start and end in same position in y, so draw a simple horizontal path to connect the rooms
                                            drawHorizontalPath(tilemap, corridorStartX, corridorStartY, corridorEndStartX, corridorEndStartY, 0, "left");
                                        }
                                    } else if (drawingMode == 2 || drawingMode == 3)
                                    {
                                        if (drawingMode == 2)
                                        {
											// draw a simple 'L' connection between room and nextRoom
                                            drawVerticalPath(tilemap, corridorStartX, corridorStartY, 0, corridorEndStartY, 1, "up");
											drawHorizontalPathUpDownConnection(tilemap, corridorStartX, corridorEndStartY, corridorEndStartX, corridorEndStartY, 0, "up", 2);
                                        } else
                                        {
											// draw a vertical corridor from up to down, which gets higher by 1 in y as x--
											// increases, eg higher wall on one side and lower on other of path
                                            int count = -1;
                                            for (int x = corridorStartX - 1; x <= corridorStartX + 1; x++)
                                            {
												corridorTile = floor;
                                                if (x == corridorStartX - 1 || x == corridorStartX + 1)
                                                    corridorTile = wall;
                                              
                                                for (int y = corridorStartY; y >= corridorEndStartY + count; y--)
                                                    tilemap.SetTile(tilemap.WorldToCell(new Vector3(x, y, 0)), corridorTile);
                                                count++;
                                            }
											// draw the connecting horizontal path
											drawHorizontalPathUpDownConnection(tilemap, corridorStartX, corridorEndStartY, corridorEndStartX, corridorEndStartY, 0, "down", 2);
                                        }
										// set the start and end of the corridors to floor tiles manually
                                        tilemap.SetTile(tilemap.WorldToCell(new Vector3(corridorStartX, corridorStartY, 0)), floor);
                                        tilemap.SetTile(tilemap.WorldToCell(new Vector3(corridorEndStartX, corridorEndStartY, 0)), floor);
                                    }
                                }
                            } else if (roomEndX >= nextRoom.getX() && !room.getConnected() && !nextRoom.getConnected())
                            {
								// there is a degree of overlap in x, as room does not end before nextRoom starts
								// they could also start in the same position in x, in which case stacking may occur if in same y pane
                                Room higherRoom = roomElevationOrder[0];
                                Room lowerRoom = roomElevationOrder[1];

                                int higherRoomEndX = (higherRoom.getX() + higherRoom.getWidth() - 1);
                                int higherRoomEndY = (higherRoom.getY() + higherRoom.getHeight() - 1);

                                int lowerRoomEndX = (lowerRoom.getX() + lowerRoom.getWidth() - 1);
                                int lowerRoomEndY = (lowerRoom.getY() + lowerRoom.getHeight() - 1);

                                int corridorStartX = 0;
                                int corridorStartY = lowerRoomEndY;

                                int corridorEndStartX = 0;
                                int corridorEndStartY = 0;
								
								// both rooms have zero clearance in y, eg are wall to wall
                                if (higherRoom.getY() - 1 == lowerRoomEndY)
                                    stacked = true;
								
								// if there is overlap in x
                                if (higherRoomEndX - 1 <= lowerRoom.getX() || lowerRoomEndX - 1 <= higherRoom.getX())
                                {	
									// if the far left room is higher
                                    if (room == higherRoom)
                                    {
                                        corridorStartX = higherRoomEndX;
                                        corridorStartY = Random.Range(higherRoom.getY() + 1, higherRoomEndY - 1);

                                        corridorEndStartX = Random.Range(corridorStartX + pathWidth, lowerRoomEndX - 1);
                                        corridorEndStartY = lowerRoomEndY;
										
										// draw the paths (simple 'L')
										drawHorizontalPath(tilemap, corridorStartX, 0, corridorEndStartX, corridorStartY, 0, "left");
										drawVerticalPath(tilemap, corridorEndStartX, corridorEndStartY, 0, corridorStartY, -1, "up");
                                    } else if (room == lowerRoom)
                                    {
										// the far left room is lower
                                        corridorStartX = lowerRoomEndX;
                                        corridorStartY = Random.Range(lowerRoom.getY() + 1, lowerRoomEndY - 1);

                                        corridorEndStartX = Random.Range(lowerRoomEndX + 1, higherRoomEndX - 1);
                                        corridorEndStartY = higherRoom.getY();
										
										// draw the paths (simple 'L')
										drawHorizontalPath(tilemap, corridorStartX, 0, corridorEndStartX, corridorStartY, 0, "left");										
										drawVerticalPath(tilemap, corridorEndStartX, corridorStartY, 0, corridorEndStartY, 1, "down");
                                    }
                                } else
                                {
									// check for every possible condition in which the x value could
									// vary between room and nextRoom and choose the start of the corridor accordingly
									// this is important because the start must be inside the range of the larger room,
									// because it is a simple straight line connection
                                    if (lowerRoom.getX() > higherRoom.getX() && lowerRoomEndX < higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(lowerRoom.getX() + 1, lowerRoomEndX - 1);
                                    } else if (lowerRoom.getX() < higherRoom.getX() && lowerRoomEndX > higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(higherRoom.getX() + 1, higherRoomEndX - 1);
                                    } else if (lowerRoom.getX() < higherRoom.getX() && lowerRoomEndX < higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(higherRoom.getX() + 1, lowerRoomEndX - 1);
                                    } else if (lowerRoom.getX() > higherRoom.getX() && lowerRoomEndX > higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(lowerRoom.getX() + 1, higherRoomEndX - 1);
                                    } else if (lowerRoom.getX() > higherRoom.getX() && lowerRoomEndX == higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(lowerRoom.getX() + 1, lowerRoomEndX - 1);
                                    } else if (lowerRoom.getX() < higherRoom.getX() && lowerRoomEndX == higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(higherRoom.getX() + 1, lowerRoomEndX - 1);
                                    } else if (lowerRoom.getX() == higherRoom.getX() && lowerRoomEndX > higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(lowerRoom.getX() + 1, higherRoomEndX - 1);
                                    } else if (lowerRoom.getX() == higherRoom.getX() && lowerRoomEndX < higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(lowerRoom.getX() + 1, lowerRoomEndX - 1);
                                    } else if (lowerRoom.getX() == higherRoom.getX() && lowerRoomEndX == higherRoomEndX)
                                    {
                                        corridorStartX = Random.Range(lowerRoom.getX() + 1, lowerRoomEndX - 1);
                                    }
									
									// draw the simple line connection if not stacked
                                    if (!stacked)
                                    {
                                        drawVerticalPath(tilemap, corridorStartX, corridorStartY, 0, higherRoom.getY(), 0, "up");
                                    } else
                                    {
										// a stack means that two of the wall coordinates need to be changed, rather than an entire corridor drawn
                                        tilemap.SetTile(tilemap.WorldToCell(new Vector3(corridorStartX, lowerRoomEndY, 0)), floor);
                                        tilemap.SetTile(tilemap.WorldToCell(new Vector3(corridorStartX, higherRoom.getY(), 0)), floor);
                                    }
                                }
                                stacked = false;
                            }
							// connect the rooms so they don't have multiple corridors drawn
                            room.setConnected(true);
                            nextRoom.setConnected(true);
                        }
                    }
                }
            }
        }
    }
	
	/* Partitions the roomList into two separate lists around the pivot
	 * @param left - the left index
	 * @param right - the right index
	 * @param mode - whether x or y
	 * @return Int
	 */
    private int partitionRoomList(List<Room> roomList, int left, int right, char mode)
    {
		// partition the list of rooms around the pivot
        int start = left;
        int pivot = roomList[start].getXY(mode);
        Room pivotRoom = roomList[start];
        left++;
        right--;
		
        while (true)
        {
			// partition elements smaller than the pivot
            while (left <= right && roomList[left].getXY(mode) <= pivot)
                left++;

			// partition elements larger than the pivot
            while (left <= right && roomList[right].getXY(mode) > pivot)
                right--;
			
			// found the smallest element
            if (left > right)
            {
                roomList[start] = roomList[left - 1];
                roomList[left - 1] = pivotRoom;

                return left;
            }

            int temp = roomList[left].getXY(mode);
            Room tempRoom = roomList[left];
            roomList[left] = roomList[right];
            roomList[right] = tempRoom;
        }
    }
	
	/* Sorts a list of rooms in terms of their mode from smallest to biggest
	 * @param left - the left index
	 * @param right - the right index
	 * @param mode - whether x or y
	 * @return List<Room>
	 */
    private List<Room> quickSort(List<Room> roomList, int left, int right, char mode)
    {
		// ensuring no faulty lists are passed in
        if (roomList == null || roomList.Count <= 1)
            return null;
		
		// if the list still needs sorting
        if (left < right)
        {
            int pivot = partitionRoomList(roomList, left, right, mode);
            quickSort(roomList, left, pivot - 1, mode);
            quickSort(roomList, pivot, right, mode);
        }

        return roomList;
    }
}