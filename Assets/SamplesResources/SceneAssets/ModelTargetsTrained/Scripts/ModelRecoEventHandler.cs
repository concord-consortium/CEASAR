/*============================================================================== 
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/
using UnityEngine;
using Vuforia;

public class ModelRecoEventHandler : BaseModelRecoEventHandler
{
    #region PUBLIC_METHODS

    public override void OnNewSearchResult(TargetFinder.TargetSearchResult searchResult)
    {
        base.OnNewSearchResult(searchResult);

        FindObjectOfType<TrackedStateManager>().TargetFinderFound(searchResult.TargetName);
    }

    #endregion // PUBLIC_METHODS
}
