using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerScript : MonoBehaviour {


    private float explDiam = 10;
    public GameObject terrain;

    // Use this for initialization
    void Start () {
        
    }


    // Update is called once per frame
    void Update () {
        //двигаем дестроер за курсором
        gameObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
    }

    private void OnMouseDown()
    {
        //красим красным
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;

        // делаем дырку в земле размера дестроера
        terrain.GetComponent<TerrainScript>().TerrainHole(gameObject, gameObject.transform.localScale.x);

        Debug.Log("FIRE_dest");
    }

    private void OnMouseUp()
    {
        //красим назад белым
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
