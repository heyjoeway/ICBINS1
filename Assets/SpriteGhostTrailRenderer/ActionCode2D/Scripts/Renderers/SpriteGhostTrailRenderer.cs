using UnityEngine;

namespace ActionCode2D.Renderers
{
    [RequireComponent(typeof(SpriteRenderer))]
	public sealed class SpriteGhostTrailRenderer : MonoBehaviour 
	{
        public Color color = Color.white * 0.5f;
        public bool enableOnAwake = true;
        public bool singleColorShader = true;
        [Range(0.016f, 1f)] public float updateInterval = 0.1f;
        [Range(1, 10)] public int ghosts = 4;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        private SpriteRenderer[] _ghostRenderers;
        private Transform _ghostContainer;
        private float _currentTime = 0f;
        private int _ghostIndex = 0;

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake () 
		{
            InitializeGhosts();
            enabled = enableOnAwake;
        }
        private void Update()
        {
            _currentTime += Utils.cappedDeltaTime;
            if (_currentTime > updateInterval)
            {
                _currentTime = 0f;
                _ghostIndex = (_ghostIndex + 1) % _ghostRenderers.Length;
                _ghostRenderers[_ghostIndex].gameObject.SetActive(true);
                PlaceGhost(_ghostRenderers[_ghostIndex]);
            }
        }

        private void OnEnable()
        {
            _ghostContainer.parent = null;
            foreach (SpriteRenderer ghost in _ghostRenderers)
            {
                ghost.gameObject.SetActive(true);
            }
        }
        private void OnDisable()
        {
            if (!gameObject.activeInHierarchy) return;

            _currentTime = 0f;
            _ghostIndex = 0;

            _ghostContainer.parent = transform;
            foreach (SpriteRenderer ghost in _ghostRenderers)
            {
                ghost.gameObject.SetActive(false);
                PlaceGhost(ghost);
            }
        }

        private void InitializeGhosts()
        {
            _ghostRenderers = new SpriteRenderer[ghosts];
            if (_ghostRenderers.Length == 0) return;

            Material material = null;
            if (singleColorShader)
            {
                Shader textShader = Shader.Find("GUI/Text Shader");
                if (textShader == null) Debug.LogError("GUI/Text Shader not found.");
                else material = new Material(textShader);
            }
            else material = _spriteRenderer.material;

            _ghostContainer = new GameObject(gameObject.name + "-Ghosts").transform;
            _ghostContainer.parent = transform;
            _ghostContainer.localPosition = Vector3.zero;
            _ghostContainer.localRotation = Quaternion.identity;

            GameObject baseGhost = new GameObject("Ghost-0");
            baseGhost.transform.parent = _ghostContainer;
            baseGhost.transform.localPosition = Vector3.zero;
            baseGhost.transform.localRotation = Quaternion.identity;
            baseGhost.SetActive(false);

            _ghostRenderers[0] = baseGhost.AddComponent<SpriteRenderer>();
            _ghostRenderers[0].material = material;
            _ghostRenderers[0].sprite = _spriteRenderer.sprite;
            _ghostRenderers[0].color = color;
            _ghostRenderers[0].sortingLayerID = _spriteRenderer.sortingLayerID;
            // _ghostRenderers[0].sortingOrder = _spriteRenderer.sortingOrder - 1;

            for (int i = 1; i < _ghostRenderers.Length; i++)
            {
                GameObject ghost = Instantiate<GameObject>(baseGhost, _ghostContainer);
                ghost.name = "Ghost-" + i;
                _ghostRenderers[i] = ghost.GetComponent<SpriteRenderer>();
            }
        }
        private void PlaceGhost(SpriteRenderer ghost)
        {
            Transform parentTemp = ghost.transform.parent;
            ghost.transform.parent = null;
            ghost.transform.localScale = transform.lossyScale;
            ghost.transform.parent = parentTemp;

            ghost.transform.position = transform.position + (Vector3.forward * 0.01F);
            ghost.transform.rotation = transform.rotation;
            ghost.flipX = _spriteRenderer.flipX;
            ghost.flipY = _spriteRenderer.flipY;
            ghost.sprite = _spriteRenderer.sprite;
        }
	}
}