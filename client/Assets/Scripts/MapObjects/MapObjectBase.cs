using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapObjectBase : MonoBehaviour {
    
    [Header("MapObject Base")]
    [SerializeField] protected Transform visual;
    [SerializeField] protected MapObjectData data;
    public MapObjectData Data => data;

    protected SpriteRenderer spriteRenderer;
    protected PolygonCollider2D collider;
    protected SnapableObject snapableObject;
    protected int rotationIndex = 0;
    public int RotationIndex => rotationIndex;

    protected void OnEnable() {
        Init();
        snapableObject.SnapToGrid();
        snapableObject.SnapToGrid();
    }

    protected void Update() {
        
    }

    public void Init(MapObjectData newData = null) {
        if(newData != null) {
            data = newData;
        }

        if (data != null) {
            // 스프라이트 적용
            if (data.Sprites != null && data.Sprites.Count > 0) {
                spriteRenderer.sprite = data.Sprites[rotationIndex];
            }
            ApplyRotation();

            // 콜라이더 적용
            ApplyCollider();
        }
    }


    protected virtual void Awake() {
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        collider = GetComponent<PolygonCollider2D>();
        snapableObject = GetComponent<SnapableObject>();
    }

    protected void ApplyCollider() {
        if (collider == null) { return; }

        if (data.UseSpriteCollider && spriteRenderer.sprite != null) {
            // 스프라이트 물리 콜라이더 적용
            var sprite = spriteRenderer.sprite;
            int shapeCount = sprite.GetPhysicsShapeCount();
            collider.pathCount = shapeCount;

            var path = new List<Vector2>();
            for (int i = 0; i < shapeCount; i++) {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                collider.SetPath(i, path);
            }
        }
        else {
            // 기본 패스 (한 칸 꽉 채우기) 적용
            collider.pathCount = 1;

            Vector2[] defaultPath = new Vector2[] {
                new Vector2(0f, -0.5f),
                new Vector2(1f,  0f),
                new Vector2(0f,  0.5f),
                new Vector2(-1f, 0f)
            };
            collider.SetPath(0, defaultPath);
        }
    }

    public void ApplyRotation() {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                int safeIndex = Mathf.Clamp(rotationIndex, 0, data.Sprites.Count - 1);
                spriteRenderer.sprite = data.Sprites[safeIndex];
                spriteRenderer.flipX = false;
            }
            else if (data.UseFlipX) {
                spriteRenderer.sprite = data.Sprites[0];
                spriteRenderer.flipX = (rotationIndex % 2 == 1);
            }
            ApplyCollider();
        }
    }

    public void SetRotationIndex(int index) {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                rotationIndex = (index + data.Sprites.Count) % data.Sprites.Count;
            }
            else if (data.UseFlipX) {
                rotationIndex = (index + 2) % 2;
            }
            ApplyRotation();
        }
    }

    public void RotateNext() {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                rotationIndex = (rotationIndex + 1) % data.Sprites.Count;
            }
            else if (data.UseFlipX) {
                rotationIndex = (rotationIndex + 1) % 2;
            }
            ApplyRotation();
        }
    }

    public void RotatePrev() {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                rotationIndex = (rotationIndex - 1 + data.Sprites.Count) % data.Sprites.Count;
            }
            else if (data.UseFlipX) {
                rotationIndex = (rotationIndex - 1 + 2) % 2;
            }
            ApplyRotation();
        }
    }
}
