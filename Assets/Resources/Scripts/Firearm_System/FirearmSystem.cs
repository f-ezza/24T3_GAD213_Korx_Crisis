using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Korx.Firearms
{
    public class FirearmSystem : MonoBehaviour
    {
        #region Fields
        [Header("Firearm Definition")]
        [SerializeField] private FirearmDefinition[] firearms;
        [SerializeField] private FirearmDefinition currentFirearm;

        [Header("Runtime Information")]
        [SerializeField] private int currentAmmo;
        [SerializeField] private bool canFire = true;
        [SerializeField] private FirearmDefinition.FireModes curFireMode;
        [SerializeField] private int currentFireModeIndex;
        private int burstCount;
        private bool isAR;

        [Header("Timers")]
        [SerializeField] private float timeSinceLastShot;
        private float timeBetweenShots;
        private float aimLerpTime = 0f;
        private float currentAimValue = 0f;

        [Header("Keybinds")]
        [SerializeField] private KeyCode primaryWeaponKey;
        [SerializeField] private KeyCode secondaryWeaponKey;
        [SerializeField] private KeyCode reloadKey;
        [SerializeField] private KeyCode changeFireMode;

        [Header("Referenced GameObjects")]
        [SerializeField] private GameObject weaponSocketContainer;
        [SerializeField] private GameObject currentFirearmObject;
        [SerializeField] private Animator currentFirearmAnimator;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioSource audioSource;

        [SerializeField]private Transform casingEjectionPoint;
        #endregion

        #region Initialization & Update
        private void Awake()
        {
            firearms = Resources.LoadAll<FirearmDefinition>("Scripts/Firearm_System/Firearms SO");
            playerAnimator = GameObject.Find("SK_FP_CH_Default_Root").GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            SetWeapon(firearms[1]);
        }

        private void Update()
        {
            HandleWeaponSwitch();
            HandleReload();
            HandleFireModeSwitch();
            HandleWeaponAiming();

            if (canFire)
            {
                HandleShooting();
            }

            timeSinceLastShot += Time.deltaTime;
        }
        #endregion

        #region Weapon Handling
        private bool isAiming = false;
        private bool hasPlayedAimSound = false;
        private void HandleWeaponAiming()
        {
            if (Input.GetMouseButton(1))
            {
                isAiming = true;
                playerAnimator.SetBool("Aim", isAiming);

                if (!hasPlayedAimSound)
                {
                    audioSource.PlayOneShot(currentFirearm.firearmAimSound);
                    hasPlayedAimSound = true;
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isAiming = false;
                hasPlayedAimSound = false; // Reset to allow sound on next aim start
                playerAnimator.SetBool("Aim", isAiming);
            }

            float targetAimValue = isAiming ? 1f : 0f;
            currentAimValue = Mathf.Lerp(currentAimValue, targetAimValue, Time.deltaTime / currentFirearm.firearmAimSpeed);

            playerAnimator.SetFloat("Aiming", currentAimValue);
            currentFirearmAnimator.SetFloat("Aiming", currentAimValue);
        }

        private void HandleWeaponSwitch()
        {
            if (Input.GetKeyDown(primaryWeaponKey) && currentFirearm != firearms[1])
            {
                SetWeapon(firearms[1]);
            }

            if (Input.GetKeyDown(secondaryWeaponKey) && currentFirearm != firearms[0])
            {
                SetWeapon(firearms[0]);
            }
        }

        private void SetWeapon(FirearmDefinition newFirearm)
        {
            Transform childTransform = weaponSocketContainer.transform.GetChild(0);
            if (childTransform != null)
            {
                Destroy(childTransform.gameObject);
            }

            currentFirearm = newFirearm;
            currentAmmo = currentFirearm.firearmFullMag;
            timeBetweenShots = 60f / currentFirearm.firearmfirerate;
            burstCount = currentFirearm.firearmBurstCount > 0 ? currentFirearm.firearmBurstCount : 3;
            curFireMode = currentFirearm.firearmFireModes[currentFireModeIndex = 0];
            canFire = true;

            currentFirearmObject = Instantiate(currentFirearm.firearmPrefab, Vector3.zero, Quaternion.identity);
            currentFirearmObject.transform.SetParent(weaponSocketContainer.transform);
            currentFirearmObject.transform.localPosition = Vector3.zero;
            currentFirearmObject.transform.localRotation = Quaternion.identity;

            currentFirearmAnimator = currentFirearmObject.GetComponent<Animator>();

            casingEjectionPoint = FindDeepChild(currentFirearmObject.transform, "SOCKET_Eject_C");

            if (casingEjectionPoint == null)
            {
                Debug.LogError("Casing Ejection Point not found!");
            }
            else
            {
                Debug.Log("Casing Ejection Point found: " + casingEjectionPoint.name);
            }
        }
        #endregion

        #region Reloading
        private void HandleReload()
        {
            if (Input.GetKeyDown(reloadKey) && currentAmmo < currentFirearm.firearmFullMag && canFire)
            {
                if(currentAmmo > 0)
                {
                    currentFirearmAnimator.Play("Reload");
                    playerAnimator.Play("Reload");
                    audioSource.PlayOneShot(currentFirearm.firearmReloadSound);
                }
                else
                {
                    currentFirearmAnimator.Play("Reload Empty");
                    playerAnimator.Play("Reload Empty");
                    audioSource.PlayOneShot(currentFirearm.firearmReloadEmptySound);
                }
                StartCoroutine(ReloadWeapon());
            }
        }

        private IEnumerator ReloadWeapon()
        {
            currentFirearmAnimator.SetBool("Reloading", true);
            canFire = false;
            yield return new WaitForSeconds(currentFirearm.firearmReloadTime);
            currentAmmo = currentFirearm.firearmFullMag;
            canFire = true;
            currentFirearmAnimator.SetBool("Reloading", false);
        }
        #endregion

        #region Fire Mode Handling
        private void HandleFireModeSwitch()
        {
            if (Input.GetKeyDown(changeFireMode) && currentFirearm.firearmFireModes.Length > 1)
            {
                currentFireModeIndex = (currentFireModeIndex + 1) % currentFirearm.firearmFireModes.Length;
                curFireMode = currentFirearm.firearmFireModes[currentFireModeIndex];

                Debug.Log("Fire mode changed to: " + curFireMode);
                audioSource.PlayOneShot(currentFirearm.firearmFireSelectSounds[Random.Range(0, currentFirearm.firearmFireSelectSounds.Length - 1)]);
            }
        }
        #endregion

        #region Shooting
        private void HandleShooting()
        {
            switch (curFireMode)
            {
                case FirearmDefinition.FireModes.Semi:
                    if (Input.GetMouseButtonDown(0) && currentAmmo > 0 && timeSinceLastShot >= timeBetweenShots)
                    {
                        timeSinceLastShot = 0f;
                        Shoot();
                    }
                    break;

                case FirearmDefinition.FireModes.Auto:
                    if (Input.GetMouseButton(0) && currentAmmo > 0 && timeSinceLastShot >= timeBetweenShots)
                    {
                        Shoot();
                        timeSinceLastShot = 0f;
                    }
                    break;

                case FirearmDefinition.FireModes.Burst:
                    if (Input.GetMouseButtonDown(0) && currentAmmo > 0 && timeSinceLastShot >= timeBetweenShots)
                    {
                        StartCoroutine(BurstFire());
                        timeSinceLastShot = 0f;
                    }
                    break;

                case FirearmDefinition.FireModes.Bolt:
                    break;
            }

            if (Input.GetMouseButtonDown(0) && currentAmmo <= 0)
            {
                audioSource.PlayOneShot(currentFirearm.firearmShootEmptySound);
            }
        }

        private void Shoot()
        {
            EjectCasing();
            if (currentFirearmAnimator != null)
            {
                currentFirearmAnimator.Play("Fire");
                playerAnimator.Play("Fire");
                audioSource.PlayOneShot(currentFirearm.firearmShootSound[Random.Range(0, currentFirearm.firearmShootSound.Length - 1)]);
            }

            RaycastHit raycastHit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, currentFirearm.firearmRange))
            {
                IDamagableObject damagableObject = raycastHit.collider.GetComponent<IDamagableObject>();

                if (damagableObject != null)
                {
                    damagableObject.OnReceivedDamage(currentFirearm.firearmDamage);

                    Debug.Log("Hit a damagable object: " + raycastHit.collider.name);
                }
                else
                {
                    Debug.Log("Hit a non-damagable object: " + raycastHit.collider.name);
                }
            }

            currentAmmo--;

            Debug.Log("Bang!");
        }

        private IEnumerator BurstFire()
        {
            for (int i = 0; i < burstCount; i++)
            {
                if (currentAmmo > 0)
                {
                    Shoot();
                    yield return new WaitForSeconds(timeBetweenShots);
                }
                else
                {
                    break;
                }
            }
        }

        private void EjectCasing()
        {
            if (currentFirearm.firearmEjectSounds != null)
            {
                int randomSoundIndex = Random.Range(0, currentFirearm.firearmEjectSounds.Length);
                Debug.Log("Playing eject sound: " + currentFirearm.firearmEjectSounds[randomSoundIndex].name);
                audioSource.PlayOneShot(currentFirearm.firearmEjectSounds[randomSoundIndex]);
            }

            if (currentFirearm.firearmCasingPrefab != null && casingEjectionPoint != null)
            {
                Debug.Log("Ejecting casing at: " + casingEjectionPoint.position);
                GameObject casing = Instantiate(currentFirearm.firearmCasingPrefab, casingEjectionPoint.position, casingEjectionPoint.rotation);

                Rigidbody casingRb = casing.GetComponent<Rigidbody>();
                if (casingRb != null)
                {
                    Vector3 ejectForce = casingEjectionPoint.right * 2.0f + casingEjectionPoint.up * 1.0f;
                    Debug.Log("Ejecting with force: " + ejectForce);
                    casingRb.AddForce(ejectForce, ForceMode.Impulse);
                }
                else
                {
                    Debug.LogWarning("Casing does not have a Rigidbody.");
                }

                Destroy(casing, 5f);
            }
            else
            {
                Debug.LogWarning("Casing prefab or ejection point is missing.");
            }

            Debug.Log("Casing Ejected");
        }
        #endregion

        #region Helpers
        private Transform FindDeepChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform result = FindDeepChild(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion
    }
}
