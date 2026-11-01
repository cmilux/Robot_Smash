using System;
using UnityEngine;

public enum TutorialStep
{
    Move,
    HitBox,
    PickUp,
    OpenInventory,
    Craft,
    Equip,
    Drop,
    DistanceAttack,
    MeleeAttack
}

//Local only event hub (not network).
//Gameplay scripts call Raise() when the local player does something and the TutorialManager listens
//always guards calls with isowner so remote player dont trigger own tutorial
public class TutorialEvents : MonoBehaviour
{
    //raised when a tutorial step is completed
    public static event Action<TutorialStep> OnStepDone;
    public static void Raise(TutorialStep step) => OnStepDone?.Invoke(step);

    //raised when tutorial is done/skipped
    public static event Action OnTutorialCompleted;
    public static void RaiseCompleted() => OnTutorialCompleted?.Invoke();
}
