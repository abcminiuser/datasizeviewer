using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;

namespace FourWalledCubicle.DataSizeViewerExt
{
    public class ListViewSortArrowAdorner : Adorner
    {
        private readonly static Geometry mAcendingArrow = Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");
        private readonly static Geometry mDescendingArrow = Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

        public ListSortDirection Direction { get; private set; }

        public ListViewSortArrowAdorner(UIElement element, ListSortDirection sortDirection) : base(element)
        {
            Direction = sortDirection;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.PushTransform(
                new TranslateTransform(
                  AdornedElement.RenderSize.Width - 15,
                  (AdornedElement.RenderSize.Height - 5) / 2));

            drawingContext.DrawGeometry(Brushes.Gray, null, Direction == ListSortDirection.Ascending ? mAcendingArrow : mDescendingArrow);

            drawingContext.Pop();
        }
    }
}
