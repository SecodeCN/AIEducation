using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCreate : MonoBehaviour
{

    public GameObject Demo = null;

    public Transform parent = null;

    public float LengthTimes = 1f;

    void Start()
    {
        if (parent == null)
            parent = this.transform;
        InitCube();
    }

    public int Count = 0;

    private const int max = 30;

    private bool[] IsCube = null;

    public void InitCube()
    {
        IsCube = new bool[max * max * max];
        if (Demo == null || parent == null)
        {
            print("Static Things Loss");
        }
        Count = Random.Range(8, max - 1);
        var index = 0;
        while (index < Count)
        {
            index += 1;
            var x = 0;
            var y = 0;
            var z = 0;
            while (IsCube[x + y * max + z * max * max])
            {
                var cnt = Random.Range(0, 3);
                if (cnt == 0)
                {
                    x += 1;
                }
                else if (cnt == 1)
                {
                    y += 1;
                }
                else
                {
                    z += 1;
                }
            }
            CreateGameObject(x, y, z);
            IsCube[x + y * max + z * max * max] = true;
        }
    }

    private void CreateGameObject(int x, int y, int z)
    {
        var go = Instantiate(Demo);
        go.transform.parent = parent;
        go.transform.position = new Vector3(x, y, z) * LengthTimes + parent.position;
        go.transform.localScale = new Vector3(1, 1, 1) * LengthTimes;
        go.name = x + "." + y + "." + z;
    }

    void Update()
    {

    }

    new public void print(object obj)
    {
        MonoBehaviour.print(obj);
    }
}
