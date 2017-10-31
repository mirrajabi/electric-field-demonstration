using UnityEngine;
using System.Collections;

public class TestCharge : MonoBehaviour
{
    public int Charge { get; set; }
    public const long K = 9000000000;

    private Vector3 _direction;

    private void Start()
    {
        Charge = 1;
    }

	private void Update ()
    {
        SetDirection();
	}

    private void SetDirection()
    {
        Vector3 direction = Vector3.zero;
        for(int i = 0; i < EntityManager.Particles.Count; i++)
        {
            QParticle particle = EntityManager.Particles[i];
            double distance = Vector3.Distance(particle.transform.position, transform.position);
            double f = particle.ElectricCharge / (distance * distance);
            direction += (float)f * (transform.position - particle.transform.position);
        }
        transform.localScale = new Vector3(0.015f, 0.015f,(EntityManager.NormalizeVectors ?  Mathf.Clamp(Mathf.Sqrt(direction.magnitude) / 4,0,0.3f) : Mathf.Sqrt(direction.magnitude) / 5) * EntityManager.VectorLengthMultiplier);
        direction.Normalize();
        transform.LookAt(transform.position + new Vector3(direction.x,  direction.y,0));
    }
}
