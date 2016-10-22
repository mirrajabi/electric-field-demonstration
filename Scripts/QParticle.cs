using UnityEngine;
using System.Collections;

public class QParticle : MonoBehaviour
{
    public long ElectricCharge;
	
	void Start()
    {
        ElectricCharge = 1;
	}

    public void SetValue(long value)
    {
        ElectricCharge = value;
        if(value >= 0)
        {
            GetComponent<Renderer>().material = EntityManager.Instance.ChargePositive;
        }
        else
        {
            GetComponent<Renderer>().material = EntityManager.Instance.ChargeNegative;
        }
    }

    public void InvertCharge()
    {
        SetValue(-ElectricCharge);
    }
}
