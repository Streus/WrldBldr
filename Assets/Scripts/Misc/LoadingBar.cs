using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadingBar : MonoBehaviour
{
	[SerializeField]
	private Image bar;

	[SerializeField]
	private Text statusText;

	[SerializeField]
	private WrldBldr.Generator gen;

	public void Update()
	{
		if (gen == null)
			return;

		if (bar != null)
			bar.fillAmount = gen.getGenerationProgress ();

		if (statusText != null)
			statusText.text = gen.getCurrentStageText ();
	}
}
