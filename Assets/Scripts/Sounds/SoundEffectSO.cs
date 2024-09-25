using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffect_", menuName = "Scriptable Objects/Sounds/Sound Effect")]
public class SoundEffectSO : ScriptableObject
{
    public string soundEffectName;
    public GameObject soundPrefab;
    public AudioClip soundEffectClip;
    public float soundEffectPitchRandomVariationMin = 0.8f;
    public float soundEffectPitchRandomVariationMax = 1.2f;

    [Range(0f, 1f)] public float soundEffectVolume = 1f;

    #region Validation
#if UNITY_EDITOR
    public void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(soundEffectName), soundEffectName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundPrefab), soundPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundEffectClip), soundEffectClip);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(soundEffectPitchRandomVariationMin),
            soundEffectPitchRandomVariationMin, nameof(soundEffectPitchRandomVariationMax),
            soundEffectPitchRandomVariationMax, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(soundEffectVolume), soundEffectVolume, true);
    }
#endif
    #endregion
}
