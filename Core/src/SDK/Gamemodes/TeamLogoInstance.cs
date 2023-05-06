using LabFusion.Extensions;
using LabFusion.Representation;
using UnityEngine.UI;
using UnityEngine;

namespace LabFusion.SDK.Gamemodes
{
    public class TeamLogoInstance
    {
        protected const float LogoDivider = 270f;

        public Team team;
        public PlayerId playerId;

        private GameObject go;
        private Canvas canvas;
        private RawImage image;

        private PlayerRep rep;

        public TeamLogoInstance(PlayerId id, Team team)
        {
            go = new GameObject($"{id.SmallId} Team Logo");

            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100000;
            go.transform.localScale = Vector3Extensions.one / LogoDivider;

            image = go.AddComponent<RawImage>();

            GameObject.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.DontUnloadUnusedAsset;

            playerId = id;
            PlayerRepManager.TryGetPlayerRep(id, out rep);

            this.team = team;

            UpdateLogo(team.Logo);
        }

        public void Toggle(bool value)
        {
            go.SetActive(value);
        }

        public void UpdateLogo(Texture2D logoTexture)
        {
            image.texture = logoTexture;
        }

        public void Cleanup()
        {
            if (!go.IsNOC())
                GameObject.Destroy(go);
        }

        public bool IsShown() => go.activeSelf;

        public void Update()
        {
            if (rep != null)
            {
                var rm = rep.RigReferences.RigManager;

                if (!rm.IsNOC())
                {
                    var head = rm.physicsRig.m_head;

                    go.transform.position = head.position + Vector3Extensions.up * rep.GetNametagOffset();
                    go.transform.LookAtPlayer();
                }
            }
        }
    }
}
