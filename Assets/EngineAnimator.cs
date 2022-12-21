using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoatAttack
{
    public class EngineAnimator : BaseController
    {
        Animator engineAnimator;

        public void Start()
        {
            engineAnimator = GetComponent<Animator>();
            engineAnimator.SetFloat("Speed", 0.5f);
        }

        private void Update()
        {
            CheckEngineAcceleration();
        }

        private void CheckEngineAcceleration()
        {
            if(engineAnimator.GetComponentInParent<HumanController>())
            {
                HumanController engineInput = GetComponentInParent<HumanController>();

                if (engineInput._throttle >= 0.1)
                {
                    float modifier = 1 + 1 * engineInput._throttle;
                    engineAnimator.SetFloat("Speed", modifier);
                }
                else
                {;
                    engineAnimator.SetFloat("Speed", 0.5f);
                }
            }
            
        }
    }

}

