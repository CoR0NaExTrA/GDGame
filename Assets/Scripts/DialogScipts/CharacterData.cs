using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Dialog/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private AudioClip _talkSfx;

    public string Name { get => _name; }
    public Sprite Portrait { get => _portrait; }
    public AudioClip TalkSfx { get => _talkSfx; }

}
