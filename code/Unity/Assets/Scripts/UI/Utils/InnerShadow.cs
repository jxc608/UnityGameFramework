using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/InnerShadow")]
    public class InnerShadow : BaseMeshEffect
    {
        [SerializeField]
        private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

        [SerializeField]
        private float m_EffectDistance = 4f;

        [SerializeField]
        private bool m_UseGraphicAlpha = true;

        /// <summary>
        ///   <para>Color for the effect.</para>
        /// </summary>
        public Color effectColor
        {
            get
            {
                return this.m_EffectColor;
            }
            set
            {
                this.m_EffectColor = value;
                if (base.graphic != null)
                {
                    base.graphic.SetVerticesDirty();
                }
            }
        }

        /// <summary>
        ///   <para>Should the shadow inherit the alpha from the graphic?</para>
        /// </summary>
        public bool useGraphicAlpha
        {
            get
            {
                return this.m_UseGraphicAlpha;
            }
            set
            {
                this.m_UseGraphicAlpha = value;
                if (base.graphic != null)
                {
                    base.graphic.SetVerticesDirty();
                }
            }
        }

        protected InnerShadow()
        {
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (IsActive() == false)
            {
                return;
            }

            var vList = new List<UIVertex>();
            vh.GetUIVertexStream(vList);

            ModifyVertices(vList);

            vh.Clear();
            vh.AddUIVertexTriangleStream(vList);
        }


        public void ModifyVertices(List<UIVertex> vList)
        {
            Dictionary<float, float> m_TopDic = new Dictionary<float, float>();
            for (int i = 0; i < vList.Count; i++)
            {
                var vertex = vList[i];
                if (m_TopDic.ContainsKey(vertex.position.x))
                {
                    m_TopDic[vertex.position.x] = Mathf.Max(m_TopDic[vertex.position.x], vertex.position.y);
                }
                else
                {
                    m_TopDic.Add(vertex.position.x, vertex.position.y);
                }
            }

            UIVertex tempVertex = vList[0];
            for (int i = 0; i < vList.Count; i++)
            {
                tempVertex = vList[i];
                if (tempVertex.position.y <= m_TopDic[tempVertex.position.x] - m_EffectDistance)
                {
                    tempVertex.color = m_EffectColor;
                }
                vList[i] = tempVertex;
            }
        }
    }
}
