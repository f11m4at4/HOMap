using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// m x n 짜리 타일맵 만들기
public class HoMap : MonoBehaviour {
    public GameObject floorTile;
    public bool drawGrid = false;
    // 타일 m x n 변수 선언
    public int tileX = 1;
    public int tileY = 1;

    public List<GameObject> prefabTileObj = new List<GameObject>();

    [System.NonSerialized]
    public GameObject selectedPrefab;
    public Rect thumbnailSize = new Rect(10, 20, 50, 50);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
