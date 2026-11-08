using TMPro;
using UnityEngine;

/// <summary>Displays a short recovery message above its owning trolly.</summary>
public sealed class TrollyRecoveryNotice : MonoBehaviour
{
    private TextMeshPro _text;
    private float _hideAt;

    public void Show(string tag)
    {
        if (_text == null)
        {
            var label = new GameObject("Recovered item notice");
            label.transform.SetParent(transform, false);
            _text = label.AddComponent<TextMeshPro>();
            _text.font = TMP_Settings.defaultFontAsset;
            _text.fontSize = 2.5f;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = Color.white;
            _text.rectTransform.sizeDelta = new Vector2(1.3f, 0.3f);
            _text.enableWordWrapping = true;
        }
        _text.text = Helper.ConvertTagToFrenchName(tag) + " : objet replacé sur le chariot.";
        _text.gameObject.SetActive(true);
        _hideAt = Time.time + 4f;
        UpdatePose();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayNotificationSound();
    }

    private void LateUpdate()
    {
        if (_text == null || !_text.gameObject.activeSelf) return;
        if (Time.time >= _hideAt)
            _text.gameObject.SetActive(false);
        else
            UpdatePose();
    }

    private void UpdatePose()
    {
        _text.transform.position = transform.position + Vector3.up * 1.5f;
        Camera camera = Camera.main;
        if (camera != null)
            _text.transform.rotation = Quaternion.LookRotation(_text.transform.position - camera.transform.position);
    }
}
