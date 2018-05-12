using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

	public float explDiam;
    public GameObject terrain;


    // Use this for initializationz
    void Start () {
        //Debug.Log("BULLET ALIVE");
        //forceX = 5;
        terrain = GameObject.Find("Terrain");
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("ENTER");
        if (collision.gameObject.name == "Terrain")
        {
            collision.gameObject.GetComponent<TerrainScript>().TerrainHole(gameObject, explDiam);
            Destroy(gameObject);
        }
        //Destroy(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
