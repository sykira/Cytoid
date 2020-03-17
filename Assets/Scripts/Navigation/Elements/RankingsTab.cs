using System;
using System.Collections.Generic;
using Proyecto26;
using RSG;
using UnityEngine;
using UnityEngine.UI;

public class RankingsTab : MonoBehaviour, ScreenInitializedListener, ScreenBecameActiveListener
{
    public GameObject icon;
    public SpinnerElement spinner;
    
    public Text rankingText;
    public RankingContainer rankingContainer;
    public TierRankingContainer tierRankingContainer;
    
    public Text rankingContainerStatusText;
    public InteractableMonoBehavior viewMoreButton;

    public void OnScreenInitialized()
    {
        rankingText.text = "";
        rankingContainerStatusText.text = "";
        viewMoreButton.gameObject.SetActive(false);
    }

    public void OnScreenBecameActive()
    {
        if (!Context.LocalPlayer.PlayRanked && this.GetScreenParent() is ResultScreen)
        {
            icon.SetActive(false);
        }
    }
    
    private DateTime updateRankingToken;
    
    public IPromise<(int, List<RankingEntry>)> UpdateRankings(string levelId, string chartType)
    {
        viewMoreButton.gameObject.SetActive(false);
        rankingText.text = "";
        spinner.IsSpinning = true;
        
        rankingContainer.Clear();
        rankingContainerStatusText.text = "GAME_PREP_RANKINGS_DOWNLOADING".Get();
        var token = updateRankingToken = DateTime.Now;
        return Context.OnlinePlayer.GetLevelRankings(levelId, chartType)
            .Then(ret =>
            {
                if (token != updateRankingToken) return (-1, null);
                var (rank, entries) = ret;
                rankingContainer.SetData(entries);
                if (rank > 0)
                {
                    if (rank > 99) rankingText.text = "#99+";
                    else rankingText.text = "#" + rank;
                }
                else rankingText.text = "N/A";

                rankingContainerStatusText.text = "";
                if (entries.Count == 0)
                {
                    rankingContainerStatusText.text = "GAME_PREP_RANKINGS_BE_THE_FIRST".Get();
                }
                
                if (entries.Count > 0)
                {
                    viewMoreButton.gameObject.SetActive(true);
                    viewMoreButton.onPointerClick.RemoveAllListeners();
                    viewMoreButton.onPointerClick.AddListener(_ =>
                    {
                        Application.OpenURL(
                            $"https://cytoid.io/levels/{levelId}"); // TODO: Jump to selected difficulty?
                    });
                }

                return ret;
            })
            .Catch(error =>
            {
                if (token != updateRankingToken) return (-1, null);
                if (error is RequestException reqError)
                {
                    if (reqError.StatusCode != 404)
                    {
                        Debug.LogError(error);
                    }
                }
                else
                {
                    Debug.LogError(error);
                }
                rankingText.text = "N/A";
                rankingContainerStatusText.text = "GAME_PREP_RANKINGS_COULD_NOT_DOWNLOAD".Get();
                throw error;
            })
            .Finally(() =>
            {
                if (token != updateRankingToken) return;
                spinner.IsSpinning = false;
            });
    }
    
    private DateTime updateTierRankingToken;
    
    public IPromise<(int, List<TierRankingEntry>)> UpdateTierRankings(string tierId)
    {
        viewMoreButton.gameObject.SetActive(false);
        rankingText.text = "";
        spinner.IsSpinning = true;
        
        tierRankingContainer.Clear();
        rankingContainerStatusText.text = "TIER_RANKINGS_DOWNLOADING".Get();
        var token = updateTierRankingToken = DateTime.Now;
        return Context.OnlinePlayer.GetTierRankings(tierId)
            .Then(ret =>
            {
                if (token != updateTierRankingToken) return (-1, null);
                var (rank, entries) = ret;
                tierRankingContainer.SetData(entries);
                if (rank > 0)
                {
                    if (rank > 99) rankingText.text = "#99+";
                    else rankingText.text = "#" + rank;
                }
                else rankingText.text = "N/A";

                rankingContainerStatusText.text = "";
                if (entries.Count == 0)
                {
                    rankingContainerStatusText.text = "TIER_RANKINGS_BE_THE_FIRST".Get();
                }
                
                // TODO: Show view more button
                /*if (entries.Count > 0)
                {
                    viewMoreButton.gameObject.SetActive(true);
                    viewMoreButton.onPointerClick.RemoveAllListeners();
                    viewMoreButton.onPointerClick.AddListener(_ =>
                    {
                        Application.OpenURL(
                            $"https://cytoid.io/levels/{levelId}"); // TODO: Jump to selected difficulty?
                    });
                }*/

                return ret;
            })
            .Catch(error =>
            {
                if (token != updateTierRankingToken) return (-1, null);
                if (error is RequestException reqError)
                {
                    if (reqError.StatusCode != 404)
                    {
                        Debug.LogError(error);
                    }
                }
                else
                {
                    Debug.LogError(error);
                }
                rankingText.text = "N/A";
                rankingContainerStatusText.text = "TIER_RANKINGS_COULD_NOT_DOWNLOAD".Get();
                throw error;
            })
            .Finally(() =>
            {
                if (token != updateTierRankingToken) return;
                spinner.IsSpinning = false;
            });
    }
    
}