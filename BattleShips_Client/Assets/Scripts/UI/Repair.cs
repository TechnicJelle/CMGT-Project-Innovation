using Shared.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Repair : MonoBehaviour
    {
        private Button _button;
        private TMP_Text _buttonText;
        
        private string _defaultText;
        [SerializeField] private string repairingText = "Repairing...";
        [SerializeField] private string doneText = "Repaired!";

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TMP_Text>();
            _defaultText = _buttonText.text;
            
            _button.onClick.AddListener(RequiestRepair);
            WebsocketClient.Instance.OnRepairDone += RepairDone;
        }

        private void OnEnable()
        {
            _button.interactable = true;
            _buttonText.text = _defaultText;
        }

        private void RequiestRepair()
        {
            WebsocketClient.Instance.Send(MessageFactory.CreateSignal(MessageFactory.MessageType.RepairingSignal));
            _button.interactable = false;
            _buttonText.text = repairingText;
            SoundManager.Instance.PlaySound(SoundManager.Sound.Repairing);
        }

        private void RepairDone()
        {
            _buttonText.text = doneText;
        }
    }
}
