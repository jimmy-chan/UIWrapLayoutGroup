namespace Kiwi.JimmyGon
{
    public class UIWrapHorizontalLayoutGroup : UIWrapHorizontalOrVerticalLayoutGroup
    {
        protected UIWrapHorizontalLayoutGroup() { }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, false);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, false);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, false);
        }
    }
}
