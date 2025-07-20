using UnityEngine;

public class TrainTriggerRelay : MonoBehaviour
{
    public SubwayTrainMover trainMover;

    private void OnTriggerEnter(Collider other)
    {
        trainMover?.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        trainMover?.OnTriggerExit(other);
    }
}
