using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarBase : MonoBehaviour
{
    [SerializeField] Image fillImageBack;
    [SerializeField] Image fillImageFront;

    [SerializeField] float fillSpeed = 0.1f;

    [SerializeField] bool delayFill = true;
    [SerializeField] float fillDelay = 0.5f;
    float currentFillAmount;
    protected float targetFillAmount;

    float priviousFillAmount;

    float t;

    WaitForSeconds waitForDelayFill;

    Coroutine bufferedFillingCoroutine;

    public virtual void Awake()
    {
        // canvas = GetComponent<Canvas>();
        // canvas.worldCamera = Camera.main;
        // if (TryGetComponent<Canvas>(out Canvas canvas))
        // {
        //     canvas.worldCamera = Camera.main;
        // }
        waitForDelayFill = new WaitForSeconds(fillDelay);
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public virtual void Initialize(float currentValue, float maxValue)
    {
        currentFillAmount = currentValue / maxValue;
        targetFillAmount = currentFillAmount;
        fillImageBack.fillAmount = currentFillAmount;
        fillImageFront.fillAmount = currentFillAmount;
    }

    public void UpdateStats(float currentValue, float maxValue)
    {
        targetFillAmount = currentValue / maxValue;
        // if (bufferedFillingCoroutine != null)
        // {
        //     StopCoroutine(bufferedFillingCoroutine);
        // }
        //当状态值变化时，根据当前值和目标值的大小，选择一个填充图像进行填充
        //如果当前值大于目标值，则选择后填充图像进行填充
        if (currentFillAmount > targetFillAmount)
        {
            Debug.Log("currentFillAmount > targetFillAmount:" + targetFillAmount);
            fillImageFront.fillAmount = targetFillAmount;
            bufferedFillingCoroutine = StartCoroutine(BufferedFillingCoroutine(fillImageBack));
            return;
        }
        //如果当前值小于目标值，则选择前填充图像进行填充
        if (currentFillAmount < targetFillAmount)
        {
            Debug.Log("currentFillAmount < targetFillAmount: " + targetFillAmount);
            fillImageBack.fillAmount = targetFillAmount;
            bufferedFillingCoroutine = StartCoroutine(BufferedFillingCoroutine(fillImageFront));
        }
    }
    protected virtual IEnumerator BufferedFillingCoroutine(Image image)
    {
        if (delayFill)
        {
            yield return waitForDelayFill;
        }
        t = 0f;
        priviousFillAmount = currentFillAmount;
        while (t < 1f)
        {
            t += Time.deltaTime * fillSpeed;
            currentFillAmount = Mathf.Lerp(priviousFillAmount, targetFillAmount, t);
            image.fillAmount = currentFillAmount;
            yield return null;
        }
    }
}
