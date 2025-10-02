using UnityEngine;
using UnityEngine.UI;

public class ShaderPrewarmerFillImage : MonoBehaviour
{
    [SerializeField] Image m_image;

   public void OnEnable()
    {
        if (null == m_image)
		{
            m_image = GetComponent<Image>();
		}

        ShaderPrewarmer.SubscribeProgress(OnProgressUpdate);
        ShaderPrewarmer.SubscribeCompleted(OnWarmupDone);
    }

    public void OnDisable()
    {
        ShaderPrewarmer.UnsubscribeProgress(OnProgressUpdate);
        ShaderPrewarmer.UnsubscribeCompleted(OnWarmupDone);
    }

    private void OnProgressUpdate(float progress)
    {
        if (null != m_image)
		{
            m_image.fillAmount = progress;
		}
    }

    private void OnWarmupDone()
    {
        if (null != m_image)
		{
            m_image.fillAmount = 1.0f;
		}
    }
}
