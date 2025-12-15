using UnityEngine;

public class InteractableObjectOutliner : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material outlineMaterial;
    private Material defaultMaterial;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int ThicknessID = Shader.PropertyToID("_Thickness");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");

    private void Awake() {
        if (!spriteRenderer) {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        defaultMaterial = spriteRenderer.material;
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetOutline(bool enable, Color? color = null, float thickness = 1f) {
        if (spriteRenderer == null) {
            return;
        }

        spriteRenderer.material = enable ? outlineMaterial : defaultMaterial;

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(ThicknessID, enable ? thickness : 0f);
        if (color.HasValue) {
            propertyBlock.SetColor(OutlineColorID, color.Value);
        }
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}