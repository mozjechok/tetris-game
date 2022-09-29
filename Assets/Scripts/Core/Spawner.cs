using UnityEngine;

using System.Collections;
using static Vectorf;

public class Spawner : MonoBehaviour
{
    public Shape[] allShapes;
    public Transform[] queuedXforms = new Transform[3];
    public ParticlePlayer spawnFx;

    public bool UseQueue = true;

    private Shape[] queuedShapes = new Shape[3];

    private float queueScale = 0.5f;

    private void Start() => InitQueue();

    private Shape GetRandomShape()
    {
        int i = Random.Range(0, allShapes.Length);

        if (!allShapes[i])
            Debug.Log("Null shape");

        return allShapes[i];
    }


    private void InitQueue()
    {
        if (!UseQueue)
            return;

        for (int i = 0; i < queuedShapes.Length; i++)
        {
            queuedShapes[i] = null;
        }

        FillQueue();
    }

    private void FillQueue()
    {
        if (!UseQueue)
            return;

        for (int i = 0; i < queuedShapes.Length; i++)
        {
            if (!queuedShapes[i])
            {
                queuedShapes[i] =
                    Instantiate<Shape>(GetRandomShape(), transform.position, Quaternion.identity);

                queuedShapes[i].transform.position = queuedXforms[i].position + queuedShapes[i].queueOffset;
                queuedShapes[i].transform.localScale =
                    new Vector3(queueScale, queueScale, queueScale);
            }
        }
    }

    private Shape GetQueuedShape()
    {
        if (UseQueue)
            return GetRandomShape();

        Shape firstShape = null;

        if (queuedShapes[0])
        {
            firstShape = queuedShapes[0];
        }

        for (int i = 1; i < queuedShapes.Length; i++)
        {
            queuedShapes[i - 1] = queuedShapes[i];
            queuedShapes[i - 1].transform.position =
                queuedXforms[i - 1].position + queuedShapes[i].queueOffset;
        }

        queuedShapes[queuedShapes.Length - 1] = null;

        FillQueue();

        return firstShape;
    }

    private IEnumerator GrowShape(Shape shape, Vector3 position, float growTime = 0.5f)
    {
        float size = 0f;

        growTime = Mathf.Clamp(growTime, 0.1f, 2f);

        float sizeDelta = Time.deltaTime / growTime;

        while (size < 1f)
        {
            shape.transform.localScale = new Vector3(size, size, size);
            shape.transform.position = position;

            size += sizeDelta;
            
            yield return null;
        }

        shape.transform.localScale = Vector3.one;
    }

    public Shape SpawnShape(Shape shape = null)
    {
        if (UseQueue)
            shape = GetQueuedShape();

        shape.transform.position = transform.position;

        StartCoroutine(GrowShape(shape, transform.position, 0.25f));

        if (spawnFx)
            spawnFx.Play();

        if (!shape)
            Debug.Log("Null shape");

        return shape;
    }
}
