namespace UnityEngine.UI
{
    /// <summary>
    /// 外发光Image
    /// </summary>
    public class GlowImage : Image
    {
        [SerializeField]
        [Range(0, 10)]
        private float mGlowSize = 2;
        public float glowSize
        {
            get
            {
                return mGlowSize;
            }
            set
            {
                if (mGlowSize != value)
                {
                    mGlowSize = value;
                    SetMaterialDirty();
                }
            }
        }

        [SerializeField]
        private Color mGlowColor = Color.white;
        public Color glowColor
        {
            get
            {
                return mGlowColor;
            }
            set
            {
                if (mGlowColor != value)
                {
                    mGlowColor = value;
                    SetMaterialDirty();
                }
            }
        }

        [SerializeField]
        [Range(0, 10)]
        private float mGlowIntensitive = 1;
        public float glowIntensitive
        {
            get
            {
                return mGlowIntensitive;
            }
            set
            {
                if (mGlowIntensitive != value)
                {
                    mGlowIntensitive = value;
                    SetMaterialDirty();
                }
            }
        }

        public enum GlowQuality
        {
            Low = 3,
            Medium = 5,
            High = 7
        }

        [SerializeField]
        private GlowQuality mGlowQuality = GlowQuality.Medium;
        public GlowQuality glowQuality
        {
            get
            {
                return mGlowQuality;
            }
            set
            {
                if (mGlowQuality != value)
                {
                    mGlowQuality = value;
                    SetMaterialDirty();
                }
            }
        }

        private Material mMaterial;

        public override Material materialForRendering
        {
            get
            {
                if (mMaterial == null) mMaterial = new Material(Shader.Find("UI/Unlit/Glow"));
                mMaterial.SetFloat("_BlurSize", glowSize);
                mMaterial.SetColor("_BlurColor", glowColor);
                mMaterial.SetFloat("_BlurIntensitive", glowIntensitive);
                string quality = glowQuality.ToString();
                string[] qualities = System.Enum.GetNames(glowQuality.GetType());
                for (int i = qualities.Length - 1; i >= 0; i--)
                {
                    if (qualities[i].Equals(quality)) mMaterial.EnableKeyword("QUALITY_" + qualities[i].ToUpper());
                    else mMaterial.DisableKeyword("QUALITY_" + qualities[i].ToUpper());
                }
                return mMaterial;
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (type == Type.Simple &&
                overrideSprite != null)
            {
                var padding = Sprites.DataUtility.GetPadding(overrideSprite);
                var size = new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);

                int spriteW = Mathf.RoundToInt(size.x);
                int spriteH = Mathf.RoundToInt(size.y);

                Rect r = GetPixelAdjustedRect();

                var v = new Vector4(
                        padding.x / spriteW,
                        padding.y / spriteH,
                        (spriteW - padding.z) / spriteW,
                        (spriteH - padding.w) / spriteH);

                if (preserveAspect && size.sqrMagnitude > 0.0f)
                {
                    var spriteRatio = size.x / size.y;
                    var rectRatio = r.width / r.height;

                    if (spriteRatio > rectRatio)
                    {
                        var oldHeight = r.height;
                        r.height = r.width * (1.0f / spriteRatio);
                        r.y += (oldHeight - r.height) * rectTransform.pivot.y;
                    }
                    else
                    {
                        var oldWidth = r.width;
                        r.width = r.height * spriteRatio;
                        r.x += (oldWidth - r.width) * rectTransform.pivot.x;
                    }
                }

                float unitPerPixel = 1 / pixelsPerUnit;

                Vector4 vp = new Vector4(r.xMin, r.yMin, r.xMax, r.yMax);
                Vector4 uvp = new Vector4(0, 0, 1, 1);

                Vector4 vc = new Vector4(
                    r.x + r.width * v.x,
                    r.y + r.height * v.y,
                    r.x + r.width * v.z,
                    r.y + r.height * v.w
                    );
                var uvc = Sprites.DataUtility.GetOuterUV(overrideSprite);
                Vector2 pixelFix = new Vector2(unitPerPixel / (vp.z - vp.x), unitPerPixel / (vp.w - vp.y));
                uvc += new Vector4(pixelFix.x, pixelFix.y, -pixelFix.x, -pixelFix.y);

                // extend size
                float gs = (glowSize + (int)glowQuality * 2) * unitPerPixel;
                Vector4 vgs = new Vector4(-gs, -gs, gs, gs);
                Vector4 diff = vp - vc - vgs;
                if (diff.x > 0)
                {
                    vp.x -= diff.x;
                    uvp.x -= diff.x / (vp.z - vp.x);
                }
                if (diff.y > 0)
                {
                    vp.y -= diff.y;
                    uvp.y -= diff.y / (vp.w - vp.y);
                }
                if (diff.z < 0)
                {
                    vp.z -= diff.z;
                    uvp.z -= diff.z / (vp.z - vp.x);
                }
                if (diff.w < 0)
                {
                    vp.w -= diff.w;
                    uvp.w -= diff.w / (vp.w - vp.y);
                }

                var color32 = color;
                toFill.Clear();
                toFill.AddVert(new Vector3(vc.x, vc.y), color32, new Vector2(uvc.x, uvc.y));
                toFill.AddVert(new Vector3(vc.x, vc.w), color32, new Vector2(uvc.x, uvc.w));
                toFill.AddVert(new Vector3(vc.z, vc.w), color32, new Vector2(uvc.z, uvc.w));
                toFill.AddVert(new Vector3(vc.z, vc.y), color32, new Vector2(uvc.z, uvc.y));
                toFill.AddVert(new Vector3(vp.x, vp.y), color32, new Vector2(uvp.x, uvp.y));
                toFill.AddVert(new Vector3(vp.x, vp.w), color32, new Vector2(uvp.x, uvp.w));
                toFill.AddVert(new Vector3(vp.z, vp.w), color32, new Vector2(uvp.z, uvp.w));
                toFill.AddVert(new Vector3(vp.z, vp.y), color32, new Vector2(uvp.z, uvp.y));
                for (int i = 0; i < 4; i++)
                {
                    int n = i + 1;
                    if (n == 4) n = 0;
                    toFill.AddTriangle(i + 4, n + 4, i);
                    toFill.AddTriangle(n + 4, n, i);
                }
                toFill.AddTriangle(0, 1, 2);
                toFill.AddTriangle(2, 3, 0);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mMaterial = null;
        }
    }
}