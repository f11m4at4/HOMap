using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(HoMap))]
public class HoMapEditor : Editor {
    // delete 키가 눌렸는지 확인
    bool isDeleteKeyDown = false;
    HoMap _map;
    void OnEnable()
    {
        _map = target as HoMap;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Event e = Event.current;


        // 바닥 타일오브젝트
        _map.floorTile = (GameObject) EditorGUILayout.ObjectField("Floor tile prefab", _map.floorTile, typeof(GameObject));

        _map.tileX = EditorGUILayout.IntField("Width", _map.tileX);
        if (_map.tileX < 1) _map.tileX = 1;
        _map.tileY = EditorGUILayout.IntField("Height", _map.tileY);
        if (_map.tileY < 1) _map.tileY = 1;

        EditorGUILayout.Space();
        _map.drawGrid = EditorGUILayout.Toggle("Draw Grid", _map.drawGrid);
        EditorGUILayout.Space();

        // 리스트에 타일을 추가 하기 위한 정의
        GameObject obj = (GameObject)EditorGUILayout.ObjectField("Add Tile Prefab ", null, typeof(GameObject));
        
       
        // 오브젝트가 null 이 아니면 리스트에 타일을 추가 하고 다시 빈칸으로 만든다.
        if(obj)
        {
            _map.prefabTileObj.Add(obj);
            obj = null;
        }
        // List 에 들어 있는 데이타 표시
        for (int i=0;i< _map.prefabTileObj.Count;i++)
        {
            GUI.SetNextControlName("tile" + i);
            _map.prefabTileObj[i] = (GameObject)EditorGUILayout.ObjectField("Tile Prefab " + i, _map.prefabTileObj[i], typeof(GameObject));
        }

        // 인스펙터뷰에서 선택된 리스트 엘레먼트를 구하기 위한 구현
        // GetNameOfFocusedControl 함수는 현재 선택된 리스트의 이름을 알려준다.
        // GUI.SetNextControlName("tile" + i); -> 이 항목에서 오브젝트의 이름을 등록해 주어야 아래 내용이 동작을 하게 된다.
        string cName = GUI.GetNameOfFocusedControl();

        if ( cName != null && cName.Length > 0)
        {
            // 이름에서 인덱스를 추출
            int idx = int.Parse(cName.Substring(4));

            // 현재 선택된 타일을 저장한다. -> Scene 뷰에서 선택 타일 보여주기 위함
            _map.selectedPrefab = _map.prefabTileObj[idx];            

            //Debug.Log("selected Item ----> " + _map.prefabTileObj[idx]);
            // delete 키로 지울때, 눌렸을때 한번이 아니라서 몇번 들어 올 수 있기 때문에            
            if (e.keyCode == KeyCode.Delete)
            {
                isDeleteKeyDown = true;
            }
            // keyUp 이벤트가 발생할때 isDeleteKeyDown 의 값이 참일때 제거
            if (e.type == EventType.keyUp)
            {
                if(isDeleteKeyDown)
                {
                    // 리스트에서 해당 이덱스 타일 제거
                    if (_map.prefabTileObj.Count > idx)
                    {
                        _map.prefabTileObj.RemoveAt(idx);
                    }
                }
                isDeleteKeyDown = false;
            }
        }

        Rect rect = new Rect(0, 0, Screen.width, 20);
        if(GUILayout.Button( "Create Map"))
        {
            CreateMap();
            SceneView.RepaintAll();
        }

        // Inspector 의 내용이 변했을때 호출 됨.
        if (GUI.changed)
        {
            //Debug.Log("Change");
          //  Repaint();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    // 선그리기 HoMap 의 tileX, tileY 기반으로.
    // 선 그리는 위치는 월드의 Center 를 기준으로 한다.
    // HoMap GameObject 의 기즈모는 제거
    void OnSceneGUI()
    {
        if (Application.isPlaying == true)
        {
            return;
        }

        // 씬뷰에서 다른 오브젝트가 선택 되지 않도록 함
        int id = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(id);

        // 선그리기
        if (_map.drawGrid)
        {
            DrawGrid();
        }
        //Handles.DrawCube(0, center, Quaternion.identity, 10);


        // 선택된 prefab 미리보기 표시
        ShowSelectedPrefab();

        // 타일 찍기
        MakeTileOnFloor();

        // gizmo 안보이게 제거해줌
        Tools.current = Tool.None;
    }

    void DrawGrid()
    {
        // 월드 좌표 벡터변수
        Vector3 center = Vector3.zero;

        // 선그리기
        Handles.color = Color.green;

        for (int i = 0; i < _map.tileY; i++)
        {
            for (int j = 0; j < _map.tileX; j++)
            {
                Handles.DrawLine(center + Vector3.right * j, center + Vector3.right * j + Vector3.forward * _map.tileY);
            }
            Handles.DrawLine(center + Vector3.forward * i, center + Vector3.forward * i + Vector3.right * _map.tileX);
        }

        Handles.DrawLine(center + Vector3.right * _map.tileX, center + Vector3.right * _map.tileX + Vector3.forward * _map.tileY);
        Handles.DrawLine(center + Vector3.forward * _map.tileY, center + Vector3.forward * _map.tileY + Vector3.right * _map.tileX);

    }

    // Create Map 버튼이 눌리면 호출
    // tileX, tileY 크기의 맵을 만든다.
    void CreateMap()
    {
        // 기본 바닥 타일이 없으면 맵을 만들지 않는다.
        if (_map.floorTile == null) return;

        Vector3 center = Vector3.zero;
        GameObject tile = (GameObject)Instantiate(_map.floorTile);

        
        tile.transform.localScale = new Vector3((_map.tileX), 1, (_map.tileY));
        tile.transform.position = center + Vector3.right * tile.transform.localScale.x * 0.5f + 
            Vector3.forward * tile.transform.localScale.z * 0.5f;

        // root 타일을 만든다. (Transform 만 있음)
        // root 타일 아래에 자식 오브젝트들( floor tile) 을 추가한다.
        GameObject tileRoot = GameObject.Find("TileRoot");
        // 기존에 root 타일이 있다면 제거한다.
        if(tileRoot)
        {
            //runtime 이 아닌 editor 프로그래밍에서는 Destory 함수는 동작하지 않는다.
            // DestoryImmediate 함수를 사용
            DestroyImmediate(tileRoot);
        }
        GameObject tiles = GameObject.Find("Tiles");
        if(tiles)
        {
            DestroyImmediate(tiles);
        }

        tileRoot = new GameObject();
        tileRoot.name = "TileRoot";
        tile.transform.parent = tileRoot.transform;
    }

    // Inspector 창에서 선택되어진 Tile prefab 을 보여준다.
    void ShowSelectedPrefab()
    {

        // 선택된 프리팹의 이미지 보여주기
        // GetAssetPreview 는 인자로 넣어준 오브젝트의 미리보기 이미지를 리턴한다.
        Texture2D preview = AssetPreview.GetAssetPreview(_map.selectedPrefab);
        
        if (preview == null)
        {
            // 미리보기 이미지가 없을 경우 오브젝트의 미리보기(프로젝트뷰에서 프리뷰창에 보이는 것과 동일)
            // 이미지를 받아 온다.
            preview = AssetPreview.GetMiniThumbnail(_map.selectedPrefab);
        }

        if (preview)
        {
            Handles.BeginGUI();

            GUI.Button(_map.thumbnailSize, preview);
           
            Handles.EndGUI();
        }
    }

    bool isKeyDown = false;
    // Floor 에 선택된 타일을 찍는다.
    // 클릭하거나 클릭한 상태로 드래그를 하면 찍게 된다.
    void MakeTileOnFloor()
    {
        Event e = Event.current;
        
        // mouse 왼쪽 버튼 클릭만 처리한다.
        if (e.button != 0)
            return;

        if(e.type == EventType.mouseDown)
        {
            isKeyDown = true;
        }
        else if(e.type == EventType.MouseUp)
        {
            isKeyDown = false;
            RaycastHit hit;
            if (e.shift && CheckRayHit("Tile", out hit))
            {
                DestroyImmediate(hit.transform.gameObject);
            }
        }
        
        // mouse 가 눌리고 있을때만 처리
        if (isKeyDown == false || e.shift || e.alt) return;
        
        // ray 와 충돌한 오브젝트가 Floor 일 경우
        // selectedPrefab 을 생성해 배치 한다.
        RaycastHit hitInfo;
        //Debug.Log("hitinfo");
        if (_map.selectedPrefab && CheckRayHit("Floor", out hitInfo))
        {
            // 인덱스 찾기
            int x = Mathf.FloorToInt(hitInfo.point.x % _map.tileX);
            int z = Mathf.FloorToInt(hitInfo.point.z % _map.tileY);
            //Debug.Log(" x : " + x + ", zy : " + z);
            Vector3 pos = new Vector3(x + 0.5f, hitInfo.point.y, z + 0.5f);
            // Floor 위에 다른 오브젝트가 있는지 여부 조사해서 있으면 더이상 그리지 않는다.
            Ray ray = new Ray(pos + Vector3.down, Vector3.up);
            RaycastHit hitTest;
            if (Physics.Raycast(ray, out hitTest))
                return;
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(_map.selectedPrefab);
            obj.name = "Tile" + _map.selectedPrefab.name;
            obj.transform.position = pos + Vector3.up * 0.5f;

            // Tiles 라는 오브젝트의 자식으로 생성한 타일을 등록한다.
            if(GameObject.Find("Tiles") == false)
            {
                GameObject tile = new GameObject();
                tile.name = "Tiles";
                tile.transform.parent = GameObject.Find("TileRoot").transform;
            }
            obj.transform.parent = GameObject.Find("Tiles").transform;
        }
    }

    bool CheckRayHit(string name, out RaycastHit hitInfo)
    {
        // Ray 를 쏴서 충돌체크
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            if (hitInfo.transform.name.Contains(name))
            {
                return true;                
            }
        }
        
        return false;
    }
}
