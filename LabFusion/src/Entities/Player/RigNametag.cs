using Il2CppSLZ.Marrow;

using Il2CppTMPro;

using LabFusion.Extensions;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class RigNametag
{
    public const float NametagHeight = 0.23f;
    public const float NameTagDivider = 250f;

    public GameObject canvasGameObject;
    public Transform canvasTransform;
    public Canvas canvas;
    public TextMeshProUGUI text;

    private string _username = "No Name";
    private bool _isQuestUser = false;

    public void CreateNametag()
    {
        canvasGameObject = new GameObject("NAMETAG CANVAS");
        canvas = canvasGameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;
        canvasTransform = canvasGameObject.transform;
        canvasTransform.localScale = Vector3Extensions.one / NameTagDivider;

        text = canvasGameObject.AddComponent<TextMeshProUGUI>();

        text.alignment = TextAlignmentOptions.Midline;
        text.enableAutoSizing = true;
        text.richText = true;

        text.font = PersistentAssetCreator.Font;
    }

    public void UpdateSettings(RigManager rigManager)
    {
        var avatar = rigManager.avatar;

        if (!avatar)
        {
            return;
        }

        float height = avatar.height / 1.76f;
        canvasTransform.localScale = Vector3Extensions.one / NameTagDivider * height;

        UpdateText();
    }

    public void UpdateTransform(RigManager rigManager)
    {
        var head = rigManager.physicsRig.m_head;
        canvasTransform.position = head.position + Vector3Extensions.up * GetNametagOffset(rigManager);
        canvasTransform.LookAtPlayer();
    }

    public static float GetNametagOffset(RigManager rigManager)
    {
        float offset = NametagHeight;

        offset *= rigManager.avatar.height;

        return offset;
    }

    public void SetUsername(string username, bool isQuestUser)
    {
        _username = username;
        _isQuestUser = isQuestUser;

        UpdateText();
    }

    private void UpdateText()
    {
        if (text.IsNOC())
        {
            return;
        }

        // Only allow color
        text.text = _username.RemoveRichTextExceptColor();

        if (_isQuestUser)
        {
            text.text += " <size=60%>Q";
        }
    }

    public void DestroyNametag()
    {
        if (!canvasGameObject.IsNOC())
        {
            GameObject.DestroyObject(canvasGameObject);
        }
    }

    public void ToggleNametag(bool isActive)
    {
        canvasGameObject.SetActive(isActive);
    }
}
