using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSystemController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ParticleSystem _particle;
    void Start()
    {
        _particle.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
