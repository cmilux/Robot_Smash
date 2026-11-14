using UnityEngine;

//credits text, written once and shown wherever
[CreateAssetMenu(fileName = "Credits", menuName = "UI/Credits")]
public class CreditsData : ScriptableObject
{
    [TextArea(5, 25)] public string text;
}
