using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Statistics;

namespace SceneAware
{
    public class SphereFitter : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Fit a sphere to a list of points using least-squares method
        public static void Fit(List<Vector3> points, out Vector3 center, out float radius)
        {
            //��ʼ��
            center = Vector3.zero;
            radius = 0f;

            //�������ģ�����ƽ��ֵ��
            foreach (Vector3 p in points)
            {
                center += p;
            }
            center /= points.Count;

            //����������
            foreach (Vector3 p in points)
            {
                float dist = (p - center).magnitude;
                if (dist > radius)
                {
                    radius = dist;
                }
            }
            //�����������ĺͰ뾶
            //float diff = 1f;
            //float eps = 0.01f; // ���������1cm�ھ���Ϊ�㹻��
            //while (diff > eps)
            //{
            //    Vector3 oldCenter = center;
            //    float oldRadius = radius;
            //    for (int i = 0; i < points.Count; i++)
            //    {
            //        Vector3 p = points[i];
            //        float dist = (p - center).magnitude;
            //        if (dist > 0f)
            //        {
            //            float alpha = Mathf.Atan2(p.y - center.y, p.x - center.x);
            //            float beta = Mathf.Asin((p.z - center.z) / dist);
            //            float delta = (dist - radius) / dist;
            //            center.x += delta * Mathf.Cos(beta) * Mathf.Cos(alpha);
            //            center.y += delta * Mathf.Cos(beta) * Mathf.Sin(alpha);
            //            center.z += delta * Mathf.Sin(beta);
            //            radius = (oldRadius + dist + radius) / 3f;
            //        }
            //    }
            //    diff = (center - oldCenter).magnitude;
            //}
        }
    }
}

