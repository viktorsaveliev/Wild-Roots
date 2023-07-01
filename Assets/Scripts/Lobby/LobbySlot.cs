using UnityEngine;
using UnityEngine.UI;

public class LobbySlot : MonoBehaviour
{
    [SerializeField] private Text _nickname;
    [SerializeField] private GameObject _character;

    public bool IsUsed { get; private set; }

    public void TakeSlot(string nickname, int skinID)
    {
        if (IsUsed)
        {
            print("[ERROR #131] Can not take a slot");
            return;
        }
        _nickname.text = nickname;
        IsUsed = true;

        _character.SetActive(true);
        _character.GetComponent<Character>().Skin.Change(skinID, true);
    }

    public void ResetSlot()
    {
        if (!IsUsed)
        {
            print("[ERROR #131] Slot specified incorrectly");
            return;
        }
        _character.SetActive(false);
        IsUsed = false;
    }
}
