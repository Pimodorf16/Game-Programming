using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade;

    public override IEnumerator AnimateTransitionIn()
    {
        //var tweener = crossFade.DOFade(1f, 1f);
        yield return null;
    }

    public override IEnumerator AnimateTransitionOut()
    {
        //var tweener = crossFade.DOFade(0f, 1f);
        yield return null;
    }
}
