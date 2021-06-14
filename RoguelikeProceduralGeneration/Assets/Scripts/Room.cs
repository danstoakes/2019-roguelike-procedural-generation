/*
 * Author: Dan Stoakes
 * Purpose: Class for defining the room datatype for room generation.
 * Date: 15/11/2019
 */

public class Room
{
    private int x;
    private int y;
    private int w;
    private int h;
    private bool connected;

    // constructor for Room, assigning all defining characteristics
    public Room(int xLeft, int yTop, int width, int height, bool corridor)
    {
        x = xLeft;
        y = yTop;
        w = width;
        h = height;
        connected = corridor;
    }

    /* Sets whether the room is connected or not
	 * @return void
	 */
    public void setConnected(bool corridor)
    {
        connected = corridor;
    }

    /* Gets the connected state
	 * @return Bool whether the room is connected
	 */
    public bool getConnected()
    {
        return connected;
    }

    /* Gets the x coordinate
	 * @return Int x coordinate
	 */
    public int getX()
    {
        return x;
    }

    /* Gets the y coordinate
	 * @return Int y coordinate
	 */
    public int getY()
    {
        return y;
    }

    /* Gets the desired (x or y) coordinate
	 * @return Int whichever coordinate
	 */
    public int getXY(char which)
    {
        if (which == 'x')
        {
            return x;
        }
        return y;
    }

    /* Gets the width
	 * @return Int width
	 */
    public int getWidth()
    {
        return w;
    }

    /* Gets the height
	 * @return Int height
	 */
    public int getHeight()
    {
        return h;
    }
}