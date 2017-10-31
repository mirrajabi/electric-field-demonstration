using UnityEngine;
using System.Collections.Generic;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    public const string TAG_GUIDE_RULE = "GuideRule";
    public const string TAG_TEST_CHARGE = "TestCharge";
    public const string TAG_QPARTICLE = "QParticle";

    public GUIStyle GuiStyle;
    public GameObject ParticlePrefab;
    public GameObject GuideRulePrefab;
    public GameObject TestChargePrefab;

    public Material ChargePositive;
    public Material ChargeNegative;

    public static List<QParticle> Particles;

    public KeyCode InsertKey = KeyCode.A;
    public KeyCode DeleteKey = KeyCode.D;
    public KeyCode InvertChargeKey = KeyCode.R;
    public KeyCode SetChargeKey = KeyCode.S;
    public KeyCode ClearAllKey = KeyCode.C;
    public KeyCode MoveKey = KeyCode.LeftControl;

    private Vector3 _cameraTopLeft;
    private Vector3 _cameraTopRight;
    private Vector3 _cameraBottomLeft;
    private Vector3 _cameraBottomRight;

    private Vector3 _depthOffset = Vector3.forward * 10;
    private float _horizontalRuleWidth;
    private float _verticalRuleHeight;
    private float _ruleTickness = 0.01f;

    public float RowHeight = 0.05f;
    public float ColumnWidth = 0.05f;

    private int _oldScreenWidth = 0;
    private int _oldScreenHeight = 0;

    private string _inputValue = "";
    private Transform _selectedParticle;

    private float _oldRowHeight;
    private float _oldColumnWidth;

    public static bool NormalizeVectors { get; set; }
    public static float VectorLengthMultiplier { get; set; }

    private void Start ()
    {
        _oldRowHeight = RowHeight;
        _oldColumnWidth = ColumnWidth;

        Instance = this;
        NormalizeVectors = true;
        VectorLengthMultiplier = 1;
        Particles = new List<QParticle>();
    }

    private void OnGUI()
    {
        RowHeight = GUI.HorizontalSlider(new Rect(10, 80, 200, 20), RowHeight, 0.1f, 0.8f);
        ColumnWidth = GUI.HorizontalSlider(new Rect(10, 110, 200, 20), ColumnWidth, 0.1f, 0.8f);
        VectorLengthMultiplier = GUI.HorizontalSlider(new Rect(10, 140, 200, 20), VectorLengthMultiplier,0f, 6f);
        NormalizeVectors = GUI.Toggle(new Rect(10, 170, 20, 20), NormalizeVectors, "Normalize Vectors");

        if (_oldScreenWidth != Screen.width || _oldScreenHeight != Screen.height
            ||RowHeight != _oldRowHeight || ColumnWidth != _oldColumnWidth)
        {
            UpdateGrids();
            _oldScreenWidth = Screen.width;
            _oldScreenHeight = Screen.height;

            _oldRowHeight = RowHeight;
            _oldColumnWidth = ColumnWidth;
        }

        _inputValue = GUI.TextArea(new Rect(10, 10, 200, 60), _inputValue);
    }

    private void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            DeleteParticle(true);
        }*/
        if (Input.GetKeyDown(InsertKey))
        {
            InstantiateNewParticle();
        }
        if (Input.GetKeyDown(DeleteKey))
        {
            DeleteParticle(false);
        }
        if (Input.GetKeyDown(ClearAllKey))
        {
            NormalizeVectors = true;
            VectorLengthMultiplier = 1;
            ClearAll();
        }
        if (Input.GetKeyDown(InvertChargeKey)/* || Input.GetMouseButtonDown(2)*/)
        {
            InvertCharge();
        }
        if (Input.GetKeyDown(SetChargeKey))
        {
            int value;
            if(int.TryParse(_inputValue,out value))
            {
                SetChargeValue(value);
            }
        }

        MoveParticle();
    }
    
    private void UpdateGrids()
    {
        RemoveOldRules();

        _cameraTopLeft = Camera.main.ScreenToWorldPoint(_depthOffset);
        _cameraTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0) + _depthOffset);
        _cameraBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0) + _depthOffset);
        _cameraBottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0) + _depthOffset);

        _verticalRuleHeight = Vector3.Distance(_cameraBottomLeft, _cameraTopLeft);
        _horizontalRuleWidth = Vector3.Distance(_cameraTopRight, _cameraTopLeft);

        int rowsCount = (int)(_verticalRuleHeight / RowHeight) + 1;
        int columnsCount = (int)(_horizontalRuleWidth / ColumnWidth) + 1;

        for (int i = 0; i < rowsCount; i++)
        {
            Vector3 pos = new Vector3((_cameraTopLeft.x + _cameraTopRight.x) / 2, _cameraTopLeft.y + i * _verticalRuleHeight / rowsCount, 0);

            GameObject newRule = (GameObject)Instantiate(GuideRulePrefab, pos, Quaternion.identity);
            newRule.transform.localScale = new Vector3(_horizontalRuleWidth, _ruleTickness, _ruleTickness);
        }
        for (int i = 0; i < columnsCount; i++)
        {
            Vector3 pos = new Vector3(_cameraTopLeft.x + i * _horizontalRuleWidth / columnsCount, (_cameraTopLeft.x + _cameraTopRight.x) / 2, 0);

            GameObject newRule = (GameObject)Instantiate(GuideRulePrefab, pos, Quaternion.identity);
            newRule.transform.localScale = new Vector3(_ruleTickness,_verticalRuleHeight, _ruleTickness);
        }

        DrawTestCharges(rowsCount,columnsCount);
    }

    private void DrawTestCharges(int rowsCount, int columnsCount)
    {
        RemoveOldTestCharges(); 
        for (int i = 0; i <= rowsCount; i++)
        {
            for (int j = 0; j <= columnsCount; j++)
            {
                Vector3 pos = new Vector3(_cameraTopLeft.x + j * _horizontalRuleWidth / columnsCount, _cameraTopLeft.y + i * _verticalRuleHeight / rowsCount, 0);

                GameObject newRule = (GameObject)Instantiate(TestChargePrefab, pos, Quaternion.identity);
                newRule.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
    }

    private void RemoveOldRules()
    {
        GameObject[] oldRules = GameObject.FindGameObjectsWithTag(TAG_GUIDE_RULE);
        for (int i = 0; i < oldRules.Length; i++)
        {
            Destroy(oldRules[i]);
        }
    }

    private void RemoveOldTestCharges()
    {
        GameObject[] oldCharges = GameObject.FindGameObjectsWithTag(TAG_TEST_CHARGE);
        for (int i = 0; i < oldCharges.Length; i++)
        {
            Destroy(oldCharges[i]);
        }
    }

    private void InstantiateNewParticle()
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
        GameObject newObject = (GameObject)Instantiate(ParticlePrefab, point, Quaternion.identity);
        QParticle newParticle = newObject.GetComponent<QParticle>() as QParticle;
        Particles.Add(newParticle);
    }

    private void MoveParticle()
    {
        if (Input.GetKeyDown(MoveKey)/* || Input.GetMouseButtonDown(1)*/)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.tag == TAG_QPARTICLE)
                    {
                        _selectedParticle = hit.transform;
                    }
                }
            }
        }
        if (Input.GetKey(MoveKey)/* || Input.GetMouseButton(1)*/)
        {
            MoveSelected();
        }
        if (Input.GetKeyUp(MoveKey)/* || Input.GetMouseButtonUp(1)*/)
        {
            _selectedParticle = null;
        }
    }

    private void MoveSelected()
    {
        if(_selectedParticle != null)
        {
            _selectedParticle.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
        }
    }

    private void DeleteParticle(bool instantiateOnSpace)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)){
            if(hit.collider != null)
            {
                if (hit.collider.gameObject.tag == TAG_QPARTICLE)
                {
                    Particles.Remove(hit.collider.gameObject.GetComponent<QParticle>());
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        else
        {
            if (instantiateOnSpace)
                InstantiateNewParticle();
        }
    }

    private void InvertCharge()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == TAG_QPARTICLE)
                {
                    hit.collider.gameObject.GetComponent<QParticle>().InvertCharge();
                }
            }
        }
    }

    private void SetChargeValue(int value)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == TAG_QPARTICLE)
                {
                    hit.collider.gameObject.GetComponent<QParticle>().SetValue(value);
                }
            }
        }
    }

    private void ClearAll()
    {
        GameObject[] oldRules = GameObject.FindGameObjectsWithTag(TAG_QPARTICLE);
        for (int i = 0; i < oldRules.Length; i++)
        {
            Particles.Remove(oldRules[i].GetComponent<QParticle>());
            Destroy(oldRules[i]);
        }
    }
}
