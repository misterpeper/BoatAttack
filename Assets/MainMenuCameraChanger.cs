using UnityEngine;
using Cinemachine;

public class MainMenuCameraChanger : MonoBehaviour
{
    private Animator animator;

    private bool menuCamera = true;
    private bool harborCamera = false;
    private bool boatCamera = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void OnRaceButtonPressed()
    {
        if (menuCamera)
        {
            animator.Play("HarborCamera");
        }
        else
        {
            animator.Play("MenuCamera");
        }

        menuCamera = !menuCamera;
    }

    public void OnBoatButtonPressed()
    {

    }

    public void OnBackButtonPressed()
    {

    }
}
