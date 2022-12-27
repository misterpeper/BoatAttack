using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoatAttack
{
    public class BoatParticlesController : BaseController
    {
        [SerializeField] List<ParticleSystem> particleSystems;
        void Start()
        {
            particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
        }
        private void Update()
        {
            CheckEngineAcceleration();
        }
        private void CheckEngineAcceleration()
        {
            if (this.GetComponentInParent<HumanController>())
            {
                HumanController engineInput = GetComponentInParent<HumanController>();

                if (engineInput._throttle == 1f)
                {
                    PlayParticleSystems();
                }

                if (engineInput._throttle == 0f)
                {
                    StopParticleSystems();
                }
            }
        }

        void PlayParticleSystems()
        {
            foreach(ParticleSystem boatParticles in particleSystems)
            {
                boatParticles.enableEmission = true;
            }
        }

        void StopParticleSystems()
        {
            foreach (ParticleSystem boatParticles in particleSystems)
            {
                boatParticles.enableEmission = false;
            }
        }


    }
}

