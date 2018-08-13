using UnityEngine;

namespace Kiwi.JimmyGon
{
    public class UIWrapGridLayoutGroup : UIWrapLayoutGroup
    {
        public enum Corner
        {
            UpperLeft,
            UpperRight,
            LowerLeft,
            LowerRight
        }

        public enum Axis
        {
            Horizontal,
            Vertical
        }

        public enum Constraint
        {
            Flexible,
            FixedColumnCount,
            FixedRowCount
        }

        [SerializeField]
        protected Corner m_StartCorner = Corner.UpperLeft;
        [SerializeField]
        protected Axis m_StartAxis = Axis.Horizontal;
        [SerializeField]
        protected Vector2 m_CellSize = new Vector2(100, 100);
        [SerializeField]
        protected Vector2 m_Spacing = Vector2.zero;
        [SerializeField]
        protected Constraint m_Constraint = Constraint.Flexible;
        [SerializeField]
        protected int m_ConstraintCount = 2;

        public Corner startCorner
        {
            get { return m_StartCorner; }
            set { SetProperty(ref m_StartCorner, value); }
        }

        public Axis startAxis
        {
            get { return m_StartAxis; }
            set { SetProperty(ref m_StartAxis, value); }
        }

        public Vector2 cellSize
        {
            get { return m_CellSize; }
            set { SetProperty(ref m_CellSize, value); }
        }

        public Vector2 spacing
        {
            get { return m_Spacing; }
            set { SetProperty(ref m_Spacing, value); }
        }

        public Constraint constraint
        {
            get { return m_Constraint; }
            set { SetProperty(ref m_Constraint, value); }
        }

        public int constraintCount
        {
            get { return m_ConstraintCount; }
            set { SetProperty(ref m_ConstraintCount, value); }
        }

        protected UIWrapGridLayoutGroup() {}

        private void SetCellsAlongAxis(int axis)
        {
            if (axis == 0) return;
            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;
                cellCountY = Mathf.CeilToInt(dataContent.Length / (float)cellCountX - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;
                cellCountX = Mathf.CeilToInt(dataContent.Length / (float)cellCountY - 0.001f);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, dataContent.Length);
                actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(dataContent.Length / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, dataContent.Length);
                actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(dataContent.Length / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y);
            Vector2 startOffset = new Vector2(GetStartOffset(0, requiredSpace.x), GetStartOffset(1, requiredSpace.y));

            for (int i = 0; i < dataContent.Length; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildRect(i, 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0], true);
                SetChildRect(i, 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1], true);
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            int minColumns = 0;
            int preferredColumns = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minColumns = preferredColumns = m_ConstraintCount;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minColumns = preferredColumns = Mathf.CeilToInt(dataContent.Length / (float)m_ConstraintCount - 0.001f);
            }
            else
            {
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(dataContent.Length));
            }
            SetLayoutInputForAxis(padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x, padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x, -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            int minRows = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minRows = Mathf.CeilToInt(dataContent.Length / (float)m_ConstraintCount - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minRows = m_ConstraintCount;
            }
            else
            {
                float width = rectTransform.rect.size.x;
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                minRows = Mathf.CeilToInt(dataContent.Length / (float)cellCountX);
            }
            float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            constraintCount = constraintCount;
        }
#endif
    }
}
