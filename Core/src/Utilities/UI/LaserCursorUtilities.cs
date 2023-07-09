using SLZ.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class LaserCursorUtilities {
        public static LaserCursor CreateLaserCursor(Action<LaserCursor> onSetupRegions = null) {
            var instance = GameObject.Instantiate(FusionContentLoader.LaserCursor);
            var transform = instance.transform;
            instance.SetActive(false);

            var cursor = instance.AddComponent<LaserCursor>();

            var arrow = transform.Find("Arrow").SetupPageElementView();
            var ray_start = transform.Find("ray_start").SetupPageElementView();
            var ray_mid = transform.Find("ray_mid").SetupPageElementView();
            var ray_mid2 = transform.Find("ray_mid2").SetupPageElementView();
            var ray_bez = transform.Find("ray_bez").SetupPageElementView();
            var ray_end = transform.Find("ray_end").SetupPageElementView();
            var ray_pulse = transform.Find("ray_pulse").SetupPageElementView();
            var sfx = transform.Find("SFX").SetupPageElementView();

            var growCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0f, 1f),
                new Keyframe(0.1271991f, 1.953225f),
                new Keyframe(0.724905f, 0.9818711f),
                new Keyframe(1f, 1f),
            });
            ray_start.blipCurve = growCurve;
            ray_mid.blipCurve = growCurve;
            ray_mid2.blipCurve = growCurve;
            ray_end.blipCurve = growCurve;

            // Create the bezier drawer
            var drawBezierCurve = arrow.gameObject.AddComponent<DrawBezierCurve>();
            drawBezierCurve.Point1 = ray_end.gameObject;
            drawBezierCurve.Point2 = ray_bez.gameObject;
            drawBezierCurve.Point3 = ray_mid.gameObject;
            drawBezierCurve.Point4 = ray_start.gameObject;
            drawBezierCurve.lineSteps = 12;
            drawBezierCurve.linePercentageFill = 0.73f;

            // Create the prismatic SFX
            var prismaticSFX = sfx.gameObject.AddComponent<PrismaticSFX>();
            prismaticSFX.velocityTran = sfx.transform;
            prismaticSFX.sourceTran = sfx.transform;
            prismaticSFX.minSpeed = 0.2f;
            prismaticSFX.maxSpeed = 4f;
            prismaticSFX.sourceMinDistance = 1f;
            prismaticSFX.pitchMod = 1f;
            prismaticSFX.enableModulatedAudio = true;
            prismaticSFX.loopClips = true;
            prismaticSFX.modulatedClips = new AudioClip[] { FusionContentLoader.LaserPrismaticSFX };
            prismaticSFX.SpatialBlend = 0.98f;

            // Fill out the laser cursor
            cursor.cursorStart = ray_start;
            cursor.cursorEnd = ray_end;
            cursor.cursorMid = ray_mid;
            cursor.cursorMid2 = ray_mid2;
            cursor.cursorBez = ray_bez;
            cursor.rayPulse = ray_pulse;
            cursor.lineEnd = arrow;
            cursor.cursorLine = arrow.GetComponent<LineRenderer>();
            cursor.bezCurve = drawBezierCurve;
            cursor.pulseLength = 0.2f;
            cursor.pulseAceleration = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            cursor.pulseSound = new AudioClip[] { FusionContentLoader.LaserPulseSound };
            cursor.raySpawn = new AudioClip[] { FusionContentLoader.LaserRaySpawn };
            cursor.rayDespawn = new AudioClip[] { FusionContentLoader.LaserRayDespawn };
            cursor.prismaticSFX = prismaticSFX;
            cursor.spatialBlend = 1f;
            cursor._sourceVolume = 0.3f;
            cursor._sourceRadius = 1f;
            cursor.cursorHidden = true;
            cursor.canShowCursor = true;
            cursor.maxMillimeters = 16;

            onSetupRegions?.Invoke(cursor);

            return cursor;
        }

        private static PageElementView SetupPageElementView(this Transform transform) {
            var highlightUI = transform.gameObject.AddComponent<HighlightUI>();
            highlightUI.color1 = new Color(1f, 1f, 1f, 0.1294118f);
            highlightUI.color2 = Color.white;

            var pageElementView = transform.gameObject.AddComponent<PageElementView>();
            pageElementView.highlightColor1 = new Color(1f, 1f, 1f, 0f);
            pageElementView.highlightColor2 = Color.white;
            pageElementView.color1 = new Color(1f, 1f, 1f, 0f);
            pageElementView.color2 = Color.white;
            pageElementView.blipCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

            pageElementView.elements = new HighlightUI[] { highlightUI };

            return pageElementView;
        }
    }
}
