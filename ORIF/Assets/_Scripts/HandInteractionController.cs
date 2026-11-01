using UnityEngine;

/// <summary>Coordinates configured hand interactors and visuals without replacing XR tracking.</summary>
public sealed class HandInteractionController
{
    private readonly GameObject[] _leftInteractors;
    private readonly GameObject[] _rightInteractors;
    private readonly GameObject _leftHandVisual;

    public HandInteractionController(GameObject[] leftInteractors, GameObject[] rightInteractors,
        GameObject leftHandVisual)
    {
        _leftInteractors = leftInteractors;
        _rightInteractors = rightInteractors;
        _leftHandVisual = leftHandVisual;
    }

    // The existing Inspector configuration places the direct interactor last.
    public Transform LeftDirectTransform => _leftInteractors[_leftInteractors.Length - 1].transform;

    public void SetLeftInteractors(bool active, bool keepDirectEnabled = false)
        => SetInteractors(_leftInteractors, active, keepDirectEnabled);

    public void SetRightInteractors(bool active, bool keepDirectEnabled = false)
        => SetInteractors(_rightInteractors, active, keepDirectEnabled);

    public void EnableLeftDirectInteractor()
        => _leftInteractors[_leftInteractors.Length - 1].SetActive(true);

    public void SetLeftHandVisible(bool visible) => _leftHandVisual.SetActive(visible);

    private static void SetInteractors(GameObject[] interactors, bool active, bool keepDirectEnabled)
    {
        Helper.ToggleArrayOfGameObjects(interactors, active);
        if (keepDirectEnabled)
            interactors[interactors.Length - 1].SetActive(true);
    }
}
