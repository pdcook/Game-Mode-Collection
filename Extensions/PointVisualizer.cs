using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using UnboundLib;
using Sonigon;
using UnityEngine.UI;
using UnboundLib.GameModes;
using UnboundLib.Extensions;
using UnboundLib.Utils;
using System.Linq;

namespace RWF
{
	public static class PointVisualizerExtensions
	{
		private static Color AverageColor(this PointVisualizer instance, List<Color> colors)
        {
			float r = 0f;
			float g = 0f;
			float b = 0f;
			float a = 0f;
			foreach (Color color in colors)
            {
				r += color.r;
				g += color.g;
				b += color.b;
				a += color.a;
            }
			float num = (float)colors.Count();
			return new Color(r / num, g / num, b / num, a / num);
        }

		// Overload for the existing DoWinSequence method to support more than two winners, or no winners
		public static IEnumerator DoWinSequence(this PointVisualizer instance, Dictionary<int, int> teamPoints, Dictionary<int, int> teamRounds, int[] winnerTeamIDs)
		{
			yield return new WaitForSecondsRealtime(0.35f);
			SoundManager.Instance.Play(instance.soundWinRound, instance.transform.GetChild(1));

			int teamCount = teamPoints.Count;

			instance.ResetBalls(teamCount);
			instance.bg.SetActive(true);

			instance.transform.GetChild(1).Find("Orange").gameObject.SetActive(true);
			instance.transform.GetChild(1).Find("Blue").gameObject.SetActive(true);

			for (int i = 0; i < teamCount; i++)
			{
				instance.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
			}

			yield return new WaitForSecondsRealtime(0.2f);

			GamefeelManager.instance.AddUIGameFeelOverTime(10f, 0.1f);

			instance.DoShowPoints(teamPoints, winnerTeamIDs);

			yield return new WaitForSecondsRealtime(0.35f);

			SoundManager.Instance.Play(instance.sound_UI_Arms_Race_A_Ball_Shrink_Go_To_Left_Corner, instance.transform);

			float c = 0f;
			float ballSmallSize = (float)instance.GetFieldValue("ballSmallSize");
			float bigBallScale = (float)instance.GetFieldValue("bigBallScale");

			while (c < instance.timeToScale)
			{
				foreach (int winnerTeamID in winnerTeamIDs)
                {
                    var rt = instance.GetData().teamBall[winnerTeamID].GetComponent<RectTransform>();
                    rt.sizeDelta = Vector2.LerpUnclamped(rt.sizeDelta, Vector2.one * ballSmallSize, instance.scaleCurve.Evaluate(c / instance.timeToScale));
                }
				c += Time.unscaledDeltaTime;
				yield return null;
			}

			yield return new WaitForSecondsRealtime(instance.timeBetween);

			c = 0f;

			while (c < instance.timeToMove)
			{
				foreach (int winnerTeamID in winnerTeamIDs)
                {
                    var trans = instance.GetData().teamBall[winnerTeamID].transform;
                    trans.position = Vector3.LerpUnclamped(trans.position, (Vector3)UIHandler.instance.roundCounterSmall.InvokeMethod("GetPointPos", winnerTeamID), instance.scaleCurve.Evaluate(c / instance.timeToMove));
                }
				c += Time.unscaledDeltaTime;
				yield return null;
			}

			SoundManager.Instance.Play(instance.sound_UI_Arms_Race_B_Ball_Go_Down_Then_Expand, instance.transform);

			foreach (int winnerTeamID in winnerTeamIDs)
            {
				instance.GetData().teamBall[winnerTeamID].transform.position = (Vector3)UIHandler.instance.roundCounterSmall.InvokeMethod("GetPointPos", winnerTeamID);
            }

			yield return new WaitForSecondsRealtime(instance.timeBetween);
			c = 0f;

			while (c < instance.timeToMove)
			{
				for (int i = 0; i < teamCount; i++)
				{
					if (!winnerTeamIDs.Contains(i))
					{
						var trans = instance.GetData().teamBall[i].transform;
						trans.position = Vector3.LerpUnclamped(trans.position, CardChoiceVisuals.instance.transform.position, instance.scaleCurve.Evaluate(c / instance.timeToMove));
					}
				}

				c += Time.unscaledDeltaTime;
				yield return null;
			}

			for (int i = 0; i < teamCount; i++)
			{
				if (!winnerTeamIDs.Contains(i))
				{
					instance.GetData().teamBall[i].transform.position = CardChoiceVisuals.instance.transform.position;
				}
			}

			yield return new WaitForSecondsRealtime(instance.timeBetween);
			c = 0f;

			while (c < instance.timeToScale)
			{
				for (int i = 0; i < teamCount; i++)
				{
					if (!winnerTeamIDs.Contains(i))
					{
						var rt = instance.GetData().teamBall[i].GetComponent<RectTransform>();
						rt.sizeDelta = Vector2.LerpUnclamped(rt.sizeDelta, Vector2.one * bigBallScale, instance.scaleCurve.Evaluate(c / instance.timeToScale));
					}
				}

				c += Time.unscaledDeltaTime;
				yield return null;
			}

			SoundManager.Instance.Play(instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, instance.transform);
			GamefeelManager.instance.AddUIGameFeelOverTime(10f, 0.2f);

			// removing this fixes player skin + face desync issues
			/*
			for (int i = 0; i < teamCount; i++) {
				if (i != winnerTeamID) {
					CardChoiceVisuals.instance.Show(i, false);
					break;
				}
			}*/

			UIHandler.instance.roundCounterSmall.UpdateRounds(teamRounds);
			UIHandler.instance.roundCounterSmall.UpdatePoints(teamPoints);

			// Reset fill amounts to prevent visual artifacts when the point visualizer is shown again
			for (int i = 0; i < teamPoints.Count; i++)
			{
				var ball = instance.GetData().teamBall[i];
				var fill = ball.transform.Find("Fill").GetComponent<ProceduralImage>();
				fill.fillAmount = 0f;
			}

			instance.InvokeMethod("Close");
		}

		// Overload for the existing DoSequence method to support more than two teams
		public static IEnumerator DoSequence(this PointVisualizer instance, Dictionary<int, int> teamPoints, Dictionary<int, int> teamRounds, int[] winnerTeamIDs)
		{
			yield return new WaitForSecondsRealtime(0.45f);

			SoundManager.Instance.Play(instance.soundWinRound, instance.transform);
			instance.ResetBalls(teamPoints.Count);
			instance.bg.SetActive(true);

			instance.transform.GetChild(1).Find("Orange").gameObject.SetActive(true);
			instance.transform.GetChild(1).Find("Blue").gameObject.SetActive(true);

			for (int i = 0; i < teamPoints.Count; i++)
			{
				instance.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
			}

			yield return new WaitForSecondsRealtime(0.2f);

			GamefeelManager.instance.AddUIGameFeelOverTime(10f, 0.1f);
			instance.DoShowPoints(teamPoints, winnerTeamIDs);

			yield return new WaitForSecondsRealtime(1.8f);

			for (int i = 0; i < teamPoints.Count; i++)
			{
				instance.GetData().teamBall[i].GetComponent<CurveAnimation>().PlayOut();
			}

			yield return new WaitForSecondsRealtime(0.25f);

			instance.InvokeMethod("Close");
		}

		// Overload for the existing DoShowPoints method to support more than two teams
		public static void DoShowPoints(this PointVisualizer instance, Dictionary<int, int> teamPoints, int[] winnerTeamIDs)
		{
			for (int i = 0; i < teamPoints.Count; i++)
			{
				var ball = instance.GetData().teamBall[i];
				var fill = ball.transform.Find("Fill").GetComponent<ProceduralImage>();
				fill.fillMethod = Image.FillMethod.Radial360;

				if (winnerTeamIDs.Contains(i))
				{
					fill.fillAmount = teamPoints[i] == 0 ? 1f : (float)teamPoints[i] / (int)GameModeManager.CurrentHandler.Settings["pointsToWinRound"];
				}
				else
				{
					fill.fillAmount = (float)teamPoints[i] / (int)GameModeManager.CurrentHandler.Settings["pointsToWinRound"];
				}
			}

			Color color = Color.white;
			string text = "TIE\nNO POINTS";
			if (winnerTeamIDs.Count() > 0)
			{
				List<Color> colors = winnerTeamIDs.Select(tID => PlayerManager.instance.GetPlayersInTeam(tID).First().GetTeamColors().color).ToList();
				color = instance.AverageColor(colors);
				text = $"POINT TO {((GameModeManager.CurrentHandler.Settings.TryGetValue("allowTeams", out object allowTeamsObj) && !(bool)allowTeamsObj) ? "" : $"TEAM{(winnerTeamIDs.Count() > 1 ? "S\n" : "")}")}";
				foreach (int winnerTeamID in winnerTeamIDs)
                {
					text += $" {ExtraPlayerSkins.GetTeamColorName(PlayerManager.instance.GetPlayersInTeam(winnerTeamID).First().colorID()).ToUpper()}";

				}
            }

			instance.text.color = color;
			instance.text.text = text;
		}
	}
}
