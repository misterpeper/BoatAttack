using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoatAttack
{
    public class EngineAnimator : BaseController
    {
        Animator engineAnimator;

        [SerializeField] float stepAnimLerp = 0.5f;
        [SerializeField] float initialAnimSpeed = 0f;
        [SerializeField] float finalAnimSpeed = 4f;

        private float dynamicEngineOnModifier;
        private float dynamicEngineOffModifier;
        private float engineOffConst = 0.1f; //Engine constant off animation speed 

        public void Start()
        {
            engineAnimator = GetComponent<Animator>();
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

                if (engineInput._throttle == 1f)
                {
                    StartCoroutine(Increment());
                }

                if(engineInput._throttle == 0f)
                {
                    StartCoroutine(Dicrement());
                }
            }

            else if (engineAnimator.GetComponentInParent<AiController>())
            {
                engineAnimator.SetFloat("Speed", 2f);
            }
        }
        IEnumerator Increment() // Increment engine speed parameter when player pressing throttle input
        {
            if (initialAnimSpeed <= finalAnimSpeed)
            {
                initialAnimSpeed += stepAnimLerp * Time.fixedDeltaTime;
            }

            dynamicEngineOnModifier = initialAnimSpeed;
            
            yield return null;

            engineAnimator.SetFloat("Speed", dynamicEngineOnModifier);
        }

        IEnumerator Dicrement() // Dicrement engine speed parameter when player is realeasing throttle button
        {
            if (dynamicEngineOnModifier >= engineOffConst)
            {
                dynamicEngineOnModifier -= stepAnimLerp * Time.fixedDeltaTime;
            }

            dynamicEngineOffModifier = dynamicEngineOnModifier;

            yield return null;

            engineAnimator.SetFloat("Speed", dynamicEngineOffModifier);
        }
    }
}

