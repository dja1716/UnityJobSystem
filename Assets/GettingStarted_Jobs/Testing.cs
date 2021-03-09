using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using System.Linq.Expressions;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Jobs;


// 1. create job struct for behavior and information about job
// 2. create instance of struct (ReallyToughTaskJob())
// 3. Schdule the job on the system
// 4. tell the job to complete


public class Testing : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfZombie;
    private List<Zombie> zombieList;

    public class Zombie
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        zombieList = new List<Zombie>();

        for(int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(pfZombie, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);

            zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }


    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if(useJobs)
        {
            //NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(zombieList.Count);
            
            
            NativeArray<float> moveYArray = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

            for(int i = 0; i < zombieList.Count; i++)
            {
                //positionArray[i] = zombieList[i].transform.position;
                moveYArray[i] = zombieList[i].moveY;
                transformAccessArray.Add(zombieList[i].transform);
            }
            /*
            ReallyToughParallelJob reallyToughParallelJob = new ReallyToughParallelJob
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };
            */

            ReallyToughParallelJobTransform reallyToughParallelJobTransform = new ReallyToughParallelJobTransform
            {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };


            // second variable is size of each batch
            JobHandle jobHandle = reallyToughParallelJobTransform.Schedule(transformAccessArray);
            jobHandle.Complete();


            // to update job after each batch
            for (int i = 0; i < zombieList.Count; i++)
            {
                //zombieList[i].transform.position = positionArray[i];
                zombieList[i].moveY = moveYArray[i];
            }

            //positionArray.Dispose();
            moveYArray.Dispose();
            transformAccessArray.Dispose();

        } else
        {
            foreach (Zombie zombie in zombieList)
            {
                zombie.transform.position += new Vector3(0, zombie.moveY * Time.deltaTime);
                if (zombie.transform.position.y > 5f)
                {
                    zombie.moveY = -math.abs(zombie.moveY);
                }
                else if (zombie.transform.position.y < -5f)
                {
                    zombie.moveY = +math.abs(zombie.moveY);
                }

                float value = 0f;

                for (int i = 0; i < 50000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }


        /*
        if(useJobs)
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            for(int i = 0; i < 10; i++)
            {
                JobHandle jobHandle = ReallyToughTaskJob();
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();


            // for individual job
            // jobHandle.Complete();


        } else
        {
            for(int i = 0; i < 10; i++)
            {
                ReallyToughTask();
            }
            
        }
        */


        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    private void ReallyToughTask() {
        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle ReallyToughTaskJob()
    {
        ReallyToughJob job = new ReallyToughJob();
        return job.Schedule();

    }
}


// in order to make a job we should make struct
// C# class and struct difference
//      class       =       reference type
//      struct      =       value type
// [BustCompile] make jobs so much faster like 100 times




[BurstCompile]
public struct ReallyToughJob : IJob
{
    //public float something;
    public void Execute()
    {
        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
[BurstCompile]
public struct ReallyToughParallelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;
    public void Execute(int index)
    {
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        else if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
[BurstCompile]
public struct ReallyToughParallelJobTransform : IJobParallelForTransform
{
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;


    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0f);
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        else if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}