using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickEffect : MonoBehaviour
{
    [Header("Gun Bounce")]
    [SerializeField] private Transform gunTransform;
    [SerializeField] private float bounceScale = 1.1f;
    [SerializeField] private float bounceDuration = 0.1f;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float recoilDistance = 0.08f;
    [SerializeField] private float recoilDuration = 0.06f;
    [SerializeField] private float recoilRotation = 6f;

    [Header("Particle")]
    [SerializeField] private ParticleSystem clickParticle;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Camera mainCamera;
    private float recoilOffsetX;
    private float recoilAngleZ;

    private void Start()
    {
        if (gunTransform != null)
        {
            originalScale = gunTransform.localScale;
            originalPosition = gunTransform.position;
        }

        mainCamera = Camera.main;
        EventBus<ClickEvent>.Subscribe(OnClick);
    }

    private void OnDestroy()
    {
        EventBus<ClickEvent>.Unsubscribe(OnClick);
    }

    private void OnClick(ClickEvent e)
    {
        if (gunTransform != null)
        {
            StartCoroutine(BounceAnimation());
            StartCoroutine(RecoilAnimation());
        }

        if (clickParticle != null)
        {
            clickParticle.Play();
        }
    }

    private void Update()
    {
        if (gunTransform == null || Mouse.current == null || mainCamera == null)
            return;

        var mouseScreen = Mouse.current.position.ReadValue();
        var mouseWorld = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(mainCamera.transform.position.z - gunTransform.position.z)));

        var targetPosition = gunTransform.position;
        targetPosition.y = mouseWorld.y + yOffset;
        targetPosition.x = originalPosition.x + recoilOffsetX;
        targetPosition.z = originalPosition.z;

        gunTransform.position = Vector3.Lerp(gunTransform.position, targetPosition, followSpeed * Time.deltaTime);
        gunTransform.rotation = Quaternion.Euler(0f, 0f, recoilAngleZ);
    }

    private IEnumerator BounceAnimation()
    {
        gunTransform.localScale = originalScale * bounceScale;
        yield return new WaitForSeconds(bounceDuration);
        gunTransform.localScale = originalScale;
    }

    private IEnumerator RecoilAnimation()
    {
        recoilOffsetX = -recoilDistance;
        recoilAngleZ = recoilRotation;

        yield return new WaitForSeconds(recoilDuration);

        recoilOffsetX = 0f;
        recoilAngleZ = 0f;
    }
}
