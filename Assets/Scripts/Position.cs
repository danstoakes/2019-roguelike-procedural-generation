/*
 * Author: Dan Stoakes
 * Purpose: Class for defining the position datatype used in A* search.
 * Date: 17/11/2019
 */

public class Position
{
    private int x;
    private int y;

    private int F;
    private int G;
    private int H;
    private Position parent;

    // constructor for Position, taking the x and y coordinates
    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /* Sets the x coordinate
	 * @return Void
	 */
    public void setX(int newX)
    {
        x = newX;
    }

    /* Gets the x coordinate
	 * @return Int x coordinate
	 */
    public int getX()
    {
        return x;
    }

    /* Sets the y coordinate
	 * @return Void
	 */
    public void setY(int newY)
    {
        y = newY;
    }

    /* Gets the y coordinate
	 * @return Int y coordinate
	 */
    public int getY()
    {
        return y;
    }

    /* Sets the F value for A*
	 * @return Void
	 */
    public void setF(int newF)
    {
        F = newF;
    }

    /* Gets the F value
	 * @return Int F value
	 */
    public int getF()
    {
        return F;
    }

    /* Sets the G value for A*
	 * @return Void
	 */
    public void setG(int newG)
    {
        G = newG;
    }

    /* Gets the G value
	 * @return Int G value
	 */
    public int getG()
    {
        return G;
    }

    /* Sets the H value for A*
	 * @return Void
	 */
    public void setH(int newH)
    {
        H = newH;
    }

    /* Gets the H value
	 * @return Int H value
	 */
    public int getH()
    {
        return H;
    }

    /* Sets the parent for A*
	 * @return Void
	 */
    public void setParent(Position newParent)
    {
        parent = newParent;
    }

    /* Gets the parent for A*
	 * @return Position parent
	 */
    public Position getParent()
    {
        return parent;
    }
}