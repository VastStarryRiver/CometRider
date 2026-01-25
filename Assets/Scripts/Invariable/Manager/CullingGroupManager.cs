using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Invariable
{
    public struct EntityData
    {
        public int configId;
        public int type;
        public string name;
        public GameObject obj;
    }

    public class CullingGroupManager : Singleton<CullingGroupManager>
    {
        private CullingGroup m_cullingGroup;
        private Dictionary<long, BoundingSphere> m_boundingSpheres;
        private Dictionary<long, EntityData> m_entityDatas;



        public void Register(long entityId, EntityData entityData, Vector3 pos, float rad = 0.5f)
        {
            if (m_cullingGroup == null)
            {
                m_cullingGroup = new CullingGroup();
                m_boundingSpheres = new Dictionary<long, BoundingSphere>();
                m_entityDatas = new Dictionary<long, EntityData>();
                m_cullingGroup.targetCamera = Utils.MainSceneCamera;
                m_cullingGroup.onStateChanged = OnStateChanged;
            }

            if (m_boundingSpheres.ContainsKey(entityId))
            {
                return;
            }

            m_cullingGroup.enabled = true;

            BoundingSphere boundingSphere = new BoundingSphere(pos, rad);

            m_boundingSpheres.Add(entityId, boundingSphere);
            m_entityDatas.Add(entityId, entityData);

            m_cullingGroup.SetBoundingSpheres(m_boundingSpheres.Values.ToArray());

            m_cullingGroup.SetBoundingSphereCount(m_boundingSpheres.Count);
        }

        public void UnRegister(long entityId)
        {
            if (m_cullingGroup == null || m_boundingSpheres == null || !m_boundingSpheres.ContainsKey(entityId))
            {
                return;
            }

            m_boundingSpheres.Remove(entityId);
            m_entityDatas.Remove(entityId);

            if (m_boundingSpheres.Count <= 0)
            {
                Dispose();
                return;
            }

            m_cullingGroup.SetBoundingSpheres(m_boundingSpheres.Values.ToArray());

            m_cullingGroup.SetBoundingSphereCount(m_boundingSpheres.Count);
        }

        public bool IsVisible(long entityId)
        {
            if (m_cullingGroup == null || m_boundingSpheres == null || !m_boundingSpheres.ContainsKey(entityId))
            {
                return false;
            }

            int index = m_boundingSpheres.Values.ToList().IndexOf(m_boundingSpheres[entityId]);
            return m_cullingGroup.IsVisible(index);
        }

        public void Dispose()
        {
            if (m_cullingGroup == null)
            {
                return;
            }

            m_cullingGroup.enabled = false;
            m_cullingGroup.Dispose();
            m_cullingGroup = null;

            m_boundingSpheres.Clear();
            m_boundingSpheres = null;

            m_entityDatas.Clear();
            m_entityDatas = null;
        }

        private void OnStateChanged(CullingGroupEvent sphere)
        {
            long entityId = m_boundingSpheres.Keys.ToList()[sphere.index];
            
            if (sphere.hasBecomeVisible)
            {
                VisibleChange(entityId, true);
            }
            else if(sphere.hasBecomeInvisible)
            {
                VisibleChange(entityId, false);
            }
        }

        private void VisibleChange(long entityId, bool visible)
        {
            if (!m_entityDatas.ContainsKey(entityId))
            {
                return;
            }

            m_entityDatas[entityId].obj.SetActive(visible);
        }
    }
}