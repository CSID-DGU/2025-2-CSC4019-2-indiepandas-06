using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapObject : MapObjectBase, ILongPressable, IClickable {

    [Header("Animation")]
    [SerializeField] private AnimationCurve easeIn;
    [SerializeField] private AnimationCurve easeOut;
    [SerializeField] private AnimationCurve scaleCurve;

    private float duration = 0.3f;

    protected void OnEnable() {
        base.OnEnable();
    }

    // 테스트용
    protected void Update() {
        base.Update();

        if (InputManager.GetKeyDown(KeyAction.Jump)) {
            RotateNext();
            PlayAppear();
        }
    }

    public void Init(MapObjectData newData = null) {
        base.Init(newData);
        
        // 추가 컴포넌트 붙이기
        if (data.AdditionalComponents != null) {
            foreach (string compName in data.AdditionalComponents) {
                var type = System.Type.GetType(compName);
                if (type != null && GetComponent(type) == null) {
                    gameObject.AddComponent(type);
                }
            }
        }
    }

    // 애니메이션 및 이펙트

    public void PlayAppear() {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AppearRoutine());
    }

    public void PlayDisappear() {
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(Vector3.one, Vector3.zero, () => gameObject.SetActive(false)));
    }

    private IEnumerator AppearRoutine() {
        visual.localPosition = Vector3.zero + new Vector3(0, 0.5f, 0);

        // 일단 커지기
        yield return StartCoroutine(ScaleRoutine(Vector3.zero, Vector3.one));

        yield return new WaitForSecondsRealtime(0.1f);

        // 바운스
        yield return StartCoroutine(MoveRoutine(new Vector3(0, 0.5f, 0), Vector3.zero, 0.2f, easeIn));
        AddDustParticle(8);
        yield return StartCoroutine(MoveRoutine(Vector3.zero, new Vector3(0, 0.3f, 0), 0.15f, easeOut));
        yield return StartCoroutine(MoveRoutine(new Vector3(0, 0.3f, 0), Vector3.zero, 0.15f, easeIn));
        AddDustParticle(5);
        yield return StartCoroutine(MoveRoutine(Vector3.zero, new Vector3(0, 0.15f, 0), 0.1f, easeOut));
        yield return StartCoroutine(MoveRoutine(new Vector3(0, 0.15f, 0), Vector3.zero, 0.1f, easeIn));
        AddDustParticle(3);

        visual.localPosition = Vector3.zero;
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, System.Action onComplete = null) {
        float time = 0f;
        while (time < duration) {
            float t = time / duration;
            float curveT = scaleCurve.Evaluate(t);
            visual.localScale = Vector3.LerpUnclamped(from, to, curveT);
            time += Time.deltaTime;
            yield return null;
        }
        visual.localScale = to;
        onComplete?.Invoke();
    }

    private IEnumerator MoveRoutine(Vector3 from, Vector3 to, float duration, AnimationCurve ease) {
        float time = 0f;
        while (time < duration) {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float easedT = ease.Evaluate(t);
            visual.localPosition = Vector3.Lerp(from, to, easedT);
            yield return null;
        }
        visual.localPosition = to;
    }

    private void AddDustParticle(int num = 5) {
        for (int i = 0; i < num; i++) {
            var particle = (Particle)Random.Range(0, 2);
			var dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			var particleID = EnvironmentManager.AddParticle(particle, transform.position);
			EnvironmentManager.SetParticleVelocity(particleID, dir * 1.5f);
        }
    }

    public void OnLongPressed() {
        if (BuildingModeManager.Instance != null && BuildingModeManager.Instance.IsBuildingMode) {
            BuildingModeManager.Instance.ShowGhost(this);
            BuildingModeManager.Instance.SelectObject(null);
            gameObject.SetActive(false);
        }
    }

    public void OnClick(UnityEngine.Vector3 worldPos) {
        if (BuildingModeManager.Instance != null && BuildingModeManager.Instance.IsBuildingMode) {
            BuildingModeManager.Instance.SelectObject(this);
        }
    }

    public void Remove() {
        if (data != null) {
            InventoryManager.Instance.AddItem(new MapObjectItem(data.LinkedItem));
        }
        // TODO 테스트
        UIManager.ShowAlert("성공적으로 제거하였습니다.", "넹", "테스트", () => {
            Debug.Log("Alert closed!");
        });

        Destroy(gameObject);
    }
}
