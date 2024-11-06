using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BTSPulseSelected : MonoBehaviour
{
	static List<Selectable> s_selectThis = new List<Selectable>();

	[SerializeField] Selectable m_selected;
	[SerializeField] float m_strength = 0.025f;
	[SerializeField] float m_speed = 1;

	EventSystem m_cachedEventSystem;
	Vector3 m_savedLocalScale;

	public static void AddSelectThis(Selectable a_selectable)
	{
		s_selectThis.Add(a_selectable);

		a_selectable.Select();

		Debug.Log($"Added and selected: {a_selectable.name}", a_selectable.gameObject);
	}

	public static void RemoveSelectThis(Selectable a_selectable)
	{
		s_selectThis.Remove(a_selectable);

		SelectFirstOrNothing();
	}

	private static void SelectFirstOrNothing()
	{
		if (null != EventSystem.current)
		{
			if (null != GetFirst())
			{
				EventSystem.current.SetSelectedGameObject(GetFirst().gameObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}

	public static Selectable GetFirst()
	{
		if (s_selectThis.Count > 0)
		{
			return s_selectThis[0];
		}

		return null;
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (null == m_cachedEventSystem)
		{
			m_cachedEventSystem = EventSystem.current;
			return;
		}

		GameObject l_current = m_cachedEventSystem.currentSelectedGameObject;

		if (null == l_current)
		{
			if (null != GetFirst())
			{
				GetFirst().Select();
			}
			return;
		}

		Transform l_transform = l_current.transform;
		Selectable l_selectable = l_current.GetComponent<Selectable>();

		if (null == l_selectable)
		{
			return;
		}

		if (l_selectable != m_selected)
		{
			if (null != m_selected)
			{
				m_selected.transform.localScale = m_savedLocalScale;
			}

			m_selected = l_selectable;
			m_savedLocalScale = l_transform.localScale;

			StopAllCoroutines();
			StartCoroutine(PulseTransform(l_transform, m_savedLocalScale));
		}

	}

	IEnumerator PulseTransform(Transform a_transform, Vector3 a_originalSize)
	{
		float l_secondsPerFrame = 1.0f / 60.0f;

		yield return new WaitForSecondsRealtime(l_secondsPerFrame);

		while (a_transform.gameObject.activeInHierarchy)
		{
//			Debug.Log($"{a_transform.name} {a_transform.gameObject.activeInHierarchy}", a_transform.gameObject);

			float timer = 0f;

			// Heart beat twice
			for (int i = 0; i < 2; i++)
			{
				// Zoom in
				while (timer < 0.1f)
				{
					yield return new WaitForSecondsRealtime(l_secondsPerFrame);
					timer += l_secondsPerFrame;

					if (null != a_transform)
					{
						a_transform.localScale = new Vector3
						(
							a_transform.localScale.x + (l_secondsPerFrame * m_strength * 2),
							a_transform.localScale.y + (l_secondsPerFrame * m_strength * 2)
						);
					}
				}
			}

			// Return to normal
			while (a_transform.localScale.x > a_originalSize.x)
			{
				yield return new WaitForSecondsRealtime(l_secondsPerFrame);

				if (null != a_transform)
				{
					a_transform.localScale = new Vector3
					(
						a_transform.localScale.x - l_secondsPerFrame * m_strength,
						a_transform.localScale.y - l_secondsPerFrame * m_strength
					);
				}
			}

			if (null != a_transform)
			{
				a_transform.localScale = a_originalSize;
			}

			yield return new WaitForSecondsRealtime(m_speed);
		}

		// The selected item went AWOL
		if (!a_transform.gameObject.activeInHierarchy)
		{
			SelectFirstOrNothing();
		}
	}
}
