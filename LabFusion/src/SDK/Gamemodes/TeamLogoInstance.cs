using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Entities;

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

        private NetworkPlayer player;

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
            NetworkPlayerManager.TryGetPlayer(id, out player);

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
            if (player != null)
            {
                var rm = player.RigReferences.RigManager;

                if (!rm.IsNOC())
                {
                    var head = rm.physicsRig.m_head;

                    go.transform.position = head.position + Vector3Extensions.up * RigNametag.GetNametagOffset(rm);
                    go.transform.LookAtPlayer();
                }
            }
        }
    }
}
