using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabReplacement : MonoBehaviour
{
    public enum Trigger
    {
        None,
        OnAwake,
        OnStart,
        OnEnable,
    }
    public Trigger trigger = Trigger.None;
    public GameObject prefab;
    
    void Awake()
    {
        if (trigger == Trigger.OnAwake)
        {
            //Debug.Log("Replace in Awake");
            DoSelfReplacing();
        }
    }

    void OnEnable()
    {
        if (trigger == Trigger.OnEnable)
        {
            //Debug.Log("Replace in OnEnable");
            DoSelfReplacing();
        }
    }

    void Start()
    {
        if (trigger == Trigger.OnStart)
        {
            //Debug.Log("Replace in Start");
            DoSelfReplacing();
        }
    }

    public void DoSelfReplacing(bool newMaterial = false, Material defaultNewMaterial = null)
    {
        GameObject go = null;
        if (prefab)
        {
            prefab.SetActive(true);
            if (transform.parent)
            {
                go = Instantiate<GameObject>(prefab, transform.parent, false);
            }
        }
        if (go)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }

        //Replace materials if needed
        if (newMaterial)
        {
            foreach (var rend in go.GetComponentsInChildren<Renderer>())
            {
                var matReplacer = rend.GetComponent<MaterialReplacer>();
                if (matReplacer != null && matReplacer.opponentMaterial != null)
                    rend.material = matReplacer.opponentMaterial;
                else
                {
                    if (defaultNewMaterial != null)
                    {
                        rend.material = defaultNewMaterial;
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}
