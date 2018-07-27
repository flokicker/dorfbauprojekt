using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class OnlineDatabaseManager : Singleton<OnlineDatabaseManager> {

	private int categoryCount = 3;
	private List<List<Feedback>> feedbackCat;
	private string feedbackFetchURL = "http://dorfbauprojekt.dx.am/fetchFeedback.php";
	private string feedbackAddURL = "http://dorfbauprojekt.dx.am/newFeedback.php";
	private bool tested = false;

	// Use this for initialization
	void Start () {
		feedbackCat = new List<List<Feedback>>();
		StartCoroutine(LoadFedback());

	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator LoadFedback()
	{
		for(int i = 0; i < categoryCount; i++)
		{
			List<Feedback> fbList = new List<Feedback>();
			using (WWW www = new WWW(feedbackFetchURL +"?category="+i))
			{
				yield return www;
				string[] lines = www.text.Split('$');
				foreach(string l in lines)
				{
					string[] args = l.Split(';');
					if(args.Length < 4) continue;
					Feedback fb = new Feedback();
					fb.category = i;
					fb.title = args[0];
					fb.text = args[1];
					fb.creator = args[2];
					fb.date = args[3];
					fbList.Add(fb);
				}
			}
			feedbackCat.Add(fbList);
		}
	}

	public static IEnumerator AddNewFeedback(Feedback fb)
	{
		Instance.feedbackCat[fb.category].Add(fb);
		using (WWW www = new WWW(Instance.feedbackAddURL +"?category="+fb.category+"&title="+fb.title+"&text="+fb.text+"&creator="+fb.creator+"&date="+fb.date))
		{
			yield return www;
		}
	}

	public static List<Feedback> GetFeedbackList(int cat)
	{
		if(cat >= Instance.feedbackCat.Count) return new List<Feedback>();
		return Instance.feedbackCat[cat];
	}
}
public class Feedback
{
	public string title, text, creator, date;
	public int category;
}
