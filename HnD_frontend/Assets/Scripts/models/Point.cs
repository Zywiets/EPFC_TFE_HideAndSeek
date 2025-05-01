using System;
using UnityEngine;

[Serializable]
public class Point : ICloneable {
    public float[] position;
    public float[] rotation;

    public Point() {
        
    }
    
    public Point(GameObject point) {
        if (point == null) {return;}
        var transform1 = point.transform;
        var position1 = transform1.position;
        position = new float[] {
            position1.x,
            position1.y,
            position1.z
        };
        var rotation1 = transform1.eulerAngles; //used instead of rotation to get angles in degrees
        rotation = new float[] {
            rotation1.x,
            rotation1.y,
            rotation1.z
        };
    }
    
    public object Clone()
    {
        Point copy = new Point()
        {
            position = (float[])this.position?.Clone(), // Deep copy array
            rotation = (float[])this.rotation?.Clone()
        };
        return copy;
    }
}